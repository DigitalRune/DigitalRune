using DigitalRune.Animation.Traits;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class QuaternionFKeyFrameAnimationTest
  {
    [Test]
    public void TraitsTest()
    {
      var animationEx = new QuaternionFKeyFrameAnimation();
      Assert.AreEqual(QuaternionFTraits.Instance, animationEx.Traits);
    }
  }
}
