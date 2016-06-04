// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Character;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of a <see cref="SrtTransform"/>.
  /// </summary>
  public class SrtTransformTraits : Singleton<SrtTransformTraits>, IAnimationValueTraits<SrtTransform>
  {
    /// <inheritdoc/>
    public void Create(ref SrtTransform reference, out SrtTransform value)
    {
      value = new SrtTransform();
    }


    /// <inheritdoc/>
    public void Recycle(ref SrtTransform value)
    {
    }


    /// <inheritdoc/>
    public void Copy(ref SrtTransform source, ref SrtTransform target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref SrtTransform value, IAnimatableProperty<SrtTransform> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<SrtTransform> property)
    {
    }


    /// <inheritdoc/>
    public void SetIdentity(ref SrtTransform identity)
    {
      identity = SrtTransform.Identity;
    }


    /// <inheritdoc/>
    public void Invert(ref SrtTransform value, ref SrtTransform inverse)
    {
      inverse = value.Inverse;
    }


    /// <inheritdoc/>
    public void Add(ref SrtTransform value0, ref SrtTransform value1, ref SrtTransform result)
    {
      result = value1 * value0;
    }


    /// <inheritdoc/>
    public void Multiply(ref SrtTransform value, int factor, ref SrtTransform result)
    {
      if (factor == 0)
      {
        result = SrtTransform.Identity;
        return;
      }

      SrtTransform srt;
      if (factor < 0)
      {
        srt = value.Inverse;
        factor = -factor;
      }
      else
      {
        srt = value;
      }

      result = srt;
      for (int i = 1; i < factor; i++)
        result = srt * result;
    }


    /// <inheritdoc/>
    public void Interpolate(ref SrtTransform source, ref SrtTransform target, float parameter, ref SrtTransform result)
    {
      SrtTransform.Interpolate(ref source, ref target, parameter, ref result);
    }


    /// <inheritdoc/>
    public void BeginBlend(ref SrtTransform value)
    {
      value = new SrtTransform();
    }


    /// <inheritdoc/>
    public void BlendNext(ref SrtTransform value, ref SrtTransform nextValue, float normalizedWeight)
    {
      var rotation = value.Rotation;
      var nextRotation = nextValue.Rotation;

      // Get angle between quaternions:
      //float cosθ = QuaternionF.Dot(rotation, nextRotation);
      float cosθ = rotation.W * nextRotation.W + rotation.X * nextRotation.X + rotation.Y * nextRotation.Y + rotation.Z * nextRotation.Z;

      // Invert one quaternion if we would move along the long arc of interpolation.
      if (cosθ < 0)
      {
        // Blend with inverted quaternion!
        rotation.W = rotation.W - normalizedWeight * nextRotation.W;
        rotation.X = rotation.X - normalizedWeight * nextRotation.X;
        rotation.Y = rotation.Y - normalizedWeight * nextRotation.Y;
        rotation.Z = rotation.Z - normalizedWeight * nextRotation.Z;
      }
      else
      {
        // Blend with normal quaternion.
        rotation.W = rotation.W + normalizedWeight * nextRotation.W;
        rotation.X = rotation.X + normalizedWeight * nextRotation.X;
        rotation.Y = rotation.Y + normalizedWeight * nextRotation.Y;
        rotation.Z = rotation.Z + normalizedWeight * nextRotation.Z;
      }

      value.Rotation = rotation;
      value.Scale += normalizedWeight * nextValue.Scale;
      value.Translation += normalizedWeight * nextValue.Translation;
    }


    /// <inheritdoc/>
    public void EndBlend(ref SrtTransform value)
    {
      value.Rotation.Normalize();
    }
  }
}
