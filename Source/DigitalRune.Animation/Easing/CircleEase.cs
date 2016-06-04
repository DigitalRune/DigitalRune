// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function that accelerates towards the target value using a circular 
  /// function.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The circle easing function is defined as: f(t) = 1 - sqrt(1 - t<sup>2</sup>)
  /// </para>
  /// <para>
  /// The valid range of t is [-1, 1]. The normalized time is internally clamped to this range.
  /// </para>
  /// </remarks>
  public class CircleEase : EasingFunction
  {
    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = MathHelper.Clamp(normalizedTime, -1, 1);
      return 1.0f - (float)Math.Sqrt(1.0f - t * t);
    }
  }
}
