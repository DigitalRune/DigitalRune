using DigitalRune.Animation.Traits;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class Vector3FKeyFrameAnimationTest
  {
    [Test]
    public void TraitsTest()
    {
      var animationEx = new Vector3FKeyFrameAnimation();
      Assert.AreEqual(Vector3FTraits.Instance, animationEx.Traits);
    }
  }
}
