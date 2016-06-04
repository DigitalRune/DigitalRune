using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class BisectionMethodDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrow()
    {
      new BisectionMethodD(null);
    }


    [Test]
    public void MaxNumberOfIterations()
    {
      BisectionMethodD rootFinder = new BisectionMethodD(x => x);
      rootFinder.MaxNumberOfIterations = 123;
      Assert.AreEqual(123, rootFinder.MaxNumberOfIterations);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MaxNumberOfIterationsException()
    {
      new BisectionMethodD(x => x).MaxNumberOfIterations = -1;
    }


    [Test]
    public void EpsilonX()
    {
      BisectionMethodD rootFinder = new BisectionMethodD(x => x);
      rootFinder.EpsilonX = 0.123;
      Assert.AreEqual(0.123, rootFinder.EpsilonX);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonXException()
    {
      new BisectionMethodD(x => x).EpsilonX = -0.1;
    }


    [Test]
    public void ExpandBracketTest1()
    {
      Func<double, double> polynomial = delegate(double x) { return (x - 1) * (x - 10) * (x - 12); };

      BisectionMethodD rootFinder = new BisectionMethodD(polynomial);
      
      double x0 = 2, x1 = 3;
      bool result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsTrue(polynomial(x0) * polynomial(x1) < 0);
      Assert.IsTrue(result);

      x0 = 2; 
      x1 = 2;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsTrue(polynomial(x0) * polynomial(x1) < 0);
      Assert.IsTrue(result);

      x0 = 0;
      x1 = 2;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsTrue(polynomial(x0) * polynomial(x1) < 0);
      Assert.IsTrue(result);

      x0 = 5;
      x1 = 3;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsTrue(polynomial(x0) * polynomial(x1) < 0);
      Assert.IsTrue(result);

      x0 = -1;
      x1 = 0;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsTrue(polynomial(x0) * polynomial(x1) < 0);
      Assert.IsTrue(result);


      x0 = -1;
      x1 = 0;
      result = rootFinder.ExpandBracket(ref x0, ref x1, 10);
      Assert.IsTrue((polynomial(x0) - 10) * (polynomial(x1) - 10) < 0);
      Assert.IsTrue(result);


      x0 = 10;
      x1 = 10.1;
      result = rootFinder.ExpandBracket(ref x0, ref x1, 10);
      Assert.IsTrue((polynomial(x0) - 10) * (polynomial(x1) - 10) < 0);
      Assert.IsTrue(result);


      x0 = -1000;
      x1 = -1000.1;
      rootFinder.MaxNumberOfIterations = 1;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsFalse(result);
    }


    [Test]
    public void FindRootTest1()
    {
      Func<double, double> polynomial = x => (x - 1)*(x - 10)*(x - 18);

      BisectionMethodD rootFinder = new BisectionMethodD(polynomial);
      rootFinder.EpsilonX = Numeric.EpsilonD / 100;

      double xRoot = rootFinder.FindRoot(0, 2);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(4, 10);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(10, 4);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(10, 12);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(2, 0);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(0, 7);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(7, 0);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-10, 2);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-1, 1);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-1, 1);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(2, 3);
      Assert.IsTrue(double.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(3, 2);
      Assert.IsTrue(double.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      rootFinder.MaxNumberOfIterations = 1;
      xRoot = rootFinder.FindRoot(0, 1000);      
      Assert.IsTrue(double.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);
    }


    [Test]
    public void FindRootForY()
    {
      Func<double, double> polynomial = x => (x - 1) * (x - 10) * (x - 18);

      BisectionMethodD rootFinder = new BisectionMethodD(polynomial);
      rootFinder.EpsilonX = Numeric.EpsilonD / 100;

      double xRoot = rootFinder.FindRoot(0, 2, 2);
      Assert.IsTrue(Numeric.AreEqual(2, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(2, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(2, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(0, 7, 2);
      Assert.IsTrue(Numeric.AreEqual(2, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(7, 0, 2);
      Assert.IsTrue(Numeric.AreEqual(2, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-10, 2, 2);
      Assert.IsTrue(Numeric.AreEqual(2, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);
    }

  }
}
