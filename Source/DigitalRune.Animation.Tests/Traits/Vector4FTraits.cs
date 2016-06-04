using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Animation.Traits.Tests
{
  [TestFixture]
  public class Vector4FTraitsTest
  {
    [Test]
    public void IdentityTest()
    {
      var traits = Vector4FTraits.Instance;
      var value = new Vector4F(-1, -2, 3, 1);
      Assert.AreEqual(value, traits.Add(value, traits.Identity()));
      Assert.AreEqual(value, traits.Add(traits.Identity(), value));
    }


    [Test]
    public void MultiplyTest()
    {
      var traits = Vector4FTraits.Instance;
      var value = new Vector4F(-1, -2, 3, 1);
      Assert.IsTrue(Vector4F.AreNumericallyEqual(Vector4F.Zero, traits.Multiply(value, 0)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(value, traits.Multiply(value, 1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(value + value, traits.Multiply(value, 2)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(value + value + value, traits.Multiply(value, 3)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(-value, traits.Multiply(value, -1)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(-value - value, traits.Multiply(value, -2)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(-value - value - value, traits.Multiply(value, -3)));
    }


    [Test]
    public void FromByTest()
    {
      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
      // the start value.

      var traits = Vector4FTraits.Instance;
      var from = new Vector4F(-1, -2, 3, 1);
      var by = new Vector4F(4, -5, 6, 1);

      var to = traits.Add(from, by);
      Assert.IsTrue(Vector4F.AreNumericallyEqual(by + from, to));

      Assert.IsTrue(Vector4F.AreNumericallyEqual(from, traits.Add(to, traits.Inverse(by))));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(by, traits.Add(traits.Inverse(from), to)));
    }


    [Test]
    public void CycleOffsetTest()
    {
      // IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
      // each iteration.

      var traits = Vector4FTraits.Instance;
      var first = new Vector4F(1, 2, 3, 1);    // Animation value of first key frame.
      var last = new Vector4F(-4, 5, -6, 5);   // Animation value of last key frame.
      var cycleOffset = traits.Add(traits.Inverse(first), last);

      // Cycle offset should be the difference between last and first key frame.
      Assert.IsTrue(Vector4F.AreNumericallyEqual(last, traits.Add(first, cycleOffset)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(last, cycleOffset + first));

      // Check multiple cycles (post-loop).
      Assert.IsTrue(Vector4F.AreNumericallyEqual(last, traits.Add(first, traits.Multiply(cycleOffset, 1))));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(cycleOffset + cycleOffset + last, traits.Add(first, traits.Multiply(cycleOffset, 3))));

      // Check multiple cycles (pre-loop).
      Assert.IsTrue(Vector4F.AreNumericallyEqual(first, traits.Add(last, traits.Multiply(cycleOffset, -1))));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(first - cycleOffset - cycleOffset, traits.Add(last, traits.Multiply(cycleOffset, -3))));
    }


    [Test]
    public void InterpolationTest()
    {
      var traits = Vector4FTraits.Instance;
      var value0 = new Vector4F(1, 2, 3, 1);
      var value1 = new Vector4F(-4, 5, -6, 5);
      Assert.IsTrue(Vector4F.AreNumericallyEqual(value0, traits.Interpolate(value0, value1, 0.0f)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(value1, traits.Interpolate(value0, value1, 1.0f)));
      Assert.IsTrue(Vector4F.AreNumericallyEqual(0.25f * value0 + 0.75f * value1, traits.Interpolate(value0, value1, 0.75f)));
    }
  }
}
