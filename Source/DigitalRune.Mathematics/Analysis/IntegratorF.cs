// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Analysis
{
  /// <summary>
  /// A base class for numerical integration of a function over an interval (single-precision).
  /// </summary>
  public abstract class IntegratorF
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int _maxNumberOfIterations = 20;
    private int _minNumberOfIterations = 5;
    private float _epsilon = Numeric.EpsilonF;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) the number of iterations of the last <see cref="Integrate"/> method call.
    /// </summary>
    /// <value>The number of iterations.</value>
    /// <remarks>
    /// This property is not thread-safe.
    /// </remarks>
    public int NumberOfIterations { get; protected set; }


    /// <summary>
    /// Gets or sets the minimum number number of iterations.
    /// </summary>
    /// <value>The minimum number number of iterations. The default value is 5.</value>
    /// <remarks>
    /// It is best to perform a minimum of iterations because for some periodic functions the
    /// computed integral value seems to converge at first and it needs a few iterations until a
    /// correct value is computed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int MinNumberOfIterations
    {
      get { return _minNumberOfIterations; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The min number of iterations must be greater than 0.");

        _minNumberOfIterations = value;
      }
    }


    /// <summary>
    /// Gets or sets the maximum number number of iterations.
    /// </summary>
    /// <value>The maximum number number of iterations. The default value is 20.</value>
    /// <remarks>
    /// In one call of <see cref="Integrate"/> no more than <see cref="MaxNumberOfIterations"/>
    /// are performed.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int MaxNumberOfIterations
    {
      get { return _maxNumberOfIterations; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The max number of iterations must be greater than 0.");

        _maxNumberOfIterations = value;
      }
    }


    /// <summary>
    /// Gets or sets the tolerance value. 
    /// </summary>
    /// <value>
    /// The tolerance value. The default is <see cref="Numeric"/>.<see cref="Numeric.EpsilonF"/>.
    /// </value>
    /// <remarks>
    /// If the absolute difference between the integral from the new iteration and the last 
    /// iteration is less than this tolerance, the integration is stopped.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public float Epsilon
    {
      get { return _epsilon; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The tolerance value must be greater than zero.");

        _epsilon = value;
      }
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
    /// Integrates the specified function within the given interval.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="lowerBound">The lower bound.</param>
    /// <param name="upperBound">The upper bound.</param>
    /// <returns>
    /// The integral of the given function over the interval 
    /// [<paramref name="lowerBound"/>, <paramref name="upperBound"/>].
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    public abstract float Integrate(Func<float, float> function, float lowerBound, float upperBound);
    #endregion
  }
}
