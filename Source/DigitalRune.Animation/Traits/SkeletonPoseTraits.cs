// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation.Character;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="SkeletonPose"/>.
  /// </summary>
  public class SkeletonPoseTraits : Singleton<SkeletonPoseTraits>, IAnimationValueTraits<SkeletonPose>
  {
    // Note:
    // SkeletonPose.Invalidate() needs to be called when the internal BoneTransforms
    // are changed. Invalidate() only has an effect if the SkeletonPose has a
    // SkeletonBoneAccessor. Newly created SkeletonPose do not have a SkeletonBoneAccessor.
    // --> Calling Invalidate() should not have a performance impact. Except for the
    //     Set() method. Set() is called in AnimationManager.Apply() where the animation
    //     value is copied into the actual SkeletonPose, which might use a SkeletonBoneAccessor.
    //     Set() is the only method where Invalidate() is mandatory. However, the
    //     SkeletonPoseTraits might be used outside of the animation service. Invalidate()
    //     should be called in all methods to be safe.


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="reference"/> is <see langword="null"/>.
    /// </exception>
    public void Create(ref SkeletonPose reference, out SkeletonPose value)
    {
      if (reference == null)
        throw new ArgumentNullException("reference", "The reference value must not be null. This exception usually occurs when an IAnimatableProperty<SkeletonPose> is being animated, but the value of the property is null. A SkeletonPose must be set before the property can be animated.");

      value = SkeletonPose.Create(reference.Skeleton);
    }


    /// <inheritdoc/>
    public void Recycle(ref SkeletonPose value)
    {
      if (value != null)
      {
        value.Recycle();
        value = null;
      }
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public void Copy(ref SkeletonPose source, ref SkeletonPose target)
    {
      SkeletonHelper.Copy(source, target);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> or <paramref name="property"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The value of <paramref name="property"/> must not be <see langword="null"/>.
    /// </exception>
    public void Set(ref SkeletonPose value, IAnimatableProperty<SkeletonPose> property)
    {
      if (value == null)
        throw new ArgumentNullException("value");
      if (property == null)
        throw new ArgumentNullException("property");

      var animationValue = property.AnimationValue;
      if (animationValue == null)
      {
        if (property.HasBaseValue)
        {
          // The SkeletonPose will be recycled in Reset().
          Create(ref value, out animationValue);
          property.AnimationValue = animationValue;
        }
        else
        {
          throw new ArgumentException("The value of the property must not be null. This exception usually occurs when an IAnimatableProperty<SkeletonPose> is being animated, but the value of the property is null. A SkeletonPose must be set before the property can be animated.");
        }
      }

      SkeletonHelper.Copy(value, animationValue);
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<SkeletonPose> property)
    {
      // ReSharper disable once RedundantIfElseBlock

      if (property.HasBaseValue)
      {
        // Recycle SkeletonPose which has been allocated in Set().
        var animationValue = property.AnimationValue;
        property.AnimationValue = null;
        Recycle(ref animationValue);
      }
      else
      {
        // IAnimatableProperty<SkeletonPose> does not have a base value. The animation value
        // does not have to be set to null.
        // --> Do nothing.
      }
    }


    /// <inheritdoc/>
    public void SetIdentity(ref SkeletonPose identity)
    {
      Debug.Assert(identity != null, "Argument must not be null.");
      identity.ResetBoneTransforms();
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="SkeletonPose"/>s do not have inverse.</exception>
    public void Invert(ref SkeletonPose value, ref SkeletonPose inverse)
    {
      throw new NotSupportedException("SkeletonPose animations cannot be looped using the loop behaviors CycleOffset. Use Cycle instead.");
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value0"/>, <paramref name="value1"/> or <paramref name="result"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public void Add(ref SkeletonPose value0, ref SkeletonPose value1, ref SkeletonPose result)
    {
      if (value0 == null)
        throw new ArgumentNullException("value0");
      if (value1 == null)
        throw new ArgumentNullException("value1");
      if (result == null)
        throw new ArgumentNullException("result");

      var boneTransforms0 = value0.BoneTransforms;
      var boneTransforms1 = value1.BoneTransforms;
      var resultTransforms = result.BoneTransforms;
      for (int i = 0; i < resultTransforms.Length; i++)
        resultTransforms[i] = boneTransforms1[i] * boneTransforms0[i];

      result.Invalidate();
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="SkeletonPose"/>s cannot be multiplied.</exception>
    public void Multiply(ref SkeletonPose value, int factor, ref SkeletonPose result)
    {
      throw new NotSupportedException("SkeletonPose animations cannot be looped using the loop behaviors CycleOffset. Use Cycle instead.");
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/>, <paramref name="target"/> or <paramref name="result"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public void Interpolate(ref SkeletonPose source, ref SkeletonPose target, float parameter, ref SkeletonPose result)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");
      if (result == null)
        throw new ArgumentNullException("result");

      var sourceTransforms = source.BoneTransforms;
      var targetTransforms = target.BoneTransforms;
      var resultTransforms = result.BoneTransforms;
      for (int i = 0; i < resultTransforms.Length; i++)
        SrtTransform.Interpolate(ref sourceTransforms[i], ref targetTransforms[i], parameter, ref resultTransforms[i]);

      result.Invalidate();
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public void BeginBlend(ref SkeletonPose value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      // Set bone transforms to zero.
      var transforms = value.BoneTransforms;
      Array.Clear(transforms, 0, transforms.Length);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> or <paramref name="nextValue"/> is <see langword="null"/>.
    /// </exception>
    public void BlendNext(ref SkeletonPose value, ref SkeletonPose nextValue, float normalizedWeight)
    {
      if (value == null)
        throw new ArgumentNullException("value");
      if (nextValue == null)
        throw new ArgumentNullException("nextValue");

      var transforms = value.BoneTransforms;
      var nextTransforms = nextValue.BoneTransforms;
      for (int i = 0; i < transforms.Length; i++)
      {
        var rotation = transforms[i].Rotation;
        var nextRotation = nextTransforms[i].Rotation;

        // Get angle between quaternions:
        //float cosθ = QuaternionF.Dot(rotation, nextRotation);
        float cosθ = rotation.W * nextRotation.W + rotation.X * nextRotation.X + rotation.Y * nextRotation.Y + rotation.Z * nextRotation.Z;

        // Invert one quaternion if we would move along the long arc of interpolation.
        if (cosθ < 0)
        {
          // Blend with inverted quaternion!
          rotation.W = rotation.W - normalizedWeight * nextRotation.W;
          rotation.X = rotation.X - normalizedWeight * nextRotation.X;
          rotation.Y = rotation.Y - normalizedWeight * nextRotation.Y;
          rotation.Z = rotation.Z - normalizedWeight * nextRotation.Z;
        }
        else
        {
          // Blend with normal quaternion.
          rotation.W = rotation.W + normalizedWeight * nextRotation.W;
          rotation.X = rotation.X + normalizedWeight * nextRotation.X;
          rotation.Y = rotation.Y + normalizedWeight * nextRotation.Y;
          rotation.Z = rotation.Z + normalizedWeight * nextRotation.Z;
        }

        transforms[i].Rotation = rotation;
        transforms[i].Scale += normalizedWeight * nextTransforms[i].Scale;
        transforms[i].Translation += normalizedWeight * nextTransforms[i].Translation;
      }
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public void EndBlend(ref SkeletonPose value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      var transforms = value.BoneTransforms;
      for (int i = 0; i < transforms.Length; i++)
        transforms[i].Rotation.Normalize();

      value.Invalidate();
    }
  }
}
