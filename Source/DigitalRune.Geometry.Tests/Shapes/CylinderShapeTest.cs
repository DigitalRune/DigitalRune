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
  public class CylinderTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new CylinderShape().Radius);
      Assert.AreEqual(0, new CylinderShape().Height);

      Assert.AreEqual(0, new CylinderShape(0, 0).Radius);
      Assert.AreEqual(0, new CylinderShape(0, 0).Height);

      Assert.AreEqual(3, new CylinderShape(3, 10).Radius);
      Assert.AreEqual(10, new CylinderShape(3, 10).Height);

      new CylinderShape(3, 1); // This is a flat disk
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new CylinderShape(-10, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new CylinderShape(1, -1);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape(2, 6).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      CylinderShape b = new CylinderShape();
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
      CylinderShape b = new CylinderShape();
      b.Radius = -1;   
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void HeightException()
    {
      CylinderShape b = new CylinderShape(3, 7);
      b.Height = -4;  
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new CylinderShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new CylinderShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(0, 80, 990), new Vector3F(20, 120, 1010)),
                     new CylinderShape(10, 40).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(10, 15, 0), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(10, 15, 0), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 15, 10), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(-10, 15, 0), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(-1, 0, 0)));
      Assert.AreEqual(new Vector3F(10, -15, 0), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(0, -1, 0)));
      Assert.AreEqual(new Vector3F(0, 15, -10), new CylinderShape(10, 30).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(new Vector3F(0, 15, 0) + 10 * new Vector3F(1, 0, 1).Normalized, new CylinderShape(10, 30).GetSupportPoint(new Vector3F(1, 1, 1)));
      Assert.AreEqual(new Vector3F(0, -15, 0) + 10 * new Vector3F(-1, 0, -1).Normalized, new CylinderShape(10, 30).GetSupportPoint(new Vector3F(-1, -1, -1)));

      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPointNormalized(new Vector3F(1, 0, 0).Normalized));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPointNormalized(new Vector3F(0, 1, 0).Normalized));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPointNormalized(new Vector3F(0, 0, 1).Normalized));
      Assert.AreEqual(new Vector3F(0, 0, 0), new CylinderShape().GetSupportPointNormalized(new Vector3F(1, 1, 1).Normalized));

      Assert.AreEqual(new Vector3F(10, 15, 0), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(1, 0, 0).Normalized));
      Assert.AreEqual(new Vector3F(10, 15, 0), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(0, 1, 0).Normalized));
      Assert.AreEqual(new Vector3F(0, 15, 10), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(0, 0, 1).Normalized));
      Assert.AreEqual(new Vector3F(-10, 15, 0), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(-1, 0, 0).Normalized));
      Assert.AreEqual(new Vector3F(10, -15, 0), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(0, -1, 0).Normalized));
      Assert.AreEqual(new Vector3F(0, 15, -10), new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(0, 0, -1).Normalized));
      Assert.AreEqual(new Vector3F(0, 15, 0) + 10 * new Vector3F(1, 0, 1).Normalized, new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(1, 1, 1).Normalized));
      Assert.AreEqual(new Vector3F(0, -15, 0) + 10 * new Vector3F(-1, 0, -1).Normalized, new CylinderShape(10, 30).GetSupportPointNormalized(new Vector3F(-1, -1, -1).Normalized));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new CylinderShape().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new CylinderShape().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new CylinderShape().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new CylinderShape().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.IsTrue(Numeric.AreEqual(10, new CylinderShape(10, 30).GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(15, new CylinderShape(10, 30).GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new CylinderShape(10, 30).GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new CylinderShape(10, 30).GetSupportPointDistance(new Vector3F(-1, 0, -1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(0, 15, 0)+10*new Vector3F(1, 0, 1).Normalized, new Vector3F(1, 1, 1)).Length, new CylinderShape(10, 30).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("CylinderShape { Radius = 1, Height = 3 }", new CylinderShape(1, 3).ToString());
    }


    [Test]
    public void Clone()
    {
      CylinderShape cylinder = new CylinderShape(1.23f, 45.6f);
      CylinderShape clone = cylinder.Clone() as CylinderShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(cylinder.Radius, clone.Radius);
      Assert.AreEqual(cylinder.Height, clone.Height);
      Assert.AreEqual(cylinder.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(cylinder.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new CylinderShape(11, 22);

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
      var b = (CylinderShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new CylinderShape(11, 22);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (CylinderShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
      Assert.AreEqual(a.Height, b.Height);
    }



    [Test]
    public void GetMesh()
    {
      var s = new CylinderShape(3, 10);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);

      // No more tests necessary because we see the result in the samples.
    }
  }
}
