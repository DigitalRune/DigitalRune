using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class ExplicitEulerIntegratorDTest
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
      VectorD state = new VectorD (new double[]{ 1, 2, 3, 4, 5, 6 });
      VectorD result = new ExplicitEulerIntegratorD(GetFirstOrderDerivatives).Integrate(state, 2, 2.5);

      Assert.AreEqual(1, result[0]);
      Assert.AreEqual(3, result[1]);
      Assert.AreEqual(5, result[2]);
      Assert.AreEqual(7, result[3]);
      Assert.AreEqual(9, result[4]);
      Assert.AreEqual(11, result[5]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new ExplicitEulerIntegratorD(null);
    }
  }    
}
