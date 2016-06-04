using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class LineSegmentTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new LineSegment().Start);
      Assert.AreEqual(new Vector3F(), new LineSegment().End);

      Assert.AreEqual(new Vector3F(), new LineSegment(Vector3F.Zero, Vector3F.Zero).Start);
      Assert.AreEqual(new Vector3F(), new LineSegment(Vector3F.Zero, Vector3F.Zero).End);

      Assert.AreEqual(new Vector3F(10, 20, 30), new LineSegment(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).Start);
      Assert.AreEqual(new Vector3F(11, 22, 33), new LineSegment(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).End);

      Assert.AreEqual(new Vector3F(10, 20, 30), new LineSegment(new LineSegmentShape(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33))).Start);
      Assert.AreEqual(new Vector3F(11, 22, 33), new LineSegment(new LineSegmentShape(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33))).End);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorArgumentNullException()
    {
      new LineSegment(null);
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new LineSegment().Equals(new LineSegment()));
      Assert.IsTrue(new LineSegment().Equals(new LineSegment(Vector3F.Zero, Vector3F.Zero)));
      Assert.IsTrue(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsTrue(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals((object)new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new LineSegment(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new Line(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new LineSegment().Equals(null));

      Assert.IsTrue(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)) == new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
      Assert.IsTrue(new LineSegment(new Vector3F(1, 2, 4), new Vector3F(4, 5, 6)) != new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
    }


    [Test]
    public void LengthTest()
    {
      Assert.AreEqual(2, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(3, 2, 3)).Length);
    }


    [Test]
    public void LengthSquaredTest()
    {
      Assert.AreEqual(4, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(3, 2, 3)).LengthSquared);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new LineSegment().GetHashCode(), new LineSegment().GetHashCode());
      Assert.AreEqual(new LineSegment().GetHashCode(), new LineSegment(Vector3F.Zero, Vector3F.Zero).GetHashCode());
      Assert.AreEqual(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new LineSegment(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("LineSegment { Start = (1; 2; 3), End = (4; 5; 6) }", new LineSegment(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).ToString());
    }
  }
}
