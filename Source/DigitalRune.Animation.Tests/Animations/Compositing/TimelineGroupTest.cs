using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class TimelineGroupTest
  {
    [Test]
    public void GetDefaultDurationTest()
    {
      var timelineGroup = new TimelineGroup();
      var animation = new Vector2FAnimation();
      var animation2 = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation(),
        Delay = TimeSpan.FromSeconds(10),
      };
      var animation3 = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation(),
        Delay = TimeSpan.FromSeconds(8),
        Duration = TimeSpan.FromSeconds(4),
      };

      Assert.AreEqual(TimeSpan.FromSeconds(0.0), timelineGroup.GetTotalDuration());

      timelineGroup.Add(animation);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), timelineGroup.GetTotalDuration());

      timelineGroup.Add(animation2);
      Assert.AreEqual(TimeSpan.FromSeconds(11.0), timelineGroup.GetTotalDuration());

      timelineGroup.Add(animation3);
      Assert.AreEqual(TimeSpan.FromSeconds(12.0), timelineGroup.GetTotalDuration());
    }


    [Test]
    public void CreateInstanceTest()
    {
      var animation1 = new SingleFromToByAnimation();
      var animation2 = new SingleFromToByAnimation();
      var animation3 = new SingleFromToByAnimation();
      
      var childGroup = new TimelineGroup();
      childGroup.Add(animation2);
      childGroup.Add(animation3);

      var rootGroup = new TimelineGroup();
      rootGroup.Add(animation1);
      rootGroup.Add(childGroup);

      var animationInstance = rootGroup.CreateInstance();
      Assert.IsNotNull(animationInstance);
      Assert.AreEqual(2, animationInstance.Children.Count);
      Assert.That(animationInstance.Children[0], Is.TypeOf<AnimationInstance<float>>());
      Assert.That(animationInstance.Children[0].Animation, Is.EqualTo(animation1));
      Assert.That(animationInstance.Children[1], Is.TypeOf<AnimationInstance>());
      Assert.That(animationInstance.Children[1].Animation, Is.EqualTo(childGroup));
      Assert.That(animationInstance.Children[1].Children[0], Is.TypeOf<AnimationInstance<float>>());
      Assert.That(animationInstance.Children[1].Children[0].Animation, Is.EqualTo(animation2));
      Assert.That(animationInstance.Children[1].Children[1], Is.TypeOf<AnimationInstance<float>>());
      Assert.That(animationInstance.Children[1].Children[1].Animation, Is.EqualTo(animation3));
    }
  }
}
