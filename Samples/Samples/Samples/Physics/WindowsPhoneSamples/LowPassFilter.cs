using DigitalRune.Mathematics.Algebra;


namespace Samples.Physics
{
  /// <summary>
  /// Filters a <see cref="Vector3F"/> signal using a first order low-pass filter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Increase the <see cref="TimeConstant"/> to increase the filter effect.
  /// </para>
  /// <para>
  /// The used filter is a first order IIR filter.
  /// </para>
  /// </remarks>
  internal class LowPassFilter
  {
    private Vector3F _filteredValue;


    /// <summary>
    /// Gets or sets the time constant (in seconds).
    /// </summary>
    /// <value>The time constant (in seconds).</value>
    /// <remarks>The default is 0.1 s.</remarks>
    public float TimeConstant { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="LowPassFilter"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    public LowPassFilter(Vector3F initialValue)
    {
      TimeConstant = 0.1f;
      _filteredValue = initialValue;
    }


    /// <summary>
    /// Filters the specified value.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="deltaTime">The time since the last call of <see cref="Filter"/>.</param>
    /// <returns>The filtered value.</returns>
    public Vector3F Filter(Vector3F value, float deltaTime)
    {
      float p = deltaTime;
      float weight1 = p / (p + TimeConstant);
      float weight2 = TimeConstant / (p + TimeConstant);
      _filteredValue = value * weight1 + _filteredValue * weight2;
      return _filteredValue;
    }
  }
}
