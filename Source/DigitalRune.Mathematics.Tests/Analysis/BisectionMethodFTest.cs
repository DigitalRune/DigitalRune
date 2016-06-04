using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class BisectionMethodFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrow()
    {
      new BisectionMethodF(null);
    }


    [Test]
    public void MaxNumberOfIterations()
    {
      BisectionMethodF rootFinder = new BisectionMethodF(x => x);
      rootFinder.MaxNumberOfIterations = 123;
      Assert.AreEqual(123, rootFinder.MaxNumberOfIterations);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MaxNumberOfIterationsException()
    {
      new BisectionMethodF(x => x).MaxNumberOfIterations = -1;
    }


    [Test]
    public void EpsilonX()
    {
      BisectionMethodF rootFinder = new BisectionMethodF(x => x);
      rootFinder.EpsilonX = 0.123f;
      Assert.AreEqual(0.123f, rootFinder.EpsilonX);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonException()
    {
      new BisectionMethodF(x => x).EpsilonX = -0.1f;
    }


    [Test]
    public void ExpandBracketTest1()
    {
      Func<float, float> polynomial = delegate(float x) { return (x - 1) * (x - 10) * (x - 12); };

      BisectionMethodF rootFinder = new BisectionMethodF(polynomial);
      
      float x0 = 2, x1 = 3;
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
      x1 = 10.1f;
      result = rootFinder.ExpandBracket(ref x0, ref x1, 10);
      Assert.IsTrue((polynomial(x0) - 10) * (polynomial(x1) - 10) < 0);
      Assert.IsTrue(result);


      x0 = -1000;
      x1 = -1000.1f;
      rootFinder.MaxNumberOfIterations = 1;
      result = rootFinder.ExpandBracket(ref x0, ref x1);
      Assert.IsFalse(result);
    }


    [Test]
    public void FindRootTest1()
    {
      Func<float, float> polynomial = x => (x - 1)*(x - 10)*(x - 18);

      BisectionMethodF rootFinder = new BisectionMethodF(polynomial);
      rootFinder.EpsilonX = Numeric.EpsilonF / 100;

      float xRoot = rootFinder.FindRoot(0, 2);
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
      Assert.IsTrue(float.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(3, 2);
      Assert.IsTrue(float.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      rootFinder.MaxNumberOfIterations = 1;
      xRoot = rootFinder.FindRoot(0, 1000);      
      Assert.IsTrue(float.IsNaN(xRoot));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);
    }


    [Test]
    public void FindRootForY()
    {
      Func<float, float> polynomial = x => (x - 1) * (x - 10) * (x - 18);

      BisectionMethodF rootFinder = new BisectionMethodF(polynomial);
      rootFinder.EpsilonX = Numeric.EpsilonF / 100;

      float xRoot = rootFinder.FindRoot(0, 2, 2);
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
