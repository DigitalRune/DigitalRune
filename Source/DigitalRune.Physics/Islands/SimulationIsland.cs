// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Describes a collection of rigid bodies and constraints that can be simulated independently.
  /// </summary>
  /// <remarks>
  /// <para>
  /// In general, it is not needed to deal with simulation islands directly and users can safely
  /// ignore this simulation detail.
  /// </para>
  /// <para>
  /// In each frame the simulation creates different batches of bodies and constraints that can
  /// be simulated independently. Such a batch is called simulation island. If rigid bodies can
  /// influence each other via constraints (contacts or joints), they are sorted into the same 
  /// island.
  /// </para>
  /// <para>
  /// <strong>Sleeping:</strong> Islands can only sleep as a whole. It is not possible that some
  /// bodies in an island are sleeping and others are awake. If one object is awake all objects are
  /// awake because the movement of the awake body can propagate to the other bodies. In 
  /// unfortunate configurations a jittering body can keep a whole island awake. 
  /// </para>
  /// <para>
  /// <strong>Static and Kinematic Bodies:</strong> <see cref="MotionType.Static"/> or 
  /// <see cref="MotionType.Kinematic"/> rigid bodies are not managed in islands. They are
  /// not part of another island and they are not in their own island. Islands do only contain
  /// <see cref="MotionType.Dynamic"/> rigid bodies. (But constraints between dynamic and 
  /// non-dynamic bodies are managed in the islands.)
  /// </para>
  /// </remarks>
  public sealed class SimulationIsland
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<SimulationIsland> Pool =
      new ResourcePool<SimulationIsland>(
        () => new SimulationIsland(),
        null,
        null);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the simulation.
    /// </summary>
    /// <value>The simulation.</value>
    internal Simulation Simulation { get; set; }


    /// <summary>
    /// Gets the constraints of this island.
    /// </summary>
    /// <value>The constraints of this island.</value>
    public ReadOnlyCollection<Constraint> Constraints
    {
      get
      {
        if (_constraints == null)
          _constraints = new ReadOnlyCollection<Constraint>(ConstraintsInternal);

        return _constraints;
      }
    }
    private ReadOnlyCollection<Constraint> _constraints;


    /// <summary>
    /// Gets the contact constraints of this island.
    /// </summary>
    /// <value>The contacts constraints of this island.</value>
    public ReadOnlyCollection<ContactConstraint> ContactConstraints
    {
      get
      {
        if (_contactConstraints == null)
          _contactConstraints = new ReadOnlyCollection<ContactConstraint>(ContactConstraintsInternal);
        
        return _contactConstraints;
      }
    }
    private ReadOnlyCollection<ContactConstraint> _contactConstraints;


    /// <summary>
    /// Gets the rigid bodies of this island.
    /// </summary>
    /// <value>The rigid bodies of this island.</value>
    public ReadOnlyCollection<RigidBody> RigidBodies
    {
      get
      {
        if (_rigidBodies == null)
          _rigidBodies = new ReadOnlyCollection<RigidBody>(RigidBodiesInternal);

        return _rigidBodies;
      }
    }
    private ReadOnlyCollection<RigidBody> _rigidBodies;


    /// <summary>
    /// Gets the constraints of this island. (For internal use only.)
    /// </summary>
    /// <value>The constraints.</value>
    internal List<Constraint> ConstraintsInternal { get; private set; }


    /// <summary>
    /// Gets the contact constraints of this island. (For internal use only.)
    /// </summary>
    /// <value>The contact constraints.</value>
    internal List<ContactConstraint> ContactConstraintsInternal { get; private set; }


    /// <summary>
    /// Gets the rigid bodies of this island. (For internal use only.)
    /// </summary>
    /// <value>The rigid bodies.</value>
    internal List<RigidBody> RigidBodiesInternal { get; private set; }


    /// <summary>
    /// Gets or sets the random generator for constraint randomization.
    /// </summary>
    /// <value>The random generator for constraint randomization.</value>
    internal Random Random { get; set;}
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="SimulationIsland"/> class from being created.
    /// </summary>
    private SimulationIsland()
    {
      ConstraintsInternal = new List<Constraint>();
      ContactConstraintsInternal = new List<ContactConstraint>();
      RigidBodiesInternal = new List<RigidBody>();
    }


    /// <summary>
    /// Creates an instance of the <see cref="SimulationIsland"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="SimulationIsland"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    internal static SimulationIsland Create()
    {
      return Pool.Obtain();
    }


    /// <summary>
    /// Recycles this instance of the <see cref="SimulationIsland"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    internal void Recycle()
    {
      Clear();
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this simulation island.
    /// </summary>
    internal void Clear()
    {
      Simulation = null;
      ConstraintsInternal.Clear();
      ContactConstraintsInternal.Clear();
      RigidBodiesInternal.Clear();
    }


    /// <summary>
    /// Controls island sleeping and returns whether this island is sleeping.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if all bodies are sleeping; otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    internal bool IsSleeping()
    {
      // Assume that all bodies are sleeping.
      bool allAreSleeping = true;

      // Check if the island touches a static or kinematic non-moving body..
      bool touchesImmovable = false;

      // Kinematic objects are in their own island but must wake up connected islands.
      // Check for contacts with kinematic objects.
      int numberOfContactConstraints = ContactConstraintsInternal.Count;
      for (int i = 0; i < numberOfContactConstraints; i++)
      {
        var contact = ContactConstraintsInternal[i];
        var bodyA = contact.BodyA;
        var bodyB = contact.BodyB;

        // Set flag if static or resting kinematic body is touched.
        // The "break" below in this loop can exit early, so that an immovable body is not 
        // detected - but "break" comes when the island sleeping should be deferred; so this is no
        // real problem.
        if (bodyA.MotionType != MotionType.Dynamic && bodyA.IsSleeping
            || bodyB.MotionType != MotionType.Dynamic && bodyB.IsSleeping)
          touchesImmovable = true;

        if (bodyA.MotionType == MotionType.Kinematic
            && !bodyA.IsSleepingCandidate)
        {
          // Body is kinematic and not sleeping. Touched islands cannot sleep!
          if (bodyB.MotionType == MotionType.Dynamic)
          {
            allAreSleeping = false;
            break;
          }
        }
        else if (bodyB.MotionType == MotionType.Kinematic
                 && !bodyB.IsSleepingCandidate)
        {
          // Same as above but with other body.
          if (bodyA.MotionType == MotionType.Dynamic)
          {
            allAreSleeping = false;
            break;
          }
        }
      }

      // Check for constraints with kinematic objects - same as for above.
      int numberOfConstraints = ConstraintsInternal.Count;
      for (int i = 0; i < numberOfConstraints; i++)
      {
        var constraint = ConstraintsInternal[i];
        //if (!constraint.Enabled)   // Enabled is already checked in the simulation island manager.
        //  continue;

        var bodyA = constraint.BodyA;
        var bodyB = constraint.BodyB;

        if (bodyA != null
            && bodyA.MotionType == MotionType.Kinematic
            && !bodyA.IsSleepingCandidate)
        {
          // Body is kinematic and not sleeping. Touched islands cannot sleep!
          if (bodyB != null && bodyB.MotionType == MotionType.Dynamic)
          {
            allAreSleeping = false;
            break;
          }
        }
        else if (bodyB != null
                 && bodyB.MotionType == MotionType.Kinematic
                 && !bodyB.IsSleepingCandidate)
        {
          // Same as above but with other body.
          if (bodyA != null && bodyA.MotionType == MotionType.Dynamic)
          {
            allAreSleeping = false;
            break;
          }
        }
      }

      // Check if all bodies in the island are sleeping.
      int numberOfRigidBodies = RigidBodiesInternal.Count;
      if (allAreSleeping)
      {
        for (int i = 0; i < numberOfRigidBodies; i++)
        {
          var body = RigidBodiesInternal[i];
          if (!body.IsSleepingCandidate)
          {
            allAreSleeping = false;
            break;
          }
        }
      }

      if (RigidBodiesInternal[0].IslandTouchesImmovable && !touchesImmovable)
      {
        // If the island was touching an unmovable body and the body is gone, 
        // wake the whole island. Probably a support has disappeared and the island will
        // fall due to gravity. This is easy to test in a 20 sphere stack. Shooting away
        // one of the lower bodies, will not wake the upper bodies long enough to start falling.
        // Therefore this check is needed.
        for (int i = 0; i < numberOfRigidBodies; i++)
        {
          var body = RigidBodiesInternal[i];
          body.WakeUp();
        }
      }
      else if (!allAreSleeping)
      {
        // If one body is awake, all bodies must be awake - at least for the next time step.
        for (int i = 0; i < numberOfRigidBodies; i++)
        {
          var body = RigidBodiesInternal[i];
          // The body must be awake - at least in this time step.
          if (body.IsSleeping)
            body.DeferSleep(Simulation.Settings.Timing.FixedTimeStep * 1);
        }
      }

      // Store touchesImmovable flag in rigid bodies.
      for (int i = 0; i < numberOfRigidBodies; i++)
      {
        var body = RigidBodiesInternal[i];
        body.IslandTouchesImmovable = touchesImmovable;
      }

      return allAreSleeping;
    }
    #endregion
  }
}
