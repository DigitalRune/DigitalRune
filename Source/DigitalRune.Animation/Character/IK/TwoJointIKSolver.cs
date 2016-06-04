// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Modifies a two-bone chain to reach a certain target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IKSolver"/> modifies two bones in a bone chain. It is typically used for arms
  /// and legs. For the solver, the first bone (see <see cref="RootBoneIndex"/>) can rotate freely.
  /// The second bone (see <see cref="HingeBoneIndex"/>) is attached with a hinge. The bone chain 
  /// can contain more than 2 bones (e.g. the upper leg can consist of several bones). But only 2 
  /// bones are modified. The bones are rotated so that the end of the chain (defined by 
  /// <see cref="TipBoneIndex"/>) reaches the target.
  /// </para>
  /// <para>
  /// Limitations of this IK solver: The bones must lie in a plane normal to the hinge axis;
  /// otherwise, the target will not be reached exactly.
  /// </para>
  /// <para>
  /// See also <see cref="IKSolver"/> for more general information. 
  /// </para>
  /// </remarks>
  public class TwoJointIKSolver : IKSolver
  {
    // TODO: Improve TwoJointIKSolver
    // - Support bones that are not normal to the hinge axis. In this case
    //   the tip can move in a plane normal to the hinge axis. In a first step
    //   this plane must be rotated so that it contains the target. Then the
    //   solution in this plane must be found. Difficult.
    // - Support TipRotationAbsolute that the user can specify optionally.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty = true;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the index of the root bone.
    /// </summary>
    /// <value>
    /// The index of the root bone; for example, the upper arm bone (shoulder joint) or the upper
    /// leg bone (hip joint).
    /// </value>
    public int RootBoneIndex
    {
      get { return _rootBoneIndex; }
      set
      {
        if (_rootBoneIndex != value)
        {
          _rootBoneIndex = value;
          OnInvalidate();
        }
      }
    }
    private int _rootBoneIndex;


    /// <summary>
    /// Gets or sets the index of the tip bone that determines the end of the chain.
    /// </summary>
    /// <value>
    /// The index of the tip bone that determines the end of the chain; for example, the hand bone
    /// (wrist joint) or the foot bone (ankle joint).
    /// </value>
    public int TipBoneIndex
    {
      get { return _tipBoneIndex; }
      set
      {
        if (_tipBoneIndex != value)
        {
          _tipBoneIndex = value;
          OnInvalidate();
        }
      }
    }
    private int _tipBoneIndex;


    /// <summary>
    /// Gets or sets the tip offset relative to the tip bone.
    /// </summary>
    /// <value>
    /// The tip offset relative to the tip bone; for example, the vector from the wrist to the hand 
    /// center or the vector from ankle to the bottom of a foot. The default value is a zero vector.
    /// </value>
    /// <remarks>
    /// If this offset is zero, the IK solver will try to move the origin of the tip bone to the
    /// <see cref="IKSolver.Target"/>. For example, if the solver is used for an arm and the hand
    /// bone is the tip bone, then the character will "grab" the target with the wrist where the
    /// hand bone starts. The <see cref="TipOffset"/> should be set to the offset from the wrist to
    /// the hand center. Then the target will be grabbed correctly with the hand center and not the
    /// wrist.
    /// </remarks>
    public Vector3F TipOffset
    {
      get { return _tipOffset; }
      set
      {
        if (_tipOffset != value)
        {
          _tipOffset = value;
          OnInvalidate();
        }
      }
    }
    private Vector3F _tipOffset;


    /// <summary>
    /// Gets or sets the index of the hinge bone.
    /// </summary>
    /// <value>
    /// The index of the hinge bone; for example, the lower arm bone (elbow joint) or the lower leg 
    /// bone (knee joint).
    /// </value>
    public int HingeBoneIndex
    {
      get { return _hingeBoneIndex; }
      set
      {
        if (_hingeBoneIndex != value)
        {
          _hingeBoneIndex = value;
          OnInvalidate();
        }
      }
    }
    private int _hingeBoneIndex;


    /// <summary>
    /// Gets or sets the hinge axis relative to the root bone.
    /// </summary>
    /// <value>
    /// The hinge axis relative to the root bone. The vector must not be a zero length
    /// vector. The default is (0, 1, 0).
    /// </value>
    public Vector3F HingeAxis
    {
      get { return _hingeAxis; }
      set
      {
        if (_hingeAxis != value)
        {
          if (!value.TryNormalize())
            throw new ArgumentException("The HingeAxis vector must not be a zero vector.");

          _hingeAxis = value;
        }
      }
    }
    private Vector3F _hingeAxis;


    /// <summary>
    /// Gets or sets the min hinge angle.
    /// </summary>
    /// <value>
    /// The min hinge angle in radians. The default is 0.
    /// </value>
    public float MinHingeAngle { get; set; }


    /// <summary>
    /// Gets or sets the max hinge angle.
    /// </summary>
    /// <value>
    /// The max hinge angle in radians. The default is 3π/4 (= 135°).
    /// </value>
    public float MaxHingeAngle { get; set; }


    /// <summary>
    /// Gets or sets the desired absolute tip bone rotation.
    /// </summary>
    /// <value>
    /// The desired absolute tip bone rotation. The default is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this value is <see langword="null"/>, the IK solver does not influence the orientation
    /// of the tip bone. This value can be set to a target rotation relative to model space. The IK
    /// solver will make sure that the tip bone ends up in this orientation.
    /// </para>
    /// <para>
    /// This is useful if, for example, a character grabs a bar with the hand. The hand bone should 
    /// be aligned with the direction of the bar. <see cref="TipBoneOrientation"/> must be set 
    /// according to the orientation of the bar.
    /// </para>
    /// </remarks>
    public QuaternionF? TipBoneOrientation { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoJointIKSolver"/> class.
    /// </summary>
    public TwoJointIKSolver()
    {
      HingeAxis = Vector3F.UnitY;
      MinHingeAngle = 0;
      MaxHingeAngle = 3 * ConstantsF.PiOver4;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the <see cref="SkeletonPose"/> was exchanged.
    /// </summary>
    protected override void OnInvalidate()
    {
      _isDirty = true;
      base.OnInvalidate();
    }


    /// <summary>
    /// Called when <see cref="IKSolver.Solve"/> is called.
    /// </summary>
    /// <param name="deltaTime">The current time step (in seconds).</param>
    protected override void OnSolve(float deltaTime)
    {
      if (_isDirty)
      {
        // Validate bone chain.
        if (!SkeletonPose.IsAncestorOrSelf(RootBoneIndex, HingeBoneIndex))
          throw new ArgumentException("The RootBoneIndex and the HingeBoneIndex do not form a valid bone chain.");
        if (!SkeletonPose.IsAncestorOrSelf(HingeBoneIndex, TipBoneIndex))
          throw new ArgumentException("The HingeBoneIndex and the TipBoneIndex do not form a valid bone chain.");
      }

      _isDirty = false;

      if (MinHingeAngle > MaxHingeAngle)
        throw new AnimationException("The MinHingeAngle must be less than or equal to the MaxHingeAngle");

      // Remember original bone transforms for interpolation at the end.
      var originalRootBoneTransform = SkeletonPose.GetBoneTransform(RootBoneIndex);
      var originalHingeBoneTransform = SkeletonPose.GetBoneTransform(HingeBoneIndex);
      var originalTipBoneTransform = SkeletonPose.GetBoneTransform(TipBoneIndex);

      var rootBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(RootBoneIndex);
      var hingeBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(HingeBoneIndex);
      var tipBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(TipBoneIndex);

      // Get tip position in model space.
      Vector3F tipAbsolute;
      if (TipBoneOrientation != null)
      {
        // If the user has specified an absolute tip rotation, then we consider this rotation and
        // use the tip bone origin as tip.
        tipAbsolute = tipBonePoseAbsolute.Translation;
        Target -= tipBonePoseAbsolute.ToParentDirection(tipBonePoseAbsolute.Scale * TipOffset);
      }
      else
      {
        // The user hasn't specified a desired tip rotation. Therefore we do not modify the
        // tip rotation and use the offset position in the tip bone as tip.
        tipAbsolute = tipBonePoseAbsolute.ToParentPosition(TipOffset);
      }

      // Abort if we already touch the target.
      if (Vector3F.AreNumericallyEqual(tipAbsolute, Target))
        return;

      // Root to target vector.
      var rootToTarget = Target - rootBonePoseAbsolute.Translation;
      var rootToTargetLength = rootToTarget.Length;
      if (Numeric.IsZero(rootToTargetLength))
        return;
      rootToTarget /= rootToTargetLength;

      // ----- Align chain with target.
      // Align the root to target vector with the root to tip vector.
      var rootToTip = tipAbsolute - rootBonePoseAbsolute.Translation;
      var rootToTipLength = rootToTip.Length;
      if (!Numeric.IsZero(rootToTipLength))
      {
        rootToTip /= rootToTipLength;
        
        var rotation = QuaternionF.CreateRotation(rootToTip, rootToTarget);
        if (rotation.Angle > Numeric.EpsilonF)
        {
          // Apply rotation to root bone.
          rootBonePoseAbsolute.Rotation = rotation * rootBonePoseAbsolute.Rotation;
          SkeletonPose.SetBoneRotationAbsolute(RootBoneIndex, rootBonePoseAbsolute.Rotation);
          hingeBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(HingeBoneIndex);

          // Compute new tip absolute tip position from the known quantities.
          tipAbsolute = rootBonePoseAbsolute.Translation + rootToTarget * rootToTipLength;
        }
      }

      // ----- Compute ideal angle.
      var rootToHinge = hingeBonePoseAbsolute.Translation - rootBonePoseAbsolute.Translation;
      var hingeToTip = tipAbsolute - hingeBonePoseAbsolute.Translation;
      var hingeAxis = hingeBonePoseAbsolute.ToParentDirection(HingeAxis);

      // Project vectors to hinge plane. Everything should be in a plane for the following 
      // computations.
      rootToHinge -= hingeAxis * Vector3F.Dot(rootToHinge, hingeAxis);
      hingeToTip -= hingeAxis * Vector3F.Dot(hingeToTip, hingeAxis);

      // Get lengths.
      float rootToHingeLength = rootToHinge.Length;
      if (Numeric.IsZero(rootToHingeLength))
        return;
      rootToHinge /= rootToHingeLength;

      float hingeToTipLength = hingeToTip.Length;
      if (Numeric.IsZero(hingeToTipLength))
        return;
      hingeToTip /= hingeToTipLength;

      // Compute current hinge angle (angle between root bone and hinge bone).
      float currentHingeAngle = (float)Math.Acos(MathHelper.Clamp(Vector3F.Dot(rootToHinge, hingeToTip), -1, 1));

      // Make sure the computed angle is about the hingeAxis and not about -hingeAxis.
      if (Vector3F.Dot(Vector3F.Cross(rootToHinge, hingeToTip), hingeAxis) < 0)
        currentHingeAngle = -currentHingeAngle;

      // Using law of cosines to compute the desired hinge angle using the triangle lengths.
      float cosDesiredHingeAngle = (rootToHingeLength * rootToHingeLength + hingeToTipLength * hingeToTipLength - rootToTargetLength * rootToTargetLength)
                                   / (2 * rootToHingeLength * hingeToTipLength);
      float desiredHingeAngle = ConstantsF.Pi - (float)Math.Acos(MathHelper.Clamp(cosDesiredHingeAngle, -1, 1));

      // Apply hinge limits.
      if (desiredHingeAngle < MinHingeAngle)
        desiredHingeAngle = MinHingeAngle;
      else if (desiredHingeAngle > MaxHingeAngle)
        desiredHingeAngle = MaxHingeAngle;

      // Compute delta rotation between current and desired angle.
      float deltaAngle = desiredHingeAngle - currentHingeAngle;
      var hingeRotation = QuaternionF.CreateRotation(hingeAxis, deltaAngle);
      hingeBonePoseAbsolute.Rotation = hingeRotation * hingeBonePoseAbsolute.Rotation;

      // Update tip position.
      tipAbsolute = hingeBonePoseAbsolute.Translation + hingeRotation.Rotate(tipAbsolute - hingeBonePoseAbsolute.Translation);

      // ----- Align chain with target.
      // If we hit a hinge limit, then we can move the tip closer to the target by aligning
      // the whole chain again.
      rootToTip = tipAbsolute - rootBonePoseAbsolute.Translation;
      rootToTipLength = rootToTip.Length;
      if (!Numeric.IsZero(rootToTipLength))
      {
        rootToTip /= rootToTipLength;
        var rotation = QuaternionF.CreateRotation(rootToTip, rootToTarget);
        rootBonePoseAbsolute.Rotation = rotation * rootBonePoseAbsolute.Rotation;
        hingeBonePoseAbsolute.Rotation = rotation * hingeBonePoseAbsolute.Rotation;
      }

      // ----- Set results.
      SkeletonPose.SetBoneRotationAbsolute(RootBoneIndex, rootBonePoseAbsolute.Rotation);
      SkeletonPose.SetBoneRotationAbsolute(HingeBoneIndex, hingeBonePoseAbsolute.Rotation);

      if (TipBoneOrientation != null)
        SkeletonPose.SetBoneRotationAbsolute(TipBoneIndex, TipBoneOrientation.Value);
      
      // ----- Apply weight, velocity limit and set results.
      bool requiresBlending = RequiresBlending();
      float maxRotationAngle;
      bool requiresLimiting = RequiresLimiting(deltaTime, out maxRotationAngle);
      if (requiresBlending || requiresLimiting)
      {
        var targetBoneTransform = SkeletonPose.GetBoneTransform(RootBoneIndex);
        if (requiresBlending)
          BlendBoneTransform(ref originalRootBoneTransform, ref targetBoneTransform);
        if (requiresLimiting)
          LimitBoneTransform(ref originalRootBoneTransform, ref targetBoneTransform, maxRotationAngle);
        SkeletonPose.SetBoneTransform(RootBoneIndex, targetBoneTransform);

        targetBoneTransform = SkeletonPose.GetBoneTransform(HingeBoneIndex);
        if (requiresBlending)
          BlendBoneTransform(ref originalHingeBoneTransform, ref targetBoneTransform);
        if (requiresLimiting)
          LimitBoneTransform(ref originalHingeBoneTransform, ref targetBoneTransform, maxRotationAngle);
        SkeletonPose.SetBoneTransform(HingeBoneIndex, targetBoneTransform);

        targetBoneTransform = SkeletonPose.GetBoneTransform(TipBoneIndex);
        if (requiresBlending)
          BlendBoneTransform(ref originalTipBoneTransform, ref targetBoneTransform);
        if (requiresLimiting)
          LimitBoneTransform(ref originalTipBoneTransform, ref targetBoneTransform, maxRotationAngle);
        SkeletonPose.SetBoneTransform(TipBoneIndex, targetBoneTransform);
      }
    }
    #endregion
  }
}
