// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Defines whether collision response between rigid bodies is enabled or disabled.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Collision response means that two bodies will not penetrate each other and bounce off of each
  /// other. This filter does not define whether collision detection is enabled or not. To disable
  /// collision detection between rigid bodies use the 
  /// <see cref="CollisionDetection.CollisionFilter"/> of the collision detection. (The 
  /// <see cref="CollisionDomain.CollisionDetection"/> is a property of the 
  /// <see cref="Simulation.CollisionDomain"/> which is a property of the 
  /// <see cref="Simulation"/>.) If the <see cref="CollisionResponseFilter"/> returns 
  /// <see langword="false"/> for a pair of rigid bodies, the simulation will not create
  /// <see cref="ContactConstraint"/>s and the bodies will be able to move through each other.
  /// </para>
  /// <para>
  /// Per default, collision response is enabled for all rigid bodies. Collision response can be 
  /// disabled for pairs of <see cref="RigidBody"/>s.
  /// </para>
  /// </remarks>
  public class CollisionResponseFilter : IPairFilter<RigidBody>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Pairwise filtering. If object pair is contained, filtering is disabled.
    private readonly HashSet<Pair<RigidBody>> _disabledPairs = new HashSet<Pair<RigidBody>>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Occurs when the filter rules have changed.
    /// </summary>
    public event EventHandler<EventArgs> Changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this filter. Collision response will be enabled for all pairs of rigid bodies.
    /// </summary>
    public void Reset()
    {
      _disabledPairs.Clear();
      OnChanged(EventArgs.Empty);
    }


    /// <summary>
    /// Enables or disables collision response between the given rigid bodies.
    /// </summary>
    /// <param name="bodyA">The first rigid bodies.</param>
    /// <param name="bodyB">The second rigid bodies.</param>
    /// <param name="responseEnabled">
    /// If set to <see langword="true"/> the collision response between <paramref name="bodyA"/> and 
    /// <paramref name="bodyB"/> is enabled. Use <see langword="false"/> to disable the collision
    /// response.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="bodyA"/> or <paramref name="bodyB"/> is <see langword="null"/>.
    /// </exception>
    public void Set(RigidBody bodyA, RigidBody bodyB, bool responseEnabled)
    {
      if (bodyA == null)
        throw new ArgumentNullException("bodyA");
      if (bodyB == null)
        throw new ArgumentNullException("bodyB");

      var pair = new Pair<RigidBody>(bodyA, bodyB);

      if (responseEnabled)
        _disabledPairs.Remove(pair);
      else
        _disabledPairs.Add(pair);

      OnChanged(EventArgs.Empty);
    }


    /// <summary>
    /// Returns <see langword="true"/> if collision response is enabled for the given pair.
    /// </summary>
    /// <param name="pair">The pair of rigid bodies.</param>
    /// <returns>
    /// <see langword="true"/> if collision response is enabled; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Filter(Pair<RigidBody> pair)
    {
      return !_disabledPairs.Contains(pair);
    }


    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
