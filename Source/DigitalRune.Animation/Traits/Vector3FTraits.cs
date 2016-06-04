// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector3F"/>.
  /// </summary>
  public class Vector3FTraits : Singleton<Vector3FTraits>, IAnimationValueTraits<Vector3F>
  {
    /// <inheritdoc/>
    public void Create(ref Vector3F reference, out Vector3F value)
    {
      value = new Vector3F();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector3F value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref Vector3F source, ref Vector3F target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector3F value, IAnimatableProperty<Vector3F> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector3F> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector3F identity)
    {
      identity = new Vector3F();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector3F value, ref Vector3F inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector3F value0, ref Vector3F value1, ref Vector3F result)
    {
      result = value0 + value1;
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector3F value, int factor, ref Vector3F result)
    {
      result = value * factor;
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector3F source, ref Vector3F target, float parameter, ref Vector3F result)
    {
      //result = source + (target - source) * parameter;

      // Optimized by inlining.
      result.X = source.X + (target.X - source.X) * parameter;
      result.Y = source.Y + (target.Y - source.Y) * parameter;
      result.Z = source.Z + (target.Z - source.Z) * parameter;
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector3F value)
    {
      value = new Vector3F();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector3F value, ref Vector3F nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector3F value)
    {
    }
  }
}
