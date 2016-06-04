// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Defines a constraint at a rigid body contact that models non-penetration, dry friction and
  /// bounciness.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The simulation automatically creates <see cref="ContactConstraint"/>s for all contacts that
  /// are found by the collision detection and where collision response is enabled.
  /// </para>
  /// <para>
  /// The property <see cref="Geometry.Collisions.Contact"/>.<see cref="Geometry.Collisions.Contact.UserData"/>
  /// is used by the simulation to store references to the <see cref="ContactConstraint"/> instances.
  /// Therefore, the <see cref="Geometry.Collisions.Contact.UserData"/> property of contacts between 
  /// rigid bodies must not be changed. In other words, the relationship between contacts and
  /// contact constraints is:
  /// <code>
  /// myContactConstraint.Contact == myContact
  /// myContact.UserData == myContactConstraint
  /// </code>
  /// </para>
  /// </remarks>
  public sealed class ContactConstraint : IConstraint
  {
    // Notes:
    // We use 1 contact constraint for each Contact. Instead, we could use one contact constraint 
    // for each ContactSet and collect all contact constraints for a body pair in one constraint
    // class. This could speed up island creation, and similar tasks where all contact
    // constraints are enumerated and only the body pairs are checked.
    //
    // Relative velocity:
    // RelativeVelocity < 0 ... Objects are getting closer.
    // RelativeVelocity > 0 ... Objects are separating.
    //
    // Garbage:
    // Contacts in the collision detection do not use resource pooling and create garbage.
    // ContactConstraints are created for each Contact and stored in Contact.UserData. 
    // ContactConstraints do not use resource pooling because we do not know whether a user keeps
    // a reference to a ContactConstraint, or whether a contact has been destroyed.
    // Since Contacts and ContactConstraints are persistent over multiple frames this should not 
    // be a problem and leads to a much cleaner API design.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<ContactConstraint> Pool =
      new ResourcePool<ContactConstraint>(
        () => new ContactConstraint(),
        null,
        null);

    // Offset vector from center of mass to position on A/B in world space.
    private Vector3F _rA;
    private float _rALengthSquared;
    private Vector3F _rB;
    private float _rBLengthSquared;
    private Vector3F _normal;             // Cached normal to avoid accessing Contact.Normal.

    // First friction constraint direction (tangent normal to the contact normal).
    private Vector3F _t0;
    // Second friction constraint direction (tangent normal to _t0 and the contact normal).
    private Vector3F _t1;

    private float _restitution;
    private float _staticFriction;
    private float _dynamicFriction;

    private readonly Constraint1D _penetrationConstraint = new Constraint1D();
    private readonly Constraint1D _frictionConstraint0 = new Constraint1D();
    private readonly Constraint1D _frictionConstraint1 = new Constraint1D();

    // Target velocity and total constraint impulse for Split Impulses.
    private float _splitImpulseTargetVelocity;
    private float _splitImpulse;

    private bool _surfaceMotionAEnabled;
    private bool _surfaceMotionBEnabled;

    private float _minConstraintImpulse;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public RigidBody BodyA
    {
      get { return _bodyA; }
    }
    private RigidBody _bodyA;


    /// <inheritdoc/>
    public RigidBody BodyB
    {
      get { return _bodyB; }
    }
    private RigidBody _bodyB;


    /// <inheritdoc/>
    bool IConstraint.Enabled
    {
      get { return true; }
    }


    /// <inheritdoc/>
    bool IConstraint.CollisionEnabled
    {
      get { return true; }
    }


    /// <inheritdoc/>
    public Simulation Simulation
    {
      get { return _simulation; }
    }
    private Simulation _simulation;


    /// <summary>
    /// Gets the contact.
    /// </summary>
    /// <value>The contact.</value>
    public Contact Contact { get; private set; }


    /// <summary>
    /// Gets or sets the error reduction parameter.
    /// </summary>
    /// <value>
    /// The error reduction parameter in the range [0, 1]. The default value is 
    /// <see cref="ConstraintSettings.ContactErrorReduction"/>.
    /// </value>
    /// <remarks>
    /// The error reduction parameter is a value between 0 and 1. It defines how fast a constraint
    /// error, in this case rigid body interpenetration, is removed. If the error reduction
    /// parameter is 0, constraint errors are not removed. If the value is 1 the simulation tries to
    /// remove the whole constraint error in one time step - which is usually unstable. A good value
    /// is for example 0.3.
    /// </remarks>
    public float ErrorReduction { get; private set; }


    /// <summary>
    /// Gets the relative velocity of the contact points in world space (including surface motion).
    /// </summary>
    /// <value>The relative velocity in world space.</value>
    internal Vector3F RelativeVelocity
    {
      get
      {
        Vector3F velA = BodyA._linearVelocity + Vector3F.Cross(BodyA._angularVelocity, _rA);
        Vector3F velB = BodyB._linearVelocity + Vector3F.Cross(BodyB._angularVelocity, _rB);
        Vector3F vRel = velB - velA;

        if (_surfaceMotionAEnabled || _surfaceMotionBEnabled)
        {
          // TODO: For performance reasons we could use a different ContactConstraint class for contacts with surface velocities.
          if (_surfaceMotionAEnabled)
          {
            var materialA = BodyA.Material.GetProperties(BodyA, Contact.PositionALocal, Contact.FeatureA);
            var surfaceVelocityA = BodyA.Pose.ToWorldDirection(materialA.SurfaceMotion);

            // Subtract motion in direction of normal vector.
            surfaceVelocityA -= Vector3F.Dot(surfaceVelocityA, Contact.Normal) * Contact.Normal;

            vRel -= surfaceVelocityA;
          }
          if (_surfaceMotionBEnabled)
          {
            var materialB = BodyB.Material.GetProperties(BodyB, Contact.PositionBLocal, Contact.FeatureB);
            var surfaceVelocityB = BodyB.Pose.ToWorldDirection(materialB.SurfaceMotion);

            // Subtract motion in direction of normal vector.
            surfaceVelocityB -= Vector3F.Dot(surfaceVelocityB, Contact.Normal) * Contact.Normal;

            vRel += surfaceVelocityB;
          }
        }

        return vRel;
      }
    }


    /// <summary>
    /// Gets the relative velocity in normal direction.
    /// </summary>
    /// <value>The relative normal velocity.</value>
    internal float RelativeNormalVelocity
    {
      // For reference. 
      // (This property is not used. Instead the code is inlined in the methods below.)
      get { return Vector3F.Dot(RelativeVelocity, Contact.Normal); }
    }


    ///// <summary>
    ///// Gets the normal impulse that was applied. 
    ///// </summary>
    ///// <value>The normal impulse.</value>
    ///// <remarks>
    ///// The impulse was applied in normal direction on <see cref="BodyB"/>. An equivalent negative 
    ///// impulse was applied on <see cref="BodyA"/>.
    ///// </remarks>
    //public float NormalImpulse
    //{
    //  get { return _penetrationConstraint.ConstraintImpulse; }
    //}


    /// <inheritdoc/>
    public Vector3F LinearConstraintImpulse
    {
      get
      {
        return _penetrationConstraint.ConstraintImpulse * _penetrationConstraint.JLinB
              + _frictionConstraint0.ConstraintImpulse * _frictionConstraint0.JLinB
              + _frictionConstraint1.ConstraintImpulse * _frictionConstraint1.JLinB;
      }
    }


    /// <inheritdoc/>
    Vector3F IConstraint.AngularConstraintImpulse
    {
      get
      {
        return Vector3F.Zero;
      }
    }


    ///// <summary>
    ///// Gets or sets the user data.
    ///// </summary>
    ///// <value>The user data.</value>
    ///// <remarks>
    ///// <para>
    ///// This property can store end-user data. This property is not used by the physics simulation.
    ///// </para>
    ///// </remarks>
    //public object UserData { get; set; }


    /// <summary>
    /// Gets or sets the stacking tolerance.
    /// </summary>
    /// <value>The stacking tolerance.</value>
    /// <remarks>
    /// See also <see cref="ConstraintSettings.StackingTolerance"/>. This is the basically the
    /// same value but is set by the constraint solver. If there a are very little objects in the
    /// island we do not use the stacking tolerance because it can create a torque for rolling 
    /// objects like spheres, cylinders and cones.
    /// </remarks>
    internal float StackingTolerance { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ContactConstraint"/> is still in 
    /// used. (Used in Simulation.UpdateContacts.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if used; otherwise, <see langword="false"/>.
    /// </value>
    internal bool Used { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactConstraint"/> class.
    /// </summary>
    private ContactConstraint()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="ContactConstraint"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="bodyA">The first body.</param>
    /// <param name="bodyB">The second body.</param>
    /// <param name="contact">The contact.</param>
    /// <returns>A new or reusable instance of the <see cref="ContactConstraint"/> class.</returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    internal static ContactConstraint Create(RigidBody bodyA, RigidBody bodyB, Contact contact)
    {
      var contactConstraint = Pool.Obtain();
      contactConstraint.Initialize(bodyA, bodyB, contact);
      return contactConstraint;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="ContactConstraint"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    internal void Recycle()
    {
      Uninitialize();
      Pool.Recycle(this);
    }


    private void Initialize(RigidBody bodyA, RigidBody bodyB, Contact contact)
    {
      Debug.Assert(bodyA != null && bodyB != null && contact != null);
      _bodyA = bodyA;
      _bodyB = bodyB;
      Contact = contact;

      _simulation = _bodyA.Simulation ?? _bodyB.Simulation;
      Debug.Assert(_simulation != null);

      ErrorReduction = _simulation.Settings.Constraints.ContactErrorReduction;
      _minConstraintImpulse = _simulation.Settings.Constraints.MinConstraintImpulse;

      Vector3F n = Contact.Normal;
      _rA = Contact.PositionAWorld - BodyA.PoseCenterOfMass.Position;
      _rB = Contact.PositionBWorld - BodyB.PoseCenterOfMass.Position;
      _rALengthSquared = -1;
      _rBLengthSquared = -1;

      Vector3F vRel = RelativeVelocity;
      float vRelN = Vector3F.Dot(vRel, n);

      // Get material-dependent values.
      var materialA = BodyA.Material.GetProperties(BodyA, Contact.PositionALocal, Contact.FeatureA);
      var materialB = BodyB.Material.GetProperties(BodyB, Contact.PositionBLocal, Contact.FeatureB);
      var materialCombiner = _simulation.Settings.MaterialPropertyCombiner;

      // Restitution is clamped to 0 if bodies are slow or if restitution is very low.
      if (Math.Abs(vRelN) > _simulation.Settings.Constraints.RestingVelocityLimit)
      {
        _restitution = materialCombiner.CombineRestitution(materialA.Restitution, materialB.Restitution);
        if (_restitution < _simulation.Settings.Constraints.RestitutionThreshold)
          _restitution = 0;
      }
      else
      {
        // The contacts must be created BEFORE forces are applied! If the bodies are moving less
        // than the RestingVelocityLimit, we set the Restitution to zero. 
        // Bodies that do not move relative to each other must have a restitution of 0 to get
        // stable stacks!
        // RestingVelocityLimit can even be zero! 
        _restitution = 0;
      }

      _staticFriction = materialCombiner.CombineFriction(materialA.StaticFriction, materialB.StaticFriction);
      _dynamicFriction = materialCombiner.CombineFriction(materialA.DynamicFriction, materialB.DynamicFriction);
      _surfaceMotionAEnabled = materialA.SupportsSurfaceMotion;
      _surfaceMotionBEnabled = materialB.SupportsSurfaceMotion;

      // Get friction directions. The first direction is in direction of the greatest tangent 
      // velocity.
      _t0 = vRel - vRelN * n;
      if (!_t0.TryNormalize())
        _t0 = n.Orthonormal1;

      _t1 = Vector3F.Cross(_t0, n);

      // Reset cached impulses.
      _penetrationConstraint.ConstraintImpulse = 0;
      _frictionConstraint0.ConstraintImpulse = 0;
      _frictionConstraint1.ConstraintImpulse = 0;
    }


    private void Uninitialize()
    {
      _simulation = null;
      _bodyA = null;
      _bodyB = null;
      Contact = null;
      Used = false;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Setup()
    {
      // Cache normal. Access to Contact.Normal is expensive.
      _normal = Contact.Normal;

      float tolerance = StackingTolerance;

      if (_rALengthSquared == -1)
      {
        // This is the first setup call and we must initialize the constraints.
        _penetrationConstraint.Prepare(BodyA, BodyB, -_normal, -Vector3F.Cross(_rA, _normal), _normal, Vector3F.Cross(_rB, _normal));
        _frictionConstraint0.Prepare(BodyA, BodyB, -_t0, -Vector3F.Cross(_rA, _t0), _t0, Vector3F.Cross(_rB, _t0));
        _frictionConstraint1.Prepare(BodyA, BodyB, -_t1, -Vector3F.Cross(_rA, _t1), _t1, Vector3F.Cross(_rB, _t1));

        _rALengthSquared = _rA.LengthSquared;
        _rBLengthSquared = _rB.LengthSquared;
      }
      else if (tolerance == 0)
      {
        // Update _ra/_rb and related values. This creates smooth movement but less stable stacks.
        _rA = Contact.PositionAWorld - BodyA.PoseCenterOfMass.Position;
        _rB = Contact.PositionBWorld - BodyB.PoseCenterOfMass.Position;

        _penetrationConstraint.Prepare(BodyA, BodyB, -_normal, -Vector3F.Cross(_rA, _normal), _normal, Vector3F.Cross(_rB, _normal));
        _frictionConstraint0.Prepare(BodyA, BodyB, -_t0, -Vector3F.Cross(_rA, _t0), _t0, Vector3F.Cross(_rB, _t0));
        _frictionConstraint1.Prepare(BodyA, BodyB, -_t1, -Vector3F.Cross(_rA, _t1), _t1, Vector3F.Cross(_rB, _t1));
      }
      else if (tolerance < 1)
      {
        // Update _ra/_rb and related values only if the angle has changed more than the tolerance.
        // This creates less smooth movement for sliding/rolling objects but stable stacks.
        tolerance = 1 - tolerance;   // Invert, so that 1 means no tolerance and 0 is full tolerance.
        var newRA = Contact.PositionAWorld - BodyA.PoseCenterOfMass.Position;
        var newRB = Contact.PositionBWorld - BodyB.PoseCenterOfMass.Position;
        //if (Vector3F.Dot(newRA, _rA) < tolerance * _rALengthSquared || Vector3F.Dot(newRB, _rB) < tolerance * _rBLengthSquared)
        if (newRA.X * _rA.X + newRA.Y * _rA.Y + newRA.Z * _rA.Z < tolerance * _rALengthSquared
            || newRB.X * _rB.X + newRB.Y * _rB.Y + newRB.Z * _rB.Z < tolerance * _rBLengthSquared)
        {
          _rA = newRA;
          _rB = newRB;

          _penetrationConstraint.Prepare(BodyA, BodyB, -_normal, -Vector3F.Cross(_rA, _normal), _normal, Vector3F.Cross(_rB, _normal));
          _frictionConstraint0.Prepare(BodyA, BodyB, -_t0, -Vector3F.Cross(_rA, _t0), _t0, Vector3F.Cross(_rB, _t0));
          _frictionConstraint1.Prepare(BodyA, BodyB, -_t1, -Vector3F.Cross(_rA, _t1), _t1, Vector3F.Cross(_rB, _t1));
        }
      }

      // Get relative velocity.
      Vector3F vRel;
      if (_surfaceMotionAEnabled || _surfaceMotionBEnabled)
      {
        vRel = RelativeVelocity;
      }
      else
      {
        //Vector3F velA = BodyA._linearVelocity + Vector3F.Cross(BodyA._angularVelocity, _rA);
        //Vector3F velB = BodyB._linearVelocity + Vector3F.Cross(BodyB._angularVelocity, _rB);
        //Vector3F vRel = velB - velA;

        Vector3F linearVelocityA = BodyA._linearVelocity;
        Vector3F linearVelocityB = BodyB._linearVelocity;
        Vector3F angularVelocityA = BodyA._angularVelocity;
        Vector3F angularVelocityB = BodyB._angularVelocity;

        float velAX = linearVelocityA.X + angularVelocityA.Y * _rA.Z - angularVelocityA.Z * _rA.Y;
        float velAY = linearVelocityA.Y - angularVelocityA.X * _rA.Z + angularVelocityA.Z * _rA.X;
        float velAZ = linearVelocityA.Z + angularVelocityA.X * _rA.Y - angularVelocityA.Y * _rA.X;
        float velBX = linearVelocityB.X + angularVelocityB.Y * _rB.Z - angularVelocityB.Z * _rB.Y;
        float velBY = linearVelocityB.Y - angularVelocityB.X * _rB.Z + angularVelocityB.Z * _rB.X;
        float velBZ = linearVelocityB.Z + angularVelocityB.X * _rB.Y - angularVelocityB.Y * _rB.X;
        vRel = new Vector3F(velBX - velAX, velBY - velAY, velBZ - velAZ);
      }

      // Compute target velocity for restitution.
      //float vRelN = Vector3F.Dot(vRel, n);
      //if (vRelN < 0)
      //  _penetrationConstraint.TargetRelativeVelocity = -_restitution * vRelN;   // Objects coming closer --> Bounce 
      //else
      //  _penetrationConstraint.TargetRelativeVelocity = 0;                       // Objects separating. Don't create bounce.

      if (_restitution > 0)
      {
        float vRelN = vRel.X * _normal.X + vRel.Y * _normal.Y + vRel.Z * _normal.Z;
        if (vRelN < 0)
          _penetrationConstraint.TargetRelativeVelocity = -_restitution * vRelN;   // Objects coming closer --> Bounce 
        else
          _penetrationConstraint.TargetRelativeVelocity = 0;                       // Objects separating. Don't create bounce.
      }
      else
      {
        _penetrationConstraint.TargetRelativeVelocity = 0; // Objects separating. Don't create bounce.
      }


      // Compute velocity for error reduction.
      _splitImpulse = 0;
      _splitImpulseTargetVelocity = 0;
      if (Contact.PenetrationDepth > _simulation.Settings.Constraints.AllowedPenetration)
      {
        float deviationLength = Contact.PenetrationDepth - _simulation.Settings.Constraints.AllowedPenetration;
        float deltaTime = _simulation.Settings.Timing.FixedTimeStep;
        float errorCorrectionVelocity = deviationLength / deltaTime * ErrorReduction;
        errorCorrectionVelocity = Math.Min(errorCorrectionVelocity, _simulation.Settings.Constraints.MaxPenetrationCorrectionVelocity);

        // Part of the error is corrected with split impulses. Part is corrected with Baumgarte.
        _splitImpulseTargetVelocity = (1 - _simulation.Settings.Constraints.BaumgarteRatio) * errorCorrectionVelocity;
        _penetrationConstraint.TargetRelativeVelocity = Math.Max(_simulation.Settings.Constraints.BaumgarteRatio * errorCorrectionVelocity, _penetrationConstraint.TargetRelativeVelocity);
      }

      // Warmstarting:
      // We warmstart penetration constraints.
      // No warmstarting for friction constraints.
      // In our experiments, warmstarting of friction constraints was not very helpful in stacks.
      _penetrationConstraint.Warmstart(BodyA, BodyB);

      _frictionConstraint0.ConstraintImpulse = 0;
      _frictionConstraint1.ConstraintImpulse = 0;

      // In this frame we apply the bounce. If the contact persists until the next
      // frame, we treat the contact as resting contact to avoid jiggle.
      _restitution = 0;
    }


    /// <inheritdoc/>
    public bool ApplyImpulse()
    {
      // Get relative velocity.
      Vector3F linearVelocityA = BodyA._linearVelocity;
      Vector3F angularVelocityA = BodyA._angularVelocity;
      Vector3F linearVelocityB = BodyB._linearVelocity;
      Vector3F angularVelocityB = BodyB._angularVelocity;

      Vector3F vRel;
      if (_surfaceMotionAEnabled || _surfaceMotionBEnabled)
      {
        vRel = RelativeVelocity;
      }
      else
      {
        float velAX = linearVelocityA.X + angularVelocityA.Y * _rA.Z - angularVelocityA.Z * _rA.Y;
        float velAY = linearVelocityA.Y - angularVelocityA.X * _rA.Z + angularVelocityA.Z * _rA.X;
        float velAZ = linearVelocityA.Z + angularVelocityA.X * _rA.Y - angularVelocityA.Y * _rA.X;
        float velBX = linearVelocityB.X + angularVelocityB.Y * _rB.Z - angularVelocityB.Z * _rB.Y;
        float velBY = linearVelocityB.Y - angularVelocityB.X * _rB.Z + angularVelocityB.Z * _rB.X;
        float velBZ = linearVelocityB.Z + angularVelocityB.X * _rB.Y - angularVelocityB.Y * _rB.X;
        vRel.X = velBX - velAX;
        vRel.Y = velBY - velAY;
        vRel.Z = velBZ - velAZ;
      }

      // Apply non-penetration impulse.
      //float impulse = _penetrationConstraint.SatisfyInequalityConstraint(BodyA, BodyB, Vector3F.Dot(vRel, Contact.Normal), 0);
      float relativeVelocity = vRel.X * _normal.X + vRel.Y * _normal.Y + vRel.Z * _normal.Z;
      float impulse = _penetrationConstraint.SatisfyContactConstraint(BodyA, BodyB, relativeVelocity);

      // Apply friction impulses.
      float normalImpulse = _penetrationConstraint.ConstraintImpulse;
      float staticFrictionLimit = _staticFriction * normalImpulse;
      //_frictionConstraint0.SatisfyFrictionConstraint(BodyA, BodyB, Vector3F.Dot(vRel, _t0), staticFrictionLimit, normalImpulse, _dynamicFriction);
      //_frictionConstraint1.SatisfyFrictionConstraint(BodyA, BodyB, Vector3F.Dot(vRel, _t1), staticFrictionLimit, normalImpulse, _dynamicFriction);
      float relativeVelocityT0 = vRel.X * _t0.X + vRel.Y * _t0.Y + vRel.Z * _t0.Z;
      float frictionImpulse0 = _frictionConstraint0.SatisfyFrictionConstraint(
        BodyA, BodyB, relativeVelocityT0, staticFrictionLimit, normalImpulse, _dynamicFriction);
      float relativeVelocityT1 = vRel.X * _t1.X + vRel.Y * _t1.Y + vRel.Z * _t1.Z;
      float frictionImpulse1 = _frictionConstraint1.SatisfyFrictionConstraint(
        BodyA, BodyB, relativeVelocityT1, staticFrictionLimit, normalImpulse, _dynamicFriction);

      _bodyA._linearVelocity.X = linearVelocityA.X
                                 + _penetrationConstraint.WJTLinA.X * impulse
                                 + _frictionConstraint0.WJTLinA.X * frictionImpulse0
                                 + _frictionConstraint1.WJTLinA.X * frictionImpulse1;
      _bodyA._linearVelocity.Y = linearVelocityA.Y
                                 + _penetrationConstraint.WJTLinA.Y * impulse
                                 + _frictionConstraint0.WJTLinA.Y * frictionImpulse0
                                 + _frictionConstraint1.WJTLinA.Y * frictionImpulse1;
      _bodyA._linearVelocity.Z = linearVelocityA.Z
                                 + _penetrationConstraint.WJTLinA.Z * impulse
                                 + _frictionConstraint0.WJTLinA.Z * frictionImpulse0
                                 + _frictionConstraint1.WJTLinA.Z * frictionImpulse1;
      _bodyA._angularVelocity.X = angularVelocityA.X
                                  + _penetrationConstraint.WJTAngA.X * impulse
                                  + _frictionConstraint0.WJTAngA.X * frictionImpulse0
                                  + _frictionConstraint1.WJTAngA.X * frictionImpulse1;
      _bodyA._angularVelocity.Y = angularVelocityA.Y
                                  + _penetrationConstraint.WJTAngA.Y * impulse
                                  + _frictionConstraint0.WJTAngA.Y * frictionImpulse0
                                  + _frictionConstraint1.WJTAngA.Y * frictionImpulse1;
      _bodyA._angularVelocity.Z = angularVelocityA.Z
                                  + _penetrationConstraint.WJTAngA.Z * impulse
                                  + _frictionConstraint0.WJTAngA.Z * frictionImpulse0
                                  + _frictionConstraint1.WJTAngA.Z * frictionImpulse1;
      _bodyB._linearVelocity.X = linearVelocityB.X
                                 + _penetrationConstraint.WJTLinB.X * impulse
                                 + _frictionConstraint0.WJTLinB.X * frictionImpulse0
                                 + _frictionConstraint1.WJTLinB.X * frictionImpulse1;
      _bodyB._linearVelocity.Y = linearVelocityB.Y
                                 + _penetrationConstraint.WJTLinB.Y * impulse
                                 + _frictionConstraint0.WJTLinB.Y * frictionImpulse0
                                 + _frictionConstraint1.WJTLinB.Y * frictionImpulse1;
      _bodyB._linearVelocity.Z = linearVelocityB.Z
                                 + _penetrationConstraint.WJTLinB.Z * impulse
                                 + _frictionConstraint0.WJTLinB.Z * frictionImpulse0
                                 + _frictionConstraint1.WJTLinB.Z * frictionImpulse1;
      _bodyB._angularVelocity.X = angularVelocityB.X
                                  + _penetrationConstraint.WJTAngB.X * impulse
                                  + _frictionConstraint0.WJTAngB.X * frictionImpulse0
                                  + _frictionConstraint1.WJTAngB.X * frictionImpulse1;
      _bodyB._angularVelocity.Y = angularVelocityB.Y
                                  + _penetrationConstraint.WJTAngB.Y * impulse
                                  + _frictionConstraint0.WJTAngB.Y * frictionImpulse0
                                  + _frictionConstraint1.WJTAngB.Y * frictionImpulse1;
      _bodyB._angularVelocity.Z = angularVelocityB.Z
                                  + _penetrationConstraint.WJTAngB.Z * impulse
                                  + _frictionConstraint0.WJTAngB.Z * frictionImpulse0
                                  + _frictionConstraint1.WJTAngB.Z * frictionImpulse1;

      // Apply error correction impulse (split impulse)
      if (_splitImpulseTargetVelocity != 0)
        _penetrationConstraint.CorrectErrors(BodyA, BodyB, _splitImpulseTargetVelocity, ref _splitImpulse);

      return Math.Abs(impulse) > _minConstraintImpulse;
    }
    #endregion
  }
}
