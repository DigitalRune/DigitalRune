// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a prismatic slider joint.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This joint allows only translational movement along the slider axis. It removes all other
  /// translational and rotational degrees of freedom. The slider axis is the x-axis of the
  /// constraint anchor.
  /// </para>
  /// <para>
  /// See also <see href="http://en.wikipedia.org/wiki/Prismatic_joint"/>.
  /// </para>
  /// </remarks>
  public class PrismaticJoint : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly LinearLimit _linearLimit;
    private readonly AngularLimit _angularLimit;
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
      get { return _linearLimit.AnchorPoseALocal; }
      set
      {
        if (value != AnchorPoseALocal)
        {
          _linearLimit.AnchorPoseALocal = value;
          _angularLimit.AnchorOrientationALocal = value.Orientation;
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the constraint anchor pose on <see cref="Constraint.BodyB"/> in local space of 
    /// <see cref="Constraint.BodyB"/>.
    /// </summary>
    /// <value>
    /// The constraint anchor pose on <see cref="Constraint.BodyB"/> in local space of 
    /// <see cref="Constraint.BodyB"/>.
    /// </value>
    public Pose AnchorPoseBLocal
    {
      get { return new Pose(_linearLimit.AnchorPositionBLocal, _angularLimit.AnchorOrientationBLocal); }
      set
      {
        if (value != AnchorPoseBLocal)
        {
          _linearLimit.AnchorPositionBLocal = value.Position;
          _angularLimit.AnchorOrientationBLocal = value.Orientation;
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
          _angularLimit.ErrorReduction = new Vector3F(value);
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
          _angularLimit.Softness = new Vector3F(value);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the minimum movement limit on the slider axis.
    /// </summary>
    /// <value>
    /// The minimum movement limit. The default is -∞, which means that there is no minimum limit.
    /// </value>
    public float Minimum
    {
      get { return _linearLimit.Minimum.X; }
      set
      {
        if (value != Minimum)
        {
          _linearLimit.Minimum = new Vector3F(value, 0, 0);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum movement limit on the slider axis.
    /// </summary>
    /// <value>
    /// The maximum movement limit. The default is +∞, which means that there is no maximum limit.
    /// </value>
    public float Maximum
    {
      get { return _linearLimit.Maximum.X; }
      set
      {
        if (value != Maximum)
        {
          _linearLimit.Maximum = new Vector3F(value, 0, 0);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the coefficient of restitution for slider limits.
    /// </summary>
    /// <value>The coefficient of restitution.</value>
    /// <remarks>
    /// If the bodies reach a limit on the slider axis (<see cref="Minimum"/> or 
    /// <see cref="Maximum"/>), the bodies will bounce back. If this property is 0, there will be
    /// no bounce. If this property is 1, the whole velocity on the slider axis is reflected.
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
          _angularLimit.MaxForce = new Vector3F(value);
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
        return _angularLimit.AngularConstraintImpulse;
      }
    }


    /// <summary>
    /// Gets the relative position on the slider axis.
    /// </summary>
    /// <value>The relative position on the slider.</value>
    public float RelativePosition
    {
      get { return _linearLimit.RelativePosition.X; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PrismaticJoint"/> class.
    /// </summary>
    public PrismaticJoint()
    {
      _linearLimit = new LinearLimit
      {
        Minimum = new Vector3F(float.NegativeInfinity, 0, 0),
        Maximum = new Vector3F(float.PositiveInfinity, 0, 0),
      };
      _angularLimit = new AngularLimit
      {
        Minimum = new Vector3F(0, 0, 0),
        Maximum = new Vector3F(0, 0, 0),
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
      _angularLimit.Simulation = Simulation;
      base.OnAddToSimulation();
    }


    /// <inheritdoc/>
    protected override void OnRemoveFromSimulation()
    {
      _linearLimit.Simulation = null;
      _angularLimit.Simulation = null;
      base.OnRemoveFromSimulation();
    }


    /// <inheritdoc/>
    protected override void OnSetup()
    {
      _linearLimit.Setup();
      _angularLimit.Setup();
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      bool result = _linearLimit.ApplyImpulse();
      result = _angularLimit.ApplyImpulse() || result;
      return result;
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // In case the bodies where changed:
      _linearLimit.BodyA = BodyA;
      _linearLimit.BodyB = BodyB;
      _angularLimit.BodyA = BodyA;
      _angularLimit.BodyB = BodyB;

      base.OnChanged();
    }
    #endregion
  }
}
