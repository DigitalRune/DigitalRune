// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  // Triangle-Triangle Overlap Test Routines
  // Implementation of "Fast and Robust Triangle-Triangle Overlap Test Using Orientation 
  // Predicates"  P. Guigue - O. Devillers Journal of Graphics Tools, 8(1), 2003
  // July, 2002; Updated December 2003
  // Other information are available from the web page 
  //     http://www.acm.org/jgt/papers/GuigueDevillers03/
  // This implementation was derived from the C implementation provided by John Ratcliff
  // (see http://codesuppository.blogspot.co.at/).
  // The code can also be found here using the MIT license: 
  //     https://github.com/erich666/jgt-code/tree/master/Volume_08/Number_1/Guigue2003
  partial class GeometryHelper
  {
    // ReSharper disable IdentifierWordIsNotInDictionary

    // TODO: 
    // - Code does not handle degenerate triangles (points and line segments).
    // - Robustness problems if triangles are nearly coplanar or when an edge is nearly coplanar
    //   to the other triangle. Bugfix: See Real-Time Rendering p. 760.

    //--------------------------------------------------------------
    #region Helper Methods
    //--------------------------------------------------------------

    //private static void CROSS(out Vector3F result, ref Vector3F v1, ref Vector3F v2)
    //{
    //  result.X = v1.Y * v2.Z - v1.Z * v2.Y;
    //  result.Y = v1.Z * v2.X - v1.X * v2.Z;
    //  result.Z = v1.X * v2.Y - v1.Y * v2.X;
    //}


    //private static float DOT(ref Vector3F v1, ref Vector3F v2)
    //{
    //  return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
    //}


    //private static void SUB(out Vector3F result, ref Vector3F v1, ref Vector3F v2)
    //{
    //  result.X = v1.X - v2.X;
    //  result.Y = v1.Y - v2.Y;
    //  result.Z = v1.Z - v2.Z;
    //}


    //private static void SCALAR(out Vector3F result, float alpha, ref Vector3F v)
    //{
    //  result.X = alpha * v.X;
    //  result.Y = alpha * v.Y;
    //  result.Z = alpha * v.Z;
    //}


    private static bool CHECK_MIN_MAX(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
                                      ref Vector3F p2, ref Vector3F q2, ref Vector3F r2)
    {
      Vector3F v1, v2, N1;
      //SUB(out v1, ref p2, ref q1);
      v1.X = p2.X - q1.X;
      v1.Y = p2.Y - q1.Y;
      v1.Z = p2.Z - q1.Z;
      //SUB(out v2, ref p1, ref q1);
      v2.X = p1.X - q1.X;
      v2.Y = p1.Y - q1.Y;
      v2.Z = p1.Z - q1.Z;
      //CROSS(out N1, ref v1, ref v2);
      N1.X = v1.Y * v2.Z - v1.Z * v2.Y;
      N1.Y = v1.Z * v2.X - v1.X * v2.Z;
      N1.Z = v1.X * v2.Y - v1.Y * v2.X;
      //SUB(out v1, ref q2, ref q1);
      v1.X = q2.X - q1.X;
      v1.Y = q2.Y - q1.Y;
      v1.Z = q2.Z - q1.Z;
      //if (DOT(ref v1, ref N1) > 0.0f)
      if (v1.X * N1.X + v1.Y * N1.Y + v1.Z * N1.Z > 0.0f)
        return false;

      //SUB(out v1, ref p2, ref p1);
      v1.X = p2.X - p1.X;
      v1.Y = p2.Y - p1.Y;
      v1.Z = p2.Z - p1.Z;
      //SUB(out v2, ref r1, ref p1);
      v2.X = r1.X - p1.X;
      v2.Y = r1.Y - p1.Y;
      v2.Z = r1.Z - p1.Z;
      //CROSS(out N1, ref v1, ref v2);
      N1.X = v1.Y * v2.Z - v1.Z * v2.Y;
      N1.Y = v1.Z * v2.X - v1.X * v2.Z;
      N1.Z = v1.X * v2.Y - v1.Y * v2.X;
      //SUB(out v1, ref r2, ref p1);
      v1.X = r2.X - p1.X;
      v1.Y = r2.Y - p1.Y;
      v1.Z = r2.Z - p1.Z;
      //if (DOT(ref v1, ref N1) > 0.0f)
      if (v1.X * N1.X + v1.Y * N1.Y + v1.Z * N1.Z > 0.0f)
        return false;

      return true;
    }
    #endregion


    //--------------------------------------------------------------
    #region 3D Overlap Tests
    //--------------------------------------------------------------

    private static bool coplanar_tri_tri3d(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
                                           ref Vector3F p2, ref Vector3F q2, ref Vector3F r2,
                                           ref Vector3F normal_1)
    {
      // Projection of the triangles in 3D onto 2D such that the area of the projection is maximized. 
      // Then perform 2D test.

      Vector2F P1, Q1, R1;
      Vector2F P2, Q2, R2;

      float n_x, n_y, n_z;
      n_x = ((normal_1.X < 0) ? -normal_1.X : normal_1.X);
      n_y = ((normal_1.Y < 0) ? -normal_1.Y : normal_1.Y);
      n_z = ((normal_1.Z < 0) ? -normal_1.Z : normal_1.Z);

      if ((n_x > n_z) && (n_x >= n_y))
      {
        // Project onto plane YZ

        P1.X = q1.Z; P1.Y = q1.Y;
        Q1.X = p1.Z; Q1.Y = p1.Y;
        R1.X = r1.Z; R1.Y = r1.Y;

        P2.X = q2.Z; P2.Y = q2.Y;
        Q2.X = p2.Z; Q2.Y = p2.Y;
        R2.X = r2.Z; R2.Y = r2.Y;

      }
      else if ((n_y > n_z) && (n_y >= n_x))
      {
        // Project onto plane XZ

        P1.X = q1.X; P1.Y = q1.Z;
        Q1.X = p1.X; Q1.Y = p1.Z;
        R1.X = r1.X; R1.Y = r1.Z;

        P2.X = q2.X; P2.Y = q2.Z;
        Q2.X = p2.X; Q2.Y = p2.Z;
        R2.X = r2.X; R2.Y = r2.Z;

      }
      else
      {
        // Project onto plane XY

        P1.X = p1.X; P1.Y = p1.Y;
        Q1.X = q1.X; Q1.Y = q1.Y;
        R1.X = r1.X; R1.Y = r1.Y;

        P2.X = p2.X; P2.Y = p2.Y;
        Q2.X = q2.X; Q2.Y = q2.Y;
        R2.X = r2.X; R2.Y = r2.Y;
      }

      return tri_tri_overlap_test_2d(ref P1, ref Q1, ref R1, ref P2, ref Q2, ref R2);
    }


    private static bool TRI_TRI_3D(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
                                   ref Vector3F p2, ref Vector3F q2, ref Vector3F r2,
                                   float dp2, float dq2, float dr2, ref Vector3F N1)
    {
      // Permutation in a canonical form of T2's vertices
      if (dp2 > 0.0f)
      {
        if (dq2 > 0.0f)
          return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref r2, ref p2, ref q2);
        else if (dr2 > 0.0f)
          return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref q2, ref r2, ref p2);
        else
          return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
      }
      else if (dp2 < 0.0f)
      {
        if (dq2 < 0.0f)
          return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2);
        else if (dr2 < 0.0f)
          return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2);
        else
          return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2);
      }
      else
      {
        if (dq2 < 0.0f)
        {
          if (dr2 >= 0.0f)
            return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref q2, ref r2, ref p2);
          else
            return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
        }
        else if (dq2 > 0.0f)
        {
          if (dr2 > 0.0f)
            return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2);
          else
            return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2);
        }
        else
        {
          if (dr2 > 0.0f)
            return CHECK_MIN_MAX(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2);
          else if (dr2 < 0.0f)
            return CHECK_MIN_MAX(ref p1, ref r1, ref q1, ref r2, ref p2, ref q2);
          else
            return coplanar_tri_tri3d(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1);
        }
      }
    }


    /// <summary>
    /// Determines whether two triangles overlap.
    /// </summary>
    /// <param name="triangle0">The first triangle.</param>
    /// <param name="triangle1">The second triangle.</param>
    /// <returns>
    /// <see langword="true" /> if the specified triangles intersect;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool HaveContact(Triangle triangle0, Triangle triangle1)
    {
      return HaveContact(ref triangle0, ref triangle1);
    }


    /// <summary>
    /// Determines whether two triangles overlap.
    /// </summary>
    /// <param name="triangle0">The first triangle. (The triangle will not be modified.)</param>
    /// <param name="triangle1">The second triangle. (The triangle will not be modified.)</param>
    /// <returns>
    /// <see langword="true" /> if the specified triangles intersect;
    /// otherwise, <see langword="false" />.
    /// </returns>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public static bool HaveContact(ref Triangle triangle0, ref Triangle triangle1)
    {
      // tri_tri_overlap_test_3d

      float dp1, dq1, dr1, dp2, dq2, dr2;
      Vector3F v1, v2;
      Vector3F N1, N2;

      // Compute normal of triangle 2.
      //SUB(out v1, ref p2, ref r2);
      v1.X = triangle1.Vertex0.X - triangle1.Vertex2.X;
      v1.Y = triangle1.Vertex0.Y - triangle1.Vertex2.Y;
      v1.Z = triangle1.Vertex0.Z - triangle1.Vertex2.Z;
      //SUB(out v2, ref q2, ref r2);
      v2.X = triangle1.Vertex1.X - triangle1.Vertex2.X;
      v2.Y = triangle1.Vertex1.Y - triangle1.Vertex2.Y;
      v2.Z = triangle1.Vertex1.Z - triangle1.Vertex2.Z;
      //CROSS(out N2, ref v1, ref v2);
      N2.X = v1.Y * v2.Z - v1.Z * v2.Y;
      N2.Y = v1.Z * v2.X - v1.X * v2.Z;
      N2.Z = v1.X * v2.Y - v1.Y * v2.X;

      // Compute distance signs of triangle 1 vertices to triangle 2. 
      //SUB(out v1, ref p1, ref r2);
      v1.X = triangle0.Vertex0.X - triangle1.Vertex2.X;
      v1.Y = triangle0.Vertex0.Y - triangle1.Vertex2.Y;
      v1.Z = triangle0.Vertex0.Z - triangle1.Vertex2.Z;
      //dp1 = DOT(ref v1, ref N2);
      dp1 = v1.X * N2.X + v1.Y * N2.Y + v1.Z * N2.Z;
      //SUB(out v1, ref q1, ref r2);
      v1.X = triangle0.Vertex1.X - triangle1.Vertex2.X;
      v1.Y = triangle0.Vertex1.Y - triangle1.Vertex2.Y;
      v1.Z = triangle0.Vertex1.Z - triangle1.Vertex2.Z;
      //dq1 = DOT(ref v1, ref N2);
      dq1 = v1.X * N2.X + v1.Y * N2.Y + v1.Z * N2.Z;
      //SUB(out v1, ref r1, ref r2);
      v1.X = triangle0.Vertex2.X - triangle1.Vertex2.X;
      v1.Y = triangle0.Vertex2.Y - triangle1.Vertex2.Y;
      v1.Z = triangle0.Vertex2.Z - triangle1.Vertex2.Z;
      //dr1 = DOT(ref v1, ref N2);
      dr1 = v1.X * N2.X + v1.Y * N2.Y + v1.Z * N2.Z;

      // No contact if all triangle 1 vertices are on the same side of the triangle 2 plane.
      if (((dp1 * dq1) > 0.0f) && ((dp1 * dr1) > 0.0f))
        return false;

      // Compute normal of triangle 1.
      //SUB(out v1, ref q1, ref p1);
      v1.X = triangle0.Vertex1.X - triangle0.Vertex0.X;
      v1.Y = triangle0.Vertex1.Y - triangle0.Vertex0.Y;
      v1.Z = triangle0.Vertex1.Z - triangle0.Vertex0.Z;
      //SUB(out v2, ref r1, ref p1);
      v2.X = triangle0.Vertex2.X - triangle0.Vertex0.X;
      v2.Y = triangle0.Vertex2.Y - triangle0.Vertex0.Y;
      v2.Z = triangle0.Vertex2.Z - triangle0.Vertex0.Z;
      //CROSS(out N1, ref v1, ref v2);
      N1.X = v1.Y * v2.Z - v1.Z * v2.Y;
      N1.Y = v1.Z * v2.X - v1.X * v2.Z;
      N1.Z = v1.X * v2.Y - v1.Y * v2.X;

      // Compute distance signs of triangle 2 vertices to triangle 1. 
      //SUB(out v1, ref p2, ref r1);
      v1.X = triangle1.Vertex0.X - triangle0.Vertex2.X;
      v1.Y = triangle1.Vertex0.Y - triangle0.Vertex2.Y;
      v1.Z = triangle1.Vertex0.Z - triangle0.Vertex2.Z;
      //dp2 = DOT(ref v1, ref N1);
      dp2 = v1.X * N1.X + v1.Y * N1.Y + v1.Z * N1.Z;
      //SUB(out v1, ref q2, ref r1);
      v1.X = triangle1.Vertex1.X - triangle0.Vertex2.X;
      v1.Y = triangle1.Vertex1.Y - triangle0.Vertex2.Y;
      v1.Z = triangle1.Vertex1.Z - triangle0.Vertex2.Z;
      //dq2 = DOT(ref v1, ref N1);
      dq2 = v1.X * N1.X + v1.Y * N1.Y + v1.Z * N1.Z;
      //SUB(out v1, ref r2, ref r1);
      v1.X = triangle1.Vertex2.X - triangle0.Vertex2.X;
      v1.Y = triangle1.Vertex2.Y - triangle0.Vertex2.Y;
      v1.Z = triangle1.Vertex2.Z - triangle0.Vertex2.Z;
      //dr2 = DOT(ref v1, ref N1);
      dr2 = v1.X * N1.X + v1.Y * N1.Y + v1.Z * N1.Z;

      // No contact if all triangle 2 vertices are on the same side of the triangle 1 plane.
      if (((dp2 * dq2) > 0.0f) && ((dp2 * dr2) > 0.0f))
        return false;

      /* Permutation in a canonical form of T1's vertices */

      if (dp1 > 0.0f)
      {
        if (dq1 > 0.0f)
          return TRI_TRI_3D(ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
        else if (dr1 > 0.0f)
          return TRI_TRI_3D(ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
        else
          return TRI_TRI_3D(ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
      }
      else if (dp1 < 0.0f)
      {
        if (dq1 < 0.0f)
          return TRI_TRI_3D(ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
        else if (dr1 < 0.0f)
          return TRI_TRI_3D(ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
        else
          return TRI_TRI_3D(ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
      }
      else
      {
        if (dq1 < 0.0f)
        {
          if (dr1 >= 0.0f)
            return TRI_TRI_3D(ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
          else
            return TRI_TRI_3D(ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
        }
        else if (dq1 > 0.0f)
        {
          if (dr1 > 0.0f)
            return TRI_TRI_3D(ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
          else
            return TRI_TRI_3D(ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
        }
        else
        {
          if (dr1 > 0.0f)
            return TRI_TRI_3D(ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, dp2, dq2, dr2, ref N1);
          else if (dr1 < 0.0f)
            return TRI_TRI_3D(ref triangle0.Vertex2, ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle1.Vertex0, ref triangle1.Vertex2, ref triangle1.Vertex1, dp2, dr2, dq2, ref N1);
          else
            return coplanar_tri_tri3d(ref triangle0.Vertex0, ref triangle0.Vertex1, ref triangle0.Vertex2, ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2, ref N1);
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region 3D Intersection Tests
    //--------------------------------------------------------------

    //// This method is called when the triangles surely intersect
    //// It constructs the segment of intersection of the two triangles
    //// if they are not coplanar.
    //private static int CONSTRUCT_INTERSECTION(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
    //                                          ref Vector3F p2, ref Vector3F q2, ref Vector3F r2,
    //                                          ref Vector3F N1, ref Vector3F N2,
    //                                          out Vector3F source, out Vector3F target)
    //{
    //  Vector3F v1, v2, N, v;
    //  float alpha;
    //  SUB(out v1, ref q1, ref p1);
    //  SUB(out v2, ref r2, ref p1);
    //  CROSS(out N, ref v1, ref v2);
    //  SUB(out v, ref p2, ref p1);
    //  if (DOT(ref v, ref N) > 0.0f)
    //  {
    //    SUB(out v1, ref r1, ref p1);
    //    CROSS(out N, ref v1, ref v2);
    //    if (DOT(ref v, ref N) <= 0.0f)
    //    {
    //      SUB(out v2, ref q2, ref p1);
    //      CROSS(out N, ref v1, ref v2);
    //      if (DOT(ref v, ref N) > 0.0f)
    //      {
    //        SUB(out v1, ref p1, ref p2);
    //        SUB(out v2, ref p1, ref r1);
    //        alpha = DOT(ref v1, ref N2) / DOT(ref v2, ref N2);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out source, ref p1, ref v1);
    //        SUB(out v1, ref p2, ref p1);
    //        SUB(out v2, ref p2, ref r2);
    //        alpha = DOT(ref v1, ref N1) / DOT(ref v2, ref N1);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out target, ref p2, ref v1);
    //        return 1;
    //      }
    //      else
    //      {
    //        SUB(out v1, ref p2, ref p1);
    //        SUB(out v2, ref p2, ref q2);
    //        alpha = DOT(ref v1, ref N1) / DOT(ref v2, ref N1);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out source, ref p2, ref v1);
    //        SUB(out v1, ref p2, ref p1);
    //        SUB(out v2, ref p2, ref r2);
    //        alpha = DOT(ref v1, ref N1) / DOT(ref v2, ref N1);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out target, ref p2, ref v1);
    //        return 1;
    //      }
    //    }
    //    else
    //    {
    //      source = Vector3F.Zero;
    //      target = Vector3F.Zero;
    //      return 0;
    //    }
    //  }
    //  else
    //  {
    //    SUB(out v2, ref q2, ref p1);
    //    CROSS(out N, ref v1, ref v2);
    //    if (DOT(ref v, ref N) < 0.0f)
    //    {
    //      source = Vector3F.Zero;
    //      target = Vector3F.Zero;
    //      return 0;
    //    }
    //    else
    //    {
    //      SUB(out v1, ref r1, ref p1);
    //      CROSS(out N, ref v1, ref v2);
    //      if (DOT(ref v, ref N) >= 0.0f)
    //      {
    //        SUB(out v1, ref p1, ref p2);
    //        SUB(out v2, ref p1, ref r1);
    //        alpha = DOT(ref v1, ref N2) / DOT(ref v2, ref N2);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out source, ref p1, ref v1);
    //        SUB(out v1, ref p1, ref p2);
    //        SUB(out v2, ref p1, ref q1);
    //        alpha = DOT(ref v1, ref N2) / DOT(ref v2, ref N2);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out target, ref p1, ref v1);
    //        return 1;
    //      }
    //      else
    //      {
    //        SUB(out v1, ref p2, ref p1);
    //        SUB(out v2, ref p2, ref q2);
    //        alpha = DOT(ref v1, ref N1) / DOT(ref v2, ref N1);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out source, ref p2, ref v1);
    //        SUB(out v1, ref p1, ref p2);
    //        SUB(out v2, ref p1, ref q1);
    //        alpha = DOT(ref v1, ref N2) / DOT(ref v2, ref N2);
    //        SCALAR(out v1, alpha, ref v2);
    //        SUB(out target, ref p1, ref v1);
    //        return 1;
    //      }
    //    }
    //  }
    //}


    //private static int TRI_TRI_INTER_3D(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
    //                                    ref Vector3F p2, ref Vector3F q2, ref Vector3F r2,
    //                                    ref Vector3F N1, ref Vector3F N2,
    //                                    float dp2, float dq2, float dr2,
    //                                    out Vector3F source, out Vector3F target, out int coplanar)
    //{
    //  coplanar = 0;

    //  if (dp2 > 0.0f)
    //  {
    //    if (dq2 > 0.0f)
    //      return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref r2, ref p2, ref q2, ref N1, ref N2, out source, out target);
    //    else if (dr2 > 0.0f)
    //      return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref q2, ref r2, ref p2, ref N1, ref N2, out source, out target);
    //    else
    //      return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1, ref N2, out source, out target);
    //  }
    //  else if (dp2 < 0.0f)
    //  {
    //    if (dq2 < 0.0f)
    //      return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2, ref N1, ref N2, out source, out target);
    //    else if (dr2 < 0.0f)
    //      return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2, ref N1, ref N2, out source, out target);
    //    else
    //      return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2, ref N1, ref N2, out source, out target);
    //  }
    //  else
    //  {
    //    if (dq2 < 0.0f)
    //    {
    //      if (dr2 >= 0.0f)
    //        return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref q2, ref r2, ref p2, ref N1, ref N2, out source, out target);
    //      else
    //        return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1, ref N2, out source, out target);
    //    }
    //    else if (dq2 > 0.0f)
    //    {
    //      if (dr2 > 0.0f)
    //        return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2, ref N1, ref N2, out source, out target);
    //      else
    //        return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2, ref N1, ref N2, out source, out target);
    //    }
    //    else
    //    {
    //      if (dr2 > 0.0f)
    //        return CONSTRUCT_INTERSECTION(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2, ref N1, ref N2, out source, out target);
    //      else if (dr2 < 0.0f)
    //        return CONSTRUCT_INTERSECTION(ref p1, ref r1, ref q1, ref r2, ref p2, ref q2, ref N1, ref N2, out source, out target);
    //      else
    //      {
    //        source = Vector3F.Zero;
    //        target = Vector3F.Zero;
    //        coplanar = 1;
    //        return coplanar_tri_tri3d(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1);
    //      }
    //    }
    //  }
    //}


    //// The following version computes the segment of intersection of the
    //// two triangles if it exists. 
    //// coplanar returns whether the triangles are coplanar
    //// source and target are the endpoints of the line segment of intersection 
    //public static int tri_tri_intersection_test_3d(ref Vector3F p1, ref Vector3F q1, ref Vector3F r1,
    //                                               ref Vector3F p2, ref Vector3F q2, ref Vector3F r2,
    //                                               ref int coplanar, out Vector3F source, out Vector3F target)
    //{
    //  float dp1, dq1, dr1, dp2, dq2, dr2;
    //  Vector3F v1, v2;
    //  Vector3F N1, N2;

    //  // Compute distance signs  of p1, q1 and r1 
    //  // to the plane of triangle(p2,q2,r2)

    //  SUB(out v1, ref p2, ref r2);
    //  SUB(out v2, ref q2, ref r2);
    //  CROSS(out N2, ref v1, ref v2);

    //  SUB(out v1, ref p1, ref r2);
    //  dp1 = DOT(ref v1, ref N2);
    //  SUB(out v1, ref q1, ref r2);
    //  dq1 = DOT(ref v1, ref N2);
    //  SUB(out v1, ref r1, ref r2);
    //  dr1 = DOT(ref v1, ref N2);

    //  if (((dp1 * dq1) > 0.0f) && ((dp1 * dr1) > 0.0f))
    //  {
    //    source = Vector3F.Zero;
    //    target = Vector3F.Zero;
    //    return 0;
    //  }

    //  // Compute distance signs  of p2, q2 and r2 
    //  // to the plane of triangle(p1,q1,r1)


    //  SUB(out v1, ref q1, ref p1);
    //  SUB(out v2, ref r1, ref p1);
    //  CROSS(out N1, ref v1, ref v2);

    //  SUB(out v1, ref p2, ref r1);
    //  dp2 = DOT(ref v1, ref N1);
    //  SUB(out v1, ref q2, ref r1);
    //  dq2 = DOT(ref v1, ref N1);
    //  SUB(out v1, ref r2, ref r1);
    //  dr2 = DOT(ref v1, ref N1);

    //  if (((dp2 * dq2) > 0.0f) && ((dp2 * dr2) > 0.0f))
    //  {
    //    source = Vector3F.Zero;
    //    target = Vector3F.Zero;
    //    return 0;
    //  }

    //  // Permutation in a canonical form of T1's vertices
    //  if (dp1 > 0.0f)
    //  {
    //    if (dq1 > 0.0f)
    //      return TRI_TRI_INTER_3D(ref r1, ref p1, ref q1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //    else if (dr1 > 0.0f)
    //      return TRI_TRI_INTER_3D(ref q1, ref r1, ref p1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //    else
    //      return TRI_TRI_INTER_3D(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //  }
    //  else if (dp1 < 0.0f)
    //  {
    //    if (dq1 < 0.0f)
    //      return TRI_TRI_INTER_3D(ref r1, ref p1, ref q1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //    else if (dr1 < 0.0f)
    //      return TRI_TRI_INTER_3D(ref q1, ref r1, ref p1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //    else
    //      return TRI_TRI_INTER_3D(ref p1, ref q1, ref r1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //  }
    //  else
    //  {
    //    if (dq1 < 0.0f)
    //    {
    //      if (dr1 >= 0.0f)
    //        return TRI_TRI_INTER_3D(ref q1, ref r1, ref p1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //      else
    //        return TRI_TRI_INTER_3D(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //    }
    //    else if (dq1 > 0.0f)
    //    {
    //      if (dr1 > 0.0f)
    //        return TRI_TRI_INTER_3D(ref p1, ref q1, ref r1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //      else
    //        return TRI_TRI_INTER_3D(ref q1, ref r1, ref p1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //    }
    //    else
    //    {
    //      if (dr1 > 0.0f)
    //        return TRI_TRI_INTER_3D(ref r1, ref p1, ref q1, ref p2, ref q2, ref r2, ref N1, ref N2, dp2, dq2, dr2, out source, out target, out coplanar);
    //      else if (dr1 < 0.0f)
    //        return TRI_TRI_INTER_3D(ref r1, ref p1, ref q1, ref p2, ref r2, ref q2, ref N1, ref N2, dp2, dr2, dq2, out source, out target, out coplanar);
    //      else
    //      {
    //        // triangles are co-planar
    //        source = Vector3F.Zero;
    //        target = Vector3F.Zero;
    //        coplanar = 1;
    //        return coplanar_tri_tri3d(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2, ref N1);
    //      }
    //    }
    //  }
    //}
    #endregion

    
    //--------------------------------------------------------------
    #region 2D Overlap Tests
    //--------------------------------------------------------------

    private static float ORIENT_2D(ref Vector2F a, ref Vector2F b, ref Vector2F c)
    {
      return ((a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X));
    }


    private static bool INTERSECTION_TEST_VERTEX(ref Vector2F P1, ref Vector2F Q1, ref Vector2F R1,
                                                 ref Vector2F P2, ref Vector2F Q2, ref Vector2F R2)
    {
      if (ORIENT_2D(ref R2, ref P2, ref Q1) >= 0.0f)
      {
        if (ORIENT_2D(ref R2, ref Q2, ref Q1) <= 0.0f)
        {
          if (ORIENT_2D(ref P1, ref P2, ref Q1) > 0.0f)
          {
            if (ORIENT_2D(ref P1, ref Q2, ref Q1) <= 0.0f)
              return true;
            else
              return false;
          }
          else
          {
            if (ORIENT_2D(ref P1, ref P2, ref R1) >= 0.0f)
              if (ORIENT_2D(ref Q1, ref R1, ref P2) >= 0.0f)
                return true;
              else
                return false;
            else
              return false;
          }
        }
        else if (ORIENT_2D(ref P1, ref Q2, ref Q1) <= 0.0f)
        {
          if (ORIENT_2D(ref R2, ref Q2, ref R1) <= 0.0f)
          {
            if (ORIENT_2D(ref Q1, ref R1, ref Q2) >= 0.0f)
              return true;
            else
              return false;
          }
          else
          {
            return false;
          }
        }
        else
        {
          return false;
        }
      }
      else if (ORIENT_2D(ref R2, ref P2, ref R1) >= 0.0f)
      {
        if (ORIENT_2D(ref Q1, ref R1, ref R2) >= 0.0f)
        {
          if (ORIENT_2D(ref P1, ref P2, ref R1) >= 0.0f)
            return true;
          else
            return false;
        }
        else if (ORIENT_2D(ref Q1, ref R1, ref Q2) >= 0.0f)
        {
          if (ORIENT_2D(ref R2, ref R1, ref Q2) >= 0.0f)
            return true;
          else
            return false;
        }
        else
        {
          return false;
        }
      }
      else
      {
        return false;
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "Q2")]
    private static bool INTERSECTION_TEST_EDGE(ref Vector2F P1, ref Vector2F Q1, ref Vector2F R1,
                                               ref Vector2F P2, ref Vector2F Q2, ref Vector2F R2)
    {
      if (ORIENT_2D(ref R2, ref P2, ref Q1) >= 0.0f)
      {
        if (ORIENT_2D(ref P1, ref P2, ref Q1) >= 0.0f)
        {
          if (ORIENT_2D(ref P1, ref Q1, ref R2) >= 0.0f)
            return true;
          else
            return false;
        }
        else
        {
          if (ORIENT_2D(ref Q1, ref R1, ref P2) >= 0.0f)
          {
            if (ORIENT_2D(ref R1, ref P1, ref P2) >= 0.0f)
              return true;
            else
              return false;
          }
          else
          {
            return false;
          }
        }
      }
      else
      {
        if (ORIENT_2D(ref R2, ref P2, ref R1) >= 0.0f)
        {
          if (ORIENT_2D(ref P1, ref P2, ref R1) >= 0.0f)
          {
            if (ORIENT_2D(ref P1, ref R1, ref R2) >= 0.0f)
            {
              return true;
            }
            else
            {
              if (ORIENT_2D(ref Q1, ref R1, ref R2) >= 0.0f)
                return true;
              else
                return false;
            }
          }
          else
          {
            return false;
          }
        }
        else
        {
          return false;
        }
      }
    }


    private static bool ccw_tri_tri_intersection_2d(ref Vector2F p1, ref Vector2F q1, ref Vector2F r1,
                                                    ref Vector2F p2, ref Vector2F q2, ref Vector2F r2)
    {
      if (ORIENT_2D(ref p2, ref q2, ref p1) >= 0.0f)
      {
        if (ORIENT_2D(ref q2, ref r2, ref p1) >= 0.0f)
        {
          if (ORIENT_2D(ref r2, ref p2, ref p1) >= 0.0f)
            return true;
          else
            return INTERSECTION_TEST_EDGE(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
        }
        else
        {
          if (ORIENT_2D(ref r2, ref p2, ref p1) >= 0.0f)
            return INTERSECTION_TEST_EDGE(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2);
          else
            return INTERSECTION_TEST_VERTEX(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
        }
      }
      else
      {
        if (ORIENT_2D(ref q2, ref r2, ref p1) >= 0.0f)
        {
          if (ORIENT_2D(ref r2, ref p2, ref p1) >= 0.0f)
            return INTERSECTION_TEST_EDGE(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2);
          else
            return INTERSECTION_TEST_VERTEX(ref p1, ref q1, ref r1, ref q2, ref r2, ref p2);
        }
        else
          return INTERSECTION_TEST_VERTEX(ref p1, ref q1, ref r1, ref r2, ref p2, ref q2);
      }
    }


    private static bool tri_tri_overlap_test_2d(ref Vector2F p1, ref Vector2F q1, ref Vector2F r1,
                                                ref Vector2F p2, ref Vector2F q2, ref Vector2F r2)
    {
      if (ORIENT_2D(ref p1, ref q1, ref r1) < 0.0f)
        if (ORIENT_2D(ref p2, ref q2, ref r2) < 0.0f)
          return ccw_tri_tri_intersection_2d(ref p1, ref r1, ref q1, ref p2, ref r2, ref q2);
        else
          return ccw_tri_tri_intersection_2d(ref p1, ref r1, ref q1, ref p2, ref q2, ref r2);
      else
        if (ORIENT_2D(ref p2, ref q2, ref r2) < 0.0f)
          return ccw_tri_tri_intersection_2d(ref p1, ref q1, ref r1, ref p2, ref r2, ref q2);
        else
          return ccw_tri_tri_intersection_2d(ref p1, ref q1, ref r1, ref p2, ref q2, ref r2);
    }
    #endregion

// ReSharper restore IdentifierWordIsNotInDictionary
  }
}
