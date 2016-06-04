// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if XNA && (WINDOWS || XBOX)

using System;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Materials;
using Microsoft.Xna.Framework.GamerServices;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Provides helper methods for working with ragdolls.
  /// </summary>
  partial class Ragdoll
  {
    /// <overloads>
    /// <summary>
    /// Creates a <see cref="Ragdoll"/> for an Xbox LIVE Avatar. (Only available on Xbox 360.)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a <see cref="Ragdoll"/> for an Xbox LIVE Avatar. (Only available on Xbox 360.)
    /// </summary>
    /// <param name="avatarPose">The avatar pose.</param>
    /// <param name="simulation">The simulation.</param>
    /// <returns>The avatar ragdoll.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="avatarPose"/> or <paramref name="simulation"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method is available only in the Xbox 360 build of the 
    /// DigitalRune.Physics.Specialized.dll.
    /// </remarks>
    public static Ragdoll CreateAvatarRagdoll(AvatarPose avatarPose, Simulation simulation)
    {
      if (avatarPose == null)
        throw new ArgumentNullException("avatarPose");
      if (simulation == null)
        throw new ArgumentNullException("simulation");

      return CreateAvatarRagdoll(avatarPose.SkeletonPose.Skeleton, simulation);
    }


    /// <summary>
    /// Creates a <see cref="Ragdoll"/> for an Xbox LIVE Avatar. (Only available on Xbox 360.)
    /// </summary>
    /// <param name="skeleton">The skeleton of the Xbox LIVE Avatar.</param>
    /// <param name="simulation">The simulation.</param>
    /// <returns>The avatar ragdoll.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeleton"/> or <paramref name="simulation"/> is 
    /// <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method is available only in the Xbox 360 build of the 
    /// DigitalRune.Physics.Specialized.dll.
    /// </remarks>
    public static Ragdoll CreateAvatarRagdoll(Skeleton skeleton, Simulation simulation)
    {
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");
      if (simulation == null)
        throw new ArgumentNullException("simulation");
      
      var ragdoll = new Ragdoll();

      // The lists ragdoll.Bodies, ragdoll.BodyOffsets and _motors contain one entry per bone - even if there 
      // is no RigidBody for this bone. - This wastes memory but simplifies the code.
      for (int i = 0; i < AvatarRenderer.BoneCount; i++)
      {
        ragdoll.Bodies.Add(null);
        ragdoll.BodyOffsets.Add(Pose.Identity);
        ragdoll.Joints.Add(null);
        ragdoll.Limits.Add(null);
        ragdoll.Motors.Add(null);
      }

      // ----- Create bodies.
      // We use the same mass for all bodies. This is not physically correct but it makes the 
      // simulation more stable, for several reasons: 
      // - It is better to avoid large mass differences. Therefore, all limbs have the same mass.
      // - Capsule shapes have a low inertia value about their height axis. This causes instability
      //   and it is better to use larger inertia values.
      var massFrame = MassFrame.FromShapeAndMass(new SphereShape(0.2f), Vector3F.One, 4, 0.1f, 1);

      // Use standard material.
      var material = new UniformMaterial();

      // Create rigid bodies for the important bones. The shapes have been manually adapted to 
      // produce useful results for thin and overweight avatars.
      // Without offset, the bodies are centered at the joint. ragdoll.BodyOffsets stores an offset pose
      // for each body. Instead, we could use TransformedShape but we can easily handle that 
      // ourselves. 
      // The collar bones are special, they use dummy shapes and are only used to connect the
      // shoulder bones.
      ragdoll.Bodies[(int)AvatarBone.Root] = new RigidBody(new BoxShape(0.22f, 0.16f, 0.16f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.Root] = new Pose(new Vector3F(0, -0.08f, -0.01f), QuaternionF.CreateRotationX(-0.0f));
      ragdoll.Bodies[(int)AvatarBone.BackLower] = new RigidBody(new BoxShape(0.22f, 0.16f, 0.16f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.BackLower] = new Pose(new Vector3F(0, 0.08f, -0.01f), QuaternionF.CreateRotationX(-0.0f));
      ragdoll.Bodies[(int)AvatarBone.BackUpper] = new RigidBody(new BoxShape(0.22f, 0.16f, 0.16f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.BackUpper] = new Pose(new Vector3F(0, 0.08f, -0.01f), QuaternionF.CreateRotationX(-0.1f));
      ragdoll.Bodies[(int)AvatarBone.Neck] = new RigidBody(new CapsuleShape(0.04f, 0.09f), massFrame, material);
      ragdoll.Bodies[(int)AvatarBone.Head] = new RigidBody(new SphereShape(0.15f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.Head] = new Pose(new Vector3F(0, 0.1f, 0));
      ragdoll.Bodies[(int)AvatarBone.CollarLeft] = new RigidBody(Shape.Empty, massFrame, material);
      ragdoll.Bodies[(int)AvatarBone.CollarRight] = new RigidBody(Shape.Empty, massFrame, material);
      ragdoll.Bodies[(int)AvatarBone.ShoulderLeft] = new RigidBody(new CapsuleShape(0.04f, 0.25f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.ShoulderLeft] = new Pose(new Vector3F(0.08f, 0, -0.02f), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      ragdoll.Bodies[(int)AvatarBone.ShoulderRight] = new RigidBody(new CapsuleShape(0.04f, 0.25f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.ShoulderRight] = new Pose(new Vector3F(-0.08f, 0, -0.02f), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      ragdoll.Bodies[(int)AvatarBone.ElbowLeft] = new RigidBody(new CapsuleShape(0.04f, 0.21f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.ElbowLeft] = new Pose(new Vector3F(0.06f, 0, -0.02f), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      ragdoll.Bodies[(int)AvatarBone.ElbowRight] = new RigidBody(new CapsuleShape(0.04f , 0.21f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.ElbowRight] = new Pose(new Vector3F(-0.06f, 0, -0.02f), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      ragdoll.Bodies[(int)AvatarBone.WristLeft] = new RigidBody(new BoxShape(0.1f, 0.04f, 0.1f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.WristLeft] = new Pose(new Vector3F(0.06f, -0.02f, -0.01f), QuaternionF.CreateRotationZ(0.0f));
      ragdoll.Bodies[(int)AvatarBone.WristRight] = new RigidBody(new BoxShape(0.1f, 0.04f, 0.1f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.WristRight] = new Pose(new Vector3F(-0.06f, -0.02f, -0.01f), QuaternionF.CreateRotationZ(0.0f));
      ragdoll.Bodies[(int)AvatarBone.HipLeft] = new RigidBody(new CapsuleShape(0.06f, 0.34f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.HipLeft] = new Pose(new Vector3F(0, -0.14f, -0.02f), QuaternionF.CreateRotationX(0.1f));
      ragdoll.Bodies[(int)AvatarBone.HipRight] = new RigidBody(new CapsuleShape(0.06f, 0.34f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.HipRight] = new Pose(new Vector3F(0, -0.14f, -0.02f), QuaternionF.CreateRotationX(0.1f));
      ragdoll.Bodies[(int)AvatarBone.KneeLeft] = new RigidBody(new CapsuleShape(0.06f, 0.36f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.KneeLeft] = new Pose(new Vector3F(0, -0.18f, -0.04f), QuaternionF.CreateRotationX(0.1f));
      ragdoll.Bodies[(int)AvatarBone.KneeRight] = new RigidBody(new CapsuleShape(0.06f, 0.36f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.KneeRight] = new Pose(new Vector3F(0, -0.18f, -0.04f), QuaternionF.CreateRotationX(0.1f));
      ragdoll.Bodies[(int)AvatarBone.AnkleLeft] = new RigidBody(new BoxShape(0.1f, 0.06f, 0.22f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.AnkleLeft] = new Pose(new Vector3F(0, -0.07f, 0.05f), QuaternionF.CreateRotationZ(0));
      ragdoll.Bodies[(int)AvatarBone.AnkleRight] = new RigidBody(new BoxShape(0.1f, 0.06f, 0.22f), massFrame, material);
      ragdoll.BodyOffsets[(int)AvatarBone.AnkleRight] = new Pose(new Vector3F(0, -0.07f, 0.05f), QuaternionF.CreateRotationZ(0));

      // ----- Add joint constraints.
      const float jointErrorReduction = 0.2f;
      const float jointSoftness = 0.0001f;
      AddJoint(ragdoll, skeleton, AvatarBone.Root, AvatarBone.BackLower, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.BackLower, AvatarBone.BackUpper, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.Neck, 0.6f, 0.000001f);
      AddJoint(ragdoll, skeleton, AvatarBone.Neck, AvatarBone.Head, 0.6f, 0.000001f);
      AddJoint(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.CollarLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.CollarRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.CollarLeft, AvatarBone.ShoulderLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.CollarRight, AvatarBone.ShoulderRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.ShoulderLeft, AvatarBone.ElbowLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.ShoulderRight, AvatarBone.ElbowRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.ElbowLeft, AvatarBone.WristLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.ElbowRight, AvatarBone.WristRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.Root, AvatarBone.HipLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.Root, AvatarBone.HipRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.HipLeft, AvatarBone.KneeLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.HipRight, AvatarBone.KneeRight, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.KneeLeft, AvatarBone.AnkleLeft, jointErrorReduction, jointSoftness);
      AddJoint(ragdoll, skeleton, AvatarBone.KneeRight, AvatarBone.AnkleRight, jointErrorReduction, jointSoftness);

      // ----- Add constraint limits.
      // We use TwistSwingLimits to define an allowed twist and swing cone for the joints. 
      // Exceptions are the back and knees, where we use AngularLimits to create hinges.
      // (We could also create a hinge with a TwistSwingLimit where the twist axis is the hinge
      // axis and no swing is allowed - but AngularLimits create more stable hinges.)
      // Another exception are the collar bones joint. We use AngularLimits to disallow any 
      // rotations. 
      AddAngularLimit(ragdoll, skeleton, AvatarBone.Root, AvatarBone.BackLower,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.BackLower).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.BackLower).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(-0.3f, 0, 0), new Vector3F(0.3f, 0, 0));

      AddAngularLimit(ragdoll, skeleton, AvatarBone.BackLower, AvatarBone.BackUpper,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.BackUpper).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.BackUpper).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(-0.3f, 0, 0), new Vector3F(0.4f, 0, 0));

      var rotationZ90Degrees = Matrix33F.CreateRotationZ(ConstantsF.PiOver2);
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.Neck, rotationZ90Degrees, rotationZ90Degrees, new Vector3F(-0.1f, -0.3f, -0.3f), new Vector3F(+0.1f, +0.3f, +0.3f));

      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.Neck, AvatarBone.Head, rotationZ90Degrees, rotationZ90Degrees, new Vector3F(-0.1f, -0.6f, -0.6f), new Vector3F(+0.1f, +0.6f, +0.6f));

      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.CollarLeft,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.CollarLeft).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.CollarLeft).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(0), new Vector3F(0));

      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.BackUpper, AvatarBone.CollarRight,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.CollarRight).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.CollarRight).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(0), new Vector3F(0));

      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.CollarLeft, AvatarBone.ShoulderLeft, Matrix33F.Identity, Matrix33F.CreateRotationY(0.7f), new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(+0.7f, +1.2f, +1.2f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.CollarRight, AvatarBone.ShoulderRight, Matrix33F.Identity, Matrix33F.CreateRotationY(-0.7f), new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(+0.7f, +1.2f, +1.2f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.ShoulderLeft, AvatarBone.ElbowLeft, Matrix33F.Identity, Matrix33F.CreateRotationY(1.2f), new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(+0.7f, +1.2f, +1.2f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.ShoulderRight, AvatarBone.ElbowRight, Matrix33F.Identity, Matrix33F.CreateRotationY(-1.2f), new Vector3F(-0.7f, -1.2f, -1.2f), new Vector3F(+0.7f, +1.2f, +1.2f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.ElbowLeft, AvatarBone.WristLeft, Matrix33F.Identity, Matrix33F.Identity, new Vector3F(-0.7f, -0.7f, -0.7f), new Vector3F(+0.7f, +0.7f, +0.7f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.ElbowRight, AvatarBone.WristRight, Matrix33F.Identity, Matrix33F.Identity, new Vector3F(-0.7f, -0.7f, -0.7f), new Vector3F(+0.7f, +0.7f, +0.7f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.Root, AvatarBone.HipLeft, rotationZ90Degrees, Matrix33F.CreateRotationX(-1.2f) * Matrix33F.CreateRotationZ(ConstantsF.PiOver2 + 0.2f), new Vector3F(-0.1f, -1.5f, -0.7f), new Vector3F(+0.1f, +1.5f, +0.7f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.Root, AvatarBone.HipRight, rotationZ90Degrees, Matrix33F.CreateRotationX(-1.2f) * Matrix33F.CreateRotationZ(ConstantsF.PiOver2 - 0.2f), new Vector3F(-0.1f, -1.5f, -0.7f), new Vector3F(+0.1f, +1.5f, +0.7f));

      AddAngularLimit(ragdoll, skeleton, AvatarBone.HipLeft, AvatarBone.KneeLeft,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.KneeLeft).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.KneeLeft).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(0, 0, 0), new Vector3F(2.2f, 0, 0));

      AddAngularLimit(ragdoll, skeleton, AvatarBone.HipRight, AvatarBone.KneeRight,
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.KneeRight).Rotation.Conjugated.ToRotationMatrix33(),
        skeleton.GetBindPoseAbsoluteInverse((int)AvatarBone.KneeRight).Rotation.Conjugated.ToRotationMatrix33(), 
        new Vector3F(0, 0, 0), new Vector3F(2.2f, 0, 0));

      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.KneeLeft, AvatarBone.AnkleLeft, rotationZ90Degrees, rotationZ90Degrees, new Vector3F(-0.1f, -0.7f, -0.3f), new Vector3F(+0.1f, +0.7f, +0.3f));
      AddTwistSwingLimit(ragdoll, skeleton, AvatarBone.KneeRight, AvatarBone.AnkleRight, rotationZ90Degrees, rotationZ90Degrees, new Vector3F(-0.1f, -0.7f, -0.3f), new Vector3F(+0.1f, +0.7f, +0.3f));

      // ----- Add motors
      // We use QuaternionMotors to create forces that rotate the bones into desired poses.
      // This can be used for damping, spring or animating the ragdoll.
      AddMotor(ragdoll, (AvatarBone)(-1), AvatarBone.Root);
      AddMotor(ragdoll, AvatarBone.Root, AvatarBone.BackLower);
      AddMotor(ragdoll, AvatarBone.BackLower, AvatarBone.BackUpper);
      AddMotor(ragdoll, AvatarBone.BackUpper, AvatarBone.Neck);
      AddMotor(ragdoll, AvatarBone.Neck, AvatarBone.Head);
      AddMotor(ragdoll, AvatarBone.BackUpper, AvatarBone.CollarLeft);
      AddMotor(ragdoll, AvatarBone.BackUpper, AvatarBone.CollarRight);
      AddMotor(ragdoll, AvatarBone.CollarLeft, AvatarBone.ShoulderLeft);
      AddMotor(ragdoll, AvatarBone.CollarRight, AvatarBone.ShoulderRight);
      AddMotor(ragdoll, AvatarBone.ShoulderLeft, AvatarBone.ElbowLeft);
      AddMotor(ragdoll, AvatarBone.ShoulderRight, AvatarBone.ElbowRight);
      AddMotor(ragdoll, AvatarBone.ElbowLeft, AvatarBone.WristLeft);
      AddMotor(ragdoll, AvatarBone.ElbowRight, AvatarBone.WristRight);
      AddMotor(ragdoll, AvatarBone.Root, AvatarBone.HipLeft);
      AddMotor(ragdoll, AvatarBone.Root, AvatarBone.HipRight);
      AddMotor(ragdoll, AvatarBone.HipLeft, AvatarBone.KneeLeft);
      AddMotor(ragdoll, AvatarBone.HipRight, AvatarBone.KneeRight);
      AddMotor(ragdoll, AvatarBone.KneeLeft, AvatarBone.AnkleLeft);
      AddMotor(ragdoll, AvatarBone.KneeRight, AvatarBone.AnkleRight);

      // ----- Set collision filters.
      // Collisions between connected bones have been disabled with Constraint.CollisionEnabled
      // = false in the joints. We need to disable a few other collisions.
      // Following bodies do not collide with anything. They are only used to connect other
      // bones.
      ragdoll.Bodies[(int)AvatarBone.Neck].CollisionObject.Enabled = false;
      ragdoll.Bodies[(int)AvatarBone.CollarLeft].CollisionObject.Enabled = false;
      ragdoll.Bodies[(int)AvatarBone.CollarRight].CollisionObject.Enabled = false;

      // We disable filters for following body pairs because they are usually penetrating each
      // other, which needs to be ignored.
      var filter = simulation.CollisionDomain.CollisionDetection.CollisionFilter as CollisionFilter;
      if (filter != null)
      {
        filter.Set(ragdoll.Bodies[(int)AvatarBone.BackUpper].CollisionObject, ragdoll.Bodies[(int)AvatarBone.ShoulderLeft].CollisionObject, false);
        filter.Set(ragdoll.Bodies[(int)AvatarBone.BackUpper].CollisionObject, ragdoll.Bodies[(int)AvatarBone.ShoulderRight].CollisionObject, false);
      }
      
      return ragdoll;
    }

    
    private static void AddJoint(Ragdoll ragdoll, Skeleton skeleton, AvatarBone parentBone, AvatarBone childBone, float errorReduction, float softness)
    {
      int parentIndex = (int)parentBone;
      int childIndex = (int)childBone;

      // To define AnchorPositionALocal/AnchorPositionBLocal:
      // To get the AnchorPositionALocal we apply jointPosesAbsolute[indexA].Inverse to 
      // convert the joint pose from model space into the joints space of parentBone. Then we apply
      // ragdoll.BodyOffsets[boneAIndex].Inverse to convert from joint space to body space. The result is
      // the joint position of B in body space of A.
      // To get AnchorPositionBLocal, we only have to apply the inverse offset.

      BallJoint joint = new BallJoint
      {
        BodyA = ragdoll.Bodies[parentIndex],
        BodyB = ragdoll.Bodies[childIndex],
        CollisionEnabled = false,
        AnchorPositionALocal = (ragdoll.BodyOffsets[parentIndex].Inverse * skeleton.GetBindPoseAbsoluteInverse(parentIndex) * skeleton.GetBindPoseAbsoluteInverse(childIndex).Inverse).Translation,
        AnchorPositionBLocal = ragdoll.BodyOffsets[childIndex].Inverse.Position,
        ErrorReduction = errorReduction,
        Softness = softness,
      };
      ragdoll.Joints[childIndex] = joint;
    }


    private static void AddTwistSwingLimit(Ragdoll ragdoll, Skeleton skeleton, AvatarBone parentBone, AvatarBone childBone, Matrix33F orientationA, Matrix33F orientationB, Vector3F minimum, Vector3F maximum)
    {
      int parentIndex = (int)parentBone;
      int childIndex = (int)childBone;

      // The difficult part is to define the constraint anchor orientation. 
      // Here is how we do it: 
      // When we look at the front side of an Avatar in bind pose, the x-axis is parallel 
      // to the arms. y points up and z is normal to the those axes.
      //
      // To define orientationA/B:
      // The anchor x-axis is the twist axis. That means, this is already the correct axis
      // for the hands (wrist joints) and orientationA/B are therefore Matrix33F.Identity.
      // For the Head, the twist axis must point up. Therefore orientationA/B must be a 90°
      // rotation about z to rotate the twist axis up.
      // For the shoulder-elbow connection, orientationA is Matrix.Identity. The swing cone must
      // not be parallel to the arm axis (because the elbow cannot bend backwards). Therefore,
      // orientationB defines a rotation that rotates the twist axis (= swing cone center) to the
      // front.
      // 
      // To define AnchorOrientationALocal/AnchorOrientationBLocal:
      // AnchorOrientationALocal must be a rotation matrix that transforms a vector from local
      // constraint anchor space to local body space of A. 
      // orientationA defines the constraint anchor orientation in model space.
      // With jointPosesAbsolute[boneAIndex].Orientation.Transposed, we convert from model space
      // to joint space. With ragdoll.BodyOffsets[boneAIndex].Orientation.Transposed, we convert from joint
      // space to body space. The combined rotation matrix converts from constraint anchor space
      // to body space.

      var limit = new TwistSwingLimit
      {
        BodyA = ragdoll.Bodies[parentIndex],
        BodyB = ragdoll.Bodies[childIndex],
        AnchorOrientationALocal = ragdoll.BodyOffsets[parentIndex].Orientation.Transposed * skeleton.GetBindPoseAbsoluteInverse(parentIndex).Rotation.ToRotationMatrix33() * orientationA,
        AnchorOrientationBLocal = ragdoll.BodyOffsets[childIndex].Orientation.Transposed * skeleton.GetBindPoseAbsoluteInverse(childIndex).Rotation.ToRotationMatrix33() * orientationB,
        Minimum = minimum,
        Maximum = maximum,
        ErrorReduction = 0.2f,
        Softness = 0.001f
      };
      ragdoll.Limits[childIndex] = limit;
    }


    private static void AddAngularLimit(Ragdoll ragdoll, Skeleton skeleton, AvatarBone parentBone, AvatarBone childBone, Matrix33F orientationA, Matrix33F orientationB, Vector3F minimum, Vector3F maximum)
    {
      // Similar to AddTwistSwingLimit

      int parentIndex = (int)parentBone;
      int childIndex = (int)childBone;

      var limit = new AngularLimit
      {
        BodyA = ragdoll.Bodies[parentIndex],
        BodyB = ragdoll.Bodies[childIndex],
        AnchorOrientationALocal = ragdoll.BodyOffsets[parentIndex].Orientation.Transposed * skeleton.GetBindPoseAbsoluteInverse(parentIndex).Rotation.ToRotationMatrix33() * orientationA,
        AnchorOrientationBLocal = ragdoll.BodyOffsets[childIndex].Orientation.Transposed * skeleton.GetBindPoseAbsoluteInverse(childIndex).Rotation.ToRotationMatrix33() * orientationB,
        Minimum = minimum,
        Maximum = maximum,
        ErrorReduction = new Vector3F(0.2f),
        Softness = new Vector3F(0.001f)
      };
      ragdoll.Limits[childIndex] = limit;
    }


    private static void AddMotor(Ragdoll ragdoll, AvatarBone parentBone, AvatarBone childBone)
    {
      // A quaternion motor controls the relative orientation between two bodies. The target
      // orientation is specified with a quaternion. The target orientations are set in
      // SetMotorTargets.

      // We can use the motors to achieve following results:
      // - No motors: The ragdoll joints are not damped and the bones swing a lot (within the 
      //   allowed limits).
      // - Damping: If DampingConstant > 0 and SpringConstant == 0, the relative bone rotations
      //   are damped. This simulates joint friction and muscles forces acting against the movement. 
      // - Springs: If DampingConstant > 0 and SpringConstant > 0, the motors try to move the
      //   ragdoll limbs to a pose defined by the TargetOrientations of the motors. This could be,
      //   for example, a defensive pose of a character.
      // - Animation: Like "Springs" but the TargetOrientation is changed in each frame. This 
      //   way the ragdoll performs a user defined animation while still reacting to impacts.

      int parentIndex = (int)parentBone;
      int childIndex = (int)childBone;

      var motor = new RagdollMotor(childIndex, parentIndex);

      ragdoll.Motors[childIndex] = motor;
    }
  }
}
#endif
