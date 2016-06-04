// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Renders the sky using atmospheric scattering.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This shader computes atmospheric scattering on the GPU, which is expensive. It is not 
  /// recommended to render the <see cref="ScatteringSkyNode"/> every frame. It is better to render 
  /// the sky into a cube map and update the cube map when necessary. 
  /// </para>
  /// </remarks>
  public class ScatteringSkyNode : SkyNode
  {
    // Notes:
    //
    // Beta: 
    // Rayleigh scattering is multiplied with 1/waveLength^4.
    // Some implementations multiply Mie scattering with 1/waveLength^0.84. 
    // Our beta coefficients also contain the 4*Pi, which occurs in some formulas.
    // For example: 
    // O'Neil(?): BetaRayleigh = 4 * Pi * 0.0025f.xxx / float3(pow(0.65, 4), pow(0.57, 4), pow(0.475, 4));
    // Bruneton: BetaRayleigh = (5.8, 13.5, 33.1) * 10^-6 for wavelengths: 680, 550, 440
    //           BetaMie = 2 * 10^-5
    //
    // We use the same scale height (e.g. 0.25%) for Rayleigh and Mie. Some implementations use a 
    // different scale height for Mie (for example 0.1%). This make sense because Mie represents 
    // the big particles which could gather in lower heights (smoke, fog, dust, ...).
    // O'Neil's Scale function assumes that the atmosphere height is 2.5 % of the planet radius.
    //
    // See Atmosphere.fxh for a description and comments of the scattering functions.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the direction to the sun.
    /// </summary>
    /// <value>The direction to the sun. This vector is automatically normalized.</value>
    public Vector3F SunDirection
    {
      get { return _sunDirection; }
      set
      {
        _sunDirection = value;
        _sunDirection.TryNormalize();
      }
    }
    private Vector3F _sunDirection;


    /// <summary>
    /// Gets or sets the color of the sun light (outside the atmosphere).
    /// </summary>
    /// <value>
    /// The color of the sun light (outside the atmosphere). The default value is (1, 1, 1).
    /// Non-white sun colors can be used for dramatic effects or alien planet.
    /// </value>
    public Vector3F SunColor { get; set; }


    /// <summary>
    /// Gets or sets the intensity of the sun light.
    /// </summary>
    /// <value>The intensity of the sun light.</value>
    public float SunIntensity { get; set; }


    /// <summary>
    /// Gets or sets the radius of the planet ground level.
    /// </summary>
    /// <value>The planet ground radius in [m]. The default value is 6360e3 m.</value>
    public float PlanetRadius { get; set; }


    /// <summary>
    /// Gets or sets the height of the atmosphere.
    /// </summary>
    /// <value>
    /// The height of the atmosphere (= the distance from the ground to the "top" of the atmosphere)
    /// in [m].
    /// </value>
    public float AtmosphereHeight { get; set; }


    /// <summary>
    /// Gets or sets the altitude (height above the ground) of the observer.
    /// </summary>
    /// <value>The observer altitude in [m].</value>
    public float ObserverAltitude { get; set; }


    /// <summary>
    /// Gets or sets the scale height which is the altitude (height above ground) where the average
    /// atmospheric density is found.
    /// </summary>
    /// <value>The scale height in [m].</value>
    public float ScaleHeight { get; set; }


    /// <summary>
    /// Gets or sets the number of samples used to compute the atmospheric scattering in the shader.
    /// </summary>
    /// <value>
    /// The number of samples used to compute the atmospheric scattering in the shader. The default
    /// value is 5.
    /// </value>
    public int NumberOfSamples { get; set; }


    /// <summary>
    /// Gets or sets the scatter/extinction coefficients for Rayleigh scattering.
    /// </summary>
    /// <value>The scatter/extinction coefficients for Rayleigh scattering.</value>
    public Vector3F BetaRayleigh { get; set; }


    /// <summary>
    /// Gets or sets the scatter/extinction coefficients for Mie scattering.
    /// </summary>
    /// <value>The scatter/extinction coefficients for Mie scattering.</value>
    public Vector3F BetaMie { get; set; }


    /// <summary>
    /// Gets or sets the scattering symmetry constant g for Mie scattering.
    /// </summary>
    /// <value>
    /// The scattering symmetry constant g for Mie scattering in the range ]-1, 1[. 
    /// The default value is 0.75.
    /// </value>
    /// <remarks>
    /// Positive values create forward scattering. Negative values create backward scattering. g is
    /// usually in [0.75, 0.999].
    /// </remarks>
    public float GMie { get; set; }


    /// <summary>
    /// Gets or sets the transmittance of the sky.
    /// </summary>
    /// <value>The transmittance of the sky. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// The transmittance defines how much light from outside the atmosphere is attenuated. If the
    /// transmittance is 1, then sky colors are added to the lights from outer space. If the
    /// transmittance is 0, then the sky colors replace the colors from outer space. 
    /// </para>
    /// <para>
    /// The actual sky transmittance is computed in the shader. The value of 
    /// <see cref="Transmittance"/> is combined with the computed value from the shader.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public float Transmittance { get; set; }


    ///// <summary>
    ///// Gets the turbidity.
    ///// </summary>
    ///// <value>The turbidity.</value>
    ///// <remarks>
    ///// The turbidity measures how polluted the air is. A turbidity of 2 describes a clear day 
    ///// whereas a turbidity of 20 represents thick haze.
    ///// </remarks>
    //public float Turbidity
    //{
    //  get
    //  {
    //    // Turbidity should be t = (OpticalThickness(Rayleigh) + OpticalThickness(Mie))/OpticalThickness(Rayleigh)
    //    // We use the same scale height for Rayleigh and Mie. That means, our optical depths are the same.
    //    // I think for some the extinction coefficients are part of the optical thickness, therefore:
    //    var turbidity = (BetaRayleigh + BetaMie) / BetaRayleigh;

    //    // Note: Since we use the same scale height for Rayleigh and Mie, we have to use
    //    // very low BetaMie - therefore our Turbidity values are near 1 and not really helpful.

    //    // turbidity is a vector. Lets return the average.
    //    return (turbidity.X + turbidity.Y + turbidity.Z) / 3;
    //  }
    //}


    /// <summary>
    /// Gets or sets the color at the horizon when there is no sunlight.
    /// </summary>
    /// <value>
    /// The color at the horizon when there is no sunlight.
    /// The default value is (0, 0, 0).
    /// </value>
    /// <remarks>
    /// <see cref="BaseHorizonColor"/>, <see cref="BaseZenithColor"/> and 
    /// <see cref="BaseColorShift"/> create a color gradient in the sky which is added to the 
    /// result of the atmospheric scattering computations. These properties basically define the
    /// color of the sky in sunless nights: 
    /// The sky at the horizon will have the <see cref="BaseHorizonColor"/>. The zenith will
    /// have the <see cref="BaseZenithColor"/>. All other base sky colors are interpolated 
    /// between those two colors. <see cref="BaseColorShift"/> determines where the sky color
    /// is exactly the average of both colors. If <see cref="BaseColorShift"/> is 0.5, the average 
    /// color of the gradient is in the middle of the top hemisphere. If this value is less than 
    /// 0.5, then the average color is shifted down to the horizon. If this value is greater than 
    /// 0.5, then the average color is shifted up to the zenith.
    /// </remarks>
    public Vector3F BaseHorizonColor { get; set; }


    /// <summary>
    /// Gets or sets the color at the zenith when there is no sunlight.
    /// </summary>
    /// <value>
    /// The color at the zenith when there is no sunlight.
    /// The default value is (0, 0, 0).
    /// </value>
    /// <inheritdoc cref="BaseHorizonColor"/>
    public Vector3F BaseZenithColor { get; set; }

    
    /// <summary>
    /// Gets or sets the relative height where the base sky color is the average of the
    /// <see cref="BaseHorizonColor"/> and the <see cref="BaseZenithColor"/>.
    /// </summary>
    /// <value>
    /// The relative height where the base sky color is the average of the 
    /// <see cref="BaseHorizonColor"/> and the <see cref="BaseZenithColor"/>, in the range [0, 1]. 
    /// The default value is 0.5.
    /// </value>
    /// <inheritdoc cref="BaseHorizonColor"/>
    public float BaseColorShift { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatteringSkyNode" /> class.
    /// </summary>
    public ScatteringSkyNode()
    {
      SunDirection = new Vector3F(0, 1, 0);
      SunColor = new Vector3F(1);
      SunIntensity = 5;
      PlanetRadius = 6360e3f;
      AtmosphereHeight = 160e3f;
      ObserverAltitude = 300;
      ScaleHeight = 15e3f;
      NumberOfSamples = 5;

      //BetaRayleigh = new Vector3F(5.8e-6f, 13.5e-6f, 23.1e-6f);
      BetaRayleigh = new Vector3F(6.95e-6f, 11.8e-6f, 24.4e-6f);
      BetaMie = new Vector3F(2e-5f);
      // Since we do not have a separate ScaleHeight for Mie, our Mie coefficients
      // must be lower than in nature!
      BetaMie /= 40;

      GMie = 0.75f;      // Use 0.99 to create a sun disk procedurally.

      Transmittance = 1;

      BaseColorShift = 0.5f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new ScatteringSkyNode Clone()
    {
      return (ScatteringSkyNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new ScatteringSkyNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SkyNode properties.
      base.CloneCore(source);

      // Clone ScatteringSkyNode properties.
      var sourceTyped = (ScatteringSkyNode)source;
      SunDirection = sourceTyped.SunDirection;
      SunColor = sourceTyped.SunColor;
      SunIntensity = sourceTyped.SunIntensity;
      PlanetRadius = sourceTyped.PlanetRadius;
      AtmosphereHeight = sourceTyped.AtmosphereHeight;
      ObserverAltitude = sourceTyped.ObserverAltitude;
      ScaleHeight = sourceTyped.ScaleHeight;
      NumberOfSamples = sourceTyped.NumberOfSamples;
      BetaRayleigh = sourceTyped.BetaRayleigh;
      BetaMie = sourceTyped.BetaMie;
      GMie = sourceTyped.GMie;
      Transmittance = sourceTyped.Transmittance;
      BaseHorizonColor = sourceTyped.BaseHorizonColor;
      BaseZenithColor = sourceTyped.BaseZenithColor;
      BaseColorShift = sourceTyped.BaseColorShift;
    }


    private static float HitSphere(Vector3F rayOrigin, Vector3F rayDirection, float radius, out float enter, out float exit)
    {
      // Solve the equation:  ||rayOrigin + distance * rayDirection|| = r
      //
      // This is a straight-forward quadratic equation:
      //   ||O + d * D|| = r
      //   =>  (O + d * D)² = r²  where V² means V.V
      //   =>  d² * D² + 2 * d * (O.D) + O² - r² = 0
      // D² is 1 because the rayDirection is normalized.
      //   =>  d = -O.D + sqrt((O.D)² - O² + r²)

      float OD = Vector3F.Dot(rayOrigin, rayDirection);
      float OO = Vector3F.Dot(rayOrigin, rayOrigin);
      float radicand = OD * OD - OO + radius * radius;
      enter = Math.Max(0, -OD - (float)Math.Sqrt(radicand));
      exit = -OD + (float)Math.Sqrt(radicand);

      return radicand;  // If radicand is negative then we do not have a result - no hit.
    }


    private static float ChapmanApproximation(float X, float h, float cosZenithAngle)
    {
      float c = (float)Math.Sqrt(X + h);
      if (cosZenithAngle >= 0)
      {
        return c / (c * cosZenithAngle + 1) * (float)Math.Exp(-h);
      }
      else
      {
        float x0 = (float)Math.Sqrt(1 - cosZenithAngle * cosZenithAngle) * (X + h);
        float c0 = (float)Math.Sqrt(x0);
        return 2 * c0 * (float)Math.Exp(X - x0) - c / (1 - c * cosZenithAngle) * (float)Math.Exp(-h);
      }
    }


    private static float GetOpticalDepthSchueler(float h, float H, float radiusGround, float cosZenithAngle)
    {
      return H * ChapmanApproximation(radiusGround / H, h / H, cosZenithAngle);
    }


    /// <summary>
    /// Gets the transmittance for a specified view direction.
    /// </summary>
    /// <param name="viewDirection">The view direction.</param>
    /// <returns>The transmittance.</returns>
    /// <remarks>
    /// This method assumes that the observer is looking into the sky. It returns 0 below the
    /// horizon.
    /// </remarks>
    public Vector3F GetTransmittance(Vector3F viewDirection)
    {
      float cosZenith = viewDirection.Y;
      if (cosZenith < 0)
        return Vector3F.Zero;

      Vector3F beta = BetaRayleigh + BetaMie;
      float opticalDepth = GetOpticalDepthSchueler(ObserverAltitude, ScaleHeight, PlanetRadius, cosZenith);
      return Exp(-opticalDepth * beta);
    }


    private static float PhaseFunctionRayleigh(float cosTheta)
    {
      float cosThetaSquared = cosTheta * cosTheta;
      return 3.0f / (16.0f * ConstantsF.Pi) * (1 + cosThetaSquared);
    }


    private static float PhaseFunction(float cosTheta, float g)
    {
      float gSquared = g * g;
      float cosThetaSquared = cosTheta * cosTheta;

      return 3 / (8 * ConstantsF.Pi)
             * ((1.0f - gSquared) / (2.0f + gSquared))
             * (1.0f + cosThetaSquared)
             / (float)Math.Pow(1.0f + gSquared - 2.0f * g * cosTheta, 1.5f);
    }


    private void ComputeScattering(Vector3F viewDirection, bool applyPhaseFunction,
                                   out Vector3F transmittance, 
                                   out Vector3F colorRayleigh, 
                                   out Vector3F colorMie)
    {
      if (viewDirection.Y < 0)
      {
        transmittance = Vector3F.Zero;
        colorRayleigh = Vector3F.Zero;
        colorMie = Vector3F.Zero;
        return;
      }

      float dummy, rayLength;
      var rayStart = new Vector3F(0, PlanetRadius + ObserverAltitude, 0);
      HitSphere(rayStart, viewDirection, PlanetRadius + AtmosphereHeight, out dummy, out rayLength);

      float neg = /*hitGround ? -1 :*/ 1;

      var rayEnd = rayStart + viewDirection * rayLength;
      float radiusEnd = rayEnd.Length;

      var zenith = rayEnd / radiusEnd;

      // Altitude of ray end.
      float h = radiusEnd - PlanetRadius;

      // Optical depth of ray end (which is the sky or the terrain).
      float cosRay = Vector3F.Dot(zenith, neg * viewDirection);
      float lastRayDepth = GetOpticalDepthSchueler(h, ScaleHeight, PlanetRadius, cosRay);

      // Optical depth of ray end to sun.
      float cosSun = Vector3F.Dot(zenith, SunDirection);
      float lastSunDepth = GetOpticalDepthSchueler(h, ScaleHeight, PlanetRadius, cosSun);

      float segmentLength = rayLength / NumberOfSamples;
      var T = new Vector3F(1, 1, 1);   // The ray transmittance (camera to sky/terrain).
      var S = new Vector3F(0, 0, 0);   // The inscattered light.
      for (int i = NumberOfSamples - 1; i >= 0; i--)
      {
        var samplePoint = rayStart + i * segmentLength * viewDirection;
        float radius = samplePoint.Length;
        zenith = samplePoint / radius;

        h = radius - PlanetRadius;

        cosRay = Vector3F.Dot(zenith, neg * viewDirection);
        float sampleRayDepth = GetOpticalDepthSchueler(h, ScaleHeight, PlanetRadius, cosRay);
        float segmentDepth = neg * (sampleRayDepth - lastRayDepth);
        var segmentT = Exp(-segmentDepth * (BetaRayleigh + BetaMie));

        cosSun = Vector3F.Dot(zenith, SunDirection);
        float sampleSunDepth = GetOpticalDepthSchueler(h, ScaleHeight, PlanetRadius, cosSun);
        float segmentSunDepth = 0.5f * (sampleSunDepth + lastSunDepth);
        var segmentS = Exp(-segmentSunDepth * (BetaRayleigh + BetaMie));

        //if (segmentT.IsNaN)
        //  segmentT = Vector3F.Zero;
        if (segmentS.IsNaN)
          segmentS = Vector3F.Zero;

        S = S * segmentT;
        S += (float)Math.Exp(-h / ScaleHeight) * segmentLength * segmentS;
        T = T * segmentT;

        lastRayDepth = sampleRayDepth;
        lastSunDepth = sampleSunDepth;
      }

      transmittance = T;

      colorRayleigh = S * BetaRayleigh;
      colorMie = S * BetaMie;

      transmittance *= Transmittance;

      if (applyPhaseFunction)
      {
        float cosTheta = Vector3F.Dot(SunDirection, viewDirection);
        colorRayleigh *= PhaseFunctionRayleigh(cosTheta);
        colorMie *= PhaseFunction(cosTheta, GMie);
      }

      colorRayleigh *= SunIntensity;
      colorMie *= SunIntensity;
    }


    /// <summary>
    /// Gets the sunlight.
    /// </summary>
    /// <returns>The intensity of the direct sunlight.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public Vector3F GetSunlight()
    {
      if (SunDirection.Y >= 0)
      {
        //----- Sun is above horizon.
        return GetTransmittance(SunDirection) * SunIntensity;
      }
      
      //----- Sun is below horizon.
      // Get angle.
      float sunAngle = (float)MathHelper.ToDegrees(Math.Asin(SunDirection.Y));
      if (sunAngle < -5)
        return Vector3F.Zero;

      // Sample horizon instead of real direction.
      Vector3F direction = SunDirection;
      direction.Y = 0;
      if (!direction.TryNormalize())
        return Vector3F.Zero;

      Vector3F horizonSunlight = GetTransmittance(direction) * SunIntensity;

      // Lerp horizon sunlight to 0 at -5°.
      float f = 1 - MathHelper.Clamp(-sunAngle / 5.0f, 0, 1);
      return horizonSunlight * f;
    }


    /// <summary>
    /// Approximates the ambient light by sampling the sky.
    /// </summary>
    /// <param name="numberOfSamples">The number of samples.</param>
    /// <returns>The ambient light created by the sky.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
    public Vector3F GetAmbientLight(int numberOfSamples)
    {
      var random = new Random(1234567);
      var sphereDistribution = new SphereDistribution
      {
        Center = Vector3F.Zero,
        InnerRadius = 1,
        OuterRadius = 1,
      };

      Vector3F ambient = new Vector3F();
      for (int i = numberOfSamples - 1; i >= 0; i--)
      {
        Vector3F sampleDirection = sphereDistribution.Next(random);
        if (sampleDirection.Y < 0)
          sampleDirection.Y *= -1;

        Vector3F transmittance;
        Vector3F colorR, colorM;
        ComputeScattering(sampleDirection, true, out transmittance, out colorR, out colorM);
        Debug.Assert(sampleDirection.IsNumericallyNormalized);

        Vector3F sample = colorR + colorM + GetBaseColor(sampleDirection);
        if (sample.IsNaN)
          numberOfSamples--;  // Ignore sample.
        else
          ambient += sample;
      }

      ambient /= numberOfSamples;

      // We have added up luminance, now we have to multiply by the solid angle 
      // of the hemisphere to get illuminance.
      return ambient * ConstantsF.TwoPi;
    }


    private Vector3F GetBaseColor(Vector3F direction)
    {
      // 0 = zenith, 1 = horizon
      float p = 1 - MathHelper.Clamp(
        (float)Math.Acos(direction.Y) / ConstantsF.Pi * 2, 
        0, 
        1); 

      var colorAverage = (BaseHorizonColor + BaseZenithColor) / 2;
      if (p < BaseColorShift)
        return InterpolationHelper.Lerp(BaseHorizonColor, colorAverage, p / BaseColorShift);
      else
        return InterpolationHelper.Lerp(colorAverage, BaseZenithColor, (p - BaseColorShift) / (1 - BaseColorShift));
    }


    /// <overloads>
    /// <summary>
    /// Approximates the <see cref="Fog"/> color by sampling the sky horizon colors.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Approximates the <see cref="Fog"/> color by sampling the sky horizon colors.
    /// </summary>
    /// <param name="numberOfSamples">The number of samples.</param>
    /// <returns>The fog color.</returns>
    /// <remarks>
    /// You might need to desaturate the fog color.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
    public Vector3F GetFogColor(int numberOfSamples)
    {
      return GetFogColor(numberOfSamples, 0);
    }


    /// <summary>
    /// Approximates the <see cref="Fog" /> color by sampling the sky horizon colors.
    /// </summary>
    /// <param name="numberOfSamples">The number of samples.</param>
    /// <param name="elevation">
    /// The elevation angle at which to sample. The angle is specified in radians and is usually
    /// in the range [0, π/2]. Use 0 to sample exactly at the horizon. Use positive values to sample 
    /// above the horizon.
    /// </param>
    /// <returns>The fog color.</returns>
    /// <remarks>You might need to desaturate the fog color.</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow")]
    public Vector3F GetFogColor(int numberOfSamples, float elevation)
    {
      var forward = new Vector3F((float)Math.Cos(elevation), (float)Math.Sin(elevation), 0);
      Debug.Assert(forward.IsNumericallyNormalized);

      var color = new Vector3F();
      for (int i = numberOfSamples - 1; i >= 0; i--)
      {
        Vector3F sampleDirection = QuaternionF.CreateRotationY(ConstantsF.TwoPi / numberOfSamples).Rotate(forward);

        // Note: Crysis computes fog color without phase function and applies the phase function
        // in the fog shader. The color difference with and without phase function seems to be
        // negligible. The intensity will be about 7 to 12 times lower with the phase function
        // (about 7 when the sun is near the horizon, about 12 when the sun is at the zenith).
        // We use the phase function because our fog shader might not apply a phase function.
        // And if it does apply the phase function, the phase function is normalized to keep the
        // average fog brightness constant.
        const bool usePhaseFunction = true;

        Vector3F transmittance;
        Vector3F colorR, colorM;
        ComputeScattering(sampleDirection, usePhaseFunction, out transmittance, out colorR, out colorM);
        Debug.Assert(sampleDirection.IsNumericallyNormalized);

        Vector3F sample = colorR + colorM + GetBaseColor(sampleDirection);
        if (sample.IsNaN)
          numberOfSamples--;  // Ignore sample.
        else
          color += sample;
      }

      color /= numberOfSamples;
      return color;
    }


    private static Vector3F Exp(Vector3F value)
    {
      return new Vector3F((float)Math.Exp(value.X), (float)Math.Exp(value.Y), (float)Math.Exp(value.Z));
    }
    #endregion
  }
}
