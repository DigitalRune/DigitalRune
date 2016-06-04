// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents a mouse cursor of the UI theme.
  /// </summary>
  public class ThemeCursorContent : INamedObject
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
    /// Gets or sets the file name of the cursor.
    /// </summary>
    /// <value>The file name of the cursor.</value>
    public string FileName { get; set; }
  }
}
