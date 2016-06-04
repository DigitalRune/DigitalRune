using System;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class BezierSegment1FTest
  {
    [Test]
    public void GetPoint()
    {
      BezierSegment1F b = new BezierSegment1F
      {
        Point1 = 1,
        ControlPoint1 = 3,
        ControlPoint2 = 4,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(1, b.GetPoint(0)));
      Assert.IsTrue(Numeric.AreEqual(8, b.GetPoint(1)));
      Assert.IsTrue(Numeric.AreEqual(2.638f, b.GetPoint(0.3f)));
    }


    [Test]
    public void GetTangent()
    {
      BezierSegment1F b = new BezierSegment1F
      {
        Point1 = 1,
        ControlPoint1 = 3,
        ControlPoint2 = 4,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(3 * (3 - 1), b.GetTangent(0)));
      Assert.IsTrue(Numeric.AreEqual(3 * (8 - 4), b.GetTangent(1)));
    }


    [Test]
    public void GetLength()
    {
      BezierSegment1F b = new BezierSegment1F
      {
        Point1 = 1,
        ControlPoint1 = 3,
        ControlPoint2 = 4,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(7, b.GetLength(0, 1, 100, Numeric.EpsilonF)));

      BezierSegment1F bSymmetric = new BezierSegment1F
      {
        Point1 = 1,
        ControlPoint1 = 3,
        ControlPoint2 = 4,
        Point2 = 6,
      };
      Assert.IsTrue(Numeric.AreEqual(2.5f, bSymmetric.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      var s = new BezierSegment1F
      {
        Point1 = 1,
        ControlPoint1 = 3,
        ControlPoint2 = 4,
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