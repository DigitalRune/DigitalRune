using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Animation.Traits.Tests
{
  [TestFixture]
  public class SingleTraitsTest
  {
    [Test]
    public void IdentityTest()
    {
      var traits = SingleTraits.Instance;
      var value = 123.45f;
      Assert.AreEqual(value, traits.Add(value, traits.Identity()));
      Assert.AreEqual(value, traits.Add(traits.Identity(), value));
    }


    [Test]
    public void MultiplyTest()
    {
      var traits = SingleTraits.Instance;
      var value = -123.45f;
      Assert.IsTrue(Numeric.AreEqual(0, traits.Multiply(value, 0)));
      Assert.IsTrue(Numeric.AreEqual(value, traits.Multiply(value, 1)));
      Assert.IsTrue(Numeric.AreEqual(value + value, traits.Multiply(value, 2)));
      Assert.IsTrue(Numeric.AreEqual(value + value + value, traits.Multiply(value, 3)));
      Assert.IsTrue(Numeric.AreEqual(-value, traits.Multiply(value, -1)));
      Assert.IsTrue(Numeric.AreEqual(-value - value, traits.Multiply(value, -2)));
      Assert.IsTrue(Numeric.AreEqual(-value - value - value, traits.Multiply(value, -3)));
    }


    [Test]
    public void FromByTest()
    {
      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
      // the start value.

      var traits = SingleTraits.Instance;
      var from = -123.45f;
      var by = 98.76f;

      var to = traits.Add(from, by);
      Assert.IsTrue(Numeric.AreEqual(by + from, to));

      Assert.IsTrue(Numeric.AreEqual(from, traits.Add(to, traits.Inverse(by))));
      Assert.IsTrue(Numeric.AreEqual(by, traits.Add(traits.Inverse(from), to)));
    }


    [Test]
    public void CycleOffsetTest()
    {
      // IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
      // each iteration.

      var traits = SingleTraits.Instance;
      var first = 456.78f;    // Animation value of first key frame.
      var last = 321.45f;     // Animation value of last key frame.
      var cycleOffset = traits.Add(traits.Inverse(first), last);

      // Cycle offset should be the difference between last and first key frame.
      Assert.IsTrue(Numeric.AreEqual(last, traits.Add(first, cycleOffset)));
      Assert.IsTrue(Numeric.AreEqual(last, cycleOffset + first));

      // Check multiple cycles (post-loop).
      Assert.IsTrue(Numeric.AreEqual(last, traits.Add(first, traits.Multiply(cycleOffset, 1))));
      Assert.IsTrue(Numeric.AreEqual(cycleOffset + cycleOffset + last, traits.Add(first, traits.Multiply(cycleOffset, 3))));

      // Check multiple cycles (pre-loop).
      Assert.IsTrue(Numeric.AreEqual(first, traits.Add(last, traits.Multiply(cycleOffset, -1))));
      Assert.IsTrue(Numeric.AreEqual(first - cycleOffset - cycleOffset, traits.Add(last, traits.Multiply(cycleOffset, -3))));
    }


    [Test]
    public void InterpolationTest()
    {
      var traits = SingleTraits.Instance;
      var value0 = 456.78f;
      var value1 = -321.45f;
      Assert.IsTrue(Numeric.AreEqual(value0, traits.Interpolate(value0, value1, 0.0f)));
      Assert.IsTrue(Numeric.AreEqual(value1, traits.Interpolate(value0, value1, 1.0f)));
      Assert.IsTrue(Numeric.AreEqual(0.25f * value0 + 0.75f * value1, traits.Interpolate(value0, value1, 0.75f)));
    }
  }
}
