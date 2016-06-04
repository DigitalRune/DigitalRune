// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS && MONOGAME
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Wraps the <see cref="D3DImage"/> to make it compatible with Direct3D 11.
  /// </summary>
  /// <remarks>
  /// The <see cref="D3D11Image"/> should be disposed of if no longer needed!
  /// </remarks>
  internal class D3D11Image : D3DImage, IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly bool _enableAlpha;

    // Use a Direct3D 9 device for interoperability. The device is shared by all D3D11Images.
    private static D3D9 _d3D9;
    private static int _referenceCount;
    private static readonly object _d3D9Lock = new object();

    // The MonoGame render target.
    private RenderTarget2D _renderTarget;

    // The back buffer (shared resource).
    private SharpDX.Direct3D11.Texture2D _texture11;
    private SharpDX.Direct3D9.Texture _texture9;
    private SharpDX.Direct3D9.Surface _surface9;  // Surface level 0 of Texture9.

    // Staging resources for synchronization.
    private SharpDX.Direct3D11.Texture2D _stagingResource11;
    private SharpDX.Direct3D9.Surface _stagingResource9;
    private bool _isAvailable9;

    // Direct3D 11 event query to check if Direct3D 11 renderer has finished.
    private SharpDX.Direct3D11.Query _query;
    // Cache query result.
    private bool _isFrameReady = true;  // true if uninitialized or D3D11 frame has completed.
                                        // false if D3D11 rendering is in progress.

    // Workaround for WPF bug.
    private bool _isLocked;   // Set in BeginRender()/EndRender().
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


    /// <summary>
    /// Gets or sets a value indicating whether the Direct3D 11 device is synchronized with WPF.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the Direct3D 11 device is synchronized with WPF; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// By default, the Direct3D 11 device used for rendering is not synchronized with Direct3D 9
    /// (WPF). Direct3D 11 and Direct3D 9 run asynchronously. The image shown in WPF may not be one
    /// or more frames old. Setting <see cref="IsSynchronized"/> ensures that the devices run in 
    /// sync and that WPF always shows to current frame.
    /// </para>
    /// <para>
    /// However, synchronizing Direct3D 11 and Direct3D 9 is costly. When switching from Direct3D 11
    /// to Direct3D 9 (and vice versa) the CPU needs to be blocked until the GPU is flushed. This
    /// reduces performance.
    /// </para>
    /// </remarks>
    public bool IsSynchronized { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="D3D11Image"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="width">The initial width of the back buffer.</param>
    /// <param name="height">The initial height of the back buffer.</param>
    /// <param name="enableAlpha">
    /// <see langword="true"/> to create a render target with an alpha channel;
    /// <see langword="false"/> to create a render target without an alpha channel.
    /// </param>
    public D3D11Image(GraphicsDevice graphicsDevice, int width, int height, bool enableAlpha)
    {
      _enableAlpha = enableAlpha;

      InitializeD3D9();
      InitializeResources(graphicsDevice, width, height);
    }


    /// <summary>
    /// Releases unmanaged resources before an instance of the <see cref="D3D11Image"/> class is 
    /// reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// This method releases unmanaged resources by calling the virtual <see cref="Dispose(bool)"/> 
    /// method, passing in <see langword="false"/>.
    /// </remarks>
    ~D3D11Image()
    {
      Dispose(false);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="D3D11Image"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="D3D11Image"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_renderTarget")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_query")]
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose of managed resources.
          UninitializeResources();
        }

        // Release unmanaged resources.
        UninitializeD3D9();

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    ///// <summary>
    ///// When implemented in a derived class, creates a new instance of the <see cref="D3DImage" />
    ///// derived class.
    ///// </summary>
    ///// <returns>The new instance.</returns>
    //protected override Freezable CreateInstanceCore()
    //{
    //  return new D3D11Image();
    //}


    private void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    /// <summary>
    /// Initializes the Direct3D 9 device.
    /// </summary>
    private static void InitializeD3D9()
    {
      lock (_d3D9Lock)
      {
        _referenceCount++;
        if (_referenceCount == 1)
          _d3D9 = new D3D9();
      }
    }


    /// <summary>
    /// Un-initializes the Direct3D 9 device, if no longer needed.
    /// </summary>
    private static void UninitializeD3D9()
    {
      lock (_d3D9Lock)
      {
        _referenceCount--;
        if (_referenceCount == 0)
        {
          _d3D9.Dispose();
          _d3D9 = null;
        }
      }
    }


    private void InitializeResources(GraphicsDevice graphicsDevice, int width, int height)
    {
      try
      {
        Debug.Assert(_renderTarget == null, "Dispose previous back buffer before creating a new back buffer.");

        // MonoGame
        var format = _enableAlpha ? SurfaceFormat.Bgra32 : SurfaceFormat.Bgr32;
        _renderTarget = new RenderTarget2D(graphicsDevice, width, height, false, format, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.DiscardContents);

        // Direct3D 11
        var device11 = (SharpDX.Direct3D11.Device)graphicsDevice.Handle;
        var formatDXGI = D3D11Helper.ToD3D11(format);
        _texture11 = D3D11Helper.CreateSharedResource(device11, width, height, formatDXGI);
        _stagingResource11 = D3D11Helper.CreateStagingResource(device11, _texture11);

        // Direct3D 9
        _texture9 = _d3D9.CreateSharedTexture(_texture11);
        _surface9 = _texture9.GetSurfaceLevel(0);
        _stagingResource9 = _d3D9.CreateStagingResource(_texture11);

        // Direct3D 11 event query.
        Debug.Assert(_isFrameReady, "_isFrameReady should be true when uninitialized.");
        var queryDescription = new SharpDX.Direct3D11.QueryDescription
        {
          Flags = SharpDX.Direct3D11.QueryFlags.None,
          Type = SharpDX.Direct3D11.QueryType.Event
        };
        _query = new SharpDX.Direct3D11.Query(device11, queryDescription);

        // Assign back buffer to D3DImage.
        // The back buffer is still empty, however we need to set a valid back buffer
        // for the layout logic. Otherwise, the size of the D3D11Image is (0, 0).
        Lock();
#if NET45
        SetBackBuffer(D3DResourceType.IDirect3DSurface9, _surface9.NativePointer, true);
#else
        SetBackBuffer(D3DResourceType.IDirect3DSurface9, _surface9.NativePointer);
#endif
        Unlock();

        if (IsSynchronized)
        {
          // Issue a copy of the surface into the staging resource to see when the
          // resource is no longer used by Direct3D 9.
          _d3D9.Copy(_surface9, _stagingResource9);
          _isAvailable9 = _d3D9.TryAccess(_stagingResource9);
        }
      }
      catch
      {
        // GPU may run out of memory.
        UninitializeResources();
        throw;
      }
    }


    private void UninitializeResources()
    {
      if (_renderTarget == null)
        return;

      // Unassign back buffer from D3DImage.
      Lock();
#if NET45
      SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero, true);
#else
      SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
#endif
      Unlock();

      // Dispose resources.
      _query.SafeDispose();
      _query = null;
      _isFrameReady = true; // Set to true while nothing is being rendered.
      _stagingResource9.SafeDispose();
      _stagingResource9 = null;
      _surface9.SafeDispose();
      _surface9 = null;
      _texture9.SafeDispose();
      _texture9 = null;
      _stagingResource11.SafeDispose();
      _stagingResource11 = null;
      _texture11.SafeDispose();
      _texture11 = null;
      _renderTarget.SafeDispose();
      _renderTarget = null;
    }


    /// <summary>
    /// Locks the back buffer and enables render operations.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="width">The width of the back buffer.</param>
    /// <param name="height">The height of the back buffer.</param>
    /// <returns>
    /// The <see cref="RenderTarget2D"/> for rendering into the back buffer. <see langword="null"/>
    /// if back buffer is currently not available.
    /// </returns>
    public RenderTarget2D BeginRender(GraphicsDevice graphicsDevice, int width, int height)
    {
      ThrowIfDisposed();

      Lock();

      // WPF bug: Lock() blocks the UI thread. Other WPF events such as drag-and-drop
      // events may run on the UI thread while Lock() is blocking!
      if (IsDisposed)
      {
        // Release pending lock. EndRender() may not be called in this case!
        Unlock();
        return null;
      }

      _isLocked = true;

      // Re-create surfaces if necessary.
      if (_renderTarget == null
          || _renderTarget.GraphicsDevice != graphicsDevice
          || _renderTarget.Width != width
          || _renderTarget.Height != height)
      {
        UninitializeResources();
        InitializeResources(graphicsDevice, width, height);
      }

      if (IsSynchronized && !_isAvailable9)
      {
        // Block until the surface is no longer used by Direct3D 9.
        _d3D9.Access(_stagingResource9);
        _isAvailable9 = true;
      }

      return _renderTarget;
    }


    /// <summary>
    /// Unlocks the back buffer and disables render operations.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    public void EndRender(GraphicsDevice graphicsDevice)
    {
      if (!_isLocked) // May appear due to WPF bug (see above).
        return;

      ThrowIfDisposed();

      var device11 = (SharpDX.Direct3D11.Device)graphicsDevice.Handle;
      try
      {
        if (_renderTarget == null)
        {
          // BeginRender was unsuccessful. (GPU out-of-memory exception.)
          return;
        }

        // Double-buffering: Copy MonoGame render target into shared resource.
        D3D11Helper.Copy(device11, (SharpDX.Direct3D11.Texture2D)_renderTarget.Handle, _texture11);

        // Place Direct3D 11 event query.
        device11.ImmediateContext.End(_query);
        SharpDX.Bool result;
        _isFrameReady = device11.ImmediateContext.GetData(_query, out result) && result;

        if (IsSynchronized)
        {
          // Issue a copy of the surface into the staging resource to see when the
          // resource is no longer used by Direct3D 11.
          D3D11Helper.Copy(device11, _texture11, _stagingResource11);

          // Block until the surface is no longer used by Direct3D 11.
          D3D11Helper.Access(device11, _stagingResource11);
        }
        else
        {
          // For optimal performance, explicitly flush command buffers.
          graphicsDevice.Flush();
        }

        // Note: Redundant calls to SetBackBuffer() are a no-op.
#if NET45
        SetBackBuffer(D3DResourceType.IDirect3DSurface9, _surface9.NativePointer, true);
#else
        SetBackBuffer(D3DResourceType.IDirect3DSurface9, _surface9.NativePointer);
#endif

        if (IsSynchronized)
        {
          // Issue a copy of the surface into the staging resource to see when the
          // resource is no longer used by Direct3D 9.
          _d3D9.Copy(_surface9, _stagingResource9);
          _isAvailable9 = _d3D9.TryAccess(_stagingResource9);
        }

        AddDirtyRect(new Int32Rect(0, 0, PixelWidth, PixelHeight));
      }
      finally
      {
        Unlock();
        _isLocked = false;
      }
    }


    /// <summary>
    /// Determines whether the Direct3D 11 device has finished rendering the current frame.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <returns>
    /// <see langword="true"/> if Direct3D 11 has finished rendering; otherwise,
    /// <see langword="false"/> if the Direct3D 11 is still busy. See remarks.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A return value of <see langword="true"/> guarantees that the current frame rendered to the
    /// back buffer is complete and ready to be copied to the front buffer in WPF.
    /// </para>
    /// <para>
    /// A return value of <see langword="false"/> means that the Direct3D 11 device is still busy.
    /// However, the back buffer might already be usable in WPF! (Since Direct3D 11 and Direct3D 9
    /// run asynchronously by default, there is a chance that the frame is already finished when the
    /// Direct3D 9 device tries to access the shared back buffer!)
    /// </para>
    /// <para>
    /// When <see cref="IsSynchronized"/> is <see langword="true"/>, then Direct3D 9 and Direct3D 11
    /// run in sync and <see cref="IsFrameReady"/> will therefore always return
    /// <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool IsFrameReady(GraphicsDevice graphicsDevice)
    {
      if (_isFrameReady)
      {
        // Frame has already completed.
        return true;
      }

      Debug.Assert(graphicsDevice != null);
      Debug.Assert(_query != null, "_isFrameReady should be true when uninitialized (_query == null).");

      SharpDX.Bool result;
      var device11 = (SharpDX.Direct3D11.Device)graphicsDevice.Handle;
      _isFrameReady = device11.ImmediateContext.GetData(_query, out result) && result;
      return _isFrameReady;
    }


    /// <summary>
    /// Tries to get the contents of the back buffer.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="data">The array of data.</param>
    /// <remarks>
    /// <see langword="true"/> if the content was copied successfully; otherwise,
    /// <see langword="false"/> if the back buffer was not available or empty.
    /// </remarks>
    public bool TryGetData<T>(T[] data) where T : struct
    {
      if (_renderTarget == null)
        return false;

      _renderTarget.GetData(data);
      return true;
    }
    #endregion
  }
}
#endif
