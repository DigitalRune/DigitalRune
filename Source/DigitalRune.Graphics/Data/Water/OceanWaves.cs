// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Analysis;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents ocean waves computed using Fast Fourier Transformation and a statistical wave
  /// spectrum.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class uses Fast Fourier Transformation (FFT) on the GPU to compute waves from many
  /// (several thousand) sine waves. The input wave spectrum is defined by a statistical spectrum of
  /// ocean waves. The <see cref="TextureSize"/> defines the number of waves which are computed. If
  /// <see cref="TextureSize"/> is 256, then the 256 x 256 waves are evaluated! The resulting wave
  /// textures tile seamlessly.
  /// </para>
  /// <para>
  /// This class only stores the settings and final textures. The <see cref="WaterWavesRenderer"/>
  /// must be used to generated the textures at runtime. <see cref="WaterWavesRenderer"/> is a scene
  /// node renderer which handles <see cref="WaterNode"/>s. If a <see cref="WaterNode"/> references
  /// <see cref="OceanWaves"/>, the renderer creates the displacement/normal map textures and stores
  /// them in the <see cref="WaterWaves.DisplacementMap"/> and <see cref="WaterWaves.NormalMap"/>
  /// properties.
  /// </para>
  /// <para>
  /// <strong>CPU Queries:</strong><br/>
  /// <see cref="EnableCpuQueries"/> can be set to <see langword="true"/> to also perform a CPU FFT
  /// on the CPU. The size of the CPU FFT is defined by <see cref="CpuSize"/>. <see cref="CpuSize"/>
  /// is usually a lot smaller than <see cref="TextureSize"/> (e.g. 16). The CPU simulation is only
  /// an approximation of the exact simulation on the GPU.
  /// </para>
  /// </remarks>
  public class OceanWaves : WaterWaves
  {
    // References:
    // http://graphics.ucsd.edu/courses/rendering/2005/jdewall/tessendorf.pdf


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty;

    // ----- CPU FFT
    // N+1 x N+1 grid.
    // Used to generate a wave height field. 
    // Each entry is a complex number.
    // h0[0,0] stores the entry for h0(-N/2,-N/2)
    // h0[N, N] stores the entry for h0(N/2, N/2)
    // Use GetIndex().
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private Vector2F[,] _h0;

    // N x N grid with precalculated valus of 1 / |k|
    // [0, 0] stores the info for k(-N/2, -N/2).
    // [N - 1, N - 1] stores the info for k(N/2 - 1, N/2 - 1).
    // Use GetIndex().
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private float[,] _oneOverKLength;

    // N x N grid with angular frequency for each k.
    // Use GetIndex().
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private float[,] _omega;

    // N x N grid containing spectrum (frequency domain).
    // Special FFT order:
    // [0, 0] contains value for (0, 0). 
    // [1, 1] contains (1, 1).
    // [N/2, N/2] contains (-N/2, -N/2).
    // [N-1, N-1] contains (-1, -1)
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private Vector2F[,] _h;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private Vector2F[,] _N;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private Vector2F[,] _D;

    private FastFourierTransformF _fft;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // Data of the WaterWavesRenderer:
    internal int LastFrame = -1;   // When the wave map was updated the last time.
    internal Texture2D H0Spectrum;
    internal RenderTarget2D DisplacementSpectrum;
    internal RenderTarget2D NormalSpectrum;


    /// <summary>
    /// Gets or sets the size of the displacement/normal map in texels.
    /// </summary>
    /// <value>
    /// The size of the displacement/normal map in texels. Must be a power of two (e.g. 2, 4, 8,
    /// etc.). The default value is 256.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 2.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not a power of two.
    /// </exception>
    public int TextureSize
    {
      // Changing size does not set dirty flag. Must be tested explicitly.

      get { return _texureSize; }
      set
      {
        if (_texureSize == value)
          return;

        if (value < 2)
          throw new ArgumentOutOfRangeException("value", "The TextureSize must be greater than 1.");
        if (!MathHelper.IsPowerOf2(value))
          throw new ArgumentException("The TextureSize must be a power of two.");

        _texureSize = value;
      }
    }
    private int _texureSize;


    /// <summary>
    /// Gets or sets the size of a single tile (one texture repetition) in world space.
    /// </summary>
    /// <value>
    /// The size of a single tile (one texture repetition) in world space. The default value is 10.
    /// </value>
    /// <inheritdoc cref="WaterWaves.TileSize"/>
    public new float TileSize
    {
      get { return base.TileSize; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == base.TileSize)
          return;

        base.TileSize = value;
        _isDirty = true;
      }
    }


    /// <summary>
    /// Gets or sets the seed of the random number generator.
    /// </summary>
    /// <value>The seed of the random number generator.</value>
    public int Seed
    {
      get { return _seed; }
      set
      {
        if (_seed == value)
          return;

        _seed = value;
        _isDirty = true;
      }
    }
    private int _seed;


    /// <summary>
    /// Gets or sets the gravity.
    /// </summary>
    /// <value>The gravity. The default value is 9.81.</value>
    public float Gravity
    {
      get { return _gravity; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_gravity == value)
          return;

        _gravity = value;
        _isDirty = true;
      }
    }
    private float _gravity;


    /// <summary>
    /// Gets or sets the wind velocity.
    /// </summary>
    /// <value>The wind velocity. The default value is (10, 0, 10).</value>
    public Vector3F Wind
    {
      get { return _wind; }
      set
      {
        if (_wind == value)
          return;

        _wind = value;
        _isDirty = true;
      }
    }
    private Vector3F _wind;


    /// <summary>
    /// Gets or sets the height scale factor.
    /// </summary>
    /// <value>The height scale factor. The default value is 0.01.</value>
    /// <remarks>
    /// This factor is used to scale the wave height. (It is NOT the wave height in world space 
    /// units.)
    /// </remarks>
    public float HeightScale { get; set; }


    /// <summary>
    /// Gets or sets the directionality of the waves.
    /// </summary>
    /// <value>
    /// The directionality of the waves. Must be greater than 0. The default value is 1.
    /// </value>
    /// <remarks>
    /// Higher <see cref="Directionality"/> values will make more waves flow in the wind direction,
    /// with less waves deviating from the wind direction.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> must be greater than 0.
    /// </exception>
    public int Directionality
    {
      get { return _directionality; }
      set
      {
        if (_directionality == value)
          return;

        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "Directionality must be greater than 0.");

        _directionality = 1;
        _isDirty = true;
      }
    }
    private int _directionality;


    /// <summary>
    /// Gets or sets the choppiness factor which scales the horizontal displacement.
    /// </summary>
    /// <value>
    /// The choppiness factor which scales the horizontal displacement. The default value is 1.
    /// </value>
    /// <remarks>
    /// Use a <see cref="Choppiness"/> of 0 to get very round waves (no horizontal displacement of 
    /// vertices). Use values greater than 0 to create choppy waves.
    /// </remarks>
    public float Choppiness
    {
      get { return _choppiness; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_choppiness == value)
          return;

        _choppiness = value;
        _isDirty = true;
      }
    }
    private float _choppiness;


    /// <summary>
    /// Gets or sets the small wave suppression.
    /// </summary>
    /// <value>The small wave suppression.</value>
    /// <remarks>
    /// This property can be used to suppress small waves. If this value is 0.01, then waves which
    /// are smaller then 1% of the highest wave are suppressed. The default value is 0.0001.
    /// </remarks>
    public float SmallWaveSuppression
    {
      get { return _smallWaveSuppression; }
      set
      {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_smallWaveSuppression == value)
          return;

        _smallWaveSuppression = value;
        _isDirty = true;
      }
    }
    private float _smallWaveSuppression;


    /// <summary>
    /// Gets or sets a value indicating whether CPU queries using <see cref="GetDisplacement"/> are
    /// enabled.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if CPU queries using <see cref="GetDisplacement"/> are enabled;
    /// otherwise, <see langword="false" />. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If <see cref="EnableCpuQueries"/> is <see langword="false"/>, the ocean wave simulation is
    /// only performed on the GPU. If <see cref="EnableCpuQueries"/> is <see langword="true"/>, the
    /// ocean wave simulation is performed on the GPU and the CPU. The CPU simulation is necessary
    /// to enable queries using <see cref="GetDisplacement"/>.
    /// </para>
    /// <para>
    /// <see cref="TextureSize"/> defines the size of the simulation grid on the GPU;
    /// <see cref="CpuSize"/> defines the size of the simulation grid on the CPU. Since the CPU
    /// simulation is slower <see cref="CpuSize"/> should be a lot smaller than
    /// <see cref="TextureSize"/> (e.g. 16 vs. 256).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool EnableCpuQueries
    {
      get { return _enableCpuQueries; }
      set
      {
        if (_enableCpuQueries == value)
          return;

        _enableCpuQueries = value;
        _isDirty = true;
      }
    }
    private bool _enableCpuQueries;


    /// <summary>
    /// Gets or sets the simulation size for CPU queries using <see cref="GetDisplacement"/>.
    /// </summary>
    /// <value>
    /// The simulation size for CPU queries using <see cref="GetDisplacement"/>. Must be a power of
    /// two. The default value is 16.
    /// </value>
    /// <inheritdoc cref="EnableCpuQueries"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 2.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not a power of two.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int CpuSize
    {
      // Changing size does not set dirty flag. Must be tested explicitly.

      get { return _cpuSize; }
      set
      {
        if (_cpuSize == value)
          return;

        if (value < 2)
          throw new ArgumentOutOfRangeException("value", "The CpuSize must be greater than 1.");
        if (!MathHelper.IsPowerOf2(value))
          throw new ArgumentException("The CpuSize must be a power of two.");

        _cpuSize = value;
      }
    }
    private int _cpuSize;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="OceanWaves"/> class.
    /// </summary>
    public OceanWaves()
    {
      _isDirty = true;
      Seed = 1234567;
      TileSize = 10;
      TextureSize = 256;
      CpuSize = 16;
      Gravity = 9.81f;
      Wind = new Vector3F(10, 0, 10);
      HeightScale = 0.01f;
      Directionality = 1;
      Choppiness = 1;
      SmallWaveSuppression = 0.0001f;
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="OceanWaves"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "H0Spectrum")]
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Dispose managed resources.
        H0Spectrum.SafeDispose();
        H0Spectrum = null;
        DisplacementSpectrum.SafeDispose();
        DisplacementSpectrum = null;
        NormalSpectrum.SafeDispose();
        NormalSpectrum = null;
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Called by the WaterWavesRenderer.
    internal void Update(GraphicsDevice graphicsDevice, float time)
    {
      // Compute new H0 spectrum if required.
      if (_isDirty || (TextureSize + 1) != H0Spectrum.Width)
        InitializeH0Spectrum(graphicsDevice);

      // Update CPU simulation if required.
      if (EnableCpuQueries)
      {
        if (_isDirty || (CpuSize + 1) != _h0.GetLength(0))
          InitializeCpuFft();

        PerformCpuFft(time);
      }

      _isDirty = false;
    }


    // Creates texture containing h0.
    private void InitializeH0Spectrum(GraphicsDevice graphicsDevice)
    {
      // See also comments in InitializeCpuFft().

      var n = TextureSize;
      var h0 = new Vector2F[(n + 1) * (n + 1)];
      var random = new Random(Seed);
      var distribution = new FastGaussianDistributionF(0, 1);

      // TODO: Do not compute the inner parts of this h0 and _h0 in PerformCpuFft() twice.
      float oneOverSqrt2 = 1 / (float)Math.Sqrt(2);
      for (int i = 0; i <= n / 2; i++)
      {
        for (int x = -i; x <= i; x++)
        {
          for (int y = -i; y <= i; y++)
          {
            if (x > -i && x < i && y > -i && y < i)
            {
              y = i - 1;
              continue;
            }

            Vector2F xi = new Vector2F(distribution.Next(random), distribution.Next(random));
            Vector2F k = new Vector2F(GetKx(x), GetKy(y));

            //h0[GetIndex(x, n), GetIndex(y, n)] =
            h0[GetIndex(x, n) + (n + 1) * GetIndex(y, n)] =
              xi * (oneOverSqrt2 * (float)Math.Sqrt(GetPhillipsSpectrum(k)));
          }
        }
      }

      H0Spectrum.SafeDispose();
      H0Spectrum = new Texture2D(graphicsDevice, n + 1, n + 1, false, SurfaceFormat.Vector2);
      H0Spectrum.SetData(h0);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private void InitializeCpuFft()
    {
      var n = CpuSize;

      if (_h0 == null || _h0.GetLength(0) != n + 1)
        _h0 = new Vector2F[n + 1, n + 1];

      var random = new Random(Seed);
      var distribution = new FastGaussianDistributionF(0, 1);

      // Create h0.
      float oneOverSqrt2 = 1 / (float)Math.Sqrt(2);
      // Instead of simply iterating over all elements of h0, we fill it in concentric
      // rectangles from the center to the border. This way, a simulation with N = 16
      // will use the same frequencies as a simulation with N = 32 because the inner h0s
      // are initialized with the same random numbers!!! 
      for (int i = 0; i <= n / 2; i++)
      {
        for (int x = -i; x <= i; x++)
        {
          for (int y = -i; y <= i; y++)
          {
            // Skip elements of the inner part which was already initialized.
            if (x > -i && x < i && y > -i && y < i)
            {
              y = i - 1;
              continue;
            }

            Vector2F xi = new Vector2F(distribution.Next(random), distribution.Next(random));
            Vector2F k = new Vector2F(GetKx(x), GetKy(y));

            _h0[GetIndex(x, n), GetIndex(y, n)] =
              xi * (oneOverSqrt2 * (float)Math.Sqrt(GetPhillipsSpectrum(k)));
          }
        }
      }

      if (_oneOverKLength == null || _oneOverKLength.GetLength(0) != n)
      {
        _oneOverKLength = new float[n, n];
        _omega = new float[n, n];
        _h = new Vector2F[n, n];
        _N = new Vector2F[n, n];
        _D = new Vector2F[n, n];
      }

      // Create _oneOverKLength and _omega
      for (int x = -n / 2; x < n / 2; x++)
      {
        for (int y = -n / 2; y < n / 2; y++)
        {
          float length = new Vector2F(GetKx(x), GetKy(y)).Length;

          _omega[GetIndex(x, n), GetIndex(y, n)] = GetOmega(length);

          // Avoid division by zero.
          if (length < 1e-8f)
            length = 1e-8f;

          _oneOverKLength[GetIndex(x, n), GetIndex(y, n)] = 1 / length;
        }
      }
    }


    private void PerformCpuFft(float time)
    {
      // Compute _h.
      int n = CpuSize;
      for (int x = -n / 2; x < n / 2; x++)
      {
        for (int y = -n / 2; y < n / 2; y++)
        {
          var indexX = GetIndex(x, n);
          var indexY = GetIndex(y, n);

          // The frequency domain images have a different order:
          var fftIndexX = x & (n - 1);
          var fftIndexY = y & (n - 1);

          var k = new Vector2F(GetKx(x), GetKy(y));

          //float omega = GetOmega(water, k.Length);
          float omega = _omega[indexX, indexY];

          // h = h0(x, y) * e^(i * omega * t) + conj(h0(-x, -y)) * e^(-i * omega * t).
          float cos = (float)Math.Cos(omega * time);
          float sin = (float)Math.Sin(omega * time);

          // conj(Z) = Z.Re - i * Z.Im.
          // e^(ix) = cos(x) + i * sin(x)
          // e^(-ix) = cos(x) - i * sin(x)
          // Multiplication of two complex: (a + ib) *  (c + id) = (ac - bd) + i(ad + bc)
          // --> h.Re = h0(x, y).Re * cos - h0(x, y).Im *sin + h0(-x, -y).Re * cos - h0(-x, -y).Im * sin
          //     h.Im = h0(x, y).Re * sin + h0(x, y).Im * cos  - h0(-x, -y).Re * sin - h0(-x, -y).Im * cos
          var h0XY = _h0[indexX, indexY] * HeightScale;
          var h0NegXNegY = _h0[GetIndex(-x, n), GetIndex(-y, n)] * HeightScale;
          var h = new Vector2F((h0XY.X + h0NegXNegY.X) * cos - (h0XY.Y + h0NegXNegY.Y) * sin,
                               (h0XY.X - h0NegXNegY.X) * sin + (h0XY.Y - h0NegXNegY.Y) * cos);
          _h[fftIndexX, fftIndexY] = h;

          // For normals, we have to perform inverse FFT for normal.X and normal.Y.
          // normal.x = InverseFFT(-i * k.x / kLength * h).Re
          // normal.y = InverseFFT(-i * k.y / kLength * h).Re
          // However, since FFT is linear and normal.X/Y are real (imaginary values are 0),
          // we can combine them into a single FFT:
          // FFT(Nx) = SpectrumNx, FFT(Ny) = SpectrumNy
          // Add and multiply by any constant, e.g. i!
          // FFT(Nx + i * Ny) = SpectrumNx + i * SpectrumNy
          // Nx + i * Ny = InverseFFT(SpectrumNx + i * SpectrumNy)
          // SpectrumNx = i * k.X * h = i * (k.X * h.Re + i * k.X * h.Im) = -k.X * h.Im + i * k.X * h.Re
          // i * SpectrumNy = i * i * k.Y * h = -1 * (k.Y * h.Re + i * k.Y * h.Im)
          _N[fftIndexX, fftIndexY] = new Vector2F(-k.X * h.Y - k.Y * h.X,
                                                  k.X * h.X - k.Y * h.Y);

          // Horizontal displacements for choppy waves:
          // Again we combine the x and y displacement into one FFT.
          // D.x = InverseFFT(-i * k.x / kLength * h).Re
          // D.y = InverseFFT(-i * k.y / kLength * h).Re
          k *= _oneOverKLength[indexX, indexY];
          _D[fftIndexX, fftIndexY] = new Vector2F(k.X * h.Y + k.Y * h.X,
                                                  -k.X * h.X + k.Y * h.Y);
        }
      }

      // Transform data from frequency domain to spatial domain.
      if (_fft == null)
        _fft = new FastFourierTransformF(n);
      else if (_fft.Capacity < n)
        _fft.Capacity = n;

      _fft.Transform2D(_h, false);
      _fft.Transform2D(_D, false);
      _fft.Transform2D(_N, false);
    }


    // Use to index _h0, _oneOverKLength, _omega
    // i must be in [-n/2, n/2]
    private static int GetIndex(int i, int gridSize)
    {
      return i + gridSize / 2;
    }


    // Returns kx for a given x position in the frequency domain grid.
    // x must be in [-N/2, N/2].
    private float GetKx(int x)
    {
      float Lx = TileSize;
      return 2 * ConstantsF.Pi * x / Lx;
    }


    // Returns ky for a given y position in the frequency domain grid.
    // y must be in [-N/2, N/2].
    private float GetKy(int y)
    {
      float Ly = TileSize;
      return 2 * ConstantsF.Pi * y / Ly;
    }


    private float GetOmega(float kLength)
    {
      // The Dispersion Relation:
      // Deep water: ω² = g * |k|
      return (float)Math.Sqrt(Gravity * kLength);

      // Shallow water: ω² = g * |k| * tanh(|k| * waterDepth)
    }


    private float GetPhillipsSpectrum(Vector2F k)
    {
      float kLength = k.Length;

      // Avoid division by zero.
      if (kLength < 1e-8f)
        kLength = 1e-8f;

      Vector2F kDirection = k / kLength;

      // Largest possible wave L = V² / g
      float windSpeedSquared = Wind.LengthSquared;
      float windSpeed = (float)Math.Sqrt(windSpeedSquared);
      if (windSpeed < Numeric.EpsilonF)
        return 0;

      float L = windSpeedSquared / Gravity;

      float l = L * SmallWaveSuppression;

      Vector2F windDirection;
      windDirection.X = Wind.X / windSpeed;
      windDirection.Y = Wind.Z / windSpeed;

      // NOTE: In AMD water:
      //   HeightScale = 0.00000375 with m_fWindVelocity = 9 and N = 128 for slower, calm seas
      //   HeightScale = 0.00001    with N = 128 for calm seas
      //   HeightScale = 0.0000075  with N = 128 gives beautiful, very calm seas
      //   HeightScale = 0.000005   with N = 64, m_fWindVelocity = 9
      // AMD to filter out waves against the wind direction:
      // if (Vector2F.Dot(kDirection, windDirection) < 0) phillips *= 0.25

      // 2 * _directionality is always 2 in Tessendorf's paper. But we can use any multiple of 2.

      return (float)(/*Height * */  // We apply height scale each frame!
                     Math.Exp(-1 / Math.Pow(kLength * L, 2) - Math.Pow(kLength * l, 2))
                     / Math.Pow(kLength, 4)
                     * Math.Pow(Vector2F.Dot(kDirection, windDirection), 2 * _directionality));
    }


    /// <summary>
    /// Gets the surface displacement caused by the water waves.
    /// </summary>
    /// <param name="x">The x position in world space.</param>
    /// <param name="z">The z position in world space.</param>
    /// <param name="displacement">The displacement vector in world space.</param>
    /// <param name="normal">The normal vector in world space.</param>
    /// <returns>
    /// <see langword="true"/> if successful; otherwise, <see langword="false"/> if the results are
    /// invalid because the CPU simulation has not been performed.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="EnableCpuQueries"/> is <see langword="false"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public bool GetDisplacement(float x, float z, out Vector3F displacement, out Vector3F normal)
    {
      if (!EnableCpuQueries)
        throw new InvalidOperationException("OceanWaves.GetDisplacement() can only be called if EnableCpuQueries is set to true.");

      displacement = new Vector3F(0);
      normal = new Vector3F(0, 1, 0);
      if (_h == null)
        return false;

      float texCoordX = (x - TileCenter.X) / TileSize + 0.5f;
      float texCoordY = (z - TileCenter.Z) / TileSize + 0.5f;

      // Point sampling or bilinear filtering:
#if false
      // Convert to array indices.
      int xIndex = Wrap((int)(texCoordX * CpuSize));
      int yIndex = Wrap((int)(texCoordY * CpuSize));

      float h = _h[xIndex, yIndex].X;
      Vector2F d = _D[xIndex, yIndex];
      Vector2F n = _N[xIndex, yIndex];
#else
      // Sample 4 values. The upper left index is (without wrapping):
      float xIndex = texCoordX * CpuSize - 0.5f;
      float yIndex = texCoordY * CpuSize - 0.5f;

      // Get the 4 indices.
      int x0 = Wrap((int)xIndex);
      int x1 = Wrap((int)xIndex + 1);
      int y0 = Wrap((int)yIndex);
      int y1 = Wrap((int)yIndex + 1);

      // Get fractions to use as lerp parameters.
      float px = MathHelper.Frac(xIndex);
      float py = MathHelper.Frac(yIndex);

      float h = InterpolationHelper.Lerp(InterpolationHelper.Lerp(_h[x0, y0].X, _h[x1, y0].X, px),
                                         InterpolationHelper.Lerp(_h[x0, y1].X, _h[x1, y1].X, px),
                                         py);

      Vector2F d = InterpolationHelper.Lerp(InterpolationHelper.Lerp(_D[x0, y0], _D[x1, y0], px),
                                            InterpolationHelper.Lerp(_D[x0, y1], _D[x1, y1], px),
                                            py);

      Vector2F n = InterpolationHelper.Lerp(InterpolationHelper.Lerp(_N[x0, y0], _N[x1, y0], px),
                                            InterpolationHelper.Lerp(_N[x0, y1], _N[x1, y1], px),
                                            py);
#endif

      displacement = new Vector3F(-d.X * Choppiness, h, -d.Y * Choppiness);

      normal = new Vector3F(-n.X, 0, -n.Y);
      normal.Y = (float)Math.Sqrt(1 - normal.X * normal.X - normal.Y * normal.Y);
      return true;
    }


    private int Wrap(int index)
    {
      int size = CpuSize;
      return ((index % size) + size) % size;
    }
    #endregion
  }
}
