// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents a visual state of a UI control.
  /// </summary>
  public class ThemeStateContent : INamedObject
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
    public bool IsInherited { get; set; }


    /// <summary>
    /// Gets the images that create the appearance of the control.
    /// </summary>
    /// <value>The images.</value>
    public List<ThemeImageContent> Images { get; private set; }


    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    /// <value>The background color.</value>
    public Color? Background { get; set; }


    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    /// <value>The foreground color.</value>
    public Color? Foreground { get; set; }


    /// <summary>
    /// Gets or sets the opacity.
    /// </summary>
    /// <value>The opacity.</value>
    public float? Opacity { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeStateContent"/> class.
    /// </summary>
    public ThemeStateContent()
    {
      Images = new List<ThemeImageContent>();
    }
  }
}
