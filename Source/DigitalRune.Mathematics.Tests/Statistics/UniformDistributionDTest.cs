using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class UniformDistributionDTest
  {
    [Test]
    public void Constructor()
    {
      var d = new UniformDistributionD(2, 3);
      Assert.AreEqual(2, d.MinValue);
      Assert.AreEqual(3, d.MaxValue);
    }

    [Test]
    public void Test1()
    {
      UniformDistributionD d = new UniformDistributionD();

      // Create 100 random values and check if they are valid. 

      for (int i = 0; i < 100; i++)
      {
        double r = d.Next(RandomHelper.Random);
        Assert.IsTrue(-1 <= r);
        Assert.IsTrue(r <= 1);
      }

      Assert.AreEqual(-1, d.MinValue);
      Assert.AreEqual(1, d.MaxValue);

      d.MaxValue = 100;
      Assert.AreEqual(-1, d.MinValue);
      Assert.AreEqual(100, d.MaxValue);

      d.MinValue = 99.9;
      Assert.AreEqual(99.9, d.MinValue);
      Assert.AreEqual(100, d.MaxValue);

      for (int i = 0; i < 100; i++)
      {
        double r = d.Next(RandomHelper.Random);
        Assert.IsTrue(99.9 <= r);
        Assert.IsTrue(r <= 100);
      }
    }
  }
}