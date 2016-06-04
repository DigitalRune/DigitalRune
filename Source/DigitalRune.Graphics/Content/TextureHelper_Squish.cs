#region ----- Copyright -----
/*
  The class Squish is a port of libsquish (https://code.google.com/p/libsquish/)
  which is licensed under the MIT License.
 
  Copyright (c) 2006 Simon Brown                        si@sjbrown.co.uk
  Copyright (c) 2007 Ignacio Castano                 icastano@nvidia.com

  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the 
  "Software"), to	deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to 
  permit persons to whom the Software is furnished to do so, subject to 
  the following conditions:

  The above copyright notice and this permission notice shall be included
  in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
  CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
  TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Threading.Tasks;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  // Notes:
  // Libraries supporting BCn block compression:
  // - AMD_Compress, ATITC (legacy)
  // - Crunch, https://code.google.com/p/crunch/
  // - D3DX (legacy)
  // - DirectXTex, https://directxtex.codeplex.com/
  // - NVIDIA Texture Tools NVTT, https://code.google.com/p/nvidia-texture-tools/
  // - squish, https://code.google.com/p/libsquish/
  // - PVRTexLib, http://community.imgtec.com/developers/powervr/tools/
  // - stb_dxt.h, http://nothings.org/stb/stb_dxt.h


  /// <summary>
  /// Defines the options for DXT texture compression using <see cref="Squish"/>.
  /// </summary>
  [Flags]
  internal enum SquishFlags
  {
    /// <summary>Use DXT1 compression.</summary>
    Dxt1 = (1 << 0),

    /// <summary>Use DXT3 compression.</summary>
    Dxt3 = (1 << 1),

    /// <summary>Use DXT5 compression.</summary>
    Dxt5 = (1 << 2),

    /// <summary>Use a very slow but very high quality colour compressor.</summary>
    ColourIterativeClusterFit = (1 << 8),

    /// <summary>Use a slow but high quality colour compressor (the default).</summary>
    ColourClusterFit = (1 << 3),

    /// <summary>Use a fast but low quality colour compressor.</summary>
    ColourRangeFit = (1 << 4),

    /// <summary>Weight the colour by alpha during cluster fit (disabled by default).</summary>
    WeightColourByAlpha = (1 << 7)
  };


  /// <summary>
  /// Provides support for DXT1/3/5 texture compression.
  /// </summary>
  /// <remarks>
  /// <see href="https://code.google.com/p/libsquish/">libSquish</see>.
  /// Copyright (c) 2006 Simon Brown. Available under MIT license.
  /// </remarks>
  internal static class Squish
  {
    //--------------------------------------------------------------
    #region alpha.h/.cpp
    //--------------------------------------------------------------

    private static int FloatToInt(float a, int limit)
    {
      // use ANSI round-to-zero behaviour to get round-to-nearest
      int i = (int)(a + 0.5f);

      // clamp to the limit
      if (i < 0)
        i = 0;
      else if (i > limit)
        i = limit;

      // done
      return i;
    }


    private static unsafe void CompressAlphaDxt3(byte* rgba, int mask, byte* block)
    {
      // quantise and pack the alpha values pairwise
      for (int i = 0; i < 8; ++i)
      {
        // quantise down to 4 bits
        float alpha1 = rgba[8 * i + 3] * (15.0f / 255.0f);
        float alpha2 = rgba[8 * i + 7] * (15.0f / 255.0f);
        int quant1 = FloatToInt(alpha1, 15);
        int quant2 = FloatToInt(alpha2, 15);

        // set alpha to zero where masked
        int bit1 = 1 << (2 * i);
        int bit2 = 1 << (2 * i + 1);
        if ((mask & bit1) == 0)
          quant1 = 0;
        if ((mask & bit2) == 0)
          quant2 = 0;

        // pack into the byte
        block[i] = (byte)(quant1 | (quant2 << 4));
      }
    }


    private static unsafe void DecompressAlphaDxt3(byte* rgba, byte* block)
    {
      // unpack the alpha values pairwise
      for (int i = 0; i < 8; ++i)
      {
        // quantise down to 4 bits
        byte quant = block[i];

        // unpack the values
        byte lo = (byte)(quant & 0x0f);
        byte hi = (byte)(quant & 0xf0);

        // convert back up to bytes
        rgba[8 * i + 3] = (byte)(lo | (lo << 4));
        rgba[8 * i + 7] = (byte)(hi | (hi >> 4));
      }
    }


    private static void FixRange(ref int min, ref int max, int steps)
    {
      if (max - min < steps)
        max = Math.Min(min + steps, 255);
      if (max - min < steps)
        min = Math.Max(0, max - steps);
    }


    private static unsafe int FitCodes(byte* rgba, int mask, byte* codes, byte* indices)
    {
      // fit each alpha value to the codebook
      int err = 0;
      for (int i = 0; i < 16; ++i)
      {
        // check this pixel is valid
        int bit = 1 << i;
        if ((mask & bit) == 0)
        {
          // use the first code
          indices[i] = 0;
          continue;
        }

        // find the least error and corresponding index
        int value = rgba[4 * i + 3];
        int least = int.MaxValue;
        int index = 0;
        for (int j = 0; j < 8; ++j)
        {
          // get the squared error from this code
          int dist = value - codes[j];
          dist *= dist;

          // compare with the best so far
          if (dist < least)
          {
            least = dist;
            index = j;
          }
        }

        // save this index and accumulate the error
        indices[i] = (byte)index;
        err += least;
      }

      // return the total error
      return err;
    }


    private static unsafe void WriteAlphaBlock(int alpha0, int alpha1, byte* indices, byte* block)
    {
      // write the first two bytes
      block[0] = (byte)alpha0;
      block[1] = (byte)alpha1;

      // pack the indices with 3 bits each
      byte* dest = block + 2;
      byte* src = indices;
      for (int i = 0; i < 2; ++i)
      {
        // pack 8 3-bit values
        int value = 0;
        for (int j = 0; j < 8; ++j)
        {
          int index = *src++;
          value |= (index << 3 * j);
        }

        // store in 3 bytes
        for (int j = 0; j < 3; ++j)
        {
          int b = (value >> 8 * j) & 0xff;
          *dest++ = (byte)b;
        }
      }
    }


    private static unsafe void WriteAlphaBlock5(int alpha0, int alpha1, byte* indices, byte* block)
    {
      // check the relative values of the endpoints
      if (alpha0 > alpha1)
      {
        // swap the indices
        byte* swapped = stackalloc byte[16];
        for (int i = 0; i < 16; ++i)
        {
          byte index = indices[i];
          if (index == 0)
            swapped[i] = 1;
          else if (index == 1)
            swapped[i] = 0;
          else if (index <= 5)
            swapped[i] = (byte)(7 - index);
          else
            swapped[i] = index;
        }

        // write the block
        WriteAlphaBlock(alpha1, alpha0, swapped, block);
      }
      else
      {
        // write the block
        WriteAlphaBlock(alpha0, alpha1, indices, block);
      }
    }


    private static unsafe void WriteAlphaBlock7(int alpha0, int alpha1, byte* indices, byte* block)
    {
      // check the relative values of the endpoints
      if (alpha0 < alpha1)
      {
        // swap the indices
        byte* swapped = stackalloc byte[16];
        for (int i = 0; i < 16; ++i)
        {
          byte index = indices[i];
          if (index == 0)
            swapped[i] = 1;
          else if (index == 1)
            swapped[i] = 0;
          else
            swapped[i] = (byte)(9 - index);
        }

        // write the block
        WriteAlphaBlock(alpha1, alpha0, swapped, block);
      }
      else
      {
        // write the block
        WriteAlphaBlock(alpha0, alpha1, indices, block);
      }
    }


    private static unsafe void CompressAlphaDxt5(byte* rgba, int mask, byte* block)
    {
      // get the range for 5-alpha and 7-alpha interpolation
      int min5 = 255;
      int max5 = 0;
      int min7 = 255;
      int max7 = 0;
      for (int i = 0; i < 16; ++i)
      {
        // check this pixel is valid
        int bit = 1 << i;
        if ((mask & bit) == 0)
          continue;

        // incorporate into the min/max
        int value = rgba[4 * i + 3];
        if (value < min7)
          min7 = value;
        if (value > max7)
          max7 = value;
        if (value != 0 && value < min5)
          min5 = value;
        if (value != 255 && value > max5)
          max5 = value;
      }

      // handle the case that no valid range was found
      if (min5 > max5)
        min5 = max5;
      if (min7 > max7)
        min7 = max7;

      // fix the range to be the minimum in each case
      FixRange(ref min5, ref max5, 5);
      FixRange(ref min7, ref max7, 7);

      // set up the 5-alpha code book
      byte* codes5 = stackalloc byte[8];
      codes5[0] = (byte)min5;
      codes5[1] = (byte)max5;
      for (int i = 1; i < 5; ++i)
        codes5[1 + i] = (byte)(((5 - i) * min5 + i * max5) / 5);
      codes5[6] = 0;
      codes5[7] = 255;

      // set up the 7-alpha code book
      byte* codes7 = stackalloc byte[8];
      codes7[0] = (byte)min7;
      codes7[1] = (byte)max7;
      for (int i = 1; i < 7; ++i)
        codes7[1 + i] = (byte)(((7 - i) * min7 + i * max7) / 7);

      // fit the data to both code books
      byte* indices5 = stackalloc byte[16];
      byte* indices7 = stackalloc byte[16];
      int err5 = FitCodes(rgba, mask, codes5, indices5);
      int err7 = FitCodes(rgba, mask, codes7, indices7);

      // save the block with least error
      if (err5 <= err7)
        WriteAlphaBlock5(min5, max5, indices5, block);
      else
        WriteAlphaBlock7(min7, max7, indices7, block);
    }


    private static unsafe void DecompressAlphaDxt5(byte* rgba, byte* block)
    {
      // get the two alpha values
      int alpha0 = block[0];
      int alpha1 = block[1];

      // compare the values to build the codebook
      byte* codes = stackalloc byte[8];
      codes[0] = (byte)alpha0;
      codes[1] = (byte)alpha1;
      if (alpha0 <= alpha1)
      {
        // use 5-alpha codebook
        for (int i = 1; i < 5; ++i)
          codes[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
        codes[6] = 0;
        codes[7] = 255;
      }
      else
      {
        // use 7-alpha codebook
        for (int i = 1; i < 7; ++i)
          codes[1 + i] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
      }

      // decode the indices
      byte* indices = stackalloc byte[16];
      byte* src = block + 2;
      byte* dest = indices;
      for (int i = 0; i < 2; ++i)
      {
        // grab 3 bytes
        int value = 0;
        for (int j = 0; j < 3; ++j)
        {
          int b = *src++;
          value |= (b << 8 * j);
        }

        // unpack 8 3-bit values from it
        for (int j = 0; j < 8; ++j)
        {
          int index = (value >> 3 * j) & 0x7;
          *dest++ = (byte)index;
        }
      }

      // write out the indexed codebook values
      for (int i = 0; i < 16; ++i)
        rgba[4 * i + 3] = codes[indices[i]];
    }
    #endregion


    //--------------------------------------------------------------
    #region clusterfit.h/.cpp
    //--------------------------------------------------------------

    private class ClusterFit
    {
      private const int MaxIterations = 8;

      private ColourSet _colours;
      private SquishFlags _flags;
      private int _iterationCount;
      private Vector3F _principle;
      private byte[] _order;
      private Vector4F[] _points_weights;
      private Vector4F _xsum_wsum;
      private Vector4F _metric;
      private Vector4F _besterror;


      public ClusterFit()
      {
        _order = new byte[16 * MaxIterations];
        _points_weights = new Vector4F[16];
      }


      public void Initialize(ColourSet colours, SquishFlags flags, Vector3F? metric)
      {
        _colours = colours;
        _flags = flags;
        _xsum_wsum = new Vector4F();

        // set the iteration count
        _iterationCount = (flags & SquishFlags.ColourIterativeClusterFit) != 0 ? MaxIterations : 1;

        // initialise the metric (old perceptual = 0.2126f, 0.7152f, 0.0722f)
        if (metric.HasValue)
          _metric = new Vector4F(metric.Value.X, metric.Value.Y, metric.Value.Z, 1.0f);
        else
          _metric = Vector4F.One;

        // initialise the best error
        _besterror = new Vector4F(float.MaxValue);

        // cache some values
        int count = _colours.Count;
        Vector3F[] values = _colours.Points;

        // get the covariance matrix
        Sym3x3 covariance = ComputeWeightedCovariance(count, values, _colours.Weights);

        // compute the principle component
        _principle = ComputePrincipleComponent(covariance);
      }


      private unsafe bool ConstructOrdering(Vector3F axis, int iteration)
      {
        // cache some values
        int count = _colours.Count;
        Vector3F[] values = _colours.Points;

        // build the list of dot products
        float* dps = stackalloc float[16];
        fixed (byte* pOrder = _order)
        {
          byte* order = pOrder + 16 * iteration;
          for (int i = 0; i < count; ++i)
          {
            dps[i] = Vector3F.Dot(values[i], axis);
            order[i] = (byte)i;
          }

          // stable sort using them
          for (int i = 0; i < count; ++i)
          {
            for (int j = i; j > 0 && dps[j] < dps[j - 1]; --j)
            {
              MathHelper.Swap(ref dps[j], ref dps[j - 1]);
              MathHelper.Swap(ref order[j], ref order[j - 1]);
            }
          }

          // check this ordering is unique
          for (int it = 0; it < iteration; ++it)
          {
            byte* prev = pOrder + 16 * it;
            bool same = true;
            for (int i = 0; i < count; ++i)
            {
              if (order[i] != prev[i])
              {
                same = false;
                break;
              }
            }
            if (same)
              return false;
          }

          // copy the ordering and weight all the points
          Vector3F[] unweighted = _colours.Points;
          float[] weights = _colours.Weights;
          _xsum_wsum = new Vector4F(0.0f);
          for (int i = 0; i < count; ++i)
          {
            int j = order[i];
            Vector4F p = new Vector4F(unweighted[j].X, unweighted[j].Y, unweighted[j].Z, 1.0f);
            Vector4F w = new Vector4F(weights[j]);
            Vector4F x = p * w;
            _points_weights[i] = x;
            _xsum_wsum += x;
          }
        }

        return true;
      }


      public unsafe void Compress(byte* block)
      {
        bool isDxt1 = ((_flags & SquishFlags.Dxt1) != 0);
        if (isDxt1)
        {
          Compress3(block);
          if (!_colours.IsTransparent)
            Compress4(block);
        }
        else
          Compress4(block);
      }


      private unsafe void Compress3(byte* block)
      {
        // declare variables
        int count = _colours.Count;
        Vector4F two = new Vector4F(2.0f);
        Vector4F half_half2 = new Vector4F(0.5f, 0.5f, 0.5f, 0.25f);
        Vector4F half = new Vector4F(0.5f);
        Vector4F grid = new Vector4F(31.0f, 63.0f, 31.0f, 0.0f);
        Vector4F gridrcp = new Vector4F(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f, 0.0f);

        // prepare an ordering using the principle axis
        ConstructOrdering(_principle, 0);

        // check all possible clusters and iterate on the total order
        Vector4F beststart = new Vector4F(0.0f);
        Vector4F bestend = new Vector4F(0.0f);
        Vector4F besterror = _besterror;
        byte* bestindices = stackalloc byte[16];
        int bestiteration = 0;
        int besti = 0, bestj = 0;

        // loop over iterations (we avoid the case that all points in first or last cluster)
        for (int iterationIndex = 0; ; )
        {
          // first cluster [0,i) is at the start
          Vector4F part0 = new Vector4F(0.0f);
          for (int i = 0; i < count; ++i)
          {
            // second cluster [i,j) is half along
            Vector4F part1 = (i == 0) ? _points_weights[0] : new Vector4F(0.0f);
            int jmin = (i == 0) ? 1 : i;
            for (int j = jmin; ; )
            {
              // last cluster [j,count) is at the end
              Vector4F part2 = _xsum_wsum - part1 - part0;

              // compute least squares terms directly
              Vector4F alphax_sum = MultiplyAdd(part1, half_half2, part0);
              Vector4F alpha2_sum = new Vector4F(alphax_sum.W);

              Vector4F betax_sum = MultiplyAdd(part1, half_half2, part2);
              Vector4F beta2_sum = new Vector4F(betax_sum.W);

              Vector4F alphabeta_sum = new Vector4F((part1 * half_half2).W);

              // compute the least-squares optimal points
              Vector4F factor =
                Reciprocal(NegativeMultiplySubtract(alphabeta_sum, alphabeta_sum, alpha2_sum * beta2_sum));
              Vector4F a = NegativeMultiplySubtract(betax_sum, alphabeta_sum, alphax_sum * beta2_sum) * factor;
              Vector4F b = NegativeMultiplySubtract(alphax_sum, alphabeta_sum, betax_sum * alpha2_sum) * factor;

              // clamp to the grid
              a = Vector4F.Clamp(a, 0.0f, 1.0f);
              b = Vector4F.Clamp(b, 0.0f, 1.0f);
              a = Truncate(MultiplyAdd(grid, a, half)) * gridrcp;
              b = Truncate(MultiplyAdd(grid, b, half)) * gridrcp;

              // compute the error (we skip the constant xxsum)
              Vector4F e1 = MultiplyAdd(a * a, alpha2_sum, b * b * beta2_sum);
              Vector4F e2 = NegativeMultiplySubtract(a, alphax_sum, a * b * alphabeta_sum);
              Vector4F e3 = NegativeMultiplySubtract(b, betax_sum, e2);
              Vector4F e4 = MultiplyAdd(two, e3, e1);

              // apply the metric to the error term
              Vector4F e5 = e4 * _metric;
              Vector4F error = new Vector4F(e5.X + e5.Y + e5.Z);

              // keep the solution if it wins
              if (CompareAnyLessThan(error, besterror))
              {
                beststart = a;
                bestend = b;
                besti = i;
                bestj = j;
                besterror = error;
                bestiteration = iterationIndex;
              }

              // advance
              if (j == count)
                break;
              part1 += _points_weights[j];
              ++j;
            }

            // advance
            part0 += _points_weights[i];
          }

          // stop if we didn't improve in this iteration
          if (bestiteration != iterationIndex)
            break;

          // advance if possible
          ++iterationIndex;
          if (iterationIndex == _iterationCount)
            break;

          // stop if a new iteration is an ordering that has already been tried
          Vector3F axis = (bestend - beststart).XYZ;
          if (!ConstructOrdering(axis, iterationIndex))
            break;
        }

        // save the block if necessary
        if (CompareAnyLessThan(besterror, _besterror))
        {
          // remap the indices
          fixed (byte* pOrder = _order)
          {
            byte* order = pOrder + 16 * bestiteration;

            byte* unordered = stackalloc byte[16];
            for (int m = 0; m < besti; ++m)
              unordered[order[m]] = 0;
            for (int m = besti; m < bestj; ++m)
              unordered[order[m]] = 2;
            for (int m = bestj; m < count; ++m)
              unordered[order[m]] = 1;

            _colours.RemapIndices(unordered, bestindices);
          }

          // save the block
          WriteColourBlock3(beststart.XYZ, bestend.XYZ, bestindices, block);

          // save the error
          _besterror = besterror;
        }
      }


      private unsafe void Compress4(byte* block)
      {
        // declare variables
        int count = _colours.Count;
        Vector4F two = new Vector4F(2.0f);
        Vector4F onethird_onethird2 = new Vector4F(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 9.0f);
        Vector4F twothirds_twothirds2 = new Vector4F(2.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f, 4.0f / 9.0f);
        Vector4F twonineths = new Vector4F(2.0f / 9.0f);
        Vector4F half = new Vector4F(0.5f);
        Vector4F grid = new Vector4F(31.0f, 63.0f, 31.0f, 0.0f);
        Vector4F gridrcp = new Vector4F(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f, 0.0f);

        // prepare an ordering using the principle axis
        ConstructOrdering(_principle, 0);

        // check all possible clusters and iterate on the total order
        Vector4F beststart = new Vector4F(0.0f);
        Vector4F bestend = new Vector4F(0.0f);
        Vector4F besterror = _besterror;
        byte* bestindices = stackalloc byte[16];
        int bestiteration = 0;
        int besti = 0, bestj = 0, bestk = 0;

        // loop over iterations (we avoid the case that all points in first or last cluster)
        for (int iterationIndex = 0; ; )
        {
          // first cluster [0,i) is at the start
          Vector4F part0 = new Vector4F(0.0f);
          for (int i = 0; i < count; ++i)
          {
            // second cluster [i,j) is one third along
            Vector4F part1 = new Vector4F(0.0f);
            for (int j = i; ; )
            {
              // third cluster [j,k) is two thirds along
              Vector4F part2 = (j == 0) ? _points_weights[0] : new Vector4F(0.0f);
              int kmin = (j == 0) ? 1 : j;
              for (int k = kmin; ; )
              {
                // last cluster [k,count) is at the end
                Vector4F part3 = _xsum_wsum - part2 - part1 - part0;

                // compute least squares terms directly
                Vector4F alphax_sum = MultiplyAdd(part2, onethird_onethird2,
                  MultiplyAdd(part1, twothirds_twothirds2, part0));
                Vector4F alpha2_sum = new Vector4F(alphax_sum.W);

                Vector4F betax_sum = MultiplyAdd(part1, onethird_onethird2,
                  MultiplyAdd(part2, twothirds_twothirds2, part3));
                Vector4F beta2_sum = new Vector4F(betax_sum.W);

                Vector4F alphabeta_sum = twonineths * (part1 + part2).W;

                // compute the least-squares optimal points
                Vector4F factor =
                  Reciprocal(NegativeMultiplySubtract(alphabeta_sum, alphabeta_sum, alpha2_sum * beta2_sum));
                Vector4F a = NegativeMultiplySubtract(betax_sum, alphabeta_sum, alphax_sum * beta2_sum) * factor;
                Vector4F b = NegativeMultiplySubtract(alphax_sum, alphabeta_sum, betax_sum * alpha2_sum) * factor;

                // clamp to the grid
                a = Vector4F.Clamp(a, 0.0f, 1.0f);
                b = Vector4F.Clamp(b, 0.0f, 1.0f);
                a = Truncate(MultiplyAdd(grid, a, half)) * gridrcp;
                b = Truncate(MultiplyAdd(grid, b, half)) * gridrcp;

                // compute the error (we skip the constant xxsum)
                Vector4F e1 = MultiplyAdd(a * a, alpha2_sum, b * b * beta2_sum);
                Vector4F e2 = NegativeMultiplySubtract(a, alphax_sum, a * b * alphabeta_sum);
                Vector4F e3 = NegativeMultiplySubtract(b, betax_sum, e2);
                Vector4F e4 = MultiplyAdd(two, e3, e1);

                // apply the metric to the error term
                Vector4F e5 = e4 * _metric;
                Vector4F error = new Vector4F(e5.X + e5.Y + e5.Z);

                // keep the solution if it wins
                if (CompareAnyLessThan(error, besterror))
                {
                  beststart = a;
                  bestend = b;
                  besterror = error;
                  besti = i;
                  bestj = j;
                  bestk = k;
                  bestiteration = iterationIndex;
                }

                // advance
                if (k == count)
                  break;
                part2 += _points_weights[k];
                ++k;
              }

              // advance
              if (j == count)
                break;
              part1 += _points_weights[j];
              ++j;
            }

            // advance
            part0 += _points_weights[i];
          }

          // stop if we didn't improve in this iteration
          if (bestiteration != iterationIndex)
            break;

          // advance if possible
          ++iterationIndex;
          if (iterationIndex == _iterationCount)
            break;

          // stop if a new iteration is an ordering that has already been tried
          Vector3F axis = (bestend - beststart).XYZ;
          if (!ConstructOrdering(axis, iterationIndex))
            break;
        }

        // save the block if necessary
        if (CompareAnyLessThan(besterror, _besterror))
        {
          // remap the indices
          fixed (byte* pOrder = _order)
          {
            byte* order = pOrder + 16 * bestiteration;

            byte* unordered = stackalloc byte[16];
            for (int m = 0; m < besti; ++m)
              unordered[order[m]] = 0;
            for (int m = besti; m < bestj; ++m)
              unordered[order[m]] = 2;
            for (int m = bestj; m < bestk; ++m)
              unordered[order[m]] = 3;
            for (int m = bestk; m < count; ++m)
              unordered[order[m]] = 1;

            _colours.RemapIndices(unordered, bestindices);
          }
          // save the block
          WriteColourBlock4(beststart.XYZ, bestend.XYZ, bestindices, block);

          // save the error
          _besterror = besterror;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region ColourBlock.h/.cpp
    //--------------------------------------------------------------

    private static int FloatTo565(Vector3F colour)
    {
      // get the components in the correct range
      int r = FloatToInt(31.0f * colour.X, 31);
      int g = FloatToInt(63.0f * colour.Y, 63);
      int b = FloatToInt(31.0f * colour.Z, 31);

      // pack into a single value
      return (r << 11) | (g << 5) | b;
    }


    private static unsafe uint Unpack565(byte* packed, byte* colour)
    {
      // build the packed value
      ushort value = (ushort)(packed[0] | (packed[1] << 8));

      // get the components in the stored range
      byte red = (byte)((value >> 11) & 0x1f);
      byte green = (byte)((value >> 5) & 0x3f);
      byte blue = (byte)(value & 0x1f);

      // scale up to 8 bits
      colour[0] = (byte)((red << 3) | (red >> 2));
      colour[1] = (byte)((green << 2) | (green >> 4));
      colour[2] = (byte)((blue << 3) | (blue >> 2));
      colour[3] = 255;

      // return the value
      return value;
    }


    private static unsafe void WriteColourBlock(int a, int b, byte* indices, byte* block)
    {
      // write the endpoints
      block[0] = (byte)(a & 0xff);
      block[1] = (byte)(a >> 8);
      block[2] = (byte)(b & 0xff);
      block[3] = (byte)(b >> 8);

      // write the indices
      for (int i = 0; i < 4; ++i)
      {
        byte* ind = indices + 4 * i;
        block[4 + i] = (byte)(ind[0] | (ind[1] << 2) | (ind[2] << 4) | (ind[3] << 6));
      }
    }


    private static unsafe void WriteColourBlock3(Vector3F start, Vector3F end, byte* indices, byte* block)
    {
      // get the packed values
      int a = FloatTo565(start);
      int b = FloatTo565(end);

      // remap the indices
      byte* remapped = stackalloc byte[16];
      if (a <= b)
      {
        // use the indices directly
        for (int i = 0; i < 16; ++i)
          remapped[i] = indices[i];
      }
      else
      {
        // swap a and b
        MathHelper.Swap(ref a, ref b);
        for (int i = 0; i < 16; ++i)
        {
          if (indices[i] == 0)
            remapped[i] = 1;
          else if (indices[i] == 1)
            remapped[i] = 0;
          else
            remapped[i] = indices[i];
        }
      }

      // write the block
      WriteColourBlock(a, b, remapped, block);
    }


    private static unsafe void WriteColourBlock4(Vector3F start, Vector3F end, byte* indices, byte* block)
    {
      // get the packed values
      int a = FloatTo565(start);
      int b = FloatTo565(end);

      // remap the indices
      byte* remapped = stackalloc byte[16];
      if (a < b)
      {
        // swap a and b
        MathHelper.Swap(ref a, ref b);
        for (int i = 0; i < 16; ++i)
          remapped[i] = (byte)((indices[i] ^ 0x1) & 0x3);
      }
      else if (a == b)
      {
        // use index 0
        for (int i = 0; i < 16; ++i)
          remapped[i] = 0;
      }
      else
      {
        // use the indices directly
        for (int i = 0; i < 16; ++i)
          remapped[i] = indices[i];
      }

      // write the block
      WriteColourBlock(a, b, remapped, block);
    }


    private static unsafe void DecompressColour(byte* rgba, byte* block, bool isDxt1)
    {
      // unpack the endpoints
      byte* codes = stackalloc byte[16];
      uint a = Unpack565(block, codes);
      uint b = Unpack565(block + 2, codes + 4);

      // generate the midpoints
      for (int i = 0; i < 3; ++i)
      {
        int c = codes[i];
        int d = codes[4 + i];

        if (isDxt1 && a <= b)
        {
          codes[8 + i] = (byte)((c + d) / 2);
          codes[12 + i] = 0;
        }
        else
        {
          codes[8 + i] = (byte)((2 * c + d) / 3);
          codes[12 + i] = (byte)((c + 2 * d) / 3);
        }
      }

      // fill in alpha for the intermediate values
      codes[8 + 3] = 255;
      codes[12 + 3] = (isDxt1 && a <= b) ? (byte)0 : (byte)255;

      // unpack the indices
      byte* indices = stackalloc byte[16];
      for (int i = 0; i < 4; ++i)
      {
        byte* ind = indices + 4 * i;
        byte packed = block[4 + i];

        ind[0] = (byte)(packed & 0x3);
        ind[1] = (byte)((packed >> 2) & 0x3);
        ind[2] = (byte)((packed >> 4) & 0x3);
        ind[3] = (byte)((packed >> 6) & 0x3);
      }

      // store out the colours
      for (int i = 0; i < 16; ++i)
      {
        int offset = 4 * indices[i];
        for (int j = 0; j < 4; ++j)
          rgba[4 * i + j] = codes[offset + j];
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region ColourSet.h/.cpp
    //--------------------------------------------------------------

    /// <summary>
    /// Represents a set of block colours.
    /// </summary>
    private class ColourSet
    {
      private int _count;
      private readonly Vector3F[] _points;
      private readonly float[] _weights;
      private readonly int[] _remap;
      private bool _transparent;


      public int Count
      {
        get { return _count; }
      }


      public Vector3F[] Points
      {
        get { return _points; }
      }


      public float[] Weights
      {
        get { return _weights; }
      }


      public bool IsTransparent
      {
        get { return _transparent; }
      }


      public ColourSet()
      {
        _points = new Vector3F[16];
        _weights = new float[16];
        _remap = new int[16];
      }


      public unsafe void Set(byte* rgba, int mask, SquishFlags flags)
      {
        _count = 0;
        _transparent = false;

        // check the compression mode for dxt1
        bool isDxt1 = ((flags & SquishFlags.Dxt1) != 0);
        bool weightByAlpha = ((flags & SquishFlags.WeightColourByAlpha) != 0);

        // create the minimal set
        for (int i = 0; i < 16; ++i)
        {
          // check this pixel is enabled
          int bit = 1 << i;
          if ((mask & bit) == 0)
          {
            _remap[i] = -1;
            continue;
          }

          // check for transparent pixels when using dxt1
          if (isDxt1 && rgba[4 * i + 3] < 128)
          {
            _remap[i] = -1;
            _transparent = true;
            continue;
          }

          // loop over previous points for a match
          for (int j = 0; ; ++j)
          {
            // allocate a new point
            if (j == i)
            {
              // normalise coordinates to [0,1]
              float x = rgba[4 * i] / 255.0f;
              float y = rgba[4 * i + 1] / 255.0f;
              float z = rgba[4 * i + 2] / 255.0f;

              // ensure there is always non-zero weight even for zero alpha
              float w = (rgba[4 * i + 3] + 1) / 256.0f;

              // add the point
              _points[_count] = new Vector3F(x, y, z);
              _weights[_count] = (weightByAlpha ? w : 1.0f);
              _remap[i] = _count;

              // advance
              ++_count;
              break;
            }

            // check for a match
            int oldbit = 1 << j;
            bool match = ((mask & oldbit) != 0)
                         && (rgba[4 * i] == rgba[4 * j])
                         && (rgba[4 * i + 1] == rgba[4 * j + 1])
                         && (rgba[4 * i + 2] == rgba[4 * j + 2])
                         && (rgba[4 * j + 3] >= 128 || !isDxt1);
            if (match)
            {
              // get the index of the match
              int index = _remap[j];

              // ensure there is always non-zero weight even for zero alpha
              float w = (rgba[4 * i + 3] + 1) / 256.0f;

              // map to this point and increase the weight
              _weights[index] += (weightByAlpha ? w : 1.0f);
              _remap[i] = index;
              break;
            }
          }
        }

        // square root the weights
        for (int i = 0; i < _count; ++i)
          _weights[i] = (float)Math.Sqrt(_weights[i]);
      }


      public unsafe void RemapIndices(byte* source, byte* target)
      {
        for (int i = 0; i < 16; ++i)
        {
          int j = _remap[i];
          if (j == -1)
            target[i] = 3;
          else
            target[i] = source[j];
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region math.h/.cpp
    //--------------------------------------------------------------

    private static Vector4F MultiplyAdd(Vector4F a, float b, Vector4F c)
    {
      return a * b + c;
    }


    private static Vector4F MultiplyAdd(Vector4F a, Vector4F b, Vector4F c)
    {
      return a * b + c;
    }


    private static Vector4F NegativeMultiplySubtract(Vector4F a, Vector4F b, Vector4F c)
    {
      return c - a * b;
    }


    private static Vector4F Reciprocal(Vector4F v)
    {
      return new Vector4F(1.0f / v.X,
                          1.0f / v.Y,
                          1.0f / v.Z,
                          1.0f / v.W);
    }


    private static bool CompareAnyLessThan(Vector4F left, Vector4F right)
    {
      return left.X < right.X
             || left.Y < right.Y
             || left.Z < right.Z
             || left.W < right.W;
    }


    private static Vector3F Truncate(Vector3F v)
    {
      return new Vector3F((float)Math.Truncate(v.X),
                          (float)Math.Truncate(v.Y),
                          (float)Math.Truncate(v.Z));
    }


    private static Vector4F Truncate(Vector4F v)
    {
      return new Vector4F((float)Math.Truncate(v.X),
                          (float)Math.Truncate(v.Y),
                          (float)Math.Truncate(v.Z),
                          (float)Math.Truncate(v.W));
    }


    private struct Sym3x3
    {
      public float M0, M1, M2, M3, M4, M5;
    }


    private static Sym3x3 ComputeWeightedCovariance(int n, Vector3F[] points, float[] weights)
    {
      // compute the centroid
      float total = 0.0f;
      Vector3F centroid = new Vector3F(0.0f);
      for (int i = 0; i < n; ++i)
      {
        total += weights[i];
        centroid += weights[i] * points[i];
      }
      if (total > float.Epsilon)
        centroid /= total;

      // accumulate the covariance matrix
      Sym3x3 covariance = new Sym3x3();
      for (int i = 0; i < n; ++i)
      {
        Vector3F a = points[i] - centroid;
        Vector3F b = weights[i] * a;

        covariance.M0 += a.X * b.X;
        covariance.M1 += a.X * b.Y;
        covariance.M2 += a.X * b.Z;
        covariance.M3 += a.Y * b.Y;
        covariance.M4 += a.Y * b.Z;
        covariance.M5 += a.Z * b.Z;
      }

      // return it
      return covariance;
    }


    private static Vector3F ComputePrincipleComponent(Sym3x3 matrix)
    {
      Vector4F row0 = new Vector4F(matrix.M0, matrix.M1, matrix.M2, 0.0f);
      Vector4F row1 = new Vector4F(matrix.M1, matrix.M3, matrix.M4, 0.0f);
      Vector4F row2 = new Vector4F(matrix.M2, matrix.M4, matrix.M5, 0.0f);
      Vector4F v = Vector4F.One;

      const int POWER_ITERATION_COUNT = 8;
      for (int i = 0; i < POWER_ITERATION_COUNT; ++i)
      {
        // matrix multiply
        Vector4F w = row0 * v.X;
        w = MultiplyAdd(row1, v.Y, w);
        w = MultiplyAdd(row2, v.Z, w);

        // get max component from xyz in all channels
        Vector4F a = new Vector4F(Math.Max(w.X, Math.Max(w.Y, w.Z)));

        // divide through and advance
        v = w / a;
      }

      return v.XYZ;
    }
    #endregion


    //--------------------------------------------------------------
    #region rangefit.h/.cpp
    //--------------------------------------------------------------

    private class RangeFit
    {
      private ColourSet _colours;
      private SquishFlags _flags;
      private Vector3F _metric;
      private Vector3F _start;
      private Vector3F _end;
      private float _besterror;


      public void Initialize(ColourSet colours, SquishFlags flags, Vector3F? metric)
      {
        _colours = colours;
        _flags = flags;

        // initialise the metric (old perceptual = 0.2126f, 0.7152f, 0.0722f)
        if (metric.HasValue)
          _metric = metric.Value;
        else
          _metric = new Vector3F(1.0f);

        // initialise the best error
        _besterror = float.MaxValue;

        // cache some values
        int count = _colours.Count;
        Vector3F[] values = _colours.Points;
        float[] weights = _colours.Weights;

        // get the covariance matrix
        Sym3x3 covariance = ComputeWeightedCovariance(count, values, weights);

        // compute the principle component
        Vector3F principle = ComputePrincipleComponent(covariance);

        // get the min and max range as the codebook endpoints
        Vector3F start = new Vector3F(0.0f);
        Vector3F end = new Vector3F(0.0f);
        if (count > 0)
        {
          float min, max;

          // compute the range
          start = end = values[0];
          min = max = Vector3F.Dot(values[0], principle);
          for (int i = 1; i < count; ++i)
          {
            float val = Vector3F.Dot(values[i], principle);
            if (val < min)
            {
              start = values[i];
              min = val;
            }
            else if (val > max)
            {
              end = values[i];
              max = val;
            }
          }
        }

        // clamp the output to [0, 1]
        start = Vector3F.Clamp(start, 0, 1);
        end = Vector3F.Clamp(end, 0, 1);

        // clamp to the grid and save
        Vector3F grid = new Vector3F(31.0f, 63.0f, 31.0f);
        Vector3F gridrcp = new Vector3F(1.0f / 31.0f, 1.0f / 63.0f, 1.0f / 31.0f);
        Vector3F half = new Vector3F(0.5f);
        _start = Truncate(grid * start + half) * gridrcp;
        _end = Truncate(grid * end + half) * gridrcp;
      }


      public unsafe void Compress(byte* block)
      {
        bool isDxt1 = ((_flags & SquishFlags.Dxt1) != 0);
        if (isDxt1)
        {
          Compress3(block);
          if (!_colours.IsTransparent)
            Compress4(block);
        }
        else
          Compress4(block);
      }


      private unsafe void Compress3(byte* block)
      {
        // cache some values
        int count = _colours.Count;
        Vector3F[] values = _colours.Points;

        // create a codebook
        Vector3F* codes = stackalloc Vector3F[3];
        codes[0] = _start;
        codes[1] = _end;
        codes[2] = 0.5f * _start + 0.5f * _end;

        // match each point to the closest code
        byte* closest = stackalloc byte[16];
        float error = 0.0f;
        for (int i = 0; i < count; ++i)
        {
          // find the closest code
          float dist = float.MaxValue;
          int idx = 0;
          for (int j = 0; j < 3; ++j)
          {
            float d = (_metric * (values[i] - codes[j])).LengthSquared;
            if (d < dist)
            {
              dist = d;
              idx = j;
            }
          }

          // save the index
          closest[i] = (byte)idx;

          // accumulate the error
          error += dist;
        }

        // save this scheme if it wins
        if (error < _besterror)
        {
          // remap the indices
          byte* indices = stackalloc byte[16];
          _colours.RemapIndices(closest, indices);

          // save the block
          WriteColourBlock3(_start, _end, indices, block);

          // save the error
          _besterror = error;
        }
      }


      private unsafe void Compress4(byte* block)
      {
        // cache some values
        int count = _colours.Count;
        Vector3F[] values = _colours.Points;

        // create a codebook
        Vector3F* codes = stackalloc Vector3F[4];
        codes[0] = _start;
        codes[1] = _end;
        codes[2] = (2.0f / 3.0f) * _start + (1.0f / 3.0f) * _end;
        codes[3] = (1.0f / 3.0f) * _start + (2.0f / 3.0f) * _end;

        // match each point to the closest code
        byte* closest = stackalloc byte[16];
        float error = 0.0f;
        for (int i = 0; i < count; ++i)
        {
          // find the closest code
          float dist = float.MaxValue;
          int idx = 0;
          for (int j = 0; j < 4; ++j)
          {
            float d = (_metric * (values[i] - codes[j])).LengthSquared;
            if (d < dist)
            {
              dist = d;
              idx = j;
            }
          }

          // save the index
          closest[i] = (byte)idx;

          // accumulate the error
          error += dist;
        }

        // save this scheme if it wins
        if (error < _besterror)
        {
          // remap the indices
          byte* indices = stackalloc byte[16];
          _colours.RemapIndices(closest, indices);

          // save the block
          WriteColourBlock4(_start, _end, indices, block);

          // save the error
          _besterror = error;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region singlecolourlookup.h
    //--------------------------------------------------------------

    private struct SourceBlock
    {
      public byte Start;
      public byte End;
      public byte Error;


      public SourceBlock(byte start, byte end, byte error)
      {
        Start = start;
        End = end;
        Error = error;
      }
    }


    private struct SingleColourLookup
    {
      public SourceBlock Sources0;
      public SourceBlock Sources1;


      public SingleColourLookup(SourceBlock sources0, SourceBlock sources1)
      {
        Sources0 = sources0;
        Sources1 = sources1;
      }
    }


    private static readonly SingleColourLookup[] _lookup_5_3 =
    {
      new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 0, 1)),
      new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 0, 2)),
      new SingleColourLookup(new SourceBlock(0, 0, 3), new SourceBlock(0, 1, 1)),
      new SingleColourLookup(new SourceBlock(0, 0, 4), new SourceBlock(0, 1, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 1, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 1, 2)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 2, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 2, 2)),
      new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 3, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 4), new SourceBlock(0, 3, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 3, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 3, 2)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 4, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 4, 2)),
      new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 5, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 4), new SourceBlock(0, 5, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 5, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 5, 2)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 6, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 6, 2)),
      new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 7, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 4), new SourceBlock(0, 7, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 7, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 7, 2)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(1, 7, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(1, 7, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 8, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 8, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(2, 7, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(2, 7, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 9, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 9, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(3, 7, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(3, 7, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 10, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 10, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(0, 10, 2)),
      new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 11, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 4), new SourceBlock(0, 11, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 11, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 11, 2)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 12, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 12, 2)),
      new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 13, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 4), new SourceBlock(0, 13, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 13, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 13, 2)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 14, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 14, 2)),
      new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 15, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 4), new SourceBlock(0, 15, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 15, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 15, 2)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(1, 15, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(1, 15, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 16, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 16, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(2, 15, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(2, 15, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 17, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 17, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(3, 15, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(3, 15, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 18, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 18, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(0, 18, 2)),
      new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 19, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 4), new SourceBlock(0, 19, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 19, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 19, 2)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 20, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 20, 2)),
      new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 21, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 4), new SourceBlock(0, 21, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(0, 21, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 21, 2)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 22, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 22, 2)),
      new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(0, 23, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 4), new SourceBlock(0, 23, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(0, 23, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(0, 23, 2)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(1, 23, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(1, 23, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 24, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 24, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(2, 23, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(2, 23, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(0, 25, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(0, 25, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(3, 23, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(3, 23, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 26, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 26, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 26, 2)),
      new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(0, 27, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 4), new SourceBlock(0, 27, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(0, 27, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 27, 2)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 28, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 28, 2)),
      new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(0, 29, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 4), new SourceBlock(0, 29, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(0, 29, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 29, 2)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 30, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 30, 2)),
      new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(0, 31, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 4), new SourceBlock(0, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(0, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(0, 31, 2)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(1, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(4, 28, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(4, 28, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(2, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(2, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(4, 29, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(4, 29, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(3, 31, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(3, 31, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(4, 30, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(4, 30, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(4, 30, 2)),
      new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(4, 31, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 4), new SourceBlock(4, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(4, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(4, 31, 2)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(5, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(5, 31, 2)),
      new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(6, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 4), new SourceBlock(6, 31, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(6, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(6, 31, 2)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(7, 31, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(7, 31, 2)),
      new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(8, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 4), new SourceBlock(8, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(8, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(8, 31, 2)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(9, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(9, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(12, 28, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(12, 28, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(10, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(10, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(12, 29, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(12, 29, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(11, 31, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(11, 31, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(12, 30, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(12, 30, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(12, 30, 2)),
      new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(12, 31, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 4), new SourceBlock(12, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(12, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(12, 31, 2)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(13, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(13, 31, 2)),
      new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(14, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 4), new SourceBlock(14, 31, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(14, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(14, 31, 2)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(15, 31, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(15, 31, 2)),
      new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(16, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 4), new SourceBlock(16, 31, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(16, 31, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(16, 31, 2)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(17, 31, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(17, 31, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(20, 28, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(20, 28, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(18, 31, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(18, 31, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(20, 29, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(20, 29, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(19, 31, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(19, 31, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(20, 30, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(20, 30, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(20, 30, 2)),
      new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(20, 31, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 4), new SourceBlock(20, 31, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(20, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(20, 31, 2)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(21, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(21, 31, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(21, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(21, 31, 2)),
      new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(22, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 4), new SourceBlock(22, 31, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(22, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(22, 31, 2)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(23, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(23, 31, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(23, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(23, 31, 2)),
      new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(24, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 4), new SourceBlock(24, 31, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(24, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(24, 31, 2)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(25, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(25, 31, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(28, 28, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(28, 28, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(26, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(26, 31, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(28, 29, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 29, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(27, 31, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(27, 31, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(28, 30, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(28, 30, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(28, 30, 2)),
      new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 31, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 4), new SourceBlock(28, 31, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(28, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(28, 31, 2)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(29, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(29, 31, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(29, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(29, 31, 2)),
      new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(30, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 4), new SourceBlock(30, 31, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 3), new SourceBlock(30, 31, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(30, 31, 2)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(31, 31, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(31, 31, 0)),
    };


    private static readonly SingleColourLookup[] _lookup_6_3 =
    {
      new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 1)),
      new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 1, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 2, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 3, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 4, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 4, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 5, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 5, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 6, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 6, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 7, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 7, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 8, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 8, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 9, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 9, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 10, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 10, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 11, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(0, 11, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 12, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 12, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 13, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 13, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 14, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 14, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 15, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 15, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 16, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 16, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 17, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(0, 17, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 18, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 18, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 19, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(0, 19, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 20, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 20, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 21, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 21, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 22, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 22, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 23, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 23, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 24, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 24, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 25, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(0, 25, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 26, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 26, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 27, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 27, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 28, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 28, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 29, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 29, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 30, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 30, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 31, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(1, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(0, 32, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(2, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 33, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(3, 31, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(0, 34, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(4, 31, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(0, 35, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(5, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(0, 36, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(6, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(0, 37, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(7, 31, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(0, 38, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(8, 31, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(0, 39, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(9, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(0, 40, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(10, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(0, 41, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(11, 31, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(0, 42, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(12, 31, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(0, 43, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(13, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(0, 44, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(14, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(0, 45, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(15, 31, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(0, 46, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(0, 47, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(0, 47, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(0, 48, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(0, 48, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(0, 49, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(0, 49, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(0, 50, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(0, 50, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(0, 51, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(0, 51, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(0, 52, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(0, 52, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(0, 53, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(0, 53, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(0, 54, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(0, 54, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(0, 55, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(0, 55, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(0, 56, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(0, 56, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(0, 57, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(0, 57, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(0, 58, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(0, 58, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(0, 59, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(0, 59, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(0, 60, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(0, 60, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(0, 61, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(0, 61, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(0, 62, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(0, 62, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(0, 63, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(0, 63, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(1, 63, 1)),
      new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(1, 63, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 0), new SourceBlock(16, 48, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(2, 63, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(16, 49, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(3, 63, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 0), new SourceBlock(16, 50, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(4, 63, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 2), new SourceBlock(16, 51, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(5, 63, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 0), new SourceBlock(16, 52, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(6, 63, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 2), new SourceBlock(16, 53, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(7, 63, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 0), new SourceBlock(16, 54, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(8, 63, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 2), new SourceBlock(16, 55, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(9, 63, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 0), new SourceBlock(16, 56, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(10, 63, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 2), new SourceBlock(16, 57, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(11, 63, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 0), new SourceBlock(16, 58, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(12, 63, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 2), new SourceBlock(16, 59, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(13, 63, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 0), new SourceBlock(16, 60, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(14, 63, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 2), new SourceBlock(16, 61, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(15, 63, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 0), new SourceBlock(16, 62, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(16, 63, 1)),
      new SingleColourLookup(new SourceBlock(39, 0, 2), new SourceBlock(16, 63, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(17, 63, 1)),
      new SingleColourLookup(new SourceBlock(40, 0, 0), new SourceBlock(17, 63, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(18, 63, 1)),
      new SingleColourLookup(new SourceBlock(40, 0, 2), new SourceBlock(18, 63, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(19, 63, 1)),
      new SingleColourLookup(new SourceBlock(41, 0, 0), new SourceBlock(19, 63, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(20, 63, 1)),
      new SingleColourLookup(new SourceBlock(41, 0, 2), new SourceBlock(20, 63, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(21, 63, 1)),
      new SingleColourLookup(new SourceBlock(42, 0, 0), new SourceBlock(21, 63, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(22, 63, 1)),
      new SingleColourLookup(new SourceBlock(42, 0, 2), new SourceBlock(22, 63, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(23, 63, 1)),
      new SingleColourLookup(new SourceBlock(43, 0, 0), new SourceBlock(23, 63, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(24, 63, 1)),
      new SingleColourLookup(new SourceBlock(43, 0, 2), new SourceBlock(24, 63, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(25, 63, 1)),
      new SingleColourLookup(new SourceBlock(44, 0, 0), new SourceBlock(25, 63, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(26, 63, 1)),
      new SingleColourLookup(new SourceBlock(44, 0, 2), new SourceBlock(26, 63, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(27, 63, 1)),
      new SingleColourLookup(new SourceBlock(45, 0, 0), new SourceBlock(27, 63, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(28, 63, 1)),
      new SingleColourLookup(new SourceBlock(45, 0, 2), new SourceBlock(28, 63, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(29, 63, 1)),
      new SingleColourLookup(new SourceBlock(46, 0, 0), new SourceBlock(29, 63, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(30, 63, 1)),
      new SingleColourLookup(new SourceBlock(46, 0, 2), new SourceBlock(30, 63, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(31, 63, 1)),
      new SingleColourLookup(new SourceBlock(47, 0, 0), new SourceBlock(31, 63, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(32, 63, 1)),
      new SingleColourLookup(new SourceBlock(47, 0, 2), new SourceBlock(32, 63, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(33, 63, 1)),
      new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(33, 63, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 0), new SourceBlock(48, 48, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(34, 63, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(48, 49, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(35, 63, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 0), new SourceBlock(48, 50, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(36, 63, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 2), new SourceBlock(48, 51, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(37, 63, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 0), new SourceBlock(48, 52, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(38, 63, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 2), new SourceBlock(48, 53, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(39, 63, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 0), new SourceBlock(48, 54, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(40, 63, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 2), new SourceBlock(48, 55, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(41, 63, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 0), new SourceBlock(48, 56, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(42, 63, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 2), new SourceBlock(48, 57, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(43, 63, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 0), new SourceBlock(48, 58, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(44, 63, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 2), new SourceBlock(48, 59, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(45, 63, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 0), new SourceBlock(48, 60, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(46, 63, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 2), new SourceBlock(48, 61, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(47, 63, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 0), new SourceBlock(48, 62, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(48, 63, 1)),
      new SingleColourLookup(new SourceBlock(55, 0, 2), new SourceBlock(48, 63, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(49, 63, 1)),
      new SingleColourLookup(new SourceBlock(56, 0, 0), new SourceBlock(49, 63, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(50, 63, 1)),
      new SingleColourLookup(new SourceBlock(56, 0, 2), new SourceBlock(50, 63, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(51, 63, 1)),
      new SingleColourLookup(new SourceBlock(57, 0, 0), new SourceBlock(51, 63, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(52, 63, 1)),
      new SingleColourLookup(new SourceBlock(57, 0, 2), new SourceBlock(52, 63, 0)),
      new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(53, 63, 1)),
      new SingleColourLookup(new SourceBlock(58, 0, 0), new SourceBlock(53, 63, 0)),
      new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(54, 63, 1)),
      new SingleColourLookup(new SourceBlock(58, 0, 2), new SourceBlock(54, 63, 0)),
      new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(55, 63, 1)),
      new SingleColourLookup(new SourceBlock(59, 0, 0), new SourceBlock(55, 63, 0)),
      new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(56, 63, 1)),
      new SingleColourLookup(new SourceBlock(59, 0, 2), new SourceBlock(56, 63, 0)),
      new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(57, 63, 1)),
      new SingleColourLookup(new SourceBlock(60, 0, 0), new SourceBlock(57, 63, 0)),
      new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(58, 63, 1)),
      new SingleColourLookup(new SourceBlock(60, 0, 2), new SourceBlock(58, 63, 0)),
      new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(59, 63, 1)),
      new SingleColourLookup(new SourceBlock(61, 0, 0), new SourceBlock(59, 63, 0)),
      new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(60, 63, 1)),
      new SingleColourLookup(new SourceBlock(61, 0, 2), new SourceBlock(60, 63, 0)),
      new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(61, 63, 1)),
      new SingleColourLookup(new SourceBlock(62, 0, 0), new SourceBlock(61, 63, 0)),
      new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 63, 1)),
      new SingleColourLookup(new SourceBlock(62, 0, 2), new SourceBlock(62, 63, 0)),
      new SingleColourLookup(new SourceBlock(63, 0, 1), new SourceBlock(63, 63, 1)),
      new SingleColourLookup(new SourceBlock(63, 0, 0), new SourceBlock(63, 63, 0)),
    };


    private static readonly SingleColourLookup[] _lookup_5_4 =
    {
      new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 1)),
      new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 1, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 3), new SourceBlock(0, 1, 1)),
      new SingleColourLookup(new SourceBlock(0, 0, 4), new SourceBlock(0, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 2, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 3, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(1, 2, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(1, 2, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 3), new SourceBlock(0, 4, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 4), new SourceBlock(0, 5, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 5, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 5, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 6, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 6, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(2, 3, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(2, 3, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 3), new SourceBlock(0, 7, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 4), new SourceBlock(1, 6, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(1, 6, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 8, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 9, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 10, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 3), new SourceBlock(0, 10, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 4), new SourceBlock(2, 7, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(2, 7, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 11, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(1, 10, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(1, 10, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 12, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 13, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 13, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 3), new SourceBlock(0, 13, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 4), new SourceBlock(0, 14, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 14, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(2, 11, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(2, 11, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 15, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(1, 14, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(1, 14, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 3), new SourceBlock(0, 16, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 4), new SourceBlock(0, 17, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 17, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(0, 17, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 18, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 18, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(2, 15, 1)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(2, 15, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 3), new SourceBlock(0, 19, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 4), new SourceBlock(1, 18, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(1, 18, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 20, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 21, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 21, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 21, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(0, 22, 1)),
      new SingleColourLookup(new SourceBlock(7, 0, 3), new SourceBlock(0, 22, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 4), new SourceBlock(2, 19, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(2, 19, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 23, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(1, 22, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(1, 22, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 24, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 25, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(0, 25, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 3), new SourceBlock(0, 25, 1)),
      new SingleColourLookup(new SourceBlock(8, 0, 4), new SourceBlock(0, 26, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 26, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(2, 23, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(2, 23, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 27, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(1, 26, 1)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(1, 26, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 3), new SourceBlock(0, 28, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 4), new SourceBlock(0, 29, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 29, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(0, 29, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 30, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 30, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(2, 27, 1)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(2, 27, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 3), new SourceBlock(0, 31, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 4), new SourceBlock(1, 30, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(1, 30, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(4, 24, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(1, 31, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(1, 31, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(1, 31, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(2, 30, 1)),
      new SingleColourLookup(new SourceBlock(11, 0, 3), new SourceBlock(2, 30, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 4), new SourceBlock(2, 31, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(2, 31, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(4, 27, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(3, 30, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 30, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(4, 28, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 31, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(3, 31, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 3), new SourceBlock(3, 31, 1)),
      new SingleColourLookup(new SourceBlock(12, 0, 4), new SourceBlock(4, 30, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(4, 30, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(6, 27, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(6, 27, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(4, 31, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(5, 30, 1)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(5, 30, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 3), new SourceBlock(8, 24, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 4), new SourceBlock(5, 31, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(5, 31, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(5, 31, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 30, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(6, 30, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 31, 1)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(6, 31, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 3), new SourceBlock(8, 27, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 4), new SourceBlock(7, 30, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(7, 30, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(8, 28, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(7, 31, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(7, 31, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(7, 31, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(8, 30, 1)),
      new SingleColourLookup(new SourceBlock(15, 0, 3), new SourceBlock(8, 30, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 4), new SourceBlock(10, 27, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(10, 27, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(8, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(9, 30, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(9, 30, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(12, 24, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(9, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(9, 31, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 3), new SourceBlock(9, 31, 1)),
      new SingleColourLookup(new SourceBlock(16, 0, 4), new SourceBlock(10, 30, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(10, 30, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(10, 31, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(10, 31, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(12, 27, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(11, 30, 1)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(11, 30, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 3), new SourceBlock(12, 28, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 4), new SourceBlock(11, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(11, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(11, 31, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(12, 30, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(12, 30, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(14, 27, 1)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(14, 27, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 3), new SourceBlock(12, 31, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 4), new SourceBlock(13, 30, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(13, 30, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(16, 24, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(13, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(13, 31, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(13, 31, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(14, 30, 1)),
      new SingleColourLookup(new SourceBlock(19, 0, 3), new SourceBlock(14, 30, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 4), new SourceBlock(14, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(14, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(16, 27, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(15, 30, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(15, 30, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(16, 28, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(15, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(15, 31, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 3), new SourceBlock(15, 31, 1)),
      new SingleColourLookup(new SourceBlock(20, 0, 4), new SourceBlock(16, 30, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(16, 30, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(18, 27, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(18, 27, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(16, 31, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(17, 30, 1)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(17, 30, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 3), new SourceBlock(20, 24, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 4), new SourceBlock(17, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(17, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(17, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(18, 30, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(18, 30, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(18, 31, 1)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(18, 31, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 3), new SourceBlock(20, 27, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 4), new SourceBlock(19, 30, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(19, 30, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(20, 28, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(19, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(19, 31, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(19, 31, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(20, 30, 1)),
      new SingleColourLookup(new SourceBlock(23, 0, 3), new SourceBlock(20, 30, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 4), new SourceBlock(22, 27, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(22, 27, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(20, 31, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(21, 30, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(21, 30, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(24, 24, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(21, 31, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(21, 31, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 3), new SourceBlock(21, 31, 1)),
      new SingleColourLookup(new SourceBlock(24, 0, 4), new SourceBlock(22, 30, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(22, 30, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(22, 31, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(22, 31, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(24, 27, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(23, 30, 1)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(23, 30, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 3), new SourceBlock(24, 28, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 4), new SourceBlock(23, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(23, 31, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(23, 31, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(24, 30, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(24, 30, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(26, 27, 1)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(26, 27, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 3), new SourceBlock(24, 31, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 4), new SourceBlock(25, 30, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(25, 30, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(28, 24, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(25, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(25, 31, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(25, 31, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(26, 30, 1)),
      new SingleColourLookup(new SourceBlock(27, 0, 3), new SourceBlock(26, 30, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 4), new SourceBlock(26, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(26, 31, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(28, 27, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(27, 30, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(27, 30, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(28, 28, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(27, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(27, 31, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 3), new SourceBlock(27, 31, 1)),
      new SingleColourLookup(new SourceBlock(28, 0, 4), new SourceBlock(28, 30, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(28, 30, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(30, 27, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(30, 27, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(28, 31, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(29, 30, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(29, 30, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 3), new SourceBlock(29, 30, 1)),
      new SingleColourLookup(new SourceBlock(29, 0, 4), new SourceBlock(29, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(29, 31, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(29, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(30, 30, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(30, 30, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(30, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(30, 31, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 3), new SourceBlock(30, 31, 1)),
      new SingleColourLookup(new SourceBlock(30, 0, 4), new SourceBlock(31, 30, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 3), new SourceBlock(31, 30, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(31, 30, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(31, 31, 1)),
      new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(31, 31, 0)),
    };


    private static readonly SingleColourLookup[] _lookup_6_4 =
    {
      new SingleColourLookup(new SourceBlock(0, 0, 0), new SourceBlock(0, 0, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 1), new SourceBlock(0, 1, 0)),
      new SingleColourLookup(new SourceBlock(0, 0, 2), new SourceBlock(0, 2, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 3, 1)),
      new SingleColourLookup(new SourceBlock(1, 0, 0), new SourceBlock(0, 3, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 1), new SourceBlock(0, 4, 0)),
      new SingleColourLookup(new SourceBlock(1, 0, 2), new SourceBlock(0, 5, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 6, 1)),
      new SingleColourLookup(new SourceBlock(2, 0, 0), new SourceBlock(0, 6, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 1), new SourceBlock(0, 7, 0)),
      new SingleColourLookup(new SourceBlock(2, 0, 2), new SourceBlock(0, 8, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 9, 1)),
      new SingleColourLookup(new SourceBlock(3, 0, 0), new SourceBlock(0, 9, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 1), new SourceBlock(0, 10, 0)),
      new SingleColourLookup(new SourceBlock(3, 0, 2), new SourceBlock(0, 11, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 12, 1)),
      new SingleColourLookup(new SourceBlock(4, 0, 0), new SourceBlock(0, 12, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 1), new SourceBlock(0, 13, 0)),
      new SingleColourLookup(new SourceBlock(4, 0, 2), new SourceBlock(0, 14, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 15, 1)),
      new SingleColourLookup(new SourceBlock(5, 0, 0), new SourceBlock(0, 15, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 1), new SourceBlock(0, 16, 0)),
      new SingleColourLookup(new SourceBlock(5, 0, 2), new SourceBlock(1, 15, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 17, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 0), new SourceBlock(0, 18, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 1), new SourceBlock(0, 19, 0)),
      new SingleColourLookup(new SourceBlock(6, 0, 2), new SourceBlock(3, 14, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 20, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 0), new SourceBlock(0, 21, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 1), new SourceBlock(0, 22, 0)),
      new SingleColourLookup(new SourceBlock(7, 0, 2), new SourceBlock(4, 15, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 23, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 0), new SourceBlock(0, 24, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 1), new SourceBlock(0, 25, 0)),
      new SingleColourLookup(new SourceBlock(8, 0, 2), new SourceBlock(6, 14, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 26, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 0), new SourceBlock(0, 27, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 1), new SourceBlock(0, 28, 0)),
      new SingleColourLookup(new SourceBlock(9, 0, 2), new SourceBlock(7, 15, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 29, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 0), new SourceBlock(0, 30, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 1), new SourceBlock(0, 31, 0)),
      new SingleColourLookup(new SourceBlock(10, 0, 2), new SourceBlock(9, 14, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(0, 32, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 0), new SourceBlock(0, 33, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 1), new SourceBlock(2, 30, 0)),
      new SingleColourLookup(new SourceBlock(11, 0, 2), new SourceBlock(0, 34, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(0, 35, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 0), new SourceBlock(0, 36, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 1), new SourceBlock(3, 31, 0)),
      new SingleColourLookup(new SourceBlock(12, 0, 2), new SourceBlock(0, 37, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(0, 38, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 0), new SourceBlock(0, 39, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 1), new SourceBlock(5, 30, 0)),
      new SingleColourLookup(new SourceBlock(13, 0, 2), new SourceBlock(0, 40, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(0, 41, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 0), new SourceBlock(0, 42, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 1), new SourceBlock(6, 31, 0)),
      new SingleColourLookup(new SourceBlock(14, 0, 2), new SourceBlock(0, 43, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(0, 44, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 0), new SourceBlock(0, 45, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 1), new SourceBlock(8, 30, 0)),
      new SingleColourLookup(new SourceBlock(15, 0, 2), new SourceBlock(0, 46, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 47, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(1, 46, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 0), new SourceBlock(0, 48, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 1), new SourceBlock(0, 49, 0)),
      new SingleColourLookup(new SourceBlock(16, 0, 2), new SourceBlock(0, 50, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(2, 47, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 0), new SourceBlock(0, 51, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 1), new SourceBlock(0, 52, 0)),
      new SingleColourLookup(new SourceBlock(17, 0, 2), new SourceBlock(0, 53, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(4, 46, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 0), new SourceBlock(0, 54, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 1), new SourceBlock(0, 55, 0)),
      new SingleColourLookup(new SourceBlock(18, 0, 2), new SourceBlock(0, 56, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(5, 47, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 0), new SourceBlock(0, 57, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 1), new SourceBlock(0, 58, 0)),
      new SingleColourLookup(new SourceBlock(19, 0, 2), new SourceBlock(0, 59, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(7, 46, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 0), new SourceBlock(0, 60, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 1), new SourceBlock(0, 61, 0)),
      new SingleColourLookup(new SourceBlock(20, 0, 2), new SourceBlock(0, 62, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(8, 47, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 0), new SourceBlock(0, 63, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 1), new SourceBlock(1, 62, 0)),
      new SingleColourLookup(new SourceBlock(21, 0, 2), new SourceBlock(1, 63, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(10, 46, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 0), new SourceBlock(2, 62, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 1), new SourceBlock(2, 63, 0)),
      new SingleColourLookup(new SourceBlock(22, 0, 2), new SourceBlock(3, 62, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(11, 47, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 0), new SourceBlock(3, 63, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 1), new SourceBlock(4, 62, 0)),
      new SingleColourLookup(new SourceBlock(23, 0, 2), new SourceBlock(4, 63, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(13, 46, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 0), new SourceBlock(5, 62, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 1), new SourceBlock(5, 63, 0)),
      new SingleColourLookup(new SourceBlock(24, 0, 2), new SourceBlock(6, 62, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(14, 47, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 0), new SourceBlock(6, 63, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 1), new SourceBlock(7, 62, 0)),
      new SingleColourLookup(new SourceBlock(25, 0, 2), new SourceBlock(7, 63, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(16, 45, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 0), new SourceBlock(8, 62, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 1), new SourceBlock(8, 63, 0)),
      new SingleColourLookup(new SourceBlock(26, 0, 2), new SourceBlock(9, 62, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(16, 48, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 0), new SourceBlock(9, 63, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 1), new SourceBlock(10, 62, 0)),
      new SingleColourLookup(new SourceBlock(27, 0, 2), new SourceBlock(10, 63, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(16, 51, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 0), new SourceBlock(11, 62, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 1), new SourceBlock(11, 63, 0)),
      new SingleColourLookup(new SourceBlock(28, 0, 2), new SourceBlock(12, 62, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(16, 54, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 0), new SourceBlock(12, 63, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 1), new SourceBlock(13, 62, 0)),
      new SingleColourLookup(new SourceBlock(29, 0, 2), new SourceBlock(13, 63, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(16, 57, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 0), new SourceBlock(14, 62, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 1), new SourceBlock(14, 63, 0)),
      new SingleColourLookup(new SourceBlock(30, 0, 2), new SourceBlock(15, 62, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(16, 60, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 0), new SourceBlock(15, 63, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 1), new SourceBlock(24, 46, 0)),
      new SingleColourLookup(new SourceBlock(31, 0, 2), new SourceBlock(16, 62, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(16, 63, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(17, 62, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 0), new SourceBlock(25, 47, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 1), new SourceBlock(17, 63, 0)),
      new SingleColourLookup(new SourceBlock(32, 0, 2), new SourceBlock(18, 62, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(18, 63, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 0), new SourceBlock(27, 46, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 1), new SourceBlock(19, 62, 0)),
      new SingleColourLookup(new SourceBlock(33, 0, 2), new SourceBlock(19, 63, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(20, 62, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 0), new SourceBlock(28, 47, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 1), new SourceBlock(20, 63, 0)),
      new SingleColourLookup(new SourceBlock(34, 0, 2), new SourceBlock(21, 62, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(21, 63, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 0), new SourceBlock(30, 46, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 1), new SourceBlock(22, 62, 0)),
      new SingleColourLookup(new SourceBlock(35, 0, 2), new SourceBlock(22, 63, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(23, 62, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 0), new SourceBlock(31, 47, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 1), new SourceBlock(23, 63, 0)),
      new SingleColourLookup(new SourceBlock(36, 0, 2), new SourceBlock(24, 62, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(24, 63, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 0), new SourceBlock(32, 47, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 1), new SourceBlock(25, 62, 0)),
      new SingleColourLookup(new SourceBlock(37, 0, 2), new SourceBlock(25, 63, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(26, 62, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 0), new SourceBlock(32, 50, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 1), new SourceBlock(26, 63, 0)),
      new SingleColourLookup(new SourceBlock(38, 0, 2), new SourceBlock(27, 62, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(27, 63, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 0), new SourceBlock(32, 53, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 1), new SourceBlock(28, 62, 0)),
      new SingleColourLookup(new SourceBlock(39, 0, 2), new SourceBlock(28, 63, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(29, 62, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 0), new SourceBlock(32, 56, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 1), new SourceBlock(29, 63, 0)),
      new SingleColourLookup(new SourceBlock(40, 0, 2), new SourceBlock(30, 62, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(30, 63, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 0), new SourceBlock(32, 59, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 1), new SourceBlock(31, 62, 0)),
      new SingleColourLookup(new SourceBlock(41, 0, 2), new SourceBlock(31, 63, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(32, 61, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 0), new SourceBlock(32, 62, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 1), new SourceBlock(32, 63, 0)),
      new SingleColourLookup(new SourceBlock(42, 0, 2), new SourceBlock(41, 46, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(33, 62, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 0), new SourceBlock(33, 63, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 1), new SourceBlock(34, 62, 0)),
      new SingleColourLookup(new SourceBlock(43, 0, 2), new SourceBlock(42, 47, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(34, 63, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 0), new SourceBlock(35, 62, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 1), new SourceBlock(35, 63, 0)),
      new SingleColourLookup(new SourceBlock(44, 0, 2), new SourceBlock(44, 46, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(36, 62, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 0), new SourceBlock(36, 63, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 1), new SourceBlock(37, 62, 0)),
      new SingleColourLookup(new SourceBlock(45, 0, 2), new SourceBlock(45, 47, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(37, 63, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 0), new SourceBlock(38, 62, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 1), new SourceBlock(38, 63, 0)),
      new SingleColourLookup(new SourceBlock(46, 0, 2), new SourceBlock(47, 46, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(39, 62, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 0), new SourceBlock(39, 63, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 1), new SourceBlock(40, 62, 0)),
      new SingleColourLookup(new SourceBlock(47, 0, 2), new SourceBlock(48, 46, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(40, 63, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(41, 62, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 0), new SourceBlock(41, 63, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 1), new SourceBlock(48, 49, 0)),
      new SingleColourLookup(new SourceBlock(48, 0, 2), new SourceBlock(42, 62, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(42, 63, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 0), new SourceBlock(43, 62, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 1), new SourceBlock(48, 52, 0)),
      new SingleColourLookup(new SourceBlock(49, 0, 2), new SourceBlock(43, 63, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(44, 62, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 0), new SourceBlock(44, 63, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 1), new SourceBlock(48, 55, 0)),
      new SingleColourLookup(new SourceBlock(50, 0, 2), new SourceBlock(45, 62, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(45, 63, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 0), new SourceBlock(46, 62, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 1), new SourceBlock(48, 58, 0)),
      new SingleColourLookup(new SourceBlock(51, 0, 2), new SourceBlock(46, 63, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(47, 62, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 0), new SourceBlock(47, 63, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 1), new SourceBlock(48, 61, 0)),
      new SingleColourLookup(new SourceBlock(52, 0, 2), new SourceBlock(48, 62, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(56, 47, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 0), new SourceBlock(48, 63, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 1), new SourceBlock(49, 62, 0)),
      new SingleColourLookup(new SourceBlock(53, 0, 2), new SourceBlock(49, 63, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(58, 46, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 0), new SourceBlock(50, 62, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 1), new SourceBlock(50, 63, 0)),
      new SingleColourLookup(new SourceBlock(54, 0, 2), new SourceBlock(51, 62, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(59, 47, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 0), new SourceBlock(51, 63, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 1), new SourceBlock(52, 62, 0)),
      new SingleColourLookup(new SourceBlock(55, 0, 2), new SourceBlock(52, 63, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(61, 46, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 0), new SourceBlock(53, 62, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 1), new SourceBlock(53, 63, 0)),
      new SingleColourLookup(new SourceBlock(56, 0, 2), new SourceBlock(54, 62, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(62, 47, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 0), new SourceBlock(54, 63, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 1), new SourceBlock(55, 62, 0)),
      new SingleColourLookup(new SourceBlock(57, 0, 2), new SourceBlock(55, 63, 0)),
      new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(56, 62, 1)),
      new SingleColourLookup(new SourceBlock(58, 0, 0), new SourceBlock(56, 62, 0)),
      new SingleColourLookup(new SourceBlock(58, 0, 1), new SourceBlock(56, 63, 0)),
      new SingleColourLookup(new SourceBlock(58, 0, 2), new SourceBlock(57, 62, 0)),
      new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(57, 63, 1)),
      new SingleColourLookup(new SourceBlock(59, 0, 0), new SourceBlock(57, 63, 0)),
      new SingleColourLookup(new SourceBlock(59, 0, 1), new SourceBlock(58, 62, 0)),
      new SingleColourLookup(new SourceBlock(59, 0, 2), new SourceBlock(58, 63, 0)),
      new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(59, 62, 1)),
      new SingleColourLookup(new SourceBlock(60, 0, 0), new SourceBlock(59, 62, 0)),
      new SingleColourLookup(new SourceBlock(60, 0, 1), new SourceBlock(59, 63, 0)),
      new SingleColourLookup(new SourceBlock(60, 0, 2), new SourceBlock(60, 62, 0)),
      new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(60, 63, 1)),
      new SingleColourLookup(new SourceBlock(61, 0, 0), new SourceBlock(60, 63, 0)),
      new SingleColourLookup(new SourceBlock(61, 0, 1), new SourceBlock(61, 62, 0)),
      new SingleColourLookup(new SourceBlock(61, 0, 2), new SourceBlock(61, 63, 0)),
      new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 62, 1)),
      new SingleColourLookup(new SourceBlock(62, 0, 0), new SourceBlock(62, 62, 0)),
      new SingleColourLookup(new SourceBlock(62, 0, 1), new SourceBlock(62, 63, 0)),
      new SingleColourLookup(new SourceBlock(62, 0, 2), new SourceBlock(63, 62, 0)),
      new SingleColourLookup(new SourceBlock(63, 0, 1), new SourceBlock(63, 63, 1)),
      new SingleColourLookup(new SourceBlock(63, 0, 0), new SourceBlock(63, 63, 0)),
    };
    #endregion


    //--------------------------------------------------------------
    #region singlecolourlookup.h/.cpp
    //--------------------------------------------------------------

    private class SingleColourFit
    {
      private SquishFlags _flags;
      private ColourSet _colours;
      private byte[] _colour;
      private Vector3F _start;
      private Vector3F _end;
      private byte _index;
      private int _error;
      private int _besterror;
      private SingleColourLookup[][] _lookups3;
      private SingleColourLookup[][] _lookups4;


      public SingleColourFit()
      {
        _colour = new byte[3];

        // build the table of lookups
        _lookups3 = new[]
        {
          _lookup_5_3,
          _lookup_6_3,
          _lookup_5_3
        };

        _lookups4 = new[]
        {
          _lookup_5_4,
          _lookup_6_4,
          _lookup_5_4
        };
      }


      public void Initialize(ColourSet colours, SquishFlags flags)
      {
        _colours = colours;
        _flags = flags;

        // grab the single colour
        Vector3F[] values = _colours.Points;
        _colour[0] = (byte)FloatToInt(255.0f * values[0].X, 255);
        _colour[1] = (byte)FloatToInt(255.0f * values[0].Y, 255);
        _colour[2] = (byte)FloatToInt(255.0f * values[0].Z, 255);

        // initialise the best error
        _besterror = int.MaxValue;
      }


      public unsafe void Compress(byte* block)
      {
        bool isDxt1 = ((_flags & SquishFlags.Dxt1) != 0);
        if (isDxt1)
        {
          Compress3(block);
          if (!_colours.IsTransparent)
            Compress4(block);
        }
        else
          Compress4(block);
      }


      private unsafe void Compress3(byte* block)
      {
        // find the best end-points and index
        ComputeEndPoints(_lookups3);

        // build the block if we win
        if (_error < _besterror)
        {
          // remap the indices
          byte* indices = stackalloc byte[16];
          fixed (byte* pIndex = &_index)
            _colours.RemapIndices(pIndex, indices);

          // save the block
          WriteColourBlock3(_start, _end, indices, block);

          // save the error
          _besterror = _error;
        }
      }


      private unsafe void Compress4(byte* block)
      {
        // find the best end-points and index
        ComputeEndPoints(_lookups4);

        // build the block if we win
        if (_error < _besterror)
        {
          // remap the indices
          byte* indices = stackalloc byte[16];
          fixed (byte* pIndex = &_index)
            _colours.RemapIndices(pIndex, indices);

          // save the block
          WriteColourBlock4(_start, _end, indices, block);

          // save the error
          _besterror = _error;
        }
      }


      private unsafe void ComputeEndPoints(SingleColourLookup[][] lookups)
      {
        // check each index combination (endpoint or intermediate)
        SourceBlock* sources = stackalloc SourceBlock[3];
        _error = int.MaxValue;
        for (int index = 0; index < 2; ++index)
        {
          // check the error for this codebook index
          int error = 0;
          for (int channel = 0; channel < 3; ++channel)
          {
            // grab the lookup table and index for this channel
            SingleColourLookup[] lookup = lookups[channel];
            int target = _colour[channel];

            // store a pointer to the source for this channel
            sources[channel] = (index == 0) ? lookup[target].Sources0 : lookup[target].Sources1;

            // accumulate the error
            int diff = sources[channel].Error;
            error += diff * diff;
          }

          // keep it if the error is lower
          if (error < _error)
          {
            _start = new Vector3F(
              sources[0].Start / 31.0f,
              sources[1].Start / 63.0f,
              sources[2].Start / 31.0f
              );
            _end = new Vector3F(
              sources[0].End / 31.0f,
              sources[1].End / 63.0f,
              sources[2].End / 31.0f
              );
            _index = (byte)(2 * index);
            _error = error;
          }
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region squish.h/.cpp
    //--------------------------------------------------------------

    // Provides instances to use per thread. (For thread-safety and to avoid memory allocations.)
    private class Context
    {
      public ColourSet ColourSet = new ColourSet();
      public SingleColourFit SingleColourFit = new SingleColourFit();
      public RangeFit RangeFit = new RangeFit();
      public ClusterFit ClusterFit = new ClusterFit();
    }


    private static SquishFlags FixFlags(SquishFlags flags)
    {
      // grab the flag bits
      SquishFlags method = flags & (SquishFlags.Dxt1 | SquishFlags.Dxt3 | SquishFlags.Dxt5);
      SquishFlags fit = flags & (SquishFlags.ColourIterativeClusterFit | SquishFlags.ColourClusterFit | SquishFlags.ColourRangeFit);
      SquishFlags extra = flags & SquishFlags.WeightColourByAlpha;

      // set defaults
      if (method != SquishFlags.Dxt3 && method != SquishFlags.Dxt5)
        method = SquishFlags.Dxt1;
      if (fit != SquishFlags.ColourRangeFit && fit != SquishFlags.ColourIterativeClusterFit)
        fit = SquishFlags.ColourClusterFit;

      // done
      return method | fit | extra;
    }


    /// <summary>
    /// Compresses a 4x4 block of pixels.
    /// </summary>
    /// <param name="rgba">The rgba values of the 16 source pixels.</param>
    /// <param name="mask">The valid pixel mask.</param>
    /// <param name="block">Storage for the compressed DXT block.</param>
    /// <param name="flags">Compression flags.</param>
    /// <param name="metric">An optional perceptual metric.</param>
    /// <param name="context">Context per thread.</param>
    /// <remarks>
    /// <para>
    /// The source pixels should be presented as a contiguous array of 16 rgba
    /// values, with each component as 1 byte each. In memory this should be:
    /// </para>
    /// <para>
    ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
    /// </para>
    /// <para>
    /// The mask parameter enables only certain pixels within the block. The lowest
    /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
    /// 16th bit are ignored. Pixels that are not enabled are allowed to take
    /// arbitrary colours in the output block. An example of how this can be used
    /// is in the CompressImage function to disable pixels outside the bounds of
    /// the image when the width or height is not divisible by 4.
    /// </para>
    /// <para>
    /// The flags parameter should specify either <see cref="SquishFlags.Dxt1"/>, 
    /// <see cref="SquishFlags.Dxt3"/> or <see cref="SquishFlags.Dxt5"/> compression, 
    /// however, DXT1 will be used by default if none is specified. When using DXT1 
    /// compression, 8 bytes of storage are required for the compressed DXT block. 
    /// DXT3 and DXT5 compression require 16 bytes of storage per block.
    /// </para>
    /// <para>
    /// The flags parameter can also specify a preferred colour compressor to use 
    /// when fitting the RGB components of the data. Possible colour compressors 
    /// are: <see cref="SquishFlags.ColourClusterFit"/> (the default), 
    /// <see cref="SquishFlags.ColourRangeFit"/> (very fast, low quality) or 
    /// <see cref="SquishFlags.ColourIterativeClusterFit"/> (slowest, best quality).
    /// </para>
    /// <para>
    /// When using <see cref="SquishFlags.ColourClusterFit"/> or 
    /// <see cref="SquishFlags.ColourIterativeClusterFit"/>, an additional 
    /// flag can be specified to weight the importance of each pixel by its alpha 
    /// value. For images that are rendered using alpha blending, this can 
    /// significantly increase the perceived quality.
    /// </para>
    /// <para>
    /// <paramref name="metric"/> can be used to weight the relative importance
    /// of each colour channel, or pass <see langword="null"/> to use the default
    /// uniform weight of (1.0, 1.0, 1.0). This replaces the previous flag-based
    /// control that allowed either uniform or "perceptual" weights with the fixed
    /// values (0.2126, 0.7152, 0.0722).
    /// </para>
    /// </remarks>
    private static unsafe void CompressMasked(byte* rgba, int mask, byte* block, SquishFlags flags, Vector3F? metric, Context context)
    {
      // fix any bad flags
      flags = FixFlags(flags);

      // get the block locations
      byte* colourBlock = block;
      byte* alphaBock = block;
      if ((flags & (SquishFlags.Dxt3 | SquishFlags.Dxt5)) != 0)
        colourBlock = block + 8;

      // create the minimal point set
      var colours = context.ColourSet;
      colours.Set(rgba, mask, flags);

      // check the compression type and compress colour
      if (colours.Count == 1)
      {
        // always do a single colour fit
        var fit = context.SingleColourFit;
        fit.Initialize(colours, flags);
        fit.Compress(colourBlock);
      }
      else if ((flags & SquishFlags.ColourRangeFit) != 0 || colours.Count == 0)
      {
        // do a range fit
        var fit = context.RangeFit;
        fit.Initialize(colours, flags, metric);
        fit.Compress(colourBlock);
      }
      else
      {
        // default to a cluster fit (could be iterative or not)
        var fit = context.ClusterFit;
        fit.Initialize(colours, flags, metric);
        fit.Compress(colourBlock);
      }

      // compress alpha separately if necessary
      if ((flags & SquishFlags.Dxt3) != 0)
        CompressAlphaDxt3(rgba, mask, alphaBock);
      else if ((flags & SquishFlags.Dxt5) != 0)
        CompressAlphaDxt5(rgba, mask, alphaBock);
    }


    /// <summary>
    /// Compresses a 4x4 block of pixels.
    /// </summary>
    /// <param name="block">Storage for the compressed DXT block.</param>
    /// <param name="rgba">The rgba values of the 16 source pixels.</param>
    /// <param name="flags">Compression flags.</param>
    /// <remarks>
    /// <para>
    /// The decompressed pixels will be written as a contiguous array of 16 rgba
    /// values, with each component as 1 byte each. In memory this is:
    /// </para>
    /// <para>
    /// { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
    /// </para>
    /// <para>
    /// The flags parameter should specify either kDxt1, kDxt3 or kDxt5 compression, 
    /// however, DXT1 will be used by default if none is specified. All other flags 
    /// are ignored.
    ///  </para>
    /// </remarks>
    private static unsafe void Decompress(byte* block, byte* rgba, SquishFlags flags)
    {
      // fix any bad flags
      flags = FixFlags(flags);

      // get the block locations
      byte* colourBlock = block;
      byte* alphaBock = block;
      if ((flags & (SquishFlags.Dxt3 | SquishFlags.Dxt5)) != 0)
        colourBlock = block + 8;

      // decompress colour
      DecompressColour(rgba, colourBlock, (flags & SquishFlags.Dxt1) != 0);

      // decompress alpha separately if necessary
      if ((flags & SquishFlags.Dxt3) != 0)
        DecompressAlphaDxt3(rgba, alphaBock);
      else if ((flags & SquishFlags.Dxt5) != 0)
        DecompressAlphaDxt5(rgba, alphaBock);
    }


    /// <summary>
    /// Computes the amount of compressed storage required.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="flags">Compression flags.</param>
    /// <returns>The memory (in bytes) required to store the compressed image.</returns>
    /// <remarks>
    /// <para>
    /// The flags parameter should specify either <see cref="SquishFlags.Dxt1"/>, 
    /// <see cref="SquishFlags.Dxt3"/> or <see cref="SquishFlags.Dxt5"/> compression, 
    /// however, DXT1 will be used by default if none is specified. All other flags 
    /// are ignored.
    /// </para>
    /// <para>
    /// Most DXT images will be a multiple of 4 in each dimension, but this 
    /// function supports arbitrary size images by allowing the outer blocks to
    /// be only partially used.
    /// </para>
    /// </remarks>
    public static int GetStorageRequirements(int width, int height, SquishFlags flags)
    {
      // fix any bad flags
      flags = FixFlags(flags);

      // compute the storage requirements
      int blockcount = ((width + 3) / 4) * ((height + 3) / 4);
      int blocksize = ((flags & SquishFlags.Dxt1) != 0) ? 8 : 16;
      return blockcount * blocksize;
    }


    /// <summary>
    /// Compresses an image in memory.
    /// </summary>
    /// <param name="rgba">The pixels of the source.</param>
    /// <param name="width">The width of the source image.</param>
    /// <param name="height">The height of the source image.</param>
    /// <param name="blocks">Storage for the compressed output.</param>
    /// <param name="flags">Compression flags.</param>
    /// <param name="metric">An optional perceptual metric.</param>
    /// <remarks>
    /// <para>
    /// The source pixels should be presented as a contiguous array of 16 rgba
    /// values, with each component as 1 byte each. In memory this should be:
    /// </para>
    /// <para>
    ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
    /// </para>
    /// <para>
    /// The flags parameter should specify either <see cref="SquishFlags.Dxt1"/>, 
    /// <see cref="SquishFlags.Dxt3"/> or <see cref="SquishFlags.Dxt5"/> compression, 
    /// however, DXT1 will be used by default if none is specified. When using DXT1 
    /// compression, 8 bytes of storage are required for the compressed DXT block. 
    /// DXT3 and DXT5 compression require 16 bytes of storage per block.
    /// </para>
    /// <para>
    /// The flags parameter can also specify a preferred colour compressor to use 
    /// when fitting the RGB components of the data. Possible colour compressors 
    /// are: <see cref="SquishFlags.ColourClusterFit"/> (the default), 
    /// <see cref="SquishFlags.ColourRangeFit"/> (very fast, low quality) or 
    /// <see cref="SquishFlags.ColourIterativeClusterFit"/> (slowest, best quality).
    /// </para>
    /// <para>
    /// When using <see cref="SquishFlags.ColourClusterFit"/> or 
    /// <see cref="SquishFlags.ColourIterativeClusterFit"/>, an additional 
    /// flag can be specified to weight the importance of each pixel by its alpha 
    /// value. For images that are rendered using alpha blending, this can 
    /// significantly increase the perceived quality.
    /// </para>
    /// <para>
    /// <paramref name="metric"/> can be used to weight the relative importance
    /// of each colour channel, or pass <see langword="null"/> to use the default
    /// uniform weight of (1.0, 1.0, 1.0). This replaces the previous flag-based
    /// control that allowed either uniform or "perceptual" weights with the fixed
    /// values (0.2126, 0.7152, 0.0722).
    /// </para>
    /// <para>
    /// Internally this function calls <see cref="CompressMasked"/> for each block, 
    /// which allows for pixels outside the image to take arbitrary values. The function 
    /// <see cref="GetStorageRequirements"/> can be called to compute the amount of memory
    /// to allocate for the compressed output.
    /// </para>
    /// </remarks>
    public static unsafe void CompressImage(byte[] rgba, int width, int height, byte[] blocks, SquishFlags flags, Vector3F? metric = null)
    {
      // fix any bad flags
      flags = FixFlags(flags);

      fixed (byte* rgbaFixed = rgba)
      fixed (byte* blocksFixed = blocks)
      {
        // (Note: Variables used in fixed-statement can't be used lambdas.)
        byte* pRgba = rgbaFixed;
        byte* pBlocks = blocksFixed;

        int columns = (width + 3) / 4;
        int rows = (height + 3) / 4;
        int numberOfBlocks = columns * rows;
        int bytesPerBlock = ((flags & SquishFlags.Dxt1) != 0) ? 8 : 16;

        // loop over blocks
#if SINGLE_THREADED
        var context = new Context();
        for (int i = 0; i < numberOfBlocks; i++)
        {
          int x = (i % columns) * 4;
          int y = (i / columns) * 4;
          byte* targetBlock = pBlocks + i * bytesPerBlock;
          CompressImageBlock(pRgba, x, y, width, height, targetBlock, flags, metric, context);
        }
#else
        Parallel.For(0, numberOfBlocks, () => new Context(), (i, state, context) =>
        {
          int x = (i % columns) * 4;
          int y = (i / columns) * 4;
          byte* targetBlock = pBlocks + i * bytesPerBlock;
          CompressImageBlock(pRgba, x, y, width, height, targetBlock, flags, metric, context);
          return context;
        }, context => { });
#endif
      }
    }


    private static unsafe void CompressImageBlock(byte* rgba, int x, int y, int width, int height, byte* block, SquishFlags flags, Vector3F? metric, Context context)
    {
      // build the 4x4 block of pixels
      byte* sourceRgba = stackalloc byte[16 * 4];
      byte* targetPixel = sourceRgba;
      int mask = 0;
      for (int py = 0; py < 4; ++py)
      {
        for (int px = 0; px < 4; ++px)
        {
          // get the source pixel in the image
          int sx = x + px;
          int sy = y + py;

          // enable if we're in the image
          if (sx < width && sy < height)
          {
            // copy the rgba value
            byte* sourcePixel = rgba + 4 * (width * sy + sx);
            for (int i = 0; i < 4; ++i)
              *targetPixel++ = *sourcePixel++;

            // enable this pixel
            mask |= (1 << (4 * py + px));
          }
          else
          {
            // skip this pixel as its outside the image
            targetPixel += 4;
          }
        }
      }

      // compress it into the output
      CompressMasked(sourceRgba, mask, block, flags, metric, context);
    }


    /// <summary>
    /// Decompresses an image in memory.
    /// </summary>
    /// <param name="blocks">The compressed DXT blocks.</param>
    /// <param name="width">The width of the source image.</param>
    /// <param name="height">The height of the source image.</param>
    /// <param name="rgba">Storage for the decompressed pixels.</param>
    /// <param name="flags">Compression flags.</param>
    /// <remarks>
    /// <para>
    /// The decompressed pixels will be written as a contiguous array of 
    /// <paramref name="width"/> * <paramref name="height"/> rgba values,
    /// with each component as 1 byte each. In memory this is:
    /// </para>
    /// <para>
    /// { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width * height
    /// </para>
    /// <para>
    /// The flags parameter should specify either <see cref="SquishFlags.Dxt1"/>, 
    /// <see cref="SquishFlags.Dxt3"/> or <see cref="SquishFlags.Dxt5"/> compression, 
    /// however, DXT1 will be used by default if none is specified. All other flags 
    /// are ignored.
    /// </para>
    /// </remarks>
    public static unsafe void DecompressImage(byte[] blocks, int width, int height, byte[] rgba, SquishFlags flags)
    {
      // fix any bad flags
      flags = FixFlags(flags);

      fixed (byte* blocksFixed = blocks)
      fixed (byte* rgbaFixed = rgba)
      {
        // (Note: Variables used in fixed-statement can't be used lambdas.)
        byte* pBlocks = blocksFixed;
        byte* pRgba = rgbaFixed;

        int columns = (width + 3) / 4;
        int rows = (height + 3) / 4;
        int numberOfBlocks = columns * rows;
        int bytesPerBlock = ((flags & SquishFlags.Dxt1) != 0) ? 8 : 16;

        // loop over blocks
#if SINGLE_THREADED
        for (int i = 0; i < numberOfBlocks; i++)
        {
          int x = (i % columns) * 4;
          int y = (i / columns) * 4;
          byte* sourceBlock = pBlocks + i * bytesPerBlock;
          DecompressImageBlock(sourceBlock, pRgba, x, y, width, height, flags);
        }
#else
        Parallel.For(0, numberOfBlocks, i =>
        {
          int x = (i % columns) * 4;
          int y = (i / columns) * 4;
          byte* sourceBlock = pBlocks + i * bytesPerBlock;
          DecompressImageBlock(sourceBlock, pRgba, x, y, width, height, flags);
        });
#endif
      }
    }


    private static unsafe void DecompressImageBlock(byte* block, byte* rgba, int x, int y, int width, int height, SquishFlags flags)
    {
      // decompress the block
      byte* targetRgba = stackalloc byte[4 * 16];
      Decompress(block, targetRgba, flags);

      // write the decompressed pixels to the correct image locations
      byte* sourcePixel = targetRgba;
      for (int py = 0; py < 4; ++py)
      {
        for (int px = 0; px < 4; ++px)
        {
          // get the target location
          int sx = x + px;
          int sy = y + py;
          if (sx < width && sy < height)
          {
            byte* targetPixel = rgba + 4 * (width * sy + sx);

            // copy the rgba value
            for (int i = 0; i < 4; ++i)
              *targetPixel++ = *sourcePixel++;
          }
          else
          {
            // skip this pixel as its outside the image
            sourcePixel += 4;
          }
        }
      }
    }
    #endregion
  }
}
