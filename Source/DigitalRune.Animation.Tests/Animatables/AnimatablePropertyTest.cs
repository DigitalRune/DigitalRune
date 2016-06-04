using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimatablePropertyTest
  {
    [Test]
    public void ShouldHaveBaseValue()
    {
      var animatableProperty = new AnimatableProperty<float>();
      Assert.IsTrue(((IAnimatableProperty<float>)animatableProperty).HasBaseValue);
    }


    [Test]
    public void ShouldNotHaveEffectiveValueWhenNotAnimated()
    {
      var animatableProperty = new AnimatableProperty<float>();
      Assert.IsFalse(((IAnimatableProperty<float>)animatableProperty).IsAnimated);
    }


    [Test]
    public void SetterShouldSetBaseValue()
    {
      var animatableProperty = new AnimatableProperty<float>();
      animatableProperty.Value = 123;
      Assert.IsTrue(((IAnimatableProperty<float>)animatableProperty).HasBaseValue);
      Assert.IsFalse(((IAnimatableProperty<float>)animatableProperty).IsAnimated);
      Assert.AreEqual(123, ((IAnimatableProperty<float>)animatableProperty).BaseValue);
    }


    [Test]
    public void SimulateAnimation()
    {
      var animatableProperty = new AnimatableProperty<float>();

      // Simulate an animation that sets IsAnimated and the AnimationValue.
      ((IAnimatableProperty<float>)animatableProperty).IsAnimated = true;
      ((IAnimatableProperty<float>)animatableProperty).AnimationValue = 999;

      Assert.IsTrue(((IAnimatableProperty<float>)animatableProperty).HasBaseValue);
      Assert.IsTrue(((IAnimatableProperty<float>)animatableProperty).IsAnimated);
      Assert.AreEqual(0, ((IAnimatableProperty)animatableProperty).BaseValue);
      Assert.AreEqual(999, ((IAnimatableProperty)animatableProperty).AnimationValue);
      Assert.AreEqual(0, ((IAnimatableProperty<float>)animatableProperty).BaseValue);
      Assert.AreEqual(999, ((IAnimatableProperty<float>)animatableProperty).AnimationValue);
      Assert.AreEqual(999, animatableProperty.Value);

      // Remove the animation.
      ((IAnimatableProperty<float>)animatableProperty).IsAnimated = false;
      Assert.IsTrue(((IAnimatableProperty<float>)animatableProperty).HasBaseValue);
      Assert.IsFalse(((IAnimatableProperty<float>)animatableProperty).IsAnimated);
      Assert.AreEqual(0, ((IAnimatableProperty<float>)animatableProperty).BaseValue);
      Assert.AreEqual(0, animatableProperty.Value);
    }
  }
}

