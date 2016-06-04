// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides information during processing of device input.
  /// </summary>
  public class InputContext
  {
    /// <summary>
    /// Gets or sets the size of the current time step.
    /// </summary>
    /// <value>The size of the current time step.</value>
    public TimeSpan DeltaTime { get; set; }


    /// <summary>
    /// Gets or sets the absolute mouse position in screen coordinates.
    /// </summary>
    /// <value>The absolute mouse position in screen coordinates.</value>
    public Vector2F ScreenMousePosition { get; set; }


    /// <summary>
    /// Gets or sets the mouse position change since the last frame in screen coordinates.
    /// </summary>
    /// <value>The mouse position change since the last frame in screen coordinates.</value>
    public Vector2F ScreenMousePositionDelta { get; set; }


    /// <summary>
    /// Gets or sets the mouse position in local coordinates (after the 
    /// <see cref="UIControl.RenderTransform"/> was undone).
    /// </summary>
    /// <value>
    /// The mouse position in local coordinates (after the <see cref="UIControl.RenderTransform"/> 
    /// was undone).
    /// </value>
    public Vector2F MousePosition { get; set; }


    /// <summary>
    /// Gets or sets the mouse position change since the last frame in local coordinates (after the 
    /// <see cref="UIControl.RenderTransform"/> was undone).
    /// </summary>
    /// <value>
    /// The mouse position change since the last frame in local coordinates (after the 
    /// <see cref="UIControl.RenderTransform"/> was undone).
    /// </value>
    public Vector2F MousePositionDelta { get; set; }


    /// <summary>
    /// Gets or sets the <see cref="LogicalPlayerIndex"/> of the player from which input is 
    /// accepted.
    /// </summary>
    /// <value>
    /// The <see cref="LogicalPlayerIndex"/> of the player from which input is accepted.
    /// </value>
    public LogicalPlayerIndex AllowedPlayer { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the mouse is over the current control.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the mouse is over the current control; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this flag is <see langword="true"/>, the <see cref="UIControl.HitTest"/> for the current
    /// control should still be made. But parent controls can set this flag to 
    /// <see langword="false"/> if they do not want the current control to handle mouse-over related
    /// actions or if they apply a clipping to the position of the child and the mouse is in the
    /// clipped area.
    /// </remarks>
    public bool IsMouseOver { get; set; }


    /// <summary>
    /// Gets a generic collection of name/value pairs which can be used to store custom data.
    /// </summary>
    /// <value>
    /// A generic collection of name/value pairs which can be used to store custom data.
    /// </value>
    public IDictionary<string, object> Data { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputContext"/> class.
    /// </summary>
    public InputContext()
    {
      Data = new Dictionary<string, object>();
    }
  }
}
