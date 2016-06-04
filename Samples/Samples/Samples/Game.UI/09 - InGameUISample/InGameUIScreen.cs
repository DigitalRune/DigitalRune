using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  // A screen with one window. This screen is used inside the 3D scene. It does not use the normal
  // mouse input. Instead it uses a mouse position defined by the reticle of the 3D camera.
  // This class overrides OnHandleInput and overrides the mouse position in the InputContext.
  // The mouse position is found by shooting a ray from the camera to the 3D scene. If the ray
  // hits a TV object, the hit coordinates relative to this UIScreen are computed. UI controls in
  // this UIScreen use the computed position as the mouse position.
  class InGameUIScreen : UIScreen
  {
    private readonly Simulation _simulation;
    private readonly CameraObject _cameraObject;
    private Vector2F _lastMousePosition;


    public InGameUIScreen(IServiceLocator services, IUIRenderer renderer)
      : base("InGame", renderer)
    {
      var gameObjectService = services.GetInstance<IGameObjectService>();
      _cameraObject = (CameraObject)gameObjectService.Objects["Camera"];

      _simulation = services.GetInstance<Simulation>();

      Background = Color.White;

      // Add one window to the screen.
      Window window = new InGameWindow(services) { X = 175, Y = 30 };
      window.Show(this);
    }


    protected override void OnHandleInput(InputContext context)
    {
      if (_cameraObject.CameraNode == null)
        return;

      // The input context contains the mouse position that is used by the UI controls of this
      // screen. The mouse position is stored in the following properties:
      // - context.ScreenMousePosition
      // - context.ScreenMousePositionDelta
      // - context.MousePosition
      // - context.MousePositionDelta
      // 
      // Currently, these properties contain the mouse position relative to the game window.
      // But the mouse position of the in-game screen is determined by the reticle of the 
      // game camera. We need to make a ray-cast to see which part of the screen is hit and
      // override the properties.
      bool screenHit = false;

      // Get the camera position and the view direction in world space.
      Vector3F cameraPosition = _cameraObject.CameraNode.PoseWorld.Position;
      Vector3F cameraDirection = _cameraObject.CameraNode.PoseWorld.ToWorldDirection(Vector3F.Forward);

      // Create a ray (ideally this shape should be cached and reused).
      var ray = new RayShape(cameraPosition, cameraDirection, 1000);

      // We are only interested in the first object that is hit by the ray.
      ray.StopsAtFirstHit = true;

      // Create a collision object for this shape.
      var rayCollisionObject = new CollisionObject(new GeometricObject(ray, Pose.Identity));

      // Use the CollisionDomain of the physics simulation to perform a ray cast.
      ContactSet contactSet = _simulation.CollisionDomain.GetContacts(rayCollisionObject).FirstOrDefault();
      if (contactSet != null && contactSet.Count > 0)
      {
        // We have hit something :-)

        // Get the contact information of the ray hit.
        Contact contact = contactSet[0];

        // Get the hit object (one object in the contact set is the ray and the other object is the hit object).
        CollisionObject hitCollisionObject = (contactSet.ObjectA == rayCollisionObject) ? contactSet.ObjectB : contactSet.ObjectA;

        RigidBody hitBody = hitCollisionObject.GeometricObject as RigidBody;
        if (hitBody != null && hitBody.UserData is string && (string)hitBody.UserData == "TV")
        {
          // We have hit a dynamic rigid body of a TV object. 

          // Get the normal vector of the contact.
          var normal = (contactSet.ObjectA == rayCollisionObject) ? -contact.Normal : contact.Normal;

          // Convert the normal vector to the local space of the TV box.
          normal = hitBody.Pose.ToLocalDirection(normal);

          // The InGameUIScreen texture is only mapped onto the -Y sides of the boxes. If the user
          // looks onto another side, he cannot interact with the game screen.
          if (normal.Y < 0.5f)
          {
            // The user looks onto the TV's front side. Now, we have to map the ray hit position 
            // to the texture coordinate of the InGameUIScreen render target/texture.
            var localHitPosition = (contactSet.ObjectA == rayCollisionObject) ? contact.PositionBLocal : contact.PositionALocal;
            var normalizedPosition = GetTextureCoordinate(localHitPosition);

            // The texture coordinate is in the range [0, 0] to [1, 1]. If we multiply it with the
            // screen extent to the position in pixels.
            var inGameScreenMousePosition = normalizedPosition * new Vector2F(ActualWidth, ActualHeight);
            var inGameScreenMousePositionDelta = inGameScreenMousePosition - _lastMousePosition;

            // Finally, we can set the mouse positions that are relative to the InGame screen. Hurray!
            context.ScreenMousePosition = inGameScreenMousePosition;
            context.ScreenMousePositionDelta = inGameScreenMousePositionDelta;

            context.MousePosition = inGameScreenMousePosition;
            context.MousePositionDelta = inGameScreenMousePositionDelta;

            // Store the mouse position so that we can compute MousePositionDelta in the next frame.
            _lastMousePosition = context.MousePosition;
            screenHit = true;
          }
        }
      }

      if (screenHit)
      {
        // Call base class to call HandleInput for all child controls. The child controls will 
        // use the overridden mouse positions.
        base.OnHandleInput(context);
      }
    }


    // Returns the texture coordinates for a given point on the TV screen.
    public Vector2F GetTextureCoordinate(Vector3F pointLocal)
    {
      // We assume that the TV screen texture is mapped flat onto on side of the
      // box and is centered. - This is not exact since the screen is actually
      // not on the surface of the TV box, but a bit deeper in the box... but it
      // works for this sample.
      const float screenWidth = 0.74f;   // Width of the screen part of the TV model.
      const float screenHeight = 0.54f;  // Height of the screen part of the TV model.
      return new Vector2F(
        pointLocal.X / screenWidth + 0.5f,
        -pointLocal.Z / screenHeight + 0.5f);
    }
  }
}
