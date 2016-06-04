using System;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class Vector2FAnimationTest
  {
    [Test]
    public void TraitsTest()
    {
      var animationEx = new Vector2FAnimation();
      Assert.AreEqual(Vector2FTraits.Instance, animationEx.Traits);
    }


    [Test]
    public void GetTotalDurationTest()
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

      var animation2 = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation
        {
          From = 10,
          To = 20,
          Duration = TimeSpan.FromSeconds(5.0),
        },
        Delay = TimeSpan.Zero,
        Speed = 1,
        FillBehavior = FillBehavior.Hold,
      };

      var animationEx = new Vector2FAnimation();
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), animationEx.GetTotalDuration());

      animationEx = new Vector2FAnimation();
      animationEx.X = animation;
      Assert.AreEqual(TimeSpan.FromSeconds(13.0), animationEx.GetTotalDuration());

      animationEx = new Vector2FAnimation();
      animationEx.Y = animation;
      Assert.AreEqual(TimeSpan.FromSeconds(13.0), animationEx.GetTotalDuration());

      animationEx = new Vector2FAnimation();
      animationEx.X = animation;
      animationEx.Y = animation2;
      Assert.AreEqual(TimeSpan.FromSeconds(13.0), animationEx.GetTotalDuration());

      animationEx = new Vector2FAnimation();
      animationEx.X = animation2;
      animationEx.Y = animation;
      Assert.AreEqual(TimeSpan.FromSeconds(13.0), animationEx.GetTotalDuration());
    }


    [Test]
    public void GetValueTest()
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

      var animation2 = new AnimationClip<float>
      {
        Animation = new SingleFromToByAnimation
        {
          From = 10,
          To = 20,
          Duration = TimeSpan.FromSeconds(5.0),
        },
        Delay = TimeSpan.Zero,
        Speed = 1,
        FillBehavior = FillBehavior.Hold,
      };

      var animationEx = new Vector2FAnimation
      {
        X = animation,
        Y = animation2,
      };


      var defaultSource = new Vector2F(1, 2);
      var defaultTarget = new Vector2F(5, 6);

      var result = animationEx.GetValue(TimeSpan.FromSeconds(0.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource.X, result.X); // animation has not started.
      Assert.AreEqual(10.0f, result.Y);           // animation2 has started.

      result = animationEx.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource.X, result.X); // animation has not started.
      Assert.AreEqual(20.0f, result.Y);           // animation2 has ended.

      result = animationEx.GetValue(TimeSpan.FromSeconds(5.0), defaultSource, defaultTarget);
      Assert.AreEqual(defaultSource.X, result.X); // animation has not started.
      Assert.AreEqual(20.0f, result.Y);           // animation2 has ended.

      result = animationEx.GetValue(TimeSpan.FromSeconds(13.0), defaultSource, defaultTarget);
      Assert.AreEqual(200, result.X);             // animation has ended.
      Assert.AreEqual(20.0f, result.Y);           // animation2 is filling.
    }
  }
}
