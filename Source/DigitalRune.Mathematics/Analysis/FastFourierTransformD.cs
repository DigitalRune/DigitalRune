// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs <i>Fast Fourier Transform</i> (FFT) (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class performs a Discrete Fourier transform (DFT) for 1D and 2D samples using Fast
  /// Fourier Transform (FFT) algorithms. Have a look at 
  /// <see href="http://paulbourke.net/miscellaneous/dft/">DFT and FFT by Paul Bourke</see> for
  /// introduction to DFT and FFT. This class uses the same notation. (Please note that the 1 / N 
  /// factor and the sign of the exponent are switched between forward and inverse FFT in some 
  /// notations.)
  /// </para>
  /// <para>
  /// 1D FFT can be performed using <see cref="Transform1D(Vector2D[],bool)"/>. This method is
  /// static and you do not need to create an instance of this class. 
  /// </para>
  /// <para>
  /// 2D FFT can be performed using <see cref="Transform2D(Vector2D[,],bool)"/>. This method is not
  /// static because it requires an internal buffer, which is allocated only once for each
  /// <see cref="FastFourierTransformD"/> instance. The size of the buffer is determine by
  /// <see cref="Capacity"/>.
  /// </para>
  /// </remarks>
  public class FastFourierTransformD
  {
    // References:
    // - http://paulbourke.net/miscellaneous/dft/
    // - Numerical Recipes in C.

    private Vector2D[] _buffer;


    /// <summary>
    /// Gets or sets the maximal capacity to reserve for internal buffers.
    /// </summary>
    /// <value>
    /// The maximal capacity to reserve for internal buffers.
    /// </value>
    /// <remarks>
    /// For 2D FFT, the capacity must be the size of largest dimension of the 2D array. 
    /// For example: To transform an array with 256x512 values, use a capacity of 512.
    /// </remarks>
    public int Capacity
    {
      get { return _buffer.Length; }
      set
      {
        if (_buffer.Length == value)
          return;

        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "Capacity must be greater than 0.");

        _buffer = new Vector2D[value];
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FastFourierTransformF"/> class.
    /// </summary>
    /// <param name="capacity">
    /// The max. capacity to reserve for internal buffers. For 2D FFT, this must be the size of
    /// largest dimension of the 2D array. For example: To transform an array with 256x512 values,
    /// use a capacity of 512.
    /// </param>
    public FastFourierTransformD(int capacity)
    {
      if (capacity <= 0)
        throw new ArgumentOutOfRangeException("capacity", "Capacity must be greater than 0.");
      
      _buffer = new Vector2D[capacity];
    }


    /// <overloads>
    /// <summary>
    /// Performs a Fast Fourier Transform in 1 dimension.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs a Fast Fourier Transform in 1 dimension using the Radix-2 Cooley-Tukey algorithm.
    /// </summary>
    /// <param name="values">
    /// The values which are replaced in-place by the FFT result. Each element represents a complex
    /// number with the real part in x and the imaginary part in y. The number of values must be a
    /// power of two (e.g. 2, 4, 8, 16, ...).
    /// </param>
    /// <param name="forward">
    /// <see langword="true"/> to perform forward FFT, <see langword="false"/> to perform inverse
    /// FFT.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The length of <paramref name="values"/> is not a power of two.
    /// </exception>
    public static void Transform1D(Vector2D[] values, bool forward)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      Transform1D(values, values.Length, forward);
    }


    /// <summary>
    /// Performs a Fast Fourier Transform using the Radix-2 Cooley-Tukey algorithm.
    /// </summary>
    /// <param name="values">
    /// The values which are replaced in-place by the FFT result. Each element represents a complex
    /// number with the real part in x and the imaginary part in y. The number of values must be a
    /// power of two (e.g. 2, 4, 8, 16, ...).
    /// </param>
    /// <param name="numberOfValues">
    /// The number of values. The array <paramref name="values"/> can be longer. Only the elements
    /// from 0 to <paramref name="numberOfValues"/> - 1 are transformed.
    /// </param>
    /// <param name="forward">
    /// <see langword="true"/> to perform forward FFT, <see langword="false"/> to perform inverse
    /// FFT.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="numberOfValues"/> is not a power of two.
    /// </exception>
    public static void Transform1D(Vector2D[] values, int numberOfValues, bool forward)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      // Values must contain n = 2^m elements.
      int n = Math.Min(numberOfValues, values.Length);
      int m = (int)MathHelper.Log2GreaterOrEqual((uint)n);

      if ((1 << m) > n)
        throw new ArgumentException("The number of values must be a power of two (e.g. 2, 4, 8, ...).");

      Transform1D(values, n, m, forward);
    }


    private static void Transform1D(Vector2D[] values, int n, int m, bool forward)
    {
      int i, i1, j, k, i2, l, l1, l2;
      double c1, c2, tx, ty, t1, t2, u1, u2, z;

      // Bit reversal
      i2 = n >> 1;
      j = 0;
      for (i = 0; i < n - 1; i++)
      {
        if (i < j)
        {
          // Swap i and j.
          tx = values[i].X;
          ty = values[i].Y;
          values[i].X = values[j].X;
          values[i].Y = values[j].Y;
          values[j].X = tx;
          values[j].Y = ty;
        }
        k = i2;
        while (k <= j)
        {
          j -= k;
          k >>= 1;
        }
        j += k;
      }

      // Compute the FFT.
      c1 = -1.0;
      c2 = 0.0;
      l2 = 1;
      for (l = 0; l < m; l++)
      {
        l1 = l2;
        l2 <<= 1;
        u1 = 1.0;
        u2 = 0.0;
        for (j = 0; j < l1; j++)
        {
          for (i = j; i < n; i += l2)
          {
            i1 = i + l1;
            t1 = u1 * values[i1].X - u2 * values[i1].Y;
            t2 = u1 * values[i1].Y + u2 * values[i1].X;
            values[i1].X = values[i].X - t1;
            values[i1].Y = values[i].Y - t2;
            values[i].X += t1;
            values[i].Y += t2;
          }

          z = u1 * c1 - u2 * c2;
          u2 = u1 * c2 + u2 * c1;
          u1 = z;
        }

        c2 = Math.Sqrt((1.0 - c1) / 2.0);

        if (forward)
          c2 = -c2;

        c1 = Math.Sqrt((1.0 + c1) / 2.0);
      }

      // Scaling for forward transform.
      if (forward)
      {
        for (i = 0; i < n; i++)
        {
          double f = 1.0 / n;
          values[i].X *= f;
          values[i].Y *= f;
        }
      }
    }


    /// <summary>
    /// Performs a Fast Fourier Transform in 2 dimensions.
    /// </summary>
    /// <param name="values">
    /// The values which are replaced in-place by the FFT result. Each element represents a complex
    /// number with the real part in x and the imaginary part in y. The number of values in each
    /// dimension must be a power of two (e.g. 2, 4, 8, 16, ...).
    /// </param>
    /// <param name="forward">
    /// <see langword="true"/> to perform forward FFT, <see langword="false"/> to perform inverse
    /// FFT.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The width or height of the array exceeds the internal buffer or is not a power of two.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public void Transform2D(Vector2D[,] values, bool forward)
    {
      if (values == null)
        throw new ArgumentNullException("values");

      int nx = values.GetLength(0);
      int ny = values.GetLength(1);

      if (nx > _buffer.Length || ny > _buffer.Length)
        throw new ArgumentException("The values array is too large for the current Capacity.");

      // Values must contain n = 2^m elements.
      int mx = (int)MathHelper.Log2GreaterOrEqual((uint)nx);
      int my = (int)MathHelper.Log2GreaterOrEqual((uint)ny);

      if ((1 << mx) > nx || (1 << my) > ny)
        throw new ArgumentException("The number of values must be a power of two (e.g. 2, 4, 8, ...) in each dimension.");

      // Transform the rows.
      for (int j = 0; j < ny; j++)
      {
        for (int i = 0; i < nx; i++)
        {
          _buffer[i].X = values[i, j].X;
          _buffer[i].Y = values[i, j].Y;
        }

        Transform1D(_buffer, nx, mx, forward);

        for (int i = 0; i < nx; i++)
        {
          values[i, j].X = _buffer[i].X;
          values[i, j].Y = _buffer[i].Y;
        }
      }

      // Transform the columns.
      for (int i = 0; i < nx; i++)
      {
        for (int j = 0; j < ny; j++)
        {
          _buffer[j].X = values[i, j].X;
          _buffer[j].Y = values[i, j].Y;
        }

        Transform1D(_buffer, ny, my, forward);

        for (int j = 0; j < ny; j++)
        {
          values[i, j].X = _buffer[j].X;
          values[i, j].Y = _buffer[j].Y;
        }
      }
    }
  }
}
