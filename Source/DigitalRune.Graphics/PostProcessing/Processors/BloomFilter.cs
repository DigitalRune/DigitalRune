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
  /// Adds a bloom effect to an LDR (low dynamic range) image.
  /// </summary>
  /// <remarks>
  /// Bloom is also called "glare".
  /// </remarks>
  public class BloomFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _bloomThresholdParameter;
    private readonly EffectParameter _bloomIntensityParameter;
    private readonly EffectParameter _bloomSaturationParameter;
    private readonly EffectParameter _sceneTextureParameter;
    private readonly EffectParameter _bloomTextureParameter;
    private readonly EffectPass _brightnessPass;
    private readonly EffectPass _combinePass;

    private readonly Blur _blur;
    private readonly DownsampleFilter _downsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bloom intensity factor.
    /// </summary>
    /// <value>The bloom intensity factor.</value>
    public float Intensity { get; set; }


    /// <summary>
    /// Gets or sets the bloom saturation.
    /// </summary>
    /// <value>The bloom saturation.</value>
    /// <remarks>
    /// The saturation of the bloom effect is controlled with this property. Use values less than 1 
    /// to decrease saturation. Use values greater than 1 to increase saturation.
    /// </remarks>
    public float Saturation { get; set; }


    /// <summary>
    /// Gets or sets the brightness threshold.
    /// </summary>
    /// <value>The brightness threshold.</value>
    /// <remarks>
    /// This is a luminance value. Areas with a smaller luminance value are cut off.
    /// </remarks>
    public float Threshold { get; set; }


    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. This value must be greater than 0. The default value is 2.
    /// </value>
    /// <remarks>
    /// To improve performance, the effect is computed on a downsampled color buffer. The width and 
    /// height of the source image are downsampled by this factor.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is 0 or negative.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int DownsampleFactor
    {
      get { return _downsampleFactor; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "DownsampleFactor must be greater than 0.");

        _downsampleFactor = value;
      }
    }
    private int _downsampleFactor;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BloomFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public BloomFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/BloomFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _bloomThresholdParameter = _effect.Parameters["BloomThreshold"];
      _bloomIntensityParameter = _effect.Parameters["BloomIntensity"];
      _bloomSaturationParameter = _effect.Parameters["BloomSaturation"];
      _sceneTextureParameter = _effect.Parameters["SceneTexture"];
      _bloomTextureParameter = _effect.Parameters["BloomTexture"];
      _brightnessPass = _effect.CurrentTechnique.Passes["Brightness"];
      _combinePass = _effect.CurrentTechnique.Passes["Combine"];

      _blur = new Blur(graphicsService);
      _blur.InitializeGaussianBlur(7, 7.0f / 3.0f, true);

      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);

      Threshold = 0.7f;
      Intensity = 1.5f;
      Saturation = 0.5f;
      DownsampleFactor = 4;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        throw new GraphicsException("Source texture format must not be a floating-point format.");

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      int downsampledWidth = Math.Max(1, source.Width / DownsampleFactor);
      int downsampledHeight = Math.Max(1, source.Height / DownsampleFactor);

      // ----- Get temporary render targets.
      var bloomFormat = new RenderTargetFormat(downsampledWidth, downsampledHeight, false, SurfaceFormat.Color, DepthFormat.None);
      RenderTarget2D bloom0 = renderTargetPool.Obtain2D(bloomFormat);
      RenderTarget2D bloom1 = renderTargetPool.Obtain2D(bloomFormat);

      // ----- Downsample scene to bloom0.
      context.RenderTarget = bloom0;
      context.Viewport = new Viewport(0, 0, bloom0.Width, bloom0.Height);
      _downsampleFilter.Process(context);

      // ----- Create bloom image
      graphicsDevice.SetRenderTarget(bloom1);
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _bloomThresholdParameter.SetValue(Threshold);
      _bloomIntensityParameter.SetValue(Intensity);
      _bloomSaturationParameter.SetValue(Saturation);
      _sceneTextureParameter.SetValue(bloom0);
      _brightnessPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // bloom0 is not needed anymore.
      renderTargetPool.Recycle(bloom0);

      // We make a two-pass blur, so source can be equal to target.
      context.SourceTexture = bloom1;
      context.RenderTarget = bloom1;
      _blur.Process(context);

      // ----- Combine scene and bloom.
      graphicsDevice.SetRenderTarget(target);
      graphicsDevice.Viewport = viewport;
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sceneTextureParameter.SetValue(source);
      _bloomTextureParameter.SetValue(bloom1);
      _combinePass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // ----- Clean-up
      _sceneTextureParameter.SetValue((Texture2D)null);
      _bloomTextureParameter.SetValue((Texture2D)null);
      renderTargetPool.Recycle(bloom1);

      // Restore original context.
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }
    #endregion
  }
}
#endif
