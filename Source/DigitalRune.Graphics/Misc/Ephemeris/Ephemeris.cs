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

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Computes the physically-based properties of sky objects like the sun and the moon.
  /// </summary>
  /// <remarks>
  /// <para>
  /// In astronomy and celestial navigation, an ephemeris (plural: ephemerides; from the Greek word
  /// ἐφημερίς ephēmeris "diary", "journal") gives the positions of astronomical objects in the sky
  /// at a given time or times. The class <see cref="Ephemeris"/> can be used to retrieve the 
  /// positions of the sun and the moon. It also computes transformations which can be used to
  /// convert between different astronomical coordinate system. Further, the light contributions of
  /// the sun and the moon are estimated.
  /// </para>
  /// <para>
  /// The input for all computations are the position (specified using <see cref="Latitude"/>,
  /// <see cref="Longitude"/> and <see cref="Altitude"/>) and the current <see cref="Time"/>. All
  /// derived values are computed when <see cref="Update"/> is called. That means, 
  /// <see cref="Update"/> must be called every time the input properties are changed. It is not
  /// called automatically, so <see cref="Update"/> must be called at least once.
  /// </para>
  /// <para>
  /// Following coordinate systems are used. All coordinate system are right-handed and can be used
  /// with cartesian coordinates (X, Y, Z) or polar coordinates (latitude, longitude).
  /// </para>
  /// <para>
  /// <strong>Ecliptic Coordinate System:</strong><br/>
  /// This coordinate system is relative to the plane defined by the path of the sun or (which is
  /// the same) the plane in which the earth moves around the sun.  That means, in the ecliptic
  /// system the latitude of the sun or the earth is always 0.<br/>
  /// Latitude and longitude are 0 at the vernal equinox. Latitude in this space is also called
  /// declination. Longitude is also called right ascension.<br/>
  /// Regarding Cartesian coordinates, the x and y axes are in the plane of the earth orbit. x is
  /// the axis where latitude and longitude are 0, which is equal to the vernal equinox. +z points
  /// north. The origin of the coordinate system can be the sun (heliocentric) or the earth
  /// (geocentric).
  /// </para>
  /// <para>
  /// <strong>Equatorial Coordinate System:</strong><br/>
  /// This coordinate system is relative to the plane defined by the earth's equator. Latitude and
  /// longitude are 0 at the vernal equinox.<br/>
  /// Regarding Cartesian coordinates, the x and y axes are in the plane of the equator. x is the
  /// axis where latitude and longitude are 0, which is equal to the vernal equinox. +y points east.
  /// +z points north. The origin of the coordinate system can be the sun (heliocentric) or the
  /// earth (geocentric).
  /// </para>
  /// <para>
  /// <strong>Geographic Coordinate System:</strong><br/>
  /// This coordinate system is relative to the plane defined by the earth's equator. This system is
  /// like the Equatorial system but the longitude is 0 at Greenwich. This means, the difference to
  /// the Equatorial system is a constant longitude offset. This coordinate system is well known
  /// from school and globes. The properties <see cref="Latitude"/>, <see cref="Longitude"/> are
  /// relative to the Geographic Coordinate System.<br/>
  /// Regarding Cartesian coordinates, the x and y axes are in the plane of the equator. x is the
  /// axis where latitude and longitude are 0, which is in the line of Greenwich. +y points east. +z
  /// points north.
  /// </para>
  /// <para>
  /// <strong>World Space:</strong><br/>
  /// This coordinate system is relative to a place on the earth. Computer game levels use this
  /// coordinate system. It is also known as "Horizontal Coordinate System" or "Horizon Coordinates".
  /// The origin of this space is defined by <see cref="Latitude"/>, <see cref="Longitude"/> (in the
  /// Geographic coordinate system) and <see cref="Altitude"/>.<br/>
  /// Regarding Cartesian coordinates, +x points east, +y points up, -z points north.
  /// </para>
  /// </remarks>
  public partial class Ephemeris
  {
    // Notes:
    // References: 
    // - "Physically-Based Outdoor Scene Lighting", Game Engine Gems 1.
    //   which is based on the paper "A physically based night sky model" and others.
    //
    // ----- Misc
    // Lat/Long/Altitude could be encapsulated in a struct GeographicLocation.
    //
    // ----- TODOs:
    // - Add unit tests.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The fractional number of centuries elapsed since January 1, 2000 GMT, terrestrial time
    // (which does not account for leap seconds).
    private double _epoch2000Centuries;

    // The fractional number of days elapsed since January 1, 1990 GMT.
    private double _epoch1990Days;

    // Heliocentric radius of earth.
    //private double _earthRadius;

    // Longitude of earth in ecliptic coordinates. (Ecliptic lat is always 0.)
    private double _earthEclipticLongitude;

    // Longitude of sun in ecliptic coordinates. (Ecliptic lat is always 0.)
    private double _sunEclipticLongitude;

    // Obliquity of the ecliptic = tilt between ecliptic and equatorial plane.
    private double _e;

    // Conversion from Equatorial to World ignoring precession.
    private Matrix44D _equatorialToWorldNoPrecession;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the latitude of the world space origin (using the Geographic coordinate space).
    /// </summary>
    /// <value>
    /// The latitude in degrees in the range [-90, 90]. (90° is at the north pole.)
    /// </value>
    public float Latitude
    {
      get { return _latitude; }
      set
      {
        if (value < -90 || value > 90)
          throw new ArgumentOutOfRangeException("value");

        _latitude = value;
      }
    }
    private float _latitude;


    /// <summary>
    /// Gets or sets the longitude of the world space origin (using the Geographic coordinate space).
    /// </summary>
    /// <value>
    /// The longitude in degrees in the range [-180, 180]. (East is positive.)
    /// </value>
    public float Longitude
    {
      get { return _longitude; }
      set
      {
        if (value < -180 || value > 180)
          throw new ArgumentOutOfRangeException("value");

        _longitude = value;
      }
    }
    private float _longitude;


    /// <summary>
    /// Gets or sets the altitude (elevation) in meters above the mean sea level.
    /// </summary>
    /// <value>The altitude (elevation) in meters above the mean sea level.</value>
    public float Altitude { get; set; }


    /// <summary>
    /// Gets or sets the date and time relative to Coordinated Universal Time (UTC).
    /// </summary>
    /// <value>
    /// The date and time relative to Coordinated Universal Time (UTC). The property is initialized
    /// with <see cref="DateTimeOffset.UtcNow"/>.
    /// </value>
    public DateTimeOffset Time { get; set; }


    /// <summary>
    /// Gets the sun position in world space in meters.
    /// </summary>
    /// <value>The sun position in world space in meters.</value>
    public Vector3D SunPosition { get; private set; }


    /// <summary>
    /// Gets the direction to the sun as seen from within the atmosphere considering optical 
    /// refraction.
    /// </summary>
    /// <value>
    /// The direction to the sun as seen from within the atmosphere considering optical refraction.
    /// </value>
    public Vector3D SunDirectionRefracted { get; private set; }


    /// <summary>
    /// Gets the moon position in world space.
    /// </summary>
    /// <value>The moon position in world space in meters.</value>
    public Vector3D MoonPosition { get; private set; }


    /// <summary>
    /// Gets the moon phase angle.
    /// </summary>
    /// <value>
    /// The moon phase angle in radians in the range [0, 2π]. A new moon has a phase angle of
    /// 0. A full moon has a phase angle of π. 
    /// </value>
    public double MoonPhaseAngle { get; set; }


    /// <summary>
    /// Gets the moon phase as a relative value.
    /// </summary>
    /// <value>The moon phase in the range [0, 1], where 0 is new moon and 1 is full moon.</value>
    public double MoonPhaseRelative
    {
      get
      {
        // The moon phase in the range [0, 1], where 0 is new moon and 1 is full moon.
        return 0.5 * (1.0 - Math.Cos(MoonPhaseAngle));
      }
    }


    /// <summary>
    /// Gets the rotation matrix which converts directions from the ecliptic coordinate system to
    /// the equatorial coordinate system.
    /// </summary>
    /// <value>
    /// The rotation matrix which converts directions from the ecliptic coordinate system to the
    /// equatorial coordinate system.
    /// </value>
    public Matrix33D EclipticToEquatorial { get; private set; }


    //public Matrix44D EclipticToWorld { get; private set; }


    /// <summary>
    /// Gets the transformation matrix which converts directions from the equatorial coordinate
    /// system to the world space.
    /// </summary>
    /// <value>
    /// The transformation matrix which converts directions from the equatorial coordinate system to
    /// the world space.
    /// </value>
    public Matrix44D EquatorialToWorld { get; private set; }


    /// <summary>
    /// Gets the rotation matrix which converts directions from the equatorial coordinate system to
    /// the geographic coordinate system.
    /// </summary>
    /// <value>
    /// The rotation matrix which converts directions from the equatorial coordinate system to the
    /// geographic coordinate system.
    /// </value>
    public Matrix33D EquatorialToGeographic { get; private set; }


    //private Matrix33D WorldToGeographic { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Ephemeris"/> class.
    /// </summary>
    public Ephemeris()
    {
      // Seattle, Washington
      Latitude = 47;
      Longitude = 122;
      Altitude = 100;

#if XBOX
      Time = new DateTimeOffset(DateTime.Now.Ticks, TimeSpan.Zero);
#else
      Time = DateTimeOffset.UtcNow;
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Computes the derived values, like sun/moon positions, transformation matrices and light
    /// intensities. This method must be called when the location or time has changed.
    /// </summary>
    /// <remarks>
    /// This method must be called when the input properties <see cref="Latitude"/>,
    /// <see cref="Longitude"/>, <see cref="Altitude"/>, or <see cref="Time"/>) have changed.
    /// </remarks>
    public void Update()
    {
      _epoch2000Centuries = ToEpoch2000Centuries(Time, true);
      _epoch1990Days = ToEpoch1990Days(Time, false);

      // To transform from ecliptic to equatorial, we rotate by the obliquity of the ecliptic.
      _e = 0.409093 - 0.000227 * _epoch2000Centuries;
      EclipticToEquatorial = Matrix33D.CreateRotationX(_e);

      // GMST = Greenwich mean sidereal time (mittlere Greenwich-Sternzeit) in radians.
      double gmst = 4.894961 + 230121.675315 * ToEpoch2000Centuries(Time, false);
      EquatorialToGeographic = Matrix33D.CreateRotationZ(-gmst);

      // The earth axis slowly changes over time (precession). The precession movement repeats
      // itself approx. all 26000 years. When we move from to horizontal or geographics,
      // we need to apply the precession.

      // In Game Engine Gems:
      //var Rx = Matrix33D.CreateRotationX(0.1118 * _epoch2000Centuries);
      //var Ry = Matrix33D.CreateRotationY(-0.00972 * _epoch2000Centuries);
      //var Rz = Matrix33D.CreateRotationZ(0.01118 * _epoch2000Centuries);
      //var precession = Rz * (Ry * Rx);

      // In original article:
      var Ry = Matrix33D.CreateRotationY(-0.00972 * _epoch2000Centuries);
      var Rz = Matrix33D.CreateRotationZ(0.01118 * _epoch2000Centuries);
      var precession = Rz * Ry * Rz;

      // In game engine gems precession is applied in EclipticToWorld and in
      // EquatorialToWorld. This makes no sense since precession cannot be valid for both
      // coordinate systems. --> We assume the precession is given in equatorial space.
      //EclipticToWorld = rLat * rLong * EclipticToEquatorial * precession;

      // Latitude rotation
      var rLat = Matrix33D.CreateRotationY(MathHelper.ToRadians(Latitude) - ConstantsD.PiOver2);

      // Longitude rotation
      // LMST = Local mean sidereal time (mittlere Ortssternzeit) in radians.
      double lmst = gmst + MathHelper.ToRadians(Longitude);
      var rLong = Matrix33D.CreateRotationZ(-lmst);

      // Earth radius at the equator. (We assume a perfect sphere. We do not support geodetic 
      // systems with imperfect earth spheres.)
      const double earthRadius = 6378.137 * 1000;
      var equatorialToHorizontalTranslation = new Vector3D(0, -earthRadius - Altitude, 0);

      // Switching of the coordinate axes between Equatorial (z up) and Horizontal (y up).
      var axisSwitch = new Matrix33D(0, 1, 0,
                                     0, 0, 1,
                                     1, 0, 0);

      EquatorialToWorld = new Matrix44D(axisSwitch * rLat * rLong * precession,
                                        equatorialToHorizontalTranslation);

      _equatorialToWorldNoPrecession = new Matrix44D(axisSwitch * rLat * rLong,
                                                     equatorialToHorizontalTranslation);

      //WorldToGeographic = EquatorialToGeographic * EquatorialToWorld.Minor.Transposed;

      ComputeSunPosition();
      ComputeMoonPosition();
      ComputeEarthPosition();

      //for (int i = 0; i < NumberOfPlanets; i++)
      //{
      //  var planet = (VisiblePlanets)i;
      //  if (planet != VisiblePlanets.Earth)
      //    ComputePlanetData(planet);
      //}
    }


    private void ComputeSunPosition()
    {
      // See http://en.wikipedia.org/wiki/Position_of_the_Sun. (But these formulas seem to be a bit
      // more precise.)

      double meanAnomaly = 6.24 + 628.302 * _epoch2000Centuries;

      // Ecliptic longitude.
      _sunEclipticLongitude = 4.895048 + 628.331951 * _epoch2000Centuries + (0.033417 - 0.000084 * _epoch2000Centuries) * Math.Sin(meanAnomaly)
                            + 0.000351 * Math.Sin(2.0 * meanAnomaly);

      // Distance from earth in astronomical units.
      double geocentricDistance = 1.000140 - (0.016708 - 0.000042 * _epoch2000Centuries) * Math.Cos(meanAnomaly)
                                  - 0.000141 * Math.Cos(2.0 * meanAnomaly);

      // Sun position.
      Vector3D sunPositionEcliptic = ToCartesian(geocentricDistance, 0, _sunEclipticLongitude);
      Vector3D sunPositionEquatorial = EclipticToEquatorial * sunPositionEcliptic;

      // Note: The sun formula is already corrected by precession.
      SunPosition = _equatorialToWorldNoPrecession.TransformDirection(sunPositionEquatorial);
      Vector3D sunDirection = SunPosition.Normalized;

      // Convert from astronomical units to meters.
      const double au = 149597870700; // 1 au = 149,597,870,700 m
      SunPosition *= au;

      // Account for atmospheric refraction.
      double elevation = Math.Asin(sunDirection.Y);
      elevation = Refract(elevation);
      sunDirection.Y = Math.Sin(elevation);
      sunDirection.Normalize();
      SunDirectionRefracted = sunDirection;
    }


    private void ComputeEarthPosition()
    {
      double Np = ((2.0 * ConstantsD.Pi) / 365.242191) * (_epoch1990Days / _planetElements[(int)VisiblePlanets.Earth].Period);
      Np = InRange(Np);

      double Mp = Np + _planetElements[(int)VisiblePlanets.Earth].EpochLongitude - _planetElements[(int)VisiblePlanets.Earth].PerihelionLongitude;

      _earthEclipticLongitude = Np + 2.0 * _planetElements[(int)VisiblePlanets.Earth].Eccentricity * Math.Sin(Mp)
                                + _planetElements[(int)VisiblePlanets.Earth].EpochLongitude;

      _earthEclipticLongitude = InRange(_earthEclipticLongitude);

      //double vp = _earthEclipticLongitude - planetElements[(int)VisiblePlanets.Earth].PerihelionLongitude;
      //_earthRadius = (planetElements[(int)VisiblePlanets.Earth].SemiMajorAxis 
      //                 * (1.0 - planetElements[(int)VisiblePlanets.Earth].Eccentricity * planetElements[(int)VisiblePlanets.Earth].Eccentricity)) 
      //               / (1.0 + planetElements[(int)VisiblePlanets.Earth].Eccentricity * Math.Cos(vp));
    }


    private void ComputeMoonPosition()
    {
      double lp = 3.8104 + 8399.7091 * _epoch2000Centuries;
      double m = 6.2300 + 628.3019 * _epoch2000Centuries;
      double f = 1.6280 + 8433.4663 * _epoch2000Centuries;
      double mp = 2.3554 + 8328.6911 * _epoch2000Centuries;
      double d = 5.1985 + 7771.3772 * _epoch2000Centuries;

      double longitude =
          lp
          + 0.1098 * Math.Sin(mp)
          + 0.0222 * Math.Sin(2 * d - mp)
          + 0.0115 * Math.Sin(2 * d)
          + 0.0037 * Math.Sin(2 * mp)
          - 0.0032 * Math.Sin(m)
          - 0.0020 * Math.Sin(2 * f)
          + 0.0010 * Math.Sin(2 * d - 2 * mp)
          + 0.0010 * Math.Sin(2 * d - m - mp)
          + 0.0009 * Math.Sin(2 * d + mp)
          + 0.0008 * Math.Sin(2 * d - m)
          + 0.0007 * Math.Sin(mp - m)
          - 0.0006 * Math.Sin(d)
          - 0.0005 * Math.Sin(m + mp);

      double latitude =
          +0.0895 * Math.Sin(f)
          + 0.0049 * Math.Sin(mp + f)
          + 0.0048 * Math.Sin(mp - f)
          + 0.0030 * Math.Sin(2 * d - f)
          + 0.0010 * Math.Sin(2 * d + f - mp)
          + 0.0008 * Math.Sin(2 * d - f - mp)
          + 0.0006 * Math.Sin(2 * d + f);

      longitude = InRange(longitude);
      _sunEclipticLongitude = InRange(_sunEclipticLongitude);
      MoonPhaseAngle = Math.Abs(longitude - _sunEclipticLongitude);
      MoonPhaseAngle = InRange(MoonPhaseAngle);

      double pip =
          +0.016593
          + 0.000904 * Math.Cos(mp)
          + 0.000166 * Math.Cos(2 * d - mp)
          + 0.000137 * Math.Cos(2 * d)
          + 0.000049 * Math.Cos(2 * mp)
          + 0.000015 * Math.Cos(2 * d + mp)
          + 0.000009 * Math.Cos(2 * d - m);

      double dMoon = 1.0 / pip; // Earth radii

      // Moon position in Cartesian coordinates of the ecliptic coordinates system.
      Vector3D moonPositionEcliptic = ToCartesian(dMoon, latitude, longitude);

      // Moon position in Cartesian coordinates of the equatorial coordinates system.
      Vector3D moonPositionEquatorial = EclipticToEquatorial * moonPositionEcliptic;

      // To [m].
      moonPositionEquatorial *= 6378.137 * 1000;

      // Note: The moon formula is already corrected by precession.
      MoonPosition = _equatorialToWorldNoPrecession.TransformPosition(moonPositionEquatorial);
    }
    #endregion
  }
}
