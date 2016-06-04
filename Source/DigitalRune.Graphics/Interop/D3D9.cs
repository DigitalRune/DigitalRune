// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS && MONOGAME
using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;
using DeviceType = SharpDX.Direct3D9.DeviceType;
using PresentInterval = SharpDX.Direct3D9.PresentInterval;
using Texture = SharpDX.Direct3D9.Texture;
using TextureFilter = SharpDX.Direct3D9.TextureFilter;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Represents a Direct3D 9 device required for Direct3D 11 interoperability.
  /// </summary>
  /// <remarks>
  /// It is not possible to set a Direct3D 11 resource (e.g. a texture or render target) in WPF
  /// directly because WPF requires Direct3D 9. The <see cref="D3D9"/> class creates a new
  /// Direct3D 9 device which can be used for sharing resources between Direct3D 11 and Direct3D
  /// 9. Call <see cref="CreateSharedTexture"/> to convert a texture from Direct3D 11 to Direct3D 9.
  /// </remarks>
  internal class D3D9 : IDisposable
  {
    // The code requires Windows Vista and up using the Windows Display Driver Model (WDDM). 
    // It does not work with the Windows 2000 Display Driver Model (XDDM).

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Direct3DEx _direct3D;
    private readonly DeviceEx _device;
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
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="D3D9"/> class.
    /// </summary>
    public D3D9()
    {
      // Create Direct3DEx device on Windows Vista/7/8 with a display configured to use 
      // the Windows Display Driver Model (WDDM). Use Direct3D on any other platform.
      _direct3D = new Direct3DEx();

      var presentParameters = new PresentParameters
      {
        Windowed = true,
        SwapEffect = SwapEffect.Discard,
        PresentationInterval = PresentInterval.Immediate,

        // The device back buffer is not used.
        BackBufferFormat = Format.Unknown,
        BackBufferWidth = 1,
        BackBufferHeight = 1,

        // Use dummy window handle.
        DeviceWindowHandle = GetDesktopWindow()
      };


      _device = new DeviceEx(_direct3D, 0, DeviceType.Hardware, IntPtr.Zero,
                             CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                             presentParameters);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="D3D9"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="D3D9"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_direct3D")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_device")]
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _device.SafeDispose();
          _direct3D.SafeDispose();
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    [DllImport("user32.dll", SetLastError = false)]
    private static extern IntPtr GetDesktopWindow();


    private void ThrowIfDisposed()
    {
      if (IsDisposed)
        throw new ObjectDisposedException(GetType().FullName);
    }


    //public bool IsOk()
    //{
    //  ThrowIfDisposed();
    //  return _device.CheckDeviceState(IntPtr.Zero) == DeviceState.Ok;
    //}


    /// <summary>
    /// Creates a Direct3D 9 texture from the specified Direct3D 11 texture.
    /// (The content is shared between the devices.)
    /// </summary>
    /// <param name="texture">The Direct3D 11 texture.</param>
    /// <returns>The Direct3D 9 texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The Direct3D 11 texture is not a shared resource, or the texture format is not supported.
    /// </exception>
    public Texture CreateSharedTexture(SharpDX.Direct3D11.Texture2D texture)
    {
      ThrowIfDisposed();

      if (texture == null)
        throw new ArgumentNullException("texture");

      var format = ToD3D9(texture.Description.Format);

      IntPtr sharedHandle;
      using (var resource = texture.QueryInterface<SharpDX.DXGI.Resource>())
        sharedHandle = resource.SharedHandle;

      if (sharedHandle == IntPtr.Zero)
        throw new ArgumentException("Unable to access resource. The texture needs to be created as a shared resource.", "texture");

      int width = texture.Description.Width;
      int height = texture.Description.Height;
      return new Texture(_device, width, height, 1, Usage.RenderTarget, format, Pool.Default, ref sharedHandle);
    }


    /// <summary>
    /// Creates a Direct3D 9 staging resource.
    /// </summary>
    /// <param name="texture">The Direct3D 11 texture.</param>
    /// <returns>The staging resource.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The texture format is not supported.
    /// </exception>
    public Surface CreateStagingResource(SharpDX.Direct3D11.Texture2D texture)
    {
      ThrowIfDisposed();

      if (texture == null)
        throw new ArgumentNullException("texture");

      var format = ToD3D9(texture.Description.Format);

      // This defines the size of staging resource. The purpose of the staging resource is
      // so we can copy & lock as a way to wait for rendering to complete. We ideally, want
      // to copy to a 1x1 staging texture but because of various driver bugs, it is more reliable
      // to use a slightly bigger texture (16x16).
      const int SharedSurfaceCopySize = 16;

      // Determine the size of the staging resource in case the queue surface is less
      // than SharedSurfaceCopySize.
      int width = Math.Min(texture.Description.Width, SharedSurfaceCopySize);
      int height = Math.Min(texture.Description.Height, SharedSurfaceCopySize);

      return Surface.CreateRenderTarget(_device, width, height, format, MultisampleType.None, 0, true);
    }


    /// <summary>
    /// Copies the content of the specified texture.
    /// </summary>
    /// <param name="source">The source surface.</param>
    /// <param name="target">The target surface.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public void Copy(Surface source, Surface target)
    {
      ThrowIfDisposed();

      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");

      int width = Math.Min(source.Description.Width, target.Description.Width);
      int height = Math.Min(source.Description.Height, target.Description.Height);
      var rectangle = new Rectangle(0, 0, width, height);
      _device.StretchRectangle(source, rectangle, target, rectangle, TextureFilter.None);
    }


    /// <summary>
    /// Attempts to read the content of the specified staging resource. Does not wait for the
    /// operation to finish.
    /// </summary>
    /// <param name="stagingResource">The staging resource.</param>
    /// <returns>
    /// <see langword="true"/> if the specified resource was read successfully; otherwise,
    /// <see langword="false"/> if the device was still busy.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stagingResource"/> is <see langword="null"/>.
    /// </exception>
    public bool TryAccess(Surface stagingResource)
    {
      ThrowIfDisposed();

      if (stagingResource == null)
        throw new ArgumentNullException("stagingResource");

      // Try to read the staging resource into memory to ensure that the GPU is finished.
      try
      {
        stagingResource.LockRectangle(LockFlags.ReadOnly | LockFlags.DoNotWait);
        stagingResource.UnlockRectangle();
      }
      catch (SharpDXException exception)
      {
        if (exception.ResultCode == ResultCode.WasStillDrawing)
          return false;

        throw;
      }

      return true;
    }


    /// <summary>
    /// Reads the content of the specified staging resource.
    /// </summary>
    /// <param name="stagingResource">The staging resource.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stagingResource"/> is <see langword="null"/>.
    /// </exception>
    public void Access(Surface stagingResource)
    {
      ThrowIfDisposed();

      if (stagingResource == null)
        throw new ArgumentNullException("stagingResource");

      // Lock the staging surface to ensure that rendering is complete.
      stagingResource.LockRectangle(LockFlags.ReadOnly);
      stagingResource.UnlockRectangle();
    }


    /// <summary>
    /// Converts the DXGI formats (Direct3D 10/Direct3D 11) to Direct3D 9 formats.
    /// </summary>
    /// <param name="format">The DXGI format.</param>
    /// <returns>The Direct3D 9 format.</returns>
    /// <exception cref="ArgumentException">
    /// The DXGI format is not supported.
    /// </exception>
    private static Format ToD3D9(SharpDX.DXGI.Format format)
    {
      switch (format)
      {
        case SharpDX.DXGI.Format.B8G8R8A8_UNorm:      // MonoGame: SurfaceFormat.Bgra32
          return Format.A8R8G8B8;
        case SharpDX.DXGI.Format.B8G8R8A8_UNorm_SRgb:
          return Format.A8R8G8B8;
        case SharpDX.DXGI.Format.B8G8R8X8_UNorm:      // MonoGame: SurfaceFormat.Bgr32
          return Format.X8R8G8B8;
        case SharpDX.DXGI.Format.R8G8B8A8_UNorm:
          return Format.A8B8G8R8;
        case SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb:
          return Format.A8B8G8R8;
        case SharpDX.DXGI.Format.R10G10B10A2_UNorm:
          return Format.A2B10G10R10;
        case SharpDX.DXGI.Format.R16G16B16A16_Float:
          return Format.A16B16G16R16F;
        default:
          throw new ArgumentException("The specified surface format is not supported.");
      }
    }
    #endregion
  }
}
#endif
