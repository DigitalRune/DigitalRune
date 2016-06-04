using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRune.Animation.Traits.Tests
{
  [TestFixture]
  public class QuaternionTraitsTest
  {
    private Random _random;


    [SetUp]
    public void Setup()
    {
      _random = new Random(123456);
    }

    [Test]
    public void XnaQuaternionMultiplication()
    {
      QuaternionF q1 = _random.NextQuaternionF();
      QuaternionF q2 = _random.NextQuaternionF();
      var q1Xna = (Quaternion)q1;
      var q2Xna = (Quaternion)q2;
      
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q1 * q2, (QuaternionF)(q1Xna * q2Xna)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(q2 * q1, (QuaternionF)(q2Xna * q1Xna)));
    }


    [Test]
    public void IdentityTest()
    {
      var traits = QuaternionTraits.Instance;
      var value = (Quaternion)_random.NextQuaternionF();
      Assert.AreEqual(value, traits.Add(value, traits.Identity()));
      Assert.AreEqual(value, traits.Add(traits.Identity(), value));
    }


    [Test]
    public void MultiplyTest()
    {
      var traits = QuaternionTraits.Instance;
      var value = (Quaternion)_random.NextQuaternionF();
      Quaternion valueInverse = value;
      valueInverse.Conjugate();
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)Quaternion.Identity, (QuaternionF)traits.Multiply(value, 0)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)value, (QuaternionF)traits.Multiply(value, 1)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)(value * value), (QuaternionF)traits.Multiply(value, 2)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)(value * value * value), (QuaternionF)traits.Multiply(value, 3)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)valueInverse, (QuaternionF)traits.Multiply(value, -1)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)valueInverse * (QuaternionF)valueInverse, (QuaternionF)traits.Multiply(value, -2)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)valueInverse * (QuaternionF)valueInverse * (QuaternionF)valueInverse, (QuaternionF)traits.Multiply(value, -3)));
    }


    [Test]
    public void FromByTest()
    {
      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
      // the start value.

      var traits = QuaternionTraits.Instance;
      var from = (Quaternion)_random.NextQuaternionF();
      var by = (Quaternion)_random.NextQuaternionF();

      var to = traits.Add(from, by);
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)by * (QuaternionF)from, (QuaternionF)to));

      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)from, (QuaternionF)traits.Add(to, traits.Inverse(by))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)by, (QuaternionF)traits.Add(traits.Inverse(from), to)));
    }


    [Test]
    public void CycleOffsetTest()
    {
      // IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
      // each iteration.

      var traits = QuaternionTraits.Instance;
      var first = (Quaternion)_random.NextQuaternionF();    // Animation value of first key frame.
      var last = (Quaternion)_random.NextQuaternionF();     // Animation value of last key frame.
      var cycleOffset = traits.Add(traits.Inverse(first), last);
      var cycleOffsetInverse = cycleOffset;
      cycleOffsetInverse.Conjugate();

      // Cycle offset should be the difference between last and first key frame.
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)last, (QuaternionF)traits.Add(first, cycleOffset)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)last, (QuaternionF)(cycleOffset * first)));

      // Check multiple cycles (post-loop).
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)last, (QuaternionF)traits.Add(first, traits.Multiply(cycleOffset, 1))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)cycleOffset * (QuaternionF)cycleOffset * (QuaternionF)last, (QuaternionF)traits.Add(first, traits.Multiply(cycleOffset, 3))));

      // Check multiple cycles (pre-loop).
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)first, (QuaternionF)traits.Add(last, traits.Multiply(cycleOffset, -1))));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)cycleOffsetInverse * (QuaternionF)cycleOffsetInverse * (QuaternionF)first, (QuaternionF)traits.Add(last, traits.Multiply(cycleOffset, -3))));
    }


    [Test]
    public void InterpolationTest()
    {
      var traits = QuaternionTraits.Instance;
      var value0 = (Quaternion)_random.NextQuaternionF();
      var value1 = (Quaternion)_random.NextQuaternionF();
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)value0, (QuaternionF)traits.Interpolate(value0, value1, 0.0f)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual((QuaternionF)value1, (QuaternionF)traits.Interpolate(value0, value1, 1.0f)));
      Assert.IsTrue(QuaternionF.AreNumericallyEqual(InterpolationHelper.Lerp((QuaternionF)value0, (QuaternionF)value1, 0.75f), (QuaternionF)traits.Interpolate(value0, value1, 0.75f)));
    }
  }
}
