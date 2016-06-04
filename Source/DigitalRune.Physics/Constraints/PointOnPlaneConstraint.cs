// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Restricts a body to move in a plane that is fixed on another body.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A plane is fixed on <see cref="Constraint.BodyA"/>. The plane goes through
  /// <see cref="AnchorPositionALocal"/> and is defined by the two axes <see cref="XAxisALocal"/> 
  /// and <see cref="YAxisALocal"/> that are also fixed on the first body. The point 
  /// <see cref="AnchorPositionBLocal"/> on the second body can only move in this plane.
  /// </para>
  /// <para>
  /// This constraint removes 1 translational degree of movement (the movement normal to the plane).
  /// It does not restrict rotational movement.
  /// </para>
  /// </remarks>
  public class PointOnPlaneConstraint : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LinearLimit _linearLimit;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the x-axis in the plane that is fixed on <see cref="Constraint.BodyA"/> in 
    /// local space of <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The x-axis in the plane on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>. <see cref="XAxisALocal"/> must be perpendicular to 
    /// <see cref="YAxisALocal"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is a zero vector.
    /// </exception>
    public Vector3F XAxisALocal
    {
      get { return _linearLimit.AnchorPoseALocal.Orientation.GetColumn(0); }
      set
      {
        if (value != XAxisALocal)
        {
          if (!value.TryNormalize())
            throw new ArgumentException("Axis vector must not be a zero vector.");

          // The free plane axes are the x- and y-axes.
          Pose anchorPoseALocal = new Pose(AnchorPositionALocal);
          anchorPoseALocal.Orientation.SetColumn(0, value);
          anchorPoseALocal.Orientation.SetColumn(1, YAxisALocal);
          anchorPoseALocal.Orientation.SetColumn(2, Vector3F.Cross(value, YAxisALocal));
          _linearLimit.AnchorPoseALocal = anchorPoseALocal;

          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the y-axis in the plane that is fixed on <see cref="Constraint.BodyA"/> in 
    /// local space of <see cref="Constraint.BodyA"/>.
    /// </summary>
    /// <value>
    /// The y-axis in the plane on <see cref="Constraint.BodyA"/> in local space of 
    /// <see cref="Constraint.BodyA"/>. <see cref="YAxisALocal"/> must be perpendicular to 
    /// <see cref="XAxisALocal"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is a zero vector.
    /// </exception>
    public Vector3F YAxisALocal
    {
      get { return _linearLimit.AnchorPoseALocal.Orientation.GetColumn(1); }
      set
      {
        if (value != YAxisALocal)
        {
          if (!value.TryNormalize())
            throw new ArgumentException("Axis vector must not be a zero vector.");

          // The free plane axes are the x- and y-axes.
          Pose anchorPoseALocal = new Pose(AnchorPositionALocal);
          anchorPoseALocal.Orientation.SetColumn(0, XAxisALocal);
          anchorPoseALocal.Orientation.SetColumn(1, value);
          anchorPoseALocal.Orientation.SetColumn(2, Vector3F.Cross(XAxisALocal, value));
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
    /// Gets or sets the minimum movement limits on the plane.
    /// </summary>
    /// <value>
    /// The minimum movement limits on the plane. The first element is the minimum limit in the
    /// plane x-axis direction. The second element is the minimum in the plane y-axis direction. The
    /// default is (-∞, -∞), which means that there is no minimum limit.
    /// </value>
    public Vector2F Minimum
    {
      get { return new Vector2F(_linearLimit.Minimum.X, _linearLimit.Minimum.Y); }
      set
      {
        var oldMin = _linearLimit.Minimum;
        if (value.X != oldMin.X && value.Y != oldMin.Y)
        {
          _linearLimit.Minimum = new Vector3F(value.X, value.Y, 0);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum movement limits on the plane.
    /// </summary>
    /// <value>
    /// The maximum movement limits on the plane. The first element is the maximum limit in the
    /// plane x-axis direction. The second element is the maximum in the plane y-axis direction. The
    /// default is (+∞, +∞), which means that there is no maximum limit.
    /// </value>
    public Vector2F Maximum
    {
      get { return new Vector2F(_linearLimit.Maximum.X, _linearLimit.Maximum.Y); }
      set
      {
        var oldMax = _linearLimit.Maximum;
        if (value.X != oldMax.X && value.Y != oldMax.Y)
        {
          _linearLimit.Maximum = new Vector3F(value.X, value.Y, 0);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the coefficient of restitution for limits.
    /// </summary>
    /// <value>The coefficient of restitution.</value>
    /// <remarks>
    /// If the bodies reach a limit on the plane (<see cref="Minimum"/> or <see cref="Maximum"/>),
    /// the bodies will bounce back. If this property is 0, there will be no bounce. If this
    /// property is 1, the whole velocity parallel to the plane is reflected.
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
    /// <value>The maximal force. The default value is +∞.</value>
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
    /// Gets the position on the plane axes relative to <see cref="AnchorPositionALocal"/>.
    /// </summary>
    /// <value>
    /// The position on the plane axes relative to <see cref="AnchorPositionALocal"/>: 
    /// (relativePositionX, relativePositionY).
    /// </value>
    public Vector2F RelativePosition
    {
      get
      {
        Vector3F relativePosition = _linearLimit.RelativePosition;
        return new Vector2F(relativePosition.X, relativePosition.Y);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PointOnPlaneConstraint"/> class.
    /// </summary>
    public PointOnPlaneConstraint()
    {
      _linearLimit = new LinearLimit
      {
        Minimum = new Vector3F(float.NegativeInfinity, float.NegativeInfinity, 0),
        Maximum = new Vector3F(float.PositiveInfinity, float.PositiveInfinity, 0),
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
