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
  public class TransformShapeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreNotEqual(null, new TransformedShape().Child);
      Assert.AreEqual(Vector3F.Zero, new TransformedShape().InnerPoint);      
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GeometryException()
    {
      TransformedShape t = new TransformedShape();
      t.Child = null;
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(Pose.Identity, new TransformedShape().Child.Pose);

      Assert.AreEqual(new Vector3F(0, 0, 0), new CompositeShape().InnerPoint);
    }


    [Test]
    public void InnerPoint2()
    {
      TransformedShape t = new TransformedShape
      {
        Child = new GeometricObject
        {
          Pose = new Pose(new Vector3F(0, 1, 0)),
          Shape = new PointShape(1, 0, 0),
        },
      };

      Assert.AreEqual(new Vector3F(1, 1, 0), t.InnerPoint);
    }


    [Test]
    public void GetAabb()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new TransformedShape().GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(new Vector3F(0, 0, 0), new TransformedShape().GetAabb(Pose.Identity).Maximum);

      TransformedShape t = new TransformedShape
      {
        Child = new GeometricObject
        {
          Pose = new Pose(new Vector3F(0, 1, 0)),
          Shape = new SphereShape(10),
        },
      };

      Assert.AreEqual(new Vector3F(-10, -9, -10), t.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(new Vector3F(10, 11, 10), t.GetAabb(Pose.Identity).Maximum);

      Assert.AreEqual(new Vector3F(-8, -9, -10), t.GetAabb(new Pose(new Vector3F(2, 0, 0))).Minimum);
      Assert.AreEqual(new Vector3F(12, 11, 10), t.GetAabb(new Pose(new Vector3F(2, 0, 0))).Maximum);
    }


    private bool _propertyChanged;
    [Test]
    public void PropertyChangedTest()
    {
      TransformedShape t = new TransformedShape();
      t.Changed += delegate { _propertyChanged = true; };

      Assert.IsFalse(_propertyChanged);

      ((GeometricObject)t.Child).Shape = new SphereShape(1);
      Assert.IsTrue(_propertyChanged);
      _propertyChanged = false;

      ((SphereShape) t.Child.Shape).Radius = 3;
      Assert.IsTrue(_propertyChanged);
      _propertyChanged = false;

      ((GeometricObject)t.Child).Pose = new Pose(new Vector3F(1, 2, 3));
      Assert.IsTrue(_propertyChanged);
      _propertyChanged = false;

      // Setting Pose to the same value does not create a changed event.
      ((GeometricObject)t.Child).Pose = new Pose(new Vector3F(1, 2, 3));
      Assert.IsFalse(_propertyChanged);
      _propertyChanged = false;

      ((GeometricObject)t.Child).Pose = Pose.Identity;
      Assert.IsTrue(_propertyChanged);
      _propertyChanged = false;

      t.Child = new GeometricObject();
      Assert.IsTrue(_propertyChanged);
      _propertyChanged = false;

      // Setting Pose to the same value does not create a changed event.
      ((GeometricObject)t.Child).Pose = Pose.Identity;
      Assert.IsFalse(_propertyChanged);
      _propertyChanged = false;
    }


    [Test]
    public void Clone()
    {
      Pose pose = new Pose(new Vector3F(1, 2, 3));
      PointShape pointShape = new PointShape(3, 4, 5);
      GeometricObject geometry = new GeometricObject(pointShape, pose);

      TransformedShape transformedShape = new TransformedShape(geometry);
      TransformedShape clone = transformedShape.Clone() as TransformedShape;
      Assert.IsNotNull(clone);
      Assert.IsNotNull(clone.Child);
      Assert.AreNotSame(geometry, clone.Child);
      Assert.IsTrue(clone.Child is GeometricObject);
      Assert.AreEqual(pose, clone.Child.Pose);
      Assert.IsNotNull(clone.Child.Shape);
      Assert.AreNotSame(pointShape, clone.Child.Shape);
      Assert.IsTrue(clone.Child.Shape is PointShape);
      Assert.AreEqual(pointShape.Position, ((PointShape)clone.Child.Shape).Position);
      Assert.AreEqual(transformedShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(transformedShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    //[Test]
    //public void SerializationXml()
    //{
    //  Pose pose = new Pose(new Vector3F(1, 2, 3));
    //  PointShape pointShape = new PointShape(3, 4, 5);
    //  var a = new TransformedShape(new GeometricObject(pointShape, pose));

    //  // Serialize object.
    //  var stream = new MemoryStream();
    //  var serializer = new XmlSerializer(typeof(Shape));
    //  serializer.Serialize(stream, a);

    //  // Output generated xml. Can be manually checked in output window.
    //  stream.Position = 0;
    //  var xml = new StreamReader(stream).ReadToEnd();
    //  Trace.WriteLine("Serialized Object:\n" + xml);

    //  // Deserialize object.
    //  stream.Position = 0;
    //  var deserializer = new XmlSerializer(typeof(Shape));
    //  var b = (TransformedShape)deserializer.Deserialize(stream);

    //  Assert.AreEqual(a.Child.Pose, b.Child.Pose);
    //  Assert.AreEqual(((PointShape)a.Child.Shape).Position, ((PointShape)b.Child.Shape).Position);
    //}


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Pose pose = new Pose(new Vector3F(1, 2, 3));
      PointShape pointShape = new PointShape(3, 4, 5);
      var a = new TransformedShape(new GeometricObject(pointShape, pose));

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (TransformedShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Child.Pose, b.Child.Pose);
      Assert.AreEqual(((PointShape)a.Child.Shape).Position, ((PointShape)b.Child.Shape).Position);
    }
  }
}
