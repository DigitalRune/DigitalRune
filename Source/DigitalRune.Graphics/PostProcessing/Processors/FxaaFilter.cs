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
  /// Applies <i>Fast Approximate Anti-Aliasing</i> (FXAA).
  /// </summary>
  /// <remarks>
  /// FXAA should be applied to a texture in sRGB space (= gamma-corrected, non-linear color 
  /// values). FXAA detects edges based on the luminance. If the source texture already contains the
  /// image luminance in the alpha channel, then you can set <see cref="ComputeLuminance"/> to 
  /// <see langword="false"/>. Per default, the luminance is computed in a separate pass.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class FxaaFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectPass _luminanceToAlphaPass;
    private readonly EffectPass _fxaaPass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether the luminance should be computed
    /// in a separate pass.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the luminance should be computed; otherwise, 
    /// <see langword="false"/> if the source texture already contains the luminance
    /// in the alpha channel of the image. The default value is <see langword="true"/>.
    /// </value>
    public bool ComputeLuminance { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FxaaFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public FxaaFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/FxaaFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _luminanceToAlphaPass = _effect.CurrentTechnique.Passes["LuminanceToAlpha"];
      _fxaaPass = _effect.CurrentTechnique.Passes["Fxaa"];

      ComputeLuminance = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      RenderTarget2D rgbLuma = null;
      if (ComputeLuminance)
      {
        var rgbLumaFormat = new RenderTargetFormat(
          context.SourceTexture.Width,
          context.SourceTexture.Height,
          false,
          context.SourceTexture.Format,
          DepthFormat.None);
        rgbLuma = renderTargetPool.Obtain2D(rgbLumaFormat);

        graphicsDevice.SetRenderTarget(rgbLuma);
        _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        _sourceTextureParameter.SetValue(context.SourceTexture);
        _luminanceToAlphaPass.Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(ComputeLuminance ? rgbLuma : context.SourceTexture);
      _fxaaPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
      renderTargetPool.Recycle(rgbLuma);
    }
    #endregion
  }
}
#endif
