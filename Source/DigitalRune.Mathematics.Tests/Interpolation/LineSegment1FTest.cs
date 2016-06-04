using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class LineSegment1FTest
  {
    [Test]
    public void Test()
    {
      var s = new LineSegment1F
      {
        Point1 = 1,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(1, s.GetPoint(0)));
      Assert.IsTrue(Numeric.AreEqual(8, s.GetPoint(1)));
      Assert.IsTrue(Numeric.AreEqual(1 + 7*0.3f, s.GetPoint(0.3f))); 
    }


    [Test]
    public void GetTangent()
    {
      var s = new LineSegment1F
      {
        Point1 = 1,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(7, s.GetTangent(0)));
      Assert.IsTrue(Numeric.AreEqual(7, s.GetTangent(0.3f)));
      Assert.IsTrue(Numeric.AreEqual(7, s.GetTangent(1)));
    }


    [Test]
    public void GetLength()
    {
      var s = new LineSegment1F
      {
        Point1 = 1,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(7, s.GetLength(0, 1, 100, Numeric.EpsilonF)));
      Assert.IsTrue(Numeric.AreEqual(7 * 0.3f, s.GetLength(0.6f, 0.3f, 100, Numeric.EpsilonF)));
      Assert.IsTrue(Numeric.AreEqual(7 * 0.3f, s.GetLength(0.1f, 0.4f, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      var s = new LineSegment1F
      {
        Point1 = 1,
        Point2 = 8,
      };
      var points = new List<float>();
      s.Flatten(points, 1, 1);
      Assert.AreEqual(2, points.Count);
      Assert.IsTrue(points.Contains(s.Point1));
      Assert.IsTrue(points.Contains(s.Point2));
    }
  }
}
