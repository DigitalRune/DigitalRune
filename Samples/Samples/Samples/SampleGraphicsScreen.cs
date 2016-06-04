using System;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // The SampleGraphicsScreen is a graphics screen that supports basic rendering:
  // - DebugRenderer2D can be used to draw 2D graphics in pixel coordinates.
  // - DebugRenderer can be used to draw 3D graphics.
  // - Scene can be used to draw MeshNodes, BillboardNodes, and ParticleSystemNodes.
  //   (Other types of scene nodes are not supported.)
  public sealed class SampleGraphicsScreen : GraphicsScreen, IDisposable
  {
    private readonly SampleFramework _sampleFramework;

    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _defaultFont;
    private readonly SpriteFont _fixedWidthFont;
    private readonly Texture2D _reticle;

    // Scene node renderers
    private readonly MeshRenderer _meshRenderer;
    private readonly BillboardRenderer _billboardRenderer;

    private CameraNode _cameraNode2D;


    // true to clear the background in each frame.
    public bool ClearBackground { get; set; }

    // The background color used when ClearBackground is set.
    public Color BackgroundColor { get; set; }

    // true to use a fixed-width font for text output in all DebugRenderers.
    public bool UseFixedWidthFont { get; set; }

    // true to draw a reticle (cross-hair) at the center of the screen.
    public bool DrawReticle { get; set; }

    // The camera node used for 3D output.
    public CameraNode CameraNode { get; set; }

    // A DebugRenderer that can be used for 2D output. Coordinates are in pixels 
    // where (0, 0) is the upper, left corner of the screen.
    public DebugRenderer DebugRenderer2D { get; private set; }

    // A DebugRenderer that can be used for 3D output.
    public DebugRenderer DebugRenderer { get; private set; }

    // A Scene that can be used to render MeshNodes, BillboardNodes, and 
    // ParticleSystemNodes.
    public Scene Scene { get; private set; }


    public SampleGraphicsScreen(IServiceLocator services)
      : base(services.GetInstance<IGraphicsService>())
    {
      _sampleFramework = services.GetInstance<SampleFramework>();

      Name = "SampleScreen";
      ClearBackground = false;
      BackgroundColor = new Color(220, 220, 220);
      DrawReticle = false;
      UseFixedWidthFont = false;

      // Use 2D texture for reticle.
      var contentManager = services.GetInstance<ContentManager>();
      _reticle = contentManager.Load<Texture2D>("Reticle");

      // Get the sprite fonts used in the UI theme.
      var uiContentManager = services.GetInstance<ContentManager>("UIContent");
      _defaultFont = uiContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _fixedWidthFont = uiContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Console");

      // Set up 2D camera such that (0, 0) is upper, left corner of screen and 
      // (screenWidth, screenHeight) is lower, right corner of screen.
      var graphicsDevice = GraphicsService.GraphicsDevice;
      int screenWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
      int screenHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
      var projection = new OrthographicProjection
      {
        Near = 0, Far = 2000,
        Left = 0, Right = screenWidth,
        Top = 0, Bottom = screenHeight,
      };
      var camera = new Camera(projection);
      _cameraNode2D = new CameraNode(camera)
      {
        PoseWorld = new Pose(new Vector3F(0, 0, 1000)),
      };

      // Initialize renderers.
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _meshRenderer = new MeshRenderer();
      _billboardRenderer = new BillboardRenderer(GraphicsService, 2048);
      DebugRenderer2D = new DebugRenderer(GraphicsService, _defaultFont)
      {
        SpriteFont = _defaultFont,
        DefaultColor = new Color(0, 0, 0),
        DefaultTextPosition = new Vector2F(10)
      };
      DebugRenderer = new DebugRenderer(GraphicsService, _defaultFont)
      {
        SpriteFont = _defaultFont,
        DefaultColor = new Color(0, 0, 0),
        DefaultTextPosition = new Vector2F(10)
      };

      Scene = new Scene();
    }


    public void Dispose()
    {
      _spriteBatch.Dispose();
      _meshRenderer.Dispose();
      _billboardRenderer.Dispose();
      DebugRenderer2D.Dispose();
      DebugRenderer.Dispose();
      Scene.Dispose(false);
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Update the scene - this must be called each frame.
      Scene.Update(deltaTime);
    }


    protected override void OnRender(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      if (ClearBackground)
        graphicsDevice.Clear(BackgroundColor);

      if (CameraNode != null)
      {
        // ----- Render Scene.
        // Set the current camera and current scene in the render context. This 
        // information is very important; it is used by the scene node renderers.
        context.CameraNode = CameraNode;
        context.Scene = Scene;

        // Frustum Culling: Get all the scene nodes that intersect the camera frustum.
        var query = Scene.Query<CameraFrustumQuery>(context.CameraNode, context);

        // Set the render states for opaque objects.
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        // Render the meshes that are visible from the camera.
        // We must set a render pass. This is a string which tells the MeshRenderer
        // which effects it has to use to render the objects. The default render
        // pass is always called "Default". 
        context.RenderPass = "Default";
        _meshRenderer.Render(query.SceneNodes, context);
        context.RenderPass = null;

        // ----- For SkinnedEffectSample:
        // Render meshes with "Sky" material pass. The SkinnedEffectSample uses a
        // ProceduralSkyDome as the background. The material has a special "Sky"
        // pass. Depth writes need to be disabled.
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        context.RenderPass = "Sky";
        _meshRenderer.Render(query.SceneNodes, context);
        context.RenderPass = null;
        // -----

        // Set the render states for alpha blended objects.
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        graphicsDevice.BlendState = BlendState.NonPremultiplied;
        graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

        // The BillboardRenderer renders BillboardNodes and ParticleSystemNodes.
        _billboardRenderer.Render(query.SceneNodes, context);

        // Clean up.
        context.CameraNode = null;
        context.Scene = null;
      }

      var font = UseFixedWidthFont ? _fixedWidthFont : _defaultFont;
      DebugRenderer2D.SpriteFont = font;
      DebugRenderer.SpriteFont = font;

      // Render 3D graphics.
      if (CameraNode != null)
      {
        context.CameraNode = CameraNode;
        DebugRenderer.Render(context);
      }

      // Render 2D graphics.
      context.CameraNode = _cameraNode2D;
      DebugRenderer2D.Render(context);

      // Draw reticle.
      if (DrawReticle && _sampleFramework.IsGuiVisible)
      {
        var viewport = context.Viewport;
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        _spriteBatch.Draw(
          _reticle,
          new Vector2(viewport.Width / 2 - _reticle.Width / 2, viewport.Height / 2 - _reticle.Height / 2),
          Color.Black);
        _spriteBatch.End();
      }

      context.CameraNode = null;
    }
  }
}
