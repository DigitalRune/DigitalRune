using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Materials;
using Microsoft.Xna.Framework.Input;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a simple car using constraints (joints and motors).",
    "",
    20)]
  [Controls(@"Sample
  Use arrow keys to control car.")]
  public class ConstraintCarSample : PhysicsSample
  {
    // The joints for the front wheels. 
    private Hinge2Joint _frontRightHinge;
    private Hinge2Joint _frontLeftHinge;

    // The motors of the front wheels. (The back wheels are not motorized.)
    private AngularVelocityMotor _frontLeftMotor;
    private AngularVelocityMotor _frontRightMotor;

    // The current steering angle of the front wheel.
    private float _steeringAngle;


    public ConstraintCarSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Create a material with high friction - this will be used for the ground and the wheels
      // to give the wheels some traction.
      UniformMaterial roughMaterial = new UniformMaterial
      {
        DynamicFriction = 1,
        StaticFriction = 1,
      };

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0), null, roughMaterial)
      {
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Now, we build a car out of one box for the chassis and 4 cylindric wheels. 
      // Front wheels are fixed with Hinge2Joints and motorized with AngularVelocityMotors.
      // Back wheels are fixed with HingeJoints and not motorized. - This creates a sloppy
      // car configuration. Please note that cars for racing games are not normally built with 
      // simple constraints - nevertheless, it is funny to do and play with it.

      // Check out the "Vehicle Sample" (not included in this project)! The "Vehicle Sample" 
      // provides a robust ray-car implementation.)

      // ----- Chassis
      BoxShape chassisShape = new BoxShape(1.7f, 1, 4f);
      MassFrame chassisMass = MassFrame.FromShapeAndDensity(chassisShape, Vector3F.One, 200, 0.01f, 3);
      // Here is a trick: The car topples over very easily. By lowering the center of mass we 
      // make it more stable.
      chassisMass.Pose = new Pose(new Vector3F(0, -1, 0));
      RigidBody chassis = new RigidBody(chassisShape, chassisMass, null)
      {
        Pose = new Pose(new Vector3F(0, 1, 0)),
      };
      Simulation.RigidBodies.Add(chassis);

      // ------ Wheels
      CylinderShape cylinderShape = new CylinderShape(0.4f, 0.3f);
      MassFrame wheelMass = MassFrame.FromShapeAndDensity(cylinderShape, Vector3F.One, 500, 0.01f, 3);
      RigidBody wheelFrontLeft = new RigidBody(cylinderShape, wheelMass, roughMaterial)
      {
        Pose = new Pose(new Vector3F(0, 1, 0), Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(wheelFrontLeft);
      RigidBody wheelFrontRight = new RigidBody(cylinderShape, wheelMass, roughMaterial)
      {
        Pose = new Pose(new Vector3F(0, 1, 0), Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(wheelFrontRight);
      RigidBody wheelBackLeft = new RigidBody(cylinderShape, wheelMass, roughMaterial)
      {
        Pose = new Pose(new Vector3F(0, 1, 0), Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(wheelBackLeft);
      RigidBody wheelBackRight = new RigidBody(cylinderShape, wheelMass, roughMaterial)
      {
        Pose = new Pose(new Vector3F(0, 1, 0), Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(wheelBackRight);

      // ----- Hinge2Joints for the front wheels.
      // A Hinge2Joint allows a limited rotation on the first constraint axis - the steering
      // axis. The second constraint axis is locked and the third constraint axis is the 
      // rotation axis of the wheels.
      _frontLeftHinge = new Hinge2Joint
      {
        BodyA = chassis,
        // --> To define the constraint anchor orientation for the chassis:
        // The columns are the axes. We set the local y axis in the first column. This is
        // steering axis. In the last column we set the -x axis. This is the wheel rotation axis.
        // In the middle column is a vector that is normal to the first and last axis.
        // (All three columns are orthonormal and form a valid rotation matrix.)
        AnchorPoseALocal = new Pose(new Vector3F(-0.9f, -0.4f, -1.4f),
                                    new Matrix33F(0, 0, -1,
                                                  1, 0, 0,
                                                  0, -1, 0)),
        BodyB = wheelFrontLeft,
        // --> To define the constraint anchor orientation for the chassis:
        // The columns are the axes. We set the local x axis in the first column. This is
        // steering axis. In the last column we set the y axis. This is the wheel rotation axis.
        // (In local space of a cylinder the cylinder axis is the +y axis.)
        // In the middle column is a vector that is normal to the first and last axis.
        // (All three columns are orthonormal and form a valid rotation matrix.)
        AnchorPoseBLocal = new Pose(new Matrix33F(1, 0, 0,
                                                  0, 0, 1,
                                                  0, -1, 0)),
        CollisionEnabled = false,
        Minimum = new Vector2F(-0.7f, float.NegativeInfinity),
        Maximum = new Vector2F(0.7f, float.PositiveInfinity),
      };
      Simulation.Constraints.Add(_frontLeftHinge);

      _frontRightHinge = new Hinge2Joint
      {
        BodyA = chassis,
        BodyB = wheelFrontRight,
        AnchorPoseALocal = new Pose(new Vector3F(0.9f, -0.4f, -1.4f),
                                    new Matrix33F(0, 0, -1,
                                                  1, 0, 0,
                                                  0, -1, 0)),
        AnchorPoseBLocal = new Pose(new Matrix33F(1, 0, 0,
                                                  0, 0, 1,
                                                  0, -1, 0)),
        CollisionEnabled = false,
        Minimum = new Vector2F(-0.7f, float.NegativeInfinity),
        Maximum = new Vector2F(0.7f, float.PositiveInfinity),
      };
      Simulation.Constraints.Add(_frontRightHinge);

      // ----- HingeJoints for the back wheels.
      // Hinges allow free rotation on the first constraint axis.
      HingeJoint backLeftHinge = new HingeJoint
      {
        BodyA = chassis,
        AnchorPoseALocal = new Pose(new Vector3F(-0.9f, -0.4f, 1.4f)),
        BodyB = wheelBackLeft,
        // --> To define the constraint anchor orientation:
        // The columns are the axes. We set the local y axis in the first column. This is
        // cylinder axis and should be the hinge axis. In the other two columns we set two 
        // orthonormal vectors.
        // (All three columns are orthonormal and form a valid rotation matrix.)
        AnchorPoseBLocal = new Pose(new Matrix33F(0, 0, 1,
                                                  1, 0, 0,
                                                  0, 1, 0)),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(backLeftHinge);
      HingeJoint backRightHinge = new HingeJoint
      {
        BodyA = chassis,
        AnchorPoseALocal = new Pose(new Vector3F(0.9f, -0.4f, 1.4f)),
        BodyB = wheelBackRight,
        AnchorPoseBLocal = new Pose(new Matrix33F(0, 0, 1,
                                                  1, 0, 0,
                                                  0, 1, 0)),
        CollisionEnabled = false,
      };
      Simulation.Constraints.Add(backRightHinge);

      // ----- Motors for the front wheels.
      // (Motor axes and target velocities are set in Update() below.)
      _frontLeftMotor = new AngularVelocityMotor
      {
        BodyA = chassis,
        BodyB = wheelFrontLeft,
        CollisionEnabled = false,

        // We use "single axis mode", which means the motor drives only on axis and does not
        // block motion orthogonal to this axis. - Rotation about the orthogonal axes is already
        // controlled by the Hinge2Joint.
        UseSingleAxisMode = true,

        // The motor has only limited power:
        MaxForce = 50000,
      };
      Simulation.Constraints.Add(_frontLeftMotor);

      _frontRightMotor = new AngularVelocityMotor
      {
        BodyA = chassis,
        BodyB = wheelFrontRight,
        CollisionEnabled = false,
        UseSingleAxisMode = true,
        MaxForce = 50000,
      };
      Simulation.Constraints.Add(_frontRightMotor);

      // ----- Drop a few boxes to create obstacles.
      BoxShape boxShape = new BoxShape(1, 1, 1);
      MassFrame boxMass = MassFrame.FromShapeAndDensity(boxShape, Vector3F.One, 100, 0.01f, 3);
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-20, 20);
        position.Y = 5;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(boxShape, boxMass, null)
        {
          Pose = new Pose(position, orientation),
        };
        Simulation.RigidBodies.Add(body);
      }
    }


    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
      // Get steering angle from arrow keys.
      if (InputService.IsDown(Keys.Left))
        _steeringAngle = 0.7f;
      else if (InputService.IsDown(Keys.Right))
        _steeringAngle = -0.7f;
      else
        _steeringAngle = 0;

      // Lock the steering angles of the front wheels.
      _frontLeftHinge.Minimum = new Vector2F(_steeringAngle, float.NegativeInfinity);
      _frontLeftHinge.Maximum = new Vector2F(_steeringAngle, float.PositiveInfinity);
      _frontRightHinge.Minimum = new Vector2F(_steeringAngle, float.NegativeInfinity);
      _frontRightHinge.Maximum = new Vector2F(_steeringAngle, float.PositiveInfinity);

      // Get velocity from arrow keys.
      float wheelVelocity = 0;
      if (InputService.IsDown(Keys.Up))
        wheelVelocity += 60;
      if (InputService.IsDown(Keys.Down))
        wheelVelocity -= 60;

      // The normal rotation axis is the -x axis.
      Vector3F axis = -Vector3F.UnitX;
      // Rotate the axis by the steering angle.
      axis = QuaternionF.CreateRotationY(_steeringAngle).Rotate(axis);

      // Set the axes and the velocities of the motors.
      _frontLeftMotor.AxisALocal = axis;
      _frontLeftMotor.TargetVelocity = wheelVelocity;
      _frontRightMotor.AxisALocal = axis;
      _frontRightMotor.TargetVelocity = wheelVelocity;

      base.Update(gameTime);
    }
  }
}
