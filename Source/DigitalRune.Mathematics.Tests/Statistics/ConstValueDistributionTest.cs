using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class ConstValueDistributionTest
  {
    [Test]
    public void Constructor()
    {
      var d = new ConstValueDistribution<float>(2.0f);
      Assert.AreEqual(2, d.Value);
    }


    [Test]
    public void Test1()
    {
      ConstValueDistribution<float> d = new ConstValueDistribution<float> { Value = -345 };

      // Create 100 random values and check if they are valid. 

      for (int i=0; i<100; i++)
      {
        float r = d.Next(RandomHelper.Random);
        Assert.AreEqual(-345, r);
      }

      Assert.AreEqual(-345, d.Value);

      d.Value = 10.5f;
      Assert.AreEqual(10.5f, d.Value);

      for (int i = 0; i < 100; i++)
      {
        float r = d.Next(RandomHelper.Random);
        Assert.AreEqual(10.5f, r);
      }

    }
  }
}
