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
  public class CapsuleTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new CapsuleShape().Radius);
      Assert.AreEqual(0, new CapsuleShape().Height);

      Assert.AreEqual(0, new CapsuleShape(0, 0).Radius);
      Assert.AreEqual(0, new CapsuleShape(0, 0).Height);

      Assert.AreEqual(3, new CapsuleShape(3, 10).Radius);
      Assert.AreEqual(10, new CapsuleShape(3, 10).Height);

      new CapsuleShape(3, 6); // This is like a sphere.
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new CapsuleShape(-10, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new CapsuleShape(1, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException3()
    {
      new CapsuleShape(10, 10);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new CapsuleShape(2, 10).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      CapsuleShape b = new CapsuleShape();
      Assert.AreEqual(0, b.Radius);
      Assert.AreEqual(0, b.Height);

      b.Height = 10;
      Assert.AreEqual(10, b.Height);
      Assert.AreEqual(0, b.Radius);

      b.Radius = 4;
      Assert.AreEqual(10, b.Height);
      Assert.AreEqual(4, b.Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RadiusException()
    {
      CapsuleShape b = new CapsuleShape();
      b.Radius = 1;   // Radius > Height --> ERROR
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void HeightException()
    {
      CapsuleShape b = new CapsuleShape(3, 7);
      b.Height = 4;  // 2*Radius > Height --> ERROR
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new CapsuleShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new CapsuleShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(0, 80, 990), new Vector3F(20, 120, 1010)),
                     new CapsuleShape(10, 40).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new CapsuleShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CapsuleShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CapsuleShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CapsuleShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(10, 5, 0), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 15, 0), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 5, 10), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(-10, 5, 0), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(-1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, -15, 0), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(0, -1, 0)));
      Assert.AreEqual(new Vector3F(0, 5, -10), new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(new Vector3F(0, 5, 0) + 10 * new Vector3F(1, 1, 1).Normalized, new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(1, 1, 1)));
      Assert.AreEqual(new Vector3F(0, -5, 0) + 10 * new Vector3F(-1, -1, -1).Normalized, new CapsuleShape(10, 30).GetSupportPoint(new Vector3F(-1, -1, -1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new CapsuleShape().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new CapsuleShape().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new CapsuleShape().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new CapsuleShape().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.IsTrue(Numeric.AreEqual(10, new CapsuleShape(10, 30).GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(15, new CapsuleShape(10, 30).GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new CapsuleShape(10, 30).GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new CapsuleShape(10, 30).GetSupportPointDistance(new Vector3F(-1, 0, -1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(0, 5, 0)+10*new Vector3F(1, 1, 1).Normalized, new Vector3F(1, 1, 1)).Length, new CapsuleShape(10, 30).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("CapsuleShape { Radius = 1, Height = 3 }", new CapsuleShape(1, 3).ToString());
    }


    [Test]
    public void Clone()
    {
      CapsuleShape capsule = new CapsuleShape(1.23f, 45.6f);
      CapsuleShape clone = capsule.Clone() as CapsuleShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(capsule.Radius, clone.Radius);
      Assert.AreEqual(capsule.Height, clone.Height);
      Assert.AreEqual(capsule.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(capsule.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new CapsuleShape(11, 22);

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
      var b = (CapsuleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new CapsuleShape(11, 22);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (CapsuleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }


    [Test]
    public void GetMesh()
    {
      var s = new CapsuleShape(3, 10);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);

      // No more tests necessary because we see the result in the samples.
    }
  }
}
