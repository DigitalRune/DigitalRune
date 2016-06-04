// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates/decelerates towards the target value using the
  /// function f(t)=t<sup>p</sup>.
  /// </summary>
  /// <remarks>
  /// The power easing function is defined as: f(t) = t<sup>p</sup>
  /// </remarks>
  public class PowerEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the exponent of the easing function.
    /// </summary>
    /// <value>
    /// The exponent of the easing function. The value must be greater than or equal to 0. Negative 
    /// values are internally clamped to 0. The default value is 2.
    /// </value>
    public float Power { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="PowerEase"/> class.
    /// </summary>
    public PowerEase()
    {
      Power = 2;
    }


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      float p = Math.Max(0.0f, Power);
      return (float)Math.Pow(t, p);
    }
  }
}
