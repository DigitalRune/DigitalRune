// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Defines the order in which in objects are rendered.
  /// </summary>
  public enum RenderOrder
  {
    /// <summary>
    /// The default/optimal render order - depends on the renderer.
    /// </summary>
    Default,

    /// <summary>
    /// Sort objects by distance and render nearest objects first.
    /// </summary>
    FrontToBack,

    /// <summary>
    /// Sort objects by distance and render furthest objects first.
    /// </summary>
    BackToFront,

    /// <summary>
    /// Render objects in the exact same order as they are passed to the renderer.
    /// </summary>
    UserDefined,
  }
}
