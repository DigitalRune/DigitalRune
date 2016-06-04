using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class FastGaussianDistributionDTest
  {
    [Test]
    public void Constructor()
    {
      var d = new FastGaussianDistributionD(2, 3);
      Assert.AreEqual(2, d.ExpectedValue);
      Assert.AreEqual(3, d.StandardDeviation);
    }


    [Test]
    public void Test1()
    {
      var d = new FastGaussianDistributionD();
      
      Assert.AreEqual(0, d.ExpectedValue);
      Assert.AreEqual(1, d.StandardDeviation);

      var random = new Random(123);
           
      // Create 100 random values and check if they are valid.            
      for (int i = 0; i<100; i++)
      {
        double r = d.Next(random);
        Assert.IsTrue(-3 <= r);
        Assert.IsTrue(r <= 3);        
      }

      d.ExpectedValue = 100;
      d.StandardDeviation = 10;

      for (int i = 0; i < 100; i++)
      {
        double r = d.Next(random);
        Assert.IsTrue(70f <= r);
        Assert.IsTrue(r <= 130);
      }
    }
  }
}
