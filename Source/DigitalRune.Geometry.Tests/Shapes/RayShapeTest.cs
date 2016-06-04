using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class RayShapeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new RayShape().Origin);
      Assert.AreEqual(new Vector3F(1, 0, 0), new RayShape().Direction);
      Assert.AreEqual(100, new RayShape().Length);
      Assert.AreEqual(false, new RayShape().StopsAtFirstHit);

      Vector3F origin = new Vector3F(1, 2, 3);
      Vector3F direction = new Vector3F(4, 5, 6).Normalized;
      float length = 10;
      Assert.AreEqual(origin, new RayShape(origin, direction, length).Origin);
      Assert.AreEqual(direction, new RayShape(origin, direction, length).Direction);
      Assert.AreEqual(length, new RayShape(origin, direction, length).Length);

      Ray ray = new Ray(origin, direction, length);
      Assert.AreEqual(origin, new RayShape(ray).Origin);
      Assert.AreEqual(direction, new RayShape(ray).Direction);
      Assert.AreEqual(length, new RayShape(ray).Length);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException()
    {
      new RayShape(new Vector3F(1, 2, 3), new Vector3F(), 10);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new RayShape(new Vector3F(1, 2, 3), new Vector3F(1, 0, 0), float.NegativeInfinity);
    }


    [Test]
    public void InnerPoint()
    {
      Vector3F origin = new Vector3F(1, 2, 3);
      Vector3F direction = new Vector3F(3, 2, 1).Normalized;
      Assert.AreEqual(origin + direction * 5, new RayShape(origin, direction, 10).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      RayShape l = new RayShape();
      Assert.AreEqual(new Vector3F(), l.Origin);
      Assert.AreEqual(new Vector3F(1, 0, 0), l.Direction);

      l.Origin = new Vector3F(1, 2, 3);
      Assert.AreEqual(new Vector3F(1, 2, 3), l.Origin);
      Assert.AreEqual(new Vector3F(1, 0, 0), l.Direction);

      l.Direction = new Vector3F(4, 5, 6).Normalized;
      Assert.AreEqual(new Vector3F(1, 2, 3), l.Origin);
      Assert.AreEqual(new Vector3F(4, 5, 6).Normalized, l.Direction);

      l.Length = 11;
      Assert.AreEqual(new Vector3F(1, 2, 3), l.Origin);
      Assert.AreEqual(new Vector3F(4, 5, 6).Normalized, l.Direction);
      Assert.AreEqual(11, l.Length);
      Assert.AreEqual(false, l.StopsAtFirstHit);

      l.StopsAtFirstHit = true;
      Assert.AreEqual(new Vector3F(1, 2, 3), l.Origin);
      Assert.AreEqual(new Vector3F(4, 5, 6).Normalized, l.Direction);
      Assert.AreEqual(11, l.Length);
      Assert.AreEqual(true, l.StopsAtFirstHit);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DirectionException()
    {
      RayShape l = new RayShape();
      l.Direction = new Vector3F();
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void LengthException()
    {
      RayShape l = new RayShape();
      l.Length = float.PositiveInfinity;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void LengthException2()
    {
      RayShape l = new RayShape();
      l.Length = float.NegativeInfinity;
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(new Vector3F(0, 0, 0), new Vector3F(100, 0, 0)), new RayShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(11, 102, 1003), new Vector3F(11, 112, 1003)),
                     new RayShape(new Vector3F(1, 2, 3), new Vector3F(0, 1, 0), 10).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                         QuaternionF.Identity)));
    }


    [Test]
    public void GetMesh()
    {
      var r = new RayShape(new Vector3F(1, 2, 3), Vector3F.UnitY, 10);
      
      var m = r.GetMesh(0, 1);
      Assert.AreEqual(1, m.NumberOfTriangles);

      Triangle t = m.GetTriangle(0);
      Assert.IsTrue(r.Origin == t.Vertex0);
      Assert.IsTrue(r.Origin + r.Direction * r.Length == t.Vertex2);
    }


    [Test]
    public void GetSupportPoint()
    {
      RayShape r = new RayShape(new Vector3F(1, 0, 0), new Vector3F(1, 1, 0).Normalized, 10);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), r.GetSupportPointNormalized(new Vector3F(-1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(r.Origin + r.Direction * r.Length, r.GetSupportPointNormalized(new Vector3F(1, 0, 0))));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 0, 0), r.GetSupportPoint(new Vector3F(-2, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(r.Origin + r.Direction * r.Length, r.GetSupportPoint(new Vector3F(2, 0, 0))));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("RayShape { Origin = (1; 2; 3), Direction = (0; 1; 0), Length = 10, StopsAtFirstHit = False }", new RayShape(new Vector3F(1, 2, 3), new Vector3F(0, 1, 0), 10).ToString());
    }


    [Test]
    public void Clone()
    {
      RayShape ray = new RayShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4).Normalized, 1234.567f);
      RayShape clone = ray.Clone() as RayShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(ray.Origin, clone.Origin);
      Assert.AreEqual(ray.Direction, clone.Direction);
      Assert.AreEqual(ray.Length, clone.Length);
      Assert.AreEqual(ray.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(ray.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new RayShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4).Normalized, 1234.567f);
      a.StopsAtFirstHit = true;

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
      var b = (RayShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Direction, b.Direction);
      Assert.AreEqual(a.Length, b.Length);
      Assert.AreEqual(a.Origin, b.Origin);
      Assert.AreEqual(a.StopsAtFirstHit, b.StopsAtFirstHit);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new RayShape(new Vector3F(1, 2, 3), new Vector3F(2, 3, 4).Normalized, 1234.567f);
      a.StopsAtFirstHit = true;

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (RayShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Direction, b.Direction);
      Assert.AreEqual(a.Length, b.Length);
      Assert.AreEqual(a.Origin, b.Origin);
      Assert.AreEqual(a.StopsAtFirstHit, b.StopsAtFirstHit);
    }
  }
}
