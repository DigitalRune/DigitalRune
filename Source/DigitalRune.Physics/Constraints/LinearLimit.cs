// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint that restricts translational movement. This constraint is configurable
  /// to create custom joints.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="AnchorPoseALocal"/> defines a constraint anchor point on the first body and three
  /// constraint axes. The movement of <see cref="AnchorPositionBLocal"/> on the second body is 
  /// restricted relative to the fixed axis on the first body.
  /// </para>
  /// <para>
  /// This constraint can be used to create custom constraints. For example using a minimum and
  /// maximum limits of (-∞, 0, 0) and (+∞, 0, 0) creates a <see cref="PointOnLineConstraint"/> 
  /// where the constraint x-axis is the line axis. Using a minimum and maximum of (-∞, -∞, 0)
  /// and (+∞, +∞, 0) creates a <see cref="PointOnPlaneConstraint"/> where the constraint x- and 
  /// y-axes define the plane and the plane is fixed on the first body. Using a minimum and maximum
  /// of (0, 0, 0) creates a <see cref="BallJoint"/>.
  /// </para>
  /// </remarks>
  public class LinearLimit : Constraint
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
    /// Gets or sets the constraint anchor pose on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor pose on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>.
    /// </value>
    public Pose AnchorPoseALocal
    {
      get { return _anchorPoseALocal; }
      set
      {
        if (_anchorPoseALocal != value)
        {
          _anchorPoseALocal = value;
          OnChanged();
        }
      }
    }
    private Pose _anchorPoseALocal = Pose.Identity;


    /// <summary>
    /// Gets or sets the constraint anchor position on <see cref="Constraint.BodyB"/> in local space
    /// of <see cref="Constraint.BodyB"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor position on <see cref="Constraint.BodyB"/> in local space of 
    /// <see cref="Constraint.BodyB"/>.
    /// </value>
    public Vector3F AnchorPositionBLocal
    {
      get { return _anchorPositionBLocal; }
      set
      {
        if (_anchorPositionBLocal != value)
        {
          _anchorPositionBLocal = value;
          OnChanged();
        }
      }
    }
    private Vector3F _anchorPositionBLocal;


    /// <summary>
    /// Gets or sets the minimum movement limit on the three constraint axes.
    /// </summary>
    /// <value>
    /// The minimum movement limits. One element for each constraint axis.
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
    /// Gets or sets the maximum movement limit on the three constraint axes.
    /// </summary>
    /// <value>
    /// The maximum movement limits. One element for each constraint axis.
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
    /// <value>
    /// The coefficients of restitution. One entry for each rotation axis.
    /// </value>
    /// <remarks>
    /// <para>
    /// If the bodies reach a limit (<see cref="Minimum"/> or <see cref="Maximum"/>), the bodies 
    /// will bounce back. If this property is 0, there will be no bounce. If this property is 1, 
    /// the whole linear velocity along the constraint axis is reflected.
    /// </para>
    /// <para>
    /// This vector defines the restitution for each linear movement axis. The minimum and maximum
    /// limit of one axis use the same restitution value.
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
    /// Gets or sets the maximal forces for the three constraint axes.
    /// </summary>
    /// <value>
    /// The maximal forces for the three constraint axes. One entry for each constraint axis.
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
        return _constraints[0].ConstraintImpulse * _constraints[0].JLinB
               + _constraints[1].ConstraintImpulse * _constraints[1].JLinB
               + _constraints[2].ConstraintImpulse * _constraints[2].JLinB;
      }
    }


    /// <inheritdoc/>
    public override Vector3F AngularConstraintImpulse
    {
      get
      {
        return Vector3F.Zero;
      }
    }


    /// <summary>
    /// Gets the relative position on the constraint axes.
    /// </summary>
    /// <value>
    /// The relative position on the constraint axes: (RelativePositionX, RelativePositionY,
    /// RelativePositionZ)
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

        // Get anchor pose/position in world space.
        Pose anchorPoseA = BodyA.Pose * AnchorPoseALocal;
        Vector3F anchorPositionB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

        // Compute anchor position on B relative to anchor pose of A.
        Vector3F relativePosition = anchorPoseA.ToLocalPosition(anchorPositionB);

        return relativePosition;
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
    /// Gets the state of a linear limit.
    /// </summary>
    /// <param name="index">
    /// The index of the limit axis. (0 = x-axis, 1 = y-axis, 2 = z-axis).
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
      // Get anchor pose/position in world space.
      Pose anchorPoseA = BodyA.Pose * AnchorPoseALocal;
      Vector3F anchorPositionB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);  // TODO: We could store rALocal and use ToWorldDirection instead of ToWorldPosition.

      // Compute anchor position on B relative to anchor pose of A.
      Vector3F relativePosition = anchorPoseA.ToLocalPosition(anchorPositionB);

      // The linear constraint axes are the fixed anchor axes of A!
      Matrix33F anchorOrientation = anchorPoseA.Orientation;

      Vector3F rA = anchorPoseA.Position - BodyA.PoseCenterOfMass.Position;
      Vector3F rB = anchorPositionB - BodyB.PoseCenterOfMass.Position;

      // Remember old states.
      LimitState oldXLimitState = _limitStates[0];
      LimitState oldYLimitState = _limitStates[1];
      LimitState oldZLimitState = _limitStates[2];

      SetupConstraint(0, relativePosition.X, anchorOrientation.GetColumn(0), rA, rB);
      SetupConstraint(1, relativePosition.Y, anchorOrientation.GetColumn(1), rA, rB);
      SetupConstraint(2, relativePosition.Z, anchorOrientation.GetColumn(2), rA, rB);

      Warmstart(0, oldXLimitState);
      Warmstart(1, oldYLimitState);
      Warmstart(2, oldZLimitState);
    }


    private void SetupConstraint(int index, float position, Vector3F axis, Vector3F rA, Vector3F rB)
    {
      // Note: Cached constraint impulses are reset in Warmstart() if necessary.

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
      var allowedDeviation = simulation.Settings.Constraints.AllowedLinearDeviation;
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
      constraint.Prepare(BodyA, BodyB, -axis, -Vector3F.Cross(rA, axis), axis, Vector3F.Cross(rB, axis));
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


    /// <summary>
    /// Applies the impulse for a single constraint axis.
    /// </summary>
    /// <param name="index">The index of the constraint axis.</param>
    /// <returns>The applied impulse.</returns>
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
