using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class RungeKutta4IntegratorDTest
  {
    public VectorD GetFirstOrderDerivatives(VectorD x, double t)
    {
      // A dummy function: f(x[index], t) = x[index] * t;
      VectorD result = new VectorD(x.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = x[i]*t;   
      return result;
    }


    [Test]
    public void Test1()
    {
      VectorD state = new VectorD(new double[]{ 0, 1 });
      VectorD result = new RungeKutta4IntegratorD(GetFirstOrderDerivatives).Integrate(state, 2, 2.5);

      Assert.AreEqual(0, result[0]);
      Assert.AreEqual(3.06103515625, result[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new RungeKutta4IntegratorD(null);
    }
  }    
}
