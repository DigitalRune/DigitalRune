// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// An iterative method for solving a linear system of equations <i>A * x = b</i>
  /// (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Iterative methods search for a solution <i>x</i> of the linear system of equations 
  /// <i>A * x = b</i>. <i>A</i> and <i>b</i> must be given. <i>A</i> is a matrix and <i>b</i> is a 
  /// vector. Internally the method produces a solution in several iterations. The method stops when
  /// the maximum number of iterations is reached or when the new solutions is very similar to the 
  /// solution of the last iteration (this case happens when the difference is less than a tolerance
  /// given by the user).
  /// </para>
  /// <para>
  /// The result of an iterative method is only an approximation. The accuracy of the result depends
  /// on the number of iterations and the initial guess for <i>x</i>. <i>Warm-starting</i> can 
  /// improve the result. This is done by providing a better initial guess for <i>x</i>: If several 
  /// similar linear systems are solved, then the solution of one linear system is possible near the
  /// solution of the other linear systems. For many applications, like computer games, the same
  /// linear system of equations is computed each frame with only minor variations in <i>A</i> and
  /// <i>b</i>.
  /// </para>
  /// <para>
  /// The advantage of an iterative solver is that is can provide an approximate solution very 
  /// quickly. In tasks like animation, a quick estimate is often better than an exact solution
  /// which takes longer to compute.
  /// </para>
  /// </remarks>
  public abstract class IterativeLinearSystemSolverD
  {
    private int _maxNumberOfIterations = 1000;
    private double _epsilon = Numeric.EpsilonD;


    /// <summary>
    /// Gets or sets the number of iterations of the last <see cref="Solve(MatrixD,VectorD)"/> 
    /// method call.
    /// </summary>
    /// <value>The number of iterations.</value>
    /// <remarks>
    /// This property is not thread-safe.
    /// </remarks>
    public int NumberOfIterations { get; protected set; }


    /// <summary>
    /// Gets or sets the maximum number number of iterations.
    /// </summary>
    /// <value>The maximum number number of iterations. The default value is 1000.</value>
    /// <remarks>
    /// In one call of <see cref="Solve(MatrixD,VectorD)"/> no more than 
    /// <see cref="MaxNumberOfIterations"/> are performed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public int MaxNumberOfIterations 
    {
      get { return _maxNumberOfIterations; } 
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The max number of iterations must be greater than 0.");
        _maxNumberOfIterations = value;
      }
    }


    /// <summary>
    /// Gets or sets the tolerance value. 
    /// </summary>
    /// <value>
    /// The tolerance value. The default is <see cref="Numeric"/>.<see cref="Numeric.EpsilonD"/>.
    /// </value>
    /// <remarks>
    /// If the absolute difference of x from the new iteration and the 
    /// x from the last iteration is less than this tolerance, the refinement of x is stopped.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public double Epsilon
    {
      get { return _epsilon; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The tolerance value must be greater than zero.");
        _epsilon = value;
      }
    }


    /// <overloads>
    /// <summary>
    /// Solves the specified linear system of equations <i>A * x = b</i>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Solves the specified linear system of equations <i>A * x = b</i>.
    /// </summary>
    /// <param name="matrixA">The matrix A.</param>
    /// <param name="vectorB">The vector b.</param>
    /// <returns>The solution vector x.</returns>
    /// <remarks>
    /// A zero vector is used as initial guess for x.
    /// </remarks>
    public VectorD Solve(MatrixD matrixA, VectorD vectorB)
    {
      return Solve(matrixA, null, vectorB);
    }


    /// <summary>
    /// Solves the specified linear system of equations <i>A * x = b</i> using an initial guess.
    /// </summary>
    /// <param name="matrixA">The matrix A.</param>
    /// <param name="initialX">
    /// The initial guess for x. If this value is <see langword="null"/>, a zero vector will be used
    /// as initial guess.
    /// </param>
    /// <param name="vectorB">The vector b.</param>
    /// <returns>The solution vector x.</returns>
    public abstract VectorD Solve(MatrixD matrixA, VectorD initialX, VectorD vectorB);
  }
}
