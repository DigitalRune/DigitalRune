// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using System;
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Color"/>. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class ColorTraits : Singleton<ColorTraits>, IAnimationValueTraits<Color>
  {
    /// <inheritdoc/>
    public void Create(ref Color reference, out Color value)
    {
      value = new Color();
    }


    /// <inheritdoc/>
    public void Recycle(ref Color value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref Color source, ref Color target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Color value, IAnimatableProperty<Color> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Color> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Color identity)
    {
      identity = Color.Transparent;
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">
    /// <see cref="Color"/> does not have an inverse.
    /// </exception>
    public void Invert(ref Color value, ref Color inverse)
    {
      throw new NotSupportedException("Color animations cannot be looped using the loop behaviors CycleOffset. Use Cycle instead.");
    }


    /// <inheritdoc/>
    public void Add(ref Color value0, ref Color value1, ref Color result)
    {
      var v = value0.ToVector4() + value1.ToVector4();
      result = new Color(v.X, v.Y, v.Z, v.W);
    }


    /// <inheritdoc/>
    public void Multiply(ref Color value, int factor, ref Color result)
    {
      var v = value.ToVector4() * factor;
      result = new Color(v.X, v.Y, v.Z, v.W);
    }


    /// <inheritdoc/>
    public void Interpolate(ref Color source, ref Color target, float parameter, ref Color result)
    {
      var s = source.ToVector4();
      var t = target.ToVector4();
      var v = s + (t - s) * parameter;
      result = new Color(v.X, v.Y, v.Z, v.W);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Color value)
    {
      value = Color.Transparent;
    }


    /// <inheritdoc/>
    public void BlendNext(ref Color value, ref Color nextValue, float normalizedWeight)
    {
      var v = value.ToVector4();
      var n = nextValue.ToVector4();
      v += normalizedWeight * n;
      value = new Color(v.X, v.Y, v.Z, v.W);
    }


    /// <inheritdoc/>
    public void EndBlend(ref Color value)
    {
    }
  }
}
#endif
