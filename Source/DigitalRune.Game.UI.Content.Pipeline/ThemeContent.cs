// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Xml.Linq;
using DigitalRune.Collections;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents a UI theme that defines the properties and visual appearance of UI controls.
  /// </summary>
  public class ThemeContent : ContentItem
  {
    /// <summary>
    /// Gets the theme description (XML file).
    /// </summary>
    /// <value>The theme description (XML file).</value>
    public XDocument Description { get; private set; }


    /// <summary>
    /// Gets the cursor definitions.
    /// </summary>
    /// <value>The cursors.</value>
    public NamedObjectCollection<ThemeCursorContent> Cursors { get; set; }


    /// <summary>
    /// Gets or sets the fonts definitions.
    /// </summary>
    /// <value>The fonts.</value>
    public NamedObjectCollection<ThemeFontContent> Fonts { get; set; }


    /// <summary>
    /// Gets or sets the textures.
    /// </summary>
    /// <value>The textures.</value>
    public NamedObjectCollection<ThemeTextureContent> Textures { get; set; }


    /// <summary>
    /// Gets or sets the styles of the controls.
    /// </summary>
    /// <value>The styles.</value>
    public NamedObjectCollection<ThemeStyleContent> Styles { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeContent"/> class.
    /// </summary>
    /// <param name="identity">The identity of the content item.</param>
    /// <param name="description">The theme description (XML file).</param>
    public ThemeContent(ContentIdentity identity, XDocument description)
    {
      Identity = identity;
      Description = description;
    }
  }
}
