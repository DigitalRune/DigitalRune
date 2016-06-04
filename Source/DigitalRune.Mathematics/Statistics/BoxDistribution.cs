// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// A distribution that returns random positions from inside a box.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The box is axis-aligned with the local coordinate axes. The position and size of the box is 
  /// defined by the its <see cref="MinValue"/> corner and its <see cref="MaxValue"/> corner.
  /// </para>
  /// </remarks>
  public class BoxDistribution : Distribution<Vector3F>
  {
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    /// <value>The minium value. The default is (-1, -1, -1).</value>
    public Vector3F MinValue
    {
      get { return _minValue; }
      set { _minValue = value; }
    }
    private Vector3F _minValue = new Vector3F(-1, -1, -1);


    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    /// <value>The maximum value. The default is (1, 1, 1).</value>
    public Vector3F MaxValue
    {
      get { return _maxValue; }
      set { _maxValue = value; }
    }
    private Vector3F _maxValue = new Vector3F(1, 1, 1);
    

    /// <inheritdoc/>
    public override Vector3F Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      float x = random.NextFloat(_minValue.X, _maxValue.X);
      float y = random.NextFloat(_minValue.Y, _maxValue.Y);
      float z = random.NextFloat(_minValue.Z, _maxValue.Z);

      return new Vector3F(x, y, z);
    }
  }
}
