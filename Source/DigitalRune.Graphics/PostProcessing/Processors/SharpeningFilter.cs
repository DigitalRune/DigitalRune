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
  /// Applies a sharpening effect using edge detection with the Laplacian operator.
  /// </summary>
  public class SharpeningFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _sharpnessParameter;


    /// <summary>
    /// Gets or sets the sharpness factor in the range [0, ∞[.
    /// </summary>
    /// <value>The sharpness factor in the range [0, ∞[. The default value is 0.5.</value>
    /// <remarks>
    /// If this value is 0, the original image is returned. If this value is greater than 0, a 
    /// sharpened image is returned.
    /// </remarks>
    public float Sharpness { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SharpeningFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SharpeningFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SharpeningFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _sharpnessParameter = _effect.Parameters["Sharpness"];

      Sharpness = 0.5f;
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
      _sharpnessParameter.SetValue(Sharpness);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
  }
}
#endif
