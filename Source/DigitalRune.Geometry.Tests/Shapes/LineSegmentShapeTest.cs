using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class LineSegmentShapeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new LineSegmentShape().Start);
      Assert.AreEqual(new Vector3F(), new LineSegmentShape().Start);
      Assert.AreEqual(new Vector3F(), new LineSegmentShape(Vector3F.Zero, Vector3F.Zero).Start);
      Assert.AreEqual(new Vector3F(), new LineSegmentShape(Vector3F.Zero, Vector3F.Zero).End);
      Assert.AreEqual(new Vector3F(1, 2, 3), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Start);
      Assert.AreEqual(new Vector3F(4, 5, 6), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).End);

      Assert.AreEqual(new Vector3F(1, 2, 3), new LineSegmentShape(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))).Start);
      Assert.AreEqual(new Vector3F(4, 5, 6), new LineSegmentShape(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))).End);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(11, 30, 45), new LineSegmentShape(new Vector3F(10, 20, 30), new Vector3F(12, 40, 60)).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      LineSegmentShape ls = new LineSegmentShape();
      Assert.AreEqual(new Vector3F(), ls.Start);
      Assert.AreEqual(new Vector3F(), ls.End);
      Assert.AreEqual(0, ls.Length);
      Assert.AreEqual(0, ls.LengthSquared);
      ls.Start = new Vector3F(1, 2, 3);
      Assert.AreEqual(new Vector3F(1, 2, 3), ls.Start);
      Assert.AreEqual(new Vector3F(), ls.End);
      Assert.AreEqual(new Vector3F(1, 2, 3).Length, ls.Length);
      Assert.AreEqual(new Vector3F(1, 2, 3).LengthSquared, ls.LengthSquared);
      ls.End = new Vector3F(4, 5, 6);
      Assert.AreEqual(new Vector3F(1, 2, 3), ls.Start);
      Assert.AreEqual(new Vector3F(4, 5, 6), ls.End);
      Assert.AreEqual(new Vector3F(3, 3, 3).Length, ls.Length);
      Assert.AreEqual(new Vector3F(3, 3, 3).LengthSquared, ls.LengthSquared);
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new LineSegmentShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new LineSegmentShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(11, 102, 1003), new Vector3F(14, 105, 1006)),
                     new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                         QuaternionF.Identity)));
    }

    [Test]
    public void GetMesh()
    {
      var l = new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));
      var m = l.GetMesh(0, 1);
      Assert.AreEqual(1, m.NumberOfTriangles);
      Triangle t = m.GetTriangle(0);
      Assert.IsTrue(l.Start == t.Vertex0);
      Assert.IsTrue(l.Start == t.Vertex1 || l.End == t.Vertex1);
      Assert.IsTrue(l.End == t.Vertex2);
    }


    [Test]
    public void GetSupportLineSegment()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new LineSegmentShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new LineSegmentShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new LineSegmentShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new LineSegmentShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Vector3F pos = new Vector3F(1, 2, 3);
      Assert.AreEqual(new Vector3F(4, 5, 6), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(4, 5, 6), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(4, 5, 6), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(1, 2, 3), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(-1, -1, -1)));
    }


    //[Test]
    //public void GetSupportLineSegmentDistance()
    //{
    //  Assert.AreEqual(0, new LineSegment().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new LineSegment().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new LineSegment().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new LineSegment().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.AreEqual(-1, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(-1, 0, 0)));
    //  Assert.AreEqual(-2, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(0, -1, 0)));
    //  Assert.AreEqual(-3, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(0, 0, -1)));
    //  Assert.AreEqual(4, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(5, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(6, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.IsTrue(Numeric.AreEqual(-Vector3F.ProjectTo(new Vector3F(1, 2, 3), new Vector3F(-1, 0, -1)).Length, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(-1, 0, -1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(4, 5, 6), new Vector3F(1, 1, 1)).Length, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("LineSegmentShape { Start = (1; 2; 3), End = (4; 5; 6) }", new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).ToString());
    }


    [Test]
    public void Clone()
    {
      LineSegmentShape lineSegment = new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4));
      LineSegmentShape clone = lineSegment.Clone() as LineSegmentShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(lineSegment.Start, clone.Start);
      Assert.AreEqual(lineSegment.End, clone.End);
      Assert.AreEqual(lineSegment.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(lineSegment.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4));

      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(Shape));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized Object:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(Shape));
      var b = (LineSegmentShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Start, b.Start);
      Assert.AreEqual(a.End, b.End);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4));

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (LineSegmentShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Start, b.Start);
      Assert.AreEqual(a.End, b.End);
    }
  }
}
