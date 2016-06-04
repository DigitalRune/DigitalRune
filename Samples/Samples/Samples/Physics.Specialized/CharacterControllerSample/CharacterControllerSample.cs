using DigitalRune.Geometry.Collisions;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Physics.Specialized
{
  [Sample(SampleCategory.PhysicsSpecialized,
    @"This sample shows how to use character controllers for 3D games.",
    @"A character controller is a module that computes the motion of an avatar. It handles:
Flying, walking, jumping, climbing, stepping up onto obstacles and stepping done from obstacles,
pushing rigid bodies, being pushed by rigid bodies, standing on moving platforms, and more...
The game builds a small level with several test objects. The character controller uses and 
upright capsule as collision shape of the avatar. The camera is attached to the character.",
  1)]
  public class CharacterControllerSample : PhysicsSpecializedSample
  {
    private readonly CharacterControllerObject _characterControllerObject;


    public CharacterControllerSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      Services.Register(typeof(DebugRenderer), null, GraphicsScreen.DebugRenderer);

      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a game object which loads and updates the test obstacles.
      GameObjectService.Objects.Add(new CharacterControllerLevelObject(Services));

      // Add a game object which uses a character controller.
      _characterControllerObject = new CharacterControllerObject(Services);
      GameObjectService.Objects.Add(_characterControllerObject);

      // Add a camera that is attached to the character controller.
      var cameraObject = new ThirdPersonCameraObject(_characterControllerObject, Services);
      GameObjectService.Objects.Add(cameraObject);
      GraphicsScreen.CameraNode = cameraObject.CameraNode;
    }


    public override void Update(GameTime gameTime)
    {
      // ----- Draw rigid bodies using the DebugRenderer of the graphics screen.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      foreach (var body in Simulation.RigidBodies)
      {
        var color = Color.Gray;

        // Draw static with different colors.
        if (body.MotionType == MotionType.Static)
          color = Color.LightGray;

        // Triggers are red and transparent. 
        if (body.CollisionObject.Type == CollisionObjectType.Trigger)
        {
          color = Color.DarkRed;
          color.A = 128;
        }

        // The character controller is drawn by the CharacterControllerObject.
        if (body == _characterControllerObject.CharacterController.Body)
          continue;

        debugRenderer.DrawObject(body, color, false, false);
      }
    }
  }
}
