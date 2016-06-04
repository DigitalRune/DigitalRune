// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders the shadow maps of <see cref="LightNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The shadow map renderers usually require a <see cref="RenderCallback"/> method to render the
  /// scene. The callback method needs to render the scene using the camera and the information
  /// given in the <see cref="RenderContext"/>.
  /// </para>
  /// </remarks>
  public interface IShadowMapRenderer
  {
    /// <summary>
    /// Gets or sets the method which renders the scene into the shadow map.
    /// </summary>
    /// <value>The callback method that renders the scene into the shadow map.</value>
    /// <remarks>
    /// The render callback renders the scene for the shadow map using the camera and the
    /// information currently set in the render context. It returns <see langword="true"/> if
    /// any objects were rendered and <see langword="false"/> if no objects were rendered (= shadow
    /// map is empty).
    /// </remarks>
    Func<RenderContext, bool> RenderCallback { get; set; }
  }
}