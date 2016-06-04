// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Performs scene post-processing, like filtering, color manipulation, etc.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A post-processor reads a source texture, which must be specified in the 
  /// <see cref="RenderContext.SourceTexture"/> property of the <see cref="RenderContext"/>. It 
  /// processes the source texture and writes its output to a render target, which must be specified
  /// in the <see cref="RenderContext.RenderTarget"/> property of the <see cref="RenderContext"/>.
  /// The render target can be <see langword="null"/> to write into the back buffer of the graphics
  /// device.
  /// </para>
  /// <para>
  /// The post-processor will always call 
  /// <see cref="GraphicsDevice"/>.<see cref="GraphicsDevice.SetRenderTarget(RenderTarget2D)"/>
  /// for the specified render target - that means it is not necessary that the render target is
  /// currently set in the graphics device.
  /// </para>
  /// <para>
  /// In general, the <see cref="RenderContext.SourceTexture"/> and the 
  /// <see cref="RenderContext.RenderTarget"/> must not reference the same render target. 
  /// Only if the post-processor uses multi-pass rendering internally, or if the post-processor is 
  /// a <see cref="PostProcessorChain"/> consisting of several post-processors, then the source 
  /// texture and the render target can reference the same object. That means, it depends on the
  /// used post-processor whether it is possible or not. If it is not possible, and the same render
  /// target is used as the source texture and the target, then XNA will throw an exception.
  /// </para>
  /// <para>
  /// Post-processors can be chained together: See class <see cref="PostProcessorChain"/> for more 
  /// information.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Maybe rename PostProcessor to Postprocessor in future version.")]
  public abstract class PostProcessor : IDisposable, INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this post-processor has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    public IGraphicsService GraphicsService { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether this post-processor is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>. The default value is 
    /// <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The methods <see cref="OnEnable"/> and <see cref="OnDisable"/> will be called automatically 
    /// when this property changes. 
    /// </para>
    /// </remarks>
    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        if (_enabled == value)
          return;

        _enabled = value;

        if (value)
          OnEnable();
        else
          OnDisable();
      }
    }
    private bool _enabled;


    /// <summary>
    /// Gets or sets the name of the post-processor.
    /// </summary>
    /// <value>The name of the post-processor.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the default target format.
    /// (This property is used by the <see cref="PostProcessorChain"/>).
    /// </summary>
    /// <value>
    /// The default target format. Per default, the <see cref="RenderTargetFormat.Width"/>,
    /// <see cref="RenderTargetFormat.Height"/> and <see cref="RenderTargetFormat.SurfaceFormat"/>
    /// are undefined. <see cref="RenderTargetFormat.Mipmap"/> is <see langword="false"/>, and
    /// <see cref="RenderTargetFormat.DepthStencilFormat"/> is <strong>DepthFormat.None</strong>.
    /// </value>
    /// <remarks>
    /// This property is used by the <see cref="PostProcessorChain"/> to choose the format of the
    /// intermediate render target if this processor is executed in the middle of the 
    /// <see cref="PostProcessorChain"/>. For example, this is used by the <see cref="HdrFilter"/>
    /// to convert a <strong>HdrBlendable</strong> input texture to an LDR <strong>Color</strong>
    /// (R8G8B8A8) texture.
    /// </remarks>
    public RenderTargetFormat DefaultTargetFormat { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PostProcessor"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    protected PostProcessor(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      GraphicsService = graphicsService;
      _enabled = true;  // Note: Virtual OnEnabled must not be called in constructor.
      DefaultTargetFormat = new RenderTargetFormat(null, null, false, null, DepthFormat.None);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="PostProcessor"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="PostProcessor"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Remove any view-dependent information from cameras.
          CameraNode.RemoveViewDependentData(this);
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when this post-processor is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called when the post-processor was previously disabled
    /// (<see cref="PostProcessor.Enabled"/> = <see langword="false"/>) and was set to enabled
    /// (<see cref="PostProcessor.Enabled"/> = <see langword="true"/>).
    /// </para>
    /// <para>
    /// Please note: Post-processors are enabled per default. <see cref="OnEnable"/> is not executed
    /// for new post-processors.
    /// </para>
    /// </remarks>
    protected virtual void OnEnable()
    {
    }


    /// <summary>
    /// Called when this post-processor is disabled.
    /// </summary>
    /// <remarks>
    /// This method is called when the post-processor was previously enabled
    /// (<see cref="PostProcessor.Enabled"/> = <see langword="true"/>) and was set to disabled
    /// (<see cref="PostProcessor.Enabled"/> = <see langword="false"/>).
    /// </remarks>
    protected virtual void OnDisable()
    {
    }


    /// <summary>
    /// Performs the post-processing using the <see cref="RenderContext.SourceTexture"/>
    /// and the <see cref="RenderContext.RenderTarget"/> specified in the render context.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// The <see cref="Process"/> method will automatically call <see cref="OnProcess"/>. 
    /// The method <see cref="OnProcess"/> needs to be implemented in derived class to perform the 
    /// post-processing.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>. 
    /// </exception>
    /// <exception cref="GraphicsException">
    /// <see cref="RenderContext.SourceTexture"/> is <see langword="null"/>. 
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public void Process(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      if (context.SourceTexture == null)
          throw new GraphicsException("Source texture is missing. The source texture must be set in RenderContext.SourceTexture.");

      if (!Enabled)
        return;

      ProcessInternal(context);
    }


    internal void ProcessInternal(RenderContext context)
    {
      Debug.Assert(Enabled, "PostProcessor.ProcessInternal should only be called when the post-processor is enabled.");

      var graphicsDevice = GraphicsService.GraphicsDevice;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      // Set render states. The blend state must be set by the user!
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      // Preform post-processing.
      OnProcess(context);

      savedRenderState.Restore();

      // Reset the texture stages. If a floating point texture is set, we get exceptions
        // when a sampler with bilinear filtering is set.
#if !MONOGAME
      graphicsDevice.ResetTextures();
#endif
    }


    /// <summary>
    /// Called when the post-processor should perform the post-processing.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// <para>
    /// This method is automatically called in <see cref="PostProcessor.Process"/>. It will not be 
    /// called if the post-processor is disabled, or if the specified <paramref name="context"/>
    /// or the <see cref="RenderContext.SourceTexture"/> is <see langword="null"/>. 
    /// </para>
    /// </remarks>
    protected abstract void OnProcess(RenderContext context);
    #endregion
  }
}
#endif
