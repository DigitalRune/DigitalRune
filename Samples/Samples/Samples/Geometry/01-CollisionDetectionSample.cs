using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to use DigitalRune Geometry for collision detection.",
    @"This sample draws 3 shapes.
The closest-point pairs of the objects and the minimal distances are visualized.
If an object is in contact with another object, it is drawn yellow.",
    1)]
  [Controls(@"Sample
  Use the arrow keys to move one of the shapes.")]
  public class CollisionDetectionSample : BasicSample
  {
    // A collision detection and a collision domain to manage collision objects.
    private readonly CollisionDetection _collisionDetection;
    private readonly CollisionDomain _domain;

    // A few collision objects
    private readonly CollisionObject _collisionObjectA;
    private readonly CollisionObject _collisionObjectB;
    private readonly CollisionObject _collisionObjectC;


    public CollisionDetectionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      // ----- Initialize collision detection and create objects.

      // Create a geometric object with a box shape.
      // Position it on the left with an arbitrary rotation.
      var geometricObjectA = new GeometricObject(
        new BoxShape(1, 2, 3),
        new Pose(new Vector3F(-2, -1, 0), Matrix33F.CreateRotationZ(0.1f)));

      // Create a geometric object with a capsule shape.
      // Position it on the right with an arbitrary rotation.
      var geometricObjectB = new GeometricObject(
        new CapsuleShape(1, 3),
        new Pose(new Vector3F(2, -1, 0), Matrix33F.CreateRotationZ(-0.2f)));

      // Create a geometric object with a complex shape that is the convex hull of
      // a circle and a rectangle. Position it on the top with an arbitrary rotation.
      // (A ConvexHullOfShapes is a collection of different shapes with different
      // positions and orientations. The ConvexHullOfShapes combines these shapes
      // into a single shape by building their convex hull.)
      var complexShape = new ConvexHullOfShapes();
      complexShape.Children.Add(new GeometricObject(new RectangleShape(1, 1), new Pose(new Vector3F(0, 0, 1))));
      complexShape.Children.Add(new GeometricObject(new CircleShape(1), new Pose(new Vector3F(0, 0, -1))));
      var geometricObjectC = new GeometricObject(
        complexShape,
        new Pose(new Vector3F(0, 2, 0), QuaternionF.CreateRotation(Vector3F.UnitZ, new Vector3F(1, 1, 1))));

      // Create collision objects for the geometric objects.
      // (A collision object is just a wrapper around the geometric object that
      // stores additional information that is required by the collision detection.)
      _collisionObjectA = new CollisionObject(geometricObjectA);
      _collisionObjectB = new CollisionObject(geometricObjectB);
      _collisionObjectC = new CollisionObject(geometricObjectC);

      // Create a collision detection.
      // (The CollisionDetection stores general parameters and it can be used to
      // perform closest-point and contact queries.)
      _collisionDetection = new CollisionDetection();

      // Create a new collision domain and add the collision objects.
      // (A CollisionDomain manages multiple collision objects. It improves the
      // performance of contact queries by reusing results of the last frame.)
      _domain = new CollisionDomain(_collisionDetection);
      _domain.CollisionObjects.Add(_collisionObjectA);
      _domain.CollisionObjects.Add(_collisionObjectB);
      _domain.CollisionObjects.Add(_collisionObjectC);
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
      Pose pose = _collisionObjectA.GeometricObject.Pose;
      pose.Position += displacement;
      // Note: We have to cast to GeometricObject because CollisionObject.GeometricObject
      // is of type IGeometricObject which does not have a setter for the Pose property.
      ((GeometricObject)_collisionObjectA.GeometricObject).Pose = pose;

      // ----- Update collision domain. This computes new contact information.
      float timeStep = (float)gameTime.ElapsedGameTime.TotalSeconds;
      _domain.Update(timeStep);   // Needs to be called once per frame.

      // ----- Draw objects using the DebugRenderer of the graphics screen.
      // We reset the DebugRenderer every frame.
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw geometric objects. If an object has a contact with any other object, 
      // it is drawn yellow.
      bool objectBHasContact = _domain.HasContact(_collisionObjectB);
      var color = objectBHasContact ? Color.Yellow : Color.Blue;
      debugRenderer.DrawObject(_collisionObjectB.GeometricObject, color, false, false);

      bool objectCHasContact = _domain.HasContact(_collisionObjectC);
      color = objectCHasContact ? Color.Yellow : Color.Red;
      debugRenderer.DrawObject(_collisionObjectC.GeometricObject, color, false, false);

      bool objectAHasContact = _domain.HasContact(_collisionObjectA);
      color = objectAHasContact ? Color.Yellow : Color.Green;
      debugRenderer.DrawObject(_collisionObjectA.GeometricObject, color, false, false);

      // Get closest points.
      // Closest-point queries are not used as often as contact queries. They are
      // not computed by the collision domain. Therefore, we ask the collision
      // detection for the closest points.
      ContactSet closestPointsAB = _collisionDetection.GetClosestPoints(_collisionObjectA, _collisionObjectB);
      ContactSet closestPointsAC = _collisionDetection.GetClosestPoints(_collisionObjectA, _collisionObjectC);
      ContactSet closestPointsBC = _collisionDetection.GetClosestPoints(_collisionObjectB, _collisionObjectC);

      // Draw closest points.
      // Each contact set contains one contact that describes the closest-point pair.
      debugRenderer.DrawPoint(closestPointsAB[0].PositionAWorld, Color.White, true);
      debugRenderer.DrawPoint(closestPointsAB[0].PositionBWorld, Color.White, true);
      debugRenderer.DrawPoint(closestPointsAC[0].PositionAWorld, Color.White, true);
      debugRenderer.DrawPoint(closestPointsAC[0].PositionBWorld, Color.White, true);
      debugRenderer.DrawPoint(closestPointsBC[0].PositionAWorld, Color.White, true);
      debugRenderer.DrawPoint(closestPointsBC[0].PositionBWorld, Color.White, true);

      // Draw lines that represent the minimal distances.
      debugRenderer.DrawLine(closestPointsAB[0].PositionAWorld, closestPointsAB[0].PositionBWorld, Color.White, true);
      debugRenderer.DrawLine(closestPointsAC[0].PositionAWorld, closestPointsAC[0].PositionBWorld, Color.White, true);
      debugRenderer.DrawLine(closestPointsBC[0].PositionAWorld, closestPointsBC[0].PositionBWorld, Color.White, true);
    }
  }
}
