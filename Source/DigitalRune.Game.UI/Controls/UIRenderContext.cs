// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Game.UI.Rendering;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides information during rendering of UI controls.
  /// </summary>
  public class UIRenderContext
  {
    /// <summary>
    /// Gets or sets the size of the current time step.
    /// </summary>
    /// <value>The size of the current time step.</value>
    public TimeSpan DeltaTime { get; set; }


    /// <summary>
    /// Gets or sets the absolute opacity.
    /// </summary>
    /// <value>The absolute opacity. The default value is 1.0f.</value>
    public float Opacity { get; set; }


    /// <summary>
    /// Gets or sets the absolute render transformation.
    /// </summary>
    /// <value>
    /// The absolute render transformation. The default value is 
    /// <see cref="Rendering.RenderTransform.Identity"/>.
    /// </value>
    public RenderTransform RenderTransform { get; set; }


    /// <summary>
    /// Gets a generic collection of name/value pairs which can be used to store custom data.
    /// </summary>
    /// <value>
    /// A generic collection of name/value pairs which can be used to store custom data.
    /// </value>
    public IDictionary<string, object> Data { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="UIRenderContext"/> class.
    /// </summary>
    public UIRenderContext()
    {
      Opacity = 1.0f;
      RenderTransform = RenderTransform.Identity;
      Data = new Dictionary<string, object>();
    }
  }
}
