// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Scales the color of an image and adds an offset. (MAD = Multiply/Add).
  /// </summary>
  public class MadFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _scaleParameter;
    private readonly EffectParameter _offsetParameter;
    private readonly EffectParameter _textureParameter;


    /// <summary>
    /// Gets or sets the scale factor.
    /// </summary>
    /// <value>The scale factor. The default is (1, 1, 1).</value>
    public Vector3F Scale { get; set; }


    /// <summary>
    /// Gets or sets the offset.
    /// </summary>
    /// <value>The offset. The default value is (0, 0, 0).</value>
    public Vector3F Offset { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="MadFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public MadFilter(IGraphicsService graphicsService) : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/MadFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _scaleParameter = _effect.Parameters["Scale"];
      _offsetParameter = _effect.Parameters["Offset"];
      _textureParameter = _effect.Parameters["SourceTexture"];

      Scale = new Vector3F(1, 1, 1);
      Offset = new Vector3F(0, 0, 0);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _scaleParameter.SetValue((Vector3)Scale);
      _offsetParameter.SetValue((Vector3)Offset);
      _textureParameter.SetValue(context.SourceTexture);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _textureParameter.SetValue((Texture2D)null);
    }
  }
}
#endif
