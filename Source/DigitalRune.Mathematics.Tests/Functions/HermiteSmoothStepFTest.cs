using NUnit.Framework;


namespace DigitalRune.Mathematics.Functions.Tests
{
  [TestFixture]
  public class HermiteSmoothStepFTest
  {
    [Test]
    public void Compute()
    {
      Assert.IsTrue(Numeric.AreEqual(0, new HermiteSmoothStepF().Compute(-1)));
      Assert.IsTrue(Numeric.AreEqual(0, new HermiteSmoothStepF().Compute(0)));
      Assert.IsTrue(Numeric.AreEqual(0.5f, new HermiteSmoothStepF().Compute(0.5f)));
      Assert.IsTrue(Numeric.AreEqual(1, new HermiteSmoothStepF().Compute(1)));
      Assert.IsTrue(Numeric.AreEqual(1, new HermiteSmoothStepF().Compute(2)));
      Assert.IsTrue(Numeric.AreEqual(1 - new HermiteSmoothStepF().Compute(1f-0.3f), new HermiteSmoothStepF().Compute(0.3f)));
      Assert.Greater(new HermiteSmoothStepF().Compute(1f - 0.3f), new HermiteSmoothStepF().Compute(0.3f));
    }
  }
}
