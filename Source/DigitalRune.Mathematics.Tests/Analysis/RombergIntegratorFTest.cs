using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class RombergIntegratorFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNullException()
    {
      new RombergIntegratorF().Integrate(null, 0, 1);
    }

    [Test]
    public void Test1()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float)Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return  -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float)Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.000001f;
      float result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      float numberOfIterations = integrator.NumberOfIterations;
      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.000002f));
    }


    [Test]
    public void Test2()
    {
      Func<float, float> f = delegate(float x) { return (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.000001f;
      float result = integrator.Integrate(fDerived, -2f, 2f);
      float numberOfIterations = integrator.NumberOfIterations;
      Assert.IsTrue(Numeric.AreEqual(f(2f) - f(-2f), result, 0.000002f));
    }


    [Test]
    public void Test3()
    {
      Func<float, float> f = delegate(float x) { return x * x * x * x * (float) Math.Log(x + Math.Sqrt(x * x + 1)); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.000001f;
      float result = integrator.Integrate(f, 0, 2);

      // Compare number of iterations with trapezoidal integrator.
      TrapezoidalIntegratorF trap = new TrapezoidalIntegratorF();
      trap.Epsilon = integrator.Epsilon;
      float result2 = trap.Integrate(f, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(result, result2, 0.000002f));
      Assert.Greater(trap.NumberOfIterations, integrator.NumberOfIterations);


      // Compare number of iterations with simpson integrator.
      SimpsonIntegratorF simpson = new SimpsonIntegratorF();
      simpson.Epsilon = integrator.Epsilon;
      result2 = simpson.Integrate(f, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(result, result2, 0.000002f));
      Assert.Greater(simpson.NumberOfIterations, integrator.NumberOfIterations);
    }


    [Test]
    public void Test4()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.000001f;
      integrator.MaxNumberOfIterations = 5;
      float result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      
      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.000002f));
    }


    [Test]
    public void Test5()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.0001f;
      float result = integrator.Integrate(fDerived, 2.3f, -1.1f);

      Assert.IsTrue(Numeric.AreEqual(f(-1.1f) - f(2.3f), result, 0.0002f));
    }


    [Test]
    public void MinNumberOfIterations()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.Epsilon = 0.01f;
      integrator.MinNumberOfIterations = 15;
      integrator.Integrate(fDerived, -1.1f, 2.3f);
      Assert.AreEqual(15, integrator.NumberOfIterations);

      integrator.MinNumberOfIterations = 5;
      integrator.Integrate(fDerived, -1.1f, 2.3f);
      Assert.Greater(15, integrator.NumberOfIterations);
    }


    [Test]
    public void IntegrateEmptyInterval()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      float result = integrator.Integrate(fDerived, 0, 0);
      Assert.AreEqual(0, result);

      result = integrator.Integrate(fDerived, -3, -3);
      Assert.AreEqual(0, result);
    }


    [Test]
    public void IntegrateUntilMaxIterations()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      RombergIntegratorF integrator = new RombergIntegratorF();
      integrator.MinNumberOfIterations = 1;
      float result = integrator.Integrate(fDerived, 1, 3);

      // Make one less iteration.
      integrator.MaxNumberOfIterations = integrator.NumberOfIterations - 1;
      result = integrator.Integrate(fDerived, -1, -3);
      Assert.IsTrue(Numeric.AreEqual(f(-3) - f(-1), result, integrator.Epsilon*10));
    }
  }
}
