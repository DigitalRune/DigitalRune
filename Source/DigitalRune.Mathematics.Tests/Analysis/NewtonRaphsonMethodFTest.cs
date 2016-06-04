using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class NewtonRaphsonMethodFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrowWhenFirstParamIsNull()
    {
      new NewtonRaphsonMethodF(null, x => x);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorShouldThrowWhenSecondParamIsNull()
    {
      new NewtonRaphsonMethodF(x => x, null);
    }


    [Test]
    public void FindRootTest1()
    {
      Func<float, float> polynomial = x => (x - 1)*(x - 10)*(x - 18);
      Func<float, float> computeDerivative = x => 3 * x * x - 58 * x + 208;

      NewtonRaphsonMethodF rootFinder = new NewtonRaphsonMethodF(polynomial, computeDerivative);

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

      xRoot = rootFinder.FindRoot(0, 3);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(3, 0);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-6, 2);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-1, 1);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(-1, 1);
      Assert.IsTrue(Numeric.AreEqual(0, polynomial(xRoot)));
      Console.WriteLine("NumberOfIterations: {0}", rootFinder.NumberOfIterations);

      xRoot = rootFinder.FindRoot(0.9f, 9.9f);
      Assert.IsTrue(float.IsNaN(xRoot)); // Here Newton fails because it jumps out of the brackets.
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
  }
}
