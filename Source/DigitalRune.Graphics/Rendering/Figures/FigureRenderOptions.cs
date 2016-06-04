// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Defines options for rendering figures.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
  [Flags]
  public enum FigureRenderOptions
  {
    /// <summary>
    /// Disable rendering of figures.
    /// </summary>
    RenderNone = 0,

    /// <summary>
    /// Render filled areas.
    /// </summary>
    RenderFill = 1,

    /// <summary>
    /// Render stroked lines.
    /// </summary>
    RenderStroke = 2,

    /// <summary>
    /// Render filled areas and stroked lines. (Default)
    /// </summary>
    RenderFillAndStroke = RenderFill | RenderStroke,
  }
}
