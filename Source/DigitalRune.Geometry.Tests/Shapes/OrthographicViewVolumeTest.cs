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
  public class OrthographicViewVolumeTest
  {
    [Test]
    public void AabbTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.Set(-1, 1, -1, 1, 2, 5);
      Aabb aabb = viewVolume.GetAabb(Pose.Identity);
      Assert.AreEqual(new Vector3F(-1, -1, -5), aabb.Minimum);
      Assert.AreEqual(new Vector3F(1, 1, -2), aabb.Maximum);
    }


    [Test]
    public void PropertiesTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume
      {
        Left = -2,
        Right = 2,
        Bottom = -1,
        Top = 1,
        Near = 2,
        Far = 10
      };

      Assert.AreEqual(-2, viewVolume.Left);
      Assert.AreEqual(2, viewVolume.Right);
      Assert.AreEqual(-1, viewVolume.Bottom);
      Assert.AreEqual(1, viewVolume.Top);
      Assert.AreEqual(2, viewVolume.Near);
      Assert.AreEqual(10, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(2, viewVolume.Height);
      Assert.AreEqual(8, viewVolume.Depth);
      Assert.AreEqual(2, viewVolume.AspectRatio);
      Assert.IsNaN(viewVolume.FieldOfViewX);
      Assert.IsNaN(viewVolume.FieldOfViewY);


      viewVolume = new OrthographicViewVolume
      {
        Left = 2,
        Right = -2,
        Bottom = 1,
        Top = -1,
        Near = 10,
        Far = 2
      };

      Assert.AreEqual(2, viewVolume.Left);
      Assert.AreEqual(-2, viewVolume.Right);
      Assert.AreEqual(1, viewVolume.Bottom);
      Assert.AreEqual(-1, viewVolume.Top);
      Assert.AreEqual(10, viewVolume.Near);
      Assert.AreEqual(2, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(2, viewVolume.Height);
      Assert.AreEqual(8, viewVolume.Depth);
      Assert.AreEqual(2, viewVolume.AspectRatio);
      Assert.IsNaN(viewVolume.FieldOfViewX);
      Assert.IsNaN(viewVolume.FieldOfViewY);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.Set(2, 2, 3, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException2()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.Set(1, 2, 4, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException3()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.Set(1, 2, 3, 4, 6, 6);
    }

    [Test]
    public void SetWidthAndHeightTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(4, 3, 2, 9);

      Assert.AreEqual(-2, viewVolume.Left);
      Assert.AreEqual(2, viewVolume.Right);
      Assert.AreEqual(-1.5f, viewVolume.Bottom);
      Assert.AreEqual(1.5f, viewVolume.Top);
      Assert.AreEqual(2, viewVolume.Near);
      Assert.AreEqual(9, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(3, viewVolume.Height);
      Assert.AreEqual(7, viewVolume.Depth);
      Assert.AreEqual(4.0f / 3.0f, viewVolume.AspectRatio);
      Assert.IsNaN(viewVolume.FieldOfViewX);
      Assert.IsNaN(viewVolume.FieldOfViewY);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(0, 1, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException2()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(2, 0, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetWidthAndHeightException3()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(2, 1, 1, 0);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new OrthographicViewVolume().ToString().Contains("OrthographicViewVolume"));
    }

    [Test]
    public void InnerPointTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(1, 1, 1, 10);
      Vector3F innerPoint = viewVolume.InnerPoint;
      Assert.AreEqual(0, innerPoint.X);
      Assert.AreEqual(0, innerPoint.Y);
      Assert.AreEqual(-5.5f, innerPoint.Z);
    }


    [Test]
    public void Clone()
    {
      OrthographicViewVolume orthographicViewVolume = new OrthographicViewVolume(-1.23f, 2.13f, -0.3f, 2.34f, 1.01f, 10.345f);
      OrthographicViewVolume clone = orthographicViewVolume.Clone() as OrthographicViewVolume;
      Assert.IsNotNull(clone);
      Assert.AreEqual(orthographicViewVolume.Left, clone.Left);
      Assert.AreEqual(orthographicViewVolume.Right, clone.Right);
      Assert.AreEqual(orthographicViewVolume.Bottom, clone.Bottom);
      Assert.AreEqual(orthographicViewVolume.Top, clone.Top);
      Assert.AreEqual(orthographicViewVolume.Near, clone.Near);
      Assert.AreEqual(orthographicViewVolume.Far, clone.Far);
      Assert.AreEqual(orthographicViewVolume.FieldOfViewX, clone.FieldOfViewX);
      Assert.AreEqual(orthographicViewVolume.FieldOfViewY, clone.FieldOfViewY);
      Assert.AreEqual(orthographicViewVolume.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(orthographicViewVolume.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new OrthographicViewVolume(-1.23f, 2.13f, -0.3f, 2.34f, 1.01f, 10.345f);

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
      var b = (OrthographicViewVolume)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Left, b.Left);
      Assert.AreEqual(a.Right, b.Right);
      Assert.AreEqual(a.Top, b.Top);
      Assert.AreEqual(a.Bottom, b.Bottom);
      Assert.AreEqual(a.Near, b.Near);
      Assert.AreEqual(a.Far, b.Far);
      Assert.AreEqual(a.InnerPoint, b.InnerPoint);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new OrthographicViewVolume(-1.23f, 2.13f, -0.3f, 2.34f, 1.01f, 10.345f);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (OrthographicViewVolume)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Left, b.Left);
      Assert.AreEqual(a.Right, b.Right);
      Assert.AreEqual(a.Top, b.Top);
      Assert.AreEqual(a.Bottom, b.Bottom);
      Assert.AreEqual(a.Near, b.Near);
      Assert.AreEqual(a.Far, b.Far);
      Assert.AreEqual(a.InnerPoint, b.InnerPoint);
    }
  }
}