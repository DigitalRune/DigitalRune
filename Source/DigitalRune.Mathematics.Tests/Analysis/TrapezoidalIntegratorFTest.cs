using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class TrapezoidalIntegratorFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MinNumberOfIterationsException()
    {
      new TrapezoidalIntegratorF().MinNumberOfIterations = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MaxNumberOfIterationsException()
    {
      new TrapezoidalIntegratorF().MaxNumberOfIterations = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonException()
    {
      new TrapezoidalIntegratorF().Epsilon = -0.001f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNullException()
    {
      new TrapezoidalIntegratorF().Integrate(null, 0, 1);
    }    


    [Test]
    public void Test1()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float)Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return  -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float)Math.Cos(x); };
      
      TrapezoidalIntegratorF integrator = new TrapezoidalIntegratorF();
      integrator.Epsilon = 0.0001f;
      float result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      float numberOfIterations = integrator.NumberOfIterations;

      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.0002f));

      integrator.Epsilon = 0.1f;
      result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      Assert.Greater(numberOfIterations, integrator.NumberOfIterations);
      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.1f));
      Assert.IsFalse(Numeric.AreEqual(f(2.3f) - f(-1.1f), result));
    }


    [Test]
    public void Test2()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      TrapezoidalIntegratorF integrator = new TrapezoidalIntegratorF();
      integrator.Epsilon = 0.0001f;
      float result = integrator.Integrate(fDerived, 2.3f, -1.1f);
      float numberOfIterations = integrator.NumberOfIterations;

      Assert.IsTrue(Numeric.AreEqual(f(-1.1f) - f(2.3f), result, 0.0002f));
    }


    [Test]
    public void MinNumberOfIterations()
    {
      Func<float, float> f = delegate(float x) { return -0.01f * x * x * x + 0.2f * x * x + 4 * x - 9 + (float) Math.Sin(x); };
      Func<float, float> fDerived = delegate(float x) { return -0.01f * 3 * x * x + 0.2f * 2 * x + 4 + (float) Math.Cos(x); };

      TrapezoidalIntegratorF integrator = new TrapezoidalIntegratorF();
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

      TrapezoidalIntegratorF integrator = new TrapezoidalIntegratorF();
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

      TrapezoidalIntegratorF integrator = new TrapezoidalIntegratorF();
      integrator.MinNumberOfIterations = 1;
      float result = integrator.Integrate(fDerived, 1, 3);

      // Make one less iteration.
      integrator.MaxNumberOfIterations = integrator.NumberOfIterations - 1;
      result = integrator.Integrate(fDerived, -1, -3);
      Assert.IsTrue(Numeric.AreEqual(f(-3) - f(-1), result, integrator.Epsilon * 10));
    }
  }
}
