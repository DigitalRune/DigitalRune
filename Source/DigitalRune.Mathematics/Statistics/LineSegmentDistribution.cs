// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// A distribution that returns random positions on a line segment.
  /// </summary>
  public class LineSegmentDistribution : Distribution<Vector3F>
  {
    /// <summary>
    /// Gets or sets the start position of the line segment.
    /// </summary>
    /// <value>The start position. The default is (-1, -1, -1).</value>
    public Vector3F Start
    {
      get { return _start; }
      set { _start = value; }
    }
    private Vector3F _start = new Vector3F(-1);


    /// <summary>
    /// Gets or sets the end position of the line segment.
    /// </summary>
    /// <value>The end position. The default is (1, 1, 1).</value>
    public Vector3F End
    {
      get { return _end; }
      set { _end = value; }
    }
    private Vector3F _end = new Vector3F(1);


    /// <inheritdoc/>
    public override Vector3F Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      return _start + (_end - _start) * (float)random.NextDouble();
    }
  }
}
