// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Modifies a skeleton using the cyclic-coordinate descent (CCD) algorithm.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IKSolver"/> uses the cyclic-coordinate descent (CCD) algorithm to modify a
  /// bone chain to reach the <see cref="IKSolver.Target"/> position. <see cref="RootBoneIndex"/>
  /// determines the first bone in the chain. <see cref="TipBoneIndex"/> determines the last bone
  /// that is included in the chain. This IK solver rotates all bones in the chain, so that the tip
  /// of the bone chain reaches the <see cref="IKSolver.Target"/> position.
  /// </para>
  /// <para>
  /// This solver uses an iterative algorithm. <see cref="NumberOfIterations"/> limits the maximal
  /// number allowed iterations. The algorithm ends early if the distance between the
  /// <see cref="IKSolver.Target"/> and the tip of the chain is less than 
  /// <see cref="AllowedDeviation"/>.
  /// </para>
  /// <para>
  /// <strong>Bone rotation limits: </strong><br/>
  /// Per default, the <see cref="IKSolver"/> assumes that the bones can rotate in any direction
  /// without rotation limits. If the bone rotations should be limited (e.g. "do not rotate about
  /// the y axis" or "do not rotate more than 45°"), then a <see cref="LimitBoneTransforms"/>
  /// callback must be set. The <see cref="LimitBoneTransforms"/> callback must be a method that
  /// checks the current bone rotations and removes any invalid rotations. See 
  /// <see cref="LimitBoneTransforms"/> for more details.
  /// </para>
  /// <para>
  /// See also <see cref="IKSolver"/> for more general information. 
  /// </para>
  /// </remarks>
  public class CcdIKSolver : IKSolver
  {
    // CCD is described in Game Programming Gems 3.
    // TODO: Add support for solving several concurrent chains.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty = true;

    // A list that is re-used in each OnSolve(). It does not store data between calls.
    private readonly List<SrtTransform> _originalTransforms = new List<SrtTransform>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the index of the root bone.
    /// </summary>
    /// <value>The index of the root bone.</value>
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
    /// Gets or sets the index of the tip bone.
    /// </summary>
    /// <value>The index of the tip bone.</value>
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
    /// Gets or sets the tip offset in tip bone space.
    /// </summary>
    /// <value>
    /// The tip offset in tip bone space. The default is a zero vector.
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
    /// Gets or sets the number of iterations.
    /// </summary>
    /// <value>The number of iterations. The default is 10.</value>
    public int NumberOfIterations { get; set; }


    /// <summary>
    /// Gets or sets the allowed distance error.
    /// </summary>
    /// <value>The allowed distance error. The default is 0.01.</value>
    /// <remarks>
    /// <para>
    /// This solver uses an iterative algorithm. The algorithm ends early if the distance between
    /// the <see cref="IKSolver.Target"/> and the tip of the chain is less than 
    /// <see cref="AllowedDeviation"/>.
    /// </para>
    /// </remarks>
    public float AllowedDeviation { get; set; }


    /// <summary>
    /// Gets or sets the bone gain.
    /// </summary>
    /// <value>The bone gain in the range ]0, 1]. The default is 1.</value>
    /// <remarks>
    /// If this value is less than 1, the algorithm will need more iterations to reach the target
    /// but the result will be smoother.
    /// </remarks>
    public float BoneGain { get; set; }


    /// <summary>
    /// Gets or sets the a callback that enforces rotation limits.
    /// </summary>
    /// <value>
    /// The callback that enforces rotation limits. The default is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this property is <see langword="null"/>, the bone rotations are not limited. If the bone 
    /// rotations should be limited, this property must be set to a method that enforces the bone 
    /// limits: The method should simply check the bones and rotate the bones back to the allowed 
    /// range. 
    /// </para>
    /// <para>
    /// This method is called every time a bone was rotated. The method will get the bone index as 
    /// the only parameter. The method should check and correct the rotation of the given bone. (But
    /// it can check other bones as well, e.g. child bones if they must have a certain orientation 
    /// in model space...)
    /// </para>
    /// </remarks>
    public Action<int> LimitBoneTransforms { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CcdIKSolver"/> class.
    /// </summary>
    public CcdIKSolver()
    {
      NumberOfIterations = 10;
      AllowedDeviation = 0.01f;
      BoneGain = 1f;
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
      if (NumberOfIterations <= 0)
        return;

      // Validate new chains.
      if (_isDirty && !SkeletonPose.IsAncestorOrSelf(RootBoneIndex, TipBoneIndex))
        throw new ArgumentException("The RootBoneIndex and the TipBoneIndex do not form a valid bone chain.");

      _isDirty = false;

      var skeleton = SkeletonPose.Skeleton;

      bool requiresBlending = RequiresBlending();
      float maxRotationAngle;
      bool requiresLimiting = RequiresLimiting(deltaTime, out maxRotationAngle);
      if (requiresBlending || requiresLimiting)
      {
        // Remember original bone transforms for interpolation with the result at the end.
        // Transforms are stored from tip to root (reverse order!).
        _originalTransforms.Clear();

        int boneIndex = TipBoneIndex;
        while (true)
        {
          _originalTransforms.Add(SkeletonPose.GetBoneTransform(boneIndex));

          if (boneIndex == RootBoneIndex)
            break;

          boneIndex = skeleton.GetParent(boneIndex);
        }
      }

      // We iterate NumberOfIteration times or until we are within the allowed deviation.
      // In each iteration we move each bone once.
      float toleranceSquared = AllowedDeviation * AllowedDeviation;
      bool targetReached = false;
      for (int i = 0; i < NumberOfIterations && !targetReached; i++)
      {
        // Iterate bones from tip to root.
        int boneIndex = TipBoneIndex;
        while (true)
        {
          // Get current tip position in local bone space.
          var bonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(boneIndex);
          var targetPositionLocal = bonePoseAbsolute.ToLocalPosition(Target);
          Vector3F tipLocal;
          if (boneIndex == TipBoneIndex)
          {
            tipLocal = TipOffset;
          }
          else
          {
            var tipBonePoseAbsolute = SkeletonPose.GetBonePoseAbsolute(TipBoneIndex);
            var tipAbsolute = tipBonePoseAbsolute.ToParentPosition(TipOffset);
            tipLocal = bonePoseAbsolute.ToLocalPosition(tipAbsolute);
          }

          if ((tipLocal - targetPositionLocal).LengthSquared < toleranceSquared)
          {
            // Target reached! If this is the first iteration and the first bone, then we
            // didn't have to do anything and can abort. Otherwise we just leave the loops.

            if (i == 0 && boneIndex == TipBoneIndex)
              return;

            targetReached = true;
            break;
          }

          // Rotate bone so that it points to the target.
          if (tipLocal.TryNormalize() && targetPositionLocal.TryNormalize())
          {
            var rotation = QuaternionF.CreateRotation(tipLocal, targetPositionLocal);
            var angle = rotation.Angle;

            // If the bone gain is less than 1, then we make a smaller correction. We will need
            // more iterations but the change is more evenly distributed over the chain.
            if (BoneGain < 1)
            {
              angle = angle * BoneGain;
              rotation.Angle = angle;
            }

            // Apply rotation to bone transform.
            if (Numeric.IsGreater(angle, 0))
            {
              var boneTransform = SkeletonPose.GetBoneTransform(boneIndex);
              boneTransform.Rotation = boneTransform.Rotation * rotation;

              SkeletonPose.SetBoneTransform(boneIndex, boneTransform);

              // Call delegate that enforces bone limits.
              if (LimitBoneTransforms != null)
                LimitBoneTransforms(boneIndex);
            }
          }

          if (boneIndex == RootBoneIndex)
            break;

          boneIndex = skeleton.GetParent(boneIndex);
        }
      }

      if (requiresBlending || requiresLimiting)
      {
        // Apply weight and the angular velocity limit.
        int boneIndex = TipBoneIndex;
        int i = 0;
        while (true)
        {
          var originalTransform = _originalTransforms[i];
          var targetTransform = SkeletonPose.GetBoneTransform(boneIndex);

          // Apply weight.
          if (requiresBlending)
            BlendBoneTransform(ref originalTransform, ref targetTransform);

          // Apply angular velocity limit.
          if (requiresLimiting)
            LimitBoneTransform(ref originalTransform, ref targetTransform, maxRotationAngle);

          SkeletonPose.SetBoneTransform(boneIndex, targetTransform);

          if (boneIndex == RootBoneIndex)
            break;

          boneIndex = skeleton.GetParent(boneIndex);
          i++;
        }
      }

      _originalTransforms.Clear();
    }
    #endregion
  }
}
