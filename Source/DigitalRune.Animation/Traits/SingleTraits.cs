// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Single"/>.
  /// </summary>
  public class SingleTraits : Singleton<SingleTraits>, IAnimationValueTraits<float>
  {
    /// <inheritdoc/>
    public void Create(ref float reference, out float value)
    {
      value = 0;
    }


    /// <inheritdoc/>
    public void Recycle(ref float value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref float source, ref float target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref float value, IAnimatableProperty<float> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<float> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref float identity)
    {
      identity = 0;
    }


    /// <inheritdoc/>
    public void Invert(ref float value, ref float inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref float value0, ref float value1, ref float result)
    {
      result = value0 + value1;
    }


    /// <inheritdoc/>
    public void Multiply(ref float value, int factor, ref float result)
    {
      result = value * factor;
    }


    /// <inheritdoc/>
    public void Interpolate(ref float source, ref float target, float parameter, ref float result)
    {
      result = source + (target - source) * parameter;
    }


    /// <inheritdoc/>
    public void BeginBlend(ref float value)
    {
      value = 0;
    }


    /// <inheritdoc/>
    public void BlendNext(ref float value, ref float nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref float value)
    {
    }
  }
}
