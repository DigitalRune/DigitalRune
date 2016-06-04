using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class HermiteSegment1FTest
  {
    [Test]
    public void GetPoint()
    {
      HermiteSegment1F s = new HermiteSegment1F
      {
        Point1 = 1,
        Tangent1 = (3 - 1) * 3,
        Tangent2 = (8 - 4) * 3,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(1, s.GetPoint(0)));
      Assert.IsTrue(Numeric.AreEqual(8, s.GetPoint(1)));
      Assert.IsTrue(Numeric.AreEqual(2.638f, s.GetPoint(0.3f)));
    }


    [Test]
    public void GetTangent()
    {
      HermiteSegment1F s = new HermiteSegment1F
      {
        Point1 = 1,
        Tangent1 = (3 - 1) * 3,
        Tangent2 = (8 - 4) * 3,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(3 * (3 - 1), s.GetTangent(0)));
      Assert.IsTrue(Numeric.AreEqual(3 * (8 - 4), s.GetTangent(1)));
    }


    [Test]
    public void GetLength()
    {
      HermiteSegment1F s = new HermiteSegment1F
      {
        Point1 = 1,
        Tangent1 = (3 - 1) * 3,
        Tangent2 = (8 - 4) * 3,
        Point2 = 8,
      };

      Assert.IsTrue(Numeric.AreEqual(7, s.GetLength(0, 1, 100, Numeric.EpsilonF)));

      HermiteSegment1F sSymmetric = new HermiteSegment1F
      {
        Point1 = 1,
        Tangent1 = (3 - 1) * 3,
        Tangent2 = (6 - 4) * 3,
        Point2 = 6,
      };
      Assert.IsTrue(Numeric.AreEqual(2.5f, sSymmetric.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      HermiteSegment1F s = new HermiteSegment1F
      {
        Point1 = 1,
        Tangent1 = (3 - 1) * 3,
        Tangent2 = (8 - 4) * 3,
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