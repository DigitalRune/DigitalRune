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
    @"This sample demonstrates various constraints: FixedJoint, BallJoint, PrismaticJoint,
CylindrialJoint, HingeJoint.",
    @"",
    15)]
  public class ConstraintSample1 : PhysicsSample
  {
    public ConstraintSample1(Microsoft.Xna.Framework.Game game)
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

      // Tip: It is best to initialize bodies in a position where the constraints are 
      // satisfied - but even if they are initialized in other poses, the simulation
      // will try to move them to the correct positions.

      // ----- FixedJoint
      // Create two boxes and connect them with a FixedJoint.      
      RigidBody box0 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(-5, 3, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box0);
      RigidBody box1 = new RigidBody(new BoxShape(1.2f, 1.2f, 1.2f))
      {
        Pose = new Pose(new Vector3F(-5, 3 - 1.2f, 0))
      };
      Simulation.RigidBodies.Add(box1);
      FixedJoint fixedJoint = new FixedJoint
      {
        BodyA = box0,
        // The attachment point on the first box is at the bottom of the box.
        AnchorPoseALocal = new Pose(new Vector3F(0, -0.5f, 0)),

        BodyB = box1,
        // The attachment point on the second box is at the top of the box.
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0.6f, 0)),

        // Disable collision between the connected bodies.
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(fixedJoint);

      // ----- BallJoint
      // Create two boxes and connect a corner of the second box with a ball-and-socked joint
      // to the first box.
      RigidBody box2 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(-3, 3, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box2);
      RigidBody box3 = new RigidBody(new BoxShape(1.2f, 1.2f, 1.2f))
      {
        Pose = new Pose(new Vector3F(-3, 3, 0))
      };
      Simulation.RigidBodies.Add(box3);
      BallJoint ballJoint = new BallJoint
      {
        BodyA = box2,
        // The attachment point on the first box is at the box bottom.
        AnchorPositionALocal = new Vector3F(0, -0.5f, 0),

        BodyB = box3,
        // The attachment point on the second box is a corner of the box.
        AnchorPositionBLocal = new Vector3F(0.6f, 0.6f, 0.6f),
      };
      Simulation.Constraints.Add(ballJoint);

      // ----- PrismaticJoint
      // A prismatic joint is like a slider (without rotation around the slider axis).
      RigidBody box4 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(-1, 3, 0)),
      };
      Simulation.RigidBodies.Add(box4);
      RigidBody box5 = new RigidBody(new BoxShape(0.5f, 1.5f, 0.5f))
      {
        Pose = new Pose(new Vector3F(-1, 3 - 0.5f, 0))
      };
      Simulation.RigidBodies.Add(box5);
      PrismaticJoint prismaticJoint = new PrismaticJoint
      {
        BodyA = box4,
        // The attachment point on the first box is in the center of the box.
        // The slider joint allows linear movement along the first constraint axis.
        // --> To define the constraint anchor orientation:
        // The columns are the axes. We set the local -y axis in the first column. This is the
        // slider axis. The other two columns are two orthonormal axes.
        // (All three columns are orthonormal and form a valid rotation matrix.)
        AnchorPoseALocal = new Pose(new Vector3F(0, 0, 0), new Matrix33F(0, 1, 0,
                                                                        -1, 0, 0,
                                                                         0, 0, 1)),
        BodyB = box5,
        // The attachment point on the second box is at the top of the box.
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0.75f, 0), new Matrix33F(0, 1, 0,
                                                                            -1, 0, 0,
                                                                             0, 0, 1)),
        CollisionEnabled = false,
        // The slider axis is -y. We limit the up movement, so that the anchor point on the second
        // body can slide up to the anchor point on the first body, but not higher.
        Minimum = 0,
        // The second body can slide down until the anchor points have a max. distance of 0.5.
        Maximum = 0.5f,
      };
      Simulation.Constraints.Add(prismaticJoint);

      // ----- CylindricalJoint
      // A cylindrical joint is a slider that allows rotation around the slider axis.
      RigidBody box6 = new RigidBody(new BoxShape(1, 1, 1))
      {
        Pose = new Pose(new Vector3F(1, 3, 0)),
      };
      Simulation.RigidBodies.Add(box6);
      RigidBody box7 = new RigidBody(new BoxShape(0.5f, 1.5f, 0.5f))
      {
        Pose = new Pose(new Vector3F(1, 3 - 0.5f, 0))
      };
      Simulation.RigidBodies.Add(box7);
      CylindricalJoint cylindricalJoint = new CylindricalJoint
      {
        BodyA = box6,
        AnchorPoseALocal = new Pose(new Vector3F(0, 0, 0), new Matrix33F(0, 1, 0,
                                                                        -1, 0, 0,
                                                                         0, 0, 1)),
        BodyB = box7,
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0.75f, 0), new Matrix33F(0, 1, 0,
                                                                            -1, 0, 0,
                                                                             0, 0, 1)),
        CollisionEnabled = false,
        // The linear movement limits on the slider axis.
        LinearMinimum = 0,
        LinearMaximum = 0.5f,
        // The rotation limits around the slider axis (in radians). Here, we allow free 
        // rotations.
        AngularMinimum = float.NegativeInfinity,
        AngularMaximum = float.PositiveInfinity,
      };
      Simulation.Constraints.Add(cylindricalJoint);

      // ----- HingeJoint
      // Hinge joints allow rotations around one axis. They can be used to model swinging doors
      // or rotating wheels.
      RigidBody cylinder0 = new RigidBody(new CylinderShape(0.1f, 2f))
      {
        Pose = new Pose(new Vector3F(3, 1, 0)),
        MotionType = MotionType.Static
      };
      Simulation.RigidBodies.Add(cylinder0);
      RigidBody box8 = new RigidBody(new BoxShape(1f, 1.8f, 0.1f))
      {
        Pose = new Pose(new Vector3F(3 + 0.5f, 1, 0))
      };
      Simulation.RigidBodies.Add(box8);
      HingeJoint hingeJoint = new HingeJoint
      {
        BodyA = cylinder0,
        AnchorPoseALocal = new Pose(new Vector3F(0, 0, 0), new Matrix33F(0, 1, 0,
                                                                        -1, 0, 0,
                                                                         0, 0, 1)),
        BodyB = box8,
        AnchorPoseBLocal = new Pose(new Vector3F(-0.5f, 0, 0), new Matrix33F(0, 1, 0,
                                                                            -1, 0, 0,
                                                                             0, 0, 1)),
        CollisionEnabled = false,
        // The rotation limits around the hinge axis (in radians).
        Minimum = -ConstantsF.PiOver2,
        Maximum = ConstantsF.PiOver2,
      };
      Simulation.Constraints.Add(hingeJoint);
    }
  }
}
