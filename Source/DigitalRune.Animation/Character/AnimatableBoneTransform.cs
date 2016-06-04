// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Animation.Character
{
  /// <summary>
  /// Represents an <see cref="SrtTransform"/> of a bone that can be animated (no base value).
  /// </summary>
  /// <remarks>
  /// When the bone transform is changed, the bone data in the skeleton pose is invalidated.
  /// </remarks>
  internal sealed class AnimatableBoneTransform : IAnimatableProperty<SrtTransform>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal SkeletonPose SkeletonPose; // Internal because the AnimatableBoneTransforms are pooled.
                                        // The field is set/reset when created/recycled.
    private readonly int _boneIndex;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    #region ----- IAnimatableProperty -----

    /// <inheritdoc/>
    bool IAnimatableProperty.HasBaseValue
    {
      get { return false; }
    }


    /// <inheritdoc/>
    object IAnimatableProperty.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "This IAnimatableProperty does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    bool IAnimatableProperty.IsAnimated { get; set; }


    /// <inheritdoc/>
    object IAnimatableProperty.AnimationValue
    {
      get { return SkeletonPose.BoneTransforms[_boneIndex]; }
    }
    #endregion


    #region ----- IAnimatableProperty<T> -----

    /// <inheritdoc/>
    SrtTransform IAnimatableProperty<SrtTransform>.BaseValue
    {
      get
      {
        throw new NotImplementedException(
          "This IAnimatableProperty does not have a BaseValue. "
          + "Check HasBaseValue before accessing BaseValue!");
      }
    }


    /// <inheritdoc/>
    SrtTransform IAnimatableProperty<SrtTransform>.AnimationValue
    {
      get { return SkeletonPose.BoneTransforms[_boneIndex]; }
      set
      {
        SkeletonPose.BoneTransforms[_boneIndex] = value;
        SkeletonPose.Invalidate(_boneIndex);
      }
    }
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableBoneTransform"/> class.
    /// </summary>
    /// <param name="boneIndex">The index of the bone.</param>
    public AnimatableBoneTransform(int boneIndex)
    {
      Debug.Assert(boneIndex >= 0, "Parameter boneIndex must not be negative.");
      _boneIndex = boneIndex;

      // Note: The SkeletonPose is set/reset when the object is created/recycled.
      // See class SkeletonPose.
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
