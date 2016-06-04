// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Vector3"/>.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class Vector3Traits : Singleton<Vector3Traits>, IAnimationValueTraits<Vector3>
  {
    /// <inheritdoc/>
    public void Create(ref Vector3 reference, out Vector3 value)
    {
      value = new Vector3();
    }


    /// <inheritdoc/>
    public void Recycle(ref Vector3 value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref Vector3 source, ref Vector3 target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Vector3 value, IAnimatableProperty<Vector3> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Vector3> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Vector3 identity)
    {
      identity = new Vector3();
    }


    /// <inheritdoc/>
    public void Invert(ref Vector3 value, ref Vector3 inverse)
    {
      inverse = -value;
    }


    /// <inheritdoc/>
    public void Add(ref Vector3 value0, ref Vector3 value1, ref Vector3 result)
    {
      Vector3.Add(ref value0, ref value1, out result);
    }


    /// <inheritdoc/>
    public void Multiply(ref Vector3 value, int factor, ref Vector3 result)
    {
      Vector3.Multiply(ref value, factor, out result);
    }


    /// <inheritdoc/>
    public void Interpolate(ref Vector3 source, ref Vector3 target, float parameter, ref Vector3 result)
    {
      Vector3.Lerp(ref source, ref target, parameter, out result);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Vector3 value)
    {
      value = new Vector3();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Vector3 value, ref Vector3 nextValue, float normalizedWeight)
    {
      value += normalizedWeight * nextValue;
    }


    /// <inheritdoc/>
    public void EndBlend(ref Vector3 value)
    {
    }
  }
}
#endif
