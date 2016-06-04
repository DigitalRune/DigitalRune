using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class RungeKutta4IntegratorFTest
  {
    public VectorF GetFirstOrderDerivatives(VectorF x, float t)
    {
      // A dummy function: f(x[index], t) = x[index] * t;
      VectorF result = new VectorF(x.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = x[i]*t;   
      return result;
    }


    [Test]
    public void Test1()
    {
      VectorF state = new VectorF(new float[]{ 0, 1 });
      VectorF result = new RungeKutta4IntegratorF(GetFirstOrderDerivatives).Integrate(state, 2, 2.5f);

      Assert.AreEqual(0f, result[0]);
      Assert.AreEqual(3.061035156f, result[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new RungeKutta4IntegratorF(null);
    }
  }    
}
