using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class RombergIntegratorDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNullException()
    {
      new RombergIntegratorD().Integrate(null, 0, 1);
    }

    [Test]
    public void Test1()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return  -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.000001;
      double result = integrator.Integrate(fDerived, -1.1, 2.3);
      double numberOfIterations = integrator.NumberOfIterations;
      Assert.IsTrue(Numeric.AreEqual(f(2.3) - f(-1.1), result, 0.000002));
    }


    [Test]
    public void Test2()
    {
      Func<double, double> f = delegate(double x) { return Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.000001;
      double result = integrator.Integrate(fDerived, -2, 2);
      double numberOfIterations = integrator.NumberOfIterations;
      Assert.IsTrue(Numeric.AreEqual(f(2) - f(-2), result, 0.000002));
    }


    [Test]
    public void Test3()
    {
      Func<double, double> f = delegate(double x) { return x * x * x * x * Math.Log(x + Math.Sqrt(x * x + 1)); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.000001;
      double result = integrator.Integrate(f, 0, 2);

      // Compare number of iterations with trapezoidal integrator.
      TrapezoidalIntegratorD trap = new TrapezoidalIntegratorD();
      trap.Epsilon = integrator.Epsilon;
      double result2 = trap.Integrate(f, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(result, result2, 0.000002));
      Assert.Greater(trap.NumberOfIterations, integrator.NumberOfIterations);


      // Compare number of iterations with simpson integrator.
      SimpsonIntegratorD simpson = new SimpsonIntegratorD();
      simpson.Epsilon = integrator.Epsilon;
      result2 = simpson.Integrate(f, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(result, result2, 0.000002));
      Assert.Greater(simpson.NumberOfIterations, integrator.NumberOfIterations);
    }


    [Test]
    public void Test4()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.000001;
      integrator.MaxNumberOfIterations = 5;
      double result = integrator.Integrate(fDerived, -1.1, 2.3);
      
      Assert.IsTrue(Numeric.AreEqual(f(2.3) - f(-1.1), result, 0.000002));
    }


    [Test]
    public void Test5()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.0001;
      double result = integrator.Integrate(fDerived, 2.3, -1.1);

      Assert.IsTrue(Numeric.AreEqual(f(-1.1) - f(2.3), result, 0.0002));
    }


    [Test]
    public void MinNumberOfIterations()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.Epsilon = 0.01;
      integrator.MinNumberOfIterations = 15;
      integrator.Integrate(fDerived, -1.1, 2.3);
      Assert.AreEqual(15, integrator.NumberOfIterations);

      integrator.MinNumberOfIterations = 5;
      integrator.Integrate(fDerived, -1.1, 2.3);
      Assert.Greater(15, integrator.NumberOfIterations);
    }


    [Test]
    public void IntegrateEmptyInterval()
    {
      Func<double, double> f = delegate(double x) { return -0.01 * x * x * x + 0.2 * x * x + 4 * x - 9 + Math.Sin(x); };
      Func<double, double> fDerived = delegate(double x) { return -0.01 * 3 * x * x + 0.2 * 2 * x + 4 + Math.Cos(x); };

      RombergIntegratorD integrator = new RombergIntegratorD();
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

      RombergIntegratorD integrator = new RombergIntegratorD();
      integrator.MinNumberOfIterations = 1;
      double result = integrator.Integrate(fDerived, 1, 3);

      // Make one less iteration.
      integrator.MaxNumberOfIterations = integrator.NumberOfIterations - 1;
      result = integrator.Integrate(fDerived, -1, -3);
      Assert.IsTrue(Numeric.AreEqual(f(-3) - f(-1), result, integrator.Epsilon*10));
    }
  }
}
