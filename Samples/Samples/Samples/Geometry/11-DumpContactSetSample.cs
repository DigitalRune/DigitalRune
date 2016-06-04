#if WINDOWS
using System;
using System.Globalization;
using System.IO;
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;


namespace Samples.Geometry
{
  [Sample(SampleCategory.Geometry,
    @"This sample shows how to serialize a contact set into a C# file, which you can then use to
inspect the contact set.",
    @"Sometimes the collision detection creates a contact set and you are not sure if the contact
info is correct or how you want to inspect the two collision objects. This samples contains a
simple method which dumps a contact set into a C# file. The resulting C# file can be added as
another sample to this Samples project. The generated sample lets you inspect the
contacts. This works only for a contact set of two triangle mesh shapes!",
    11)]
  public class DumpContactSetSample : BasicSample
  {
    public DumpContactSetSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      GraphicsScreen.ClearBackground = true;

      // Create two collision objects with triangle mesh shapes.
      var meshA = new SphereShape(1).GetMesh(0.01f, 4);
      var shapeA = new TriangleMeshShape(meshA, true) { Partition = new CompressedAabbTree() };
      var poseA = new Pose(new Vector3F(-1, 0, 0), RandomHelper.Random.NextQuaternionF());
      var collisionObjectA = new CollisionObject(new GeometricObject(shapeA, poseA));

      var meshB = new BoxShape(0.2f, 2, 1f).GetMesh(0.01f, 4);
      var shapeB = new TriangleMeshShape(meshB, true) { Partition = new CompressedAabbTree() };
      var poseB = new Pose(new Vector3F(0.1f, 0, 0), RandomHelper.Random.NextQuaternionF());
      var collisionObjectB = new CollisionObject(new GeometricObject(shapeB, poseB));

      // Explicitly create a contact set. (Normally you would get the contact set
      // from the collision domain...)
      var contactSet = ContactSet.Create(collisionObjectA, collisionObjectB);

      // Create a C# sample which visualizes the contact set.
      const string Filename = "DumpedContactSet001.cs";
      DumpContactSet(contactSet, Filename);

      GraphicsScreen.DebugRenderer2D.DrawText(
        "Contact set dumped into the file: " + Filename, 
        new Vector2F(300, 300), 
        Color.Black);
    }


    public static void DumpContactSet(ContactSet contactSet, string filename)
    {
      var className = Path.GetFileNameWithoutExtension(filename);
      var text = new StringBuilder();
      text.Append(@"using DigitalRune.Geometry;
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
  [Sample(SampleCategory.Geometry, """", """", 1000)]
  [Controls(@""Sample
  Press <Space> to toggle wireframe rendering.
  Use numpad to move one object."")]
  public class " + className + @" : BasicSample
  {
    private readonly CollisionDetection _collisionDetection = new CollisionDetection();
    private CollisionObject _objectA;
    private CollisionObject _objectB;
    private ContactSet _contactSet;
    private bool _drawWireframe;


    public " + className + @"(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
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
      // Create two collision objects with triangle mesh shapes.");

      Append(text, contactSet.ObjectA, true);
      text.AppendLine();
      Append(text, contactSet.ObjectB, false);
      text.Append(@"
    }
  }
}");

      File.WriteAllText(filename, text.ToString());
    }


    private static void Append(StringBuilder text, CollisionObject co, bool isFirstCollisionObject)
    {
      text.Append(@"
      {
        var mesh = new TriangleMesh();");

      var shape = co.GeometricObject.Shape as TriangleMeshShape;
      if (shape == null)
        throw new NotSupportedException("The shapes must be TriangleMeshShapes");

      var mesh = shape.Mesh;
      for (int i = 0; i < mesh.NumberOfTriangles; i++)
      {
        text.Append(@"
        mesh.Add(");
        Append(text, mesh.GetTriangle(i));
        text.Append(");");
      }

      text.Append(@"

        var pose = new Pose(");
      Append(text, co.GeometricObject.Pose.Position);
      text.Append(", ");
      Append(text, QuaternionF.CreateRotation(co.GeometricObject.Pose.Orientation));
      text.Append(@");
        var scale = ");
      Append(text, co.GeometricObject.Scale);
      text.Append(@";
        var shape = new TriangleMeshShape(mesh) { Partition = new CompressedAabbTree() };
        shape.IsTwoSided = ");
      Append(text, shape.IsTwoSided);
      text.Append(@";
        shape.EnableContactWelding = ");
      Append(text, shape.EnableContactWelding);
      text.Append(@";
        ");

      if (isFirstCollisionObject)
        text.Append("_objectA");
      else
        text.Append("_objectB");

      text.Append(@" = new CollisionObject(new GeometricObject(shape, scale, pose));
      }");
    }


    private static void Append(StringBuilder text, Triangle t)
    {
      text.Append("new Triangle(");
      Append(text, t.Vertex0);
      text.Append(", ");
      Append(text, t.Vertex1);
      text.Append(", ");
      Append(text, t.Vertex2);
      text.Append(")");
    }


    private static void Append(StringBuilder text, Vector3F v)
    {
      text.Append(string.Format(CultureInfo.InvariantCulture, "new Vector3F({0}f, {1}f, {2}f)", 
        v.X, v.Y, v.Z));
    }


    private static void Append(StringBuilder text, QuaternionF q)
    {
      text.Append(string.Format(CultureInfo.InvariantCulture, "new QuaternionF({0}f, {1}f, {2}f, {3}f)",
        q.W, q.X, q.Y, q.Z));
    }


    private static void Append(StringBuilder text, bool b)
    {
      text.Append(b ? "true" : "false");
    }
  }
}
#endif