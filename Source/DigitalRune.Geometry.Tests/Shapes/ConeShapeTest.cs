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
  public class ConeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new ConeShape().Radius);
      Assert.AreEqual(0, new ConeShape().Height);

      Assert.AreEqual(0, new ConeShape(0, 0).Radius);
      Assert.AreEqual(0, new ConeShape(0, 0).Height);

      Assert.AreEqual(3, new ConeShape(3, 10).Radius);
      Assert.AreEqual(10, new ConeShape(3, 10).Height);

      new ConeShape(3, 0); // This is a flat disk
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new ConeShape(-10, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new ConeShape(1, -1);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 5, 0), new ConeShape(2, 10).InnerPoint);    
    }


    [Test]
    public void TestProperties()
    {
      ConeShape b = new ConeShape();
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
      ConeShape b = new ConeShape();
      b.Radius = -1;   
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void HeightException()
    {
      ConeShape b = new ConeShape(3, 7);
      b.Height = -4;  
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new ConeShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new ConeShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(0, 100, 990), new Vector3F(20, 140, 1010)),
                     new ConeShape(10, 40).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConeShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConeShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConeShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConeShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(10, 0, 0), new ConeShape(10, 30).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 30, 0), new ConeShape(10, 30).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 10), new ConeShape(10, 30).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(-10, 0, 0), new ConeShape(10, 30).GetSupportPoint(new Vector3F(-1, 0, 0)));
      Assert.AreEqual(new Vector3F(10, 0, 0), new ConeShape(10, 30).GetSupportPoint(new Vector3F(0, -1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, -10), new ConeShape(10, 30).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(new Vector3F(0, 30, 0), new ConeShape(10, 30).GetSupportPoint(new Vector3F(1, 1, 1)));
      Assert.AreEqual(10 * new Vector3F(-1, 0, -1).Normalized, new ConeShape(10, 30).GetSupportPoint(new Vector3F(-1, -1, -1)));

      ConeShape c= new ConeShape(10, 30);
      c.Radius = 0;
      Assert.AreEqual(new Vector3F(0, 0, 0), c.GetSupportPoint(new Vector3F(1, 0, 0)));
      c.Height = 0;
      Assert.AreEqual(new Vector3F(0, 0, 0), c.GetSupportPoint(new Vector3F(0, 1, 0)));
      c.Radius = 0;
      Assert.AreEqual(new Vector3F(0, 0, 0), c.GetSupportPoint(new Vector3F(0, 1, 0)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new ConeShape().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new ConeShape().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new ConeShape().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new ConeShape().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.IsTrue(Numeric.AreEqual(10, new ConeShape(10, 30).GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(30, new ConeShape(10, 30).GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new ConeShape(10, 30).GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new ConeShape(10, 30).GetSupportPointDistance(new Vector3F(-1, 0, -1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(0, 30, 0), new Vector3F(1, 1, 1)).Length, new ConeShape(10, 30).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("ConeShape { Radius = 1, Height = 3 }", new ConeShape(1, 3).ToString());
    }


    [Test]
    public void Clone()
    {
      ConeShape cone = new ConeShape(1.23f, 45.6f);
      ConeShape clone = cone.Clone() as ConeShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(cone.Radius, clone.Radius);
      Assert.AreEqual(cone.Height, clone.Height);
      Assert.AreEqual(cone.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(cone.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new ConeShape(11, 22);

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
      var b = (ConeShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new ConeShape(11, 22);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (ConeShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }


    [Test]
    public void GetMesh()
    {
      var s = new ConeShape(3, 10);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);

      // No more tests necessary because we see the result in the samples.
    }
  }
}
