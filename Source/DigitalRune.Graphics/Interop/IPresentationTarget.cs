// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Provides a window handle (HWND) which can be used to present a 3D scene.
  /// </summary>
  public interface IPresentationTarget
  {
    /// <summary>
    /// Gets or sets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <remarks>
    /// This property is automatically set when a presentation target is added to a graphics 
    /// service. Do not set this property manually!
    /// </remarks>
    IGraphicsService GraphicsService { get; set; }


    /// <summary>
    /// Gets the window handle (HWND).
    /// </summary>
    /// <value>
    /// The window handle (HWND) of the control. Can be <see cref="IntPtr.Zero"/> in which case the 
    /// presentation target should be ignored.
    /// </value>
    [Obsolete("Property will be removed in a future release.")]
    IntPtr Handle { get; }


    /// <summary>
    /// Gets the width of the presentation target in pixels.
    /// </summary>
    /// <value>The width of the presentation target in pixels.</value>
    int Width { get; }


    /// <summary>
    /// Gets the height of the presentation target in pixels.
    /// </summary>
    /// <value>The height of the presentation target in pixels.</value>
    int Height { get; }


    /// <summary>
    /// Gets or sets a value indicating whether the presentation target is displayed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the presentation target is visible; otherwise,
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// For the graphics engine: It is not necessary to render to invisible presentation targets.
    /// </remarks>
    bool IsVisible { get; }


    /// <summary>
    /// Called by the <see cref="GraphicsManager"/> before rendering into the presentation target.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <returns>
    /// <see langword="true"/> if successful; otherwise, <see langword="false"/> if the presentation
    /// target is not available.
    /// </returns>
    /// <remarks>
    /// If this method returns <see langword="false"/>, no graphics screen are rendered into the
    /// presentation target.
    /// </remarks>
    bool BeginRender(RenderContext context);


    /// <summary>
    /// Called by the <see cref="GraphicsManager"/> after rendering into the presentation target.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// This method is always called - even if <see cref="BeginRender"/> returned
    /// <see langword="false"/>.
    /// </remarks>
    void EndRender(RenderContext context);
  }
}
