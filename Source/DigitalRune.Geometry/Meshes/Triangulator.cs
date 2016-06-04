#region ----- Copyright -----
/* 
    Polygon triangulation is based on the Triangulator written by John W. Ratcliff 
    which is licensed under the MIT license. The code was ported to C#, refactored 
    and documented.

      Copyright (c) 2009 by John W. Ratcliff

      Permission is hereby granted, free of charge, to any person obtaining a copy
      of this software and associated documentation files (the "Software"), to deal
      in the Software without restriction, including without limitation the rights
      to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
      copies of the Software, and to permit persons to whom the Software is furnished
      to do so, subject to the following conditions:

      The above copyright notice and this permission notice shall be included in all
      copies or substantial portions of the Software.

      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
      WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
      CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Triangulates polygons.
  /// </summary>
  /// <remarks>
  /// The <see cref="Triangulator"/> implements the <i>ear clipping algorithm</i>.
  /// </remarks>
  internal class Triangulator
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<Triangulator> Pool =
      new ResourcePool<Triangulator>(
        () => new Triangulator(),
        null,
        null);

    private Vector3F[] _vertices;
    private int[] _indices;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="Triangulator"/> class from being created.
    /// </summary>
    private Triangulator()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="Triangulator"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="Triangulator"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle()"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    public static Triangulator Create()
    {
      return Pool.Obtain();
    }


    /// <summary>
    /// Recycles this instance of the <see cref="Triangulator"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Triangulates a polygon specified by a list of vertices.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Triangulates a polygon specified by a list of vertices.
    /// </summary>
    /// <param name="polygonVertices">The vertices.</param>
    /// <param name="triangleIndices">
    /// The list that stores the resulting triangles: Each value in the list is an index into
    /// <paramref name="polygonVertices"/>; three indices define a triangle.
    /// </param>
    /// <returns>The number of triangles added to <paramref name="triangleIndices"/>.</returns>
    /// <remarks>
    /// <para>
    /// The method supports triangulation of convex and concave polygons. The polygon needs to be 
    /// planar. Either the x, y, or z component of the vertices should be constant.
    /// </para>
    /// <para>
    /// Polygons with duplicate points, holes, or self-intersections are not supported. (The result
    /// can be a partially or incorrectly triangulated polygon.)
    /// </para>
    /// <para>
    /// The resulting triangles have the same winding order as the polygon vertices.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="polygonVertices"/> or <paramref name="triangleIndices"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public int Triangulate(IList<Vector3F> polygonVertices, IList<int> triangleIndices)
    {
      return Triangulate(polygonVertices, 0, polygonVertices.Count, triangleIndices);
    }


    /// <summary>
    /// Triangulates a polygon specified by a list of vertices.
    /// </summary>
    /// <param name="polygonVertices">The vertices.</param>
    /// <param name="startIndex">
    /// The index of the start vertex in <paramref name="polygonVertices"/>.
    /// </param>
    /// <param name="vertexCount">The number of vertices in the polygon.</param>
    /// <param name="triangleIndices">
    /// The list that stores the resulting triangles: Each value in the list is an index into
    /// <paramref name="polygonVertices"/>; three indices define a triangle.
    /// </param>
    /// <returns>The number of triangles added to <paramref name="triangleIndices"/>.</returns>
    /// <remarks>
    /// <para>
    /// The method supports triangulation of convex and concave polygons. The polygon needs to be 
    /// planar. Either the x, y, or z component of the vertices should be constant.
    /// </para>
    /// <para>
    /// Polygons with holes or self-intersections are not supported. (The result can be a partially
    /// or incorrectly triangulated polygon.)
    /// </para>
    /// <para>
    /// The resulting triangles have the same winding order as the polygon vertices.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="polygonVertices"/> or <paramref name="triangleIndices"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public int Triangulate(IList<Vector3F> polygonVertices, int startIndex, int vertexCount, IList<int> triangleIndices)
    {
      if (polygonVertices == null)
        throw new ArgumentNullException("polygonVertices");
      if (triangleIndices == null)
        throw new ArgumentNullException("triangleIndices");

      if (vertexCount < 3)
        return 0;

      // Copy vertices to array for fast access.
      if (_vertices == null || _vertices.Length < vertexCount)
        _vertices = new Vector3F[vertexCount];

      for (int i = 0; i < vertexCount; i++)
        _vertices[i] = polygonVertices[startIndex + i];

      // Transform polygon into xy plane.
      Transform(_vertices, vertexCount);

      // Remember number of indices to determine number of newly added triangles.
      int originalIndexCount = triangleIndices.Count;

      // Get indices in counterclockwise order.
      if (_indices == null || _indices.Length < vertexCount)
        _indices = new int[vertexCount];

      bool flipped = GetCounterClockwiseIndices(_vertices, _indices, vertexCount);

      // Clip ears until polygon is triangulated.
      int count = 2 * vertexCount;   // Safeguard in case polygon is invalid.
      for (int v = vertexCount - 1; vertexCount > 2; )
      {
        // Terminate if no more ears were found.
        if (count-- <= 0)
          break;

        // Get next vertices in counterclockwise order.
        int u = v;
        if (u >= vertexCount)
          u = 0;

        v = u + 1;
        if (v >= vertexCount)
          v = 0;

        int w = v + 1;
        if (w >= vertexCount)
          w = 0;

        // Check if (vertices[u], vertices[v], vertices[w]) is an ear.
        if (IsEar(_vertices, _indices, u, v, w, vertexCount))
        {
          // Clip ear.
          int a = _indices[u] + startIndex;
          int b = _indices[v] + startIndex;
          int c = _indices[w] + startIndex;
          if (flipped)
            MathHelper.Swap(ref a, ref c);

          triangleIndices.Add(a);
          triangleIndices.Add(b);
          triangleIndices.Add(c);

          // Remove the vertex that has been clipped.
          for (int s = v, t = v + 1; t < vertexCount; s++, t++)
            _indices[s] = _indices[t];

          vertexCount--;
          count = 2 * vertexCount;   // Reset safeguard.
        }
      }

      return (triangleIndices.Count - originalIndexCount) / 3;
    }


    // Transforms the polygon into the xy plane.
    private static void Transform(Vector3F[] vertices, int numberOfVertices)
    {
      // We could calculate the polygon normal to find the exact polygon plane and
      // then transform the vertices into the xy plane. However, this is expensive
      // and introduces numerical errors.
      // Instead we assume that the polygon is close to one of the primary planes
      // xy, xz, or yz.

      // Compute AABB and determine dominant axes.
      int i0, i1, i2;
      Vector3F extent = GetAabb(vertices, numberOfVertices).Extent;
      if (extent.X >= extent.Y && extent.X >= extent.Z)
      {
        i0 = 0;
        if (extent.Y >= extent.Z)
        {
          i1 = 1;
          i2 = 2;
        }
        else
        {
          i1 = 2;
          i2 = 1;
        }
      }
      else if (extent.Y >= extent.X && extent.Y >= extent.Z)
      {
        i0 = 1;
        if (extent.X >= extent.Z)
        {
          i1 = 0;
          i2 = 2;
        }
        else
        {
          i1 = 2;
          i2 = 0;
        }
      }
      else
      {
        i0 = 2;
        if (extent.X >= extent.Y)
        {
          i1 = 0;
          i2 = 1;
        }
        else
        {
          i1 = 1;
          i2 = 0;
        }
      }

      if (i2 != 2)
      {
        // Reorder components.
        for (int i = 0; i < numberOfVertices; i++)
        {
          Vector3F v = vertices[i];
          vertices[i] = new Vector3F(v[i0], v[i1], v[i2]);
        }
      }
    }


    // Computes the AABB of the polygon.
    private static Aabb GetAabb(Vector3F[] vertices, int numberOfVertices)
    {
      Aabb aabb = new Aabb(vertices[0], vertices[0]);
      for (int i = 1; i < numberOfVertices; i++)
        aabb.Grow(vertices[i]);

      return aabb;
    }


    // Returns the indices of the polygon in counterclockwise order.
    private static bool GetCounterClockwiseIndices(Vector3F[] vertices, int[] indices, int numberOfVertices)
    {
      if (GetSignedArea(vertices, numberOfVertices) > 0.0f)
      {
        // Vertices are given in counterclockwise order.
        for (int i = 0; i < numberOfVertices; i++)
          indices[i] = i;

        return false;
      }

      // Vertices are given in clockwise order.
      for (int i = 0; i < numberOfVertices; i++)
        indices[i] = (numberOfVertices - 1) - i;

      return true;
    }


    // Gets the signed area of a planar non-self-intersecting polygon.
    // (Positive area = counterclockwise order, negative area = clockwise order)
    private static float GetSignedArea(Vector3F[] vertices, int numberOfVertices)
    {
      // The polygon needs to lie in the xy plane.
      // Reference: http://mathworld.wolfram.com/PolygonArea.html
      float area = 0.0f;
      for (int p = numberOfVertices - 1, q = 0; q < numberOfVertices; p = q, q++)
        area += vertices[p].X * vertices[q].Y - vertices[q].X * vertices[p].Y;

      return area / 2.0f;
    }


    // Determines whether the triangle with the indices (u, v, w) is an ear of the 
    // polygon. (An ear is a triangle formed by three consecutive vertices A, B, C
    // for which no other vertices of the polygon are inside the triangle.)
    private static bool IsEar(Vector3F[] vertices, int[] indices, int u, int v, int w, int numberOfVertices)
    {
      Vector2F A = new Vector2F(vertices[indices[u]].X, vertices[indices[u]].Y);
      Vector2F B = new Vector2F(vertices[indices[v]].X, vertices[indices[v]].Y);
      Vector2F C = new Vector2F(vertices[indices[w]].X, vertices[indices[w]].Y);

      // Check whether triangle (A, B, C) is clockwise or collinear.
      Vector2F a = B - A;
      Vector2F b = C - A;
      float aXb = a.X * b.Y - a.Y * b.X;  // Cross product = signed area of parallelogram
      if (aXb < Numeric.EpsilonFSquared)
        return false;

      // Check whether any of the remaining vertices is inside the triangle (A, B, C).
      for (int p = 0; p < numberOfVertices; p++)
      {
        if (p == u || p == v || p == w)
          continue;

        Vector2F P = new Vector2F(vertices[indices[p]].X, vertices[indices[p]].Y);
        if (IsInside(A, B, C, P))
          return false;
      }

      return true;
    }


    // Tests if a point P is inside the given counterclockwise triangle (A, B, C).
    private static bool IsInside(Vector2F A, Vector2F B, Vector2F C, Vector2F P)
    {
      Vector2F a = C - B;   // Triangle edge opposite A.
      Vector2F b = A - C;   // Triangle edge opposite B.
      Vector2F c = B - A;   // Triangle edge opposite C.
      Vector2F ap = P - A;
      Vector2F bp = P - B;
      Vector2F cp = P - C;

      // Build cross products:
      var aXbp = a.X * bp.Y - a.Y * bp.X;   // a x bp
      var cXap = c.X * ap.Y - c.Y * ap.X;   // c x ap
      var bXcp = b.X * cp.Y - b.Y * cp.X;   // b x cp

      // The sign of the cross product indicates whether P lies on the left or
      // right side of the triangle edge. If the point lies on the left of all 
      // triangle edges, then it is inside.
      return (aXbp >= 0.0f) && (bXcp >= 0.0f) && (cXap >= 0.0f);
    }
    #endregion
  }
}
