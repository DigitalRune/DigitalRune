#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  /// <summary>
  /// Renders <see cref="VolumetricLightNode"/>s into the current render target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Volumetric lights are created using raymarching in the shader: The shader computes the light
  /// intensities at several position along the view ray (within the camera space AABB of the light)
  /// and additively blends the result to the scene.
  /// </para>
  /// <para>
  /// <strong>Off-Screen Rendering:</strong><br/>
  /// Large amounts of volumetric lights or a high number of samples per light can reduce
  /// the frame rate, if the game is limited by the GPU's fill rate. One solution to this problem
  /// is to render volumetric lights into a low-resolution off-screen buffer. This reduces the
  /// amount of work per pixel, at the expense of additional image processing overhead and image
  /// quality.
  /// </para>
  /// <para>
  /// To enable off-screen rendering set the property <see cref="EnableOffscreenRendering"/> to
  /// <see langword="true"/>. In addition a low-resolution copy of the depth buffer (half width and
  /// height) needs to be stored in <c>renderContext.Data[RenderContextKey.DepthBufferHalf]</c>.
  /// </para>
  /// <para>
  /// In XNA off-screen rendering clears the current back buffer. If necessary the renderer will
  /// automatically rebuild the back buffer including the depth buffer. For the rebuild step it will
  /// use the same parameters (e.g. near and far bias) as the current
  /// <see cref="RebuildZBufferRenderer"/> stored in
  /// <c>renderContext.Data[RenderContextKey.RebuildZBufferRenderer]</c>.
  /// </para>
  /// <note type="warning">
  /// When off-screen rendering is enabled the <see cref="VolumetricLightRenderer"/> automatically 
  /// switches render targets and invalidates the current depth-stencil buffer.
  /// </note>
  /// </remarks>
  public class VolumetricLightRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The blend state for rendering volumetric effects into the off-screen buffer.
    /// </summary>
    private static readonly BlendState BlendStateOffscreen = new BlendState
    {
      ColorBlendFunction = BlendFunction.Add,
      ColorSourceBlend = Blend.One,
      ColorDestinationBlend = Blend.InverseSourceAlpha,

      // Separate alpha blend function (requires HiDef profile!).
      AlphaBlendFunction = BlendFunction.Add,
      AlphaSourceBlend = Blend.Zero,
      AlphaDestinationBlend = Blend.InverseSourceAlpha,
    };

    private readonly Vector3[] _frustumFarCorners = new Vector3[4];

    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterColor;
    private readonly EffectParameter _parameterNumberOfSamples;
    private readonly EffectParameter _parameterDepthInterval;
    private readonly EffectParameter _parameterLightDiffuse;
    private readonly EffectParameter _parameterLightPosition;
    private readonly EffectParameter _parameterLightRange;
    private readonly EffectParameter _parameterLightAttenuation;
    private readonly EffectParameter _parameterLightTexture;
    private readonly EffectParameter _parameterLightTextureMatrix;
    private readonly EffectParameter _parameterLightTextureMipMap;
    private readonly EffectParameter _parameterLightDirection;
    private readonly EffectParameter _parameterLightAngles;
    private readonly EffectParameter _parameterRandomSeed;
    private readonly EffectPass _passPointLight;
    private readonly EffectPass _passPointLightTextureRgb;
    private readonly EffectPass _passPointLightTextureAlpha;
    private readonly EffectPass _passSpotlight;
    private readonly EffectPass _passSpotlightTextureRgb;
    private readonly EffectPass _passSpotlightTextureAlpha;
    private readonly EffectPass _passProjectorLightTextureRgb;
    private readonly EffectPass _passProjectorLightTextureAlpha;
    private UpsampleFilter _upsampleFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether noise is animated.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if noise is animated; otherwise, <see langword="false" />.
    /// </value>
    /// <remarks>
    /// The effect uses noise to hide banding caused by a low number of samples.
    /// </remarks>
    public bool AnimateNoise { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether off-screen rendering is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if volumetric lights are rendered into an off-screen buffer;
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="EnableOffscreenRendering"/> is set, all volumetric lights are rendered into
    /// a low-resolution off-screen buffer. The final off-screen buffer is upscaled and combined
    /// with the scene.
    /// </para>
    /// <para>
    /// This option should be enabled if the amount of volumetric lights and/or the number of
    /// samples per volumetric light causes a frame rate drop.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> A downsampled version of the depth buffer (half width and
    /// height) needs to be stored in <c>renderContext.Data[RenderContextKey.DepthBufferHalf]</c>.
    /// </para>
    /// <para>
    /// When off-screen rendering is used, the hardware depth buffer information is lost. This
    /// renderer restores the depth buffer when it combines the off-screen buffer with the render
    /// target in the final step. The restored depth buffer is not totally accurate.
    /// For the rebuild step the renderer will use the same parameters (e.g. near and far bias) as 
    /// the current <see cref="RebuildZBufferRenderer"/> stored in 
    /// <c>renderContext.Data[RenderContextKey.RebuildZBufferRenderer]</c>.
    /// </para>
    /// </remarks>
    public bool EnableOffscreenRendering { get; set; }


    /// <summary>
    /// Gets or sets the depth threshold used for edge detection when upsampling the off-screen
    /// buffer.
    /// </summary>
    /// <value>The depth threshold in world space units. The default value is 1 unit.</value>
    /// <remarks>
    /// <para>
    /// When off-screen rendering is enabled (see <see cref="EnableOffscreenRendering"/>), the
    /// renderer uses bilinear interpolation when upsampling the off-screen buffer, except for edges
    /// where nearest-depth upsampling is used. The <see cref="DepthThreshold"/> is the threshold
    /// value used for edge detection.
    /// </para>
    /// <para>
    /// In general: A large depth threshold improves image quality, but can cause edge artifacts. A
    /// small depth threshold improves the quality at geometry edges, but may reduce quality at 
    /// non-edges.
    /// </para>
    /// </remarks>
    public float DepthThreshold { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VolumetricLightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public VolumetricLightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      // Load effect.
      var effect = graphicsService.Content.Load<Effect>("VolumetricLight");
      _parameterViewportSize = effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = effect.Parameters["FrustumCorners"];
      _parameterGBuffer0 = effect.Parameters["GBuffer0"];
      _parameterColor = effect.Parameters["Color"];
      _parameterNumberOfSamples = effect.Parameters["NumberOfSamples"];
      _parameterDepthInterval = effect.Parameters["DepthInterval"];
      _parameterLightDiffuse = effect.Parameters["LightDiffuse"];
      _parameterLightPosition = effect.Parameters["LightPosition"];
      _parameterLightRange = effect.Parameters["LightRange"];
      _parameterLightAttenuation = effect.Parameters["LightAttenuation"];
      _parameterLightTexture = effect.Parameters["LightTexture"];
      _parameterLightTextureMatrix = effect.Parameters["LightTextureMatrix"];
      _parameterLightTextureMipMap = effect.Parameters["LightTextureMipMap"];
      _parameterLightDirection = effect.Parameters["LightDirection"];
      _parameterLightAngles = effect.Parameters["LightAngles"];
      _parameterRandomSeed = effect.Parameters["RandomSeed"];
      _passPointLight = effect.Techniques[0].Passes["PointLight"];
      _passPointLightTextureRgb = effect.Techniques[0].Passes["PointLightTextureRgb"];
      _passPointLightTextureAlpha = effect.Techniques[0].Passes["PointLightTextureAlpha"];
      _passSpotlight = effect.Techniques[0].Passes["Spotlight"];
      _passSpotlightTextureRgb = effect.Techniques[0].Passes["SpotlightTextureRgb"];
      _passSpotlightTextureAlpha = effect.Techniques[0].Passes["SpotlightTextureAlpha"];
      _passProjectorLightTextureRgb = effect.Techniques[0].Passes["ProjectorLightTextureRgb"];
      _passProjectorLightTextureAlpha = effect.Techniques[0].Passes["ProjectorLightTextureAlpha"];

      AnimateNoise = true;
      DepthThreshold = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is VolumetricLightNode;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");
      if (order != RenderOrder.UserDefined)
        throw new NotImplementedException("Render order must be 'UserDefined'.");
      if (context.CameraNode == null)
        throw new GraphicsException("Camera node needs to be set in render context.");
      if (context.GBuffer0 == null)
        throw new GraphicsException("GBuffer0 needs to be set in render context.");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var viewport = context.Viewport;
      int width = viewport.Width;
      int height = viewport.Height;
      var renderTargetPool = graphicsService.RenderTargetPool;
      var cameraNode = context.CameraNode;
      var projection = cameraNode.Camera.Projection;
      Pose view = cameraNode.PoseWorld.Inverse;
      Pose cameraPose = cameraNode.PoseWorld;
      float near = projection.Near;
      float far = projection.Far;

      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // Save render state.
      var originalRasterizerState = graphicsDevice.RasterizerState;
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalBlendState = graphicsDevice.BlendState;

      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      RenderTarget2D offscreenBuffer = null;
      Texture depthBufferHalf = null;
      if (!EnableOffscreenRendering || context.RenderTarget == null)
      {
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        _parameterGBuffer0.SetValue(context.GBuffer0);
      }
      else
      {
        // Render at half resolution into off-screen buffer.
        width = Math.Max(1, width / 2);
        height = Math.Max(1, height / 2);

        graphicsDevice.BlendState = BlendStateOffscreen;

        offscreenBuffer = renderTargetPool.Obtain2D(
          new RenderTargetFormat(width, height, false, context.RenderTarget.Format, DepthFormat.None));
        graphicsDevice.SetRenderTarget(offscreenBuffer);
        graphicsDevice.Clear(Color.Black);

        // Get half-res depth buffer.
        object obj;
        if (context.Data.TryGetValue(RenderContextKeys.DepthBufferHalf, out obj)
            && obj is Texture2D)
        {
          depthBufferHalf = (Texture2D)obj;
          _parameterGBuffer0.SetValue(depthBufferHalf);
        }
        else
        {
          string message = "Downsampled depth buffer is not set in render context. (The downsampled "
                             + "depth buffer (half width and height) is required by the VolumetricLightRenderer "
                             + "to use half-res off-screen rendering. It needs to be stored in "
                             + "RenderContext.Data[RenderContextKeys.DepthBufferHalf].)";
          throw new GraphicsException(message);
        }
      }

      // Set global effect parameters.
      _parameterViewportSize.SetValue(new Vector2(width, height));

      var isHdrEnabled = context.RenderTarget != null && context.RenderTarget.Format == SurfaceFormat.HdrBlendable;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as VolumetricLightNode;
        if (node == null)
          continue;

        // VolumetricLightNode is visible in current frame.
        node.LastFrame = frame;

        // Effect parameters for volumetric light properties.
        _parameterColor.SetValue((Vector3)node.Color / node.NumberOfSamples);
        _parameterNumberOfSamples.SetValue(node.NumberOfSamples);
        _parameterLightTextureMipMap.SetValue((float)node.MipMapBias);

        // The volumetric light effect is created for the parent light node.
        var lightNode = node.Parent as LightNode;
        if (lightNode == null)
          continue;

        Pose lightPose = lightNode.PoseWorld;

        // Get start and end depth values of light AABB in view space.
        var lightAabbView = lightNode.Shape.GetAabb(lightNode.ScaleWorld, view * lightPose);
        var startZ = Math.Max(-lightAabbView.Maximum.Z, near) / far;
        var endZ = Math.Min(-lightAabbView.Minimum.Z / far, 1);
        _parameterDepthInterval.SetValue(new Vector2(startZ, endZ));

        // Get a rectangle that covers the light in screen space.
        var rectangle = GraphicsHelper.GetScissorRectangle(cameraNode, new Viewport(0, 0, width, height), lightNode);
        var texCoordTopLeft = new Vector2F(rectangle.Left / (float)width, rectangle.Top / (float)height);
        var texCoordBottomRight = new Vector2F(rectangle.Right / (float)width, rectangle.Bottom / (float)height);

        GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

        // Convert frustum far corners from view space to world space.
        for (int j = 0; j < _frustumFarCorners.Length; j++)
          _frustumFarCorners[j] = (Vector3)cameraPose.ToWorldDirection((Vector3F)_frustumFarCorners[j]);

        _parameterFrustumCorners.SetValue(_frustumFarCorners);

        Vector2 randomSeed = AnimateNoise ? new Vector2((float)MathHelper.Frac(context.Time.TotalSeconds))
                                          : new Vector2(0);
        _parameterRandomSeed.SetValue(randomSeed);

        // Set light parameters and apply effect pass.
        if (lightNode.Light is PointLight)
        {
          var light = (PointLight)lightNode.Light;

          float hdrScale = isHdrEnabled ? light.HdrScale : 1;
          _parameterLightDiffuse.SetValue((Vector3)light.Color * light.DiffuseIntensity * hdrScale);
          _parameterLightPosition.SetValue((Vector3)(lightPose.Position - cameraPose.Position));
          _parameterLightRange.SetValue(light.Range);
          _parameterLightAttenuation.SetValue(light.Attenuation);

          bool hasTexture = (light.Texture != null);
          if (hasTexture)
          {
            _parameterLightTexture.SetValue(light.Texture);

            // Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
            // cube map and objects or texts in it are mirrored.)
            var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
            _parameterLightTextureMatrix.SetValue((Matrix)(mirrorZ * lightPose.Inverse));
          }

          if (hasTexture)
          {
            if (light.Texture.Format == SurfaceFormat.Alpha8)
              _passPointLightTextureAlpha.Apply();
            else
              _passPointLightTextureRgb.Apply();
          }
          else
          {
            _passPointLight.Apply();
          }
        }
        else if (lightNode.Light is Spotlight)
        {
          var light = (Spotlight)lightNode.Light;

          float hdrScale = isHdrEnabled ? light.HdrScale : 1;
          _parameterLightDiffuse.SetValue((Vector3)light.Color * light.DiffuseIntensity * hdrScale);
          _parameterLightPosition.SetValue((Vector3)(lightPose.Position - cameraPose.Position));
          _parameterLightRange.SetValue(light.Range);
          _parameterLightAttenuation.SetValue(light.Attenuation);
          _parameterLightDirection.SetValue((Vector3)lightPose.ToWorldDirection(Vector3F.Forward));
          _parameterLightAngles.SetValue(new Vector2(light.FalloffAngle, light.CutoffAngle));

          bool hasTexture = (light.Texture != null);
          if (hasTexture)
          {
            _parameterLightTexture.SetValue(light.Texture);

            var proj = Matrix44F.CreatePerspectiveFieldOfView(light.CutoffAngle * 2, 1, 0.1f, 100);
            _parameterLightTextureMatrix.SetValue((Matrix)(GraphicsHelper.ProjectorBiasMatrix * proj * (lightPose.Inverse * new Pose(cameraPose.Position))));
          }

          if (hasTexture)
          {
            if (light.Texture.Format == SurfaceFormat.Alpha8)
              _passSpotlightTextureAlpha.Apply();
            else
              _passSpotlightTextureRgb.Apply();
          }
          else
          {
            _passSpotlight.Apply();
          }
        }
        else if (lightNode.Light is ProjectorLight)
        {
          var light = (ProjectorLight)lightNode.Light;

          float hdrScale = isHdrEnabled ? light.HdrScale : 1;
          _parameterLightDiffuse.SetValue((Vector3)light.Color * light.DiffuseIntensity * hdrScale);
          _parameterLightPosition.SetValue((Vector3)(lightPose.Position - cameraPose.Position));
          _parameterLightRange.SetValue(light.Projection.Far);
          _parameterLightAttenuation.SetValue(light.Attenuation);

          _parameterLightTexture.SetValue(light.Texture);

          _parameterLightTextureMatrix.SetValue((Matrix)(GraphicsHelper.ProjectorBiasMatrix * light.Projection * (lightPose.Inverse * new Pose(cameraPose.Position))));

          if (light.Texture.Format == SurfaceFormat.Alpha8)
            _passProjectorLightTextureAlpha.Apply();
          else
            _passProjectorLightTextureRgb.Apply();
        }
        else
        {
          continue;
        }

        // Draw a screen space quad covering the light.
        graphicsDevice.DrawQuad(rectangle);
      }

      _parameterGBuffer0.SetValue((Texture)null);
      _parameterLightTexture.SetValue((Texture)null);

      if (offscreenBuffer != null)
      {
        // ----- Combine off-screen buffer with scene.
        graphicsDevice.BlendState = BlendState.Opaque;

        // The previous scene render target is bound as texture.
        // --> Switch scene render targets!
        var sceneRenderTarget = context.RenderTarget;
        var renderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(sceneRenderTarget));
        context.SourceTexture = offscreenBuffer;
        context.RenderTarget = renderTarget;

        // Use the UpsampleFilter, which supports "nearest-depth upsampling".
        // (Nearest-depth upsampling is an "edge-aware" method that tries to
        // maintain the original geometry and prevent blurred edges.)
        if (_upsampleFilter == null)
        {
          _upsampleFilter = new UpsampleFilter(graphicsService);
          _upsampleFilter.Mode = UpsamplingMode.NearestDepth;
          _upsampleFilter.RebuildZBuffer = true;
        }

        _upsampleFilter.DepthThreshold = DepthThreshold;
        context.SceneTexture = sceneRenderTarget;

        _upsampleFilter.Process(context);

        context.SceneTexture = null;
        context.SourceTexture = null;
        renderTargetPool.Recycle(offscreenBuffer);
        renderTargetPool.Recycle(sceneRenderTarget);
      }

      // Restore render states.
      graphicsDevice.RasterizerState = originalRasterizerState;
      graphicsDevice.DepthStencilState = originalDepthStencilState;
      graphicsDevice.BlendState = originalBlendState;
    }
    #endregion
  }
}
#endif