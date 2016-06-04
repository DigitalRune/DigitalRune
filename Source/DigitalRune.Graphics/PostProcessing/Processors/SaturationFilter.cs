// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Changes the saturation of the input texture.
  /// </summary>
  /// <remarks>
  /// This filter assumes that the saturation of the input texture is 1. The saturation of each 
  /// pixel is changed to the given target <see cref="Saturation"/>.
  /// </remarks>
  public class SaturationFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _saturationParameter;


    /// <summary>
    /// Gets or sets the saturation.
    /// </summary>
    /// <value>
    /// The saturation. A value of 0 creates a grayscale image. A value of 1 leaves the source image
    /// unchanged. Values greater than 1 increase the saturation. The default value is 0.
    /// </value>
    public float Saturation { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SaturationFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SaturationFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SaturationFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _saturationParameter = _effect.Parameters["Saturation"];
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
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _saturationParameter.SetValue(Saturation);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
  }
}
#endif
