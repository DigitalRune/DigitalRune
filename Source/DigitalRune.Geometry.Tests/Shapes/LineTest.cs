using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class LineTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new Line().PointOnLine);
      Assert.AreEqual(new Vector3F(), new Line().Direction);

      Assert.AreEqual(new Vector3F(), new Line(Vector3F.Zero, Vector3F.Zero).PointOnLine);
      Assert.AreEqual(new Vector3F(), new Line(Vector3F.Zero, Vector3F.Zero).Direction);

      Assert.AreEqual(new Vector3F(10, 20, 30), new Line(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).PointOnLine);
      Assert.AreEqual(new Vector3F(11, 22, 33), new Line(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).Direction);

      Assert.AreEqual(new Vector3F(10, 20, 30), new Line(new LineShape(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33).Normalized)).PointOnLine);
      Assert.AreEqual(new Vector3F(11, 22, 33).Normalized, new Line(new LineShape(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33).Normalized)).Direction);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorArgumentNullException()
    {
      new Line(null);
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new Line().Equals(new Line()));
      Assert.IsTrue(new Line().Equals(new Line(Vector3F.Zero, Vector3F.Zero)));
      Assert.IsTrue(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsTrue(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals((object)new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new Line(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Line().Equals(null));

      Assert.IsTrue(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)) == new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
      Assert.IsTrue(new Line(new Vector3F(1, 2, 4), new Vector3F(4, 5, 6)) != new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new Line().GetHashCode(), new Line().GetHashCode());
      Assert.AreEqual(new Line().GetHashCode(), new Line(Vector3F.Zero, Vector3F.Zero).GetHashCode());
      Assert.AreEqual(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new Line(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("Line { PointOnLine = (1; 2; 3), Direction = (4; 5; 6) }", new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).ToString());
    }


    [Test]
    public void PositiveUniformScaling()
    {
      Vector3F point0 = new Vector3F(10, 20, -40);
      Vector3F point1 = new Vector3F(-22, 34, 45);
      Line line = new Line(point0, (point1 - point0).Normalized);

      Vector3F scale = new Vector3F(3.5f);
      point0 *= scale;
      point1 *= scale;
      line.Scale(ref scale);

      Vector3F dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point1, out dummy));
    }


    [Test]
    public void NegativeUniformScaling()
    {
      Vector3F point0 = new Vector3F(10, 20, -40);
      Vector3F point1 = new Vector3F(-22, 34, 45);
      Line line = new Line(point0, (point1 - point0).Normalized);

      Vector3F scale = new Vector3F(-3.5f);
      point0 *= scale;
      point1 *= scale;
      line.Scale(ref scale);

      Vector3F dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point1, out dummy));
    }


    [Test]
    public void NonuniformScaling()
    {
      Vector3F scale = new Vector3F(1, 2, 3);
      Assert.That(() => new Line().Scale(ref scale), Throws.Exception.TypeOf<NotSupportedException>());
    }


    [Test]
    public void ToWorld()
    {
      Vector3F point0 = new Vector3F(10, 20, -40);
      Vector3F point1 = new Vector3F(-22, 34, 45);
      Line line = new Line(point0, (point1 - point0).Normalized);

      Pose pose = new Pose(new Vector3F(-5, 100, -20), Matrix33F.CreateRotation(new Vector3F(1, 2, 3), 0.123f));
      point0 = pose.ToWorldPosition(point0);
      point1 = pose.ToWorldPosition(point1);
      line.ToWorld(ref pose);

      Vector3F dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point1, out dummy));
    }


    [Test]
    public void ToLocal()
    {
      Vector3F point0 = new Vector3F(10, 20, -40);
      Vector3F point1 = new Vector3F(-22, 34, 45);
      Line line = new Line(point0, (point1 - point0).Normalized);

      Pose pose = new Pose(new Vector3F(-5, 100, -20), Matrix33F.CreateRotation(new Vector3F(1, 2, 3), 0.123f));
      point0 = pose.ToLocalPosition(point0);
      point1 = pose.ToLocalPosition(point1);
      line.ToLocal(ref pose);

      Vector3F dummy;
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point0, out dummy));
      Assert.IsTrue(GeometryHelper.GetClosestPoint(line, point1, out dummy));
    }
  }
}
