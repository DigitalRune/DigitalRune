using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Meshes.Tests
{
  [TestFixture]
  public class SupportMappingSamplerTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestArgumentException1()
    {
      GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), -1, 5);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestArgumentException2()
    {
      GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0.4f, 0);
    }


    [Test]
    public void TestIterationLimit()
    {
      IList<Vector3F> hull0 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0, 1);
      IList<Vector3F> hull1 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0, 2);
      IList<Vector3F> hull2 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0, 3);
      IList<Vector3F> hull3 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0, 4);

      Assert.Greater(hull1.Count, hull0.Count);
      Assert.Greater(hull2.Count, hull1.Count);
      Assert.Greater(hull3.Count, hull2.Count);
    }


    [Test]
    public void TestDistanceLimit()
    {
      IList<Vector3F> hull0 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0.4f, 6);
      IList<Vector3F> hull1 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0.2f, 6);
      IList<Vector3F> hull2 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0.1f, 6);
      IList<Vector3F> hull3 = GeometryHelper.SampleConvexShape(new CapsuleShape(1, 3), 0, 6);

      Assert.Greater(hull1.Count, hull0.Count);
      Assert.Greater(hull2.Count, hull1.Count);
      Assert.Greater(hull3.Count, hull2.Count);
    }


    [Test]
    public void TestBox()
    {
      IList<Vector3F> hull = GeometryHelper.SampleConvexShape(new BoxShape(1, 1, 1), 0.1f, 10);
      Assert.AreEqual(8, hull.Count);
    }
  }
}
