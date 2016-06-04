// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Manages and renders the visual appearance of a UI.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A renderer draws a control when the <see cref="Render"/> method is called. Controls are
  /// usually rendered by copying images from a texture atlas to the screen - but different 
  /// renderers can use different methods. The renderer has a <see cref="SpriteBatch"/> that is used
  /// to render the images. <see cref="BeginBatch"/> can be called to start a batch with the
  /// default settings of the renderer. <see cref="BeginBatch"/> is normally automatically called
  /// when a <see cref="UIScreen"/> is rendered and the batch is finished with 
  /// <see cref="EndBatch"/> when the whole screen was rendered. This way all controls are rendered
  /// as a single batch. If a control must be rendered with different render states or a different
  /// sprite batch, <see cref="EndBatch"/> must be called to flush the current batch.
  /// </para>
  /// </remarks>
  public interface IUIRenderer 
  {
    /// <summary>
    /// Gets the graphics device.
    /// </summary>
    /// <value>The graphics device.</value>
    GraphicsDevice GraphicsDevice { get; }

    
    /// <summary>
    /// Gets the sprite batch that is used to draw all images for the UI controls.
    /// </summary>
    /// <value>The sprite batch that is used to draw all images for the UI controls.</value>
    SpriteBatch SpriteBatch { get; }

    
    /// <summary>
    /// Gets the UI control templates that define the game object properties for the different 
    /// styles.
    /// </summary>
    /// <value>
    /// The UI control templates that define the game object properties for the different styles.
    /// </value>
    /// <remarks>
    /// A renderer defines the style of controls. When controls are loaded, they use
    /// <see cref="GetAttribute{T}"/> to initialize their properties. The <see cref="UIControl"/>
    /// creates a template game object for each style and caches the template in this dictionary.
    /// </remarks>
    Dictionary<string, GameObject> Templates { get; }


    /// <summary>
    /// Gets a style-specific attribute value.
    /// </summary>
    /// <typeparam name="T">The type of the attribute value.</typeparam>
    /// <param name="style">The style.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="result">The attribute value.</param>
    /// <returns>
    /// <see langword="true"/> if the renderer can provide a value for the attribute; otherwise, 
    /// <see langword="false"/> if the renderer does not know the style or the attribute.
    /// </returns>
    /// <remarks>
    /// The renderer can define the visual style of the <see cref="UIControl"/>s. When a control is 
    /// loaded, it initializes its properties with values provided by this method.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
    bool GetAttribute<T>(string style, string name, out T result);


    /// <summary>
    /// Gets a mouse cursor.
    /// </summary>
    /// <param name="name">
    /// The name of the mouse cursor. This name is any string - usually the asset name - that 
    /// identifies the cursor, e.g. "Wait". Can be <see langword="null"/> to get the default 
    /// cursor.
    /// </param>
    /// <returns>The mouse cursor.</returns>
    /// <remarks>
    /// This object must be of type <strong>System.Windows.Forms.Cursor</strong>. (The type 
    /// <see cref="System.Object"/> is used to avoid referencing 
    /// <strong>System.Windows.Forms.dll</strong> in this portable library.)
    /// </remarks>
    object GetCursor(string name);


    /// <summary>
    /// Gets a sprite font.
    /// </summary>
    /// <param name="name">
    /// The name of the font. This name is any string - usually the asset name - that identifies the 
    /// font, e.g. "Console". Can be <see langword="null"/> to get the default font.
    /// </param>
    /// <returns>The font.</returns>
    SpriteFont GetFont(string name);


    /// <summary>
    /// Gets a texture.
    /// </summary>
    /// <param name="name">
    /// The name of the texture. This name is any string - usually the asset name - that identifies 
    /// the texture, e.g. "UITexture". Can be <see langword="null"/> to get the default texture.
    /// </param>
    /// <returns>
    /// The font.
    /// </returns>
    Texture2D GetTexture(string name);


    /// <summary>
    /// Calls the <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch.Begin"/> method of the
    /// <see cref="SpriteBatch"/> with default settings.
    /// </summary>
    /// <remarks>
    /// This method remembers if it was already called. Redundant calls of this method are safe.
    /// </remarks>
    void BeginBatch();


    /// <summary>
    /// Renders the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    /// <param name="context">The render context.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    void Render(UIControl control, UIRenderContext context);

    
    /// <summary>
    /// Calls <see cref="Microsoft.Xna.Framework.Graphics.SpriteBatch.End"/> method of the
    /// <see cref="SpriteBatch"/> to commit the current batch.
    /// </summary>
    void EndBatch();
  }
}
