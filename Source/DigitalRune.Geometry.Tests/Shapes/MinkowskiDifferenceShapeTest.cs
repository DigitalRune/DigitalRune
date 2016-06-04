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
  public class MinkowskiDifferenceShapeTest
  {
    MinkowskiDifferenceShape cs;
    
    [SetUp]
    public void SetUp()
    {
      cs = new MinkowskiDifferenceShape();
      cs.ObjectA = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(1, 0, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));
      cs.ObjectB = new GeometricObject(new LineSegmentShape(new Vector3F(0, 5, 0), new Vector3F(0, -5, 0)), Pose.Identity);
    }

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(Vector3F.Zero, ((PointShape)new MinkowskiDifferenceShape().ObjectA.Shape).Position);
      Assert.AreEqual(Vector3F.Zero, ((PointShape)new MinkowskiDifferenceShape().ObjectB.Shape).Position);
      Assert.AreEqual(Pose.Identity, new MinkowskiDifferenceShape().ObjectA.Pose);
      Assert.AreEqual(Pose.Identity, new MinkowskiDifferenceShape().ObjectB.Pose);

      var m = new MinkowskiDifferenceShape(
        new GeometricObject(new CircleShape(3), new Pose(new Vector3F(1, 0, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2))),
        new GeometricObject(new LineSegmentShape(new Vector3F(0, 5, 0), new Vector3F(0, -5, 0)), Pose.Identity));
      Assert.AreEqual(new Vector3F(1, 0, 0), m.ObjectA.Pose.Position);
      Assert.AreEqual(new Vector3F(0, 0, 0), m.ObjectB.Pose.Position);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      var m = new MinkowskiDifferenceShape(null, new GeometricObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException2()
    {
      var m = new MinkowskiDifferenceShape(new GeometricObject(), null);
    }


    [Test]
    public void PropertiesTest()
    {
      Assert.AreEqual(3, ((CircleShape)cs.ObjectA.Shape).Radius);
      Assert.AreEqual(new Vector3F(0, 5, 0), ((LineSegmentShape)cs.ObjectB.Shape).Start);
      Assert.AreEqual(new Vector3F(0, -5, 0), ((LineSegmentShape) cs.ObjectB.Shape).End);
      Assert.AreEqual(new Pose(new Vector3F(1, 0, 0), QuaternionF.CreateRotationX(ConstantsF.PiOver2)), cs.ObjectA.Pose);
      Assert.AreEqual(Pose.Identity, cs.ObjectB.Pose);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException()
    {
      cs.ObjectA = null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException2()
    {
      cs.ObjectB = null;
    }    


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(1, 0, 0), cs.InnerPoint);
    }


    //[Test]
    //public void GetAabb()
    //{
    //  Assert.AreEqual(new Aabb(), new ConvexHullOfPoints().GetAabb(Pose.Identity));
    //  Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
    //                 new ConvexHullOfPoints().GetAabb(new Pose(new Vector3F(10, 100, -13),
    //                                                                     QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
    //  Assert.AreEqual(new Aabb(new Vector3F(11, 102, 1003), new Vector3F(11, 102, 1003)),
    //                 new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000),
    //                                                                     QuaternionF.Identity)));
    //  QuaternionF rotation = QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f);
    //  Vector3F worldPos = rotation.Rotate(new Vector3F(1, 2, 3)) + new Vector3F(10, 100, 1000);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Minimum));
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Maximum));
    //}


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(4, 5, 0), cs.GetSupportPoint(new Vector3F(1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(4, 5, 0), cs.GetSupportPoint(new Vector3F(0, 1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(1, 5, 3), cs.GetSupportPoint(new Vector3F(0, 0, 1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-2, 5, 0), cs.GetSupportPoint(new Vector3F(-1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(4, -5, 0), cs.GetSupportPoint(new Vector3F(0, -1, 0))));

      MinkowskiDifferenceShape m = new MinkowskiDifferenceShape();
      ((GeometricObject)m.ObjectB).Shape = new LineSegmentShape(new Vector3F(1, 0, 0), new Vector3F(3, 0, 0));
      Assert.AreEqual(new Vector3F(-1, 0, 0), m.GetSupportPoint(new Vector3F(1, 1, 0)));
      ((GeometricObject)m.ObjectB).Pose = new Pose(new Vector3F(1, 1, 0), QuaternionF.Identity);
      Assert.AreEqual(new Vector3F(-2, -1, 0), m.GetSupportPoint(new Vector3F(1, 1, 0)));
      ((GeometricObject)m.ObjectA).Shape = new CircleShape(20);
      Assert.AreEqual(new Vector3F(18, -1, 0), m.GetSupportPoint(new Vector3F(1, 0, 0)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Assert.AreEqual(1, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(2, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(3, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(1, 2, 3), new Vector3F(1, 1, 1)).Length, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    //[Test]
    //public void ToStringTest()
    //{
    //  Assert.AreEqual("MinkowskiDifferenceShape()", cs.ToString());
    //}


    [Test]
    public void Clone()
    {
      Pose poseA = new Pose(new Vector3F(1, 2, 3));
      PointShape pointA = new PointShape(3, 4, 5);
      GeometricObject geometryA = new GeometricObject(pointA, poseA);

      Pose poseB = new Pose(new Vector3F(1, 2, 3));
      PointShape pointB = new PointShape(3, 4, 5);
      GeometricObject geometryB = new GeometricObject(pointB, poseB);

      MinkowskiDifferenceShape minkowskiDifferenceShape = new MinkowskiDifferenceShape(geometryA, geometryB);
      MinkowskiDifferenceShape clone = minkowskiDifferenceShape.Clone() as MinkowskiDifferenceShape;
      Assert.IsNotNull(clone);
      Assert.IsNotNull(clone.ObjectA);
      Assert.IsNotNull(clone.ObjectB);
      Assert.AreNotSame(geometryA, clone.ObjectA);
      Assert.AreNotSame(geometryB, clone.ObjectB);
      Assert.IsTrue(clone.ObjectA is GeometricObject);
      Assert.IsTrue(clone.ObjectB is GeometricObject);
      Assert.AreEqual(poseA, clone.ObjectA.Pose);
      Assert.AreEqual(poseB, clone.ObjectB.Pose);
      Assert.IsNotNull(clone.ObjectA.Shape);
      Assert.IsNotNull(clone.ObjectB.Shape);
      Assert.AreNotSame(pointA, clone.ObjectA.Shape);
      Assert.AreNotSame(pointB, clone.ObjectB.Shape);
      Assert.IsTrue(clone.ObjectA.Shape is PointShape);
      Assert.IsTrue(clone.ObjectB.Shape is PointShape);
      Assert.AreEqual(pointA.Position, ((PointShape)clone.ObjectA.Shape).Position);
      Assert.AreEqual(pointB.Position, ((PointShape)clone.ObjectB.Shape).Position);
      Assert.AreEqual(minkowskiDifferenceShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(minkowskiDifferenceShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    //[Test]
    //public void SerializationXml()
    //{
    //  Pose poseA = new Pose(new Vector3F(1, 2, 3));
    //  PointShape pointA = new PointShape(3, 4, 5);
    //  GeometricObject geometryA = new GeometricObject(pointA, poseA);

    //  Pose poseB = new Pose(new Vector3F(11, 22, 33));
    //  PointShape pointB = new PointShape(33, 44, 55);
    //  GeometricObject geometryB = new GeometricObject(pointB, poseB);

    //  var a = new MinkowskiDifferenceShape(geometryA, geometryB);

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
    //  var b = (MinkowskiDifferenceShape)deserializer.Deserialize(stream);

    //  Assert.AreEqual(a.ObjectA.Pose, b.ObjectA.Pose);
    //  Assert.AreEqual(a.ObjectB.Pose, b.ObjectB.Pose);
    //  Assert.AreEqual(((PointShape)a.ObjectA.Shape).Position, ((PointShape)b.ObjectA.Shape).Position);
    //  Assert.AreEqual(((PointShape)a.ObjectB.Shape).Position, ((PointShape)b.ObjectB.Shape).Position);
    //}


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Pose poseA = new Pose(new Vector3F(1, 2, 3));
      PointShape pointA = new PointShape(3, 4, 5);
      GeometricObject geometryA = new GeometricObject(pointA, poseA);

      Pose poseB = new Pose(new Vector3F(11, 22, 33));
      PointShape pointB = new PointShape(33, 44, 55);
      GeometricObject geometryB = new GeometricObject(pointB, poseB);

      var a = new MinkowskiDifferenceShape(geometryA, geometryB);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (MinkowskiDifferenceShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.ObjectA.Pose, b.ObjectA.Pose);
      Assert.AreEqual(a.ObjectB.Pose, b.ObjectB.Pose);
      Assert.AreEqual(((PointShape)a.ObjectA.Shape).Position, ((PointShape)b.ObjectA.Shape).Position);
      Assert.AreEqual(((PointShape)a.ObjectB.Shape).Position, ((PointShape)b.ObjectB.Shape).Position);
    }
  }
}
