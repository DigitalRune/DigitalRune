#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{

  // Implements a simple graphics screen using normal "forward rendering", which
  // can be used in the post-processing samples.
  // It creates a G-buffer (depth buffer + normal buffer) and a velocity buffer. Some 
  // post-processors need the depth buffer, for example the DepthOfFieldFilter or 
  // the SsaoFilter. The EdgeFilter needs both the depth buffer and the normal buffer.
  // The ObjectMotionBlur processor needs the velocity buffer.
  public sealed class PostProcessingGraphicsScreen : GraphicsScreen, IDisposable
  {
    // Pre-allocate array to avoid allocations at runtime.
    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    private readonly SampleFramework _sampleFramework;

    private readonly SpriteBatch _spriteBatch;
    private readonly ClearGBufferRenderer _clearGBufferRenderer;
    private readonly RebuildZBufferRenderer _rebuildZBufferRenderer;
    private readonly MeshRenderer _meshRenderer;
    private readonly SkyRenderer _skyRenderer;
    private readonly BillboardRenderer _billboardRenderer;
    private readonly Texture2D _reticle;


    // Temporary list for collecting all mesh nodes that have moved since the last
    // frame.
    private readonly List<SceneNode> _movingMeshes = new List<SceneNode>();


    // The active camera used to render the scene. This property must be set by 
    // the samples. The default value is null.
    public CameraNode ActiveCameraNode { get; set; }


    public Scene Scene { get; private set; }


    // The post-processor chain. Sample classes will set and change the post-processors.
    public PostProcessorChain PostProcessors { get; private set; }


    // A debug renderer which can be used by the samples and game objects. 
    // (Note: DebugRenderer.Clear() is not called automatically.)
    public DebugRenderer DebugRenderer { get; private set; }


    public bool DrawReticle { get; set; }


    public PostProcessingGraphicsScreen(IServiceLocator services)
      : base(services.GetInstance<IGraphicsService>())
    {
      _sampleFramework = services.GetInstance<SampleFramework>();

      _spriteBatch = new SpriteBatch(GraphicsService.GraphicsDevice);
      _clearGBufferRenderer = new ClearGBufferRenderer(GraphicsService);
      _rebuildZBufferRenderer = new RebuildZBufferRenderer(GraphicsService);
      _meshRenderer = new MeshRenderer();
      _skyRenderer = new SkyRenderer(GraphicsService);
      _billboardRenderer = new BillboardRenderer(GraphicsService, 2048);

      Scene = new Scene();
      PostProcessors = new PostProcessorChain(GraphicsService);

      // Use 2D texture for reticle.
      var contentManager = services.GetInstance<ContentManager>();
      _reticle = contentManager.Load<Texture2D>("Reticle");

      // Use the sprite font of the GUI.
      var uiContentManager = services.GetInstance<ContentManager>("UIContent");
      var spriteFont = uiContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      DebugRenderer = new DebugRenderer(GraphicsService, spriteFont)
      {
        DefaultColor = new Color(0, 0, 0),
        DefaultTextPosition = new Vector2F(10),
      };
    }


    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
      _spriteBatch.Dispose();
      _meshRenderer.Dispose();
      _skyRenderer.Dispose();
      _billboardRenderer.Dispose();
      _movingMeshes.Clear();

      Scene.Dispose(false);
      PostProcessors.Dispose();
      DebugRenderer.Dispose();
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Update the scene - this must be called each frame.
      Scene.Update(deltaTime);
    }


    protected override void OnRender(RenderContext context)
    {
#if OPENGL
      GraphicsService.RenderTargetPool.Clear();  // Required because of a GraphicsDevice.SetRenderTarget() bug...
#endif

      // Abort if no active camera is set.
      if (ActiveCameraNode == null)
        return;

      // Set the current camera and current scene in the render context. This 
      // information is very important; it is used by the scene node renderers.
      context.CameraNode = ActiveCameraNode;
      context.Scene = Scene;

      // Remember the final render target and viewport of this graphics screen.
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      // Frustum Culling: Get all the scene nodes that intersect the camera frustum.
      var query = Scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Create G-Buffer (= depth buffer + normal buffer).
      RenderGBuffer(query, context);

      // Create velocity buffer, which is needed for motion-blur.
      RenderVelocityBuffer(query, context);

      // Render the scene.
      var sceneRenderTarget = RenderScene(query, context);

      // Perform post-processing. 
      // The source image is the current offscreen buffer.
      context.SourceTexture = sceneRenderTarget;
      // The result is written to the render target of this graphics screen.
      context.RenderTarget = target;
      context.Viewport = viewport;
      PostProcessors.Process(context);
      context.SourceTexture = null;

      DebugRenderer.Render(context);

      // ----- Draw Reticle
      if (DrawReticle && _sampleFramework.IsGuiVisible)
      {
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        _spriteBatch.Draw(
          _reticle,
          new Vector2(viewport.Width / 2 - _reticle.Width / 2, viewport.Height / 2 - _reticle.Height / 2),
          Color.Black);
        _spriteBatch.End();
      }

      // Recycle temporary render targets and clean up render context.
      var renderTargetPool = GraphicsService.RenderTargetPool;
      renderTargetPool.Recycle(context.GBuffer0);
      renderTargetPool.Recycle(context.GBuffer1);
      context.GBuffer0 = null;
      context.GBuffer1 = null;
      renderTargetPool.Recycle((RenderTarget2D)context.Data[RenderContextKeys.VelocityBuffer]);
      context.Data[RenderContextKeys.VelocityBuffer] = null;
      renderTargetPool.Recycle(sceneRenderTarget);
      context.RenderPass = null;
      context.CameraNode = null;
      context.Scene = null;
    }


    private void RenderGBuffer(CameraFrustumQuery frustumQuery, RenderContext context)
    {
      // Create render targets for the G-buffer.
      context.GBuffer0 = GraphicsService.RenderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          true,        // Note: Only the SaoFilter for SSAO requires mipmaps to boost performance.
          SurfaceFormat.Single,
          DepthFormat.Depth24Stencil8));
      context.GBuffer1 = GraphicsService.RenderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          false,
          SurfaceFormat.Color,
          DepthFormat.None));

      var graphicsDevice = GraphicsService.GraphicsDevice;

      _renderTargetBindings[0] = new RenderTargetBinding(context.GBuffer0);
      _renderTargetBindings[1] = new RenderTargetBinding(context.GBuffer1);
      graphicsDevice.SetRenderTargets(_renderTargetBindings);

      // Update the info in the render context. Some renderers or effects might use this info.
      context.RenderTarget = context.GBuffer0;
      context.Viewport = graphicsDevice.Viewport;

      // Clear the depth-stencil buffer.
      graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

      // Reset the G-buffer to default values.
      _clearGBufferRenderer.Render(context);

      // Render the meshes using the "GBuffer" material pass.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      context.RenderPass = "GBuffer";
      _meshRenderer.Render(frustumQuery.SceneNodes, context);
      context.RenderPass = null;
    }


    private void RenderVelocityBuffer(CameraFrustumQuery frustumQuery, RenderContext context)
    {
      // Create a render target for the velocity buffer and store it in the render context.
      RenderTarget2D velocityBuffer = GraphicsService.RenderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          false,
          SurfaceFormat.HalfVector2,
          DepthFormat.Depth24Stencil8));
      context.Data[RenderContextKeys.VelocityBuffer] = velocityBuffer;

      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.SetRenderTarget(velocityBuffer);

      graphicsDevice.Clear(Color.Black);

      // Update the info in the render context. Some renderers or effects might use this info.
      context.RenderTarget = velocityBuffer;
      context.Viewport = graphicsDevice.Viewport;

      // We do not have a valid depth buffer. In this pass we do not want to render
      // all objects again, therefore we rebuild the hardware depth buffer from the
      // G-Buffer using the RebuildZBufferRenderer. (As a side effect, this also 
      // clears the color target.)
      _rebuildZBufferRenderer.Render(context, false);

      // Render the meshes using the "Velocity" material pass.
      // It is only necessary to render meshes that have moved since the last frame.
      foreach (var node in frustumQuery.SceneNodes)
      {
        if (node is MeshNode && node.LastPoseWorld.HasValue && node.LastPoseWorld.Value != node.PoseWorld)
          _movingMeshes.Add(node);
      }

      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      context.RenderPass = "Velocity";
      _meshRenderer.Render(_movingMeshes, context);
      context.RenderPass = null;

      _movingMeshes.Clear();
    }


    private RenderTarget2D RenderScene(CameraFrustumQuery frustumQuery, RenderContext context)
    {
      // Set an offscreen render target. The size is determined by the current viewport.
      RenderTarget2D sceneRenderTarget = GraphicsService.RenderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          false,
          SurfaceFormat.Color,
          DepthFormat.Depth24Stencil8));

      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.SetRenderTarget(sceneRenderTarget);

      // Update the info in the render context. Some renderers or effects might use this info.
      context.RenderTarget = sceneRenderTarget;
      context.Viewport = graphicsDevice.Viewport;

      graphicsDevice.Clear(Color.CornflowerBlue);

      // Render the meshes using the "Default" material.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
      context.RenderPass = "Default";
      _meshRenderer.Render(frustumQuery.SceneNodes, context);
      context.RenderPass = null;

      // Render the sky.
      _skyRenderer.Render(frustumQuery.SceneNodes, context);

      // Set the render states for alpha blended objects and render BillboardNodes
      // and ParticleSystemNodes.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = BlendState.NonPremultiplied;
      graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
      _billboardRenderer.Render(frustumQuery.SceneNodes, context);

      return sceneRenderTarget;
    }
  }
}
#endif
