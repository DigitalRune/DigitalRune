using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationManagerTest
  {
    [Test]
    public void CheckDefaultValues()
    {
      var manager = new AnimationManager();
      Assert.AreEqual(Environment.ProcessorCount > 1, manager.EnableMultithreading);
    }


    [Test]
    public void UpdateWithZeroTimeShouldBeAllowed()
    {
      var manager = new AnimationManager();
      manager.Update(TimeSpan.Zero);
    }


    [Test]
    public void ShouldDoNothingWhenTimeIsNegative()
    {
      var manager = new AnimationManager();
      manager.Update(TimeSpan.FromSeconds(-1));
    }


    [Test]
    public void ShouldDoNothingWhenEmpty()
    {
      var manager = new AnimationManager();
      manager.Update(new TimeSpan(333333));
      manager.Update(new TimeSpan(333333));
      manager.Update(new TimeSpan(333333));
    }


    [Test]
    public void InvalidParameters()
    {
      var objectA = new AnimatableObject("ObjectA");
      var objectB = new AnimatableObject("ObjectA");
      var property = new AnimatableProperty<float>();
      var animation = new SingleFromToByAnimation();
      var objects = new[] { objectA, objectB };

      var manager = new AnimationManager();

      // Should throw exception.
      Assert.That(() => { manager.IsAnimated((IAnimatableObject)null); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.IsAnimated((IAnimatableProperty)null); }, Throws.TypeOf<ArgumentNullException>());

      // Should throw exception.
      Assert.That(() => { manager.CreateController(null, objects); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.CreateController(animation, (IEnumerable<IAnimatableObject>)null); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.CreateController(null, objectA); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.CreateController(animation, (IAnimatableObject)null); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.CreateController(null, property); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.CreateController(animation, (IAnimatableProperty)null); }, Throws.TypeOf<ArgumentNullException>());

      // Should throw exception.
      Assert.That(() => { manager.StartAnimation(null, objects); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.StartAnimation(animation, (IEnumerable<IAnimatableObject>)null); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.StartAnimation(null, objectA); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.StartAnimation(animation, (IAnimatableObject)null); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.StartAnimation(null, property); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { manager.StartAnimation(animation, (IAnimatableProperty)null); }, Throws.TypeOf<ArgumentNullException>());

      // Should not throw exception.
      Assert.That(() => manager.StopAnimation((IEnumerable<IAnimatableObject>)null), Throws.Nothing);
      Assert.That(() => manager.StopAnimation((IAnimatableObject)null), Throws.Nothing);
      Assert.That(() => manager.StopAnimation((IAnimatableProperty)null), Throws.Nothing);
    }


    [Test]
    public void ApplyAnimationWithDurationZero()
    {
      var property = new AnimatableProperty<float> { Value = 123.4f };
      var manager = new AnimationManager();

      var animation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 234.5f,
        FillBehavior = FillBehavior.Stop,        
      };
      var controller = manager.StartAnimation(animation, property);
      controller.UpdateAndApply();
      Assert.AreEqual(234.5f, property.Value);

      manager.Update(new TimeSpan(166666));
      Assert.AreEqual(234.5f, property.Value);

      manager.ApplyAnimations();
      Assert.AreEqual(123.4f, property.Value);
    }


    [Test]
    public void StartStopAnimationWithinOneFrame()
    {
      var property = new AnimatableProperty<float> { Value = 123.4f };
      var manager = new AnimationManager();

      var animation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 234.5f,
        FillBehavior = FillBehavior.Stop,
      };

      var controller = manager.CreateController(animation, property);
      Assert.AreEqual(123.4f, property.Value);

      // Start
      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(234.5f, property.Value);

      // Stop
      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(123.4f, property.Value);
    }


    [Test]
    public void StartStopAnimationsWithinOneFrame0()
    {
      var property = new AnimatableProperty<float> { Value = 123.4f };
      var manager = new AnimationManager();

      // Start base animation.
      var animation0 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 234.5f,
        FillBehavior = FillBehavior.Stop,
      };
      var controller0 = manager.StartAnimation(animation0, property);
      controller0.UpdateAndApply();
      Assert.AreEqual(234.5f, property.Value);

      // Start additive animation.
      var animation1 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 10.0f,
        IsAdditive = true,
        FillBehavior = FillBehavior.Stop,
      };
      var controller1 = manager.StartAnimation(animation1, property, AnimationTransitions.Compose());
      controller1.UpdateAndApply();
      Assert.AreEqual(234.5f + 10.0f, property.Value);

      // Stop additive animation.
      controller1.Stop();
      controller1.UpdateAndApply();
      Assert.AreEqual(234.5f, property.Value);

      // Stop base animation.
      controller0.Stop();
      controller0.UpdateAndApply();
      Assert.AreEqual(123.4f, property.Value);
    }


    [Test]
    public void StartStopAnimationsWithinOneFrame1()
    {
      var property = new AnimatableProperty<float> { Value = 123.4f };
      var manager = new AnimationManager();

      // Start base animation.
      var animation0 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 234.5f,
        FillBehavior = FillBehavior.Stop,
      };
      var controller0 = manager.StartAnimation(animation0, property);
      controller0.UpdateAndApply();
      Assert.AreEqual(234.5f, property.Value);

      // Start additive animation.
      var animation1 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 10.0f,
        IsAdditive = true,
        FillBehavior = FillBehavior.Stop,
      };
      var controller1 = manager.StartAnimation(animation1, property, AnimationTransitions.Compose());
      controller1.UpdateAndApply();
      Assert.AreEqual(234.5f + 10.0f, property.Value);

      // Stop base animation.
      controller0.Stop();
      controller0.UpdateAndApply();
      Assert.AreEqual(123.4f + 10.0f, property.Value);

      // Stop additive animation.
      controller1.Stop();
      controller1.UpdateAndApply();
      Assert.AreEqual(123.4f, property.Value);
    }


    [Test]
    public void AdditiveAnimation()
    {
      var property = new AnimatableProperty<float> { Value = 123.4f };
      var manager = new AnimationManager();

      // Start base animation.
      var animation0 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.FromSeconds(1.0),
        To = 234.5f,
        FillBehavior = FillBehavior.Stop,
      };
      var controller0 = manager.StartAnimation(animation0, property);
      Assert.AreEqual(123.4f, property.Value);
      
      // Start additive animation.
      var animation1 = new SingleFromToByAnimation
      {
        Duration = TimeSpan.FromSeconds(1.0),
        From = 0.0f,
        To = 10.0f,
        IsAdditive = true,
        FillBehavior = FillBehavior.Hold,
      };
      var controller1 = manager.StartAnimation(animation1, property, AnimationTransitions.Compose());
      Assert.AreEqual(123.4f, property.Value);

      manager.Update(TimeSpan.FromSeconds(1.0));
      Assert.AreEqual(123.4f, property.Value);

      manager.ApplyAnimations();
      Assert.AreEqual(234.5f + 10.0f, property.Value);

      manager.Update(new TimeSpan(166666));
      Assert.AreEqual(234.5f + 10.0f, property.Value);

      manager.ApplyAnimations();
      Assert.AreEqual(123.4f + 10.0f, property.Value);

      // Stop additive animation.
      controller1.Stop();
      controller1.UpdateAndApply();
      Assert.AreEqual(123.4f, property.Value);      
    }


    [Test]
    public void ShouldRemoveAnimationsIfTargetsAreGarbageCollected()
    {
      var obj = new AnimatableObject("TestObject");
      var property = new AnimatableProperty<float>();
      obj.Properties.Add("Value", property);

      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        TargetProperty = "Value",
      };

      var manager = new AnimationManager();
      var controller = manager.StartAnimation(animation, obj);
      controller.AutoRecycle();
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);

      // Garbage-collect target object.
      obj = null;
      property = null;
      GC.Collect();

      // Controller should be still valid, because AnimationManager.Update() needs 
      // to be called first.
      Assert.IsTrue(controller.IsValid);

      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.ApplyAnimations();

      // Animation instance should now be recycled.
      Assert.IsFalse(controller.IsValid);
    }


    [Test]
    public void ShouldRemoveAnimationsIfInactive()
    {
      var obj = new AnimatableObject("TestObject");
      var property = new AnimatableProperty<float>();
      obj.Properties.Add("Value", property);

      var animation = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        TargetProperty = "Value",
      };

      var manager = new AnimationManager();
      var controllerA = manager.StartAnimation(animation, obj);
      controllerA.AutoRecycle();
      controllerA.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsTrue(controllerA.IsValid);

      manager.Update(TimeSpan.FromSeconds(0.5));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);
      Assert.IsTrue(controllerA.IsValid);

      // Replace animation instance with new instance.
      var controllerB = manager.StartAnimation(animation, obj);
      controllerB.AutoRecycle();
      controllerB.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.IsTrue(controllerA.IsValid);
      Assert.IsTrue(controllerB.IsValid);

      // controllerA should be removed automatically. 
      // (Note: Cleanup is done incrementally, not every frame. 
      // It is okay if it takes a few updates.)
      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.Update(TimeSpan.FromSeconds(0.1));
      manager.ApplyAnimations();
      Assert.AreEqual(150.0f, property.Value);
      Assert.IsFalse(controllerA.IsValid);
      Assert.IsTrue(controllerB.IsValid);   
    }


    [Test]
    public void AnimationShapshots()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };

      var manager = new AnimationManager();
      var byAnimation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 25.0f,
      };

      var byController = manager.CreateController(byAnimation, property);
      Assert.AreEqual(10.0f, property.Value);

      // Without snapshot.
      byController.Start(AnimationTransitions.Replace());
      byController.UpdateAndApply();
      Assert.AreEqual(35.0f, property.Value);

      property.Value = 100.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(125.0f, property.Value);

      byController.Stop();
      byController.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);

      // With snapshot.
      property.Value = 10.0f;
      byController.Start(AnimationTransitions.SnapshotAndReplace());
      byController.UpdateAndApply();
      Assert.AreEqual(35.0f, property.Value);

      property.Value = 100.0f;
      manager.Update(TimeSpan.Zero);
      manager.ApplyAnimations();
      Assert.AreEqual(35.0f, property.Value);

      // Create another snapshot.
      var additiveAnimation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        To = 200.0f,
        IsAdditive = true,
      };
      var additiveController = manager.CreateController(additiveAnimation, property);
      additiveController.Start(AnimationTransitions.SnapshotAndReplace());
      additiveController.UpdateAndApply();
      Assert.AreEqual(235.0f, property.Value);

      byController.Stop();
      Assert.AreEqual(235.0f, property.Value);

      byController.Start(AnimationTransitions.Replace());
      byController.UpdateAndApply();
      Assert.AreEqual(60.0f, property.Value); // 35.0f (Snapshot) + 25.0f (By Animation)
    }


    private class TestObject
    {
      public float Value { get; set; }
    }


    [Test]
    public void SnapshotFromDelegateAnimatableProperty()
    {
      var testObject = new TestObject { Value = 123.4f };
      Func<float> getter = () => testObject.Value;
      Action<float> setter = f => { testObject.Value = f; };
      var property = new DelegateAnimatableProperty<float>(getter, setter);

      var manager = new AnimationManager();
      var byAnimation = new SingleFromToByAnimation
      {
        Duration = TimeSpan.Zero,
        By = 25.0f,
      };

      var controller = manager.StartAnimation(byAnimation, property, AnimationTransitions.SnapshotAndReplace());
      controller.UpdateAndApply();

      // The DelegateAnimatableProperty<T> does not provide a base value.
      // --> No snapshot is created.
      Assert.AreEqual(25.0f, testObject.Value); // 0.0f (SingleTraits.Identity) + 25.0f (By Animation)
    }


    [Test]
    public void AnimateProperty()
    {
      var property = new AnimatableProperty<float> { Value = 10.0f };

      var animationA = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        TargetObject = "ObjectA",     // Should be ignored.
        TargetProperty = "PropertyA", // Should be ignored.
      };
      var animationB = new SingleFromToByAnimation
      {
        From = 200.0f,
        To = 300.0f,
        TargetObject = "ObjectB",     // Should be ignored.
        TargetProperty = "PropertyB", // Should be ignored.
      };
      var animationGroup = new TimelineGroup();
      animationGroup.Add(animationA);
      animationGroup.Add(animationB);
      
      var manager = new AnimationManager();

      // Should assign both animations to 'property'.
      var controller = manager.CreateController(animationGroup, property);
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[0]).Property);
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.IsFalse(manager.IsAnimated(property));

      // When started then animationB (last in the composition chain) should be active.
      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(200.0f, property.Value);
      Assert.IsTrue(manager.IsAnimated(property));

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(10.0f, property.Value);
      Assert.IsFalse(manager.IsAnimated(property));

      // Same test for AnimationManager.StartAnimation()
      controller = manager.StartAnimation(animationGroup, property);
      controller.UpdateAndApply();
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[0]).Property);
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.AreEqual(200.0f, property.Value);
      Assert.IsTrue(manager.IsAnimated(property));

      manager.StopAnimation(property);
      manager.UpdateAndApplyAnimation(property);
      Assert.AreEqual(10.0f, property.Value);
      Assert.IsFalse(manager.IsAnimated(property));
    }


    [Test]
    public void AnimateObject()
    {
      var obj = new AnimatableObject("TestObject");
      var property = new AnimatableProperty<float> { Value = 10.0f };
      obj.Properties.Add("Value", property);
      var property2 = new AnimatableProperty<float> { Value = 20.0f };
      obj.Properties.Add("Value2", property2);

      var animationA = new SingleFromToByAnimation
      {
        From = 100.0f,
        To = 200.0f,
        TargetObject = "ObjectA",     // Should be ignored.
        TargetProperty = "Value",
      };
      var animationB = new SingleFromToByAnimation
      {
        From = 200.0f,
        To = 300.0f,
        TargetObject = "ObjectB",     // Should be ignored.
        TargetProperty = "",
      };
      var animationGroup = new TimelineGroup();
      animationGroup.Add(animationA);
      animationGroup.Add(animationB);

      var manager = new AnimationManager();

      // Should assign animationA to 'obj'.
      var controller = manager.CreateController(animationGroup, obj);
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[0]).Property);
      Assert.AreEqual(null, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.IsFalse(manager.IsAnimated(property));
      Assert.IsFalse(manager.IsAnimated(property2));
      Assert.IsFalse(manager.IsAnimated(obj));

      // When started then animationA should be active.
      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(20.0f, property2.Value);
      Assert.IsTrue(manager.IsAnimated(obj));
      Assert.IsTrue(manager.IsAnimated(property));
      Assert.IsFalse(manager.IsAnimated(property2));

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(10.0f, property.Value);
      Assert.AreEqual(20.0f, property2.Value);
      Assert.IsFalse(manager.IsAnimated(obj));
      Assert.IsFalse(manager.IsAnimated(property));
      Assert.IsFalse(manager.IsAnimated(property2));

      // Same test for AnimationManager.StartAnimation()
      controller = manager.StartAnimation(animationGroup, obj);
      controller.UpdateAndApply();
      Assert.AreEqual(property, ((AnimationInstance<float>)controller.AnimationInstance.Children[0]).Property);
      Assert.AreEqual(null, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.AreEqual(100.0f, property.Value);
      Assert.AreEqual(20.0f, property2.Value);
      Assert.IsTrue(manager.IsAnimated(obj));
      Assert.IsTrue(manager.IsAnimated(property));
      Assert.IsFalse(manager.IsAnimated(property2));

      manager.StopAnimation(obj);
      manager.UpdateAndApplyAnimation(obj);
      Assert.AreEqual(10.0f, property.Value);
      Assert.AreEqual(20.0f, property2.Value);
      Assert.IsFalse(manager.IsAnimated(obj));
      Assert.IsFalse(manager.IsAnimated(property));
      Assert.IsFalse(manager.IsAnimated(property2));
    }


    [Test]
    public void AnimateObjects()
    {
      var objectA = new AnimatableObject("ObjectA");
      var propertyA1 = new AnimatableProperty<float> { Value = 10.0f };
      objectA.Properties.Add("Value", propertyA1);
      var propertyA2 = new AnimatableProperty<float> { Value = 20.0f };
      objectA.Properties.Add("Value2", propertyA2);

      var objectB = new AnimatableObject("ObjectB");
      var propertyB = new AnimatableProperty<float> { Value = 30.0f };
      objectB.Properties.Add("Value", propertyB);

      var objectC = new AnimatableObject("ObjectC");
      var propertyC = new AnimatableProperty<float> { Value = 40.0f };
      objectC.Properties.Add("Value", propertyC);

      var animationA1 = new SingleFromToByAnimation // Should be assigned to ObjectA
      {
        From = 100.0f,
        To = 200.0f,
        TargetObject = "ObjectXyz", // Ignored because ObjectA is selected by animationGroup1.
        TargetProperty = "Value",   // Required.
      };
      var animationA2 = new SingleFromToByAnimation // Should be assigned to ObjectA
      {
        From = 200.0f,
        To = 300.0f,
        TargetObject = "ObjectB",   // Ignored because ObjectA is selected by animationGroup1.
        TargetProperty = "Value",   // Required.
      };
      var animationA3 = new Vector3FFromToByAnimation // Ignored because of incompatible type.
      {
        From = new Vector3F(300.0f),
        To = new Vector3F(400.0f),
        TargetObject = "ObjectA",
        TargetProperty = "Value",
      };
      var animationA4 = new SingleFromToByAnimation   // Ignored because TargetProperty is not set.
      {
        From = 400.0f,
        To = 500.0f,
        TargetObject = "",
        TargetProperty = "",
      };
      var animationGroupA = new TimelineGroup { TargetObject = "ObjectA" };
      animationGroupA.Add(animationA1);
      animationGroupA.Add(animationA2);
      animationGroupA.Add(animationA3);
      animationGroupA.Add(animationA4);

      var animationB1 = new SingleFromToByAnimation // Should be assigned to ObjectB
      {
        From = 100.0f,
        To = 200.0f,
        TargetObject = "ObjectB",
        TargetProperty = "Value",
      };
      var animationA5 = new SingleFromToByAnimation
      {
        From = 600.0f,
        To = 700.0f,
        TargetObject = "",
        TargetProperty = "Value",
      };

      var animationGroupRoot = new TimelineGroup();
      animationGroupRoot.Add(animationGroupA);
      animationGroupRoot.Add(animationB1);
      animationGroupRoot.Add(animationA5);

      var manager = new AnimationManager();

      // CreateController()
      var controller = manager.CreateController(animationGroupRoot, new[] { objectA, objectB, objectC });
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[0]).Property);
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[1]).Property);
      Assert.AreEqual(null, ((AnimationInstance<Vector3F>)controller.AnimationInstance.Children[0].Children[2]).Property);
      Assert.AreEqual(null, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[3]).Property);
      Assert.AreEqual(propertyB, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[2]).Property);
      Assert.AreEqual(10.0f, propertyA1.Value);
      Assert.AreEqual(20.0f, propertyA2.Value);
      Assert.AreEqual(30.0f, propertyB.Value);
      Assert.AreEqual(40.0f, propertyC.Value);
      Assert.IsFalse(manager.IsAnimated(objectA));
      Assert.IsFalse(manager.IsAnimated(propertyA1));
      Assert.IsFalse(manager.IsAnimated(propertyA2));
      Assert.IsFalse(manager.IsAnimated(objectB));
      Assert.IsFalse(manager.IsAnimated(propertyB));
      Assert.IsFalse(manager.IsAnimated(objectC));
      Assert.IsFalse(manager.IsAnimated(propertyC));

      controller.Start();
      controller.UpdateAndApply();
      Assert.AreEqual(600.0f, propertyA1.Value);
      Assert.AreEqual(20.0f, propertyA2.Value);
      Assert.AreEqual(100.0f, propertyB.Value);
      Assert.AreEqual(40.0f, propertyC.Value);
      Assert.IsTrue(manager.IsAnimated(objectA));
      Assert.IsTrue(manager.IsAnimated(propertyA1));
      Assert.IsFalse(manager.IsAnimated(propertyA2));
      Assert.IsTrue(manager.IsAnimated(objectB));
      Assert.IsTrue(manager.IsAnimated(propertyB));
      Assert.IsFalse(manager.IsAnimated(objectC));
      Assert.IsFalse(manager.IsAnimated(propertyC));

      controller.Stop();
      controller.UpdateAndApply();
      Assert.AreEqual(10.0f, propertyA1.Value);
      Assert.AreEqual(20.0f, propertyA2.Value);
      Assert.AreEqual(30.0f, propertyB.Value);
      Assert.AreEqual(40.0f, propertyC.Value);
      Assert.IsFalse(manager.IsAnimated(objectA));
      Assert.IsFalse(manager.IsAnimated(propertyA1));
      Assert.IsFalse(manager.IsAnimated(propertyA2));
      Assert.IsFalse(manager.IsAnimated(objectB));
      Assert.IsFalse(manager.IsAnimated(propertyB));
      Assert.IsFalse(manager.IsAnimated(objectC));
      Assert.IsFalse(manager.IsAnimated(propertyC));

      // StartAnimation()
      controller = manager.StartAnimation(animationGroupRoot, new[] { objectA, objectB, objectC });
      controller.UpdateAndApply();
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[0]).Property);
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[1]).Property);
      Assert.AreEqual(null, ((AnimationInstance<Vector3F>)controller.AnimationInstance.Children[0].Children[2]).Property);
      Assert.AreEqual(null, ((AnimationInstance<float>)controller.AnimationInstance.Children[0].Children[3]).Property);
      Assert.AreEqual(propertyB, ((AnimationInstance<float>)controller.AnimationInstance.Children[1]).Property);
      Assert.AreEqual(propertyA1, ((AnimationInstance<float>)controller.AnimationInstance.Children[2]).Property);
      Assert.AreEqual(600.0f, propertyA1.Value);
      Assert.AreEqual(20.0f, propertyA2.Value);
      Assert.AreEqual(100.0f, propertyB.Value);
      Assert.AreEqual(40.0f, propertyC.Value);
      Assert.IsTrue(manager.IsAnimated(objectA));
      Assert.IsTrue(manager.IsAnimated(propertyA1));
      Assert.IsFalse(manager.IsAnimated(propertyA2));
      Assert.IsTrue(manager.IsAnimated(objectB));
      Assert.IsTrue(manager.IsAnimated(propertyB));
      Assert.IsFalse(manager.IsAnimated(objectC));
      Assert.IsFalse(manager.IsAnimated(propertyC));

      manager.StopAnimation(new[] { objectA, objectB, objectC });
      manager.UpdateAndApplyAnimation(new[] { objectA, objectB, objectC });
      Assert.AreEqual(10.0f, propertyA1.Value);
      Assert.AreEqual(20.0f, propertyA2.Value);
      Assert.AreEqual(30.0f, propertyB.Value);
      Assert.AreEqual(40.0f, propertyC.Value);
      Assert.IsFalse(manager.IsAnimated(objectA));
      Assert.IsFalse(manager.IsAnimated(propertyA1));
      Assert.IsFalse(manager.IsAnimated(propertyA2));
      Assert.IsFalse(manager.IsAnimated(objectB));
      Assert.IsFalse(manager.IsAnimated(propertyB));
      Assert.IsFalse(manager.IsAnimated(objectC));
      Assert.IsFalse(manager.IsAnimated(propertyC));
    }
  }
}
