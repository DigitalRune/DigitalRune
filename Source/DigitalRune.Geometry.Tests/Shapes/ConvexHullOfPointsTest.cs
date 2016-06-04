using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class ConvexHullOfPointsTest
  {
    [Test]
    public void EmptyConvexHullOfPoints()
    {
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints(Enumerable.Empty<Vector3F>());
      Assert.AreEqual(0, convexHullOfPoints.Points.Count);
      Assert.AreEqual(Vector3F.Zero, convexHullOfPoints.InnerPoint);
      Assert.AreEqual(new Aabb(), convexHullOfPoints.GetAabb(Pose.Identity));
    }


    [Test]
    public void OnePoint()
    {
      Vector3F point = new Vector3F(1, 0, 0);
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints(new[] { point });
      Assert.AreEqual(1, convexHullOfPoints.Points.Count);
      Assert.AreEqual(point, convexHullOfPoints.InnerPoint);
      Assert.AreEqual(new Aabb(point, point), convexHullOfPoints.GetAabb(Pose.Identity));
    }


    [Test]
    public void TwoPoints()
    {
      Vector3F point0 = new Vector3F(1, 0, 0);
      Vector3F point1 = new Vector3F(10, 0, 0);
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints(new[] { point0, point1 });
      Assert.AreEqual(2, convexHullOfPoints.Points.Count);
      Assert.AreEqual((point0 + point1) / 2, convexHullOfPoints.InnerPoint);
      Assert.AreEqual(new Aabb(point0, point1), convexHullOfPoints.GetAabb(Pose.Identity));
    }


    [Test]
    public void ThreePoints()
    {
      Vector3F point0 = new Vector3F(1, 1, 1);
      Vector3F point1 = new Vector3F(2, 1, 1);
      Vector3F point2 = new Vector3F(1, 2, 1);
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints(new[] { point0, point1, point2 });
      Assert.AreEqual(3, convexHullOfPoints.Points.Count);
      Assert.AreEqual((point0 + point1 + point2) / 3, convexHullOfPoints.InnerPoint);
      Assert.AreEqual(new Aabb(new Vector3F(1, 1, 1), new Vector3F(2, 2, 1)), convexHullOfPoints.GetAabb(Pose.Identity));
    }


    [Test]
    public void GetSupportPoint()
    {
      ConvexHullOfPoints emptyConvexHullOfPoints = new ConvexHullOfPoints(Enumerable.Empty<Vector3F>());
      Assert.AreEqual(new Vector3F(0, 0, 0), emptyConvexHullOfPoints.GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), emptyConvexHullOfPoints.GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(new Vector3F(0, 0, 0), emptyConvexHullOfPoints.GetSupportPoint(new Vector3F(0, 0, 1)));
      Assert.AreEqual(new Vector3F(0, 0, 0), emptyConvexHullOfPoints.GetSupportPoint(new Vector3F(1, 1, 1)));

      Vector3F p0 = new Vector3F(2, 0, 0);
      Vector3F p1 = new Vector3F(-1, -1, -2);
      Vector3F p2 = new Vector3F(0, 2, -3);
      Assert.AreEqual(p0, new ConvexHullOfPoints(new[] { p0, p1, p2 }).GetSupportPoint(new Vector3F(1, 0, 0)));
      Assert.AreEqual(p2, new ConvexHullOfPoints(new[] { p0, p1, p2 }).GetSupportPoint(new Vector3F(0, 1, 0)));
      Assert.AreEqual(p2, new ConvexHullOfPoints(new[] { p0, p1, p2 }).GetSupportPoint(new Vector3F(0, 0, -1)));
      Assert.AreEqual(p1, new ConvexHullOfPoints(new[] { p0, p1, p2 }).GetSupportPoint(new Vector3F(-1, 0, 1)));
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("ConvexHullOfPoints { Count = 3 }", new ConvexHullOfPoints(new[] { new Vector3F(1, 0, 0), new Vector3F(0, 1, 0), new Vector3F(0, 0, 1) }).ToString());
    }


    [Test]
    public void Clone()
    {
      ConvexHullOfPoints convexHullOfPoints = new ConvexHullOfPoints(
        new[]
        {
          new Vector3F(0, 0, 0),
          new Vector3F(1, 0, 0),
          new Vector3F(0, 2, 0),
          new Vector3F(0, 0, 3),
          new Vector3F(1, 5, 0),
          new Vector3F(0, 1, 7),
        });
      ConvexHullOfPoints clone = convexHullOfPoints.Clone() as ConvexHullOfPoints;
      Assert.IsNotNull(clone);

      for (int i = 0; i < clone.Points.Count; i++)
        Assert.AreEqual(convexHullOfPoints.Points[i], clone.Points[i]);

      Assert.AreEqual(convexHullOfPoints.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(convexHullOfPoints.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SimpleTetrahedron()
    {
      List<Vector3F> points = new List<Vector3F>
      {
        new Vector3F(0, 0, 0),
        new Vector3F(0, 1, 0),
        new Vector3F(1, 0, 0),
        new Vector3F(0, 0, 1),
      };

      ConvexHullOfPoints convex = new ConvexHullOfPoints(points);
      
      // Sample primary directions
      Vector3F right = new Vector3F(2, 0, 0);
      AssertSupportPointsAreEquivalent(GetSupportPoint(right, points), convex.GetSupportPoint(right), right);
      Vector3F left = new Vector3F(-2, 0, 0);
      AssertSupportPointsAreEquivalent(GetSupportPoint(left, points), convex.GetSupportPoint(left), left);
      Vector3F up = new Vector3F(0, 2, 0);
      AssertSupportPointsAreEquivalent(GetSupportPoint(up, points), convex.GetSupportPoint(up), up);
      Vector3F down = new Vector3F(0, -2, 0);
      AssertSupportPointsAreEquivalent(GetSupportPoint(down, points), convex.GetSupportPoint(down), down);
      Vector3F back = new Vector3F(0, 0, 2);
      AssertSupportPointsAreEquivalent(GetSupportPoint(back, points), convex.GetSupportPoint(back), back);
      Vector3F front = new Vector3F(0, 0, -2);
      AssertSupportPointsAreEquivalent(GetSupportPoint(front, points), convex.GetSupportPoint(front), front);

      // Sample random directions
      for (int i = 0; i < 10; i++)
      {
        Vector3F direction = RandomHelper.Random.NextVector3F(-1, 1);
        Vector3F supportPoint = convex.GetSupportPoint(direction);
        Vector3F reference = GetSupportPoint(direction, points);

        // The support points can be different, e.g. if a an edge of face is normal to the 
        // direction. When projected onto the direction both support points must be at equal
        // distance.          
        AssertSupportPointsAreEquivalent(reference, supportPoint, direction);
      }
    }


    [Test]
    public void RandomConvexHullOfPoints()
    {
      // Use a fixed seed.
      RandomHelper.Random = new Random(12345);

      // Try polyhedra with 0, 1, 2, ... points.
      for (int numberOfPoints = 0; numberOfPoints < 100; numberOfPoints++)
      {
        List<Vector3F> points = new List<Vector3F>(numberOfPoints);

        // Create random polyhedra.
        for (int i = 0; i < numberOfPoints; i++)
          points.Add(
            new Vector3F(
              RandomHelper.Random.NextFloat(-10, 10),
              RandomHelper.Random.NextFloat(-20, 20),
              RandomHelper.Random.NextFloat(-100, 100)));

        ConvexHullOfPoints convex = new ConvexHullOfPoints(points);

        // Sample primary directions
        Vector3F right = new Vector3F(2, 0, 0);
        AssertSupportPointsAreEquivalent(GetSupportPoint(right, points), convex.GetSupportPoint(right), right);
        Vector3F left = new Vector3F(-2, 0, 0);
        AssertSupportPointsAreEquivalent(GetSupportPoint(left, points), convex.GetSupportPoint(left), left);
        Vector3F up = new Vector3F(0, 2, 0);
        AssertSupportPointsAreEquivalent(GetSupportPoint(up, points), convex.GetSupportPoint(up), up);
        Vector3F down = new Vector3F(0, -2, 0);
        AssertSupportPointsAreEquivalent(GetSupportPoint(down, points), convex.GetSupportPoint(down), down);
        Vector3F back = new Vector3F(0, 0, 2);
        AssertSupportPointsAreEquivalent(GetSupportPoint(back, points), convex.GetSupportPoint(back), back);
        Vector3F front = new Vector3F(0, 0, -2);
        AssertSupportPointsAreEquivalent(GetSupportPoint(front, points), convex.GetSupportPoint(front), front);

        // Sample random directions
        for (int i = 0; i < 10; i++)
        {
          Vector3F direction = RandomHelper.Random.NextVector3F(-1, 1);
          if (direction.IsNumericallyZero)
            continue;

          Vector3F supportPoint = convex.GetSupportPoint(direction);
          Vector3F reference = GetSupportPoint(direction, points);

          // The support points can be different, e.g. if a an edge of face is normal to the 
          // direction. When projected onto the direction both support points must be at equal
          // distance.          
          AssertSupportPointsAreEquivalent(reference, supportPoint, direction);
        }
      }
    }


    private Vector3F GetSupportPoint(Vector3F direction, List<Vector3F> points)
    {
      // This is the default method that is used without the internal BSP tree.
      float maxDistance = float.NegativeInfinity;
      Vector3F supportVertex = new Vector3F();
      int numberOfPoints = points.Count;
      for (int i = 0; i < numberOfPoints; i++)
      {
        float distance = Vector3F.Dot(points[i], direction);
        if (distance > maxDistance)
        {
          supportVertex = points[i];
          maxDistance = distance;
        }
      }
      return supportVertex;
    }


    private void AssertSupportPointsAreEquivalent(Vector3F expected, Vector3F actual, Vector3F direction)
    {
      // The support points can be different, e.g. if a an edge of face is normal to the 
      // direction. When projected onto the direction both support points must be at equal
      // distance.          
      bool areEqual = Numeric.AreEqual(
        Vector3F.ProjectTo(expected, direction).Length,
        Vector3F.ProjectTo(actual, direction).Length);

      Assert.IsTrue(areEqual);
    }
  }
}
