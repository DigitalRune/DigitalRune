// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector4F"/>.
  /// </summary>
  public class Vector4FTraits : Singleton<Vector4FTraits>, IAnimationValueTraits<Vector4F>
  {
    /// <inheritdoc/>
    public void Create(ref Vector4F reference, out Vector4F value)
    {
      value = new Vector4F();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector4F value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref Vector4F source, ref Vector4F target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector4F value, IAnimatableProperty<Vector4F> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector4F> property)
    {      
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector4F identity)
    {
      identity = new Vector4F();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector4F value, ref Vector4F inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector4F value0, ref Vector4F value1, ref Vector4F result)
    {
      result = value0 + value1;
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector4F value, int factor, ref Vector4F result)
    {
      result = value * factor;
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector4F source, ref Vector4F target, float parameter, ref Vector4F result)
    {
      //result = source + (target - source) * parameter;

      // Optimized by inlining.
      result.X = source.X + (target.X - source.X) * parameter;
      result.Y = source.Y + (target.Y - source.Y) * parameter;
      result.Z = source.Z + (target.Z - source.Z) * parameter;
      result.W = source.W + (target.W - source.W) * parameter;
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector4F value)
    {
      value = new Vector4F();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector4F value, ref Vector4F nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector4F value)
    {
    }
  }
}
