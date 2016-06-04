// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that decelerates/accelerates towards the target value using a
  /// logarithmic function.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The logarithmic easing function is defined as: f(t) = log<sub>b</sub>((b - 1)t + 1)
  /// </para>
  /// <para>
  /// The <see cref="LogarithmicEase"/> is the inverse of the <see cref="ExponentialEase"/>. The
  /// <see cref="LogarithmicEase"/> decelerates where the <see cref="ExponentialEase"/> accelerates.
  /// </para>
  /// </remarks>
  public class LogarithmicEase : EasingFunction
  {
    /// <summary>
    /// Gets or sets the base of the logarithm.
    /// </summary>
    /// <value>
    /// The base of the logarithm. The default value is 2.
    /// </value>
    public float Base
    {
      get { return _base; }
      set
      {
        if (value <= 1)
          throw new ArgumentOutOfRangeException("value", "The base of the logarithm must be greater than 1.");

        _base = value;
      }
    }
    private float _base = 2;


    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      if (normalizedTime < 0)
        return 0;

      // logB(x) = ln(x) / ln(B)
      return (float)(Math.Log((Base - 1) * normalizedTime + 1) / Math.Log(Base));
    }
  }
}
