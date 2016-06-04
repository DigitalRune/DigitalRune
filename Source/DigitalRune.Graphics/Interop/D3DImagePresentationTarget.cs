// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#if MONOGAME
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
#endif


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// A WPF control that implements <see cref="IPresentationTarget"/> to host a 3D view.
  /// (Only available in MonoGame-compatible builds.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="D3DImagePresentationTarget"/> is derived from the WPF <see cref="Image"/> 
  /// element and hosts a <see cref="D3DImage"/> as the image source.
  /// </para>
  /// <para>
  /// The <see cref="D3DImage"/> manages two buffers: the <i>back buffer</i> and the <i>front
  /// buffer</i>. DigitalRune Graphics renders content into the back buffer using Direct3D 11. WPF
  /// copies the content of the back buffer into the front buffer and displays the result using
  /// Direct3D 9. See <see cref="D3DImage"/> documentation for additional information.
  /// </para>
  /// <para>
  /// When content is rendered into the back buffer, the back buffer is automatically marked as
  /// dirty to notify WPF that the front buffer needs to be updated. You can also manually mark
  /// the back buffer as dirty to trigger an update of the front buffer. The following example shows
  /// how to mark all registered D3DImages as dirty.
  /// </para>
  /// <code lang="csharp" title="Mark D3DImage dirty to update the front buffer">
  /// <![CDATA[
  /// foreach (var presentationTarget in _graphicsService.PresentationTargets)
  /// {
  ///     var d3dImagePresentationTarget = presentationTarget as D3DImagePresentationTarget;
  ///     if (d3dImagePresentationTarget != null && d3dImagePresentationTarget.IsVisible)
  ///     {
  ///         var d3DImage = (D3DImage)d3dImagePresentationTarget.Source;
  ///         d3DImage.Lock();
  ///         d3DImage.AddDirtyRect(new Int32Rect(0, 0, d3DImage.PixelWidth, d3DImage.PixelHeight));
  ///         d3DImage.Unlock();
  ///     }
  /// }
  /// ]]>
  /// </code>
  /// <para>
  /// It is usually not necessary to manually trigger the update of the front buffer. However, if
  /// you run into synchronization problems, as described below, this may be relevant.
  /// </para>
  /// <para>
  /// <strong>Synchronization:</strong><br/>
  /// By default, the Direct3D 11 device used for rendering is not synchronized with Direct3D 9
  /// (WPF). Direct3D 11 and Direct3D 9 run asynchronously. The image shown in WPF may be one or
  /// more frames old. Setting <see cref="IsSynchronized"/> ensures that the devices run in sync
  /// and that WPF always shows the current frame.
  /// </para>
  /// <para>
  /// However, synchronizing Direct3D 11 and Direct3D 9 is costly. When switching from Direct3D 11
  /// to Direct3D 9 (and vice versa) the CPU needs to be blocked until the GPU is flushed. This
  /// reduces performance. Therefore, synchronization is disabled by default.
  /// </para>
  /// <para>
  /// To avoid synchronization problems, you need to apply one of these 3 solutions:
  /// </para>
  /// <list type="number">
  /// <item>
  /// <term>Continuous rendering</term>
  /// <description>
  /// Most interactive applications continuously render new frames - usually 30 or 60 frames per
  /// second. In this case, synchronization is not a problem: The image shown in WPF may lag one or
  /// more frames behind, but WPF will eventually catch up and show the new frame.
  /// (<see cref="IsSynchronized"/> should be set to <see langword="false"/> for optimal
  /// performance!)
  /// </description>
  /// </item>
  /// <item>
  /// <term>Enable synchronization</term>
  /// <description>
  /// Explicit synchronization can be enabled by setting <see cref="IsSynchronized"/> to
  /// <see langword="true"/>. This ensures that WPF always shows the new frame. However, as
  /// mentioned above, this synchronization greatly reduces performance. The CPU is blocked until
  /// the Direct3D 11 device has finished rendering the new frame.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Manual invalidation</term>
  /// <description>
  /// There is a third solution that does not require continuously rendering new frames or using
  /// explicit synchronization: Render the new frame and then poll <see cref="IsFrameReady"/>. When
  /// this property returns true, manually trigger an update of the front buffer, as described
  /// above.
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Clean-up:</strong><br/>
  /// Be sure to remove the <see cref="D3DImagePresentationTarget"/> from the
  /// <see cref="IGraphicsService"/> when it is no longer needed - usually when the element is
  /// <see cref="FrameworkElement.Unloaded"/>. This ensures that all unmanaged resources are
  /// properly disposed of.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "D3D11Image is disposed when presentation target is removed from graphics service.")]
  public class D3DImagePresentationTarget : Image, IPresentationTarget
  {
    // Note:
    // The synchronization problems no longer appear on Windows 10 + .NET 4.6.1 + AMD/Intel/Nvidia.
    // It seems that either Windows 10, .NET 4.6.1, or the Windows 10 drivers have changed the
    // GPU scheduling and fixed the problem. (But it could be that the problem is still there, just
    // far more seldom.)


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

#if MONOGAME
    private D3D11Image _d3D11Image;
#if !NET45
    private bool _isFrontBufferAvailable; // Locally cached value.
#endif
    private int _nativeWidth;
    private int _nativeHeight;
#endif
#endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

#if MONOGAME
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
    IntPtr IPresentationTarget.Handle
    {
      get { return IntPtr.Zero; }
    }


    /// <inheritdoc/>
    bool IPresentationTarget.IsVisible
    {
      get
      {
#if NET45
        // Ignore IsFrontBufferAvailable in .NET 4.5.
        return IsVisible;
#else
        return _isFrontBufferAvailable && IsVisible;
#endif
      }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Width
    {
      get { return (_d3D11Image != null) ? _d3D11Image.PixelWidth : 0; }
    }


    /// <inheritdoc/>
    int IPresentationTarget.Height
    {
      get { return (_d3D11Image != null) ? _d3D11Image.PixelWidth : 0; }
    }
#else
    /// <inheritdoc/>
    IGraphicsService IPresentationTarget.GraphicsService { get; set; }

    /// <summary>
    /// Gets the graphics service.
    /// </summary>
    /// <value>The graphics service.</value>
    /// <inheritdoc cref="IPresentationTarget.GraphicsService"/>
    public IGraphicsService GraphicsService { get { throw new NotImplementedException(); } }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IntPtr IPresentationTarget.Handle { get { throw new NotImplementedException(); } }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IPresentationTarget.IsVisible { get { throw new NotImplementedException(); } }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    int IPresentationTarget.Width { get { throw new NotImplementedException(); } }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    int IPresentationTarget.Height { get { throw new NotImplementedException(); } }
#endif

    /// <summary>
    /// Identifies the <see cref="EnableAlpha"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty EnableAlphaProperty = DependencyProperty.Register(
      "EnableAlpha",
      typeof(bool),
      typeof(D3DImagePresentationTarget),
      new PropertyMetadata(false, OnEnableAlphaChanged));

    /// <summary>
    /// Gets or sets a value indicating whether a render target with an alpha channel is created.
    /// This is a dependency property.
    /// </summary>
    /// <value>
    /// The a value indicating whether a render target with an alpha channel is created. The default
    /// value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// An alpha channel is required for transparency effects. If <see cref="EnableAlpha"/> is
    /// <see langword="false"/>, the 3D image is opaque and overwrites any WPF elements in the
    /// background. If <see cref="EnableAlpha"/> is <see langword="true"/>, the 3D image can contain
    /// transparent pixels. Transparent parts are blended with other WPF elements in the background.
    /// </para>
    /// <para>
    /// Changing this property invalidates the content and the 3D image needs to be re-rendered.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> WPF requires <i>premultiplied alpha</i>, which means that the
    /// color values in the 3D image needs to be multiplied by the alpha value!
    /// </para>
    /// </remarks>
    [Description("Gets or sets the a value indicating whether a render target with an alpha channel is created.")]
    [Category("Appearance")]
    public bool EnableAlpha
    {
      get { return (bool)GetValue(EnableAlphaProperty); }
      set { SetValue(EnableAlphaProperty, value); }
    }


    /// <summary>
    /// Identifies the <see cref="IsSynchronized"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSynchronizedProperty = DependencyProperty.Register(
      "IsSynchronized",
      typeof(bool),
      typeof(D3DImagePresentationTarget),
      new PropertyMetadata(false, OnIsSynchronizedChanged));

    /// <summary>
    /// Gets or sets a value indicating whether the Direct3D 11 device is synchronized with WPF.
    /// This is a dependency property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the Direct3D 11 device is synchronized with WPF; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// By default, the Direct3D 11 device used for rendering is not synchronized with Direct3D 9
    /// (WPF). Direct3D 11 and Direct3D 9 run asynchronously. The image shown in WPF may be one or
    /// more frames old. Setting <see cref="IsSynchronized"/> ensures that the devices run in sync
    /// and that WPF always shows to current frame.
    /// </para>
    /// <para>
    /// However, synchronizing Direct3D 11 and Direct3D 9 is costly. When switching from Direct3D 11
    /// to Direct3D 9 (and vice versa) the CPU needs to be blocked until the GPU is flushed. This
    /// reduces performance.
    /// </para>
    /// </remarks>
    [Description("Gets or sets a value indicating whether the Direct3D 11 device is synchronized with WPF.")]
    [Category("Misc")]
    public bool IsSynchronized
    {
      get { return (bool)GetValue(IsSynchronizedProperty); }
      set { SetValue(IsSynchronizedProperty, value); }
    }


    /// <summary>
    /// Gets a value indicating whether the Direct3D 11 device has finished rendering the current
    /// frame.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if Direct3D 11 has finished rendering; otherwise,
    /// <see langword="false"/> if the Direct3D 11 device is busy.
    /// </value>
    /// <remarks>
    /// <para>
    /// A return value of <see langword="true"/> guarantees that the current frame rendered to the
    /// back buffer is complete and ready to be copied to the front buffer in WPF.
    /// </para>
    /// <para>
    /// A return value of <see langword="false"/> means that the Direct3D 11 device is still busy.
    /// However, the back buffer might already be usable in WPF! (Since Direct3D 11 and Direct3D 9
    /// run asynchronously by default, there is a chance that the result is already finished when
    /// the Direct3D 9 device tries to access the shared back buffer!)
    /// </para>
    /// <para>
    /// When the application is continuously rendering new frames, for example, at 60 frames per
    /// second, it is not useful to query <see cref="IsFrameReady"/>. The flag is reset every time a
    /// new frame is started. Therefore, the value of the property will be <see langword="false"/>
    /// in most cases.
    /// </para>
    /// <para>
    /// When <see cref="IsSynchronized"/> is <see langword="true"/>, then Direct3D 9 and Direct3D 11
    /// run in sync and <see cref="IsFrameReady"/> will therefore always return
    /// <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool IsFrameReady
    {
#if MONOGAME
      get { return _d3D11Image == null || _d3D11Image.IsFrameReady(GraphicsService.GraphicsDevice); }
#else
      get { return false; }
#endif
    }


    private static readonly DependencyPropertyKey IsFrontBufferAvailablePropertyKey = DependencyProperty.RegisterReadOnly(
      "IsFrontBufferAvailable",
      typeof(bool),
      typeof(D3DImagePresentationTarget),
      new FrameworkPropertyMetadata(
        false,
        FrameworkPropertyMetadataOptions.None,
        OnIsFrontBufferAvailableChanged));

    /// <summary>
    /// Identifies the <see cref="IsFrontBufferAvailable"/> dependency property.
    /// </summary>
    /// <AttachedPropertyComments>
    /// <summary>
    /// Gets a value that indicates whether a front buffer exists for the internal <see cref="D3DImage" />.
    /// This is a dependency property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a front buffer exists for the internal <see cref="D3DImage"/>; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// </AttachedPropertyComments>
    public static readonly DependencyProperty IsFrontBufferAvailableProperty = IsFrontBufferAvailablePropertyKey.DependencyProperty;

    /// <summary>
    /// Gets a value that indicates whether a front buffer exists for the internal <see cref="D3DImage" />.
    /// This is a dependency property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a front buffer exists for the internal <see cref="D3DImage"/>; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    [Browsable(false)]
    public bool IsFrontBufferAvailable
    {
      get { return (bool)GetValue(IsFrontBufferAvailableProperty); }
#if MONOGAME
      private set
      {
        // DependencyProperties can only be read on UI thread, but
        // IPresentationTarget.IsVisible might be read from non-UI thread.
        // --> Cache value locally to avoid cross-thread exception.
#if !NET45
        _isFrontBufferAvailable = value;
#endif
        SetValue(IsFrontBufferAvailablePropertyKey, value);
      }
#endif
      }


    /// <summary>
    /// Occurs when the <see cref="IsFrontBufferAvailable"/> property value changed.
    /// </summary>
    public event DependencyPropertyChangedEventHandler IsFrontBufferAvailableChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="D3DImagePresentationTarget"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static D3DImagePresentationTarget()
    {
      SnapsToDevicePixelsProperty.OverrideMetadata(typeof(D3DImagePresentationTarget), new FrameworkPropertyMetadata(true));
      StretchProperty.OverrideMetadata(typeof(D3DImagePresentationTarget), new FrameworkPropertyMetadata(Stretch.Fill));
    }


#if MONOGAME
    /// <summary>
    /// Initializes a new instance of the <see cref="D3DImagePresentationTarget"/> class.
    /// </summary>
    public D3DImagePresentationTarget()
    {
      GetNativeSize();
    }
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="D3DImagePresentationTarget"/> class.
    /// </summary>
    public D3DImagePresentationTarget()
    {
      throw new NotImplementedException("Only available in MonoGame-compatible builds.");
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the <see cref="EnableAlpha"/> property changed.
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <param name="eventArgs">
    /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
    /// </param>
    private static void OnEnableAlphaChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      var target = (D3DImagePresentationTarget)dependencyObject;
      bool oldValue = (bool)eventArgs.OldValue;
      bool newValue = (bool)eventArgs.NewValue;
      target.OnEnableAlphaChanged(oldValue, newValue);
    }

    /// <summary>
    /// Called when the <see cref="EnableAlpha"/> property changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnEnableAlphaChanged(bool oldValue, bool newValue)
    {
#if MONOGAME
      if (GraphicsService != null)
      {
        UninitializeImageSource();
        InitializeImageSource(GraphicsService);
      }
#endif
    }


#if MONOGAME
    /// <summary>
    /// When overridden in a derived class, participates in rendering operations that are directed
    /// by the layout system. This method is invoked after layout update, and before rendering, if
    /// the element's <see cref="UIElement.RenderSize"/> has changed as a result of layout update.
    /// </summary>
    /// <param name="sizeInfo">
    /// The packaged parameters ( SizeChangedInfo), which includes old and new sizes, and which
    /// dimension actually changes.
    /// </param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      GetNativeSize();
    }


    private void GetNativeSize()
    {
      // ---- Determine native pixels size.
      // Get DPI scale (as of .NET 4.6.1, this returns the DPI of the primary monitor, if you have several different DPIs).
      double dpiScale = 1.0; // Default value for 96 dpi.
      var presentationSource = PresentationSource.FromVisual(this);
      if (presentationSource != null)
      {
        var compositionTarget = presentationSource.CompositionTarget as HwndTarget;
        if (compositionTarget != null)
          dpiScale = compositionTarget.TransformToDevice.M11;
      }

      Size size = RenderSize;
      _nativeWidth = (int)(size.Width < 0 ? 0 : Math.Ceiling(size.Width * dpiScale));
      _nativeHeight = (int)(size.Height < 0 ? 0 : Math.Ceiling(size.Height * dpiScale));
    }
#endif


    /// <summary>
    /// Called when the <see cref="IsSynchronized"/> property changed.
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <param name="eventArgs">
    /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
    /// </param>
    private static void OnIsSynchronizedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      var target = (D3DImagePresentationTarget)dependencyObject;
      bool oldValue = (bool)eventArgs.OldValue;
      bool newValue = (bool)eventArgs.NewValue;
      target.OnIsSynchronizedChanged(oldValue, newValue);
    }

    /// <summary>
    /// Called when the <see cref="IsSynchronized"/> property changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnIsSynchronizedChanged(bool oldValue, bool newValue)
    {
#if MONOGAME
      if (_d3D11Image != null)
        _d3D11Image.IsSynchronized = newValue;
#endif
    }


#if MONOGAME
    private void OnGraphicsServiceChanged(IGraphicsService oldGraphicsService, IGraphicsService newGraphicsService)
    {
      if (Dispatcher.CheckAccess())
        SwitchGraphicsService(oldGraphicsService, newGraphicsService);
      else
        Dispatcher.Invoke((Action)(() => SwitchGraphicsService(oldGraphicsService, newGraphicsService)));
    }


    private void SwitchGraphicsService(IGraphicsService oldGraphicsService, IGraphicsService newGraphicsService)
    {
      if (oldGraphicsService != null)
        UninitializeImageSource();

      if (newGraphicsService != null)
        InitializeImageSource(newGraphicsService);
    }


    private void InitializeImageSource(IGraphicsService graphicsService)
    {
      Debug.Assert(Dispatcher.CheckAccess(), "Method needs to be called on the thread associated with the current dispatcher.");
      Debug.Assert(_d3D11Image == null, "UninitializeImageSource() needs to be called before InitializeImageSource().");

      int width = Math.Max(_nativeWidth, 1);
      int height = Math.Max(_nativeHeight, 1);
      _d3D11Image = new D3D11Image(graphicsService.GraphicsDevice, width, height, EnableAlpha);
      _d3D11Image.IsSynchronized = IsSynchronized;
      _d3D11Image.IsFrontBufferAvailableChanged += OnD3DImageIsFrontBufferAvailableChanged;

      IsFrontBufferAvailable = _d3D11Image.IsFrontBufferAvailable;
      Source = _d3D11Image;
    }


    private void UninitializeImageSource()
    {
      Debug.Assert(Dispatcher.CheckAccess(), "Method needs to be called on the thread associated with the current dispatcher.");
      Debug.Assert(_d3D11Image != null, "InitializeImageSource() needs to be called before UninitializeImageSource().");

      _d3D11Image.IsFrontBufferAvailableChanged -= OnD3DImageIsFrontBufferAvailableChanged;
      Source = null;

      _d3D11Image.Dispose();
      _d3D11Image = null;

      IsFrontBufferAvailable = false;
    }


    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      if (!CheckAccess())
        throw new InvalidOperationException("Invalid cross-thread access. Method must be called on the UI thread.");

      var graphicsDevice = GraphicsService.GraphicsDevice;
      int width = Math.Max(_nativeWidth, 1);
      int height = Math.Max(_nativeHeight, 1);
      var renderTarget = _d3D11Image.BeginRender(graphicsDevice, width, height);
      if (renderTarget == null)
        return false;

      Debug.Assert(_d3D11Image.PixelWidth == width);
      Debug.Assert(_d3D11Image.PixelHeight == height);

      graphicsDevice.SetRenderTarget(renderTarget);
      context.RenderTarget = renderTarget;
      context.Viewport = new Viewport(0, 0, renderTarget.Width, renderTarget.Height);
      return true;
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      if (GraphicsService == null)
      {
        // WPF bug: D3DImage.Lock() blocks the UI thread. Other WPF events such as
        // drag-and-drop events may run on the UI thread while Lock() is blocking!
        // --> When we get here: An event handler has removed the D3DImagePresentationTarget.
        return;
      }

      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.SetRenderTarget(null);
      context.RenderTarget = null;

      _d3D11Image.EndRender(graphicsDevice);
    }


    private void OnD3DImageIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
    {
      IsFrontBufferAvailable = (bool)eventArgs.NewValue;
    }


    /// <summary>
    /// Takes a snapshot of the presentation target and stores it as a bitmap.
    /// </summary>
    /// <returns>The snapshot of the presentation target.</returns>
    public BitmapSource ToBitmap()
    {
      if (_d3D11Image == null)
        return null;

      // Copy back buffer to WriteableBitmap.
      int width = _d3D11Image.PixelWidth;
      int height = _d3D11Image.PixelHeight;
      var format = EnableAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
      var writeableBitmap = new WriteableBitmap(width, height, 96, 96, format, null);
      writeableBitmap.Lock();
      try
      {
        uint[] data = new uint[width * height];
        _d3D11Image.TryGetData(data);

        // Get a pointer to the back buffer.
        unsafe
        {
          uint* pBackbuffer = (uint*)writeableBitmap.BackBuffer;
          for (int i = 0; i < data.Length; i++)
            pBackbuffer[i] = data[i];
        }

        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
      }
      finally
      {
        writeableBitmap.Unlock();
      }

      return writeableBitmap;
    }
#else
    /// <inheritdoc/>
    bool IPresentationTarget.BeginRender(RenderContext context)
    {
      throw new NotImplementedException("Only available in MonoGame-compatible builds.");
    }


    /// <inheritdoc/>
    void IPresentationTarget.EndRender(RenderContext context)
    {
      throw new NotImplementedException("Only available in MonoGame-compatible builds.");
    }


    /// <summary>
    /// Takes a snapshot of the presentation target and stores it as a bitmap.
    /// </summary>
    /// <returns>The snapshot of the presentation target.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    public BitmapSource ToBitmap()
    {
      throw new NotImplementedException("Only available in MonoGame-compatible builds.");
    }
#endif


    /// <summary>
    /// Raises the <see cref="IsFrontBufferAvailableChanged"/> when the 
    /// <see cref="IsFrontBufferAvailable"/> property changed.
    /// </summary>
    /// <param name="dependencyObject">The dependency object.</param>
    /// <param name="eventArgs">
    /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
    /// </param>
    private static void OnIsFrontBufferAvailableChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      var presentationTarget = (D3DImagePresentationTarget)dependencyObject;
      var handler = presentationTarget.IsFrontBufferAvailableChanged;
      if (handler != null)
        handler(presentationTarget, eventArgs);
    }
    #endregion
  }
}
#endif
