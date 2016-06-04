// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{

  /// <summary>
  /// Finds roots using the regula falsi (false position) method (single-precision).
  /// </summary>
  /// <remarks>
  /// The false position method is a standard textbook method. If a faster method is required take a
  /// look at <i>Ridder's method</i> or <i>Brent's method</i>.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class RegulaFalsiMethodF : RootFinderF
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RegulaFalsiMethodF"/> class.
    /// </summary>
    /// <param name="function">The function f(x), which root we want to find.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public RegulaFalsiMethodF(Func<float, float> function)
      : base(function)
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
    /// <returns>The x value such that <i>f(x) = 0</i>; or <i>NaN</i> if no root is found.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override float FindRoot(Func<float, float> function, float x0, float x1)
    {
      NumberOfIterations = 0;

      float yLow = function(x0);
      float yHigh = function(x1);

      // Is one of the bounds the solution?
      if (Numeric.IsZero(yLow, EpsilonY))
        return x0;
      if (Numeric.IsZero(yHigh, EpsilonY))
        return x1;

      // Is the bracket valid?
      if (Numeric.AreEqual(x0, x1, EpsilonX) || yLow * yHigh >= 0)
        return float.NaN;

      // Setup xLow and xHigh.
      float xLow, xHigh;
      if (yLow < 0)
      {
        xLow = x0;
        xHigh = x1;
      }
      else
      {
        xLow = x1;
        xHigh = x0;
        MathHelper.Swap(ref yLow, ref yHigh);
      }

      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;

        // The step size. dx steps from xLow to xHigh.
        float dx = xHigh - xLow;

        // Assume that the function is linear between xLow and xHigh.
        // And choose x for f(x) = 0 under this assumption
        float x = xLow + dx * yLow / (yLow - yHigh);
        float y = function(x);

        // Stop if x is the result or if the stepsize dx is less than Epsilon.
        if (Numeric.IsZero(y, EpsilonY) || Numeric.IsZero(dx, EpsilonX))
          return x;

        // Choose new bracket.
        if (y < 0)
        {
          xLow = x;
          yLow = y;
        }
        else
        {
          xHigh = x;
          yHigh = y;
        }
      }
      return float.NaN;
    }
  }
}
