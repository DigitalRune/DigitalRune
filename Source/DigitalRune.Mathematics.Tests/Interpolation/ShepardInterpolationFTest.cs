using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class ShepardInterpolationFTest
  {
    [Test]
    public void Test1()
    {
      float[] xValues = new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      float[] yValues = new float[] { 0, 4, 5, 3, 1, 2, 3, 7, 8, 9 };

      // Setup scattered interpolation with Shepard's method.
      ShepardInterpolationF shepard = new ShepardInterpolationF();
      for (int i = 0; i < xValues.Length; i++)
        shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, xValues[i]), new VectorF(1, yValues[i])));
      shepard.Power = 3f;

      Assert.IsTrue(Numeric.AreEqual(yValues[0], shepard.Compute(new VectorF(1, 0))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[2], shepard.Compute(new VectorF(1, 2))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[3], shepard.Compute(new VectorF(1, 3))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[4], shepard.Compute(new VectorF(1, 4))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[8], shepard.Compute(new VectorF(1, 8))[0]));

      // Test clear and reuse object.
      shepard.Clear();
      for (int i = 0; i < xValues.Length; i++)
        shepard.Add(new Pair<VectorF, VectorF>(new VectorF(1, xValues[i]), new VectorF(1, yValues[i])));
      Assert.IsTrue(Numeric.AreEqual(yValues[0], shepard.Compute(new VectorF(1, 0))[0]));
    }
  }
}
