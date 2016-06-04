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
  public class RectangleTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new RectangleShape().WidthX);
      Assert.AreEqual(0, new RectangleShape().WidthY);

      Assert.AreEqual(0, new RectangleShape(0, 0).WidthX);
      Assert.AreEqual(0, new RectangleShape(0, 0).WidthY);

      Assert.AreEqual(10, new RectangleShape(10, 11).WidthX);
      Assert.AreEqual(11, new RectangleShape(10, 11).WidthY);

      Assert.AreEqual(10, new RectangleShape(new Vector2F(10, 11)).WidthX);
      Assert.AreEqual(11, new RectangleShape(new Vector2F(10, 11)).WidthY);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new RectangleShape(-10, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException2()
    {
      new RectangleShape(1, -10);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new RectangleShape(2, 4).InnerPoint);
    }


    [Test]
    public void Width()
    {
      RectangleShape b = new RectangleShape();
      Assert.AreEqual(0, b.WidthX);
      Assert.AreEqual(0, b.WidthY);

      b.WidthX = 10;
      Assert.AreEqual(10, b.WidthX);
      Assert.AreEqual(0, b.WidthY);

      b.WidthY = 11;
      Assert.AreEqual(10, b.WidthX);
      Assert.AreEqual(11, b.WidthY);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WidthXException()
    {
      RectangleShape b = new RectangleShape();
      b.WidthX = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WidthYException()
    {
      RectangleShape b = new RectangleShape();
      b.WidthY = -1;
    }


    [Test]
    public void Extent()
    {
      RectangleShape r = new RectangleShape();

      r.Extent = new Vector2F(1, 2);
      Assert.AreEqual(1, r.WidthX);
      Assert.AreEqual(2, r.WidthY);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ExtentXException()
    {
      RectangleShape r = new RectangleShape();
      r.Extent = new Vector2F(-1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ExtentYException()
    {
      RectangleShape r = new RectangleShape();
      r.Extent = new Vector2F(1, -1);
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new RectangleShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
                     new RectangleShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(5, 90, 1000), new Vector3F(15, 110, 1000)),
                     new RectangleShape(10, 20).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(5, 100, 990), new Vector3F(15, 100, 1010)),
                     new RectangleShape(10, 20).GetAabb(new Pose(new Vector3F(10, 100, 1000),
                                                                   QuaternionF.CreateRotationX(ConstantsF.PiOver2))));
      // TODO: Test complex rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new RectangleShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new RectangleShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new RectangleShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new RectangleShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.AreEqual(new Vector3F(5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(-5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(-1, 0, 0)));
      Assert.AreEqual(new Vector3F(5, -10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(0, -1, 0)));
      Assert.AreEqual(new Vector3F(5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(new Vector3F(5, 10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(1, 1, 1)));
      Assert.AreEqual(new Vector3F(-5, -10, 0), new RectangleShape(10, 20).GetSupportPoint(new Vector3F(-1, -1, -1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new RectangleShape().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new RectangleShape().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new RectangleShape().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new RectangleShape().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.IsTrue(Numeric.AreEqual(5, new RectangleShape(10, 20).GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(10, new RectangleShape(10, 20).GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(0, new RectangleShape(10, 20).GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(5, 10, 0), new Vector3F(-1, -1, 0)).Length, new RectangleShape(10, 20).GetSupportPointDistance(new Vector3F(-1, -1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(5, 10, 0), new Vector3F(1, 1, 1)).Length, new RectangleShape(10, 20).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("RectangleShape { WidthX = 1, WidthY = 2 }", new RectangleShape(1, 2).ToString());
    }


    [Test]
    public void Clone()
    {
      RectangleShape rectangle = new RectangleShape(0.1234f, 2.345f);
      RectangleShape clone = rectangle.Clone() as RectangleShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(rectangle.WidthX, clone.WidthX);
      Assert.AreEqual(rectangle.WidthY, clone.WidthY);
      Assert.AreEqual(rectangle.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(rectangle.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new RectangleShape(11, 22);

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
      var b = (RectangleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.WidthX, b.WidthX);
      Assert.AreEqual(a.WidthY, b.WidthY);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new RectangleShape(11, 22);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (RectangleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.WidthX, b.WidthX);
      Assert.AreEqual(a.WidthY, b.WidthY);
    }
  }
}
