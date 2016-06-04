// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Represents a simulated vehicle.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class is very similar to <see cref="DigitalRune.Physics.Specialized.Vehicle"/>, but
  /// it uses constraints instead of simple force effects, which makes the car more stable.
  /// </para>
  /// <para>
  /// A vehicle consists of one rigid body for the <see cref="Chassis"/> and several 
  /// <see cref="Wheels"/>. 
  /// </para>
  /// <para>
  /// <strong>Using rays for wheels: </strong>
  /// Each wheel is a ray that samples the ground. This is very efficient and allows for tuning and 
  /// artificial behavior. 
  /// </para>
  /// <para>
  /// The forward direction of the vehicle is the -z direction.
  /// </para>
  /// </remarks>
  public class ConstraintVehicle
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
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
          throw new ArgumentNullException();

        _chassis = value;

        foreach (var wheel in Wheels)
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
          ConstraintVehicleHandler.Add(this);
          Simulation.RigidBodies.Add(Chassis);
          foreach (var wheel in Wheels)
          {
            wheel.Enabled = true;   // Wheel.Enabled adds the ray objects to the simulation.
          }
        }
        else
        {
          // Remove from simulation.
          ConstraintVehicleHandler.Remove(this);
          foreach (var wheel in Wheels)
          {
            wheel.Enabled = false;  // Wheel.Enabled removes the ray objects from the simulation.
          }
          Simulation.RigidBodies.Remove(Chassis);
          ConstraintVehicleHandler.Remove(this);
        }
      }
    }


    /// <summary>
    /// Gets the wheels.
    /// </summary>
    /// <value>The wheels. The default is an empty collection.</value>
    public NotifyingCollection<ConstraintWheel> Wheels { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintVehicle"/> class.
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
    public ConstraintVehicle(Simulation simulation, RigidBody chassis)
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");
      if (chassis == null)
        throw new ArgumentNullException("chassis");

      Simulation = simulation;
      _chassis = chassis;

      Wheels = new NotifyingCollection<ConstraintWheel>(false, false);
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
    private void OnWheelsChanged(object sender, CollectionChangedEventArgs<ConstraintWheel> eventArgs)
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

      _chassis.WakeUp();
    }
    #endregion
  }
}
