using System;
using DigitalRune.Animation.Easing;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class SingleFromToByAnimationTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var animation = new SingleFromToByAnimation();
      Assert.AreEqual(TimeSpan.FromSeconds(1.0), animation.Duration);
      Assert.AreEqual(FillBehavior.Hold, animation.FillBehavior);
      Assert.IsNull(animation.TargetProperty);
      Assert.IsFalse(animation.From.HasValue);
      Assert.IsFalse(animation.To.HasValue);
      Assert.IsFalse(animation.By.HasValue);
      Assert.IsFalse(animation.IsAdditive);
      Assert.IsNull(animation.EasingFunction);
    }


    [Test]
    public void AnimateUsingDefaults()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = null;
      animation.To = null;
      animation.By = null;
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(50.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(100.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void AnimateFrom()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = 20.0f;
      animation.To = null;
      animation.By = null;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(10.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void AnimateTo()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = null;
      animation.To = 20.0f;
      animation.By = null;
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(10.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void AnimateFromTo()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = 20.0f;
      animation.To = 40.0f;
      animation.By = null;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(30.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(40.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));

      animation.By = 100.0f;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(30.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(40.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void ShouldIgnoreByIfFromToIsSet()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = 20.0f;
      animation.To = 40.0f;
      animation.By = 100.0f;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(30.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(40.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void ShouldIgnoreByIfToIsSet()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = null;
      animation.To = 40.0f;
      animation.By = 100.0f;
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(40.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void AnimateFromBy()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = 20.0f;
      animation.To = null;
      animation.By = 100.0f;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(70.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(120.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));

      animation.By = -10.0f;
      Assert.AreEqual(20.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(15.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(10.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void AnimateBy()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = null;
      animation.To = null;
      animation.By = 10.0f;
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(5.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(10.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));

      animation.By = -10.0f;
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.AreEqual(-5.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.AreEqual(-10.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }


    [Test]
    public void EasingFunctionTest()
    {
      var animation = new SingleFromToByAnimation();
      animation.From = 0.0f;
      animation.To = 100.0f;

      animation.EasingFunction = new CubicEase { Mode = EasingMode.EaseIn };
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.Greater(25.0f, animation.GetValue(TimeSpan.FromSeconds(0.25), 0.0f, 100.0f));
      Assert.Greater(50.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.Greater(75.0f, animation.GetValue(TimeSpan.FromSeconds(0.75), 0.0f, 100.0f));
      Assert.AreEqual(100.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));

      animation.EasingFunction = new CubicEase { Mode = EasingMode.EaseOut };
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.Less(25.0f, animation.GetValue(TimeSpan.FromSeconds(0.25), 0.0f, 100.0f));
      Assert.Less(50.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.Less(75.0f, animation.GetValue(TimeSpan.FromSeconds(0.75), 0.0f, 100.0f));
      Assert.AreEqual(100.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));

      animation.EasingFunction = new CubicEase { Mode = EasingMode.EaseInOut };
      Assert.AreEqual(0.0f, animation.GetValue(TimeSpan.FromSeconds(0.0), 0.0f, 100.0f));
      Assert.Greater(25.0f, animation.GetValue(TimeSpan.FromSeconds(0.25), 0.0f, 100.0f));
      Assert.AreEqual(50.0f, animation.GetValue(TimeSpan.FromSeconds(0.5), 0.0f, 100.0f));
      Assert.Less(75.0f, animation.GetValue(TimeSpan.FromSeconds(0.75), 0.0f, 100.0f));
      Assert.AreEqual(100.0f, animation.GetValue(TimeSpan.FromSeconds(1.0), 0.0f, 100.0f));
    }
  }
}
