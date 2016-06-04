using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class QRDecompositionFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new QRDecompositionF(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new QRDecompositionF(new MatrixF(2, 3));  // more columns than rows.
    }


    [Test]
    public void TestMatricesWithoutFullRank()
    {
      MatrixF a = new MatrixF(3, 3);
      QRDecompositionF qr = new QRDecompositionF(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);

      a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 4, 5, 6 } });
      qr = new QRDecompositionF(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, qr.Q * qr.R));
      qr = new QRDecompositionF(a.Transposed);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a.Transposed, qr.Q * qr.R));

      a = new MatrixF(new float[,] { { 1, 2 }, { 1, 2 }, { 1, 2 } });
      qr = new QRDecompositionF(a);
      Assert.AreEqual(false, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, qr.Q * qr.R));

      MatrixF h = qr.H;  // Just call this one to see if it runs through.
    }


    [Test]
    public void TestMatricesWithFullRank()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      QRDecompositionF qr = new QRDecompositionF(a);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, qr.Q * qr.R));
      qr = new QRDecompositionF(a.Transposed);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a.Transposed, qr.Q * qr.R));

      a = new MatrixF(new float[,] { { 1, 2 }, { 4, 5 }, { 4, 5 } });
      qr = new QRDecompositionF(a);
      Assert.AreEqual(true, qr.HasNumericallyFullRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, qr.Q * qr.R));

      MatrixF h = qr.H;  // Just call this one to see if it runs through.
    }


    [Test]
    public void TestRandomRegularA()
    {
      RandomHelper.Random = new Random(1);

      for (int i = 0; i < 100; i++)
      {
        VectorF column1 = new VectorF(3);
        RandomHelper.Random.NextVectorF(column1, 1, 2);
        VectorF column2 = new VectorF(3);
        RandomHelper.Random.NextVectorF(column2, 1, 2);

        // Make linearly independent.
        if (column1 / column1[0] == column2 / column2[0])
          column2[0]++;

        // Create linearly independent third column.
        VectorF column3 = column1 + column2;
        column3[1]++;

        // Create A.
        MatrixF a = new MatrixF(3, 3);
        a.SetColumn(0, column1);
        a.SetColumn(1, column2);
        a.SetColumn(2, column3);

        QRDecompositionF d = new QRDecompositionF(a);
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.
        Assert.AreEqual(true, d.HasNumericallyFullRank);

        // Check solving of linear equations.
        MatrixF b = new MatrixF(3, 2);
        RandomHelper.Random.NextMatrixF(b, 0, 1);

        MatrixF x = d.SolveLinearEquations(b);
        MatrixF b2 = a * x;
        Assert.IsTrue(MatrixF.AreNumericallyEqual(b, b2, 0.01f));

        MatrixF h = d.H;  // Just call this one to see if it runs through.
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
        MatrixF a = new MatrixF(3, 3);
        RandomHelper.Random.NextMatrixF(a, 0, 1);

        QRDecompositionF d = new QRDecompositionF(a);
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.
        
        // Check solving of linear equations.        
        if (d.HasNumericallyFullRank)
        {
          MatrixF b = new MatrixF(3, 2);
          RandomHelper.Random.NextMatrixF(b, 0, 1);
          MatrixF x = d.SolveLinearEquations(b);
          MatrixF b2 = a * x;
          Assert.IsTrue(MatrixF.AreNumericallyEqual(b, b2, 0.01f));
        }

        MatrixF h = d.H;  // Just call this one to see if it runs through.
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
        MatrixF a = new MatrixF(10, 3);
        RandomHelper.Random.NextMatrixF(a, 0, 1);

        QRDecompositionF d = new QRDecompositionF(a);
        MatrixF b = new MatrixF(10, 1);
        RandomHelper.Random.NextMatrixF(b, 0, 1);
        MatrixF x = d.SolveLinearEquations(b);
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R));
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.Q * d.R)); // Second time with the cached values.


        // Check solving of linear equations.        
        if (d.HasNumericallyFullRank)
        {
          // Compare with Least squares solution (Gauss-Transformation and Cholesky).
          MatrixF spdMatrix = a.Transposed * a;
          CholeskyDecompositionF ch = new CholeskyDecompositionF(spdMatrix);
          MatrixF x2 = ch.SolveLinearEquations(a.Transposed * b);

          Assert.IsTrue(MatrixF.AreNumericallyEqual(x, x2, 0.0001f));
        }

        MatrixF h = d.H;  // Just call this one to see if it runs through.
      }
    }



    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      QRDecompositionF decomp = new QRDecompositionF(a);
      decomp.SolveLinearEquations(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixF b = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });

      QRDecompositionF decomp = new QRDecompositionF(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException3()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 1, 2, 3 } });
      MatrixF b = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, {7, 8, -9 } });

      QRDecompositionF decomp = new QRDecompositionF(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixF a = new MatrixF(new[,] {{ 0, float.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      // Any result is ok. We must only check for infinite loops!
      var d = new QRDecompositionF(a);
      d = new QRDecompositionF(new MatrixF(4, 3, float.NaN));
    }
  }
}
