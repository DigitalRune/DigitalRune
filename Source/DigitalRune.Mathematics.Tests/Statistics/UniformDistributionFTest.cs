using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class UniformDistributionFTest
  {
    [Test]
    public void Constructor()
    {
      var d = new UniformDistributionF(2, 3);
      Assert.AreEqual(2, d.MinValue);
      Assert.AreEqual(3, d.MaxValue);
    }


    [Test]
    public void Test1()
    {
      UniformDistributionF d = new UniformDistributionF();

      // Create 100 random values and check if they are valid. 

      for (int i=0; i<100; i++)
      {
        float r = d.Next(RandomHelper.Random);
        Assert.IsTrue(-1 <= r);
        Assert.IsTrue(r <= 1);        
      }

      Assert.AreEqual(-1, d.MinValue);
      Assert.AreEqual(1, d.MaxValue);

      d.MaxValue = 100;
      Assert.AreEqual(-1, d.MinValue);
      Assert.AreEqual(100, d.MaxValue);

      d.MinValue = 99.9f;
      Assert.AreEqual(99.9f, d.MinValue);
      Assert.AreEqual(100, d.MaxValue);

      for (int i = 0; i < 100; i++)
      {
        float r = d.Next(RandomHelper.Random);
        Assert.IsTrue(99.9f <= r);
        Assert.IsTrue(r <= 100);
      }
    }
  }
}
