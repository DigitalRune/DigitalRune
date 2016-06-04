// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Reduces the resolution of an input texture.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This post-processor reduces the resolution of the <see cref="RenderContext.SourceTexture"/>
  /// to match the target <see cref="RenderContext.Viewport"/>. 
  /// </para>
  /// <para>
  /// If this post-processor is used in a <see cref="PostProcessorChain"/>, you can use the 
  /// property <see cref="PostProcessor.DefaultTargetFormat"/> to specify the target resolution.
  /// </para>
  /// <para>
  /// Render targets are downsampled by averaging samples. However, some render targets might
  /// require a different downsample function. The <see cref="DownsampleFilter"/> will
  /// check if the source texture is the depth buffer (<see cref="RenderContext.GBuffer0"/>), 
  /// and when this is the case, it will perform a special downsampling.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DownsampleFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _sourceSizeParameter;
    private readonly EffectParameter _targetSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectPass _linear2Pass;
    private readonly EffectPass _linear4Pass;
    private readonly EffectPass _linear6Pass;
    private readonly EffectPass _linear8Pass;
    private readonly EffectPass _point2Pass;
    private readonly EffectPass _point3Pass;
    private readonly EffectPass _point4Pass;
    private readonly EffectPass _point2DepthPass;
    private readonly EffectPass _point3DepthPass;
    private readonly EffectPass _point4DepthPass;



    /// <summary>
    /// Initializes a new instance of the <see cref="DownsampleFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public DownsampleFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/DownsampleFilter");
      _sourceSizeParameter = _effect.Parameters["SourceSize"];
      _targetSizeParameter = _effect.Parameters["TargetSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _linear2Pass = _effect.CurrentTechnique.Passes["Linear2"];
      _linear4Pass = _effect.CurrentTechnique.Passes["Linear4"];
      _linear6Pass = _effect.CurrentTechnique.Passes["Linear6"];
      _linear8Pass = _effect.CurrentTechnique.Passes["Linear8"];
      _point2Pass = _effect.CurrentTechnique.Passes["Point2"];
      _point3Pass = _effect.CurrentTechnique.Passes["Point3"];
      _point4Pass = _effect.CurrentTechnique.Passes["Point4"];
      _point2DepthPass = _effect.CurrentTechnique.Passes["Point2Depth"];
      _point3DepthPass = _effect.CurrentTechnique.Passes["Point3Depth"];
      _point4DepthPass = _effect.CurrentTechnique.Passes["Point4Depth"];
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      // The width/height of the current input.
      int sourceWidth = context.SourceTexture.Width;
      int sourceHeight = context.SourceTexture.Height;

      // The target width/height.
      int targetWidth = context.Viewport.Width;
      int targetHeight = context.Viewport.Height;

      // Surface format of input.
      bool isFloatingPointFormat = TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format);

      // Floating-point formats cannot use linear filtering, so we need two different paths.
      RenderTarget2D last = null;
      if (!isFloatingPointFormat)
      {
        // ----- We can use bilinear hardware filtering.
        do
        {
          // Determine downsample factor. Use the largest possible factor to minimize passes.
          int factor;
          if (sourceWidth / 2 <= targetWidth && sourceHeight / 2 <= targetHeight)
            factor = 2;
          else if (sourceWidth / 4 <= targetWidth && sourceHeight / 4 <= targetHeight)
            factor = 4;
          else if (sourceWidth / 6 <= targetWidth && sourceHeight / 6 <= targetHeight)
            factor = 6;
          else
            factor = 8;

          // Downsample to this target size.
          int tempTargetWidth = Math.Max(targetWidth, sourceWidth / factor);
          int tempTargetHeight = Math.Max(targetHeight, sourceHeight / factor);

          // Is this the final pass that renders into context.RenderTarget?
          bool isFinalPass = (tempTargetWidth <= targetWidth && tempTargetHeight <= targetHeight);
          RenderTarget2D temp = null;
          if (isFinalPass)
          {
            graphicsDevice.SetRenderTarget(context.RenderTarget);
            graphicsDevice.Viewport = context.Viewport;
          }
          else
          {
            // Get temporary render target for intermediate steps.
            var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
            temp = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
            graphicsDevice.SetRenderTarget(temp);
          }

          _sourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
          _targetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
          _sourceTextureParameter.SetValue(last ?? context.SourceTexture);
          if (factor == 2)
            _linear2Pass.Apply();
          else if (factor == 4)
            _linear4Pass.Apply();
          else if (factor == 6)
            _linear6Pass.Apply();
          else if (factor == 8)
            _linear8Pass.Apply();

          graphicsDevice.DrawFullScreenQuad();

          GraphicsService.RenderTargetPool.Recycle(last);
          last = temp;
          sourceWidth = tempTargetWidth;
          sourceHeight = tempTargetHeight;
        } while (sourceWidth > targetWidth || sourceHeight > targetHeight);
      }
      else
      {
        // ----- We cannot use hardware filtering. :-(
        do
        {
          // Determine downsample factor. Use the largest possible factor to minimize passes.
          int factor;
          if (sourceWidth / 2 <= targetWidth && sourceHeight / 2 <= targetHeight)
            factor = 2;
          else if (sourceWidth / 3 <= targetWidth && sourceHeight / 3 <= targetHeight)
            factor = 3;
          else
            factor = 4;

          // Downsample to this target size.
          int tempTargetWidth = Math.Max(targetWidth, sourceWidth / factor);
          int tempTargetHeight = Math.Max(targetHeight, sourceHeight / factor);

          // Is this the final pass that renders into context.RenderTarget?
          bool isFinalPass = (tempTargetWidth <= targetWidth && tempTargetHeight <= targetHeight);
          RenderTarget2D temp = null;
          if (isFinalPass)
          {
            graphicsDevice.SetRenderTarget(context.RenderTarget);
            graphicsDevice.Viewport = context.Viewport;
          }
          else
          {
            // Get temporary render target for intermediate steps.
            var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
            temp = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
            graphicsDevice.SetRenderTarget(temp);
          }

          _sourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
          _targetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
          var source = last ?? context.SourceTexture;
          _sourceTextureParameter.SetValue(source);
          if (source != context.GBuffer0)
          {
            if (factor == 2)
              _point2Pass.Apply();
            else if (factor == 3)
              _point3Pass.Apply();
            else
              _point4Pass.Apply();
          }
          else
          {
            // This is the depth buffer and it needs special handling.
            if (factor == 2)
              _point2DepthPass.Apply();
            else if (factor == 3)
              _point3DepthPass.Apply();
            else
              _point4DepthPass.Apply();
          }

          graphicsDevice.DrawFullScreenQuad();

          GraphicsService.RenderTargetPool.Recycle(last);
          last = temp;
          sourceWidth = tempTargetWidth;
          sourceHeight = tempTargetHeight;
        } while (sourceWidth > targetWidth || sourceHeight > targetHeight);

        _sourceTextureParameter.SetValue((Texture2D)null);

        Debug.Assert(last == null, "Intermediate render target should have been recycled.");
      }
    }
  }
}
#endif
