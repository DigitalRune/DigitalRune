// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Provides helper methods for statistical tasks.
  /// </summary>
  public static class StatisticsHelper
  {
    /// <overloads>
    /// <summary>
    /// Computes the covariance matrix for a list of points.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the covariance matrix for a list of 3-dimensional points (single-precision).
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>The covariance matrix.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    public static Matrix33F ComputeCovarianceMatrix(IList<Vector3F> points)
    {
      // Notes: See "Real-Time Collision Detection" p. 93

      if (points == null)
        throw new ArgumentNullException("points");

      int numberOfPoints = points.Count;
      float oneOverNumberOfPoints = 1f / numberOfPoints;

      // Compute the center of mass.
      Vector3F centerOfMass = Vector3F.Zero;
      for (int i = 0; i < numberOfPoints; i++)
        centerOfMass += points[i];
      centerOfMass *= oneOverNumberOfPoints;

      // Compute covariance matrix.
      float c00 = 0;
      float c11 = 0;
      float c22 = 0;
      float c01 = 0;
      float c02 = 0;
      float c12 = 0;

      for (int i = 0; i < numberOfPoints; i++)
      {
        // Translate points so that center of mass is at origin.
        Vector3F p = points[i] - centerOfMass;

        // Compute covariance of translated point.
        c00 += p.X * p.X;
        c11 += p.Y * p.Y;
        c22 += p.Z * p.Z;
        c01 += p.X * p.Y;
        c02 += p.X * p.Z;
        c12 += p.Y * p.Z;
      }
      c00 *= oneOverNumberOfPoints;
      c11 *= oneOverNumberOfPoints;
      c22 *= oneOverNumberOfPoints;
      c01 *= oneOverNumberOfPoints;
      c02 *= oneOverNumberOfPoints;
      c12 *= oneOverNumberOfPoints;

      Matrix33F covarianceMatrix = new Matrix33F(c00, c01, c02,
                                                 c01, c11, c12,
                                                 c02, c12, c22);
      return covarianceMatrix;
    }


    /// <summary>
    /// Computes the covariance matrix for a list of 3-dimensional points (double-precision).
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>The covariance matrix.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    public static Matrix33D ComputeCovarianceMatrix(IList<Vector3D> points)
    {
      // Notes: See "Real-Time Collision Detection" p. 93

      if (points == null)
        throw new ArgumentNullException("points");

      int numberOfPoints = points.Count;
      float oneOverNumberOfPoints = 1f / numberOfPoints;

      // Compute the center of mass.
      Vector3D centerOfMass = Vector3D.Zero;
      for (int i = 0; i < numberOfPoints; i++)
        centerOfMass += points[i];
      centerOfMass *= oneOverNumberOfPoints;

      // Compute covariance matrix.
      double c00 = 0;
      double c11 = 0;
      double c22 = 0;
      double c01 = 0;
      double c02 = 0;
      double c12 = 0;

      for (int i = 0; i < numberOfPoints; i++)
      {
        // Translate points so that center of mass is at origin.
        Vector3D p = points[i] - centerOfMass;

        // Compute covariance of translated point.
        c00 += p.X * p.X;
        c11 += p.Y * p.Y;
        c22 += p.Z * p.Z;
        c01 += p.X * p.Y;
        c02 += p.X * p.Z;
        c12 += p.Y * p.Z;
      }
      c00 *= oneOverNumberOfPoints;
      c11 *= oneOverNumberOfPoints;
      c22 *= oneOverNumberOfPoints;
      c01 *= oneOverNumberOfPoints;
      c02 *= oneOverNumberOfPoints;
      c12 *= oneOverNumberOfPoints;

      Matrix33D covarianceMatrix = new Matrix33D(c00, c01, c02,
                                                 c01, c11, c12,
                                                 c02, c12, c22);
      return covarianceMatrix;
    }


    /// <summary>
    /// Computes the covariance matrix for a list of n-dimensional points (single-precision).
    /// </summary>
    /// <param name="points">
    /// The points. All points must have the same <see cref="VectorF.NumberOfElements"/>.
    /// </param>
    /// <returns>The covariance matrix.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    public static MatrixF ComputeCovarianceMatrix(IList<VectorF> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The list of points is empty.");

      int numberOfElements = points[0].NumberOfElements;
      int numberOfPoints = points.Count;
      float oneOverNumberOfPoints = 1f / numberOfPoints;

      // Compute the center of mass.
      VectorF centerOfMass = new VectorF(numberOfElements);
      for (int i = 0; i < numberOfPoints; i++)
      {
        // Check dimension.
        if (points[i].NumberOfElements != numberOfElements)
          throw new ArgumentException("All vectors in 'points' must have the same number of elements.");

        centerOfMass += points[i];
      }
      centerOfMass *= oneOverNumberOfPoints;

      // Compute covariance matrix.
      MatrixF c = new MatrixF(numberOfElements, numberOfElements);

      for (int i = 0; i < numberOfPoints; i++)
      {
        // Translate points so that center of mass is at origin.
        VectorF p = points[i] - centerOfMass;

        // Compute covariance of translated point. 
        // (Only one half of the matrix is computed because C is symmetric.)
        for (int row = 0; row < numberOfElements; row++)
          for (int column = row; column < numberOfElements; column++)
            c[row, column] += p[row] * p[column];
      }

      // Divide by numberOfPoints
      for (int row = 0; row < numberOfElements; row++)
        for (int column = row; column < numberOfElements; column++)
          c[row, column] *= oneOverNumberOfPoints;

      // Set the other half of the symmetric matrix. 
      for (int row = 0; row < numberOfElements; row++)
        for (int column = row + 1; column < numberOfElements; column++)
          c[column, row] = c[row, column];

      return c;
    }


    /// <summary>
    /// Computes the covariance matrix for a list of n-dimensional points (double-precision).
    /// </summary>
    /// <param name="points">
    /// The points. All points must have the same <see cref="VectorF.NumberOfElements"/>.
    /// </param>
    /// <returns>The covariance matrix.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    public static MatrixD ComputeCovarianceMatrix(IList<VectorD> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The list of points is empty.");

      int numberOfElements = points[0].NumberOfElements;
      int numberOfPoints = points.Count;
      float oneOverNumberOfPoints = 1f / numberOfPoints;

      // Compute the center of mass.
      VectorD centerOfMass = new VectorD(numberOfElements);
      for (int i = 0; i < numberOfPoints; i++)
      {
        // Check dimension.
        if (points[i].NumberOfElements != numberOfElements)
          throw new ArgumentException("All vectors in 'points' must have the same number of elements.");

        centerOfMass += points[i];
      }
      centerOfMass *= oneOverNumberOfPoints;

      // Compute covariance matrix.
      MatrixD c = new MatrixD(numberOfElements, numberOfElements);

      for (int i = 0; i < numberOfPoints; i++)
      {
        // Check dimension.
        if (points[i].NumberOfElements != numberOfElements)
          throw new ArgumentException("All vectors in 'points' must have the same number of elements.");
        
        // Translate points so that center of mass is at origin.
        VectorD p = points[i] - centerOfMass;

        // Compute covariance of translated point. 
        // (Only one half of the matrix is computed because C is symmetric.)
        for (int row = 0; row < numberOfElements; row++)
          for (int column = row; column < numberOfElements; column++)
            c[row, column] += p[row] * p[column];
      }

      // Divide by numberOfPoints
      for (int row = 0; row < numberOfElements; row++)
        for (int column = row; column < numberOfElements; column++)
          c[row, column] *= oneOverNumberOfPoints;

      // Set the other half of the symmetric matrix. 
      for (int row = 0; row < numberOfElements; row++)
        for (int column = row + 1; column < numberOfElements; column++)
          c[column, row] = c[row, column];

      return c;
    }
  }
}
