using System;
using DigitalRune.Collections;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class RadialBasisRegressionFTest
  {
    [Test]
    public void Test1()
    {
      float[] xValues = new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      float[] yValues = new float[] { 0, 4, 5, 3, 1, 2, 3, 7, 8, 9 };

      // Setup scattered interpolation with radial basis functions.
      RadialBasisRegressionF rbr = new RadialBasisRegressionF();
      rbr.BasisFunction = (x, i) => MathHelper.Gaussian(x, 1, 0, 0.5f);  // Use Gaussian function as basis function.
      //rbr.BaseFunction = delegate(float x) { return x; }; // Use identity functions as basis function.
      for (int i = 0; i < xValues.Length; i++)
        rbr.Add(new Pair<VectorF, VectorF>(new VectorF(1, xValues[i]), new VectorF(1, yValues[i])));
      rbr.Setup();

      Assert.IsTrue(Numeric.AreEqual(yValues[0], rbr.Compute(new VectorF(1, 0))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[2], rbr.Compute(new VectorF(1, 2))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[3], rbr.Compute(new VectorF(1, 3))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[4], rbr.Compute(new VectorF(1, 4))[0]));
      Assert.IsTrue(Numeric.AreEqual(yValues[8], rbr.Compute(new VectorF(1, 8))[0]));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void BaseFunctionException()
    {
      // Setup scattered interpolation with radial basis functions.
      RadialBasisRegressionF rbr = new RadialBasisRegressionF();
      rbr.BasisFunction = null;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void TestException()
    {
      float[] xValues = new float[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
      float[] yValues = new float[] { 0, 4, 5, 3, 1, 2, 3, 7, 8, 9 };

      // Setup scattered interpolation with radial basis functions.
      RadialBasisRegressionF rbr = new RadialBasisRegressionF();
      rbr.BasisFunction = (x, i) => MathHelper.Gaussian(x, 1, 0, 100);  // Use Gaussian function as basis function.
      for (int i = 0; i < xValues.Length; i++)
        rbr.Add(new Pair<VectorF, VectorF>(new VectorF(1, xValues[i]), new VectorF(1, yValues[i])));
      rbr.Setup();

      rbr.Compute(new VectorF(1, 0));
    }
  }
}
