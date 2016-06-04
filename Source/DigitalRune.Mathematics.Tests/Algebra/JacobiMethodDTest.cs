using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class JacobiMethodDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void MaxNumberOfIterationsException()
    {
      new JacobiMethodD().MaxNumberOfIterations = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonException()
    {
      new JacobiMethodD().Epsilon = -0.001;
    }


    [Test]
    public void SolveWithDefaultInitialGuess()
    {
      MatrixD A = new MatrixD(new double[,] { { 4 } });
      VectorD b = new VectorD(new double[] { 20 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, b);

      Assert.IsTrue(VectorD.AreNumericallyEqual(new VectorD(1, 5), x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }



    [Test]
    public void Test1()
    {
      MatrixD A = new MatrixD(new double[,] { { 4 } });
      VectorD b = new VectorD(new double[] { 20 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorD.AreNumericallyEqual(new VectorD(1, 5), x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test2()
    {
      MatrixD A = new MatrixD(new double[,] { { 1, 0 }, 
                                              { 0, 1 }});
      VectorD b = new VectorD(new double[] { 20, 28 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorD.AreNumericallyEqual(b, x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test3()
    {
      MatrixD A = new MatrixD(new double[,] { { 2, 0 }, 
                                              { 0, 2 }});
      VectorD b = new VectorD(new double[] { 20, 28 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorD.AreNumericallyEqual(b/2, x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test4()
    {
      MatrixD A = new MatrixD(new double[,] { { -12, 2 }, 
                                              { 2, 3 }});
      VectorD b = new VectorD(new double[] { 20, 28 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, null, b);

      VectorD solution = MatrixD.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorD.AreNumericallyEqual(solution, x));      
    }


    [Test]
    public void Test5()
    {
      MatrixD A = new MatrixD(new double[,] { { -21, 2, -4, 0 }, 
                                              { 2, 3, 0.1, -1 },
                                              { 2, 10, 111.1, -11 },
                                              { 23, 112, 111.1, -143 }});
      VectorD b = new VectorD(new double[] { 20, 28, -12, 0.1 });

      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(A, null, b);

      VectorD solution = MatrixD.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorD.AreNumericallyEqual(solution, x));
    }


    [Test]
    public void Test6()
    {
      MatrixD A = new MatrixD(new double[,] { { -21, 2, -4, 0 }, 
                                              { 2, 3, 0.1, -1 },
                                              { 2, 10, 111.1, -11 },
                                              { 23, 112, 111.1, -143 }});
      VectorD b = new VectorD(new double[] { 20, 28, -12, 0.1 });

      JacobiMethodD solver = new JacobiMethodD();
      solver.MaxNumberOfIterations = 10;
      VectorD x = solver.Solve(A, null, b);

      VectorD solution = MatrixD.SolveLinearEquations(A, b);
      Assert.IsFalse(VectorD.AreNumericallyEqual(solution, x));
      Assert.AreEqual(10, solver.NumberOfIterations);
    }


    [Test]
    public void Test7()
    {
      MatrixD A = new MatrixD(new double[,] { { -21, 2, -4, 0 }, 
                                              { 2, 3, 0.1, -1 },
                                              { 2, 10, 111.1, -11 },
                                              { 23, 112, 111.1, -143 }});
      VectorD b = new VectorD(new double[] { 20, 28, -12, 0.1 });

      JacobiMethodD solver = new JacobiMethodD();
      solver.MaxNumberOfIterations = 10;
      solver.Epsilon = 0.1;
      VectorD x = solver.Solve(A, null, b);

      VectorD solution = MatrixD.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorD.AreNumericallyEqual(solution, x, 0.1));
      Assert.IsFalse(VectorD.AreNumericallyEqual(solution, x));
      Assert.Greater(10, solver.NumberOfIterations);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestArgumentNullException()
    {
      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(null, null, new VectorD());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestArgumentNullException2()
    {
      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(new MatrixD(), null, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException()
    {
      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(new MatrixD(3, 4), null, new VectorD(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException2()
    {
      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(new MatrixD(3, 3), null, new VectorD(4));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException3()
    {
      JacobiMethodD solver = new JacobiMethodD();
      VectorD x = solver.Solve(new MatrixD(3, 3), new VectorD(4), new VectorD(3));
    }
  }
}
