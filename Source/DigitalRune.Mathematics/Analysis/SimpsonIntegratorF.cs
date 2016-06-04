// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs numerical integration using the <i>Simpson's rule</i> (single-precision).
  /// </summary>
  public class SimpsonIntegratorF : IntegratorF
  {
    /// <summary>
    /// Integrates the specified function within the given interval.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="lowerBound">The lower bound.</param>
    /// <param name="upperBound">The upper bound.</param>
    /// <returns>
    /// The integral of the given function over the interval 
    /// [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="function"/> is <see langword="null"/>.
    /// </exception>
    public override float Integrate(Func<float, float> function, float lowerBound, float upperBound)
    {
      NumberOfIterations = 0;

      if (function == null)
        throw new ArgumentNullException("function");

      // see Numerical Recipes p. 144
      float integral = 0;
      float oldTrapezoidalIntegral = 0;
      float oldIntegral = 0;
      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;
        float trapezoidalIntegral = TrapezoidalIntegratorF.Integrate(function, lowerBound, upperBound, oldTrapezoidalIntegral, i + 1);
        integral = (4.0f * trapezoidalIntegral - oldTrapezoidalIntegral) / 3.0f;

        if (NumberOfIterations >= MinNumberOfIterations)        // Avoid spurious early convergence
          if (Numeric.AreEqual(integral, oldIntegral, Epsilon))
            return integral;

        oldIntegral = integral;
        oldTrapezoidalIntegral = trapezoidalIntegral;
      }
      return integral;
    }
  }
}
