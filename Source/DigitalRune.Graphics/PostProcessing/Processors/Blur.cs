// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Blurs the image using a convolution filter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Per default, a box blur is performed. The type of blur can be changed using
  /// <list type="bullet">
  /// <item>
  /// <description><see cref="InitializeBoxBlur"/>, </description>
  /// </item>
  /// <item>
  /// <description><see cref="InitializeGaussianBlur"/>, </description>
  /// </item>
  /// <item>
  /// <description><see cref="InitializePoissonBlur"/>, </description>
  /// </item>
  /// <item>
  /// <description>
  /// or by changing the sample <see cref="Offsets"/> and sample <see cref="Weights"/> directly.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// Many convolution blur filters are separable and can be performed in two passes (a horizontal
  /// blur and a vertical blur). If this is the case, <see cref="IsSeparable"/> can be set to
  /// <see langword="true"/> and two passes will be performed. In the second pass, the x and y
  /// values in <see cref="Offsets"/> are switched internally.
  /// </para>
  /// <para>
  /// <strong>Limitations:</strong><br/>
  /// Anisotropic or joint bilateral filtering in log-space is not supported. When
  /// <see cref="FilterInLogSpace"/> is set, the properties <see cref="IsAnisotropic"/> and
  /// <see cref="IsBilateral"/> are ignored.
  /// </para>
  /// </remarks>
  public class Blur : PostProcessor
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // 13-tap Poisson Disk
    // (Offsets taken from Shader X² Depth of Field article. New Poisson disk 
    // kernels can be generated using the DigitalRune Poisson Disk Generator.)
    private static readonly Vector2[] PoissonDiskKernel =
    {
      new Vector2(0, 0),
      new Vector2(-0.326212f,-0.40581f),
      new Vector2(-0.840144f,-0.07358f),
      new Vector2(-0.695914f,0.457137f),
      new Vector2(-0.203345f,0.620716f),
      new Vector2(0.96234f,-0.194983f),
      new Vector2(0.473434f,-0.480026f),
      new Vector2(0.519456f,0.767022f),
      new Vector2(0.185461f,-0.893124f),
      new Vector2(0.507431f,0.064425f),
      new Vector2(0.89642f,0.412458f),
      new Vector2(-0.32194f,-0.932615f),
      new Vector2(-0.791559f,-0.59771f)
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterWeights;
    private readonly EffectParameter _parameterSourceTexture;
    private readonly EffectParameter _parameterOffsets;

    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterBlurParameters0;

    // Arrays for internal use.
    private readonly Vector3[] _frustumFarCorners = new Vector3[4];
    private readonly Vector2[] _horizontalOffsets;
    private Vector2[] _verticalOffsets;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the maximum number of samples that are supported.
    /// </summary>
    /// <value>The maximum number of samples.</value>
    /// <remarks>
    /// This constant value determines the length of the arrays <see cref="Offsets"/> and 
    /// <see cref="Weights"/>, and the max. allowed <see cref="NumberOfSamples"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Max number of samples may depend on profile.")]
    public int MaxNumberOfSamples
    {
      get { return 23; }
    }


    /// <summary>
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>
    /// The number of samples. This value must be greater than 0 and less than 
    /// <see cref="MaxNumberOfSamples"/>.
    /// </value>
    /// <remarks>
    /// This property determines how many samples ("taps") will be performed to blur each pixel.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 1 or more than <see cref="MaxNumberOfSamples"/>.
    /// </exception>
    public int NumberOfSamples
    {
      get { return _numberOfSamples; }
      set
      {
        if (value < 1 || value > MaxNumberOfSamples)
          throw new ArgumentOutOfRangeException("value", "NumberOfSamples must be in the range [1, MaxNumberOfSamples].");

        _numberOfSamples = value;
      }
    }
    private int _numberOfSamples;


    /// <summary>
    /// Gets the sample offsets.
    /// </summary>
    /// <value>
    /// The sample offsets in pixels. For example, a value of (1, 0) can be set to sample 1 pixel
    /// to the right.
    /// </value>
    /// <remarks>
    /// Only the first <see cref="NumberOfSamples"/> elements of this array are used. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is required for EffectParameter.")]
    public Vector2[] Offsets { get; private set; }


    /// <summary>
    /// Gets or sets the weights of the samples.
    /// </summary>
    /// <value>The weights.</value>
    /// <remarks>
    /// Only the first <see cref="NumberOfSamples"/> elements of this array are used. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is required for EffectParameter.")]
    public float[] Weights { get; private set; }


    /// <summary>
    /// Gets or sets the scale that is applied to the offsets.
    /// </summary>
    /// <value>The scale applied to the offsets.</value>
    /// <remarks>
    /// Usually, a scale of 1 is used, but some blur kernels, like the Poisson disk kernel require a
    /// scale greater than 1. (The <see cref="Offsets"/> of a Poisson disk kernel are usually 
    /// defined for a range of [-1, 1]. The scale determines the effective size (radius) of the 
    /// Poisson disk. See also <see cref="InitializePoissonBlur"/>.)
    /// </remarks>
    public float Scale { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether to use an anisotropic filter kernel.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the filter kernel is anisotropic; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// By default an isotropic filter kernel is used, which means that the filter is equal in all
    /// dimensions. The specified filter is equally applied to all pixels in x- and y-direction.
    /// </para>
    /// <para>
    /// In contrast, an anisotropic filter kernel is adjusted for each pixel. Surface position and
    /// normal are read from the G-buffer and the filter kernel is scaled and rotated to match the
    /// underlying surface.
    /// </para>
    /// <para>
    /// Currently, anisotropic filtering is only supported for separable filter kernels.
    /// <see cref="IsSeparable"/> must be set to <see langword="true"/>.
    /// </para>
    /// </remarks>
    public bool IsAnisotropic { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether joint bilateral filtering (= edge-aware filtering)
    /// is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if joint bilateral filtering is enabled; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Joint bilateral filtering (= edge-aware filtering) can be enabled to blur surfaces in a 3D
    /// scene and prevent filtering across object boundaries and depth discontinuities.
    /// </para>
    /// <para>
    /// <i>Bilateral filtering</i> means that the filter kernels is only applied to pixels that are
    /// close (e.g. geometric similarity or photometric similarity). In this case the sample weights
    /// (e.g. the Gaussian weights) are scaled by a range function. <i>Joint</i> or <i>cross
    /// bilateral filtering</i> means that the range function is applied to a second image instead
    /// of the image that is being processed.
    /// </para>
    /// <para>
    /// When <see cref="IsBilateral"/> is enabled the depth of each sample is read from the depth
    /// buffer (G-buffer 0). The sample weights are weighted based on the depth difference to the
    /// current pixel. Samples near the current pixel (small depth difference) contribute more than
    /// distant samples (large depth difference).
    /// </para>
    /// </remarks>
    public bool IsBilateral { get; set; }


    /// <summary>
    /// Gets or sets the edge softness for bilateral filtering.
    /// </summary>
    /// <value>
    /// The edge softness for bilateral filtering in world space units. The default value is 0.1.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <see cref="IsBilateral"/> is enabled, an edge-aware blur is used to avoid blurring over
    /// depth discontinuities. The sensitivity of the edge-aware blur is defined by 
    /// <see cref="EdgeSoftness"/>. The value is the max allowed depth difference of two pixel in
    /// world space units (at 1 unit in front of the camera). Pixels that closer than this threshold 
    /// are blurred together; pixels which are farther apart are ignored.
    /// </para>
    /// <para>
    /// Decrease the value to make edges crisper. Increase to make edges softer.
    /// </para>
    /// </remarks>
    public float EdgeSoftness { get; set; }


    /// <summary>
    /// Gets or sets a value that controls how scene depth influences the filter scale.
    /// (Only used by anisotropic or bilateral blurs.)
    /// </summary>
    /// <value>The depth scaling value in the range [0, 1]. The default value is 0.7.</value>
    /// <remarks>
    /// <para>
    /// This property is only relevant for anisotropic (<see cref="IsAnisotropic"/>) or bilateral
    /// (<see cref="IsBilateral"/>) filters.
    /// </para>
    /// <para>
    /// Setting <see cref="DepthScaling"/> to 0 disables depth scaling. The filter radius is
    /// constant over the entire scene.
    /// </para>
    /// <para>
    /// If <see cref="DepthScaling"/> is 1, then the filter radius is scaled with scene depth,
    /// getting smaller in the distance. The filter radius at a distance of 1 unit is 100%. The
    /// filter radius at infinity is 0.
    /// </para>
    /// <para>
    /// The value can be between 0 and 1, for example: If <see cref="DepthScaling"/> is 0.7, the 
    /// filter radius decreases slowly with scene depth. The filter radius at a distance of 1 unit
    /// is 100%. The filter radius at infinity is 30%.
    /// </para>
    /// </remarks>
    public float DepthScaling { get; set; }
    

    /// <summary>
    /// Gets or sets a value indicating whether the configured blur filter is separable.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is separable; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Filters like a box blur or a Gaussian blur are separable. This means, the result of an n x n
    /// blur is equal to an n x 1 blur followed by an 1 x n blur. If this flag is set, this 
    /// processor will perform a blur pass in two steps: First, the image blurred using the 
    /// configured <see cref="Offsets"/>. Then, the x and y values of the <see cref="Offsets"/> are 
    /// swapped and the blurred image is blurred again.
    /// </remarks>
    public bool IsSeparable { get; set; }


    /// <summary>
    /// Gets or sets the number of blur passes.
    /// </summary>
    /// <value>The number of passes. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// If this value is greater than 1, the processor will perform several consecutive blur passes.
    /// </para>
    /// <para>
    /// Note: If <see cref="IsSeparable"/> is set, the effective number of passes is 2 * 
    /// <see cref="NumberOfPasses"/> because each blur step is performed in two passes.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 1.
    /// </exception>
    public int NumberOfPasses
    {
      get { return _numberOfPasses; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "NumberOfPasses must be greater than 0.");

        _numberOfPasses = value;
      }
    }
    private int _numberOfPasses;


    /// <summary>
    /// Gets or sets a value indicating whether log-space filtering is used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if log-space filtering is used; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Log-space filtering must be applied for <i>Exponential Shadow Maps</i>.
    /// </remarks>
    public bool FilterInLogSpace { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Blur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public Blur(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/Blur");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterWeights = _effect.Parameters["Weights"];
      _parameterSourceTexture = _effect.Parameters["SourceTexture"];
      _parameterOffsets = _effect.Parameters["Offsets"];

      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterBlurParameters0 = _effect.Parameters["BlurParameters0"];

      _horizontalOffsets = new Vector2[MaxNumberOfSamples];
      // _verticalOffsets is initialized when needed.

      Scale = 1;
      Offsets = new Vector2[MaxNumberOfSamples];
      Weights = new float[MaxNumberOfSamples];
      NumberOfPasses = 1;
      EdgeSoftness = 0.1f;
      DepthScaling = 0.7f;

      InitializeBoxBlur(9, true);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Computes sample offsets and weights for a 13-tap Poisson disk filter kernel.
    /// </summary>
    /// <remarks>
    /// The Poisson disk blur is 1-pass effect. <see cref="IsSeparable"/> is set to 
    /// <see langword="false"/> and the <see cref="Scale"/> is set to 5. The scale determines the 
    /// size (radius) of the Poisson disk. (The <see cref="Offsets"/> of a Poisson disk kernel are 
    /// usually defined for a range of [-1, 1]. The scale determines the effective size (radius) of 
    /// the Poisson disk.)
    /// </remarks>
    public void InitializePoissonBlur()
    {
      NumberOfSamples = PoissonDiskKernel.Length;
      Scale = 5;
      IsSeparable = false;

      for (int i = 0; i < NumberOfSamples; i++)
        Offsets[i] = PoissonDiskKernel[i];

      float weight = 1.0f / NumberOfSamples;
      for (int i = 0; i < NumberOfSamples; i++)
        Weights[i] = weight;
    }


    // Note: The initialization methods below generate blur kernels. The methods 
    // contain code to generate unordered sample offsets or ordered (ascending) 
    // sample offsets. Unordered sample offsets are faster to compute because we
    // can calculate two samples at once. But ordered samples might be more cache
    // efficient - in particular when using extremely large filter kernels.


    /// <summary>
    /// Computes sample offsets and weights for box blur filter kernel.
    /// </summary>
    /// <param name="numberOfSamples">
    /// The number of samples. This value must be an odd number (e.g. 3, 5, 7, ...).
    /// </param>
    /// <param name="useHardwareFiltering">
    /// If set to <see langword="true"/> hardware filtering is used to increase the blur effect; 
    /// otherwise, hardware filtering is not used. Use <see langword="false"/> if you are filtering
    /// floating-point textures.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSamples"/> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="numberOfSamples"/> is an even number. A box blur requires an odd number of
    /// samples.
    /// </exception>
    public void InitializeBoxBlur(int numberOfSamples, bool useHardwareFiltering)
    {
      if (numberOfSamples < 1)
        throw new ArgumentOutOfRangeException("numberOfSamples", "The numberOfSamples must be greater than 0.");
      if (numberOfSamples % 2 == 0)
        throw new ArgumentException("The number of samples must be odd.");

      // Initialize weights with 0.
      for (int i = 0; i < MaxNumberOfSamples; i++)
        Weights[i] = 0;

      NumberOfSamples = numberOfSamples;
      Scale = 1;
      IsSeparable = true;

      if (useHardwareFiltering)
      {
        // Sample the center pixel in the middle and then between pixel.

        // All pixels use the same weight. The sum of weights must be 1. Since 
        // all off-center samples sample two pixels at once they use the double 
        // weight.
        float weight = 1.0f / (2 * numberOfSamples - 1);

        /* 
          // ----- Unordered samples
          Offsets[0] = new Vector2(0, 0);
          Weights[0] = weight;
          for (int i = 1; i < numberOfSamples; i += 2)
          {
            var offset = new Vector2(1.5f + (i - 1), 0); // = 1.5 + k * 2
            Offsets[i] = offset;
            Offsets[i + 1] = -offset;
            Weights[i] = weight * 2;
            Weights[i + 1] = weight * 2;
          }
        */

        // ----- Ordered samples
        int left = (numberOfSamples - 1) / 2; // Number of samples on the left.
        int right = numberOfSamples / 2;      // Number of samples on the right.
        int count = 0;                        // Number of samples generated.

        Debug.Assert(left + right + 1 == numberOfSamples, "Wrong number of samples?");

        // Samples on the left.
        for (int i = -left; i <= -1; i++)
        {
          Offsets[count] = new Vector2(2 * i + 0.5f, 0);
          Weights[count] = weight * 2;
          count++;
        }

        // Center sample.
        Offsets[count] = new Vector2(0, 0);
        Weights[count] = weight;
        count++;

        // Samples on the right.
        for (int i = 1; i <= right; i++)
        {
          Offsets[count] = new Vector2(2 * i - 0.5f, 0);
          Weights[count] = weight * 2;
          count++;
        }

        Debug.Assert(count == numberOfSamples, "Wrong number of samples generated?");
        Debug.Assert(Numeric.AreEqual(Weights.Take(numberOfSamples).Sum(), 1.0f), "Invalid sample weights.");
      }
      else
      {
        // Sample in the middle of pixels.

        // All samples use the same weight. The sum of weights must be 1.
        float weight = 1.0f / numberOfSamples;

        /* 
          // ----- Unordered samples
          Offsets[0] = new Vector2(0, 0);
          Weights[0] = weight;
          for (int i = 1; i < numberOfSamples; i += 2)
          {
            var offset = new Vector2((int)(i / 2), 0);
            Offsets[i] = offset;
            Offsets[i + 1] = -offset;
            Weights[i] = weight;
            Weights[i + 1] = weight;
          }
        */

        // ----- Ordered samples
        int left = (numberOfSamples - 1) / 2; // Number of samples on the left.
        int right = numberOfSamples / 2;      // Number of samples on the right.
        int count = 0;                        // Number of samples generated.

        Debug.Assert(left + right + 1 == numberOfSamples, "Wrong number of samples?");

        // Samples on the left.
        for (int i = -left; i <= -1; i++)
        {
          Offsets[count] = new Vector2(i, 0);
          Weights[count] = weight;
          count++;
        }

        // Center sample.
        Offsets[count] = new Vector2(0, 0);
        Weights[count] = weight;
        count++;

        // Samples on the right.
        for (int i = 1; i <= right; i++)
        {
          Offsets[count] = new Vector2(i, 0);
          Weights[count] = weight;
          count++;
        }

        Debug.Assert(count == numberOfSamples, "Wrong number of samples generated?");
        Debug.Assert(Numeric.AreEqual(Weights.Take(numberOfSamples).Sum(), 1.0f), "Invalid sample weights.");
      }
    }


    /// <summary>
    /// Computes sample offsets and weights for Gaussian blur filter kernel.
    /// </summary>
    /// <param name="numberOfSamples">
    /// The number of samples. This value must be an odd number (e.g. 3, 5, 7, ...).
    /// </param>
    /// <param name="standardDeviation">The standard deviation.</param>
    /// <param name="useHardwareFiltering">
    /// If set to <see langword="true"/> hardware filtering is used to increase the blur effect;
    /// otherwise, hardware filtering is not used. Use <see langword="false"/> if you are filtering
    /// floating-point textures.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSamples"/> is zero or negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="numberOfSamples"/> is an even number. A Gaussian blur requires an odd number of
    /// samples.
    /// </exception>
    public void InitializeGaussianBlur(int numberOfSamples, float standardDeviation, bool useHardwareFiltering)
    {
      if (numberOfSamples < 1)
        throw new ArgumentOutOfRangeException("numberOfSamples", "The numberOfSamples must be greater than 0.");
      if (numberOfSamples % 2 == 0)
        throw new ArgumentException("Gaussian blur expects an odd number of samples.");

      // Initialize weights with 0.
      for (int i = 0; i < MaxNumberOfSamples; i++)
        Weights[i] = 0;

      NumberOfSamples = numberOfSamples;
      Scale = 1;
      IsSeparable = true;

      // Define the Gaussian function coefficient that we use.
      float coefficient = 1 / (float)Math.Sqrt(ConstantsF.TwoPi) / standardDeviation;

      if (useHardwareFiltering)
      {
        // Sample the center pixel in the middle and then between pixel.
        // We sample 2 pixels per tap, so we can sample twice as wide.
        standardDeviation = standardDeviation * 2;

        /* 
          // ----- Unordered samples
          Offsets[0] = new Vector2(0, 0);
          Weights[0] = MathHelper.Gaussian(0, coefficient, 0, standardDeviation);
          float weightSum = Weights[0];
          for (int i = 1; i < numberOfSamples; i += 2)
          {
            // Get an offset between two pixels.
            var offset = new Vector2(1.5f + (i - 1), 0); // = 1.5 + k * 2
            // Get the offsets of the neighboring pixel centers.
            var o0 = offset.X - 0.5f;
            var o1 = offset.X + 0.5f;
            // Compute the weights of the pixel centers.
            var w0 = MathHelper.Gaussian(o0, coefficient, 0, standardDeviation);
            var w1 = MathHelper.Gaussian(o1, coefficient, 0, standardDeviation);
            // Shift the offset to the pixel center that has the higher weight.
            offset.X = (o0 * w0 + o1 * w1) / (w0 + w1);

            Offsets[i] = offset;
            Offsets[i + 1] = -offset;
            Weights[i] = w0 + w1;
            Weights[i + 1] = Weights[i];
            weightSum += Weights[i] * 2;
          }

          // Normalize weights.
          for (int i = 0; i < numberOfSamples; i++)
            Weights[i] /= weightSum;
        */

        // ----- Ordered samples
        int left = (numberOfSamples - 1) / 2; // Number of samples on the left.
        int right = numberOfSamples / 2;      // Number of samples on the right.
        int count = 0;                        // Number of samples generated.

        Debug.Assert(left + right + 1 == numberOfSamples, "Wrong number of samples?");

        float weight;         // The weight of the current sample.
        float weightSum = 0;  // The sum of all weights (for normalization).

        // Samples on the left.
        for (int i = -left; i <= -1; i++)
        {
          Vector2 offset = new Vector2(2 * i + 0.5f, 0);

          // Get the offsets and weights of the neighboring pixel centers.
          var o0 = offset.X - 0.5f;
          var o1 = offset.X + 0.5f;
          var w0 = MathHelper.Gaussian(o0, coefficient, 0, standardDeviation);
          var w1 = MathHelper.Gaussian(o1, coefficient, 0, standardDeviation);

          // Shift the offset to the pixel center that has the higher weight.
          offset.X = (o0 * w0 + o1 * w1) / (w0 + w1);

          Offsets[count] = offset;
          weight = w0 + w1;
          Weights[count] = weight;
          weightSum += weight;
          count++;
        }

        // Center sample.
        Offsets[count] = new Vector2(0, 0);
        weight = MathHelper.Gaussian(0, coefficient, 0, standardDeviation);
        Weights[count] = weight;
        weightSum += weight;
        count++;

        // Samples on the right.
        for (int i = 1; i <= right; i++)
        {
          Vector2 offset = new Vector2(2 * i - 0.5f, 0);

          // Get the offsets and weights of the neighboring pixel centers.
          var o0 = offset.X - 0.5f;
          var o1 = offset.X + 0.5f;
          var w0 = MathHelper.Gaussian(o0, coefficient, 0, standardDeviation);
          var w1 = MathHelper.Gaussian(o1, coefficient, 0, standardDeviation);

          // Shift the offset to the pixel center that has the higher weight.
          offset.X = (o0 * w0 + o1 * w1) / (w0 + w1);

          Offsets[count] = offset;
          weight = w0 + w1;
          Weights[count] = weight;
          weightSum += weight;
          count++;
        }

        // Normalize weights.
        for (int i = 0; i < numberOfSamples; i++)
          Weights[i] /= weightSum;

        Debug.Assert(count == numberOfSamples, "Wrong number of samples generated?");
        Debug.Assert(Numeric.AreEqual(Weights.Take(numberOfSamples).Sum(), 1.0f), "Invalid sample weights.");
      }
      else
      {
        // Sample in the middle of pixels.

        /* 
          // ----- Unordered samples
          Offsets[0] = new Vector2(0, 0);
          Weights[0] = MathHelper.Gaussian(0, coefficient, 0, standardDeviation);
          float weightSum = Weights[0];
          for (int i = 1; i < numberOfSamples; i += 2)
          {
            var offset = new Vector2(1 + i / 2, 0);
            Offsets[i] = offset;
            Offsets[i + 1] = -offset;
            Weights[i] = MathHelper.Gaussian(offset.X, coefficient, 0, standardDeviation);
            Weights[i + 1] = Weights[i];
            weightSum += (Weights[i] * 2);
          }

          // Normalize weights.
          for (int i = 0; i < numberOfSamples; i++)
            Weights[i] /= weightSum;
        */

        // ----- Ordered samples
        int left = (numberOfSamples - 1) / 2; // Number of samples on the left.
        int right = numberOfSamples / 2;      // Number of samples on the right.
        int count = 0;                        // Number of samples generated.

        Debug.Assert(left + right + 1 == numberOfSamples, "Wrong number of samples?");

        float weight;         // The weight of the current sample.
        float weightSum = 0;  // The sum of all weights (for normalization).

        // Samples on the left.
        for (int i = -left; i <= -1; i++)
        {
          Offsets[count] = new Vector2(i, 0);
          weight = MathHelper.Gaussian(i, coefficient, 0, standardDeviation);
          Weights[count] = weight;
          weightSum += weight;
          count++;
        }

        // Center sample.
        Offsets[count] = new Vector2(0, 0);
        weight = MathHelper.Gaussian(0, coefficient, 0, standardDeviation);
        Weights[count] = weight;
        weightSum += weight;
        count++;

        // Samples on the right.
        for (int i = 1; i <= right; i++)
        {
          Offsets[count] = new Vector2(i, 0);
          weight = MathHelper.Gaussian(i, coefficient, 0, standardDeviation);
          Weights[count] = weight;
          weightSum += weight;
          count++;
        }

        // Normalize weights.
        for (int i = 0; i < numberOfSamples; i++)
          Weights[i] /= weightSum;

        Debug.Assert(count == numberOfSamples, "Wrong number of samples generated?");
        Debug.Assert(Numeric.AreEqual(Weights.Take(numberOfSamples).Sum(), 1.0f), "Invalid sample weights.");
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var renderTargetPool = GraphicsService.RenderTargetPool;

      var viewport = context.Viewport;
      Vector2 size = new Vector2(viewport.Width, viewport.Height);

      // Choose suitable technique.
      // We do not have shader for each sample count. 
      int numberOfSamples = NumberOfSamples;
      SetCurrentTechnique(ref numberOfSamples);

      // Apply current scale and texture size to offsets.
      for (int i = 0; i < NumberOfSamples; i++)
      {
        _horizontalOffsets[i].X = Offsets[i].X * Scale / size.X;
        _horizontalOffsets[i].Y = Offsets[i].Y * Scale / size.Y;
      }

      // Make sure the other samples are 0 (e.g. if we want 11 samples but the 
      // next best shader supports only 15 samples).
      for (int i = NumberOfSamples; i < numberOfSamples; i++)
      {
        _horizontalOffsets[i].X = 0;
        _horizontalOffsets[i].Y = 0;
        Weights[i] = 0;
      }

      // If we have a separable filter, we initialize _verticalOffsets too.
      if (IsSeparable)
      {
        if (_verticalOffsets == null)
          _verticalOffsets = new Vector2[MaxNumberOfSamples];

        float aspectRatio = size.X / size.Y;
        for (int i = 0; i < NumberOfSamples; i++)
        {
          _verticalOffsets[i].X = _horizontalOffsets[i].Y * aspectRatio;
          _verticalOffsets[i].Y = _horizontalOffsets[i].X * aspectRatio;
        }
        for (int i = NumberOfSamples; i < numberOfSamples; i++)
        {
          _verticalOffsets[i].X = 0;
          _verticalOffsets[i].Y = 0;
        }
      }

      // Use hardware filtering if possible.
      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      bool isAnisotropic = IsAnisotropic;
      bool isBilateral = IsBilateral;
      if (FilterInLogSpace)
      {
        // Anisotropic and bilateral filtering in log-space is not implemented.
        isAnisotropic = false;
        isBilateral = false;
      }
      else
      {
        if (isAnisotropic || isBilateral)
        {
          context.ThrowIfCameraMissing();

          var cameraNode = context.CameraNode;
          var projection = cameraNode.Camera.Projection;
          float far = projection.Far;

          GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, _frustumFarCorners);
          _parameterFrustumCorners.SetValue(_frustumFarCorners);

          _parameterBlurParameters0.SetValue(new Vector4(
            far,
            viewport.AspectRatio,
            1.0f / (EdgeSoftness + 0.001f) * far,
            DepthScaling));

          context.ThrowIfGBuffer0Missing();
          Texture2D depthBuffer = context.GBuffer0;
          if (viewport.Width < depthBuffer.Width && viewport.Height < depthBuffer.Height)
          {
            // Use half-resolution depth buffer.
            object obj;
            if (context.Data.TryGetValue(RenderContextKeys.DepthBufferHalf, out obj))
            {
              var depthBufferHalf = obj as Texture2D;
              if (depthBufferHalf != null)
                depthBuffer = depthBufferHalf;
            }
          }

          _parameterGBuffer0.SetValue(depthBuffer);
        }
      }

      _parameterViewportSize.SetValue(size);
      _parameterWeights.SetValue(Weights);

      int effectiveNumberOfPasses = IsSeparable ? NumberOfPasses * 2 : NumberOfPasses;

      // We use up to two temporary render targets for ping-ponging.
      var tempFormat = new RenderTargetFormat((int)size.X, (int)size.Y, false, context.SourceTexture.Format, DepthFormat.None);
      var tempTarget0 = (effectiveNumberOfPasses > 1)
                        ? renderTargetPool.Obtain2D(tempFormat)
                        : null;
      var tempTarget1 = (effectiveNumberOfPasses > 2)
                        ? renderTargetPool.Obtain2D(tempFormat)
                        : null;

      for (int i = 0; i < effectiveNumberOfPasses; i++)
      {
        if (i == effectiveNumberOfPasses - 1)
        {
          graphicsDevice.SetRenderTarget(context.RenderTarget);
          graphicsDevice.Viewport = viewport;
        }
        else if (i % 2 == 0)
        {
          graphicsDevice.SetRenderTarget(tempTarget0);
        }
        else
        {
          graphicsDevice.SetRenderTarget(tempTarget1);
        }

        if (i == 0)
          _parameterSourceTexture.SetValue(context.SourceTexture);
        else if (i % 2 == 0)
          _parameterSourceTexture.SetValue(tempTarget1);
        else
          _parameterSourceTexture.SetValue(tempTarget0);

        Vector2[] offsets;
        if (IsSeparable && i % 2 != 0
            && !isAnisotropic) // The anisotropic filter only reads Offsets[i].x
        {
          offsets = _verticalOffsets;
        }
        else
        {
          offsets = _horizontalOffsets;
        }

        _parameterOffsets.SetValue(offsets);

        int passIndex = 0;
        if (isAnisotropic)
          passIndex = i % 2;

        _effect.CurrentTechnique.Passes[passIndex].Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      _parameterSourceTexture.SetValue((Texture2D)null);

      renderTargetPool.Recycle(tempTarget0);
      renderTargetPool.Recycle(tempTarget1);
    }


    // Chooses a suitable effect technique. The number of samples is "rounded up" to the next 
    // suitable shader. See the list of available techniques in Blur.fx.
    private void SetCurrentTechnique(ref int numberOfSamples)
    {
      if (numberOfSamples % 2 == 0)
        numberOfSamples++;

      if (numberOfSamples < 3)
      {
        numberOfSamples = 3;
      }
      else if (numberOfSamples > 9)
      {
        if (numberOfSamples < 15)
          numberOfSamples = 15;
        else if (numberOfSamples > 15 && numberOfSamples < 23)
          numberOfSamples = 23;
      }

      const int TechniquesPerMode = 6;
      int shaderNumber;
      switch (numberOfSamples)
      {
        case 3: shaderNumber = 0; break;
        case 5: shaderNumber = 1; break;
        case 7: shaderNumber = 2; break;
        case 9: shaderNumber = 3; break;
        case 15: shaderNumber = 4; break;
        default: shaderNumber = 5; break;
      }

      EffectTechnique technique;
      if (!IsAnisotropic)
      {
        if (FilterInLogSpace)
        {
          technique = _effect.Techniques[2 * TechniquesPerMode + shaderNumber];
          Debug.Assert(technique.Name == "Logarithmic" + numberOfSamples);
        }

        if (!IsBilateral)
        {
          technique = _effect.Techniques[shaderNumber];
          Debug.Assert(technique.Name == "Normal" + numberOfSamples);
        }
        else
        {
          technique = _effect.Techniques[TechniquesPerMode + shaderNumber];
          Debug.Assert(technique.Name == "Bilateral" + numberOfSamples);
        }
      }
      else
      {
        if (!IsBilateral)
        {
          technique = _effect.Techniques[3 * TechniquesPerMode + shaderNumber];
          Debug.Assert(technique.Name == "Anisotropic" + numberOfSamples);
        }
        else
        {
          technique = _effect.Techniques[4 * TechniquesPerMode + shaderNumber];
          Debug.Assert(technique.Name == "AnisotropicBilateral" + numberOfSamples);
        }
      }

      _effect.CurrentTechnique = technique;
    }
    #endregion
  }
}
#endif
