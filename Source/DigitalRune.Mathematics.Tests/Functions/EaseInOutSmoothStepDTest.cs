using NUnit.Framework;


namespace DigitalRune.Mathematics.Functions.Tests
{
  [TestFixture]
  public class EaseInOutSmoothStepDTest
  {
    [Test]
    public void Compute()
    {
      Assert.IsTrue(Numeric.AreEqual(0, new EaseInOutSmoothStepD().Compute(-1)));
      Assert.IsTrue(Numeric.AreEqual(0, new EaseInOutSmoothStepD().Compute(0)));
      Assert.Greater(0.5, new EaseInOutSmoothStepD().Compute(0.3));
      Assert.IsTrue(Numeric.AreEqual(0.5, new EaseInOutSmoothStepD().Compute(0.5)));
      Assert.Less(0.5, new EaseInOutSmoothStepD().Compute(0.6));
      Assert.IsTrue(Numeric.AreEqual(1, new EaseInOutSmoothStepD().Compute(1)));
      Assert.IsTrue(Numeric.AreEqual(1, new EaseInOutSmoothStepD().Compute(2)));
    }
  }
}
