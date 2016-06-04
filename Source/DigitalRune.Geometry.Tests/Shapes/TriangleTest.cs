using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class TriangleTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3F(), new Triangle().Vertex0);
      Assert.AreEqual(new Vector3F(), new Triangle().Vertex1);
      Assert.AreEqual(new Vector3F(), new Triangle().Vertex2);

      Assert.AreEqual(new Vector3F(1, 2, 3), new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Vertex0);
      Assert.AreEqual(new Vector3F(4, 5, 6), new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Vertex1);
      Assert.AreEqual(new Vector3F(7, 8, 9), new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Vertex2);

      Assert.AreEqual(new Vector3F(1, 2, 3), new Triangle(new TriangleShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))).Vertex0);
      Assert.AreEqual(new Vector3F(4, 5, 6), new Triangle(new TriangleShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))).Vertex1);
      Assert.AreEqual(new Vector3F(7, 8, 9), new Triangle(new TriangleShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))).Vertex2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorArgumentNullException()
    {
      new Triangle(null);
    }


    [Test]
    public void TestProperties()
    {
      Triangle t = new Triangle();
      Assert.AreEqual(new Vector3F(), t.Vertex0);
      Assert.AreEqual(new Vector3F(), t.Vertex1);
      Assert.AreEqual(new Vector3F(), t.Vertex2);

      t.Vertex0 = new Vector3F(1, 2, 3);
      Assert.AreEqual(new Vector3F(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3F(), t.Vertex1);
      Assert.AreEqual(new Vector3F(), t.Vertex2);

      t.Vertex1 = new Vector3F(4, 5, 6);
      Assert.AreEqual(new Vector3F(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3F(4, 5, 6), t.Vertex1);
      Assert.AreEqual(new Vector3F(), t.Vertex2);

      t.Vertex2 = new Vector3F(9, 7, 8);
      Assert.AreEqual(new Vector3F(1, 2, 3), t.Vertex0);
      Assert.AreEqual(new Vector3F(4, 5, 6), t.Vertex1);
      Assert.AreEqual(new Vector3F(9, 7, 8), t.Vertex2);

      Assert.IsTrue(Vector3F.AreNumericallyEqual(Vector3F.Cross(new Vector3F(3, 3, 3), new Vector3F(8, 5, 5)).Normalized, t.Normal));

      // Degenerate triangles can have any normal.
      Assert.IsTrue(Numeric.AreEqual(1, new Triangle().Normal.Length));
    }


    [Test]
    public void TestIndexer()
    {
      Triangle t = new Triangle();
      Assert.AreEqual(new Vector3F(), t[0]);
      Assert.AreEqual(new Vector3F(), t[1]);
      Assert.AreEqual(new Vector3F(), t[2]);

      t[0] = new Vector3F(1, 2, 3);
      Assert.AreEqual(new Vector3F(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3F(), t[1]);
      Assert.AreEqual(new Vector3F(), t[2]);

      t[1] = new Vector3F(4, 5, 6);
      Assert.AreEqual(new Vector3F(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3F(4, 5, 6), t[1]);
      Assert.AreEqual(new Vector3F(), t[2]);

      t[2] = new Vector3F(7, 8, 9);
      Assert.AreEqual(new Vector3F(1, 2, 3), t[0]);
      Assert.AreEqual(new Vector3F(4, 5, 6), t[1]);
      Assert.AreEqual(new Vector3F(7, 8, 9), t[2]);

      Assert.AreEqual(t.Vertex0, t[0]);
      Assert.AreEqual(t.Vertex1, t[1]);
      Assert.AreEqual(t.Vertex2, t[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException0()
    {
      var t = new Triangle();
      t[3] = Vector3F.Zero;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException1()
    {
      var t = new Triangle();
      t[-1] = Vector3F.Zero;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException2()
    {
      Vector3F v = new Triangle()[3];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void TestIndexerException3()
    {
      Vector3F v = new Triangle()[-1];
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new Triangle().Equals(new Triangle()));
      Assert.IsTrue(new Triangle().Equals(new Triangle(Vector3F.Zero, Vector3F.Zero, Vector3F.Zero)));
      Assert.IsTrue(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Equals(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))));
      Assert.IsTrue(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Equals((object)new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))));
      Assert.IsFalse(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Equals(new Triangle(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))));
      Assert.IsFalse(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).Equals(new TriangleShape(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9))));
      Assert.IsFalse(new Triangle().Equals(null));

      Assert.IsTrue(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)) == new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)));
      Assert.IsTrue(new Triangle(new Vector3F(1, 2, 4), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)) != new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)));
    }


    [Test]
    public void LengthTest()
    {
      Assert.AreEqual(2, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(3, 2, 3)).Length);
    }


    [Test]
    public void LengthSquaredTest()
    {
      Assert.AreEqual(4, new LineSegment(new Vector3F(1, 2, 3), new Vector3F(3, 2, 3)).LengthSquared);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new Triangle().GetHashCode(), new Triangle().GetHashCode());
      Assert.AreEqual(new Triangle().GetHashCode(), new Triangle(Vector3F.Zero, Vector3F.Zero, Vector3F.Zero).GetHashCode());
      Assert.AreEqual(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).GetHashCode(), new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).GetHashCode());
      Assert.AreNotEqual(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).GetHashCode(), new Triangle(new Vector3F(0, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).GetHashCode());
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new Triangle(new Vector3F(1, 2, 3), new Vector3F(4, 5, 6), new Vector3F(7, 8, 9)).ToString().StartsWith("Triangle { "));
    }


    //--------------------------------------------------------------
    #region Triangle Mirroring
    //--------------------------------------------------------------

    [Test]
    public void NegativeScale()
    {
      // What happens to the triangle normal when a negative scale is applied?
      // --> To get the correct normal from the transformed mesh, we have to change
      // the winding order if an odd number of scale components (X, Y, Z) are negative.

      RandomHelper.Random = new Random(1234567);
      for (int i = 0; i < 100; i++)
      {
        var tA = new Triangle();
        tA.Vertex0 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex1 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));
        tA.Vertex2 = new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100));

        // Random scale including negative scale for mirroring.
        var s = new Vector3F(RandomHelper.Random.NextFloat(-2, 2), RandomHelper.Random.NextFloat(-2, 2), RandomHelper.Random.NextFloat(-2, 2));

        // Random pose.
        var p = new Pose(
          new Vector3F(RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100), RandomHelper.Random.NextFloat(-100, 100)),
          RandomHelper.Random.NextQuaternionF());

        // For the correct triangle normal we have to use the inverse transpose:
        //   (M^-1)^T = 1 / scale
        var n = Vector3F.Cross(tA.Vertex1 - tA.Vertex0, tA.Vertex2 - tA.Vertex0) / s;
        n = p.ToWorldDirection(n);

        if (n.TryNormalize())
        {
          // Lets transform the triangle.
          tA.Vertex0 = p.ToWorldPosition(tA.Vertex0 * s);
          tA.Vertex1 = p.ToWorldPosition(tA.Vertex1 * s);
          tA.Vertex2 = p.ToWorldPosition(tA.Vertex2 * s);

          // Change the winding order, so that we get the same result.
          if (s.X * s.Y * s.Z < 0)
            MathHelper.Swap(ref tA.Vertex0, ref tA.Vertex1);

          bool areEqual = Vector3F.AreNumericallyEqual(n, tA.Normal, 0.001f);
          if (!areEqual)
            Debugger.Break();
          Assert.IsTrue(areEqual);
        }
      }
    }
    #endregion
  }
}
