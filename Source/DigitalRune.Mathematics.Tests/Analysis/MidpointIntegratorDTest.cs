using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class MidpointIntegratorDTest
  {
    public VectorD GetFirstOrderDerivatives(VectorD x, double t)
    {
      // A dummy function: f(x[index], t) = index * t;
      VectorD result = new VectorD(x.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = i*t;   
      return result;
    }


    [Test]
    public void Test1()
    {
      VectorD state = new VectorD (new double[] { 0, 1, 2, 3, 4, 5 });
      VectorD result = new MidpointIntegratorD(GetFirstOrderDerivatives).Integrate(state, 2, 2.5);

      Assert.AreEqual(0, result[0]);
      Assert.AreEqual(2.125, result[1]);
      Assert.AreEqual(4.25, result[2]);
      Assert.AreEqual(6.375, result[3]);
      Assert.AreEqual(8.5, result[4]);
      Assert.AreEqual(10.625, result[5]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new MidpointIntegratorD(null);
    }
  }    
}
