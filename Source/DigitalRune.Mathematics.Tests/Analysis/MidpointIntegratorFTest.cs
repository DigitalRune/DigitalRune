using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class MidpointIntegratorFTest
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
      VectorF state = new VectorF (new float[] { 0, 1, 2, 3, 4, 5 });
      VectorF result = new MidpointIntegratorF(GetFirstOrderDerivatives).Integrate(state, 2, 2.5f);

      Assert.AreEqual(0f, result[0]);
      Assert.AreEqual(2.125f, result[1]);
      Assert.AreEqual(4.25f, result[2]);
      Assert.AreEqual(6.375f, result[3]);
      Assert.AreEqual(8.5f, result[4]);
      Assert.AreEqual(10.625f, result[5]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new MidpointIntegratorF(null);
    }
  }    
}
