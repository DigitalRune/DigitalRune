// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS && MONOGAME
using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using SurfaceFormat = Microsoft.Xna.Framework.Graphics.SurfaceFormat;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Provides helper methods for Direct3D 11.
  /// </summary>
  internal static class D3D11Helper
  {
    /// <summary>
    /// Creates a shared resource.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="width">The width in texels.</param>
    /// <param name="height">The height in texels.</param>
    /// <param name="format">The format.</param>
    /// <returns>The shared resource.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="device"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D CreateSharedResource(Device device, int width, int height, Format format)
    {
      if (device == null)
        throw new ArgumentNullException("device");

      var description = new Texture2DDescription
      {
        Width = width,
        Height = height,
        MipLevels = 1,
        ArraySize = 1,
        Format = format,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Default,
        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        CpuAccessFlags = 0,
        OptionFlags = ResourceOptionFlags.Shared,
      };

      return new Texture2D(device, description);
    }


    ///// <summary>
    ///// Gets the shared handle for the specified resource.
    ///// </summary>
    ///// <param name="texture">The shared resource.</param>
    ///// <returns>The shared handle.</returns>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="texture"/> is <see langword="null"/>.
    ///// </exception>
    //public static IntPtr GetSharedHandle(Texture2D texture)
    //{
    //  if (texture == null)
    //    throw new ArgumentNullException("texture");

    //  using (var resource = texture.QueryInterface<SharpDX.DXGI.Resource>())
    //    return resource.SharedHandle;
    //}


    /// <summary>
    /// Creates a Direct3D 11 staging resource for the specified texture.
    /// </summary>
    /// <param name="device">The Direct3D 11 device.</param>
    /// <param name="texture">The Direct3D 11 texture.</param>
    /// <returns>The staging resource.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="device"/> or <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The texture format is not supported.
    /// </exception>
    public static Texture2D CreateStagingResource(Device device, Texture2D texture)
    {
      if (device == null)
        throw new ArgumentNullException("device");
      if (texture == null)
        throw new ArgumentNullException("texture");

      // This defines the size of staging resource. The purpose of the staging resource is
      // so we can copy & lock as a way to wait for rendering to complete. We ideally, want
      // to copy to a 1x1 staging texture but because of various driver bugs, it is more reliable
      // to use a slightly bigger texture (16x16).
      const int SharedSurfaceCopySize = 16;

      // Determine the size of the staging resource in case the queue surface is less
      // than SharedSurfaceCopySize.
      int width = Math.Min(texture.Description.Width, SharedSurfaceCopySize);
      int height = Math.Min(texture.Description.Height, SharedSurfaceCopySize);

      var format = texture.Description.Format;

      var texture2DDescription = new Texture2DDescription
      {
        Width = width,
        Height = height,
        MipLevels = 1,
        ArraySize = 1,
        Format = format,
        SampleDescription = new SampleDescription(1, 0),
        Usage = ResourceUsage.Staging,
        BindFlags = BindFlags.None,
        CpuAccessFlags = CpuAccessFlags.Read,
        OptionFlags = ResourceOptionFlags.None
      };

      return new Texture2D(device, texture2DDescription);
    }


    /// <summary>
    /// Copies the content of the specified texture.
    /// </summary>
    /// <param name="device">The Direct3D 11 device.</param>
    /// <param name="source">The source texture.</param>
    /// <param name="target">The target texture.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="device"/>, <paramref name="source"/> or <paramref name="target"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static void Copy(Device device, Texture2D source, Texture2D target)
    {
      if (device == null)
        throw new ArgumentNullException("device");
      if (source == null)
        throw new ArgumentNullException("source");
      if (target == null)
        throw new ArgumentNullException("target");

      int sourceWidth = source.Description.Width;
      int sourceHeight = source.Description.Height;
      int targetWidth = target.Description.Width;
      int targetHeight = target.Description.Height;

      if (sourceWidth == targetWidth && sourceHeight == targetHeight)
      {
        device.ImmediateContext.CopyResource(source, target);
      }
      else
      {
        int width = Math.Min(sourceWidth, targetWidth);
        int height = Math.Min(sourceHeight, targetHeight);
        var region = new ResourceRegion(0, 0, 0, width, height, 1);
        device.ImmediateContext.CopySubresourceRegion(source, 0, region, target, 0);
      }
    }


    /// <summary>
    /// Attempts to read the content of the specified staging resource. Does not wait for the
    /// operation to finish.
    /// </summary>
    /// <param name="device">The Direct3D 11 device.</param>
    /// <param name="stagingResource">The staging resource.</param>
    /// <returns>
    /// <see langword="true"/> if the specified resource was read successfully; otherwise,
    /// <see langword="false"/> if the device was still busy.
    /// </returns>
    public static bool TryAccess(Device device, Texture2D stagingResource)
    {
      // Try to read the staging resource into memory to ensure that the GPU is finished.
      try
      {
        var dataBox = device.ImmediateContext.MapSubresource(stagingResource, 0, MapMode.Read, MapFlags.DoNotWait);
        device.ImmediateContext.UnmapSubresource(stagingResource, 0);
        return !dataBox.IsEmpty;
      }
      catch (SharpDXException exception)
      {
        if (exception.ResultCode == SharpDX.DXGI.ResultCode.WasStillDrawing)
          return false;

        throw;
      }
    }


    /// <summary>
    /// Reads the content of the specified staging resource.
    /// </summary>
    /// <param name="device">The Direct3D 11 device.</param>
    /// <param name="stagingResource">The staging resource.</param>
    public static void Access(Device device, Texture2D stagingResource)
    {
      // Read the staging resource into memory to ensure that the GPU is finished.
      device.ImmediateContext.MapSubresource(stagingResource, 0, MapMode.Read, MapFlags.None);
      device.ImmediateContext.UnmapSubresource(stagingResource, 0);
    }


    /// <summary>
    /// Converts the specified MonoGame surface format to DXGI (Direct3D 10/Direct3D 11).
    /// </summary>
    /// <param name="format">The MonoGame surface format.</param>
    /// <returns>The DXGI format.</returns>
    /// <exception cref="ArgumentException">
    /// The surface format is not supported.
    /// </exception>
    public static Format ToD3D11(SurfaceFormat format)
    {
      switch (format)
      {
        case SurfaceFormat.Bgra32:
          return Format.B8G8R8A8_UNorm;
        //case SurfaceFormat.???:               // sRGB formats not yet in MonoGame.
        //  return Format.B8G8R8A8_UNorm_SRgb;
        case SurfaceFormat.Bgr32:
          return Format.B8G8R8X8_UNorm;
        case SurfaceFormat.Color:
          return Format.R8G8B8A8_UNorm;
        //case SurfaceFormat.???:               // sRGB formats not yet in MonoGame.
        //  return Format.R8G8B8A8_UNorm_SRgb;
        case SurfaceFormat.Rgba1010102:
          return Format.R10G10B10A2_UNorm;
        case SurfaceFormat.HalfVector4:
          return Format.R16G16B16A16_Float;
        default:
          throw new ArgumentException("The specified surface format is not supported.");
      }
    }
  }
}
#endif
