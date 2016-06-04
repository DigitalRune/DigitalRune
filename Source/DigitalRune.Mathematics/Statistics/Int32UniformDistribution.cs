// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Represents a uniform distribution that returns random <see cref="Int32"/> values for a given 
  /// interval [<see cref="MinValue"/>, <see cref="MaxValue"/>].
  /// </summary>
  /// <remarks>
  /// Every time <see cref="Next"/> is called, a new random value from the interval 
  /// [<see cref="MinValue"/>, <see cref="MaxValue"/>] is returned. All values in this interval have
  /// the same chance to be chosen.
  /// </remarks>
  public class Int32UniformDistribution : Distribution<int>
  {
    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    /// <value>The maximum value. The default is 100.</value>
    public int MaxValue { get; set; }


    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    /// <value>The minimum value. The default is 0.</value>
    public int MinValue { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="Int32UniformDistribution"/> class.
    /// </summary>
    public Int32UniformDistribution()
    {
      MinValue = 0;
      MaxValue = 100;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Int32UniformDistribution"/> class.
    /// </summary>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    public Int32UniformDistribution(int minValue, int maxValue)
    {
      MinValue = minValue;
      MaxValue = maxValue;
    }


    /// <inheritdoc/>
    public override int Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      return random.NextInteger(MinValue, MaxValue);
    }
  }
}
