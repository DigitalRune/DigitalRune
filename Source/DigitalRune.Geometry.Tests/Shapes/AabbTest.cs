using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class AabbTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new Aabb().Minimum);
      Assert.AreEqual(new Vector3F(), new Aabb().Maximum);

      Assert.AreEqual(new Vector3F(), new Aabb(Vector3F.Zero, Vector3F.Zero).Minimum);
      Assert.AreEqual(new Vector3F(), new Aabb(Vector3F.Zero, Vector3F.Zero).Maximum);

      Assert.AreEqual(new Vector3F(10, 20, 30), new Aabb(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).Minimum);
      Assert.AreEqual(new Vector3F(11, 22, 33), new Aabb(new Vector3F(10, 20, 30), new Vector3F(11, 22, 33)).Maximum);
    }


    [Test]
    public void TestProperties()
    {
      Aabb b = new Aabb();
      Assert.AreEqual(new Vector3F(), b.Minimum);
      Assert.AreEqual(new Vector3F(), b.Maximum);

      b.Minimum = new Vector3F(-10, -20, -30);
      Assert.AreEqual(new Vector3F(-10, -20, -30), b.Minimum);
      Assert.AreEqual(new Vector3F(), b.Maximum);

      b.Maximum = new Vector3F(100, 200, 300);
      Assert.AreEqual(new Vector3F(-10, -20, -30), b.Minimum);
      Assert.AreEqual(new Vector3F(100, 200, 300), b.Maximum);

      Assert.AreEqual(new Vector3F(90f / 2, 180f / 2, 270f / 2), b.Center);
      Assert.AreEqual(new Vector3F(110, 220, 330), b.Extent);
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new Aabb().Equals(new Aabb()));
      Assert.IsTrue(new Aabb().Equals(new Aabb(Vector3F.Zero, Vector3F.Zero)));
      Assert.IsTrue(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new Aabb(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).Equals(new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6))));
      Assert.IsFalse(new Aabb().Equals(null));

      Assert.IsTrue(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)) == new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
      Assert.IsTrue(new Aabb(new Vector3F(1, 2, 4), new Vector3F(4, 5, 6)) != new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(), new Aabb()));
      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)), 
                                             new Aabb(new Vector3F(1, 2, 3 + Numeric.EpsilonF / 2), new Vector3F(4, 5, 6))));
      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)),
                                             new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6 + Numeric.EpsilonF / 2))));
      Assert.IsFalse(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)),
                                              new Aabb(new Vector3F(1, 2, 3 + 10 * Numeric.EpsilonF), new Vector3F(4, 5, 6))));

      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(), new Aabb()));
      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)),
                                             new Aabb(new Vector3F(1, 2, 3.1f), new Vector3F(4, 5, 6)),
                                             0.2f));
      Assert.IsTrue(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)),
                                             new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5.1f, 6)),
                                             0.2f));
      Assert.IsFalse(Aabb.AreNumericallyEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)),
                                             new Aabb(new Vector3F(1, 2, 3.3f), new Vector3F(4, 5, 6)),
                                             0.2f));
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), new Aabb().GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, 1000), new Vector3F(10, 100, 1000)),
                      new Aabb().GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      Assert.AreEqual(new Aabb(new Vector3F(10, 100, 1000), new Vector3F(10, 100, 1000)),
                      new Aabb().GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.CreateRotation(new Vector3F(1, 2, 3), 0.7f))));
      
      
      Aabb aabb = new Aabb(new Vector3F(1, 10, 100), new Vector3F(2, 20, 200));
      Assert.AreEqual(aabb, aabb.GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(11, 110, 1100), new Vector3F(12, 120, 1200)),
                      aabb.GetAabb(new Pose(new Vector3F(10, 100, 1000), QuaternionF.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void Grow()
    {
      var a = new Aabb(new Vector3F(1, 2, 3), new Vector3F(3, 4, 5));
      a.Grow(new Aabb(new Vector3F(1, 2, 3), new Vector3F(3, 4, 5)));

      Assert.AreEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(3, 4, 5)), a);

      a.Grow(new Aabb(new Vector3F(-1, 2, 3), new Vector3F(3, 4, 5)));
      Assert.AreEqual(new Aabb(new Vector3F(-1, 2, 3), new Vector3F(3, 4, 5)), a);

      a.Grow(new Aabb(new Vector3F(1, 2, 3), new Vector3F(3, 5, 5)));
      Assert.AreEqual(new Aabb(new Vector3F(-1, 2, 3), new Vector3F(3, 5, 5)), a);

      var geo = new GeometricObject(new SphereShape(3), new Pose(new Vector3F(1, 0, 0)));
      a.Grow(geo);
      Assert.AreEqual(new Aabb(new Vector3F(-2, -3, -3), new Vector3F(4, 5, 5)), a);
    }


    [Test]
    public void GrowFromPoint()
    {
      var a = new Aabb(new Vector3F(1, 2, 3), new Vector3F(3, 4, 5));
      a.Grow(new Vector3F(10, -20, -30));
      Assert.AreEqual(new Aabb(new Vector3F(1, -20, -30), new Vector3F(10, 4, 5)), a);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new Aabb().GetHashCode(), new Aabb().GetHashCode());
      Assert.AreEqual(new Aabb().GetHashCode(), new Aabb(Vector3F.Zero, Vector3F.Zero).GetHashCode());
      Assert.AreEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new Aabb(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode(), new LineSegmentShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetHashCode());
    }


    //[Test]
    //public void GetSupportPoint()
    //{
    //  Assert.AreEqual(new Vector3F(0, 0, 0), new Aabb().GetSupportPoint(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(new Vector3F(0, 0, 0), new Aabb().GetSupportPoint(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(new Vector3F(0, 0, 0), new Aabb().GetSupportPoint(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(new Vector3F(0, 0, 0), new Aabb().GetSupportPoint(new Vector3F(1, 1, 1)));

    //  Assert.AreEqual(new Vector3F(4, 5, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(new Vector3F(4, 5, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(new Vector3F(4, 5, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(new Vector3F(1, 5, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(-1, 0, 0)));
    //  Assert.AreEqual(new Vector3F(4, 2, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, -1, 0)));
    //  Assert.AreEqual(new Vector3F(4, 5, 3), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(0, 0, -1)));
    //  Assert.AreEqual(new Vector3F(4, 5, 6), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(1, 1, 1)));
    //  Assert.AreEqual(new Vector3F(1, 2, 3), new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).GetSupportPoint(new Vector3F(-1, -1, -1)));
    //}


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new Aabb().GetSupportPointDistance(new Vector3F(1, 0, 0)));
    //  Assert.AreEqual(0, new Aabb().GetSupportPointDistance(new Vector3F(0, 1, 0)));
    //  Assert.AreEqual(0, new Aabb().GetSupportPointDistance(new Vector3F(0, 0, 1)));
    //  Assert.AreEqual(0, new Aabb().GetSupportPointDistance(new Vector3F(1, 1, 1)));

    //  Aabb aabb = new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));
    //  Assert.IsTrue(Numeric.AreEqual(4, aabb.GetSupportPointDistance(new Vector3F(1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(5, aabb.GetSupportPointDistance(new Vector3F(0, 1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(6, aabb.GetSupportPointDistance(new Vector3F(0, 0, 1))));
    //  Assert.IsTrue(Numeric.AreEqual(-1, aabb.GetSupportPointDistance(new Vector3F(-1, 0, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(-2, aabb.GetSupportPointDistance(new Vector3F(0, -1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(-3, aabb.GetSupportPointDistance(new Vector3F(0, 0, -1))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.Dot(new Vector3F(1, 2, 6), new Vector3F(-1, -1, 0).Normalized), aabb.GetSupportPointDistance(new Vector3F(-1, -1, 0))));
    //  Assert.IsTrue(Numeric.AreEqual(Vector3F.ProjectTo(new Vector3F(4, 5, 6), new Vector3F(1, 1, 1)).Length, aabb.GetSupportPointDistance(new Vector3F(1, 1, 1))));
    //}


    [Test]
    public void Scale()
    {
      Aabb aabb = new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));
      aabb.Scale(new Vector3F(-2, 3, 4));
      Assert.AreEqual(new Aabb(new Vector3F(-8, 6, 12), new Vector3F(-2, 15, 24)), aabb);

      aabb = new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));
      aabb.Scale(new Vector3F(2, -3, 4));
      Assert.AreEqual(new Aabb(new Vector3F(2, -15, 12), new Vector3F(8, -6, 24)), aabb);

      aabb = new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6));
      aabb.Scale(new Vector3F(2, 3, -4));
      Assert.AreEqual(new Aabb(new Vector3F(2, 6, -24), new Vector3F(8, 15, -12)), aabb);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("Aabb { Minimum = (1; 2; 3), Maximum = (4; 5; 6) }", new Aabb(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6)).ToString());
    }
  }
}
