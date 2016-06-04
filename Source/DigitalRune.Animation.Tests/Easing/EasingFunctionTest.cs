using NUnit.Framework;


namespace DigitalRune.Animation.Easing.Tests
{
  [TestFixture]
  public class EasingFunctionTest
  {
    [Test]
    public void ShouldThrowWhenModeIsInvalid()
    {
      var easingFunction = new QuadraticEase();      
      Assert.That(
        () =>
        {
          easingFunction.Mode = (EasingMode)99;
          easingFunction.Ease(0.0f);
        }, Throws.TypeOf<InvalidAnimationException>());
      
    }
  }
}