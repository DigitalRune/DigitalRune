using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public class QuadraticEaseTest : BaseEasingFunctionTest<QuadraticEase>
  {
    [SetUp]
    public void Setup()
    {
      EasingFunction = new QuadraticEase();
    }


    [Test]
    public void EaseInTest()
    {
      EasingFunction.Mode = EasingMode.EaseIn;
      TestEase();
    }


    [Test]
    public void EaseOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseOut;
      TestEase();
    }


    [Test]
    public void EaseInOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseInOut;
      TestEase();

      // Check center.
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");
    }
  }
}

