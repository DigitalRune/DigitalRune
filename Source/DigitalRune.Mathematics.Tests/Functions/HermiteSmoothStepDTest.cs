using NUnit.Framework;


namespace DigitalRune.Mathematics.Functions.Tests
{
  [TestFixture]
  public class HermiteSmoothStepDTest
  {
    [Test]
    public void Compute()
    {
      Assert.IsTrue(Numeric.AreEqual(0, new HermiteSmoothStepD().Compute(-1)));
      Assert.IsTrue(Numeric.AreEqual(0, new HermiteSmoothStepD().Compute(0)));
      Assert.IsTrue(Numeric.AreEqual(0.5, new HermiteSmoothStepD().Compute(0.5)));
      Assert.IsTrue(Numeric.AreEqual(1, new HermiteSmoothStepD().Compute(1)));
      Assert.IsTrue(Numeric.AreEqual(1, new HermiteSmoothStepD().Compute(2)));
      Assert.IsTrue(Numeric.AreEqual(1 - new HermiteSmoothStepD().Compute(1-0.3), new HermiteSmoothStepD().Compute(0.3)));
      Assert.Greater(new HermiteSmoothStepD().Compute(1 - 0.3), new HermiteSmoothStepD().Compute(0.3));
    }
  }
}
