// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates towards the target value using a sine 
  /// function.
  /// </summary>
  /// <remarks>
  /// The sine easing function is defined as: f(t) = 1 - sin((1 - t) * π/2)
  /// </remarks>
  public class SineEase : EasingFunction
  {
    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      return 1.0f - (float)Math.Sin((1 - t) * ConstantsF.PiOver2);
    }
  }
}
