// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// The type of step interpolation.
  /// </summary>
  public enum StepInterpolation
  {
    /// <summary>
    /// A step interpolation between two values <i>a</i> and <i>b</i> with an interpolation 
    /// parameter <i>u</i> that returns <i>a</i> for <i> u = 0</i> and 
    /// <i>b</i> for <i>u &gt; 0</i>.
    /// </summary>
    Left,

    /// <summary>
    /// A step interpolation between two values <i>a</i> and <i>b</i> with an interpolation
    /// parameter <i>u</i> that returns <i>a</i> for all <i>u &lt; 0.5</i> and <i>b</i> for
    /// <i>u ≥ 0.5</i>.
    /// </summary>
    Centered,

    /// <summary>
    /// A step interpolation between two values <i>a</i> and <i>b</i> with an interpolation 
    /// parameter <i>u</i> that returns <i>a</i> for all <i>u &lt; 1</i> and <i>b</i> for
    /// <i>u = 1</i>.
    /// </summary>
    Right,
  }
}
