using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests.Animations.NBlendAnimation
{
  [TestFixture]
  public class BlendGroupTest
  {
    [Test]
    public void EnumeratorTest()
    {
      var blendGroup = new BlendGroup();
      Assert.AreEqual(0, blendGroup.Count());

      var animation0 = new SingleFromToByAnimation();
      var animation1 = new SingleFromToByAnimation();
      blendGroup = new BlendGroup { animation0, animation1 };
      Assert.AreEqual(2, blendGroup.Count());
      Assert.AreEqual(animation0, blendGroup.ElementAt(0));
      Assert.AreEqual(animation1, blendGroup.ElementAt(1));
    }


    [Test]
    public void CopyTo()
    {
      var blendGroup = new BlendGroup();
      ITimeline[] array = new ITimeline[0];
      Assert.That(() => ((IList<ITimeline>)blendGroup).CopyTo(null, 0), Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => ((IList<ITimeline>)blendGroup).CopyTo(array, -1), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => ((IList<ITimeline>)blendGroup).CopyTo(array, 1), Throws.ArgumentException);
      Assert.That(() => ((IList<ITimeline>)blendGroup).CopyTo(array, 0), Throws.Nothing);

      var animation0 = new SingleFromToByAnimation();
      var animation1 = new SingleFromToByAnimation();
      blendGroup = new BlendGroup { animation0, animation1 };

      array = new ITimeline[1];
      Assert.That(() => ((IList<ITimeline>)blendGroup).CopyTo(array, 0), Throws.ArgumentException);

      array = new ITimeline[2];
      ((IList<ITimeline>)blendGroup).CopyTo(array, 0);
      Assert.AreEqual(animation0, array[0]);
      Assert.AreEqual(animation1, array[1]);

      array = new ITimeline[3];
      ((IList<ITimeline>)blendGroup).CopyTo(array, 1);
      Assert.AreEqual(null, array[0]);
      Assert.AreEqual(animation0, array[1]);
      Assert.AreEqual(animation1, array[2]);
    }



    [Test]
    public void EmptyBlendGroup()
    {
      var property1 = new AnimatableProperty<float>();
      var blendGroup = new BlendGroup();

      Assert.That(() => blendGroup.GetWeight(0), Throws.TypeOf<ArgumentOutOfRangeException>());
      Assert.That(() => blendGroup.SetWeight(0, 1.0f), Throws.TypeOf<ArgumentOutOfRangeException>());

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(blendGroup, property1);
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(AnimationState.Stopped, controller.State);

      blendGroup.SynchronizeDurations();
      blendGroup.Clear();
      controller = manager.StartAnimation(blendGroup, property1);
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }


    [Test]
    public void ShouldDoNothingWhenWeightsAreZero()
    {
      var blendGroup = new BlendGroup { FillBehavior = FillBehavior.Stop };
      var animation1 = new SingleFromToByAnimation { From = 0, To = 100, Duration = TimeSpan.FromSeconds(1.0) };
      var animation2 = new SingleFromToByAnimation { From = 100, To = 300, Duration = TimeSpan.FromSeconds(2.0) };
      blendGroup.Add(animation1);
      blendGroup.Add(animation2);
      blendGroup.SetWeight(0, 0);
      blendGroup.SetWeight(1, 0);

      Assert.That(() => blendGroup.SynchronizeDurations(), Throws.Nothing);
      blendGroup.Update();
      Assert.AreEqual(0.0f, blendGroup.GetNormalizedWeight(0));
      Assert.AreEqual(0.0f, blendGroup.GetNormalizedWeight(1));
    }


    [Test]
    public void BlendGroupWithOneAnimation()
    {
      var property1 = new AnimatableProperty<float> { Value = 123.45f };

      var blendGroup = new BlendGroup { FillBehavior = FillBehavior.Stop };
      var animation = new SingleFromToByAnimation { From = 0, To = 100, Duration = TimeSpan.FromSeconds(1.0) };
      blendGroup.Add(animation);
      Assert.AreEqual(1.0f, blendGroup.GetWeight(0));
      Assert.AreEqual(1.0f, blendGroup.GetWeight(animation));

      blendGroup.SetWeight(0, 10.0f);
      Assert.AreEqual(10.0f, blendGroup.GetWeight(0));
      Assert.AreEqual(10.0f, blendGroup.GetWeight(animation));

      blendGroup.SetWeight(animation, 0.5f);
      Assert.AreEqual(0.5f, blendGroup.GetWeight(0));
      Assert.AreEqual(0.5f, blendGroup.GetWeight(animation));

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(blendGroup, property1);
      controller.UpdateAndApply();
      Assert.AreEqual(0.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(25.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(50.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(75.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(123.45f, property1.Value);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }


    [Test]
    public void BlendGroupWithTwoAnimationsNotSynchronized()
    {
      var property1 = new AnimatableProperty<float> { Value = 123.45f };

      var blendGroup = new BlendGroup { FillBehavior = FillBehavior.Stop };
      var animation1 = new SingleFromToByAnimation { From = 0, To = 100, Duration = TimeSpan.FromSeconds(1.0) };
      var animation2 = new SingleFromToByAnimation { From = 100, To = 300, Duration = TimeSpan.FromSeconds(2.0) };
      blendGroup.Add(animation1);
      blendGroup.Add(animation2);
      Assert.AreEqual(1.0f, blendGroup.GetWeight(0));
      Assert.AreEqual(1.0f, blendGroup.GetWeight(1));
      Assert.AreEqual(TimeSpan.FromSeconds(2.0), blendGroup.GetTotalDuration());

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(blendGroup, property1);
      controller.UpdateAndApply();
      Assert.AreEqual(50.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));      // t = 0.25
      manager.ApplyAnimations();
      Assert.AreEqual((25.0f + 125.0f) / 2.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 2);
      manager.Update(TimeSpan.Zero);       // t = 0.25
      manager.ApplyAnimations();
      Assert.IsTrue(Numeric.AreEqual(10.0f / 12.0f * 25.0f + 2.0f / 12.0f * 125.0f, property1.Value));

      blendGroup.SetWeight(0, 0);
      manager.Update(TimeSpan.Zero);       // t = 0.25
      manager.ApplyAnimations();
      Assert.AreEqual(125.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 0);
      manager.Update(TimeSpan.Zero);       // t = 0.25
      manager.ApplyAnimations();
      Assert.AreEqual(25.0f, property1.Value);

      blendGroup.SetWeight(1, 2);
      manager.Update(TimeSpan.FromSeconds(0.25));      // t = 0.5
      manager.ApplyAnimations();
      Assert.IsTrue(Numeric.AreEqual(10.0f / 12.0f * 50.0f + 2.0f / 12.0f * 150.0f, property1.Value));

      manager.Update(TimeSpan.FromSeconds(1.0));       // t = 1.5
      manager.ApplyAnimations();
      Assert.IsTrue(Numeric.AreEqual(10.0f / 12.0f * 100.0f + 2.0f / 12.0f * 250.0f, property1.Value));

      manager.Update(TimeSpan.FromSeconds(1.0));       // t = 2.5
      manager.ApplyAnimations();
      Assert.AreEqual(123.45f, property1.Value);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }


    [Test]
    public void BlendGroupWithTwoAnimationsSynchronized()
    {
      var property1 = new AnimatableProperty<float> { Value = 123.45f };

      var blendGroup = new BlendGroup { FillBehavior = FillBehavior.Stop };
      var animation1 = new SingleFromToByAnimation { From = 0, To = 100, Duration = TimeSpan.FromSeconds(1.0) };
      var animation2 = new SingleFromToByAnimation { From = 100, To = 300, Duration = TimeSpan.FromSeconds(2.0) };
      blendGroup.Add(animation1);
      blendGroup.Add(animation2);
      Assert.AreEqual(1.0f, blendGroup.GetWeight(0));
      Assert.AreEqual(1.0f, blendGroup.GetWeight(1));
      Assert.AreEqual(TimeSpan.FromSeconds(2.0), blendGroup.GetTotalDuration());

      blendGroup.SynchronizeDurations();
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), blendGroup.GetTotalDuration());

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(blendGroup, property1);
      controller.UpdateAndApply();
      Assert.AreEqual(50.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.75));      // t = 0.75
      manager.ApplyAnimations();
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), blendGroup.GetTotalDuration());
      Assert.AreEqual(0.5f * 50.0f + 0.5f * 200.0f, property1.Value);

      blendGroup.SetWeight(0, 0);
      Assert.AreEqual(TimeSpan.FromSeconds(2.0), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.FromSeconds(0.25));       // t = 1.0
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 0);
      Assert.AreEqual(TimeSpan.FromSeconds(1.0), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.Zero);       // t = 1.0
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 1);
      Assert.AreEqual(new TimeSpan((long)((1.0f * 10.0f / 11.0f + 2.0f * 1.0f / 11.0f) * TimeSpan.TicksPerSecond)), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.FromSeconds(0.5));       // t = 1.5
      manager.ApplyAnimations();
      Assert.AreEqual(123.45f, property1.Value);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }


    [Test]
    public void BlendGroupWithAnimationAndTimelineGroups()
    {
      var testObject = new AnimatableObject("TestObject");
      var property1 = new AnimatableProperty<float> { Value = 123.45f };
      testObject.Properties.Add("Property1", property1);

      var blendGroup = new BlendGroup
      {
        new SingleFromToByAnimation { From = 0, To = 100, TargetProperty = "Property1" },
        new TimelineGroup { new SingleFromToByAnimation { From = 100, To = 300, Duration = TimeSpan.FromSeconds(2.0), TargetProperty = "Property1" }, },
      };
      blendGroup.SynchronizeDurations();
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), blendGroup.GetTotalDuration());

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(blendGroup, testObject);
      controller.UpdateAndApply();
      Assert.AreEqual(50.0f, property1.Value);

      manager.Update(TimeSpan.FromSeconds(0.75));      // t = 0.75
      manager.ApplyAnimations();
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), blendGroup.GetTotalDuration());
      Assert.AreEqual(0.5f * 50.0f + 0.5f * 200.0f, property1.Value);

      blendGroup.SetWeight(0, 0);
      Assert.AreEqual(TimeSpan.FromSeconds(2.0), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.FromSeconds(0.25));       // t = 1.0
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 0);
      Assert.AreEqual(TimeSpan.FromSeconds(1.0), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.Zero);       // t = 1.0
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property1.Value);

      blendGroup.SetWeight(0, 10);
      blendGroup.SetWeight(1, 1);
      Assert.AreEqual(new TimeSpan((long)((1.0f * 10.0f / 11.0f + 2.0f * 1.0f / 11.0f) * TimeSpan.TicksPerSecond)), blendGroup.GetTotalDuration());
      manager.Update(TimeSpan.FromSeconds(0.5));       // t = 1.5
      manager.ApplyAnimations();
      Assert.AreEqual(AnimationState.Filling, controller.State);
      Assert.IsTrue(Numeric.AreEqual(100.0f * 10.0f / 11.0f + 300.0f * 1.0f / 11.0f, property1.Value));

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.AreEqual(123.45f, property1.Value);
    }
  }
}
