// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
   This file is a port of the image resizing functions in NVIDIA Texture Tools
   (NVTT, source http://code.google.com/p/nvidia-texture-tools/).
   
   Original source code: "This code is in the public domain -- castanyo@yahoo.es"
   
   References:
   - nvidia-texture-tools\src\nvimage\Filter.cpp
   - Jonathan Blow: "Mipmapping, Part 1", The Inner Product, December 2001,
     http://number-none.com/product/Mipmapping,%20Part%201/index.html
   - Jonathan Blow: "Mipmapping, Part 2", The Inner Product, Januar 2002,
     http://number-none.com/product/Mipmapping,%20Part%202/index.html
   - Dale A. Schumacher: "General Filtered Image Rescaling", Graphics Gems III,
     http://tog.acm.org/GraphicsGems/gemsiii/filter.c
   - A.V. Oppenheim, R.W. Schafer: "Digital Signal Processing", Prentice-Hall, 1975
   - R.W. Hamming: "Digital Filters", Prentice-Hall, Englewood Cliffs, NJ, 1983
   - W.K. Pratt, "Digital Image Processing", John Wiley and Sons, 1978
   - H.S. Hou, H.C. Andrews, "Cubic Splines for Image Interpolation and Digital Filtering",
     IEEE Trans. Acoustics, Speech, and Signal Proc., vol. ASSP-26, no. 6, Dec. 1978, pp. 508-517
   - Paul Heckbert's zoom library, http://www.xmission.com/~legalize/zoom.html
   - "Reconstruction Filters in Computer Graphics", SIGGRAPH 88,
     http://www.mentallandscape.com/Papers_siggraph88.pdf
   - http://www.dspguide.com/ch16.htm
   - Stephen Guthe, Paul Heckbert: "Non-Power-of-Two Mipmapping", NVIDIA,
     https://developer.nvidia.com/content/non-power-two-mipmapping
*/
#endregion

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Defines a filter for image resizing.
  /// </summary>
  public enum ResizeFilter
  {
    /// <summary>Box filter.</summary>
    Box,
    /// <summary>Triangle (bilinear/tent) filter.</summary>
    Triangle,
    /// <summary>Cubic filter.</summary>
    Cubic,
    /// <summary>Quadric (bell) filter.</summary>
    Quadric,
    /// <summary>Cubic b-spline filter.</summary>
    BSpline,
    /// <summary>Mitchell &amp; Netravali's two-param cubic filter. (Source: "Reconstruction Filters in Computer Graphics", SIGGRAPH 88)</summary>
    Mitchell,
    /// <summary>Lanczos3 filter.</summary>
    Lanczos,
    /// <summary>Sinc filter.</summary>
    Sinc,
    /// <summary>Kaiser filter.</summary>
    Kaiser,
  }


  partial class TextureHelper
  {
    //--------------------------------------------------------------
    #region Filter Functions
    //--------------------------------------------------------------

    /// <summary>
    /// Defines a filter function f(x).
    /// </summary>
    private abstract class Filter
    {
      /// <summary>
      /// Gets or sets the width of the filter.
      /// </summary>
      /// <value>
      /// The width of the filter. The width is relative to the output image. The total filter range
      /// is [-Width, +Width].
      /// </value>
      public float Width { get; private set; }


      /// <summary>
      /// Initializes a new instance of the <see cref="Filter"/> class.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      protected Filter(float width)
      {
        Width = width;
      }


      /// <summary>
      /// Samples the function using a box filter.
      /// </summary>
      /// <param name="x">The start position of the kernel sample.</param>
      /// <param name="scale">The scale factor that is applied to the filter kernel.</param>
      /// <param name="numberOfSamples">The number of samples.</param>
      /// <returns>The filter weight for sample <paramref name="x"/>.</returns>
      public float SampleBox(float x, float scale, int numberOfSamples)
      {
        // Example: x = 1, scale = 1
        //
        // 1 |     +-----+
        //   |     |     |
        //   |     |     |
        //   +-----+-----+-----+--
        //   0     1     2     3 

        double sum = 0;
        float oneOverN = 1.0f / numberOfSamples;
        for (int sample = 0; sample < numberOfSamples; sample++)
        {
          float offset = (sample + 0.5f) * oneOverN;
          float p = (x + offset) * scale;
          sum += Evaluate(p);
        }

        return (float)(sum * oneOverN);
      }


      /// <summary>
      /// Samples the function using a triangle filter.
      /// </summary>
      /// <param name="x">The start position of the kernel sample.</param>
      /// <param name="scale">The scale factor that is applied to the filter kernel.</param>
      /// <param name="numberOfSamples">The number of samples.</param>
      /// <returns>The filter weight for sample <paramref name="x"/>.</returns>
      public float SampleTriangle(float x, float scale, int numberOfSamples)
      {
        // Example: x = 1, scale = 1
        //
        // 1 |      ^
        //   |     / \
        //   |    /   \
        //   |   /     \
        //   +--/-+---+-\--+---
        //   0    1   2    3 

        double sum = 0;
        float oneOverN = 1.0f / numberOfSamples;
        for (int sample = 0; sample < numberOfSamples; sample++)
        {
          float offset = (2 * sample + 1.0f) * oneOverN;
          float p = (x + offset - 0.5f) * scale;
          float value = Evaluate(p);

          float weight = (offset > 1.0f) ? 2.0f - offset : offset;
          sum += value * weight;
        }

        return (float)(2 * sum * oneOverN);
      }


      /// <summary>
      /// Evaluates the function f(x).
      /// </summary>
      /// <param name="x">The function argument x.</param>
      /// <returns>The function value f(x).</returns>
      protected abstract float Evaluate(float x);
    }


    /// <summary>
    /// Defines a box filer.
    /// </summary>
    private class BoxFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="BoxFilter"/> class with a default width of 0.5.
      /// </summary>
      public BoxFilter()
        : base(0.5f)
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="BoxFilter"/> class with the specified width.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      public BoxFilter(float width)
        : base(width)
      {
      }

      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        return Math.Abs(x) <= Width ? 1.0f : 0.0f;
      }
    }


    /// <summary>
    /// Defines a triangle filter.
    /// </summary>
    private class TriangleFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="TriangleFilter"/> class with a default width of 1.0.
      /// </summary>
      public TriangleFilter()
        : base(1.0f)
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="TriangleFilter"/> class with the specified width.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      public TriangleFilter(float width)
        : base(width)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        x = Math.Abs(x);
        return x < Width ? Width - x : 0.0f;
      }
    }


    /// <summary>
    /// Defines a cubic filter.
    /// </summary>
    private class CubicFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="CubicFilter"/> class.
      /// </summary>
      public CubicFilter()
        : base(1.0f)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        // Cubic filter from Thatcher Ulrich:
        // f(t) = 2|t|³ - 3|t|² + 1, -1 <= t <= 1

        x = Math.Abs(x);
        return x < 1.0f ? (2.0f * x - 3.0f) * x * x + 1.0f : 0.0f;
      }
    }


    /// <summary>
    /// Defines a quadric filter.
    /// </summary>
    private class QuadricFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="QuadricFilter"/> class.
      /// </summary>
      public QuadricFilter()
        : base(1.5f)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        x = Math.Abs(x);
        if (x < 0.5f)
          return 0.75f - x * x;

        if (x < 1.5f)
        {
          float t = x - 1.5f;
          return 0.5f * t * t;
        }

        return 0.0f;
      }
    }


    /// <summary>
    /// Defines a B-spline filter.
    /// </summary>
    private class BSplineFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="BSplineFilter"/> class.
      /// </summary>
      public BSplineFilter()
        : base(2.0f)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        // Cubic b-spline filter from Paul Heckbert.
        x = Math.Abs(x);
        if (x < 1.0f)
          return (4.0f + x * x * (-6.0f + x * 3.0f)) / 6.0f;

        if (x < 2.0f)
        {
          float t = 2.0f - x;
          return t * t * t / 6.0f;
        }

        return 0.0f;
      }
    }


    /// <summary>
    /// Defines Mitchell &amp; Netravali's filter.
    /// </summary>
    private class MitchellFilter : Filter
    {
      float _p0, _p2, _p3;
      float _q0, _q1, _q2, _q3;


      /// <summary>
      /// Initializes a new instance of the <see cref="MitchellFilter"/> class.
      /// </summary>
      public MitchellFilter()
        : base(2.0f)
      {
        SetParameters(1.0f / 3.0f, 1.0f / 3.0f);
      }


      /// <summary>
      /// Sets the cubic filter parameters.
      /// </summary>
      /// <param name="b">The parameter B.</param>
      /// <param name="c">The parameter C.</param>
      /// <remarks>
      /// <para>
      /// Some values (B, C) correspond to well-known cubic splines. (1, 0) is the cubic B-spline.
      /// (0, C) is the family of cardinal cubics. (0, 0.5) is the Catmull-Rom spline. (B, 0) are
      /// Duff's tensioned B-splines.
      /// </para>
      /// <para>
      /// The default values for Mitchell &amp; Netravali's filter are (1/3, 1/3).
      /// </para>
      /// </remarks>
      public void SetParameters(float b, float c)
      {
        _p0 = (6.0f - 2.0f * b) / 6.0f;
        _p2 = (-18.0f + 12.0f * b + 6.0f * c) / 6.0f;
        _p3 = (12.0f - 9.0f * b - 6.0f * c) / 6.0f;
        _q0 = (8.0f * b + 24.0f * c) / 6.0f;
        _q1 = (-12.0f * b - 48.0f * c) / 6.0f;
        _q2 = (6.0f * b + 30.0f * c) / 6.0f;
        _q3 = (-b - 6.0f * c) / 6.0f;
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        x = Math.Abs(x);
        if (x < 1.0f)
          return _p0 + x * x * (_p2 + x * _p3);

        if (x < 2.0f)
          return _q0 + x * (_q1 + x * (_q2 + x * _q3));

        return 0.0f;
      }
    }

    // Sinc function.
    private static float Sinc(float x)
    {
      const float epsilon = 0.0001f;
      if (Math.Abs(x) < epsilon)
        return 1.0f + x * x * (-1.0f / 6.0f + x * x * 1.0f / 120.0f);

      return (float)(Math.Sin(x) / x);
    }


    /// <summary>
    /// Defines the Lanczos filter.
    /// </summary>
    private class LanczosFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="LanczosFilter"/> class.
      /// </summary>
      public LanczosFilter()
        : base(3.0f)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        x = Math.Abs(x);
        return x < 3.0f ? Sinc(ConstantsF.Pi * x) * Sinc(ConstantsF.Pi * x / 3.0f) : 0.0f;
      }
    }


    /// <summary>
    /// Defines the Sinc filter.
    /// </summary>
    private class SincFilter : Filter
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="SincFilter"/> class with a default width of 3.0.
      /// </summary>
      public SincFilter()
        : this(3.0f)
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="SincFilter"/> class with the specified width.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      public SincFilter(float width)
        : base(width)
      {
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        return Sinc(ConstantsF.Pi * x);
      }
    }


    // Bessel function of the first kind from Jon Blow's article.
    // http://mathworld.wolfram.com/BesselFunctionoftheFirstKind.html
    // http://en.wikipedia.org/wiki/Bessel_function
    private static float Bessel0(float x)
    {
      const float epsilon = 1e-6f;
      float xh, sum, pow, ds;
      int k;

      xh = 0.5f * x;
      sum = 1.0f;
      pow = 1.0f;
      k = 0;
      ds = 1.0f;
      while (ds > sum * epsilon)
      {
        ++k;
        pow = pow * (xh / k);
        ds = pow * pow;
        sum = sum + ds;
      }

      return sum;
    }


    /*// Alternative Bessel function from Paul Heckbert.
    static float _Bessel0(float x)
    {
        const float epsilon = 1e-6f;
        float sum = 1.0f;
        float y = x * x / 4.0f;
        float t = y;
        for(int i = 2; t > epsilon; i++) 
        {
            sum += t;
            t *= y / (i * i);
        }

        return sum;
    }//*/


    /// <summary>
    /// Defines the Kaiser filter.
    /// </summary>
    private class KaiserFilter : Filter
    {
      private float _alpha;
      private float _stretch;


      /// <summary>
      /// Initializes a new instance of the <see cref="KaiserFilter"/> class with a default width of 3.0.
      /// </summary>
      public KaiserFilter()
        : this(3.0f)
      {
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="KaiserFilter"/> class with the specified width.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      public KaiserFilter(float width)
        : base(width)
      {
        SetParameters(4.0f, 1.0f);
      }


      /// <summary>
      /// Sets the parameters of the Kaiser filter.
      /// </summary>
      /// <param name="alpha">The alpha value.</param>
      /// <param name="stretch">The stretch value.</param>
      public void SetParameters(float alpha, float stretch)
      {
        _alpha = alpha;
        _stretch = stretch;
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        float sinc_value = Sinc(ConstantsF.Pi * x * _stretch);
        float t = x / Width;
        if ((1 - t * t) >= 0)
          return sinc_value * Bessel0(_alpha * (float)Math.Sqrt(1 - t * t)) / Bessel0(_alpha);

        return 0;
      }
    }


    /// <summary>
    /// Defines a Gaussian filter.
    /// </summary>
    private class GaussianFilter : Filter
    {
      private float _variance;


      /// <summary>
      /// Initializes a new instance of the <see cref="GaussianFilter"/> class.
      /// </summary>
      /// <param name="width">The width of the filter.</param>
      public GaussianFilter(float width)
        : base(width)
      {
        SetParameters(1.0f);
      }


      /// <summary>
      /// Sets the parameters of the Gaussian filter.
      /// </summary>
      /// <param name="variance">The variance.</param>
      private void SetParameters(float variance)
      {
        _variance = variance;
      }


      /// <inheritdoc/>
      protected override float Evaluate(float x)
      {
        return 1.0f / (float)Math.Sqrt(2 * ConstantsF.Pi * _variance) * (float)Math.Exp(-x * x / (2 * _variance));
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Convolution
    //--------------------------------------------------------------

    /// <summary>
    /// Applies the a filter kernel to the specified image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="kernel">The filter kernel. (Needs to be square.)</param>
    /// <param name="wrapMode">The texture address mode.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> or <paramref name="kernel"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="kernel"/> is non-square.
    /// </exception>
    public static void Convolve(Image image, float[,] kernel, TextureAddressMode wrapMode)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (kernel == null)
        throw new ArgumentNullException("kernel");
      if (kernel.GetLength(0) != kernel.GetLength(1))
        throw new ArgumentException("Filter kernel needs to be square.", "kernel");

      int width = image.Width;
      int height = image.Height;
      int kernelSize = kernel.GetLength(0);
      int kernelOffset = kernelSize / 2;

      var tmpImage = new Image(width, height, image.Format);
      Buffer.BlockCopy(image.Data, 0, tmpImage.Data, 0, image.Data.Length);

      // ReSharper disable AccessToDisposedClosure
      using (var tempImage4F = new ImageAccessor(tmpImage))
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < height; y++)
#else
        Parallel.For(0, height, y =>
#endif
        {
          for (int x = 0; x < width; x++)
          {
            // Apply 2D kernel at (x, y).
            Vector4F color = new Vector4F();
            for (int row = 0; row < kernelSize; row++)
            {
              int srcY = y + row - kernelOffset;
              for (int column = 0; column < kernelSize; column++)
              {
                int srcX = x + column - kernelOffset;
                color += kernel[row, column] * tempImage4F.GetPixel(srcX, srcY, wrapMode);
              }
            }

            image4F.SetPixel(x, y, color);
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }
    #endregion


    //--------------------------------------------------------------
    #region Resizing
    //--------------------------------------------------------------

    /// <summary>
    /// Defines a 1D polyphase kernel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A polyphase kernel uses different filter weights for each pixel of the output image.
    /// </para>
    /// <para>
    /// See <see href="https://developer.nvidia.com/content/non-power-two-mipmapping">Stephen Guthe,
    /// Paul Heckbert: "Non-Power-of-Two Mipmapping", NVIDIA</see>.
    /// </para>
    /// </remarks>
    private class PolyphaseKernel
    {
      // Notes: 
      // Weights = float[Length, WindowSize]
      // Width is from filter center to border. So the Window contains 2 * Width.


      /// <summary>
      /// Gets the filter weights.
      /// </summary>
      /// <value>The filter weights.</value>
      /// <remarks>
      /// <c>Weights[i, ]</c> provides the filter weights for pixel <c>i</c> in the output image.
      /// </remarks>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
      public float[,] Weights { get; private set; }


      /// <summary>
      /// Gets the window size.
      /// </summary>
      /// <value>The window size.</value>
      public int WindowSize
      {
        get { return Weights.GetLength(1); }
      }


      /// <summary>
      /// Gets the length of the output image.
      /// </summary>
      /// <value>The length of the output image.</value>
      public int Length
      {
        get { return Weights.GetLength(0); }
      }


      /// <summary>
      /// Gets the filter width in the input image.
      /// </summary>
      /// <value>The filter width in the input image.</value>
      public float Width { get; private set; }


      /// <summary>
      /// Initializes a new instance of the <see cref="PolyphaseKernel"/> class.
      /// </summary>
      /// <param name="filter">The filter function.</param>
      /// <param name="srcLength">The length of the input image.</param>
      /// <param name="dstLength">The length of the output image.</param>
      /// <param name="numberOfSamples">The number of samples.</param>
      /// <exception cref="ArgumentNullException">
      /// <paramref name="filter"/> is <see langword="null"/>.
      /// </exception>
      /// <exception cref="ArgumentOutOfRangeException">
      /// <paramref name="srcLength"/>, <paramref name="dstLength"/>, or
      /// <paramref name="numberOfSamples"/> is invalid.
      /// </exception>
      public PolyphaseKernel(Filter filter, int srcLength, int dstLength, int numberOfSamples = 32)
      {
        if (filter == null)
          throw new ArgumentNullException("filter");
        if (srcLength <= 0)
          throw new ArgumentOutOfRangeException("srcLength");
        if (dstLength <= 0)
          throw new ArgumentOutOfRangeException("dstLength");
        if (numberOfSamples <= 0)
          throw new ArgumentOutOfRangeException("numberOfSamples");

        // scale < 1 ... Downsampling
        // scale > 1 ... Upsampling
        float scale = (float)dstLength / srcLength;
        float inverseScale = 1.0f / scale;

        if (scale > 1)
        {
          // Upsampling
          numberOfSamples = 1;
          scale = 1;
        }

        // filter.Width ... filter width in output image.
        // Width ... filter width in input image.
        Width = filter.Width * inverseScale;
        int windowSize = (int)Math.Ceiling(Width * 2) + 1;

        // Each pixel in the output image uses its own set of filter weights.
        Weights = new float[dstLength, windowSize];
        for (int i = 0; i < dstLength; i++)
        {
          // i ... pixel index in output image.
          // i + 0.5 ... kernel center in output image.
          // (i + 0.5f) * inverseScale ... kernel center in input image.
          float center = (i + 0.5f) * inverseScale;

          // Kernel range in input image.
          int left = (int)Math.Floor(center - Width);
          int right = (int)Math.Ceiling(center + Width);
          Debug.Assert(right - left <= windowSize);

          // Calculate filter weights for pixel i in output image.
          float total = 0.0f;
          for (int j = 0; j < windowSize; j++)
          {
            float sample = filter.SampleBox(left + j - center, scale, numberOfSamples);
            Weights[i, j] = sample;
            total += sample;
          }

          // Normalize weights.
          float inverseTotal = 1.0f / total;
          for (int j = 0; j < windowSize; j++)
            Weights[i, j] *= inverseTotal;
        }
      }
    }


    /// <summary>
    /// Create a filter with default settings.
    /// </summary>
    /// <param name="filter">The filter type.</param>
    /// <returns>The <see cref="Filter"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="filter"/> is invalid.
    /// </exception>
    private static Filter GetFilter(ResizeFilter filter)
    {
      switch (filter)
      {
        case ResizeFilter.Box:
          return new BoxFilter();
        case ResizeFilter.Triangle:
          return new TriangleFilter();
        case ResizeFilter.Cubic:
          return new CubicFilter();
        case ResizeFilter.Quadric:
          return new QuadricFilter();
        case ResizeFilter.BSpline:
          return new BSplineFilter();
        case ResizeFilter.Mitchell:
          return new MitchellFilter();
        case ResizeFilter.Lanczos:
          return new LanczosFilter();
        case ResizeFilter.Sinc:
          return new SincFilter();
        case ResizeFilter.Kaiser:
          return new KaiserFilter();
        default:
          throw new ArgumentException("Invalid resize filter.", "filter");
      }
    }


    /// <overloads>
    /// <summary>
    /// Resizes a texture/image.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Resizes a 2D texture or 3D (volume) texture.
    /// </summary>
    /// <param name="srcTexture">The input texture.</param>
    /// <param name="srcMipIndex">The mipmap level of the input image.</param>
    /// <param name="srcArrayOrFaceIndex">
    /// The array index (or the face index for cube maps) of the input image. Must be 0 for volume
    /// textures.
    /// </param>
    /// <param name="dstTexture">The output texture.</param>
    /// <param name="dstMipIndex">The mipmap level of the output image.</param>
    /// <param name="dstArrayOrFaceIndex">
    /// The array index (or the face index for cube maps) of the output image. Must be 0 for volume
    /// textures.
    /// </param>
    /// <param name="filter">The filter to use for resizing.</param>
    /// <param name="alphaTransparency">
    /// <see langword="true"/> if the image contains uses non-premultiplied alpha; otherwise,
    /// <see langword="false"/> if the image uses premultiplied alpha or has no alpha.
    /// </param>
    /// <param name="wrapMode">
    /// The texture address mode that will be used for sampling the at runtime.
    /// </param>
    public static void Resize(Texture srcTexture, int srcMipIndex, int srcArrayOrFaceIndex, Texture dstTexture, int dstMipIndex, int dstArrayOrFaceIndex, ResizeFilter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      Resize(srcTexture, srcMipIndex, srcArrayOrFaceIndex, dstTexture, dstMipIndex, dstArrayOrFaceIndex, GetFilter(filter), alphaTransparency, wrapMode);
    }


    /// <summary>
    /// Resizes a 2D image.
    /// </summary>
    /// <param name="srcImage">The input image.</param>
    /// <param name="dstImage">The output image.</param>
    /// <param name="filter">The filter to use for resizing.</param>
    /// <param name="alphaTransparency">
    /// <see langword="true"/> if the image contains uses non-premultiplied alpha; otherwise,
    /// <see langword="false"/> if the image uses premultiplied alpha or has no alpha.
    /// </param>
    /// <param name="wrapMode">
    /// The texture address mode that will be used for sampling the at runtime.
    /// </param>
    public static void Resize(Image srcImage, Image dstImage, ResizeFilter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      Resize(srcImage, dstImage, GetFilter(filter), alphaTransparency, wrapMode);
    }


    private static void Resize(Texture srcTexture, int srcMipIndex, int srcArrayOrFaceIndex, Texture dstTexture, int dstMipIndex, int dstArrayOrFaceIndex, Filter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      if (srcTexture == null)
        throw new ArgumentNullException("srcTexture");
      if (dstTexture == null)
        throw new ArgumentNullException("dstTexture");

      if (srcTexture == dstTexture && srcMipIndex == dstMipIndex)
        return;

      int srcDepth = srcTexture.GetDepth(srcMipIndex);
      int dstDepth = dstTexture.GetDepth(dstMipIndex);
      if (srcDepth == dstDepth)
      {
        // Resize 2D.
        int srcIndex = srcTexture.GetImageIndex(srcMipIndex, srcArrayOrFaceIndex, 0);
        int srcWidth = srcTexture.Images[srcIndex].Width;
        int srcHeight = srcTexture.Images[srcIndex].Height;

        int dstIndex = dstTexture.GetImageIndex(dstMipIndex, dstArrayOrFaceIndex, 0);
        int dstWidth = dstTexture.Images[dstIndex].Width;
        int dstHeight = dstTexture.Images[dstIndex].Height;

        var kernelX = new PolyphaseKernel(filter, srcWidth, dstWidth, 32);
        var kernelY = new PolyphaseKernel(filter, srcHeight, dstHeight, 32);

#if SINGLE_THREADED
        for (int z = 0; z < srcDepth; z++)
#else
        Parallel.For(0, srcDepth, z =>
#endif
        {
          var srcImage = srcTexture.Images[srcTexture.GetImageIndex(srcMipIndex, srcArrayOrFaceIndex, z)];
          var dstImage = dstTexture.Images[dstTexture.GetImageIndex(dstMipIndex, dstArrayOrFaceIndex, z)];
          Resize2D(srcImage, dstImage, alphaTransparency, wrapMode, kernelX, kernelY);
        }
#if !SINGLE_THREADED
        );
#endif
      }
      else
      {
        // Resize 3D.
        Resize3D(srcTexture, srcMipIndex, srcArrayOrFaceIndex, dstTexture, dstMipIndex, dstArrayOrFaceIndex, filter, alphaTransparency, wrapMode);
      }
    }


    private static void Resize(Image srcImage, Image dstImage, Filter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      if (srcImage == null)
        throw new ArgumentNullException("srcImage");
      if (dstImage == null)
        throw new ArgumentNullException("dstImage");
      if (filter == null)
        throw new ArgumentNullException("filter");

      var kernelX = new PolyphaseKernel(filter, srcImage.Width, dstImage.Width, 32);
      var kernelY = new PolyphaseKernel(filter, srcImage.Height, dstImage.Height, 32);

      Resize2D(srcImage, dstImage, alphaTransparency, wrapMode, kernelX, kernelY);
    }


    private static void Resize2D(Image srcImage, Image dstImage, bool alphaTransparency, TextureAddressMode wrapMode, PolyphaseKernel kernelX, PolyphaseKernel kernelY)
    {
      var tmpImage = new Image(dstImage.Width, srcImage.Height, srcImage.Format);

      // ReSharper disable AccessToDisposedClosure
      using (var srcImage4F = new ImageAccessor(srcImage))
      using (var tmpImage4F = new ImageAccessor(tmpImage))
      using (var dstImage4F = new ImageAccessor(dstImage))
      {
        // Resize horizontally: srcImage --> tmpImage
        {
          float scale = (float)tmpImage4F.Width / srcImage4F.Width;
          float inverseScale = 1.0f / scale;

#if SINGLE_THREADED
          for (int y = 0; y < tmpImage4F.Height; y++)
#else
          Parallel.For(0, tmpImage4F.Height, y =>
#endif
          {
            // Apply polyphase kernel horizontally.
            for (int x = 0; x < tmpImage4F.Width; x++)
            {
              float center = (x + 0.5f) * inverseScale;

              int left = (int)Math.Floor(center - kernelX.Width);
              int right = (int)Math.Ceiling(center + kernelX.Width);
              Debug.Assert(right - left <= kernelX.WindowSize);

              float totalRgbWeights = 0.0f;
              Vector4F sum = new Vector4F();
              for (int i = 0; i < kernelX.WindowSize; i++)
              {
                Vector4F color = srcImage4F.GetPixel(left + i, y, wrapMode);

                //if (Numeric.IsNaN(color.X) || Numeric.IsNaN(color.Y) || Numeric.IsNaN(color.Z) || Numeric.IsNaN(color.W)
                //   || color.X < 0 || color.Y < 0 || color.Z < 0 || color.W < 0
                //   || color.X > 1 || color.Y > 1 || color.Z > 1 || color.W > 1)
                //  Debugger.Break();

                const float alphaEpsilon = 1.0f / 256.0f;
                float alpha = alphaTransparency ? color.W + alphaEpsilon : 1.0f;

                float weight = kernelX.Weights[x, i];
                float rgbWeight = weight * alpha;
                totalRgbWeights += rgbWeight;

                sum.X += color.X * rgbWeight;
                sum.Y += color.Y * rgbWeight;
                sum.Z += color.Z * rgbWeight;
                sum.W += color.W * weight;

                //if (Numeric.IsNaN(sum.X) || Numeric.IsNaN(sum.Y) || Numeric.IsNaN(sum.Z) || Numeric.IsNaN(sum.W)
                //   || sum.X < 0 || sum.Y < 0 || sum.Z < 0 || sum.W < 0
                //   || sum.X > 1 || sum.Y > 1 || sum.Z > 1 || sum.W > 1)
                //  Debugger.Break();
              }

              float f = 1 / totalRgbWeights;
              sum.X *= f;
              sum.Y *= f;
              sum.Z *= f;

              //if (Numeric.IsNaN(sum.X) || Numeric.IsNaN(sum.Y) || Numeric.IsNaN(sum.Z) || Numeric.IsNaN(sum.W)
              //   || sum.X < 0 || sum.Y < 0 || sum.Z < 0 || sum.W < 0
              //   || sum.X > 1 || sum.Y > 1 || sum.Z > 1 || sum.W > 1)
              //  Debugger.Break();

              tmpImage4F.SetPixel(x, y, sum);
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }

        // Resize vertically: tmpImage --> dstImage
        {
          float scale = (float)dstImage4F.Height / tmpImage4F.Height;
          float inverseScale = 1.0f / scale;

#if SINGLE_THREADED
          for (int x = 0; x < dstImage4F.Width; x++)
#else
          Parallel.For(0, dstImage4F.Width, x =>
#endif
          {
            // Apply polyphase kernel vertically.
            for (int y = 0; y < dstImage4F.Height; y++)
            {
              float center = (y + 0.5f) * inverseScale;

              int left = (int)Math.Floor(center - kernelY.Width);
              int right = (int)Math.Ceiling(center + kernelY.Width);
              Debug.Assert(right - left <= kernelY.WindowSize);

              float totalRgbWeights = 0.0f;
              Vector4F sum = new Vector4F();
              for (int i = 0; i < kernelY.WindowSize; i++)
              {
                Vector4F color = tmpImage4F.GetPixel(x, left + i, wrapMode);

                const float alphaEpsilon = 1.0f / 256.0f;
                float alpha = alphaTransparency ? color.W + alphaEpsilon : 1.0f;

                float weight = kernelY.Weights[y, i];
                float rgbWeight = weight * alpha;
                totalRgbWeights += rgbWeight;

                sum.X += color.X * rgbWeight;
                sum.Y += color.Y * rgbWeight;
                sum.Z += color.Z * rgbWeight;
                sum.W += color.W * weight;
              }

              float f = 1 / totalRgbWeights;
              sum.X *= f;
              sum.Y *= f;
              sum.Z *= f;

              dstImage4F.SetPixel(x, y, sum);
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }
      }
      // ReSharper restore AccessToDisposedClosure
    }


    private static void Resize3D(Texture srcTexture, int srcMipIndex, int srcArrayOrFaceIndex, Texture dstTexture, int dstMipIndex, int dstArrayOrFaceIndex, Filter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      int srcIndex = srcTexture.GetImageIndex(srcMipIndex, srcArrayOrFaceIndex, 0);
      int srcWidth = srcTexture.Images[srcIndex].Width;
      int srcHeight = srcTexture.Images[srcIndex].Height;
      int srcDepth = srcTexture.GetDepth(srcMipIndex);

      int dstIndex = dstTexture.GetImageIndex(dstMipIndex, dstArrayOrFaceIndex, 0);
      int dstWidth = dstTexture.Images[dstIndex].Width;
      int dstHeight = dstTexture.Images[dstIndex].Height;
      int dstDepth = dstTexture.GetDepth(dstMipIndex);

      // Resize volume.
      var kernelX = new PolyphaseKernel(filter, srcWidth, dstWidth, 32);
      var kernelY = new PolyphaseKernel(filter, srcHeight, dstHeight, 32);
      var kernelZ = new PolyphaseKernel(filter, srcDepth, dstDepth, 32);

      var tmpTexture = new Texture(new TextureDescription
      {
        Dimension = TextureDimension.Texture3D,
        Width = dstWidth,
        Height = srcHeight,
        Depth = srcDepth,
        MipLevels = 1,
        ArraySize = 1,
        Format = DataFormat.R32G32B32A32_FLOAT
      });
      var tmpTexture2 = new Texture(new TextureDescription
      {
        Dimension = TextureDimension.Texture3D,
        Width = dstWidth,
        Height = dstHeight,
        Depth = srcDepth,
        MipLevels = 1,
        ArraySize = 1,
        Format = DataFormat.R32G32B32A32_FLOAT
      });

      // ReSharper disable AccessToDisposedClosure
      using (var srcVolume = new VolumeAccessor(srcTexture, srcMipIndex, srcArrayOrFaceIndex))
      using (var tmpVolume = new VolumeAccessor(tmpTexture, 0, 0))
      using (var tmpVolume2 = new VolumeAccessor(tmpTexture2, 0, 0))
      using (var dstVolume = new VolumeAccessor(dstTexture, dstMipIndex, dstArrayOrFaceIndex))
      {
        // Resize horizontally: srcVolume --> tmpVolume
        {
          float scale = (float)tmpVolume.Width / srcVolume.Width;
          float inverseScale = 1.0f / scale;

#if SINGLE_THREADED
          for (int z = 0; z < tmpVolume.Depth; z++)
#else
          Parallel.For(0, tmpVolume.Depth, z =>
#endif
          {
            for (int y = 0; y < tmpVolume.Height; y++)
            {
              // Apply polyphase kernel horizontally.
              for (int x = 0; x < tmpVolume.Width; x++)
              {
                float center = (x + 0.5f) * inverseScale;

                int left = (int)Math.Floor(center - kernelX.Width);
                int right = (int)Math.Ceiling(center + kernelX.Width);
                Debug.Assert(right - left <= kernelX.WindowSize);

                float totalRgbWeights = 0.0f;
                Vector4F sum = new Vector4F();
                for (int i = 0; i < kernelX.WindowSize; i++)
                {
                  Vector4F color = srcVolume.GetPixel(left + i, y, z, wrapMode);

                  const float alphaEpsilon = 1.0f / 256.0f;
                  float alpha = alphaTransparency ? color.W + alphaEpsilon : 1.0f;

                  float weight = kernelX.Weights[x, i];
                  float rgbWeight = weight * alpha;
                  totalRgbWeights += rgbWeight;

                  sum.X += color.X * rgbWeight;
                  sum.Y += color.Y * rgbWeight;
                  sum.Z += color.Z * rgbWeight;
                  sum.W += color.W * weight;
                }

                float f = 1 / totalRgbWeights;
                sum.X *= f;
                sum.Y *= f;
                sum.Z *= f;

                tmpVolume.SetPixel(x, y, z, sum);
              }
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }

        // Resize vertically: tmpVolume --> tmpVolume2
        {
          float scale = (float)tmpVolume2.Height / tmpVolume.Height;
          float inverseScale = 1.0f / scale;

#if SINGLE_THREADED
          for (int z = 0; z < tmpVolume2.Depth; z++)
#else
          Parallel.For(0, tmpVolume2.Depth, z =>
#endif
          {
            for (int x = 0; x < tmpVolume2.Width; x++)
            {
              // Apply polyphase kernel vertically.
              for (int y = 0; y < tmpVolume2.Height; y++)
              {
                float center = (y + 0.5f) * inverseScale;

                int left = (int)Math.Floor(center - kernelY.Width);
                int right = (int)Math.Ceiling(center + kernelY.Width);
                Debug.Assert(right - left <= kernelY.WindowSize);

                float totalRgbWeights = 0.0f;
                Vector4F sum = new Vector4F();
                for (int i = 0; i < kernelY.WindowSize; i++)
                {
                  Vector4F color = tmpVolume.GetPixel(x, left + i, z, wrapMode);

                  const float alphaEpsilon = 1.0f / 256.0f;
                  float alpha = alphaTransparency ? color.W + alphaEpsilon : 1.0f;

                  float weight = kernelY.Weights[y, i];
                  float rgbWeight = weight * alpha;
                  totalRgbWeights += rgbWeight;

                  sum.X += color.X * rgbWeight;
                  sum.Y += color.Y * rgbWeight;
                  sum.Z += color.Z * rgbWeight;
                  sum.W += color.W * weight;
                }

                float f = 1 / totalRgbWeights;
                sum.X *= f;
                sum.Y *= f;
                sum.Z *= f;

                tmpVolume2.SetPixel(x, y, z, sum);
              }
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }

        // Resize depth: tmpVolume2 --> dstVolume
        {
          float scale = (float)dstVolume.Depth / tmpVolume2.Depth;
          float inverseScale = 1.0f / scale;

#if SINGLE_THREADED
          for (int y = 0; y < dstVolume.Height; y++)
#else
          Parallel.For(0, dstVolume.Height, y =>
#endif
          {
            for (int x = 0; x < dstVolume.Width; x++)
            {
              // Apply polyphase kernel along z direction.
              for (int z = 0; z < dstVolume.Depth; z++)
              {
                float center = (z + 0.5f) * inverseScale;

                int left = (int)Math.Floor(center - kernelZ.Width);
                int right = (int)Math.Ceiling(center + kernelZ.Width);
                Debug.Assert(right - left <= kernelZ.WindowSize);

                float totalRgbWeights = 0.0f;
                Vector4F sum = new Vector4F();
                for (int i = 0; i < kernelZ.WindowSize; i++)
                {
                  Vector4F color = tmpVolume2.GetPixel(x, y, left + i, wrapMode);

                  const float alphaEpsilon = 1.0f / 256.0f;
                  float alpha = alphaTransparency ? color.W + alphaEpsilon : 1.0f;

                  float weight = kernelZ.Weights[z, i];
                  float rgbWeight = weight * alpha;
                  totalRgbWeights += rgbWeight;

                  sum.X += color.X * rgbWeight;
                  sum.Y += color.Y * rgbWeight;
                  sum.Z += color.Z * rgbWeight;
                  sum.W += color.W * weight;
                }

                float f = 1 / totalRgbWeights;
                sum.X *= f;
                sum.Y *= f;
                sum.Z *= f;

                dstVolume.SetPixel(x, y, z, sum);
              }
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }
      }
      // ReSharper restore AccessToDisposedClosure
    }
    #endregion
  }
}
