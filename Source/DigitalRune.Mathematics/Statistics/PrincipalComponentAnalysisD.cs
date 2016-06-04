// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Performs a Principal Component Analysis (PCA) using the covariance method (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class takes a list of data points, computes the covariance matrix C and performs
  /// <see cref="EigenvalueDecompositionD"/> on the covariance matrix. The resulting eigenvectors
  /// represent the uncorrelated principal components of the data. The principal components
  /// ("natural axes") are the basis of a new coordinate system where the covariance matrix is a
  /// diagonal matrix. The first principal component is the direction where the variance of the
  /// data projected onto the principal component is greatest. The second greatest variance is on
  /// the second principal component, and so forth.
  /// </para>
  /// <para>
  /// The matrix of the principal components (<see cref="V"/>) is an orthogonal matrix, with
  /// <c>C = V * D * V<sup>T</sup></c>, where C is the covariance matrix and D is the diagonal 
  /// covariance matrix in the space formed by the principal components.
  /// </para>
  /// </remarks>
  public class PrincipalComponentAnalysisD
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the matrix of the principal components.
    /// </summary>
    /// <value>The matrix of the principal components.</value>
    /// <remarks>
    /// Each column in this matrix represents a principal component. Columns are ordered by
    /// decreasing variance. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public MatrixD V { get; private set; }


    /// <summary>
    /// Gets the variances.
    /// </summary>
    /// <value>The variances.</value>
    /// <remarks>
    /// Each element in the vector represents the variance of the data points along a principle
    /// component. The variances are sorted by decreasing value, so that the largest variance is the
    /// first element.
    /// </remarks>
    public VectorD Variances { get; private set; }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Creates the principal component analysis for the given list of points.
    /// </summary>
    /// <param name="points">
    /// The list of data points. All points must have the same 
    /// <see cref="VectorD.NumberOfElements"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    public PrincipalComponentAnalysisD(IList<VectorD> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The list of points is empty.");

      // Compute covariance matrix.
      MatrixD covarianceMatrix = StatisticsHelper.ComputeCovarianceMatrix(points);

      // Perform Eigenvalue decomposition.
      EigenvalueDecompositionD evd = new EigenvalueDecompositionD(covarianceMatrix);

      int numberOfElements = evd.RealEigenvalues.NumberOfElements;
      Variances = new VectorD(numberOfElements);
      V = new MatrixD(numberOfElements, numberOfElements);

      // Sort eigenvalues by decreasing value.
      // Since covarianceMatrix is symmetric, we have no imaginary eigenvalues.
      for (int i = 0; i < Variances.NumberOfElements; i++)
      {
        int index = evd.RealEigenvalues.IndexOfLargestElement;
        
        Variances[i] = evd.RealEigenvalues[index];
        V.SetColumn(i, evd.V.GetColumn(index));

        evd.RealEigenvalues[index] = double.NegativeInfinity;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
