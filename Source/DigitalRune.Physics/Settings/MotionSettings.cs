// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.Settings
{
  /// <summary>
  /// Defines motion-related simulation settings.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Continuous Collision Detection (CCD):</strong> The purpose of CCD is to detect 
  /// collisions of fast moving objects that would otherwise be missed. The opposite of CCD is
  /// Discrete Collision Detection which only checks for collision at one position for each frame.
  /// If the objects move, collisions between the old position and new position are not detected.
  /// Example: A bullet (e.g. a small sphere shape) is fired at a wall. In one frame the bullet is 
  /// in front of the wall. In the next frame the bullet is behind the wall without touching it. If 
  /// this happens Discrete Collision Detection does not report a collision. This problem is known
  /// as "tunneling". CCD is more expensive but detects all collisions. (CCD has some limitations, 
  /// for example, some collisions that occur because of rotational movement can be missed. But 
  /// such limitations are hardly noticeable).
  /// </para>
  /// <para>
  /// <strong>Motion Clamping:</strong> If CCD is enabled, the <see cref="Simulation"/> uses a
  /// strategy called "Motion Clamping" to deal with fast moving objects: In each time step fast
  /// moving objects are only moved to their first time of impact, so that a collision is detected
  /// in the next time step. This is obviously not physically correct because the fast moving object
  /// moves a smaller distance than it should - but for fast moving objects, like bullets, it is
  /// more important to detect all collisions and it is usually not noticeable when the movement 
  /// distance is clamped.
  /// </para>
  /// <para>
  /// <strong>CCD Settings:</strong> CCD can be globally enabled or disabled with 
  /// <see cref="CcdEnabled"/>. CCD is only performed for rigid bodies that have a velocity beyond 
  /// <see cref="CcdVelocityThreshold"/> and if <see cref="RigidBody.CcdEnabled"/> is set. Further,
  /// a filter predicate method (<see cref="CcdFilter"/>) can be set. CCD is only used if no 
  /// predicate method is set or if the predicate method returns <see langword="true"/> for a pair
  /// of rigid bodies.
  /// </para>
  /// </remarks>
  public class MotionSettings
  {
    /// <summary>
    /// Gets or sets a value indicating whether Continuous Collision Detection (CCD) is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if CCD is enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// By setting this property to <see langword="false"/> the Continuous Collision Detection can 
    /// be globally disabled in a physics simulation. 
    /// </para>
    /// <para>
    /// CCD can also be enabled/disabled per rigid body (see 
    /// <see cref="RigidBody"/>.<see cref="RigidBody.CcdEnabled"/>); or for pairs of rigid bodies 
    /// by using the <see cref="CcdFilter"/> predicate.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool CcdEnabled { get; set; }


    /// <summary>
    /// Gets or sets a predicate method that defines whether CCD is enabled between a pair of rigid 
    /// bodies.
    /// </summary>
    /// <value>
    /// A method that returns <see langword="true"/> if CCD should be used. If this method returns 
    /// <see langword="false"/>, CCD is not used for the given pair of bodies. If this value is 
    /// <see langword="null"/>, CCD is enabled for all pairs. The default is 
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The method does not need to check whether parameters are <see langword="null"/>. It is 
    /// guaranteed that the method is never called with <see langword="null"/> parameters.
    /// </para>
    /// <para>
    /// <strong>Thread-safety:</strong> This method may be called concurrently from different
    /// threads. It must be safe for threading!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Func<RigidBody, RigidBody, bool> CcdFilter { get; set; }


    /// <summary>
    /// Gets or sets the velocity threshold for Continuous Collision Detection (CCD).
    /// </summary>
    /// <value>
    /// The CCD velocity threshold. The default value is 5.
    /// </value>
    /// <remarks>
    /// Continuous Collision Detection (CCD) is only performed for rigid bodies that are faster than
    /// this velocity limit.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float CcdVelocityThreshold
    {
      get { return _ccdVelocityThreshold; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "CcdVelocityThreshold must be greater than or equal to 0.");

        _ccdVelocityThreshold = value;
        CcdVelocityThresholdSquared = value * value;
      }
    }
    private float _ccdVelocityThreshold;
    internal float CcdVelocityThresholdSquared { get; private set; }


    /// <summary>
    /// Gets or sets the maximal linear velocity.
    /// </summary>
    /// <value>The maximal linear velocity. The default is 100.</value>
    /// <remarks>
    /// All linear velocities are clamped to this limit because high velocities can lead to 
    /// unstable simulations.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxLinearVelocity
    {
      get { return _maxLinearVelocity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxLinearVelocity must be greater than or equal to 0.");

        _maxLinearVelocity = value;
        MaxLinearVelocitySquared = value * value;
      }
    }
    private float _maxLinearVelocity;
    internal float MaxLinearVelocitySquared { get; private set; }


    /// <summary>
    /// Gets or sets the maximal angular velocity.
    /// </summary>
    /// <value>
    /// The maximal angular velocity. The default is 100.
    /// </value>
    /// <remarks>
    /// All angular velocities are clamped to this limit because high velocities can lead to 
    /// unstable simulations.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxAngularVelocity
    {
      get { return _maxAngularVelocity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxAngularVelocity must be greater than or equal to 0.");

        _maxAngularVelocity = value;
        MaxAngularVelocitySquared = value * value;
      }
    }
    private float _maxAngularVelocity;
    internal float MaxAngularVelocitySquared { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether bodies that leave the simulation are automatically
    /// removed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if bodies outside the simulation world are removed; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The area of a simulation is defined by <see cref="Simulation.World"/>. If this flag is set
    /// to <see langword="true"/>, bodies that leave the axis-aligned bounding box of this area are
    /// automatically removed from the simulation. Bodies are also removed if a value in their 
    /// pose is NaN.
    /// </para>
    /// </remarks>
    public bool RemoveBodiesOutsideWorld { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="MotionSettings"/> class.
    /// </summary>
    public MotionSettings()
    {
      CcdEnabled = true;
      CcdVelocityThreshold = 5;
      MaxLinearVelocity = 100;
      MaxAngularVelocity = 100;
      RemoveBodiesOutsideWorld = true;
    }
  }
}
