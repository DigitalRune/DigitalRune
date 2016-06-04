// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Maps the orientation of a spine bone in the upper back of a character.
  /// </summary>
  /// <remarks>
  /// The <see cref="UpperBackBoneMapper"/> is a specialized <see cref="ChainBoneMapper"/>. It can
  /// be used for a bone in the spine that is connected to neck bone and two shoulder bones. It
  /// modifies the rotation of the spine bone so that the overall rotation of the upper back is
  /// conserved. 
  /// </remarks>
  public class UpperBackBoneMapper : BoneMapper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty = true;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the spine bone index for the first skeleton.
    /// </summary>
    /// <value>The spine bone index for the first skeleton.</value>
    public int SpineBoneIndexA
    {
      get { return _spineBoneIndexA; }
      set
      {
        if (_spineBoneIndexA != value)
        {
          _spineBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _spineBoneIndexA;


    /// <summary>
    /// Gets or sets the neck bone index for the first skeleton.
    /// </summary>
    /// <value>The neck bone index for the first skeleton.</value>
    public int NeckBoneIndexA
    {
      get { return _neckBoneIndexA; }
      set
      {
        if (_neckBoneIndexA != value)
        {
          _neckBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _neckBoneIndexA;


    /// <summary>
    /// Gets or sets the left shoulder bone index for the first skeleton.
    /// </summary>
    /// <value>The left shoulder bone index for the first skeleton.</value>
    public int LeftShoulderBoneIndexA
    {
      get { return _leftShoulderBoneIndexA; }
      set
      {
        if (_leftShoulderBoneIndexA != value)
        {
          _leftShoulderBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _leftShoulderBoneIndexA;


    /// <summary>
    /// Gets or sets the right shoulder bone index for the first skeleton.
    /// </summary>
    /// <value>The right shoulder bone index for the first skeleton.</value>
    public int RightShoulderBoneIndexA
    {
      get { return _rightShoulderBoneIndexA; }
      set
      {
        if (_rightShoulderBoneIndexA != value)
        {
          _rightShoulderBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _rightShoulderBoneIndexA;


    /// <summary>
    /// Gets or sets the spine bone index for the second skeleton.
    /// </summary>
    /// <value>The spine bone index for the second skeleton.</value>
    public int SpineBoneIndexB
    {
      get { return _spineBoneIndexB; }
      set
      {
        if (_spineBoneIndexB != value)
        {
          _spineBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _spineBoneIndexB;


    /// <summary>
    /// Gets or sets the neck bone index for the second skeleton.
    /// </summary>
    /// <value>The neck bone index for the second skeleton.</value>
    public int NeckBoneIndexB
    {
      get { return _neckBoneIndexB; }
      set
      {
        if (_neckBoneIndexB != value)
        {
          _neckBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _neckBoneIndexB;


    /// <summary>
    /// Gets or sets the left shoulder bone index for the second skeleton.
    /// </summary>
    /// <value>The left shoulder bone index for the second skeleton.</value>
    public int LeftShoulderBoneIndexB
    {
      get { return _leftShoulderBoneIndexB; }
      set
      {
        if (_leftShoulderBoneIndexB != value)
        {
          _leftShoulderBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _leftShoulderBoneIndexB;


    /// <summary>
    /// Gets or sets the right shoulder bone index for the second skeleton.
    /// </summary>
    /// <value>The right shoulder bone index for the second skeleton.</value>
    public int RightShoulderBoneIndexB
    {
      get { return _rightShoulderBoneIndexB; }
      set
      {
        if (_rightShoulderBoneIndexB != value)
        {
          _rightShoulderBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _rightShoulderBoneIndexB;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="UpperBackBoneMapper"/> class.
    /// </summary>
    /// <param name="spineBoneIndexA">The spine bone index in the first skeleton.</param>
    /// <param name="neckBoneIndexA">The neck bone index in the first skeleton.</param>
    /// <param name="leftShoulderBoneIndexA">The left shoulder bone index in the first skeleton.</param>
    /// <param name="rightShoulderBoneIndexA">The right shoulder bone index in the first skeleton.</param>
    /// <param name="spineBoneIndexB">The spine bone in the second skeleton.</param>
    /// <param name="neckBoneIndexB">The neck bone index in the second skeleton.</param>
    /// <param name="leftShoulderBoneIndexB">The left shoulder bone index in the second skeleton.</param>
    /// <param name="rightShoulderBoneIndexB">The right shoulder bone index in the second skeleton.</param>
    public UpperBackBoneMapper(int spineBoneIndexA, int neckBoneIndexA, int leftShoulderBoneIndexA, int rightShoulderBoneIndexA, int spineBoneIndexB, int neckBoneIndexB, int leftShoulderBoneIndexB, int rightShoulderBoneIndexB)
    {
      SpineBoneIndexA = spineBoneIndexA;
      NeckBoneIndexA = neckBoneIndexA;
      LeftShoulderBoneIndexA = leftShoulderBoneIndexA;
      RightShoulderBoneIndexA = rightShoulderBoneIndexA;
      SpineBoneIndexB = spineBoneIndexB;
      NeckBoneIndexB = neckBoneIndexB;
      LeftShoulderBoneIndexB = leftShoulderBoneIndexB;
      RightShoulderBoneIndexB = rightShoulderBoneIndexB;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when <see cref="BoneMapper.Invalidate"/> is called.
    /// </summary>
    protected override void OnInvalidate()
    {
      _isDirty = true;
    }


    private void CacheDerivedData()
    {
      // Nothing to cache here, but we use this method to check bone indices.

      if (!_isDirty)
        return;

      _isDirty = false;

      // Check bone indices.
      var skeletonInstanceA = SkeletonMapper.SkeletonPoseA;
      var skeletonInstanceB = SkeletonMapper.SkeletonPoseB;

      // Check bone indices.
      if (SpineBoneIndexA < 0 || SpineBoneIndexA >= skeletonInstanceA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("SpineBoneIndexA is out of range.");
      if (NeckBoneIndexA < 0 || NeckBoneIndexA >= skeletonInstanceA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("NeckBoneIndexA is out of range.");
      if (LeftShoulderBoneIndexA < 0 || LeftShoulderBoneIndexA >= skeletonInstanceA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("LeftShoulderBoneIndexA is out of range.");
      if (RightShoulderBoneIndexA < 0 || RightShoulderBoneIndexA >= skeletonInstanceA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("RightShoulderBoneIndexA is out of range.");

      if (SpineBoneIndexB < 0 || SpineBoneIndexB >= skeletonInstanceB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("SpineBoneIndexB is out of range.");
      if (NeckBoneIndexB < 0 || NeckBoneIndexB >= skeletonInstanceB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("NeckBoneIndexB is out of range.");
      if (LeftShoulderBoneIndexB < 0 || LeftShoulderBoneIndexB >= skeletonInstanceB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("LeftShoulderBoneIndexB is out of range.");
      if (RightShoulderBoneIndexB < 0 || RightShoulderBoneIndexB >= skeletonInstanceB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("RightShoulderBoneIndexB is out of range.");
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapAToB"/> was called.
    /// </summary>
    protected override void OnMapAToB()
    {
      CacheDerivedData();

      DoWork(SkeletonMapper.RotationOffset, SkeletonMapper.SkeletonPoseA, SkeletonMapper.SkeletonPoseB, 
             SpineBoneIndexA, NeckBoneIndexA, LeftShoulderBoneIndexA, RightShoulderBoneIndexA, 
             SpineBoneIndexB, NeckBoneIndexB, LeftShoulderBoneIndexB, RightShoulderBoneIndexB);
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapBToA"/> was called.
    /// </summary>
    protected override void OnMapBToA()
    {
      CacheDerivedData();

      DoWork(SkeletonMapper.RotationOffset.Conjugated, SkeletonMapper.SkeletonPoseB, SkeletonMapper.SkeletonPoseA,
             SpineBoneIndexB, NeckBoneIndexB, LeftShoulderBoneIndexB, RightShoulderBoneIndexB,
             SpineBoneIndexA, NeckBoneIndexA, LeftShoulderBoneIndexA, RightShoulderBoneIndexA);
    }


    private static void DoWork(QuaternionF skeletonOffset, SkeletonPose skeletonA, SkeletonPose skeletonB, int boneIndexA, int neckBoneIndexA, int leftShoulderBoneIndexA, int rightShoulderBoneIndexA, int boneIndexB, int neckBoneIndexB, int leftShoulderBoneIndexB, int rightShoulderBoneIndexB)
    {
      // Reset root bone.
      skeletonB.ResetBoneTransforms(boneIndexB, boneIndexB, false, true, false);

      // Get absolute positions all bones.
      var boneA = skeletonA.GetBonePoseAbsolute(boneIndexA).Translation;
      var neckA = skeletonA.GetBonePoseAbsolute(neckBoneIndexA).Translation;
      var leftShoulderA = skeletonA.GetBonePoseAbsolute(leftShoulderBoneIndexA).Translation;
      var rightShoulderA = skeletonA.GetBonePoseAbsolute(rightShoulderBoneIndexA).Translation;
      var boneB = skeletonB.GetBonePoseAbsolute(boneIndexB).Translation;
      var neckB = skeletonB.GetBonePoseAbsolute(neckBoneIndexB).Translation;
      var leftShoulderB = skeletonB.GetBonePoseAbsolute(leftShoulderBoneIndexB).Translation;
      var rightShoulderB = skeletonB.GetBonePoseAbsolute(rightShoulderBoneIndexB).Translation;

      // Abort if any bone to bone distance is 0.
      if (Vector3F.AreNumericallyEqual(boneA, neckA)
          || Vector3F.AreNumericallyEqual(boneA, rightShoulderA)
          || Vector3F.AreNumericallyEqual(leftShoulderA, rightShoulderA)
          || Vector3F.AreNumericallyEqual(boneB, neckB)
          || Vector3F.AreNumericallyEqual(boneB, rightShoulderB)
          || Vector3F.AreNumericallyEqual(leftShoulderB, rightShoulderB))
      {
        return;
      }

      // Get shoulder axis vectors in model B space.
      var shoulderAxisA = rightShoulderA - leftShoulderA;
      shoulderAxisA = skeletonOffset.Rotate(shoulderAxisA);
      var shoulderAxisB = rightShoulderB - leftShoulderB;

      // Create a twist rotation from the shoulder vectors.
      var shoulderRotation = QuaternionF.CreateRotation(shoulderAxisB, shoulderAxisA);

      // Apply this twist to the spine. (Modifies the neckB position.)
      neckB = boneB + shoulderRotation.Rotate(neckB - boneB);

      // Get spine vectors in model B space.
      var spineAxisA = neckA - boneA;
      spineAxisA = skeletonOffset.Rotate(spineAxisA);
      var spineAxisB = neckB - boneB;

      // Create swing rotation from spine vectors.
      var spineRotation = QuaternionF.CreateRotation(spineAxisB, spineAxisA);

      // Apply the shoulder twist rotation followed by the spine swing rotation.
      skeletonB.RotateBoneAbsolute(boneIndexB, spineRotation * shoulderRotation);
    }
    #endregion
  }
}
