// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.Settings
{
  /// <summary>
  /// Defines sleeping-related simulation settings.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Sleeping:</strong> Sleeping is also known as deactivation: Rigid bodies that do not
  /// move for a certain amount of time are deactivated ("put to sleep"). The simulation will do
  /// less work for sleeping bodies which results in a performance gain. Bodies are also put to
  /// sleep if they have a very low velocity for a certain period. For sleeping bodies the velocity
  /// is clamped to zero, which means that sleeping also improves the simulation stability by
  /// removing small (possible erroneous) velocities.
  /// </para>
  /// <para>
  /// Rigid bodies will start to sleep if their <see cref="RigidBody.LinearVelocity"/> is below
  /// <see cref="LinearVelocityThreshold"/> and their <see cref="RigidBody.AngularVelocity"/> is
  /// below <see cref="AngularVelocityThreshold"/> for a time larger than 
  /// <see cref="TimeThreshold"/>.
  /// </para>
  /// <para>
  /// The <see cref="RigidBody"/> class also has several sleep related methods and properties, see
  /// <see cref="RigidBody.CanSleep"/>, <see cref="RigidBody.IsSleeping"/>, 
  /// <see cref="RigidBody.Sleep"/> and <see cref="RigidBody.WakeUp"/>.
  /// </para>
  /// <para>
  /// <strong>Simulation Islands:</strong> A <see cref="SimulationIsland"/> can only sleep as a
  /// whole. It is not possible that some bodies in an island are sleeping and others are awake. If
  /// one object is awake all objects are awake because the movement of the awake body can propagate
  /// to the other bodies. In unfortunate configurations a jittering body can keep a whole island
  /// awake. 
  /// </para>
  /// </remarks>
  public class SleepingSettings
  {
    // Optional future settings:
    // - SleepingEnabled
    //   Not necessary because TimeThreshold can be set to large value. Or body.CanSleep
    //   can be set to false for all bodies.


    /// <summary>
    /// Gets or sets the linear sleeping velocity threshold.
    /// </summary>
    /// <value>
    /// The linear velocity threshold. The default is 0.4.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float LinearVelocityThreshold
    {
      get { return _linearVelocityThreshold; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "LinearVelocityThreshold must be greater than or equal to 0.");

        _linearVelocityThreshold = value;
        LinearVelocityThresholdSquared = value * value;
      }
    }
    private float _linearVelocityThreshold;
    internal float LinearVelocityThresholdSquared { get; private set; }


    /// <summary>
    /// Gets or sets the angular sleeping velocity threshold.
    /// </summary>
    /// <value>
    /// The angular velocity threshold. The default is 0.5.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float AngularVelocityThreshold
    {
      get { return _angularVelocityThreshold; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "AngularVelocityThreshold must be greater than or equal to 0.");

        _angularVelocityThreshold = value;
        AngularVelocityThresholdSquared = value * value;
      }
    }
    private float _angularVelocityThreshold;
    internal float AngularVelocityThresholdSquared { get; private set; }


    /// <summary>
    /// Gets or sets the sleeping time threshold in seconds.
    /// </summary>
    /// <value>
    /// The time threshold in seconds. The default is 1 s.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float TimeThreshold
    {
      get { return _timeThreshold; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "TimeThreshold must be greater than or equal to 0.");

        _timeThreshold = value;
      }
    }
    private float _timeThreshold;


    //public float WakeUpForceLimit
    //{
    //  get { return _wakeUpForceLimit; }
    //  set
    //  {
    //    _wakeUpForceLimit = value;
    //    WakeUpForceLimitSquared = value * value;
    //  }
    //}
    //private float _wakeUpForceLimit;
    //internal float WakeUpForceLimitSquared { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="SleepingSettings"/> class.
    /// </summary>
    public SleepingSettings()
    {
      AngularVelocityThreshold = 0.5f;
      LinearVelocityThreshold = 0.4f;
      TimeThreshold = 1;
    }
  }
}
