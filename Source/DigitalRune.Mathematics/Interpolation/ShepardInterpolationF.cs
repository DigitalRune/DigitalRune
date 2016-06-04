// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Scattered Interpolation using Shepard's method (single-precision).
  /// </summary>
  /// <remarks>
  /// Implemented as described in the paper "Pose Space Deformation: A Unified Approach to Shape 
  /// Interpolation and Skeleton-Driven Deformation".
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public class ShepardInterpolationF : ScatteredInterpolationF
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private float _power = 2;
    private VectorF _weights;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the exponent for the power of the distance.
    /// </summary>
    /// <value>The exponent. The default value is <c>2.0f</c></value>
    /// <remarks>
    /// The reference data values are weighted by the inverse power of the distance: 
    /// weight<sub>1</sub> = | x - x<sub>1</sub> | <sup>-Power</sup>.
    /// </remarks>
    public float Power
    {
      get { return _power; }
      set { _power = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when <see cref="ScatteredInterpolationF.Setup"/> is called.
    /// </summary>
    /// <remarks>
    /// Here internal values can be computed from the registered reference pairs if required. It is
    /// assured that the reference data pairs have valid dimensions: All x values have the same
    /// number of elements and all y values have the same number of elements. All reference data
    /// values are not <see langword="null"/>.
    /// </remarks>
    protected override void OnSetup()
    {
      _weights = new VectorF(Count); 
    }


    /// <summary>
    /// Called when <see cref="ScatteredInterpolationF.Compute"/> is called.
    /// </summary>
    /// <param name="x">The x value.</param>
    /// <returns>The y value.</returns>
    /// <remarks>
    /// When this method is called, <see cref="ScatteredInterpolationF.Setup"/> has already been
    /// executed. And the parameter <paramref name="x"/> is not <see langword="null"/>.
    /// </remarks>
    protected override VectorF OnCompute(VectorF x)
    {
      // Compute weights.
      float weightSum = 0;
      int numberOfPairs = Count;
      for (int i = 0; i < numberOfPairs; i++)
      {
        _weights[i] = (float) Math.Pow((x - GetX(i)).Length + Numeric.EpsilonF, -Power);
        weightSum += _weights[i];
      }

      // Compute result as weighted sum.
      VectorF y = new VectorF(GetY(0).NumberOfElements);
      for (int i = 0; i < numberOfPairs; i++)
        y += _weights[i] * GetY(i);

      return y / weightSum;
    }
    #endregion
  } 
}
