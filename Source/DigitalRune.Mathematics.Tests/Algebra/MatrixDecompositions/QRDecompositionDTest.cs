using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class QRDecompositionDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new QRDecompositionD(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new QRDecompositionD(new MatrixD(2, 3));  // more columns than rows.
    }


    [Test]
    public void TestMatricesWithoutFullRank()
    {
      MatrixD a = new MatrixD(3, 3);
      QRDecompositionD qr = new QRDecompositionD(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);

      a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 4, 5, 6 } });
      qr = new QRDecompositionD(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, qr.Q * qr.R));
      qr = new QRDecompositionD(a.Transposed);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a.Transposed, qr.Q * qr.R));

      a = new MatrixD(new double[,] { { 1, 2 }, { 1, 2 }, { 1, 2 } });
      qr = new QRDecompositionD(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, qr.Q * qr.R));

      MatrixD h = qr.H;  // Just call this one to see if it runs through.
    }


    [Test]
    public void TestMatricesWithFullRank()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      QRDecompositionD qr = new QRDecompositionD(a);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, qr.Q * qr.R));
      qr = new QRDecompositionD(a.Transposed);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a.Transposed, qr.Q * qr.R));

      a = new MatrixD(new double[,] { { 1, 2 }, { 4, 5 }, { 4, 5 } });
      qr = new QRDecompositionD(a);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, qr.Q * qr.R));

      MatrixD h = qr.H;  // Just call this one to see if it runs through.
    }


    [Test]
    public void TestRandomRegularA()
    {
      RandomHelper.Random = new Random(1);

      for (int i = 0; i < 100; i++)
      {
        VectorD column1 = new VectorD(3);
        RandomHelper.Random.NextVectorD(column1, 1, 2);
        
        VectorD column2 = new VectorD(3);
        RandomHelper.Random.NextVectorD(column2, 1, 2);

        // Make linearly independent.
        if (column1 / column1[0] == column2 / column2[0])
          column2[0]++;

        // Create linearly independent third column.
        VectorD column3 = column1 + column2;
        column3[1]++;

        // Create A.
        MatrixD a = new MatrixD(3, 3);
        a.SetColumn(0, column1);
        a.SetColumn(1, column2);
        a.SetColumn(2, column3);

        QRDecompositionD d = new QRDecompositionD(a);
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.
        Assert.AreEqual(true, d.HasNumericallyFullRank);

        // Check solving of linear equations.
        MatrixD b = new MatrixD(3, 2);
        RandomHelper.Random.NextMatrixD(b, 0, 1);

        MatrixD x = d.SolveLinearEquations(b);
        MatrixD b2 = a * x;
        Assert.IsTrue(MatrixD.AreNumericallyEqual(b, b2, 0.01f));

        MatrixD h = d.H;  // Just call this one to see if it runs through.
        h = d.H; // Call it secont time to cover code with internal caching.
      }
    }


    [Test]
    public void TestRandomSquareA()
    {
      RandomHelper.Random = new Random(1);

      for (int i = 0; i < 100; i++)
      {
        // Create A.
        MatrixD a = new MatrixD(3, 3);
        RandomHelper.Random.NextMatrixD(a, 0, 1);

        QRDecompositionD d = new QRDecompositionD(a);
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.
        
        // Check solving of linear equations.        
        if (d.HasNumericallyFullRank)
        {
          MatrixD b = new MatrixD(3, 2);
          RandomHelper.Random.NextMatrixD(b, 0, 1);
          MatrixD x = d.SolveLinearEquations(b);
          MatrixD b2 = a * x;
          Assert.IsTrue(MatrixD.AreNumericallyEqual(b, b2, 0.01f));
        }

        MatrixD h = d.H;  // Just call this one to see if it runs through.
        h = d.H; // Call it secont time to cover code with internal caching.
      }
    }


    [Test]
    public void TestRandomRectangularA()
    {
      RandomHelper.Random = new Random(1);

      // Every transpose(A) * A is SPD if A has full column rank and m>n.
      for (int i = 0; i < 100; i++)
      {
        // Create A.
        MatrixD a = new MatrixD(10, 3);
        RandomHelper.Random.NextMatrixD(a, 0, 1);

        QRDecompositionD d = new QRDecompositionD(a);
        MatrixD b = new MatrixD(10, 1);
        RandomHelper.Random.NextMatrixD(b, 0, 1);
        MatrixD x = d.SolveLinearEquations(b);
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.


        // Check solving of linear equations.        
        if (d.HasNumericallyFullRank)
        {
          // Compare with Least squares solution (Gauss-Transformation and Cholesky).
          MatrixD spdMatrix = a.Transposed * a;
          CholeskyDecompositionD ch = new CholeskyDecompositionD(spdMatrix);
          MatrixD x2 = ch.SolveLinearEquations(a.Transposed * b);

          Assert.IsTrue(MatrixD.AreNumericallyEqual(x, x2, 0.0001f));
        }

        MatrixD h = d.H;  // Just call this one to see if it runs through.
      }
    }



    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      QRDecompositionD decomp = new QRDecompositionD(a);
      decomp.SolveLinearEquations(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixD b = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

      QRDecompositionD decomp = new QRDecompositionD(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException3()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 1, 2, 3 } });
      MatrixD b = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, {7, 8, -9 } });

      QRDecompositionD decomp = new QRDecompositionD(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixD a = new MatrixD(new[,] {{ 0, 1, 2 }, 
                                      { 1, 4, 3 },
                                      { 5, 3, double.NaN}});

      // Any result is ok. We must only check for infinite loops!
      var d = new QRDecompositionD(a);
      d = new QRDecompositionD(new MatrixD(4, 3, double.NaN));
    }
  }
}
