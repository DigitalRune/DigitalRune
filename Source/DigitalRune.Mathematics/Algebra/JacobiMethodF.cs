// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// An iterative solver using the Jacobi method (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The method will always converge if the matrix A is strictly or irreducibly diagonally 
  /// dominant. Strict row diagonal dominance means that for each row, the absolute value of the
  /// diagonal term is greater than the sum of absolute values of other terms.
  /// </para>
  /// <para>
  /// See <see href="http://en.wikipedia.org/wiki/Jacobi_method"/> for an introduction to this 
  /// method and for an explanation of the convergence criterion.
  /// </para>
  /// </remarks>
  public class JacobiMethodF : IterativeLinearSystemSolverF
  {
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
      // TODO: We can possible improve the method by reordering after each step. 
      // This can be done randomly or we sort by the "convergence" of the elements. 
      // See book Physics-Based Animation.

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
        for (int j = 0; j < vectorB.NumberOfElements; j++)
        {
          float delta = 0;
          for (int k = 0; k < j; k++)
            delta += matrixA[j, k] * xOld[k];

          for (int k = j + 1; k < vectorB.NumberOfElements; k++)
            delta += matrixA[j, k] * xOld[k];

          xNew[j] = (vectorB[j] - delta) / matrixA[j, j];
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
