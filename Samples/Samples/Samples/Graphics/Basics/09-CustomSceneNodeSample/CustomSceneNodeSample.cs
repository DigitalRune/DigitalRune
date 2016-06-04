using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample shows how to create and render custom scene node types.",
    @"The sample defines a TextNode, which is a new type of scene node. The TextNode represents 
a text label at a 3D position in world space. To render the text labels a custom scene node 
renderer is implemented.",
    9)]
  public class CustomSceneNodeSample : Sample
  {
    private readonly CameraObject _cameraObject;

    private readonly Scene _scene;
    private readonly TextRenderer _textRenderer;
    private readonly DebugRenderer _debugRenderer;


    public CustomSceneNodeSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Add a custom game object which controls the camera.
      _cameraObject = new CameraObject(Services);
      GameObjectService.Objects.Add(_cameraObject);

      // Create a new empty scene.
      _scene = new Scene();

      // Add the camera node to the scene.
      _scene.Children.Add(_cameraObject.CameraNode);

      // Add a few TextNodes. Position them along a circle.
      for (int i = 0; i < 36; i++)
      {
        Vector3F position = Matrix33F.CreateRotationZ(MathHelper.ToRadians((float)i * 10)) * new Vector3F(1, 0, 0);
        var textNode = new TextNode
        {
          PoseLocal = new Pose(position),
          Color = Color.Yellow,
          Text = i.ToString()
        };
        _scene.Children.Add(textNode);
      }

      // Initialize the TextRenderer.
      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      _textRenderer = new TextRenderer(GraphicsService, spriteFont);

      // For debugging:
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont);
    }



    public override void Update(GameTime gameTime)
    {
      // Just for debugging: Draw coordinate axes at (0, 0, 0).
      _debugRenderer.Clear();
      _debugRenderer.DrawAxes(Pose.Identity, 0.5f, true);

      // Update the scene - this must be called once per frame.
      _scene.Update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Set render context info.
      context.CameraNode = _cameraObject.CameraNode;
      context.Scene = _scene;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Frustum culling: Get all scene nodes which overlap the view frustum.
      var query = _scene.Query<CameraFrustumQuery>(context.CameraNode, context);

      // Render all TextNodes that are in the camera frustum.
      _textRenderer.Render(query.SceneNodes, context);

      // Draw debug info.
      _debugRenderer.Render(context);

      // Clean up.
      context.Scene = null;
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // IMPORTANT: Dispose scene nodes if they are no longer needed!
        _scene.Dispose(false);  // Disposes current and all descendant nodes.

        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
