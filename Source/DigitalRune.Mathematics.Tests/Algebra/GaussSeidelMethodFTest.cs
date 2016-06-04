using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class GaussSeidelMethodFTest
  {
    [Test]
    public void Test1()
    {
      MatrixF A = new MatrixF(new float[,] { { 4 } });
      VectorF b = new VectorF(new float[] { 20 });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorF.AreNumericallyEqual(new VectorF(1, 5), x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test2()
    {
      MatrixF A = new MatrixF(new float[,] { { 1, 0 }, 
                                             { 0, 1 }});
      VectorF b = new VectorF(new float[] { 20, 28 });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorF.AreNumericallyEqual(b, x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test3()
    {
      MatrixF A = new MatrixF(new float[,] { { 2, 0 }, 
                                             { 0, 2 }});
      VectorF b = new VectorF(new float[] { 20, 28 });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(A, null, b);

      Assert.IsTrue(VectorF.AreNumericallyEqual(b / 2, x));
      Assert.AreEqual(2, solver.NumberOfIterations);
    }


    [Test]
    public void Test4()
    {
      MatrixF A = new MatrixF(new float[,] { { -12, 2 }, 
                                             { 2, 3 }});
      VectorF b = new VectorF(new float[] { 20, 28 });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(A, null, b);

      VectorF solution = MatrixF.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorF.AreNumericallyEqual(solution, x));
    }


    [Test]
    public void Test5()
    {
      MatrixF A = new MatrixF(new float[,] { { -21, 2, -4, 0 }, 
                                             { 2, 3, 0.1f, -1 },
                                             { 2, 10, 111.1f, -11 },
                                             { 23, 112, 111.1f, -143 }});
      VectorF b = new VectorF(new float[] { 20, 28, -12, 0.1f });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(A, null, b);

      VectorF solution = MatrixF.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorF.AreNumericallyEqual(solution, x));
    }


    [Test]
    public void Test6()
    {
      MatrixF A = new MatrixF(new float[,] { { -21, 2, -4, 0 }, 
                                             { 2, 3, 0.1f, -1 },
                                             { 2, 10, 111.1f, -11 },
                                             { 23, 112, 111.1f, -143 }});
      VectorF b = new VectorF(new float[] { 20, 28, -12, 0.1f });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      solver.MaxNumberOfIterations = 4;
      VectorF x = solver.Solve(A, null, b);

      VectorF solution = MatrixF.SolveLinearEquations(A, b);
      Assert.IsFalse(VectorF.AreNumericallyEqual(solution, x));
      Assert.AreEqual(4, solver.NumberOfIterations);
    }


    [Test]
    public void Test7()
    {
      MatrixF A = new MatrixF(new float[,] { { -21, 2, -4, 0 }, 
                                             { 2, 3, 0.1f, -1 },
                                             { 2, 10, 111.1f, -11 },
                                             { 23, 112, 111.1f, -143 }});
      VectorF b = new VectorF(new float[] { 20, 28, -12, 0.1f });

      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      solver.Epsilon = 0.1f;
      VectorF x = solver.Solve(A, null, b);

      VectorF solution = MatrixF.SolveLinearEquations(A, b);
      Assert.IsTrue(VectorF.AreNumericallyEqual(solution, x, 0.1f));
      Assert.IsFalse(VectorF.AreNumericallyEqual(solution, x));
      Assert.Greater(12, solver.NumberOfIterations); // For normal accuracy (EpsilonF) we need 12 iterations.
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestArgumentNullException()
    {
      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(null, null, new VectorF());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestArgumentNullException2()
    {
      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(new MatrixF(), null, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException()
    {
      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(new MatrixF(3, 4), null, new VectorF(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException2()
    {
      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(new MatrixF(3, 3), null, new VectorF(4));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException3()
    {
      GaussSeidelMethodF solver = new GaussSeidelMethodF();
      VectorF x = solver.Solve(new MatrixF(3, 3), new VectorF(4), new VectorF(3));
    }
  }
}
