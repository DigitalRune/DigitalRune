// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector2"/>.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class Vector2Traits : Singleton<Vector2Traits>, IAnimationValueTraits<Vector2>
  {
    /// <inheritdoc/>
    public void Create(ref Vector2 reference, out Vector2 value)
    {
      value = new Vector2();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector2 value)
    {
    }

    /// <inheritdoc/>
    public void Copy(ref Vector2 source, ref Vector2 target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector2 value, IAnimatableProperty<Vector2> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector2> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector2 identity)
    {
      identity = new Vector2();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector2 value, ref Vector2 inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector2 value0, ref Vector2 value1, ref Vector2 result)
    {
      Vector2.Add(ref value0, ref value1, out result);
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector2 value, int factor, ref Vector2 result)
    {
      Vector2.Multiply(ref value, factor, out result);
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector2 source, ref Vector2 target, float parameter, ref Vector2 result)
    {
      Vector2.Lerp(ref source, ref target, parameter, out result);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector2 value)
    {
      value = new Vector2();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector2 value, ref Vector2 nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector2 value)
    {
    }
  }
}
#endif
