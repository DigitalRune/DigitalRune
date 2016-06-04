// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Specifies the visibility of a <see cref="ScrollBar"/> for scrollable content. 
  /// </summary>
  public enum ScrollBarVisibility
  {
    /// <summary>
    /// A scroll bar does not appear even when the <see cref="ScrollViewer"/> cannot display all of
    /// the content. The content size is restricted by the size of the <see cref="ScrollViewer"/>.
    /// </summary>
    Disabled,

    /// <summary>
    /// A scroll bar appears when the <see cref="ScrollViewer"/> cannot display all of the content. 
    /// </summary>
    Auto,

    /// <summary>
    /// A scroll bar does not appear even when the <see cref="ScrollViewer"/> cannot display all of 
    /// the content. The content size is not limited by the size of the <see cref="ScrollViewer"/>.
    /// </summary>
    Hidden,

    /// <summary>
    /// A scroll bar always appears. 
    /// </summary>
    Visible
  }
}
