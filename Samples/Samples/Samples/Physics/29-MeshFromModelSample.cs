#if !ANDROID && !IOS   // Cannot read from vertex buffer in MonoGame/OpenGLES.
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates how to load a triangle mesh from an graphics model and use this 
mesh as a collision shape.",
    @"",
    29)]
  public class MeshFromModelSample : PhysicsSample
  {
    public MeshFromModelSample(Microsoft.Xna.Framework.Game game)
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

      // Use content pipeline to load a model.
      var bowlModelNode = ContentManager.Load<ModelNode>("Bowl");

      // Get mesh of the imported model.
      var meshNode = bowlModelNode.GetDescendants().OfType<MeshNode>().First();

      // Extract the triangle mesh from the DigitalRune Graphics Mesh instance. 
      // Note: XNA graphics use clockwise winding for triangle front sides and DigitalRune Physics uses
      // counter-clockwise winding for front sides. FromModel() automatically flips the 
      // winding order. 
      TriangleMesh mesh = MeshHelper.ToTriangleMesh(meshNode.Mesh);
      // Apply the transformation of the mesh node.
      mesh.Transform(meshNode.PoseWorld * Matrix44F.CreateScale(meshNode.ScaleWorld));

      // Note: To convert an XNA Model instance to a triangle mesh you can use:
      //TriangleMesh mesh = TriangleMesh.FromModel(bowlModel);

      // Meshes are usually "one-sided" (objects can pass through the backside of the triangles)!
      // If you need to reverse the triangle winding order, use this:
      // Reverse winding order:
      //for (int i = 0; i < mesh.NumberOfTriangles; i++)
      //{
      //  var dummy = mesh.Indices[i * 3 + 1];
      //  mesh.Indices[i * 3 + 1] = mesh.Indices[i * 3 + 2];
      //  mesh.Indices[i * 3 + 2] = dummy;
      //}

      // Create a collision shape that uses the mesh.
      TriangleMeshShape meshShape = new TriangleMeshShape(mesh);

      // Meshes are usually "one-sided" and objects moving into a backside can move through the
      // mesh. Objects are only stopped if they approach from the front. If IsTwoSided is set,
      // objects are blocked from both sides. 
      meshShape.IsTwoSided = true;

      // Optional: Assign a spatial partitioning scheme to the triangle mesh. (A spatial partition
      // adds an additional memory overhead, but it improves collision detection speed tremendously!)
      meshShape.Partition = new AabbTree<int>
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

      // Create a static rigid body with the mesh shape.
      // We explicitly specify a mass frame. We can use any mass frame for static bodies (because
      // static bodies are effectively treated as if they have infinite mass). If we do not specify 
      // a mass frame in the rigid body constructor, the constructor will automatically compute an 
      // approximate mass frame (which can take some time for large meshes).
      var bowlBody = new RigidBody(meshShape, new MassFrame(), null)
      {
        Name = "Bowl",
        Pose = new Pose(new Vector3F()),
        MotionType = MotionType.Static
      };
      Simulation.RigidBodies.Add(bowlBody);

      // Add a dynamic sphere.
      Shape sphereShape = new SphereShape(0.4f);
      RigidBody sphere = new RigidBody(sphereShape)
      {
        Name = "Sphere",
        Pose = new Pose(new Vector3F(0, 10, 0)),
      };
      Simulation.RigidBodies.Add(sphere);
    }
  }
}
#endif