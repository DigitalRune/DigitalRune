using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class CircleDistributionTest
  {   
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void InvalidInnerRadius()
    {
      new CircleDistribution { InnerRadius = -1 };
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void InvalidOuterRadius()
    {
      new CircleDistribution { OuterRadius = -0.1f };
    }


    [Test]
    public void Properties()
    {
      CircleDistribution d = new CircleDistribution();

      Assert.AreEqual(0, d.InnerRadius);
      Assert.AreEqual(1, d.OuterRadius);
      Assert.AreEqual(Vector2F.One, d.Scale);

      d.InnerRadius = 2;
      Assert.AreEqual(2, d.InnerRadius);
      Assert.AreEqual(1, d.OuterRadius);
      Assert.AreEqual(Vector2F.One, d.Scale);

      d.OuterRadius = 3;
      Assert.AreEqual(2, d.InnerRadius);
      Assert.AreEqual(3, d.OuterRadius);
      Assert.AreEqual(Vector2F.One, d.Scale);

      d.Scale = new Vector2F(0.5f, 2);
      Assert.AreEqual(2, d.InnerRadius);
      Assert.AreEqual(3, d.OuterRadius);
      Assert.AreEqual(new Vector2F(0.5f, 2), d.Scale);
    }


    [Test]
    public void ZeroRadius()
    {
      var random = new Random(123456);
      CircleDistribution d = new CircleDistribution { OuterRadius = 0 };

      Assert.AreEqual(new Vector3F(), d.Next(random));
      Assert.AreEqual(new Vector3F(), d.Next(random));
      Assert.AreEqual(new Vector3F(), d.Next(random));
    }


    [Test]
    public void Values()
    {
      var random = new Random(123456);
      CircleDistribution d = new CircleDistribution();

      for (int i = 0; i < 100; i++)
      {
        d.InnerRadius = RandomHelper.Random.NextFloat(0, 100);
        d.OuterRadius = RandomHelper.Random.NextFloat(0, 100);

        var value = d.Next(random);

        var radius = value.Length;
        Assert.IsTrue(d.InnerRadius < radius && radius < d.OuterRadius
                      || d.OuterRadius < radius && radius < d.InnerRadius);
        Assert.AreEqual(0, value.Z);
      }
    }
  }
}
