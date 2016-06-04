// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="QuaternionF"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Linear interpolation (LERP) is used for quaternion interpolation. Spherical linear
  /// interpolation (SLERP) is not used because it is slower and the difference to LERP is minor.
  /// </para>
  /// </remarks>
  public class QuaternionFTraits : Singleton<QuaternionFTraits>, IAnimationValueTraits<QuaternionF>
  {
    /// <inheritdoc/>
    public void Create(ref QuaternionF reference, out QuaternionF value)
    {
      value = new QuaternionF();
    }


    /// <inheritdoc/>
    public void Recycle(ref QuaternionF value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref QuaternionF source, ref QuaternionF target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref QuaternionF value, IAnimatableProperty<QuaternionF> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<QuaternionF> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref QuaternionF identity)
    {
      identity = QuaternionF.Identity;
    }


    /// <inheritdoc/>
    public void Invert(ref QuaternionF value, ref QuaternionF inverse)
    {
      // Since it is a unit vector, we can return Conjugated instead of Inverse.
      inverse = value;
      inverse.Conjugate();
    }


    /// <inheritdoc/>
    public void Add(ref QuaternionF value0, ref QuaternionF value1, ref QuaternionF result)
    {
      result = value1 * value0;
    }


    /// <inheritdoc/>
    public void Multiply(ref QuaternionF value, int factor, ref QuaternionF result)
    {
      result = value;
      result.Power(factor);
    }


    /// <inheritdoc/>
    public void Interpolate(ref QuaternionF source, ref QuaternionF target, float parameter, ref QuaternionF result)
    {
      //result = InterpolationHelper.Lerp(source, target, parameter);

      // Optimized by inlining:
      // Get angle between quaternions
      //float cosθ = QuaternionF.Dot(sourceRotation, targetRotation);
      float cosθ = source.W * target.W + source.X * target.X + source.Y * target.Y + source.Z * target.Z;

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        // Lerp with inverted target!
        result.W = source.W - (target.W + source.W) * parameter;
        result.X = source.X - (target.X + source.X) * parameter;
        result.Y = source.Y - (target.Y + source.Y) * parameter;
        result.Z = source.Z - (target.Z + source.Z) * parameter;
      }
      else
      {
        // Normal lerp.
        result.W = source.W + (target.W - source.W) * parameter;
        result.X = source.X + (target.X - source.X) * parameter;
        result.Y = source.Y + (target.Y - source.Y) * parameter;
        result.Z = source.Z + (target.Z - source.Z) * parameter;
      }

      // Linear interpolation creates non-normalized quaternions. We need to 
      // re-normalize the result.
      result.Normalize();
    }


    /// <inheritdoc/>
    public void BeginBlend(ref QuaternionF value)
    {
      value = new QuaternionF();
    }


    /// <inheritdoc/>
    public void BlendNext(ref QuaternionF value, ref QuaternionF nextValue, float normalizedWeight)
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
    public void EndBlend(ref QuaternionF value)
    {
      value.Normalize();
    }
  }
}
