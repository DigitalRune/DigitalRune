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
  /// Copies a texture into a render target.
  /// </summary>
  public class CopyFilter : PostProcessor
  {
    private readonly Effect _effect;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _viewportSizeParameter;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public CopyFilter(IGraphicsService graphicsService) : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/CopyFilter");
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      // Set sampler state. (Floating-point textures cannot use linear filtering. (XNA would throw an exception.))
      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      // Set the render target - but only if no kind of alpha blending is currently set.
      // If alpha-blending is set, then we have to assume that the render target is already
      // set - everything else does not make sense.
      if (graphicsDevice.BlendState.ColorDestinationBlend == Blend.Zero
          && graphicsDevice.BlendState.AlphaDestinationBlend == Blend.Zero)
      {
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        graphicsDevice.Viewport = context.Viewport;
      }

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
  }
}
#endif
