// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates/decelerates towards the target value using an 
  /// exponential function.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The exponential easing function is defined as: f(t) = (1 - e<sup>kt</sup>) / (1 - e<sup>k</sup>)
  /// </para>
  /// <para>
  /// The <see cref="ExponentialEase"/> is the inverse of the <see cref="LogarithmicEase"/>. The
  /// <see cref="ExponentialEase"/> accelerates where the <see cref="LogarithmicEase"/> decelerates.
  /// </para>
  /// <para>
  /// Note: The exponential easing function can also be written as 
  ///   f(t) = (b<sup>t</sup> - 1) / (b - 1)
  /// where b = e<sup>k</sup>.
  /// </para>
  /// </remarks>
  public class ExponentialEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the exponent of the easing function.
    /// </summary>
    /// <value>
    /// The exponent of the easing function. The default value is 2.
    /// </value>
    public float Exponent { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ExponentialEase"/> class.
    /// </summary>
    public ExponentialEase()
    {
      Exponent = 2;
    }


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      float k = Exponent;

      if (Numeric.IsZero(k))
      {
        // When the constant k goes towards 0, the function f(t) goes towards t.
        //  lim    f(t) = t
        // k --> 0
        return t;
      }

      return (1.0f - (float)Math.Exp(k * t)) / (1.0f - (float)Math.Exp(k));
    }
  }
}
