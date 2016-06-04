using System.Linq;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Materials;
using DigitalRune.Physics.Specialized;


namespace Samples.Animation
{
  // This class creates a rigid bodies, joints, limits and motors for a ragdoll for the Dude model.
  public static class DudeRagdollCreator
  {
    /// <summary>
    /// Initializes the ragdoll for the given skeleton pose.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="ragdoll">The ragdoll.</param>
    /// <param name="simulation">The simulation in which the ragdoll will be used.</param>
    /// <param name="scale">A scaling factor to scale the size of the ragdoll.</param>
    public static void Create(SkeletonPose skeletonPose, Ragdoll ragdoll, Simulation simulation, float scale)
    {
      var skeleton = skeletonPose.Skeleton;

      const float totalMass = 80;     // The total mass of the ragdoll.
      const int numberOfBodies = 17;

      // Get distance from foot to head as a measure for the size of the ragdoll.
      int head = skeleton.GetIndex("Head");
      int footLeft = skeleton.GetIndex("L_Ankle1");
      var headPosition = skeletonPose.GetBonePoseAbsolute(head).Translation;
      var footPosition = skeletonPose.GetBonePoseAbsolute(footLeft).Translation;
      var headToFootDistance = (headPosition - footPosition).Length;

      // We use the same mass properties for all bodies. This is not realistic but more stable 
      // because large mass differences or thin bodies (arms!) are less stable.
      // We use the mass properties of sphere proportional to the size of the model.
      var massFrame = MassFrame.FromShapeAndMass(new SphereShape(headToFootDistance / 8), Vector3F.One, totalMass / numberOfBodies, 0.1f, 1);

      var material = new UniformMaterial();

      #region ----- Add Bodies and Body Offsets -----

      var numberOfBones = skeleton.NumberOfBones;
      ragdoll.Bodies.AddRange(Enumerable.Repeat<RigidBody>(null, numberOfBones));
      ragdoll.BodyOffsets.AddRange(Enumerable.Repeat(Pose.Identity, numberOfBones));

      var pelvis = skeleton.GetIndex("Pelvis");
      ragdoll.Bodies[pelvis] = new RigidBody(new BoxShape(0.3f * scale, 0.4f * scale, 0.55f * scale), massFrame, material);
      ragdoll.BodyOffsets[pelvis] = new Pose(new Vector3F(0, 0, 0));

      var backLower = skeleton.GetIndex("Spine");
      ragdoll.Bodies[backLower] = new RigidBody(new BoxShape(0.36f * scale, 0.4f * scale, 0.55f * scale), massFrame, material);
      ragdoll.BodyOffsets[backLower] = new Pose(new Vector3F(0.18f * scale, 0, 0));

      var backUpper = skeleton.GetIndex("Spine2");
      ragdoll.Bodies[backUpper] = new RigidBody(new BoxShape(0.5f * scale, 0.4f * scale, 0.65f * scale), massFrame, material);
      ragdoll.BodyOffsets[backUpper] = new Pose(new Vector3F(0.25f * scale, 0, 0));

      var neck = skeleton.GetIndex("Neck");
      ragdoll.Bodies[neck] = new RigidBody(new CapsuleShape(0.12f * scale, 0.3f * scale), massFrame, material);
      ragdoll.BodyOffsets[neck] = new Pose(new Vector3F(0.15f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      ragdoll.Bodies[neck].CollisionObject.Enabled = false;

      ragdoll.Bodies[head] = new RigidBody(new SphereShape(0.2f * scale), massFrame, material);
      ragdoll.BodyOffsets[head] = new Pose(new Vector3F(0.15f * scale, 0.02f * scale, 0));

      var armUpperLeft = skeleton.GetIndex("L_UpperArm");
      ragdoll.Bodies[armUpperLeft] = new RigidBody(new CapsuleShape(0.12f * scale, 0.6f * scale), massFrame, material);
      ragdoll.BodyOffsets[armUpperLeft] = new Pose(new Vector3F(0.2f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var armLowerLeft = skeleton.GetIndex("L_Forearm");
      ragdoll.Bodies[armLowerLeft] = new RigidBody(new CapsuleShape(0.08f * scale, 0.5f * scale), massFrame, material);
      ragdoll.BodyOffsets[armLowerLeft] = new Pose(new Vector3F(0.2f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var handLeft = skeleton.GetIndex("L_Hand");
      ragdoll.Bodies[handLeft] = new RigidBody(new BoxShape(0.2f * scale, 0.06f * scale, 0.15f * scale), massFrame, material);
      ragdoll.BodyOffsets[handLeft] = new Pose(new Vector3F(0.1f * scale, 0, 0));

      var armUpperRight = skeleton.GetIndex("R_UpperArm");
      ragdoll.Bodies[armUpperRight] = new RigidBody(new CapsuleShape(0.12f * scale, 0.6f * scale), massFrame, material);
      ragdoll.BodyOffsets[armUpperRight] = new Pose(new Vector3F(0.2f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var armLowerRight = skeleton.GetIndex("R_Forearm");
      ragdoll.Bodies[armLowerRight] = new RigidBody(new CapsuleShape(0.08f * scale, 0.5f * scale), massFrame, material);
      ragdoll.BodyOffsets[armLowerRight] = new Pose(new Vector3F(0.2f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var handRight = skeleton.GetIndex("R_Hand");
      ragdoll.Bodies[handRight] = new RigidBody(new BoxShape(0.2f * scale, 0.06f * scale, 0.15f * scale), massFrame, material);
      ragdoll.BodyOffsets[handRight] = new Pose(new Vector3F(0.1f * scale, 0, 0));

      var legUpperLeft = skeleton.GetIndex("L_Thigh1");
      ragdoll.Bodies[legUpperLeft] = new RigidBody(new CapsuleShape(0.16f * scale, 0.8f * scale), massFrame, material);
      ragdoll.BodyOffsets[legUpperLeft] = new Pose(new Vector3F(0.4f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var legLowerLeft = skeleton.GetIndex("L_Knee2");
      ragdoll.Bodies[legLowerLeft] = new RigidBody(new CapsuleShape(0.12f * scale, 0.65f * scale), massFrame, material);
      ragdoll.BodyOffsets[legLowerLeft] = new Pose(new Vector3F(0.32f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      //var footLeft = skeleton.GetIndex("L_Ankle1");
      ragdoll.Bodies[footLeft] = new RigidBody(new BoxShape(0.20f * scale, 0.5f * scale, 0.3f * scale), massFrame, material);
      ragdoll.BodyOffsets[footLeft] = new Pose(new Vector3F(0.16f * scale, 0.15f * scale, 0));

      var legUpperRight = skeleton.GetIndex("R_Thigh");
      ragdoll.Bodies[legUpperRight] = new RigidBody(new CapsuleShape(0.16f * scale, 0.8f * scale), massFrame, material);
      ragdoll.BodyOffsets[legUpperRight] = new Pose(new Vector3F(0.4f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var legLowerRight = skeleton.GetIndex("R_Knee");
      ragdoll.Bodies[legLowerRight] = new RigidBody(new CapsuleShape(0.12f * scale, 0.65f * scale), massFrame, material);
      ragdoll.BodyOffsets[legLowerRight] = new Pose(new Vector3F(0.32f * scale, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));

      var footRight = skeleton.GetIndex("R_Ankle");
      ragdoll.Bodies[footRight] = new RigidBody(new BoxShape(0.20f * scale, 0.5f * scale, 0.3f * scale), massFrame, material);
      ragdoll.BodyOffsets[footRight] = new Pose(new Vector3F(0.16f * scale, 0.15f * scale, 0));
      #endregion

      #region ----- Set Collision Filters -----

      // Collisions between connected bodies will be disabled in AddJoint(). (A BallJoint 
      // has a property CollisionEnabled which decides whether connected bodies can 
      // collide.)
      // But we need to disable some more collision between bodies that are not directly
      // connected but still too close to each other.
      var filter = (ICollisionFilter)simulation.CollisionDomain.CollisionDetection.CollisionFilter;
      filter.Set(ragdoll.Bodies[backUpper].CollisionObject, ragdoll.Bodies[head].CollisionObject, false);
      filter.Set(ragdoll.Bodies[armUpperRight].CollisionObject, ragdoll.Bodies[backLower].CollisionObject, false);
      filter.Set(ragdoll.Bodies[armUpperLeft].CollisionObject, ragdoll.Bodies[backLower].CollisionObject, false);
      filter.Set(ragdoll.Bodies[legUpperLeft].CollisionObject, ragdoll.Bodies[legUpperRight].CollisionObject, false);
      #endregion

      #region ----- Add Joints -----

      AddJoint(skeletonPose, ragdoll, pelvis, backLower);
      AddJoint(skeletonPose, ragdoll, backLower, backUpper);
      AddJoint(skeletonPose, ragdoll, backUpper, neck);
      AddJoint(skeletonPose, ragdoll, neck, head);
      AddJoint(skeletonPose, ragdoll, backUpper, armUpperLeft);
      AddJoint(skeletonPose, ragdoll, armUpperLeft, armLowerLeft);
      AddJoint(skeletonPose, ragdoll, armLowerLeft, handLeft);
      AddJoint(skeletonPose, ragdoll, backUpper, armUpperRight);
      AddJoint(skeletonPose, ragdoll, armUpperRight, armLowerRight);
      AddJoint(skeletonPose, ragdoll, armLowerRight, handRight);
      AddJoint(skeletonPose, ragdoll, pelvis, legUpperLeft);
      AddJoint(skeletonPose, ragdoll, legUpperLeft, legLowerLeft);
      AddJoint(skeletonPose, ragdoll, legLowerLeft, footLeft);
      AddJoint(skeletonPose, ragdoll, pelvis, legUpperRight);
      AddJoint(skeletonPose, ragdoll, legUpperRight, legLowerRight);
      AddJoint(skeletonPose, ragdoll, legLowerRight, footRight);
      #endregion

      #region ----- Add Limits -----

      // Choosing limits is difficult. 
      // We create hinge limits with AngularLimits in the back and in the knee. 
      // For all other joints we use TwistSwingLimits with symmetric cones. 

      AddAngularLimit(skeletonPose, ragdoll, pelvis, backLower, new Vector3F(0, 0, -0.3f), new Vector3F(0, 0, 0.3f));
      AddAngularLimit(skeletonPose, ragdoll, backLower, backUpper, new Vector3F(0, 0, -0.3f), new Vector3F(0, 0, 0.4f));
      AddAngularLimit(skeletonPose, ragdoll, backUpper, neck, new Vector3F(0, 0, -0.3f), new Vector3F(0, 0, 0.3f));
      AddTwistSwingLimit(ragdoll, neck, head, Matrix33F.Identity, Matrix33F.Identity, new Vector3F(-0.1f, -0.5f, -0.7f), new Vector3F(0.1f, 0.5f, 0.7f));

      var parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(backUpper).Inverse;
      var childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(armUpperLeft).Inverse;
      var bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;
      AddTwistSwingLimit(ragdoll, backUpper, armUpperLeft, bindPoseRelative.Orientation * Matrix33F.CreateRotationY(-0.5f) * Matrix33F.CreateRotationZ(-0.5f), Matrix33F.Identity, new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(0.7f, 1.2f, 1.2f));

      AddTwistSwingLimit(ragdoll, armUpperLeft, armLowerLeft, Matrix33F.CreateRotationZ(-1.2f), Matrix33F.Identity, new Vector3F(-0.3f, -1.2f, -1.2f), new Vector3F(0.3f, 1.2f, 1.2f));
      AddTwistSwingLimit(ragdoll, armLowerLeft, handLeft, Matrix33F.Identity, Matrix33F.CreateRotationX(+ConstantsF.PiOver2), new Vector3F(-0.3f, -0.7f, -0.7f), new Vector3F(0.3f, 0.7f, 0.7f));

      parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(backUpper).Inverse;
      childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(armUpperRight).Inverse;
      bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;
      AddTwistSwingLimit(ragdoll, backUpper, armUpperRight, bindPoseRelative.Orientation * Matrix33F.CreateRotationY(0.5f) * Matrix33F.CreateRotationZ(-0.5f), Matrix33F.Identity, new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(0.7f, 1.2f, 1.2f));

      AddTwistSwingLimit(ragdoll, armUpperRight, armLowerRight, Matrix33F.CreateRotationZ(-1.2f), Matrix33F.Identity, new Vector3F(-0.3f, -1.2f, -1.2f), new Vector3F(0.3f, 1.2f, 1.2f));
      AddTwistSwingLimit(ragdoll, armLowerRight, handRight, Matrix33F.Identity, Matrix33F.CreateRotationX(-ConstantsF.PiOver2), new Vector3F(-0.3f, -0.7f, -0.7f), new Vector3F(0.3f, 0.7f, 0.7f));

      parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(pelvis).Inverse;
      childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(legUpperLeft).Inverse;
      bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;
      AddTwistSwingLimit(ragdoll, pelvis, legUpperLeft, bindPoseRelative.Orientation * Matrix33F.CreateRotationZ(1.2f), Matrix33F.Identity, new Vector3F(-0.1f, -0.7f, -1.5f), new Vector3F(+0.1f, +0.7f, +1.5f));

      AddAngularLimit(skeletonPose, ragdoll, legUpperLeft, legLowerLeft, new Vector3F(0, 0, -2.2f), new Vector3F(0, 0, 0.0f));
      AddTwistSwingLimit(ragdoll, legLowerLeft, footLeft, Matrix33F.Identity, Matrix33F.Identity, new Vector3F(-0.1f, -0.3f, -0.7f), new Vector3F(0.1f, 0.3f, 0.7f));

      parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(pelvis).Inverse;
      childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(legUpperRight).Inverse;
      bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;
      AddTwistSwingLimit(ragdoll, pelvis, legUpperRight, bindPoseRelative.Orientation * Matrix33F.CreateRotationZ(1.2f), Matrix33F.Identity, new Vector3F(-0.1f, -0.7f, -1.5f), new Vector3F(+0.1f, +0.7f, +1.5f));

      AddAngularLimit(skeletonPose, ragdoll, legUpperRight, legLowerRight, new Vector3F(0, 0, -2.2f), new Vector3F(0, 0, 0.0f));
      AddTwistSwingLimit(ragdoll, legLowerRight, footRight, Matrix33F.Identity, Matrix33F.Identity, new Vector3F(-0.1f, -0.3f, -0.7f), new Vector3F(0.1f, 0.3f, 0.7f));
      #endregion

      #region ----- Add Motors -----

      ragdoll.Motors.AddRange(Enumerable.Repeat<RagdollMotor>(null, numberOfBones));
      ragdoll.Motors[pelvis] = new RagdollMotor(pelvis, -1);
      ragdoll.Motors[backLower] = new RagdollMotor(backLower, pelvis);
      ragdoll.Motors[backUpper] = new RagdollMotor(backUpper, backLower);
      ragdoll.Motors[neck] = new RagdollMotor(neck, backUpper);
      ragdoll.Motors[head] = new RagdollMotor(head, neck);
      ragdoll.Motors[armUpperLeft] = new RagdollMotor(armUpperLeft, backUpper);
      ragdoll.Motors[armLowerLeft] = new RagdollMotor(armLowerLeft, armUpperLeft);
      ragdoll.Motors[handLeft] = new RagdollMotor(handLeft, armLowerLeft);
      ragdoll.Motors[armUpperRight] = new RagdollMotor(armUpperRight, backUpper);
      ragdoll.Motors[armLowerRight] = new RagdollMotor(armLowerRight, armUpperRight);
      ragdoll.Motors[handRight] = new RagdollMotor(handRight, armLowerRight);
      ragdoll.Motors[legUpperLeft] = new RagdollMotor(legUpperLeft, pelvis);
      ragdoll.Motors[legLowerLeft] = new RagdollMotor(legLowerLeft, legUpperLeft);
      ragdoll.Motors[footLeft] = new RagdollMotor(footLeft, legLowerLeft);
      ragdoll.Motors[legUpperRight] = new RagdollMotor(legUpperRight, pelvis);
      ragdoll.Motors[legLowerRight] = new RagdollMotor(legLowerRight, legUpperRight);
      ragdoll.Motors[footRight] = new RagdollMotor(footRight, legLowerRight);
      #endregion
    }


    /// <summary>
    /// Adds a BallJoint between the specified bones.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="ragdoll">The ragdoll.</param>
    /// <param name="parent">The parent.</param>
    /// <param name="child">The child.</param>
    private static void AddJoint(SkeletonPose skeletonPose, Ragdoll ragdoll, int parent, int child)
    {
      // Get bodies and offsets for the bones.
      var skeleton = skeletonPose.Skeleton;
      var childBody = ragdoll.Bodies[child];
      var childOffset = ragdoll.BodyOffsets[child];
      var parentBody = ragdoll.Bodies[parent];
      var parentOffset = ragdoll.BodyOffsets[parent];

      // Get bind poses of the bones in model space.
      var parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(parent).Inverse;
      var childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(child).Inverse;

      // The child pose relative to the parent bone.
      var bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;

      // Add BallJoint that connects the two bones. The position of the joint is the
      // origin of the child bone.
      BallJoint joint = new BallJoint
      {
        BodyA = parentBody,
        BodyB = childBody,
        CollisionEnabled = false,
        AnchorPositionALocal = (parentOffset.Inverse * bindPoseRelative).Position,
        AnchorPositionBLocal = childOffset.Inverse.Position,
        ErrorReduction = 0.2f,
        Softness = 0.0001f,
      };
      ragdoll.Joints.Add(joint);
    }


    /// <summary>
    /// Adds a TwistSwingLimit between the specified bones. 
    /// </summary>
    /// <param name="ragdoll">The ragdoll.</param>
    /// <param name="parent">The parent bone.</param>
    /// <param name="child">The child bone.</param>
    /// <param name="parentAnchorOrientationLocal">The constraint anchor orientation relative to the parent bone.</param>
    /// <param name="childAnchorOrientationLocal">The constraint anchor orientation relative to the child bone.</param>
    /// <param name="minimum">The minimum limits (twist/swing/swing).</param>
    /// <param name="maximum">The maximum limits (twist/swing/swing).</param>
    private static void AddTwistSwingLimit(Ragdoll ragdoll, int parent, int child, Matrix33F parentAnchorOrientationLocal, Matrix33F childAnchorOrientationLocal, Vector3F minimum, Vector3F maximum)
    {
      var childBody = ragdoll.Bodies[child];
      var childOffset = ragdoll.BodyOffsets[child];
      var parentBody = ragdoll.Bodies[parent];
      var parentOffset = ragdoll.BodyOffsets[parent];

      var limit = new TwistSwingLimit
      {
        BodyA = parentBody,
        BodyB = childBody,
        AnchorOrientationALocal = parentOffset.Orientation.Transposed * parentAnchorOrientationLocal,
        AnchorOrientationBLocal = childOffset.Orientation.Transposed * childAnchorOrientationLocal,
        Minimum = minimum,
        Maximum = maximum,
        ErrorReduction = 0.2f,
        Softness = 0.001f
      };
      ragdoll.Limits.Add(limit);
    }


    /// <summary>
    /// Adds an AngularLimit between the specified bones.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="ragdoll">The ragdoll.</param>
    /// <param name="parent">The parent bone.</param>
    /// <param name="child">The child bone.</param>
    /// <param name="minimum">The minimum limits for each constraint axis (x/y/z).</param>
    /// <param name="maximum">The maximum limits for each constraint axis (x/y/z).</param>
    /// <remarks>
    /// The constraint anchor orientation is the orientation of the child bone.
    /// </remarks>
    private static void AddAngularLimit(SkeletonPose skeletonPose, Ragdoll ragdoll, int parent, int child, Vector3F minimum, Vector3F maximum)
    {
      var skeleton = skeletonPose.Skeleton;
      var childBody = ragdoll.Bodies[child];
      var childOffset = ragdoll.BodyOffsets[child];
      var parentBody = ragdoll.Bodies[parent];
      var parentOffset = ragdoll.BodyOffsets[parent];

      var parentBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(parent).Inverse;
      var childBindPoseAbsolute = (Pose)skeleton.GetBindPoseAbsoluteInverse(child).Inverse;
      var bindPoseRelative = parentBindPoseAbsolute.Inverse * childBindPoseAbsolute;

      var limit = new AngularLimit
      {
        BodyA = parentBody,
        BodyB = childBody,
        AnchorOrientationALocal = parentOffset.Orientation.Transposed * bindPoseRelative.Orientation,
        AnchorOrientationBLocal = childOffset.Orientation.Transposed,
        Minimum = minimum,
        Maximum = maximum,
        ErrorReduction = new Vector3F(0.2f),
        Softness = new Vector3F(0.001f)
      };
      ragdoll.Limits.Add(limit);
    }
  }
}
