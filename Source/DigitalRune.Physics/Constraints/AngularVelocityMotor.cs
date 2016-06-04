// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a motor that controls the relative angular velocity of two constrained bodies.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The motor accelerates/decelerates both bodies until the relative angular velocity of the two
  /// bodies a round an axis (<see cref="AxisALocal"/>) fixed on the first body is equal to 
  /// <see cref="TargetVelocity"/>. 
  /// </para>
  /// </remarks>
  public class AngularVelocityMotor : Constraint
  {
    // Rotation axes fixed on body A.TargetVelocity relative to A: ωB - ωA

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector3F _minImpulseLimits;
    private Vector3F _maxImpulseLimits;
    private readonly Constraint1D[] _constraints =
    {
      new Constraint1D(), 
      new Constraint1D(), 
      new Constraint1D(),
    };
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the rotation axis that is fixed on <see cref="Constraint.BodyA"/> in local 
    /// space of <see cref="Constraint.BodyA"/>. 
    /// </summary>
    /// <value>The rotation axis in local space of <see cref="Constraint.BodyA"/>.</value>
    /// <remarks>
    /// <para>
    /// This axis is the constraint axis around which the angular velocity is controlled.
    /// This vector is automatically normalized if possible.
    /// </para>
    /// <para>
    /// If <see cref="UseSingleAxisMode"/> is <see langword="true"/> and <see cref="AxisALocal"/>
    /// is a zero vector, the motor is disabled. 
    /// If <see cref="UseSingleAxisMode"/> is <see langword="false"/> and <see cref="AxisALocal"/>
    /// is a zero vector, the motor is enabled and cancels all rotational velocities. 
    /// </para>
    /// </remarks>
    public Vector3F AxisALocal
    {
      get { return _axisALocal; }
      set
      {
        if (_axisALocal != value)
        {
          _axisALocal = value;
          if (!_axisALocal.TryNormalize())
            _axisALocal = Vector3F.Zero;
          OnChanged();
        }
      }
    }

    private Vector3F _axisALocal = Vector3F.UnitX;


    /// <summary>
    /// Gets or sets the target angular velocity around the rotation axis (<see cref="AxisALocal"/>).
    /// </summary>
    /// <value>The target angular velocity.</value>
    public float TargetVelocity
    {
      get { return _targetVelocity; }
      set
      {
        if (_targetVelocity != value)
        {
          _targetVelocity = value;
          OnChanged();
        }
      }
    }
    private float _targetVelocity;


    /// <summary>
    /// Gets or sets the softness.
    /// </summary>
    /// <value>The softness. The default value is 0.</value>
    /// <remarks>
    /// The softness parameter can be used to allow the constraint to be violated by a small amount.
    /// This has the effect that the joint appears "soft". If the value is 0 the constraint is
    /// "hard" and the simulation will try to counter all constraint violations. A small positive
    /// value (e.g. 0.001) can be used to make the constraint soft.
    /// </remarks>
    public float Softness
    {
      get { return _softness; }
      set
      {
        if (_softness != value)
        {
          _softness = value;
          OnChanged();
        }
      }
    }
    private float _softness;


    /// <summary>
    /// Gets or sets the maximal force that is applied by this motor.
    /// </summary>
    /// <value>The maximal force. The default value is +∞.</value>
    /// <remarks>
    /// This property defines the maximal force that the motor can apply.
    /// </remarks>
    public float MaxForce
    {
      get { return _maxForce; }
      set
      {
        if (_maxForce != value)
        {
          _maxForce = value;
          OnChanged();
        }
      }
    }
    private float _maxForce = float.PositiveInfinity;


    /// <inheritdoc/>
    public override Vector3F LinearConstraintImpulse
    {
      get
      {
        return Vector3F.Zero;
      }
    }


    /// <inheritdoc/>
    public override Vector3F AngularConstraintImpulse
    {
      get
      {
        return _constraints[0].ConstraintImpulse * _constraints[0].JAngB
             + _constraints[1].ConstraintImpulse * _constraints[1].JAngB
             + _constraints[2].ConstraintImpulse * _constraints[2].JAngB;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the motor applies forces only on a single axis.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if motor applies forces on a single axis; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// In single-axis-mode the motor applies forces on a single constraint axis. If 
    /// <see cref="UseSingleAxisMode"/> is <see langword="false"/>, the motor applies a force on 
    /// the same axis as in single-axis-mode but also on two orthogonal axes. In this 
    /// multiple-axes-mode the motor drives the bodies on the main constraint axis and cancels all 
    /// movements orthogonal to this axis. This multiple-axis motor is more stable but costs a bit 
    /// more performance.
    /// </remarks>
    public bool UseSingleAxisMode
    {
      // TODO: We could create a separate single axis motor.
      // In a single axis motor the user could specify MinForce and MaxForce so that the motor
      // can accelerate and decelerate with different settings. For example, the user could
      // specify that the breaking force is limit to 0, then the motor does not decelerate.

      get { return _useSingleAxisMode; }
      set
      {
        if (_useSingleAxisMode != value)
        {
          _useSingleAxisMode = value;
          OnChanged();
        }
      }
    }
    private bool _useSingleAxisMode;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnSetup()
    {
      var deltaTime = Simulation.Settings.Timing.FixedTimeStep;

      var anchorOrientationA = BodyA.Pose.Orientation;
      var axis = anchorOrientationA * AxisALocal;

      if (!UseSingleAxisMode)
      {
        // One constraint for each axis fixed on A.
        var targetVelocityVector = axis * TargetVelocity;
        SetupConstraint(0, targetVelocityVector.X, anchorOrientationA.GetColumn(0), deltaTime);
        SetupConstraint(1, targetVelocityVector.Y, anchorOrientationA.GetColumn(1), deltaTime);
        SetupConstraint(2, targetVelocityVector.Z, anchorOrientationA.GetColumn(2), deltaTime);
      }
      else
      {
        // One constraint about the velocity axis.
        if (axis.IsNumericallyZero)
        {
          // No velocity axis.
          _minImpulseLimits[0] = 0;
        }
        else
        {
          SetupConstraint(0, TargetVelocity, axis, deltaTime);
        }
      }

      // No warmstarting.
      _constraints[0].ConstraintImpulse = 0;
      _constraints[1].ConstraintImpulse = 0;
      _constraints[2].ConstraintImpulse = 0;
    }


    private void SetupConstraint(int index, float targetVelocity, Vector3F axis, float deltaTime)
    {
      Constraint1D constraint = _constraints[index];

      constraint.TargetRelativeVelocity = targetVelocity;

      // Impulse limits
      float impulseLimit = MaxForce * deltaTime;
      _minImpulseLimits[index] = -impulseLimit;
      _maxImpulseLimits[index] = impulseLimit;

      // Note: Softness must be set before!
      constraint.Softness = Softness / deltaTime;
      constraint.Prepare(BodyA, BodyB, Vector3F.Zero, -axis, Vector3F.Zero,axis);
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      if (!UseSingleAxisMode)
      {
        Vector3F impulse = new Vector3F();
        impulse.X = ApplyImpulse(0);
        impulse.Y = ApplyImpulse(1);
        impulse.Z = ApplyImpulse(2);

        return impulse.LengthSquared > Simulation.Settings.Constraints.MinConstraintImpulseSquared;
      }
      else
      {
        var impulse = ApplyImpulse(0);
        return Math.Abs(impulse) > Simulation.Settings.Constraints.MinConstraintImpulse;
      }
    }


    private float ApplyImpulse(int index)
    {
      if (_minImpulseLimits[index] != 0)
      {
        Constraint1D constraint = _constraints[index];
        float relativeVelocity = constraint.GetRelativeVelocity(BodyA, BodyB);
        float impulse = constraint.SatisfyConstraint(
          BodyA,
          BodyB,
          relativeVelocity,
          _minImpulseLimits[index],
          _maxImpulseLimits[index]);

        return impulse;
      }

      return 0;
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // Delete cached data.
      _constraints[0].ConstraintImpulse = 0;
      _constraints[1].ConstraintImpulse = 0;
      _constraints[2].ConstraintImpulse = 0;

      base.OnChanged();
    }
    #endregion
  }
}
