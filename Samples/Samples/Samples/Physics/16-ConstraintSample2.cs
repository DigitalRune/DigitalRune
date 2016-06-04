using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates various constraints: PointOnLine, PointOnPlane, NoRotation, DistanceLimit.",
    "",
    16)]
  public class ConstraintSample2 : PhysicsSample
  {
    public ConstraintSample2(Microsoft.Xna.Framework.Game game)
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

      // ----- PointOnLineConstraint
      RigidBody box0 = new RigidBody(new BoxShape(0.1f, 0.5f, 6))
      {
        Pose = new Pose(new Vector3F(-4, 3, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box0);
      RigidBody sphere0 = new RigidBody(new SphereShape(0.5f))
      {
        Pose = new Pose(new Vector3F(-4, 2, 0))
      };
      Simulation.RigidBodies.Add(sphere0);
      PointOnLineConstraint pointOnLineConstraint = new PointOnLineConstraint
      {
        BodyA = box0,
        // The line goes through this point:
        AnchorPositionALocal = new Vector3F(0, -0.25f, 0),
        BodyB = sphere0,
        AnchorPositionBLocal = new Vector3F(0, 0.5f, 0),
        // The line axis:
        AxisALocal = Vector3F.UnitZ,
        // The movement on the line axis:
        Minimum = -3,
        Maximum = +3,
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(pointOnLineConstraint);

      // ----- PointOnPlaneConstraint
      RigidBody box1 = new RigidBody(new BoxShape(2f, 0.5f, 6))
      {
        Pose = new Pose(new Vector3F(-1, 3, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box1);
      RigidBody sphere1 = new RigidBody(new SphereShape(0.5f))
      {
        Pose = new Pose(new Vector3F(-1, 2, 0))
      };
      Simulation.RigidBodies.Add(sphere1);
      PointOnPlaneConstraint pointOnPlaneConstraint = new PointOnPlaneConstraint
      {
        BodyA = box1,
        // The plane goes through this point:
        AnchorPositionALocal = new Vector3F(0, -0.25f, 0),
        BodyB = sphere1,
        AnchorPositionBLocal = new Vector3F(0, 0.5f, 0),
        // Two orthonormal vectors that define the plane:
        XAxisALocal = Vector3F.UnitX,
        YAxisALocal = Vector3F.UnitZ,
        // Limits for the x axis and y axis movement:
        Minimum = new Vector2F(-1, -3),
        Maximum = new Vector2F(1, 3),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(pointOnPlaneConstraint);

      // ----- NoRotationConstraint
      // This constraint keeps two rotations synchronized. 
      RigidBody box2 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(2, 3, 0)),
      };
      Simulation.RigidBodies.Add(box2);
      RigidBody box3 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(2.5f, 2, 0))
      };
      Simulation.RigidBodies.Add(box3);
      NoRotationConstraint noRotationConstraint = new NoRotationConstraint
      {
        BodyA = box2,
        BodyB = box3,
        CollisionEnabled = true,
      };
      Simulation.Constraints.Add(noRotationConstraint);

      // ----- Distance limit.
      // Here, the distance of two cone tips is kept at a constant distance.
      RigidBody cone0 = new RigidBody(new ConeShape(0.5f, 1f))
      {
        Pose = new Pose(new Vector3F(4, 3, 0), Matrix33F.CreateRotationZ(ConstantsF.Pi)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(cone0);
      RigidBody cone1 = new RigidBody(new ConeShape(0.5f, 1f))
      {
        Pose = new Pose(new Vector3F(4, 0, 0))
      };
      Simulation.RigidBodies.Add(cone1);
      DistanceLimit distanceLimit = new DistanceLimit
      {
        BodyA = cone0,
        BodyB = cone1,
        // The attachment points are the tips of the cones.
        AnchorPositionALocal = new Vector3F(0, 1, 0),
        AnchorPositionBLocal = new Vector3F(0, 1, 0),
        // The tips should always have a distance of 0.5 units.
        MinDistance = 0.5f,
        MaxDistance = 0.5f,
      };
      Simulation.Constraints.Add(distanceLimit);
    }
  }
}
