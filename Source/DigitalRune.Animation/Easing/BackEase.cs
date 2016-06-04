// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that moves slightly in the opposite direction but then starts
  /// to accelerate towards the target value.
  /// </summary>
  /// <remarks>
  /// The back easing function is defined as: 
  ///   f(t) = t<sup>3</sup> - t ∙ <c>α</c> ∙ sin(t ∙ π)
  /// where <c>α</c> is the amplitude.
  /// </remarks>
  public class BackEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the amplitude of the backwards motion.
    /// </summary>
    /// <value>
    /// The amplitude of the backwards motion. The value should be in the range [0, ∞[. Negative
    /// values are internally clamped to 0. The default value is 1.
    /// </value>
    public float Amplitude { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BackEase"/> class.
    /// </summary>
    public BackEase()
    {
      Amplitude = 1;
    }


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;

      // The amplitude needs to be positive, otherwise the function won't converge to 1.
      float α = Math.Max(0.0f, Amplitude);

      return t * t * t - t * α * (float)Math.Sin(t * ConstantsF.Pi);
    }
  }
}
