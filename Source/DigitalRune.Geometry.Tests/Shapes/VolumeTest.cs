using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Tests.Shapes
{
  [TestFixture]
  public class VolumeTest
  {
    [SetUp]
    public void SetUp()
    {
      RandomHelper.Random = new Random(1234567);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetShapeVolumeArgumentOutOfRangeException()
    {
      var s = new SphereShape(1);
      var c = new MinkowskiSumShape(new GeometricObject(s), new GeometricObject(new PointShape()));
      c.GetVolume(-1f, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetShapeVolumeArgumentOutOfRangeException2()
    {
      var s = new SphereShape(1);
      var c = new MinkowskiSumShape(new GeometricObject(s), new GeometricObject(new PointShape()));
      c.GetVolume(0.01f, -1);
    }


    [Test]
    public void BoxVolume()
    {
      var s = new BoxShape(1, 2, 3);
      Assert.AreEqual(1 * 2 * 3, s.GetVolume(0.0001f, 10));

      var m = s.GetMesh(0.001f, 4);

      Assert.AreEqual(1 * 2 * 3, m.GetVolume());
    }

    [Test]
    public void SphereTest()
    {
      var s = new SphereShape(1);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void ApproximateVolume()
    {
      var s = new SphereShape(1);
      var c = new MinkowskiSumShape(new GeometricObject(s), new GeometricObject(new PointShape()));
      var v = c.GetVolume(0.001f, 
                          0); // !!!

      // v is AABB volume.
      Assert.AreEqual(2 * 2 * 2, v);
    }


    [Test]
    public void CapsuleTest()
    {
      var s = new CapsuleShape(1, 2);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void ConeTest()
    {
      var s = new ConeShape(1, 2);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void CylinderTest()
    {
      var s = new CylinderShape(1, 2);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void ConvexHullOfPointsTest()
    {
      var s = new BoxShape(1, 2, 3);
      var v0 = s.GetVolume(0.001f, 10);

      var s1 = new ConvexHullOfPoints(s.GetMesh(0.1f, 1).Vertices);
      var v1 = s1.GetVolume(0.001f, 10);

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void TriangleMeshShapeTest()
    {
      var s = new CylinderShape(1, 2);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var s1 = new TriangleMeshShape(m);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void OrthographicViewVolumeTest()
    {
      var s = new OrthographicViewVolume(-1, 0, -1, 0, 0.1f, 10);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var s1 = new TriangleMeshShape(m);
      var v1 = m.GetVolume();

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void PerspectiveViewVolumeTest()
    {
      var s = new PerspectiveViewVolume(0.4f, 2, 0.1f, 1f);
      var v0 = s.GetVolume(0.001f, 10);

      var m = s.GetMesh(0.001f, 10);
      var s1 = new TriangleMeshShape(m);
      var v1 = s1.GetVolume(0.0001f, 10);

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void ScaledConvexShape()
    {
      var s = new CylinderShape(1, 2);
      var v0 = s.GetVolume(0.001f, 10);

      var s1 = new ScaledConvexShape(new CylinderShape(10, 10), new Vector3F(0.1f, 0.2f, 0.1f));
      var v1 = s1.GetVolume(0.0001f, 10);

      Assert.IsTrue(Numeric.AreEqual(v0, v1, 0.01f * (1 + v0)));  // 1% error is allowed.
    }


    [Test]
    public void CompositeShapeTest()
    {
      var c = new CompositeShape();

      Assert.AreEqual(0, c.GetVolume(0.1f, 10));

      c.Children.Add(
        new GeometricObject(
          new BoxShape(1, 2, 3),
          new Vector3F(10, 10, 10),
          new Pose(new Vector3F(1, 2, 3), RandomHelper.Random.NextQuaternionF())));
      c.Children.Add(
        new GeometricObject(
          new BoxShape(4, 5, 6),
          new Vector3F(2, 2, 2),
          new Pose(new Vector3F(10, -2, 0), RandomHelper.Random.NextQuaternionF())));

      var v0 = c.GetVolume(0.001f, 10);

      Assert.AreEqual(10 * 20 * 30 + 8 * 10 * 12, v0);
    }


    [Test]
    public void TransformedShapeTest()
    {
      var c = new TransformedShape();

      Assert.AreEqual(0, c.GetVolume(0.1f, 10));

      c.Child = new GeometricObject(
          new BoxShape(1, 2, 3),
          new Vector3F(10, 10, 10),
          new Pose(new Vector3F(1, 2, 3), RandomHelper.Random.NextQuaternionF()));

      var v0 = c.GetVolume(0.001f, 10);

      Assert.AreEqual(10 * 20 * 30, v0);
    }


    [Test]
    public void NoVolumeShapes()
    {
      Assert.AreEqual(0, new CircleShape(1).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new RectangleShape(1, 2).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new RayShape(Vector3F.Zero, Vector3F.UnitZ, 10).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new LineShape(Vector3F.Zero, Vector3F.UnitZ).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new LineSegmentShape(Vector3F.UnitY, Vector3F.UnitX).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new TriangleShape(Vector3F.UnitY, Vector3F.UnitX, Vector3F.Zero).GetVolume(0.001f, 20));
      Assert.AreEqual(0, new PointShape(Vector3F.UnitY).GetVolume(0.001f, 20));
      Assert.AreEqual(0, Shape.Empty.GetVolume(0.001f, 20));

      var s = new CircleShape(1);
      var m = s.GetMesh(0.001f, 10);
      var v1 = m.GetVolume();

      Assert.AreEqual(0, v1);
    }


    [Test]
    public void InfiniteVolumeShapes()
    {
      Assert.AreEqual(float.PositiveInfinity, new PlaneShape().GetVolume(0.001f, 20));
      Assert.AreEqual(float.PositiveInfinity, new HeightField(0, 0, 1, 2, new float[] { 1, 2, 0, 3 }, 2, 2).GetVolume(0.001f, 20));
    }


    [Test]
    public void EmptyTriangleMesh()
    {
      var m = new TriangleMesh();
      Assert.AreEqual(0, m.GetVolume());
    }
  }
}
