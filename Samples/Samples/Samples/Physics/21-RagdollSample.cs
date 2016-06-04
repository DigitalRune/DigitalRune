using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create several ragdolls.",
    "", 
    21)]
  public class RagdollSample : PhysicsSample
  {
    public RagdollSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane", // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Add a number of ragdolls.
      for (int i = 0; i < 5; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-3, 3);
        position.Y = 1 + i;
        AddRagdoll(Simulation, 1, position, 0.0005f, true);
      }

      // Add some random, static boxes.
      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-5, 5);
        position.Y = 0.5f;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(position, orientation),
          MotionType = MotionType.Static
        };
        Simulation.RigidBodies.Add(body);
      }
    }


    // Using a softness value > 0 is important to give the ragdoll a more natural movement and to
    // avoid jittering.
    public static void AddRagdoll(Simulation simulation, float scale, Vector3F ragdollPosition, float softness, bool addDamping)
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
      // external tools, such as a 3D modeler or a game editor.

      #region ----- Create rigid bodies for the most relevant body parts -----

      // The density used for all bodies.
      const float density = 1000;

      BoxShape pelvisShape = new BoxShape(0.3f * scale, 0.22f * scale, 0.20f * scale);
      MassFrame pelvisMass = MassFrame.FromShapeAndDensity(pelvisShape, Vector3F.One, density, 0.01f, 3);
      RigidBody pelvis = new RigidBody(pelvisShape, pelvisMass, null)
      {
        Pose = new Pose(new Vector3F(0f, 0.01f * scale, -0.03f * scale) + ragdollPosition),
      };
      simulation.RigidBodies.Add(pelvis);

      BoxShape torsoShape = new BoxShape(0.35f * scale, 0.22f * scale, 0.44f * scale);
      MassFrame torsoMass = MassFrame.FromShapeAndDensity(torsoShape, Vector3F.One, density, 0.01f, 3);
      RigidBody torso = new RigidBody(torsoShape, torsoMass, null)
      {
        Pose = new Pose(new Vector3F(0f, 0.01f * scale, -0.4f * scale) + ragdollPosition),
      };
      simulation.RigidBodies.Add(torso);

      SphereShape headShape = new SphereShape(0.13f * scale);
      MassFrame headMass = MassFrame.FromShapeAndDensity(headShape, Vector3F.One, density, 0.01f, 3);
      RigidBody head = new RigidBody(headShape, headMass, null)
      {
        Pose = new Pose(new Vector3F(0f * scale, 0f, -0.776f * scale) + ragdollPosition),
      };
      simulation.RigidBodies.Add(head);

      CapsuleShape upperArmShape = new CapsuleShape(0.08f * scale, 0.3f * scale);
      MassFrame upperArmMass = MassFrame.FromShapeAndDensity(upperArmShape, Vector3F.One, density, 0.01f, 3);
      RigidBody leftUpperArm = new RigidBody(upperArmShape, upperArmMass, null)
      {
        Pose = new Pose(new Vector3F(-0.32f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(leftUpperArm);
      RigidBody rightUpperArm = new RigidBody(upperArmShape, upperArmMass, null)
      {
        Pose = new Pose(new Vector3F(0.32f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(rightUpperArm);

      CapsuleShape lowerArmShape = new CapsuleShape(0.08f * scale, 0.4f * scale);
      MassFrame lowerArmMass = MassFrame.FromShapeAndDensity(lowerArmShape, Vector3F.One, density, 0.01f, 3);
      RigidBody leftLowerArm = new RigidBody(lowerArmShape, lowerArmMass, null)
      {
        Pose = new Pose(new Vector3F(-0.62f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(leftLowerArm);
      RigidBody rightLowerArm = new RigidBody(lowerArmShape, lowerArmMass, null)
      {
        Pose = new Pose(new Vector3F(0.62f * scale, 0.06f * scale, -0.53f * scale) + ragdollPosition, Matrix33F.CreateRotationZ(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(rightLowerArm);

      CapsuleShape upperLegShape = new CapsuleShape(0.09f * scale, 0.5f * scale);
      MassFrame upperLegMass = MassFrame.FromShapeAndDensity(upperLegShape, Vector3F.One, density, 0.01f, 3);
      RigidBody leftUpperLeg = new RigidBody(upperLegShape, upperLegMass, null)
      {
        Pose = new Pose(new Vector3F(-0.10f * scale, 0.01f * scale, 0.233f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(leftUpperLeg);

      RigidBody rightUpperLeg = new RigidBody(upperLegShape, upperLegMass, null)
      {
        Pose = new Pose(new Vector3F(0.10f * scale, 0.01f * scale, 0.233f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(rightUpperLeg);

      CapsuleShape lowerLegShape = new CapsuleShape(0.08f * scale, 0.4f * scale);
      MassFrame lowerLegMass = MassFrame.FromShapeAndDensity(pelvisShape, Vector3F.One, density, 0.01f, 3);
      RigidBody leftLowerLeg = new RigidBody(lowerLegShape, lowerLegMass, null)
      {
        Pose = new Pose(new Vector3F(-0.11f * scale, 0.01f * scale, 0.7f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(leftLowerLeg);
      RigidBody rightLowerLeg = new RigidBody(lowerLegShape, lowerLegMass, null)
      {
        Pose = new Pose(new Vector3F(0.11f * scale, 0.01f * scale, 0.7f * scale) + ragdollPosition, Matrix33F.CreateRotationX(ConstantsF.PiOver2)),
      };
      simulation.RigidBodies.Add(rightLowerLeg);

      BoxShape footShape = new BoxShape(0.12f * scale, 0.28f * scale, 0.07f * scale);
      MassFrame footMass = MassFrame.FromShapeAndDensity(footShape, Vector3F.One, density, 0.01f, 3);
      RigidBody leftFoot = new RigidBody(footShape, footMass, null)
      {
        Pose = new Pose(new Vector3F(-0.11f * scale, -0.06f * scale, 0.94f * scale) + ragdollPosition),
      };
      simulation.RigidBodies.Add(leftFoot);
      RigidBody rightFoot = new RigidBody(footShape, footMass, null)
      {
        Pose = new Pose(new Vector3F(0.11f * scale, -0.06f * scale, 0.94f * scale) + ragdollPosition),
      };
      simulation.RigidBodies.Add(rightFoot);
      #endregion

      #region ----- Add joints between body parts -----

      float errorReduction = 0.3f;
      float maxForce = float.PositiveInfinity;

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(pelvisJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(neckJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(leftShoulderJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(rightShoulderJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(leftElbowJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(rightElbowJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(leftHipJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(rightHipJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(leftKneeJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(rightKneeJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(leftAnkleJoint);

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
        ErrorReduction = errorReduction,
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(rightAnkleJoint);
      #endregion

      #region ----- Add damping to improve stability -----

      if (addDamping)
      {
        // Damping removes jiggling and improves stability.
        softness = 0.05f;
        maxForce = float.PositiveInfinity;

        AddDamping(simulation, pelvis, torso, softness, maxForce);
        AddDamping(simulation, torso, head, softness, maxForce);
        AddDamping(simulation, torso, leftUpperArm, softness, maxForce);
        AddDamping(simulation, leftUpperArm, leftLowerArm, softness, maxForce);
        AddDamping(simulation, torso, rightUpperArm, softness, maxForce);
        AddDamping(simulation, rightUpperArm, rightLowerArm, softness, maxForce);
        AddDamping(simulation, pelvis, leftUpperLeg, softness, maxForce);
        AddDamping(simulation, pelvis, rightUpperLeg, softness, maxForce);
        AddDamping(simulation, leftUpperLeg, leftLowerLeg, softness, maxForce);
        AddDamping(simulation, rightUpperLeg, rightLowerLeg, softness, maxForce);
        AddDamping(simulation, leftLowerLeg, leftFoot, softness, maxForce);
        AddDamping(simulation, rightLowerLeg, rightFoot, softness, maxForce);
      }
      #endregion
    }


    private static void AddDamping(Simulation simulation, RigidBody bodyA, RigidBody bodyB, float softness, float maxForce)
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
        Softness = softness,
        MaxForce = maxForce,
      };
      simulation.Constraints.Add(damping);
    }
  }
}
