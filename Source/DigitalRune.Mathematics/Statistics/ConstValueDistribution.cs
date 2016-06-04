// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Represents a probability distribution that contains only 1 value with 100% probability. Hence 
  /// this distribution always returns a single constant value - no uncertainty.
  /// </summary>
  /// <typeparam name="T">The type of the constant.</typeparam>
  public class ConstValueDistribution<T> : Distribution<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the single constant value.
    /// </summary>
    /// <value>The single constant value. The default is 0.</value>
    public T Value { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstValueDistribution{T}"/> class.
    /// </summary>
    public ConstValueDistribution()
    {  
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConstValueDistribution{T}"/> class.
    /// </summary>
    /// <param name="value">The single constant value.</param>
    public ConstValueDistribution(T value)
    {
      Value = value;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override T Next(Random random)
    {
      return Value;
    }
    #endregion
  }
}
