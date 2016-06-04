// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a force onto rigid bodies.
  /// </summary>
  /// <remarks>
  /// <para>
  /// If a force effect is added to a <see cref="Physics.Simulation"/> (see 
  /// <see cref="Physics.Simulation.ForceEffects"/>), it will be called during a simulation time
  /// step to apply forces to the rigid bodies of the simulation.
  /// </para>
  /// <para>
  /// <strong>Self-Removal:</strong> It is allowed that a force effect instance removes itself at
  /// any time from the <see cref="Physics.Simulation"/>. This can be used to create one-shot
  /// effects - a force effect that applies forces and automatically removes itself from the
  /// simulation in <see cref="OnApply"/>.
  /// </para>
  /// <para>
  /// <strong>Order of events:</strong> The force effect has three special On<i>Xxx</i> methods that
  /// will be used in this order:
  /// <list type="table">
  /// <item>
  /// <term><see cref="OnAddToSimulation"/></term>
  /// <description>
  /// Is called when the force effect instance was added to the 
  /// <see cref="Physics.Simulation.ForceEffects"/> collection of a 
  /// <see cref="Physics.Simulation"/>. Before this method is called, the property 
  /// <see cref="Simulation"/> is <see langword="null"/>. When this method is called, the property
  /// <see cref="Simulation"/> is set to the simulation.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="OnApply"/></term>
  /// <description>
  /// Is called when the simulation wants the force effect to apply its forces to the rigid bodies.
  /// <see cref="OnApply"/> can be called after <see cref="OnAddToSimulation"/>, but never before.
  /// The property <see cref="Simulation"/> is always initialized when <see cref="OnApply"/> is
  /// called. <see cref="OnApply"/> is only called if the force effect is <see cref="Enabled"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="OnRemoveFromSimulation"/></term>
  /// <description>
  /// Is called before the force effect instance is removed from the 
  /// <see cref="Physics.Simulation.ForceEffects"/> collection of <see cref="Physics.Simulation"/>.
  /// When this method is called, the property <see cref="Simulation"/> is still set. After 
  /// <see cref="OnRemoveFromSimulation"/> the property <see cref="Simulation"/> is 
  /// <see langword="null"/>. <see cref="OnApply"/> can only be called between 
  /// <see cref="OnAddToSimulation"/> and <see cref="OnRemoveFromSimulation"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>Applying Forces:</strong> A <see cref="ForceEffect"/> must not call 
  /// <see cref="RigidBody"/>.<see cref="RigidBody.AddForce(Vector3F)"/> or other force related
  /// methods of the <see cref="RigidBody"/> directly. Those methods are reserved for the user. The
  /// reason is: If a user applies a force with 
  /// <see cref="RigidBody"/>.<see cref="RigidBody.AddForce(Vector3F)"/>, the added force is 
  /// constant for the whole duration of 
  /// <see cref="Physics.Simulation"/>.<see cref="Physics.Simulation.Update(TimeSpan)"/>. If the 
  /// simulation divides one call to <see cref="Physics.Simulation.Update(TimeSpan)"/> in several 
  /// sub time steps, the same user force is applied in all sub time steps. But force effects are 
  /// called by the simulation in each sub time step. They can set a different force in each sub 
  /// time step. Therefore, force effects must use a different set of methods and these methods are 
  /// <see cref="AddForce(RigidBody, Vector3F, Vector3F)"/>, 
  /// <see cref="AddForce(RigidBody, Vector3F)"/> and <see cref="AddTorque(RigidBody, Vector3F)"/> 
  /// of this class. 
  /// </para>
  /// <para>
  /// To sum up: <i>Classes derived from <see cref="ForceEffect"/> must use the 
  /// <strong>AddForce</strong>/<strong>AddTorque</strong> methods of the <see cref="ForceEffect"/> 
  /// base class to apply forces to rigid bodies.</i>
  /// </para>
  /// </remarks>
  public abstract class ForceEffect
  {
    // Order: Simulation --> OnAddToSimulation --> OnApply --> ... --> OnApply --> OnRemoveFromSimulation --> Simulation = null 

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the simulation to which this force effect belongs.
    /// </summary>
    /// <value>The simulation.</value>
    /// <remarks>
    /// This method is <see langword="null"/> before <see cref="OnAddToSimulation"/> and after
    /// <see cref="OnRemoveFromSimulation"/>. Between these method calls this property is set to 
    /// the simulation that owns this force effect.
    /// </remarks>
    public Simulation Simulation
    {
      get { return _simulation; }
      internal set
      {
        if (_simulation != value)
        {
          if (_simulation != null)
            OnRemoveFromSimulation();

          _simulation = value;

          if (_simulation != null)
            OnAddToSimulation();
        }
      }
    }
    private Simulation _simulation;


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ForceEffect"/> is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// If this property is <see langword="false"/>, the force effect does not apply any
    /// forces. <see cref="OnApply"/> is only called if this property is <see langword="true"/>.
    /// </remarks>
    public bool Enabled
    {
      get { return _enabled; }
      set 
      { 
        if (value != _enabled)
        {
          _enabled = value;

          if (_enabled)
            OnEnabled();
          else
            OnDisabled();
        }
        
      }
    }
    private bool _enabled;


    //public float Duration { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ForceEffect"/> class.
    /// </summary>
    protected ForceEffect()
    {
      _enabled = true;  // Note: Virtual OnEnabled() must not be called in constructor.
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Applies forces to the rigid bodies.
    /// </summary>
    internal void Apply()
    {
      if (Enabled)
        OnApply();
    }


    /// <summary>
    /// Called when the simulation wants this force effect to apply forces to rigid bodies.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong>
    /// This method must be implemented in derived classes. This method is only called after the
    /// force effect was added to a simulation and <see cref="OnAddToSimulation"/> was called. 
    /// </para>
    /// <para>
    /// This method is responsible for applying the forces of the effect to the rigid bodies. To
    /// apply a force the methods <see cref="ForceEffect.AddForce(RigidBody, Vector3F, Vector3F)"/>, 
    /// <see cref="ForceEffect.AddForce(RigidBody, Vector3F)"/> and/or 
    /// <see cref="ForceEffect.AddTorque(RigidBody, Vector3F)"/> of the <see cref="ForceEffect"/> 
    /// base class must be used. Do not use the <strong>AddForce</strong>/<strong>AddTorque</strong> 
    /// methods of the <see cref="RigidBody"/> class.
    /// </para>
    /// </remarks>
    protected abstract void OnApply();


    /// <summary>
    /// Called when this force effect was enabled.
    /// </summary>
    protected virtual void OnEnabled()
    {
    }


    /// <summary>
    /// Called when this force effect was disabled.
    /// </summary>
    protected virtual void OnDisabled()
    {
    }


    /// <summary>
    /// Called when this force effect is added to a simulation.
    /// </summary>
    /// <remarks>
    /// The simulation to which the force effect is added is set in the property 
    /// <see cref="Simulation"/>.
    /// </remarks>
    protected virtual void OnAddToSimulation()
    {
    }


    /// <summary>
    /// Called when this force effect is removed from a simulation.
    /// </summary>
    /// <remarks>
    /// The simulation from which the force effect is removed is set in the property 
    /// <see cref="Simulation"/>. After <see cref="OnRemoveFromSimulation"/> the property 
    /// <see cref="Simulation"/> will be reset to <see langword="null"/>.
    /// </remarks>
    protected virtual void OnRemoveFromSimulation()
    {
    }


    /// <summary>
    /// Applies a force to the rigid body.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <param name="forceWorld">The force in world space.</param>
    /// <param name="positionWorld">
    /// The world space position where the force is a applied on the body.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    protected static void AddForce(RigidBody body, Vector3F forceWorld, Vector3F positionWorld)
    {
      Vector3F r = positionWorld - body.PoseCenterOfMass.Position;
      body.AccumulatedForce += forceWorld;
      body.AccumulatedTorque += Vector3F.Cross(r, forceWorld);
    }


    /// <summary>
    /// Applies a force to the rigid body at the center of mass.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <param name="forceWorld">The force in world space.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    protected static void AddForce(RigidBody body, Vector3F forceWorld)
    {
      body.AccumulatedForce += forceWorld;
    }


    /// <summary>
    /// Applies a torque to the rigid body at the center of mass.
    /// </summary>
    /// <param name="body">The rigid body.</param>
    /// <param name="torqueWorld">The torque in world space.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    protected static void AddTorque(RigidBody body, Vector3F torqueWorld)
    {
      body.AccumulatedTorque += torqueWorld;
    }
    #endregion
  }
}
