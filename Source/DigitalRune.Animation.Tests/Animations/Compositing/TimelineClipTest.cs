using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class TimelineClipTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var timelineClip = new TimelineClip();
      Assert.IsNull(timelineClip.Timeline);
      Assert.False(timelineClip.ClipStart.HasValue);
      Assert.False(timelineClip.ClipEnd.HasValue);
      Assert.IsFalse(timelineClip.IsClipReversed);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), timelineClip.ClipOffset);
      Assert.AreEqual(LoopBehavior.Constant, timelineClip.LoopBehavior);
    }


    [Test]
    public void ShouldThrowWhenClipIsInvalid()
    {
      var timelineClip = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation(),
        ClipStart = TimeSpan.FromSeconds(0.75),
        ClipEnd = TimeSpan.FromSeconds(0.25),
      };

      Assert.That(() => timelineClip.GetTotalDuration(), Throws.TypeOf<InvalidAnimationException>());
      Assert.That(() => timelineClip.GetState(TimeSpan.Zero), Throws.TypeOf<InvalidAnimationException>());
      Assert.That(() => timelineClip.GetAnimationTime(TimeSpan.Zero), Throws.TypeOf<InvalidAnimationException>());
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenDurationIsNegative()
    {
      var animation = new TimelineClip();
      animation.Duration = TimeSpan.FromSeconds(-1.0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenSpeedIsNegative()
    {
      var animation = new TimelineClip();
      animation.Speed = -1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ShouldThrowWhenSpeedIsNaN()
    {
      var animation = new TimelineClip();
      animation.Speed = float.NaN;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ShouldThrowWhenLoopBehaviorIsCycleOffset()
    {
      var timelineClip = new TimelineClip
      {
        LoopBehavior = LoopBehavior.CycleOffset
      };
    }


    [Test]
    public void TargetObjectTest()
    {
      var animation = new TimelineClip();
      Assert.IsNull(animation.TargetObject);

      animation.TargetObject = "";
      Assert.IsEmpty(animation.TargetObject);

      animation.TargetObject = "Object XY";
      Assert.AreEqual("Object XY", animation.TargetObject);
    }


    [Test]
    public void GetTotalDuration()
    {
      var animation = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(1.0) },
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
    public void GetTotalDuration2()
    {
      var timelineClip = new TimelineClip();
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), timelineClip.GetTotalDuration());

      timelineClip.Timeline = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation(),
        Delay = TimeSpan.FromSeconds(10),
        Duration = TimeSpan.FromSeconds(4.0),
        Speed = 2.0f,
      };
      Assert.AreEqual(TimeSpan.FromSeconds(12.0), timelineClip.GetTotalDuration());
    }


    [Test]
    public void GetValueTest()
    {
      var timelineClip = new TimelineClip();
      timelineClip.GetState(TimeSpan.Zero);     // Should not crash.
      timelineClip.GetAnimationTime(TimeSpan.FromSeconds(0.0)); // Should not crash.

      timelineClip.Delay = TimeSpan.FromSeconds(100);
      timelineClip.Speed = 1.0f;
      timelineClip.ClipStart = TimeSpan.FromSeconds(10.5);
      timelineClip.ClipEnd = TimeSpan.FromSeconds(11.5);
      timelineClip.ClipOffset = TimeSpan.FromSeconds(-0.5);
      timelineClip.Duration = TimeSpan.FromSeconds(4.0);
      timelineClip.LoopBehavior = LoopBehavior.Oscillate;

      timelineClip.Timeline = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(4.0) },
        Delay = TimeSpan.FromSeconds(10),
        Duration = null,
        Speed = 2.0f,
      };

      // Delayed
      Assert.AreEqual(AnimationState.Delayed, timelineClip.GetState(TimeSpan.FromSeconds(99.0)));
      Assert.IsFalse(timelineClip.GetAnimationTime(TimeSpan.FromSeconds(99.0)).HasValue);

      // Playing
      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(100.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(100.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(100.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.5), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(100.5)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(101.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(101.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(101.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.5), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(101.5)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(102.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(102.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(102.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.5), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(102.5)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(103.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(103.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(103.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.5), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(103.5)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(104.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(104.0)));

      // Filling
      Assert.AreEqual(AnimationState.Filling, timelineClip.GetState(TimeSpan.FromSeconds(104.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(104.5)));

      timelineClip.FillBehavior = FillBehavior.Stop;

      // Stopped
      Assert.AreEqual(AnimationState.Stopped, timelineClip.GetState(TimeSpan.FromSeconds(104.5)));
      Assert.IsFalse(timelineClip.GetAnimationTime(TimeSpan.FromSeconds(104.5)).HasValue);
    }


    [Test]
    public void ReverseClip()
    {
      var timelineClip = new TimelineClip();
      timelineClip.GetState(TimeSpan.Zero);     // Should not crash.
      timelineClip.GetAnimationTime(TimeSpan.FromSeconds(0.0)); // Should not crash.

      timelineClip.Delay = TimeSpan.FromSeconds(100);
      timelineClip.Speed = 1.0f;
      timelineClip.ClipStart = TimeSpan.FromSeconds(10);
      timelineClip.ClipEnd = TimeSpan.FromSeconds(12);
      timelineClip.ClipOffset = TimeSpan.FromSeconds(-0.1);
      timelineClip.IsClipReversed = true;
      timelineClip.Duration = TimeSpan.FromSeconds(8.0);
      timelineClip.LoopBehavior = LoopBehavior.Oscillate;

      timelineClip.Timeline = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation { Duration = TimeSpan.FromSeconds(4.0) },
        Delay = TimeSpan.FromSeconds(10),
        Duration = null,
        Speed = 2.0f,
      };

      Assert.IsTrue(timelineClip.IsClipReversed);

      // Delayed
      Assert.AreEqual(AnimationState.Delayed, timelineClip.GetState(TimeSpan.FromSeconds(99.0)));
      Assert.IsFalse(timelineClip.GetAnimationTime(TimeSpan.FromSeconds(99.0)).HasValue);

      // Playing
      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(100.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(100.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(101.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.1), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(101.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(102.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.1), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(102.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(103.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(103.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(104.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(104.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(105.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.1), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(105.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(106.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.1), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(106.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(107.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(10.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(107.0)));

      Assert.AreEqual(AnimationState.Playing, timelineClip.GetState(TimeSpan.FromSeconds(108.0)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(108.0)));

      // Filling
      Assert.AreEqual(AnimationState.Filling, timelineClip.GetState(TimeSpan.FromSeconds(108.5)));
      Assert.AreEqual(TimeSpan.FromSeconds(11.9), timelineClip.GetAnimationTime(TimeSpan.FromSeconds(108.5)));

      timelineClip.FillBehavior = FillBehavior.Stop;

      // Stopped
      Assert.AreEqual(AnimationState.Stopped, timelineClip.GetState(TimeSpan.FromSeconds(108.5)));
      Assert.IsFalse(timelineClip.GetAnimationTime(TimeSpan.FromSeconds(108.5)).HasValue);
    }


    [Test]
    public void CreateInstanceTest()
    {
      var timelineClip = new TimelineClip
      {
        Timeline = new SingleFromToByAnimation(),
        ClipStart = TimeSpan.FromSeconds(0.75),
        ClipEnd = TimeSpan.FromSeconds(0.25),
      };

      var animationInstance = timelineClip.CreateInstance();
      Assert.IsNotNull(animationInstance);
      Assert.AreEqual(1, animationInstance.Children.Count);
      Assert.AreEqual(timelineClip.Timeline, animationInstance.Children[0].Animation);
      Assert.AreEqual(0, animationInstance.Children[0].Children.Count);
    }
  }
}
