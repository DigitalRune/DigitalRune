// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Defines the type of a key or button press.
  /// </summary>
  public enum PressType
  {
    /// <summary>
    /// The button is currently held down. 
    /// </summary>
    Down,

    /// <summary>
    /// The button was up in the previous frame and is pressed down in this frame.
    /// </summary>
    Press,

    /// <summary>
    /// The button was pressed twice within a short time.
    /// </summary>
    DoubleClick,

    // Others (not often needed in games): Release, TripleClick, ...
  }
}
