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
  /// Renders <see cref="SpriteNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class SpriteRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SpriteRenderer(IGraphicsService graphicsService)
      : this(graphicsService, (SpriteFont)null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteBatch">
    /// The sprite batch used for rendering. Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    [Obsolete("It is no longer necessary to specify a SpriteBatch.")]
    public SpriteRenderer(IGraphicsService graphicsService, SpriteBatch spriteBatch)
      : this(graphicsService, (SpriteFont)null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteFont">
    /// The default font, which is used in case the font of a <see cref="TextSprite"/> is not set.
    /// Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SpriteRenderer(IGraphicsService graphicsService, SpriteFont spriteFont)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Order = 6;
      _spriteBatch = graphicsService.GetSpriteBatch();
      _spriteFont = spriteFont;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="spriteBatch">
    /// The sprite batch used for rendering. Can be <see langword="null"/>.
    /// </param>
    /// <param name="spriteFont">
    /// The default font, which is used in case the font of a <see cref="TextSprite"/> is not set.
    /// Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    [Obsolete("It is no longer necessary to specify a SpriteBatch.")]
    public SpriteRenderer(IGraphicsService graphicsService, SpriteBatch spriteBatch, SpriteFont spriteFont)
      : this(graphicsService, spriteFont)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is SpriteNode;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.Validate(_spriteBatch);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      // Camera properties
      var cameraNode = context.CameraNode;
      Matrix44F viewProjection = cameraNode.Camera.Projection * cameraNode.View;
      var viewport = graphicsDevice.Viewport;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      SpriteSortMode sortMode;
      switch (order)
      {
        case RenderOrder.Default:
          sortMode = SpriteSortMode.Texture;
          break;
        case RenderOrder.FrontToBack:
          sortMode = SpriteSortMode.FrontToBack;
          break;
        case RenderOrder.BackToFront:
          sortMode = SpriteSortMode.BackToFront;
          break;
        case RenderOrder.UserDefined:
        default:
          sortMode = SpriteSortMode.Deferred;
          break;
      }

      _spriteBatch.Begin(sortMode, graphicsDevice.BlendState, null, graphicsDevice.DepthStencilState, null);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as SpriteNode;
        if (node == null)
          continue;

        // SpriteNode is visible in current frame.
        node.LastFrame = frame;

        // Position, size, and origin in pixels.
        Vector3F position = new Vector3F();
        Vector2 size = new Vector2();
        Vector2 origin = new Vector2();

        var bitmapSprite = node.Sprite as ImageSprite;
        if (bitmapSprite != null)
        {
          var packedTexture = bitmapSprite.Texture;
          if (packedTexture != null)
          {
            // Project into viewport and snap to pixels.
            position = viewport.ProjectToViewport(node.PoseWorld.Position, viewProjection);
            position.X = (int)(position.X + 0.5f);
            position.Y = (int)(position.Y + 0.5f);

            // Get source rectangle (pixel bounds).
            var sourceRectangle = packedTexture.GetBounds(node.AnimationTime);
            size = new Vector2(sourceRectangle.Width, sourceRectangle.Height);

            // Premultiply color.
            Vector3F color3F = node.Color;
            float alpha = node.Alpha;
            Color color = new Color(color3F.X * alpha, color3F.Y * alpha, color3F.Z * alpha, alpha);

            // Get absolute origin (relative to pixel bounds).
            origin = (Vector2)node.Origin * size;

            // Draw using SpriteBatch.
            _spriteBatch.Draw(
              packedTexture.TextureAtlas, new Vector2(position.X, position.Y), sourceRectangle,
              color, node.Rotation, origin, (Vector2)node.Scale, SpriteEffects.None, position.Z);
          }
        }
        else
        {
          var textSprite = node.Sprite as TextSprite;
          if (textSprite != null)
          {
            var font = textSprite.Font ?? _spriteFont;
            if (font != null)
            {
              // Text can be a string or StringBuilder.
              var text = textSprite.Text as string;
              if (text != null)
              {
                if (text.Length > 0)
                {
                  // Project into viewport and snap to pixels.
                  position = viewport.ProjectToViewport(node.PoseWorld.Position, viewProjection);
                  position.X = (int)(position.X + 0.5f);
                  position.Y = (int)(position.Y + 0.5f);

                  // Premultiply color.
                  Vector3F color3F = node.Color;
                  float alpha = node.Alpha;
                  Color color = new Color(color3F.X * alpha, color3F.Y * alpha, color3F.Z * alpha, alpha);

                  // Get absolute origin (relative to pixel bounds).
                  size = font.MeasureString(text);
                  origin = (Vector2)node.Origin * size;

                  // Draw using SpriteBatch.
                  _spriteBatch.DrawString(
                    font, text, new Vector2(position.X, position.Y),
                    color, node.Rotation, origin, (Vector2)node.Scale,
                    SpriteEffects.None, position.Z);
                }
              }
              else
              {
                var stringBuilder = textSprite.Text as StringBuilder;
                if (stringBuilder != null && stringBuilder.Length > 0)
                {
                  // Project into viewport and snap to pixels.
                  position = viewport.ProjectToViewport(node.PoseWorld.Position, viewProjection);
                  position.X = (int)(position.X + 0.5f);
                  position.Y = (int)(position.Y + 0.5f);

                  // Premultiply color.
                  Vector3F color3F = node.Color;
                  float alpha = node.Alpha;
                  Color color = new Color(color3F.X * alpha, color3F.Y * alpha, color3F.Z * alpha, alpha);

                  // Get absolute origin (relative to pixel bounds).
                  size = font.MeasureString(stringBuilder);
                  origin = (Vector2)node.Origin * size;

                  // Draw using SpriteBatch.
                  _spriteBatch.DrawString(
                    font, stringBuilder, new Vector2(position.X, position.Y),
                    color, node.Rotation, origin, (Vector2)node.Scale,
                    SpriteEffects.None, position.Z);
                }
              }
            }
          }
        }

        // Store bounds an depth for hit tests.
        node.LastBounds = new Rectangle(
          (int)(position.X - origin.X),
          (int)(position.Y - origin.Y),
          (int)(size.X * node.Scale.X),
          (int)(size.Y * node.Scale.Y));

        node.LastDepth = position.Z;
      }

      _spriteBatch.End();
      savedRenderState.Restore();
    }
    #endregion
  }
}
