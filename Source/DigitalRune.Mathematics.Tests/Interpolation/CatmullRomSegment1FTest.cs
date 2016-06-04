using System;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class CatmullRomSegment1FTest
  {
    [Test]
    public void GetPoint()
    {
      HermiteSegment1F h = new HermiteSegment1F
      {
        Point1 = 3,
        Tangent1 = (7 - 1) * 0.5f,
        Tangent2 = (8 - 3) * 0.5f,
        Point2 = 7,
      };

      CatmullRomSegment1F s = new CatmullRomSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(3, s.GetPoint(0)));
      Assert.IsTrue(Numeric.AreEqual(7, s.GetPoint(1)));
      Assert.IsTrue(Numeric.AreEqual(h.GetPoint(0.3f), s.GetPoint(0.3f)));
    }


    [Test]
    public void GetTangent()
    {
      HermiteSegment1F h = new HermiteSegment1F
      {
        Point1 = 3,
        Tangent1 = (7 - 1) * 0.5f,
        Tangent2 = (8 - 3) * 0.5f,
        Point2 = 7,
      };

      CatmullRomSegment1F s = new CatmullRomSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(h.Tangent1, s.GetTangent(0)));
      Assert.IsTrue(Numeric.AreEqual(h.Tangent2, s.GetTangent(1)));
      Assert.IsTrue(Numeric.AreEqual(h.GetTangent(0.81f), s.GetTangent(0.81f)));
    }


    [Test]
    public void GetLength()
    {
      CatmullRomSegment1F s = new CatmullRomSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };
      Assert.IsTrue(Numeric.AreEqual(4, s.GetLength(0, 1, 100, Numeric.EpsilonF)));

      CatmullRomSegment1F sSymmetric = new CatmullRomSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 9,
      };
      Assert.IsTrue(Numeric.AreEqual(2f, sSymmetric.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      CatmullRomSegment1F s = new CatmullRomSegment1F
      {
        Point1 = 1,
        Point2 = 3,
        Point3 = 7,
        Point4 = 8,
      };
      var points = new List<float>();
      s.Flatten(points, 1, 1);
      Assert.AreEqual(2, points.Count);
      Assert.IsTrue(points.Contains(s.Point2));
      Assert.IsTrue(points.Contains(s.Point3));
    }
  }
}