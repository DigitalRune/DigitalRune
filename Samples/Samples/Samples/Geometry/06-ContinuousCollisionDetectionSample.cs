using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use DigitalRune Geometry for Continuous Collision Detection (CCD).",
    @"This sample draws 2 geometric objects. One object moves from left to right. The other
object moves from top to bottom. The objects are drawn in their start and end position.
With Continuous Collision Detection it is possible to find the time of impact of the
two objects as they move from their start to their end position.
The gray objects show the start and end positions of the objects.
The red objects show the first time of impact.",
    6)]
  [Controls(@"Sample
  Use the arrow keys to move one of the objects.")]
  public class ContinuousCollisionDetectionSample : BasicSample
  {
    // A collision detection and a collision domain to manage collision objects.
    private readonly CollisionDetection _collisionDetection;

    // Two objects.
    private readonly CollisionObject _collisionObjectA;
    private readonly CollisionObject _collisionObjectB;
    private Pose _startPoseA;        // Start pose of object A.
    private Pose _startPoseB;        // Start pose of object A.
    private Pose _targetPoseA;       // Target pose of object A.
    private Pose _targetPoseB;       // Target pose of object B.

    // The time of impact.
    // Object A and B start at their current position (time = 0) and move to their target position
    // (time = 1). The time of impact is the time when the object start to touch during their
    // movement from start to target position
    private float _timeOfImpact;


    public ContinuousCollisionDetectionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // ----- Initialize collision detection and create objects.

      // Create a geometric object with a capsule shape.
      // Position it on the top with an arbitrary rotation.
      _startPoseA = new Pose(new Vector3F(0, 2, 0), Matrix33F.CreateRotationZ(0.1f));
      var geometricObjectA = new GeometricObject(new CapsuleShape(0.2f, 1), _startPoseA);
      _collisionObjectA = new CollisionObject(geometricObjectA);

      // Object A moves to the bottom of the screen.
      _targetPoseA = new Pose(new Vector3F(0, -2, 0), Matrix33F.CreateRotationZ(0.63f));

      // Create a geometric object with a composite shape.
      // Position it on the left with an arbitrary rotation.
      _startPoseB = new Pose(new Vector3F(-3, -1, 0), Matrix33F.CreateRotationZ(0.2f));
      var composite = new CompositeShape();
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 1, 0.1f), new Pose(new Vector3F(-0.75f, 0.5f, -0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 1, 0.1f), new Pose(new Vector3F(0.75f, 0.5f, -0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 1, 0.1f), new Pose(new Vector3F(-0.75f, 0.5f, 0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(0.1f, 1, 0.1f), new Pose(new Vector3F(0.75f, 0.5f, 0.5f))));
      composite.Children.Add(new GeometricObject(new BoxShape(1.8f, 0.1f, 1.1f), new Pose(new Vector3F(0, 1f, 0))));
      var geometricObjectB = new GeometricObject(composite, _startPoseB);

      // Object B moves to the left of the screen.
      _targetPoseB = new Pose(new Vector3F(3, -1, 0), Matrix33F.CreateRotationZ(0.3f));

      // Create collision objects for the geometric objects. 
      // (A collision object is just a wrapper around the geometric object that stores additional 
      // information that is required by the collision detection.)
      _collisionObjectA = new CollisionObject(geometricObjectA);
      _collisionObjectB = new CollisionObject(geometricObjectB);

      // Create a collision detection.
      // (The CollisionDetection stores general parameters and it can be used to perform
      // closest-point and contact queries.)
      _collisionDetection = new CollisionDetection();
    }


    public override void Update(GameTime gameTime)
    {
      // ----- Move object A with arrow keys.
      // Compute displacement.
      Vector3F displacement = Vector3F.Zero;
      if (InputService.IsDown(Keys.Up))
        displacement.Y += 0.1f;
      if (InputService.IsDown(Keys.Left))
        displacement.X -= 0.1f;
      if (InputService.IsDown(Keys.Down))
        displacement.Y -= 0.1f;
      if (InputService.IsDown(Keys.Right))
        displacement.X += 0.1f;

      // Update the position of object A (green box).
      _startPoseA.Position += displacement;

      // ----- CONTINUOUS COLLISION DETECTION -----
      // Object A moves from its current position to _targetPoseA.
      // Object B moves from its current position to _targetPoseB.
      // Let's see if they collide during their movement.
      ((GeometricObject)_collisionObjectA.GeometricObject).Pose = _startPoseA;
      ((GeometricObject)_collisionObjectB.GeometricObject).Pose = _startPoseB;
      _timeOfImpact = _collisionDetection.GetTimeOfImpact(
        _collisionObjectA,
        _targetPoseA,
        _collisionObjectB,
        _targetPoseB,
        0.05f);
      // The resulting time of impact is 1 if they do not collide or if they collide in their
      // end position. The time of impact is less then one if they collide before they reach
      // their target position.
      // -----

      // ----- Draw objects using the DebugRenderer of the graphics screen.
      // We reset the DebugRenderer every frame.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw game objects in the start pose.
      debugRenderer.DrawShape(_collisionObjectA.GeometricObject.Shape, _startPoseA, Vector3F.One, Color.Gray, false, false);
      debugRenderer.DrawShape(_collisionObjectB.GeometricObject.Shape, _startPoseB, Vector3F.One, Color.Gray, false, false);

      // Draw game objects in the target pose.
      debugRenderer.DrawShape(_collisionObjectA.GeometricObject.Shape, _targetPoseA, Vector3F.One, Color.Gray, false, false);
      debugRenderer.DrawShape(_collisionObjectB.GeometricObject.Shape, _targetPoseB, Vector3F.One, Color.Gray, false, false);

      // Draw objects at the first time of impact.
      if (_timeOfImpact < 1)
      {
        // Set objects to intermediate pose at time of impact.
        var poseA = Pose.Interpolate(_startPoseA, _targetPoseA, _timeOfImpact);
        var poseB = Pose.Interpolate(_startPoseB, _targetPoseB, _timeOfImpact);

        // Draw intermediate objects.       
        debugRenderer.DrawShape(_collisionObjectA.GeometricObject.Shape, poseA, Vector3F.One, Color.Red, false, false);
        debugRenderer.DrawShape(_collisionObjectB.GeometricObject.Shape, poseB, Vector3F.One, Color.Red, false, false);
      }
    }
  }
}
