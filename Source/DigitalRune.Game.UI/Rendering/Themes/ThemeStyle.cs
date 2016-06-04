// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using DigitalRune.Game.UI.Controls;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents a visual style of a UI control.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Styles define the properties and visual appearance of UI controls. Each style has a number of 
  /// visual states (<see cref="States"/>). In each frame a control is in one visual state (see 
  /// <see cref="UIControl.VisualState"/>) and this state defines the current appearance.
  /// </para>
  /// <para>
  /// A style can inherit from another style. For example, if a style "TextBlock" was already
  /// defined in the theme, then a "GreenTextBlock" can inherit from "TextBlock" and only needs to 
  /// set the <see cref="UIControl.Foreground"/> color property to green - all other properties are 
  /// inherited from the parent style.
  /// </para>
  /// </remarks>
  public class ThemeStyle : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    /// <value>The name of the style.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the style from which this style inherits settings.
    /// </summary>
    /// <value>The style from which this style inherits settings.</value>
    public ThemeStyle Inherits { get; set; }


    /// <summary>
    /// Gets the attributes that have been defined for this style.
    /// </summary>
    /// <value>The attributes.</value>
    public NamedObjectCollection<ThemeAttribute> Attributes { get; private set; }


    /// <summary>
    /// Gets the visual states.
    /// </summary>
    /// <value>The visual states.</value>
    /// <remarks>
    /// The <see cref="UIRenderer"/> selects the first matching state from this collection. The
    /// visual states should therefore be sorted by priority in case there are conflicting states.
    /// For example, when a style includes inherited visual states (see 
    /// <see cref="ThemeState.IsInherited"/>) and normal visual states, then the inherited visual 
    /// states need to be the first in the collection. Otherwise, the render will most likely ignore
    /// them.
    /// </remarks>
    public NamedObjectCollection<ThemeState> States { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeStyle"/> class.
    /// </summary>
    public ThemeStyle()
    {
      Attributes = new NamedObjectCollection<ThemeAttribute>();
      States = new NamedObjectCollection<ThemeState>();
    }
  }
}
