// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Adds crepuscular rays ("god rays") to a scene.
  /// </summary>
  public class GodRayFilter : PostProcessor
  {
    // Notes:
    // Implementation similar to GPU Gems 3 article "Volumetric Light Scattering
    // as a Post-Process" with following changes:
    // - Only a circular area around the light source is blurred.
    // - The depth buffer is used to mask the original scene. Black pixels are created
    //   for occluders where the depth is < 1. Only the sky with depth = 1 will generate
    //   light shafts.
    // - Linear decay instead of exponential decay. The original exponential decay behaves
    //   poorly because it does not take sample distance or number of samples into account.
    //  
    //
    // Ideas for improvement:
    // - Use SourceTexture alpha as occlusion mask. This allows to handle clouds and
    //   alpha-blended objects.
    // - Combine with scene in last blur pass instead of separate pass.
    //
    // Optimizations for mobile:
    // See "Bringing AAA graphics to mobile platforms", Niklas Smedberg, Epic, GDC 2012


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _parameters0Parameter;
    private readonly EffectParameter _parameters1Parameter;
    private readonly EffectParameter _intensityParameter;
    private readonly EffectParameter _numberOfSamplesParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _gBuffer0Parameter;
    private readonly EffectParameter _rayTextureParameter;
    private readonly EffectPass _createMaskPass;
    private readonly EffectPass _blurPass;
    private readonly EffectPass _combinePass;

    private readonly DownsampleFilter _downsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the scale of the light shafts.
    /// </summary>
    /// <value>The sampling density. The default value is 1.</value>
    /// <remarks>
    /// If this value is 1, we sample at equidistant points to the light source. If this value
    /// is less than 1, we sample at half of this range but with the same number of samples.
    /// Lowering this value creates shorter light shafts with higher sampling density.
    /// </remarks>
    public float Scale { get; set; }


    /// <summary>
    /// Gets or sets the weight of samples along the light ray.
    /// </summary>
    /// <value>The weight of samples along the light ray. The default value is 0.5.</value>
    [Obsolete("This property is not used anymore.")]
    public float Weight { get; set; }


    /// <summary>
    /// Gets or sets the decay factor.
    /// </summary>
    /// <value>The decay factor. The default value is 0.9.</value>
    /// <remarks>
    /// If this value is 1, all samples have equal weight. If this value is less than
    /// 1, samples further away from the current pixel will have less influence.
    /// </remarks>
    [Obsolete("This property is not used anymore.")]
    public float Decay { get; set; }


    /// <summary>
    /// Gets or sets the exposure.
    /// </summary>
    /// <value>The exposure. The default value is 0.1.</value>
    /// <remarks>
    /// This is an overall factor that applied to the light shaft image.
    /// </remarks>
    [Obsolete("Property Exposure is obsolete. Use property Intensity instead.")]
    public float Exposure { get; set; }


    /// <summary>
    /// Gets or sets the light direction.
    /// </summary>
    /// <value>The normalized light direction.</value>
    /// <remarks>
    /// The direction of the light rays (pointing away from the light source).
    /// </remarks>
    public Vector3F LightDirection { get; set; }


    /// <summary>
    /// Gets or sets the light radius.
    /// </summary>
    /// <value>The light radius. The default value is 0.2.</value>
    /// <remarks>
    /// <para>
    /// The light radius determines the size of the area around the sun from which light shafts
    /// originate. The light radius is relative to the screen height, i.e. if the light
    /// radius is 1 then it is equal to the screen height.
    /// </para>
    /// </remarks>
    public float LightRadius { get; set; }


    /// <summary>
    /// Gets or sets the intensity.
    /// </summary>
    /// <value>The intensity. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This is an overall factor that applied to the light shaft image. It can be used like
    /// an exposure factor or a tint color.
    /// </remarks>
    public Vector3F Intensity { get; set; }


    /// <summary>
    /// Gets or sets the downsample factor.
    /// </summary>
    /// <value>
    /// The downsample factor. This value must be greater than 0. The default value is 4.
    /// </value>
    /// <remarks>
    /// To improve performance, god rays are computed on a downsampled depth buffer. The width and
    /// height of the depth buffer are downsampled by this factor.
    /// </remarks>
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
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>The number of samples.</value>
    /// <remarks>
    /// To create light shafts, a ray from the current pixel to the light source
    /// is sampled. This value determines the number of samples.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public int NumberOfSamples
    {
      get { return _numberOfSamples; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The number of samples must not be negative.");

        _numberOfSamples = value;
      }
    }
    private int _numberOfSamples;


    /// <summary>
    /// Gets or sets the number of blur passes.
    /// </summary>
    /// <value>
    /// The number of blur passes. The default value is 2.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public int NumberOfPasses
    {
      get { return _numberOfPasses; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The number of passes must not be negative.");

        _numberOfPasses = value;
      }
    }
    private int _numberOfPasses;


    /// <summary>
    /// Gets or sets the softness.
    /// </summary>
    /// <value>The softness.</value>
    /// <remarks>
    /// If this value is 0, then the god rays are added using additive blending. If this value is
    /// 1, then the god rays are added using the softer "Screen" blending.
    /// </remarks>
    public float Softness { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GodRayFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public GodRayFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      Effect effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/GodRayFilter");
      _viewportSizeParameter = effect.Parameters["ViewportSize"];
      _parameters0Parameter = effect.Parameters["Parameters0"];
      _parameters1Parameter = effect.Parameters["Parameters1"];
      _intensityParameter = effect.Parameters["Intensity"];
      _numberOfSamplesParameter = effect.Parameters["NumberOfSamples"];
      _sourceTextureParameter = effect.Parameters["SourceTexture"];
      _gBuffer0Parameter = effect.Parameters["GBuffer0"];
      _rayTextureParameter = effect.Parameters["RayTexture"];
      _createMaskPass = effect.CurrentTechnique.Passes["CreateMask"];
      _blurPass = effect.CurrentTechnique.Passes["Blur"];
      _combinePass = effect.CurrentTechnique.Passes["Combine"];

      _downsampleFilter = graphicsService.GetDownsampleFilter();

      Scale = 1;
      LightDirection = new Vector3F(0, -1, 0);
      LightRadius = 0.2f;
      Intensity = new Vector3F(1, 1, 1);
      DownsampleFactor = 4;
      NumberOfSamples = 8;
      NumberOfPasses = 2;
      Softness = 1;
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
      var renderTargetPool = GraphicsService.RenderTargetPool;

      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      // Get temporary render targets.
      var sourceSize = new Vector2F(source.Width, source.Height);
      var isFloatingPointFormat = TextureHelper.IsFloatingPointFormat(source.Format);

      var sceneFormat = new RenderTargetFormat(source.Width, source.Height, false, source.Format, DepthFormat.None);
      var maskedScene = renderTargetPool.Obtain2D(sceneFormat);

      var rayFormat = new RenderTargetFormat(
        Math.Max(1, (int)(sourceSize.X / DownsampleFactor)),
        Math.Max(1, (int)(sourceSize.Y / DownsampleFactor)),
        false,
        source.Format,
        DepthFormat.None);
      var rayImage0 = renderTargetPool.Obtain2D(rayFormat);
      var rayImage1 = renderTargetPool.Obtain2D(rayFormat);

      // Get view and view-projection transforms.
      var cameraNode = context.CameraNode;
      Matrix44F projection = cameraNode.Camera.Projection.ToMatrix44F();
      Matrix44F view = cameraNode.View;
      Matrix44F viewProjection = projection * view;

      // We simply place the light source "far away" in opposite light ray direction.
      Vector4F lightPositionWorld = new Vector4F(-LightDirection * 10000, 1);

      // Convert to clip space.
      Vector4F lightPositionProj = viewProjection * lightPositionWorld;
      Vector3F lightPositionClip = Vector4F.HomogeneousDivide(lightPositionProj);

      // Convert from clip space [-1, 1] to texture space [0, 1].
      Vector2 lightPosition = new Vector2(lightPositionClip.X * 0.5f + 0.5f, -lightPositionClip.Y * 0.5f + 0.5f);

      // We use dot²(forward, -LightDirection) as a smooth S-shaped attenuation
      // curve to reduce the god ray effect when we look orthogonal or away from the sun.
      var lightDirectionView = view.TransformDirection(LightDirection);
      float z = Math.Max(0, lightDirectionView.Z);
      float attenuation = z * z;

      // Common effect parameters.
      _parameters0Parameter.SetValue(new Vector4(lightPosition.X, lightPosition.Y, LightRadius * LightRadius, Scale));
      _parameters1Parameter.SetValue(new Vector2(Softness, graphicsDevice.Viewport.AspectRatio));
      _intensityParameter.SetValue((Vector3)Intensity * attenuation);
      _numberOfSamplesParameter.SetValue(NumberOfSamples);
      _gBuffer0Parameter.SetValue(context.GBuffer0);

      // First, create a scene image where occluders are black.
      graphicsDevice.SetRenderTarget(maskedScene);
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(source);
      graphicsDevice.SamplerStates[0] = isFloatingPointFormat ? SamplerState.PointClamp : SamplerState.LinearClamp;
      graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;   // G-Buffer 0.
      _createMaskPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Downsample image.
      context.SourceTexture = maskedScene;
      context.RenderTarget = rayImage0;
      context.Viewport = new Viewport(0, 0, rayImage0.Width, rayImage0.Height);
      _downsampleFilter.Process(context);

      // Compute light shafts.
      _viewportSizeParameter.SetValue(new Vector2(context.Viewport.Width, context.Viewport.Height));
      graphicsDevice.SamplerStates[0] = isFloatingPointFormat ? SamplerState.PointClamp : SamplerState.LinearClamp;
      for (int i = 0; i < NumberOfPasses; i++)
      {
        graphicsDevice.SetRenderTarget(rayImage1);
        _sourceTextureParameter.SetValue(rayImage0);
        _blurPass.Apply();
        graphicsDevice.DrawFullScreenQuad();

        // Put the current result in variable rayImage0.
        MathHelper.Swap(ref rayImage0, ref rayImage1);
      }

      // Combine light shaft image with scene.
      graphicsDevice.SetRenderTarget(target);
      graphicsDevice.Viewport = viewport;
      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(source);
      _rayTextureParameter.SetValue(rayImage0);
      graphicsDevice.SamplerStates[0] = isFloatingPointFormat ? SamplerState.PointClamp : SamplerState.LinearClamp;
      graphicsDevice.SamplerStates[1] = isFloatingPointFormat ? SamplerState.PointClamp : SamplerState.LinearClamp;
      _combinePass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // Clean-up
      _sourceTextureParameter.SetValue((Texture2D)null);
      _gBuffer0Parameter.SetValue((Texture2D)null);
      _rayTextureParameter.SetValue((Texture2D)null);
      renderTargetPool.Recycle(maskedScene);
      renderTargetPool.Recycle(rayImage0);
      renderTargetPool.Recycle(rayImage1);
      context.SourceTexture = source;
      context.RenderTarget = target;
      context.Viewport = viewport;
    }
    #endregion
  }
}
#endif
