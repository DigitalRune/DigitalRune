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
  /// Blurs the input texture using the Kawase bloom filter.
  /// </summary>
  /// <remarks>
  /// Kawase bloom filter blurs the image using several fullscreen passes. 
  /// <see cref="NumberOfPasses"/> is 8 per default.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class KawaseBlur : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _iterationParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _useHalfPixelOffsetParameter;
    private readonly EffectParameter _viewportSizeParameter;


    /// <summary>
    /// Gets or sets the number of passes.
    /// </summary>
    /// <value>The number of passes. The default value is 8.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is 0 or negative.
    /// </exception>
    public int NumberOfPasses
    {
      get { return _numberOfPasses; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The number of passes must be greater than 0.");

        _numberOfPasses = value;
      }
    }
    private int _numberOfPasses;


    /// <summary>
    /// Initializes a new instance of the <see cref="KawaseBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public KawaseBlur(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/KawaseBlur");
      _iterationParameter = _effect.Parameters["Iteration"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _useHalfPixelOffsetParameter = _effect.Parameters["UseHalfPixelOffset"];
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];

      NumberOfPasses = 8;
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

      // We use two temporary render targets.
      var tempFormat = new RenderTargetFormat(targetWidth, targetHeight, false, context.SourceTexture.Format, DepthFormat.None);
      RenderTarget2D temp0 = (NumberOfPasses > 1)
                             ? renderTargetPool.Obtain2D(tempFormat)
                             : null;
      RenderTarget2D temp1 = (NumberOfPasses > 2)
                             ? renderTargetPool.Obtain2D(tempFormat)
                             : null;

      bool useHalfPixelOffset = !TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format);
      if (useHalfPixelOffset)
      {
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        _useHalfPixelOffsetParameter.SetValue(1.0f);
      }
      else
      {
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        _useHalfPixelOffsetParameter.SetValue(0.0f);
      }

      _viewportSizeParameter.SetValue(new Vector2(targetWidth, targetHeight));

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

        // The iteration value goes from 0 ... (n - 1) or 1 ... n depending on 
        // whether a half-pixel offset is used.
        _iterationParameter.SetValue((float)(useHalfPixelOffset ? i : i + 1));

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
