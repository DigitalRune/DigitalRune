// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a motor that controls the linear distance of two constrained bodies.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The motor pushes both bodies until the relative position of the constraint anchor on the
  /// second body relative to the constraint anchor on the first body is equal to 
  /// <see cref="TargetPosition"/>. The motor acts like a damped-spring that pushes the bodies at
  /// the constraint anchor points (controlled by <see cref="SpringConstant"/> and
  /// <see cref="DampingConstant"/>).
  /// </para>
  /// </remarks>
  public class PositionMotor : Constraint
  {
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
    /// Gets or sets the target position of the <see cref="AnchorPositionBLocal"/> relative to 
    /// <see cref="AnchorPoseALocal"/>
    /// </summary>
    /// <value>The target position.</value>
    /// <remarks>
    /// This target position is the target position the constraint anchor on the second body
    /// relative to the constraint anchor position and constraint anchor axes on the first body.
    /// </remarks>
    public Vector3F TargetPosition
    {
      get { return _targetPosition; }
      set
      {
        if (_targetPosition != value)
        {
          _targetPosition = value;
          OnChanged();
        }
      }
    }
    private Vector3F _targetPosition;


    /// <summary>
    /// Gets or sets the spring constant.
    /// </summary>
    /// <value>The spring constant. The default value is 6000.</value>
    public float SpringConstant
    {
      get { return _springConstant; }
      set
      {
        if (_springConstant != value)
        {
          _springConstant = value;
          OnChanged();
        }
      }
    }
    private float _springConstant = 6000;


    /// <summary>
    /// Gets or sets the damping constant.
    /// </summary>
    /// <value>The damping constant. The default value is 900.</value>
    public float DampingConstant
    {
      get { return _dampingConstant; }
      set
      {
        if (_dampingConstant != value)
        {
          _dampingConstant = value;
          OnChanged();
        }
      }
    }
    private float _dampingConstant = 900;


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
    /// Gets the relative position of the constraint anchor on <see cref="Constraint.BodyB"/>
    /// relative to the constraint anchor on <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>The relative position.</value>
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

        // Get positions/poses in world space.
        Pose anchorPoseA = BodyA.Pose * AnchorPoseALocal;
        Vector3F anchorPositionB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

        // Compute anchor pose of B relative to anchor pose of A.
        Vector3F relativePosition = anchorPoseA.ToLocalPosition(anchorPositionB);

        return relativePosition;
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
      // In a single axis motor the user could specify MinForce and MaxForce so that he can
      // create inequality constraints.

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


    //public float MinVelocity { get; set; }


    /// <summary>
    /// Gets or sets the maximal velocity.
    /// </summary>
    /// <value>The maximal velocity.</value>
    /// <remarks>
    /// The motor will not create a velocity larger than this limit.
    /// </remarks>
    public float MaxVelocity
    {
      get { return _maxVelocity; }
      set
      {
        if (_maxVelocity != value)
        {
          _maxVelocity = value;
          OnChanged();
        }
      }
    }
    private float _maxVelocity = float.PositiveInfinity;
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
      // Anchor pose/position in world space.
      Pose anchorPoseA = BodyA.Pose * AnchorPoseALocal;
      Vector3F anchorPositionB = BodyB.Pose.ToWorldPosition(AnchorPositionBLocal);

      // Compute anchor pose of B relative to anchor pose of A.
      Vector3F relativePosition = anchorPoseA.ToLocalPosition(anchorPositionB);

      // The linear constraint axes are the fixed anchor axes of A!
      Matrix33F anchorOrientation = anchorPoseA.Orientation;

      Vector3F rA = anchorPoseA.Position - BodyA.PoseCenterOfMass.Position;
      Vector3F rB = anchorPositionB - BodyB.PoseCenterOfMass.Position;

      var deltaTime = Simulation.Settings.Timing.FixedTimeStep;
      float errorReduction = ConstraintHelper.ComputeErrorReduction(deltaTime, SpringConstant, DampingConstant);
      float softness = ConstraintHelper.ComputeSoftness(deltaTime, SpringConstant, DampingConstant);

      if (!UseSingleAxisMode)
      {
        SetupConstraint(0, relativePosition.X, TargetPosition.X, anchorOrientation.GetColumn(0), rA, rB, deltaTime, errorReduction, softness);
        SetupConstraint(1, relativePosition.Y, TargetPosition.Y, anchorOrientation.GetColumn(1), rA, rB, deltaTime, errorReduction, softness);
        SetupConstraint(2, relativePosition.Z, TargetPosition.Z, anchorOrientation.GetColumn(2), rA, rB, deltaTime, errorReduction, softness);
      }
      else
      {
        var axis = TargetPosition - relativePosition;
        var deviation = axis.Length;
        if (Numeric.IsZero(deviation))
          axis = Vector3F.UnitX;
        else
          axis.Normalize();

        SetupConstraint(0, -deviation, 0, axis, rA, rB, deltaTime, errorReduction, softness);
      }

      // No warmstarting.
      _constraints[0].ConstraintImpulse = 0;
      _constraints[1].ConstraintImpulse = 0;
      _constraints[2].ConstraintImpulse = 0;
    }


    private void SetupConstraint(int index, float position, float targetPosition, Vector3F axis,
                                 Vector3F rA, Vector3F rB, float deltaTime, float errorReduction, float softness)
    {
      // Note: Cached constraint impulses are reset in Warmstart() if necessary.

      Constraint1D constraint = _constraints[index];
      Simulation simulation = Simulation;

      // ----- Error correction
      float deviation = targetPosition - position;

      if (Numeric.IsZero(deviation))
      {
        _minImpulseLimits[index] = 0;
        _maxImpulseLimits[index] = 0;
        constraint.ConstraintImpulse = 0;
        return;
      }

      float fullCorrectionSpeed = deviation / deltaTime;
      float targetVelocity = fullCorrectionSpeed * errorReduction;
      //float minSpeed = MathHelper.Clamp(MinVelocity, -Math.Abs(fullCorrectionSpeed), Math.Abs(fullCorrectionSpeed));
      //if (targetVelocity >= 0 && targetVelocity < minSpeed)
      //  targetVelocity = minSpeed;
      //else if (targetVelocity <= 0 && targetVelocity > -minSpeed)
      //  targetVelocity = -minSpeed;
      float maxErrorCorrectionVelocity = simulation.Settings.Constraints.MaxErrorCorrectionVelocity;
      targetVelocity = MathHelper.Clamp(targetVelocity, -maxErrorCorrectionVelocity, maxErrorCorrectionVelocity);
      targetVelocity = MathHelper.Clamp(targetVelocity, -MaxVelocity, MaxVelocity);

      constraint.TargetRelativeVelocity = targetVelocity;

      // ----- Impulse limits
      float impulseLimit = MaxForce * deltaTime;
      _minImpulseLimits[index] = -impulseLimit;
      _maxImpulseLimits[index] = impulseLimit;

      constraint.Softness = softness / deltaTime;
      constraint.Prepare(BodyA, BodyB, -axis, -Vector3F.Cross(rA, axis), axis, Vector3F.Cross(rB, axis));
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
