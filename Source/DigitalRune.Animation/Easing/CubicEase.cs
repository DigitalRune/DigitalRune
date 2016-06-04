// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates towards the target value using a cubic 
  /// function f(t)=t<sup>3</sup>.
  /// </summary>
  /// <remarks>
  /// The cubic easing function is defined as: f(t) = t<sup>3</sup>
  /// </remarks>
  public class CubicEase : EasingFunction
  {
    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      return t * t * t;
    }
  }
}
