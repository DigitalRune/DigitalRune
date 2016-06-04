using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public class ExponentialEaseTest : BaseEasingFunctionTest<ExponentialEase>
  {
    [SetUp]
    public void Setup()
    {
      EasingFunction = new ExponentialEase();
    }


    [Test]
    public void EaseInTest()
    {
      EasingFunction.Mode = EasingMode.EaseIn;
      TestEase();

      EasingFunction.Exponent = 3.5f;
      TestEase();

      EasingFunction.Exponent = 0.0f;
      TestEase();

      EasingFunction.Exponent = -2.3f;
      TestEase();
    }


    [Test]
    public void EaseOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseOut;
      TestEase();

      EasingFunction.Exponent = 3.5f;
      TestEase();

      EasingFunction.Exponent = 0.0f;
      TestEase();

      EasingFunction.Exponent = -2.3f;
      TestEase();
    }


    [Test]
    public void EaseInOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseInOut;
      TestEase();

      // Check center.
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Exponent = 3.5f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Exponent = 0.0f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Exponent = -2.3f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");
    }
  }
}

