// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Applies a damping force (viscous drag).
  /// </summary>
  /// <remarks>
  /// The effect applies a force that is proportional to the current velocity of a rigid body. The
  /// force will slow the bodies movement.
  /// </remarks>
  public class Damping : ForceField
  {
    // Notes:
    // We use a simplified damping that is adequate for games:
    //    vDamped = (1 - c * dt) * v
    //
    // Derivation (Source: Box2D, b2Island.cpp):
    // -----------------------------------------
    // ODE:       dv/dt + c * v = 0
    // Solution:  v(t) = v0 * exp(-c * t)
    // Time step: v(t + dt) = v0 * exp(-c * (t + dt)) = v0 * exp(-c * t) * exp(-c * dt) = v * exp(-c * dt)
    //            v2 = exp(-c * dt) * v1
    // Taylor expansion: v2 = (1 - c * dt) * v1
    //
    // 
    // If the damping constant should be interpreted as "the proportion of velocity lost per 
    // second", then following formula gives better results:
    //    vDamped = pow(1 - c, dt) * v
    // (Source: http://code.google.com/p/bullet/issues/detail?id=74)
    // When timeStep is 1 second, the correct damping is applied.
    // When timestep is 0.5 second, two calls of the function will result in
    //    vDamped = pow(1 - c, 0.5) * pow(1 - c, 0.5) * v
    // which is equivalent to: 
    //    velocity *= 1-damping
    // 
    // Alternatively, we could compute a force and let the force do the damping.
    

    /// <summary>
    /// The linear damping coefficient.
    /// </summary>
    /// <value>The linear damping. The default value is <c>0.02</c>.</value>
    /// <remarks>
    /// The higher this constant is, the higher the damping force will be that damps linear
    /// movement.
    /// </remarks>
    public float LinearDamping { get; set; }


    /// <summary>
    /// The angular damping coefficient.
    /// </summary>
    /// <value>The angular damping. The default value is <c>0.2</c>.</value>
    /// <remarks>
    /// The higher this constant is, the higher the damping force will be that damps rotational
    /// movement.
    /// </remarks>
    public float AngularDamping { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Damping"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Damping"/> class.
    /// </summary>
    /// <remarks>
    /// The property <see cref="ForceField.AreaOfEffect"/> is initialized with a new instance of
    /// <see cref="GlobalAreaOfEffect"/>.
    /// </remarks>
    public Damping()
    {
      Initialize();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Damping"/> class.
    /// </summary>
    /// <param name="areaOfEffect">The area of effect.</param>
    public Damping(IAreaOfEffect areaOfEffect)
      : base(areaOfEffect)
    {
      Initialize();
    }


    private void Initialize()
    {
      LinearDamping = 0.02f;
      AngularDamping = 0.2f;
    }


    /// <inheritdoc/>
    public override void Apply(RigidBody body)
    {
      if (body == null)
        throw new ArgumentNullException("body", "Rigid body in area of effect must not be null.");

      if (body.IsSleeping)
        return;

      float fixedTimeStep = Simulation.Settings.Timing.FixedTimeStep;

      // Using: vDamped = (1 - c * dt) * v
      body.LinearVelocity = (1 - LinearDamping * fixedTimeStep) * body.LinearVelocity;
      body.AngularVelocity = (1 - AngularDamping * fixedTimeStep) * body.AngularVelocity;

      // Using: vDamped = pow(1.0 - c, dt) * v
      //body.LinearVelocity = (float)Math.Pow(1 - LinearDamping, fixedTimeStep) * body.LinearVelocity;
      //body.AngularVelocity = (float)Math.Pow(1 - AngularDamping, fixedTimeStep) * body.AngularVelocity;

      // Using a damping force:
      //AddForce(body, -LinearDamping * body.LinearVelocity);
      //AddTorque(body, -AngularDamping * body.AngularVelocity);
    }
  }
}
