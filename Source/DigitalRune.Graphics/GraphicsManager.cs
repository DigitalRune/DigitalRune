// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Interop;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#if PORTABLE || WINDOWS_UWP
#pragma warning disable 1574  // Disable warning "XML comment has cref attribute that could not be resolved."
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages graphics-related objects, like graphics screens and presentation targets, and graphics
  /// resources.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class implements the <see cref="IGraphicsService"/> for a 3D application or game. (See
  /// also <see cref="IGraphicsService"/> for more information.)
  /// </para>
  /// <para>
  /// The method <see cref="Update"/> must be called once per frame to update the graphics service
  /// and the registered graphics screens. The methods <see cref="Render(bool)"/> or
  /// <see cref="Render(IPresentationTarget)"/> must be called to render the screens.
  /// <see cref="Render(bool)"/> renders the screens to the back buffer; the XNA game will
  /// automatically display this back buffer in the game window.
  /// </para>
  /// <para>
  /// The method <see cref="Render(IPresentationTarget)"/> or
  /// <see cref="Render(IPresentationTarget,IList{GraphicsScreen})"/> can be used to display
  /// graphics in a Windows Forms or WPF application. These methods render the graphics screens into
  /// a <i>presentation target</i>. A presentation target (see interface
  /// <see cref="IPresentationTarget"/>) is a Windows Forms control or a WPF control where the
  /// graphics can be displayed.
  /// </para>
  /// <para>
  /// <strong>Windows Forms:</strong>
  /// Windows Forms applications can host a <see cref="FormsPresentationTarget"/>. The methods
  /// <see cref="Render(IPresentationTarget)"/> or
  /// <see cref="Render(IPresentationTarget,IList{GraphicsScreen})"/> can be used to render a scene
  /// into this presentation target. The method first renders the graphics screens into the back
  /// buffer and then displays the result in the specified presentation target. The method
  /// <see cref="Present"/> can be used to display the current back buffer content in a presentation
  /// target (without re-rendering the screens).
  /// </para>
  /// <para>
  /// <strong>WPF:</strong>
  /// WPF applications can host an <see cref="ElementPresentationTarget"/> (legacy) or a
  /// <see cref="D3DImagePresentationTarget"/> (recommended, MonoGame only).
  /// The methods <see cref="Render(IPresentationTarget)"/> or
  /// <see cref="Render(IPresentationTarget,IList{GraphicsScreen})"/> can be used to render a scene
  /// directly into a <see cref="D3DImagePresentationTarget"/>.
  /// </para>
  /// </remarks>
  public class GraphicsManager : IGraphicsService, IDisposable
  {
    // Notes:
    // An old version of the GraphicsManager inherited from the XNA GraphicsDeviceManager.
    // Using the GraphicsDeviceManager is not good because this class is different on 
    // other platforms, like SL5, SLXNA.

    // TODO: Allow updating GraphicsScreens in parallel?
    // TODO: Use other constructors for Silverlight, XNA+SL on WP7.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<GraphicsScreen> _tempScreens = new List<GraphicsScreen>();
    private RenderContext _context;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <inheritdoc/>
    public ContentManager Content { get; private set; }


    /// <inheritdoc/>
    public RenderTargetPool RenderTargetPool { get; private set; }


    /// <inheritdoc/>
    public GraphicsDevice GraphicsDevice { get; private set; }


    /// <inheritdoc/>
    public GraphicsScreenCollection Screens { get; private set; }


    /// <inheritdoc/>
    public object GameForm { get; set; }


    /// <inheritdoc/>
    public PresentationTargetCollection PresentationTargets { get; private set; }


    //    /// <summary>
    //    /// Gets or sets a value indicating whether multithreading is enabled.
    //    /// </summary>
    //    /// <value>
    //    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    //    /// default value is <see langword="false"/>.
    //    /// </value>
    //    /// <remarks>
    //    /// <para>
    //    /// When multithreading is enabled the graphics manager will update the graphics screens
    //    /// in parallel on multiple threads to improve performance. Multithreading requires that
    //    /// the <see cref="GraphicsScreen.Update"/> of the graphics screens is thread-safe and can
    //    /// be executed in parallel. In general, graphics screens can depend on other graphics screens
    //    /// or access shared data, therefore multithreading is not enabled per default.
    //    /// </para>
    //    /// <para>
    //    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    //    /// current system has more than one CPU core and if the other cores are not fully utilized by
    //    /// the application. Multithreading should be disabled if the system has only one CPU core or
    //    /// if all other CPU cores are busy. In some cases it might be necessary to run a benchmark of
    //    /// the application and compare the performance with and without multithreading to decide 
    //    /// whether multithreading should be enabled or not.
    //    /// </para>
    //    /// <para>
    //    /// The graphics manager internally uses the class <see cref="Parallel"/> for parallelization.
    //    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    //    /// how the workload is distributed among the worker threads and more. (See 
    //    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    //    /// </para>
    //    /// </remarks>
    //    /// <seealso cref="Parallel"/>
    //#if XNA
    //    [ContentSerializer(Optional = true)]
    //#endif
    //    public bool EnableMultithreading { get; set; }


    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Per default, the collection contains a <see cref="StockEffectInterpreter"/>,
    /// a <see cref="DefaultEffectInterpreter"/>, a <see cref="SceneEffectInterpreter"/> and a 
    /// <see cref="Dxsas08EffectInterpreter"/>.
    /// </para>
    /// <para>
    /// Effect interpreters at the start of the collection have higher priority.
    /// </para>
    /// </remarks>
    public EffectInterpreterCollection EffectInterpreters { get; private set; }


    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// Per default, the collection contains a <see cref="StockEffectBinder"/>, a
    /// <see cref="DefaultEffectBinder"/>, and a <see cref="SceneEffectBinder"/>.
    /// </para>
    /// <para>
    /// Effect binders at the start of the collection have higher priority.
    /// </para>
    /// </remarks>
    public EffectBinderCollection EffectBinders { get; private set; }


    /// <inheritdoc/>
    public Dictionary<string, object> Data { get; private set; }


    /// <inheritdoc/>
    public TimeSpan Time { get; set; }


    /// <inheritdoc/>
    public TimeSpan DeltaTime { get; private set; }

    /// <inheritdoc/>
    public int Frame { get; private set; }


    internal ShapeMeshCache ShapeMeshCache { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsManager"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsManager"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="gameWindow">
    /// The game window in Windows. <see langword="null"/> on non-Windows platforms (Xbox 360, 
    /// Windows Phone 7, etc.).
    /// </param>
    /// <param name="content">
    /// The content manager that can be used to load predefined DigitalRune Graphics content (e.g. 
    /// post-processing effects, lookup textures, etc.).
    /// </param>
    /// <remarks>
    /// Use this constructor in Windows if <see cref="PresentationTargets"/> are used.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="content"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public GraphicsManager(GraphicsDevice graphicsDevice, GameWindow gameWindow, ContentManager content)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (content == null)
        throw new ArgumentNullException("content");

      GraphicsDevice = graphicsDevice;
      graphicsDevice.DeviceResetting += OnGraphicsDeviceResetting;
      //graphicsDevice.DeviceReset += OnGraphicsDeviceReset;
      GraphicsDevice.Disposing += OnGraphicsDeviceDisposing;

      Content = content;

      RenderTargetPool = new RenderTargetPool(this);
      Screens = new GraphicsScreenCollection();

      if (gameWindow != null)
        GameForm = PlatformHelper.GetForm(gameWindow.Handle);

      PresentationTargets = new PresentationTargetCollection();
      PresentationTargets.CollectionChanged += OnPresentationTargetsChanged;

      EffectInterpreters = new EffectInterpreterCollection
      {
        new StockEffectInterpreter(),
        new DefaultEffectInterpreter(),
        new SceneEffectInterpreter(),
#if !WINDOWS_PHONE && !XBOX360
        new TerrainEffectInterpreter(),
#endif
        new Dxsas08EffectInterpreter(),
      };
      EffectBinders = new EffectBinderCollection
      {
        new StockEffectBinder(),
        new DefaultEffectBinder(this),
        new SceneEffectBinder(),
#if !WINDOWS_PHONE && !XBOX360
        new TerrainEffectBinder(),
#endif
      };

      Data = new Dictionary<string, object>();
      Frame = -1;
      ShapeMeshCache = new ShapeMeshCache(this);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsManager"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="content">
    /// The content manager that can be used to load predefined DigitalRune Graphics content
    /// (e.g. post-processing effects, lookup textures, etc.).
    /// </param>
    public GraphicsManager(GraphicsDevice graphicsDevice, ContentManager content)
      : this(graphicsDevice, null, content)
    {
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="GraphicsManager"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="GraphicsManager"/> class
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
          RenderTargetPool.Dispose();
          ShapeMeshCache.Dispose();
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    /// <summary>
    /// Creates a new render context.
    /// </summary>
    /// <returns>The render context.</returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Derived classes can override this method to return a
    /// custom render context. The base implementation returns a new instance of 
    /// <see cref="RenderContext"/>.
    /// </remarks>
    protected virtual RenderContext CreateRenderContext()
    {
      return new RenderContext(this);
    }


    private void OnGraphicsDeviceResetting(object sender, EventArgs eventArgs)
    {
      RenderTargetPool.Clear();

      // Reset the texture stages. If a floating point texture is set, we get exceptions
      // when a sampler with bilinear filtering is set.
      GraphicsDevice.ResetTextures();
    }


    //private void OnGraphicsDeviceReset(object sender, EventArgs eventArgs)
    //{
    //}


    private void OnGraphicsDeviceDisposing(object sender, EventArgs eventArgs)
    {
      // Clean up resources.
      RenderTargetPool.Clear();

      // Dispose custom data. - Don't do this. Because the device will be
      // reset and automatically recreated when the game window moves between
      // screens for example...
      //foreach(var entry in Data)
      //  if (entry.Value is IDisposable)
      //    ((IDisposable)entry.Value).Dispose();
    }


    /// <summary>
    /// Called when presentation targets are added or removed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="CollectionChangedEventArgs{T}"/> instance containing the event data.</param>
    private void OnPresentationTargetsChanged(object sender, CollectionChangedEventArgs<IPresentationTarget> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Update IPresentationTarget.GraphicsService property.
      Debug.Assert(eventArgs.Action == CollectionChangedAction.Add
        || eventArgs.Action == CollectionChangedAction.Clear
        || eventArgs.Action == CollectionChangedAction.Remove
        || eventArgs.Action == CollectionChangedAction.Replace,
        "Unexpected CollectionChangedAction.");

      if (eventArgs.OldItems != null)
      {
        foreach (var item in eventArgs.OldItems)
        {
          item.GraphicsService = null;
        }
      }

      if (eventArgs.NewItems != null)
      {
        foreach (var item in eventArgs.NewItems)
        {
          if (item.GraphicsService != null)
            throw new GraphicsException("Cannot add presentation target. The presentation target has already been added to another graphics service.");

          item.GraphicsService = this;
        }
      }
    }


    private static void CopyScreens(IList<GraphicsScreen> source, List<GraphicsScreen> target)
    {
      Debug.Assert(source != null, "Source list must not be null.");
      Debug.Assert(target != null, "Target list must not be null.");
      Debug.Assert(target.Count == 0, "Target list expected to be empty.");

      int numberOfScreens = source.Count;
      for (int i = 0; i < numberOfScreens; i++)
      {
        var screen = source[i];
        if (screen != null)
          target.Add(source[i]);
      }
    }


    /// <summary>
    /// Updates the graphics service and the registered graphics screens.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// <para>
    /// This methods
    /// </para>
    /// <list type="bullet">
    /// <item>updates <see cref="DeltaTime"/>,</item>
    /// <item>increments <see cref="Frame"/>,</item>
    /// <item>
    /// updates the graphics screens, i.e. calls <see cref="GraphicsScreen.Update"/> of all graphics
    /// screens registered in the <see cref="Screens"/> collection
    /// </item>
    /// <item>and performs other internal tasks.</item>
    /// </list>
    /// <para>
    /// This method needs to be called once per frame before calling <see cref="Render(bool)"/>.
    /// </para>
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      ThrowIfDisposed();

      if (_context == null)
        _context = CreateRenderContext();

      DeltaTime = deltaTime;
      Time += deltaTime;

      _context.DeltaTime = deltaTime;
      _context.Time = Time;

      Frame = (Frame < int.MaxValue) ? Frame + 1 : 0;
      _context.Frame = Frame;

      try
      {
        // Create temporary list because original collection may be modified during update.
        CopyScreens(Screens, _tempScreens);

        // Update graphics screens.
        int numberOfScreens = _tempScreens.Count;
        for (int i = numberOfScreens - 1; i >= 0; i--)
        {
          var screen = _tempScreens[i];
          screen.Update(deltaTime);

          if (screen.IsVisible
              && screen.Coverage == GraphicsScreenCoverage.Full
              && !screen.RenderPreviousScreensToTexture)
          {
            // The current screen occludes all screens in the background.
            break;
          }
        }

        RenderTargetPool.Update();
        ShapeMeshCache.Update();
      }
      finally
      {
        // If GraphicsScreen.Update() throws, let's at least clear _tempScreens, so we can try again
        // in the next frame.
        _tempScreens.Clear();
      }
    }


    /// <overloads>
    /// <summary>
    /// Renders the graphics screens to the back buffer or another presentation target.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Renders the graphics screens to the back buffer.
    /// </summary>
    /// <param name="forceRendering">
    /// If set to <see langword="true"/> the screens are rendered even if the game window is
    /// currently hidden. If set to <see langword="false"/>, the rendering is skipped if the game
    /// window is currently not visible.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the graphics screens were rendered; <see langword="false"/> if 
    /// rendering was skipped because the game window is currently not visible.
    /// </returns>
    /// <remarks>
    /// The graphics screens are rendered to the back buffer using the viewport which is currently
    /// set in the graphics device.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public bool Render(bool forceRendering)
    {
      ThrowIfDisposed();

      if (_context == null)
        throw new GraphicsException("GraphicsManager.Update(TimeSpan) has not been called. The graphics service needs to be updated before content can be rendered.");

      _context.PresentationTarget = null;

      var originalViewport = GraphicsDevice.Viewport;
      _context.Viewport = originalViewport;
      _context.RenderTarget = null;

      try
      {
        // Draw scene for game window.
        if (forceRendering || GameForm == null || PlatformHelper.IsFormVisible(GameForm))
        {
          RenderScreens(Screens);
          return true;
        }

        return false;
      }
      finally
      {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Viewport = originalViewport;
      }
    }


    /// <summary>
    /// Renders the registered graphics screens into the given presentation target.
    /// </summary>
    /// <param name="presentationTarget">The presentation target.</param>
    /// <returns>
    /// <see langword="true"/> if the graphics screens where rendered; <see langword="false"/> if
    /// rendering was skipped because the presentation target is currently not visible or invalid.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="presentationTarget"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public bool Render(IPresentationTarget presentationTarget)
    {
      return Render(presentationTarget, Screens);
    }


    /// <summary>
    /// Renders the specified graphics screens into the given presentation target.
    /// </summary>
    /// <param name="presentationTarget">The presentation target.</param>
    /// <param name="screens">The graphics screens to be rendered.</param>
    /// <returns>
    /// <see langword="true"/> if the graphics screens where rendered; <see langword="false"/> if
    /// rendering was skipped because the presentation target is currently not visible or invalid.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="presentationTarget"/> or <paramref name="screens"/> is
    /// <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public bool Render(IPresentationTarget presentationTarget, IList<GraphicsScreen> screens)
    {
      ThrowIfDisposed();

      if (presentationTarget == null)
        throw new ArgumentNullException("presentationTarget");
      if (screens == null)
        throw new ArgumentNullException("screens");
      if (_context == null)
        throw new GraphicsException("GraphicsManager.Update(TimeSpan) has not been called. The graphics service needs to be updated before content can be rendered.");

      if (GraphicsDevice.PresentationParameters.IsFullScreen
          || !presentationTarget.IsVisible
          || presentationTarget.Width <= 0
          || presentationTarget.Height <= 0)
      {
        return false;
      }

      _context.PresentationTarget = presentationTarget;

      Viewport originalViewport = GraphicsDevice.Viewport;
      try
      {
        if (presentationTarget.BeginRender(_context))
          RenderScreens(screens);
      }
      finally
      {
        presentationTarget.EndRender(_context);

        GraphicsDevice.Viewport = originalViewport;
        _context.Viewport = originalViewport;
        _context.PresentationTarget = null;
      }

      return true;
    }


    /// <summary>
    /// Presents the current back buffer target into the specified presentation target. (Windows
    /// Forms only!)
    /// </summary>
    /// <param name="presentationTarget">The presentation target.</param>
    /// <returns>
    /// <see langword="true"/> if the back buffer was presented successfully; 
    /// <see langword="false"/> if the operation was skipped because the presentation target is 
    /// currently not visible or invalid.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="presentationTarget"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [Obsolete("Method will be removed in future release. Call Render(IPresentationTarget, IList<GraphicsScreen>) instead.")]
    public bool Present(IPresentationTarget presentationTarget)
    {
      ThrowIfDisposed();

      if (presentationTarget == null)
        throw new ArgumentNullException("presentationTarget");

      // Similar to Render(IPresentationTarget) but without RenderScreens().
      if (GraphicsDevice.PresentationParameters.IsFullScreen
          || !presentationTarget.IsVisible
          || presentationTarget.Width <= 0
          || presentationTarget.Height <= 0
          || presentationTarget.Handle == IntPtr.Zero)
      {
        return false;
      }

      var presentationParameters = GraphicsDevice.PresentationParameters;
      int width = Math.Min(presentationTarget.Width, presentationParameters.BackBufferWidth);
      int height = Math.Min(presentationTarget.Height, presentationParameters.BackBufferHeight);
      Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
      try
      {
#if !MONOGAME
        GraphicsDevice.Present(sourceRectangle, null, presentationTarget.Handle);
        return true;
#else
        throw new NotImplementedException("MonoGame builds support only D3DImagePresentationTargets.");
#endif
      }
      // ReSharper disable EmptyGeneralCatchClause
      catch
      {
        // Do nothing. This happens when the layout of the window changes during rendering.
        // For example, when the user docks windows an OutOfVideoMemoryException might occur.
      }
      // ReSharper restore EmptyGeneralCatchClause

      return false;
    }


    /// <summary>
    /// Renders all visible <see cref="GraphicsScreen" />s.
    /// </summary>
    /// <param name="screens">The graphics screens.</param>
    private void RenderScreens(IList<GraphicsScreen> screens)
    {
      if (GraphicsDevice == null || GraphicsDevice.IsDisposed)
        return;

      // Create temporary list because original collection may be modified during update.
      try
      {
        CopyScreens(screens, _tempScreens);

        // ----- Render screens from back to front.
        var finalViewport = _context.Viewport;
        var finalRenderTarget = _context.RenderTarget;
        GraphicsScreen screenThatRequiresSourceTexture = null;  // The next screen that needs the previous screens as source.
        int numberOfScreens = _tempScreens.Count;
        for (int i = GetIndexOfFirstVisibleScreen(_tempScreens); i < numberOfScreens; i++)
        {
          var screen = _tempScreens[i];

          if (screen == screenThatRequiresSourceTexture)
          {
            Debug.Assert(_context.RenderTarget != null, "Previous graphics screens should have been rendered into an off-screen render target.");
            Debug.Assert(_context.SourceTexture == null, "The RenderContext.SourceTexture should have been recycled.");

            _context.SourceTexture = _context.RenderTarget;
            _context.RenderTarget = finalRenderTarget;
            screenThatRequiresSourceTexture = null;
          }

          if (screenThatRequiresSourceTexture == null)
          {
            // Check if one of the next screens needs the current output in an off-screen render target.
            for (int j = i + 1; j < numberOfScreens; j++)
            {
              var topScreen = _tempScreens[j];
              if (topScreen.IsVisible && topScreen.RenderPreviousScreensToTexture)
              {
                screenThatRequiresSourceTexture = topScreen;
                var format = topScreen.SourceTextureFormat;

                // If not specified, choose default values for width and height.
                if (!format.Width.HasValue)
                  format.Width = finalViewport.Width;
                if (!format.Height.HasValue)
                  format.Height = finalViewport.Height;

                _context.RenderTarget = RenderTargetPool.Obtain2D(format);
                break;
              }
            }
          }

          GraphicsDevice.SetRenderTarget(_context.RenderTarget);

          // For the back buffer we use the special viewport. Off-screen render targets
          // always use the full size.
          if (_context.RenderTarget == finalRenderTarget)
            GraphicsDevice.Viewport = finalViewport;

          // Make sure the viewport in the render context is up-to-date.
          // (Note: SetRenderTarget() always resets GraphicsDevice.Viewport.)
          _context.Viewport = GraphicsDevice.Viewport;

          screen.Render(_context);

          if (_context.SourceTexture != null)
          {
            RenderTargetPool.Recycle(_context.SourceTexture as RenderTarget2D);
            _context.SourceTexture = null;
          }
        }

        Debug.Assert(_context.SourceTexture == null, "The RenderContext.SourceTexture should have been recycled.");
        Debug.Assert(_context.RenderTarget == finalRenderTarget, "The last graphics screen must render into the back buffer.");
      }
      finally
      {
        // If GraphicsScreen.Render() throws, let's at least clear _tempScreens, so we can try again
        // in the next frame.
        _tempScreens.Clear();
      }
    }


    /// <summary>
    /// Gets the index of first (backmost) visible graphics screen.
    /// </summary>
    /// <param name="screens">The graphics screens.</param>
    /// <returns>
    /// The index of the visible graphics screen. (The returned index is a conservative value.
    /// <see cref="GraphicsScreen.IsVisible" /> should be checked for all graphics screens.)
    /// </returns>
    private static int GetIndexOfFirstVisibleScreen(List<GraphicsScreen> screens)
    {
      Debug.Assert(screens != null, "List of graphics screens must not be null.");

      for (int index = screens.Count - 1; index >= 0; --index)
      {
        var screen = screens[index];
        if (screen.IsVisible
            && screen.Coverage == GraphicsScreenCoverage.Full
            && !screen.RenderPreviousScreensToTexture)
        {
          // Current screen covers all pixels and hides all screens below.
          // And the screen does not need another screen as input.
          return index;
        }

        // So this is partially transparent screen. Screens in the background need
        // to be rendered first. Continue search.
      }

      return 0;
    }
    #endregion
  }
}
