// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint that limits the distance of two points.
  /// </summary>
  /// <remarks>
  /// This constraint forces the constraint anchor points on the two bodies to have a distance
  /// between <see cref="MinDistance"/> and <see cref="MaxDistance"/>.
  /// </remarks>
  public class DistanceLimit : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private float _deltaTime;
    private Vector3F _ra;
    private Vector3F _rb;
    private Vector3F _axis;
    private bool _minLimitIsActive;
    private bool _maxLimitIsActive;

    private readonly Constraint1D _constraint = new Constraint1D();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the constraint anchor position on <see cref="Constraint.BodyA"/> in local space
    /// of <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor position on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>.
    /// </value>
    public Vector3F AnchorPositionALocal
    {
      get { return _anchorPositionALocal; }
      set
      {
        _anchorPositionALocal = value;
        OnChanged();
      }
    }
    private Vector3F _anchorPositionALocal;


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
        _anchorPositionBLocal = value;
        OnChanged();
      }
    }
    private Vector3F _anchorPositionBLocal;


    /// <summary>
    /// Gets or sets the minimal allowed distance between the constraint anchor points.
    /// </summary>
    /// <value>
    /// The minimal allowed distance between the constraint anchor points. The default value is 0.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MinDistance
    {
      get { return _minDistance; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MinDistance must be 0 or positive.");

        _minDistance = value;
        OnChanged();
      }
    }
    private float _minDistance;


    /// <summary>
    /// Gets or sets the maximal allowed distance between the constraint anchor points.
    /// </summary>
    /// <value>
    /// The maximal allowed distance between the constraint anchor points. The default value is 1.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxDistance
    {
      get { return _maxDistance; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxDistance must be 0 or positive.");

        _maxDistance = value;
        OnChanged();
      }
    }
    private float _maxDistance = 1;


    /// <inheritdoc/>
    public override Vector3F LinearConstraintImpulse
    {
      get { return _constraint.ConstraintImpulse * _constraint.JLinB; }
    }


    /// <inheritdoc/>
    public override Vector3F AngularConstraintImpulse
    {
      get { return Vector3F.Zero; }
    }


    /// <summary>
    /// Gets or sets the error reduction parameter.
    /// </summary>
    /// <value>The error reduction parameter in the range [0, 1].</value>
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
        _errorReduction = value;
        OnChanged();
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
    /// Gets or sets the maximal force that is applied by this constraint.
    /// </summary>
    /// <value>The maximal force. The default value is +∞.</value>
    /// <remarks>
    /// This property defines the maximal force that can be apply to keep the constraint satisfied. 
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
      _deltaTime = Simulation.Settings.Timing.FixedTimeStep;

      // Anchors in world space.
      var anchorA = BodyA.Pose.ToWorldPosition(AnchorPositionALocal);
      var anchorB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

      // The constraint acts on the axis between the anchors.
      _axis = anchorB - anchorA;

      float distance = _axis.Length;

      // Check if limits are active.
      _minLimitIsActive = distance < MinDistance && !Numeric.IsZero(distance);
      _maxLimitIsActive = distance > MaxDistance;

      // Abort if no limit is active.
      if (!_minLimitIsActive && !_maxLimitIsActive)
      {
        _constraint.ConstraintImpulse = 0;
        return;
      }

      _axis.TryNormalize();

      _ra = anchorA - BodyA.PoseCenterOfMass.Position;
      _rb = anchorB - BodyB.PoseCenterOfMass.Position;

      // Too close together = positive deviation and positive target velocity.
      // Too far apart = negative deviation and negative target velocity.
      var deviation = (_minLimitIsActive) ? MinDistance - distance : MaxDistance - distance;
      _constraint.TargetRelativeVelocity = deviation * ErrorReduction / _deltaTime;

      float maxErrorCorrectionVelocity = Simulation.Settings.Constraints.MaxErrorCorrectionVelocity;
      _constraint.TargetRelativeVelocity = MathHelper.Clamp(_constraint.TargetRelativeVelocity, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);

      _constraint.Softness = Softness / _deltaTime;
      _constraint.Prepare(BodyA, BodyB, -_axis, -Vector3F.Cross(_ra, _axis), _axis, Vector3F.Cross(_rb, _axis));

      // To keep it simple we do not warmstart. Warmstarting can only be done if the same limit
      // was active the last time.
      _constraint.ConstraintImpulse = 0;
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      // Nothing to do if no limit is active.
      if (!_minLimitIsActive && !_maxLimitIsActive)
        return false;

      // Relative velocity is positive if bodies are separating.
      float relativeVelocity = _constraint.GetRelativeVelocity(BodyA, BodyB);

      // If the max limit is reached, we must apply a negative impulse to bring bodies closer together.
      // If the min limit is reached, we must apply a positive impulse to get more separation.
      float impulseLimit = MaxForce * _deltaTime;
      float minImpulseLimit = (_maxLimitIsActive) ? -impulseLimit : 0;
      float maxImpulseLimit = (_minLimitIsActive) ? impulseLimit : 0;

      // Apply constraint impulse.
      float impulse = _constraint.SatisfyConstraint(BodyA, BodyB, relativeVelocity, minImpulseLimit, maxImpulseLimit);

      return Math.Abs(impulse) > Simulation.Settings.Constraints.MinConstraintImpulse;
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // Delete cached data.
      _constraint.ConstraintImpulse = 0;

      base.OnChanged();
    }
    #endregion
  }
}
