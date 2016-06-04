using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;


namespace Samples
{
  // Creates an explosion when a button is pressed.
  [Controls(@"Explosion
  Press <Middle Mouse> or <Right Shoulder> to create an explosion at the targeted position.")]
  public class ExplosionObject : GameObject
  {
    private readonly IInputService _inputService;
    private readonly Simulation _simulation;
    private readonly IGameObjectService _gameObjectService;


    public ExplosionObject(IServiceLocator services)
    {
      Name = "Explosion";

      _inputService = services.GetInstance<IInputService>();
      _simulation = services.GetInstance<Simulation>();
      _gameObjectService = services.GetInstance<IGameObjectService>();
    }


    // OnUpdate() is called once per frame.
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      if (_inputService.IsPressed(MouseButtons.Middle, true) || _inputService.IsPressed(Buttons.RightShoulder, true, LogicalPlayerIndex.One))
      {
        // The user has triggered an explosion.

        // The explosion is created at the position that is targeted with the cross-hair.
        // We can perform a ray hit-test to find the position. The ray starts at the camera
        // position and shoots forward (-z direction).
        var cameraGameObject = (CameraObject)_gameObjectService.Objects["Camera"];
        var cameraNode = cameraGameObject.CameraNode;
        Vector3F cameraPosition = cameraNode.PoseWorld.Position;
        Vector3F cameraDirection = cameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);

        // Create a ray for hit-testing.
        var ray = new RayShape(cameraPosition, cameraDirection, 1000);

        // The ray should stop at the first hit. We only want the first object.
        ray.StopsAtFirstHit = true;

        // The collision detection requires a CollisionObject.
        var rayCollisionObject = new CollisionObject(new GeometricObject(ray, Pose.Identity))
        {
          // In SampleGame.ResetPhysicsSimulation() a collision filter was set:
          //   CollisionGroup = 0 ... objects that support hit-testing
          //   CollisionGroup = 1 ... objects that are ignored during hit-testing
          //   CollisionGroup = 2 ... objects (rays) for hit-testing
          CollisionGroup = 2,
        };

        // Get the first object that has contact with the ray.
        ContactSet contactSet = _simulation.CollisionDomain.GetContacts(rayCollisionObject).FirstOrDefault();
        if (contactSet != null && contactSet.Count > 0)
        {
          // The ray has hit something.

          // The contact set contains all detected contacts between the ray and another object.
          // Get the first contact in the contact set. (A ray hit usually contains exactly 1 contact.)
          Contact contact = contactSet[0];

          // Create an explosion at the hit position.
          var explosion = new Explosion { Position = contact.Position };
          _simulation.ForceEffects.Add(explosion);

          // Note: The Explosion force effect removes itself automatically from the simulation once 
          // it has finished.
        }
      }
    }
  }
}
