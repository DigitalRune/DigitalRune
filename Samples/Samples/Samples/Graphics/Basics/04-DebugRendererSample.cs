using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample demonstrates the DebugRenderer.",
    @"The DebugRenderer helps to render simple graphics to display debug information:
text, points, lines, arrows, bounding boxes, geometric objects, ...",
    4)]
  public class DebugRendererSample : Sample
  {
    private readonly CameraObject _cameraObject;
    private readonly DebugRenderer _debugRenderer;
    private readonly Model _xnaModel;
    private readonly ModelNode _modelNode;
    private readonly GeometricObject _geometricObject;


    public DebugRendererSample(Microsoft.Xna.Framework.Game game)
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

      // Load a sprite font.
      var spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");

      // Create a new debug renderer.
      _debugRenderer = new DebugRenderer(GraphicsService, spriteFont)
      {
        DefaultColor = Color.White,
      };

      // A normal XNA model.
      _xnaModel = ContentManager.Load<Model>("Saucer3/saucer");

      // A DigitalRune model.
      _modelNode = ContentManager.Load<ModelNode>("Dude/Dude").Clone();
      _modelNode.PoseLocal = new Pose(new Vector3F(6, 0, -7));

      // Create a geometric object with a height field shape.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
        for (int x = 0; x < numberOfSamplesX; x++)
          samples[z * numberOfSamplesX + x] = 1.0f + (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 1.0f);
      HeightField heightField = new HeightField(0, 0, 120, 120, samples, numberOfSamplesX, numberOfSamplesZ);
      _geometricObject = new GeometricObject(heightField, new Pose(new Vector3F(5, 0, -5)))
      {
        Scale = new Vector3F(0.01f, 0.05f, 0.02f),
      };
    }


    public override void Update(GameTime gameTime)
    {
      Random random = new Random(1234567);

      // The debug renderer stores all draw commands. In this sample we recreate 
      // the draw jobs each frame. --> Clear draw jobs of last frame.
      _debugRenderer.Clear();

      // The DebugRenderer can draw stuff "in" the scene (enabled z test) or "over" 
      // the scene (disabled z test). 

      // Draw some points and line "in" and "over" the scene.
      for (int i = 0; i < 10; i++)
      {
        var position = new Vector3F(-6, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        _debugRenderer.DrawPoint(position, Color.Green, false);
      }

      for (int i = 0; i < 10; i++)
      {
        var position = new Vector3F(-4, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        _debugRenderer.DrawPoint(position, Color.Yellow, true);
      }

      for (int i = 0; i < 10; i++)
      {
        var start = new Vector3F(-2, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        var end = new Vector3F(-2, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        _debugRenderer.DrawLine(start, end, Color.Green, false);
      }

      for (int i = 0; i < 10; i++)
      {
        var start = new Vector3F(0, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        var end = new Vector3F(0, 0, -3) + random.NextVector3F(-0.5f, 0.5f);
        _debugRenderer.DrawLine(start, end, Color.Yellow, true);
      }

      // Text without a specified position is drawn at a default position.
      _debugRenderer.DefaultTextPosition = new Vector2F(10, 100);
      _debugRenderer.DrawText("White objects are positioned in screen space.");
      _debugRenderer.DrawText("Yellow objects are positioned in world space. Depth test disabled.");
      _debugRenderer.DrawText("Other objects are positioned in world space. Depth test enabled.");

      // Text can also be drawn in world space coordinates or in screen space coordinates.
      _debugRenderer.DrawText("WorldSpacePosition (0, 0, 0)", new Vector3F(0, 0, 0), Color.Green, false);
      _debugRenderer.DrawText("WorldSpacePosition (0, 0, -1)", new Vector3F(0, 0, -1), Color.Yellow, true);
      _debugRenderer.DrawText("ScreenPosition (600, 40)", new Vector2F(600, 40), Color.White);
      _debugRenderer.DrawText("ScreenPosition (640, 360)", new Vector2F(640, 360), Color.White);

      // It is often useful to copy textures to the screen for debugging.
      _debugRenderer.DrawTexture(NoiseHelper.GetGrainTexture(GraphicsService, 128), new Rectangle(1000, 10, 128, 128));

      // Axes can be drawn to display poses (positions and orientations).
      _debugRenderer.DrawAxes(new Pose(new Vector3F(0, 0, 0)), 1, true);

      // Axis-aligned bounding boxes (AABB)
      _debugRenderer.DrawAabb(new Aabb(new Vector3F(-0.5f), new Vector3F(0.5f)), new Pose(new Vector3F(2, 0, -3)), Color.Green, false);
      _debugRenderer.DrawAabb(new Aabb(new Vector3F(-0.5f), new Vector3F(0.5f)), new Pose(new Vector3F(4, 0, -3)), Color.Yellow, true);

      // Box shapes
      var orientation = random.NextQuaternionF();
      _debugRenderer.DrawBox(1, 1, 1, new Pose(new Vector3F(-6, 0, -5), orientation), new Color(255, 0, 0, 100), false, false);
      _debugRenderer.DrawBox(1, 1, 1, new Pose(new Vector3F(-6, 0, -5), orientation), Color.Green, true, false);
      _debugRenderer.DrawBox(1, 1, 1, new Pose(new Vector3F(-4, 0, -5), orientation), Color.Yellow, true, true);

      // View volumes (frustums)
      var viewVolume = new PerspectiveViewVolume(1, 2, 0.1f, 1f);
      _debugRenderer.DrawViewVolume(viewVolume, new Pose(new Vector3F(-2, 0, -5), orientation), new Color(0, 255, 0, 100), false, false);
      _debugRenderer.DrawViewVolume(viewVolume, new Pose(new Vector3F(-2, 0, -5), orientation), Color.Green, true, false);
      _debugRenderer.DrawViewVolume(viewVolume, new Pose(new Vector3F(0, 0, -5), orientation), Color.Yellow, true, true);

      // Spheres
      _debugRenderer.DrawSphere(0.5f, new Pose(new Vector3F(2, 0, -5), orientation), new Color(0, 0, 255, 100), false, false);
      _debugRenderer.DrawSphere(0.5f, new Pose(new Vector3F(2, 0, -5), orientation), Color.Green, true, false);
      _debugRenderer.DrawSphere(0.5f, new Pose(new Vector3F(4, 0, -5), orientation), Color.Yellow, true, true);

      // Capsules
      _debugRenderer.DrawCapsule(0.3f, 1, new Pose(new Vector3F(-6, 0, -7), orientation), new Color(255, 255, 0, 100), false, false);
      _debugRenderer.DrawCapsule(0.3f, 1, new Pose(new Vector3F(-6, 0, -7), orientation), Color.Green, true, false);
      _debugRenderer.DrawCapsule(0.3f, 1, new Pose(new Vector3F(-4, 0, -7), orientation), Color.Yellow, true, true);

      // Cylinders
      _debugRenderer.DrawCylinder(0.3f, 1, new Pose(new Vector3F(-2, 0, -7), orientation), new Color(255, 0, 255, 100), false, false);
      _debugRenderer.DrawCylinder(0.3f, 1, new Pose(new Vector3F(-2, 0, -7), orientation), Color.Green, true, false);
      _debugRenderer.DrawCylinder(0.3f, 1, new Pose(new Vector3F(0, 0, -7), orientation), Color.Yellow, true, true);

      // Cones
      _debugRenderer.DrawCone(0.3f, 1, new Pose(new Vector3F(2, 0, -7), orientation), new Color(0, 255, 255, 100), false, false);
      _debugRenderer.DrawCone(0.3f, 1, new Pose(new Vector3F(2, 0, -7), orientation), Color.Green, true, false);
      _debugRenderer.DrawCone(0.3f, 1, new Pose(new Vector3F(4, 0, -7), orientation), Color.Yellow, true, true);

      // The debug renderer can draw any IGeometricObjects, like SceneNodes, RigidBodies, etc.
      _debugRenderer.DrawObject(_geometricObject, Color.Brown, false, false);
      _debugRenderer.DrawObject(_geometricObject, Color.Yellow, true, true);

      // The debug renderer can also an XNA model (without materials).
      _debugRenderer.DrawModel(_xnaModel, new Pose(new Vector3F(0, 2, -2), orientation), new Vector3F(1, 2, 1), new Color(128, 255, 64, 100), false, false);
      _debugRenderer.DrawModel(_xnaModel, new Pose(new Vector3F(0, 2, -2), orientation), new Vector3F(1, 2, 1), Color.LightCyan, true, false);

      // Draw a DigitalRune model.
      _debugRenderer.DrawModel(_modelNode, Color.Peru, true, false);

      // Draw the bounding shapes of the meshes in this model.
      foreach (var node in _modelNode.GetSubtree())
        _debugRenderer.DrawObject(node, Color.PeachPuff, true, false);
    }


    private void Render(RenderContext context)
    {
      // The debug renderer uses the camera node set in the render context.
      // Without a camera node the debug renderer cannot draw any 3D objects.
      context.CameraNode = _cameraObject.CameraNode;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Render the debug info. Internally, the DebugRenderer sorts the draw jobs 
      // and renders them in batches.
      _debugRenderer.Render(context);

      // Clean up.
      context.CameraNode = null;
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _modelNode.Dispose(false);
        _debugRenderer.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
