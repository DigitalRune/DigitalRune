// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Creates a depth-of-field effect.
  /// </summary>
  public class DepthOfFieldFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _screenSizeParameter;
    private readonly EffectParameter _depthTextureParameter;
    private readonly EffectParameter _nearBlurDistanceParameter;
    private readonly EffectParameter _nearFocusDistanceParameter;
    private readonly EffectParameter _farFocusDistanceParameter;
    private readonly EffectParameter _farBlurDistanceParameter;
    private readonly EffectParameter _farParameter;
    private readonly EffectParameter _blurTextureParameter;
    private readonly EffectParameter _downsampledDepthTextureParameter;
    private readonly EffectParameter _downsampledCocTextureParameter;
    private readonly EffectParameter _offsetsParameter;
    private readonly EffectParameter _weightsParameter;
    private readonly EffectParameter _sceneTextureParameter;
    private readonly EffectPass _circleOfConfusionPass;
    private readonly EffectPass _blurPass;
    private readonly EffectPass _depthOfFieldPass;

    private readonly DownsampleFilter _downsampleFilter;

    // Blur processor used to blur CoC and depth.
    private readonly Blur _cocBlur;

    private Vector2F _lastViewportSize;
    private Vector2[] _horizontalOffsets;
    private Vector2[] _verticalOffsets;
    private float[] _weights;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the near distance where the blur starts to decrease.
    /// </summary>
    /// <value>The near blur distance.</value>
    public float NearBlurDistance { get; set; }


    /// <summary>
    /// Gets or sets the near distance where the objects start to be in focus.
    /// </summary>
    /// <value>The near focus distance.</value>
    public float NearFocusDistance { get; set; }


    /// <summary>
    /// Gets or sets the far distance where objects start to get blurry.
    /// </summary>
    /// <value>The far focus distance.</value>
    public float FarFocusDistance { get; set; }


    /// <summary>
    /// Gets or sets the far distance after which objects are maximal blurred.
    /// </summary>
    /// <value>The far blur distance.</value>
    public float FarBlurDistance { get; set; }


    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. This value must be greater than 0. The default value is 2.
    /// </value>
    /// <remarks>
    /// To improve performance, depth of field is computed on a downsampled depth buffer. 
    /// The width and height of the depth buffer are downsampled by this factor.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 1.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int DownsampleFactor
    {
      get { return _downsampleFactor; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "DownsampleFactor must not be greater than 0.");

        _downsampleFactor = value;
      }
    }
    private int _downsampleFactor;


    /// <summary>
    /// Gets or sets the blur strength.
    /// </summary>
    /// <value>
    /// The blur strength. 0 means "no blur". Values greater 0 increase the blur effect.
    /// The default value is 1. 
    /// </value>
    public float BlurStrength
    {
      get { return _blurStrength; }
      set
      {
        _blurStrength = value;

        // Invalidate _lastViewport to force an update of the blur samples.
        _lastViewportSize = new Vector2F(-1, -1);
      }
    }
    private float _blurStrength;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DepthOfFieldFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public DepthOfFieldFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/DepthOfFieldFilter");
      _screenSizeParameter = _effect.Parameters["ScreenSize"];
      _depthTextureParameter = _effect.Parameters["DepthTexture"];
      _nearBlurDistanceParameter = _effect.Parameters["NearBlurDistance"];
      _nearFocusDistanceParameter = _effect.Parameters["NearFocusDistance"];
      _farFocusDistanceParameter = _effect.Parameters["FarFocusDistance"];
      _farBlurDistanceParameter = _effect.Parameters["FarBlurDistance"];
      _farParameter = _effect.Parameters["Far"];
      _blurTextureParameter = _effect.Parameters["BlurTexture"];
      _downsampledDepthTextureParameter = _effect.Parameters["DownsampledDepthTexture"];
      _downsampledCocTextureParameter = _effect.Parameters["DownsampledCocTexture"];
      _offsetsParameter = _effect.Parameters["Offsets"];
      _weightsParameter = _effect.Parameters["Weights"];
      _sceneTextureParameter = _effect.Parameters["SceneTexture"];
      _circleOfConfusionPass = _effect.CurrentTechnique.Passes["CircleOfConfusion"];
      _blurPass = _effect.CurrentTechnique.Passes["Blur"];
      _depthOfFieldPass = _effect.CurrentTechnique.Passes["DepthOfField"];

      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);

      _cocBlur = new Blur(graphicsService);
      _cocBlur.InitializeBoxBlur(5, false);

      NearBlurDistance = 2;
      NearFocusDistance = 5;
      FarFocusDistance = 6;
      FarBlurDistance = 10;
      _downsampleFactor = 2;
      BlurStrength = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    protected override void OnProcess(RenderContext context)
    {
      context.ThrowIfCameraMissing();
      context.ThrowIfGBuffer0Missing();

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var cameraNode = context.CameraNode;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      var sourceSize = new Vector2F(source.Width, source.Height);
      int width = (int)sourceSize.X;
      int height = (int)sourceSize.Y;
      int downsampledWidth = Math.Max(1, width / DownsampleFactor);
      int downsampledHeight = Math.Max(1, height / DownsampleFactor);

      if (TextureHelper.IsFloatingPointFormat(source.Format))
      {
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        InitializeGaussianBlur(new Vector2F(downsampledWidth, downsampledHeight), false);
      }
      else
      {
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        InitializeGaussianBlur(new Vector2F(downsampledWidth, downsampledHeight), true);
      }

      // Get temporary render targets.
      var downsampleFormat = new RenderTargetFormat(downsampledWidth, downsampledHeight, false, source.Format, DepthFormat.None);      
      RenderTarget2D blurredScene0 = renderTargetPool.Obtain2D(downsampleFormat);
      RenderTarget2D blurredScene1 = renderTargetPool.Obtain2D(downsampleFormat);

      var blurredDepthFormat = new RenderTargetFormat(downsampledWidth, downsampledHeight, false, context.GBuffer0.Format, DepthFormat.None);
      RenderTarget2D blurredDepth0 = renderTargetPool.Obtain2D(blurredDepthFormat);

      var cocFormat = new RenderTargetFormat(width, height, false, SurfaceFormat.Single, DepthFormat.None);
      RenderTarget2D cocImage = renderTargetPool.Obtain2D(cocFormat);

      var downSampledCocFormat = new RenderTargetFormat(downsampledWidth, downsampledHeight, false, cocFormat.SurfaceFormat, DepthFormat.None);
      RenderTarget2D cocImageBlurred = renderTargetPool.Obtain2D(downSampledCocFormat);

      // ----- Create CoC map.
      _effect.CurrentTechnique = _effect.Techniques[0];
      graphicsDevice.SetRenderTarget(cocImage);
      _screenSizeParameter.SetValue(new Vector2(cocImage.Width, cocImage.Height));
      _depthTextureParameter.SetValue(context.GBuffer0);
      _nearBlurDistanceParameter.SetValue(NearBlurDistance);
      _nearFocusDistanceParameter.SetValue(NearFocusDistance);
      _farFocusDistanceParameter.SetValue(FarFocusDistance);
      _farBlurDistanceParameter.SetValue(FarBlurDistance);
      _farParameter.SetValue(cameraNode.Camera.Projection.Far);
      _circleOfConfusionPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // ----- Downsample cocImage to cocImageBlurred.
      context.SourceTexture = cocImage;
      context.RenderTarget = cocImageBlurred;
      context.Viewport = new Viewport(0, 0, cocImageBlurred.Width, cocImageBlurred.Height);
      _downsampleFilter.Process(context);

      renderTargetPool.Recycle(cocImage);

      // ----- Downsample source to blurredScene0.
      context.SourceTexture = source;
      context.RenderTarget = blurredScene0;
      context.Viewport = new Viewport(0, 0, blurredScene0.Width, blurredScene0.Height);
      _downsampleFilter.Process(context);

      // ----- Downsample depth texture to blurredDepth0.
      context.SourceTexture = context.GBuffer0;
      context.RenderTarget = blurredDepth0;
      context.Viewport = new Viewport(0, 0, blurredDepth0.Width, blurredDepth0.Height);
      _downsampleFilter.Process(context);

      // ----- Blur scene.
      // Horizontal blur
      graphicsDevice.SetRenderTarget(blurredScene1);
      _screenSizeParameter.SetValue(new Vector2(blurredScene0.Width, blurredScene0.Height));
      _blurTextureParameter.SetValue(blurredScene0);
      _downsampledDepthTextureParameter.SetValue(blurredDepth0);
        _downsampledCocTextureParameter.SetValue(cocImageBlurred);
      _offsetsParameter.SetValue(_horizontalOffsets);
      _weightsParameter.SetValue(_weights);
      _blurPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Vertical blur.
      graphicsDevice.SetRenderTarget(blurredScene0);
      _blurTextureParameter.SetValue(blurredScene1);
      _offsetsParameter.SetValue(_verticalOffsets);
      _blurPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      renderTargetPool.Recycle(blurredScene1);

      // ----- Blur cocImageBlurred.
      context.SourceTexture = cocImageBlurred;
      context.RenderTarget = cocImageBlurred;
      context.Viewport = new Viewport(0, 0, cocImageBlurred.Width, cocImageBlurred.Height);
      _cocBlur.Process(context);   // We make a two pass blur, so context.SourceTexture can be equal to context.RenderTarget.

      // ----- Blur depth.
      context.SourceTexture = blurredDepth0;
      context.RenderTarget = blurredDepth0;
      context.Viewport = new Viewport(0, 0, blurredDepth0.Width, blurredDepth0.Height);
      _cocBlur.Process(context);

      // ----- Create final DoF image.
      _effect.CurrentTechnique = _effect.Techniques[0];
      graphicsDevice.SetRenderTarget(target);
      graphicsDevice.Viewport = viewport;
      _screenSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sceneTextureParameter.SetValue(source);
      _blurTextureParameter.SetValue(blurredScene0);
      _depthTextureParameter.SetValue(context.GBuffer0);
      _downsampledDepthTextureParameter.SetValue(blurredDepth0);
      _downsampledCocTextureParameter.SetValue(cocImageBlurred);
      _depthOfFieldPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // ----- Clean-up
      _depthTextureParameter.SetValue((Texture2D)null);
      _blurTextureParameter.SetValue((Texture2D)null);
      _downsampledDepthTextureParameter.SetValue((Texture2D)null);
      _downsampledCocTextureParameter.SetValue((Texture2D)null);
      _sceneTextureParameter.SetValue((Texture2D)null);
      renderTargetPool.Recycle(blurredScene0);
      renderTargetPool.Recycle(blurredDepth0);
      renderTargetPool.Recycle(cocImageBlurred);
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }


    private void InitializeGaussianBlur(Vector2F viewportSize, bool useHardwareFiltering)
    {
      if (_horizontalOffsets != null && _lastViewportSize == viewportSize)
        return;

      _lastViewportSize = viewportSize;

      int numberOfSamples = _offsetsParameter.Elements.Count;
      float standardDeviation = numberOfSamples / 3.0f * BlurStrength;

      if (_horizontalOffsets == null)
      {
        _horizontalOffsets = new Vector2[numberOfSamples];
        _verticalOffsets = new Vector2[numberOfSamples];
        _weights = new float[numberOfSamples];
      }

      // Define the Gaussian function coefficient that we use.
      float coefficient = 1 / (float)Math.Sqrt(ConstantsF.TwoPi) / standardDeviation;

      float weightSum;

      if (useHardwareFiltering)
      {
        // We sample 2 pixels per tap, so we can sample twice as wide.
        standardDeviation = standardDeviation * 2;

        // Sample the center pixel in the middle and then between pixel.
        _horizontalOffsets[0] = new Vector2(0, 0);
        _verticalOffsets[0] = new Vector2(0, 0);
        _weights[0] = (BlurStrength > 0) ? MathHelper.Gaussian(0, coefficient, 0, standardDeviation) : 1;
        weightSum = _weights[0];
        for (int i = 1; i < numberOfSamples; i += 2)
        {
          // Get an offset between two pixels.
          var offset = new Vector2(1.5f + (i - 1), 0); // = 1.5 + k * 2

          // Get the offsets of the neighboring pixel centers.
          var o0 = offset.X - 0.5f;
          var o1 = offset.X + 0.5f;

          // Compute the weights of the pixel centers.
          var w0 = (BlurStrength > 0) ? MathHelper.Gaussian(o0, coefficient, 0, standardDeviation) : 0;
          var w1 = (BlurStrength > 0) ? MathHelper.Gaussian(o1, coefficient, 0, standardDeviation) : 0;
          _weights[i] = (w0 + w1);
          _weights[i + 1] = _weights[i];
          weightSum += (_weights[i] * 2);

          // Shift the offset to the pixel center that has the higher weight.
          offset.X = (o0 * w0 + o1 * w1) / (w0 + w1);

          _horizontalOffsets[i] = offset / viewportSize.X;
          _horizontalOffsets[i + 1] = -_horizontalOffsets[i];
          _verticalOffsets[i] = new Vector2(0, offset.X) / viewportSize.Y;
          _verticalOffsets[i + 1] = -_verticalOffsets[i];
        }
      }
      else
      {
        // Same as above but: Sample in the middle of pixels.
        _horizontalOffsets[0] = new Vector2(0, 0);
        _verticalOffsets[0] = new Vector2(0, 0);
        _weights[0] = (BlurStrength > 0) ? MathHelper.Gaussian(0, coefficient, 0, standardDeviation) : 1;
        weightSum = _weights[0];
        for (int i = 1; i < numberOfSamples; i += 2)
        {
          var offset = new Vector2(1 + i / 2, 0);

          _weights[i] = (BlurStrength > 0) ? MathHelper.Gaussian(offset.X, coefficient, 0, standardDeviation) : 0;
          _weights[i + 1] = _weights[i];
          weightSum += (_weights[i] * 2);

          _horizontalOffsets[i] = offset / viewportSize.X;
          _horizontalOffsets[i + 1] = -_horizontalOffsets[i];
          _verticalOffsets[i] = new Vector2(0, offset.X) / viewportSize.Y;
          _verticalOffsets[i + 1] = -_verticalOffsets[i];
        }
      }

      // Normalize weights.
      for (int i = 0; i < numberOfSamples; i++)
        _weights[i] /= weightSum;
    }
    #endregion
  }
}
#endif
