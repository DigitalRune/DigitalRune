using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry, "", "", 1000)]
  [Controls(@"Sample
  Press <Space> to toggle wireframe rendering.
  Use numpad to move one object.")]
  public class VisualizeContactSet : BasicSample
  {
    private readonly CollisionDetection _collisionDetection = new CollisionDetection();
    private CollisionObject _objectA;
    private CollisionObject _objectB;
    private ContactSet _contactSet;
    private bool _drawWireframe;


    public VisualizeContactSet(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      SetCamera(new Vector3F(0, 1, 10), 0, 0);

      CreateObjects();

      _contactSet = _collisionDetection.GetContacts(_objectA, _objectB);
    }


    public override void Update(GameTime gameTime)
    {
      var debugRenderer = GraphicsScreen.DebugRenderer;
      debugRenderer.Clear();

      // Draw coordinate cross at world origin.
      debugRenderer.DrawAxes(Pose.Identity, 10, false);

      // Draw objects.
      debugRenderer.DrawObject(_objectA.GeometricObject, GraphicsHelper.GetUniqueColor(_objectA), _drawWireframe, false);
      debugRenderer.DrawObject(_objectB.GeometricObject, GraphicsHelper.GetUniqueColor(_objectB), _drawWireframe, false);

      // Draw contact info.
      debugRenderer.DrawContacts(_contactSet, 0.1f, null, true);

      // Toggle wireframe rendering.
      if (InputService.IsPressed(Keys.Space, true))
        _drawWireframe = !_drawWireframe;

      // Change background color if we have a contact.
      GraphicsScreen.BackgroundColor = (_contactSet != null && _contactSet.HaveContact)
        ? new Color(220, 200, 200, 255)
        : new Color(200, 220, 200, 255);

      // Move one object with keyboard NumPad.
      var translation = new Vector3F();
      if (InputService.IsDown(Keys.NumPad4))
        translation.X -= 1;
      if (InputService.IsDown(Keys.NumPad6))
        translation.X += 1;
      if (InputService.IsDown(Keys.NumPad8))
        translation.Y += 1;
      if (InputService.IsDown(Keys.NumPad5))
        translation.Y -= 1;
      if (InputService.IsDown(Keys.NumPad7))
        translation.Z -= 1;
      if (InputService.IsDown(Keys.NumPad9))
        translation.Z += 1;

      if (!translation.IsNumericallyZero)
      {
        var go = (GeometricObject)_objectA.GeometricObject;

        float scale = go.Aabb.Extent.Length * 0.1f;
        translation *= scale * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        go.Pose = new Pose(go.Pose.Position + translation, go.Pose.Orientation);

        if (_contactSet != null)
          _collisionDetection.UpdateContacts(_contactSet, 0.001f);
        else
          _contactSet = _collisionDetection.GetContacts(_objectA, _objectB);
      }
    }


    private void CreateObjects()
    {
      // Create two collision objects with triangle mesh shapes.
      var meshA = new TriangleMesh();
      meshA.Add(new Triangle(new Vector3F(0, 1, 0), new Vector3F(0, 1, 0), new Vector3F(0, 1, 0)));
      meshA.Add(new Triangle(new Vector3F(0, 1, 0), new Vector3F(0, 1, 0), new Vector3F(0, 1, 0)));
      var shapeA = new TriangleMeshShape() { Partition = new CompressedAabbTree() }; 
      var poseA = new Pose(new Vector3F(-1, 0, 0));
      _objectA = new CollisionObject(new GeometricObject(shapeA, poseA));

      var meshB = new BoxShape(0.2f, 2, 1f).GetMesh(0.05f, 4);
      var shapeB = new TriangleMeshShape(meshB, true) { Partition = new CompressedAabbTree() };
      var poseB = new Pose(new Vector3F(0.1f, 0, 0));
      _objectB = new CollisionObject(new GeometricObject(shapeB, poseB)); 
    }
  }
}
