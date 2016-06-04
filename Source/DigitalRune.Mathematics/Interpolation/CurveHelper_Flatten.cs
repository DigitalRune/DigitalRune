// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  static partial class CurveHelper
  {
    // Notes:
    // The tolerance in flattening is the allowed error per segment. It it is the allowed error
    // for the total curve, then long curves to be more finely segmented than short curves.
    // Example: A half circle is flattened to two segment. Then we expect that the full circle is
    // flattened to 4 segments. Therefore the tolerance must not be relative to the total length.

    /// <summary>
    /// Flattens the specified curve. See <see cref="ICurve{TParam, TPoint}.Flatten"/>.
    /// </summary>
    /// <remarks>
    /// This method cannot be used for curves that contain gaps!
    /// </remarks>
    internal static void Flatten(ICurve<float, Vector2F> curve, ICollection<Vector2F> points, int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      float totalLength = curve.GetLength(0, 1, maxNumberOfIterations, tolerance / 10);

      // No line segments if the curve has zero length.
      if (totalLength <= 0)
        return;

      // A single line segment if the curve's length is less than the tolerance.
      if (totalLength < tolerance)
      {
        points.Add(curve.GetPoint(0));
        points.Add(curve.GetPoint(1));
        return;
      }

      var list = ResourcePools<Vector2F>.Lists.Obtain();
      
      Flatten(curve, list, 0, 1, curve.GetPoint(0), curve.GetPoint(1),  0, totalLength, 1, maxNumberOfIterations, tolerance);

      foreach (var point in list)
        points.Add(point);

      ResourcePools<Vector2F>.Lists.Recycle(list);
    }


    /// <summary>
    /// Flattens the specified curve. See <see cref="ICurve{TParam, TPoint}.Flatten"/>.
    /// </summary>
    /// <remarks>
    /// This method cannot be used for curves that contain gaps!
    /// </remarks>
    internal static void Flatten(ICurve<float, Vector3F> curve, ICollection<Vector3F> points, int maxNumberOfIterations, float tolerance)
    {
      if (tolerance <= 0)
        throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than zero.");

      float totalLength = curve.GetLength(0, 1, maxNumberOfIterations, tolerance);

      // No line segments if the curve has zero length.
      if (totalLength == 0)
        return;

      // A single line segment if the curve's length is less than the tolerance.
      if (totalLength < tolerance)
      {
        points.Add(curve.GetPoint(0));
        points.Add(curve.GetPoint(1));
        return;
      }

      var list = ResourcePools<Vector3F>.Lists.Obtain();

      Flatten(curve, list, 0, 1, curve.GetPoint(0), curve.GetPoint(1), 0, totalLength, 1, maxNumberOfIterations, tolerance);

      foreach (var point in list)
        points.Add(point);

      ResourcePools<Vector3F>.Lists.Recycle(list);
    }


    private static void Flatten(ICurve<float, Vector2F> curve, List<Vector2F> points, float p0, float p1, Vector2F point0, Vector2F point1, float length0, float length1, int iteration, int maxNumberOfIterations, float tolerance)
    {
      if (iteration >= maxNumberOfIterations 
          || Math.Abs((length1 - length0) - (point1 - point0).Length) < tolerance)
      {
        points.Add(point0);
        points.Add(point1);
        return;
      }

      // Subdivide.
      var pMiddle = (p0 + p1) / 2;
      var pointMiddle = curve.GetPoint(pMiddle);
      var lengthMiddle = length0 + curve.GetLength(p0, pMiddle, maxNumberOfIterations, tolerance / 10);
      Flatten(curve, points, p0, pMiddle, point0, pointMiddle, length0, lengthMiddle, iteration + 1, maxNumberOfIterations, tolerance);
      Flatten(curve, points, pMiddle, p1, pointMiddle, point1, lengthMiddle, length1, iteration + 1, maxNumberOfIterations, tolerance);
    }


    private static void Flatten(ICurve<float, Vector3F> curve, List<Vector3F> points, float p0, float p1, Vector3F point0, Vector3F point1, float length0, float length1, int iteration, int maxNumberOfIterations, float tolerance)
    {
      if (iteration >= maxNumberOfIterations
          || Math.Abs((length1 - length0) - (point1 - point0).Length) < tolerance)
      {
        points.Add(point0);
        points.Add(point1);
        return;
      }

      // Subdivide.
      var pMiddle = (p0 + p1) / 2;
      var pointMiddle = curve.GetPoint(pMiddle);
      var lengthMiddle = length0 + curve.GetLength(p0, pMiddle, maxNumberOfIterations, tolerance / 10);
      Flatten(curve, points, p0, pMiddle, point0, pointMiddle, length0, lengthMiddle, iteration + 1, maxNumberOfIterations, tolerance);
      Flatten(curve, points, pMiddle, p1, pointMiddle, point1, lengthMiddle, length1, iteration + 1, maxNumberOfIterations, tolerance);
    }


    /// <summary>
    /// Computes the length of a list of 2D line segments.
    /// </summary>
    internal static float GetLength(List<Vector2F> lineSegments)
    {
      int numberOfSegments = lineSegments.Count / 2;
      float length = 0;
      for (int i = 0; i < numberOfSegments; i++)
        length += (lineSegments[2 * i + 0] - lineSegments[2 * i + 1]).Length;

      return length;
    }


    /// <summary>
    /// Computes the length of a list of 3D line segments.
    /// </summary>
    internal static float GetLength(List<Vector3F> lineSegments)
    {
      int numberOfSegments = lineSegments.Count / 2;
      float length = 0;
      for (int i = 0; i < numberOfSegments; i++)
        length += (lineSegments[2 * i + 0] - lineSegments[2 * i + 1]).Length;

      return length;
    }
  }
}
