// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Controls;
#if !SILVERLIGHT
using Keys = Microsoft.Xna.Framework.Input.Keys;
#else
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Provides the ability to configure and control the graphical user interface.
  /// </summary>
  /// <remarks>
  /// <see cref="UIScreen"/>s can be added to the <see cref="Screens"/> collection. Screens are 
  /// automatically updated. But screens are not automatically drawn! 
  /// <see cref="UIScreen.Draw(TimeSpan)"/> must be called manually.
  /// </remarks>
  public interface IUIService
  {
    /// <summary>
    /// Gets or sets the mouse cursor, overriding the default mouse cursor.
    /// </summary>
    /// <value>
    /// The mouse cursor, overriding the current mouse cursor. Set to <see langword="null"/> to 
    /// use the default mouse cursor. 
    /// </value>
    /// <remarks>
    /// <para>
    /// This object must be of type <strong>System.Windows.Forms.Cursor</strong>. (The type 
    /// <see cref="System.Object"/> is used to avoid referencing 
    /// <strong>System.Windows.Forms.dll</strong> in this portable library.)
    /// </para>
    /// <para>
    /// Normally, the control under the mouse determines the current mouse cursor. If this property
    /// is set, the automatically determined cursor is overridden. This is useful, for example,
    /// if a Wait cursor should displayed.
    /// </para>
    /// </remarks>
    object Cursor { get; set; }


    /// <summary>
    /// Gets the <strong>Form</strong> of the XNA <strong>Game</strong>.
    /// </summary>
    /// <value>The <strong>Form</strong> of the XNA <strong>Game</strong>.</value>
    /// <remarks>
    /// <para>
    /// This property is only set in Windows (desktop) builds. On most other platforms it is 
    /// <see langword="null"/>.
    /// </para>
    /// <para>
    /// This object must be of type <strong>System.Windows.Forms.Form</strong>. (The type 
    /// <see cref="System.Object"/> is used to avoid referencing 
    /// <strong>System.Windows.Forms.dll</strong> in this portable library.)
    /// </para>
    /// </remarks>
    object GameForm { get; }


    /// <summary>
    /// Gets the input service.
    /// </summary>
    /// <value>The input service.</value>
    IInputService InputService { get; }


    /// <summary>
    /// Gets or sets the key map that translates <see cref="Keys"/> to characters.
    /// </summary>
    /// <value>
    /// The key map. The default is a key map that is automatically chosen based on the current
    /// culture settings.
    /// </value>
    KeyMap KeyMap { get; set; }


    /// <summary>
    /// Gets the <see cref="UIScreen"/>s.
    /// </summary>
    /// <value>The <see cref="UIScreen"/>s.</value>
    UIScreenCollection Screens { get; }
  }
}
