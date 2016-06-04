using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Animation.Traits.Tests
{
  [TestFixture]
  public class QuaternionFTraitsTest
  {
    private Random _random;


    [SetUp]
    public void Setup()
    {
      _random = new Random(123456);
    }


    [Test]
    public void IdentityTest()
    {
      var traits = QuaternionFTraits.Instance;
      var value = _random.NextQuaternionF();
      Assert.AreEqual(value, traits.Add(value, traits.Identity()));
      Assert.AreEqual(value, traits.Add(traits.Identity(), value));
    }


    [Test]
    public void MultiplyTest()
    {
      var traits = QuaternionFTraits.Instance;
      var value = _random.NextQuaternionF();
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(QuaternionF.Identity, traits.Multiply(value, 0)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value, traits.Multiply(value, 1)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value * value, traits.Multiply(value, 2)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value * value * value, traits.Multiply(value, 3)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value.Inverse, traits.Multiply(value, -1)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value.Inverse * value.Inverse, traits.Multiply(value, -2)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value.Inverse * value.Inverse * value.Inverse, traits.Multiply(value, -3)));
    }


    [Test]
    public void FromByTest()
    {
      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
      // the start value.

      var traits = QuaternionFTraits.Instance;
      var from = _random.NextQuaternionF();
      var by = _random.NextQuaternionF();

      var to = traits.Add(from, by);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(by * from, to));
      
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(from, traits.Add(to, traits.Inverse(by))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(by, traits.Add(traits.Inverse(from), to)));
    }


    [Test]
    public void CycleOffsetTest()
    {
      // IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
      // each iteration.

      var traits = QuaternionFTraits.Instance;
      var first = _random.NextQuaternionF();    // Animation value of first key frame.
      var last = _random.NextQuaternionF();     // Animation value of last key frame.
      var cycleOffset = traits.Add(traits.Inverse(first), last);

      // Cycle offset should be the difference between last and first key frame.
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(last, traits.Add(first, cycleOffset)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(last, cycleOffset * first));

      // Check multiple cycles (post-loop).
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(last, traits.Add(first, traits.Multiply(cycleOffset, 1))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(cycleOffset * cycleOffset * last, traits.Add(first, traits.Multiply(cycleOffset, 3))));

      // Check multiple cycles (pre-loop).
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(first, traits.Add(last, traits.Multiply(cycleOffset, -1))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(cycleOffset.Inverse * cycleOffset.Inverse * first, traits.Add(last, traits.Multiply(cycleOffset, -3))));
    }


    [Test]
    public void InterpolationTest()
    {
      var traits = QuaternionFTraits.Instance;
      var value0 = _random.NextQuaternionF();
      var value1 = _random.NextQuaternionF();
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value0, traits.Interpolate(value0, value1, 0.0f)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(value1, traits.Interpolate(value0, value1, 1.0f)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(InterpolationHelper.Lerp(value0, value1, 0.75f), traits.Interpolate(value0, value1, 0.75f)));
    }
  }
}
