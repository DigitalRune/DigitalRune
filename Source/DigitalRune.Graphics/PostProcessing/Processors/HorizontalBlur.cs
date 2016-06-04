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
  /// Blurs the input texture using a horizontal blur filter. (Experimental)
  /// </summary>
  /// <remarks>
  /// This horizontal blur is expensive and should be executed on a downsampled (and maybe blurred) 
  /// image.
  /// </remarks>
  internal class HorizontalBlur : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _numberOfSamplesParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _iterationParameter;


    /// <summary>
    /// Gets or sets the number of passes.
    /// </summary>
    /// <value>The number of passes. The default value is 3.</value>
    public int NumberOfPasses { get; set; }


    /// <summary>
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>The number of samples. This should be an even number. The default value is 8.</value>
    public int NumberOfSamples { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="HorizontalBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public HorizontalBlur(IGraphicsService graphicsService) 
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/HorizontalBlur");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _numberOfSamplesParameter = _effect.Parameters["NumberOfSamples"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _iterationParameter = _effect.Parameters["Iteration"];

      NumberOfPasses = 3;
      NumberOfSamples = 8;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      // The target width/height.
      int targetWidth = context.Viewport.Width;
      int targetHeight = context.Viewport.Height;

      var tempFormat = new RenderTargetFormat(targetWidth, targetHeight, false, context.SourceTexture.Format, DepthFormat.None);
      RenderTarget2D temp0 = renderTargetPool.Obtain2D(tempFormat);
      RenderTarget2D temp1 = renderTargetPool.Obtain2D(tempFormat);

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      _viewportSizeParameter.SetValue(new Vector2(targetWidth, targetHeight));
      _numberOfSamplesParameter.SetValue(NumberOfSamples / 2);

      for (int i = 0; i < NumberOfPasses; i++)
      {
        if (i == NumberOfPasses - 1)
        {
          graphicsDevice.SetRenderTarget(context.RenderTarget);
          graphicsDevice.Viewport = context.Viewport;
        }
        else if (i % 2 == 0)
        {
          graphicsDevice.SetRenderTarget(temp0);
        }
        else
        {
          graphicsDevice.SetRenderTarget(temp1);
        }

        if (i == 0)
          _sourceTextureParameter.SetValue(context.SourceTexture);
        else if (i % 2 == 0)
          _sourceTextureParameter.SetValue(temp1);
        else
          _sourceTextureParameter.SetValue(temp0);

        _iterationParameter.SetValue(i);
        _effect.CurrentTechnique.Passes[0].Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      // Clean-up
      _sourceTextureParameter.SetValue((Texture2D)null);

      renderTargetPool.Recycle(temp0);
      renderTargetPool.Recycle(temp1);
    }
  }
}
#endif
