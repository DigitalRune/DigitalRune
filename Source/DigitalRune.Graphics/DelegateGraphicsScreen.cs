// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// A <see cref="GraphicsScreen"/> that calls user-defined methods to update and render the 
  /// screen.
  /// </summary>
  public class DelegateGraphicsScreen : GraphicsScreen
  {
    /// <summary>
    /// Gets or sets the update callback method.
    /// </summary>
    /// <value>The update callback method. (Can be <see langword="null"/>.)</value>
    /// <remarks>
    /// This method is called by the <see cref="DelegateGraphicsScreen"/> when 
    /// <see cref="OnUpdate"/> is called. The second parameter is the elapsed time since the last 
    /// frame.
    /// </remarks>
    public Action<GraphicsScreen, TimeSpan> UpdateCallback { get; set; }


    /// <summary>
    /// Gets or sets the render callback method.
    /// </summary>
    /// <value>The render callback method. (Can be <see langword="null"/>.)</value>
    /// <remarks>
    /// This method is called by the <see cref="DelegateGraphicsScreen"/> when 
    /// <see cref="OnRender"/> is called.
    /// </remarks>
    public Action<RenderContext> RenderCallback { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateGraphicsScreen"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    public DelegateGraphicsScreen(IGraphicsService graphicsService) 
      : this(graphicsService, null, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateGraphicsScreen"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="updateCallback">
    /// The update callback method. (Can be <see langword="null"/>.)
    /// </param>
    /// <param name="renderCallback">
    /// The render callback method. (Can be <see langword="null"/>.)
    /// </param>
    public DelegateGraphicsScreen(IGraphicsService graphicsService, 
                                  Action<GraphicsScreen, TimeSpan> updateCallback,
                                  Action<RenderContext> renderCallback)
      : base(graphicsService)
    {
      UpdateCallback = updateCallback;
      RenderCallback = renderCallback;
    }


    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (UpdateCallback != null)
        UpdateCallback(this, deltaTime);
    }


    /// <inheritdoc/>
    protected override void OnRender(RenderContext context)
    {
      if (RenderCallback != null)
        RenderCallback(context);
    }
  }
}
