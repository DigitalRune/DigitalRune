using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public class ElasticEaseTest : BaseEasingFunctionTest<ElasticEase>
  {
    [SetUp]
    public void Setup()
    {
      EasingFunction = new ElasticEase();
    }


    [Test]
    public void EaseInTest()
    {
      EasingFunction.Mode = EasingMode.EaseIn;
      TestEase();

      EasingFunction.Oscillations = 4;
      EasingFunction.Springiness = 4;
      TestEase();

      EasingFunction.Oscillations = 0;
      EasingFunction.Springiness = 0;
      TestEase();

      EasingFunction.Oscillations = -1;
      EasingFunction.Springiness = -1;
      TestEase();
    }


    [Test]
    public void EaseOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseOut;
      TestEase();

      EasingFunction.Oscillations = 4;
      EasingFunction.Springiness = 4;
      TestEase();

      EasingFunction.Oscillations = 0;
      EasingFunction.Springiness = 0;
      TestEase();

      EasingFunction.Oscillations = -1;
      EasingFunction.Springiness = -1;
      TestEase();
    }


    [Test]
    public void EaseInOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseInOut;
      TestEase();

      // Check center.
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Oscillations = 4;
      EasingFunction.Springiness = 4;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Oscillations = 0;
      EasingFunction.Springiness = 0;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Oscillations = -1;
      EasingFunction.Springiness = -1;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");
    }
  }
}

