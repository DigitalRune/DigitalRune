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
  /// Defines a motor that controls the relative orientation of two constrained bodies using
  /// quaternions.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The motor pushes both bodies until the relative orientation of the constraint anchor on the
  /// second body relative to the constraint anchor on the first body is equal to 
  /// <see cref="TargetOrientation"/>. The motor acts like a damped-spring that rotates the bodies
  /// (controlled by <see cref="SpringConstant"/> and <see cref="DampingConstant"/>).
  /// </para>
  /// <para>
  /// The target orientation is defined using a <see cref="QuaternionF"/>. In contrast, 
  /// <see cref="EulerMotor"/> is a motor that controls the orientation where the target orientation
  /// is defined using 3 Euler angle.
  /// </para>
  /// </remarks>
  public class QuaternionMotor : Constraint
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
    /// Gets or sets the target orientation of the anchor on <see cref="Constraint.BodyB"/> 
    /// relative to the anchor on <see cref="Constraint.BodyA"/>. 
    /// </summary>
    /// <value>The target orientation.</value>
    /// <remarks>
    /// This target orientation is the target orientation of <see cref="AnchorOrientationBLocal"/>
    /// relative to <see cref="AnchorOrientationALocal"/>.
    /// </remarks>
    public QuaternionF TargetOrientation
    {
      get { return _targetOrientation; }
      set
      {
        if (_targetOrientation != value)
        {
          _targetOrientation = value;
          OnChanged();
        }
      }
    }
    private QuaternionF _targetOrientation = QuaternionF.Identity;



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
      float errorReduction = ConstraintHelper.ComputeErrorReduction(deltaTime, SpringConstant, DampingConstant);
      float softness = ConstraintHelper.ComputeSoftness(deltaTime, SpringConstant, DampingConstant);

      // Get anchor orientations in world space.
      Matrix33F anchorOrientationA = BodyA.Pose.Orientation * AnchorOrientationALocal;
      Matrix33F anchorOrientationB = BodyB.Pose.Orientation * AnchorOrientationBLocal;

      Matrix33F relativeOrientationMatrix = anchorOrientationA.Transposed * anchorOrientationB;
      QuaternionF relativeOrientation = QuaternionF.CreateRotation(relativeOrientationMatrix);
      QuaternionF deltaRotation = TargetOrientation * relativeOrientation.Conjugated;

      float angle = deltaRotation.Angle;
      if (angle > ConstantsF.Pi)
      {
        // Quaternion should be the shortest arc quaternion (angle < 180°).
        deltaRotation = -deltaRotation;
        angle = ConstantsF.TwoPi - angle;
        Debug.Assert(Numeric.AreEqual(angle, deltaRotation.Angle));
      }

      Vector3F axis = new Vector3F(deltaRotation.X, deltaRotation.Y, deltaRotation.Z);
      if (!axis.TryNormalize())
      {
        // Do nothing.
        _minImpulseLimits[0] = 0;
        _minImpulseLimits[1] = 0;
        _minImpulseLimits[2] = 0;
      }
      else
      {
        // Axis is in local space of anchor A.
        // Convert axis to world space.
        axis = anchorOrientationA * axis;

        // Main axis is the quaternion axis.
        bool isActive = !Numeric.IsZero(angle);
        SetupConstraint(0, -angle, 0, axis, deltaTime, errorReduction, softness, isActive);
        if (!UseSingleAxisMode)
        {
          // In multi-axes-mode mode: constrain rotation on 2 orthogonal axes.
          SetupConstraint(1, 0, 0, axis.Orthonormal1, deltaTime, errorReduction, softness, isActive);
          SetupConstraint(2, 0, 0, axis.Orthonormal2, deltaTime, errorReduction, softness, isActive);
        }
      }

      // No warmstarting.
      _constraints[0].ConstraintImpulse = 0;
      _constraints[1].ConstraintImpulse = 0;
      _constraints[2].ConstraintImpulse = 0;
    }


    private void SetupConstraint(int index, float angle, float targetAngle, Vector3F axis,
                                 float deltaTime, float errorReduction, float softness, bool isActive)
    {
      // Note: Cached constraint impulses are reset in Warmstart() if necessary.

      Constraint1D constraint = _constraints[index];
      Simulation simulation = Simulation;

      if (!isActive)
      {
        _minImpulseLimits[index] = 0;
        _maxImpulseLimits[index] = 0;
        constraint.ConstraintImpulse = 0;
        return;
      }

      // ----- Error correction
      float deviation = targetAngle - angle;
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

      // Note: Softness must be set before!
      constraint.Softness = softness / deltaTime;
      constraint.Prepare(BodyA, BodyB, Vector3F.Zero, -axis, Vector3F.Zero, axis);
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
