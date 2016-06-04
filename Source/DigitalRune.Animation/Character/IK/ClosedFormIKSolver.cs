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
  /// Modifies a skeleton using a non-iterative, closed-form IK solver.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This <see cref="IKSolver"/> uses non-iterative, closed-form IK solver algorithm to modify a
  /// bone chain to reach the <see cref="IKSolver.Target"/> position. <see cref="RootBoneIndex"/>
  /// determines the first bone in the chain. <see cref="TipBoneIndex"/> determines the last bone
  /// that is included in the chain. This IK solver rotates all bones in the chain, so that the tip
  /// of the bone chain reaches the <see cref="IKSolver.Target"/> position.
  /// </para>
  /// <para>
  /// This IK solver does not support bone rotation limits.
  /// </para>
  /// <para>
  /// See also <see cref="IKSolver"/> for more general information. 
  /// </para>
  /// </remarks>
  public class ClosedFormIKSolver : IKSolver
  {
    // Non-iterative, closed-form IK solver, see Game Programming Gems 8
    // The article has also a variant that works with changing bone lengths and offsets
    // (for skeletons with slider joints).

    // TODO: Optimize: Do not cache the values for all bones. Only for the needed bones.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached values for the bone chain. Bones are sorted from root to tip.
    private readonly List<float> _boneLengths = new List<float>();
    private readonly List<int> _boneIndices = new List<int>();
    private float _totalChainLength = -1;

    // The absolute bone transforms.
    private readonly List<SrtTransform> _bones = new List<SrtTransform>();

    // A list that is re-used in each OnSolve(). It does not store data between calls.
    private readonly List<SrtTransform> _originalBoneTransforms = new List<SrtTransform>();
    
    private bool _isDirty = true;
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
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the <see cref="SkeletonPose"/> was exchanged.
    /// </summary>
    protected override void OnInvalidate()
    {
      _isDirty = false;
      base.OnInvalidate();
    }


    private void UpdateDerivedValues()
    {
      // Validate chain. Compute bone lengths, bone indices and total chain length.
      if (_isDirty)
        return;

      _boneLengths.Clear();
      _boneIndices.Clear();
      _totalChainLength = -1;

      try
      {
        // Check if chain is valid.
        if (!SkeletonPose.IsAncestorOrSelf(RootBoneIndex, TipBoneIndex))
          throw new ArgumentException("The RootBoneIndex and the TipBoneIndex do not form a valid bone chain.");

        // Get bone indices.
        SkeletonPose.GetChain(RootBoneIndex, TipBoneIndex, _boneIndices);

        var numberOfBones = _boneIndices.Count;

        // Get bone lengths. We compute the bone lengths from an actual bone pose because
        // our archer test model had a scale in the bone transforms. Therefore we cannot
        // compute the length from only the bind poses.
        _boneLengths.Capacity = numberOfBones;
        _totalChainLength = 0;
        for (int i = 0; i < numberOfBones - 1; i++)
        {
          int boneIndex = _boneIndices[i];
          int childIndex = _boneIndices[i + 1];
          var boneVector = SkeletonPose.GetBonePoseAbsolute(childIndex).Translation - SkeletonPose.GetBonePoseAbsolute(boneIndex).Translation;
          float boneLength = boneVector.Length;
          _boneLengths.Add(boneLength);
          _totalChainLength += boneLength;
        }

        // Tip bone.
        _boneLengths.Add((SkeletonPose.GetBonePoseAbsolute(TipBoneIndex).Scale * TipOffset).Length);
        _totalChainLength += _boneLengths[numberOfBones - 1];

        // Initialize _bones list with dummy values.
        _bones.Clear();
        for (int i = 0; i < numberOfBones; i++)
          _bones.Add(SrtTransform.Identity);

        _isDirty = true;
      }
      catch
      {
        _boneLengths.Clear();
        _boneIndices.Clear();
        _totalChainLength = 0;

        throw;
      }
    }


    /// <summary>
    /// Called when <see cref="IKSolver.Solve"/> is called.
    /// </summary>
    /// <param name="deltaTime">The current time step (in seconds).</param>
    protected override void OnSolve(float deltaTime)
    {
      UpdateDerivedValues();

      if (_totalChainLength == 0)
        return;

      var skeleton = SkeletonPose.Skeleton;

      // Make a local copy of the absolute bone poses. The following operations will be performed
      // on the data in _bone and not on the skeleton pose.
      var numberOfBones = _boneIndices.Count;
      for (int i = 0; i < numberOfBones; i++)
        _bones[i] = SkeletonPose.GetBonePoseAbsolute(_boneIndices[i]);

      // Calculate the position at the tip of the last bone. 
      // If TipOffset is not 0, then we can rotate the last bone.
      // If TipOffset is 0, then the last bone defines the tip but is not rotated.
      // --> numberOfBones is set to the number of affected bones.
      Vector3F tipAbsolute;
      if (TipOffset.IsNumericallyZero)
      {
        numberOfBones--;
        tipAbsolute = SkeletonPose.GetBonePoseAbsolute(TipBoneIndex).Translation;
      }
      else
      {
        tipAbsolute = SkeletonPose.GetBonePoseAbsolute(TipBoneIndex).ToParentPosition(TipOffset);
      }

      // The root bone rotation that aligns the whole chain with the target.
      QuaternionF chainRotation = QuaternionF.Identity;
      Vector3F boneToTarget, boneToTip;
      float remainingChainLength = _totalChainLength;

      // Apply the soft limit to the distance to the IK goal
      //vecToIkGoal = Target - _bones[0].Translation;
      //distToIkGoal = vecToIkGoal.Length;
      //// Limit the extension to 98% and ramp it up over 5% of the chains length
      //vecToIkGoal *= (LimitValue(distToIkGoal, _totalChainLength * 0.98f, _totalChainLength * 0.08f)) / distToIkGoal;
      //Vector3F goalPosition = _bones[0].Translation + vecToIkGoal;

      var targetAbsolute = Target;

      // This algorithms iterates once over all bones from root to tip.
      for (int i = 0; i < numberOfBones; i++)
      {
        if (i > 0)
        {
          // Transform the bone position by the overall chain offset.
          var translation = _bones[0].Translation 
                            + chainRotation.Rotate(_bones[i].Translation - _bones[0].Translation);
          _bones[i] = new SrtTransform(_bones[i].Scale, _bones[i].Rotation, translation);
        }

        // The bone to tip vector of the aligned chain (without other IK rotations!).
        boneToTip = tipAbsolute - _bones[i].Translation;
        float boneToTipLength = boneToTip.Length;
        boneToTip /= boneToTipLength; // TODO: Check for division by 0?

        if (i > 0)
        {
          // Calculate the new absolute bone position.
          var translation = _bones[i - 1].ToParentPosition(skeleton.GetBindPoseRelative(_boneIndices[i]).Translation);
          _bones[i] = new SrtTransform(_bones[i].Scale, _bones[i].Rotation, translation);
        }

        // The bone to target vector of the new chain configuration.
        boneToTarget = targetAbsolute - _bones[i].Translation;
        float boneToTargetLength = boneToTarget.Length;
        boneToTarget /= boneToTargetLength;

        if (i == 0)
        {
          // This is the first bone: Compute rotation that aligns the whole initial chain with
          // the target.
          chainRotation = QuaternionF.CreateRotation(boneToTip, boneToTarget);

          // Update tip.
          tipAbsolute = _bones[i].Translation + (boneToTarget * boneToTipLength);

          // Apply chainRotation to root bone.
          _bones[i] = new SrtTransform(_bones[i].Scale, chainRotation * _bones[i].Rotation, _bones[i].Translation);
        }
        else
        {
          // Apply the chain alignment rotation. Also the parent bones have changed, so we apply
          // an additional rotation that accounts for the ancestor rotations. This additional
          // rotation aligns the last bone with the target.
          // TODO: Find an explanation/derivation of this additional rotation.
          _bones[i] = new SrtTransform(
            _bones[i].Scale, 
            QuaternionF.CreateRotation(boneToTip, boneToTarget) * chainRotation * _bones[i].Rotation, 
            _bones[i].Translation);
        }

        // Now, solve the bone using trigonometry. 
        // The last bone was already aligned with the target. For the second last bone we use
        // the law of cosines. For all other bones we use the complicated steps described in the
        // GPG article.
        if (i <= numberOfBones - 2)
        {
          // Length of chain after this bone.
          remainingChainLength -= _boneLengths[i];

          // The direction of the current bone. For the tip bone we use the TipOffset.
          Vector3F boneDirection;
          if (i != TipBoneIndex)
            boneDirection = _bones[i].Rotation.Rotate(skeleton.GetBindPoseRelative(_boneIndices[i + 1]).Translation);
          else
            boneDirection = _bones[i].Rotation.Rotate(TipOffset);

          if (!boneDirection.TryNormalize())
            continue;

          // The bone rotates around an axis normal to the bone to target direction and the bone
          // vector.
          Vector3F rotationAxis = Vector3F.Cross(boneToTarget, boneDirection);
          if (!rotationAxis.TryNormalize())
            continue;       // TODO: If this happens, can we choose a useful direction?

          // The current angle between bone direction and bone to target vector.
          float currentAngle = (float)Math.Acos(MathHelper.Clamp(Vector3F.Dot(boneDirection, boneToTarget), -1, 1));

          // Side lengths of the involved triangles.
          var a = _boneLengths[i];
          var b = boneToTargetLength;
          var c = remainingChainLength;
          var d = boneToTipLength;

          float desiredAngle;
          if (i == numberOfBones - 2)
          {
            // Use trigonometry (law of cosines) to determine the desired angle.
            desiredAngle = (float)Math.Acos(MathHelper.Clamp((a * a + b * b - c * c) / (2 * a * b), -1.0f, 1.0f));
          }
          else
          {
            // The maximal angle that this bone can have where the chain still reaches the tip.
            float maxTipAngle;
            if (boneToTipLength > remainingChainLength)
            {
              maxTipAngle = (float)Math.Acos(MathHelper.Clamp((a * a + d * d - c * c) / (2 * a * d), -1.0f, 1.0f));
            }
            else
            {
              // Tip is very near and this bone can bend more than 180°. Add additional chain length
              // in radians.
              maxTipAngle = (float)Math.Acos(MathHelper.Clamp((a * 0.5f) / remainingChainLength, 0.0f, 1.0f));
              maxTipAngle += ((c - d) / a);
            }

            // The maximal angle that this bone can have where the chain still reaches the target.
            float maxTargetAngle;
            if (boneToTargetLength > remainingChainLength)
            {              
              maxTargetAngle = (float)Math.Acos(MathHelper.Clamp((a * a + b * b - c * c) / (2 * a * b), -1.0f, 1.0f));
            }
            else
            {
              // Target is very near and this bone can bend more than 180°. Add additional chain 
              // length in radians.
              maxTargetAngle = (float)Math.Acos(MathHelper.Clamp((a * 0.5f) / remainingChainLength, 0.0f, 1.0f));
              maxTargetAngle += ((c - b) / a);
            }

            // If we set the desired angle to maxTargetAngle, the remain bones must be all 
            // stretched. We want to keep the chain appearance, therefore, we set a smaller angle.
            // The new angle relative to the final remaining chain should have the same ratio as the 
            // current angle to the current remaining chain.
            if (!Numeric.IsZero(maxTipAngle))
              desiredAngle = maxTargetAngle * (currentAngle / maxTipAngle);
            else
              desiredAngle = maxTargetAngle;   // Avoiding divide by zero.
          }
          
          // The rotation angle that we have to apply.
          float deltaAngle = desiredAngle - currentAngle;

          // Apply the rotation to the current bones 
          _bones[i] = new SrtTransform(
            _bones[i].Scale, 
            (QuaternionF.CreateRotation(rotationAxis, deltaAngle) * _bones[i].Rotation).Normalized, 
            _bones[i].Translation);
        }
      }

      bool requiresBlending = RequiresBlending();
      float maxRotationAngle;
      bool requiresLimiting = RequiresLimiting(deltaTime, out maxRotationAngle);
      if (requiresBlending || requiresLimiting)
      {
        // We have to blend the computed results with the original bone transforms.

        // Get original bone transforms.
        for (int i = 0; i < numberOfBones; i++)
          _originalBoneTransforms.Add(SkeletonPose.GetBoneTransform(_boneIndices[i]));

        for (int i = 0; i < numberOfBones; i++)
        {
          int boneIndex = _boneIndices[i];

          var originalBoneTransform = _originalBoneTransforms[i];

          // Set absolute bone pose and let the skeleton compute the bone transform for us.
          SkeletonPose.SetBoneRotationAbsolute(boneIndex, _bones[i].Rotation);
          var targetBoneTransform = SkeletonPose.GetBoneTransform(boneIndex);

          // Apply weight.
          if (requiresBlending)
            BlendBoneTransform(ref originalBoneTransform, ref targetBoneTransform);

          // Apply angular velocity limit.
          if (requiresLimiting)
            LimitBoneTransform(ref originalBoneTransform, ref targetBoneTransform, maxRotationAngle);

          // Set final bone transform.
          SkeletonPose.SetBoneTransform(boneIndex, targetBoneTransform);
        }

        _originalBoneTransforms.Clear();
      }
      else
      {
        // Weight is 1 and angular velocity limit is not active. 
        // --> Just copy the compute rotations.

        for (int i = 0; i < numberOfBones; i++)
        {
          int boneIndex = _boneIndices[i];
          SkeletonPose.SetBoneRotationAbsolute(boneIndex, _bones[i].Rotation);
        }
      }
    }


    ///// <summary>
    ///// Limits the value to be less than the limit. The value is softened between 
    ///// limit - softening and limit.
    ///// </summary>
    ///// <param name="value">The value.</param>
    ///// <param name="limit">The limit.</param>
    ///// <param name="softening">The softening zone.</param>
    ///// <returns></returns>
    //private float LimitValue(float value, float limit, float softening)
    //{
    //  float tan225 = (float)Math.Tan(ConstantsF.PiOver4 / 2);  // tan(22.5°)
    //  float sin45 = (float)Math.Sin(ConstantsF.PiOver4);       // sin(45°)

    //  softening = softening / (sin45 * tan225);

    //  if (value > (limit + (softening * tan225)))
    //    return limit;
    //  if (value < (limit - (softening * (sin45 - tan225))))
    //    return value;

    //  return (float)(limit + (softening * (Math.Cos(Math.Asin(tan225 + ((limit - value) / softening)))) - softening));
    //}
    #endregion
  }
}
