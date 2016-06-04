using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to detect collisions between many moving objects.",
    @"A box is created with 6 planes. Objects with different shapes fly around in the box
and bounce off of each other.",
    4)]
  public class MassCollisionsSample : BasicSample
  {
    // Create a custom GeometricObject class which stores a LinearVelocity.
    private class MovingGeometricObject : GeometricObject
    {
      public Vector3F LinearVelocity { get; set; }
      public Vector3F AngularVelocity { get; set; }
    }

    private float _defaultPlaneShapeMeshSize;

    // A few constants.
    private const int NumberOfObjects = 150;                    // Number of moving objects.
    private const float BoxSize = 5f;                           // The size of the box where objects can move.
    private const float ObjectSize = 0.5f;                      // The preferred size of a single object.
    private const float MaxLinearVelocity = 5;                  // The maximal linear speed of an object.
    private const float MaxAngularVelocity = ConstantsF.TwoPi;  // The maximal angular speed of an object.

    // The collision domain that manages collision objects.
    private readonly CollisionDomain _domain;


    public MassCollisionsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 0, 20), 0, 0);

      // We use one collision domain that manages all objects.
      _domain = new CollisionDomain(new CollisionDetection());

      // Create the 6 box planes.
      CreateBoundaryPlanes();

      // Create a lot of random objects.
      CreateRandomObjects();
    }


    // Creates 6 planes that form a box. Objects can move inside the box.
    private void CreateBoundaryPlanes()
    {
      // Planes (class DigitalRune.Geometry.Shapes.PlaneShape) are infinite planes that 
      // divide the world into two half-spaces. The space into which the plane normal
      // is pointing is "empty" and the other space is considered "solid".

      // Planes are infinite, but to draw a visual representation of the plane a mesh 
      // is generated with plane.GetMesh() in the DebugRenderer. Rectangle meshes
      // are automatically generated and the size of these rectangles is defined with
      // PlaneShape.MeshSize. 
      // Here we set MeshSize such that rectangles for each plane have the size of 
      // a box face - but don't forget that planes are infinite for the collision detection.
      _defaultPlaneShapeMeshSize = PlaneShape.MeshSize;
      PlaneShape.MeshSize = BoxSize * 2;

      // Left plane.
      var leftPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(Vector3F.UnitX, 0),
        Pose = new Pose(new Vector3F(-BoxSize, 0, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(leftPlane));

      // Right plane.
      var rightPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3F.UnitX, 0),
        Pose = new Pose(new Vector3F(BoxSize, 0, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(rightPlane));

      // Top plane.
      var topPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3F.UnitY, 0),
        Pose = new Pose(new Vector3F(0, BoxSize, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(topPlane));

      // Bottom plane.
      var bottomPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(Vector3F.UnitY, 0),
        Pose = new Pose(new Vector3F(0, -BoxSize, 0)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(bottomPlane));

      // Front plane.
      var frontPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(-Vector3F.UnitZ, 0),
        Pose = new Pose(new Vector3F(0, 0, BoxSize)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(frontPlane));

      // Back plane.
      var backPlane = new MovingGeometricObject
      {
        Shape = new PlaneShape(Vector3F.UnitZ, 0),
        Pose = new Pose(new Vector3F(0, 0, -BoxSize)),
      };
      _domain.CollisionObjects.Add(new CollisionObject(backPlane));
    }


    // Creates a lot of random objects.
    private void CreateRandomObjects()
    {
      var random = new Random(12345);

      for (int i = 0; i < NumberOfObjects; i++)
      {
        // Randomly choose a shape.
        Shape randomShape;
        switch (random.Next(0, 7))
        {
          case 0:
            // Box
            randomShape = new BoxShape(ObjectSize, ObjectSize * 2, ObjectSize * 3);
            break;
          case 1:
            // Capsule
            randomShape = new CapsuleShape(0.3f * ObjectSize, 2 * ObjectSize);
            break;
          case 2:
            // Cone
            randomShape = new ConeShape(1 * ObjectSize, 2 * ObjectSize);
            break;
          case 3:
            // Cylinder
            randomShape = new CylinderShape(0.4f * ObjectSize, 2 * ObjectSize);
            break;
          case 4:
            // Sphere
            randomShape = new SphereShape(ObjectSize);
            break;
          case 5:
            // Convex hull of several points.
            ConvexHullOfPoints hull = new ConvexHullOfPoints();
            hull.Points.Add(new Vector3F(-1 * ObjectSize, -2 * ObjectSize, -1 * ObjectSize));
            hull.Points.Add(new Vector3F(2 * ObjectSize, -1 * ObjectSize, -0.5f * ObjectSize));
            hull.Points.Add(new Vector3F(1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize));
            hull.Points.Add(new Vector3F(-1 * ObjectSize, 2 * ObjectSize, 1 * ObjectSize));
            hull.Points.Add(new Vector3F(-1 * ObjectSize, 0.7f * ObjectSize, -0.6f * ObjectSize));
            randomShape = hull;
            break;
          case 6:
            // A composite shape: two boxes that form a "T" shape.
            var composite = new CompositeShape();
            composite.Children.Add(
              new GeometricObject(
                new BoxShape(ObjectSize, 3 * ObjectSize, ObjectSize),
                new Pose(new Vector3F(0, 0, 0))));
            composite.Children.Add(
              new GeometricObject(
                new BoxShape(2 * ObjectSize, ObjectSize, ObjectSize),
                new Pose(new Vector3F(0, 2 * ObjectSize, 0))));
            randomShape = composite;
            break;
          default:
#if WINDOWS
            Trace.Fail("Ups, we shouldn't land here :-(");
#endif
            randomShape = new SphereShape();
            break;
        }

        // Create an object with the random shape, pose, color and velocity.
        Pose randomPose = new Pose(
          random.NextVector3F(-BoxSize + ObjectSize * 2, BoxSize - ObjectSize * 2),
          random.NextQuaternionF());

        var newObject = new MovingGeometricObject
        {
          Pose = randomPose,
          Shape = randomShape,
          LinearVelocity = random.NextQuaternionF().Rotate(new Vector3F(MaxLinearVelocity, 0, 0)),
          AngularVelocity = random.NextQuaternionF().Rotate(Vector3F.Forward)
                            * RandomHelper.Random.NextFloat(0, MaxAngularVelocity),
        };

        // Add collision object to collision domain.
        _domain.CollisionObjects.Add(new CollisionObject(newObject));

        // We will collect a few statistics for debugging.
        Profiler.SetFormat("NumObjects", 1, "The total number of objects.");
        Profiler.SetFormat("NumObjectPairs", 1, "The number of objects pairs, which have to be checked.");
        Profiler.SetFormat("BroadPhasePairs", 1, "The number of overlaps reported by the broad phase.");
        Profiler.SetFormat("ContactSetCount", 1, "The number of actual collisions.");
      }
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Reset default values.
        PlaneShape.MeshSize = _defaultPlaneShapeMeshSize;
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Reflect velocity if objects collide:
      // The collision domain contains a ContactSet for each pair of touching objects.
      foreach (var contactSet in _domain.ContactSets)
      {
        // Get the touching objects.
        var objectA = (MovingGeometricObject)contactSet.ObjectA.GeometricObject;
        var objectB = (MovingGeometricObject)contactSet.ObjectB.GeometricObject;

        // In rare cases, the collision detection cannot compute a contact because of
        // numerical problems. Ignore this case, we usually get a contact in the next frame.
        if (contactSet.Count == 0)
          continue;

        // Get the contact normal of the first collision point.
        var contact = contactSet[0];
        Vector3F normal = contact.Normal;

        // Check if the objects move towards or away from each other in the direction of the normal.
        if (Vector3F.Dot(objectB.LinearVelocity - objectA.LinearVelocity, normal) <= 0)
        {
          // Objects move towards each other. --> Reflect their velocities.
          objectA.LinearVelocity -= 2 * Vector3F.ProjectTo(objectA.LinearVelocity, normal);
          objectB.LinearVelocity -= 2 * Vector3F.ProjectTo(objectB.LinearVelocity, normal);
          objectA.AngularVelocity = -objectA.AngularVelocity;
          objectB.AngularVelocity = -objectB.AngularVelocity;
        }
      }

      // Get the size of the current time step.
      float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Move objects.
      var objects = _domain.CollisionObjects.Select(co => co.GeometricObject).OfType<MovingGeometricObject>();
      foreach (var obj in objects)
      {
        // Update position.
        Vector3F position = obj.Pose.Position + obj.LinearVelocity * timeStep;

        // Update rotation.
        Vector3F rotationAxis = obj.AngularVelocity;
        float angularSpeed = obj.AngularVelocity.Length;
        Matrix33F rotation = (Numeric.IsZero(angularSpeed))
          ? Matrix33F.Identity
          : Matrix33F.CreateRotation(rotationAxis, angularSpeed * timeStep);
        var orientation = rotation * obj.Pose.Orientation;

        // Incrementally updating the rotation matrix will eventually create a 
        // matrix which is not a rotation matrix anymore because of numerical 
        // problems. Re-othogonalization makes sure that the matrix represents a
        // rotation.
        orientation.Orthogonalize();

        obj.Pose = new Pose(position, orientation);
      }

      // Update collision domain. This computes new contact information.
      _domain.Update(timeStep);

      // Record some statistics.
      int numberOfObjects = _domain.CollisionObjects.Count;
      Profiler.AddValue("NumObjects", numberOfObjects);

      // If there are n objects, we can have max. n * (n - 1) / 2 collisions.
      Profiler.AddValue("NumObjectPairs", numberOfObjects * (numberOfObjects - 1f) / 2f);

      // The first part of the collision detection is the "broad-phase" which
      // filters out objects that cannot collide (e.g. using a fast bounding box test).
      Profiler.AddValue("BroadPhasePairs", _domain.NumberOfBroadPhaseOverlaps);

      // Finally, the collision detection computes the exact contact information and creates
      // a ContactSet with the Contacts for each pair of colliding objects.
      Profiler.AddValue("ContactSetCount", _domain.ContactSets.Count);

      // Draw objects using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();
      foreach (var collisionObject in _domain.CollisionObjects)
        debugRenderer.DrawObject(collisionObject.GeometricObject, GraphicsHelper.GetUniqueColor(collisionObject), false, false);
    }
  }
}
