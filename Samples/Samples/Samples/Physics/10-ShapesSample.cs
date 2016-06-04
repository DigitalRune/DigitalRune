using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create rigid bodies with different shapes.",
    "",
    10)]
  public class ShapesSample : PhysicsSample
  {
    public ShapesSample(Microsoft.Xna.Framework.Game game)
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

      // ----- Add a sphere.
      Shape sphere = new SphereShape(0.5f);
      Simulation.RigidBodies.Add(new RigidBody(sphere));

      // ----- Add a box.
      BoxShape box = new BoxShape(0.5f, 0.9f, 0.7f);
      Simulation.RigidBodies.Add(new RigidBody(box));

      // ----- Add a capsule.
      CapsuleShape capsule = new CapsuleShape(0.4f, 1.2f);
      Simulation.RigidBodies.Add(new RigidBody(capsule));

      // ----- Add a cone.
      ConeShape cone = new ConeShape(0.5f, 1f);
      Simulation.RigidBodies.Add(new RigidBody(cone));

      // ----- Add a cylinder.
      CylinderShape cylinder = new CylinderShape(0.3f, 1f);
      Simulation.RigidBodies.Add(new RigidBody(cylinder));

      // ----- Add a convex hull of random points.
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints();
      for (int i = 0; i < 20; i++)
        convexHullOfPoints.Points.Add(RandomHelper.Random.NextVector3F(-0.5f, 0.5f));
      Simulation.RigidBodies.Add(new RigidBody(convexHullOfPoints));

      // ----- Add a convex polyhedron. 
      // (A ConvexPolyhedron is similar to the ConvexHullOfPoints. The difference is that 
      // the points in a ConvexHullOfPoints can be changed at runtime. A ConvexPolyhedron 
      // cannot be changed at runtime, but it is faster.)
      List<Vector3F> points = new List<Vector3F>();
      for (int i = 0; i < 20; i++)
        points.Add(RandomHelper.Random.NextVector3F(-0.7f, 0.7f));
      ConvexPolyhedron convexPolyhedron = new ConvexPolyhedron(points);
      Simulation.RigidBodies.Add(new RigidBody(convexPolyhedron));

      // ----- Add a composite shape (a table that consists of 5 boxes).
      CompositeShape composite = new CompositeShape();
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 0.8f, 0.1f), new Pose(new Vector3F(-0.75f, 0.4f, -0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 0.8f, 0.1f), new Pose(new Vector3F(0.75f, 0.4f, -0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 0.8f, 0.1f), new Pose(new Vector3F(-0.75f, 0.4f, 0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 0.8f, 0.1f), new Pose(new Vector3F(0.75f, 0.4f, 0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(1.8f, 0.1f, 1.1f), new Pose(new Vector3F(0, 0.8f, 0))));
      Simulation.RigidBodies.Add(new RigidBody(composite));

      // ----- Add a convex hull of multiple shapes.
      ConvexHullOfShapes convexHullOfShapes = new ConvexHullOfShapes();
      convexHullOfShapes.Children.Add(new GeometricObject(new CylinderShape(0.2f, 0.8f), new Pose(new Vector3F(-0.4f, 0, 0))));
      convexHullOfShapes.Children.Add(new GeometricObject(new CylinderShape(0.2f, 0.8f), new Pose(new Vector3F(+0.4f, 0, 0))));
      Simulation.RigidBodies.Add(new RigidBody(convexHullOfShapes));

      // ----- Add the Minkowski sum of two shapes. 
      // (The Minkowski sum is a mathematical operation that combines two shapes. 
      // Here a circle is combined with a sphere. The result is a wheel.)
      MinkowskiSumShape minkowskiSum = new MinkowskiSumShape();
      minkowskiSum.ObjectA = new GeometricObject(new SphereShape(0.2f), Pose.Identity);
      minkowskiSum.ObjectB = new GeometricObject(new CircleShape(0.5f), Pose.Identity);
      Simulation.RigidBodies.Add(new RigidBody(minkowskiSum));

      // Create another Minkowski sum. (Here a small sphere is added to a box to create a 
      // box with rounded corners.)
      minkowskiSum = new MinkowskiSumShape();
      minkowskiSum.ObjectA = new GeometricObject(new SphereShape(0.1f), Pose.Identity);
      minkowskiSum.ObjectB = new GeometricObject(new BoxShape(0.2f, 0.5f, 0.8f), Pose.Identity);
      Simulation.RigidBodies.Add(new RigidBody(minkowskiSum));

      // ----- Add a triangle mesh. 
      // A triangle mesh could be loaded from a file or built from an XNA model. 
      // Here we first create a composite shape and convert the shape into a triangle 
      // mesh. (Any Shape in DigitalRune.Geometry can be converted to a triangle mesh.)
      CompositeShape dumbbell = new CompositeShape();
      dumbbell.Children.Add(new GeometricObject(new SphereShape(0.4f), new Pose(new Vector3F(0.6f, 0.0f, 0.0f))));
      dumbbell.Children.Add(new GeometricObject(new SphereShape(0.4f), new Pose(new Vector3F(-0.6f, 0.0f, 0.0f))));
      dumbbell.Children.Add(new GeometricObject(new CylinderShape(0.1f, 0.6f), new Pose(Matrix33F.CreateRotationZ(ConstantsF.PiOver2))));

      TriangleMeshShape triangleMeshShape = new TriangleMeshShape(dumbbell.GetMesh(0.01f, 2));

      // Optional: We can enable "contact welding". When this flag is enabled, the triangle shape
      // precomputes additional internal information for the mesh. The collision detection will 
      // be able to compute better contacts (e.g. better normal vectors at triangle edges).
      // Pro: Collision detection can compute better contact information.
      // Con: Contact welding information needs a bit of memory. And the collision detection is
      // a bit slower.
      triangleMeshShape.EnableContactWelding = true;

      // Optional: Assign a spatial partitioning scheme to the triangle mesh. (A spatial partition
      // adds an additional memory overhead, but it improves collision detection speed tremendously!)
      triangleMeshShape.Partition = new AabbTree<int>()
      {
        // The tree is automatically built using a mixed top-down/bottom-up approach. Bottom-up
        // building is slower but produces better trees. If the tree building takes too long,
        // we can lower the BottomUpBuildThreshold (default is 128).
        BottomUpBuildThreshold = 0,
      };

      Simulation.RigidBodies.Add(new RigidBody(triangleMeshShape));

      // ----- Set random positions/orientations.
      // (Start with the second object. The first object is the ground plane which should
      // not be changed.)
      for (int i = 1; i < Simulation.RigidBodies.Count; i++)
      {
        RigidBody body = Simulation.RigidBodies[i];

        Vector3F position = RandomHelper.Random.NextVector3F(-3, 3);
        position.Y = 3;   // Position the objects 3m above ground.
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();
        body.Pose = new Pose(position, orientation);
      }
    }
  }
}
