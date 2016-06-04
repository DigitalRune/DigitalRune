// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Defines the mapping direction of a <see cref="BoneMapper"/>
  /// </summary>
  [Flags]
  public enum BoneMappingDirection
  {
    /// <summary>
    /// Skeleton A is mapped to skeleton B. 
    /// </summary>
    AToB = 1,
    /// <summary>
    /// Skeleton B is mapped to skeleton A. 
    /// </summary>
    BToA = 2,
    /// <summary>
    /// The bone mapper maps in both directions.
    /// </summary>
    Both = 3,
  }


  /// <summary>
  /// Maps a bone transform of a skeleton to a bone transform of another skeleton.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A bone mapper observes the bone transform of a certain bone on one skeleton and sets the bone
  /// transform of the corresponding bone in another skeleton. Typically, bone mappers try to set
  /// bone transform so that posture of the target skeleton is as similar as possible to the posture
  /// of the source skeleton.
  /// </para>
  /// </remarks>
  public abstract class BoneMapper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the skeleton mapper. (This property is set automatically and should be
    /// treated as read-only.)
    /// </summary>
    /// <value>The skeleton mapper.</value>
    /// <remarks>
    /// This property is automatically set when the <see cref="BoneMapper"/> is added to a
    /// <see cref="SkeletonMapper"/>. 
    /// </remarks>
    public SkeletonMapper SkeletonMapper { get; set; }


    /// <summary>
    /// Gets or sets the desired mapping direction.
    /// </summary>
    /// <value>
    /// The mapping direction direction. The default is <see cref="BoneMappingDirection.Both"/>.
    /// </value>
    /// <remarks>
    /// This bone mapper only maps in the direction defined by this property. For example, 
    /// if the <see cref="Direction"/> is set to <see cref="BoneMappingDirection.AToB"/>, then
    /// the bone mapper does nothing when <see cref="MapBToA"/> is called.
    /// </remarks>
    public BoneMappingDirection Direction { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneMapper"/> class.
    /// </summary>
    protected BoneMapper()
    {
      Direction = BoneMappingDirection.Both;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invalidates all cached data of this bone mapper.
    /// </summary>
    /// <remarks>
    /// This method is automatically called by the <see cref="SkeletonMapper"/> when the mapped 
    /// <see cref="SkeletonPose"/>s are changed and cached data should be invalidated.
    /// </remarks>
    public void Invalidate()
    {
      OnInvalidate();
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.Invalidate"/> is called.
    /// </summary>
    protected virtual void OnInvalidate()
    {
    }


    /// <summary>
    /// Sets the bone transform in the second skeleton to match skeleton pose of the first skeleton.
    /// </summary>
    /// <exception cref="AnimationException"><see cref="SkeletonMapper"/> is not set.</exception>
    public void MapAToB()
    {
      if ((Direction & BoneMappingDirection.AToB) == 0)
        return;

      if (SkeletonMapper == null)
        throw new AnimationException("SkeletonMapper is not set.");

      OnMapAToB();
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapAToB"/> was called.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong><br/>
    /// This method must be implemented to perform the mapping operation. When this method is 
    /// called, it is guaranteed that <see cref="BoneMapper.Direction"/> is configured to map in
    /// this direction, and <see cref="BoneMapper.SkeletonMapper"/> is set.
    /// </remarks>
    protected abstract void OnMapAToB();


    /// <summary>
    /// Sets the bone transform in the first skeleton to match skeleton pose of the second
    /// skeleton.
    /// </summary>
    /// <exception cref="AnimationException"><see cref="SkeletonMapper"/> is not set.</exception>
    public void MapBToA()
    {
      if ((Direction & BoneMappingDirection.BToA) == 0)
        return;

      if (SkeletonMapper == null)
        throw new AnimationException("SkeletonMapper is not set.");

      OnMapBToA();
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapBToA"/> was called.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong><br/>
    /// This method must be implemented to perform the mapping operation. When this method is 
    /// called, it is guaranteed that <see cref="BoneMapper.Direction"/> is configured to map in 
    /// this direction, and <see cref="BoneMapper.SkeletonMapper"/> is set.
    /// </remarks>
    protected abstract void OnMapBToA();
    #endregion
  }
}
