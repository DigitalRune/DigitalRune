using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace WpfInteropSample2
{
  // Implements a simple graphics screen which owns a scene, a camera and renders 
  // the scene using simple forward rendering. 
  internal class MyGraphicsScreen : GraphicsScreen
  {
    private readonly MeshRenderer _meshRenderer;
    private readonly DebugRenderer _debugRenderer;

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _frameCount;

    public Scene Scene { get; private set; }
    public CameraNode CameraNode { get; private set; }


    public MyGraphicsScreen(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _meshRenderer = new MeshRenderer();

      var contentManager = ServiceLocator.Current.GetInstance<ContentManager>();
      var spriteFont = contentManager.Load<SpriteFont>("SpriteFont1");
      _debugRenderer = new DebugRenderer(graphicsService, spriteFont);

      Scene = new Scene();

      // Add a camera with a perspective projection.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        ConstantsF.PiOver4,
        graphicsService.GraphicsDevice.Viewport.AspectRatio,
        0.1f,
        100.0f);
      CameraNode = new CameraNode(new Camera(projection))
      {
        Name = "CameraPerspective",
        PoseWorld = Pose.FromMatrix(Matrix44F.CreateLookAt(new Vector3F(10, 5, 10), new Vector3F(0, 1, 0), new Vector3F(0, 1, 0)).Inverse),
      };
      Scene.Children.Add(CameraNode);
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      Scene.Update(deltaTime);

      _frameCount++;

      // At regular intervals reset the debug output and write the current FPS.
      if (_stopwatch.Elapsed.TotalSeconds > 1)
      {
        _debugRenderer.Clear();
        _debugRenderer.DrawText("FPS: " + _frameCount / (float)_stopwatch.Elapsed.TotalSeconds);
        _stopwatch.Restart();
        _frameCount = 0;
      }
    }


    protected override void OnRender(RenderContext context)
    {
      context.Scene = Scene;
      context.CameraNode = CameraNode;

      var graphicsDevice = GraphicsService.GraphicsDevice;

      // Clear background.
      graphicsDevice.Clear(new Color(60, 60, 60));

      // Frustum Culling: Get all scene nodes that intersect the camera frustum.
      var query = Scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Set render state.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

      // Use a MeshRenderer to render all MeshNodes that are in the camera frustum.
      // We use the shader effects and effect parameters for the render pass named 
      // "Default" (see the material (.drmat) files of the assets).
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context);
      context.RenderPass = null;

      // Render debug info.
      _debugRenderer.Render(context);

      context.CameraNode = null;
      context.Scene = null;
    }
  }
}