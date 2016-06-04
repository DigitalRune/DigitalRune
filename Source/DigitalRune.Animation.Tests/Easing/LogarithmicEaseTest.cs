using System;
using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public class LogarithmicEaseTest : BaseEasingFunctionTest<LogarithmicEase>
  {
    [SetUp]
    public void Setup()
    {
      EasingFunction = new LogarithmicEase();
    }


    [Test]
    public void ShouldThrowWhenBaseIsInvalid()
    {
      Assert.That(() => EasingFunction.Base = -1, Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => EasingFunction.Base = 0, Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => EasingFunction.Base = 1, Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void EaseInTest()
    {
      EasingFunction.Mode = EasingMode.EaseIn;
      TestEase();

      EasingFunction.Base = 1.5f;
      TestEase();

      EasingFunction.Base = 4.0f;
      TestEase();

      EasingFunction.Base = 10.3f;
      TestEase();
    }


    [Test]
    public void EaseOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseOut;
      TestEase();

      EasingFunction.Base = 1.5f;
      TestEase();

      EasingFunction.Base = 4.0f;
      TestEase();

      EasingFunction.Base = 10.3f;
      TestEase();
    }


    [Test]
    public void EaseInOutTest()
    {
      EasingFunction.Mode = EasingMode.EaseInOut;
      TestEase();

      // Check center.
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Base = 1.5f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Base = 4.0f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");

      EasingFunction.Base = 10.3f;
      TestEase();
      Assert.IsTrue(Numeric.AreEqual(0.5f, EasingFunction.Ease(0.5f)), "Easing function using EaseInOut failed for t = 0.5.");
    }
  }
}

