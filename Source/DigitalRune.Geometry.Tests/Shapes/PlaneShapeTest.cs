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
  public class PlaneShapeTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(0, 1, 0), new PlaneShape().Normal );
      Assert.AreEqual(0, new PlaneShape().DistanceFromOrigin);

      Vector3F normal = new Vector3F(1, 2, 3).Normalized;
      var distanceFromOrigin = 4;
      Assert.AreEqual(normal, new PlaneShape(normal, distanceFromOrigin).Normal);
      Assert.AreEqual(distanceFromOrigin, new PlaneShape(normal, distanceFromOrigin).DistanceFromOrigin);
      Assert.AreEqual(normal, new PlaneShape(new Plane(normal, distanceFromOrigin)).Normal);
      Assert.AreEqual(distanceFromOrigin, new PlaneShape(new Plane(normal, distanceFromOrigin)).DistanceFromOrigin);

      Assert.AreEqual(new Vector3F(-1, 0, 0), new PlaneShape(new Vector3F(-1, 0, 0), new Vector3F(-4, 1, 2)).Normal);
      Assert.AreEqual(distanceFromOrigin, new PlaneShape(new Vector3F(-1, 0, 0), new Vector3F(-4, 1, 2)).DistanceFromOrigin);

      Vector3F p0 = new Vector3F(1,4,0);
      Vector3F p1 = new Vector3F(0,4,-1);
      Vector3F p2 = new Vector3F(-1,4,0);
      Assert.AreEqual(new Vector3F(0, 1, 0).Normalized, new PlaneShape(p0, p1, p2).Normal);
      Assert.AreEqual(4, new PlaneShape(p0, p1, p2).DistanceFromOrigin);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException1()
    {
      new PlaneShape(new Vector3F(), 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new PlaneShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(1, 2, 3));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException3()
    {
      new PlaneShape(new Vector3F(), new Vector3F(4, 5, 6));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException4()
    {
      new PlaneShape(new Plane(new Vector3F(0.5f, 0, 0), 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException5()
    {
      new PlaneShape(new Vector3F(1, 2, 3), new Vector3F(11, 22, 33), new Vector3F(111, 222, 333));
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(12, 0, 0),  new PlaneShape(new Vector3F(-1, 0, 0), -12).InnerPoint);
    }


    [Test]
    public void Properties()
    {
      PlaneShape p = new PlaneShape();
      Assert.AreEqual(new Vector3F(0, 1, 0), p.Normal);
      Assert.AreEqual(0, p.DistanceFromOrigin);

      p.DistanceFromOrigin = -10;
      Assert.AreEqual(new Vector3F(0, 1, 0), p.Normal);
      Assert.AreEqual(-10, p.DistanceFromOrigin);

      var normal = new Vector3F(1, 2, 3).Normalized;
      p.Normal = normal;
      Assert.AreEqual(normal, p.Normal);
      Assert.AreEqual(-10, p.DistanceFromOrigin);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DirectionException()
    {
      PlaneShape p = new PlaneShape();
      p.Normal = new Vector3F();
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      float nInf = float.NegativeInfinity;
      float pInf = float.PositiveInfinity;
      Assert.AreEqual(new Aabb(new Vector3F(nInf, nInf, nInf), new Vector3F(pInf, 0, pInf)), new PlaneShape().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(nInf), new Vector3F(pInf)),
                     new PlaneShape().GetAabb(new Pose(new Vector3F(10, 100, -13),
                                                                         QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
      Assert.AreEqual(new Aabb(new Vector3F(nInf, nInf, nInf), new Vector3F(12, pInf, pInf)),
                      new PlaneShape(new Vector3F(1, 0, 0), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(8, nInf, nInf), new Vector3F(pInf, pInf, pInf)),
                      new PlaneShape(new Vector3F(-1, 0, 0), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(nInf, nInf, nInf), new Vector3F(pInf, 102, pInf)),
                      new PlaneShape(new Vector3F(0, 1, 0), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(nInf, 98, nInf), new Vector3F(pInf, pInf, pInf)),
                      new PlaneShape(new Vector3F(0, -1, 0), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(nInf, nInf, nInf), new Vector3F(pInf, pInf, 1002)),
                      new PlaneShape(new Vector3F(0, 0, 1), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(nInf, nInf, 998), new Vector3F(pInf, pInf, pInf)),
                      new PlaneShape(new Vector3F(0, 0, -1), 2).GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetMesh()
    {
      PlaneShape p = new PlaneShape(new Vector3F(1, 2, 3).Normalized, 10);

      var m = p.GetMesh(0, 1);
      Assert.Greater(m.NumberOfTriangles, 0);

      // Check if all vertices lie in the plane.
      for (int i = 0; i < m.NumberOfTriangles; i++)
      {
        Triangle t = m.GetTriangle(i);

        Assert.IsTrue(GeometryHelper.GetDistance(new Plane(p), t.Vertex0) < 0.0001f);
        Assert.IsTrue(GeometryHelper.GetDistance(new Plane(p), t.Vertex1) < 0.0001f);
        Assert.IsTrue(GeometryHelper.GetDistance(new Plane(p), t.Vertex2) < 0.0001f);
      }
    }


    [Test]
    public void GetIntersection()
    {
      Plane a = new Plane(new Vector3F(1, 2, 3).Normalized, 10);
      Plane b = new Plane(new Vector3F(-2, 7, -4).Normalized, -22);
      Plane c = new Plane(new Vector3F(34, -22, -6).Normalized, 5);
      Vector3F intersection = GeometryHelper.GetIntersection(a, b, c);
      Assert.IsTrue(GeometryHelper.GetDistance(a, intersection) < 0.00001f);
      Assert.IsTrue(GeometryHelper.GetDistance(b, intersection) < 0.00001f);
      Assert.IsTrue(GeometryHelper.GetDistance(c, intersection) < 0.00001f);

      a = new Plane(new Vector3F(1, 0, 0), 10);
      b = new Plane(new Vector3F(-2, 7, -4).Normalized, -22);
      c = new Plane(a.Normal, 5);
      intersection = GeometryHelper.GetIntersection(a, b, c);
      Assert.IsTrue(float.IsNaN(intersection.X));
      Assert.IsTrue(float.IsNaN(intersection.Y));
      Assert.IsTrue(float.IsNaN(intersection.Z));

      a = new Plane(new Vector3F(1, 0, 0).Normalized, 10);
      b = new Plane(new Vector3F(-2, 7, -4).Normalized, -22);
      c = new Plane(a.Normal, a.DistanceFromOrigin);
      intersection = GeometryHelper.GetIntersection(a, b, c);
      Assert.IsTrue(float.IsNaN(intersection.X));
      Assert.IsTrue(float.IsNaN(intersection.Y));
      Assert.IsTrue(float.IsNaN(intersection.Z));

      // Note: If vector a is e.g. (1, 2, 3).Normalized then an intersection is returned because
      // of numerical errors in matrix.TryInvert, the matrix looks non-singular.
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("PlaneShape { Normal = (0; 0; 1), DistanceFromOrigin = 3 }", new PlaneShape(new Vector3F(0, 0, 1), 3).ToString());
    }


    [Test]
    public void Clone()
    {
      PlaneShape plane = new PlaneShape(new Vector3F(1, 2, 3).Normalized, new Vector3F(2, 3, 4));
      PlaneShape clone = plane.Clone() as PlaneShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(plane.Normal, clone.Normal);
      Assert.AreEqual(plane.DistanceFromOrigin, clone.DistanceFromOrigin);
      Assert.AreEqual(plane.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(plane.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new PlaneShape(new Vector3F(1, 2, 3).Normalized, 44);

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
      var b = (PlaneShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Normal, b.Normal);
      Assert.AreEqual(a.DistanceFromOrigin, b.DistanceFromOrigin);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new PlaneShape(new Vector3F(1, 2, 3).Normalized, 44);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (PlaneShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Normal, b.Normal);
      Assert.AreEqual(a.DistanceFromOrigin, b.DistanceFromOrigin);
    }
  }
}
