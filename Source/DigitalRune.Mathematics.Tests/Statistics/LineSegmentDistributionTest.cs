using System;
using NUnit.Framework;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class LineSegmentDistributionTest
  {
    [Test]
    public void Test1()
    {
      var random = new Random(123456);
      var d = new LineSegmentDistribution
      {
        Start = new Vector3F(1, -2, -3),
        End = new Vector3F(-10, -20, 30),
      };

      // Create 100 random values and check if they are valid. 
      for (int i=0; i<100; i++)
      {
        Vector3F r = d.Next(random);
        Assert.IsTrue(1 >= r.X);
        Assert.IsTrue(-2 >= r.Y);
        Assert.IsTrue(-3 <= r.Z);
        Assert.IsTrue(r.X >= -10);
        Assert.IsTrue(r.Y >= -20);
        Assert.IsTrue(r.Z <= 30);

        var factorX = (r.X - d.Start.X) / (d.End.X - d.Start.X);
        var factorY = (r.Y - d.Start.Y) / (d.End.Y - d.Start.Y);
        var factorZ = (r.Z - d.Start.Z) / (d.End.Z - d.Start.Z);
        Assert.IsTrue(Numeric.AreEqual(factorX, factorY));
        Assert.IsTrue(Numeric.AreEqual(factorX, factorZ));
        Assert.IsTrue(-0.00001f < factorX &&  factorX < 1.00001f);
      }

      Assert.AreEqual(new Vector3F(1, -2, -3), d.Start);
      Assert.AreEqual(new Vector3F(-10, -20, 30), d.End);

      d.Start = new Vector3F(0.1f, 0.2f, 0.3f);
      Assert.AreEqual(new Vector3F(0.1f, 0.2f, 0.3f), d.Start);
      Assert.AreEqual(new Vector3F(-10, -20, 30), d.End);

      d.End = new Vector3F(0.2f, 0.4f, 0.6f);
      Assert.AreEqual(new Vector3F(0.1f, 0.2f, 0.3f), d.Start);
      Assert.AreEqual(new Vector3F(0.2f, 0.4f, 0.6f), d.End);

      // Create 100 random values and check if they are valid. 

      for (int i = 0; i < 100; i++)
      {
        Vector3F r = d.Next(random);
        Assert.IsTrue(0.1f <= r.X);
        Assert.IsTrue(0.2f <= r.Y);
        Assert.IsTrue(0.3f <= r.Z);
        Assert.IsTrue(r.X <= 0.2f);
        Assert.IsTrue(r.Y <= 0.4f);
        Assert.IsTrue(r.Z <= 0.6f);

        var factorX = (r.X - d.Start.X) / (d.End.X - d.Start.X);
        var factorY = (r.Y - d.Start.Y) / (d.End.Y - d.Start.Y);
        var factorZ = (r.Z - d.Start.Z) / (d.End.Z - d.Start.Z);
        Assert.IsTrue(Numeric.AreEqual(factorX, factorY));
        Assert.IsTrue(Numeric.AreEqual(factorX, factorZ));
        Assert.IsTrue(-0.00001f < factorX && factorX < 1.00001f);
      }
    }
  }
}
