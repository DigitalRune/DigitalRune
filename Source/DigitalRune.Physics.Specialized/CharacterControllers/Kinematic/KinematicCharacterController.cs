// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


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
  /// step over obstacles and climb onto steps up to a certain height limit (see 
  /// <see cref="StepHeight"/>). 
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
  /// To move the character <see cref="Move"/> must be called. This will immediately compute a 
  /// new position. But the <see cref="Simulation"/> can also move the character, for example,
  /// when it is pushed by kinematic objects. Therefore, the position is only final after
  /// the simulation was updated.
  /// </para>
  /// <para>
  /// General notes: In first person shooters character speeds up to 10 m/s are used. This is about
  /// twice as fast as normal human movement. For this high speed, the gravity is often set to a 
  /// higher than normal value, like 16 m/s², to account for this unnatural speed.
  /// </para>
  /// </remarks>
  public partial class KinematicCharacterController
  {
    // Notes:
    // This character controller uses a "kinematic character controller" approach where the 
    // position is changed using several sliding methods (Fly, Slide, StepUp, StepDown). This 
    // allows to control all aspects of the movement. 
    // In contrast, a "dynamic character controller" sets a velocity and lets the simulation 
    // compute the new position of the rigid body. This is a lot simpler, but the movement will
    // not be as smooth and as controllable in many situations.
    // Another advantage of a kinematic approach is that it can be used in games that do not use
    // physics - only collision detection is required. If that is the case, remove the rigid body
    // and use a CollisionObject instead.
    //
    // Continuous collision detection (CCD) was not available when most parts of the character
    // controller were created. Therefore, no CCD methods are used in the sliding methods.
    // Character speed is usually not so high that tunneling can occur.
    //
    // This code handles many special cases. A lot of complexity can be removed when the game
    // levels are "well-formed", e.g. slopes are used instead of stairs, invisible walls are used
    // instead of slope limits. For example, the character can smoothly step over high steps (e.g. 
    // 1 m steps if StepHeight is 1 and NumberOfSolverIterations and NumberOfSlideIterations are
    // sufficiently high) this is hardly needed in games. 
    //
    // Gravity is applied using the exact equations of motion instead of using a simple Euler
    // integration step. This is necessary in order achieve exact, reproducible heights when 
    // jumping. Otherwise the height would vary depending on the time step!
    //   v' = v + g∙t
    //   s' = s + v∙t + 1/2∙g∙t² 

    
    // TODO: Things to optimize and improve:
    // - Try to re-use contacts over iterations.
    // - Try a skin approach: Add an outer capsule that detects contacts. Penetrations of the
    //   skin is allowed but not of the inner capsule.
    // - ...


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A force effect that takes applies pushing and gravity forces to touched objects.
    // It also makes the character move with moving ground.
    private readonly CharacterControllerForceEffect _forceEffect;

    private Vector3F _gravityVelocity;   // Current velocity from gravity.
    private Vector3F _jumpVelocity;      // Current velocity from jumping.
    private Vector3F _oldPosition;       // The last valid position (set at the beginning of Move()).
    private Vector3F _desiredPosition;   // The desired target position (set in Move()).

    private bool _hadGroundContact;      // true if the last move ended with ground contact.
    private bool _isSteppingUp;          // Are we currently stepping up?
    private bool _isSteppingDown;        // Are we currently stepping down?
    
    internal Vector3F _lastDesiredVelocity;    
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
    /// the simulation when the character is enabled/disabled. When disabled the character
    /// will not move when <see cref="Move"/> or <see cref="ResolvePenetrations"/> are called.
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
          Simulation.ForceEffects.Add(_forceEffect);
        }
        else
        {
          Simulation.RigidBodies.Remove(Body);
          Simulation.ForceEffects.Remove(_forceEffect);
        }
      }
    }


    /// <summary>
    /// Gets or sets the gravity.
    /// </summary>
    /// <value>The gravity.The default is 9.81.</value>
    /// <remarks>
    /// The gravity is always acting in -<see cref="UpVector"/> direction. This value is the
    /// magnitude of the gravity (which is an acceleration). If the gravity is 0, then the 
    /// character is free-flying.
    /// </remarks>
    public float Gravity
    {
      get { return _gravity; }
      set { _gravity = value; }
    }
    private float _gravity = 9.81f;


    /// <summary>
    /// Gets or sets the maximum velocity of the character.
    /// </summary>
    /// <value>
    /// The maximum velocity of the character. The default value is 20.</value>
    /// <remarks>
    /// The velocity of the character is limited to this value.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MaxVelocity
    {
      get { return _maxVelocity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MaxVelocity must be 0 or positive.");

        _maxVelocity = value;
      }
    }
    private float _maxVelocity = 20;


    /// <summary>
    /// Gets the current velocity of the character controller.
    /// </summary>
    /// <value>
    /// The current velocity of the character controller.
    /// </value>
    /// <remarks>
    /// This property is updated in <see cref="Move"/>.
    /// </remarks>
    public Vector3F Velocity { get; private set; }
    

    /// <summary>
    /// Gets or sets the maximal push force with which the character pushes other objects.
    /// </summary>
    /// <value>The maximal push force. The default value is 10000.</value>
    public float PushForce
    {
      get { return _pushForce; }
      set { _pushForce = value; }
    }
    private float _pushForce = 10000;


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
    private float _slopeLimit = ConstantsF.PiOver4; // = 45°;
    private float _cosSlopeLimit = (float)Math.Cos(ConstantsD.PiOver4);


    /// <summary>
    /// Gets or sets the height of the step.
    /// </summary>
    /// <value>The height of the step. The default value is 0.4.</value>
    /// <remarks>
    /// <para>
    /// <strong>Up steps:</strong> The character automatically tries to move up low obstacles/steps.
    /// To move up onto a step it is necessary that the obstacle is not higher than this value and
    /// that there is enough space for the character to stand on. 
    /// </para>
    /// <para>
    /// <strong>Down steps:</strong> If the character loses contact with the ground it tries to step
    /// down onto solid ground. If it cannot find ground within the step height, it will simply fall
    /// in a ballistic curve (defined by gravity). Here is an example why down-stepping is 
    /// necessary: If the character moves horizontally on a down inclined plane, it will always
    /// touch the plane. But, if the step height is set to <c>0</c>, the character will not try to
    /// step down and instead will "bounce" down the plane on short ballistic curves.
    /// </para>
    /// </remarks>
    public float StepHeight
    {
      get { return _stepHeight; }
      set { _stepHeight = value; }
    }
    private float _stepHeight = 0.4f;


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
    /// Gets or sets the jump maneuverability.
    /// </summary>
    /// <value>
    /// The jump maneuverability in the range [0, 1]. The default is 0.05 (5%).
    /// </value>
    /// <remarks>
    /// If this value is 0, the character cannot change direction during a jump. Values greater 
    /// than 0, give the player more control. When this property is set to 1, the character has 
    /// full control over its movement direction while jumping. 
    /// </remarks>
    public float JumpManeuverability
    {
      get { return _jumpManeuverability; }
      set { _jumpManeuverability = value; }
    }
    private float _jumpManeuverability = 0.05f;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="KinematicCharacterController"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    public KinematicCharacterController(Simulation simulation)
      : this(simulation, Vector3F.UnitY)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="KinematicCharacterController"/> class.
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <param name="upVector">The normalized up vector.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="simulation" /> is <see langword="null"/>.
    /// </exception>
    public KinematicCharacterController(Simulation simulation, Vector3F upVector)
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");

      Simulation = simulation;

      InitializeBody(upVector);

      _forceEffect = new CharacterControllerForceEffect(this);

      // Enable the character controller. (Adds body and force effect to simulation.)
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
    /// This method tries to move with the given <paramref name="moveVelocity"/>. It will slide on 
    /// obstacles, it will try to step up/down obstacles, and it will be affected by gravity. If the
    /// gravity is non-zero, the character is "walking". It will not walk up planes that are steeper
    /// than the <see cref="SlopeLimit"/>. It will try to follow upward/downward steps within the 
    /// <see cref="StepHeight"/>. If the gravity is <c>0</c>, the character is "flying" and the 
    /// <see cref="SlopeLimit"/> and <see cref="StepHeight"/> are not applied. 
    /// </para>
    /// <para>
    /// If the <paramref name="moveVelocity"/> is a zero vector, only gravity will be applied. 
    /// </para>
    /// </remarks>
    public void Move(Vector3F moveVelocity, float jumpVelocity, float deltaTime)
    {
      if (!Enabled)
        return;

      // Remove any velocities that were added by the simulation.
      Body.LinearVelocity = Vector3F.Zero;

      _oldPosition = Position;
      Vector3F desiredMovement = moveVelocity * deltaTime;
      
      if (Gravity == 0)
      {
        // ----- Flying
        _gravityVelocity = Vector3F.Zero;
        _jumpVelocity = Vector3F.Zero;

        desiredMovement += jumpVelocity * UpVector * deltaTime;

        // Limit velocity.
        float desiredMovementLength = desiredMovement.Length;
        float maxMovementLength = MaxVelocity * deltaTime;
        if (desiredMovementLength > maxMovementLength)
        {
          desiredMovement.Length = maxMovementLength;
          desiredMovementLength = maxMovementLength;
        }

        // Gather all objects in the area that can be reached by the movement.
        CollectObstacles(desiredMovementLength);

        // Call fly to move to the _desiredPosition.
        _desiredPosition = _oldPosition + desiredMovement;
        Fly();
      }
      else
      {
        // ----- Walking
        if (_hadGroundContact                                             // On ground.
            || (IsClimbing && (_jumpVelocity + _gravityVelocity).Y <= 0)  // Or climbing and any jumping has ended.
            || _isSteppingUp)                                             // Or currently stepping up.
        {
          // The character is on ground or in a situation where it has support.
          _jumpVelocity = jumpVelocity * UpVector;
          _gravityVelocity = Vector3F.Zero;

          // Add jump velocity to the desired movement.
          desiredMovement += _jumpVelocity * deltaTime;
        }
        else
        {
          // The character is airborne.

          // ----- Jump Maneuverability
          // The jump maneuverability defines whether the character can change the direction
          // during a jump.
          //
          //   JumpManeuverability = 0 ....... No maneuverability while in the air. 
          //                                   (The initial jump velocity dictates the direction.)
          //   JumpManeuverability = 1 ....... Full maneuverability while in air. 
          //                                   (The current user-input determines the direction.)
          //   0 < JumpManeuverability < 1 ... Limited maneuverability while in the air. 
          //
          // Depending on maneuverability, we lerp between the old lateral velocity and the
          // new lateral velocity. 
          Vector3F lastMovement = Velocity * deltaTime;
          lastMovement -= Vector3F.ProjectTo(lastMovement, UpVector);        // Remove vertical component.
          desiredMovement -= Vector3F.ProjectTo(desiredMovement, UpVector);  // Remove vertical component.
          desiredMovement = InterpolationHelper.Lerp(lastMovement, desiredMovement, JumpManeuverability);

          // Add jump velocity to the desired movement.
          _jumpVelocity = Vector3F.Max(jumpVelocity * UpVector, _jumpVelocity);
          desiredMovement += _jumpVelocity * deltaTime;

          if (jumpVelocity > 0)
          {
            // Jump button is still pressed. Do not apply gravity yet.
            _gravityVelocity = Vector3F.Zero;
          }
          else
          {
            // Apply gravity velocity using the exact equation of motion.
            Vector3F lastGravityVelocity = _gravityVelocity;

            // v' = v + g∙t
            _gravityVelocity += -UpVector * Gravity * deltaTime;
            if (_gravityVelocity.LengthSquared > MaxVelocity * MaxVelocity) // Limit gravity.
              _gravityVelocity.Length = MaxVelocity;

            // s' = s + v∙t + 1/2∙g∙t² 
            //    = s + 1/2∙(v' + v)t 
            desiredMovement += 0.5f * deltaTime * (_gravityVelocity + lastGravityVelocity);
          }
        }

        // Limit velocity.
        float desiredMovementLength = desiredMovement.Length;
        float maxMovementLength = MaxVelocity * deltaTime;
        if (desiredMovementLength > maxMovementLength)
        {
          desiredMovement.Length = maxMovementLength;
          desiredMovementLength = maxMovementLength;
        }
        
        // Gather all objects in the area that can be reached by the movement.
        CollectObstacles(Math.Max(desiredMovementLength, StepHeight));
        UpdateContacts();

        _desiredPosition = _oldPosition + desiredMovement;

        // Slide() can slide up small obstacles. StepUp() is only needed when StepHeight is high
        // and SlopeLimit is low. In many games Slide(false) can replace the following steps.
        // (Because of the spherical capsule form it will slide over small steps.)
        bool stopAtObstacle = (_hadGroundContact || _isSteppingUp);
        _isSteppingUp = false;
        bool blocked = !Slide(stopAtObstacle);
        if (blocked)
        {
          // The slide stopped at an obstacle. Try to step up. If we cannot step up, continue
          // the slide.
          _isSteppingUp = StepUp();
          if (!_isSteppingUp)
            Slide(false);
        }

        // If we did not move up and we are not jumping, try a down step.
        if (!_isSteppingUp
          && (_hadGroundContact || _isSteppingDown)
          && _jumpVelocity == Vector3F.Zero
          && !IsClimbing)
        {
          // Method 1: Keep stepping down as long as we touch any ground.
          // Pro: The character is glued to the ground. 
          // Con: When stepping over a cliff, the character bends down really fast as long as
          //      the capsule touches the cliff. (Note: This behavior is not visible if the
          //      NumberOfSlideIterations is low. Set NumberOfSlideIterations to 10 or higher
          //      to test is behavior.)
          _isSteppingDown = StepDown(!_isSteppingDown);

          // Methods 2: Only step down as long as we are on allowed ground.
          // Pro: Smooth movement over a cliff. 
          // Con: The character can lose ground when walking down high steps.
          //StepDown(!_isSteppingDown);
          //_isSteppingDown = HasGroundContact;

          // Method 3: Make a collision query and test if there is a step ahead and below of the 
          // character. This is more expensive but the only we to distinguish a cliff and a step.
          // ...
        }
        else
        {
          _isSteppingDown = false;
        }

        // Limit amount of movement to the length of the desired movement.
        // (Position corrections could have added additional movement.)
        // If we are not downstepping, we limit the movement by the length of the desired
        // movement. We do not want to gain speed. Only when we are downstepping we use
        // the larger maxMovementLength = MaxVelocity * dt. Otherwise, downstepping appears
        // very slow.
        if (!_isSteppingDown)
          maxMovementLength = desiredMovementLength;

        Vector3F actualMovement = Position - _oldPosition;
        float actualMovementLength = actualMovement.Length;
        if (actualMovementLength > maxMovementLength)
        {
          if (!Numeric.IsZero(actualMovementLength))
          {
            Position = _oldPosition + actualMovement / actualMovementLength * maxMovementLength;
            actualMovement = Position - _oldPosition;
            UpdateContacts();
          }
        }

        // Remember ground contact for next frame.
        _hadGroundContact = HasGroundContact;

        // If we stand on ground or if we moved up without jumping, then the gravity 
        // velocity must be reset.
        if (_jumpVelocity == Vector3F.Zero && Vector3F.Dot(actualMovement, UpVector) > 0)
        {
          _gravityVelocity = Vector3F.Zero;
        }
      }

      _lastDesiredVelocity = desiredMovement / deltaTime;
      Velocity = (Position - _oldPosition) / deltaTime;

      Body.LinearVelocity = Vector3F.Zero;
    }
    #endregion
  }
}
