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
  partial class Ephemeris
  {
    /// <summary>
    /// Clamps an angle to the interval [0, 2π].
    /// </summary>
    private static double InRange(double x)
    {
      while (x > ConstantsD.TwoPi)
        x -= ConstantsD.TwoPi;
      while (x < 0)
        x += ConstantsD.TwoPi;
      return x;
    }


    /// <summary>
    /// Converts polar coordinates to Cartesian coordinates.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <returns>The position in Cartesian coordinates.</returns>
    /// <remarks>
    /// The Cartesian coordinate system is right handed; y points east; z points up.
    /// In other words: Latitude and longitude are relative to +x; z increases with latitude. 
    /// </remarks>
    private static Vector3D ToCartesian(double radius, double latitude, double longitude)
    {
      double sinLat = Math.Sin(latitude);
      double cosLat = Math.Cos(latitude);
      double sinLong = Math.Sin(longitude);
      double cosLong = Math.Cos(longitude);

      Vector3D v;
      v.X = radius * cosLong * cosLat;
      v.Y = radius * sinLong * cosLat;  // East
      v.Z = radius * sinLat;            // Up
      return v;
    }


    /// <summary>
    /// Converts the specified <see cref="DateTimeOffset"/> to the Julian Date.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/>.</param>
    /// <param name="terrestrialTime">
    /// Specifying terrestrial time means you want atomic clock time, not corrected by leap seconds 
    /// to account for slowing of the earth's rotation, as opposed to GMT which does account for 
    /// leap seconds.
    /// </param>
    /// <returns>
    /// Days and fractions since noon Universal Time on January 1, 4713 BCE on the Julian calendar.
    /// </returns>
    /// <remarks>
    /// Julian Dates are used for astronomical calculations (such as our own ephemeris model) and 
    /// represent days and fractions since noon Universal Time on January 1, 4713 BCE on the Julian 
    /// calendar. Note that due to precision limitations of 64-bit doubles, the resolution of the 
    /// date returned may be as low as within 8 hours.
    /// </remarks>
    private static double ToJulianDate(DateTimeOffset dateTimeOffset, bool terrestrialTime)
    {
      // See http://de.wikipedia.org/wiki/Julianisches_Datum.

      // Convert to GMT/UTC.
      var dateTime = dateTimeOffset.UtcDateTime;
      double h = dateTime.Hour + (dateTime.Minute + dateTime.Second / 60.0 + dateTime.Millisecond / 60.0 / 1000.0) / 60.0;
      double d = dateTime.Day + (h / 24.0);

      double y, m;
      if (dateTime.Month < 3)
      {
        y = dateTime.Year - 1;
        m = dateTime.Month + 12;
      }
      else
      {
        y = dateTime.Year;
        m = dateTime.Month;
      }

      double result = 1720996.5 - Math.Floor(y / 100.0) + Math.Floor(y / 400.0) + Math.Floor(365.25 * y)
                      + Math.Floor(30.6001 * (m + 1)) + d;

      // UTC is an approximation (within 0.9 seconds) for UT1. 
      // Terrestrial time (http://en.wikipedia.org/wiki/Terrestrial_Time) is ahead of UT1 by 
      // deltaT (http://en.wikipedia.org/wiki/DeltaT) which is a number which depends on date, 
      // earth mass, melting ice, etc. deltaT = 65 s is accurate enough for us.
      if (terrestrialTime)
        result += 65.0 / 60.0 / 60.0 / 24.0;

      return result;
    }


    /// <summary>
    /// Converts the specified <see cref="DateTimeOffset"/> to the number of centuries since 
    /// January 1, 2000.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/>.</param>
    /// <param name="terrestrialTime">
    /// Specifying terrestrial time means you want atomic clock time, not corrected by leap seconds 
    /// to account for slowing of the earth's rotation, as opposed to GMT which does account for 
    /// leap seconds.
    /// </param>
    /// <returns>The fractional number of centuries elapsed since January 1, 2000.</returns>
    /// <remarks>
    /// This method converts a date to centuries and fraction since January 1, 2000. Used for
    /// internal astronomical calculations. Since this number is smaller than that returned by
    /// <see cref="ToJulianDate"/>, it is of higher precision.
    /// </remarks>
    private static double ToEpoch2000Centuries(DateTimeOffset dateTimeOffset, bool terrestrialTime)
    {
      double julianDate = ToJulianDate(dateTimeOffset, terrestrialTime);
      return (julianDate - 2451545.0) / 36525.0;  // A Julian year is 365.25 days.
    }


    /// <summary>
    /// Converts the specified <see cref="DateTimeOffset"/> to the number of days since 
    /// January 1, 1990.
    /// </summary>
    /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/>.</param>
    /// <param name="terrestrialTime">
    /// Specifying terrestrial time means you want atomic clock time, not corrected by leap seconds 
    /// to account for slowing of the earth's rotation, as opposed to GMT which does account for 
    /// leap seconds.
    /// </param>
    /// <returns>The fractional number of days elapsed since January 1, 1990.</returns>
    /// <remarks>
    /// This method converts a date to days elapsed since January 1, 1990 on the Julian calendar. 
    /// Used for internal astronomical calculations. Since this number is smaller than that returned 
    /// by <see cref="ToJulianDate"/>, it is of higher precision.
    /// </remarks>
    private static double ToEpoch1990Days(DateTimeOffset dateTimeOffset, bool terrestrialTime)
    {
      double julianDate = ToJulianDate(dateTimeOffset, terrestrialTime);
      return julianDate - 2447891.5;
    }


    /// <summary>
    /// Simulates atmospheric refraction for objects close to the horizon.
    /// </summary>
    /// <param name="elevation">The elevation angle of the object above the horizon.</param>
    /// <returns>
    /// The elevation angle of the object above the horizon after simulating atmospheric refraction.
    /// </returns>
    /// <remarks>
    /// This method does not model variations in atmosphere pressure and temperature.
    /// </remarks>
    private static double Refract(double elevation)
    {
      // See Zimmerman, John C. 1981. Sun-pointing programs and their accuracy.
      // SAND81-0761, Experimental Systems Operation Division 4721, Sandia National Laboratories, Albuquerque, NM.

      //float prestemp;     // temporary pressure/temperature correction
      double refcor;        // temporary refraction correction 
      double tanelev;       // tangent of the solar elevation angle

      if (elevation > MathHelper.ToRadians(85.0))
      {
        // No refraction near zenith. (Algorithm does not work there.)
        refcor = 0.0;
      }
      else
      {
        // Refract.
        tanelev = Math.Tan(elevation);
        if (elevation >= MathHelper.ToRadians(5.0))
        {
          refcor = 58.1 / tanelev - 0.07 / (Math.Pow(tanelev, 3)) + 0.000086 / (Math.Pow(tanelev, 5));
        }
        else if (elevation >= MathHelper.ToRadians(-0.575))
        {
          double degElev = MathHelper.ToDegrees(elevation);
          refcor = 1735.0 + degElev * (-518.2 + degElev * (103.4 + degElev * (-12.79 + degElev * 0.711)));
        }
        else
        {
          refcor = -20.774 / tanelev;
        }

        //prestemp = (pdat->press * 283.0) / (1013.0 * (273.0 + pdat->temp));
        //refcor *= prestemp / 3600.0;
        refcor /= 3600.0;
      }

      // Refracted solar elevation angle.
      return elevation + MathHelper.ToRadians(refcor);
    }
  }
}
