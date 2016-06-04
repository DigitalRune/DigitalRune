using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class SphereTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new SphereShape().Radius);
      Assert.AreEqual(0, new SphereShape(0).Radius);
      Assert.AreEqual(10, new SphereShape(10).Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new SphereShape(-1);
    }


    [Test]
    public void Radius()
    {
      SphereShape s = new SphereShape();
      Assert.AreEqual(0, s.Radius);
      s.Radius = 3;
      Assert.AreEqual(3, s.Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RadiusException()
    {
      SphereShape s = new SphereShape();
      s.Radius = -1;
    }


    [Test]
    public void Volume()
    {
      var s = new SphereShape(17);
      Assert.AreEqual(4f/3f * ConstantsF.Pi * 17 * 17 * 17, s.GetVolume(0.1f, 1));
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new SphereShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new SphereShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(0, 90, 990), new Vector3F(20, 110, 1010)),
                     new SphereShape(10).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(10, 0, 0), new SphereShape(10).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 10, 0), new SphereShape(10).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 10), new SphereShape(10).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(5.773502f), new SphereShape(10).GetSupportPoint(new Vector3F(1, 1, 1))));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3F(1, 1, 1)));
    //}


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new SphereShape(2).InnerPoint);
    }


    [Test]
    public void HaveContactWithPoint()
    {
      Assert.IsTrue(GeometryHelper.HaveContact(0, new Vector3F()));
      Assert.IsTrue(GeometryHelper.HaveContact(0, new Vector3F(Numeric.EpsilonF, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(0, new Vector3F(Numeric.EpsilonF)));

      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3F(0, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3F(-10, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3F(-10.00001f, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(10, new Vector3F(0, 10.01f, 0)));
    }

    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("SphereShape { Radius = 10 }", new SphereShape(10).ToString());
    }


    [Test]
    public void Clone()
    {
      SphereShape sphere = new SphereShape(0.1234f);
      SphereShape clone = sphere.Clone() as SphereShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(sphere.Radius, clone.Radius);
      Assert.AreEqual(sphere.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(sphere.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new SphereShape(11);

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
      var b = (SphereShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new SphereShape(11);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (SphereShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
    }


    [Test]
    public void GetMesh()
    {
      var s = new SphereShape(3);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);
      
      for (int i = 0; i < mesh.Vertices.Count; i++)
        Assert.IsTrue(Numeric.AreEqual(3, mesh.Vertices[i].Length));

    }
  }
}
