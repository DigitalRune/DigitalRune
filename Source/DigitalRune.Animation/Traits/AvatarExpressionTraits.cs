// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if XNA && (WINDOWS || XBOX)
using System;
using Microsoft.Xna.Framework.GamerServices;


namespace DigitalRune.Animation.Traits
{
  /// <summary>
  /// Describes the properties of an <see cref="AvatarExpression"/>. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animations.dll.
  /// </remarks>
  public class AvatarExpressionTraits : Singleton<AvatarExpressionTraits>, IAnimationValueTraits<AvatarExpression>
  {
    /// <inheritdoc/>
    public void Create(ref AvatarExpression reference, out AvatarExpression value)
    {
      value = new AvatarExpression();
    }


    /// <inheritdoc/>
    public void Recycle(ref AvatarExpression value)
    {
    }

    
    /// <inheritdoc/>
    public void Copy(ref AvatarExpression source, ref AvatarExpression target)
    {
      target = source;
    }


    /// <inheritdoc/>
    public void Set(ref AvatarExpression value, IAnimatableProperty<AvatarExpression> property)
    {
      property.AnimationValue = value;
    }


    /// <inheritdoc/>
    public void Reset(IAnimatableProperty<AvatarExpression> property)
    {      
    }


    /// <inheritdoc/>
    public void SetIdentity(ref AvatarExpression identity)
    {
      identity.LeftEye = AvatarEye.Neutral;
      identity.LeftEyebrow = AvatarEyebrow.Neutral;
      identity.Mouth = AvatarMouth.Neutral;
      identity.RightEye = AvatarEye.Neutral;
      identity.RightEyebrow = AvatarEyebrow.Neutral;
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">
    /// <see cref="AvatarExpression"/> does not have an inverse.
    /// </exception>
    public void Invert(ref AvatarExpression value, ref AvatarExpression inverse)
    {
      throw new NotSupportedException("AvatarExpression animations cannot be looped using the loop behaviors CycleOffset. Use Cycle instead.");
    }

    
    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="AvatarExpression"/>s cannot be added.</exception>
    public void Add(ref AvatarExpression value0, ref AvatarExpression value1, ref AvatarExpression result)
    {
      throw new NotSupportedException("AvatarExpressions cannot be added.");
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="AvatarExpression"/>s cannot be multiplied.</exception>
    public void Multiply(ref AvatarExpression value, int factor, ref AvatarExpression result)
    {
      throw new NotSupportedException("AvatarExpressions cannot be multiplied.");
    }


    /// <inheritdoc/>
    public void Interpolate(ref AvatarExpression source, ref AvatarExpression target, float parameter, ref AvatarExpression result)
    {
      result = source;
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="AvatarExpression"/>s cannot be blended.</exception>
    public void BeginBlend(ref AvatarExpression value)
    {
      throw new NotSupportedException("AvatarExpressions cannot be blended.");
    }


    /// <inheritdoc/>
    /// <exception cref="NotSupportedException"><see cref="AvatarExpression"/>s cannot be blended.</exception>
    public void BlendNext(ref AvatarExpression value, ref AvatarExpression nextValue, float normalizedWeight)
    {
      throw new NotSupportedException("AvatarExpressions cannot be blended.");
    }


    /// <inheritdoc/>
    public void EndBlend(ref AvatarExpression value)
    {
    }
  }
}
#endif
