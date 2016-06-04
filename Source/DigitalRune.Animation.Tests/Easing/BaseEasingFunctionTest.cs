using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public abstract class BaseEasingFunctionTest<T> where T : IEasingFunction
  {
    // The easing function to be tested.
    protected T EasingFunction { get; set; }


    protected void TestEase()
    {
      // Check limits.
      Assert.IsTrue(Numeric.IsZero(EasingFunction.Ease(0.0f)), "Easing function failed for t = 0.");
      Assert.IsTrue(Numeric.AreEqual(1.0f, EasingFunction.Ease(1.0f)), "Easing function failed for t = 1.");

      // Sample function at several intervals.
      const float from = -2.5f;
      const float to = 2.5f;
      const float step = 0.01f;
      for (float t = from; t < to; t += step)
        Assert.IsTrue(Numeric.IsFinite(EasingFunction.Ease(t)), "Sampling easing function at " + t + " failed.");
    }
  }
}
