// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Finds roots using an improved Newton-Raphson method (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This root finding algorithm uses a combination of the <i>bisection method</i> and the
  /// <i>Newton-Raphson</i> to provide global convergence. 
  /// </para>
  /// <para>
  /// This algorithm needs a function <i>f'(x)</i> which can compute the derivative of the function
  /// <i>f(x)</i> as additional inputs.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class ImprovedNewtonRaphsonMethodD : NewtonRaphsonMethodD
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ImprovedNewtonRaphsonMethodD"/> class.
    /// </summary>
    /// <param name="function">The function f(x), which root we want to find.</param>
    /// <param name="derivative">The function f'(x), which computes the derivative.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="derivative"/> is <see langword="null"/>.
    /// </exception>
    public ImprovedNewtonRaphsonMethodD(Func<double, double> function, Func<double, double> derivative)
      : base(function, derivative)
    {
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
    /// <returns>
    /// The x value such that <i>f(x) = 0</i>; or <i>NaN</i> if no root is found.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override double FindRoot(Func<double, double> function, double x0, double x1)
    {
      Debug.Assert(function != null);
      Debug.Assert(Derivative != null);

      NumberOfIterations = 0;


      double yLow = function(x0);
      double yHigh = function(x1);

      // Is one of the bounds the solution?
      if (Numeric.IsZero(yLow, EpsilonY))
        return x0;

      if (Numeric.IsZero(yHigh, EpsilonY))
        return x1;

      // Is the bracket valid?
      if (Numeric.AreEqual(x0, x1, EpsilonX) || yLow * yHigh >= 0)
        return double.NaN;

      double xLow, xHigh;
      if (yLow < 0)
      {
        xLow = x0;
        xHigh = x1;
      }
      else
      {
        xLow = x1;
        xHigh = x0;
      }

      // Initial guess:
      double x = (x0 + x1) / 2;

      // The step size before the last.
      double dxOld = Math.Abs(x1 - x0);
      // The step size.
      double dx = dxOld;

      double y = function(x);

      // Stop if x is the result or if the step size dx is less than Epsilon.
      if (Numeric.IsZero(y, EpsilonY) || Numeric.IsZero(dx, EpsilonX))
        return x;

      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;

        double dyOverDt = Derivative(x);

        // Choose new x. 
        // Bisect if Newton would jump out of the brackets or step size would be too small.
        if ((((x - xHigh) * dyOverDt - y) * ((x - xLow) * dyOverDt - y) > 0)
            || (Math.Abs(2 * y) > Math.Abs(dxOld * dyOverDt)))
        {
          // Bisect.
          dxOld = dx;
          dx = (xHigh - xLow) / 2;
          x = xLow + dx;
        }
        else
        {
          // Newton step.
          dxOld = dx;
          dx = y / dyOverDt;
          x -= dx;
        }

        y = function(x);

        // Stop if x is the result or if the step size dx is less than Epsilon.
        if (Numeric.IsZero(y, EpsilonY) || Numeric.IsZero(dx, EpsilonX))
          return x;

        // Choose new bracket.
        if (y < 0)
          xLow = x;
        else
          xHigh = x;
      }
      return double.NaN;
    }
  }
}
