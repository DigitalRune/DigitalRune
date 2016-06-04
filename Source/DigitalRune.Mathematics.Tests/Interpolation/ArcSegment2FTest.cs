using System.Collections.Generic;
using System.Linq;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class ArcSegment2FTest
  {
    [Test]
    public void GetPoint()
    {
      var a = new ArcSegment2F
      {
        Point1 = new Vector2F(1, 2),
        Point2 = new Vector2F(10, -3),
      };

      Assert.IsTrue(Vector2F.AreNumericallyEqual(a.Point1, a.GetPoint(0), 0.001f));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(a.Point2, a.GetPoint(1), 0.001f));
    }


    [Test]
    public void GetLength()
    {
      var b = new ArcSegment2F
      {
        Point1 = new Vector2F(1, 2),
        Point2 = new Vector2F(10, -3),
      };

      float length1 = b.GetLength(0, 1, 20, Numeric.EpsilonF);

      float approxLength = 0;
      const float step = 0.0001f;
      for (float u = 0; u <= 1.0f - step; u += step)
        approxLength += (b.GetPoint(u) - b.GetPoint(u + step)).Length;

      Assert.IsTrue(Numeric.AreEqual(approxLength, length1, 0.001f));
      Assert.IsTrue(Numeric.AreEqual(b.GetLength(0, 1, 100, Numeric.EpsilonF), b.GetLength(0, 0.5f, 100, Numeric.EpsilonF) + b.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
      Assert.IsTrue(Numeric.AreEqual(b.GetLength(0, 1, 100, Numeric.EpsilonF), b.GetLength(1, 0, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      var s = new ArcSegment2F
      {
        Point1 = new Vector2F(1, 2),
        Point2 = new Vector2F(10, -3),
      };
      var points = new List<Vector2F>();
      var tolerance = 1f;
      s.Flatten(points, 10, tolerance);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(points[0], s.Point1));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(points.Last(), s.Point2));
      var curveLength = s.GetLength(0, 1, 10, tolerance);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);
    }
  }
}