// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Computes the minimum, average and maximum luminance of a texture.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The render target should be a 1 x 1 texture with 3 floating-point channels (e.g. 
  /// HalfVector4). The luminance info is stored in this order: (minimum, average, maximum). That
  /// means, the minimum luminance is stored in the "red" channel, and so on.
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
  /// </remarks>
  public class LuminanceFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// View-dependent information stored per camera node.
    /// </summary>
    private sealed class ViewDependentData : IDisposable
    {
      public readonly IGraphicsService GraphicsService;

      // The luminance render target with the old scene.
      public RenderTarget2D LastLuminance;

      public ViewDependentData(IGraphicsService graphicsService)
      {
        GraphicsService = graphicsService;
      }

      public void Dispose()
      {
        GraphicsService.RenderTargetPool.Recycle(LastLuminance);
        LastLuminance = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _useGeometricMeanParameter;
    private readonly EffectParameter _useAdaptionParameter;
    private readonly EffectParameter _deltaTimeParameter;
    private readonly EffectParameter _adaptionSpeedParameter;
    private readonly EffectParameter _lastLuminanceTextureParameter;
    private readonly EffectParameter _textureParameter;
    private readonly EffectParameter _sourceSizeParameter;
    private readonly EffectParameter _targetSizeParameter;
    private readonly EffectPass _createPass;
    private readonly EffectPass _downsamplePass;
    private readonly EffectPass _finalPass;

    private readonly DownsampleFilter _downsampleFilter;
    private readonly CopyFilter _copyFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the adaption speed of the eye.
    /// </summary>
    /// <value>The adaption speed in the range [0, ∞[. Use small values like 0.02.</value>
    public float AdaptionSpeed { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether dynamic eye adaption should be used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if dynamic eye adaption is used; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If dynamic eye adaption is used, the luminance of the previous frames will influence the 
    /// luminance values of the current frame. This can be used in HDR rendering to model the 
    /// behavior of the human eye behavior: The eye slowly adapts to new lighting conditions. 
    /// </remarks>
    public bool UseAdaption { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the average luminance is computed using the 
    /// geometric mean.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the geometric mean is used for the average luminance; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool UseGeometricMean { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminanceFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public LuminanceFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/LuminanceFilter");
      _useGeometricMeanParameter = _effect.Parameters["UseGeometricMean"];
      _useAdaptionParameter = _effect.Parameters["UseAdaption"];
      _deltaTimeParameter = _effect.Parameters["DeltaTime"];
      _adaptionSpeedParameter = _effect.Parameters["AdaptionSpeed"];
      _lastLuminanceTextureParameter = _effect.Parameters["LastLuminanceTexture"];
      _textureParameter = _effect.Parameters["SourceTexture"];
      _sourceSizeParameter = _effect.Parameters["SourceSize"];
      _targetSizeParameter = _effect.Parameters["TargetSize"];
      _createPass = _effect.CurrentTechnique.Passes["Create"];
      _downsamplePass = _effect.CurrentTechnique.Passes["Downsample"];
      _finalPass = _effect.CurrentTechnique.Passes["Final"];

      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);
      _copyFilter = PostProcessHelper.GetCopyFilter(graphicsService);

      UseGeometricMean = true;
      UseAdaption = true;
      AdaptionSpeed = 0.02f;

      DefaultTargetFormat = new RenderTargetFormat(1, 1, false, SurfaceFormat.HalfVector4, DepthFormat.None);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnDisable()
    {
      CameraNode.InvalidateViewDependentData(this);
    }


    /// <summary>
    /// Resets this luminance adaption.
    /// </summary>
    /// <remarks>
    /// This method should be called if there was a cut in the visual scene and the next frame is
    /// very different from the last frame.
    /// </remarks>
    [Obsolete("The method LuminanceFilter.Reset() is obsolete. Use CameraNode.InvalidateViewDependentData() instead.")]
    public void Reset()
    {
      CameraNode.InvalidateViewDependentData(this);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    protected override void OnProcess(RenderContext context)
    {
      context.ThrowIfCameraMissing();

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;
      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      RenderTarget2D temp128x128 = renderTargetPool.Obtain2D(
        new RenderTargetFormat(128, 128, false, SurfaceFormat.HalfVector4, DepthFormat.None));
      RenderTarget2D temp64x64 = renderTargetPool.Obtain2D(
        new RenderTargetFormat(64, 64, false, SurfaceFormat.HalfVector4, DepthFormat.None));
      RenderTarget2D luminance = renderTargetPool.Obtain2D(
        new RenderTargetFormat(1, 1, false, SurfaceFormat.HalfVector4, DepthFormat.None));

      // ----- Downsample scene into temp128x128.
      context.RenderTarget = temp128x128;
      context.Viewport = new Viewport(0, 0, temp128x128.Width, temp128x128.Height);
      _downsampleFilter.Process(context);

      _useGeometricMeanParameter.SetValue(UseGeometricMean);

      // Get view-dependent information stored in camera node.
      var cameraNode = context.CameraNode;
      object dummy;
      cameraNode.ViewDependentData.TryGetValue(this, out dummy);
      var data = dummy as ViewDependentData;
      if (data == null)
      {
        data = new ViewDependentData(GraphicsService);
        cameraNode.ViewDependentData[this] = data;
      }

      if (UseAdaption)
      {
        // Use adaption if required by user and if we already have luminance info.
        _useAdaptionParameter.SetValue(data.LastLuminance != null);
        _deltaTimeParameter.SetValue((float)context.DeltaTime.TotalSeconds);
        _adaptionSpeedParameter.SetValue(AdaptionSpeed);
        _lastLuminanceTextureParameter.SetValue(data.LastLuminance);
      }
      else
      {
        _useAdaptionParameter.SetValue(false);
        _lastLuminanceTextureParameter.SetValue((Texture2D)null);

        // Reset old luminance info.
        data.Dispose();
      }

      // ----- First downsample temp128x128 into temp64x64 and create luminance info.
      graphicsDevice.SetRenderTarget(temp64x64);
      _textureParameter.SetValue(temp128x128);
      _sourceSizeParameter.SetValue(new Vector2(temp128x128.Width, temp128x128.Height));
      _targetSizeParameter.SetValue(new Vector2(temp64x64.Width, temp64x64.Height));
      _createPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      // temp128x128 is not needed anymore.
      renderTargetPool.Recycle(temp128x128);

      // ----- Downsample luminance info.
      RenderTarget2D last = temp64x64;
      while (last.Width > 2)
      {
        Debug.Assert(last.Width == last.Height, "The render target must be quadratic");

        RenderTarget2D temp = renderTargetPool.Obtain2D(
          new RenderTargetFormat(last.Width / 2, last.Height / 2, false, last.Format, DepthFormat.None));

        graphicsDevice.SetRenderTarget(temp);
        _textureParameter.SetValue(last);
        _sourceSizeParameter.SetValue(new Vector2(last.Width, last.Height));
        _targetSizeParameter.SetValue(new Vector2(temp.Width, temp.Height));
        _downsamplePass.Apply();
        graphicsDevice.DrawFullScreenQuad();

        renderTargetPool.Recycle(last);
        last = temp;
      }

      // ----- Sample 'last' and store final info in 'luminance'.
      graphicsDevice.SetRenderTarget(luminance);
      _textureParameter.SetValue(last);
      _sourceSizeParameter.SetValue(new Vector2(last.Width, last.Height));
      _targetSizeParameter.SetValue(new Vector2(luminance.Width, luminance.Height));
      _finalPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      renderTargetPool.Recycle(last);

      // ----- Copy luminance to original context.RenderTarget.
      context.SourceTexture = luminance;
      context.RenderTarget = target;
      context.Viewport = viewport;
      _copyFilter.Process(context);

      // ----- Store luminance for next frame.
      renderTargetPool.Recycle(data.LastLuminance);
      data.LastLuminance = luminance;

      // Restore original context.
      context.SourceTexture = source;

      _textureParameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
