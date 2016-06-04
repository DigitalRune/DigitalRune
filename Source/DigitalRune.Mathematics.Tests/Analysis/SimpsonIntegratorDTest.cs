using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class SimpsonIntegratorDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNullException()
    {
      new SimpsonIntegratorD().Integrate(null, 0, 1);
    }


    [Test]
    public void Test1()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return  -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
      integrator.Epsilon = 0.000001f;
      double result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      double numberOfIterations = integrator.NumberOfIterations;
      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.000002f));

      integrator.Epsilon = 0.1f;
      result = integrator.Integrate(fDerived, -1.1f, 2.3f);
      Assert.Greater(numberOfIterations, integrator.NumberOfIterations);
      Assert.IsTrue(Numeric.AreEqual(f(2.3f) - f(-1.1f), result, 0.1f));
    }


    [Test]
    public void Test2()
    {
      Func<double, double> f = delegate(double x) { return x * x* x* x * Math.Log(x + Math.Sqrt(x*x + 1)); };

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
      integrator.Epsilon = 0.001f;
      double result = integrator.Integrate(f, 0, 2);

      // Compare number of iterations with trapezoidal integrator.
      TrapezoidalIntegratorD trap = new TrapezoidalIntegratorD();
      trap.Epsilon = integrator.Epsilon;
      double result2 = trap.Integrate(f, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(result, result2, 0.002f));
      Assert.Greater(trap.NumberOfIterations, integrator.NumberOfIterations);
    }


    [Test]
    public void Test3()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
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

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
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

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
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

      SimpsonIntegratorD integrator = new SimpsonIntegratorD();
      integrator.MinNumberOfIterations = 1;
      double result = integrator.Integrate(fDerived, 1, 3);

      // Make one less iteration.
      integrator.MaxNumberOfIterations = integrator.NumberOfIterations - 1;
      result = integrator.Integrate(fDerived, -1, -3);
      Assert.IsTrue(Numeric.AreEqual(f(-3) - f(-1), result, integrator.Epsilon * 10));
    }
  }
}
