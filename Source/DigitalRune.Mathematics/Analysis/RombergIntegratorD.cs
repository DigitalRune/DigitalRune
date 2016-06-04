// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// Performs numerical integration using the <i>Romberg's method</i> (double-precision).
  /// </summary>
  public class RombergIntegratorD : IntegratorD
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    public override double Integrate(Func<double, double> function, double lowerBound, double upperBound)
    {
      NumberOfIterations = 0;

      if (function == null)
        throw new ArgumentNullException("function");

      // see http://de.wikipedia.org/wiki/Romberg-Integration

      if (lowerBound == upperBound)
        return 0.0;

      double[,] i = new double[MaxNumberOfIterations + 1, MaxNumberOfIterations + 1];
      double h = upperBound - lowerBound;
      double fLowerBound = function(lowerBound);
      double fUpperBound = function(upperBound);
      i[0, 0] = h / 2.0 * (fLowerBound + fUpperBound);

      int n;
      for (n = 1; n <= MaxNumberOfIterations; n++)
      {
        NumberOfIterations++;

        double temp = 0;
        int steps = (int)Math.Pow(2, n);
        for (int j = 1; j < steps; j++)
          temp += function(lowerBound + ((j * h) / steps));

        i[n, 0] = h / (2 * steps) * (fLowerBound + fUpperBound + 2 * temp);

        int k;
        for (k = 1; k <= n; k++)
        {
          int s = n - k;
          i[s, k] = (Math.Pow(4, k) * i[s + 1, k - 1] - i[s, k - 1]) / (Math.Pow(4, k) - 1.0);
        }

        if (NumberOfIterations >= MinNumberOfIterations)
          if (Numeric.AreEqual(i[0, n], i[0, n - 1], Epsilon))
            return i[0, n];
      }

      return i[0, n - 1];
    }
  }
}
