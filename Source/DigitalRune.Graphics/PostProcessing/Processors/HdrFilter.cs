// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Performs HDR tone mapping supporting bloom/glare, dynamic eye adaption and scotopic vision.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This post-processor is also responsible for converting the input texture from an HDR format
  /// (usually <strong>HdrBlendable</strong>) to an LDR format. Therefore, the surface format
  /// in <see cref="PostProcessor.DefaultTargetFormat"/> is set to <strong>Color</strong>.
  /// </para>
  /// <para>
  /// This post-processor stores luminance information of a frame for use in the next frame. The 
  /// information is stored with the currently active camera node. When there is a cut in the scene 
  /// (i.e. a new level is loaded or the view changes significantly), the method 
  /// <see cref="CameraNode.InvalidateViewDependentData()"/> of the camera node needs to be called 
  /// to reset the luminance information. Further, this post-processor expects that it is called 
  /// once per frame for a certain camera node. It might not work as expected if it is called 
  /// several times per frame (e.g. to process different views using the same camera).
  /// </para>
  /// <para>
  /// <strong>Exposure and Middle Gray</strong><br/>
  /// The <see cref="HdrFilter"/> computes the average brightness of the scene and applies a
  /// brightness scale factor ("exposure") so that the average brightness of the final image is
  /// equal to <see cref="MiddleGray"/>. That means, if you are in a dark cave, the filter will
  /// make the image a lot brighter. If you are in a white dessert, it will make the image darker.
  /// Both images will have the same average brightness defined by <see cref="MiddleGray"/>.
  /// The brightness scale factor is limited by <see cref="MinExposure"/> and 
  /// <see cref="MaxExposure"/>. That means, if you set <see cref="MinExposure"/> =
  /// <see cref="MaxExposure"/> = 1, then there will be no brightness change.
  /// If your scene is very bright or very dark, then you have to adjust <see cref="MiddleGray"/>. 
  /// Use <see cref="MinExposure"/> and <see cref="MaxExposure"/> to limit the allowed brightness 
  /// changes.
  /// </para>
  /// <para>
  /// <strong>Night Tone mapping with Blue Shift (Scotopic Vision):</strong><br/>
  /// The human visual system perceives low luminance scenes (e.g. night scenes) different than
  /// well-lit scenes. Under daylight the cones in the human eye dominate and create the normal
  /// color image (photopic vision). The cones start to lose sensitivity at about 3.4 cd/m², and
  /// the rods start to get more dominant. A dark scene perceived by the rods (scotopic vision) is
  /// monochrome with a blue shift. This <see cref="HdrFilter"/> supports physically-based night 
  /// tone mapping. Per default it is disabled. The blue shift is controlled using the properties
  /// <see cref="EnableBlueShift"/>, <see cref="BlueShiftColor"/>, <see cref="BlueShiftCenter"/> and
  /// <see cref="BlueShiftRange"/>. Depending on the lighting in the scene, 
  /// <see cref="BlueShiftColor"/> might need to be scaled. It is also possible to use a different
  /// color for a more dramatic effect. <see cref="BlueShiftCenter"/> and 
  /// <see cref="BlueShiftRange"/> might also need to be changed if the scene is not lit using
  /// physically-based light intensities and lighting models.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class HdrFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _bloomThresholdParameter;
    private readonly EffectParameter _middleGrayParameter;
    private readonly EffectParameter _minExposureParameter;
    private readonly EffectParameter _maxExposureParameter;
    private readonly EffectParameter _blueShiftColorParameter;
    private readonly EffectParameter _blueShiftParameter;
    private readonly EffectParameter _bloomIntensityParameter;
    private readonly EffectParameter _sceneTextureParameter;
    private readonly EffectParameter _luminanceTextureParameter;
    private readonly EffectParameter _bloomTextureParameter;
    private readonly EffectPass _brightnessPass;
    private readonly EffectPass _combinePass;
    private readonly EffectPass _combineWithBlueShiftPass;

    private readonly Blur _blur;
    private readonly LuminanceFilter _luminance;
    private readonly RenderTarget2D _luminanceTarget;
    private readonly DownsampleFilter _downsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bloom intensity factor.
    /// </summary>
    /// <value>The bloom intensity factor. The default value is 1.</value>
    public float BloomIntensity { get; set; }


    /// <summary>
    /// Gets or sets the brightness threshold.
    /// </summary>
    /// <value>The brightness threshold.</value>
    /// <remarks>
    /// This is a luminance value. Areas with a smaller luminance value are cut off.
    /// The default value is 0.2.
    /// </remarks>
    public float BloomThreshold { get; set; }


    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. This value must be greater than 0. The default value is 2.
    /// </value>
    /// <remarks>
    /// To improve performance, the bloom effect is computed on a downsampled color buffer. 
    /// The width and height of the source image are downsampled by this factor.
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
          throw new ArgumentOutOfRangeException("value", "DownsampleFactor must not be greater than 0.");

        _downsampleFactor = value;
      }
    }
    private int _downsampleFactor;


    /// <summary>
    /// Gets or sets the average gray level.
    /// </summary>
    /// <value>The average gray level. The default value is 0.18.</value>
    /// <remarks>
    /// The average luminance of the scene is mapped to this gray value.
    /// </remarks>
    public float MiddleGray { get; set; }


    /// <summary>
    /// Gets or sets the min exposure factor.
    /// </summary>
    /// <value>The min exposure.</value>
    public float MinExposure { get; set; }


    /// <summary>
    /// Gets or sets the max exposure factor.
    /// </summary>
    /// <value>The max exposure.</value>
    public float MaxExposure { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the average luminance is computed using the 
    /// geometric mean.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the geometric mean is used for the average luminance; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool UseGeometricMean
    {
      get { return _luminance.UseGeometricMean; }
      set { _luminance.UseGeometricMean = value; }
    }


    /// <summary>
    /// Gets or sets the adaption speed of the eye.
    /// </summary>
    /// <value>The adaption speed in the range [0, ∞[. Use small values like 0.02.</value>
    public float AdaptionSpeed
    {
      get { return _luminance.AdaptionSpeed; }
      set { _luminance.AdaptionSpeed = value; }
    }


    /// <summary>
    /// Gets or sets a value indicating whether dynamic eye adaption should be used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if dynamic eye adaption is used; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If dynamic eye adaption is used, the luminance of the previous frames will influence the 
    /// luminance values of the current frame. This models the behavior of the human eye: The eye 
    /// slowly adapts to new lighting conditions. 
    /// </remarks>
    public bool UseAdaption
    {
      get { return _luminance.UseAdaption; }
      set { _luminance.UseAdaption = value; }
    }


    /// <summary>
    /// Gets or sets a value indicating whether a blue shift is applied to scenes with low average
    /// luminance.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if a blue shift is applied to scenes with low average luminance;
    /// otherwise, <see langword="false" />. The default value is <see langword="false" />.
    /// </value>
    public bool EnableBlueShift { get; set; }


    /// <summary>
    /// Gets or sets the color of the blue shift.
    /// </summary>
    /// <value>The blue shift color. The default value is (1.05/4, 0.97/4, 1.27/4).</value>
    public Vector3F BlueShiftColor { get; set; }


    /// <summary>
    /// Gets or sets scene luminance where 50% blue shift is applied
    /// </summary>
    /// <value>The blue shift center. The default value is 0.04.</value>
    /// <remarks>
    /// <para>
    /// The blue shift depends on the average scene luminance. If the average scene luminance is
    /// equal to to <see cref="BlueShiftCenter"/>, then the resulting image is the average of the
    /// normal image and a fully blue shifted image.
    /// </para>
    /// <para>
    /// The default value of <see cref="BlueShiftCenter"/> is 0.04. This value is suitable for a
    /// scene that is rendered using physically-based lighting, i.e. light intensities are based on
    /// real world light intensities and luminance values are in cd/m². If the scene lighting is not
    /// physically-based, then this value needs to be adapted.
    /// </para>
    /// </remarks>
    public float BlueShiftCenter { get; set; }


    /// <summary>
    /// Gets or sets the range of the blue shift.
    /// </summary>
    /// <value>The range of the blue shift. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// This value defines the luminance range where the blue shift is applied. For a human eye and
    /// physically-based lighting, this value should be 1. With this default value, the blue shift
    /// is 100% when the average scene luminance is 0. The blue shift is 50% when the luminance is 
    /// equal to <see cref="BlueShiftCenter"/>. The blue shift is approx. 1% when the luminance is 
    /// <see cref="BlueShiftCenter"/> x 100. (The cones in the human eye start to lose their
    /// sensitivity at 3.4 cd/m².)
    /// </para>
    /// <para>
    /// <see cref="BlueShiftRange"/> is proportional to the luminance range on a logarithmic scale.
    /// That means, if the <see cref="BlueShiftRange"/> is 1, the 1% point is at 
    /// <see cref="BlueShiftCenter"/> x 100. If the <see cref="BlueShiftRange"/> is set to 0.5, the
    /// 1% point is at <see cref="BlueShiftCenter"/> x 10 (= 100<sup>0.5</sup>). If the 
    /// <see cref="BlueShiftRange"/> is set to 2, the 1% point is at 
    /// <see cref="BlueShiftCenter"/> x 10000 (= 100<sup>2</sup>).
    /// </para>
    /// </remarks>
    public float BlueShiftRange { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="HdrFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public HdrFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/HdrFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _bloomThresholdParameter = _effect.Parameters["BloomThreshold"];
      _middleGrayParameter = _effect.Parameters["MiddleGray"];
      _minExposureParameter = _effect.Parameters["MinExposure"];
      _maxExposureParameter = _effect.Parameters["MaxExposure"];
      _blueShiftColorParameter = _effect.Parameters["BlueShiftColor"];
      _blueShiftParameter = _effect.Parameters["BlueShift"];
      _bloomIntensityParameter = _effect.Parameters["BloomIntensity"];
      _sceneTextureParameter = _effect.Parameters["SceneTexture"];
      _luminanceTextureParameter = _effect.Parameters["LuminanceTexture"];
      _bloomTextureParameter = _effect.Parameters["BloomTexture"];
      _brightnessPass = _effect.CurrentTechnique.Passes["Brightness"];
      _combinePass = _effect.CurrentTechnique.Passes["Combine"];
      _combineWithBlueShiftPass = _effect.CurrentTechnique.Passes["CombineWithBlueShift"];

      BloomIntensity = 1;
      BloomThreshold = 0.2f;
      DownsampleFactor = 4;
      MiddleGray = 0.18f;
      MinExposure = 0.01f;
      MaxExposure = 2.0f;

      // The physically based blue shift color is in CIE Yxy: x = 0.3 and y = 0.3.
      // As RGB this is (1.05, 0.97, 1.27). This color should be scaled by a user-defined
      // scale. The scotopic luminance computed in the shader is usually in the range [0, 4]. 
      // Therefore, we apply a scale factor of 0.25 as a default.
      BlueShiftColor = new Vector3F(1.05f, 0.97f, 1.27f) * 0.25f;
      BlueShiftCenter = 0.04f;
      BlueShiftRange = 1;

      _blur = new Blur(graphicsService);
      _blur.InitializeGaussianBlur(7, 7.0f / 3.0f, true);
      _luminance = new LuminanceFilter(graphicsService);
      _luminanceTarget = graphicsService.RenderTargetPool.Obtain2D(_luminance.DefaultTargetFormat);

      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);

      var defaultTargetFormat = DefaultTargetFormat;
      defaultTargetFormat.SurfaceFormat = SurfaceFormat.Color;
      DefaultTargetFormat = defaultTargetFormat;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets the dynamic internal states, especially dynamic luminance adaption.
    /// </summary>
    /// <remarks>
    /// This method should be called if there was a cut in the visual scene and the next frame is
    /// very different from the last frame.
    /// </remarks>
    [Obsolete("The method HdrFilter.Reset() is obsolete. Use CameraNode.InvalidateViewDependentData() instead.")]
    public void Reset()
    {
      _luminance.Reset();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      int downsampledWidth = Math.Max(1, source.Width / DownsampleFactor);
      int downsampledHeight = Math.Max(1, source.Height / DownsampleFactor);

      // ----- Get temporary render targets.
      var sourceDownsampled = renderTargetPool.Obtain2D(new RenderTargetFormat(
        downsampledWidth,
        downsampledHeight,
        false,
        source.Format,
        DepthFormat.None));
      var bloom = renderTargetPool.Obtain2D(new RenderTargetFormat(
        downsampledWidth,
        downsampledHeight,
        false,
        SurfaceFormat.Color,
        DepthFormat.None));

      // ----- Downsample scene.
      context.RenderTarget = sourceDownsampled;
      context.Viewport = new Viewport(0, 0, sourceDownsampled.Width, sourceDownsampled.Height);
      _downsampleFilter.Process(context);

      // ----- Compute luminance.
      context.SourceTexture = sourceDownsampled;
      context.RenderTarget = _luminanceTarget;
      context.Viewport = new Viewport(0, 0, _luminanceTarget.Width, _luminanceTarget.Height);
      _luminance.Process(context);

      // ----- Create bloom image.
      graphicsDevice.SetRenderTarget(bloom);
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _bloomThresholdParameter.SetValue(BloomThreshold);
      _middleGrayParameter.SetValue(MiddleGray);
      _minExposureParameter.SetValue(MinExposure);
      _maxExposureParameter.SetValue(MaxExposure);
      _bloomIntensityParameter.SetValue(BloomIntensity);
      _sceneTextureParameter.SetValue(sourceDownsampled);
      _luminanceTextureParameter.SetValue(_luminanceTarget);
      _brightnessPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // sourceDownsampled is not needed anymore.
      renderTargetPool.Recycle(sourceDownsampled);

      // We make a two-pass blur, so source can be equal to target.
      context.SourceTexture = bloom;
      context.RenderTarget = bloom;
      context.Viewport = new Viewport(0, 0, bloom.Width, bloom.Height);
      _blur.Process(context);

      // ----- Combine scene and bloom.
      graphicsDevice.SetRenderTarget(target);
      graphicsDevice.Viewport = viewport;
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sceneTextureParameter.SetValue(source);
      _bloomTextureParameter.SetValue(bloom);
      _luminanceTextureParameter.SetValue(_luminanceTarget);
      _middleGrayParameter.SetValue(MiddleGray);

      if (EnableBlueShift)
      {
        _blueShiftColorParameter.SetValue((Vector3)BlueShiftColor);
        _blueShiftParameter.SetValue(new Vector2(1 / BlueShiftCenter, 1 / BlueShiftRange));
        _combineWithBlueShiftPass.Apply();
      }
      else
      {
        _combinePass.Apply();
      }

      graphicsDevice.DrawFullScreenQuad();

      // ----- Clean-up
      _sceneTextureParameter.SetValue((Texture2D)null);
      _luminanceTextureParameter.SetValue((Texture2D)null);
      _bloomTextureParameter.SetValue((Texture2D)null);
      renderTargetPool.Recycle(bloom);
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }
    #endregion
  }
}
#endif
