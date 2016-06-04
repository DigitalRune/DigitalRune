// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;
using DigitalRune.Mathematics;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Represents a simulated vehicle.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A vehicle consists of one rigid body for the <see cref="Chassis"/> and several 
  /// <see cref="Wheels"/>. 
  /// </para>
  /// <para>
  /// <strong>Using rays for wheels: </strong>
  /// Each wheel is a ray that samples the ground. This is very efficient and allows for tuning and 
  /// artificial behavior. It is also more stable, because there are no constraints between the car 
  /// and wheel bodies which could be violated. The disadvantage is that the wheel movement is not 
  /// smooth when moving over non-smooth terrain (e.g. steps).
  /// </para>
  /// <para>
  /// The forward direction of the vehicle is the -z direction.
  /// </para>
  /// </remarks>
  public class Vehicle
  {
    // TODOs:
    // - Make up axis (= suspension axis) of wheels configurable.
    // - Try different friction/slip models. 
    //     - Reduce friction (more slip) at high speeds.
    //     - Current friction limit is circular (= equal for rolling friction and side friction).
    //       Try to use different friction for forward and side.
    // - Add drag force (F = k * v²).
    // - When the brakes are at maximum, do not rotate the wheels (visual effect only).
    // - Reduce grip on nearly vertical slopes.
    // - If wheels lose ground contact at high speeds, maybe something like an "allowed 
    //   penetration" depth can be used for the suspension.
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A force effect is used to update the car and to compute forces. The force effect is 
    // updated by the simulation. It is not necessary for the user to call something like
    // vehicle.Update().
    private readonly VehicleForceEffect _forceEffect;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the simulation.
    /// </summary>
    /// <value>The simulation.</value>
    public Simulation Simulation { get; private set; }


    /// <summary>
    /// Gets or sets the chassis.
    /// </summary>
    /// <value>The chassis.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null"/>.
    /// </exception>
    public RigidBody Chassis
    {
      get { return _chassis; }
      set 
      { 
        if (value == null)
          throw new ArgumentNullException("value");

        _chassis = value; 

        foreach(var wheel in Wheels)
          wheel.OnChassisChanged();
      }
    }
    private RigidBody _chassis;


    /// <summary>
    /// Gets or sets a value indicating whether this vehicle is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the vehicle is enabled; otherwise, <see langword="false"/>. 
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// All simulation objects are automatically added to or removed from the simulation and
    /// collision domain when the vehicle is enabled or disabled.
    /// </remarks>
    public bool Enabled
    {
      get { return Chassis.Simulation != null; }
      set
      {
        if (Enabled == value)
          return;

        if (value)
        {
          // Add to simulation.
          Simulation.ForceEffects.Add(_forceEffect);
          Simulation.RigidBodies.Add(Chassis);
          foreach (var wheel in Wheels)
          {
            wheel.Enabled = true;   // Wheel.Enabled adds the ray objects to the simulation.
          }
        }
        else
        {
          // Remove from simulation.
          foreach (var wheel in Wheels)
          {
            wheel.Enabled = false;  // Wheel.Enabled removes the ray objects from the simulation.
          }
          Simulation.RigidBodies.Remove(Chassis);
          Simulation.ForceEffects.Remove(_forceEffect);
        }
      }
    }


    /// <summary>
    /// Gets the wheels.
    /// </summary>
    /// <value>The wheels. The default is an empty collection.</value>
    public NotifyingCollection<Wheel> Wheels { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Vehicle"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <param name="chassis">The rigid body for the chassis.</param>
    /// <remarks>
    /// The car will NOT be automatically enabled! The property <see cref="Enabled"/> needs to be 
    /// set.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="simulation"/> or <paramref name="chassis"/> is <see langword="null"/>.
    /// </exception>
    public Vehicle(Simulation simulation, RigidBody chassis)
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");
      if (chassis == null)
        throw new ArgumentNullException("chassis");

      Simulation = simulation;
      _chassis = chassis;
      _forceEffect = new VehicleForceEffect(this);
      
      Wheels = new NotifyingCollection<Wheel>(false, false);
      Wheels.CollectionChanged += OnWheelsChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the <see cref="Wheels"/> collection is modified.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="CollectionChangedEventArgs{T}"/> instance containing the event data.
    /// </param>
    private void OnWheelsChanged(object sender, CollectionChangedEventArgs<Wheel> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Disable removed wheels.
      foreach (var wheel in eventArgs.OldItems)
      {
        wheel.Enabled = false;
        wheel.Vehicle = null;
      }

      // Enable newly added wheels.
      foreach (var wheel in eventArgs.NewItems)
      {
        if (wheel.Vehicle != null)
          throw new InvalidOperationException("Wheel cannot be added to vehicle because it already belongs to another vehicle.");

        wheel.Vehicle = this;
        wheel.Enabled = Enabled;
      }
    }


    /// <summary>
    /// Sets the steering angles for a standard 4 wheel car.
    /// </summary>
    /// <param name="steeringAngle">The steering angle.</param>
    /// <param name="frontLeft">The front left wheel.</param>
    /// <param name="frontRight">The front right wheel.</param>
    /// <param name="backLeft">The back left wheel.</param>
    /// <param name="backRight">The back right wheel.</param>
    /// <remarks>
    /// In a real car, the steerable front wheels do not always have the same steering angle. Have a
    /// look at http://www.asawicki.info/Mirror/Car%20Physics%20for%20Games/Car%20Physics%20for%20Games.html
    /// (section "Curves") for an explanation. The steering angle defines the angle of the inner
    /// wheel. The outer wheel is adapted. This works only for 4 wheels in a normal car setup.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="frontLeft"/>, <paramref name="frontRight"/>, <paramref name="backLeft"/>, or
    /// <paramref name="backRight"/> is <see langword="null"/>.
    /// </exception>
    public static void SetCarSteeringAngle(float steeringAngle, Wheel frontLeft, Wheel frontRight, Wheel backLeft, Wheel backRight)
    {
      if (frontLeft == null)
        throw new ArgumentNullException("frontLeft");
      if (frontRight == null)
        throw new ArgumentNullException("frontRight");
      if (backLeft == null)
        throw new ArgumentNullException("backLeft");
      if (backRight == null)
        throw new ArgumentNullException("backRight");

      backLeft.SteeringAngle = 0;
      backRight.SteeringAngle = 0;

      if (Numeric.IsZero(steeringAngle))
      {
        frontLeft.SteeringAngle = 0;
        frontRight.SteeringAngle = 0;
        return;
      }

      Wheel inner, outer;
      if (steeringAngle > 0)
      {
        inner = frontLeft;
        outer = frontRight;
      }
      else
      {
        inner = frontRight;
        outer = frontLeft;
      }

      inner.SteeringAngle = steeringAngle;

      float backToFront = backLeft.Offset.Z - frontLeft.Offset.Z;
      float rightToLeft = frontRight.Offset.X - frontLeft.Offset.X;

      float innerAngle = Math.Abs(steeringAngle);
      float outerAngle = (float)Math.Atan2(backToFront, backToFront / Math.Tan(innerAngle) + rightToLeft);

      outer.SteeringAngle = Math.Sign(steeringAngle) * outerAngle;
    }
    #endregion
  }
}
