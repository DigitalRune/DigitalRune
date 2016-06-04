// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that models a spring that starts to oscillate until it reaches
  /// the target value.
  /// </summary>
  public class ElasticEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the number of oscillations periods.
    /// </summary>
    /// <value>
    /// The number of oscillations periods. The value must be greater than or equal to 0. Negative
    /// values are internally clamped to 0. The default value is 3.
    /// </value>
    public int Oscillations { get; set; }


    /// <summary>
    /// Gets or sets the stiffness of the spring.
    /// </summary>
    /// <value>
    /// <para>
    /// The stiffness of the spring. The springiness determines how fast the amplitude of the 
    /// oscillations grows (in case of the ease-in). A value of 0 means that the amplitude grows 
    /// linearly.
    /// </para>
    /// <para>
    /// The value must be greater than or equal to 0. Negative values are internally clamped to 0.
    /// The default value is 3.
    /// </para>
    /// </value>
    public float Springiness { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticEase"/> class.
    /// </summary>
    public ElasticEase()
    {
      Oscillations = 3;
      Springiness = 3;
    }


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      float oscillations = Math.Max(0.0f, Oscillations);
      float k = Math.Max(0.0f, Springiness);

      // The spring motion is defined as
      //   f(t) = f0∙A(t)∙sin(2∙π∙f∙t + φ)

      // The start value f0 and the offset φ are 0 in our case.

      // The frequency is defined by the number of oscillations:
      //   f = oscillations + 1/4
      float f = oscillations + 0.25f;

      //
      // The amplitude of a damped spring is 
      //   A(t) = e^(-kt)
      // But in the case of an ease-in we want start with small oscillations and grow in size.
      // Therefore we need to take the inverse:
      //   A(t) = 1 - e^(-kt)
      // We also need to normalize the amplitude such that it exactly reaches 1 at t = 1.
      //   A(t) = (1 - e^(-kt)) / (1 - e^(-k∙1))
      //        = (1 - e^(-kt)) / (1 - e^(-k))

      float a;
      if (Numeric.IsZero(k))
      {
        // When the constant k goes towards 0, the amplitude A(t) goes towards t.
        //  lim    A(t) = t
        // k --> 0
        a = t;
      }
      else
      {
        a = (1.0f - (float)Math.Exp(k * t)) / (1.0f - (float)Math.Exp(k));
      }

      //  f(t) = A(t)∙sin(2∙π∙f∙t)
      return a * (float)Math.Sin(ConstantsF.TwoPi * f * t);
    }
  }
}
