// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Describes the pressed modifier keys.
  /// </summary>
  [Flags]
  public enum ModifierKeys
  {
    /// <summary>
    /// No modifier key is pressed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Alt is pressed.
    /// </summary>
    Alt = 1,

    /// <summary>
    /// Control is pressed.
    /// </summary>
    Control = 2,

    /// <summary>
    /// Shift is pressed.
    /// </summary>
    Shift = 4,

    /// <summary>
    /// Apple key is pressed.
    /// </summary>
    Apple = 8,

    /// <summary>
    /// Windows key is pressed.
    /// </summary>
    Windows = Apple,

    /// <summary>
    /// ChatPadGreen is pressed.
    /// </summary>
    ChatPadGreen = 16,

    /// <summary>
    /// ChatPadOrange is pressed.
    /// </summary>
    ChatPadOrange = 32,

    /// <summary>
    /// Shift and Alt are pressed.
    /// </summary>
    ShiftAlt = Shift | Alt,

    /// <summary>
    /// Control and Alt are pressed.
    /// </summary>
    ControlAlt = Control | Alt,

    /// <summary>
    /// Control and Shift are pressed.
    /// </summary>
    ControlShift = Control | Shift,

    /// <summary>
    /// Control, Shift and Alt are pressed.
    /// </summary>
    ControlShiftAlt = Control | Shift | Alt,
  }
}
