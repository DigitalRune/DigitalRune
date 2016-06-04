#if XNA
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "Loads a height field that was generated in the XNA content pipeline.",
    "",
    32)]
  public class ContentPipelineHeightFieldSample : PhysicsSample
  {
    // This sample uses the HeightFieldProcessor (see project "Samples.Content.Pipeline"). 
    // This content pipeline processor reads a grayscale texture and creates a model and a height 
    // field shape.

    private ModelNode _heightFieldModelNode;
    private RigidBody _heightFieldBody;


    public ContentPipelineHeightFieldSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Load height field model and add it to the graphics scene.
      _heightFieldModelNode = ContentManager.Load<ModelNode>("HeightField/TerrainHeights").Clone();
      GraphicsScreen.Scene.Children.Add(_heightFieldModelNode);

      // The UserData contains the collision shape of type HeightField.
      HeightField heightField = (HeightField)_heightFieldModelNode.UserData;

      _heightFieldModelNode.PoseWorld = new Pose(new Vector3F(-heightField.WidthX / 2, 0, -heightField.WidthZ / 2));

      // Create rigid body.
      _heightFieldBody = new RigidBody(heightField, null, null)
      {
        MotionType = MotionType.Static,
        Pose = _heightFieldModelNode.PoseWorld,

        // The PhysicsSample class should not draw the height field. 
        UserData = "NoDraw",
      };
      Simulation.RigidBodies.Add(_heightFieldBody);

      // Distribute a few spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(sphereShape) { Pose = new Pose(position) };
        Simulation.RigidBodies.Add(body);
      }

      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(boxShape) { Pose = new Pose(position) };
        Simulation.RigidBodies.Add(body);
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _heightFieldModelNode.Dispose(false);

        // Detach shape from rigid body to avoid any "memory leaks".
        _heightFieldBody.Shape = Shape.Empty;
      }

      base.Dispose(disposing);
    }
  }
}
#endif