using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class BSplineSegment1FTest
  {
    [Test]
    public void GetPoint()
    {
      BSplineSegment1F b = new BSplineSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };

      Assert.Less(1, b.GetPoint(0));
      Assert.Greater(8, b.GetPoint(1));
    }


    [Test]
    public void GetTangent()
    {
      BSplineSegment1F b = new BSplineSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };

      Assert.Less(0, b.GetTangent(0));
      Assert.Greater(8, b.GetTangent(1));

      BSplineSegment1F bSymmetric = new BSplineSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 9,
      };
      Assert.IsTrue(Numeric.AreEqual(bSymmetric.GetTangent(0), bSymmetric.GetTangent(1)));
    }


    [Test]
    public void GetLength()
    {
      BSplineSegment1F bSymmetric = new BSplineSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 9,
      };
      Assert.IsTrue(
        Numeric.AreEqual(
          bSymmetric.GetLength(0.5f, 0, 100, Numeric.EpsilonF), bSymmetric.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      BSplineSegment1F s = new BSplineSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };
      var points = new List<float>();
      s.Flatten(points, 1, 1);
      Assert.AreEqual(2, points.Count);
      Assert.IsTrue(points.Contains(s.GetPoint(0)));
      Assert.IsTrue(points.Contains(s.GetPoint(1)));
    }
  }
}