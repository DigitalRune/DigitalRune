// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Computes the Cholesky Decomposition of a matrix (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The Cholesky Decomposition can be used on a square matrix A that is symmetric and positive 
  /// definite (SPD).
  /// </para>
  /// <para>
  /// Positive definite means that: v<sup>T</sup> * A * v > 0 for all vectors v. (The equivalent 
  /// interpretation is that A has all positive eigenvalues.)
  /// </para>
  /// <para>
  /// The matrix is decomposed into a lower triangular matrix L so that A = L * L<sup>T</sup>
  /// </para>
  /// <para>
  /// If the matrix is not symmetric and positive definite, L will be a partial decomposition and
  /// the flag <see cref="IsSymmetricPositiveDefinite"/> is set to <see langword="false"/>.
  /// </para>
  /// <para>
  /// Applications:
  /// <list type="bullet">
  /// <item>
  /// Cholesky Decomposition can be used to solve linear equations for matrices that are SPD. This 
  /// method is about a factor of 2 faster than other methods.
  /// </item>
  /// <item>It can be used to determine efficiently if a matrix is SPD.</item>
  /// </list>
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cholesky")]
  public class CholeskyDecompositionD
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly MatrixD _l;
    private readonly bool _isSymmetricPositiveDefinite;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the original matrix is symmetric and positive definite.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the original matrix is symmetric and positive definite; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsSymmetricPositiveDefinite
    {
      get { return _isSymmetricPositiveDefinite; }
    }


    /// <summary>
    /// Gets the lower triangular matrix L. (This property returns the internal matrix, not a copy.)
    /// </summary>
    /// <value>The lower triangular matrix L.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public MatrixD L
    {
      get { return _l; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Creates the Cholesky decomposition of the given matrix.
    /// </summary>
    /// <param name="matrixA">The square matrix A.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="matrixA"/> is <see langword="null"/>.
    /// </exception>
    public CholeskyDecompositionD(MatrixD matrixA)
    {
      // Note: A different algorithm can be found in the Numerical Recipes book.

      if (matrixA == null)
        throw new ArgumentNullException("matrixA");

      int n = matrixA.NumberOfRows;
      _isSymmetricPositiveDefinite = true;

      // Is matrix square?
      if (matrixA.NumberOfColumns != n)
      {
        // Not square!
        n = Math.Min(n, matrixA.NumberOfColumns);
        _isSymmetricPositiveDefinite = false;
      }

      _l = new MatrixD(n, n);

      for (int j = 0; j < n; j++)
      {
        double d = 0;
        for (int k = 0; k < j; k++)
        {
          double s = 0;

          for (int i = 0; i < k; i++)
            s += _l[k, i] * _l[j, i];

          s = (matrixA[j, k] - s) / _l[k, k];
          L[j, k] = s;
          d = d + s * s;
          _isSymmetricPositiveDefinite = _isSymmetricPositiveDefinite && (matrixA[k, j] == matrixA[j, k]);
        }

        d = matrixA[j, j] - d;
        _isSymmetricPositiveDefinite = _isSymmetricPositiveDefinite && (d > 0);
        _l[j, j] = Math.Sqrt(Math.Max(d, 0));
        //for (int k = j + 1; k < n; k++)
        //  _l[j, k] = 0;      // Not needed. Already initialized with 0.
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Solves the equation <c>A * X = B</c>.
    /// </summary>
    /// <param name="matrixB">The matrix B with as many rows as A and any number of columns.</param>
    /// <returns>X, so that <c>A * X = B</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="matrixB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of rows does not match.
    /// </exception>
    /// <exception cref="MathematicsException">
    /// The matrix A is not symmetric and positive definite.
    /// </exception>
    public MatrixD SolveLinearEquations(MatrixD matrixB)
    {
      if (matrixB == null)
        throw new ArgumentNullException("matrixB");
      if (matrixB.NumberOfRows != L.NumberOfRows)
        throw new ArgumentException("The number of rows does not match.", "matrixB");
      if (IsSymmetricPositiveDefinite == false)
        throw new MathematicsException("The original matrix A is not symmetric and positive definite.");

      // Initialize x as a copy of B.
      MatrixD x = matrixB.Clone();

      // Solve L*Y = B.
      for (int k = 0; k < L.NumberOfRows; k++)
      {
        for (int j = 0; j < matrixB.NumberOfColumns; j++)
        {
          for (int i = 0; i < k; i++)
            x[k, j] -= x[i, j] * L[k, i];
          x[k, j] /= L[k, k];
        }
      }

      // Solve transpose(L) * X = Y.
      for (int k = L.NumberOfRows - 1; k >= 0; k--)
      {
        for (int j = 0; j < matrixB.NumberOfColumns; j++)
        {
          for (int i = k + 1; i < L.NumberOfRows; i++)
            x[k, j] -= x[i, j] * L[i, k];
          x[k, j] /= L[k, k];
        }
      }

      return x;
    }
    #endregion
  }
}
