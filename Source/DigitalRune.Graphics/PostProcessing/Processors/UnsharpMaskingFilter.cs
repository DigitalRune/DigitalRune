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
  /// Applies an unsharp masking filter.
  /// </summary>
  /// <remarks>
  /// Unsharp masking blurs the source texture (using <see cref="Blur"/>) and then linearly
  /// interpolates between the blurred texture and the source texture using <see cref="Sharpness"/>
  /// as the interpolation factor. <see cref="Sharpness"/> values greater than 1 (extrapolation), 
  /// create a sharpening effect.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class UnsharpMaskingFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sharpnessParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _blurredTextureParameter;


    /// <summary>
    /// Gets or sets the sharpness factor in the range [0, ∞[.
    /// </summary>
    /// <value>The sharpness factor. The default value 1.2.</value>
    /// <remarks>
    /// If this value is 0, a blurred images is returned. If this value is 1, the original image is 
    /// returned. If this value is greater than 1, a sharpened image is returned.
    /// </remarks>
    public float Sharpness { get; set; }


    /// <summary>
    /// Gets the blur processor.
    /// </summary>
    /// <value>The blur processor.</value>
    public Blur Blur { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="UnsharpMaskingFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public UnsharpMaskingFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/UnsharpMaskingFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sharpnessParameter = _effect.Parameters["Sharpness"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _blurredTextureParameter = _effect.Parameters["BlurredTexture"];

      Sharpness = 1.2f;
      Blur = new Blur(graphicsService);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      if (TextureHelper.IsFloatingPointFormat(source.Format))
      {
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
      }
      else
      {
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
      }

      // Blur source texture.
      var tempFormat = new RenderTargetFormat(source.Width, source.Height, false, source.Format, DepthFormat.None);
      var blurredImage = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
      context.RenderTarget = blurredImage;
      context.Viewport = new Viewport(0, 0, blurredImage.Width, blurredImage.Height);
      Blur.Process(context);

      // Unsharp masking.
      context.RenderTarget = target;
      context.Viewport = viewport;
      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sharpnessParameter.SetValue(Sharpness);
      _sourceTextureParameter.SetValue(source);
      _blurredTextureParameter.SetValue(blurredImage);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Clean-up
      _sourceTextureParameter.SetValue((Texture2D)null);
      _blurredTextureParameter.SetValue((Texture2D)null);
      GraphicsService.RenderTargetPool.Recycle(blurredImage);
    }
  }
}
#endif
