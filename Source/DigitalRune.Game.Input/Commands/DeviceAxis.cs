// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Describes an analog input source, like an axis of joystick.
  /// </summary>
  public enum DeviceAxis
  {
    /// <summary>
    /// The x-coordinate of the absolute mouse position.
    /// </summary>
    MouseXAbsolute,

    /// <summary>
    /// The y-coordinate of the absolute mouse position.
    /// </summary>
    MouseYAbsolute,

    /// <summary>
    /// The x-coordinate of the mouse position change since the last frame.
    /// </summary>
    MouseXRelative,

    /// <summary>
    /// The y-coordinate of the mouse position change since the last frame.
    /// </summary>
    MouseYRelative,
    
    /// <summary>
    /// The value of the mouse wheel.
    /// </summary>
    MouseWheel,

    /// <summary>
    /// The horizontal axis of the left thumb stick on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadStickLeftX,

    /// <summary>
    /// The vertical axis of the left thumb stick on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadStickLeftY,

    /// <summary>
    /// The horizontal axis of the right thumb stick on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadStickRightX,

    /// <summary>
    /// The vertical axis of the right thumb stick on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadStickRightY,

    /// <summary>
    /// The value of the left trigger button on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadTriggerLeft,

    /// <summary>
    /// The value of the right trigger button on a gamepad.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Consistent with XNA.")]
    GamePadTriggerRight,
  }
}
