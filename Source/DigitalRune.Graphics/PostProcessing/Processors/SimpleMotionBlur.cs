// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Creates a very simple motion blur effect by mixing the old blurred scene with the new scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This post-processor stores information of a frame for use in the next frame. The information
  /// is stored with the currently active camera node. When there is a cut in the scene (i.e. a new
  /// level is loaded or the view changes significantly), the method 
  /// <see cref="CameraNode.InvalidateViewDependentData()"/> of the camera node needs to be called
  /// to reset the information. Further, this post-processor expects that it is called once per
  /// frame for a certain camera node. It might not work as expected if it is called several times
  /// per frame (e.g. to process different views using the same camera).
  /// </para>
  /// </remarks>
  public class SimpleMotionBlur : PostProcessor
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

      // The render target with the old scene.
      public RenderTarget2D LastBlurredScene;

      public ViewDependentData(IGraphicsService graphicsService)
      {
        GraphicsService = graphicsService;
      }

      public void Dispose()
      {
        GraphicsService.RenderTargetPool.Recycle(LastBlurredScene);
        LastBlurredScene = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _lastSourceTextureParameter;

    private readonly CopyFilter _copyFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the strength of the blur effect in the range [0, 1[.
    /// </summary>
    /// <value>The strength. The default value 0.8.</value>
    public float Strength { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleMotionBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SimpleMotionBlur(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/SimpleMotionBlur");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _strengthParameter = _effect.Parameters["Strength"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _lastSourceTextureParameter = _effect.Parameters["LastSourceTexture"];

      _copyFilter = PostProcessHelper.GetCopyFilter(graphicsService);

      Strength = 0.8f;
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
    /// Resets the motion blur effect. (The next frame will not be blurred.)
    /// </summary>
    /// <remarks>
    /// This method should be called if there was a cut in the visual scene and the next frame
    /// should not be blurred.
    /// </remarks>
    [Obsolete("The method SimpleMotionBlur.Reset() is obsolete. Use CameraNode.InvalidateViewDependentData() instead.")]
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
      var source = context.SourceTexture;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      var tempFormat = new RenderTargetFormat(source.Width, source.Height, false, source.Format, DepthFormat.None);
      RenderTarget2D blurredScene = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);

      if (TextureHelper.IsFloatingPointFormat(source.Format))
      {
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
      }
      else
      {
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
      }

      context.RenderTarget = blurredScene;
      context.Viewport = new Viewport(0, 0, blurredScene.Width, blurredScene.Height);

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

      if (data.LastBlurredScene == null)
      {
        // This is the first frame. Simply remember the current source for the next frame.
        _copyFilter.Process(context);
      }
      else
      {
        // Create new blurred scene.
        graphicsDevice.SetRenderTarget(blurredScene);

        _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        _strengthParameter.SetValue(Strength);
        _sourceTextureParameter.SetValue(source);
        _lastSourceTextureParameter.SetValue(data.LastBlurredScene);
        _effect.CurrentTechnique.Passes[0].Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      // Copy blurredScene to target.
      context.SourceTexture = blurredScene;
      context.RenderTarget = target;
      context.Viewport = viewport;
      _copyFilter.Process(context);

      // Recycle old blurred scene and store new scene (switch render targets).
      GraphicsService.RenderTargetPool.Recycle(data.LastBlurredScene);
      data.LastBlurredScene = blurredScene;

      _sourceTextureParameter.SetValue((Texture2D)null);
      _lastSourceTextureParameter.SetValue((Texture2D)null);

      // Restore original context.
      context.SourceTexture = source;
    }
    #endregion
  }
}
#endif
