using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class TrapezoidalIntegratorDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MinNumberOfIterationsException()
    {
      new TrapezoidalIntegratorD().MinNumberOfIterations = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MaxNumberOfIterationsException()
    {
      new TrapezoidalIntegratorD().MaxNumberOfIterations = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonException()
    {
      new TrapezoidalIntegratorD().Epsilon = -0.001f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNullException()
    {
      new TrapezoidalIntegratorD().Integrate(null, 0, 1);
    }    


    [Test]
    public void Test1()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return  -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };
      
      TrapezoidalIntegratorD integrator = new TrapezoidalIntegratorD();
      integrator.Epsilon = 0.0001f;
      double result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      double numberOfIterations = integrator.NumberOfIterations;

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
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      TrapezoidalIntegratorD integrator = new TrapezoidalIntegratorD();
      integrator.Epsilon = 0.0001f;
      double result = integrator.Integrate(fDerived, 2.3f, -1.1f);
      double numberOfIterations = integrator.NumberOfIterations;

      Assert.IsTrue(Numeric.AreEqual(f(-1.1f) - f(2.3f), result, 0.0002f));
    }


    [Test]
    public void MinNumberOfIterations()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      TrapezoidalIntegratorD integrator = new TrapezoidalIntegratorD();
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
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      TrapezoidalIntegratorD integrator = new TrapezoidalIntegratorD();
      double result = integrator.Integrate(fDerived, 0, 0);
      Assert.AreEqual(0, result);

      result = integrator.Integrate(fDerived, -3, -3);
      Assert.AreEqual(0, result);
    }


    [Test]
    public void IntegrateUntilMaxIterations()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      TrapezoidalIntegratorD integrator = new TrapezoidalIntegratorD();
      integrator.MinNumberOfIterations = 1;
      double result = integrator.Integrate(fDerived, 1, 3);

      // Make one less iteration.
      integrator.MaxNumberOfIterations = integrator.NumberOfIterations - 1;
      result = integrator.Integrate(fDerived, -1, -3);
      Assert.IsTrue(Numeric.AreEqual(f(-3) - f(-1), result, integrator.Epsilon * 10));
    }
  }
}
