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
  public class PerspectiveViewVolumeTest
  {
    [Test]
    public void ConvertToHorizontalFieldOfViewTest()
    {
      float horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(60), 1);
      float expectedFieldOfView = MathHelper.ToRadians(60);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, horizontalFieldOfView));

      horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(60), (float)(4.0 / 3.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(75.178179);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, horizontalFieldOfView));

      horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(45), (float)(16.0 / 9.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(72.734351);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, horizontalFieldOfView));
    }

    [Test]
    public void ConvertToVerticalFieldOfViewTest()
    {
      float verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(90), 1);
      float expectedFieldOfView = MathHelper.ToRadians(90);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, verticalFieldOfView));

      verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(75), (float)(4.0 / 3.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(59.840444);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, verticalFieldOfView));

      verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(90), (float)(16.0 / 9.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(58.715507);
      Assert.IsTrue(Numeric.AreEqual(expectedFieldOfView, verticalFieldOfView));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetHorizontalViewException()
    {
      PerspectiveViewVolume.GetFieldOfViewX(0, 4.0f / 3.0f);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetHorizontalViewException2()
    {
      PerspectiveViewVolume.GetFieldOfViewX(ConstantsF.PiOver4, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetVerticalViewException()
    {
      PerspectiveViewVolume.GetFieldOfViewY(0, 4.0f / 3.0f);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetVerticalViewException2()
    {
      PerspectiveViewVolume.GetFieldOfViewY(ConstantsF.PiOver4, 0);
    }

    [Test]
    public void GetExtentTest()
    {
      float extent;
      extent = PerspectiveViewVolume.GetExtent(MathHelper.ToRadians(90), 1);
      Assert.IsTrue(Numeric.AreEqual(2, extent));

      extent = PerspectiveViewVolume.GetExtent(MathHelper.ToRadians(60), 10);
      Assert.IsTrue(Numeric.AreEqual(11.547005f, extent));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetExtentException()
    {
      PerspectiveViewVolume.GetExtent(0, 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetExtentException2()
    {
      PerspectiveViewVolume.GetExtent(ConstantsF.PiOver4, -0.1f);
    }

    [Test]
    public void GetWidthAndHeightTest()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, 1, out width, out height);
      Assert.IsTrue(Numeric.AreEqual(2, width));
      Assert.IsTrue(Numeric.AreEqual(2, height));

      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, out width, out height);
      Assert.IsTrue(Numeric.AreEqual(2.0528009f, width));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f, height));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetWidthAndHeightException()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(-0.1f, 1, 1, out width, out height);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetWidthAndHeightException2()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 0, 1, out width, out height);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetWidthAndHeightException3()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, -0.1f, out width, out height);
    }

    [Test]
    public void GetFieldOfViewTest()
    {
      float fieldOfView;
      fieldOfView = PerspectiveViewVolume.GetFieldOfView(2, 1);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(90), fieldOfView));

      fieldOfView = PerspectiveViewVolume.GetFieldOfView(1.1547005f, 1);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(60), fieldOfView));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetFieldOfViewException()
    {
      PerspectiveViewVolume.GetFieldOfView(-0.1f, 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetFieldOfViewException2()
    {
      PerspectiveViewVolume.GetFieldOfView(1, 0);
    }

    [Test]
    public void AabbTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(-1, 1, -1, 1, 2, 5);
      Aabb aabb = frustum.GetAabb(Pose.Identity);
      Assert.AreEqual(new Vector3F(-2.5f, -2.5f, -5), aabb.Minimum);
      Assert.AreEqual(new Vector3F(2.5f, 2.5f, -2), aabb.Maximum);

      frustum.Set(0, 2, 0, 2, 1, 5);
      aabb = frustum.GetAabb(Pose.Identity);
      Assert.AreEqual(new Vector3F(0f, 0, -5), aabb.Minimum);
      Assert.AreEqual(new Vector3F(10, 10, -1), aabb.Maximum);

      frustum.Set(1, 2, 1, 2, 1, 5);
      aabb = frustum.GetAabb(Pose.Identity);
      Assert.AreEqual(new Vector3F(1, 1, -5), aabb.Minimum);
      Assert.AreEqual(new Vector3F(10, 10, -1), aabb.Maximum);
    }


    [Test]
    public void PropertiesTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume
      {
        Left = -2,
        Right = 2,
        Bottom = -1,
        Top = 1,
        Near = 2,
        Far = 10
      };

      Assert.AreEqual(-2, frustum.Left);
      Assert.AreEqual(2, frustum.Right);
      Assert.AreEqual(-1, frustum.Bottom);
      Assert.AreEqual(1, frustum.Top);
      Assert.AreEqual(2, frustum.Near);
      Assert.AreEqual(10, frustum.Far);
      Assert.AreEqual(4, frustum.Width);
      Assert.AreEqual(2, frustum.Height);
      Assert.AreEqual(8, frustum.Depth);
      Assert.AreEqual(2, frustum.AspectRatio);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(90), frustum.FieldOfViewX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(53.130102f), frustum.FieldOfViewY));


      frustum = new PerspectiveViewVolume
      {
        Left = 2,
        Right = -2,
        Bottom = 1,
        Top = -1,
        Near = 10,
        Far = 2
      };

      Assert.AreEqual(2, frustum.Left);
      Assert.AreEqual(-2, frustum.Right);
      Assert.AreEqual(1, frustum.Bottom);
      Assert.AreEqual(-1, frustum.Top);
      Assert.AreEqual(10, frustum.Near);
      Assert.AreEqual(2, frustum.Far);
      Assert.AreEqual(4, frustum.Width);
      Assert.AreEqual(2, frustum.Height);
      Assert.AreEqual(8, frustum.Depth);
      Assert.AreEqual(2, frustum.AspectRatio);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(90), frustum.FieldOfViewX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(53.130102f), frustum.FieldOfViewY));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void NearException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume
      {
        Near = 0,
        Far = 10
      };
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void FarException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume
      {
        Near = 1,
        Far = 0
      };
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(2, 2, 3, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException2()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(1, 2, 4, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException3()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(1, 2, 3, 4, 6, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetException4()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(1, 2, 3, 4, 0, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException5()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.Set(1, 2, 3, 4, 1, 0);
    }

    [Test]
    public void SetFieldOfViewTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

      Assert.IsTrue(Numeric.AreEqual(-2.0528009f / 2.0f, frustum.Left));
      Assert.IsTrue(Numeric.AreEqual(2.0528009f / 2.0f, frustum.Right));
      Assert.IsTrue(Numeric.AreEqual(-1.1547005f / 2.0f, frustum.Bottom));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f / 2.0f, frustum.Top));
      Assert.AreEqual(1, frustum.Near);
      Assert.AreEqual(10, frustum.Far);
      Assert.IsTrue(Numeric.AreEqual(2.0528009f, frustum.Width));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f, frustum.Height));
      Assert.AreEqual(9, frustum.Depth);
      Assert.AreEqual(16.0f / 9.0f, frustum.AspectRatio);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(91.492843f), frustum.FieldOfViewX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(60), frustum.FieldOfViewY));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(0), 16.0f / 9.0f, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException2()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(180), 16.0f / 9.0f, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException3()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 0, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException4()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 0, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException5()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 0);
    }

    [Test]
    public void SetFieldOfView2Test()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

      PerspectiveViewVolume frustum2 = new PerspectiveViewVolume { Near = 1, Far = 10 };
      frustum2.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f);

      Assert.AreEqual(frustum.Near, frustum2.Near);
      Assert.AreEqual(frustum.Far, frustum2.Far);
    }


    [Test]
    public void SetWidthAndHeightTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(2.0528009f, 1.1547005f, 1, 10);

      Assert.IsTrue(Numeric.AreEqual(-2.0528009f / 2.0f, frustum.Left));
      Assert.IsTrue(Numeric.AreEqual(2.0528009f / 2.0f, frustum.Right));
      Assert.IsTrue(Numeric.AreEqual(-1.1547005f / 2.0f, frustum.Bottom));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f / 2.0f, frustum.Top));
      Assert.AreEqual(1, frustum.Near);
      Assert.AreEqual(10, frustum.Far);
      Assert.IsTrue(Numeric.AreEqual(2.0528009f, frustum.Width));
      Assert.IsTrue(Numeric.AreEqual(1.1547005f, frustum.Height));
      Assert.AreEqual(9, frustum.Depth);
      Assert.AreEqual(16.0f / 9.0f, frustum.AspectRatio);
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(91.492843f), frustum.FieldOfViewX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(60), frustum.FieldOfViewY));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(0, 1.1547005f, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException2()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(2.0528009f, 0, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException3()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(2.0528009f, 1.1547005f, 0, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetWidthAndHeightException4()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(2.0528009f, 1.1547005f, 1, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetWidthAndHeightException5()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(2.0528009f, 1.1547005f, 1, 1);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new PerspectiveViewVolume().ToString().Contains("PerspectiveViewVolume"));
    }

    [Test]
    public void InnerPointTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetWidthAndHeight(1, 1, 1, 10);
      Vector3F innerPoint = frustum.InnerPoint;
      Assert.AreEqual(0, innerPoint.X);
      Assert.AreEqual(0, innerPoint.Y);
      Assert.AreEqual(-5.5f, innerPoint.Z);
    }


    [Test]
    public void Clone()
    {
      PerspectiveViewVolume perspectiveViewVolume = new PerspectiveViewVolume(1.23f, 2.13f, 1.01f, 10.345f);
      PerspectiveViewVolume clone = perspectiveViewVolume.Clone() as PerspectiveViewVolume;
      Assert.IsNotNull(clone);
      Assert.AreEqual(perspectiveViewVolume.Left, clone.Left);
      Assert.AreEqual(perspectiveViewVolume.Right, clone.Right);
      Assert.AreEqual(perspectiveViewVolume.Bottom, clone.Bottom);
      Assert.AreEqual(perspectiveViewVolume.Top, clone.Top);
      Assert.AreEqual(perspectiveViewVolume.Near, clone.Near);
      Assert.AreEqual(perspectiveViewVolume.Far, clone.Far);
      Assert.AreEqual(perspectiveViewVolume.FieldOfViewX, clone.FieldOfViewX);
      Assert.AreEqual(perspectiveViewVolume.FieldOfViewY, clone.FieldOfViewY);
      Assert.AreEqual(perspectiveViewVolume.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(perspectiveViewVolume.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new PerspectiveViewVolume(1.23f, 2.13f, 1.01f, 10.345f);

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
      var b = (PerspectiveViewVolume)deserializer.Deserialize(stream);

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
      var a = new PerspectiveViewVolume(1.23f, 2.13f, 1.01f, 10.345f);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (PerspectiveViewVolume)deserializer.Deserialize(stream);

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