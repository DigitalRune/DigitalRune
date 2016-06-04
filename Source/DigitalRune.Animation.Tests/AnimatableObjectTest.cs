using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimatableObjectTest
  {
    [Test]
    public void ConstructorShouldThrowWhenArgumentIsNull()
    {
      Assert.That(() => new AnimatableObject(null), Throws.TypeOf<ArgumentNullException>());
    }


    [Test]
    public void ConstructorShouldThrowWhenArgumentIsEmpty()
    {
      Assert.That(() => new AnimatableObject(""), Throws.TypeOf<ArgumentException>());
    }


    [Test]
    public void NameShouldBeSet()
    {
      var animatableProperty = new AnimatableObject("Name of object");
      Assert.AreEqual("Name of object", animatableProperty.Name);
    }


    [Test]
    public void ShouldReturnAllAnimatableProperties()
    {
      var widthProperty = new AnimatableProperty<float>();
      var heightProperty = new AnimatableProperty<float>();
      var textProperty = new AnimatableProperty<string>();
      var animatable = new AnimatableObject("Object");
      animatable.Properties.Add("Width", widthProperty);
      animatable.Properties.Add("Height", heightProperty);
      animatable.Properties.Add("Text", textProperty);

      Assert.That(animatable.GetAnimatedProperties(), Has.Member(widthProperty));
      Assert.That(animatable.GetAnimatedProperties(), Has.Member(heightProperty));
      Assert.That(animatable.GetAnimatedProperties(), Has.Member(textProperty));
    }


    [Test]
    public void ShouldReturnAnimatableProperties()
    {
      var widthProperty = new AnimatableProperty<float>();
      var heightProperty = new AnimatableProperty<float>();
      var textProperty = new AnimatableProperty<string>();
      var animatable = new AnimatableObject("Object");
      animatable.Properties.Add("Width", widthProperty);
      animatable.Properties.Add("Height", heightProperty);
      animatable.Properties.Add("Text", textProperty);

      Assert.That(animatable.Properties.Values, Has.Member(widthProperty));
      Assert.That(animatable.Properties.Values, Has.Member(heightProperty));
      Assert.That(animatable.Properties.Values, Has.Member(textProperty));

      Assert.AreEqual(widthProperty, animatable.GetAnimatableProperty<float>("Width"));
      Assert.AreEqual(heightProperty, animatable.GetAnimatableProperty<float>("Height"));
      Assert.AreEqual(textProperty, animatable.GetAnimatableProperty<string>("Text"));
    }


    [Test]
    public void ShouldReturnNullWhenNameIsNotFound()
    {
      var widthProperty = new AnimatableProperty<float>();
      var animatable = new AnimatableObject("Object");
      animatable.Properties.Add("Width", widthProperty);

      Assert.IsNull(animatable.GetAnimatableProperty<float>("WrongName"));
    }


    [Test]
    public void ShouldReturnNullWhenTypeDoesNotMatch()
    {
      var widthProperty = new AnimatableProperty<float>();
      var animatable = new AnimatableObject("Object");
      animatable.Properties.Add("Width", widthProperty);

      Assert.IsNull(animatable.GetAnimatableProperty<string>("Width"));
    }
  }
}