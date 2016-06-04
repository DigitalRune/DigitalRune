// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Easing
{
  /// <summary>
  /// Defines how the easing functions interpolate.
  /// </summary>
  public enum EasingMode
  {
    ///<summary>
    /// The interpolation follows the formula of the easing function.
    ///</summary>
    EaseIn,

    /// <summary>
    /// The interpolation follows the reverse of the formula of the easing function.
    /// </summary>
    EaseOut,

    /// <summary>
    /// The interpolation uses <see cref="EaseIn"/> for the first half of the interpolation and
    /// <see cref="EaseOut"/> for the second half.
    /// </summary>
    EaseInOut,
  }
}
