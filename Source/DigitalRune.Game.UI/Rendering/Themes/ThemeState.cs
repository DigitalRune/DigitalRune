// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents a visual state of a UI control.
  /// </summary>
  /// <remarks>
  /// <para>
  /// In each frame a control is in one visual state (see <see cref="UIControl.VisualState"/>) and 
  /// this state defines the current appearance (e.g. the sprite images that should be drawn). 
  /// </para>
  /// <para>
  /// <strong>Inherited states:</strong> Controls that are nested inside other controls (e.g. a 
  /// <see cref="TextBlock"/> inside a <see cref="Button"/>) can inherit a visual state from its
  /// parent control. For example, a <see cref="TextBlock"/> does not have a "Focused" state. But
  /// the UI theme can define a "Focused" state for the <see cref="TextBlock"/> and set the 
  /// <see cref="IsInherited"/> flag. This state will be used when the visual parent control of the 
  /// <see cref="TextBlock"/> is in its "Focused" state. 
  /// </para>
  /// </remarks>
  public class ThemeState : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the state.
    /// </summary>
    /// <value>The name of the state.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this state is inherited.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this state is inherited; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <see cref="UIControl"/>s have <see cref="UIControl.VisualState"/>s like "Default", 
    /// "Disabled". The possible set of visual states depends on the control type. For example, a 
    /// <see cref="Button"/> has a "Focused" state whereas a <see cref="TextBlock"/> does not have
    /// this visual state. But: If the <see cref="TextBlock"/> is set as the 
    /// <see cref="ContentControl.Content"/> of a <see cref="Button"/>, it can inherit the "Focused"
    /// state from the parent button control. In this case <see cref="IsInherited"/> must be set to 
    /// <see langword="true"/>. The inherited visual state is used, when the control does not have 
    /// the state but is inside a parent control that has the state.
    /// </remarks>
    public bool IsInherited { get; set; }


    /// <summary>
    /// Gets the images that create the appearance of the control.
    /// </summary>
    /// <value>The images.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<ThemeImage> Images { get; private set; }


    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    /// <value>The background color.</value>
    /// <remarks>
    /// If a control is in this state, then the <see cref="UIControl.Background"/> property is set
    /// to this value (unless this value is <see langword="null"/>).
    /// </remarks>
    public Color? Background { get; set; }


    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    /// <value>The foreground color.</value>
    /// <remarks>
    /// If a control is in this state, then the <see cref="UIControl.Foreground"/> property is set
    /// to this value (unless this value is <see langword="null"/>).
    /// </remarks>
    public Color? Foreground { get; set; }


    /// <summary>
    /// Gets or sets the opacity.
    /// </summary>
    /// <value>The opacity.</value>
    /// <remarks>
    /// If a control is in this state, then the <see cref="UIControl.Opacity"/> property is set
    /// to this value (unless this value is <see langword="null"/>).
    /// </remarks>
    public float? Opacity { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeState"/> class.
    /// </summary>
    public ThemeState()
    {
      Images = new List<ThemeImage>();
    }
  }
}
