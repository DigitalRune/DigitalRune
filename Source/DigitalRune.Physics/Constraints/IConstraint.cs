// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint between two bodies.
  /// </summary>
  /// <remarks>
  /// A constraint limits the movement of two bodies relative two each other. It restricts the
  /// degrees of movement of one body relative to the other body.
  /// </remarks>
  public interface IConstraint
  {
    /// <summary>
    /// Gets the first body.
    /// </summary>
    /// <value>The first body.</value>
    RigidBody BodyA { get; }


    /// <summary>
    /// Gets the second body.
    /// </summary>
    /// <value>The second body.</value>
    RigidBody BodyB { get; }


    /// <summary>
    /// Gets a value indicating whether this constraint is enabled.
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
    bool Enabled { get; }


    /// <summary>
    /// Gets a value indicating whether collisions between <see cref="BodyA"/> and 
    /// <see cref="BodyB"/> are disabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if collisions are enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// This property can be set to <see langword="false"/> to disable collision detection between 
    /// the constraint bodies. Disabling collisions improves performance and is often necessary,
    /// for example, for two connected limbs in a ragdoll that are always penetrating each other.
    /// </remarks>
    bool CollisionEnabled { get; }


    /// <summary>
    /// Gets the simulation to which this constraint belongs.
    /// </summary>
    /// <value>
    /// The simulation. The default value is <see langword="null"/>. This property is automatically
    /// set when the constraint is added to a simulation.
    /// </value>
    Simulation Simulation { get; }


    /// <summary>
    /// Gets or sets the linear constraint impulse that was applied. 
    /// </summary>
    /// <value>The linear constraint impulse in world space.</value>
    /// <remarks>
    /// <para>
    /// This impulse was applied in the constraint anchor on <see cref="Constraint.BodyB"/>. An
    /// equivalent negative impulse was applied on <see cref="Constraint.BodyA"/>.
    /// </para>
    /// <para>
    /// The constraint might also have applied an angular constraint impulse, see 
    /// <see cref="AngularConstraintImpulse"/>.
    /// </para>
    /// </remarks>
    Vector3F LinearConstraintImpulse { get; }


    /// <summary>
    /// Gets or sets the angular constraint impulse that was applied.
    /// </summary>
    /// <value>The angular constraint impulse in world space.</value>
    /// <remarks>
    /// <para>
    /// This impulse was applied at the center of mass of <see cref="Constraint.BodyB"/>. An
    /// equivalent negative impulse was applied at the center of mass of 
    /// <see cref="Constraint.BodyA"/>.
    /// </para>
    /// <para>
    /// The constraint might also have applied a linear constraint impulse, see 
    /// <see cref="LinearConstraintImpulse"/>.
    /// </para>
    /// </remarks>
    Vector3F AngularConstraintImpulse { get; }


    /// <summary>
    /// Called by the simulation to prepare this constraint for constraint solving for a new time
    /// step.
    /// </summary>
    void Setup();


    /// <summary>
    /// Called by the simulation to apply an impulse that satisfies the constraint.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a constraint impulse larger than 
    /// <see cref="ConstraintSettings.MinConstraintImpulse"/> was applied.
    /// </returns>
    /// <remarks>
    /// This method is called by the simulation multiple times per time step. In each time step
    /// <see cref="Setup"/> must be called once before calling this method.
    /// </remarks>
    bool ApplyImpulse();
  }
}
