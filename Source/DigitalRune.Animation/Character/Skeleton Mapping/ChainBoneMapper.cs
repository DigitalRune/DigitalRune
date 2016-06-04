// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Maps the orientation of a whole bone chain.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="ChainBoneMapper"/> can be used if the mapped skeletons have a different number of
  /// bones. A bone chain starts at a bone (<see cref="RootBoneIndexA"/> and 
  /// <see cref="RootBoneIndexB"/>). This bone defines the root (or origin) of the chain. And the
  /// chain has an end bone (<see cref="TipBoneIndexA"/> and <see cref="TipBoneIndexB"/>). The end
  /// bone is not included in the chain. It defines the tip of the chain. 
  /// </para>
  /// <para>
  /// The <see cref="ChainBoneMapper"/> computes a direction vector from chain origin to chain tip
  /// for each chain. Then it will apply a bone rotation to the root bone, so that both chain
  /// direction vectors are parallel. - Only the bone rotation of the root bone of the target
  /// skeleton is modified. Other bones in the chain are not modified.
  /// </para>
  /// <para>
  /// Note: The <see cref="ChainBoneMapper"/> can also be used if the chain contains only a single
  /// bone.
  /// </para>
  /// <para>
  /// For the target skeleton, the <see cref="ChainBoneMapper"/> chooses a reference position which
  /// is either the bind pose or a direct-mapped pose (<see cref="DirectBoneMapper"/>).
  /// <see cref="MapFromBindPose"/> determines which reference pose should be used. The chosen
  /// reference pose influences the twist of the whole bone chain. 
  /// </para>
  /// </remarks>
  public class ChainBoneMapper : BoneMapper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // true if cached data is invalid.
    private bool _isDirty = true;

    // A nested direct bone mapper that is used if MapFromBindPose is false.
    private DirectBoneMapper _directBoneMapper;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bone index of the first bone of the bone chain in the first skeleton.
    /// </summary>
    /// <value>The root bone index for the first skeleton.</value>
    public int RootBoneIndexA
    {
      get { return _rootBoneIndexA; }
      set
      {
        if (_rootBoneIndexA != value)
        {
          _rootBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _rootBoneIndexA;


    /// <summary>
    /// Gets or sets the bone index where the bone chain of the first skeleton ends. This is the
    /// bone index of the first bone that is NOT included in the chain.
    /// </summary>
    /// <value>
    /// The bone index where the bone chain of the first skeleton ends. (= The first bone AFTER the
    /// chain.)
    /// </value>
    public int TipBoneIndexA
    {
      get { return _tipBoneIndexA; }
      set
      {
        if (_tipBoneIndexA != value)
        {
          _tipBoneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _tipBoneIndexA;


    /// <summary>
    /// Gets or sets the bone index of the first bone of the bone chain in the second skeleton.
    /// </summary>
    /// <value>The root bone index for the second skeleton.</value>
    public int RootBoneIndexB
    {
      get { return _rootBoneIndexB; }
      set
      {
        if (_rootBoneIndexB != value)
        {
          _rootBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _rootBoneIndexB;


    /// <summary>
    /// Gets or sets the bone index where the bone chain of the second skeleton ends. This is the
    /// bone index of the first bone that is NOT included in the chain.
    /// </summary>
    /// <value>
    /// The bone index where the bone chain of the second skeleton ends. (= The first bone AFTER the
    /// chain.)
    /// </value>
    public int TipBoneIndexB
    {
      get { return _tipBoneIndexB; }
      set
      {
        if (_tipBoneIndexB != value)
        {
          _tipBoneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _tipBoneIndexB;


    /// <summary>
    /// Gets or sets a value indicating whether the chain mapping uses the bind pose as the
    /// reference orientation for the target skeleton.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the bind pose is used as the reference orientation; otherwise, 
    /// <see langword="false"/> a direct-mapped bone pose is used as reference orientation. The
    /// default is <see langword="true"/>.
    /// </value>
    public bool MapFromBindPose
    {
      get { return _mapFromBindPose; }
      set
      {
        if (_mapFromBindPose != value)
        {
          _mapFromBindPose = value;
          Invalidate();
        }
      }
    }
    private bool _mapFromBindPose;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainBoneMapper"/> class.
    /// </summary>
    /// <param name="rootBoneIndexA">
    /// The bone index in the first skeleton where the bone chain starts.
    /// </param>
    /// <param name="tipBoneIndexA">
    /// The bone index in the first skeleton where the bone chain ends. This is the index 
    /// of the first bone that is NOT included in the chain (= the first bone after the chain).
    /// </param>
    /// <param name="rootBoneIndexB">
    /// The bone index in the second skeleton where the bone chain starts.
    /// </param>
    /// <param name="tipBoneIndexB">
    /// The bone index in the second skeleton where the bone chain ends. This is the index 
    /// of the first bone that is NOT included in the chain (= the first bone after the chain).
    /// </param>
    public ChainBoneMapper(int rootBoneIndexA, int tipBoneIndexA, int rootBoneIndexB, int tipBoneIndexB)
    {
      RootBoneIndexA = rootBoneIndexA;
      TipBoneIndexA = tipBoneIndexA;
      RootBoneIndexB = rootBoneIndexB;
      TipBoneIndexB = tipBoneIndexB;

      MapFromBindPose = true;
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
      // Abort if cached data is still valid.
      if (!_isDirty)
        return;

      _isDirty = false;

      // Check bone indices.
      if (RootBoneIndexA < 0 || RootBoneIndexA >= SkeletonMapper.SkeletonPoseA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("RootBoneIndexA is out of range.");
      if (TipBoneIndexA < 0 || TipBoneIndexA >= SkeletonMapper.SkeletonPoseA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("TipBoneIndexA is out of range.");

      if (RootBoneIndexB < 0 || RootBoneIndexB >= SkeletonMapper.SkeletonPoseB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("RootBoneIndexB is out of range.");
      if (TipBoneIndexB < 0 || TipBoneIndexB >= SkeletonMapper.SkeletonPoseB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("TipBoneIndexB is out of range.");

      if (!SkeletonMapper.SkeletonPoseA.IsAncestorOrSelf(RootBoneIndexA, TipBoneIndexA))
        throw new AnimationException("RootBoneIndexA and TipBoneIndexA do not form a valid bone chain.");
      if (!SkeletonMapper.SkeletonPoseB.IsAncestorOrSelf(RootBoneIndexB, TipBoneIndexB))
        throw new AnimationException("RootBoneIndexB and TipBoneIndexB do not form a valid bone chain.");

      if (!MapFromBindPose)
      {
        // Create a nested direct bone mapper.
        _directBoneMapper = new DirectBoneMapper(RootBoneIndexA, RootBoneIndexB)
        {
          MapTranslations = false,

          // Since the bone mapper is not added to a skeleton mapper, we must set 
          // SkeletonMapper manually.
          SkeletonMapper = SkeletonMapper, 
        };
      }
      else
      {
        _directBoneMapper = null;
      }
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapAToB"/> was called.
    /// </summary>
    protected override void OnMapAToB()
    {
      CacheDerivedData();

      var skeletonInstanceA = SkeletonMapper.SkeletonPoseA;
      var skeletonInstanceB = SkeletonMapper.SkeletonPoseB;

      // ----- Set start transform.
      // Note: It is important to start from an absolute bone pose that does not depend
      // on the last frame. Otherwise, errors accumulate and the bone chain starts to twist.
      if (MapFromBindPose)
      {
        // Reset the bone rotation of the root bone to start from the bind pose.
        skeletonInstanceB.ResetBoneTransforms(RootBoneIndexB, RootBoneIndexB, false, true, false);
      }
      else
      {
        // Start from a direct-mapped bone pose. 
        _directBoneMapper.MapAToB();
      }

      // Get absolute bone poses.
      var rootBoneA = skeletonInstanceA.GetBonePoseAbsolute(RootBoneIndexA);
      var tipBoneA = skeletonInstanceA.GetBonePoseAbsolute(TipBoneIndexA);
      var rootBoneB = skeletonInstanceB.GetBonePoseAbsolute(RootBoneIndexB);
      var tipBoneB = skeletonInstanceB.GetBonePoseAbsolute(TipBoneIndexB);

      // Compute direction vector for the two chains (origin to tip).
      var directionB = tipBoneB.Translation - rootBoneB.Translation;
      var directionA = tipBoneA.Translation - rootBoneA.Translation;

      // Abort if any chain has zero length.
      if (directionB.IsNumericallyZero || directionA.IsNumericallyZero)
        return;

      // Apply global skeleton rotation offset to rotate all into model B space.
      directionA = SkeletonMapper.RotationOffset.Rotate(directionA);

      // Compute and apply rotation between the two direction vectors.
      var rotation = QuaternionF.CreateRotation(directionB, directionA);
      skeletonInstanceB.RotateBoneAbsolute(RootBoneIndexB, rotation);
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapBToA"/> was called.
    /// </summary>
    protected override void OnMapBToA()
    {
      CacheDerivedData();

      var skeletonInstanceA = SkeletonMapper.SkeletonPoseA;
      var skeletonInstanceB = SkeletonMapper.SkeletonPoseB;

      if (MapFromBindPose)
      {
        // Reset the bone rotation of the root bone to start from the bind pose.
        skeletonInstanceA.ResetBoneTransforms(RootBoneIndexA, RootBoneIndexA, false, true, false);
      }
      else
      {
        _directBoneMapper.MapBToA();
      }

      var rootBoneA = skeletonInstanceA.GetBonePoseAbsolute(RootBoneIndexA);
      var tipBoneA = skeletonInstanceA.GetBonePoseAbsolute(TipBoneIndexA);
      var rootBoneB = skeletonInstanceB.GetBonePoseAbsolute(RootBoneIndexB);
      var tipBoneB = skeletonInstanceB.GetBonePoseAbsolute(TipBoneIndexB);

      var directionB = tipBoneB.Translation - rootBoneB.Translation;
      var directionA = tipBoneA.Translation - rootBoneA.Translation;

      if (directionB.IsNumericallyZero || directionA.IsNumericallyZero)
        return;

      directionB = SkeletonMapper.RotationOffset.Conjugated.Rotate(directionB);

      var rotation = QuaternionF.CreateRotation(directionA, directionB);
      skeletonInstanceA.RotateBoneAbsolute(RootBoneIndexA, rotation);
    }
    #endregion
  }
}
