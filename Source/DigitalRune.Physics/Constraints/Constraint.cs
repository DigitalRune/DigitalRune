// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Constraints
{  
  /// <summary>
  /// Defines a constraint between two rigid bodies.
  /// </summary>
  /// <remarks>
  /// A constraint limits the movement of two bodies relative two each other. It restricts the
  /// degrees of movement of one body relative to the other body.
  /// </remarks>
  public abstract class Constraint : IConstraint
  {
    // If something important changes, call OnChanged. The bodies will wake up automatically.
    // Override OnChanged to reset values (cached impulses, etc.).
    // BodyX = null not allowed, use Simulation.World instead.
    //
    // Optional:
    // - Property Name for debugging.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first body.
    /// </summary>
    /// <value>
    /// The first body. (Use the special <see cref="Physics.Simulation.World"/> body to constrain
    /// the other body relative to world space.)
    /// </value>
    /// <remarks>
    /// If you want to constrain a rigid body relative to world space, use the special
    /// <see cref="Physics.Simulation.World"/> as the other body of the two constraint bodies.
    /// </remarks>
    public RigidBody BodyA
    {
      get { return _bodyA; }
      set
      {
        if (_bodyA != value)
        {
          RemoveFromCollisionFilter();
          _bodyA = value;
          AddToCollisionFilter();
          OnChanged();
        }
      }
    }
    private RigidBody _bodyA;


    /// <summary>
    /// Gets or sets the second body.
    /// </summary>
    /// <value>
    /// The second body. (Use the special <see cref="Physics.Simulation.World"/> body to constrain
    /// the other body relative to world space.)
    /// </value>
    /// <remarks>
    /// If you want to constrain a rigid body relative to world space, use the special
    /// <see cref="Physics.Simulation.World"/> as the other body of the two constraint bodies.
    /// </remarks>
    public RigidBody BodyB
    {
      get { return _bodyB; }
      set
      {
        if (_bodyB != value)
        {
          RemoveFromCollisionFilter();
          _bodyB = value;
          AddToCollisionFilter();
          OnChanged();
        }
      }
    }
    private RigidBody _bodyB;


    /// <summary>
    /// Gets or sets a value indicating whether this constraint is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <see cref="Enabled"/> can be set to <see langword="false"/> to temporarily disable the
    /// constraint. If the constraint should be disabled for a longer period, it is more efficient
    /// to remove the constraint from the <see cref="Simulation"/>.
    /// </remarks>
    public bool Enabled
    {
      get { return _enabled; }
      set
      {
        if (_enabled != value)
        {
          RemoveFromCollisionFilter();
          _enabled = value;
          AddToCollisionFilter();
          OnChanged();
        }
      }
    }
    private bool _enabled = true;


    /// <summary>
    /// Gets or sets a value indicating whether collisions between <see cref="BodyA"/> and
    /// <see cref="BodyB"/> are disabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if collisions are enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/> (= collisions are enabled).
    /// </value>
    /// <remarks>
    /// <para>
    /// This property can be set to <see langword="false"/> to disable collision detection between
    /// the constraint bodies. Disabling collisions improves performance and is often necessary,
    /// for example, for two connected limbs in a ragdoll that are always penetrating each other.
    /// </para>
    /// <para>
    /// This flag can only be used if the collision detection of the <see cref="Simulation"/>
    /// uses a collision filter that implements <see cref="ICollisionFilter"/>. This is the case
    /// with a new <see cref="Physics.Simulation"/> instance: By default,
    /// <c>Simulation.CollisionDomain.CollisionDetection.CollisionFilter</c> is set to a
    /// <see cref="CollisionFilter"/> instance. If the <see cref="CollisionDetection.CollisionFilter"/> 
    /// property is set to a custom filter that does not implement <see cref="ICollisionFilter"/>,
    /// then this property does nothing.
    /// </para>
    /// <para>
    /// If you change the collision filtering manually by explicitly enabling/disabling collisions
    /// between rigid body pairs in the collision filter, then <see cref="CollisionEnabled"/>
    /// should not be used (leave the default value of <see langword="true"/>). Otherwise, 
    /// the constraint might override your filter settings. To avoid conflicts, either define
    /// the collision filtering between body pairs manually using the collision filter or use the 
    /// <see cref="CollisionEnabled"/> property to disable collisions.
    /// </para>
    /// </remarks>
    public bool CollisionEnabled
    {
      get { return _collisionEnabled; }
      set
      {
        if (_collisionEnabled != value)
        {
          RemoveFromCollisionFilter();
          _collisionEnabled = value;
          AddToCollisionFilter();
          OnChanged();
        }
      }
    }
    private bool _collisionEnabled = true;


    /// <inheritdoc/>
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


    /// <inheritdoc/>
    public abstract Vector3F LinearConstraintImpulse { get; }


    /// <inheritdoc/>
    public abstract Vector3F AngularConstraintImpulse { get; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when this constraint is added to a simulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The simulation to which the constraint is added is set in the property 
    /// <see cref="Simulation"/>.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnAddToSimulation"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnAddToSimulation"/> method.
    /// </para>
    /// </remarks>
    /// <exception cref="PhysicsException">
    /// <see cref="BodyA"/> or <see cref="BodyB"/> is <see langword="null"/>. The constraint bodies 
    /// must not be <see langword="null"/> when a constraint/joint is added to a simulation.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected virtual void OnAddToSimulation()
    {
      if (BodyA == null || BodyB == null)
        throw new PhysicsException("BodyA and BodyB must not be null when a constraint/joint is added to a simulation.");

      AddToCollisionFilter();
      OnChanged();
    }


    /// <summary>
    /// Called when this constraint is removed from a simulation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The simulation from which the constraint is removed is set in the property 
    /// <see cref="Simulation"/>. After <see cref="OnRemoveFromSimulation"/> the property 
    /// <see cref="Simulation"/> will be reset to <see langword="null"/>.
    /// <para>
    /// </para>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnRemoveFromSimulation"/>
    /// in a derived class, be sure to call the base class's <see cref="OnRemoveFromSimulation"/>
    /// method.
    /// </para>
    /// </remarks>
    protected virtual void OnRemoveFromSimulation()
    {
      RemoveFromCollisionFilter();
      OnChanged();
    }


    /// <summary>
    /// Called by the simulation to prepare this constraint for constraint solving for a new time
    /// step.
    /// </summary>
    /// <exception cref="PhysicsException">
    /// <see cref="BodyA"/> or <see cref="BodyB"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public void Setup()
    {
      // Enabled is checked in the SimulationIslandManager!
      // Disabled constraints are not added to islands!
      //if (!Enabled)
      //  return;

      if (BodyA == null || BodyB == null)
        throw new PhysicsException("BodyA and BodyB must not be null.");

      OnSetup();
    }


    /// <summary>
    /// Called when constraint should be set-up for a new time step.
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="Constraint.Setup"/>, but only if the constraint is 
    /// enabled and all <see cref="Constraint"/> properties are properly initialized.
    /// </remarks>
    protected abstract void OnSetup();


    /// <summary>
    /// Called by the simulation to apply an impulse that satisfies the constraint.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a constraint larger than 
    /// <see cref="ConstraintSettings.MinConstraintImpulse"/> was applied.
    /// </returns>
    /// <remarks>
    /// This method is called by the simulation multiple times per time step. In each time step
    /// <see cref="Constraint.Setup"/> must be called once before calling this method.
    /// </remarks>
    /// <exception cref="PhysicsException">
    /// <see cref="Setup"/> was not called before <see cref="ApplyImpulse"/>.</exception>
    public bool ApplyImpulse()
    {
      // Enabled is checked in the SimulationIslandManager!
      // Disabled constraints are not added to islands!
      //if (!Enabled)
      //  return false;

      // This check is already made in Setup(). We avoid it here for performance reasons.
      //if (BodyA == null || BodyB == null)
      //  throw new PhysicsException("Setup must be called once per time step before ApplyImpulse is called.");

      return OnApplyImpulse();
    }


    /// <summary>
    /// Called when the constraint impulse should be applied.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a constraint larger than 
    /// <see cref="ConstraintSettings.MinConstraintImpulse"/> was applied.
    /// </returns>
    /// <remarks>
    /// This method is called by <see cref="Constraint.ApplyImpulse"/> to apply an impulse that
    /// satisfies the constraint. This method is only called if the constraint is enabled and if all 
    /// <see cref="Constraint"/> properties are properly initialized.
    /// </remarks>
    protected abstract bool OnApplyImpulse();


    private void AddToCollisionFilter()
    {
      if (!CollisionEnabled 
          && Enabled 
          && BodyA != null
          && BodyB != null
          && Simulation != null)
      {
        Simulation.RegisterInCollisionFilter(this);
      }
    }


    private void RemoveFromCollisionFilter()
    {
      if (!CollisionEnabled
          && Enabled
          && BodyA != null
          && BodyB != null
          && Simulation != null)
      {
        Simulation.UnregisterFromCollisionFilter(this);
      }
    }

    
    /// <summary>
    /// Called when properties of this constraint were changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      // TODO: A separate OnBodyA/BChanged would be helpful for composite joints.

      // Wake up bodies.
      if (Enabled)
      {
        if (BodyA != null)
          BodyA.WakeUp();

        if (BodyB != null)
          BodyB.WakeUp();
      }
    }
    #endregion
  }
}
