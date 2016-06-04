// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs numerical integration using the <i>extended trapezoidal rule</i> (single-precision).
  /// </summary>
  public class TrapezoidalIntegratorF : IntegratorF
  {
    /// <summary>
    /// Refines the given integral by adding 2<sup>n-2</sup> additional interior points.
    /// </summary>
    internal static float Integrate(Func<float, float> function, float lowerBound, float upperBound, float integral, int iteration)
    {
      // see Numerical Recipes p. 141.

      if (iteration == 1)
      {
        return 0.5f * (upperBound - lowerBound) * (function(lowerBound) + function(upperBound));
      }
      else
      {
        int numberOfPoints = 1;
        for (int i = 1; i < iteration - 1; i++)
          numberOfPoints <<= 1;

        float spacing = (upperBound - lowerBound) / numberOfPoints;   // The spacing of the points to be added.
        float x = lowerBound + 0.5f * spacing;
        float sum = 0;
        for (int i = 0; i < numberOfPoints; i++, x += spacing)
          sum += function(x);

        return 0.5f * (integral + (upperBound - lowerBound) * sum / numberOfPoints);
      }
    }


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

      float integral = 0;
      float oldIntegral = 0;
      for (int i = 0; i < MaxNumberOfIterations; i++)
      {
        NumberOfIterations++;
        integral = Integrate(function, lowerBound, upperBound, oldIntegral, i + 1);
        if (NumberOfIterations >= MinNumberOfIterations)        // Avoid spurious early convergence
          if (Numeric.AreEqual(integral, oldIntegral, Epsilon))
            return integral;

        oldIntegral = integral;
      }
      return integral;
    }
  }
}
