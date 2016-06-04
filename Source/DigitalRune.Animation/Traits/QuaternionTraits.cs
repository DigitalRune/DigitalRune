// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="Quaternion"/>. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </para>
  /// <para>
  /// Linear interpolation (LERP) is used for quaternion interpolation. Spherical linear
  /// interpolation (SLERP) is not used because it is slower and the difference to LERP is minor.
  /// </para>
  /// </remarks>
  public class QuaternionTraits : Singleton<QuaternionTraits>, IAnimationValueTraits<Quaternion>
  {
    /// <inheritdoc/>
    public void Create(ref Quaternion reference, out Quaternion value)
    {
      value = new Quaternion();
    }


    /// <inheritdoc/>
    public void Recycle(ref Quaternion value)
    {
    }

    
    /// <inheritdoc/>
    public void Copy(ref Quaternion source, ref Quaternion target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref Quaternion value, IAnimatableProperty<Quaternion> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<Quaternion> property)
    {      
    }


    /// <inheritdoc/>
    public void SetIdentity(ref Quaternion identity)
    {
      identity = Quaternion.Identity;
    }


    /// <inheritdoc/>
    public void Invert(ref Quaternion value, ref Quaternion inverse)
    {
      // Since it is a unit vector, we can return Conjugated instead of Inverse.
      inverse = value;
      inverse.Conjugate();
    }


    /// <inheritdoc/>
    public void Add(ref Quaternion value0, ref Quaternion value1, ref Quaternion result)
    {
      result = value1 * value0;
    }


    /// <inheritdoc/>
    public void Multiply(ref Quaternion value, int factor, ref Quaternion result)
    {
      QuaternionF q = (QuaternionF)value;
      q.Power(factor);
      result = (Quaternion)q;
    }


    /// <inheritdoc/>
    public void Interpolate(ref Quaternion source, ref Quaternion target, float parameter, ref Quaternion result)
    {
      Quaternion.Lerp(ref source, ref target, parameter, out result);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref Quaternion value)
    {
      value = new Quaternion();
    }


    /// <inheritdoc/>
    public void BlendNext(ref Quaternion value, ref Quaternion nextValue, float normalizedWeight)
    {
      // Get angle between quaternions:
      //float cosθ = QuaternionF.Dot(value, nextValue);
      float cosθ = value.W * nextValue.W + value.X * nextValue.X + value.Y * nextValue.Y + value.Z * nextValue.Z;

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        // Blend with inverted quaternion!
        value.W = value.W - normalizedWeight * nextValue.W;
        value.X = value.X - normalizedWeight * nextValue.X;
        value.Y = value.Y - normalizedWeight * nextValue.Y;
        value.Z = value.Z - normalizedWeight * nextValue.Z;
      }
      else
      {
        // Blend with normal quaternion.
        value.W = value.W + normalizedWeight * nextValue.W;
        value.X = value.X + normalizedWeight * nextValue.X;
        value.Y = value.Y + normalizedWeight * nextValue.Y;
        value.Z = value.Z + normalizedWeight * nextValue.Z;
      }
    }


    /// <inheritdoc/>
    public void EndBlend(ref Quaternion value)
    {
      value.Normalize();
    }
  }
}
#endif
