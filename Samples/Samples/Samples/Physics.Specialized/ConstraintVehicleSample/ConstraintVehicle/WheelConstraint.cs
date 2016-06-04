// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Defines a complex constraints which models a wheel including suspension spring, a hard 
  /// suspension limit, sideways friction, forward motor forces.
  /// </summary>
  internal class WheelConstraint : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly ConstraintWheel _wheel;

    // A constraint modeling a damped spring for the suspension.
    private readonly Constraint1D _suspensionSpringConstraint;

    // A constraint modeling a hard suspension limit. This constraint
    // is active when the suspension is compressed below its min length.
    private readonly Constraint1D _suspensionLimitConstraint;
    private bool _suspensionLimitIsActive;

    // A lateral friction constraint which avoids any sliding orthogonal
    // to the forward direction.
    private readonly Constraint1D _sideConstraint;

    // A constraint which controls the forward movement. If the wheel is
    // rotated by the motor, this constraint applies a forward velocity.
    // If the wheel is braking, this constraint acts like a friction constraint
    // which stops the car.
    private readonly Constraint1D _forwardConstraint;

    // The max impulse which may be applied by the forward constraint. This
    // value is determined by the motor force, the braking force and the
    // rolling friction force.
    private float _forwardImpulseLimit;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    public override Vector3F LinearConstraintImpulse
    {
      get
      {
        return _suspensionSpringConstraint.ConstraintImpulse * _suspensionSpringConstraint.JLinB
               + _suspensionLimitConstraint.ConstraintImpulse * _suspensionLimitConstraint.JLinB
               + _sideConstraint.ConstraintImpulse * _sideConstraint.JLinB
               + _forwardConstraint.ConstraintImpulse * _forwardConstraint.JLinB;
      }
    }


    public override Vector3F AngularConstraintImpulse
    {
      get { return Vector3F.Zero; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public WheelConstraint(ConstraintWheel wheel)
    {
      _wheel = wheel;

      _suspensionSpringConstraint = new Constraint1D();
      _suspensionLimitConstraint = new Constraint1D();
      _sideConstraint = new Constraint1D();
      _forwardConstraint = new Constraint1D();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when constraint should be set up for a new time step.
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="Constraint.Setup"/>, but only if the constraint is 
    /// enabled and all <see cref="Constraint"/> properties are properly initialized.
    /// </remarks>
    protected override void OnSetup()
    {
      Debug.Assert(_wheel.HasGroundContact, "WheelConstraint must be disabled if wheel does not touch ground.");

      // The pose where the suspension is fixed on the car.
      Pose anchorPoseA = BodyA.Pose * new Pose(_wheel.Offset);

      // The ground contact position relative to the anchor pose.
      Vector3F relativePosition = anchorPoseA.ToLocalPosition(_wheel.GroundPosition);

      // Radius vectors from the centers of mass to the points where the 
      // suspension forces should be applied:
      Vector3F rA = anchorPoseA.Position - BodyA.PoseCenterOfMass.Position;
      Vector3F rB = _wheel.GroundPosition - BodyB.PoseCenterOfMass.Position;

      // Get some simulation settings.
      float deltaTime = Simulation.Settings.Timing.FixedTimeStep;
      float allowedDeviation = Simulation.Settings.Constraints.AllowedPenetration;
      float maxErrorCorrectionVelocity = Simulation.Settings.Constraints.MaxErrorCorrectionVelocity;

      // The position of ground contact relative to the suspension anchor pose.
      float relativePositionY = relativePosition.Y;

      // The suspension axis (= the Y axis of the chassis body).
      var suspensionAxis = BodyA.Pose.Orientation.GetColumn(1);

      {
        // ----- Set up the suspension spring constraint:
        float deviation = -_wheel.SuspensionRestLength - _wheel.Radius - relativePositionY + allowedDeviation;

        // Compute error reduction and softness from spring and damping values. 
        // Note: The wheel suspension stiffness and damping are scaled by the mass because they
        // should be scaled with different chassis masses.
        var mass = BodyA.MassFrame.Mass;
        var suspensionSpring = _wheel.SuspensionStiffness * mass;
        var suspensionDamping = _wheel.SuspensionDamping * mass;
        float errorReduction = ConstraintHelper.ComputeErrorReduction(deltaTime, suspensionSpring, suspensionDamping);
        float softness = ConstraintHelper.ComputeSoftness(deltaTime, suspensionSpring, suspensionDamping);

        _suspensionSpringConstraint.TargetRelativeVelocity = MathHelper.Clamp(deviation * errorReduction / deltaTime, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);
        _suspensionSpringConstraint.Softness = softness / deltaTime;
        _suspensionSpringConstraint.Prepare(BodyA, BodyB, -suspensionAxis, -Vector3F.Cross(rA, suspensionAxis), suspensionAxis, Vector3F.Cross(rB, suspensionAxis));
      }

      // ----- Set up the hard suspension limit:
      _suspensionLimitIsActive = relativePositionY > -_wheel.MinSuspensionLength - _wheel.Radius;
      if (_suspensionLimitIsActive)
      {
        float deviation = -_wheel.MinSuspensionLength - _wheel.Radius - relativePositionY + allowedDeviation;

        // This constraint is as "hard" as a normal contact constraint.
        float errorReduction = Simulation.Settings.Constraints.ContactErrorReduction;

        _suspensionLimitConstraint.TargetRelativeVelocity = MathHelper.Clamp(deviation * errorReduction / deltaTime, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);
        _suspensionLimitConstraint.Prepare(BodyA, BodyB, -suspensionAxis, -Vector3F.Cross(rA, suspensionAxis), suspensionAxis, Vector3F.Cross(rB, suspensionAxis));
      }
      else
      {
        _suspensionLimitConstraint.ConstraintImpulse = 0;
      }

      // The forward and side constraints are applied in the ground plane. But to make the 
      // car more stable we can optionally apply the impulses at a higher position (to avoid rolling).
      rA = rA - (1 - _wheel.RollReduction) * (_wheel.SuspensionLength + _wheel.Radius) * suspensionAxis;

      // ---- Set up lateral friction constraint:
      _sideConstraint.TargetRelativeVelocity = 0;
      _sideConstraint.Prepare(BodyA, BodyB, -_wheel.GroundRight, -Vector3F.Cross(rA, _wheel.GroundRight), _wheel.GroundRight, Vector3F.Cross(rB, _wheel.GroundRight));

      // ----- Set up forward constraint (motor, brake, friction):
      _forwardConstraint.Prepare(BodyA, BodyB, -_wheel.GroundForward, -Vector3F.Cross(rA, _wheel.GroundForward), _wheel.GroundForward, Vector3F.Cross(rB, _wheel.GroundForward));

      if (Math.Abs(_wheel.MotorForce) > Math.Abs(_wheel.BrakeForce))
      {
        // The wheel is driven by the motor.
        // The constraint tries to accelerate the car as much as possible. The actual acceleration
        // is limited by the motor and brake force.
        _forwardConstraint.TargetRelativeVelocity = -Math.Sign(_wheel.MotorForce) * float.PositiveInfinity;
        _forwardImpulseLimit = (Math.Abs(_wheel.MotorForce) - Math.Abs(_wheel.BrakeForce)) * deltaTime;
      }
      else
      {
        // The wheel is freely rolling or braking.
        _forwardConstraint.TargetRelativeVelocity = 0;
        _forwardImpulseLimit = Math.Max(_wheel.RollingFrictionForce, _wheel.BrakeForce - Math.Abs(_wheel.MotorForce)) * deltaTime;
      }

      // Warmstart the suspension constraints.
      _suspensionSpringConstraint.ApplyImpulse(BodyA, BodyB, _suspensionSpringConstraint.ConstraintImpulse);
      _suspensionLimitConstraint.ApplyImpulse(BodyA, BodyB, _suspensionLimitConstraint.ConstraintImpulse);

      // Do not warmstart the tangential friction-based constraints.
      _forwardConstraint.ConstraintImpulse = 0;
      _sideConstraint.ConstraintImpulse = 0;
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
    protected override bool OnApplyImpulse()
    {
      if (_wheel.HasGroundContact)
      {
        float deltaTime = Simulation.Settings.Timing.FixedTimeStep;

        // Spring constraint
        float relativeVelocityUp = _suspensionSpringConstraint.GetRelativeVelocity(BodyA, BodyB);
        float upImpulse = _suspensionSpringConstraint.SatisfyConstraint(BodyA, BodyB, relativeVelocityUp, -_wheel.MaxSuspensionForce * deltaTime, 0);

        // Hard suspension limit
        if (_suspensionLimitIsActive)
          upImpulse += _suspensionLimitConstraint.SatisfyConstraint(BodyA, BodyB, relativeVelocityUp, float.NegativeInfinity, 0);

        // Side and forward constraints
        // These are more complex and they are handled here inline because the 
        // total constraint impulse in the ground plane is limited by suspension
        // impulse which pushes into the ground plane.
        var relativeVelocityRight = _sideConstraint.GetRelativeVelocity(BodyA, BodyB);
        float sideImpulse = _sideConstraint.JWJTInverse * (-relativeVelocityRight);
        float oldTotalSideImpulse = _sideConstraint.ConstraintImpulse;
        float newTotalSideImpulse = oldTotalSideImpulse + sideImpulse;

        var relativeVelocityForward = _forwardConstraint.GetRelativeVelocity(BodyA, BodyB);
        float forwardImpulse = _forwardConstraint.JWJTInverse * (_forwardConstraint.TargetRelativeVelocity - relativeVelocityForward);
        float oldTotalForwardImpulse = _forwardConstraint.ConstraintImpulse;
        float newTotalForwardImpulse = oldTotalForwardImpulse + forwardImpulse;
        newTotalForwardImpulse = MathHelper.Clamp(newTotalForwardImpulse, -_forwardImpulseLimit, _forwardImpulseLimit);

        // The friction force is limited by the normal force that presses onto the ground:
        //   MaxFrictionForce = µ * NormalForce
        Vector3F suspensionAxis = _suspensionSpringConstraint.JLinB;
        float normalImpulse = _suspensionSpringConstraint.ConstraintImpulse
                              + _suspensionLimitConstraint.ConstraintImpulse * Vector3F.Dot(_wheel.GroundNormal, suspensionAxis);

        // Limit normal impulse. This can help, for example, after a jump when the
        // car hits the ground and creates a very high normal impulse. This could make
        // the front wheels sticky while the back wheels break out. Limiting the normal
        // impulse lets all wheels of the car slide.
        if (normalImpulse > _wheel.MaxSuspensionForce * deltaTime)
          normalImpulse = _wheel.MaxSuspensionForce * deltaTime;

        float maxFrictionImpulse = _wheel.Friction * Math.Abs(normalImpulse);

        // Compute combined force parallel to ground surface.
        Vector3F tangentImpulse = newTotalForwardImpulse * _wheel.GroundForward + newTotalSideImpulse * _wheel.GroundRight;
        float tangentImpulseLength = tangentImpulse.Length;
        if (tangentImpulseLength > maxFrictionImpulse)
        {
          // Not enough traction - that means, we are sliding!
          var skidImpulse = tangentImpulseLength - maxFrictionImpulse;
          var skidVelocity = skidImpulse / BodyA.MassFrame.Mass;
          _wheel.SkidEnergy = maxFrictionImpulse * skidVelocity * deltaTime;

          // The friction forces must be scaled. 
          float factor = maxFrictionImpulse / tangentImpulseLength;
          newTotalForwardImpulse *= factor;
          newTotalSideImpulse *= factor;
        }
        else
        {
          // The force is within the friction cone. No sliding.
          _wheel.SkidEnergy = 0;
        }

        _sideConstraint.ConstraintImpulse = newTotalSideImpulse;
        sideImpulse = newTotalSideImpulse - oldTotalSideImpulse;
        _sideConstraint.ApplyImpulse(BodyA, BodyB, sideImpulse);

        _forwardConstraint.ConstraintImpulse = newTotalForwardImpulse;
        forwardImpulse = newTotalForwardImpulse - oldTotalForwardImpulse;
        _forwardConstraint.ApplyImpulse(BodyA, BodyB, forwardImpulse);

        // Constraint solver iterations must continue as long as a significant
        // impulse is applied.
        var minConstraintImpulse = Simulation.Settings.Constraints.MinConstraintImpulse;
        return Math.Abs(upImpulse) > minConstraintImpulse
               || Math.Abs(forwardImpulse) > minConstraintImpulse
               || Math.Abs(sideImpulse) > minConstraintImpulse;
      }

      return false;
    }


    /// <summary>
    /// Called when properties of this constraint were changed.
    /// </summary>
    protected override void OnChanged()
    {
      // Reset the stored impulses (to disable warmstarting) when something significant 
      // was changed.
      _suspensionSpringConstraint.ConstraintImpulse = 0;
      _suspensionLimitConstraint.ConstraintImpulse = 0;
      _forwardConstraint.ConstraintImpulse = 0;
      _sideConstraint.ConstraintImpulse = 0;

      base.OnChanged();
    }
    #endregion
  }
}
