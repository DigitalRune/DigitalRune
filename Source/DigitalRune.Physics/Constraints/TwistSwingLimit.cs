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
  /// Defines a twist and swing limits to limit rotations.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This limit is often used to limit the rotation of ragdoll joints. 
  /// <see cref="AnchorOrientationALocal"/> defines a coordinate frame fixed on the first body. 
  /// <see cref="AnchorOrientationBLocal"/> defines a coordinate frame fixed on the second body.
  /// Twist is defined as the rotation of the x-axis (x is the twist axis). Swing is defined as the
  /// deviation of the x-axes. The y- and the z-axes fixed on the first body are the swing axes. The
  /// twist and swing values are angles in the range [-π, π]. Using the properties 
  /// <see cref="Minimum"/> and <see cref="Maximum"/> the twist and swing angles can be limited.
  /// Limiting the swing angles creates a limit cone. Different minimum and maximum swing limits can
  /// be chosen for the swing axes. This can be used to create a deformed limit cone. 
  /// </para>
  /// <para>
  /// This constraint should not be used if the swing on a swing axis is locked (minimum and maximum
  /// is set to 0). In this case a different constraint type should be used (e.g. a 
  /// <see cref="HingeJoint"/>). Using very non-uniform swing limits is also not recommended. The 
  /// swing limits appear "smoother" if the cone is symmetric.
  /// </para>
  /// </remarks>
  public class TwistSwingLimit : Constraint
  {
    // x is the twist axis. 
    // y and z are swing axes.
    // The twist rotation axis is fixed on BodyB. The swing axes are fixed on Body A.
    // Using different limits for SwingX/YMin/Max creates a cone that is made up of different
    // elliptic curves.
    // Should not be used if swings are locked.

    // Possible improvements:
    // Don't simply rotate the x-axis back into the cone - rotate it to the nearest
    // cone border. Maybe use the normal direction of the ellipse...


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LimitState[] _limitStates = new LimitState[2];
    private Vector2F _minImpulseLimits;
    private Vector2F _maxImpulseLimits;
    private readonly Constraint1D[] _constraints =
    {
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
    /// Gets or sets the minimum movement limit on the twist and swing axes.
    /// </summary>
    /// <value>
    /// <para>
    /// The minimum movement limits in radians: (MinTwistAngleX, MinSwingAngleY, MinSwingAngleZ).
    /// The default is (0, -π/4, -π/4), which means no twist and the cone opens to -45° from the
    /// twist axis. The minimum limits of the swing axes must not be positive.
    /// </para>
    /// <para>The twist and swing angles are angles in the range [-π, π].</para>
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A minimum swing limit is positive.
    /// </exception>
    public Vector3F Minimum
    {
      get { return _minimum; }
      set
      {
        if (_minimum != value)
        {
          if (value.Y > 0 || value.Z > 0)
            throw new ArgumentOutOfRangeException("value", "Minimum swing limits must be less than or equal to 0.");

          _minimum = value;
          OnChanged();
        }
      }
    }
    private Vector3F _minimum = new Vector3F(0, -ConstantsF.PiOver4, -ConstantsF.PiOver4);


    /// <summary>
    /// Gets or sets the maximum movement limit on the twist and swing axes.
    /// </summary>
    /// <value>
    /// <para>
    /// The maximum movement limits in radians: (MaxTwistAngleX, MaxSwingAngleY, MaxSwingAngleZ).
    /// The default is (0, π/4, π/4), which means no twist and the cone opens to +45° from the twist
    /// axis. The maximum limits of the swing axes must not be negative.
    /// </para>
    /// <para>
    /// The twist and swing angles are angles in the range [-π, π].
    /// </para>
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A maximum swing limit is negative.
    /// </exception>
    public Vector3F Maximum
    {
      get { return _maximum; }
      set
      {
        if (_maximum != value)
        {
          if (value.Y < 0 || value.Z < 0)
            throw new ArgumentOutOfRangeException("value", "Maximum swing limits must be greater than or equal to 0.");

          _maximum = value;
          OnChanged();
        }
      }
    }
    private Vector3F _maximum = new Vector3F(0, ConstantsF.PiOver4, ConstantsF.PiOver4);


    /// <summary>
    /// Gets or sets the error reduction parameter.
    /// </summary>
    /// <value>The error reduction parameter in the range [0, 1]. The default value is 0.2.</value>
    /// <remarks>
    /// The error reduction parameter is a value between 0 and 1. It defines how fast a constraint 
    /// error is removed. If the error reduction parameter is 0, constraint errors are not removed. 
    /// If the value is 1 the simulation tries to remove the whole constraint error in one time 
    /// step - which is usually unstable. A good value is for example 0.2.
    /// </remarks>
    public float ErrorReduction
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
    private float _errorReduction = 0.2f;


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
    /// Gets or sets the coefficient of restitution for limits.
    /// </summary>
    /// <value>
    /// The coefficient of restitution for twist and swing: (TwistRestitution, SwingRestitution)
    /// </value>
    /// <remarks>
    /// If the bodies reach a limit on the line axis (<see cref="Minimum"/> or 
    /// <see cref="Maximum"/>), the bodies will bounce back. If this property is 0, there will be no
    /// bounce. If this property is 1, the whole velocity is reflected.
    /// </remarks>
    public Vector2F Restitution
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
    private Vector2F _restitution;


    /// <summary>
    /// Gets or sets the maximal forces that are applied by this constraint.
    /// </summary>
    /// <value>
    /// The maximal forces for the twist and swing limits: (MaxForceTwist, MaxForceSwing). 
    /// The default value is (+∞, +∞).
    /// </value>
    /// <remarks>
    /// This property defines the maximal force that can be apply to keep the constraint satisfied. 
    /// </remarks>
    public Vector2F MaxForce
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
    private Vector2F _maxForce = new Vector2F(float.PositiveInfinity);


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
               + _constraints[1].ConstraintImpulse * _constraints[1].JAngB;
      }
    }


    //public Vector3F RelativePosition
    //{
    //  get
    //  {
    //    if (BodyA == null)
    //      throw new PhysicsException("BodyA must not be null.");
    //    if (BodyB == null)
    //      throw new PhysicsException("BodyB must not be null.");

    //    // Anchor orientation in world space.
    //    Matrix33F anchorOrientationA = BodyA.Pose.Orientation * AnchorOrientationALocal;
    //    Matrix33F anchorOrientationB = BodyB.Pose.Orientation * AnchorOrientationBLocal;

    //    // Anchor orientation of B relative to A.
    //    Matrix33F relativeOrientation = anchorOrientationA.Transposed * anchorOrientationB;

    //    // The Euler angles.
    //    Vector3F angles = GetAngles(relativeOrientation);

    //    return angles;
    //  }
    //}
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
      // Get anchor orientations in world space.
      Matrix33F anchorOrientationA = BodyA.Pose.Orientation * AnchorOrientationALocal;
      Matrix33F anchorOrientationB = BodyB.Pose.Orientation * AnchorOrientationBLocal;

      // Get the quaternion that rotates something from anchor orientation A to 
      // anchor orientation B:
      //   QB = QTotal * QA
      //   => QTotal = QB * QA.Inverse
      QuaternionF total = QuaternionF.CreateRotation(anchorOrientationB * anchorOrientationA.Transposed);

      // Compute swing axis and angle.
      Vector3F xAxisA = anchorOrientationA.GetColumn(0);
      Vector3F yAxisA = anchorOrientationA.GetColumn(1);
      Vector3F xAxisB = anchorOrientationB.GetColumn(0);
      QuaternionF swing = QuaternionF.CreateRotation(xAxisA, xAxisB);

      Vector3F swingAxis = new Vector3F(swing.X, swing.Y, swing.Z);
      if (!swingAxis.TryNormalize())
        swingAxis = yAxisA;

      float swingAngle = swing.Angle;

      Debug.Assert(
        0 <= swingAngle && swingAngle <= ConstantsF.Pi,
        "QuaternionF.CreateRotation(Vector3F, Vector3F) should only create rotations along the \"short arc\".");

      // The swing limits create a deformed cone. If we look onto the x-axis of A:
      // y-axis goes to the right. z-axis goes up. 
      Vector3F xAxisBInAnchorA = Matrix33F.MultiplyTransposed(anchorOrientationA, xAxisB);
      float directionY = xAxisBInAnchorA.Y;
      float directionZ = xAxisBInAnchorA.Z;

      // In this plane, we have an ellipse with the formula:
      //   y²/a² + z²/b² = 1, where a and b are the ellipse radii.
      // We don't know the exact radii. We can compute them from the swing min/max angles.
      // To make it simpler, we do not use a flat ellipse. We use the swing z limit for a.
      // And the swing y limit for b.
      // We have a different ellipse for each quarter.
      float ellipseA = (directionY > 0) ? Maximum.Z : -Minimum.Z;
      float ellipseB = (directionZ > 0) ? -Minimum.Y : Maximum.Y;

      // The angles are in radians: angle = bow/radius. So our a and b are on the unit sphere.
      // This creates an elliptic thing on the unit sphere - not in a plane. We don't care because
      // we only need a smooth interpolation between the swing y and z limits.
      // No we look for the swing angle in the direction of xAxisB.
      // The next step can derived from following formulas:
      //     y²/a² + z²/b² = 1                   The ellipse formula.
      //     slope = directionZ / directionY     The direction in which we need the limit.
      //     slope = z/y                         The (y,z) is the point on the ellipse in the given direction.
      //     swingLimit = sqrt(y² + z²)          This is the distance of (y,z) from the center.
      // Since our ellipse is on a sphere, swingLimit is an angle (= bow / radius).

      float swingLimit = ellipseB;
      if (!Numeric.IsZero(directionY))
      {
        float slope = directionZ / directionY;
        float slopeSquared = slope * slope;
        float ellipseASquared = ellipseA * ellipseA;
        float ellipseBSquared = ellipseB * ellipseB;
        swingLimit = (float)Math.Sqrt((1 + slopeSquared) / (1 / ellipseASquared + slopeSquared / ellipseBSquared));

        // The ellipse normal would give us a better swing axis. But our computed swingAngle
        // is not correct for this axis...
        // Create a swing axis from the ellipse normal.
        //float k = ellipseASquared / ellipseBSquared * directionZ / directionY;
        //var normal = anchorOrientationA * new Vector3F(0, -k, 1).Normalized;
        //if (Vector3F.Dot(normal, swingAxis) < 0)
        //  swingAxis = -normal;
        //else
        //  swingAxis = normal;
      }

#if DEBUG
      //Debug.Assert(QuaternionF.Dot(swing, total) >= 0);
      var swingAxisALocal = Matrix33F.MultiplyTransposed(anchorOrientationA, swingAxis);
      Debug.Assert(Numeric.IsZero(swingAxisALocal.X));
#endif

      // We define our rotations like this:
      // First we twist around the x-axis of A. Then we swing.
      //   QTotal = QSwing * QTwist
      //   => QSwing.Inverse * QTotal = QTwist
      QuaternionF twist = swing.Conjugated * total;
      twist.Normalize();

      // The quaternion returns an angle in the range [0, 2π].
      float twistAngle = twist.Angle;

      // The minimum and maximum twist limits are in the range [-π, π].
      if (twistAngle > ConstantsF.Pi)
      {
        // Convert the twistAngle to the range used by the twist limits.
        twistAngle = -(ConstantsF.TwoPi - twistAngle);
        Debug.Assert(-ConstantsF.TwoPi < twistAngle && twistAngle <= ConstantsF.TwoPi);
      }

      // The axis of the twist quaternion is parallel to xAxisA.
      Vector3F twistAxis = new Vector3F(twist.X, twist.Y, twist.Z);
      if (Vector3F.Dot(twistAxis, xAxisA) < 0)
      {
        // The axis of the twist quaternion points in the opposite direction of xAxisA.
        // The twist angle need to be inverted.
        twistAngle = -twistAngle;
      }

      // Remember old states.
      LimitState oldXLimitState = _limitStates[0];
      LimitState oldYLimitState = _limitStates[1];

      // Note: All axes between xAxisA and xAxisB should be valid twist axes.
      SetupConstraint(0, twistAngle, xAxisB, Minimum[0], Maximum[0]);
      SetupConstraint(1, swingAngle, swingAxis, -swingLimit, swingLimit);

      // Warm-start the constraints if the previous limit state matches the new limit state.
      Warmstart(0, oldXLimitState);
      Warmstart(1, oldYLimitState);
    }


    private void SetupConstraint(int index, float position, Vector3F axis, float minimum, float maximum)
    {
      // Note: Cached constraint impulses are reset in Warmstart() if necessary.

      Constraint1D constraint = _constraints[index];
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
      var allowedAngularDeviation = simulation.Settings.Constraints.AllowedAngularDeviation;
      if (position > maximum + allowedAngularDeviation)
        deviation = maximum - position + allowedAngularDeviation;
      else if (position < minimum - allowedAngularDeviation)
        deviation = minimum - position - allowedAngularDeviation;

      float targetVelocity = deviation * ErrorReduction / deltaTime;
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
      constraint.Softness = Softness / deltaTime;
      constraint.Prepare(BodyA, BodyB, Vector3F.Zero, -axis, Vector3F.Zero, axis);
    }


    private void Warmstart(int index, LimitState oldState)
    {
      // If the limit state has not changed and is active, we warmstart.
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
      float impulseTwist = ApplyImpulse(0);
      float impulseSwing = ApplyImpulse(1);

      return Math.Abs(impulseTwist) > Simulation.Settings.Constraints.MinConstraintImpulse
              || Math.Abs(impulseSwing) > Simulation.Settings.Constraints.MinConstraintImpulse;
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

      base.OnChanged();
    }


    /// <summary>
    /// Gets a point on the swing limit cone (for debug visualization).
    /// </summary>
    /// <param name="angle">The angle about the twist axis in radians.</param>
    /// <param name="coneTip">The position of the tip of the cone.</param>
    /// <param name="distanceFromTip">The distance from tip.</param>
    /// <returns>
    /// A point that lies on the cone. The distance to the tip of the cone is 
    /// <paramref name="distanceFromTip"/>.
    /// </returns>
    /// <remarks>
    /// The swing limits form a deformed cone around the twist axis. This method can be used to get
    /// points to draw a debug visualization of the swing limit cone. Call this method for angles
    /// between 0 to 360°. Draw lines between neighbor points and the points and the cone tip. This
    /// creates a wire frame visualization of the swing limit cone.
    /// </remarks>
    public Vector3F GetPointOnCone(float angle, Vector3F coneTip, float distanceFromTip)
    {
      // angle = 0 is BodyA +Y axis. angles rotate around BodyA +X axis.

      // The computation is the same as above.

      float directionY = (float)Math.Cos(angle);
      float directionZ = (float)Math.Sin(angle);

      float ellipseA = (directionY > 0) ? Maximum.Z : -Minimum.Z;
      float ellipseB = (directionZ > 0) ? -Minimum.Y : Maximum.Y;

      float swingLimit = ellipseB;
      if (!Numeric.IsZero(directionY))
      {
        float slope = directionZ / directionY;
        float slopeSquared = slope * slope;
        float ellipseASquared = ellipseA * ellipseA;
        float ellipseBSquared = ellipseB * ellipseB;
        swingLimit = (float)Math.Sqrt((1 + slopeSquared) / (1 / ellipseASquared + slopeSquared / ellipseBSquared));
      }

      var swingAxis = new Vector3F(0, -directionZ, directionY);
      var swing = QuaternionF.CreateRotation(swingAxis, swingLimit);

      var pointInAnchorA = swing.Rotate(new Vector3F(distanceFromTip, 0, 0));

      var pointInA = AnchorOrientationALocal * pointInAnchorA;
      if (BodyA != null)
      {
        var point = BodyA.Pose.ToWorldDirection(pointInA);
        return point + coneTip;
      }
      else
      {
        return pointInA + coneTip;
      }
    }
    #endregion
  }
}
