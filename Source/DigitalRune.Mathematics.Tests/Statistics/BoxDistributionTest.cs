using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class BoxDistributionTest
  {   
    [Test]
    public void ZeroWidth()
    {
      var random = new Random(123456);
      var d = new BoxDistribution { MinValue = new Vector3F(1, 2, 3), MaxValue = new Vector3F(1, 2, 3) };

      Assert.AreEqual(new Vector3F(1, 2, 3), d.Next(random));
      Assert.AreEqual(new Vector3F(1, 2, 3), d.Next(random));
      Assert.AreEqual(new Vector3F(1, 2, 3), d.Next(random));
    }


    [Test]
    public void Values()
    {
      var random = new Random(123456);
      var d = new BoxDistribution();

      for (int i = 0; i < 100; i++)
      {
        d.MinValue = RandomHelper.Random.NextVector3F(-1, 1);
        d.MaxValue = RandomHelper.Random.NextVector3F(-1, 1);

        var value = d.Next(random);

        Assert.IsTrue(d.MinValue.X <= value.X && value.X <= d.MaxValue.X || d.MaxValue.X <= value.X && value.X <= d.MinValue.X);
        Assert.IsTrue(d.MinValue.Y <= value.Y && value.Y <= d.MaxValue.Y || d.MaxValue.Y <= value.Y && value.Y <= d.MinValue.Y);
        Assert.IsTrue(d.MinValue.Z <= value.Z && value.Z <= d.MaxValue.Z || d.MaxValue.Z <= value.Z && value.Z <= d.MinValue.Z);
      }
    }
  }
}
