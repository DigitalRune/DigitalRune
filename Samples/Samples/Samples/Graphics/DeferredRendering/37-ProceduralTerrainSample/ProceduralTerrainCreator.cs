// Credits:
// This code is based on the work of Ian Parberry (http://larc.unt.edu/ian/research/tobler/).
// Original code is licensed under the GNU All-Permissive License:
//
// Copyright Ian Parberry, May 2014.
//
// This file is made available under the GNU All-Permissive License.
//
// Copying and distribution of this file, with or without modification,
// are permitted in any medium without royalty provided the copyright
// notice and this notice are preserved.  This file is offered as-is,
// without any warranty.


using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Threading;


namespace Samples.Graphics
{
  /// <summary>
  /// Creates a terrain using exponentially distributed noise.
  /// </summary>
  public class ProceduralTerrainCreator
  {
    // Perlin's N.
    private const int N = 0x1000;
    private static readonly float Sqrt2 = (float)Math.Sqrt(2);

    // A bit mask, one less than permutation table size.
    private readonly int _maskB;

    // A table with permutations of the numbers 0 to permutation table size - 1.
    private readonly int[] _permutations;

    // A table with B 2D gradients (normalized).
    private readonly Vector2F[] _gradients;

    // A table with exponentially decreasing magnitudes.
    private readonly float[] _magnitudes;


    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralTerrainCreator"/> class.
    /// </summary>
    /// <param name="seed">The seed for the random number generator (e.g. 7777).</param>
    /// <param name="permutationTableSize">
    /// The size of the permutation table (e.g. 256). Must be a power of two.
    /// </param>
    /// <param name="mu">
    /// The constant µ that defines the exponential distribution. Use a value of 1 to get standard
    /// Perlin noise. Use a value greater than 1 (e.g. 1.02) to get Perlin noise with exponentially
    /// distributed gradients. For a <paramref name="permutationTableSize"/> of 256, µ should be in
    /// the range [1, 1.16].
    /// </param>
    public ProceduralTerrainCreator(int seed, int permutationTableSize, float mu)
    {
      if (!MathHelper.IsPowerOf2(permutationTableSize))
        throw new ArgumentException("The permutation table size must be a power of 2 (e.g. 256).");

      _maskB = permutationTableSize - 1;

      var random = new Random(seed);

      // Create table of random gradient vectors (normalized).
      _gradients = new Vector2F[permutationTableSize];
      for (int i = 0; i < _gradients.Length; i++)
      {
        var direction = random.NextVector2F(-1, 1);
        if (!direction.TryNormalize())
          direction = new Vector2F(1, 0);

        _gradients[i] = direction;
      }

      // Create table with a permutation of the values 0 to permutationTableSize.
      _permutations = new int[permutationTableSize];
      for (int i = 0; i < _permutations.Length; i++)
        _permutations[i] = i;
      for (int i = _permutations.Length - 1; i > 0; i--)
        MathHelper.Swap(ref _permutations[i], ref _permutations[random.NextInteger(0, i)]);

      // Create table with gradient magnitudes.
      _magnitudes = new float[permutationTableSize];
      float s = 1; // First magnitude.
      for (int i = 0; i < _magnitudes.Length; i++)
      {
        _magnitudes[i] = s;
        s /= mu;
      }
    }


    private float ComputeCubicSpline(float x)
    {
      return x * x * (3.0f - 2.0f * x);
    }


    private float ComputeNoise(Vector2F position)
    {
      float t0 = position.X + N;
      int bx0 = ((int)t0) & _maskB;
      int bx1 = (bx0 + 1) & _maskB;
      float rx0 = t0 - (int)t0;
      float rx1 = rx0 - 1.0f;

      float t1 = position.Y + N;
      int by0 = ((int)t1) & _maskB;
      int by1 = (by0 + 1) & _maskB;
      float ry0 = t1 - (int)t1;
      float ry1 = ry0 - 1.0f;

      int b00 = _permutations[(_permutations[bx0] + by0) & _maskB];
      int b10 = _permutations[(_permutations[bx1] + by0) & _maskB];
      int b01 = _permutations[(_permutations[bx0] + by1) & _maskB];
      int b11 = _permutations[(_permutations[bx1] + by1) & _maskB];

      float sx = ComputeCubicSpline(rx0);

      float u = _magnitudes[b00] * Vector2F.Dot(_gradients[b00], new Vector2F(rx0, ry0));
      float v = _magnitudes[b10] * Vector2F.Dot(_gradients[b10], new Vector2F(rx1, ry0));
      float a = InterpolationHelper.Lerp(u, v, sx);

      u = _magnitudes[b01] * Vector2F.Dot(_gradients[b01], new Vector2F(rx0, ry1));
      v = _magnitudes[b11] * Vector2F.Dot(_gradients[b11], new Vector2F(rx1, ry1));
      float b = InterpolationHelper.Lerp(u, v, sx);

      float sy = ComputeCubicSpline(ry0);
      return InterpolationHelper.Lerp(a, b, sy);
    }


    private float ComputeTurbulence(Vector2F position, int numberOfOctaves)
    {
      float sum = 0.0f;
      Vector2F p = position;
      float scale = 1.0f;

      for (int i = 0; i < numberOfOctaves; i++)
      {
        // Apply persistence.
        scale *= 0.5f;

        // Add in an octave of noise.
        sum += ComputeNoise(p) * scale;

        // Apply lacunarity.
        p[0] *= 2.0f;
        p[1] *= 2.0f;
      }

      return Sqrt2 * sum / (1.0f - scale);
    }


    /// <summary>
    /// Creates the terrain height field.
    /// </summary>
    /// <param name="noiseOriginX">The x origin in the 2D noise.</param>
    /// <param name="noiseOriginZ">The z origin in the 2D noise.</param>
    /// <param name="noiseWidthX"> The x width of the terrain tile in the 2D noise.</param>
    /// <param name="noiseWidthZ"> The z width of the terrain tile in the 2D noise.</param>
    /// <param name="averageHeight">The average height.</param>
    /// <param name="heightScale">
    /// A factor which is multiplied with the terrain height values.</param>
    /// <param name="numberOfOctaves">
    /// The number of octaves.
    /// </param>
    /// <param name="heights">
    /// The array where the created height values are stored.
    /// This is a 1-dimensional <see cref="float"/> array with
    /// <paramref name="numberOfSamplesX"/> * <paramref name="numberOfSamplesZ"/> elements.
    /// </param>
    /// <param name="numberOfSamplesX">The number of height samples in x.</param>
    /// <param name="numberOfSamplesZ">The number of height samples in z.</param>
    public void CreateTerrain(float noiseOriginX, float noiseOriginZ, float noiseWidthX, float noiseWidthZ,
      float averageHeight, float heightScale, int numberOfOctaves,
      float[] heights, int numberOfSamplesX, int numberOfSamplesZ)
    {
      float stepX = noiseWidthX / (numberOfSamplesX - 1);
      float stepZ = noiseWidthZ / (numberOfSamplesZ - 1);

      //for (int z = 0; z < arrayHeight; z++)
      Parallel.For(0, numberOfSamplesZ, z =>
      {
        for (int x = 0; x < numberOfSamplesX; x++)
        {
          Vector2F position = new Vector2F(noiseOriginX + x * stepX, noiseOriginZ + z * stepZ);
          float noise = ComputeTurbulence(position, numberOfOctaves);
          heights[z * numberOfSamplesX + x] = averageHeight + heightScale * noise;
        }
      });
    }
  }
}
