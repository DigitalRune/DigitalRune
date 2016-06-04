// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents a UI theme that defines the properties and visual appearance of UI controls.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Theme"/> can be built at runtime, but usually a theme is defined in an XML files 
  /// that is processed in the XNA Content Pipeline and loaded via the XNA ContentManager.
  /// </para>
  /// <para>
  /// The XML file specifies mouse cursors (only used in Windows), fonts, a texture and styles. The 
  /// texture is a texture atlas containing all images that are necessary to render the controls. 
  /// The styles define property values and the visual appearance of UI controls.
  /// </para>
  /// <para>
  /// <strong>XML Format:</strong>
  /// See the example themes to learn how the XML file is structured. Colors, rectangles and 4D 
  /// vectors are specified using 4 float values separated with commas, semicolons and/or spaces, 
  /// for example "100,200,300,400". (Colors are defined as <i>"red,green,blue,alpha"</i>, 
  /// rectangles are defined as <i>"x,y,width,height"</i>, borders/margins/paddings are defined as 
  /// "left,top,right,bottom".)
  /// </para>
  /// <para>
  /// <strong>Processing and Loading:</strong>
  /// To load a theme add the XML file to an XNA Content Project. The content project needs to 
  /// reference the following DigitalRune content pipeline assemblies: 
  /// "DigitalRune.Mathematics.Content.Pipeline.dll" and DigitalRune.Game.UI.Content.Pipeline.dll".
  /// Once the assembly references are added, set the <strong>Content Importer</strong> and the 
  /// <strong>Content Processor</strong> of the XML file to "UI Theme - DigitalRune". The theme and 
  /// all related files are then automatically built together with the content project.
  /// </para>
  /// <para>
  /// At runtime the theme can be loaded using the game's ContentManager.
  /// <code lang="csharp">
  /// <![CDATA[
  /// Theme theme = Content.Load<Theme>("BlendBlue");
  /// ]]>
  /// </code>
  /// </para>
  /// </remarks>
  public class Theme
  {
    /// <summary>
    /// Gets or sets the content manager.
    /// </summary>
    /// <value>The content manager.</value>
    public ContentManager Content { get; set; }


    /// <summary>
    /// Gets the cursor definitions.
    /// </summary>
    /// <value>The cursors.</value>
    public NamedObjectCollection<ThemeCursor> Cursors { get; private set; }


    /// <summary>
    /// Gets the fonts definitions.
    /// </summary>
    /// <value>The fonts.</value>
    public NamedObjectCollection<ThemeFont> Fonts { get; private set; }


    /// <summary>
    /// Gets the textures.
    /// </summary>
    /// <value>The textures.</value>
    public NamedObjectCollection<ThemeTexture> Textures { get; private set; }


    /// <summary>
    /// Gets the styles of the controls.
    /// </summary>
    /// <value>The styles.</value>
    public NamedObjectCollection<ThemeStyle> Styles { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    public Theme()
    {
      Cursors = new NamedObjectCollection<ThemeCursor>();
      Fonts = new NamedObjectCollection<ThemeFont>();
      Textures = new NamedObjectCollection<ThemeTexture>();
      Styles = new NamedObjectCollection<ThemeStyle>();
    }
  }
}
