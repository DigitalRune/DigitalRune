// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a joint that allows rotations on two axis. The joint can be used to model the front
  /// wheel of a car.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This joint is best understood by an example: It models the front wheel of a car. The front 
  /// wheel rotates freely around one axis (rolling axis). It can also rotate about an axis that is 
  /// normal to the wheel axis (steering axis). The second rotation is controlled by the steering 
  /// wheel and is used to steer the car. Per convention the steering axis is the x-axis of the
  /// constraint anchors and the rolling axis is the z-axis. The steering and rolling angles can be 
  /// limited with <see cref="Minimum"/> and <see cref="Maximum"/>.
  /// </para>
  /// </remarks>
  public class Hinge2Joint : Constraint
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
    /// Gets or sets the minimum rotation limits about the two rotation axis.
    /// </summary>
    /// <value>
    /// The minimum rotation limits in radians. The first element is the minimum angle about the
    /// constraint anchor x-axis. The second element is the minimum angle about the constraint
    /// anchor z-axis. The default is (-π / 4, -∞).
    /// </value>
    public Vector2F Minimum
    {
      get { return new Vector2F(_angularLimit.Minimum.X, _angularLimit.Minimum.Z); }
      set
      {
        if (value != Minimum)
        {
          _angularLimit.Minimum = new Vector3F(value.X, 0, value.Y);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the maximum rotation limits about the two rotation axis.
    /// </summary>
    /// <value>
    /// The maximum rotation limits in radians. The first element is the maximum angle about the
    /// constraint anchor x-axis. The second element is the maximum angle about the constraint
    /// anchor z-axis. The default is (+π / 4, +∞).
    /// </value>
    public Vector2F Maximum
    {
      get { return new Vector2F(_angularLimit.Maximum.X, _angularLimit.Maximum.Z); }
      set
      {
        if (value != Maximum)
        {
          _angularLimit.Maximum = new Vector3F(value.X, 0, value.Y);
          OnChanged();
        }
      }
    }


    /// <summary>
    /// Gets or sets the coefficient of restitution.
    /// </summary>
    /// <value>The coefficient of restitution.</value>
    /// <remarks>
    /// If the bodies reach a limit (see <see cref="Minimum"/> or <see cref="Maximum"/>), the bodies 
    /// will bounce back. If this property is 0, there will be no bounce. If this property is 1, the 
    /// whole velocity is reflected.
    /// </remarks>
    public float Restitution
    {
      get { return _angularLimit.Restitution.X; }
      set
      {
        if (value != Restitution)
        {
          _angularLimit.Restitution = new Vector3F(value);
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


    //public float RelativePosition
    //{
    //  get { return _angularLimit.RelativePosition.X; }
    //}
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Hinge2Joint"/> class.
    /// </summary>
    public Hinge2Joint()
    {
      _linearLimit = new LinearLimit
      {
        Minimum = new Vector3F(0, 0, 0),
        Maximum = new Vector3F(0, 0, 0),
      };
      _angularLimit = new AngularLimit
      {
        Minimum = new Vector3F(-ConstantsF.PiOver4, 0, float.NegativeInfinity),
        Maximum = new Vector3F(ConstantsF.PiOver4, 0, float.PositiveInfinity),
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
