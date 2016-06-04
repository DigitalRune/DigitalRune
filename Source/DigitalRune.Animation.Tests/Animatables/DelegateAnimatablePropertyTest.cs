using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class DelegateAnimatablePropertyTest
  {
    private class TestObject 
    {
      public float Value { get; set; }
    }


    [Test]
    public void Constructor()
    {
      var testObject = new TestObject { Value = 123.4f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      // Getter, setter
      Assert.AreEqual(getter, property.GetValue);
      Assert.AreEqual(setter, property.SetValue);

      // IAnimatable
      Assert.IsFalse(((IAnimatableProperty)property).HasBaseValue);
      Assert.That(() => { var value = ((IAnimatableProperty)property).BaseValue; }, Throws.TypeOf<NotImplementedException>());
      Assert.IsFalse(((IAnimatableProperty)property).IsAnimated);
      Assert.AreEqual(123.4f, ((IAnimatableProperty)property).AnimationValue);

      // IAnimatable<T>
      Assert.That(() => { var value = ((IAnimatableProperty<float>)property).BaseValue; }, Throws.TypeOf<NotImplementedException>());
      Assert.AreEqual(123.4f, ((IAnimatableProperty<float>)property).AnimationValue);
    }


    [Test]
    public void NullValuesShouldBeAllowed()
    {
      var property = new DelegateAnimatableProperty<float>(null, null);

      // Getter, setter
      Assert.AreEqual(null, property.GetValue);
      Assert.AreEqual(null, property.SetValue);

      // IAnimatable
      Assert.IsFalse(((IAnimatableProperty)property).HasBaseValue);
      Assert.That(() => { var value = ((IAnimatableProperty)property).BaseValue; }, Throws.TypeOf<NotImplementedException>());
      Assert.IsFalse(((IAnimatableProperty)property).IsAnimated);
      Assert.AreEqual(null, ((IAnimatableProperty)property).AnimationValue);

      // IAnimatable<T>
      Assert.That(() => { var value = ((IAnimatableProperty<float>)property).BaseValue; }, Throws.TypeOf<NotImplementedException>());
      Assert.AreEqual(0.0f, ((IAnimatableProperty<float>)property).AnimationValue);

      // Should have no effect:
      ((IAnimatableProperty<float>)property).AnimationValue = 100.0f;
    }


    [Test]
    public void ShouldGetValue()
    {
      var testObject = new TestObject { Value = 123.4f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      Assert.AreEqual(123.4f, property.GetValue());
    }


    [Test]
    public void ShouldSetValue()
    {
      var testObject = new TestObject { Value = 123.4f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      property.SetValue(234.5f);
      Assert.AreEqual(234.5f, ((IAnimatableProperty<float>)property).AnimationValue);
      Assert.AreEqual(234.5f, property.GetValue());
    }


    [Test]
    public void ShouldSetAnimationValue()
    {
      var testObject = new TestObject { Value = 123.4f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      ((IAnimatableProperty<float>)property).AnimationValue = 234.5f;
      Assert.AreEqual(234.5f, ((IAnimatableProperty<float>)property).AnimationValue);
      Assert.AreEqual(234.5f, property.GetValue());
    }


    [Test]
    public void IsAnimatedShouldBeImplemented()
    {
      var testObject = new TestObject();
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      Assert.IsFalse(((IAnimatableProperty)property).IsAnimated);

      ((IAnimatableProperty)property).IsAnimated = true;
      Assert.IsTrue(((IAnimatableProperty)property).IsAnimated);
    }


    [Test]
    public void AnimateProperty()
    {
      var testObject = new TestObject { Value = 10.0f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(null, null);

      var animation = new SingleFromToByAnimation
      {
        From = 100,
        To = 200,
        Duration = TimeSpan.FromSeconds(1.0),
        IsAdditive = true,
      };

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(animation, property);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();

      property.GetValue = getter;
      property.SetValue = setter;

      manager.ApplyAnimations();

      Assert.AreEqual(150.0f, testObject.Value);
      Assert.IsTrue(((IAnimatableProperty)property).IsAnimated);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();

      Assert.AreEqual(200.0f, testObject.Value);
      Assert.IsTrue(((IAnimatableProperty)property).IsAnimated);

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(200.0f, testObject.Value);
      Assert.IsFalse(((IAnimatableProperty)property).IsAnimated);
    }
  }
}
