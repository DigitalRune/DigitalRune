// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint that fixes the relative orientation of two bodies.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This joint removes all 3 rotational degrees of freedom. It forces the constraint anchor
  /// orientations to be equal. The translational movement is not restricted and both bodies can
  /// rotate relative to other bodies as long as both constraint bodies perform the same rotation.
  /// </para>
  /// <para>
  /// On its own this constraint is not very useful, but combined with constraints that restrict
  /// translational movement it creates practical joints. For example, if this constraint is
  /// combined with a <see cref="PointOnLineConstraint"/>, it creates a point-on-line constraint
  /// with fixed relative orientations.
  /// </para>
  /// <para>
  /// One constraint body can be set to the simulation <see cref="Simulation.World"/> to create a
  /// body that cannot rotate. But in this case it is more efficient and more stable to lock the
  /// rotations of the body using <see cref="RigidBody"/> properties 
  /// (<see cref="RigidBody.LockRotationX"/>, <see cref="RigidBody.LockRotationY"/> and 
  /// <see cref="RigidBody.LockRotationZ"/>).
  /// </para>
  /// </remarks>
  public class NoRotationConstraint : Constraint
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly AngularLimit _angularLimit;
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
      get { return _angularLimit.AnchorOrientationALocal; }
      set
      {
        if (value != AnchorOrientationALocal)
        {
          _angularLimit.AnchorOrientationALocal = value;
          OnChanged();
        }
      }
    }


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
      get { return _angularLimit.AnchorOrientationBLocal; }
      set
      {
        if (value != AnchorOrientationBLocal)
        {
          _angularLimit.AnchorOrientationBLocal = value;
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
      get { return _angularLimit.ErrorReduction.X; }
      set
      {
        if (value != ErrorReduction)
        {
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
      get { return _angularLimit.Softness.X; }
      set
      {
        if (value != Softness)
        {
          _angularLimit.Softness = new Vector3F(value);
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
      get { return _angularLimit.MaxForce.X; }
      set
      {
        if (value != MaxForce)
        {
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
        return Vector3F.Zero;
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
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="NoRotationConstraint"/> class.
    /// </summary>
    public NoRotationConstraint()
    {
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
      _angularLimit.Simulation = Simulation;
      base.OnAddToSimulation();
    }


    /// <inheritdoc/>
    protected override void OnRemoveFromSimulation()
    {
      _angularLimit.Simulation = null;
      base.OnRemoveFromSimulation();
    }


    /// <inheritdoc/>
    protected override void OnSetup()
    {
      _angularLimit.Setup();
    }


    /// <inheritdoc/>
    protected override bool OnApplyImpulse()
    {
      return _angularLimit.ApplyImpulse();
    }


    /// <inheritdoc/>
    protected override void OnChanged()
    {
      // In case the bodies where changed:
      _angularLimit.BodyA = BodyA;
      _angularLimit.BodyB = BodyB;

      base.OnChanged();
    }
    #endregion
  }
}
