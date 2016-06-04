using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class ExplicitEulerIntegratorFTest
  {
    public VectorF GetFirstOrderDerivatives(VectorF x, float t)
    {
      // A dummy function: f(x[index], t) = index * t;
      VectorF result = new VectorF(x.NumberOfElements);
      for (int i = 0; i < result.NumberOfElements; i++)
        result[i] = i*t;   
      return result;
    }


    [Test]
    public void Test1()
    {
      VectorF state = new VectorF (new float[]{ 1, 2, 3, 4, 5, 6 });
      VectorF result = new ExplicitEulerIntegratorF(GetFirstOrderDerivatives).Integrate(state, 2, 2.5f);

      Assert.AreEqual(1f, result[0]);
      Assert.AreEqual(3f, result[1]);
      Assert.AreEqual(5f, result[2]);
      Assert.AreEqual(7f, result[3]);
      Assert.AreEqual(9f, result[4]);
      Assert.AreEqual(11f, result[5]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new ExplicitEulerIntegratorF(null);
    }
  }    
}
