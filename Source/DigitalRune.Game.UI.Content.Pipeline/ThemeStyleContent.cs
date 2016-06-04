// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using DigitalRune.Game.UI.Rendering;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents a visual style of a UI control.
  /// </summary>
  public class ThemeStyleContent : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the style.
    /// </summary>
    /// <value>The name of the style.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the name of the parent style from which this style inherits settings.
    /// </summary>
    /// <value>The name of the parent style.</value>
    public string Inherits { get; set; }


    /// <summary>
    /// Gets the attributes that have been defined for this style.
    /// </summary>
    /// <value>The attributes.</value>
    public NamedObjectCollection<ThemeAttributeContent> Attributes { get; private set; }


    /// <summary>
    /// Gets the visual states.
    /// </summary>
    /// <value>The visual states.</value>
    public NamedObjectCollection<ThemeStateContent> States { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeStyle"/> class.
    /// </summary>
    public ThemeStyleContent()
    {
      Attributes = new NamedObjectCollection<ThemeAttributeContent>();
      States = new NamedObjectCollection<ThemeStateContent>();
    }
  }
}
