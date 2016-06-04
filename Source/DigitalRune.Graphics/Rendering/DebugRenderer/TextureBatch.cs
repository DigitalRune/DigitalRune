// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders a batch of textures (usually for debugging).
  /// </summary>
  /// <remarks>
  /// A valid <see cref="SpriteBatch"/> must be set; otherwise, <see cref="Render"/> will not draw
  /// any points.
  /// </remarks>
  internal sealed class TextureBatch 
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>Describes a draw info for a texture.</summary>
    private struct TextureInfo
    {
      /// <summary>The texture.</summary>
      public readonly Texture2D Texture;

      /// <summary>The target position and size in screen space.</summary>
      public readonly Rectangle Rectangle;

      /// <summary>
      /// Initializes a new instance of the <see cref="TextureInfo"/> struct.
      /// </summary>
      /// <param name="texture">The texture.</param>
      /// <param name="rectangle">The position rectangle in screen space.</param>
      public TextureInfo(Texture2D texture, Rectangle rectangle)
      {
        Texture = texture;
        Rectangle = rectangle;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<TextureInfo> _textures = new List<TextureInfo>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the sprite batch.
    /// </summary>
    /// <value>The sprite batch.</value>
    /// <remarks>
    /// If this value is <see langword="null"/>, then <see cref="Render"/> does nothing.
    /// </remarks>
    public SpriteBatch SpriteBatch { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBatch"/> class.
    /// </summary>
    /// <param name="spriteBatch">
    /// The sprite batch. If this value is <see langword="null"/>, then the batch will not draw 
    /// anything when <see cref="Render"/> is called.
    /// </param>
    public TextureBatch(SpriteBatch spriteBatch)
    {
      SpriteBatch = spriteBatch;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all textures.
    /// </summary>
    public void Clear()
    {
      _textures.Clear();
    }


    /// <summary>
    /// Adds a texture.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="rectangle">The target position and size in screen space.</param>
    public void Add(Texture2D texture, Rectangle rectangle)
    {
      if (texture != null)
        _textures.Add(new TextureInfo(texture, rectangle));
    }


    /// <summary>
    /// Draws the textures.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// If <see cref="SpriteBatch"/> is <see langword="null"/>, then <see cref="Render"/> does 
    /// nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      if (SpriteBatch == null)
        return;

      var count = _textures.Count;
      if (count == 0)
        return;

      context.Validate(SpriteBatch);

      var savedRenderState = new RenderStateSnapshot(SpriteBatch.GraphicsDevice);

      SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

      for (int i = 0; i < count; i++)
      {
        var textureInfo = _textures[i];

        if (textureInfo.Texture.IsDisposed)
          continue;

        if (TextureHelper.IsFloatingPointFormat(textureInfo.Texture.Format))
        {
          // Floating-point textures must not use linear hardware filtering!
          SpriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
          SpriteBatch.Draw(textureInfo.Texture, textureInfo.Rectangle, Color.White);
          SpriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }
        else
        {
          SpriteBatch.Draw(textureInfo.Texture, textureInfo.Rectangle, Color.White);
        }
      }

      SpriteBatch.End();

      savedRenderState.Restore();
    }
    #endregion
  }
}
