// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines the behavior of an animation when its duration is exceeded.
  /// </summary>
  public enum FillBehavior
  {
    /// <summary>
    /// When the animation reaches the end of its duration it holds its last animation value until
    /// it is stopped or reset. (In some animation systems this behavior is also called 'Freeze'.) 
    /// </summary>
    Hold,

    /// <summary>
    /// The animation is stopped when it reaches the end of its duration. (In some animation systems
    /// this behavior is called 'Remove'.)
    /// </summary>
    Stop
  }
}
