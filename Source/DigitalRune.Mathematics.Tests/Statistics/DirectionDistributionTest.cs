using System;
using NUnit.Framework;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class DirectionDistributionTest
  {
    [Test]
    public void Test1()
    {
      var random = new Random(123456);
      var d = new DirectionDistribution
      {
        Direction = Vector3F.UnitY,
        Deviation = MathHelper.ToRadians(30),
      };

      // Create 100 random values and check if they are valid. 
      for (int i=0; i<100; i++)
      {
        Vector3F r = d.Next(random);
        Assert.IsTrue(r.IsNumericallyNormalized);

        float angle = Vector3F.GetAngle(Vector3F.UnitY, r);
        Assert.IsTrue(angle <= MathHelper.ToRadians(30));
      }

      Assert.AreEqual(MathHelper.ToRadians(30), d.Deviation);
      Assert.AreEqual(new Vector3F(0, 1, 0), d.Direction);

      d.Deviation = 0.1f;
      Assert.AreEqual(0.1f, d.Deviation);
      Assert.AreEqual(new Vector3F(0, 1, 0), d.Direction);

      d.Direction = new Vector3F(1, 2, 3);
      Assert.AreEqual(0.1f, d.Deviation);
      Assert.AreEqual(new Vector3F(1, 2, 3), d.Direction);

      // Create 100 random values and check if they are valid. 
      for (int i = 0; i < 100; i++)
      {
        Vector3F r = d.Next(random);
        Assert.IsTrue(r.IsNumericallyNormalized);

        float angle = Vector3F.GetAngle(d.Direction, r);
        Assert.IsTrue(angle <= 0.1f);
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidDirection()
    {
      new DirectionDistribution { Direction = new Vector3F(Numeric.EpsilonF / 10, 0, 0) };
    }
  }
}
