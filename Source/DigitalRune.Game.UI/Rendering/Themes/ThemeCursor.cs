// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents a mouse cursor of the UI theme.
  /// </summary>
  public class ThemeCursor : INamedObject
  {
    /// <summary>
    /// Gets the name of the cursor.
    /// </summary>
    /// <value>The name of the cursor.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this cursor is the default cursor.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this cursor is the default cursor; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDefault { get; set; }


    /// <summary>
    /// Gets or sets the cursor.
    /// </summary>
    /// <value>The cursor.</value>
    /// <remarks>
    /// This object must be of type <strong>System.Windows.Forms.Cursor</strong>. (The type 
    /// <see cref="System.Object"/> is used to avoid referencing 
    /// <strong>System.Windows.Forms.dll</strong> in this portable library.)
    /// </remarks>
    public object Cursor { get; set; }
  }
}
