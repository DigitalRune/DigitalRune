// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders a batch of texts (positioned in screen space or in world space).
  /// </summary>
  /// <remarks>
  /// A valid <see cref="SpriteBatch"/> and <see cref="SpriteFont"/> must be set; otherwise, 
  /// <see cref="Render"/> will not draw anything.
  /// </remarks>
  internal sealed class TextBatch
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Describes a draw info for a 2D text.
    /// </summary>
    /// <remarks>
    /// Text can be specified as <see cref="string"/> or <see cref="StringBuilder"/>. 
    /// </remarks>
    private struct TextInfo2D
    {
      /// <summary>The text as <see cref="string"/> or <see cref="StringBuilder"/>.</summary>
      public object Text;

      /// <summary>The position in screen space.</summary>
      public Vector2F Position;

      /// <summary>
      /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
      /// <see cref="Position"/>; (1, 1) means that the lower-right corner of the text is at 
      /// <see cref="Position"/>. Use (0.5, 0.5) to center the text.
      /// </summary>
      public Vector2F RelativeOrigin;

      /// <summary>The color.</summary>
      public Color Color;
    }


    /// <summary>
    /// Describes a draw info for a text.
    /// </summary>
    /// <remarks>
    /// Text can be specified as <see cref="string"/> or <see cref="StringBuilder"/>. 
    /// </remarks>
    private struct TextInfo3D
    {
      /// <summary>The text as <see cref="string"/> or <see cref="StringBuilder"/>.</summary>
      public object Text;

      /// <summary>
      /// The position in world space.
      /// </summary>
      public Vector3F Position;

      /// <summary>
      /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
      /// <see cref="Position"/>; (1, 1) means that the lower-right corner of the text is at 
      /// <see cref="Position"/>. Use (0.5, 0.5) to center the text.
      /// </summary>
      public Vector2F RelativeOrigin;

      /// <summary>The color.</summary>
      public Color Color;
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<TextInfo2D> _texts2D = new List<TextInfo2D>();
    private readonly List<TextInfo3D> _texts3D = new List<TextInfo3D>();
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


    /// <summary>
    /// Gets or sets the sprite font.
    /// </summary>
    /// <value>The sprite font.</value>
    /// <remarks>
    /// If this value is <see langword="null"/>, then <see cref="Render"/> does nothing.
    /// </remarks>
    public SpriteFont SpriteFont { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether text should be drawn with enabled depth test.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if depth test is used; otherwise, <see langword="false"/>.
    /// </value>
    public bool EnableDepthTest { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBatch"/> class.
    /// </summary>
    /// <param name="spriteBatch">
    /// The sprite batch. If this value is <see langword="null"/>, then the batch will not draw 
    /// anything when <see cref="Render"/> is called.
    /// </param>
    /// <param name="spriteFont">
    /// The sprite font. If this value is <see langword="null"/>, then the batch will not draw 
    /// anything when <see cref="Render"/> is called.
    /// </param>
    public TextBatch(SpriteBatch spriteBatch, SpriteFont spriteFont)
    {
      SpriteBatch = spriteBatch;
      SpriteFont = spriteFont;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all texts.
    /// </summary>
    public void Clear()
    {
      _texts2D.Clear();
      _texts3D.Clear();
    }


    /// <overloads>
    /// <summary>
    /// Adds text to the <see cref="TextBatch"/> for rendering.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void Add(string text, Vector2F position, Vector2F relativeOrigin, Color color)
    {
      if (string.IsNullOrEmpty(text))
        return;

      _texts2D.Add(new TextInfo2D
      {
        Text = text,
        Position = new Vector2F(position.X, position.Y),
        RelativeOrigin = relativeOrigin,
        Color = color
      });
    }


    /// <summary>
    /// Adds text on a 2D position in screen space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in screen space (measured in pixels).</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void Add(StringBuilder text, Vector2F position, Vector2F relativeOrigin, Color color)
    {
      if (text == null)
        return;

      _texts2D.Add(new TextInfo2D
      {
        Text = text,
        Position = new Vector2F(position.X, position.Y),
        RelativeOrigin = relativeOrigin,
        Color = color
      });
    }


    /// <summary>
    /// Adds text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void Add(string text, Vector3F position, Vector2F relativeOrigin, Color color)
    {
      if (string.IsNullOrEmpty(text))
        return;

      _texts3D.Add(new TextInfo3D
      {
        Text = text,
        Position = position,
        RelativeOrigin = relativeOrigin,
        Color = color
      });
    }


    /// <summary>
    /// Adds text on a 3D position in world space.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="position">The position in world space.</param>
    /// <param name="relativeOrigin">
    /// The relative origin of the text. (0, 0) means that the upper-left corner of the text is at
    /// <paramref name="position"/>; (1, 1) means that the lower-right corner of the text is at 
    /// <paramref name="position"/>. Use (0.5, 0.5) to center the text.
    /// </param>
    /// <param name="color">The color.</param>
    public void Add(StringBuilder text, Vector3F position, Vector2F relativeOrigin, Color color)
    {
      if (text == null)
        return;

      _texts3D.Add(new TextInfo3D
      {
        Text = text,
        Position = position,
        RelativeOrigin = relativeOrigin,
        Color = color
      });
    }


    /// <summary>
    /// Draws the texts.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// If <see cref="SpriteBatch"/> or <see cref="SpriteFont"/> are <see langword="null"/>, then 
    /// <see cref="Render"/> does nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      if (SpriteBatch == null || SpriteFont == null)
        return;

      context.Validate(SpriteBatch);

      if (_texts2D.Count == 0 && _texts3D.Count == 0)
        return;

      if (_texts3D.Count > 0)
        context.ThrowIfCameraMissing();

      var savedRenderState = new RenderStateSnapshot(SpriteBatch.GraphicsDevice);

      if (EnableDepthTest)
      {
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
      }
      else
      {
        SpriteBatch.Begin();
      }

      // ----- Draw world space text.
      if (_texts3D.Count > 0)
      {
        CameraNode cameraNode = context.CameraNode;
        Matrix44F viewProjection = cameraNode.Camera.Projection * cameraNode.View;
        Viewport viewport = SpriteBatch.GraphicsDevice.Viewport;

        foreach (var textInfo in _texts3D)
        {
          // Transform position from world space to the viewport.
          Vector3F pos = viewport.ProjectToViewport(textInfo.Position, viewProjection);
          if (pos.Z < 0 || pos.Z > 1)
            continue;

          // Snap to pixels. Also add a small bias in one direction because when we draw text at
          // certain positions (e.g. view space origin) and the presentation target width is an
          // odd number, the pos will be exactly at pixel centers and due to numerical errors it
          // would jitter between pixels if the camera moves slightly.
          pos.X = (float)Math.Round(pos.X + 0.01f);
          pos.Y = (float)Math.Round(pos.Y + 0.01f);

          var textAsString = textInfo.Text as string;
          if (!string.IsNullOrEmpty(textAsString))
          {
            var textOrigin = GetOrigin(textAsString, textInfo.RelativeOrigin);
            SpriteBatch.DrawString(SpriteFont, textAsString, new Vector2(pos.X, pos.Y), textInfo.Color, 0, textOrigin, 1.0f, SpriteEffects.None, pos.Z);
          }
          else
          {
            var textAsStringBuilder = textInfo.Text as StringBuilder;
            if (textAsStringBuilder != null && textAsStringBuilder.Length > 0)
            {
              var textOrigin = GetOrigin(textAsStringBuilder, textInfo.RelativeOrigin);
              SpriteBatch.DrawString(SpriteFont, textAsStringBuilder, new Vector2(pos.X, pos.Y), textInfo.Color, 0, textOrigin, 1, SpriteEffects.None, pos.Z);
            }
          }
        }
      }

      // ----- Draw screen space text.
      foreach (var textInfo in _texts2D)
      {
        var textAsString = textInfo.Text as string;
        if (!string.IsNullOrEmpty(textAsString))
        {
          var textOrigin = GetOrigin(textAsString, textInfo.RelativeOrigin);
          SpriteBatch.DrawString(SpriteFont, textAsString, (Vector2)textInfo.Position, textInfo.Color, 0, textOrigin, 1, SpriteEffects.None, 0);
        }
        else
        {
          var textAsStringBuilder = textInfo.Text as StringBuilder;
          if (textAsStringBuilder != null && textAsStringBuilder.Length > 0)
          {
            var textOrigin = GetOrigin(textAsStringBuilder, textInfo.RelativeOrigin);
            SpriteBatch.DrawString(SpriteFont, textAsStringBuilder, (Vector2)textInfo.Position, textInfo.Color, 0, textOrigin, 1, SpriteEffects.None, 0);
          }
        }
      }

      SpriteBatch.End();

      savedRenderState.Restore();
    }


    private Vector2 GetOrigin(string text, Vector2F relativeOrigin)
    {
      if (relativeOrigin == Vector2F.Zero)
        return new Vector2();

      Vector2 origin = SpriteFont.MeasureString(text);
      origin.X *= relativeOrigin.X;
      origin.Y *= relativeOrigin.Y;

      // Snap to pixels.
      origin.X = (float)Math.Round(origin.X);
      origin.Y = (float)Math.Round(origin.Y);

      return origin;
    }


    private Vector2 GetOrigin(StringBuilder text, Vector2F relativeOrigin)
    {
      if (relativeOrigin == Vector2F.Zero)
        return new Vector2();

      Vector2 origin = SpriteFont.MeasureString(text);
      origin.X *= relativeOrigin.X;
      origin.Y *= relativeOrigin.Y;

      // Snap to pixels.
      origin.X = (float)Math.Round(origin.X);
      origin.Y = (float)Math.Round(origin.Y);

      return origin;
    }
    #endregion
  }
}
