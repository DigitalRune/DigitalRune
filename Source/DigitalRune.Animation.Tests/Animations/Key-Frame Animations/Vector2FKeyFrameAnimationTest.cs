using DigitalRune.Animation.Traits;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class Vector2FKeyFrameAnimationTest
  {
    [Test]
    public void TraitsTest()
    {
      var animationEx = new Vector2FKeyFrameAnimation();
      Assert.AreEqual(Vector2FTraits.Instance, animationEx.Traits);
    }
  }
}
