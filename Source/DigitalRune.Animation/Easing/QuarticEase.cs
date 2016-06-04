// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates towards the target value using the function 
  /// f(t)=t<sup>4</sup>.
  /// </summary>
  /// <remarks>
  /// The easing function is defined as: f(t) = t<sup>4</sup>
  /// </remarks>
  public class QuarticEase : EasingFunction
  {
    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      return t * t * t * t;
    }
  }
}
