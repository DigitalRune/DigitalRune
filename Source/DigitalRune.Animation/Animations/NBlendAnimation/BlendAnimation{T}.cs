// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Blends animations within a <see cref="BlendGroup"/>. (For internal use only.)
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  public class BlendAnimation<T> : BlendAnimation, IAnimation<T> 
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<IAnimation<T>> _animations = new List<IAnimation<T>>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc cref="IAnimation{T}.Traits"/>
    public IAnimationValueTraits<T> Traits
    {
      get { return (_animations.Count > 0) ? _animations[0].Traits : null; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    internal override void Initialize(BlendGroup blendGroup, string targetProperty)
    {
      base.Initialize(blendGroup, targetProperty);

      var numberOfAnimations = blendGroup.Count;
      for (int i = 0; i < numberOfAnimations; i++)
        _animations.Add(null);
    }


    /// <inheritdoc/>
    internal override void AddAnimation(int index, IAnimation animation)
    {
      var typedAnimation = animation as IAnimation<T>;
      if (typedAnimation != null)
        _animations[index] = typedAnimation;
    }


    /// <inheritdoc/>
    public override AnimationInstance CreateInstance()
    {
      return AnimationInstance<T>.Create(this);
    }


    /// <inheritdoc cref="IAnimation{T}.GetValue"/>
    /// <exception cref="InvalidAnimationException">
    /// Cannot evaluate blend animation because the blend animation is empty or the blend group is
    /// not set.
    /// </exception>
    public void GetValue(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      int numberOfAnimations = _animations.Count;

      if (Group == null)
        throw new InvalidAnimationException("Cannot evaluate blend animation because the blend group is not set.");

      Group.Update();

      // 'defaultSource', 'defaultTarget' and 'result' may be the same instance! We need to 
      // ensure that the source and target values are not overwritten by GetValue().
      // --> Use local variables to get animation values. 
      var traits = Traits;
      T value;
      traits.Create(ref defaultSource, out value);

      T nextValue;
      traits.Create(ref defaultSource, out nextValue);

      // Start blend operation.
      traits.BeginBlend(ref value);
      bool hasValue = false;
      for (int i = 0; i < numberOfAnimations; i++)
      {
        var animation = _animations[i];
        if (animation != null)
        {
          float normalizedWeight = Group.GetNormalizedWeight(i);
          if (normalizedWeight > 0.0f)
          {
            hasValue = true;
            TimeSpan animationTime = new TimeSpan((long)(time.Ticks * Group.GetTimeNormalizationFactor(i)));

            // Get next value.
            animation.GetValue(animationTime, ref defaultSource, ref defaultTarget, ref nextValue);

            // Blend next animation value to intermediate result.
            traits.BlendNext(ref value, ref nextValue, normalizedWeight);
          }
        }
      }

      if (hasValue)
      {
        // Finalize blend operation.
        traits.EndBlend(ref value);
        traits.Copy(ref value, ref result);
      }
      else
      {
        // The blend animation is empty or all animations are disabled.
        traits.Copy(ref defaultSource, ref result);
      }

      traits.Recycle(ref nextValue);
      traits.Recycle(ref value);
    }
    #endregion
  }
}
