// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// A distribution that returns random positions from a circular area.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The circle lies in the xy-plane. Two radii can be specified:
  /// <see cref="InnerRadius"/> and <see cref="OuterRadius"/>. These radii define a ring. Random
  /// values from the area of this ring are created.
  /// </para>
  /// </remarks>
  public class CircleDistribution : Distribution<Vector3F>
  {
    /// <summary>
    /// Gets or sets the center of the circle.
    /// </summary>
    /// <value>The center position. The default is (0, 0, 0).</value>
    public Vector3F Center
    {
      get { return _center; }
      set { _center = value; }
    }
    private Vector3F _center;


    /// <summary>
    /// Gets or sets the inner radius of the ring.
    /// </summary>
    /// <value>The radius. The default is 0.</value>
    /// <remarks>
    /// The <see cref="InnerRadius"/> and the <see cref="OuterRadius"/> define a ring. Random values
    /// that are created are from the area of this ring. If <see cref="InnerRadius"/> is 0 (default),
    /// random values from the whole circle area are created. If <see cref="InnerRadius"/> is equal 
    /// to <see cref="OuterRadius"/>, all random values lie on the circumference of the circle.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float InnerRadius
    {
      get { return _innerRadius; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The inner radius must be greater than or equal 0.");

        _innerRadius = value;
        _innerArea = _innerRadius * _innerRadius;
      }
    }
    private float _innerRadius;
    private float _innerArea;


    /// <summary>
    /// Gets or sets the outer radius of the ring.
    /// </summary>
    /// <value>The outer radius. The default is 1.</value>
    /// <remarks>
    /// The <see cref="InnerRadius"/> and the <see cref="OuterRadius"/> define a ring. Random values
    /// that are created are from the area of this ring. If <see cref="InnerRadius"/> is 0 (default),
    /// random values from the whole circle area are created. If <see cref="InnerRadius"/> is equal 
    /// to <see cref="OuterRadius"/>, all random values lie on the circumference of the circle.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float OuterRadius
    {
      get { return _outerRadius; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The outer radius must be greater than or equal 0.");

        _outerRadius = value;
        _outerArea = _outerRadius * _outerRadius;
      }
    }
    private float _outerRadius = 1;
    private float _outerArea = 1;


    /// <summary>
    /// Gets or sets the scale factors that are multiplied to the random position.
    /// </summary>
    /// <value>The scale factors in x and y direction. The default value is (1, 1).</value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Vector2F Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
    private Vector2F _scale = new Vector2F(1, 1);


    /// <inheritdoc/>
    public override Vector3F Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      float angle = random.NextFloat(-ConstantsF.Pi, ConstantsF.Pi);

      // Note: It is okay if inner and outer area are swapped. No need to swap the values.
      float randomArea = random.NextFloat(_innerArea, _outerArea);
      float radius = (float)Math.Sqrt(randomArea);

      float x = (float)Math.Cos(angle) * radius;
      float y = (float)Math.Sqrt(radius * radius - x * x) * Math.Sign(angle);

      return _center + new Vector3F(x * _scale.X, y * _scale.Y, 0);
    }
  }
}
