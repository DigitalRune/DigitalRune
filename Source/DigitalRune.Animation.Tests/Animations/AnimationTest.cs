using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationsTest
  {
    [Test]
    public void TargetObjectTest()
    {
      var animation = new SingleFromToByAnimation();
      Assert.IsNull(animation.TargetObject);

      animation.TargetObject = "";
      Assert.IsEmpty(animation.TargetObject);

      animation.TargetObject = "Object XY";
      Assert.AreEqual("Object XY", animation.TargetObject);
    }


    [Test]
    public void TargetPropertyTest()
    {
      var animation = new SingleFromToByAnimation();
      Assert.IsNull(animation.TargetProperty);

      animation.TargetProperty = "";
      Assert.IsEmpty(animation.TargetProperty);

      animation.TargetProperty = "Property XY";
      Assert.AreEqual("Property XY", animation.TargetProperty);
    }


    [Test]
    public void AnimationStateTest()
    {
      var animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation
        {
          From = 100,
          To = 200,
          Duration = TimeSpan.FromSeconds(6.0),
        },
        Delay = TimeSpan.FromSeconds(10),
        Speed = 2,
        FillBehavior = FillBehavior.Hold,
      };

      float defaultSource = 1.0f;
      float defaultTarget = 2.0f;

      // Delayed
      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(-1.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Delayed, animation.GetState(TimeSpan.FromSeconds(-1.0)));

      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Delayed, animation.GetState(TimeSpan.Zero));

      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(9.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Delayed, animation.GetState(TimeSpan.FromSeconds(9.0)));

      // Playing
      Assert.AreEqual(100.0f, animation.GetValue(TimeSpan.FromSeconds(10.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Playing, animation.GetState(TimeSpan.FromSeconds(10.0)));

      Assert.AreEqual(150.0f, animation.GetValue(TimeSpan.FromSeconds(11.5), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Playing, animation.GetState(TimeSpan.FromSeconds(11.5)));

      Assert.AreEqual(200.0f, animation.GetValue(TimeSpan.FromSeconds(13.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Playing, animation.GetState(TimeSpan.FromSeconds(13.0)));

      // Filling
      Assert.AreEqual(200.0f, animation.GetValue(TimeSpan.FromSeconds(14.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Filling, animation.GetState(TimeSpan.FromSeconds(14.0)));

      // Stopped
      animation.FillBehavior = FillBehavior.Stop;
      Assert.AreEqual(defaultSource, animation.GetValue(TimeSpan.FromSeconds(14.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Stopped, animation.GetState(TimeSpan.FromSeconds(14.0)));
    }


    [Test]
    public void ZeroDuration()
    {
      var animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation
        {
          From = 100,
          To = 200,
          Duration = TimeSpan.FromSeconds(0),
        },
        Delay = TimeSpan.FromSeconds(10),
        Speed = 2,
        FillBehavior = FillBehavior.Hold,
      };

      float defaultSource = 1.0f;
      float defaultTarget = 2.0f;

      Assert.AreEqual(200.0f, animation.GetValue(TimeSpan.FromSeconds(10.0), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Playing, animation.GetState(TimeSpan.FromSeconds(10.0)));

      Assert.AreEqual(200.0f, animation.GetValue(TimeSpan.FromSeconds(10.1), defaultSource, defaultTarget));
      Assert.AreEqual(AnimationState.Filling, animation.GetState(TimeSpan.FromSeconds(10.1)));
    }


    [Test]
    public void IsAdditiveTest()
    {
      var animation = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation
        {
          From = 100,
          To = 200,
          Duration = TimeSpan.FromSeconds(6.0),
        },
        IsAdditive = true,
        Delay = TimeSpan.FromSeconds(10),
        Speed = 2,
        FillBehavior = FillBehavior.Hold,
      };

      float defaultSource = 1.0f;
      float defaultTarget = 2.0f;

      float value = animation.GetValue(TimeSpan.FromSeconds(-1.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource, value);

      value = animation.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource, value);

      value = animation.GetValue(TimeSpan.FromSeconds(9.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource, value);

      value = animation.GetValue(TimeSpan.FromSeconds(10.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource + 100.0f, value);

      value = animation.GetValue(TimeSpan.FromSeconds(11.5), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource + 150.0f, value);

      value = animation.GetValue(TimeSpan.FromSeconds(13.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource + 200.0f, value);

      value = animation.GetValue(TimeSpan.FromSeconds(14.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource + 200.0f, value);

      animation.FillBehavior = FillBehavior.Stop;
      value = animation.GetValue(TimeSpan.FromSeconds(14.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource, value);
    }


    [Test]
    public void CreateInstanceTest()
    {
      var animationInstance = new SingleFromToByAnimation().CreateInstance();
      Assert.IsNotNull(animationInstance);

      var timelineInstance = ((ITimeline)new SingleFromToByAnimation()).CreateInstance();
      Assert.IsNotNull(timelineInstance);
    }
  }
}
