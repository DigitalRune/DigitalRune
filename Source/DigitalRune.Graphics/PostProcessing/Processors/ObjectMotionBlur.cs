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
  /// Creates a motion blur using velocity buffers.
  /// </summary>
  /// <remarks>
  /// This effect needs two velocity buffers in the render context:
  /// (<see cref="RenderContextKeys.VelocityBuffer"/> and 
  /// <see cref="RenderContextKeys.LastVelocityBuffer"/> (optional).
  /// In each buffer a velocity vector is stored per pixel. The velocity vector shows the movement 
  /// (relative to texture space) that this pixel has moved. One velocity buffer should contain the 
  /// velocity vectors of the current frame and the other optional one the velocity vectors of the 
  /// last frame.
  /// </remarks>
  public class ObjectMotionBlur : PostProcessor
  {
    // Note:
    // This effect implements a motion blur effect using velocity buffers.
    // See DirectX 9 SDK Sample "PixelMotionBlur" or 
    // http://mynameismjp.wordpress.com/samples-tutorials-tools/motion-blur-sample/
    // For soft-edge motion blur, see the paper "A Reconstruction Filter for Plausible Motion Blur".


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _numberOfSamplesParameter;
    private readonly EffectParameter _velocityTextureParameter;
    private readonly EffectParameter _velocityTexture2Parameter;
    private readonly EffectParameter _maxBlurRadiusParameter;
    private readonly EffectParameter _sourceSizeParameter;
    private readonly EffectParameter _gBuffer0Parameter;
    private readonly EffectParameter _jitterTextureParameter;
    private readonly EffectParameter _softZExtentParameter;
    private readonly EffectPass _singlePass;
    private readonly EffectPass _dualPass;
    private readonly EffectPass _downsampleMaxParameter;
    private readonly EffectPass _downsampleMaxFromFloatBufferParameter;
    private readonly EffectPass _neighborMaxPass;
    private readonly EffectPass _softEdgePass;
    private readonly Texture2D _jitterTexture;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>
    /// The number of samples. It is recommended to use an odd number of samples when using the
    /// soft-edge blur. The default value is 9.
    /// </value>
    public float NumberOfSamples { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the edges of motion blurred objects should be 
    /// softened.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if edges should be softened; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Simple motion blur effects blur the moving objects but they do not soften the silhouette of 
    /// the moving object. If <see cref="SoftenEdges"/> is set to <see langword="true"/>, the 
    /// silhouette is blurred as well, which creates visually better results but costs more 
    /// performance.
    /// </remarks>
    public bool SoftenEdges { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether last velocity buffer should be used in addition to
    /// the current velocity buffer to expand the blurred region.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the velocity buffer of the last frame is used in addition to the
    /// current velocity buffer; otherwise, <see langword="false"/>. The default value is 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag is not relevant if <see cref="SoftenEdges"/> is <see langword="true"/>. If
    /// <see cref="UseLastVelocityBuffer"/> is <see langword="true"/>, the current 
    /// <see cref="RenderContext"/> should contain the velocity buffer of the last frame
    /// (property <see cref="RenderContextKeys.LastVelocityBuffer"/>). If the render context does 
    /// not contain the buffer, then the visual result is the same as if 
    /// <see cref="UseLastVelocityBuffer"/> is <see langword="false"/>.
    /// </remarks>
    public bool UseLastVelocityBuffer { get; set; }


    /// <summary>
    /// Gets or sets the max blur radius in pixels.
    /// </summary>
    /// <value>The max blur radius in pixels. The default value is 20.</value>
    public float MaxBlurRadius { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectMotionBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ObjectMotionBlur(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/ObjectMotionBlur");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _numberOfSamplesParameter = _effect.Parameters["NumberOfSamples"];
      _velocityTextureParameter = _effect.Parameters["VelocityTexture"];
      _velocityTexture2Parameter = _effect.Parameters["VelocityTexture2"];
      _maxBlurRadiusParameter = _effect.Parameters["MaxBlurRadius"];
      _sourceSizeParameter = _effect.Parameters["SourceSize"];
      _gBuffer0Parameter = _effect.Parameters["GBuffer0"];
      _jitterTextureParameter = _effect.Parameters["JitterTexture"];
      _softZExtentParameter = _effect.Parameters["SoftZExtent"];
      _singlePass = _effect.CurrentTechnique.Passes["Single"];
      _dualPass = _effect.CurrentTechnique.Passes["Dual"];
      _downsampleMaxParameter = _effect.CurrentTechnique.Passes["DownsampleMax"];
      _downsampleMaxFromFloatBufferParameter = _effect.CurrentTechnique.Passes["DownsampleMaxFromFloatBuffer"];
      _neighborMaxPass = _effect.CurrentTechnique.Passes["NeighborMax"];
      _softEdgePass = _effect.CurrentTechnique.Passes["SoftEdge"];
      _jitterTexture = NoiseHelper.GetGrainTexture(GraphicsService, 128);

      NumberOfSamples = 9;
      MaxBlurRadius = 20;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      // Get velocity buffers from render context.
      object value0, value1;
      context.Data.TryGetValue(RenderContextKeys.VelocityBuffer, out value0);
      context.Data.TryGetValue(RenderContextKeys.LastVelocityBuffer, out value1);
      var velocityBuffer0 = value0 as Texture2D;
      var velocityBuffer1 = value1 as Texture2D;

      if (velocityBuffer0 == null)
        throw new GraphicsException("VelocityBuffer needs to be set in the render context (RenderContext.Data[\"VelocityBuffer\"]).");

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      if (!SoftenEdges)
      {
        // ----- Motion blur using one or two velocity buffers
        _effect.CurrentTechnique = _effect.Techniques[0];
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        graphicsDevice.Viewport = context.Viewport;
        _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        _sourceTextureParameter.SetValue(context.SourceTexture);
        _numberOfSamplesParameter.SetValue((int)NumberOfSamples);

        _velocityTextureParameter.SetValue(velocityBuffer0);
        if (TextureHelper.IsFloatingPointFormat(velocityBuffer0.Format))
          graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

        if (velocityBuffer1 == null || !UseLastVelocityBuffer)
        {
          _singlePass.Apply();
        }
        else
        {
          _velocityTexture2Parameter.SetValue(velocityBuffer1);
          if (TextureHelper.IsFloatingPointFormat(velocityBuffer1.Format))
            graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
          else
            graphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;

          _dualPass.Apply();
        }
        graphicsDevice.DrawFullScreenQuad();
      }
      else
      {
        // ----- Advanced motion blur (based on paper "A Reconstruction Filter for Plausible Motion Blur")
        context.ThrowIfCameraMissing();
        context.ThrowIfGBuffer0Missing();

        // The width/height of the current velocity input.
        int sourceWidth;
        int sourceHeight;
        if (context.RenderTarget != null)
        {
          sourceWidth = velocityBuffer0.Width;
          sourceHeight = velocityBuffer0.Height;
        }
        else
        {
          sourceWidth = context.Viewport.Width;
          sourceHeight = context.Viewport.Height;
        }

        // The downsampled target width/height.
        int targetWidth = Math.Max(1, (int)(sourceWidth / MaxBlurRadius));
        int targetHeight = Math.Max(1, (int)(sourceHeight / MaxBlurRadius));

        var tempFormat = new RenderTargetFormat(targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.None);
        RenderTarget2D temp0 = renderTargetPool.Obtain2D(tempFormat);
        RenderTarget2D temp1 = renderTargetPool.Obtain2D(tempFormat);

        // ----- Downsample max velocity buffer
        _effect.CurrentTechnique = _effect.Techniques[0];
        _maxBlurRadiusParameter.SetValue(new Vector2(MaxBlurRadius / sourceWidth, MaxBlurRadius / sourceHeight));
        Texture2D currentVelocityBuffer = velocityBuffer0;
        bool isFinalPass;
        do
        {
          // Downsample to this target size.
          sourceWidth = Math.Max(targetWidth, sourceWidth / 2);
          sourceHeight = Math.Max(targetHeight, sourceHeight / 2);

          // Is this the final downsample pass?
          isFinalPass = (sourceWidth <= targetWidth && sourceHeight <= targetHeight);

          // Get temporary render target for intermediate steps.
          RenderTarget2D temp = null;
          if (!isFinalPass)
          {
            tempFormat.Width = sourceWidth;
            tempFormat.Height = sourceHeight;
            temp = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
          }

          graphicsDevice.SetRenderTarget(isFinalPass ? temp0 : temp);

          _sourceSizeParameter.SetValue(new Vector2(sourceWidth * 2, sourceHeight * 2));
          _viewportSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
          _velocityTextureParameter.SetValue(currentVelocityBuffer);
          if (TextureHelper.IsFloatingPointFormat(currentVelocityBuffer.Format))
            graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
          else
            graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

          if (currentVelocityBuffer == velocityBuffer0)
            _downsampleMaxFromFloatBufferParameter.Apply();
          else
            _downsampleMaxParameter.Apply();

          graphicsDevice.DrawFullScreenQuad();

          if (currentVelocityBuffer != velocityBuffer0)
            GraphicsService.RenderTargetPool.Recycle((RenderTarget2D)currentVelocityBuffer);

          currentVelocityBuffer = temp;
        } while (!isFinalPass);

        // ----- Compute max velocity of neighborhood.
        graphicsDevice.SetRenderTarget(temp1);
        _velocityTextureParameter.SetValue(temp0);
        _neighborMaxPass.Apply();
        graphicsDevice.DrawFullScreenQuad();

        renderTargetPool.Recycle(temp0);

        // Compute motion blur.
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        graphicsDevice.Viewport = context.Viewport;
        _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        _sourceTextureParameter.SetValue(context.SourceTexture);
        _numberOfSamplesParameter.SetValue((int)NumberOfSamples);
        _velocityTextureParameter.SetValue(velocityBuffer0);
        if (TextureHelper.IsFloatingPointFormat(velocityBuffer0.Format))
          graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

        _velocityTexture2Parameter.SetValue(temp1);
        if (TextureHelper.IsFloatingPointFormat(temp1.Format))
          graphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[2] = SamplerState.LinearClamp;

        _gBuffer0Parameter.SetValue(context.GBuffer0);
        _jitterTextureParameter.SetValue(_jitterTexture);
        _softZExtentParameter.SetValue(0.01f / context.CameraNode.Camera.Projection.Far);  // 1 mm to 10 cm.
        _softEdgePass.Apply();
        graphicsDevice.DrawFullScreenQuad();

        _sourceTextureParameter.SetValue((Texture2D)null);
        _velocityTextureParameter.SetValue((Texture2D)null);
        _velocityTexture2Parameter.SetValue((Texture2D)null);
        _gBuffer0Parameter.SetValue((Texture2D)null);

        renderTargetPool.Recycle(temp1);
      }
    }
    #endregion
  }
}
#endif
