using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a landscape using a triangle mesh.",
    "",
    12)]
  public class TriangleMeshSample : PhysicsSample
  {
    public TriangleMeshSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // The triangle mesh could be loaded from a file, such as an XNA Model.
      // In this example will use the same height field as in Sample11 and convert
      // the shape into a triangle mesh.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var heights = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
        for (int x = 0; x < numberOfSamplesX; x++)
          heights[z * numberOfSamplesX + x] = (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 5f + 5f);
      HeightField heightField = new HeightField(0, 0, 100, 100, heights, numberOfSamplesX, numberOfSamplesZ);

      // Convert the height field to a triangle mesh.
      ITriangleMesh mesh = heightField.GetMesh(0.01f, 3);

      // Create a shape for the triangle mesh.
      TriangleMeshShape triangleMeshShape = new TriangleMeshShape(mesh);

      // Optional: We can enable "contact welding". When this flag is enabled, the triangle shape
      // precomputes additional internal information for the mesh. The collision detection will 
      // be able to compute better contacts (e.g. better normal vectors at triangle edges).
      // Pro: Collision detection can compute better contact information.
      // Con: Contact welding information needs a bit of memory. And the collision detection is
      // a bit slower.
      triangleMeshShape.EnableContactWelding = true;

      // Optional: Assign a spatial partitioning scheme to the triangle mesh. (A spatial partition
      // adds an additional memory overhead, but it improves collision detection speed tremendously!)
      triangleMeshShape.Partition = new CompressedAabbTree
      {
        // The tree is automatically built using a mixed top-down/bottom-up approach. Bottom-up
        // building is slower but produces better trees. If the tree building takes too long,
        // we can lower the BottomUpBuildThreshold (default is 128).
        BottomUpBuildThreshold = 0,
      };

      // Optional: The partition will be automatically built when needed. For static meshes it is
      // built only once when it is needed for the first time. Building the AABB tree can take a 
      // few seconds for very large meshes.
      // By calling Update() manually we can force the partition to be built right now:
      //triangleMeshShape.Partition.Update(false);
      // We could also call this method in a background thread while the level is loading. Or,
      // we can build the triangle mesh and the AABB tree in the XNA content pipeline and avoid the
      // building of the tree at runtime (see Sample 33).

      // Create a static rigid body using the shape and add it to the simulation.
      // We explicitly specify a mass frame. We can use any mass frame for static bodies (because
      // static bodies are effectively treated as if they have infinite mass). If we do not specify 
      // a mass frame in the rigid body constructor, the constructor will automatically compute an 
      // approximate mass frame (which can take some time for large meshes).
      var ground = new RigidBody(triangleMeshShape, new MassFrame(), null)
      {
        Pose = new Pose(new Vector3F(-50, 0, -50f)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(ground);

      // Distribute a few spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(sphereShape)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(body);
      }

      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
