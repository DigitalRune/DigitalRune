using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationTransitionsTest
  {
    [Test]
    public void SnapshotAndReplace()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };
      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        IsAdditive = true,
      };

      var manager = new AnimationManager();

      var controller = manager.CreateController(animation, property);
      Assert.AreEqual(10.0f, property.Value);

      controller.Start(AnimationTransitions.SnapshotAndReplace());
      controller.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      // Changing the base value has no effect.
      property.Value = 20.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(110.0f, property.Value);

      // Start second animation using SnapshotAndReplace.
      controller = manager.CreateController(animation, property);
      Assert.AreEqual(110.0f, property.Value);

      controller.Start(AnimationTransitions.SnapshotAndReplace());
      controller.UpdateAndApply();
      Assert.AreEqual(210.0f, property.Value);
      
      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(20.0f, property.Value);
    }


    [Test]
    public void ReplaceAll()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };
      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        IsAdditive = true,
      };

      var manager = new AnimationManager();

      var controller = manager.CreateController(animation, property);
      Assert.AreEqual(10.0f, property.Value);

      controller.Start(AnimationTransitions.Replace());
      controller.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      // Changing the base value has no effect.
      property.Value = 20.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(120.0f, property.Value);

      // Start second animation using Replace.
      controller = manager.CreateController(animation, property);
      Assert.AreEqual(120.0f, property.Value);

      controller.Start(AnimationTransitions.Replace());
      controller.UpdateAndApply();
      Assert.AreEqual(120.0f, property.Value);

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(20.0f, property.Value);
    }


    [Test]
    public void ReplaceAllWithFadeIn()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Replace(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(100.0f, property.Value);

      // Changing the base value has no effect.
      property.Value = 150.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(175.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);

      // Start second animation using Replace.
      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 100.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Replace(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(200.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(AnimationState.Stopped, controllerA.State);
    }


    [Test]
    public void Replace()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };
      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        IsAdditive = true,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animation, property);
      Assert.AreEqual(10.0f, property.Value);

      controllerA.Start(AnimationTransitions.Replace());
      controllerA.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      // Changing the base value has no effect.
      property.Value = 20.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(120.0f, property.Value);

      // Start second animation using Replace.
      var controllerB = manager.CreateController(animation, property);
      Assert.AreEqual(120.0f, property.Value);

      controllerB.Start(AnimationTransitions.Replace(controllerA.AnimationInstance));
      controllerB.UpdateAndApply();
      Assert.AreEqual(120.0f, property.Value);

      controllerB.Stop();
      controllerB.UpdateAndApply();
      Assert.AreEqual(20.0f, property.Value);
    }


    [Test]
    public void ReplaceWithFadeIn()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Replace(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(100.0f, property.Value);

      // Changing the base value has no effect.
      property.Value = 150.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(175.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);

      // Start second animation using Replace.
      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 100.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Replace(controllerA.AnimationInstance, TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(200.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(AnimationState.Stopped, controllerA.State);
    }


    [Test]
    public void Compose()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Compose());
      controllerA.UpdateAndApply();
      Assert.AreEqual(200.0f, property.Value);

      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 10.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Compose());
      controllerB.UpdateAndApply();
      Assert.AreEqual(210.0f, property.Value);

      controllerA.Stop();
      controllerA.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      controllerB.Stop();
      controllerB.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
    }


    [Test]
    public void ComposeAfter()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Compose());
      controllerA.UpdateAndApply();
      Assert.AreEqual(200.0f, property.Value);

      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 10.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Compose(controllerB.AnimationInstance));
      controllerB.UpdateAndApply();
      Assert.AreEqual(210.0f, property.Value);

      controllerA.Stop();
      controllerA.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      controllerB.Stop();
      controllerB.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
    }


    [Test]
    public void ComposeWithFadeIn()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Compose(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);

      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 10.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Compose(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(200.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(205.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(210.0f, property.Value);

      controllerA.Stop();
      controllerA.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      controllerB.Stop();
      controllerB.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
    }


    [Test]
    public void ComposeAfterWithFadeIn()
    {
      var property = new AnimatableProperty<float> { Value = 100.0f };
      var animationA = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
      };

      var manager = new AnimationManager();

      var controllerA = manager.CreateController(animationA, property);
      Assert.AreEqual(100.0f, property.Value);

      controllerA.Start(AnimationTransitions.Compose(TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);

      var animationB = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 10.0f,
      };

      var controllerB = manager.CreateController(animationB, property);
      Assert.AreEqual(200.0f, property.Value);

      controllerB.Start(AnimationTransitions.Compose(controllerA.AnimationInstance, TimeSpan.FromSeconds(1.0)));
      Assert.AreEqual(200.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(205.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(210.0f, property.Value);

      controllerA.Stop();
      controllerA.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      controllerB.Stop();
      controllerB.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
    }


    [Test]
    public void FadeOut()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };
      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        IsAdditive = true,
      };

      var manager = new AnimationManager();

      var controller = manager.CreateController(animation, property);
      Assert.AreEqual(10.0f, property.Value);

      controller.Start(AnimationTransitions.Compose());
      controller.UpdateAndApply();
      Assert.AreEqual(110.0f, property.Value);

      // Changing the base value has no effect.
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(110.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(1.0));
      manager.ApplyAnimations();
      Assert.AreEqual(210.0f, property.Value);

      controller.Stop(TimeSpan.FromSeconds(1.0));
      Assert.AreEqual(210.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(110.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(10.0f, property.Value);
      Assert.AreEqual(AnimationState.Filling, controller.State);

      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.ApplyAnimations();
      Assert.AreEqual(10.0f, property.Value);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }
  }
}
