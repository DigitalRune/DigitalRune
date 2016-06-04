// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Specifies when the <see cref="ButtonBase.Click"/> event should be raised for a control. 
  /// </summary>
  public enum ClickMode
  {
    /// <summary>
    /// Specifies that the <see cref="ButtonBase.Click"/> event should be raised when the button is
    /// pressed and released.
    /// </summary>
    Release,

    /// <summary>
    /// Specifies that the <see cref="ButtonBase.Click"/> event should be raised when the button is
    /// pressed.
    /// </summary>
    Press,

    //Hover,
  }
}
