// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.ForceEffects;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Updates a vehicle and applies suspension, motor and friction forces.
  /// </summary>
  internal class VehicleForceEffect : ForceEffect
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the vehicle.
    /// </summary>
    /// <value>The vehicle.</value>
    public Vehicle Vehicle { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleForceEffect"/> class.
    /// </summary>
    /// <param name="vehicle">The vehicle.</param>
    internal VehicleForceEffect(Vehicle vehicle)
    {
      Debug.Assert(vehicle != null);
      Vehicle = vehicle;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when the simulation wants this force effect to apply forces to rigid bodies.
    /// </summary>
    protected override void OnApply()
    {
      RigidBody chassis = Vehicle.Chassis;
      Pose chassisPose = chassis.Pose;
      float mass = chassis.MassFrame.Mass;
      Vector3F up = chassisPose.ToWorldDirection(Vector3F.UnitY);
      float deltaTime = Simulation.Settings.Timing.FixedTimeStep;
      int numberOfWheels = Vehicle.Wheels.Count;

      for (int i = 0; i < numberOfWheels; i++)
      {
        var wheel = Vehicle.Wheels[i];

        // Update contact info.
        wheel.PreviousSuspensionLength = wheel.SuspensionLength;
        wheel.UpdateContactInfo();

        float normalDotUp = Vector3F.Dot(wheel.GroundNormal, up);
        if (!wheel.HasGroundContact || Numeric.IsLessOrEqual(normalDotUp, 0))
        {
          // -----  The simple case: The wheel is in the air.          
          // To keep it simple: The wheel continues spinning in the same direction.

          // Damp angular velocity.
          wheel.AngularVelocity *= 0.99f;

          // Update rotation angle.
          wheel.RotationAngle += wheel.AngularVelocity * deltaTime;

          // TODO: Slow down or stop spin when braking. 
          // TODO: Reverse velocity when force reverses.          
          continue;
        }


        // ----- The wheel touches the ground.

        // ----- Suspension force
        float springForce = wheel.SuspensionStiffness
                            * mass
                            * (wheel.SuspensionRestLength - wheel.SuspensionLength)
                            * normalDotUp;

        // The compression velocity:
        // (Note: Alternatively we could compute the  velocity from point velocities of chassis 
        // and the hit body.)
        float compressionVelocity = (wheel.PreviousSuspensionLength - wheel.SuspensionLength) / deltaTime;
        float damping = (compressionVelocity > 0) ? wheel.SuspensionCompressionDamping : wheel.SuspensionRelaxationDamping;
        float dampingForce = damping * mass * compressionVelocity;

        // Force acting on chassis in up direction:
        float normalForce = MathHelper.Clamp(springForce + dampingForce, 0, wheel.MaxSuspensionForce);

        // If the suspension has reached the minimum length, we must add a large force
        // that stops any further compression.
        if (wheel.SuspensionLength <= wheel.MinSuspensionLength)
        {
          // ComputeStopImpulse computes an impulse that stops any movement between the 
          // ground and the wheel in a given direction (up direction in this case).
          float force = ComputeStopImpulse(wheel, up) / deltaTime;  // force = impulse / dt

          // force can be negative if the ground and the chassis are already separating.
          // Only apply the force if is positive (= it pushes the chassis away from the ground).
          if (force > 0)
            normalForce += force;
        }

        AddForce(chassis, normalForce * up, wheel.GroundPosition);
        AddForce(wheel.TouchedBody, -normalForce * up, wheel.GroundPosition);

        // ----- Ground tangents
        Vector3F right = chassisPose.ToWorldDirection(Matrix33F.CreateRotationY(wheel.SteeringAngle) * Vector3F.UnitX);
        Vector3F groundForward = Vector3F.Cross(wheel.GroundNormal, right).Normalized;
        Vector3F groundRight = Vector3F.Cross(groundForward, wheel.GroundNormal).Normalized;

        // ----- Side force
        float sideForce = ComputeStopImpulse(wheel, groundRight) / deltaTime;  // force = impulse / dt
        // Assume that all wheels act together:
        sideForce /= numberOfWheels;

        // ----- Forward force
        float rollingFrictionForce;
        if (Math.Abs(wheel.MotorForce) > wheel.BrakeForce)
        {
          // If the motor is driving the car, assume that the friction force has the same
          // magnitude (100% friction, the whole motor force is delivered to the ground.)
          rollingFrictionForce = wheel.MotorForce - wheel.BrakeForce;
        }
        else
        {
          // The motor is off, or we are braking. 
          // Compute a friction force that would stop the car.
          rollingFrictionForce = ComputeStopImpulse(wheel, groundForward) / deltaTime;

          // Limit the friction force by the wheel.RollingFrictionForce (can be 0) or the
          // the current brake force.
          float brakeForce = wheel.BrakeForce - Math.Abs(wheel.MotorForce);
          float maxFriction = Math.Max(wheel.RollingFrictionForce, brakeForce);
          rollingFrictionForce = MathHelper.Clamp(rollingFrictionForce, -maxFriction, maxFriction);
        }

        // The current side force and rolling friction force assume perfect friction. But the
        // friction force is limited by the normal force that presses onto the ground:
        //   MaxFrictionForce = µ * NormalForce
        float maxFrictionForce = wheel.Friction * normalForce;

        // Compute combined force parallel to ground surface.
        Vector3F tangentForce = rollingFrictionForce * groundForward + sideForce * groundRight;
        float tangentForceLength = tangentForce.Length;
        if (tangentForceLength > maxFrictionForce)
        {
          // Not enough traction - that means, we are sliding!
          var skidForce = tangentForceLength - maxFrictionForce;
          var skidVelocity = skidForce * deltaTime / mass;
          wheel.SkidEnergy = maxFrictionForce * skidVelocity * deltaTime;

          // The friction forces must be scaled. 
          float factor = maxFrictionForce / tangentForceLength;
          rollingFrictionForce *= factor;
          sideForce *= factor;
        }
        else
        {
          // The force is within the friction cone. No sliding.
          wheel.SkidEnergy = 0;
        }

        // Apply rolling friction force. This drives the car.
        AddForce(chassis, rollingFrictionForce * groundForward, wheel.GroundPosition);
        AddForce(wheel.TouchedBody, -rollingFrictionForce * groundForward, wheel.GroundPosition);

        // Apply side friction force
        // If we apply the side force on the ground position, the car starts rolling (tilt) in 
        // tight curves. If we apply the force on a higher position, rolling is reduced.
        Vector3F sideForcePosition = wheel.GroundPosition + wheel.RollReduction * (wheel.SuspensionLength + wheel.Radius) * up;
        AddForce(chassis, sideForce * groundRight, sideForcePosition);
        AddForce(wheel.TouchedBody, -sideForce * groundRight, sideForcePosition);

        // ----- Update AngularVelocity and Rotation.
        // We set the angular velocity, so that the wheel matches the moving underground.
        Vector3F relativeContactVelocity = chassis.GetVelocityOfWorldPoint(wheel.GroundPosition)
                                           - wheel.TouchedBody.GetVelocityOfWorldPoint(wheel.GroundPosition);
        float forwardVelocity = Vector3F.Dot(relativeContactVelocity, groundForward);
        wheel.AngularVelocity = forwardVelocity / wheel.Radius;
        wheel.RotationAngle += wheel.AngularVelocity * deltaTime;

        // TODO: Use a more realistic AngularVelocity!
        // - Apply the skid energy to show sliding wheels. 
        // - Set AngularVelocity to 0 when brakes are active - for more dramatic effect.
      }
    }


    // Computes an impulse that would stop motion in the given direction.
    private float ComputeStopImpulse(Wheel wheel, Vector3F direction)
    {
      // A = chassis, B = ground
      var bodyA = Vehicle.Chassis;
      var bodyB = wheel.TouchedBody;

      // This method computes a constraint impulse that makes the relative velocity of the touching
      // points 0 in the given direction. - A 1D no-movement constraint.
      // If you want to learn more about this, there are a few literature references in the
      // DigitalRune Physics Documentation (section "Best Practices and Recommended Literature").

      // Radius vectors.
      var rA = wheel.GroundPosition - bodyA.PoseCenterOfMass.Position;
      var rB = wheel.GroundPosition - bodyB.PoseCenterOfMass.Position;

      // Jacobians.
      var jLinA = -direction;
      var jAngA = -Vector3F.Cross(rA, direction);
      var jLinB = direction;
      var jAngB = Vector3F.Cross(rB, direction);

      // M^-1 * J^T
      var WJTLinA = bodyA.MassInverse * jLinA;
      var WJTAngA = bodyA.InertiaInverseWorld * jAngA;
      var WJTLinB = bodyB.MassInverse * jLinB;
      var WJTAngB = bodyB.InertiaInverseWorld * jAngB;

      // J * M^-1 * J^T
      float JWJT = Vector3F.Dot(jLinA, WJTLinA) + Vector3F.Dot(jAngA, WJTAngA)
                   + Vector3F.Dot(jLinB, WJTLinB) + Vector3F.Dot(jAngB, WJTAngB);
      var JWJTInverse = 1 / JWJT;

      // Relative velocity = J * v
      var relativeVelocity = Vector3F.Dot(jLinA, bodyA.LinearVelocity)
                             + Vector3F.Dot(jAngA, bodyA.AngularVelocity)
                             + Vector3F.Dot(jLinB, bodyB.LinearVelocity)
                             + Vector3F.Dot(jAngB, bodyB.AngularVelocity);

      // The impulse (lambda) is (J * M^-1 * J^T)^-1 * (newRelativeVelocity - oldRelativeVelocity).
      return JWJTInverse * relativeVelocity;
    }
    #endregion
  }
}
