// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Materials;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Controls the movement of a game character.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The character is represented by an upright capsule. When the character moves, it will slide 
  /// along obstacles to create a smooth movement. The character can move on light slopes, but will 
  /// be stopped by steep slopes (see <see cref="SlopeLimit"/>). The character will automatically 
  /// step over small obstacles. 
  /// </para>
  /// <para>
  /// A single <see cref="RigidBody"/> (see property <see cref="Body"/>) is created for the
  /// character capsule and added to a <see cref="Simulation"/>, which is specified when the
  /// character controller is created. The <see cref="RigidBody"/> can be used to set the collision
  /// group and collision filtering. The <see cref="RigidBody"/> is automatically added to the 
  /// <see cref="Simulation"/> when the character controller is created and removed from the 
  /// <see cref="Simulation"/> when the character controller is disabled.
  /// </para>
  /// <para>
  /// To teleport the character to a new position, simply set the property <see cref="Position"/> 
  /// directly. 
  /// </para>
  /// <para>
  /// The character controller applies gravity itself and the <see cref="Body"/> should be excluded
  /// from global force effects like gravity and damping.
  /// </para>
  /// <para>
  /// To move the character <see cref="Move"/> must be called. This will set the velocity of the
  /// <see cref="Body"/>. The body will be moved by the simulation. Therefore, the position is only 
  /// final after the simulation was updated.
  /// </para>
  /// <para>
  /// General notes: In first person shooters character speeds up to 10 m/s are used. This is about
  /// twice as fast as normal human movement. For this high speed, the gravity is often set to a 
  /// higher than normal value, like 16 m/s², to account for this unnatural speed.
  /// </para>
  /// </remarks>
  public class DynamicCharacterController
  {
    // Notes:
    // This character is a very simple and efficient character controller. It is less stable
    // and has less features than the KinematicCharacterController, but should be sufficient for
    // most cases.
    // The character controller assumes that the positive y-axis is the up direction. A rigid body 
    // with a capsule shape is used to represent the character. The rotations of the capsule are 
    // locked to keep it upright. In each Move(), a linear velocity is applied so that the next 
    // Simulation.Update() moves the capsule in the desired direction.
    // 
    // Jumping using the DynamicCharacterController is less exact than when using the
    // KinematicCharacterController. The jump height varies depending on the time step. The 
    // DynamicCharacterController only sets the linear velocity of the rigid body and the physics 
    // simulation updates the velocity which results in an Euler integration step:
    //   v' = v + g∙t
    //   s' = s + v'∙t
    // The KinematicCharacterController directly updates the position applying the exact equation 
    // of motion:
    //   v' = v + g∙t
    //   s' = s + v∙t + 1/2∙g∙t²

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector3F _gravityVelocity;   // Current velocity from gravity.
    private Vector3F _jumpVelocity;      // Current velocity from jumping.

    // A ray is used to sense for ground under the capsule.
    private readonly CollisionObject _ray;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the simulation.
    /// </summary>
    /// <value>The simulation.</value>
    public Simulation Simulation { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether this character controller is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the character controller is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// The rigid body (see <see cref="Body"/>) is automatically added/removed from
    /// the simulation when the character is enabled/disabled. 
    /// </remarks>
    public bool Enabled
    {
      get { return Body.Simulation != null; }
      set
      {
        if (Enabled == value)
          return;

        if (value)
        {
          Simulation.RigidBodies.Add(Body);
          Simulation.CollisionDomain.CollisionObjects.Add(_ray);

          // Disable collisions between the ray and the body.
          var filter = Simulation.CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
          if (filter != null)
            filter.Set(Body.CollisionObject, _ray, false);
        }
        else
        {
          // Undo steps from above.

          // Disable collisions between the ray and the body.
          var filter = Simulation.CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
          if (filter != null)
            filter.Set(Body.CollisionObject, _ray, true);

          Simulation.RigidBodies.Remove(Body);
          Simulation.CollisionDomain.CollisionObjects.Remove(_ray);
        }
      }
    }


    /// <summary>
    /// Gets or sets the body.
    /// </summary>
    /// <value>The body.</value>
    /// <remarks>
    /// <para>
    /// The body is automatically added to or removed from the <see cref="Simulation"/> when the 
    /// character is enabled/disabled (see <see cref="Enabled"/> ).
    /// </para>
    /// </remarks>
    public RigidBody Body { get; private set; }


    /// <summary>
    /// Gets or sets the collision group.
    /// </summary>
    /// <value>The collision group.</value>
    public int CollisionGroup
    {
      get { return Body.CollisionObject.CollisionGroup; }
      set 
      { 
        Body.CollisionObject.CollisionGroup = value;
        _ray.CollisionGroup = value;
      }
    }


    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height. The default is 1.8.</value>
    /// <remarks>
    /// This property assumes that the character's shape is a <see cref="CapsuleShape"/>.
    /// </remarks>
    public float Height
    {
      get { return ((CapsuleShape)Body.Shape).Height; }
      set
      {
        // To crouch the capsule height is changed to 1 m.
        var position = Position;
        ((CapsuleShape)Body.Shape).Height = value;

        // Changing the shape also changes the character position. We want the character to stay
        // on the ground. 
        Position = position;
      }
    }


    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width. The default is 0.8.</value>
    /// <remarks>
    /// This property assumes that the character's shape is a <see cref="CapsuleShape"/>.
    /// </remarks>
    public float Width
    {
      get { return ((CapsuleShape)Body.Shape).Radius * 2; }
      set { ((CapsuleShape)Body.Shape).Radius = value / 2; }
    }


    /// <summary>
    /// Gets or sets the position of the character.
    /// </summary>
    /// <value>The position of the character.</value>
    /// <remarks>
    /// The <see cref="Position"/> is the bottom position (the lowest point of the character's 
    /// body).
    /// </remarks>
    public Vector3F Position
    {
      get
      {
        return Body.Pose.Position - Height / 2 * Vector3F.UnitY;
      }
      set
      {
        var pose = Body.Pose;
        pose.Position = value + Height / 2 * Vector3F.UnitY;
        Body.Pose = pose;
      }
    }


    /// <summary>
    /// Gets or sets the gravity.
    /// </summary>
    /// <value>The gravity.The default is 9.81.</value>
    /// <remarks>
    /// The gravity is always acting in -y direction. This value is the magnitude of the gravity 
    /// (which is an acceleration). If the gravity is 0, then the character is free-flying.
    /// </remarks>
    public float Gravity
    {
      get { return _gravity; }
      set { _gravity = value; }
    }
    private float _gravity = 9.81f;


    /// <summary>
    /// Gets or sets the slope limit (in radians).
    /// </summary>
    /// <value>The slope limit. The default is the π/4 (= 45°).</value>
    /// <remarks>
    /// The character can move up inclined planes. If the inclination is higher than this value the
    /// character will not move up.
    /// </remarks>
    public float SlopeLimit
    {
      get { return _slopeLimit; }
      set
      {
        _slopeLimit = value;

        // Cache the cosine of the slope limit.
        _cosSlopeLimit = (float)Math.Cos(SlopeLimit);
      }
    }
    private float _slopeLimit = ConstantsF.PiOver4; // = 45°
    private float _cosSlopeLimit = (float)Math.Cos(ConstantsD.PiOver4);
    

    /// <summary>
    /// Gets or sets a value indicating whether this instance is climbing.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is climbing; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this property is set to <see langword="true"/>, gravity is not applied and the 
    /// character can move up (e.g. on a ladder or a climbable wall).
    /// </remarks>
    public bool IsClimbing { get; set; }


    /// <summary>
    /// Gets a value indicating whether this character has ground contact.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if character has ground contact; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this value is <see langword="true"/>, the character stands on a plane with allowed
    /// inclination. 
    /// </remarks>
    public bool HasGroundContact
    {
      get
      {
        if (_hasGroundContact.HasValue)
          return _hasGroundContact.Value;  // Return cached value.

        _hasGroundContact = false;

        // Check all contact sets of the body.
        foreach (var contactSet in Simulation.CollisionDomain.GetContacts(Body.CollisionObject))
        {
          // The character's body should be object A in the contact set.
          bool swapped = (Body.CollisionObject != contactSet.ObjectA);

          // Check all contacts.
          foreach (var contact in contactSet)
          {
            // Normal pointing to the CC.
            var normal = swapped ? contact.Normal : -contact.Normal;

            // Abort with true when there is an allowed slope the character can stand on.
            if (Vector3F.Dot(normal, Vector3F.UnitY) >= _cosSlopeLimit)
            {
              _hasGroundContact = true;
              return true;
            }
          }
        }

        return false;
      }
    }
    private bool? _hasGroundContact;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="KinematicCharacterController"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="simulation" /> is <see langword="null"/>.
    /// </exception>
    public DynamicCharacterController(Simulation simulation)
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");

      Simulation = simulation;

      CapsuleShape shape = new CapsuleShape(0.4f, 1.8f);
      MassFrame mass = new MassFrame { Mass = 80 };  // Push strength is proportional to the mass!
      UniformMaterial material = new UniformMaterial
      {
        // The body should be frictionless, so that it can be easily pushed by the simulation to
        // valid positions. And it does not slow down when sliding along walls.
        StaticFriction = 0.0f,
        DynamicFriction = 0.0f,

        // The body should not bounce when being hit or pushed.
        Restitution = 0
      };

      Body = new RigidBody(shape, mass, material)
      {
        // We set the mass explicitly and it should not automatically change when the 
        // shape is changed; e.g. a ducked character has a smaller shape, but still the same mass.
        AutoUpdateMass = false,

        // This body is under our control and should never be deactivated by the simulation.
        CanSleep = false,
        CcdEnabled = true,

        // The capsule does not rotate in any direction.
        LockRotationX = true,
        LockRotationY = true,
        LockRotationZ = true,

        Name = "CharacterController",

        Pose = new Pose(new Vector3F(0, shape.Height / 2, 0)),
      };


      // Create a ray that senses the space below the capsule. The ray starts in the capsule
      // center (to detect penetrations) and extends 0.4 units below the capsule bottom.
      RayShape rayShape = new RayShape(Vector3F.Zero, -Vector3F.UnitY, shape.Height / 2 + 0.4f)
      {
        StopsAtFirstHit = true,
      };
      GeometricObject rayGeometry = new GeometricObject(rayShape, Body.Pose);
      _ray = new CollisionObject(rayGeometry);

      // Whenever the Body moves, the ray moves with it.
      Body.PoseChanged += (s, e) => rayGeometry.Pose = Body.Pose;

      // Enable the character controller. (Adds body to simulation.)
      Enabled = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Moves the character to a new position while avoiding penetrations and sliding along 
    /// obstacles.
    /// </summary>
    /// <param name="moveVelocity">The desired linear walk or fly velocity.</param>
    /// <param name="jumpVelocity">
    /// The jump velocity. Set a velocity vector to let the character jump. The character will only
    /// jump if it starts on the ground. If gravity is turned off, the character will fly into the
    /// given direction.
    /// </param>
    /// <param name="deltaTime">The size of the time step.</param>
    /// <remarks>
    /// <para>
    /// This method does nothing if the character controller is disabled.
    /// </para>
    /// <para>
    /// This method tries to move with the given <paramref name="moveVelocity"/>. It will
    /// slide on obstacles and it will be affected by gravity. If the gravity is non-zero, the 
    /// character is "walking". It will not walk up planes that are steeper than the 
    /// <see cref="SlopeLimit"/>. 
    /// </para>
    /// <para>
    /// If the <paramref name="moveVelocity"/> is a zero vector, only gravity will be applied. 
    /// </para>
    /// </remarks>
    public void Move(Vector3F moveVelocity, float jumpVelocity, float deltaTime)
    {
      if (!Enabled)
        return;

      // Invalidate cached values.
      _hasGroundContact = null;

      if (Gravity == 0)
      {
        // ----- Flying
        _gravityVelocity = Vector3F.Zero;
        _jumpVelocity = Vector3F.Zero;
        Body.LinearVelocity = moveVelocity;
      }
      else
      {
        // ----- Walking

        // Find ground contact of ray.
        ContactSet rayContactSet = null;
        foreach (var cs in Simulation.CollisionDomain.GetContacts(_ray))
        {
          // Get first contact.
          rayContactSet = cs;
          break;
        }

        bool isAllowedSlope = false;
        Vector3F groundNormal = Vector3F.UnitY;
        if (rayContactSet != null && rayContactSet.Count > 0)
        {
          Contact contact = rayContactSet[0];
          var swapped = (rayContactSet.ObjectB == _ray);
          groundNormal = swapped ? contact.Normal : -contact.Normal;

          isAllowedSlope = (Vector3F.Dot(groundNormal, Vector3F.UnitY) >= _cosSlopeLimit);

          // Get the downwards direction of the slope (in xz-plane).
          var downDirection = groundNormal;
          downDirection.Y = 0;

          if (!isAllowedSlope && downDirection.TryNormalize())
          {
            // Slope is too steep. Character cannot move up the slope. 
            // Block any movement against the slope.
            float movementAgainstWall = -Vector3F.Dot(moveVelocity, downDirection);
            // x > 0 ... up the slope
            // x = 0 ... tangential to slope
            // x < 0 ... down the slope

            if (movementAgainstWall > 0)
            {
              // Cancel the velocity that moves the character up the slope.
              moveVelocity += movementAgainstWall * downDirection;
            }
          }
        }

        if (HasGroundContact                                              // On ground.
            || (IsClimbing && (_jumpVelocity + _gravityVelocity).Y <= 0)) // Or climbing and any jumping has ended.
        {
          // The character is on ground or in a situation where it has support.
          _jumpVelocity = Vector3F.Zero;
          _gravityVelocity = Vector3F.Zero;
        }
        else
        {
          // The character is airborne.
          if (jumpVelocity > 0)
          {
            // Jump button is still pressed. Do not apply gravity yet.
            _gravityVelocity = Vector3F.Zero;
          }
          else
          {
            // Increase velocity due to gravity.
            _gravityVelocity += -Vector3F.UnitY * Gravity * deltaTime;
          }
        }

        _jumpVelocity = Vector3F.Max(jumpVelocity * Vector3F.UnitY, _jumpVelocity);
        Body.LinearVelocity = moveVelocity + _jumpVelocity + _gravityVelocity;

        // Down steps: 
        // If the character moves down on an inclined plane, the character should stay in touch
        // with the plane and not step horizontal and lose ground contact.
        if (_jumpVelocity == Vector3F.Zero                          // Not jumping.
            && !IsClimbing                                          // Not climbing.
            && isAllowedSlope                                       // On an allowed slope.
            && Vector3F.Dot(Body.LinearVelocity, groundNormal) > 0) // Moving down the plane.
        {
          // Make velocity parallel to the ground plane.
          var tangent = Vector3F.Cross(groundNormal, Body.LinearVelocity);
          var downForward = -Vector3F.Cross(groundNormal, tangent);
          downForward.Normalize();
          Body.LinearVelocity = downForward * Body.LinearVelocity.Length;
        }

        // Invalidate cached values.
        _hasGroundContact = null;
      }
    }
    #endregion
  }
}
