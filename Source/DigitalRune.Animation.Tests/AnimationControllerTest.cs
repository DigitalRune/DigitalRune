using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationControllerTest
  {
    [Test]
    public void CreateController()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.CreateController(animation, property);

      Assert.AreEqual(manager, controller.AnimationService);
      Assert.AreEqual(animation, controller.AnimationInstance.Animation);
      Assert.IsFalse(controller.AutoRecycleEnabled);
      Assert.IsTrue(controller.IsValid);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.IsFalse(controller.Time.HasValue);
    }


    [Test]
    public void StartAnimation()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.StartAnimation(animation, property);

      Assert.AreEqual(manager, controller.AnimationService);
      Assert.AreEqual(animation, controller.AnimationInstance.Animation);
      Assert.IsFalse(controller.AutoRecycleEnabled);
      Assert.IsTrue(controller.IsValid);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);
    }


    [Test]
    public void SetAnimationTime()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.StartAnimation(animation, property);

      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);
      controller.Time = TimeSpan.FromSeconds(0.5);
      Assert.AreEqual(TimeSpan.FromSeconds(0.5), controller.Time);
      Assert.AreEqual(TimeSpan.FromSeconds(0.5), controller.AnimationInstance.Time);
    }


    [Test]
    public void RecycleController()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.CreateController(animation, property);

      controller.Recycle();
      Assert.AreEqual(manager, controller.AnimationService);
      Assert.AreEqual(null, controller.AnimationInstance);
      Assert.IsFalse(controller.IsValid);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.IsFalse(controller.Time.HasValue);
    }


    [Test]    
    public void AutoRecycleEnabled()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.CreateController(animation, property);

      controller.Start();
      controller.Stop();
      Assert.IsTrue(controller.IsValid);

      controller.AutoRecycleEnabled = true;

      controller.Start();
      controller.Stop();
      Assert.IsFalse(controller.IsValid);      
    }


    [Test]
    public void AutoRecycle()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.CreateController(animation, property);

      controller.Start();
      controller.Stop();
      Assert.IsTrue(controller.IsValid);

      controller.AutoRecycle();

      controller.Start();
      controller.Stop();
      Assert.IsFalse(controller.IsValid);
    }


    [Test]
    public void ControlAnimation()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation 
      { 
        From = 100.0f, 
        To = 200.0f,
      };
      var controller = manager.CreateController(animation, property);

      Assert.AreEqual(0.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.IsFalse(controller.Time.HasValue);

      controller.Pause();
      Assert.AreEqual(0.0f, property.Value);
      Assert.IsTrue(controller.IsPaused);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.IsFalse(controller.Time.HasValue);

      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsTrue(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsTrue(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);

      controller.Resume();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.5), controller.Time);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(1.0), controller.Time);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(200.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Filling, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(1.5), controller.Time);

      Assert.That(() => controller.Start(), Throws.TypeOf<AnimationException>());

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(0.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
      Assert.IsFalse(controller.Time.HasValue);

      // Restart
      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.0), controller.Time);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);
      Assert.IsFalse(controller.IsPaused);
      Assert.AreEqual(AnimationState.Playing, controller.State);
      Assert.AreEqual(TimeSpan.FromSeconds(0.5), controller.Time);
    }


    [Test]
    public void ShouldDoNothingIfInvalid()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var controller = manager.CreateController(animation, property);

      controller.Recycle();
      Assert.IsFalse(controller.IsValid);

      // The following has no effect.
      controller.AutoRecycleEnabled = false;
      controller.Time = TimeSpan.Zero;
      controller.AutoRecycle();
      controller.Recycle();
      controller.Pause();
      controller.Resume();
      controller.Stop();
      controller.Stop(TimeSpan.Zero);
      controller.Stop(TimeSpan.FromSeconds(1.0));

      // Only Start and UpdateAndApply should throw an exceptions.
      Assert.That(() => controller.Start(), Throws.TypeOf<AnimationException>());
      Assert.That(() => controller.UpdateAndApply(), Throws.TypeOf<AnimationException>());
    }


    [Test]
    public void StopAnimationImmediately()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
      };
      var controller = manager.StartAnimation(animation, property);
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);

      controller.Stop(TimeSpan.Zero);
      controller.UpdateAndApply();
      Assert.AreEqual(0.0f, property.Value);
    }


    [Test]
    public void FadeOutAnimation()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 100.0f,
      };
      var controller = manager.StartAnimation(animation, property);
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(AnimationState.Playing, controller.State);

      controller.Stop(TimeSpan.FromSeconds(1.0));
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(AnimationState.Playing, controller.State);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(50.0f, property.Value);
      Assert.AreEqual(AnimationState.Filling, controller.State);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(0.0f, property.Value);
      Assert.AreEqual(AnimationState.Filling, controller.State);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(0.0f, property.Value);
      Assert.AreEqual(AnimationState.Stopped, controller.State);
    }


    [Test]
    public void Speed()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.FromSeconds(1),
        From = 100.0f,
        To = 200.0f,
      };

      var controller = manager.StartAnimation(animation, property);
      controller.AutoRecycle();
      controller.UpdateAndApply();
      Assert.AreEqual(1.0f, controller.Speed);

      // Normal speed.
      controller.Speed = 1.0f;
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(125.0f, property.Value);

      // Double speed.
      controller.Speed = 2.0f;
      manager.Update(TimeSpan.FromSeconds(0.25));
      manager.ApplyAnimations();
      Assert.AreEqual(175.0f, property.Value);

      // Half speed.
      controller.Speed = 0.5f;
      manager.Update(TimeSpan.FromSeconds(0.2));
      manager.ApplyAnimations();
      Assert.AreEqual(185.0f, property.Value);

      // Negative speed.
      controller.Speed = -0.5f;
      manager.Update(TimeSpan.FromSeconds(0.2));
      manager.ApplyAnimations();
      Assert.AreEqual(175.0f, property.Value);

      controller.Stop();
      Assert.IsNaN(controller.Speed);
    }


    [Test]
    public void AnimationWeight()
    {
      var manager = new AnimationManager();
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.FromSeconds(1.0),
        From = 100.0f,
        To = 200.0f,
      };

      var controller = manager.StartAnimation(animation, property);
      controller.AutoRecycle();
      controller.UpdateAndApply();
      Assert.AreEqual(1.0f, controller.Weight);
      Assert.AreEqual(100.0f, property.Value);

      controller.Weight = 0.5f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(50.0f, property.Value);

      controller.Stop();
      Assert.IsNaN(controller.Weight);
    }
  }
}
