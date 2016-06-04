// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    // Line methods adapted from Game Programming Gems 2, section 2.3
    // Other methods, see: 
    //  Schneider and Eberly: "Geometric Tools for Computer Graphics", 
    //  Coutinho: "Dynamic Simulations of Multibody Systems" (only for line vs. line segment), 
    //  Ericson: "Real-Time Collision Detection"
    //
    // Possible optimizations:
    // - This code could be optimized by inlining all the GetLineParameter and AdjustXxx methods to 
    //   remove duplicate computations.


    /// <summary>
    /// Gets the closest point of a line to a point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="line">The line.</param>
    /// <param name="closestPointOnLine">
    /// The point on the line that is closest to <paramref name="point"/>.
    /// </param>
    /// <returns><see langword="true"/> if the <paramref name="point"/> is on the line.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public static bool GetClosestPoint(Line line, Vector3F point, out Vector3F closestPointOnLine)
    {
      float parameter;
      GetLineParameter(new LineSegment(line.PointOnLine, line.PointOnLine + line.Direction), point, out parameter);
      closestPointOnLine = line.PointOnLine + parameter * line.Direction;
      return Vector3F.AreNumericallyEqual(point, closestPointOnLine);
    }


    /// <overloads>
    /// <summary>
    /// Gets the closest points between two primitives.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the closest points of two lines.
    /// </summary>
    /// <param name="line0">The first line.</param>
    /// <param name="line1">The second line.</param>
    /// <param name="point0">
    /// The point on <paramref name="line0"/> that is closest to <paramref name="line1"/>.
    /// </param>
    /// <param name="point1">
    /// The point on <paramref name="line1"/> that is closest to <paramref name="line0"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the lines are touching (<paramref name="point0"/> and 
    /// <paramref name="point1"/> are identical); otherwise <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool GetClosestPoints(Line line0, Line line1, out Vector3F point0, out Vector3F point1)
    {
      float s, t;
      GetLineParameters(
        new LineSegment(line0.PointOnLine, line0.PointOnLine + line0.Direction),
        new LineSegment(line1.PointOnLine, line1.PointOnLine + line1.Direction),
        out s,
        out t);

      point0 = line0.PointOnLine + s * line0.Direction;
      point1 = line1.PointOnLine + t * line1.Direction;
      return Vector3F.AreNumericallyEqual(point0, point1);
    }

    
    /// <summary>
    /// Gets the closest point of a line segment to a point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="lineSegment">The line segment.</param>
    /// <param name="closestPointOnLineSegment">
    /// The point on the line segment that is closest to <paramref name="point"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="point"/> is on the line segment; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public static bool GetClosestPoints(LineSegment lineSegment, Vector3F point, out Vector3F closestPointOnLineSegment)
    {
      float parameter;
      GetLineParameter(lineSegment, point, out parameter);
      parameter = Math.Max(0, Math.Min(1, parameter));
      closestPointOnLineSegment = lineSegment.Start + parameter * (lineSegment.End - lineSegment.Start);
      return Vector3F.AreNumericallyEqual(point, closestPointOnLineSegment);
    }


    /// <summary>
    /// Gets the closest points of two line segments.
    /// </summary>
    /// <param name="segment0">The first line segment.</param>
    /// <param name="segment1">The second line segment.</param>
    /// <param name="point0">
    /// The point on <paramref name="segment0"/> that is closest to <paramref name="segment1"/>.
    /// </param>
    /// <param name="point1">
    /// The point on <paramref name="segment1"/> that is closest to <paramref name="segment0"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the line segments are touching (<paramref name="point0"/> and 
    /// <paramref name="point1"/> are identical); otherwise <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool GetClosestPoints(LineSegment segment0, LineSegment segment1, out Vector3F point0, out Vector3F point1)
    {
      float s, t;
      GetLineParameters(segment0, segment1, out s, out t);
      AdjustClosestPoints(segment0, segment1, s, t, out point0, out point1);
      return Vector3F.AreNumericallyEqual(point0, point1);
    }


    /// <summary>
    /// Gets the closest points of a line and a line segment.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="segment">The line segment.</param>
    /// <param name="pointOnLine">
    /// The point on <paramref name="line"/> that is closest to <paramref name="segment"/>.
    /// </param>
    /// <param name="pointOnSegment">
    /// The point on <paramref name="segment"/> that is closest to <paramref name="line"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the line and the line segment are touching 
    /// (<paramref name="pointOnLine"/> and <paramref name="pointOnSegment"/> are identical); 
    /// otherwise <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public static bool GetClosestPoints(LineSegment segment, Line line, out Vector3F pointOnLine, out Vector3F pointOnSegment)
    {
      float s, t;
      GetLineParameters(new LineSegment(line.PointOnLine, line.PointOnLine + line.Direction), segment, out s, out t);
      AdjustClosestPoints(segment, line, s, t, out pointOnLine, out pointOnSegment);
      return Vector3F.AreNumericallyEqual(pointOnLine, pointOnSegment);
    }


    /// <summary>
    /// Gets the line parameters for the closest points.
    /// </summary>
    /// <param name="segmentA">The first segment.</param>
    /// <param name="segmentB">The second segment.</param>
    /// <param name="s">The line parameter for <paramref name="segmentA"/>.</param>
    /// <param name="t">The line parameter for <paramref name="segmentB"/>.</param>
    /// <remarks>
    /// Line parameter 0 is the start point of the line segment. Line parameter 1 is the end of the
    /// line segment. A line parameter between 0 and 1 defines a point on the line segment. The line
    /// parameters are not clamped, thus they can be outside [0, 1].
    /// </remarks>
    internal static void GetLineParameters(LineSegment segmentA, LineSegment segmentB, out float s, out float t)
    {
      float epsilonSquared = Numeric.EpsilonFSquared;

      // Compute parameters form Equation (1) and (2).
      Vector3F lA = segmentA.End - segmentA.Start;
      Vector3F lB = segmentB.End - segmentB.Start;

      // From Equation (15)
      float l11 = lA.LengthSquared;
      float l22 = lB.LengthSquared;

      if (l11 < epsilonSquared)
      {
        // Segment A has length 0.
        s = 0;
        GetLineParameter(segmentB, segmentA.Start, out t);
      }
      else if (l22 < epsilonSquared)
      {
        // Segment B has length 0.
        t = 0;
        GetLineParameter(segmentA, segmentB.Start, out s);
      }
      else
      {
        // No segment has length 0.
        // From Equation (3)
        Vector3F aToB = segmentB.Start - segmentA.Start;

        // From Equation (15)
        float l12 = -Vector3F.Dot(lA, lB);

        float detL = l11 * l22 - l12 * l12;
        if (Math.Abs(detL) < epsilonSquared)
        {
          // Parallel lines.
          // We can choose any point on the lines. 
          s = 0;
          GetLineParameter(segmentB, segmentA.Start, out t);

          // Clamp to range of segments.
          if (t < 0)
          {
            t = 0;
            GetLineParameter(segmentA, segmentB.Start, out s);
          }
          else if (t > 1)
          {
            t = 1;
            GetLineParameter(segmentA, segmentB.End, out s);
          }
        }
        else
        {
          // Non-parallel lines.
          // From Equation (15)
          float ra = Vector3F.Dot(lA, aToB);
          float rb = -Vector3F.Dot(lB, aToB);

          // Equation (12)
          t = (l11 * rb - ra * l12) / detL;

          //float s = (l22 * ra - rb * l12) / detL; // Cramer's Rule
          s = (ra - l12 * t) / l11;

        //  Debug.Assert(Numeric.AreEqual(s * l11 + t * l12, ra), s * l11 + t * l12 + " should be equal to " + ra);
        //  Debug.Assert(Numeric.AreEqual(s * l12 + t * l22, rb), s * l12 + t * l22 + " should be equal to " + rb);
        }
      }
    }


    // Similar to GetLineParameters(LineSegment, LineSegment, ...)
    internal static void GetLineParameter(LineSegment lineSegment, Vector3F point, out float parameter)
    {
      float lengthSquared = lineSegment.LengthSquared;
      if (lengthSquared < Numeric.EpsilonFSquared)
      {
        // Segment has zero length.
        parameter = 0;
        return;
      }

      Vector3F lineToPoint = point - lineSegment.Start;

      // Parameter computed from equation 20.
      parameter = Vector3F.Dot(lineSegment.End - lineSegment.Start, lineToPoint) / lengthSquared;
    }


    // Like the AdjustClosestPoint for two segments but simpler.
    private static void AdjustClosestPoints(LineSegment segment, Line line, float s, float t, out Vector3F pointOnLine, out Vector3F pointOnSegment)
    {
      if (t < 0)
      {
        pointOnSegment = segment.Start;
        GetLineParameter(new LineSegment(line.PointOnLine, line.PointOnLine + line.Direction), pointOnSegment, out s);
        pointOnLine = line.PointOnLine + s * line.Direction;
      }
      else if (t > 1)
      {
        pointOnSegment = segment.End;
        GetLineParameter(new LineSegment(line.PointOnLine, line.PointOnLine + line.Direction), pointOnSegment, out s);
        pointOnLine = line.PointOnLine + s * line.Direction;
      }
      else
      {
        pointOnLine = line.PointOnLine + s * line.Direction;
        pointOnSegment = segment.Start + t * (segment.End - segment.Start);
      }
    }


    // This method takes not-clamped line parameters and computes parameters which lie on the line segment.
    // If a parameter is not in [0,1] we have to clamp it to the segment. In this case there might
    // be a new closer point on the other segment. 
    private static void AdjustClosestPoints(LineSegment segment0, LineSegment segment1, float s, float t, out Vector3F point0, out Vector3F point1)
    {
      if ((s < 0 || s > 1) && (t < 0 || t > 1))
      {
        // s and t are out of range.
        s = Math.Max(0, Math.Min(1, s));
        point0 = segment0.Start + s * (segment0.End - segment0.Start);
        GetLineParameter(segment1, point0, out t);
        if (t < 0 || t > 1)
        {
          // The new t has to be adjusted as well.
          t = Math.Max(0, Math.Min(1, t));
          point1 = segment1.Start + t * (segment1.End - segment1.Start);
          GetLineParameter(segment0, point1, out s);
          s = Math.Max(0, Math.Min(1, s));
          point0 = segment0.Start + s * (segment0.End - segment0.Start);
        }
        else
        {
          point1 = segment1.Start + t * (segment1.End - segment1.Start);
        }
      }
      else if (s < 0)
      {
        // Only s is out of range.
        point0 = segment0.Start;
        GetLineParameter(segment1, point0, out t);
        t = Math.Max(0, Math.Min(1, t));
        point1 = segment1.Start + t * (segment1.End - segment1.Start);
      }
      else if (s > 1)
      {
        // Only s is out of range.
        point0 = segment0.End;
        GetLineParameter(segment1, point0, out t);
        t = Math.Max(0, Math.Min(1, t));
        point1 = segment1.Start + t * (segment1.End - segment1.Start);
      }
      else if (t < 0)
      {
        // Only t is out of range.
        point1 = segment1.Start;
        GetLineParameter(segment0, point1, out s);
        s = Math.Max(0, Math.Min(1, s));
        point0 = segment0.Start + s * (segment0.End - segment0.Start);
      }
      else if (t > 1)
      {
        // Only t is out of range.
        point1 = segment1.End;
        GetLineParameter(segment0, point1, out s);
        s = Math.Max(0, Math.Min(1, s));
        point0 = segment0.Start + s * (segment0.End - segment0.Start);
      }
      else
      {
        point0 = segment0.Start + s * (segment0.End - segment0.Start);
        point1 = segment1.Start + t * (segment1.End - segment1.Start);
      }
    }
  }
}
