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
  /// A distribution that returns random positions from a spherical volume.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Two radii can be specified: <see cref="InnerRadius"/> and <see cref="OuterRadius"/>. These
  /// radii define a spherical shell. The method <see cref="Next"/> returns random positions from
  /// within this shell. 
  /// </para>
  /// </remarks>
  public class SphereDistribution : Distribution<Vector3F>
  {
    /// <summary>
    /// Gets or sets the center of the sphere.
    /// </summary>
    /// <value>The center position. The default is (0, 0, 0).</value>
    public Vector3F Center
    {
      get { return _center; }
      set { _center = value; }
    }
    private Vector3F _center;


    /// <summary>
    /// Gets or sets the inner radius of the sphere.
    /// </summary>
    /// <value>The radius. The default is 0.</value>
    /// <remarks>
    /// The <see cref="InnerRadius"/> and the <see cref="OuterRadius"/> define a spherical shell. 
    /// Random values that are created are from the volume of this shell. If 
    /// <see cref="InnerRadius"/> is 0 (default), random values from the whole sphere volume are 
    /// created. If <see cref="InnerRadius"/> is equal to <see cref="OuterRadius"/>, all random 
    /// positions lie on the surface of the sphere.
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
        _innerVolume = _innerRadius * _innerRadius * _innerRadius;
      }
    }
    private float _innerRadius;
    private float _innerVolume;


    /// <summary>
    /// Gets or sets the outer radius of the circle.
    /// </summary>
    /// <value>The outer radius. The default is 1.</value>
    /// <remarks>
    /// The <see cref="InnerRadius"/> and the <see cref="OuterRadius"/> define a spherical shell. 
    /// Random values that are created are from the volume of this shell. If 
    /// <see cref="InnerRadius"/> is 0 (default), random values from the whole sphere volume are 
    /// created. If <see cref="InnerRadius"/> is equal to <see cref="OuterRadius"/>, all random 
    /// positions lie on the surface of the sphere.
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
        _outerVolume = _outerRadius * _outerRadius * _outerRadius;
      }
    }
    private float _outerRadius = 1;
    private float _outerVolume = 1;


    /// <summary>
    /// Gets or sets the scale factors that are multiplied to the random position.
    /// </summary>
    /// <value>The scale factors in x, y and z direction. The default value is (1, 1, 1).</value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Vector3F Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
    private Vector3F _scale = new Vector3F(1);


    /// <inheritdoc/>
    public override Vector3F Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      float z = random.NextFloat(-1, 1);
      
      // Project the "radius vector" z into the xy plane.
      float zProjected = (float)Math.Sqrt(1 - z * z) * Math.Sign(z);

      float angle = random.NextFloat(-ConstantsF.Pi, ConstantsF.Pi);
      float x = (float)Math.Cos(angle) * zProjected;
      float y = (float)Math.Sqrt(zProjected * zProjected - x * x) * Math.Sign(angle);
      
      Vector3F direction = new Vector3F(x, y, z);

      // Note: It is okay if inner and outer radius are swapped. No need to swap the values.
      float randomVolume = random.NextFloat(_innerVolume, _outerVolume);
      float radius = (float)Math.Pow(randomVolume, 1.0f / 3.0f);

      return _center + direction * radius * _scale;
    }
  }
}
