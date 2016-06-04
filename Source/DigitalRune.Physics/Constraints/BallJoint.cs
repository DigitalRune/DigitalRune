// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a ball-and-socked joint.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This joint is known as ball joint, ball-and-socked joint or spherical joint. It removes all 3
  /// translational degrees of freedom and allows only rotations. The rotations are not limited.
  /// </para>
  /// <para>
  /// See also <see href="http://en.wikipedia.org/wiki/Ball_joint"/>.
  /// </para>
  /// </remarks>
  public class BallJoint : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private float _deltaTime;
    private Vector3F _anchorAWorld;
    private Vector3F _anchorBWorld;
    private Matrix33F _kInverse;
    private Vector3F _targetVelocity;
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


    /// <inheritdoc/>
    public override Vector3F LinearConstraintImpulse
    {
      get
      {
        return _constraintImpulse;
      }
    }
    private Vector3F _constraintImpulse;


    /// <inheritdoc/>
    public override Vector3F AngularConstraintImpulse
    {
      get
      {
        return Vector3F.Zero;
      }
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

      // Get anchor positions in world space.
      _anchorAWorld = BodyA.Pose.ToWorldPosition(AnchorPositionALocal);
      _anchorBWorld = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

      // Compute inverse of K matrix. Softness is added to K matrix.
      var k = BodyA.ComputeKMatrix(_anchorAWorld) + BodyB.ComputeKMatrix(_anchorBWorld);
      k.M00 += Softness / _deltaTime;
      k.M11 += Softness / _deltaTime;
      k.M22 += Softness / _deltaTime;
      _kInverse = k.Inverse;

      // Constraint error
      var deviation = _anchorBWorld - _anchorAWorld;
      float deviationLength = deviation.Length;

      // Axis of error
      var dirAToB = deviation;
      dirAToB.TryNormalize();

      // Our relative velocity is defined like this:
      // > 0 objects are separating.
      // < 0 objects are getting closer.
      // The target velocity is ≤ 0 because when we have an error, anchors must come closer.
      _targetVelocity = -deviationLength * (1f / _deltaTime) * ErrorReduction * dirAToB;
      if (_targetVelocity.LengthSquared > Simulation.Settings.Constraints.MaxErrorCorrectionVelocitySquared)
        _targetVelocity = -Simulation.Settings.Constraints.MaxErrorCorrectionVelocity * dirAToB;

      // Warmstart
      BodyA.ApplyImpulse(-_constraintImpulse, _anchorAWorld);
      BodyB.ApplyImpulse(_constraintImpulse, _anchorBWorld);
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      // Compute relative velocity.
      Vector3F vA = BodyA.GetVelocityOfWorldPoint(_anchorAWorld);
      Vector3F vB = BodyB.GetVelocityOfWorldPoint(_anchorBWorld);
      Vector3F relativeVelocity = (vB - vA);

      // Compute constraint impulse.
      Vector3F impulse = _kInverse * (_targetVelocity - relativeVelocity - Softness / _deltaTime * _constraintImpulse);

      // Impulse accumulation and clamping.
      Vector3F oldConstraintImpulse = _constraintImpulse;
      _constraintImpulse += impulse;
      float impulseMagnitude = _constraintImpulse.Length;
      float maxImpulseMagnitude = MaxForce * _deltaTime;
      if (impulseMagnitude > maxImpulseMagnitude)
      {
        _constraintImpulse = _constraintImpulse / impulseMagnitude * maxImpulseMagnitude;
        impulse = _constraintImpulse - oldConstraintImpulse;
      }

      // Apply impulses
      BodyA.ApplyImpulse(-impulse, _anchorAWorld);
      BodyB.ApplyImpulse(impulse, _anchorBWorld);

      return impulse.LengthSquared > Simulation.Settings.Constraints.MinConstraintImpulseSquared;
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // Delete cached data.
      _constraintImpulse = Vector3F.Zero;

      base.OnChanged();
    }
    #endregion
  }
}
