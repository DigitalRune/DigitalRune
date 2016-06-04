using NUnit.Framework;


namespace DigitalRune.Tests
{
  [TestFixture]
  public class PrimeHelperTest
  {
    [Test]
    public void IsPrimesTest()
    {
      Assert.IsFalse(PrimeHelper.IsPrime(-3));
      Assert.IsFalse(PrimeHelper.IsPrime(-1));
      Assert.IsFalse(PrimeHelper.IsPrime(0));
      Assert.IsFalse(PrimeHelper.IsPrime(1));
      Assert.IsFalse(PrimeHelper.IsPrime(4));
      Assert.IsFalse(PrimeHelper.IsPrime(10));
      Assert.IsTrue(PrimeHelper.IsPrime(2));
      Assert.IsTrue(PrimeHelper.IsPrime(3));
      Assert.IsTrue(PrimeHelper.IsPrime(5));
      Assert.IsTrue(PrimeHelper.IsPrime(7));
      Assert.IsTrue(PrimeHelper.IsPrime(397));
    }


    [Test]
    public void NextPrimesTest()
    {
      for (int n = 1; n < int.MaxValue / 2; n <<= 1)
      {
        int prime = PrimeHelper.NextPrime(n);
        Assert.GreaterOrEqual(prime, n);
        Assert.IsTrue(PrimeHelper.IsPrime(prime));
        
        if (n > 1)
        {
          // Prime number should be close to actual number.
          Assert.LessOrEqual(prime, (int)(n * 1.5));
        }
      }
    }
  }
}
