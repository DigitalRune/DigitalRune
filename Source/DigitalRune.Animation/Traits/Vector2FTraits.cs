// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector2F"/>.
  /// </summary>
  public class Vector2FTraits : Singleton<Vector2FTraits>, IAnimationValueTraits<Vector2F>
  {
    /// <inheritdoc/>
    public void Create(ref Vector2F reference, out Vector2F value)
    {
      value = new Vector2F();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector2F value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref Vector2F source, ref Vector2F target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector2F value, IAnimatableProperty<Vector2F> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector2F> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector2F identity)
    {
      identity = new Vector2F();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector2F value, ref Vector2F inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector2F value0, ref Vector2F value1, ref Vector2F result)
    {
      result = value0 + value1;
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector2F value, int factor, ref Vector2F result)
    {
      result = value * factor;
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector2F source, ref Vector2F target, float parameter, ref Vector2F result)
    {
      //result = source + (target - source) * parameter;

      // Optimized by inlining.
      result.X = source.X + (target.X - source.X) * parameter;
      result.Y = source.Y + (target.Y - source.Y) * parameter;
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector2F value)
    {
      value = new Vector2F();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector2F value, ref Vector2F nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector2F value)
    {
    }
  }
}
