// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Provides helper methods for computing and manipulating mass properties.
  /// </summary>
  public static partial class MassHelper
  {
    /// <summary>
    /// Changes the mass to the given target mass.
    /// </summary>
    /// <param name="targetMass">The target mass.</param>
    /// <param name="mass">The mass.</param>
    /// <param name="inertia">The inertia.</param>
    private static void AdjustMass(float targetMass, ref float mass, ref Matrix33F inertia)
    {
      float scale = targetMass / mass;
      mass = targetMass;
      inertia = inertia * scale;
    }


    /// <summary>
    /// Diagonalizes the inertia matrix.
    /// </summary>
    /// <param name="inertia">The inertia matrix.</param>
    /// <param name="inertiaDiagonal">The inertia of the principal axes.</param>
    /// <param name="rotation">
    /// The rotation that rotates from principal axis space to parent/world space.
    /// </param>
    /// <remarks>
    /// All valid inertia matrices can be transformed into a coordinate space where all elements
    /// non-diagonal matrix elements are 0. The axis of this special space are the principal axes.
    /// </remarks>
    internal static void DiagonalizeInertia(Matrix33F inertia, out Vector3F inertiaDiagonal, out Matrix33F rotation)
    {
      // Alternatively we could use Jacobi transformation (iterative method, see Bullet/btMatrix3x3.diagonalize() 
      // and Numerical Recipes book) or we could find the eigenvalues using the characteristic 
      // polynomial which is a cubic polynomial and then solve for the eigenvectors (see Numeric
      // Recipes and "Mathematics for 3D Game Programming and Computer Graphics" chapter ray-tracing
      // for cubic equations and computation of bounding boxes.

      // Perform eigenvalue decomposition.
      var eigenValueDecomposition = new EigenvalueDecompositionF(inertia.ToMatrixF());
      inertiaDiagonal = eigenValueDecomposition.RealEigenvalues.ToVector3F();
      rotation = eigenValueDecomposition.V.ToMatrix33F();

      if (!rotation.IsRotation)
      {
        // V is orthogonal but not necessarily a rotation. If it is no rotation
        // we have to swap two columns.
        MathHelper.Swap(ref inertiaDiagonal.Y, ref inertiaDiagonal.Z);

        Vector3F dummy = rotation.GetColumn(1);
        rotation.SetColumn(1, rotation.GetColumn(2));
        rotation.SetColumn(2, dummy);

        Debug.Assert(rotation.IsRotation);
      }
    }


    /// <summary>
    /// Gets the inertia matrix of mass where the center of mass is moved away from the origin.
    /// </summary>
    /// <param name="mass">The mass.</param>
    /// <param name="inertia">
    /// The inertia matrix that is valid when the center of mass is in the origin.
    /// </param>
    /// <param name="translation">The translation of the center of mass.</param>
    /// <returns>
    /// The inertia matrix of the translated mass for rotations around the origin.
    /// </returns>
    /// <remarks>
    /// This method can be used if mass and inertia are given for an object where the center of
    /// mass is in the origin. The mass is shifted from the origin to the given new position 
    /// (<paramref name="translation"/>). The new inertia for rotations around the origin is 
    /// returned.
    /// </remarks>
    /// <seealso cref="GetUntranslatedMassInertia"/>
    private static Matrix33F GetTranslatedMassInertia(float mass, Matrix33F inertia, Vector3F translation)
    {
      // The current center of mass is at the origin. 
      // Using the "transfer of axes" or "parallel axes" theorem:
      Vector3F translation2 = translation * translation;
      inertia.M00 += mass * (translation2.Y + translation2.Z);
      inertia.M11 += mass * (translation2.X + translation2.Z);
      inertia.M22 += mass * (translation2.X + translation2.Y);
      inertia.M01 -= mass * translation.X * translation.Y;
      inertia.M02 -= mass * translation.X * translation.Z;
      inertia.M12 -= mass * translation.Y * translation.Z;

      inertia.M10 = inertia.M01;
      inertia.M20 = inertia.M02;
      inertia.M21 = inertia.M12;

      return inertia;
    }


    /// <summary>
    /// Gets the inertia matrix of mass where the center of mass is moved back to the origin.
    /// </summary>
    /// <param name="mass">The mass.</param>
    /// <param name="inertia">
    /// The inertia matrix for rotations around the origin (not the center of mass) of the 
    /// translated mass.
    /// </param>
    /// <param name="translation">
    /// The translation of the center of mass. This translation will be undone.
    /// </param>
    /// <returns>
    /// The inertia matrix for rotations around the origin when the center of mass is in the origin.
    /// </returns>
    /// <remarks>
    /// This method does the inverse of <see cref="GetTranslatedMassInertia"/>. This method can be 
    /// used if mass and inertia are given for an object where the center of mass is not in the
    /// origin and the inertia is given for rotation around the origin. The mass is shifted from
    /// the translated position (<paramref name="translation"/>) back to the origin and the new
    /// inertia is returned.
    /// </remarks>
    /// <seealso cref="GetTranslatedMassInertia"/>
    private static Matrix33F GetUntranslatedMassInertia(float mass, Matrix33F inertia, Vector3F translation)
    {
      if (translation.IsNumericallyZero)
        return inertia;

      // Do the inverse of GetTranslatedMassInertia.
      Vector3F translation2 = translation * translation;
      inertia.M00 -= mass * (translation2.Y + translation2.Z);
      inertia.M11 -= mass * (translation2.X + translation2.Z);
      inertia.M22 -= mass * (translation2.X + translation2.Y);
      inertia.M01 += mass * translation.X * translation.Y;
      inertia.M02 += mass * translation.X * translation.Z;
      inertia.M12 += mass * translation.Y * translation.Z;

      inertia.M10 = inertia.M01;
      inertia.M20 = inertia.M02;
      inertia.M21 = inertia.M12;

      return inertia;
    }
  }
}
