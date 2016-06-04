// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint that restricts rotational movement. This constraint is configurable
  /// to create custom joints.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This constraint computes the relative rotation between <see cref="AnchorOrientationALocal"/>
  /// fixed on the first body and <see cref="AnchorOrientationBLocal"/> fixed on the second body.
  /// It computes the three Euler angles for the relative orientation (see 
  /// <see cref="ConstraintHelper.GetEulerAngles"/>).
  /// </para>
  /// <para>
  /// <strong>Euler Angles:</strong>
  /// The Euler angles are computed for following order of rotations: The first rotation
  /// is about the x-axis. The second rotation is about the rotated y-axis after the first 
  /// rotation. The last rotation is about the final z-axis.
  /// </para>
  /// <para>
  /// The Euler angles are unique if the second angle is less than +/- 90°. The limits for the
  /// rotation angles are [-180°, 180°] for the first and the third angle. And the limit for the
  /// second angle is ]-90°, 90°[. Important: For <see cref="Minimum"/> and <see cref="Maximum"/> 
  /// these angles must be specified in radians not degrees.
  /// </para>
  /// <para>
  /// Each rotation of the three Euler angle rotations can be restricted using <see cref="Minimum"/>
  /// and <see cref="Maximum"/>. Important: The second rotation must be fixed in the range 
  /// ]-90°, 90°[ (otherwise a Gimbal Lock situation occurs).
  /// </para>
  /// <para>
  /// This constraint can be used to create custom constraints. For example using a minimum and
  /// maximum limits of (0, 0, 0) creates a <see cref="NoRotationConstraint"/>. 
  /// Combining the <see cref="AngularLimit"/> with a <see cref="BallJoint"/> and using a minimum 
  /// and maximum of (-∞, 0, 0) and (+∞, 0, 0) creates a <see cref="HingeJoint"/>. Combining
  /// the <see cref="AngularLimit"/> with a <see cref="BallJoint"/> and using a minimum and maximum 
  /// of (-π/4, 0, -∞) and (+π/4, 0, +∞) creates a <see cref="Hinge2Joint"/> where the first
  /// rotation axis is the limited steering axis and the third axis is the rolling axis.
  /// </para>
  /// </remarks>
  public class AngularLimit : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LimitState[] _limitStates = new LimitState[3];
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
    /// Gets or sets the constraint anchor orientation on <see cref="Constraint.BodyA"/> in local 
    /// space of <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor orientation on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>.
    /// </value>
    public Matrix33F AnchorOrientationALocal
    {
      get { return _anchorOrientationALocal; }
      set
      {
        if (_anchorOrientationALocal != value)
        {
          _anchorOrientationALocal = value;
          OnChanged();
        }
      }
    }
    private Matrix33F _anchorOrientationALocal = Matrix33F.Identity;


    /// <summary>
    /// Gets or sets the constraint anchor orientation on <see cref="Constraint.BodyB"/> in local 
    /// space of <see cref="Constraint.BodyB"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor orientation on <see cref="Constraint.BodyB"/> in local space of 
    /// <see cref="Constraint.BodyB"/>.
    /// </value>
    public Matrix33F AnchorOrientationBLocal
    {
      get { return _anchorOrientationBLocal; }
      set
      {
        if (_anchorOrientationBLocal != value)
        {
          _anchorOrientationBLocal = value;
          OnChanged();
        }
      }
    }
    private Matrix33F _anchorOrientationBLocal = Matrix33F.Identity;


    /// <summary>
    /// Gets or sets the minimum movement limit on the three constraint axes (in radians).
    /// </summary>
    /// <value>
    /// The minimum movement limits in radians. One element for each constraint axis.
    /// The default is (-∞, -∞, -∞), which means that there is no minimum limit.
    /// </value>
    public Vector3F Minimum
    {
      get { return _minimum; }
      set
      {
        if (_minimum != value)
        {
          _minimum = value;
          OnChanged();
        }
      }
    }
    private Vector3F _minimum;


    /// <summary>
    /// Gets or sets the maximum movement limit on the three constraint axes (in radians).
    /// </summary>
    /// <value>
    /// The maximum movement limits in radians. One element for each constraint axis.
    /// The default is (+∞, +∞, +∞), which means that there is no maximum limit.
    /// </value>
    public Vector3F Maximum
    {
      get { return _maximum; }
      set
      {
        if (_maximum != value)
        {
          _maximum = value;
          OnChanged();
        }
      }
    }
    private Vector3F _maximum;


    /// <summary>
    /// Gets or sets the error reduction parameter.
    /// </summary>
    /// <value>
    /// The error reduction parameter in the range [0, 1]. One entry for each constraint axis.
    /// </value>
    /// <remarks>
    /// The error reduction parameter is a value between 0 and 1. It defines how fast a constraint 
    /// error is removed. If the error reduction parameter is 0, constraint errors are not removed. 
    /// If the value is 1 the simulation tries to remove the whole constraint error in one time 
    /// step - which is usually unstable. A good value is for example 0.2.
    /// </remarks>
    public Vector3F ErrorReduction
    {
      get { return _errorReduction; }
      set
      {
        if (_errorReduction != value)
        {
          _errorReduction = value;
          OnChanged();
        }
      }
    }
    private Vector3F _errorReduction = new Vector3F(0.2f);


    /// <summary>
    /// Gets or sets the softness.
    /// </summary>
    /// <value>
    /// The softness. One element for each constraint axis. The default value is (0, 0, 0).
    /// </value>
    /// <remarks>
    /// The softness parameter can be used to allow the constraint to be violated by a small amount.
    /// This has the effect that the joint appears "soft". If the value is 0 the constraint is
    /// "hard" and the simulation will try to counter all constraint violations. A small positive
    /// value (e.g. 0.001) can be used to make the constraint soft.
    /// </remarks>
    public Vector3F Softness
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
    private Vector3F _softness;


    /// <summary>
    /// Gets or sets the coefficients of restitution.
    /// </summary>
    /// <value>The coefficients of restitution. One entry for each rotation axis.</value>
    /// <remarks>
    /// <para>
    /// If the bodies reach a limit (<see cref="Minimum"/> or <see cref="Maximum"/>), the bodies 
    /// will bounce back. If this property is 0, there will be no bounce. If this property is 1, 
    /// the whole angular velocity about the constraint axis is reflected.
    /// </para>
    /// <para>
    /// This vector defines the restitution for each rotation axis. The minimum and maximum limit of
    /// one axis use the same restitution value.
    /// </para>
    /// </remarks>
    public Vector3F Restitution
    {
      get { return _restitution; }
      set
      {
        if (_restitution != value)
        {
          _restitution = value;
          OnChanged();
        }
      }
    }
    private Vector3F _restitution;


    /// <summary>
    /// Gets or sets the maximal forces for the three rotational constraints.
    /// </summary>
    /// <value>
    /// The maximal forces for the three rotational constraints. One entry for each rotation axis.
    /// The default value is (+∞, +∞, +∞).
    /// </value>
    /// <remarks>
    /// This property defines the maximal force that can be apply to keep the constraint satisfied. 
    /// </remarks>
    public Vector3F MaxForce
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
    private Vector3F _maxForce = new Vector3F(float.PositiveInfinity);


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
    /// Gets the relative rotations about the constraint axes (the three Euler angles).
    /// </summary>
    /// <value>
    /// The relative rotation angles about the constraint axes in radians: (Angle0, Angle1, Angle2)
    /// </value>
    /// <exception cref="PhysicsException">
    /// <see cref="Constraint.BodyA"/> or <see cref="Constraint.BodyB"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public Vector3F RelativePosition
    {
      get
      {
        if (BodyA == null)
          throw new PhysicsException("BodyA must not be null.");
        if (BodyB == null)
          throw new PhysicsException("BodyB must not be null.");

        // Get anchor orientations in world space.
        Matrix33F anchorOrientationA = BodyA.Pose.Orientation * AnchorOrientationALocal;
        Matrix33F anchorOrientationB = BodyB.Pose.Orientation * AnchorOrientationBLocal;

        // Get anchor orientation of B relative to A.
        Matrix33F relativeOrientation = anchorOrientationA.Transposed * anchorOrientationB;

        // The Euler angles.
        Vector3F angles = ConstraintHelper.GetEulerAngles(relativeOrientation);

        return angles;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the state of an angular limit about a certain axis.
    /// </summary>
    /// <param name="index">
    /// The index of the limit axis. (0 = first rotation axis, 1 = second rotation axis, 
    /// 2 = third rotation axis)
    /// </param>
    /// <returns>
    /// The limit state on the given axis.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not in the range [0, 2].
    /// </exception>
    public LimitState GetLimitState(int index)
    {
      if (index < 0 || index > 2)
        throw new ArgumentOutOfRangeException("index", "The index must be 0, 1 or 2.");

      return _limitStates[index];
    }


    /// <inheritdoc/>
    protected override void OnSetup()
    {
      // Get anchor orientations in world space.
      Matrix33F anchorOrientationA = BodyA.Pose.Orientation * AnchorOrientationALocal;
      Matrix33F anchorOrientationB = BodyB.Pose.Orientation * AnchorOrientationBLocal;

      // Get anchor orientation of B relative to A.
      Matrix33F relativeOrientation = anchorOrientationA.Transposed * anchorOrientationB;

      Vector3F angles = ConstraintHelper.GetEulerAngles(relativeOrientation);

      // The constraint axes: See OneNote for a detailed derivation of these non-intuitive axes.
      var xA = anchorOrientationA.GetColumn(0);  // Anchor x-axis on A.
      var zB = anchorOrientationB.GetColumn(2);  // Anchor z-axis on B.
      Vector3F constraintAxisY = Vector3F.Cross(zB, xA);
      Vector3F constraintAxisX = Vector3F.Cross(constraintAxisY, zB);
      Vector3F constraintAxisZ = Vector3F.Cross(xA, constraintAxisY);

      // Remember old states.
      LimitState oldXLimitState = _limitStates[0];
      LimitState oldYLimitState = _limitStates[1];
      LimitState oldZLimitState = _limitStates[2];

      SetupConstraint(0, angles.X, constraintAxisX);
      SetupConstraint(1, angles.Y, constraintAxisY);
      SetupConstraint(2, angles.Z, constraintAxisZ);

      Warmstart(0, oldXLimitState);
      Warmstart(1, oldYLimitState);
      Warmstart(2, oldZLimitState);
    }


    private void SetupConstraint(int index, float position, Vector3F axis)
    {
      // Note: Cached constraint impulses are reset in Warmstart() if necessary.

      if (axis.IsNumericallyZero)
      {
        // The constraint is possibly violated so much that we could not compute valid euler angles
        // and constraint axis (The y angle must not be outside +/- π/2!)
        _limitStates[index] = LimitState.Inactive;
        return;
      }

      Constraint1D constraint = _constraints[index];
      float minimum = Minimum[index];
      float maximum = Maximum[index];
      Simulation simulation = Simulation;
      float deltaTime = simulation.Settings.Timing.FixedTimeStep;

      // ----- Determine limit state.
      if (minimum > maximum)
      {
        _limitStates[index] = LimitState.Inactive;

        // Nothing more to do.
        return;
      }
      if (Numeric.AreEqual(minimum, maximum))
      {
        _limitStates[index] = LimitState.Locked;
      }
      else if (position <= minimum)
      {
        _limitStates[index] = LimitState.Min;
      }
      else if (position >= maximum)
      {
        _limitStates[index] = LimitState.Max;
      }
      else
      {
        _limitStates[index] = LimitState.Inactive;

        // Nothing more to do.
        return;
      }

      Debug.Assert(_limitStates[index] != LimitState.Inactive);

      // ----- Error correction
      float deviation = 0;
      var allowedDeviation = simulation.Settings.Constraints.AllowedAngularDeviation;
      if (_limitStates[index] == LimitState.Locked)
        allowedDeviation = 0;
      if (position > maximum + allowedDeviation)
        deviation = maximum - position + allowedDeviation;
      else if (position < minimum - allowedDeviation)
        deviation = minimum - position - allowedDeviation;

      float targetVelocity = deviation * ErrorReduction[index] / deltaTime;
      float maxErrorCorrectionVelocity = simulation.Settings.Constraints.MaxErrorCorrectionVelocity;
      targetVelocity = MathHelper.Clamp(targetVelocity, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);

      // ----- Restitution
      float restitution = Restitution[index];
      if (restitution > simulation.Settings.Constraints.RestitutionThreshold)
      {
        float velocity = constraint.GetRelativeVelocity(BodyA, BodyB);
        if (_limitStates[index] == LimitState.Min)
        {
          if (velocity < -Simulation.Settings.Constraints.RestingVelocityLimit)
            targetVelocity = Math.Max(targetVelocity, -velocity * restitution);
        }
        else if (_limitStates[index] == LimitState.Max)
        {
          if (velocity > Simulation.Settings.Constraints.RestingVelocityLimit)
            targetVelocity = Math.Min(targetVelocity, -velocity * restitution);
        }
      }
      constraint.TargetRelativeVelocity = targetVelocity;

      // ----- Impulse limits
      float impulseLimit = MaxForce[index] * deltaTime;
      if (_limitStates[index] == LimitState.Min)
      {
        _minImpulseLimits[index] = 0;
        _maxImpulseLimits[index] = impulseLimit;
      }
      else if (_limitStates[index] == LimitState.Max)
      {
        _minImpulseLimits[index] = -impulseLimit;
        _maxImpulseLimits[index] = 0;
      }
      else //if (_limitStates[index] == LimitState.Locked)
      {
        _minImpulseLimits[index] = -impulseLimit;
        _maxImpulseLimits[index] = impulseLimit;
      }

      // Note: Softness must be set before!
      constraint.Softness = Softness[index] / deltaTime;
      constraint.Prepare(BodyA, BodyB, Vector3F.Zero, -axis, Vector3F.Zero, axis);
    }


    private void Warmstart(int index, LimitState oldState)
    {
      // If the limit state has not changed and the limit is active, we warmstart.
      // Otherwise, we reset the cached constraint impulse.

      Constraint1D constraint = _constraints[index];
      if (oldState != LimitState.Inactive && oldState == _limitStates[index])
        constraint.Warmstart(BodyA, BodyB);
      else if (constraint != null)
        constraint.ConstraintImpulse = 0;
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      Vector3F impulse = new Vector3F();
      impulse.X = ApplyImpulse(0);
      impulse.Y = ApplyImpulse(1);
      impulse.Z = ApplyImpulse(2);

      return impulse.LengthSquared > Simulation.Settings.Constraints.MinConstraintImpulseSquared;
    }


    private float ApplyImpulse(int index)
    {
      if (_limitStates[index] != LimitState.Inactive)
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
