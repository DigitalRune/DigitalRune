// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Copies a bone transform from one skeleton to the other skeleton.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is the simplest <see cref="BoneMapper"/>. It simply reads the bone transform of the bone
  /// in the first skeleton and sets the same bone transform in the bone in the second skeleton.
  /// This mapping can be used if the mapped skeletons and bone orientations (in the bind pose) are
  /// very similar.
  /// </para>
  /// <para>
  /// The <see cref="DirectBoneMapper"/> works either in local bone space or in model space (see 
  /// <see cref="MapAbsoluteTransforms"/>). Per default, it works in local bone space 
  /// (<see cref="MapAbsoluteTransforms"/> is <see langword="false"/>). When working in local bone 
  /// space, the bone mapper will transfer all orientation changes relative to the parent bones; for
  /// example, if the bone in the source skeleton was rotate up by 90°, it will also be rotated up 
  /// by 90° in the target skeleton. When working in model space 
  /// (<see cref="MapAbsoluteTransforms"/> is <see langword="true"/>) the bone mapper will transfer 
  /// the absolute bone pose relative to the model space; for example, if the bone in the source 
  /// skeleton is pointing down (relative to model space), the bone in the target skeleton will also
  /// be rotated so that it points down.
  /// </para> 
  /// </remarks>
  public class DirectBoneMapper : BoneMapper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // true if cached data is invalid.
    private bool _isDirty = true;

    // Converts from A space to B space. Only rotations.
    private QuaternionF _rotationAToB;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bone index for the first skeleton.
    /// </summary>
    /// <value>The bone index for the first skeleton.</value>
    public int BoneIndexA
    {
      get { return _boneIndexA; }
      set
      {
        if (_boneIndexA != value)
        {
          _boneIndexA = value;
          Invalidate();
        }
      }
    }
    private int _boneIndexA;


    /// <summary>
    /// Gets or sets the bone index for the second skeleton.
    /// </summary>
    /// <value>The bone index for the second skeleton.</value>
    public int BoneIndexB
    {
      get { return _boneIndexB; }
      set
      {
        if (_boneIndexB != value)
        {
          _boneIndexB = value;
          Invalidate();
        }
      }
    }
    private int _boneIndexB;


    /// <summary>
    /// Gets or sets a value indicating whether translations are mapped or ignored. (If 
    /// <see cref="MapAbsoluteTransforms"/> is set, translations are always ignored and this
    /// property is not used.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if translations in the bone transform are mapped; otherwise, 
    /// <see langword="false"/> if translations are ignored and only rotations are mapped. The 
    /// default is <see langword="true"/>.
    /// </value>
    public bool MapTranslations { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the mapping is performed in model space.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the bone transform relative to model space is mapped to the other 
    /// skeleton; otherwise, <see langword="false"/> if the bone transform relative to the local 
    /// bone space is mapped to the other skeleton. The default is <see langword="false"/>.
    /// </value>
    public bool MapAbsoluteTransforms
    {
      get { return _mapAbsoluteTransforms; }
      set
      {
        if (_mapAbsoluteTransforms != value)
        {
          _mapAbsoluteTransforms = value;
          Invalidate();
        }
      }
    }
    private bool _mapAbsoluteTransforms;


    /// <summary>
    /// Gets or sets the scale of second skeleton relative to the first skeleton. (Only relevant if
    /// bone translations are mapped.)
    /// </summary>
    /// <value>
    /// The scale of the second skeleton relative to the first skeleton. The default is 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// Translations that are mapped are multiplied with this scale factor. So if the second
    /// skeleton is about twice as large as the first skeleton, set this value to 2.
    /// </para>
    /// <para>
    /// <see cref="EstimateScale"/> can be used to set this value to an automatically guessed scale
    /// factor. 
    /// </para>
    /// </remarks>
    public float ScaleAToB { get; set; }


    // TODO: public QuaternionF RotationOffset { get; set; }
    // Offset from BoneA to BoneB. This could be used to correct the Archer - Dude mapping.
    // The archer is in T-Pose. The Dude has lowered arms. The rotation offset rotates from
    // horizontal arms to lowered arms.
    // Currently we use the ChainBoneMapper for this cases.
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectBoneMapper"/> class.
    /// </summary>
    /// <param name="boneIndexA">The bone index for the first skeleton.</param>
    /// <param name="boneIndexB">The bone index for the second skeleton.</param>
    public DirectBoneMapper(int boneIndexA, int boneIndexB)
    {
      BoneIndexA = boneIndexA;
      BoneIndexB = boneIndexB;
      MapTranslations = true;
      MapAbsoluteTransforms = false;
      ScaleAToB = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Analyzes the skeletons and sets <see cref="ScaleAToB"/> to a guessed scale factor.
    /// </summary>
    public void EstimateScale()
    {
      // Use parent bone lengths to estimate the scale factor.
      var bindPoseARelative = SkeletonMapper.SkeletonPoseA.Skeleton.GetBindPoseRelative(BoneIndexA);
      var bindPoseBRelative = SkeletonMapper.SkeletonPoseB.Skeleton.GetBindPoseRelative(BoneIndexB);
      ScaleAToB = bindPoseBRelative.Translation.Length / bindPoseARelative.Translation.Length;
    }


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
      var skeletonInstanceA = SkeletonMapper.SkeletonPoseA;
      var skeletonInstanceB = SkeletonMapper.SkeletonPoseB;
      var skeletonA = skeletonInstanceA.Skeleton;
      var skeletonB = skeletonInstanceB.Skeleton;

      if (BoneIndexA < 0 || BoneIndexA >= skeletonInstanceA.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("BoneIndexA is out of range.");

      if (BoneIndexB < 0 || BoneIndexB >= skeletonInstanceB.Skeleton.NumberOfBones)
        throw new IndexOutOfRangeException("BoneIndexB is out of range.");

      // Compute offsets.
      var bindPoseAAbsoluteInverse = skeletonA.GetBindPoseAbsoluteInverse(BoneIndexA);
      var bindPoseBAbsoluteInverse = skeletonB.GetBindPoseAbsoluteInverse(BoneIndexB);

      if (MapAbsoluteTransforms)
      {
        _rotationAToB = SkeletonMapper.RotationOffset;
      }
      else
      {
        // Read from right to left: 
        // Change from A bone space to model A space.
        // Apply rotation offset to change from model A space to model B space.
        // Change model B space to B bone space.
        // --> The result transforms from bone A space to bone B space.
        _rotationAToB = bindPoseBAbsoluteInverse.Rotation * SkeletonMapper.RotationOffset * bindPoseAAbsoluteInverse.Rotation.Conjugated;
      }
      _rotationAToB.Normalize();
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapAToB"/> was called.
    /// </summary>
    protected override void OnMapAToB()
    {
      CacheDerivedData();

      if (MapAbsoluteTransforms)
        MapAbsolute(
          MapTranslations,
          SkeletonMapper.SkeletonPoseA,
          BoneIndexA,
          SkeletonMapper.SkeletonPoseB,
          BoneIndexB,
          ScaleAToB,
          _rotationAToB.Conjugated,
          _rotationAToB);
      else
        MapLocal(
          MapTranslations,
          SkeletonMapper.SkeletonPoseA,
          BoneIndexA,
          SkeletonMapper.SkeletonPoseB,
          BoneIndexB,
          ScaleAToB,
          _rotationAToB.Conjugated,
          _rotationAToB);
    }


    /// <summary>
    /// Called when <see cref="BoneMapper.MapBToA"/> was called.
    /// </summary>
    protected override void OnMapBToA()
    {
      CacheDerivedData();

      if (MapAbsoluteTransforms)
        MapAbsolute(
          MapTranslations,
          SkeletonMapper.SkeletonPoseB,
          BoneIndexB,
          SkeletonMapper.SkeletonPoseA,
          BoneIndexA,
          1 / ScaleAToB,
          _rotationAToB,
          _rotationAToB.Conjugated);
      else
        MapLocal(
          MapTranslations,
          SkeletonMapper.SkeletonPoseB,
          BoneIndexB,
          SkeletonMapper.SkeletonPoseA,
          BoneIndexA,
          1 / ScaleAToB,
          _rotationAToB,
          _rotationAToB.Conjugated);
    }


    /// <summary>
    /// Perform mapping in absolute space.
    /// </summary>
    private static void MapAbsolute(bool mapTranslations, SkeletonPose skeletonA, int boneIndexA, SkeletonPose skeletonB, int boneIndexB, float scaleAToB, QuaternionF rotationBToA, QuaternionF rotationAToB)
    {
      // The current absolute bone pose of bone A.
      var boneAActualRotationAbsolute = skeletonA.GetBonePoseAbsolute(boneIndexA).Rotation;
      var boneABindRotationAbsolute = skeletonA.Skeleton.GetBindPoseAbsoluteInverse(boneIndexA).Rotation;

      var boneBBindRotationAbsolute = skeletonB.Skeleton.GetBindPoseAbsoluteInverse(boneIndexB).Rotation.Inverse;

      var relativeRotation = boneAActualRotationAbsolute * boneABindRotationAbsolute;

      // Rotation: Using similarity transformation: (Read from right to left.)
      // Rotate from model B space to model A space.
      // Apply the bone transform rotation in model A space
      // Rotate back from model A space to model B space. 
      relativeRotation = rotationAToB * relativeRotation * rotationBToA;
      skeletonB.SetBoneRotationAbsolute(boneIndexB, relativeRotation * boneBBindRotationAbsolute);

      // TODO: Map translations.
      // How? 
      // Map translation relative to model space? 
      // Map translation relative to the next common bone ancestor that is in both skeletons
      // (then we would need a mechanism or user input that gives us this ancestor, complicated)?
      // Map translation relative to local space (the bone transform translation as in MapLocal)?
    }


    /// <summary>
    /// Perform mapping in local bone space.
    /// </summary>
    private static void MapLocal(bool mapTranslations, SkeletonPose skeletonA, int boneIndexA, SkeletonPose skeletonB, int boneIndexB, float scaleAToB, QuaternionF rotationBToA, QuaternionF rotationAToB)
    {
      var boneTransform = skeletonA.GetBoneTransform(boneIndexA);

      // Remove any scaling.
      boneTransform.Scale = Vector3F.One;

      // Rotation: Using similarity transformation: (Read from right to left.)
      // Rotate from bone B space to bone A space.
      // Apply the bone transform rotation in bone A space
      // Rotate back from bone A space to bone B space. 
      boneTransform.Rotation = rotationAToB * boneTransform.Rotation * rotationBToA;

      // If we should map translations, then we scale the translation and rotate it from
      // bone A space to bone B space.
      if (mapTranslations)
        boneTransform.Translation = rotationAToB.Rotate(boneTransform.Translation * scaleAToB);
      else
        boneTransform.Translation = Vector3F.Zero;

      // Apply new bone transform to B.
      skeletonB.SetBoneTransform(boneIndexB, boneTransform);
    }
    #endregion
  }
}
