// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
#if !UNITY
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector4"/>.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class Vector4Traits : Singleton<Vector4Traits>, IAnimationValueTraits<Vector4>
  {
    /// <inheritdoc/>
    public void Create(ref Vector4 reference, out Vector4 value)
    {
      value = new Vector4();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector4 value)
    {
    }

    
    /// <inheritdoc/>
    public void Copy(ref Vector4 source, ref Vector4 target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector4 value, IAnimatableProperty<Vector4> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector4> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector4 identity)
    {
      identity = new Vector4();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector4 value, ref Vector4 inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector4 value0, ref Vector4 value1, ref Vector4 result)
    {
      Vector4.Add(ref value0, ref value1, out result);
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector4 value, int factor, ref Vector4 result)
    {
      Vector4.Multiply(ref value, factor, out result);
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector4 source, ref Vector4 target, float parameter, ref Vector4 result)
    {
      Vector4.Lerp(ref source, ref target, parameter, out result);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector4 value)
    {
      value = new Vector4();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector4 value, ref Vector4 nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector4 value)
    {
    }
  }
}
#endif
