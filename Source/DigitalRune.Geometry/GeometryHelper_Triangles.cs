// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    // Notes:
    // Part of the code is based on 
    //   "Real-Time Collision Detection by Christer Ericson, published by Morgan Kaufmann Publishers, (C) 2005 Elevier Inc"
    // For Barycentric coordinates see pp. 51
    // For closest points on triangle see p. 141


    /// <overloads>
    /// <summary>
    /// Gets the barycentric coordinates of a point and a triangle.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the barycentric coordinates of a point and a triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point.</param>
    /// <param name="u">The barycentric coordinate u.</param>
    /// <param name="v">The barycentric coordinate v.</param>
    /// <param name="w">The barycentric coordinate w.</param>
    /// <remarks>
    /// The <paramref name="point"/> is projected into the plane of the <paramref name="triangle"/>
    /// and the barycentric coordinates are computed for the project point.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void GetBarycentricFromPoint(Triangle triangle, Vector3F point, out float u, out float v, out float w)
    {
      Vector3F v0 = triangle.Vertex0;
      Vector3F v1 = triangle.Vertex1;
      Vector3F v2 = triangle.Vertex2;

      Vector3F ab = v1 - v0;
      Vector3F ac = v2 - v0;
      Vector3F ap = point - v0;
      Vector3F bp = point - v1;
      Vector3F cp = point - v2;
      float d1 = Vector3F.Dot(ab, ap);
      float d2 = Vector3F.Dot(ac, ap);
      float d3 = Vector3F.Dot(ab, bp);
      float d4 = Vector3F.Dot(ac, bp);
      float d5 = Vector3F.Dot(ab, cp);
      float d6 = Vector3F.Dot(ac, cp);
      float va = d3 * d6 - d5 * d4;
      float vb = d5 * d2 - d1 * d6;
      float vc = d1 * d4 - d3 * d2;
      float denominator = 1 / (va + vb + vc);
      u = va * denominator;
      v = vb * denominator;
      w = vc * denominator;

      #region ----- Triangle vs Point in Triangle Plane -----
      //// Following code can be used if point is in the triangle plane.
      //// Unnormalized triangle normal.
      //Vector3F m = Vector3F.Cross(v1 - v0, v2 - v0);

      //// Nominators and one-over-denominator for u and v ratios
      //float nu, nv, ood;

      //// Absolute normal components for determining projection plane.
      //Vector3F mAbs = Vector3F.Absolute(m);

      //// Compute area in plane of targets projection (to avoid degeneracies)
      //if (mAbs.X >= mAbs.Y && mAbs.X >= mAbs.Z)
      //{
      //  // Project to yz plane.

      //  // Area of PBC in yz plane.
      //  nu = GetTriangleArea2D(point.Y, point.Z, v1.Y, v1.Z, v2.Y, v2.Z);

      //  // Area of PCA in yz plane.
      //  nv = GetTriangleArea2D(point.Y, point.Z, v2.Y, v2.Z, v0.Y, v0.Z);

      //  // 1 / (2 * area of ABC in yz plane).
      //  ood = 1 / m.X;
      //}
      //else if (mAbs.Y >= mAbs.X && mAbs.Y >= mAbs.Z)
      //{
      //  // Project to xz plane.

      //  // Area of PBC in xz plane.
      //  nu = GetTriangleArea2D(point.X, point.Z, v1.X, v1.Z, v2.X, v2.Z);

      //  // Area of PCA in xz plane.
      //  nv = GetTriangleArea2D(point.X, point.Z, v2.X, v2.Z, v0.X, v0.Z);

      //  // 1 / (2 * area of ABC in yz plane).
      //  ood = 1 / -m.Y;
      //}
      //else
      //{
      //  // Project to xy plane.

      //  // Area of PBC in xy plane.
      //  nu = GetTriangleArea2D(point.X, point.Y, v1.X, v1.Y, v2.X, v2.Y);

      //  // Area of PCA in xy plane.
      //  nv = GetTriangleArea2D(point.X, point.Y, v2.X, v2.Y, v0.X, v0.Y);

      //  // 1 / (2 * area of ABC in yz plane).
      //  ood = 1 / m.Z;
      //}

      //u = nu * ood;
      //v = nv * ood;
      //w = 1 - u - v;
      #endregion
    }


    /// <summary>
    /// Determines whether the projection of a point (into the triangle plane) is inside the given
    /// triangle. (This overload uses per-reference parameters for performance.)
    /// </summary>
    /// <inheritdoc cref="GetBarycentricFromPoint(Triangle,Vector3F,out float,out float,out float)"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [CLSCompliant(false)]
    public static void GetBarycentricFromPoint(ref Triangle triangle, ref Vector3F point, out float u, out float v, out float w)
    {
      Vector3F v0 = triangle.Vertex0;
      Vector3F v1 = triangle.Vertex1;
      Vector3F v2 = triangle.Vertex2;

      Vector3F ab = v1 - v0;
      Vector3F ac = v2 - v0;
      Vector3F ap = point - v0;
      Vector3F bp = point - v1;
      Vector3F cp = point - v2;
      float d1 = Vector3F.Dot(ab, ap);
      float d2 = Vector3F.Dot(ac, ap);
      float d3 = Vector3F.Dot(ab, bp);
      float d4 = Vector3F.Dot(ac, bp);
      float d5 = Vector3F.Dot(ab, cp);
      float d6 = Vector3F.Dot(ac, cp);
      float va = d3 * d6 - d5 * d4;
      float vb = d5 * d2 - d1 * d6;
      float vc = d1 * d4 - d3 * d2;
      float denominator = 1 / (va + vb + vc);
      u = va * denominator;
      v = vb * denominator;
      w = vc * denominator;
    }


    //// Returns a value proportional to the triangle area of a 2D triangle.
    //private static float GetTriangleArea2D(float x1, float y1, float x2, float y2, float x3, float y3)
    //{
    //  return (x1 - x2) * (y2 - y3) - (x2 - x3) * (y1 - y2);
    //}


    /// <summary>
    /// Computes the ray vs. triangle contact.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="isTwoSided">
    /// if set to <see langword="true"/> the triangle is treated as a two-sided triangle. Ray
    /// contacts with the back-side of a one-sided triangle are not reported.
    /// </param>
    /// <param name="ray">The ray.</param>
    /// <param name="hitDistance">
    /// The hit distance. This is the distance on the ray from the ray origin to the contact point.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the ray hits the triangle; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool GetContact(Ray ray, Triangle triangle, bool isTwoSided, out float hitDistance)
    {
      // This code is also used inline in RayHeightFieldAlgorithm and RayTriangleAlgorithm. Sync changes!
      hitDistance = float.NaN;

      Vector3F v0 = triangle.Vertex0;
      Vector3F v1 = triangle.Vertex1;
      Vector3F v2 = triangle.Vertex2;

      Vector3F d1 = (v1 - v0);
      Vector3F d2 = (v2 - v0);
      Vector3F n = Vector3F.Cross(d1, d2);
      
      // Tolerance value, see SOLID, Bergen: "Collision Detection in Interactive 3D Environments".
      float ε = n.Length * Numeric.EpsilonFSquared;

      Vector3F r = ray.Direction * ray.Length;

      float δ = -Vector3F.Dot(r, n);

      // Degenerate triangle --> No hit.
      if (ε == 0.0f || Numeric.IsZero(δ, ε))
        return false;

      // Shooting into back face? 
      if (!isTwoSided && δ < 0)
        return false;

      Vector3F triangleToRayOrigin = ray.Origin - v0;
      float λ = Vector3F.Dot(triangleToRayOrigin, n) / δ;
      hitDistance = λ * ray.Length;
      if (λ < 0 || λ > 1)
        return false;

      // The ray hit the triangle plane.
      Vector3F u = Vector3F.Cross(triangleToRayOrigin, r);
      float μ1 = Vector3F.Dot(d2, u) / δ;
      float μ2 = Vector3F.Dot(-d1, u) / δ;
      if (μ1 + μ2 <= 1 + ε && μ1 >= -ε && μ2 >= -ε)
      {
        // Hit!
        // Hit Normal: 
        //n.Normalize();
        //if (δ > 0)
        //  n = -n;

        return true;
      }
      else
      {
        return false;
      }  
    }


    /// <summary>
    /// Gets the point on the triangle defined by the given barycentric coordinates.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="u">The barycentric coordinate u.</param>
    /// <param name="v">The barycentric coordinate v.</param>
    /// <param name="w">The barycentric coordinate w.</param>
    /// <returns>
    /// The point computes as 
    ///   <paramref name="u"/> * <see cref="Triangle.Vertex0"/> 
    ///   + <paramref name="v"/> * <see cref="Triangle.Vertex1"/> 
    ///   + <paramref name="w"/> * <see cref="Triangle.Vertex2"/>.
    /// </returns>
    /// <remarks>
    /// The barycentric coordinates are not clamped. The resulting point can lie outside of the
    /// triangle.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Vector3F GetPointFromBarycentric(Triangle triangle, float u, float v, float w)
    {
      return u * triangle.Vertex0 + v * triangle.Vertex1 + w * triangle.Vertex2;
    }



    /// <summary>
    /// Determines whether the point is in front of the triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point.</param>
    /// <returns>
    /// A value greater than 0 if the point is in front. A value less than 0 if the point is on the
    /// back-side.
    /// </returns>
    /// <remarks>
    /// A point is in front of the triangle if the vertex order of the triangle viewed from the
    /// point position is counter-clockwise (CCW). The absolute of the returned value is linearly
    /// proportional to the distance from the triangle plane.
    /// </remarks>
    public static float IsInFront(Triangle triangle, Vector3F point)
    {
      return Vector3F.Dot(triangle.Normal, point - triangle.Vertex0);
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the projection of a point (into the triangle plane) is inside the given
    /// triangle.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the projection of a point (into the triangle plane) is inside the given
    /// triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point.</param>
    /// <returns>
    /// <see langword="true"/> if the orthogonal projection of <paramref name="point"/> is inside 
    /// the triangle; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsOver(Triangle triangle, Vector3F point)
    {
      float u, v, w;
      GetBarycentricFromPoint(triangle, point, out u, out v, out w);
      return u >= 0 && v >= 0 && w >= 0;
    }


    /// <summary>
    /// Determines whether the projection of a point (into the triangle plane) is inside the given
    /// triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point.</param>
    /// <param name="epsilon">
    /// The allowed numerical tolerance. This value "increases" the size of the triangle and makes
    /// the check tolerant to numerical errors. Use a very small value, like 0.0001.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the orthogonal projection of <paramref name="point"/> is inside 
    /// the triangle; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsOver(Triangle triangle, Vector3F point, float epsilon)
    {
      float u, v, w;
      GetBarycentricFromPoint(triangle, point, out u, out v, out w);
      return u >= -epsilon && v >= -epsilon && w >= -epsilon;
    }


    /// <summary>
    /// Determines whether the projection of a point (into the triangle plane) is inside the given
    /// triangle. (This overload uses per-reference parameters for performance.)
    /// </summary>
    /// <inheritdoc cref="IsOver(Triangle, Vector3F)"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
    [CLSCompliant(false)]
    public static bool IsOver(ref Triangle triangle, ref Vector3F point)
    {
      float u, v, w;
      GetBarycentricFromPoint(ref triangle, ref point, out u, out v, out w);
      return u >= 0 && v >= 0 && w >= 0;
    }


    /// <summary>
    /// Gets the barycentric coordinates (<paramref name="u"/>, <paramref name="v"/>,
    /// <paramref name="w"/> of the point in a triangle which is closest to
    /// the given <paramref name="point"/>).
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="point">The point.</param>
    /// <param name="u">The barycentric coordinate u.</param>
    /// <param name="v">The barycentric coordinate v.</param>
    /// <param name="w">The barycentric coordinate w.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void GetClosestPoint(Triangle triangle, Vector3F point, out float u, out float v, out float w)
    {
      // See Ericson: "Real-Time Collision Detection", p. 141

      Vector3F v0 = triangle.Vertex0;
      Vector3F v1 = triangle.Vertex1;
      Vector3F v2 = triangle.Vertex2;

      u = v = w = 0;

      // Check if point is in vertex region outside Vertex0.
      Vector3F ab = v1 - v0;
      Vector3F ac = v2 - v0;
      Vector3F ap = point - v0;
      float d1 = Vector3F.Dot(ab, ap);
      float d2 = Vector3F.Dot(ac, ap);
      if (d1 <= 0 && d2 <= 0)
      {
        u = 1;
        return;
      }

      // Check if point is in vertex region outside Vertex1.
      Vector3F bp = point - v1;
      float d3 = Vector3F.Dot(ab, bp);
      float d4 = Vector3F.Dot(ac, bp);
      if (d3 >= 0 && d4 <= d3)
      {
        v = 1;
        return;
      }

      // Check if point is in edge region of AB. If it is
      // return projection of the point onto the edge.
      float vc = d1 * d4 - d3 * d2;
      if (vc <= 0 && d1 >= 0 && d3 <= 0)
      {
        v = d1 / (d1 - d3);
        u = 1 - v;
        return;
      }

      // Check if point is in vertex region outside Vertex2.
      Vector3F cp = point - v2;
      float d5 = Vector3F.Dot(ab, cp);
      float d6 = Vector3F.Dot(ac, cp);
      if (d6 >= 0 && d5 <= d6)
      {
        w = 1;
        return;
      }

      // Check if point is in edge region of AC. If it is
      // return projection of the point onto the edge.
      float vb = d5 * d2 - d1 * d6;
      if (vb <= 0 && d2 >= 0 && d6 <= 0)
      {
        w = d2 / (d2 - d6);
        u = 1 - w;
        return;
      }

      // Check if point is in edge region of BC. If it is
      // return projection of the point onto the edge.
      float va = d3 * d6 - d5 * d4;
      if (va <= 0 && (d4 - d3) >= 0 && (d5 - d6) >= 0)
      {
        w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
        v = 1 - w;
        return;
      }

      // The nearest point is in the triangle.
      float denominator = 1 / (va + vb + vc);
      u = va * denominator;
      v = vb * denominator;
      w = vc * denominator;
    }


    // Old code from EPA algorithm:
    ///// <summary>
    ///// Updates the cached closest-point info from the given vertex array.
    ///// </summary>
    ///// <param name="vertices">The vertex array.</param>
    ///// <returns><see langword="true"/> if the point could be computed; <see langword="false"/> if the 
    ///// triangle is degenerated.</returns>
    //public bool UpdateClosestPointToOrigin(List<Vector3F> vertices)
    //{
    //  // Lets compute lambda1 and lambda2 such that the closest point to the origin x of Triangle (a, b, c)
    //  // is: a + lambda1 * ab + lambda2 * ac = x
    //  // If x is the closest point to the origin, then the vector x is normal to the triangle. Hence,
    //  // it is also normal to the edges:
    //  // ab * x = 0
    //  // ac * x = 0
    //  // Now we substitute x with a + lambda1 * ab + lambda2 * ac.
    //  // We solve the linear system of equations with Cramer's rule... -->
    //  // det = ab²*ac² - (ab*ac)²
    //  // lambda1 = ((a*ac)(ab*ac) - (a*ab)*ac²) / det
    //  // lambda2 = ((ab*ac)(a*ab) - ab²*(a*ac)) / det
    //  // The closest point is in the triangle if lambda1 + lambda2 <= 1
    //  // To avoid the division by det we store det and do not divide the lambdas by det.
    //  // Then the closest point is in the triangle if lambda1 + lambda2 <= det.

    //  Vector3F a = vertices[_indices[0]];
    //  Vector3F b = vertices[_indices[1]];
    //  Vector3F c = vertices[_indices[2]];
    //  Vector3F ab = b - a;
    //  Vector3F ac = c - a;
    //  float ab2 = ab.LengthSquared;
    //  float ac2 = ac.LengthSquared;
    //  float aDotAb = Vector3F.Dot(a, ab);
    //  float aDotAc = Vector3F.Dot(a, ac);
    //  float abDotAc = Vector3F.Dot(ab, ac);

    //  _det = ab2 * ac2 - abDotAc * abDotAc;
    //  _lambda1 = aDotAc * abDotAc - aDotAb * ac2;
    //  _lambda2 = abDotAc * aDotAb - ab2 * aDotAc;

    //  if (_det > Numeric.EpsilonF)
    //  {
    //    _closestPointToOrigin = a + (_lambda1 * ab + _lambda2 * ac) / _det;
    //    _distanceToOriginSquared = _closestPointToOrigin.LengthSquared;
    //    return true;
    //  }

    //  return false;
    //}


    /// <inheritdoc cref="Triangulator.Triangulate(IList{Vector3F},IList{int})"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Indices")]
    public static int Triangulate(IList<Vector3F> polygonVertices, IList<int> triangleIndices)
    {
      return Triangulate(polygonVertices, 0, polygonVertices.Count, triangleIndices);
    }


    /// <inheritdoc cref="Triangulator.Triangulate(IList{Vector3F},int,int,IList{int})"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Indices")]
    public static int Triangulate(IList<Vector3F> polygonVertices, int startIndex, int vertexCount, IList<int> triangleIndices)
    {
      if (vertexCount < 3)
        return 0;

      var triangulator = Triangulator.Create();
      int numberOfTriangles = triangulator.Triangulate(polygonVertices, startIndex, vertexCount, triangleIndices);
      triangulator.Recycle();

      return numberOfTriangles;
    }
  }
}
