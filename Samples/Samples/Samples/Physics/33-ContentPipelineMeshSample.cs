using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "Loads a large model that was generated in the XNA content pipeline.",
    "",
    33)]
  public class ContentPipelineMeshSample : PhysicsSample
  {
    // This sample uses the ModelWithCollisionMeshProcessor (see project "Samples.Content.Pipeline"). 
    // This content pipeline processor reads a creates a model for rendering together with a 
    // TriangleMeshShape for collision detection.
    

    private readonly ModelNode _modelNode;
    private readonly RigidBody _rigidBody;


    public ContentPipelineMeshSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",           // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // ----- Load triangle mesh model.
      var sharedModelNode = ContentManager.Load<ModelNode>("Saucer/saucer");

      // Let's create a clone because we do not want to change the shared Saucer 
      // instance stored in the content manager.
      _modelNode = sharedModelNode.Clone();
      
      _modelNode.PoseWorld = new Pose(new Vector3F(0, 2, -5), Matrix33F.CreateRotationY(MathHelper.ToRadians(-30)));
      _modelNode.ScaleLocal = new Vector3F(8);

      // The UserData contains the collision shape of type TriangleMeshShape.
      TriangleMeshShape triangleMesh = (TriangleMeshShape)_modelNode.UserData;

      // Add model node to graphics scene.
      GraphicsScreen.Scene.Children.Add(_modelNode);

      // Create rigid body.
      // We explicitly specify a mass frame. We can use any mass frame for static bodies (because
      // static bodies are effectively treated as if they have infinite mass). If we do not specify
      // a mass frame in the rigid body constructor, the constructor will automatically compute an
      // approximate mass frame (which can take some time for large meshes).
      _rigidBody = new RigidBody(triangleMesh, new MassFrame(), null)
      {
        MotionType = MotionType.Static,
        Pose = _modelNode.PoseWorld,
        Scale = _modelNode.ScaleWorld,
        
        // The PhysicsSample class should not draw the height field. 
        UserData = "NoDraw",
      };
      Simulation.RigidBodies.Add(_rigidBody);

      // Distribute a few spheres and boxes across the triangle mesh.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 40; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-15, 10);
        position.Y = RandomHelper.Random.NextFloat(20, 40);

        RigidBody body = new RigidBody(sphereShape) { Pose = new Pose(position) };
        Simulation.RigidBodies.Add(body);
      }

      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 40; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-15, 10);
        position.Y = RandomHelper.Random.NextFloat(20, 40);

        RigidBody body = new RigidBody(boxShape) { Pose = new Pose(position) };
        Simulation.RigidBodies.Add(body);
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _modelNode.Dispose(false);

        // Detach shape from rigid body to avoid any "memory leaks".
        _rigidBody.Shape = Shape.Empty;
      }

      base.Dispose(disposing);
    }
  }
}
