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
  public class BoxTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new BoxShape().WidthX);
      Assert.AreEqual(0, new BoxShape().WidthY);
      Assert.AreEqual(0, new BoxShape().WidthZ);

      Assert.AreEqual(0, new BoxShape(0, 0, 0).WidthX);
      Assert.AreEqual(0, new BoxShape(0, 0, 0).WidthY);
      Assert.AreEqual(0, new BoxShape(0, 0, 0).WidthZ);

      Assert.AreEqual(10, new BoxShape(10, 11, 12).WidthX);
      Assert.AreEqual(11, new BoxShape(10, 11, 12).WidthY);
      Assert.AreEqual(12, new BoxShape(10, 11, 12).WidthZ);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new BoxShape(-10, 0, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new BoxShape(0, -1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException3()
    {
      new BoxShape(0, 0, -1);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new BoxShape(1, 2, 3).InnerPoint);
    }


    [Test]
    public void Width()
    {
      BoxShape b = new BoxShape();
      Assert.AreEqual(0, b.WidthX);
      Assert.AreEqual(0, b.WidthY);
      Assert.AreEqual(0, b.WidthZ);

      b.WidthX = 10;
      Assert.AreEqual(10, b.WidthX);
      Assert.AreEqual(0, b.WidthY);
      Assert.AreEqual(0, b.WidthZ);

      b.WidthY = 11;
      Assert.AreEqual(10, b.WidthX);
      Assert.AreEqual(11, b.WidthY);
      Assert.AreEqual(0, b.WidthZ);

      b.WidthZ = 12;
      Assert.AreEqual(10, b.WidthX);
      Assert.AreEqual(11, b.WidthY);
      Assert.AreEqual(12, b.WidthZ);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WidthXException()
    {
      BoxShape b = new BoxShape();
      b.WidthX = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WidthYException()
    {
      BoxShape b = new BoxShape();
      b.WidthY = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WidthZException()
    {
      BoxShape b = new BoxShape();
      b.WidthZ = -1;
    }

    [Test]
    public void Extent()
    {
      BoxShape b = new BoxShape();

      b.Extent = new Vector3F(1, 2, 3);
      Assert.AreEqual(1, b.WidthX);
      Assert.AreEqual(2, b.WidthY);
      Assert.AreEqual(3, b.WidthZ);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ExtentXException()
    {
      BoxShape b = new BoxShape();
      b.Extent = new Vector3F(-1, 1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ExtentYException()
    {
      BoxShape b = new BoxShape();
      b.Extent = new Vector3F(1, -1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ExtentZException()
    {
      BoxShape b = new BoxShape();
      b.Extent = new Vector3F(1, 1, -1);
    }


    [Test]
    public void Volume()
    {
      var box = new BoxShape(2, 3, 7);
      Assert.AreEqual(2 * 3 * 7, box.GetVolume(0.1f, 1));
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new BoxShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new BoxShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(5, 90, 985), new Vector3F(15, 110, 1015)),
                     new BoxShape(10, 20, 30).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new BoxShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new BoxShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new BoxShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new BoxShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(5, 10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(5, 10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(5, 10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(-5, 10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(-1, 0, 0)));
      Assert.AreEqual(new Vector3F(5, -10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(0, -1, 0)));
      Assert.AreEqual(new Vector3F(5, 10, -15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(new Vector3F(5, 10, 15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(1, 1, 1)));
      Assert.AreEqual(new Vector3F(-5, -10, -15), new BoxShape(10, 20, 30).GetSupportPoint(new Vector3F(-1, -1, -1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new BoxShape().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new BoxShape().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new BoxShape().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new BoxShape().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.IsTrue(Numeric.AreEqual(5, new BoxShape(10, 20, 30).GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new BoxShape(10, 20, 30).GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(15, new BoxShape(10, 20, 30).GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(5, 10, 15), new Vector3F(-1, -1, 0)).Length, new BoxShape(10, 20, 30).GetSupportPointDistance(new Vector3F(-1, -1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(5, 10, 15), new Vector3F(1, 1, 1)).Length, new BoxShape(10, 20, 30).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void HaveContactWithPoint()
    {
      var b = new Vector3F(0, 0, 0);

      Assert.IsTrue(GeometryHelper.HaveContact(b, new Vector3F()));
      Assert.IsTrue(GeometryHelper.HaveContact(b, new Vector3F(Numeric.EpsilonF, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(b, new Vector3F(Numeric.EpsilonF + 0.000001f)));

      b = new Vector3F(10, 20, 30);
      Assert.IsTrue(GeometryHelper.HaveContact(b, new Vector3F(0, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(b, new Vector3F(-5, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(b, new Vector3F(-5.00001f, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(b, new Vector3F(0, 10.01f, 0)));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("BoxShape { WidthX = 1, WidthY = 2, WidthZ = 3 }", new BoxShape(1, 2, 3).ToString());
    }


    [Test]
    public void Clone()
    {
      BoxShape box = new BoxShape(0.1234f, 2.345f, 5.43f);
      BoxShape clone = box.Clone() as BoxShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(box.WidthX, clone.WidthX);
      Assert.AreEqual(box.WidthY, clone.WidthY);
      Assert.AreEqual(box.WidthZ, clone.WidthZ);
      Assert.AreEqual(box.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(box.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new BoxShape(11, 22, 33);
      
      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(Shape));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized BoxShape:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(Shape));
      var b = (BoxShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.WidthX, b.WidthX);
      Assert.AreEqual(a.WidthY, b.WidthY);
      Assert.AreEqual(a.WidthZ, b.WidthZ);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new BoxShape(11, 22, 33);

      // Serialize object.
      var stream = new MemoryStream();      
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (BoxShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.WidthX, b.WidthX);
      Assert.AreEqual(a.WidthY, b.WidthY);
      Assert.AreEqual(a.WidthZ, b.WidthZ);
    }


    [Test]
    public void GetMesh()
    {
      var s = new BoxShape(1, 2, 3);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.AreEqual(12, mesh.NumberOfTriangles);

      // No more tests necessary because we see the result in the samples.
    }
  }
}
