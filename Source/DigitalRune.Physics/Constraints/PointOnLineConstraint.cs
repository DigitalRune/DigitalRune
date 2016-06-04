// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint that forces one body to move on a line that is fixed on the other body.
  /// </summary>
  /// <remarks>
  /// A line axis is fixed on <see cref="Constraint.BodyA"/>. The line goes through the point
  /// <see cref="AnchorPositionALocal"/> on the first body and the direction of the line is defined
  /// by <see cref="AxisALocal"/>. <see cref="AnchorPositionBLocal"/> on 
  /// <see cref="Constraint.BodyB"/> is constrained to be on the line on the first body. This joint
  /// removes 2 translational degrees of movement (only relative movement on the line is allowed).
  /// It does not restrict rotational movement.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
  public class PointOnLineConstraint : Constraint
  {
    // DOF: 1T, 3R

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LinearLimit _linearLimit;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the line axis that is fixed on <see cref="Constraint.BodyA"/> in local space
    /// of <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The line axis on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is a zero vector.
    /// </exception>
    public Vector3F AxisALocal
    {
      get { return _linearLimit.AnchorPoseALocal.Orientation.GetColumn(0); }
      set
      {
        if (value != AxisALocal)
        {
          if (!value.TryNormalize())
            throw new ArgumentException("Line axis vector must not be a zero vector.");

          // The free line axis is x-axis.
          Pose anchorPoseALocal = new Pose(AnchorPositionALocal);
          anchorPoseALocal.Orientation.SetColumn(0, value);
          anchorPoseALocal.Orientation.SetColumn(1, value.Orthonormal1);
          anchorPoseALocal.Orientation.SetColumn(2, value.Orthonormal2);
          _linearLimit.AnchorPoseALocal = anchorPoseALocal;

          OnChanged();
        }
      }
    }


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
      get { return _linearLimit.AnchorPoseALocal.Position; }
      set
      {
        if (value != AnchorPositionALocal)
        {
          _linearLimit.AnchorPoseALocal = new Pose(value, _linearLimit.AnchorPoseALocal.Orientation);
          OnChanged();
        }
      }
    }


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
      get { return _linearLimit.AnchorPositionBLocal; }
      set
      {
        if (value != AnchorPositionBLocal)
        {
          _linearLimit.AnchorPositionBLocal = value;
          OnChanged();
        }
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
      get { return _linearLimit.ErrorReduction.X; }
      set
      {
        if (value != ErrorReduction)
        {
          _linearLimit.ErrorReduction = new Vector3F(value);
          OnChanged();
        }
      }
    }


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
      get { return _linearLimit.Softness.X; }
      set
      {
        if (value != Softness)
        {
          _linearLimit.Softness = new Vector3F(value);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the minimum movement limit on the line.
    /// </summary>
    /// <value>
    /// The minimum movement limit. The default is -∞, which means that there is no minimum limit.
    /// </value>
    public float Minimum
    {
      get { return _linearLimit.Minimum.X; }
      set
      {
        var oldMin = _linearLimit.Minimum;
        if (value != oldMin.X)
        {
          _linearLimit.Minimum = new Vector3F(value, oldMin.Y, oldMin.Z);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum movement limit on the line.
    /// </summary>
    /// <value>
    /// The maximum movement limit. The default is +∞, which means that there is no maximum limit.
    /// </value>
    public float Maximum
    {
      get { return _linearLimit.Maximum.X; }
      set
      {
        var oldMax = _linearLimit.Maximum;
        if (value != oldMax.X)
        {
          _linearLimit.Maximum = new Vector3F(value, oldMax.Y, oldMax.Z);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the coefficient of restitution for limits.
    /// </summary>
    /// <value>The coefficient of restitution.</value>
    /// <remarks>
    /// If the bodies reach a limit on the line axis (<see cref="Minimum"/> or 
    /// <see cref="Maximum"/>), the bodies will bounce back. If this property is 0, there will be
    /// no bounce. If this property is 1, the whole velocity on the line axis is reflected.
    /// </remarks>
    public float Restitution
    {
      get { return _linearLimit.Restitution.X; }
      set
      {
        if (value != Restitution)
        {
          _linearLimit.Restitution = new Vector3F(value);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximal force that is applied by this constraint.
    /// </summary>
    /// <value>
    /// The maximal force. The default value is +∞.
    /// </value>
    /// <remarks>
    /// This property defines the maximal force that can be apply to keep the constraint satisfied. 
    /// </remarks>
    public float MaxForce
    {
      get { return _linearLimit.MaxForce.X; }
      set
      {
        if (value != Restitution)
        {
          _linearLimit.MaxForce = new Vector3F(value);
          OnChanged();
        }
      }
    }


    /// <inheritdoc/>
    public override Vector3F LinearConstraintImpulse
    {
      get
      {
        return _linearLimit.LinearConstraintImpulse;
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
    /// Gets the position on the line axis relative to <see cref="AnchorPositionALocal"/>.
    /// </summary>
    /// <value>The position on the line axis relative to <see cref="AnchorPositionALocal"/>.</value>
    public float RelativePosition
    {
      get { return _linearLimit.RelativePosition.X; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PointOnLineConstraint"/> class.
    /// </summary>
    public PointOnLineConstraint()
    {
      _linearLimit = new LinearLimit
      {
        Minimum = new Vector3F(float.NegativeInfinity, 0, 0),
        Maximum = new Vector3F(float.PositiveInfinity, 0, 0),
      };
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnAddToSimulation()
    {
      _linearLimit.Simulation = Simulation;
      base.OnAddToSimulation();
    }


    /// <inheritdoc/>
    protected override void OnRemoveFromSimulation()
    {
      _linearLimit.Simulation = null;
      base.OnRemoveFromSimulation();
    }


    /// <inheritdoc/>
    protected override void OnSetup()
    {
      _linearLimit.Setup();
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      return _linearLimit.ApplyImpulse();
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // In case the bodies where changed:
      _linearLimit.BodyA = BodyA;
      _linearLimit.BodyB = BodyB;

      base.OnChanged();
    }
    #endregion
  }
}
