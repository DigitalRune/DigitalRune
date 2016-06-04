#if WP7
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample application simulates a ragdoll with breakable joints.",
    "The sample was created for the Windows Phone and uses the accelerometer.",
    101)]
  public class WpRagdollSample : BasicSample
  {
    // The gravity acceleration.
    private const float GravityAcceleration = 10;

    // The density used for all ragdoll bodies.
    private const float Density = 1000;

    // Joint parameters for ragdoll joints:
    private const float JointErrorReduction = 0.3f;
    private const float JointMaxForce = float.PositiveInfinity;
    // Using a softness value > 0 is important to give the ragdoll a more natural movement and to
    // avoid jittering.
    private const float JointSoftness = 0.0005f;

    // Adding damping reduces jittering of the ragdoll. 
    private const bool DampingEnabled = true;
    private const float DampingSoftness = 0.5f;
    private const float DampingMaxForce = float.PositiveInfinity;

    // We assume that joints break when the constraint force exceeds this limit.
    private const float BreakForceLimit = 15000;

    private Gravity _gravity;


    public WpRagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;

      // Set a fixed camera.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        MathHelper.ToRadians(30),
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        1f,
        100.0f);
      Vector3F cameraTarget = new Vector3F(0, 1, 0);
      Vector3F cameraPosition = new Vector3F(0, 12, 0);
      Vector3F cameraUpVector = new Vector3F(0, 0, -1);
      GraphicsScreen.CameraNode = new CameraNode(new Camera(projection))
      {
        View = Matrix44F.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector),
      };

      InitializePhysics();
    }


    /// <summary>
    /// Initializes the physics simulation.
    /// </summary>
    private void InitializePhysics()
    {
      // Add a gravity force.
      _gravity = new Gravity { Acceleration = new Vector3F(0, -GravityAcceleration, 0) };
      Simulation.ForceEffects.Add(_gravity);

      // Add a damping force.
      Simulation.ForceEffects.Add(new Damping());

      // Add a few spheres.
      Simulation.RigidBodies.Add(new RigidBody(new SphereShape(0.3f))
      {
        Pose = new Pose(new Vector3F(0, 1, 0)),
      });
      Simulation.RigidBodies.Add(new RigidBody(new SphereShape(0.2f))
      {
        Pose = new Pose(new Vector3F(1, 1, 0)),
      });
      Simulation.RigidBodies.Add(new RigidBody(new SphereShape(0.4f))
      {
        Pose = new Pose(new Vector3F(0, 1, 2)),
      });

      // Add ragdoll.
      AddRagdoll(1, new Vector3F(0, 2, 0));

      // The Simulation performs 2 sub-time-steps per frame because we have set
      // the FixedTimeStep of the simulation to 1/60 s and the TargetElapsedTime of the game
      // is 1/30 s (30 Hz). In the event SubTimeStepFinished, we call our method 
      // HandleBreakableJoints() to check the forces in the joints and disable constraints where
      // the force is too large. 
      // Instead, we could also call HandleBreakableJoints() in the Update() method of the game
      // but then it is only called with the 30 Hz of the game. It is more accurate to call the
      // method at the end of each simulation sub-time-step (60 Hz).
      Simulation.SubTimeStepFinished += (s, e) => HandleBreakableJoints();

      // Add 6 planes that keep the bodies inside the visible area. The exact positions and angles
      // have been determined by experimentation.
      var groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);
      var nearPlane = new RigidBody(new PlaneShape(-Vector3F.UnitY, -8))
      {
        Name = "NearPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(nearPlane);
      var leftPlane = new RigidBody(new PlaneShape(Matrix33F.CreateRotationZ(MathHelper.ToRadians(-22f)) * Vector3F.UnitX, -4.8f))
      {
        Name = "LeftPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(leftPlane);

      var rightPlane = new RigidBody(new PlaneShape(Matrix33F.CreateRotationZ(MathHelper.ToRadians(22f)) * -Vector3F.UnitX, -4.8f))
      {
        Name = "RightPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(rightPlane);

      var topPlane = new RigidBody(new PlaneShape(Matrix33F.CreateRotationX(MathHelper.ToRadians(14f)) * Vector3F.UnitZ, -3f))
      {
        Name = "TopPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(topPlane);

      var bottomPlane = new RigidBody(new PlaneShape(Matrix33F.CreateRotationX(MathHelper.ToRadians(-14f)) * -Vector3F.UnitZ, -3f))
      {
        Name = "BottomPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(bottomPlane);
    }



    public void AddRagdoll(float scale, Vector3F ragdollPosition)
    {
      // Ragdolls are usually used in games to create realistic death animations of 
      // characters. The character is usually rendered using a skinned triangle mesh.
      // But in the physics simulation the body parts of the character are represented
      // using simple shapes, such as spheres, capsules, boxes, or convex polyhedra, 
      // which are connected with joints.
      // The physics simulations computes how these parts collide and fall. The positions
      // and orientations are then read back each frame to update the animation of the
      // triangle mesh.

      // In this example the ragdoll is built from spheres, capsules and boxes. The
      // rigid bodies are created in code. In practice, ragdolls should be built using
      // external tools, such as a 3D modeller or a game editor.

      #region ----- Create rigid bodies for the most relevant body parts -----

      BoxShape pelvisShape = new BoxShape(0.3f * scale, 0.22f * scale, 0.20f * scale);
      MassFrame pelvisMass = MassFrame.FromShapeAndDensity(pelvisShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody pelvis = new RigidBody(pelvisShape, pelvisMass, null)
      {
        Pose = new Pose(new Vector3F(0f, 0.01f * scale, -0.03f * scale) + ragdollPosition),
      };
      Simulation.RigidBodies.Add(pelvis);

      BoxShape torsoShape = new BoxShape(0.35f * scale, 0.22f * scale, 0.44f * scale);
      MassFrame torsoMass = MassFrame.FromShapeAndDensity(torsoShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody torso = new RigidBody(torsoShape, torsoMass, null)
      {
        Pose = new Pose(new Vector3F(0f, 0.01f * scale, -0.4f * scale) + ragdollPosition),
      };
      Simulation.RigidBodies.Add(torso);

      SphereShape headShape = new SphereShape(0.13f * scale);
      MassFrame headMass = MassFrame.FromShapeAndDensity(headShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody head = new RigidBody(headShape, headMass, null)
      {
        Pose = new Pose(new Vector3F(0f * scale, 0f, -0.776f * scale) + ragdollPosition),
      };
      Simulation.RigidBodies.Add(head);

      CapsuleShape upperArmShape = new CapsuleShape(0.08f * scale, 0.3f * scale);
      MassFrame upperArmMass = MassFrame.FromShapeAndDensity(upperArmShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody leftUpperArm = new RigidBody(upperArmShape, upperArmMass, null)
      {
        Pose = new Pose(new Vector3F(-0.32f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(leftUpperArm);
      RigidBody rightUpperArm = new RigidBody(upperArmShape, upperArmMass, null)
      {
        Pose = new Pose(new Vector3F(0.32f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(rightUpperArm);

      CapsuleShape lowerArmShape = new CapsuleShape(0.08f * scale, 0.4f * scale);
      MassFrame lowerArmMass = MassFrame.FromShapeAndDensity(lowerArmShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody leftLowerArm = new RigidBody(lowerArmShape, lowerArmMass, null)
      {
        Pose = new Pose(new Vector3F(-0.62f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(leftLowerArm);
      RigidBody rightLowerArm = new RigidBody(lowerArmShape, lowerArmMass, null)
      {
        Pose = new Pose(new Vector3F(0.62f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(rightLowerArm);

      CapsuleShape upperLegShape = new CapsuleShape(0.09f * scale, 0.5f * scale);
      MassFrame upperLegMass = MassFrame.FromShapeAndDensity(upperLegShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody leftUpperLeg = new RigidBody(upperLegShape, upperLegMass, null)
      {
        Pose = new Pose(new Vector3F(-0.10f * scale, 0.01f * scale, 0.233f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(leftUpperLeg);

      RigidBody rightUpperLeg = new RigidBody(upperLegShape, upperLegMass, null)
      {
        Pose = new Pose(new Vector3F(0.10f * scale, 0.01f * scale, 0.233f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(rightUpperLeg);

      CapsuleShape lowerLegShape = new CapsuleShape(0.08f * scale, 0.4f * scale);
      MassFrame lowerLegMass = MassFrame.FromShapeAndDensity(pelvisShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody leftLowerLeg = new RigidBody(lowerLegShape, lowerLegMass, null)
      {
        Pose = new Pose(new Vector3F(-0.11f * scale, 0.01f * scale, 0.7f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(leftLowerLeg);
      RigidBody rightLowerLeg = new RigidBody(lowerLegShape, lowerLegMass, null)
      {
        Pose = new Pose(new Vector3F(0.11f * scale, 0.01f * scale, 0.7f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      Simulation.RigidBodies.Add(rightLowerLeg);

      BoxShape footShape = new BoxShape(0.12f * scale, 0.28f * scale, 0.07f * scale);
      MassFrame footMass = MassFrame.FromShapeAndDensity(footShape, Vector3F.One, Density, 0.01f, 3);
      RigidBody leftFoot = new RigidBody(footShape, footMass, null)
      {
        Pose = new Pose(new Vector3F(-0.11f * scale, -0.06f * scale, 0.94f * scale) + ragdollPosition),
      };
      Simulation.RigidBodies.Add(leftFoot);
      RigidBody rightFoot = new RigidBody(footShape, footMass, null)
      {
        Pose = new Pose(new Vector3F(0.11f * scale, -0.06f * scale, 0.94f * scale) + ragdollPosition),
      };
      Simulation.RigidBodies.Add(rightFoot);
      #endregion

      #region ----- Add joints between body parts -----

      Vector3F pelvisJointPosition = new Vector3F(0f, 0.026f * scale, -0.115f * scale) + ragdollPosition;
      HingeJoint pelvisJoint = new HingeJoint
      {
        BodyA = torso,
        BodyB = pelvis,
        AnchorPoseALocal = new Pose(torso.Pose.ToLocalPosition(pelvisJointPosition)),
        AnchorPoseBLocal = new Pose(pelvis.Pose.ToLocalPosition(pelvisJointPosition)),
        Minimum = -0.5f,
        Maximum = 1.1f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(pelvisJoint);

      Vector3F neckJointPosition = new Vector3F(0f, 0.026f * scale, -0.690f * scale) + ragdollPosition;
      HingeJoint neckJoint = new HingeJoint
      {
        BodyA = head,
        BodyB = torso,
        AnchorPoseALocal = new Pose(head.Pose.ToLocalPosition(neckJointPosition)),
        AnchorPoseBLocal = new Pose(torso.Pose.ToLocalPosition(neckJointPosition)),
        Minimum = -1f,
        Maximum = 1f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(neckJoint);

      Vector3F leftShoulderJointPosition = new Vector3F(-0.193f * scale, 0.056f * scale, -0.528f * scale) + ragdollPosition;
      Vector3F leftShoulderJointAxis = new Vector3F(0, -1, -1).Normalized;
      Matrix33F leftShoulderJointOrientation = new Matrix33F();
      leftShoulderJointOrientation.SetColumn(0, leftShoulderJointAxis);
      leftShoulderJointOrientation.SetColumn(1, leftShoulderJointAxis.Orthonormal1);
      leftShoulderJointOrientation.SetColumn(2, leftShoulderJointAxis.Orthonormal2);
      BallJoint leftShoulderJoint = new BallJoint
      {
        BodyA = leftUpperArm,
        BodyB = torso,
        AnchorPositionALocal = leftUpperArm.Pose.ToLocalPosition(leftShoulderJointPosition),
        AnchorPositionBLocal = torso.Pose.ToLocalPosition(leftShoulderJointPosition),
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(leftShoulderJoint);

      Vector3F rightShoulderJointPosition = new Vector3F(0.193f * scale, 0.056f * scale, -0.528f * scale) + ragdollPosition;
      Vector3F rightShoulderJointAxis = new Vector3F(0, 1, 1).Normalized;
      Matrix33F rightShoulderJointOrientation = new Matrix33F();
      rightShoulderJointOrientation.SetColumn(0, rightShoulderJointAxis);
      rightShoulderJointOrientation.SetColumn(1, rightShoulderJointAxis.Orthonormal1);
      rightShoulderJointOrientation.SetColumn(2, rightShoulderJointAxis.Orthonormal2);
      BallJoint rightShoulderJoint = new BallJoint
      {
        BodyA = rightUpperArm,
        BodyB = torso,
        AnchorPositionALocal = rightUpperArm.Pose.ToLocalPosition(rightShoulderJointPosition),
        AnchorPositionBLocal = torso.Pose.ToLocalPosition(rightShoulderJointPosition),
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(rightShoulderJoint);

      Vector3F leftElbowJointPosition = new Vector3F(-0.451f * scale, 0.071f * scale, -0.538f * scale) + ragdollPosition;
      Matrix33F elbowAxisOrientation = new Matrix33F(0, 0, -1,
        0, 1, 0,
        1, 0, 0);
      HingeJoint leftElbowJoint = new HingeJoint
      {
        BodyA = leftLowerArm,
        BodyB = leftUpperArm,
        AnchorPoseALocal = new Pose(leftLowerArm.Pose.ToLocalPosition(leftElbowJointPosition), leftLowerArm.Pose.Orientation.Inverse * elbowAxisOrientation),
        AnchorPoseBLocal = new Pose(leftUpperArm.Pose.ToLocalPosition(leftElbowJointPosition), leftUpperArm.Pose.Orientation.Inverse * elbowAxisOrientation),
        Minimum = -2,
        Maximum = 0,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(leftElbowJoint);

      Vector3F rightElbowJointPosition = new Vector3F(0.451f * scale, 0.071f * scale, -0.538f * scale) + ragdollPosition;
      HingeJoint rightElbowJoint = new HingeJoint
      {
        BodyA = rightLowerArm,
        BodyB = rightUpperArm,
        AnchorPoseALocal = new Pose(rightLowerArm.Pose.ToLocalPosition(rightElbowJointPosition), rightLowerArm.Pose.Orientation.Inverse * elbowAxisOrientation),
        AnchorPoseBLocal = new Pose(rightUpperArm.Pose.ToLocalPosition(rightElbowJointPosition), rightUpperArm.Pose.Orientation.Inverse * elbowAxisOrientation),
        Minimum = 0,
        Maximum = 2,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(rightElbowJoint);

      Vector3F leftHipJointPosition = new Vector3F(-0.107f * scale, 0.049f * scale, 0.026f * scale) + ragdollPosition;
      HingeJoint leftHipJoint = new HingeJoint
      {
        BodyA = pelvis,
        BodyB = leftUpperLeg,
        AnchorPoseALocal = new Pose(pelvis.Pose.ToLocalPosition(leftHipJointPosition)),
        AnchorPoseBLocal = new Pose(leftUpperLeg.Pose.ToLocalPosition(leftHipJointPosition), leftUpperLeg.Pose.Orientation.Inverse),
        Minimum = -0.1f,
        Maximum = 1.2f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(leftHipJoint);

      Vector3F rightHipJointPosition = new Vector3F(0.107f * scale, 0.049f * scale, 0.026f * scale) + ragdollPosition;
      HingeJoint rightHipJoint = new HingeJoint
      {
        BodyA = pelvis,
        BodyB = rightUpperLeg,
        AnchorPoseALocal = new Pose(pelvis.Pose.ToLocalPosition(rightHipJointPosition)),
        AnchorPoseBLocal = new Pose(rightUpperLeg.Pose.ToLocalPosition(rightHipJointPosition), rightUpperLeg.Pose.Orientation.Inverse),
        Minimum = -0.1f,
        Maximum = 1.2f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(rightHipJoint);

      Vector3F leftKneeJointPosition = new Vector3F(-0.118f * scale, -0.012f * scale, 0.439f * scale) + ragdollPosition;
      HingeJoint leftKneeJoint = new HingeJoint
      {
        BodyA = leftLowerLeg,
        BodyB = leftUpperLeg,
        AnchorPoseALocal = new Pose(leftLowerLeg.Pose.ToLocalPosition(leftKneeJointPosition)),
        AnchorPoseBLocal = new Pose(leftUpperLeg.Pose.ToLocalPosition(leftKneeJointPosition)),
        Minimum = 0,
        Maximum = 1.7f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(leftKneeJoint);

      Vector3F rightKneeJointPosition = new Vector3F(0.118f * scale, -0.012f * scale, 0.439f * scale) + ragdollPosition;
      HingeJoint rightKneeJoint = new HingeJoint
      {
        BodyA = rightLowerLeg,
        BodyB = rightUpperLeg,
        AnchorPoseALocal = new Pose(rightLowerLeg.Pose.ToLocalPosition(rightKneeJointPosition)),
        AnchorPoseBLocal = new Pose(rightUpperLeg.Pose.ToLocalPosition(rightKneeJointPosition)),
        Minimum = 0,
        Maximum = 1.7f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(rightKneeJoint);

      Vector3F leftAnkleJointPosition = new Vector3F(-0.118f * scale, -0.016f * scale, 0.861f * scale) + ragdollPosition;
      HingeJoint leftAnkleJoint = new HingeJoint
      {
        BodyA = leftFoot,
        BodyB = leftLowerLeg,
        AnchorPoseALocal = new Pose(leftFoot.Pose.ToLocalPosition(leftAnkleJointPosition)),
        AnchorPoseBLocal = new Pose(leftLowerLeg.Pose.ToLocalPosition(leftAnkleJointPosition), leftLowerLeg.Pose.Orientation.Inverse),
        Minimum = -0.4f,
        Maximum = 0.9f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(leftAnkleJoint);

      Vector3F rightAnkleJointPosition = new Vector3F(0.118f * scale, -0.016f * scale, 0.861f * scale) + ragdollPosition;
      HingeJoint rightAnkleJoint = new HingeJoint
      {
        BodyA = rightFoot,
        BodyB = rightLowerLeg,
        AnchorPoseALocal = new Pose(rightFoot.Pose.ToLocalPosition(rightAnkleJointPosition)),
        AnchorPoseBLocal = new Pose(rightLowerLeg.Pose.ToLocalPosition(rightAnkleJointPosition), rightLowerLeg.Pose.Orientation.Inverse),
        Minimum = -0.4f,
        Maximum = 0.9f,
        CollisionEnabled = false,
        ErrorReduction = JointErrorReduction,
        Softness = JointSoftness,
        MaxForce = JointMaxForce,
      };
      Simulation.Constraints.Add(rightAnkleJoint);
      #endregion

      #region ----- Add damping to improve stability -----

      if (DampingEnabled)
      {
        // Damping removes jiggling and improves stability.
        AddDamping(pelvis, torso);
        AddDamping(torso, head);
        AddDamping(torso, leftUpperArm);
        AddDamping(leftUpperArm, leftLowerArm);
        AddDamping(torso, rightUpperArm);
        AddDamping(rightUpperArm, rightLowerArm);
        AddDamping(pelvis, leftUpperLeg);
        AddDamping(pelvis, rightUpperLeg);
        AddDamping(leftUpperLeg, leftLowerLeg);
        AddDamping(rightUpperLeg, rightLowerLeg);
        AddDamping(leftLowerLeg, leftFoot);
        AddDamping(rightLowerLeg, rightFoot);
      }
      #endregion
    }


    /// <summary>
    /// Adds a rotational damping between the given bodies.
    /// </summary>
    /// <param name="bodyA">The first body.</param>
    /// <param name="bodyB">The second body.</param>
    private void AddDamping(RigidBody bodyA, RigidBody bodyB)
    {
      // Usually an AngularVelocityMotor rotates bodies, but in this case the 
      // TargetVelocity is set to 0 (default). The AngularVelocityMotor acts as
      // a damping.
      AngularVelocityMotor damping = new AngularVelocityMotor
      {
        BodyA = bodyA,
        BodyB = bodyB,
        // A softness of 0 would make the joint stiff. A softness value > 0 is important to 
        // mimic damping.
        Softness = DampingSoftness,
        MaxForce = DampingMaxForce,
      };
      Simulation.Constraints.Add(damping);
    }


    public override void Update(GameTime gameTime)
    {
      // ----- Tilt and shake effect:
      // The accelerometer determines the direction of gravity in our scene.
      // We also want to detect shakes: If the user shakes the camera left-and-right, the bodies
      // should fly around accordingly. This can be achieved by applying a non-linear transformation 
      // to the accelerometer values:
      //      y = (2 * x) ^ 3

      // Get accelerometer value transformed into world space.
      Vector3F accelerometerVector = new Vector3F(
        -InputService.AccelerometerValue.Y,
        InputService.AccelerometerValue.Z,
        -InputService.AccelerometerValue.X);

      // Apply non-linear transformation.
      var gravity = (2 * accelerometerVector) * (2 * accelerometerVector) * (2 * accelerometerVector);

      // Clamp negative y force - the "shake" should not increase the gravity in the "down" direction.
      gravity.Y = DigitalRune.Mathematics.MathHelper.Clamp(gravity.Y, -1, float.PositiveInfinity);
      _gravity.Acceleration = gravity * GravityAcceleration;

      // ----- Draw rigid bodies using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      foreach (var body in Simulation.RigidBodies)
      {
        var color = Color.Gray;
        // Draw static with different colors.
        if (body.MotionType == MotionType.Static)
          color = Color.LightGray;

        debugRenderer.DrawObject(body, color, false, false);
      }
    }


    /// <summary>
    /// Disables constraints where the constraint force is above a limit. 
    /// </summary>
    /// <remarks>
    /// This method is called whenever the simulation has finished a sub-time-step and has 
    /// calculated the new joint forces, body positions, etc.
    /// </remarks>
    private void HandleBreakableJoints()
    {
      // Check all joints. If a joint force was higher than a limit, the joint is disabled.
      // The simulation uses impulses. Therefore, we compute an equivalent impulse:
      // impulse = force * deltaTime
      float breakImpulseLimit = BreakForceLimit * Simulation.Settings.Timing.FixedTimeStep;

      // Loop over constraints.
      foreach (Constraint constraint in Simulation.Constraints)
      {
        // Disable constraint if the constraint impulse of the last time step was above limit.
        if (constraint.Enabled && constraint.LinearConstraintImpulse.Length > breakImpulseLimit)
        {
          var bodyA = constraint.BodyA;
          var bodyB = constraint.BodyB;

          // Disable all constraints between these two bodies. (If damping is enabled, we want
          // to disable the damping constraint too if the joint breaks.)
          foreach (Constraint otherConstraint in Simulation.Constraints)
          {
            if (otherConstraint.BodyA == bodyA && otherConstraint.BodyB == bodyB
                || otherConstraint.BodyA == bodyB && otherConstraint.BodyB == bodyA)
            {
              otherConstraint.Enabled = false;
            }
          }
        }
      }
    }
  }
}
#endif