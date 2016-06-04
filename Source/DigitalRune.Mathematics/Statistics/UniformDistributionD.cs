// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Represents a uniform distribution that returns random values for a given interval 
  /// [<see cref="MinValue"/>, <see cref="MaxValue"/>] (double-precision).
  /// </summary>
  /// <remarks>
  /// Every time <see cref="Next"/> is called, a new random value from the interval 
  /// [<see cref="MinValue"/>, <see cref="MaxValue"/>] is returned. All values in this interval have
  /// the same chance to be chosen.
  /// </remarks>
  public class UniformDistributionD : Distribution<double>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    /// <value>The maximum value. The default is 1.</value>
    public double MaxValue { get; set; }


    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    /// <value>The minimum value. The default is -1.</value>
    public double MinValue { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="UniformDistributionD"/> class.
    /// </summary>
    public UniformDistributionD()
    {
      MinValue = -1;
      MaxValue = 1;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformDistributionD"/> class.
    /// </summary>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    public UniformDistributionD(double minValue, double maxValue)
    {
      MinValue = minValue;
      MaxValue = maxValue;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override double Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      return MinValue + (MaxValue - MinValue) * random.NextDouble();
    }
    #endregion
  }
}
