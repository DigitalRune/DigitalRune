// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Rotates a bone to look at a target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IKSolver"/> rotates a bone - usually the head bone - to look to the 
  /// <see cref="IKSolver.Target"/> position. The bone to be rotated is specified by 
  /// <see cref="BoneIndex"/>.
  /// </para>
  /// <para>
  /// <see cref="Forward"/> defines the forward look direction relative to the bone space. 
  /// <see cref="Up"/> defines the up direction relative to the bone space. <see cref="EyeOffset"/> 
  /// can be used to offset look origin relative to the bone space. For example, the origin of a 
  /// "head" bone is often at the neck of a character. <see cref="EyeOffset"/> can be used to set
  /// the offset from the bone origin to the eyes of the character. 
  /// </para>
  /// <para>
  /// <see cref="Limit"/> specifies an angular limit like a cone around the forward direction.
  /// The IK solver will not rotate the beyond this limit. See <see cref="Limit">description</see>
  /// of the property.
  /// </para>
  /// <para>
  /// See also <see cref="IKSolver"/> for more general information. 
  /// </para>
  /// </remarks>
  public class LookAtIKSolver : IKSolver
  {
    // TODO: more flexible limits

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the index of the bone.
    /// </summary>
    /// <value>The index of the bone (usually the head bone).</value>
    public int BoneIndex { get; set; }


    /// <summary>
    /// Gets or sets the forward direction in bone space.
    /// </summary>
    /// <value>The forward direction in bone space.</value>
    public Vector3F Forward { get; set; }


    /// <summary>
    /// Gets or sets the up direction in bone space.
    /// </summary>
    /// <value>The up direction in bone space.</value>
    public Vector3F Up { get; set; }


    /// <summary>
    /// Gets or sets the eye offset in bone space.
    /// </summary>
    /// <value>The eye offset in bone space.</value>
    public Vector3F EyeOffset { get; set; }


    /// <summary>
    /// Gets or sets the rotation limit.
    /// </summary>
    /// <value>
    /// The rotation limit in radians in the range [0, π/2[. If the value is not in this 
    /// range, the limit is disabled. Per default, the value is ∞ and the limit is disabled.
    /// </value>
    public float Limit { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LookAtIKSolver"/> class.
    /// </summary>
    public LookAtIKSolver()
    {
      Limit = float.PositiveInfinity;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when <see cref="IKSolver.Solve"/> is called.
    /// </summary>
    /// <param name="deltaTime">The current time step (in seconds).</param>
    protected override void OnSolve(float deltaTime)
    {
      // Get cosine of limit or -1 if unlimited.
      float cosLimits = -1;
      if (0 <= Limit && Limit < ConstantsF.PiOver2)
        cosLimits = (float)Math.Cos(Limit);

      var skeleton = SkeletonPose.Skeleton;
      int parentIndex = skeleton.GetParent(BoneIndex);

      // Bone pose without a bone transform.
      var unanimatedBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(parentIndex) * skeleton.GetBindPoseRelative(BoneIndex);

      // Target position and direction in bone space.
      var targetPositionLocal = unanimatedBonePoseAbsolute.ToLocalPosition(Target);
      var targetDirection = targetPositionLocal - EyeOffset;
      if (!targetDirection.TryNormalize())
        return;

      // The axes of the view space (where forward is -z, relative to bone space). 
      Vector3F forward = Forward;
      Vector3F up = Up;
      Vector3F side = Vector3F.Cross(up, -forward);

      // This matrix converts from view space to bone space (in other words, it 
      // rotates the -z direction into the view direction).
      var boneFromView = new Matrix33F(side.X, up.X, -forward.X,
                                       side.Y, up.Y, -forward.Y,
                                       side.Z, up.Z, -forward.Z);

      // Get the components of the target direction relative to the view space axes.
      float targetUp = Vector3F.Dot(targetDirection, up);
      float targetSide = Vector3F.Dot(targetDirection, side);
      float targetForward = Vector3F.Dot(targetDirection, forward);

      // Limit rotations of the desired up and side vector.
      // The target forward direction is inverted if necessary. (If limited the bone 
      // does not never rotate back.)
      if (cosLimits > 0)
      {
        cosLimits = (float)Math.Sqrt(1 - cosLimits * cosLimits);
        if (targetUp > 0 && targetUp > cosLimits)
          targetUp = cosLimits;
        else if (targetUp < 0 && -targetUp > cosLimits)
          targetUp = -cosLimits;

        if (targetSide > 0 && targetSide > cosLimits)
          targetSide = cosLimits;
        else if (targetSide < 0 && -targetSide > cosLimits)
          targetSide = -cosLimits;

        targetForward = Math.Abs(targetForward);
      }

      // Make new target direction vector that conforms to the limits.
      targetDirection = Math.Sign(targetForward)
        * forward * (float)Math.Sqrt(Math.Max(0, 1 - targetUp * targetUp - targetSide * targetSide))
        + side * targetSide + up * targetUp;

      Debug.Assert(targetDirection.IsNumericallyNormalized);

      // Make axes of desired view space. 
      forward = targetDirection;
      side = Vector3F.Cross(up, -forward);
      if (!side.TryNormalize())
        return;

      up = Vector3F.Cross(side, forward);
      Debug.Assert(up.IsNumericallyNormalized);

      // Create new view space matrix.
      var boneFromNewView = new Matrix33F(
        side.X, up.X, -forward.X,
        side.Y, up.Y, -forward.Y,
        side.Z, up.Z, -forward.Z);

      // Apply a bone transform that rotates the rest view space to the desired view space.
      QuaternionF boneTransform = QuaternionF.CreateRotation(boneFromNewView * boneFromView.Transposed);

      var startTransform = SkeletonPose.GetBoneTransform(BoneIndex);
      var lookAtTransform = new SrtTransform(startTransform.Scale, boneTransform, startTransform.Translation);

      // Apply weight.
      if (RequiresBlending())
        BlendBoneTransform(ref startTransform, ref lookAtTransform);

      // Apply angular velocity limit.
      float maxRotationAngle;
      if (RequiresLimiting(deltaTime, out maxRotationAngle))
        LimitBoneTransform(ref startTransform, ref lookAtTransform, maxRotationAngle);

      SkeletonPose.SetBoneTransform(BoneIndex, lookAtTransform);
    }
    #endregion
  }
}
