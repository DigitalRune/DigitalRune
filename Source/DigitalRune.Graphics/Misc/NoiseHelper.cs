// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Contains helper method for the Noise shaders.
  /// </summary>
  /// <remarks>
  /// This static class creates the required lookup textures. The textures are cached and re-used
  /// as long as the <see cref="GraphicsDevice"/> is the same.
  /// </remarks>
  public static class NoiseHelper
  {
    // See GPU Gems 2, 26.4 Implementing Improved Perlin Noise. We use the optimized 
    // version for 3D noise which is in the GPU Gems 2 source code.
    // Other noise variants:
    // - Use precomputed noise lookup texture.
    // - Quick Noise, ShaderX7 is faster but needs more texture memory, and could 
    //   have texture filter artifacts at low frequencies.

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Contains a list of all noise texture for one graphics device.
    private class NoiseTextures : IDisposable
    {
      // A dictionary of simple colored grain texture (not needed for Perlin noise).
      // (The dictionary key is the width of the quadratic texture.)
      public readonly Dictionary<int, Texture2D> GrainTextures = new Dictionary<int, Texture2D>();

      // The plain permutation texture used in non-optimized 3D noise and 4D noise.
      public Texture2D PermutationTexture;

      // The optimized permutation texture used in 3D noise.
      public Texture2D Permutation3DTexture;

      // The optimized gradient texture used in 3D noise.
      public Texture2D Gradient3DTexture;

      // The gradient texture used in 4D noise.
      public Texture2D Gradient4DTexture;

      // The 16x16 dither texture.
      public Texture2D DitherTexture;

      // The default noise texture.
      public Texture2D NoiseTexture;

      public void Dispose()
      {
        foreach (var texture in GrainTextures.Values)
          texture.Dispose();
        GrainTextures.Clear();

        if (PermutationTexture != null)
          PermutationTexture.Dispose();
        if (Permutation3DTexture != null)
          Permutation3DTexture.Dispose();
        if (Gradient3DTexture != null)
          Gradient3DTexture.Dispose();
        if (Gradient4DTexture != null)
          Gradient4DTexture.Dispose();
        if (DitherTexture != null)
          DitherTexture.Dispose();
        if (NoiseTexture != null)
          NoiseTexture.Dispose();

        PermutationTexture = null;
        Permutation3DTexture = null;
        Gradient3DTexture = null;
        Gradient4DTexture = null;
        DitherTexture = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The width of the default jitter map in texels. 
    /// (Used in shadow mask renderers, DefaultEffectBinder.)
    /// </summary>
    internal const int DefaultJitterMapWidth = 64;


    /// <summary>
    /// Gradients for Improved Perlin Noise in 3D.
    /// </summary>
    private static readonly float[] Gradients3D =
    { 
      1,1,0,   -1,1,0,   1,-1,0,   -1,-1,0,
      1,0,1,   -1,0,1,   1,0,-1,   -1,0,-1, 
      0,1,1,   0,-1,1,   0,1,-1,   0,-1,-1,
      1,1,0,   0,-1,1,   -1,1,0,   0,-1,-1
    };


    /// <summary>
    /// Gradients for Improved Perlin Noise in 4D.
    /// </summary>
    private static readonly float[] Gradients4D =
    {
      0, -1, -1, -1,    0, -1, -1, 1,    0, -1, 1, -1,    0, -1, 1, 1,
      0, 1, -1, -1,     0, 1, -1, 1,     0, 1, 1, -1,     0, 1, 1, 1,
      -1, -1, 0, -1,   -1, 1, 0, -1,     1, -1, 0, -1,    1, 1, 0, -1,
      -1, -1, 0, 1,    -1, 1, 0, 1,      1, -1, 0, 1,     1, 1, 0, 1,
  
      -1, 0, -1, -1,    1, 0, -1, -1,     -1, 0, -1, 1,    1, 0, -1, 1,
      -1, 0, 1, -1,     1, 0, 1, -1,      -1, 0, 1, 1,     1, 0, 1, 1,
      0, -1, -1, 0,     0, -1, -1, 0,     0, -1, 1, 0,     0, -1, 1, 0,
      0, 1, -1, 0,      0, 1, -1, 0,      0, 1, 1, 0,      0, 1, 1, 0
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the noise textures from <see cref="IGraphicsService.Data"/>. If necessary a new entry 
    /// is created.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A noise textures.</returns>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private static NoiseTextures GetNoiseTextures(IGraphicsService graphicsService)
    {
      const string key = "__NoiseTextures";
      object textures;
      if (!graphicsService.Data.TryGetValue(key, out textures) || !(textures is NoiseTextures))
      {
        textures = new NoiseTextures();
        graphicsService.Data[key] = textures;
      }

      return (NoiseTextures)textures;
    }


    /// <overloads>
    /// <summary>
    /// Gets a tileable noise texture.
    /// </summary>
    /// </overloads>
    ///
    /// <summary>
    /// Gets a tileable noise texture.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="size">The width of the quadratic texture in pixels.</param>
    /// <param name="numberOfOscillations">
    /// Defines the scale/detail of the noise. The noise changes smoothly from dark to light and
    /// back, like a sine function. This value defines the max. number of such oscillations. For
    /// example, if this value is 10, then the noise texture will contain 5 to 10 oscillations.
    /// </param>
    /// <returns>A texture containing Perlin noise.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="size" /> or <paramref name="numberOfOscillations" /> is less than 1.
    /// </exception>
    /// <remarks>
    /// This method returns a quadratic RGBA texture that contains noise. Each channel contains a
    /// different 8-bit noise value, i.e. the texture contains 4 different Perlin noise images.
    /// </remarks>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Texture2D GetNoiseTexture(IGraphicsService graphicsService, int size, int numberOfOscillations)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (size < 1)
        throw new ArgumentOutOfRangeException("size", "size must be greater than 0.");
      if (numberOfOscillations < 1)
        throw new ArgumentOutOfRangeException("numberOfOscillations", "numberOfOscillations must be greater than 0.");

      var data = new Color[size * size];
      double f = numberOfOscillations / (double)size;
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          double nx = PerlinNoise.Compute(f * x, f * y, 0.0 / 4.0 * size, numberOfOscillations, numberOfOscillations, numberOfOscillations);
          double ny = PerlinNoise.Compute(f * x, f * y, 1.0 / 4.0 * size, numberOfOscillations, numberOfOscillations, numberOfOscillations);
          double nz = PerlinNoise.Compute(f * x, f * y, 2.0 / 4.0 * size, numberOfOscillations, numberOfOscillations, numberOfOscillations);
          double nw = PerlinNoise.Compute(f * x, f * y, 3.0 / 4.0 * size, numberOfOscillations, numberOfOscillations, numberOfOscillations);

          data[y * size + x] = new Color(
            (byte)((nx / 2 + 0.5) * 255),
            (byte)((ny / 2 + 0.5) * 255),
            (byte)((nz / 2 + 0.5) * 255),
            (byte)((nw / 2 + 0.5) * 255));
        }
      }

      var texture = new Texture2D(graphicsService.GraphicsDevice, size, size, false, SurfaceFormat.Color);
      texture.SetData(data);

      return texture;
    }


    /// <summary>
    /// Gets a tileable noise texture.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A texture containing Perlin noise.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// This method returns a quadratic RGBA texture that contains noise. Each channel contains a
    /// different 8-bit noise value, i.e. the texture contains 4 different Perlin noise images.
    /// </remarks>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Texture2D GetNoiseTexture(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.NoiseTexture == null || textures.NoiseTexture.IsDisposed)
        textures.NoiseTexture = GetNoiseTexture(graphicsService, 128, 8);

      return textures.NoiseTexture;
    }


    /// <summary>
    /// Gets a grain texture.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="size">The width of the texture in pixels.</param>
    /// <returns>A grain texture.</returns>
    /// <remarks>
    /// This method returns a quadratic RGBA texture that contains random color values. (For each 
    /// pixel, each channel contains a random 8-bit value). This method will always return the same 
    /// texture if it is called for the same <see cref="GraphicsDevice"/> and the same size.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="size"/> is less than 1.
    /// </exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Texture2D GetGrainTexture(IGraphicsService graphicsService, int size)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (size < 1)
        throw new ArgumentOutOfRangeException("size", "size must be greater than 0.");

      var textures = GetNoiseTextures(graphicsService);
      Texture2D grainTexture;
      if (!textures.GrainTextures.TryGetValue(size, out grainTexture) || grainTexture.IsDisposed)
      {
        grainTexture = new Texture2D(graphicsService.GraphicsDevice, size, size, false, SurfaceFormat.Color);
        Color[] data = new Color[size * size];
        var random = new Random(1234567);
        for (int i = 0; i < data.Length; i++)
          data[i] = new Color(random.NextByte(), random.NextByte(), random.NextByte(), random.NextByte());

        grainTexture.SetData(data);

        textures.GrainTextures[size] = grainTexture;
      }

      return grainTexture;
    }


    /// <summary>
    /// Gets the permutation lookup texture (used in 4D noise, but not in the optimized 3D noise).
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The permutation lookup texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetPermutationTexture(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.PermutationTexture == null || textures.PermutationTexture.IsDisposed)
      {
        var p = PerlinNoise.Permutation;  // The Improved Perlin Noise permutation table.

        // Create texture.
        if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
        {
          textures.PermutationTexture = new Texture2D(graphicsService.GraphicsDevice, 256, 1, false, SurfaceFormat.Alpha8);
          textures.PermutationTexture.SetData(p, 0, 256);
        }
        else
        {
          // No Alpha8 in Reach :-(
          textures.PermutationTexture = new Texture2D(graphicsService.GraphicsDevice, 256, 1, false, SurfaceFormat.Color);
          Color[] data = new Color[256];
          for (int i = 0; i < 256; i++)
            data[i] = new Color(p[i], p[i], p[i], p[i]);

          textures.PermutationTexture.SetData(data);
        }
      }

      return textures.PermutationTexture;
    }


    /// <summary>
    /// Gets the optimized permutation lookup texture for 3D Perlin noise.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The permutation lookup texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetPermutation3DTexture(IGraphicsService graphicsService)
    {
      // ReSharper disable InconsistentNaming
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.Permutation3DTexture == null || textures.Permutation3DTexture.IsDisposed)
      {
        // ----- Create texture.
        var p = PerlinNoise.Permutation;  // The Improved Perlin Noise permutation table.

        // The texture contains several pre-computed values for optimized 3D noise.
        textures.Permutation3DTexture = new Texture2D(graphicsService.GraphicsDevice, 256, 256, false, SurfaceFormat.Color);
        Color[] data = new Color[256 * 256];
        for (int X = 0; X < 256; X++)
        {
          for (int Y = 0; Y < 256; Y++)
          {
            int A = p[X] + Y;
            int AA = p[A];
            int AB = p[A + 1];
            int B = p[X + 1] + Y;
            int BA = p[B];
            int BB = p[B + 1];
            data[Y * 256 + X] = new Color((byte)AA, (byte)AB, (byte)BA, (byte)BB);
          }
        }
        textures.Permutation3DTexture.SetData(data);
      }

      return textures.Permutation3DTexture;
      // ReSharper restore InconsistentNaming
    }


    /// <summary>
    /// Gets the optimized gradient lookup texture for 3D Perlin noise.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The gradient lookup texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetGradient3DTexture(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.Gradient3DTexture == null || textures.Gradient3DTexture.IsDisposed)
      {
        var permutation = PerlinNoise.Permutation;  // The Improved Perlin Noise permutation table.

        // ----- Create texture.
        // The texture contains not only the gradients. It contains some precomputations 
        // for optimized noise too (the Permutation lookup).
        textures.Gradient3DTexture = new Texture2D(graphicsService.GraphicsDevice, 256, 1, false, SurfaceFormat.NormalizedByte4);
        NormalizedByte4[] data = new NormalizedByte4[256];
        for (int i = 0; i < 256; i++)
        {
          int p = permutation[i] % 16;
          data[i] = new NormalizedByte4(Gradients3D[p * 3 + 0],
                                        Gradients3D[p * 3 + 1],
                                        Gradients3D[p * 3 + 2], 0);
        }

        textures.Gradient3DTexture.SetData(data);
      }

      return textures.Gradient3DTexture;
    }


    /// <summary>
    /// Gets the gradient lookup texture for 4D Perlin noise.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The gradient lookup texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetGradient4DTexture(IGraphicsService graphicsService)
    {
      // TODO: Not tested yet.

      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.Gradient4DTexture == null || textures.Gradient4DTexture.IsDisposed)
      {
        // Create texture.
        textures.Gradient4DTexture = new Texture2D(graphicsService.GraphicsDevice, 32, 1, false, SurfaceFormat.NormalizedByte4);
        NormalizedByte4[] data = new NormalizedByte4[32];
        for (int i = 0; i < 32; i++)
        {
          data[i] = new NormalizedByte4(Gradients4D[i * 4 + 0],
                                        Gradients4D[i * 4 + 1],
                                        Gradients4D[i * 4 + 2],
                                        Gradients4D[i * 4 + 3]);
        }
        textures.Gradient4DTexture.SetData(data);
      }

      return textures.Gradient4DTexture;
    }


    /// <summary>
    /// Gets a 16x16 dither map.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>A 16x16 dither map.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public static Texture2D GetDitherTexture(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var textures = GetNoiseTextures(graphicsService);
      if (textures.DitherTexture == null || textures.DitherTexture.IsDisposed)
      {
        // ----- Random 16x16 permutation
        //const int n = 16;
        //byte[] data = 
        //{
        //  192,  11, 183, 125,  26, 145,  44, 244,   8, 168, 139,  38, 174,  27, 141,  43,
        //  115, 211, 150,  68, 194,  88, 177, 131,  61, 222,  87, 238,  74, 224, 100, 235,
        //   59,  33,  96, 239,  51, 232,  16, 210, 117,  32, 187,   1, 157, 121,  14, 165,
        //  248, 128, 217,   2, 163, 105, 154,  81, 247, 149,  97, 205,  52, 182, 209,  84,
        //   20, 172,  80, 140, 202,  41, 185,  55,  24, 197,  65, 129, 252,  35,  70, 147,
        //  201,  63, 189,  28,  90, 254, 116, 219, 137, 107, 231,  17, 144, 119, 228, 109,
        //   46, 245, 103, 229, 134,  13,  67, 162,   6, 170,  47, 178,  76, 193,   4, 167,
        //  133,   9, 159,  54, 175, 124, 225,  93, 242,  79, 214,  99, 241,  56, 221,  92,
        //  186, 218,  78, 208,  37, 196,  25, 188,  42, 142,  29, 158,  21, 130, 156,  40,
        //  102,  31, 148, 111, 234,  85, 151, 120, 207, 113, 255,  86, 184, 212,  69, 236,
        //  176,  73, 253,   0, 138,  58, 249,  71,  10, 173,  62, 200,  50, 114,  12, 123,
        //   23, 204, 118, 191,  91, 181,  19, 164, 216, 101, 233,   3, 135, 169, 246, 152,
        //  223,  60, 143,  48, 240,  34, 220,  82, 132,  36, 146, 106, 227,  30,  95,  49,
        //   83, 166,  18, 199,  98, 155, 122,  53, 237, 179,  57, 190,  77, 195, 127, 180,
        //  230, 108, 215,  64, 171,   5, 206, 161,  22,  94, 251,  15, 153,  45, 243,   7,
        //   72, 136,  39, 250, 104, 226,  75, 112, 198, 126,  66, 213, 110, 203,  89, 160
        //};

        // ------ Regular 16x16 dither pattern
        const int n = 16;
        byte[] data = CreateDitherMap();

        textures.DitherTexture = new Texture2D(graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Alpha8);
        textures.DitherTexture.SetData(data);
      }

      return textures.DitherTexture;
    }


    // Creates a 16x16 dither map.
    [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private static byte[] CreateDitherMap()
    {
      // The 16x16 dither matrix is computed as the (modified) outer product of 
      // a 4x4 matrix with itself. (Reference: Graphics Gems II, p. 74)
      int[,] m4x4 =
      {
        {  0, 14,  3, 13 },
        { 11,  5,  8,  6 },
        { 12,  2, 15,  1 },
        {  7,  9,  4, 10 }
      };

      byte[] m = new byte[16 * 16];

      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          for (int k = 0; k < 4; k++)
            for (int l = 0; l < 4; l++)
              m[(4 * k + i) * 16 + 4 * l + j] = (byte)(m4x4[i, j] * 16 + m4x4[k, l]);

      return m;
    }
    #endregion
  }
}
