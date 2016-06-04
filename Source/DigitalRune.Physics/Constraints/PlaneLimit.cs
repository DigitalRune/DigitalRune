// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a limit plane and a limit point that must stay in front of the plane.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This limit places a plane on the first body. The constraint anchor point on the second body is
  /// forced to be in front of the plane on the other body.
  /// </para>
  /// <para>
  /// This constraint can be added to other joints. For example, the rotations of a 
  /// <see cref="BallJoint"/> can be limited by placing several <see cref="PlaneLimit"/>s.
  /// </para>
  /// </remarks>
  public class PlaneLimit : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _limitIsActive;
    private readonly Constraint1D _constraint = new Constraint1D();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the constraint plane that this is fixed on <see cref="Constraint.BodyA"/> 
    /// (defined in local space of <see cref="Constraint.BodyA"/>).
    /// </summary>
    /// <value>
    /// The constraint plane that this is fixed on <see cref="Constraint.BodyA"/> (defined in local
    /// space of <see cref="Constraint.BodyA"/>).
    /// </value>
    public Plane PlaneALocal
    {
      get { return _planeALocal; }
      set
      {
        _planeALocal = value;
        OnChanged();
      }
    }
    private Plane _planeALocal = new Plane(Vector3F.UnitY, 0);


    /// <summary>
    /// Gets or sets the constraint anchor position on <see cref="Constraint.BodyB"/> in local space
    /// of <see cref="Constraint.BodyB"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor position on <see cref="Constraint.BodyB"/> in local space of 
    /// <see cref="Constraint.BodyB"/>.
    /// </value>
    /// <remarks>
    /// This point on the second body is restricted to be in front of the plane that is fixed on
    /// <see cref="Constraint.BodyA"/>.
    /// </remarks>
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
    /// Gets or sets the coefficient of restitution.
    /// </summary>
    /// <value>The coefficient of restitution.</value>
    /// <remarks>
    /// If the constraint anchor point on the second body collides with the plane on the first body, 
    /// the bodies will bounce back. If this property is 0, there will be no bounce. If this 
    /// property is 1, the whole velocity is reflected.
    /// </remarks>
    public float Restitution
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
    private float _restitution;


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


    /// <summary>
    /// Gets or sets the constraint impulse that was applied. 
    /// </summary>
    /// <value>The constraint impulse in world space.</value>
    /// <remarks>
    /// This impulse was applied in the constraint anchor point on <see cref="Constraint.BodyB"/> in
    /// direction of the plane normal. An equivalent negative impulse was applied on 
    /// <see cref="Constraint.BodyA"/>.
    /// </remarks>
    public Vector3F ConstraintImpulse
    {
      get { return _constraint.ConstraintImpulse * _constraint.JLinB; }
    }


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
      var planeALocal = PlaneALocal;
      var planeNormal = BodyA.Pose.ToWorldDirection(planeALocal.Normal);

      // Calculate a point on the new plane.
      Vector3F pointOnPlane = BodyA.Pose.Position + planeNormal * planeALocal.DistanceFromOrigin;

      // Project point on to normal vector to get the new DistanceFromOrigin.
      var distanceFromOrigin = Vector3F.Dot(pointOnPlane, planeNormal);

      Vector3F anchorPositionB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

      float separation = Vector3F.Dot(anchorPositionB, planeNormal) - distanceFromOrigin;
      float penetrationDepth = -separation;

      Vector3F rA = anchorPositionB - BodyA.PoseCenterOfMass.Position;
      Vector3F rB = anchorPositionB - BodyB.PoseCenterOfMass.Position;

      // Remember old state.
      bool wasActive = _limitIsActive;

      if (separation <= 0)
      {
        _limitIsActive = true;

        Simulation simulation = Simulation;
        float deltaTime = simulation.Settings.Timing.FixedTimeStep;

        // ----- Determine limit state.

        // ----- Error correction
        float targetVelocity = penetrationDepth * ErrorReduction / deltaTime;
        float maxErrorCorrectionVelocity = simulation.Settings.Constraints.MaxErrorCorrectionVelocity;
        targetVelocity = MathHelper.Clamp(targetVelocity, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);

        // ----- Restitution
        if (Restitution > simulation.Settings.Constraints.RestitutionThreshold)
        {
          float velocity = _constraint.GetRelativeVelocity(BodyA, BodyB);
          if (velocity < -Simulation.Settings.Constraints.RestingVelocityLimit)
            targetVelocity = Math.Max(targetVelocity, -velocity * Restitution);
        }

        _constraint.TargetRelativeVelocity = targetVelocity;

        // ----- Impulse limits

        _constraint.Softness = Softness / deltaTime;
        _constraint.Prepare(BodyA, BodyB, -planeNormal, -Vector3F.Cross(rA, planeNormal), planeNormal, Vector3F.Cross(rB, planeNormal));
      }
      else
      {
        _limitIsActive = false;
      }

      // If the limit state has not changed, we warmstart.
      // Otherwise, we reset the cached constraint impulse.
      if (wasActive && _limitIsActive)
        _constraint.Warmstart(BodyA, BodyB);
      else
        _constraint.ConstraintImpulse = 0;
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      if (!_limitIsActive)
        return false;

      var relativeVelocity = _constraint.GetRelativeVelocity(BodyA, BodyB);
      var impulse = _constraint.SatisfyConstraint(BodyA, BodyB, relativeVelocity, 0, MaxForce);
      
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
