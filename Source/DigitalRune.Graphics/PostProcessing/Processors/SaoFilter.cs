// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Creates a <i>Screen-Space Ambient Occlusion</i> (SSAO) effect using the "Scalable Ambient
  /// Obscurance" method to approximate ambient occlusion in real-time. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class implements SSAO using the "Scalable Ambient Obscurance" method. This method creates
  /// a higher quality result, but might be slower than the SSAO method used by the
  /// <see cref="SsaoFilter"/>.
  /// </para>
  /// <para>
  /// To improve the performance of this post processor the G-Buffer 0 should be mipmapped. If the
  /// G-Buffer 0 texture does not use mip maps, then this effect might be slow with a large sampling
  /// <see cref="Radius"/> or when the scene is close to the camera.
  /// </para>
  /// </remarks>
  public class SaoFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectParameter _frustumInfoParameter;
    private readonly EffectParameter _numberOfAOSamplesParameter;
    private readonly EffectParameter _aoParameters0;
    private readonly EffectParameter _aoParameters1;
    private readonly EffectParameter _aoParameters2;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _occlusionTextureParameter;
    private readonly EffectParameter _gBuffer0Parameter;
    //private readonly EffectParameter _viewParameter;
    //private readonly EffectParameter _gBuffer1Parameter;

    private readonly EffectPass _createAOPass;
    private readonly EffectPass _blurHorizontalPass;
    private readonly EffectPass _blurVerticalPass;
    private readonly EffectPass _blurVerticalAndCombinePass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the strength of the ambient occlusion.
    /// </summary>
    /// <value>
    /// The strength. 0 means no ambient occlusion; 1 means full ambient occlusion.
    /// </value>
    public float Strength { get; set; }


    /// <summary>
    /// Gets or sets the max ambient occlusion value.
    /// </summary>
    /// <value>The max ambient occlusion in the range [0, 1]. The default value is 1.</value>
    /// <remarks>
    /// If <see cref="MaxOcclusion"/> is less than 1, the occlusion is clamped. Very occluded
    /// spots will be gray instead of black. Lowering the <see cref="MaxOcclusion"/> helps to
    /// make the ambient occlusion more uniform and avoid very dark spots.
    /// </remarks>
    public float MaxOcclusion { get; set; }


    /// <summary>
    /// Gets or sets the sample radius in world space units.
    /// </summary>
    /// <value>The sample radius in world space units. The default value is 0.5.</value>
    public float Radius { get; set; }


    /// <summary>
    /// Gets or sets the minimum bias used to avoid sampling artifacts.
    /// </summary>
    /// <value>The minimum bias in world space units. The default value is 0.02.</value>
    /// <remarks>
    /// <para>
    /// The bias is similar to the depth bias used in shadow mapping. It avoids dark ambient 
    /// occlusion spots on flat surface or very flat concave edges. 
    /// </para>
    /// <para>
    /// The effective bias is at least <see cref="MinBias"/>. It increases with distance from the 
    /// camera with a rate defined by <see cref="Bias"/>. To remove dark ambient occlusion spots,
    /// near the camera increase <see cref="MinBias"/>. To remove dark ambient occlusion spots
    /// in the distance, increase <see cref="Bias"/>. 
    /// </para>
    /// </remarks>
    public float MinBias { get; set; }


    /// <summary>
    /// Gets or sets the bias used to avoid sampling artifacts.
    /// </summary>
    /// <value>The bias. The default value is 0.0004.</value>
    /// <inheritdoc cref="MinBias" />
    public float Bias { get; set; }


    /// <summary>
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>The number of samples; must be greater than 0. The default value is 11.</value>
    public int NumberOfSamples
    {
      get { return _numberOfSamples; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "NumberOfSamples must be greater than 0.");

        _numberOfSamples = value;
      }
    }
    private int _numberOfSamples;


    /// <summary>
    /// Gets or sets the sample distribution.
    /// </summary>
    /// <value>The sample distribution. The default value is 7.</value>
    /// <remarks>
    /// This property influences how the samples are distributed in the sampled disk. Normally,
    /// you do not need to change this value. However, if <see cref="NumberOfSamples"/> is changed
    /// and the samples align or are not distributed uniformly, you can change the value. You can
    /// try any value equal to or greater than 1.
    /// </remarks>
    public float SampleDistribution      // = Number of spiral turns.
    {
      get { return _sampleDistribution; }
      set
      {
        if (Numeric.IsZero(value))
          throw new ArgumentOutOfRangeException("value", "SampleDistribution must not be 0.");

        _sampleDistribution = value;
      }
    }
    private float _sampleDistribution;


    /// <summary>
    /// Gets or sets the blur scale.
    /// </summary>
    /// <value>The blur scale. The default value is 2.</value>
    /// <remarks>
    /// This factor is used to scale the samples offsets used when blurring the computed ambient
    /// occlusion. Increasing this factor creates a smoother ambient occlusion but may lead to
    /// dithering/checker board artifacts. Normally, the blur scale will be 2 or 3.
    /// </remarks>
    public float BlurScale { get; set; }


    /// <summary>
    /// Gets or sets the edge sharpness.
    /// </summary>
    /// <value>The edge sharpness. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// The computed ambient occlusion is blurred using an edge aware blur to avoid blurring over
    /// depth discontinuities. The sensitivity of the edge aware blur is defined by 
    /// <see cref="EdgeSharpness"/>. If <see cref="EdgeSharpness"/> is 0, the blur is a normal
    /// blur ignoring edges.
    /// </para>
    /// <para>
    /// Increase value to make edges crisper. Decrease value to reduce temporal flicker.
    /// </para>
    /// </remarks>
    [Obsolete("EdgeSharpness has been replaced by EdgeSoftness which offers better control.")]
    public float EdgeSharpness { get; set; }


    /// <summary>
    /// Gets or sets the edge softness.
    /// </summary>
    /// <value>The edge softness in world space units. The default value is 0.5.</value>
    /// <remarks>
    /// <para>
    /// The computed ambient occlusion is blurred using an edge-aware blur to avoid blurring over
    /// depth discontinuities. The sensitivity of the edge-aware blur is defined by 
    /// <see cref="EdgeSoftness"/>. The value is the max allowed depth difference of two pixel in
    /// world space units. Pixels that closer than this threshold are blurred together; pixels which
    /// are farther apart are ignored.
    /// </para>
    /// <para>
    /// Decrease the value to make edges crisper. Increase the value to reduce temporal flicker.
    /// </para>
    /// </remarks>
    public float EdgeSoftness { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the ambient occlusion should be applied to the
    /// source image - or if a black-white AO image is produced, ignoring the source image.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the ambient occlusion buffer is applied to the source image;
    /// the result is the source image where occluded pixels are darkened. <see langword="false"/>
    /// if the source image is ignored and the processor produces a grayscale image (white =
    /// not occluded, black = fully occluded). The default value is <see langword="true"/>.
    /// </value>
    public bool CombineWithSource { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SsaoFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SaoFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      Effect effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SaoFilter");
      _frustumInfoParameter = effect.Parameters["FrustumInfo"];
      _numberOfAOSamplesParameter = effect.Parameters["NumberOfAOSamples"];
      _aoParameters0 = effect.Parameters["AOParameters0"];
      _aoParameters1 = effect.Parameters["AOParameters1"];
      _aoParameters2 = effect.Parameters["AOParameters2"];
      _sourceTextureParameter = effect.Parameters["SourceTexture"];
      _occlusionTextureParameter = effect.Parameters["OcclusionTexture"];
      _gBuffer0Parameter = effect.Parameters["GBuffer0"];
      //_viewParameter = _effect.Parameters["View"];
      //_gBuffer1Parameter = _effect.Parameters["GBuffer1"];
      _createAOPass = effect.CurrentTechnique.Passes["CreateAO"];
      _blurHorizontalPass = effect.CurrentTechnique.Passes["BlurHorizontal"];
      _blurVerticalPass = effect.CurrentTechnique.Passes["BlurVertical"];
      _blurVerticalAndCombinePass = effect.CurrentTechnique.Passes["BlurVerticalAndCombine"];

      Strength = 1;
      MaxOcclusion = 1;
      Radius = 0.5f;
      MinBias = 0.02f;
      Bias = 0.0004f;
      NumberOfSamples = 11;
      SampleDistribution = 7;
      BlurScale = 2;
      EdgeSoftness = 0.5f;
      CombineWithSource = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      context.ThrowIfCameraMissing();

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;
      var cameraNode = context.CameraNode;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      Projection projection = cameraNode.Camera.Projection;
      Matrix44F projMatrix = projection;
      float near = projection.Near;
      float far = projection.Far;

      _frustumInfoParameter.SetValue(new Vector4(
        projection.Left / near,
        projection.Top / near,
        (projection.Right - projection.Left) / near,
        (projection.Bottom - projection.Top) / near));

      _numberOfAOSamplesParameter.SetValue(NumberOfSamples);

      // The height of a 1 unit object 1 unit in front of the camera.
      // (Compute 0.5 unit multiply by 2 and divide by 2 to convert from [-1, 1] to [0, 1] range.)
      float projectionScale =
        projMatrix.TransformPosition(new Vector3F(0, 0.5f, -1)).Y
        - projMatrix.TransformPosition(new Vector3F(0, 0, -1)).Y;

      _aoParameters0.SetValue(new Vector4(
        projectionScale,
        Radius,
        Strength / (float)Math.Pow(Radius, 6),
        Bias));

      _aoParameters1.SetValue(new Vector4(
        viewport.Width,
        viewport.Height,
        far,
        MaxOcclusion));

      _aoParameters2.SetValue(new Vector4(
        SampleDistribution,
        1.0f / (EdgeSoftness + 0.001f) * far,
        BlurScale,
        MinBias));

      context.ThrowIfGBuffer0Missing();
      _gBuffer0Parameter.SetValue(context.GBuffer0);

      //var view = cameraNode.View;
      //_viewParameter.SetValue((Matrix)view);
      //_gBuffer1Parameter.SetValue(context.GBuffer1);

      // We use two temporary render targets.
      var format = new RenderTargetFormat(
        context.Viewport.Width,
        context.Viewport.Height,
        false,
        SurfaceFormat.Color,
        DepthFormat.None);

      var tempTarget0 = renderTargetPool.Obtain2D(format);
      var tempTarget1 = renderTargetPool.Obtain2D(format);

      // Create SSAO.
      graphicsDevice.SetRenderTarget(tempTarget0);
      _createAOPass.Apply();

      graphicsDevice.Clear(new Color(1.0f, 1.0f, 1.0f, 1.0f));
      graphicsDevice.DrawFullScreenQuad();

      // Horizontal blur.
      graphicsDevice.SetRenderTarget(tempTarget1);
      _occlusionTextureParameter.SetValue(tempTarget0);
      _blurHorizontalPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Vertical blur
      graphicsDevice.SetRenderTarget(target);
      graphicsDevice.Viewport = viewport;
      _occlusionTextureParameter.SetValue(tempTarget1);
      if (!CombineWithSource)
      {
        _blurVerticalPass.Apply();
      }
      else
      {
        if (_sourceTextureParameter != null)
          _sourceTextureParameter.SetValue(source);

        if (TextureHelper.IsFloatingPointFormat(source.Format))
          graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        _blurVerticalAndCombinePass.Apply();
      }

      graphicsDevice.DrawFullScreenQuad();

      // Clean up.
      renderTargetPool.Recycle(tempTarget0);
      renderTargetPool.Recycle(tempTarget1);
      if (_sourceTextureParameter != null)
        _sourceTextureParameter.SetValue((Texture2D)null);
      _occlusionTextureParameter.SetValue((Texture2D)null);
      _gBuffer0Parameter.SetValue((Texture2D)null);
      //_gBuffer1Parameter.SetValue((Texture2D)null);
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }
    #endregion
  }
}
#endif
