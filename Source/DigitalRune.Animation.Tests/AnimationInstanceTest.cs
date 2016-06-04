using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationInstanceTest
  {
    [Test]
    public void ShouldThrowWhenAnimationWeightIsOutOfRange()
    {
      var animationInstance = new SingleFromToByAnimation().CreateInstance();

      // Valid range.
      animationInstance.Weight = 0.0f;
      animationInstance.Weight = 1.0f;

      // Invalid range.
      Assert.That(() => animationInstance.Weight = -0.1f, Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => animationInstance.Weight = 1.1f, Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => animationInstance.Weight = float.NaN, Throws.TypeOf<ArgumentOutOfRangeException>());
    }


    [Test]
    public void AnimationBlending()
    {
      var animation = new SingleFromToByAnimation
      {
        From = 100,
        To = 200,
        IsAdditive = true,
        Duration = TimeSpan.FromSeconds(1),
      };

      var animationInstance = animation.CreateInstance() as AnimationInstance<float>;
      animationInstance.Time = TimeSpan.FromSeconds(0.5);

      animationInstance.Weight = 0.0f;
      Assert.AreEqual(1.0f, animationInstance.GetValue(1.0f, 2.0f));

      animationInstance.Weight = 1.0f;
      Assert.AreEqual(151.0f, animationInstance.GetValue(1.0f, 2.0f));

      animationInstance.Weight = 0.75f;
      Assert.AreEqual(1.0f + 0.75f * 150.0f, animationInstance.GetValue(1.0f, 2.0f));
    }
  }
}
