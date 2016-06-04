using NUnit.Framework;


namespace DigitalRune.Mathematics.Functions.Tests
{
  [TestFixture]
  public class GaussianFunctionDTest
  {
    [Test]
    public void Test()
    {
      GaussianFunctionD f = new GaussianFunctionD();
      Assert.AreEqual(1, f.Coefficient);
      Assert.AreEqual(0, f.ExpectedValue);
      Assert.AreEqual(1, f.StandardDeviation);

      f = new GaussianFunctionD(3, 4, 5);
      Assert.AreEqual(3, f.Coefficient);
      Assert.AreEqual(4, f.ExpectedValue);
      Assert.AreEqual(5, f.StandardDeviation);

      f = new GaussianFunctionD { ExpectedValue = 10, StandardDeviation = 2, Coefficient = 4 };
      Assert.AreEqual(4, f.Coefficient);
      Assert.AreEqual(10, f.ExpectedValue);
      Assert.AreEqual(2, f.StandardDeviation);

      Assert.Less(f.Compute(3), f.Compute(4f));
      Assert.Less(f.Compute(5), f.Compute(8f));
      Assert.Less(f.Compute(8), f.Compute(10f));
      Assert.Greater(f.Compute(10), f.Compute(11f));
      Assert.Greater(f.Compute(11), f.Compute(12f));
      Assert.Greater(f.Compute(14), f.Compute(16f));

      Assert.IsTrue(Numeric.AreEqual(f.Compute(8), f.Compute(12)));
    }
  }
}
