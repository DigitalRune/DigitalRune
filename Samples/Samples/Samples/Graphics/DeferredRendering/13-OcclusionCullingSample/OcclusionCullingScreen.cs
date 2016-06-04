#if !WP7 && !WP8
using System.Collections.Generic;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples
{
  enum DebugVisualization
  {
    None,         // Debug visualization disabled.
    CameraHzb,    // Visualize camera HZB.
    LightHzb,     // Visualize light HZB.
    Object,       // Visualize occlusion query of specific object.
    ShadowCaster, // Visualize occlusion query of shadow caster. (Conservative shadow caster culling.)
    ShadowVolume, // Visualize occlusion query of shadow volume. (Progressive shadow caster culling.)
  }


  // This graphics screen demonstrates occlusion culling using the OcclusionBuffer.
  class OcclusionCullingScreen : DeferredGraphicsScreen
  {
    private readonly List<SceneNode> _sceneNodes;
    private readonly CustomSceneQuery _sceneQuery;
    private readonly CameraNode _topDownCameraNode; // For split-screen view.
    private readonly DebugRenderer _debugRenderer;


    // Enables/disables occlusion culling.
    public bool EnableCulling { get; set; }

    // The OcclusionBuffer that implements frustum culling, occlusion culling and
    // shadow caster culling.
    public OcclusionBuffer OcclusionBuffer { get; private set; }

    // The main directional light source. (Needs to be set for shadow caster culling.)
    public LightNode LightNode { get; set; }

    // Shows a top-down view of the scene.
    public bool ShowTopDownView { get; set; }

    // Debug visualization:
    public DebugVisualization DebugVisualization { get; set; }
    public int DebugLevel { get; set; }
    public SceneNode DebugObject { get; set; }


    public OcclusionCullingScreen(IServiceLocator services)
      : base(services)
    {
      _sceneNodes = new List<SceneNode>();

      // Create new occlusion buffer with default settings.
      OcclusionBuffer = new OcclusionBuffer(GraphicsService);
      OcclusionBuffer.ProgressiveShadowCasterCulling = true;

      EnableCulling = true;

      // Create a second camera for rendering a top-down view of the scene.
      var topDownPerspective = new PerspectiveProjection();
      topDownPerspective.SetFieldOfView(MathHelper.ToRadians(90), 1, 1, 512);
      _topDownCameraNode = new CameraNode(new Camera(topDownPerspective));
      _topDownCameraNode.PoseWorld = new Pose(new Vector3F(-10, 120, -10));
      _topDownCameraNode.LookAt(new Vector3F(-10, 0, -10), Vector3F.UnitZ);

      _sceneQuery = new CustomSceneQuery();
      _debugRenderer = new DebugRenderer(GraphicsService, null);

      // The DigitalRune Profiler is used to measure execution times.
      Profiler.SetFormat("Occlusion.Render", 1e3f, "[ms]");
      Profiler.SetFormat("Occlusion.Query", 1e3f, "[ms]");
    }


    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          OcclusionBuffer.Dispose();
          _debugRenderer.Dispose();
        }

        base.Dispose(disposing);
      }
    }


    // Changes the current debug visualization mode.
    public void ToggleVisualization(DebugVisualization mode)
    {
      if (DebugVisualization == mode)
        DebugVisualization = DebugVisualization.None;
      else
        DebugVisualization = mode;
    }


    // Needs to be called when occlusion culling gets disabled:
    public void ResetShadowCasters()
    {
      // Shadow casting scene nodes are internally marked as visible/hidden.
      // When shadow caster culling disabled, this state needs to be reset.
      CopyNodesToList(Scene, _sceneNodes);
      OcclusionBuffer.ResetShadowCasters(_sceneNodes);
      _sceneNodes.Clear();
    }


    protected override void OnRender(RenderContext context)
    {
      if (ActiveCameraNode == null)
        return;

      context.Scene = Scene;
      context.CameraNode = ActiveCameraNode;
      context.LodCameraNode = ActiveCameraNode;

      // Copy all scene nodes into a list.
      CopyNodesToList(Scene, _sceneNodes);

      // ----- Occlusion Culling
      // Usually, we would make a scene query to get all scene nodes within the 
      // viewing frustum. But in this example we will use the new OcclusionBuffer.
      if (EnableCulling)
      {
        // Render all occluders into the occlusion buffer.
        // - "_sceneNodes" is a list of all scene nodes. The OcclusionBuffer will
        //   go through the list and render all occluders.
        // - "LightNode" is the main directional light that casts a cascaded shadow.
        //   Passing the light node to the OcclusionBuffer activates shadow caster
        //   culling.
        // - A custom scene node renderer can be passed to the OcclusionBuffer. In
        //   this example, the ground mesh "Gravel/Gravel.fbx" has a material with an
        //   "Occluder" render pass. When we pass the "MeshRenderer" to the OcclusionBuffer
        //   the ground mesh will be rendered directly into the occlusion buffer.
        Profiler.Start("Occlusion.Render");
        context.RenderPass = "Occluder";
        OcclusionBuffer.Render(_sceneNodes, LightNode, MeshRenderer, context);
        context.RenderPass = null;
        Profiler.Stop("Occlusion.Render");

        // Perform occlusion culling on the specified list of scene nodes.
        // - The scene nodes will be tested against the occluders. If a scene node
        //   is hidden, it will be replaced with a null entry in the list.
        // - When shadow caster culling is active, shadow casting scene nodes will
        //   also be tested against the occluders. If the shadow is not visible,
        //   the shadow caster will internally be marked as occluded. The ShadowMapRenderer
        //   will automatically skip occluded scene nodes.
        Profiler.Start("Occlusion.Query");
        OcclusionBuffer.Query(_sceneNodes, context);
        Profiler.Stop("Occlusion.Query");
      }

      // The base DeferredGraphicsScreen expects a CustomSceneQuery.
      // --> Copy the occlusion culling results to a CustomSceneQuery.
      _sceneQuery.Set(ActiveCameraNode, _sceneNodes, context);

      var renderTargetPool = GraphicsService.RenderTargetPool;
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var originalRenderTarget = context.RenderTarget;
      var fullViewport = context.Viewport;

      RenderTarget2D topDownRenderTarget = null;
      const int topDownViewSize = 384;
      if (ShowTopDownView)
      {
        // Render top-down scene into an offscreen render target.
        var format = new RenderTargetFormat(context.RenderTarget)
        {
          Width = topDownViewSize, 
          Height = topDownViewSize,
        };
        topDownRenderTarget = renderTargetPool.Obtain2D(format);

        context.Scene = Scene;
        context.CameraNode = _topDownCameraNode;
        context.Viewport = new Viewport(0, 0, topDownViewSize, topDownViewSize);
        context.RenderTarget = topDownRenderTarget;
        RenderScene(_sceneQuery, context, true, false, false, false);

        _debugRenderer.Clear();
        _debugRenderer.DrawObject(ActiveCameraNode, Color.Red, true, true);
        _debugRenderer.Render(context);

        context.RenderTarget = originalRenderTarget;
        context.Viewport = fullViewport;
      }

      // Render regular 3D scene.
      context.Scene = Scene;
      context.CameraNode = ActiveCameraNode;
      RenderScene(_sceneQuery, context, true, false, true, false);

      // Render debug visualization on top of scene.
      bool renderObject = false;
      switch (DebugVisualization)
      {
        case DebugVisualization.CameraHzb:
          OcclusionBuffer.VisualizeCameraBuffer(DebugLevel, context);
          break;
        case DebugVisualization.LightHzb:
          OcclusionBuffer.VisualizeLightBuffer(DebugLevel, context);
          break;
        case DebugVisualization.Object:
          OcclusionBuffer.VisualizeObject(DebugObject, context);
          renderObject = true;
          break;
        case DebugVisualization.ShadowCaster:
          OcclusionBuffer.VisualizeShadowCaster(DebugObject, context);
          break;
        case DebugVisualization.ShadowVolume:
          OcclusionBuffer.VisualizeShadowVolume(DebugObject, context);
          renderObject = true;
          break;
      }

      if (renderObject)
      {
        _debugRenderer.Clear();
        _debugRenderer.DrawObject(DebugObject, Color.Yellow, true, true);
        _debugRenderer.Render(context);
      }

      if (ShowTopDownView)
      {
        // Copy offscreen buffer to screen.
        context.Viewport = fullViewport;
        graphicsDevice.Viewport = fullViewport;

        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        SpriteBatch.Draw(
          topDownRenderTarget,
          new Rectangle(fullViewport.Width - topDownViewSize, fullViewport.Height - topDownViewSize, topDownViewSize, topDownViewSize),
          Color.White);
        SpriteBatch.End();

        renderTargetPool.Recycle(topDownRenderTarget);
      }

      // Clean-up
      _sceneNodes.Clear();
      _sceneQuery.Reset();

      context.Scene = null;
      context.CameraNode = null;
      context.LodCameraNode = null;
    }


    private void CopyNodesToList(SceneNode node, List<SceneNode> list)
    {
      if (node.IsEnabled)
      {
        if (node.Shape != Shape.Empty)
          list.Add(node);

        if (node.Children != null)
          foreach (var child in node.Children)
            CopyNodesToList(child, list);
      }
    }
  }
}
#endif