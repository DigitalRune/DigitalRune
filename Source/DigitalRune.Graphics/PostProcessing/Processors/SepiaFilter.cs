// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Converts a colored image to sepia colors.
  /// </summary>
  public class SepiaFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectPass _fullSepiaPass;
    private readonly EffectPass _partialSepiaPass;


    /// <summary>
    /// Gets or sets the strength of the effect.
    /// </summary>
    /// <value>
    /// The strength factor. If this value is 0.0, the source image is not changed. If this value is 
    /// 1.0, the source image is converted to sepia. If this value is between 0.0 and 1.0, a linear 
    /// interpolation between the source image and the sepia image is returned. The default value is
    /// 1.0.
    /// </value>
    public float Strength { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SepiaFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SepiaFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SepiaFilter");
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _strengthParameter = _effect.Parameters["Strength"];
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _fullSepiaPass = _effect.CurrentTechnique.Passes["Full"];
      _partialSepiaPass = _effect.CurrentTechnique.Passes["Partial"];

      Strength = 1;
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
      if (Numeric.AreEqual(Strength, 1))
      {
        _fullSepiaPass.Apply();
      }
      else
      {
        _strengthParameter.SetValue(Strength);
        _partialSepiaPass.Apply();
      }

      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
  }
}
#endif
