// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Represents an easing function based on cubic Hermite interpolation (also known as "smooth 
  /// step").
  /// </summary>
  /// <remarks>
  /// The Hermite easing function is defined as: f(t) = (3 t<sup>2</sup> - t<sup>3</sup>) / 2
  /// </remarks>
  public class HermiteEase : EasingFunction
  {
    /// <inheritdoc/>
    protected override float EaseIn(float normalizedTime)
    {
      float t = normalizedTime;
      if (t <= 0)
        return 0;
      
      if (t >= 1)
        return 1;

      return t * t * (3 - t) / 2;
    }
  }
}
