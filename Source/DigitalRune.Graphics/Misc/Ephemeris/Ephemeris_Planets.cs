// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
// The ephemeris model is based on:
//
//    "Physically-Based Outdoor Scene Lighting", by Frank Kane (Founder of Sundog Software, LLC),
//    Game Engine Gems 1.
//
//    Copyright (c) 2004-2008  Sundog Software, LLC. All rights reserved worldwide.
//
// Code is used with permission from Frank Kane.
#endregion

using DigitalRune.Mathematics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// The planets which are visible to the naked eye.
  /// </summary>
  internal enum VisiblePlanets
  {
    Mercury,
    Venus,
    Earth,
    Mars,
    Jupiter,
    Saturn,
  };


  partial class Ephemeris
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Stores the parameters required to uniquely identify a specific planet orbit.
    private struct OrbitalElements
    {
      public double Period;
      public double EpochLongitude;
      public double PerihelionLongitude;
      public double Eccentricity;
      public double SemiMajorAxis;
      public double Inclination;
      public double LongitudeAscendingNode;
      public double AngularDiameter;
      public double VisualMagnitude;


      public OrbitalElements(double period, double epochLongitude, double perihelionLongitude,
        double eccentricity, double semiMajorAxis, double inclination, double longitudeAscendingNode,
        double angularDiameter, double visualMagnitude)
      {
        Period = period;
        EpochLongitude = epochLongitude;
        PerihelionLongitude = perihelionLongitude;
        Eccentricity = eccentricity;
        SemiMajorAxis = semiMajorAxis;
        Inclination = inclination;
        LongitudeAscendingNode = longitudeAscendingNode;
        AngularDiameter = angularDiameter;
        VisualMagnitude = visualMagnitude;
      }
    };


    //// Stores the calculated position and brightness of a planet.
    //private struct PlanetData
    //{
    //  // Equatorial coordinates.
    //  public double RightAscension;
    //  public double Declination;

    //  // Measures the brightness of the planet as seen on earth. Lower values mean more brightness.
    //  // (I think this is the same as "apparent magnitude".)
    //  public double VisualMagnitude;
    //};
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    //private const int NumberOfPlanets = 6;

    // The orbits of the visible planets.
    private readonly OrbitalElements[] _planetElements =
    {
      // Mercury
      new OrbitalElements(0.240852, MathHelper.ToRadians(60.750646), MathHelper.ToRadians(77.299833), 
        0.205633, 0.387099, MathHelper.ToRadians(7.004540), MathHelper.ToRadians(48.212740), 6.74, -0.42),

      // Venus
      new OrbitalElements(0.615211, MathHelper.ToRadians(88.455855), MathHelper.ToRadians(131.430236), 
        0.006778, 0.723332, MathHelper.ToRadians(3.394535), MathHelper.ToRadians(76.589820), 16.92, -4.40),

      // Earth
      new OrbitalElements(1.00004, MathHelper.ToRadians(99.403308), MathHelper.ToRadians(102.768413), 
        0.016713, 1.00000, 0, 0, 0, 0),

      // Mars
      new OrbitalElements(1.880932, MathHelper.ToRadians(240.739474), MathHelper.ToRadians(335.874939), 
        0.093396, 1.523688, MathHelper.ToRadians(1.849736), MathHelper.ToRadians(49.480308), 9.36, -1.52),

      // Jupiter
      new OrbitalElements(11.863075, MathHelper.ToRadians(90.638185), MathHelper.ToRadians(14.170747), 
        0.048482, 5.202561, MathHelper.ToRadians(1.303613), MathHelper.ToRadians(100.353142), 196.74, -9.40),

      // Saturn
      new OrbitalElements(29.471362, MathHelper.ToRadians(287.690033), MathHelper.ToRadians(92.861407), 
        0.055581, 9.554747, MathHelper.ToRadians(2.488980), MathHelper.ToRadians(113.576139), 165.60, -8.88)
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    //private readonly PlanetData[] _planetData = new PlanetData[NumberOfPlanets];
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    ///// <summary>
    ///// Computes the planet data of the specified planet.
    ///// </summary>
    ///// <param name="planet">The planet.</param>
    ///// <remarks>
    ///// <see cref="ComputeEarthPosition"/> must have been called before this method!
    ///// </remarks>
    //void ComputePlanetData(VisiblePlanets planet)
    //{
    //  int index = (int)planet;

    //  double Np = ((2.0 * ConstantsD.Pi) / 365.242191) * (_epoch1990Days / planetElements[index].Period);
    //  Np = InRange(Np);

    //  double Mp = Np + planetElements[index].EpochLongitude - planetElements[index].PerihelionLongitude;

    //  double heliocentricLongitude = Np + 2.0 * planetElements[index].Eccentricity * Math.Sin(Mp) +
    //                                 planetElements[index].EpochLongitude;
    //  heliocentricLongitude = InRange(heliocentricLongitude);

    //  double vp = heliocentricLongitude - planetElements[index].PerihelionLongitude;

    //  double radius = (planetElements[index].SemiMajorAxis
    //              * (1.0 - planetElements[index].Eccentricity * planetElements[index].Eccentricity))
    //           / (1.0 + planetElements[index].Eccentricity * Math.Cos(vp));

    //  double heliocentricLatitude = Math.Asin(Math.Sin(heliocentricLongitude -
    //                                         planetElements[index].LongitudeAscendingNode) * Math.Sin(planetElements[index].Inclination));
    //  heliocentricLatitude = InRange(heliocentricLatitude);

    //  double y = Math.Sin(heliocentricLongitude - planetElements[index].LongitudeAscendingNode) *
    //             Math.Cos(planetElements[index].Inclination);
    //  double x = Math.Cos(heliocentricLongitude - planetElements[index].LongitudeAscendingNode);

    //  double projectedHeliocentricLongitude = Math.Atan2(y, x) + planetElements[index].LongitudeAscendingNode;

    //  double projectedRadius = radius * Math.Cos(heliocentricLatitude);

    //  double eclipticLongitude;

    //  if (index > (int)VisiblePlanets.Earth)
    //  {
    //    eclipticLongitude = Math.Atan((_earthRadius * Math.Sin(projectedHeliocentricLongitude - _earthEclipticLongitude))
    //                              / (projectedRadius - _earthRadius * Math.Cos(projectedHeliocentricLongitude - _earthEclipticLongitude)))
    //                        + projectedHeliocentricLongitude;
    //  }
    //  else
    //  {
    //    eclipticLongitude = ConstantsD.Pi + _earthEclipticLongitude + Math.Atan((projectedRadius * Math.Sin(_earthEclipticLongitude - projectedHeliocentricLongitude))
    //                                       / (_earthRadius - projectedRadius * Math.Cos(_earthEclipticLongitude - projectedHeliocentricLongitude)));
    //  }

    //  eclipticLongitude = InRange(eclipticLongitude);

    //  double eclipticLatitude = Math.Atan((projectedRadius * Math.Tan(heliocentricLatitude)
    //                                   * Math.Sin(eclipticLongitude - projectedHeliocentricLongitude))
    //                                  / (_earthRadius * Math.Sin(projectedHeliocentricLongitude - _earthEclipticLongitude)));

    //  double ra = Math.Atan2((Math.Sin(eclipticLongitude) * Math.Cos(_e) - Math.Tan(eclipticLatitude) * Math.Sin(_e))
    //                     , Math.Cos(eclipticLongitude));

    //  double dec = Math.Asin(Math.Sin(eclipticLatitude) * Math.Cos(_e) + Math.Cos(eclipticLatitude) * Math.Sin(_e)
    //                    * Math.Sin(eclipticLongitude));

    //  double dist2 = _earthRadius * _earthRadius + radius * radius - 2 * _earthRadius * radius * Math.Cos(heliocentricLongitude - _earthEclipticLongitude);
    //  double dist = Math.Sqrt(dist2);

    //  double d = eclipticLongitude - heliocentricLongitude;
    //  double phase = 0.5 * (1.0 + Math.Cos(d));

    //  double visualMagnitude;

    //  if (index == (int)VisiblePlanets.Venus)
    //  {
    //    d *= MathHelper.ToDegrees(d);
    //    visualMagnitude = -4.34 + 5.0 * Math.Log10(radius * dist) + 0.013 * d + 4.2E-7 * d * d * d;
    //  }
    //  else
    //  {
    //    visualMagnitude = 5.0 * Math.Log10((radius * dist) / Math.Sqrt(phase)) + planetElements[index].VisualMagnitude;
    //  }

    //  _planetData[index].RightAscension = ra;
    //  _planetData[index].Declination = dec;
    //  _planetData[index].VisualMagnitude = visualMagnitude;
    //}


    ///// <summary>
    ///// Gets the planet position.
    ///// </summary>
    ///// <param name="planet">The planet.</param>
    ///// <param name="rightAscension">The right ascension of .</param>
    ///// <param name="declination">The declination.</param>
    ///// <param name="visualMagnitude">The visual magnitude (lower values are brighter).</param>
    //public void GetPlanetPosition(VisiblePlanets planet, out double rightAscension, out double declination, out double visualMagnitude)
    //{
    //  if (planet == VisiblePlanets.Earth)
    //    throw new ArgumentException("This method must not be called for the earth.");

    //  var planetData = _planetData[(int)planet];
    //  rightAscension = planetData.RightAscension;
    //  declination = planetData.Declination;
    //  visualMagnitude = planetData.VisualMagnitude;

    //  // Note: To convert rightAscension and declination to horizontal coordinates:
    //  // Convert to cartesian coordinates. Apply EquatorialToWorld matrix.
    //  // To convert visualMagnitude to illuminance:
    //  // Use the formula of paper "A Physically-Based Night Sky Model" to compute irradiance.
    //  // When rendering as a sprite you must probably convert from irradiance to radiance using
    //  // the angular size of the sprite (solid angle).
    //  // Or use http://zfx.info/viewtopic.php?f=5&t=1298#p15341 to compute the "surface brightness".
    //  // To convert from radiometric units to photometric units multiply with 683 for green
    //  // (and smaller values for mixed spectrums).
    //}
    #endregion
  }
}
