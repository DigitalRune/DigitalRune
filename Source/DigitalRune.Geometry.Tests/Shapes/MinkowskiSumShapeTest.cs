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
  public class MinkowskiSumShapeTest
  {
    GeometricObject child0, child1;
    MinkowskiSumShape cs;
    
    [SetUp]
    public void SetUp()
    {
      child0 = new GeometricObject(new CircleShape(3), new Pose(new Vector3F(), QuaternionF.CreateRotationX(ConstantsF.PiOver2)));
      child1 = new GeometricObject(new LineSegmentShape(new Vector3F(0, 5, 0), new Vector3F(0, -5, 0)), Pose.Identity);

      cs = new MinkowskiSumShape 
      {
        ObjectA = child0, 
        ObjectB = child1
      };
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(0, 0, 0), new ConvexHullOfShapes().InnerPoint);
      Assert.AreEqual(new Vector3F(0, 0, 0), cs.InnerPoint);
      cs.ObjectB = new GeometricObject(new PointShape(new Vector3F(5, 0, 0)), new Pose(new Vector3F(1, 0, 0), QuaternionF.Identity));
      Assert.AreEqual(new Vector3F(6, 0, 0), cs.InnerPoint);
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
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3F(1, 1, 1)));

      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, -5, 0), cs.GetSupportPoint(new Vector3F(1, -1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, 5, 0), cs.GetSupportPoint(new Vector3F(1, 1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -5, 3), cs.GetSupportPoint(new Vector3F(0, 0, 1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-3, -5, 0), cs.GetSupportPoint(new Vector3F(-1, 0, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(3, -5, 0), cs.GetSupportPoint(new Vector3F(0, -1, 0))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -5, -3), cs.GetSupportPoint(new Vector3F(0, 0, -1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 5, 0) + 3 * new Vector3F(1, 0, 1).Normalized, cs.GetSupportPoint(new Vector3F(1, 1, 1))));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -5, 0) + 3 * new Vector3F(-1, 0, -1).Normalized, cs.GetSupportPoint(new Vector3F(-1, -1, -1))));
    }


    //[Test]
    //public void ToStringTest()
    //{
    //  Assert.AreEqual("MinkowskiSumShape()", cs.ToString());
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

      MinkowskiSumShape minkowskiSumShape = new MinkowskiSumShape(geometryA, geometryB);
      MinkowskiSumShape clone = minkowskiSumShape.Clone() as MinkowskiSumShape;
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
      Assert.AreEqual(minkowskiSumShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(minkowskiSumShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
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

    //  var a = new MinkowskiSumShape(geometryA, geometryB);

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
    //  var b = (MinkowskiSumShape)deserializer.Deserialize(stream);

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

      var a = new MinkowskiSumShape(geometryA, geometryB);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (MinkowskiSumShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.ObjectA.Pose, b.ObjectA.Pose);
      Assert.AreEqual(a.ObjectB.Pose, b.ObjectB.Pose);
      Assert.AreEqual(((PointShape)a.ObjectA.Shape).Position, ((PointShape)b.ObjectA.Shape).Position);
      Assert.AreEqual(((PointShape)a.ObjectB.Shape).Position, ((PointShape)b.ObjectB.Shape).Position);
    }
  }
}
