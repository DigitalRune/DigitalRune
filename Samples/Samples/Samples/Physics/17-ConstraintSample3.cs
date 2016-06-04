using System;
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
    @"This sample demonstrates various constraints: HingeJoint with AngularVelocityMotor, 
PrismaticJoint with LinearVelocityMotor, BallJoint with TwistSwingLimit and QuaternionMotor",
    @"",
    17)]
  public class ConstraintSample3 : PhysicsSample
  {
    private PrismaticJoint _prismaticJoint;
    private LinearVelocityMotor _linearVelocityMotor;
    private BallJoint _ballJoint;
    private TwistSwingLimit _twistSwingLimit;


    public ConstraintSample3(Microsoft.Xna.Framework.Game game)
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

      // ----- HingeJoint with AngularVelocityMotor
      // A board is fixed on a pole like a wind wheel.
      // An AngularVelocityMotor creates a rotation with constant angular velocity around the
      // hinge axis.
      RigidBody box0 = new RigidBody(new BoxShape(0.1f, 2f, 0.1f))
      {
        Pose = new Pose(new Vector3F(-2, 1, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box0);
      RigidBody box1 = new RigidBody(new BoxShape(0.1f, 0.4f, 1f))
      {
        Pose = new Pose(new Vector3F(-2 + 0.05f, 1.8f, 0))
      };
      Simulation.RigidBodies.Add(box1);
      HingeJoint hingeJoint = new HingeJoint
      {
        BodyA = box0,
        BodyB = box1,
        AnchorPoseALocal = new Pose(new Vector3F(0.05f, 0.8f, 0)),
        AnchorPoseBLocal = new Pose(new Vector3F(-0.05f, 0, 0)),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(hingeJoint);
      AngularVelocityMotor angularVelocityMotor = new AngularVelocityMotor
      {
        BodyA = box0,
        // The rotation axis is the local x axis of BodyA.
        AxisALocal = Vector3F.UnitX,
        BodyB = box1,
        TargetVelocity = 10,
        // The motor power is limit, so that the rotation can be stopped by other objects blocking
        // the movement.
        MaxForce = 10000,
        // The HingeJoint controls all other axes. So this motor must only act on the hinge
        // axis.
        UseSingleAxisMode = true,
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(angularVelocityMotor);

      // ----- A PrismaticJoint with a LinearVelocityMotor.
      RigidBody box2 = new RigidBody(new BoxShape(0.7f, 0.7f, 0.7f))
      {
        Pose = new Pose(new Vector3F(0, 2, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box2);
      RigidBody box3 = new RigidBody(new BoxShape(0.5f, 1.5f, 0.5f))
      {
        Pose = new Pose(new Vector3F(0, 1, 0))
      };
      Simulation.RigidBodies.Add(box3);
      _prismaticJoint = new PrismaticJoint
      {
        BodyA = box2,
        BodyB = box3,
        AnchorPoseALocal = new Pose(new Vector3F(0, 0, 0), new Matrix33F(0, 1, 0,
                                                                        -1, 0, 0,
                                                                         0, 0, 1)),
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0, 0), new Matrix33F(0, 1, 0,
                                                                        -1, 0, 0,
                                                                         0, 0, 1)),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(_prismaticJoint);
      _linearVelocityMotor = new LinearVelocityMotor
      {
        BodyA = box2,
        AxisALocal = -Vector3F.UnitY,
        BodyB = box3,
        TargetVelocity = 1,
        CollisionEnabled = false,
        MaxForce = 10000,
        UseSingleAxisMode = true,
      };
      Simulation.Constraints.Add(_linearVelocityMotor);

      // ----- A BallJoint with a TwistSwingLimit and a QuaternionMotor
      // The ball joint connects a cylinder to a static box.
      // The twist-swing limits rotational movement.
      // The quaternion motor acts like a spring that controls the angle of joint.
      RigidBody box4 = new RigidBody(new BoxShape(0.5f, 0.5f, 0.5f))
      {
        Pose = new Pose(new Vector3F(2, 2, 0)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(box4);
      RigidBody cylinder0 = new RigidBody(new CylinderShape(0.1f, 0.75f))
      {
        Pose = new Pose(new Vector3F(2, 2 - 0.75f / 2 - 0.25f, 0))
      };
      Simulation.RigidBodies.Add(cylinder0);
      _ballJoint = new BallJoint
      {
        BodyA = box4,
        BodyB = cylinder0,
        AnchorPositionALocal = new Vector3F(0, -0.25f, 0),
        AnchorPositionBLocal = new Vector3F(0, 0.75f / 2, 0),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(_ballJoint);
      _twistSwingLimit = new TwistSwingLimit
      {
        BodyA = box4,
        // The first column is the twist axis (-y). The other two columns are the swing axes.
        AnchorOrientationALocal = new Matrix33F(0, 1, 0,
                                               -1, 0, 0,
                                                0, 0, 1),
        BodyB = cylinder0,
        AnchorOrientationBLocal = new Matrix33F(0, 1, 0,
                                               -1, 0, 0,
                                                0, 0, 1),
        CollisionEnabled = false,
        // The twist is limited to +/- 10°. The swing limits are +/- 40° and +/- 60°. This creates
        // a deformed cone that limits the swing movements (see visualization).
        Minimum = new Vector3F(-MathHelper.ToRadians(10), -MathHelper.ToRadians(40), -MathHelper.ToRadians(60)),
        Maximum = new Vector3F(MathHelper.ToRadians(10), MathHelper.ToRadians(40), MathHelper.ToRadians(60)),
      };
      Simulation.Constraints.Add(_twistSwingLimit);
      QuaternionMotor quaternionMotor = new QuaternionMotor
      {
        BodyA = box4,
        AnchorOrientationALocal = Matrix33F.Identity,
        BodyB = cylinder0,
        AnchorOrientationBLocal = Matrix33F.Identity,
        CollisionEnabled = false,
        // The QuaternionMotor controls the orientation of the second body relative to the first
        // body. Here, we define that the cylinder should swing 30° away from the default 
        // orientation.
        TargetOrientation = QuaternionF.CreateRotationZ(MathHelper.ToRadians(30)),
        // Position and orientation motors are similar to damped-springs. We can control
        // the stiffness and damping of the spring. (It is also possible to set the SpringConstant
        // to 0 if the QuaternionMotor should only act as a rotational damping.)
        SpringConstant = 100,
        DampingConstant = 20,
      };
      Simulation.Constraints.Add(quaternionMotor);
    }


    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
      // ----- Control the slider velocity.
      // Invert the velocity when the jointed bodies reach certain limits.
      if (_prismaticJoint.RelativePosition < 0)
        _linearVelocityMotor.TargetVelocity = Math.Abs(_linearVelocityMotor.TargetVelocity);
      else if (_prismaticJoint.RelativePosition > 1.25f)
        _linearVelocityMotor.TargetVelocity = -Math.Abs(_linearVelocityMotor.TargetVelocity);

      base.Update(gameTime);

      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.DrawTwistSwingLimit(_ballJoint, _twistSwingLimit, 1, false);
    }
  }
}
