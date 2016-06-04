// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Controls the bone transforms of a <see cref="SkeletonPose"/> to resemble the posture of
  /// another <see cref="SkeletonPose"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Skeleton mapping is mainly used for two reasons: 
  /// <list type="bullet">
  /// <item>
  /// <i>Ragdoll Mapping:</i> A high detail skeleton (e.g. 60 bones) is mapped to a low detail
  /// skeleton (e.g. 15 bones). The high detail skeleton controls a visual character model. The low
  /// detail skeleton creates a ragdoll.
  /// </item>
  /// <item>
  /// <i>Motion Retargeting: </i> The animations of one character should be applied to another 
  /// character that uses a different skeleton.
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// To use a skeleton mapper, set the two skeleton poses (<see cref="SkeletonPoseA"/> and
  /// <see cref="SkeletonPoseB"/>). If the skeletons use different model space (e.g. different
  /// forward or up directions), <see cref="RotationOffset"/> must be set. Then 
  /// <see cref="BoneMapper"/>s must be added to the <see cref="BoneMappers"/> collection. A bone is
  /// only mapped if a <see cref="BoneMapper"/> for this bone is added to the skeleton mapper.
  /// </para>
  /// <para>
  /// In most cases, the order of the bone mappers in the <see cref="BoneMappers"/> collection is
  /// relevant. It is recommended to map parent bones before their child bones.
  /// </para>
  /// <para>
  /// After the bone mappers are defined, <see cref="MapAToB"/> or <see cref="MapBToA"/> can be
  /// called to map the bone transforms of one skeleton to the other skeleton.
  /// </para>
  /// </remarks>
  public class SkeletonMapper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first skeleton pose.
    /// </summary>
    /// <value>The first skeleton pose. Can be <see langword="null"/>.</value>
    public SkeletonPose SkeletonPoseA
    {
      get { return _skeletonPoseA; }
      set
      {
        if (_skeletonPoseA != value)
        {
          _skeletonPoseA = value;
          
          Invalidate();
        }
      }
    }
    private SkeletonPose _skeletonPoseA;


    /// <summary>
    /// Gets or sets the second skeleton pose.
    /// </summary>
    /// <value>The second skeleton pose. Can be <see langword="null"/>.</value>
    public SkeletonPose SkeletonPoseB
    {
      get { return _skeletonPoseB; }
      set
      {
        if (_skeletonPoseB != value)
        {
          _skeletonPoseB = value;

          Invalidate();
        }
      }
    }
    private SkeletonPose _skeletonPoseB;


    /// <summary>
    /// Gets the bone mappers.
    /// </summary>
    /// <value>The bone mappers. The default is an empty collection.</value>
    public BoneMapperCollection BoneMappers { get; private set; }


    /// <summary>
    /// Gets or sets the rotation offset between <see cref="SkeletonPoseA"/> and
    /// <see cref="SkeletonPoseB"/>. (This a rotation that transforms rotations from model A space
    /// to model B space.)
    /// </summary>
    /// <value>
    /// The rotation offset. The default value is <see cref="QuaternionF.Identity"/>.
    /// </value>
    /// <remarks>
    /// The rotation offset rotates the first skeleton into the direction of the second skeleton.
    /// The rotation offset must be used if the skeletons use different model spaces. For example,
    /// if one skeleton uses Y as the local "up" axis and the other skeleton uses Z as the local
    /// "up" axis.
    /// </remarks>
    public QuaternionF RotationOffset
    {
      get { return _rotationOffset; }
      set
      {
        if (_rotationOffset != value)
        {
          _rotationOffset = value;
          Invalidate();
        }
      }
    }
    private QuaternionF _rotationOffset = QuaternionF.Identity;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkeletonMapper"/> class.
    /// </summary>
    /// <param name="skeletonPoseA">The first skeleton pose. Can be <see langword="null"/>.</param>
    /// <param name="skeletonPoseB">The second skeleton pose. Can be <see langword="null"/>.</param>
    public SkeletonMapper(SkeletonPose skeletonPoseA, SkeletonPose skeletonPoseB)
    {
      _skeletonPoseA = skeletonPoseA;
      _skeletonPoseB = skeletonPoseB;

      BoneMappers = new BoneMapperCollection();
      BoneMappers.CollectionChanged += OnBoneMappersChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnBoneMappersChanged(object sender, CollectionChangedEventArgs<BoneMapper> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // ----- Set/Unset BoneMapper.SkeletonMapper
      if (eventArgs.OldItems != null)
        foreach (var item in eventArgs.OldItems)
          item.SkeletonMapper = null;

      if (eventArgs.NewItems != null)
        foreach (var item in eventArgs.NewItems)
          item.SkeletonMapper = this;
    }


    private void Invalidate()
    {
      foreach(var boneMapper in BoneMappers)
        boneMapper.Invalidate();
    }


    /// <summary>
    /// Modifies the second skeleton pose to match the first skeleton pose.
    /// </summary>
    public void MapAToB()
    {
      if (SkeletonPoseA == null || SkeletonPoseB == null)
        return;

      //_skeletonPoseB.ResetBoneTransforms(); // It is up to the bone mapper, if it wants to map from the bind pose or the current pose.

      foreach(var boneMapper in BoneMappers)
        boneMapper.MapAToB();
    }


    /// <summary>
    /// Modifies the first skeleton pose to match the second skeleton pose.
    /// </summary>
    public void MapBToA()
    {
      if (SkeletonPoseA == null || SkeletonPoseB == null)
        return;

      //_skeletonPoseA.ResetBoneTransforms(); // It is up to the bone mapper, if it wants to map from the bind pose or the current pose.

      foreach (var boneMapper in BoneMappers)
        boneMapper.MapBToA();
    }
    #endregion
  }
}
