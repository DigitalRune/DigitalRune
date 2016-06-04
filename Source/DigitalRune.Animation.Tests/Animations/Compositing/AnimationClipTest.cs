using System;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationClipTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var animationClip = new AnimationClip<float>();
      Assert.IsNull(animationClip.Traits);
      Assert.IsNull(animationClip.Animation);
      Assert.IsFalse(animationClip.ClipStart.HasValue);
      Assert.IsFalse(animationClip.ClipEnd.HasValue);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), animationClip.ClipOffset);
      Assert.AreEqual(LoopBehavior.Constant, animationClip.LoopBehavior);
    }


    [Test]
    [ExpectedException(typeof(InvalidAnimationException))]
    public void ShouldThrowWhenClipIsInvalid()
    {
      var animationClip = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation(),
        ClipStart = TimeSpan.FromSeconds(0.75),
        ClipEnd = TimeSpan.FromSeconds(0.25f),
      };

      float defaultSource = 100.0f;
      float defaultTarget = 200.0f;
      animationClip.GetValue(TimeSpan.FromSeconds(0.25), defaultSource, defaultTarget);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenDurationIsNegative()
    {
      var animation = new AnimationClip<float>();
      animation.Duration = TimeSpan.FromSeconds(-1.0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenSpeedIsNegative()
    {
      var animation = new AnimationClip<float>();
      animation.Speed = -1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenSpeedIsNaN()
    {
      var animation = new AnimationClip<float>();
      animation.Speed = float.NaN;
    }


    [Test]
    public void TargetObjectTest()
    {
      var animation = new AnimationClip<float>();
      Assert.IsNull(animation.TargetObject);

      animation.TargetObject = "";
      Assert.IsEmpty(animation.TargetObject);

      animation.TargetObject = "Object XY";
      Assert.AreEqual("Object XY", animation.TargetObject);
    }


    [Test]
    public void TargetPropertyTest()
    {
      var animation = new AnimationClip<float>();
      Assert.IsNull(animation.TargetProperty);

      animation.TargetProperty = "";
      Assert.IsEmpty(animation.TargetProperty);

      animation.TargetProperty = "Property XY";
      Assert.AreEqual("Property XY", animation.TargetProperty);
    }


    [Test]
    public void TraitsTest()
    {
      var animationClip = new AnimationClip<float>();
      Assert.IsNull(animationClip.Traits);

      animationClip.Animation = new SingleFromToByAnimation();
      Assert.That(animationClip.Traits, Is.TypeOf<SingleTraits>());
    }


    [Test]
    public void GetTotalDuration()
    {
      var animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(1.0) },
        Delay = TimeSpan.FromSeconds(10),
        Duration = null,
        Speed = 1.0f,
      };

      // Default duration is 1 second.
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), animation.GetTotalDuration());

      animation.Duration = TimeSpan.FromSeconds(5.0);
      Assert.AreEqual(TimeSpan.FromSeconds(15.0), animation.GetTotalDuration());

      animation.Speed = 2.0f;
      Assert.AreEqual(TimeSpan.FromSeconds(12.5), animation.GetTotalDuration());

      animation.Delay = TimeSpan.FromSeconds(-1.0);
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), animation.GetTotalDuration());

      animation.Speed = 0.0f;
      Assert.AreEqual(TimeSpan.MaxValue, animation.GetTotalDuration());
    }


    [Test]
    public void GetTotalDurationTest()
    {
      var animationClip = new AnimationClip<float>();
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), animationClip.GetTotalDuration());

      animationClip.Animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation(),
        Delay = TimeSpan.FromSeconds(10),
        Duration = TimeSpan.FromSeconds(4.0),
        Speed = 2.0f,
      };
      Assert.AreEqual(TimeSpan.FromSeconds(12.0), animationClip.GetTotalDuration());
    }


    [Test]
    public void GetValueTest()
    {
      float defaultSource = 100.0f;
      float defaultTarget = 200.0f;

      var animationClip = new AnimationClip<float>();

      Assert.That(() => { animationClip.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget); }, Throws.TypeOf<InvalidAnimationException>());

      animationClip.Delay = TimeSpan.FromSeconds(100);
      animationClip.Speed = 1.0f;
      animationClip.ClipStart = TimeSpan.FromSeconds(10.5);
      animationClip.ClipEnd = TimeSpan.FromSeconds(11.5);
      animationClip.ClipOffset = TimeSpan.FromSeconds(-0.5);
      animationClip.Duration = TimeSpan.FromSeconds(4.0);
      animationClip.LoopBehavior = LoopBehavior.Oscillate;

      animationClip.Animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(4.0) },
        Delay = TimeSpan.FromSeconds(10),
        Duration = null,
        Speed = 2.0f,
      };

      Assert.AreEqual(defaultSource, animationClip.GetValue(TimeSpan.FromSeconds(99.0), defaultSource, defaultTarget));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(100.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(125.0f, animationClip.GetValue(TimeSpan.FromSeconds(100.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(101.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(175.0f, animationClip.GetValue(TimeSpan.FromSeconds(101.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(102.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(125.0f, animationClip.GetValue(TimeSpan.FromSeconds(102.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(103.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(175.0f, animationClip.GetValue(TimeSpan.FromSeconds(103.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(104.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(150.0f, animationClip.GetValue(TimeSpan.FromSeconds(104.5), defaultSource, defaultTarget)));
    }


    [Test]
    public void ReverseClip()
    {
      float defaultSource = 100.0f;
      float defaultTarget = 200.0f;

      var animationClip = new AnimationClip<float>();

      Assert.That(() => { animationClip.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget); }, Throws.TypeOf<InvalidAnimationException>());

      animationClip.Delay = TimeSpan.FromSeconds(100);
      animationClip.Speed = 1.0f;
      animationClip.ClipStart = TimeSpan.FromSeconds(10.5);
      animationClip.ClipEnd = TimeSpan.FromSeconds(11.5);
      animationClip.ClipOffset = TimeSpan.FromSeconds(-0.1);
      animationClip.IsClipReversed = true;
      animationClip.Duration = TimeSpan.FromSeconds(4.0);
      animationClip.LoopBehavior = LoopBehavior.Oscillate;

      animationClip.Animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(4.0) },
        Delay = TimeSpan.FromSeconds(10),
        Duration = null,
        Speed = 2.0f,
      };

      Assert.IsTrue(animationClip.IsClipReversed);
      Assert.AreEqual(defaultSource, animationClip.GetValue(TimeSpan.FromSeconds(99.0), defaultSource, defaultTarget));
      Assert.IsTrue(Numeric.AreEqual(170.0f, animationClip.GetValue(TimeSpan.FromSeconds(100.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(155.0f, animationClip.GetValue(TimeSpan.FromSeconds(100.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(130.0f, animationClip.GetValue(TimeSpan.FromSeconds(101.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(145.0f, animationClip.GetValue(TimeSpan.FromSeconds(101.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(170.0f, animationClip.GetValue(TimeSpan.FromSeconds(102.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(155.0f, animationClip.GetValue(TimeSpan.FromSeconds(102.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(130.0f, animationClip.GetValue(TimeSpan.FromSeconds(103.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(145.0f, animationClip.GetValue(TimeSpan.FromSeconds(103.5), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(170.0f, animationClip.GetValue(TimeSpan.FromSeconds(104.0), defaultSource, defaultTarget)));
      Assert.IsTrue(Numeric.AreEqual(170.0f, animationClip.GetValue(TimeSpan.FromSeconds(104.5), defaultSource, defaultTarget)));
    }
  }
}
