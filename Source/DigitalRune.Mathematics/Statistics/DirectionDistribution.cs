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
  /// A distribution that returns a random direction vector.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Direction"/> defines the main direction. <see cref="Next"/> returns a random
  /// direction vector that randomly deviates from <see cref="Direction"/>. The deviation can be
  /// uniformly distributed or follow an approximated Gaussian distribution (similar to
  /// <see cref="FastGaussianDistributionF"/>); see <see cref="IsUniform"/>. If the distribution is
  /// uniform (<see cref="IsUniform"/> is <see langword="true"/>, default), <see cref="Deviation"/>
  /// defines the maximal deviation angle in radians. If the distribution is Gaussian 
  /// (<see cref="IsUniform"/> is <see langword="false"/>), <see cref="Deviation"/> defines the
  /// standard deviation angle in radians.
  /// </para>
  /// </remarks>
  public class DirectionDistribution : Distribution<Vector3F>
  {
    /// <summary>
    /// Gets or sets the direction of the cone (the central vector in the cone).
    /// </summary>
    /// <value>The direction of the cone. The default is (0, 1, 0).</value>
    /// <exception cref="ArgumentException">
    /// The vector is not a valid direction. The length is 0.
    /// </exception>
    public Vector3F Direction
    {
      get { return _direction; }
      set
      {
        if (_direction != value)
        {
          _direction = value;
          try
          {
            _normalizedDirection = _direction.Normalized;
            _orthonormal = _normalizedDirection.Orthonormal1;
          }
          catch (DivideByZeroException)
          {
            throw new ArgumentException("The vector is not a valid direction. The length is numerically 0.");
          }
        }
      }
    }
    private Vector3F _direction = Vector3F.UnitY;
    private Vector3F _normalizedDirection = Vector3F.UnitY;  // For optimization.
    private Vector3F _orthonormal = Vector3F.UnitZ;


    /// <summary>
    /// Gets or sets the angle of the cone measured from the central vector to a border vector.
    /// </summary>
    /// <value>The angle of the cone in radians. The default is π/4 radians (= 45°).</value>
    public float Deviation
    {
      get { return _deviation; }
      set { _deviation = value; }
    }
    private float _deviation = ConstantsF.PiOver4;


    /// <summary>
    /// Gets or sets a value indicating whether the random direction vectors are distributed
    /// uniformly or follow a Gaussian distribution
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this distribution is uniform; otherwise, <see langword="false"/>
    /// if the distribution is Gaussian. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// For the Gaussian distribution an approximated Gaussian distribution is used similar to 
    /// <see cref="FastGaussianDistributionF"/>.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public bool IsUniform
    {
      get { return _isUniform; }
      set { _isUniform = value; }
    }
    private bool _isUniform = true;


    
    /// <inheritdoc/>
    public override Vector3F Next(Random random)
    {
      if (random == null)
        throw new ArgumentNullException("random");

      float angle1;
      if (IsUniform)
        angle1 = (float)random.NextDouble();
      else
        angle1 = (float)(random.NextDouble(-1, 1) + random.NextDouble(-1, 1) + random.NextDouble(-1, 1));

      angle1 *= _deviation;
        
      float angle2 = (float)random.NextDouble() * ConstantsF.TwoPi;

      // TODO: Optimize!
      return (QuaternionF.CreateRotation(_normalizedDirection, angle2) * QuaternionF.CreateRotation(_orthonormal, angle1)).Rotate(_normalizedDirection);
    }
  }
}
