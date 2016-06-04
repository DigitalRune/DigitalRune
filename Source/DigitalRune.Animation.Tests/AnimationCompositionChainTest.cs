using System;
using System.Collections.Generic;
using DigitalRune.Animation.Traits;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class AnimationCompositionChainTest
  {
    [Test]
    public void ConstructorShouldThrowWhenNull()
    {
      Assert.That(() => { AnimationCompositionChain<float>.Create(null, SingleTraits.Instance); }, Throws.TypeOf<ArgumentNullException>());
      Assert.That(() => { AnimationCompositionChain<float>.Create(new AnimatableProperty<float>(), null); }, Throws.TypeOf<ArgumentNullException>());      
    }


    [Test]
    public void GetEnumerator()
    {
      var property = new AnimatableProperty<float>();
      var compositionChain = AnimationCompositionChain<float>.Create(property, SingleTraits.Instance);

      foreach (var animationInstance in compositionChain)
      { }

      Assert.That(() => { compositionChain.GetEnumerator(); }, Throws.Nothing);
      Assert.That(() => { ((IEnumerable<AnimationInstance>)compositionChain).GetEnumerator(); }, Throws.TypeOf<NotImplementedException>());
    }


    [Test]
    public void ShouldImplementIList()
    {
      var property = new AnimatableProperty<float>();
      var compositionChain = AnimationCompositionChain<float>.Create(property, SingleTraits.Instance);
      var list = (IList<AnimationInstance>)compositionChain;
      var animationInstance0 = AnimationInstance<float>.Create(new SingleFromToByAnimation());
      var animationInstance1 = AnimationInstance<float>.Create(new SingleFromToByAnimation());
      var animationInstance2 = AnimationInstance<float>.Create(new SingleFromToByAnimation());
      var wrongInstance = AnimationInstance.Create(new TimelineGroup());

      // The enumerator is not implemented (to prevent garbage).
      Assert.That(() => { list.GetEnumerator(); }, Throws.TypeOf<NotImplementedException>());

      // Add
      Assert.That(() => list.Add(wrongInstance), Throws.ArgumentException);
      list.Add(animationInstance0);

      // Contains
      Assert.IsTrue(list.Contains(animationInstance0));
      Assert.IsFalse(list.Contains(animationInstance1));
      Assert.IsFalse(list.Contains(wrongInstance));

      // IndexOf
      Assert.AreEqual(0, list.IndexOf(animationInstance0));
      Assert.AreEqual(-1, list.IndexOf(animationInstance1));
      Assert.AreEqual(-1, list.IndexOf(wrongInstance));

      // IsReadOnly
      Assert.IsFalse(list.IsReadOnly);

      // Insert
      list.Insert(1, animationInstance1);
      Assert.That(() => list.Insert(0, wrongInstance), Throws.ArgumentException);

      // Indexer
      Assert.AreEqual(animationInstance1, list[1]);
      list[0] = animationInstance2;
      Assert.That(() => list[0] = wrongInstance, Throws.ArgumentException);

      // CopyTo
      AnimationInstance[] array = new AnimationInstance[2];
      list.CopyTo(array, 0);
      Assert.AreEqual(animationInstance2, array[0]);
      Assert.AreEqual(animationInstance1, array[1]);

      // Remove
      Assert.IsTrue(list.Remove(animationInstance2));
      Assert.AreEqual(1, list.Count);
      Assert.IsFalse(list.Remove(wrongInstance));

      // RemoveAt
      list.RemoveAt(0);
      Assert.AreEqual(0, list.Count);
    }
  }
}
