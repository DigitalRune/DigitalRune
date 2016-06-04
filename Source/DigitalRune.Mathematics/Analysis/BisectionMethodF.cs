// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Finds roots using the bisection method (single-precision).
  /// </summary>
  public class BisectionMethodF : RootFinderF
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BisectionMethodF"/> class.
    /// </summary>
    /// <param name="function">The function f(x), which root we want to find.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public BisectionMethodF(Func<float, float> function)
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
    /// <returns>
    /// The x value such that <i>f(x) = 0</i>; or <i>NaN</i> if no root is found.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override float FindRoot(Func<float, float> function, float x0, float x1)
    {
      NumberOfIterations = 0;

      float y0 = function(x0);
      float yMid = function(x1);

      // Is one of the bounds the solution?
      if (Numeric.IsZero(y0, EpsilonY))
        return x0;
      if (Numeric.IsZero(yMid, EpsilonY))
        return x1;

      // Is the bracket valid?
      if (Numeric.AreEqual(x0, x1, EpsilonX) || y0 * yMid >= 0)
        return float.NaN;

      // Setup the step size dx and x0, such that f(x0) < 0.
      float dx;
      if (y0 < 0)
      {
        dx = x1 - x0;
      }
      else
      {
        dx = x0 - x1;
        x0 = x1;
      }

      // Assert: The root is within x0 and x0 + dx.
      // Assert: f(x0) < 0.

      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;
        dx *= 0.5f;
        float xMid = x0 + dx;
        yMid = function(xMid);

        // Stop if xMid is the result or if the stepsize dx is less than Epsilon.
        if (Numeric.IsZero(yMid, EpsilonY) || Numeric.IsZero(dx, EpsilonX))
          return xMid;

        if (yMid < 0)
          x0 = xMid;
      }

      return float.NaN;
    }
  }
}
