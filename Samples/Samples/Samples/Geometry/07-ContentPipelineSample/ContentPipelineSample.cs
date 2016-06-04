using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample demonstrates how to use the XNA content pipeline to load collision shapes from
a collision model (e.g. FBX file) that was created in a 3D modelling tool (e.g. Blender, Maya).",
    @"This sample draws 2 ship models and a saucer model. The user can move one ship using the 
IJKL keys. The collision shape are loaded using the XNA content pipeline. 
A CollisionDomain is used to detect collisions between the ships and the saucer models.
The blue ship uses the triangle mesh. 
The other ships use approximated shapes.

See also: Samples/Geometry/07 - ContentPipelineSample/README.TXT",
    7)]
  [Controls(@"Sample
  Use arrow keys to move the second ship.
  Press <Space> to toggle debug drawing of collision shapes.")]
  public class ContentPipelineSample : BasicSample
  {
    // The collision domain manages CollisionObjects and computes collisions.
    private readonly CollisionDomain _collisionDomain;

    private readonly SaucerObject _saucerObject;
    private readonly ShipObject _shipObjectA;
    private readonly ShipObject _shipObjectB;

    private bool _drawDebugInfo;   // true if the collision shapes should be drawn for debugging.


    public ContentPipelineSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue; 
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // Initialize collision detection.
      // Note: The physics Simulation also has a collision domain (Simulation.CollisionDomain)
      // which we could use and which is updated together with the Simulation in
      // SampleGame.cs. But in this example we create our own CollisionDomain for demonstration
      // purposes.
      _collisionDomain = new CollisionDomain(new CollisionDetection());

      // Register CollisionDomain in service container.
      Services.Register(typeof(CollisionDomain), null, _collisionDomain);
      
      // Add game objects which manage graphics models and their collision representations.  
      _saucerObject = new SaucerObject(Services) { Name = "Saucer" };
      _shipObjectA = new ShipObject(Services) { Name = "ShipA" };
      _shipObjectB = new ShipObject(Services) { Name = "ShipB" };

      GameObjectService.Objects.Add(_saucerObject);
      GameObjectService.Objects.Add(_shipObjectA);
      GameObjectService.Objects.Add(_shipObjectB);

      // Position the second ship right of the first ship with an arbitrary rotation.
      _shipObjectB.Pose = new Pose(new Vector3F(2, 0, 0), QuaternionF.CreateRotationY(0.7f) * QuaternionF.CreateRotationX(1.2f));

      // Position the saucer left of the first ship with an arbitrary rotation.
      _saucerObject.Pose = new Pose(new Vector3F(-2.5f, 0, 0), QuaternionF.CreateRotationY(0.2f) * QuaternionF.CreateRotationX(0.4f));
    }


    public override void Update(GameTime gameTime)
    {
      // Get elapsed time.
      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // Move second ship with arrow keys.
      Vector3F shipMovement = Vector3F.Zero;
      if (InputService.IsDown(Keys.Left))
        shipMovement.X -= 1 * deltaTime;
      if (InputService.IsDown(Keys.Right))
        shipMovement.X += 1 * deltaTime;
      if (InputService.IsDown(Keys.Down))
        shipMovement.Y -= 1 * deltaTime;
      if (InputService.IsDown(Keys.Up))
        shipMovement.Y += 1 * deltaTime;

      // The movement is relative to the view of the user. We must rotate the movement vector
      // into world space.
      shipMovement = GraphicsScreen.CameraNode.PoseWorld.ToWorldDirection(shipMovement);

      // Update pose of second ship.
      var shipBPose = _shipObjectB.Pose;
      _shipObjectB.Pose = new Pose(shipBPose.Position + shipMovement, shipBPose.Orientation);

      // Toggle debug drawing with Space key.
      if (InputService.IsPressed(Keys.Space, true))
        _drawDebugInfo = !_drawDebugInfo;

      // Update collision domain. - This will compute collisions.
      _collisionDomain.Update(deltaTime);

      // Now we could, for example, ask the collision domain if the ships are colliding.
      bool shipsAreColliding = _collisionDomain.HaveContact(
        _shipObjectA.CollisionObject, 
        _shipObjectB.CollisionObject);

      // Use the debug renderer of the graphics screen to draw debug info and collision shapes.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      if (_collisionDomain.ContactSets.Count > 0)
        debugRenderer.DrawText("\n\nCOLLISION DETECTED");
      else
        debugRenderer.DrawText("\n\nNo collision detected");

      if (_drawDebugInfo)
      {
        foreach(var collisionObject in _collisionDomain.CollisionObjects)
          debugRenderer.DrawObject(collisionObject.GeometricObject, Color.Gray, false, false);
      }
    }
  }
}
