// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// An iterative solver using the Successive Over Relaxation (SOR) method (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// See <see href="http://en.wikipedia.org/wiki/Successive_over-relaxation"/> for an introduction 
  /// to this method and for an explanation of the convergence criterion.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SorMethodF : IterativeLinearSystemSolverF
  {
    /// <summary>
    /// Gets or sets the relaxation factor.
    /// </summary>
    /// <value>The relaxation factor. The default value is <c>1</c>.</value>
    /// <remarks>
    /// When this value is between 0 and 1 the method is termed under-relaxation, and when this
    /// value is greater than 1 the method is termed over relaxation. If this value is 1, the SOR
    /// method is simply the Gauss-Seidel method.
    /// </remarks>
    public float RelaxationFactor { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SorMethodF"/> class.
    /// </summary>
    public SorMethodF()
    {
      RelaxationFactor = 1;
    }


    /// <summary>
    /// Solves the specified linear system of equations <i>Ax=b</i>.
    /// </summary>
    /// <param name="matrixA">The matrix A.</param>
    /// <param name="initialX">
    /// The initial guess for x. If this value is <see langword="null"/>, a zero vector will be used
    /// as initial guess.
    /// </param>
    /// <param name="vectorB">The vector b.</param>
    /// <returns>The solution vector x.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="matrixA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vectorB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="matrixA"/> is not a square matrix.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of elements of <paramref name="initialX"/> does not match.
    /// </exception>
    public override VectorF Solve(MatrixF matrixA, VectorF initialX, VectorF vectorB)
    {
      NumberOfIterations = 0;

      if (matrixA == null)
        throw new ArgumentNullException("matrixA");
      if (vectorB == null)
        throw new ArgumentNullException("vectorB");
      if (matrixA.IsSquare == false)
        throw new ArgumentException("Matrix A must be a square matrix.", "matrixA");
      if (matrixA.NumberOfRows != vectorB.NumberOfElements)
        throw new ArgumentException("The number of rows of A and b do not match.");
      if (initialX != null && initialX.NumberOfElements != vectorB.NumberOfElements)
        throw new ArgumentException("The number of elements of the initial guess for x and b do not match.");

      VectorF xOld = initialX ?? new VectorF(vectorB.NumberOfElements);
      VectorF xNew = new VectorF(vectorB.NumberOfElements);
      bool isConverged = false;
      // Make iterations until max iteration count or the result has converged.
      for (int i = 0; i < MaxNumberOfIterations && !isConverged; i++)
      {
        for (int j=0; j<vectorB.NumberOfElements; j++)
        {
          float delta = 0;
          for (int k=0; k < j; k++)
            delta += matrixA[j, k] * xNew[k];

          for (int k=j+1; k < vectorB.NumberOfElements; k++)
            delta += matrixA[j, k] * xOld[k];

          delta = (vectorB[j] - delta) / matrixA[j, j];
          xNew[j] = xOld[j] + RelaxationFactor * (delta - xOld[j]);
        }

        // Test convergence
        isConverged = VectorF.AreNumericallyEqual(xOld, xNew, Epsilon);

        xOld = xNew.Clone();
        NumberOfIterations = i + 1;
      }
   
      return xNew;
    }
  }
}
