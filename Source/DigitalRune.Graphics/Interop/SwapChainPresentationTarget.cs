// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xna.Framework.Graphics;
#endif


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Represents a UWP <strong>SwapChainPanel</strong> control that implements
  /// <see cref="IPresentationTarget"/> to host a 3D view. (Only available on the Universal Windows
  /// Platform.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Thread-safety:</strong><br/> The <see cref="SwapChainPresentationTarget"/> can be used
  /// if the game loop runs in a parallel thread. In this case, the property <see cref="Lock"/> must
  /// be set to an object that is locked when the game loop is running. The
  /// <see cref="SwapChainPresentationTarget"/> uses this lock when it access the graphics service.
  /// </para>
  /// </remarks>
  [CLSCompliant(false)]
  public class SwapChainPresentationTarget :
#if WINDOWS_UWP
    SwapChainPanel, 
#endif
    IPresentationTarget
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

#if WINDOWS_UWP
    private readonly object _dummyLock = new object();
    private bool _rendering;
    private SwapChainRenderTarget _renderTarget;
    private double _width;
    private double _height;
    private float _compositionScaleX;
    private float _compositionScaleY;
    private bool _requiresResize;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties and Events
    //--------------------------------------------------------------

#if WINDOWS_UWP
    /// <inheritdoc/>
    IGraphicsService IPresentationTarget.GraphicsService
    {
      get { return _graphicsService; }
      set
      {
        if (_graphicsService == value)
          return;

        var oldGraphicsService = _graphicsService;
        _graphicsService = value;

        OnGraphicsServiceChanged(oldGraphicsService, _graphicsService);
      }
    }
    private IGraphicsService _graphicsService;


    /// <inheritdoc/>
    IntPtr IPresentationTarget.Handle
    {
      get { return IntPtr.Zero; }
    }


    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <inheritdoc cref="IPresentationTarget.GraphicsService"/>
    public IGraphicsService GraphicsService
    {
      get { return _graphicsService; }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Width
    {
      get { return (int)Math.Ceiling(_width * _compositionScaleX); }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Height
    {
      get { return (int)Math.Ceiling(_height * _compositionScaleY); }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IPresentationTarget.IsVisible
    {
      get { return true; }
    }
#else
    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    IGraphicsService IPresentationTarget.GraphicsService
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
      set { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    IntPtr IPresentationTarget.Handle
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }


    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <inheritdoc cref="IPresentationTarget.GraphicsService"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    public IGraphicsService GraphicsService
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    int IPresentationTarget.Width
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    int IPresentationTarget.Height
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IPresentationTarget.IsVisible
    {
      get { throw new NotImplementedException("Only available on Universal Windows Platform."); }
    }
#endif


    /// <summary>
    /// Gets or sets the synchronization object that is used to lock operations on graphics data.
    /// </summary>
    /// <value>The lock object.</value>
    /// <remarks>
    /// This property must be set when the game loop runs in a parallel thread instead of the UI
    /// thread.
    /// </remarks>
    public object Lock { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

#if WINDOWS_UWP
    /// <summary>
    /// Initializes a new instance of the <see cref="SwapChainPresentationTarget"/> class.
    /// </summary>
    public SwapChainPresentationTarget()
    {
      Unloaded += OnUnloaded;
      SizeChanged += OnSizeChanged;
      CompositionScaleChanged += OnCompositionScaleChanged;
    }


    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      RemoveRenderTarget();
    }


    private void RemoveRenderTarget()
    {
      lock (Lock ?? _dummyLock)
      {
        _renderTarget.SafeDispose();
        _renderTarget = null;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if WINDOWS_UWP
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    private void OnGraphicsServiceChanged(IGraphicsService oldGraphicsService, IGraphicsService newGraphicsService)
    {
      if (oldGraphicsService != null)
      {
        RemoveRenderTarget();
      }
    }


    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      lock (Lock ?? _dummyLock)
      {
        _width = e.NewSize.Width;
        _height = e.NewSize.Height;
        _requiresResize = true;
      }
    }


    private void OnCompositionScaleChanged(SwapChainPanel sender, object args)
    {
      lock (Lock ?? _dummyLock)
      {
        _compositionScaleX = CompositionScaleX;
        _compositionScaleY = CompositionScaleY;
        _requiresResize = true;
      }
    }


    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      var graphicsDevice = GraphicsService.GraphicsDevice;

      if (_renderTarget == null
          || _renderTarget.GraphicsDevice != graphicsDevice
          || _requiresResize)
      {
        if (_renderTarget == null)
        {
          _renderTarget = new SwapChainRenderTarget(
            graphicsDevice,
            this,
            _width,
            _height,
            _compositionScaleX,
            _compositionScaleY,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24Stencil8,
            0,
            RenderTargetUsage.PreserveContents,
            PresentInterval.Default);
        }
        else
        {
          _renderTarget.Resize(_width, _height, _compositionScaleX, _compositionScaleY);
        }

        _requiresResize = false;
      }

      graphicsDevice.SetRenderTarget(_renderTarget);
      context.RenderTarget = _renderTarget;
      context.Viewport = new Viewport(0, 0, _renderTarget.Width, _renderTarget.Height);
      _rendering = true;
      return true;
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      if (!_rendering)
        return;

      try
      {
        var graphicsDevice = GraphicsService.GraphicsDevice;

        graphicsDevice.SetRenderTarget(null);
        context.RenderTarget = null;
        _renderTarget.Present();
      }
      catch (Exception)
      {
        // Do nothing.
      }
      finally
      {
        _rendering = false;
      }
    }
#else
    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      throw new NotImplementedException("Only available on Universal Windows Platform.");
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      throw new NotImplementedException("Only available on Universal Windows Platform.");
    }
#endif
    #endregion
  }
}
