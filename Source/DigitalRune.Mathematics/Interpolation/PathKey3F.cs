// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a waypoint of a 3-dimensional path (singe-precision).
  /// </summary>
  /// <inheritdoc cref="Path3F"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class PathKey3F : CurveKey<float, Vector3F>
  {
    private float _parameter;


    /// <summary>
    /// Gets the parameter.
    /// </summary>
    /// <returns>The parameter.</returns>
    protected override float GetParameter()
    {
      return _parameter;
    }


    /// <summary>
    /// Sets the parameter.
    /// </summary>
    /// <param name="value">The parameter</param>
    protected override void SetParameter(float value)
    {
      _parameter = value;
    }
  }
}
