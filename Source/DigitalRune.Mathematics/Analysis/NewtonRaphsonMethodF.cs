// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Mathematics.Analysis
{

  /// <summary>
  /// Finds roots using the Newton-Raphson method (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This algorithm needs a function <i>f'(x)</i> which can compute the derivative of the function 
  /// <i>f(x)</i> as additional inputs.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class NewtonRaphsonMethodF : RootFinderF
  {
    /// <summary>
    /// Gets a function that computes the derivative <i>f'(x)</i>.
    /// </summary>
    public Func<float, float> Derivative { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="NewtonRaphsonMethodF"/> class.
    /// </summary>
    /// <param name="function">The function f(x), which root we want to find.</param>
    /// <param name="derivative">The function f'(x), which computes the derivative.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="derivative"/> is <see langword="null"/>.
    /// </exception>
    public NewtonRaphsonMethodF(Func<float, float> function, Func<float, float> derivative)
      : base(function)
    {
      if (derivative == null)
        throw new ArgumentNullException("derivative");

      Derivative = derivative;
    }


    /// <summary>
    /// Finds the root of the given function.
    /// </summary>
    /// <param name="function">The function f.</param>
    /// <param name="x0">
    /// An x value such that the root lies between <paramref name="x0"/> and <paramref name="x1"/>.
    /// </param>
    /// <param name="x1">
    /// An x value such that the root lies between <paramref name="x0"/> and <paramref name="x1"/>.
    /// </param>
    /// <returns>The x value such that <i>f(x) = 0</i>; or <i>NaN</i> if no root is found.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override float FindRoot(Func<float, float> function, float x0, float x1)
    {
      Debug.Assert(function != null);
      Debug.Assert(Derivative != null);

      NumberOfIterations = 0;

      float yLow = function(x0);
      float yHigh = function(x1);

      // Is one of the bounds the solution?
      if (Numeric.IsZero(yLow, EpsilonY))
        return x0;
      if (Numeric.IsZero(yHigh, EpsilonY))
        return x1;

      // Initial guess:
      float x = (x0 + x1) / 2;

      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;

        float y = function(x);
        float dyOverDt = Derivative(x);
        float dx = y / dyOverDt;

        // Stop if x is the result or if the step size dx is less than Epsilon.
        if (Numeric.IsZero(y, EpsilonY) || Numeric.IsZero(dx, EpsilonX))
          return x;

        x -= dx;
        if ((x0 - x) * (x - x1) < 0)
          return float.NaN; // We have left the original bracket.
      }
      return float.NaN;
    }
  }
}
