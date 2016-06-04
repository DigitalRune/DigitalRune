// Define following symbol to use the simpler DynamicCharacterController. Undefine the symbol
// to use the more complex KinematicCharacterController.
//#define USE_DYNAMIC_CHARACTER_CONTROLLER 

using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Specialized;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Physics.Specialized
{
  // Controls the position and orientation of the player using a character controller.
  // The player can walk, jump, crawl. It can climb on ladders (red objects in the level) and it
  // can hold onto ledges.
  [Controls(@"Character Controller
  Use <W>, <A>, <S>, <D> or <Left Stick> to move.
  Use mouse or <Right Stick> to control camera.
  Press <R>/<F> or <DPad Up>/<DPad Down> to move up/down.
  Press <Space> or <A> on gamepad to jump.
  Press <Left Shift> or <Right Trigger> to crouch.
  Press <H> or <X> on gamepad to enable ""Hulk"" mode.")]
  public class CharacterControllerObject : GameObject
  {
    // This class can use 2 character controller implementations:
    // - DynamicCharacterController:
    //   A very simple character controller that uses a rigid body, applies velocities and lets the
    //   physics simulation compute the new position.
    // - KinematicCharacterController:
    //   An advanced character controller that also uses a rigid body, but controls the movement
    //   itself.This implementation allows more control over player movement and is more stable - 
    //   but also a lot more complex and slower.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    private const float LinearVelocityMagnitude = 5f;
    private const float AngularVelocityMagnitude = 0.1f;
    private const float ThumbStickFactor = 15;

    // When the jump button is pressed, the player moves up with the JumpVelocity.
    private const float JumpVelocity = 4f;

    // The JumpVelocity is sustained as long as the jump button is pressed, but no longer
    // than DynamicJumpTime. 
    private const float DynamicJumpTime = 10 / 60f;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IInputService _inputService;
    private readonly DebugRenderer _debugRenderer;

    private readonly Simulation _simulation;
    private float _timeSinceLastJump;

    // Orientation of camera.
    private float _yaw;
    private float _pitch;

    // The character can be in "Hulk" mode where it is stronger and can easily push heavy objects.
    private bool _isHulk;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // A "CharacterController" controls the character that can collide with other objects.
#if USE_DYNAMIC_CHARACTER_CONTROLLER
    public DynamicCharacterController CharacterController { get; private set; }
#else
    public KinematicCharacterController CharacterController { get; private set; }
#endif


    /// <summary>
    /// Gets the pose (position and orientation) of the player.
    /// </summary>
    /// <value>The pose (position and orientation) of the player.</value>
    /// <remarks>
    /// This is the bottom position, not the head position!
    /// </remarks>
    public Pose Pose
    {
      get
      {
        Vector3F position = CharacterController.Position;
        QuaternionF orientation = QuaternionF.CreateRotationY(_yaw) * QuaternionF.CreateRotationX(_pitch);
        return new Pose(position, orientation);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public CharacterControllerObject(IServiceLocator services)
    {
      Name = "CharacterController";

      _inputService = services.GetInstance<IInputService>();
      _debugRenderer = services.GetInstance<DebugRenderer>();
      _simulation = services.GetInstance<Simulation>();

      // Create character controller.
#if USE_DYNAMIC_CHARACTER_CONTROLLER
      CharacterController = new DynamicCharacterController(_simulation);
#else
      CharacterController = new KinematicCharacterController(_simulation);
#endif
      CharacterController.Enabled = false;
      CharacterController.Position = new Vector3F(0, 0, 10);
      CharacterController.Gravity = 10;   // Setting gravity to 0 switches to fly mode (instead of walking).

      // Special: No gravity and damping for character controller.
      // The character controller uses a rigid body. The gravity and damping force effects
      // should not influence this body. The character controller handles gravity itself.
      // Gravity and Damping are ForceEffects. Each force effect has an AreaOfEffect which,
      // by default, is a GlobalAreaOfEffect (= affects all bodies in the simulation). We can 
      // set a predicate method that excludes the rigid body of the character controller.
      GlobalAreaOfEffect areaOfEffect = new GlobalAreaOfEffect
      {
        Exclude = body => body == CharacterController.Body,
      };
      _simulation.ForceEffects.OfType<Gravity>().First().AreaOfEffect = areaOfEffect;
      _simulation.ForceEffects.OfType<Damping>().First().AreaOfEffect = areaOfEffect;

      // Special: Collision filtering.
      // We will have some collision objects that should not collide with the character controller.
      // We will use collision group 3 for the character and 4 for objects that should not collide
      // with it.
      CharacterController.CollisionGroup = 3;
      ICollisionFilter filter = (ICollisionFilter)_simulation.CollisionDomain.CollisionDetection.CollisionFilter;
      filter.Set(3, 4, false);  // Disable collisions between group 3 and 4.
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      CharacterController.Enabled = true;
    }


    protected override void OnUnload()
    {
      CharacterController.Enabled = false;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Mouse centering (controlled by the MenuComponent) is disabled if the game
      // is inactive or if the GUI is active. In these cases, we do not want to move
      // the player.
      if (!_inputService.EnableMouseCentering)
        return;

      float deltaTimeF = (float)deltaTime.TotalSeconds;

      // ----- Hulk Mode
      // Toggle "Hulk" mode if <H> or <X> (gamepad) is pressed.      
      if (_inputService.IsPressed(Keys.H, false) || _inputService.IsPressed(Buttons.X, false, LogicalPlayerIndex.One))
        ToggleHulk();

      // ----- Crouching
      if (_inputService.IsPressed(Keys.LeftShift, false) || _inputService.IsPressed(Buttons.RightTrigger, false, LogicalPlayerIndex.One))
      {
        Crouch();
      }
      else if (!_inputService.IsDown(Keys.LeftShift) && !_inputService.IsDown(Buttons.RightTrigger, LogicalPlayerIndex.One) && CharacterController.Height <= 1)
      {
        StandUp();
      }

      // ----- Update orientation 
      // Update _yaw and _pitch.
      UpdateOrientation(deltaTimeF);

      // Compute the new orientation of the camera.
      QuaternionF orientation = QuaternionF.CreateRotationY(_yaw) * QuaternionF.CreateRotationX(_pitch);

      // ----- Compute translation
      // Create velocity from <W>, <A>, <S>, <D> and gamepad sticks.       
      Vector3F moveDirection = Vector3F.Zero;
      if (Keyboard.GetState().IsKeyDown(Keys.W))
        moveDirection.Z--;
      if (Keyboard.GetState().IsKeyDown(Keys.S))
        moveDirection.Z++;
      if (Keyboard.GetState().IsKeyDown(Keys.A))
        moveDirection.X--;
      if (Keyboard.GetState().IsKeyDown(Keys.D))
        moveDirection.X++;

      GamePadState gamePadState = _inputService.GetGamePadState(LogicalPlayerIndex.One);
      moveDirection.X += gamePadState.ThumbSticks.Left.X;
      moveDirection.Z -= gamePadState.ThumbSticks.Left.Y;

      // Rotate the velocity vector from view space to world space.
      moveDirection = orientation.Rotate(moveDirection);

      // Add velocity from <R>, <F> keys. 
      // <R> or DPad up is used to move up ("rise"). 
      // <F> or DPad down is used to move down ("fall").
      if (Keyboard.GetState().IsKeyDown(Keys.R) || gamePadState.DPad.Up == ButtonState.Pressed)
        moveDirection.Y++;
      if (Keyboard.GetState().IsKeyDown(Keys.F) || gamePadState.DPad.Down == ButtonState.Pressed)
        moveDirection.Y--;

      // ----- Climbing
      bool hasLadderContact = HasLadderContact();
      bool hasLedgeContact = HasLedgeContact();
      CharacterController.IsClimbing = hasLadderContact || hasLedgeContact;

      // When the character is walking (gravity > 0) it cannot walk up/down - only on a ladder.
      if (CharacterController.Gravity != 0 && !hasLadderContact)
        moveDirection.Y = 0;

      // ----- Moving
      moveDirection.TryNormalize();
      Vector3F moveVelocity = moveDirection * LinearVelocityMagnitude;

      // ----- Jumping
      if ((_inputService.IsPressed(Keys.Space, false) || _inputService.IsPressed(Buttons.A, false, LogicalPlayerIndex.One))
          && (CharacterController.HasGroundContact || CharacterController.IsClimbing))
      {
        // Jump button was newly pressed and the character has support to start the jump.
        _timeSinceLastJump = 0;
      }

      float jumpVelocity = 0;
      if ((_inputService.IsDown(Keys.Space) || _inputService.IsDown(Buttons.A, LogicalPlayerIndex.One)))
      {
        // Jump button is still down.
        if (_timeSinceLastJump + deltaTimeF <= DynamicJumpTime)
        {
          // The DynamicJumpTime has not been exceeded.
          // Set a jump velocity to make the jump higher.
          jumpVelocity = JumpVelocity;
        }
        else if (_timeSinceLastJump <= DynamicJumpTime)
        {
          // The jump time exceeds DynamicJumpTime in this time step. 
          //   _timeSinceLastJump <= DynamicJumpTime
          //   _timeSinceLastJump + deltaTime > DynamicJumpTime

          // In order to achieve exact, reproducible jump heights we need to split 
          // the time step:
          float deltaTime0 = DynamicJumpTime - _timeSinceLastJump;
          float deltaTime1 = deltaTimeF - deltaTime0;

          // The first part of the movement is a jump with active jump velocity.
          jumpVelocity = JumpVelocity;
          _timeSinceLastJump += deltaTime0;
          CharacterController.Move(moveVelocity, jumpVelocity, deltaTime0);

          // The second part of the movement is a jump without jump velocity.
          jumpVelocity = 0;
          deltaTimeF = deltaTime1;
        }
      }

      _timeSinceLastJump += deltaTimeF;

      // ----- Move the character.
      CharacterController.Move(moveVelocity, jumpVelocity, deltaTimeF);

      // Draw character controller capsule.
      // The character controller is also transparent The hulk is green, of course.
      var color = _isHulk ? Color.DarkGreen : Color.Gray;
      color.A = 128;
      _debugRenderer.DrawObject(CharacterController.Body, color, false, false);
    }


    private void UpdateOrientation(float deltaTime)
    {
      GamePadState gamePadState = _inputService.GetGamePadState(LogicalPlayerIndex.One);

      // Compute new yaw and pitch from mouse movement and gamepad.
      float deltaYaw = -_inputService.MousePositionDelta.X;
      deltaYaw -= gamePadState.ThumbSticks.Right.X * ThumbStickFactor;
      _yaw += deltaYaw * deltaTime * AngularVelocityMagnitude;

      float deltaPitch = -_inputService.MousePositionDelta.Y;
      deltaPitch += gamePadState.ThumbSticks.Right.Y * ThumbStickFactor;
      _pitch += deltaPitch * deltaTime * AngularVelocityMagnitude;

      // Limit the pitch angle to +/- 90°.
      _pitch = MathHelper.Clamp(_pitch, -ConstantsF.PiOver2, ConstantsF.PiOver2);
    }


    private void ToggleHulk()
    {
      // In "Hulk" mode the character is green, 10 times stronger and has more mass.
      _isHulk = !_isHulk;

      // Only the KinematicCharacterController has a PushForce property. The push force of the 
      // DynamicCharacterController is proportional to its mass.
      CharacterController.Body.MassFrame.Mass = _isHulk ? 500 : 80;
#if !USE_DYNAMIC_CHARACTER_CONTROLLER
      CharacterController.PushForce = _isHulk ? 100000 : 10000;
#endif
    }


    private void Crouch()
    {
      // To crouch the capsule height is changed to 1 m.      
      CharacterController.Height = 1f;

      // The drawing data in the UserData (see RigidBodyRenderer) must be invalidated.
      CharacterController.Body.UserData = null;
    }


    private void StandUp()
    {
      // Similar to crouch - only we make the character capsule taller again. 
      // Before we change the height of the capsule we need to check if there is enough 
      // room to stand up. To check this we position a smaller capsule in this area and
      // test for collisions.
      CapsuleShape testCapsule = new CapsuleShape(0.38f, 1.6f);
      GeometricObject testObject = new GeometricObject(testCapsule, new Pose(CharacterController.Position + 1.0f * Vector3F.UnitY));
      CollisionObject testCollisionObject = new CollisionObject(testObject)
      {
        CollisionGroup = 4,                 // Should not collide with the character.
        Type = CollisionObjectType.Trigger, // Use a trigger because we do not need to compute detailed
      };                                    // collision information.

      // Check whether the test capsule touches anything.
      if (!_simulation.CollisionDomain.HasContact(testCollisionObject))
      {
        // No contact, enough room to stand up.
        CharacterController.Height = 1.8f;

        // The drawing data in the UserData (see RigidBodyRenderer) must be invalidated.
        CharacterController.Body.UserData = null;
      }
    }


    private bool HasLadderContact()
    {
      // If the character touches a body named "ladder" then it can climb up/down.
      return _simulation.CollisionDomain
                        .GetContactObjects(CharacterController.Body.CollisionObject)
                        .Select(collisionObject => collisionObject.GeometricObject)
                        .OfType<RigidBody>()
                        .Any(rigidBody => rigidBody.Name == "Ladder");

      // (Note: For simplicity we use LINQ expressions in this example to check whether the 
      // character touches the "Ladder". LINQ creates garbage on the managed heap which can 
      // be problematic on the Xbox 360.)
    }


    private bool HasLedgeContact()
    {
      // Here is a primitive way to detect ledges and edges where the character can climb:
      // We use a box that is ~10 cm high and wider than the character capsule. We check for 
      // collisions in the top part of the character and a second time on a lower positions.
      // If the top part is free of collisions and the lower part collides with something, then
      // we have found a ledge and let the character hang onto it.

      // Note: This objects should not be allocated in each frame. Create them once, add them
      // to the collision domain and move them with the character controller - like the ray
      // of the DynamicCharacterController.
      BoxShape box = new BoxShape(CharacterController.Width + 0.2f, 0.1f, CharacterController.Width + 0.2f);
      GeometricObject geometricObject = new GeometricObject(box)
      {
        Pose = new Pose(CharacterController.Position + new Vector3F(0, 1.6f, 0))
      };
      var collisionObject = new CollisionObject(geometricObject)
      {
        CollisionGroup = 4,                 // Should not collide with character.
        Type = CollisionObjectType.Trigger, // Use a trigger because we do not need to compute detailed
      };                                    // collision information.

      // First test:
      if (_simulation.CollisionDomain.HasContact(collisionObject))
        return false;

      // Second test:
      geometricObject.Pose = new Pose(CharacterController.Position + new Vector3F(0, 1.5f, 0));
      if (_simulation.CollisionDomain.HasContact(collisionObject))
        return true;

      return false;
    }
    #endregion
  }
}
