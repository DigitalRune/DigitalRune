// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Creates a <i>Screen-Space Ambient Occlusion</i> (SSAO) effect to approximate ambient 
  /// occlusion in real-time.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The default values are suitable for typical scenes, where 1 world space unit = 1 meter. If the
  /// rendered scenes have a different scale, the property <see cref="MaxDistances"/> needs to be 
  /// adjusted.
  /// </para>
  /// <para>
  /// A different SSAO method with higher quality is implemented in the <see cref="SaoFilter"/>.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SsaoFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _farParameter;
    private readonly EffectParameter _radiusParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _maxDistancesParameter;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _gBuffer0Parameter;
    private readonly EffectParameter _occlusionTextureParameter;
    private readonly EffectPass _createLinesAPass;
    private readonly EffectPass _createLinesBPass;
    private readonly EffectPass _blurHorizontalPass;
    private readonly EffectPass _blurVerticalPass;
    private readonly EffectPass _combinePass;
    private readonly EffectPass _copyPass;

    private readonly Blur _blur;

    private readonly CopyFilter _copyFilter;
    private readonly DownsampleFilter _downsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the strength of the ambient occlusion.
    /// </summary>
    /// <value>
    /// The strength. 0 means, not ambient occlusion. 1 means, full ambient occlusion. 
    /// </value>
    public float Strength { get; set; }


    /// <summary>
    /// Gets or sets the inner and outer ambient occlusion radius.
    /// </summary>
    /// <value>
    /// The ambient occlusion radii, given as a vector with 2 components: (inner radius, outer 
    /// radius). The values are relative to the screen size. The default value is (0.01, 0.02). 
    /// </value>
    /// <remarks>
    /// Depending on the current <see cref="Quality"/>, the SSAO shader samples occlusion at 
    /// one or two distances around a pixel. A smaller radius creates smaller and more pronounced
    /// ambient occlusion shadows. A larger radius creates a wider and softer shadow. With high 
    /// quality settings, the shader samples ambient occlusion at two radii to catch details
    /// and soft occlusion shadows.
    /// </remarks>
    public Vector2F Radii { get; set; }


    /// <summary>
    /// Gets or sets the scale factors.
    /// </summary>
    /// <value>
    /// The scale factors, given as a vector with 2 components: (min scale factor, max scale 
    /// factor). The default is (0.5, 2).
    /// </value>
    /// <remarks>
    /// The shader uses random sample offsets around each pixel to sample occlusion. Each sample
    /// offset is scaled by a random value. <see cref="Scale"/> defines the min and max size of
    /// this random scale factor. For example: If <see cref="Scale"/> is (1, 1), then the sample
    /// offsets are not scaled. If <see cref="Scale"/> is (0.5, 2), then the random offsets
    /// are scaled with random values between 0.5 and 2.
    /// </remarks>
    public Vector2F Scale { get; set; }


    /// <summary>
    /// Gets or sets the max distances for ambient occlusion.
    /// </summary>
    /// <value>
    /// The max distances, given as a vector with 2 components: (max distance for the inner radius,
    /// max distance for the outer radius). The values are specified in world space units. The 
    /// default is (0.5, 1.0).
    /// </value>
    /// <remarks>
    /// <para>
    /// To avoid dark halos around objects, the ambient occlusion disappears if the distance between
    /// a shaded point and its occluder is greater than <see cref="MaxDistances"/>. 
    /// <see cref="MaxDistances"/> allows to define separate max distance values for the inner and
    /// the outer sampling radius (see <see cref="Radii"/>).
    /// </para>
    /// <para>
    /// The max distances are given in world space units. The default values are suitable for
    /// typical scenes, where 1 world space unit = 1 meter. The values need to be adjusted if the
    /// scene uses a different scale.
    /// </para>
    /// </remarks>
    public Vector2F MaxDistances { get; set; }


    /// <summary>
    /// Gets or sets the quality level.
    /// </summary>
    /// <value>
    /// The quality level in the range [0, 2]. 0 means, "no ambient occlusion". Higher value
    /// create a higher quality effect. The default is 2. 
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 0 or greater than 2.
    /// </exception>
    public int Quality
    {
      get { return _quality; }
      set
      {
        if (value < 0 || value > 2)
          throw new ArgumentOutOfRangeException("value", "SsaoFilter.Quality must not be less than 0 or greater than 2.");

        _quality = value;
      }
    }
    private int _quality;


    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. This value must be greater than 0. The default value is 2.
    /// </value>
    /// <remarks>
    /// To improve performance, ambient occlusion is computed on a downsampled depth buffer. 
    /// The width and height of the depth buffer are downsampled by this factor.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int DownsampleFactor
    {
      get { return _downsampleFactor; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "SsaoFilter.DownsampleFactor must be greater than 0.");

        _downsampleFactor = value;
      }
    }
    private int _downsampleFactor;


    /// <summary>
    /// Gets or sets the number of blur passes.
    /// </summary>
    /// <value>
    /// The number of blur passes. The default value is 1.
    /// </value>
    /// <remarks>
    /// The ambient occlusion shader creates a noisy ambient occlusion texture. For better quality,
    /// this noisy image is blurred. <see cref="NumberOfBlurPasses"/> defines the number of 
    /// blur passes that are applied to the ambient occlusion texture.
    /// </remarks>
    public int NumberOfBlurPasses { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the ambient occlusion should be blurred using
    /// an edge-aware blur.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an edge-aware blur should be used; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If <see cref="NumberOfBlurPasses"/> is greater than 0, then the ambient occlusion buffer
    /// is blurred. An edge-aware blur avoids dark halos around objects that could be created when
    /// dark ambient occlusion shadows are blurred beyond visible edges. If
    /// <see cref="UseEdgeAwareBlur"/> is <see langword="false"/>, a normal blur is used which
    /// creates a better blur but could lead to artifacts like dark halos. 
    /// </remarks>
    public bool UseEdgeAwareBlur { get; set; }


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
    public SsaoFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SsaoFilter");
      _farParameter = _effect.Parameters["Far"];
      _radiusParameter = _effect.Parameters["Radius"];
      _strengthParameter = _effect.Parameters["Strength"];
      _maxDistancesParameter = _effect.Parameters["MaxDistances"];
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _gBuffer0Parameter = _effect.Parameters["GBuffer0"];
      _occlusionTextureParameter = _effect.Parameters["OcclusionTexture"];
      _createLinesAPass = _effect.CurrentTechnique.Passes["CreateLinesA"];
      _createLinesBPass = _effect.CurrentTechnique.Passes["CreateLinesB"];
      _blurHorizontalPass = _effect.CurrentTechnique.Passes["BlurHorizontal"];
      _blurVerticalPass = _effect.CurrentTechnique.Passes["BlurVertical"];
      _combinePass = _effect.CurrentTechnique.Passes["Combine"];
      _copyPass = _effect.CurrentTechnique.Passes["Copy"];

      Radii = new Vector2F(0.01f, 0.02f);
      MaxDistances = new Vector2F(0.5f, 1.0f);
      Strength = 1f;
      NumberOfBlurPasses = 1;
      DownsampleFactor = 2;
      Quality = 2;
      Scale = new Vector2F(0.5f, 2f);
      CombineWithSource = true;

      _blur = new Blur(graphicsService);
      _blur.InitializeGaussianBlur(7, 7 / 3, true);

      _copyFilter = PostProcessHelper.GetCopyFilter(graphicsService);
      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);

      Random random = new Random(123456);
      Vector3[] vectors = new Vector3[9];

      // 16 random vectors for Crytek-style point samples.
      //for (int i = 0; i < vectors.Length; i++)
      //  vectors[i] = (Vector3)random.NextQuaternionF().Rotate(Vector3F.One).Normalized;
      //    //* random.NextFloat(0.5f, 1) // Note: StarCraft 2 uses varying length to vary the sample offset length.

      // We create rotated random vectors with uniform distribution in 360°. Each random vector
      // is further rotated with small random angle.
      float jitterAngle = ConstantsF.TwoPi / vectors.Length / 4;
      for (int i = 0; i < vectors.Length; i++)
        vectors[i] = (Vector3)(Matrix33F.CreateRotationZ(ConstantsF.TwoPi * i / vectors.Length + random.NextFloat(-jitterAngle, jitterAngle)) * new Vector3F(1, 0, 0)).Normalized;

      // Permute randomVectors.
      for (int i = 0; i < vectors.Length; i++)
        MathHelper.Swap(ref vectors[i], ref vectors[random.Next(i, vectors.Length - 1)]);

      // Scale random vectors.
      for (int i = 0; i < vectors.Length; i++)
        vectors[i].Z = random.NextFloat(Scale.X, Scale.Y);

      _effect.Parameters["RandomVectors"].SetValue(vectors);
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

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;
      var cameraNode = context.CameraNode;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      if (Quality == 0)
      {
        // No ambient occlusion.
        if (!CombineWithSource)
        {
          // CombineWithSource is not set. --> Simply clear the render target to white.
          graphicsDevice.SetRenderTarget(target);
          graphicsDevice.Viewport = viewport;
          graphicsDevice.Clear(Color.White);
        }
        else
        {
          // Copy source image to target.
          _copyFilter.Process(context);
        }
        return;
      }

      // Try to get downsampled depth buffer from render context.
      // If we cannot find it in the render context, we downsample it manually.
      Texture2D downsampledDepthTexture = null;
      RenderTarget2D downsampledDepthTarget = null;
      if (DownsampleFactor == 2)
      {
        object dummy;
        if (context.Data.TryGetValue(RenderContextKeys.DepthBufferHalf, out dummy))
          downsampledDepthTexture = dummy as Texture2D;
      }

      if (downsampledDepthTexture == null)
      {
        context.ThrowIfGBuffer0Missing();

        if (DownsampleFactor == 1)
        {
          downsampledDepthTexture = context.GBuffer0;
        }
        else
        {
          // Downsample manually.
          // If we do not downsample the depth target, we get artifacts (strange horizontal and vertical
          // lines). TODO: Check what causes the artifacts and try to remove the downsampling.
          downsampledDepthTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(
            context.GBuffer0.Width / DownsampleFactor,
            context.GBuffer0.Height / DownsampleFactor,
            false,
            context.GBuffer0.Format,
            DepthFormat.None));
          context.SourceTexture = context.GBuffer0;
          context.RenderTarget = downsampledDepthTarget;
          context.Viewport = new Viewport(0, 0, downsampledDepthTarget.Width, downsampledDepthTarget.Height);
          _downsampleFilter.Process(context);
          downsampledDepthTexture = downsampledDepthTarget;
        }
      }

      // We use two temporary render targets.
      // We do not use a floating point format because float textures cannot use hardware filtering.

      RenderTarget2D temp0;
      if (!CombineWithSource && target != null
          && target.Width == context.GBuffer0.Width / DownsampleFactor
          && target.Height == context.GBuffer0.Height / DownsampleFactor
          && Strength < 1)
      {
        // If we do not have to combine the AO result with the source image, and if the target
        // image has the half resolution, then we can use the target image directly and do not
        // need a temporary render target.
        // Also, a Strength > 1 is always applied in a separate pass because applying a Strength 
        // > 1 before the blur has no effect.

        temp0 = target;
      }
      else
      {
        temp0 = renderTargetPool.Obtain2D(new RenderTargetFormat(
          context.GBuffer0.Width / DownsampleFactor,
          context.GBuffer0.Height / DownsampleFactor,
          false,
          SurfaceFormat.Color,
          DepthFormat.None));
      }

      // Create SSAO.
      graphicsDevice.SetRenderTarget(temp0);
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _farParameter.SetValue(cameraNode.Camera.Projection.Far);
      _radiusParameter.SetValue((Vector2)Radii);
      _maxDistancesParameter.SetValue((Vector2)MaxDistances);
      _strengthParameter.SetValue(Strength < 1 ? Strength : 1);
      _gBuffer0Parameter.SetValue(downsampledDepthTexture);
      if (Quality == 1)
        _createLinesAPass.Apply();
      else
        _createLinesBPass.Apply();

      graphicsDevice.DrawFullScreenQuad();

      if (UseEdgeAwareBlur)
      {
        RenderTarget2D temp1 = renderTargetPool.Obtain2D(new RenderTargetFormat(
          context.GBuffer0.Width / DownsampleFactor,
          context.GBuffer0.Height / DownsampleFactor,
          false,
          SurfaceFormat.Color,
          DepthFormat.None));

        for (int i = 0; i < NumberOfBlurPasses; i++)
        {
          // Blur horizontally.
          // Note: We use a bilateral filter which is not separable - but the results are still ok
          // if we use separate the horizontal and vertical blur.
          graphicsDevice.SetRenderTarget(temp1);
          _occlusionTextureParameter.SetValue(temp0);
          _blurHorizontalPass.Apply();
          graphicsDevice.DrawFullScreenQuad();

          // Blur vertically.
          graphicsDevice.SetRenderTarget(temp0);
          _occlusionTextureParameter.SetValue(temp1);
          _blurVerticalPass.Apply();
          graphicsDevice.DrawFullScreenQuad();
        }

        // A few render targets are not needed anymore.
        renderTargetPool.Recycle(downsampledDepthTarget);
        renderTargetPool.Recycle(temp1);
      }
      else
      {
        // A few render targets are not needed anymore.
        renderTargetPool.Recycle(downsampledDepthTarget);

        context.SourceTexture = temp0;
        context.RenderTarget = temp0;
        context.Viewport = new Viewport(0, 0, temp0.Width, temp0.Height);
        for (int i = 0; i < NumberOfBlurPasses; i++)
          _blur.Process(context);
      }

      _strengthParameter.SetValue(Strength > 1 ? Strength : 1);

      if (CombineWithSource)
      {
        if (TextureHelper.IsFloatingPointFormat(source.Format))
          graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

        // Combine with scene.
        graphicsDevice.SetRenderTarget(target);
        graphicsDevice.Viewport = viewport;
        _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        _occlusionTextureParameter.SetValue(temp0);
        _sourceTextureParameter.SetValue(source);
        _combinePass.Apply();
        graphicsDevice.DrawFullScreenQuad();
      }
      else
      {
        if (temp0 != target)
        {
          graphicsDevice.SetRenderTarget(target);
          graphicsDevice.Viewport = viewport;
          _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
          _occlusionTextureParameter.SetValue(temp0);
          _copyPass.Apply();
          graphicsDevice.DrawFullScreenQuad();
        }
      }

      // Clean-up
      if (temp0 != target)
        renderTargetPool.Recycle(temp0);

      _sourceTextureParameter.SetValue((Texture2D)null);
      _gBuffer0Parameter.SetValue((Texture2D)null);
      _occlusionTextureParameter.SetValue((Texture2D)null);
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }
    #endregion
  }
}
#endif
