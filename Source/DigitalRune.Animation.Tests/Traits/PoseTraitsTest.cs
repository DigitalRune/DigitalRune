//using System;
//using DigitalRune.Geometry;
//using DigitalRune.Mathematics.Algebra;
//using DigitalRune.Mathematics.Statistics;
//using NUnit.Framework;


//namespace DigitalRune.Animation.Traits.Tests
//{
//  [TestFixture]
//  public class PoseTraitsTest
//  {
//    private Random _random;


//    [SetUp]
//    public void Setup()
//    {
//      _random = new Random(123456);
//    }


//    private static bool AreNumericallyEqual(Pose expected, Pose actual)
//    {
//      return Vector3F.AreNumericallyEqual(expected.Position, actual.Position)
//             && Matrix33F.AreNumericallyEqual(expected.Orientation, actual.Orientation);
//    }


//    [Test]
//    public void IdentityTest()
//    {
//      var traits = PoseTraits.Instance;
//      var value = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());
//      Assert.AreEqual(value, traits.Add(value, traits.Identity));
//      Assert.AreEqual(value, traits.Add(traits.Identity, value));
//    }


//    [Test]
//    public void MultiplyTest()
//    {
//      var traits = PoseTraits.Instance;
//      var value = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());
//      Assert.IsTrue(AreNumericallyEqual(Pose.Identity, traits.Multiply(value, 0)));
//      Assert.IsTrue(AreNumericallyEqual(value, traits.Multiply(value, 1)));
//      Assert.IsTrue(AreNumericallyEqual(value * value, traits.Multiply(value, 2)));
//      Assert.IsTrue(AreNumericallyEqual(value * value * value, traits.Multiply(value, 3)));
//      Assert.IsTrue(AreNumericallyEqual(value.Inverse, traits.Multiply(value, -1)));
//      Assert.IsTrue(AreNumericallyEqual(value.Inverse * value.Inverse, traits.Multiply(value, -2)));
//      Assert.IsTrue(AreNumericallyEqual(value.Inverse * value.Inverse * value.Inverse, traits.Multiply(value, -3)));
//    }


//    [Test]
//    public void FromByTest()
//    {
//      // IAnimationValueTraits<T> is used in a from-by animation to a add a relative offset to
//      // the start value.

//      var traits = PoseTraits.Instance;
//      var from = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());
//      var by = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());

//      var to = traits.Add(from, by);
//      Assert.IsTrue(AreNumericallyEqual(by * from, to));

//      Assert.IsTrue(AreNumericallyEqual(from, traits.Add(to, traits.Inverse(by))));
//      Assert.IsTrue(AreNumericallyEqual(by, traits.Add(traits.Inverse(from), to)));
//    }


//    [Test]
//    public void CycleOffsetTest()
//    {
//      // IAnimationValueTraits<T> is used in a cyclic animation to a add the cycle offset in
//      // each iteration.

//      var traits = PoseTraits.Instance;
//      var first = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());    // Animation value of first key frame.
//      var last = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());     // Animation value of last key frame.
//      var cycleOffset = traits.Add(traits.Inverse(first), last);

//      // Cycle offset should be the difference between last and first key frame.
//      Assert.IsTrue(AreNumericallyEqual(last, traits.Add(first, cycleOffset)));
//      Assert.IsTrue(AreNumericallyEqual(last, cycleOffset * first));

//      // Check multiple cycles (post-loop).
//      Assert.IsTrue(AreNumericallyEqual(last, traits.Add(first, traits.Multiply(cycleOffset, 1))));
//      Assert.IsTrue(AreNumericallyEqual(cycleOffset * cycleOffset * last, traits.Add(first, traits.Multiply(cycleOffset, 3))));

//      // Check multiple cycles (pre-loop).
//      Assert.IsTrue(AreNumericallyEqual(first, traits.Add(last, traits.Multiply(cycleOffset, -1))));
//      Assert.IsTrue(AreNumericallyEqual(cycleOffset.Inverse * cycleOffset.Inverse * first, traits.Add(last, traits.Multiply(cycleOffset, -3))));
//    }


//    [Test]
//    public void InterpolationTest()
//    {
//      var traits = PoseTraits.Instance;
//      var value0 = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());
//      var value1 = new Pose(_random.NextVector3F(-100, 100), _random.NextQuaternionF());
//      Assert.IsTrue(AreNumericallyEqual(value0, traits.Interpolate(value0, value1, 0.0f)));
//      Assert.IsTrue(AreNumericallyEqual(value1, traits.Interpolate(value0, value1, 1.0f)));
//      Assert.IsTrue(AreNumericallyEqual(Pose.Interpolate(value0, value1, 0.75f), traits.Interpolate(value0, value1, 0.75f)));
//    }
//  }
//}
