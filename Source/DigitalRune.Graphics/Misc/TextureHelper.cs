// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides helper methods for textures.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class provides several default textures (e.g. <see cref="GetDefaultTexture2DWhite"/>).
  /// These default textures are only created once per graphics device and are reused. These
  /// textures must not be modified.
  /// </para>
  /// </remarks>
  public static class TextureHelper
  {
    // Contains all default textures for one graphics device.
    private class DefaultTextures : IDisposable
    {
      public Texture2D DefaultTexture2DBlack;
      public Texture2D DefaultTexture2DBlack4F;
      public Texture2D DefaultTexture2DWhite;
      public Texture3D DefaultTexture3DBlack;
      public Texture3D DefaultTexture3DWhite;
      public TextureCube DefaultTextureCubeBlack;
      public TextureCube DefaultTextureCubeWhite;
      public Texture2D DefaultNormalTexture;

      public void Dispose()
      {
        if (DefaultTexture2DBlack != null)
          DefaultTexture2DBlack.Dispose();
        if (DefaultTexture2DBlack4F != null)
          DefaultTexture2DBlack4F.Dispose();
        if (DefaultTexture2DWhite != null)
          DefaultTexture2DWhite.Dispose();
        if (DefaultTexture3DBlack != null)
          DefaultTexture3DBlack.Dispose();
        if (DefaultTexture3DWhite != null)
          DefaultTexture3DWhite.Dispose();
        if (DefaultTextureCubeBlack != null)
          DefaultTextureCubeBlack.Dispose();
        if (DefaultTextureCubeWhite != null)
          DefaultTextureCubeWhite.Dispose();
        if (DefaultNormalTexture != null)
          DefaultNormalTexture.Dispose();

        DefaultTexture2DBlack = null;
        DefaultTexture2DBlack4F = null;
        DefaultTexture2DWhite = null;
        DefaultTexture3DBlack = null;
        DefaultTexture3DWhite = null;
        DefaultTextureCubeBlack = null;
        DefaultTextureCubeWhite = null;
        DefaultNormalTexture = null;
      }
    }


    /// <summary>
    /// Gets the default textures from <see cref="IGraphicsService.Data"/>. If necessary a new entry
    /// is created.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private static DefaultTextures GetDefaultTextures(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__DefaultTextures";
      object defaultTextures;
      if (!graphicsService.Data.TryGetValue(key, out defaultTextures) || !(defaultTextures is DefaultTextures))
      {
        defaultTextures = new DefaultTextures();
        graphicsService.Data[key] = defaultTextures;
      }

      return (DefaultTextures)defaultTextures;
    }


    /// <summary>
    /// Gets a black 2D texture with 1x1 pixels.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A black 2D texture with 1x1 pixels.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetDefaultTexture2DBlack(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTexture2DBlack == null || defaultTextures.DefaultTexture2DBlack.IsDisposed)
      {
        defaultTextures.DefaultTexture2DBlack = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
        defaultTextures.DefaultTexture2DBlack.SetData(new[] { Color.Black });
      }

      return defaultTextures.DefaultTexture2DBlack;
    }


    /// <summary>
    /// Gets a black 2D texture with 1x1 pixels using Vector4 format.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A black 2D texture with 1x1 pixels.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    internal static Texture2D GetDefaultTexture2DBlack4F(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTexture2DBlack4F == null || defaultTextures.DefaultTexture2DBlack4F.IsDisposed)
      {
        defaultTextures.DefaultTexture2DBlack4F = new Texture2D(graphicsService.GraphicsDevice, 1, 1, false, SurfaceFormat.Vector4);
        defaultTextures.DefaultTexture2DBlack4F.SetData(new[] { Vector4.Zero });
      }

      return defaultTextures.DefaultTexture2DBlack4F;
    }


    /// <summary>
    /// Gets a white 2D texture with 1x1 pixels.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A white 2D texture with 1x1 pixels.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetDefaultTexture2DWhite(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTexture2DWhite == null || defaultTextures.DefaultTexture2DWhite.IsDisposed)
      {
        defaultTextures.DefaultTexture2DWhite = new Texture2D(graphicsService.GraphicsDevice, 1, 1);
        defaultTextures.DefaultTexture2DWhite.SetData(new[] { Color.White });
      }

      return defaultTextures.DefaultTexture2DWhite;
    }


    /// <summary>
    /// Gets a 1x1 normal map. The normal vector is (0, 0, 1).
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// A 1x1 normal map. The normal stored in the map is (0, 0, 1).
    /// The returned normal map can be used for effects which expect an uncompressed normal map
    /// and for effects which expect a DXT5nm normal map.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetDefaultNormalTexture(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultNormalTexture == null || defaultTextures.DefaultNormalTexture.IsDisposed)
      {
        defaultTextures.DefaultNormalTexture = new Texture2D(graphicsService.GraphicsDevice, 1, 1);

        // Components of a normal vector are in the range [-1, 1].
        // The components are compressed to the range [0, 1].
        // normal = (0, 0, 1) --> (0.5, 0.5, 1.0)
        // DXT5nm compression stores the x-component in the Alpha channel.
        // The following constant works for most cases (no compression, DXT5nm compression).
        defaultTextures.DefaultNormalTexture.SetData(new[] { new Color(128, 128, 255, 128) });
      }

      return defaultTextures.DefaultNormalTexture;
    }


    /// <summary>
    /// Gets a black 3D texture with 1x1 pixels.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A black 3D texture with 1x1 pixels.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture3D GetDefaultTexture3DBlack(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTexture3DBlack == null || defaultTextures.DefaultTexture3DBlack.IsDisposed)
      {
        defaultTextures.DefaultTexture3DBlack = new Texture3D(graphicsService.GraphicsDevice, 1, 1, 1, false, SurfaceFormat.Color);
        defaultTextures.DefaultTexture3DBlack.SetData(new[] { Color.Black });
      }

      return defaultTextures.DefaultTexture3DBlack;
    }


    /// <summary>
    /// Gets a white 3D texture with 1x1 pixels.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A white 3D texture with 1x1 pixels.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture3D GetDefaultTexture3DWhite(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTexture3DWhite == null || defaultTextures.DefaultTexture3DWhite.IsDisposed)
      {
        defaultTextures.DefaultTexture3DWhite = new Texture3D(graphicsService.GraphicsDevice, 1, 1, 1, false, SurfaceFormat.Color);
        defaultTextures.DefaultTexture3DWhite.SetData(new[] { Color.White });
      }

      return defaultTextures.DefaultTexture3DWhite;
    }


    /// <summary>
    /// Gets a cubemap texture where each face consists of 1 black pixel.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A cubemap texture where each face consists of 1 black pixel.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static TextureCube GetDefaultTextureCubeBlack(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTextureCubeBlack == null || defaultTextures.DefaultTextureCubeBlack.IsDisposed)
      {
        defaultTextures.DefaultTextureCubeBlack = new TextureCube(graphicsService.GraphicsDevice, 1, false, SurfaceFormat.Color);
        var black = new[] { Color.Black };
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.PositiveX, black);
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.PositiveY, black);
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.PositiveZ, black);
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.NegativeX, black);
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.NegativeY, black);
        defaultTextures.DefaultTextureCubeBlack.SetData(CubeMapFace.NegativeZ, black);
      }

      return defaultTextures.DefaultTextureCubeBlack;
    }


    /// <summary>
    /// Gets a cubemap texture where each face consists of 1 white pixel.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A cubemap texture where each face consists of 1 white pixel.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static TextureCube GetDefaultTextureCubeWhite(this IGraphicsService graphicsService)
    {
      var defaultTextures = GetDefaultTextures(graphicsService);
      if (defaultTextures.DefaultTextureCubeWhite == null || defaultTextures.DefaultTextureCubeWhite.IsDisposed)
      {
        defaultTextures.DefaultTextureCubeWhite = new TextureCube(graphicsService.GraphicsDevice, 1, false, SurfaceFormat.Color);
        var white = new[] { Color.White };
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.PositiveX, white);
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.PositiveY, white);
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.PositiveZ, white);
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.NegativeX, white);
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.NegativeY, white);
        defaultTextures.DefaultTextureCubeWhite.SetData(CubeMapFace.NegativeZ, white);
      }

      return defaultTextures.DefaultTextureCubeWhite;
    }


    /// <summary>
    /// Gets the normals fitting texture for calculating "best fit" normals.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The normals fitting texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Texture2D GetNormalsFittingTexture(this IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "NormalsFittingTexture";
      object normalsFittingTexture;
      if (!graphicsService.Data.TryGetValue(key, out normalsFittingTexture)
          || !(normalsFittingTexture is Texture2D))
      {
        normalsFittingTexture = graphicsService.Content.Load<Texture2D>("DigitalRune/NormalsFittingTexture");
        graphicsService.Data[key] = normalsFittingTexture;
      }

      return (Texture2D)normalsFittingTexture;
    }


    /// <summary>
    /// Determines whether the specified surface format is a floating-point format.
    /// </summary>
    /// <param name="format">The surface format.</param>
    /// <returns>
    /// <see langword="true"/> if the specified format is a floating-point format; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Invalid format specified.
    /// </exception>
    public static bool IsFloatingPointFormat(SurfaceFormat format)
    {
      switch (format)
      {
        case SurfaceFormat.Color:
        case SurfaceFormat.Bgr565:
        case SurfaceFormat.Bgra5551:
        case SurfaceFormat.Bgra4444:
        case SurfaceFormat.Dxt1:
        case SurfaceFormat.Dxt3:
        case SurfaceFormat.Dxt5:
        case SurfaceFormat.NormalizedByte2:
        case SurfaceFormat.NormalizedByte4:
        case SurfaceFormat.Rgba1010102:
        case SurfaceFormat.Rg32:
        case SurfaceFormat.Rgba64:
        case SurfaceFormat.Alpha8:
          return false;

        case SurfaceFormat.Single:
        case SurfaceFormat.Vector2:
        case SurfaceFormat.Vector4:
        case SurfaceFormat.HalfSingle:
        case SurfaceFormat.HalfVector2:
        case SurfaceFormat.HalfVector4:
        case SurfaceFormat.HdrBlendable:
          return true;

        default:
          throw new ArgumentOutOfRangeException("format");
      }
    }
  }
}
