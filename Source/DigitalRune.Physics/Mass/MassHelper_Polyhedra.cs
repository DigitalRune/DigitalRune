// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics
{
  public static partial class MassHelper
  {
    // See book Game Physics pp. 76.

    /// <summary>
    /// Computes the polyhedron mass sub expressions as described in the book "Game Physics".
    /// </summary>
    private static void ComputePolyhedronMassSubExpressions(
      float w0, float w1, float w2,
      out float f1, out float f2, out float f3,
      out float g0, out float g1, out float g2)
    {
      float temp0 = w0 + w1;
      f1 = temp0 + w2;
      float temp1 = w0 * w0;
      float temp2 = temp1 + w1 * temp0;
      f2 = temp2 + w2 * f1;
      f3 = w0 * temp1 + w1 * temp2 + w2 * f2;
      g0 = f2 + w0 * (f1 + w0);
      g1 = f2 + w1 * (f1 + w1);
      g2 = f2 + w2 * (f1 + w2);
    }


    /// <summary>
    /// Gets the mass properties of the given triangle mesh for a density of 1.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="mass">The mass.</param>
    /// <param name="centerOfMass">The center of mass.</param>
    /// <param name="inertia">The inertia matrix.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void GetMass(ITriangleMesh mesh, out float mass, out Vector3F centerOfMass, out Matrix33F inertia)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      // Integral variables.
      float i0 = 0, i1 = 0, i2 = 0, i3 = 0, i4 = 0, i5 = 0, i6 = 0, i7 = 0, i8 = 0, i9 = 0;

      int numberOfTriangles = mesh.NumberOfTriangles;
      for (int triangleIndex = 0; triangleIndex < numberOfTriangles; triangleIndex++)
      {
        var triangle = mesh.GetTriangle(triangleIndex);

        // Vertex coordinates.
        Vector3F v0 = triangle.Vertex0;
        Vector3F v1 = triangle.Vertex1;
        Vector3F v2 = triangle.Vertex2;

        // Edges and cross products of edges
        Vector3F a = v1 - v0;
        Vector3F b = v2 - v0;
        Vector3F d = Vector3F.Cross(a, b);

        // Compute integral terms.
        float f1x, f2x, f3x, g0x, g1x, g2x;
        ComputePolyhedronMassSubExpressions(v0.X, v1.X, v2.X, out f1x, out f2x, out f3x, out g0x, out g1x, out g2x);
        float f1y, f2y, f3y, g0y, g1y, g2y;
        ComputePolyhedronMassSubExpressions(v0.Y, v1.Y, v2.Y, out f1y, out f2y, out f3y, out g0y, out g1y, out g2y);
        float f1z, f2z, f3z, g0z, g1z, g2z;
        ComputePolyhedronMassSubExpressions(v0.Z, v1.Z, v2.Z, out f1z, out f2z, out f3z, out g0z, out g1z, out g2z);

        // Update integrals.
        i0 += d.X * f1x;
        i1 += d.X * f2x;
        i2 += d.Y * f2y;
        i3 += d.Z * f2z;
        i4 += d.X * f3x;
        i5 += d.Y * f3y;
        i6 += d.Z * f3z;
        i7 += d.X * (v0.Y * g0x + v1.Y * g1x + v2.Y * g2x);
        i8 += d.Y * (v0.Z * g0y + v1.Z * g1y + v2.Z * g2y);
        i9 += d.Z * (v0.X * g0z + v1.X * g1z + v2.X * g2z);
      }

      i0 /= 6.0f;
      i1 /= 24.0f;
      i2 /= 24.0f;
      i3 /= 24.0f;
      i4 /= 60.0f;
      i5 /= 60.0f;
      i6 /= 60.0f;
      i7 /= 120.0f;
      i8 /= 120.0f;
      i9 /= 120.0f;

      mass = i0;

      centerOfMass = 1.0f / mass * new Vector3F(i1, i2, i3);
      // Clamp to zero.
      if (Numeric.IsZero(centerOfMass.X))
        centerOfMass.X = 0;
      if (Numeric.IsZero(centerOfMass.Y))
        centerOfMass.Y = 0;
      if (Numeric.IsZero(centerOfMass.Z))
        centerOfMass.Z = 0;

      // Inertia around the world origin.
      inertia.M00 = i5 + i6;
      inertia.M11 = i4 + i6;
      inertia.M22 = i4 + i5;
      inertia.M01 = inertia.M10 = Numeric.IsZero(i7) ? 0 : -i7;
      inertia.M12 = inertia.M21 = Numeric.IsZero(i8) ? 0 : -i8;
      inertia.M02 = inertia.M20 = Numeric.IsZero(i9) ? 0 : -i9;

      // Inertia around center of mass.
      inertia = GetUntranslatedMassInertia(mass, inertia, centerOfMass);
    }
  }
}
