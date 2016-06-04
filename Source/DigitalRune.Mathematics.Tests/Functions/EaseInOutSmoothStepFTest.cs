using NUnit.Framework;


namespace DigitalRune.Mathematics.Functions.Tests
{
  [TestFixture]
  public class EaseInOutSmoothStepFTest
  {
    [Test]
    public void Compute()
    {
      Assert.IsTrue(Numeric.AreEqual(0, new EaseInOutSmoothStepF().Compute(-1)));
      Assert.IsTrue(Numeric.AreEqual(0, new EaseInOutSmoothStepF().Compute(0)));
      Assert.Greater(0.5f, new EaseInOutSmoothStepF().Compute(0.3f));
      Assert.IsTrue(Numeric.AreEqual(0.5f, new EaseInOutSmoothStepF().Compute(0.5f)));
      Assert.Less(0.5f, new EaseInOutSmoothStepF().Compute(0.6f));
      Assert.IsTrue(Numeric.AreEqual(1, new EaseInOutSmoothStepF().Compute(1)));
      Assert.IsTrue(Numeric.AreEqual(1, new EaseInOutSmoothStepF().Compute(2)));
    }
  }
}
