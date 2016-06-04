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


namespace UwpInteropSample
{
  // Implements a simple graphics screen which owns a scene, a camera and renders
  // the scene using simple forward rendering.
  // The camera moves in a circle.
  // The frames per second are measured and displayed.
  internal class MyGraphicsScreen : GraphicsScreen
  {
    private readonly MeshRenderer _meshRenderer;
    private readonly DebugRenderer _debugRenderer;

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _frameCount;
    private float _cameraRotation;

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
        Name = "CameraPerspective"
      };
      Scene.Children.Add(CameraNode);
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Rotate camera around origin.
      _cameraRotation += 0.1f * (float)deltaTime.TotalSeconds;
      var cameraPosition = Matrix33F.CreateRotationY(_cameraRotation) * new Vector3F(10, 5, 10);
      var targetPosition = new Vector3F(0, 1, 0);
      var up = new Vector3F(0, 1, 0);
      var viewMatrix = Matrix44F.CreateLookAt(cameraPosition, targetPosition, up);
      CameraNode.PoseWorld = Pose.FromMatrix(viewMatrix.Inverse);

      Scene.Update(deltaTime);
    }


    protected override void OnRender(RenderContext context)
    {
      _frameCount++;

      // At regular intervals reset the debug output and write the current FPS.
      if (_stopwatch.Elapsed.TotalSeconds > 1)
      {
        _debugRenderer.Clear();
        _debugRenderer.DrawText("FPS: " + _frameCount / (float)_stopwatch.Elapsed.TotalSeconds);
        _stopwatch.Restart();
        _frameCount = 0;
      }

      context.Scene = Scene;
      context.CameraNode = CameraNode;

      var graphicsDevice = GraphicsService.GraphicsDevice;

      // Clear background.
      graphicsDevice.Clear(Color.Transparent);

      // Frustum culling: Get all scene nodes that intersect the camera frustum.
      var query = Scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Set render state.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

      // Use a MeshRenderer to render all MeshNodes that are in the camera frustum.
      // We use the shader effects and effect parameters for the render pass named
      // "Default" (see the material (.drmat files) of the assets).
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