// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a control point on a <see cref="Curve2F"/> (single-precision).
  /// </summary>
  /// <inheritdoc cref="Curve2F"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class CurveKey2F : CurveKey<float, Vector2F>
  {
    /// <summary>
    /// Gets the parameter.
    /// </summary>
    /// <returns>The parameter.</returns>
    protected override float GetParameter()
    {
      return Point.X;
    }


    /// <summary>
    /// Sets the parameter.
    /// </summary>
    /// <param name="value">The parameter</param>
    protected override void SetParameter(float value)
    {
      Point = new Vector2F(value, Point.Y);
    }
  }
}
