using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class BezierSegment2FTest
  {
    [Test]
    public void GetPoint()
    {
      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2F(1, 2),
        ControlPoint1 = new Vector2F(10, 3),
        ControlPoint2 = new Vector2F(7, 8),
        Point2 = new Vector2F(10, 2),
      };

      Assert.IsTrue(Vector2F.AreNumericallyEqual(b.Point1, b.GetPoint(0)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(b.Point2, b.GetPoint(1)));
    }


    [Test]
    public void GetTangent()
    {
      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2F(1, 2),
        ControlPoint1 = new Vector2F(10, 3),
        ControlPoint2 = new Vector2F(7, 8),
        Point2 = new Vector2F(10, 2),
      };

      Assert.IsTrue(Vector2F.AreNumericallyEqual(3 * (b.ControlPoint1 - b.Point1), b.GetTangent(0)));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(3 * (b.Point2 - b.ControlPoint2), b.GetTangent(1)));
    }


    [Test]
    public void GetLength()
    {
      BezierSegment2F b = new BezierSegment2F
      {
        Point1 = new Vector2F(1, 2),
        ControlPoint1 = new Vector2F(4, 5),
        ControlPoint2 = new Vector2F(7, 8),
        Point2 = new Vector2F(10, 2),
      };

      float lowerBound = (b.Point2 - b.Point1).Length;
      float upperBound = (b.Point2 - b.ControlPoint2).Length
                         + (b.ControlPoint2 - b.ControlPoint1).Length
                         + (b.ControlPoint1 - b.Point1).Length;
      Assert.Less(lowerBound, b.GetLength(0, 1, 100, Numeric.EpsilonF));
      Assert.Greater(upperBound, b.GetLength(0, 1, 100, Numeric.EpsilonF));

      float length1 = b.GetLength(0, 1, 20, Numeric.EpsilonF);
      float length2 = b.GetLengthWithDeCasteljau(20, Numeric.EpsilonF);

      Assert.IsTrue(Numeric.AreEqual(length1, length2));
      // Compare numerical integration method and de Casteljau method.

      float approxLength = 0;
      const float step = 0.0001f;
      for (float u = 0; u <= 1.0f; u += step)
        approxLength += (b.GetPoint(u) - b.GetPoint(u + step)).Length;

      Assert.IsTrue(Numeric.AreEqual(approxLength, length1, 0.01f));
      Assert.IsTrue(Numeric.AreEqual(b.GetLength(0, 1, 100, Numeric.EpsilonF), b.GetLength(0, 0.5f, 100, Numeric.EpsilonF) + b.GetLength(0.5f, 1, 100, Numeric.EpsilonF)));
      Assert.IsTrue(Numeric.AreEqual(b.GetLength(0, 1, 100, Numeric.EpsilonF), b.GetLength(1, 0, 100, Numeric.EpsilonF)));
    }


    [Test]
    public void Flatten()
    {
      var s = new BezierSegment2F
      {
        Point1 = new Vector2F(1, 2),
        ControlPoint1 = new Vector2F(4, 5),
        ControlPoint2 = new Vector2F(7, 8),
        Point2 = new Vector2F(10, 2),
      };
      var points = new List<Vector2F>();
      var tolerance = 0.01f;
      s.Flatten(points, 10, tolerance);
      Assert.IsTrue(points.Contains(s.Point1));
      Assert.IsTrue(points.Contains(s.Point2));
      var curveLength = s.GetLength(0, 1, 10, tolerance);
      Assert.IsTrue(CurveHelper.GetLength(points) >= curveLength - tolerance * points.Count / 2);
      Assert.IsTrue(CurveHelper.GetLength(points) <= curveLength);
    }
  }
}