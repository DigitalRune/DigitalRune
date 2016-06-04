using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

// Both XNA and DigitalRune have a class called MathHelper. To avoid compiler errors
// we need to define which MathHelper we want to use.
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample demonstrates how a character controller can be implemented with DigitalRune Geometry.",
    @"A small level with interesting obstacles is created. The character controller uses a capsule
to represent the character.
The level also contains a trigger volume that changes color if the character touches the volume.
The implementation of the character controller covers basic functionality:
- Sliding along walls.
- Stepping up and down.
- Jumping.
- Limits for step heights and slope angles.
- ...
The character controller is already pretty capable but far from perfect.

Important:
This character controller is an educational example. The DigitalRune.Physics library contains
a more advanced, faster and more stable character controller implementation.",
    8)]
  [Controls(@"Sample
  Use <W>, <A>, <S>, <D> and mouse to move.
  Press <Space> to jump")]
  public class CharacterControllerSample : BasicSample
  {
    // The collision domain that manages collision objects.
    private readonly CollisionDomain _domain;

    // The character controller (see CharacterController.cs).
    private readonly CharacterController _character;

    private readonly CameraNode _cameraNode;

    // The current orientation angles of the camera. The character controller manages
    // only position and not orientation.
    private float _yaw;
    private float _pitch;

    // A trigger volume. The character can run through this object. If it touches the
    // trigger volume, it changes its color.
    private CollisionObject _triggerVolume;


    public CharacterControllerSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;

      // Create a camera.
      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(
        ConstantsF.PiOver4,
        GraphicsService.GraphicsDevice.Viewport.AspectRatio,
        0.1f,
        1000.0f);
      _cameraNode = new CameraNode(new Camera(projection));
      GraphicsScreen.CameraNode = _cameraNode;

      // We use one collision domain that computes collision info for all game objects.
      _domain = new CollisionDomain(new CollisionDetection());

      // Create collision objects for a test level.
      CharacterControllerLevel.Load(_domain);

      // Add collision filter:
      // The _domain contains a lot of collision objects for obstacles in the level.
      // We do not need to compute contacts between these static obstacles. To avoid
      // this, the CharacterControllerLevel puts all level collision objects into
      // the collision group 1. We add a broad phase collision filter which filters out
      // collision checks between objects of collision group 1.
      _domain.BroadPhase.Filter = new DelegatePairFilter<CollisionObject>(
        pair =>
        {
          if (pair.First.CollisionGroup == 1 && pair.Second.CollisionGroup == 1)
            return false;

          return true;
        });

      // Create character controller. 
      _character = new CharacterController(_domain);
      _character.Position = new Vector3F(0, 0, 1);

      // Create the trigger volume. 
      _triggerVolume = new CollisionObject(
        new GeometricObject(new SphereShape(3), new Pose(new Vector3F(-5, 0, 5))))
      {
        // We do not want to compute detailed contact information (contact points, contact 
        // normal vectors, etc.). We are only interested if the object touches another object or not.
        // Therefore, we set the collision object type to "trigger". Trigger objects are better for 
        // performance than normal collision objects. Additionally, the character controller should
        // be able to walk through the trigger volume. The character controller treats objects as 
        // solids if it finds contact information (contact positions with contact normal vectors).
        Type = CollisionObjectType.Trigger
      };
      _domain.CollisionObjects.Add(_triggerVolume);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        GraphicsScreen.CameraNode = null;
        _cameraNode.Dispose(false);
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // Compute new collision information.
      _domain.Update(gameTime.ElapsedGameTime);

      // Update the character controller.
      ControlCharacter((float)gameTime.ElapsedGameTime.TotalSeconds);

      // Check whether the character controller touches the trigger volume.
      // If there is a contact, we change the color of the trigger volume.
      bool characterTouchesTriggerVolume = _domain.HaveContact(_character.CollisionObject, _triggerVolume);

      // ----- Draw everything using the debug renderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw all geometric objects (except the trigger volume).
      foreach (var collisionObject in _domain.CollisionObjects)
      {
        if (collisionObject != _triggerVolume)
        {
          debugRenderer.DrawObject(
            collisionObject.GeometricObject,
            GraphicsHelper.GetUniqueColor(collisionObject),
            false,
            false);
        }
      }

      // For debugging: Draw contacts of the character capsule.
      // Draw line to visualize contact normal.
      debugRenderer.DrawContacts(_domain.ContactSets, 0.1f, Color.White, true);

      // Draw trigger volume (transparent using alpha blending).
      debugRenderer.DrawObject(
        _triggerVolume.GeometricObject,
        characterTouchesTriggerVolume ? new Color(255, 0, 0, 128) : new Color(255, 255, 255, 128),
        false,
        false);
    }


    // Handle character-related input and move the character.
    private void ControlCharacter(float deltaTime)
    {
      // Compute new orientation from mouse movement.
      float deltaYaw = -InputService.MousePositionDelta.X;
      _yaw += deltaYaw * deltaTime * 0.1f;
      float deltaPitch = -InputService.MousePositionDelta.Y;
      _pitch += deltaPitch * deltaTime * 0.1f;

      // Limit the pitch angle.
      _pitch = MathHelper.Clamp(_pitch, -ConstantsF.PiOver2, ConstantsF.PiOver2);

      // Compute new orientation of the camera.
      QuaternionF cameraOrientation = QuaternionF.CreateRotationY(_yaw) * QuaternionF.CreateRotationX(_pitch);

      // Create velocity from WASD keys.
      // TODO: Diagonal movement is faster ;-). Fix this.
      Vector3F velocityVector = Vector3F.Zero;
      if (Keyboard.GetState().IsKeyDown(Keys.W))
        velocityVector.Z--;
      if (Keyboard.GetState().IsKeyDown(Keys.S))
        velocityVector.Z++;
      if (Keyboard.GetState().IsKeyDown(Keys.A))
        velocityVector.X--;
      if (Keyboard.GetState().IsKeyDown(Keys.D))
        velocityVector.X++;
      velocityVector *= 10 * deltaTime;

      // Velocity vector is currently in view space. -z is the forward direction. 
      // We have to convert this vector to world space by rotating it.
      velocityVector = QuaternionF.CreateRotationY(_yaw).Rotate(velocityVector);

      // New compute desired character controller position in world space:
      Vector3F targetPosition = _character.Position + velocityVector;

      // Check if user wants to jump.
      bool jump = Keyboard.GetState().IsKeyDown(Keys.Space);

      // Call character controller to compute a new valid position. The character
      // controller slides along obstacles, handles stepping up/down, etc.
      _character.Move(targetPosition, deltaTime, jump);

      // ----- Set view matrix for graphics.
      // For third person we move the eye position back, behind the body (+z direction is 
      // the "back" direction).
      Vector3F thirdPersonDistance = cameraOrientation.Rotate(new Vector3F(0, 0, 6));

      // Compute camera pose (= position + orientation). 
      _cameraNode.PoseWorld = new Pose
      {
        Position = _character.Position         // Floor position of character
                   + new Vector3F(0, 1.6f, 0)  // + Eye height
                   + thirdPersonDistance,
        Orientation = cameraOrientation.ToRotationMatrix33()
      };
    }
  }
}
