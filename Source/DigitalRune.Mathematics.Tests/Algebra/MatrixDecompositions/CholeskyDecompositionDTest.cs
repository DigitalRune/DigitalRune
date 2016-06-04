using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class CholeskyDecompositionDTest
  {

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new CholeskyDecompositionD(null);
    }    



    [Test]
    public void Test1()
    {
      MatrixD a = new MatrixD(new double[,] { { 2, -1, 0}, 
                                             { -1, 2, -1}, 
                                             { 0, -1, 2} });

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);

      Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

      MatrixD l = d.L;

      Assert.AreEqual(0, l[0, 1]);
      Assert.AreEqual(0, l[0, 2]);
      Assert.AreEqual(0, l[1, 2]);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, l * l.Transposed));

      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, l * l.Transposed));

      // Check solving of linear equations.
      MatrixD x = new MatrixD(new double[,] { { 1, 2}, 
                                             { 3, 4}, 
                                             { 5, 6} });
      MatrixD b = a * x;

      Assert.IsTrue(MatrixD.AreNumericallyEqual(x, d.SolveLinearEquations(b)));
    }


    [Test]
    public void Test2()
    {
      // Every transpose(A)*A is SPD if A has full column rank and m > n.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3}, 
                                             { 4, 5, 6}, 
                                             { 7, 8, 9},
                                             { 10, 11, -12}});
      //MatrixD a = new MatrixD(new double[,] { { 1, 2}, 
      //                                       { 4, 5}, 
      //                                       { 7, 8}});

      a = a.Transposed * a;

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);

      Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

      MatrixD l = d.L;

      Assert.AreEqual(0, l[0, 1]);
      Assert.AreEqual(0, l[0, 2]);
      Assert.AreEqual(0, l[1, 2]);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, l * l.Transposed));
    }


    [Test]
    public void TestEmptyMatrix()
    {
      MatrixD a = new MatrixD(2, 2);

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);

      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestRectangularA()
    {
      MatrixD a = new MatrixD(new double[,] { { 2, -1}, 
                                             { -1, 2}, 
                                             { 0, -1} });

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestNonSymmetricA()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3}, 
                                             { 2, 2, 2}, 
                                             { 0, 0, 0} });

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestNonSpdA()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3}, 
                                             { 2, 5, 6}, 
                                             { 3, 6, 4} });

      CholeskyDecompositionD d = new CholeskyDecompositionD(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestRandomSpdA()
    {
      RandomHelper.Random = new Random(1);

      // Every transpose(A) * A is SPD if A has full column rank and m>n.
      for (int i = 0; i < 100; i++)
      {        
        VectorD column1 = new VectorD(4);
        RandomHelper.Random.NextVectorD(column1, 1, 2);
        VectorD column2 = new VectorD(4);
        RandomHelper.Random.NextVectorD(column2, 1, 2);

        // Make linearly independent.
        if (column1 / column1[0] == column2 / column2[0])
          column2[0]++;

        // Create linearly independent third column.
        VectorD column3 = column1 + column2;
        column3[1]++;

        // Create A.
        MatrixD a = new MatrixD(4, 3);
        a.SetColumn(0, column1);
        a.SetColumn(1, column2);
        a.SetColumn(2, column3);

        MatrixD spdMatrix = a.Transposed * a;

        CholeskyDecompositionD d = new CholeskyDecompositionD(spdMatrix);

        Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

        MatrixD l = d.L;

        // Test if L is a lower triangular matrix.
        for (int j = 0; j < l.NumberOfRows; j++)
          for (int k = 0; k < l.NumberOfColumns; k++)
            if (j < k)
              Assert.AreEqual(0, l[j, k]);

        Assert.IsTrue(MatrixD.AreNumericallyEqual(spdMatrix, l * l.Transposed));

        // Check solving of linear equations.
        MatrixD b = new MatrixD(3, 2);
        RandomHelper.Random.NextMatrixD(b, 0, 1);

        MatrixD x = d.SolveLinearEquations(b);
        MatrixD b2 = spdMatrix * x;
        Assert.IsTrue(MatrixD.AreNumericallyEqual(b, b2, 0.01f));
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      CholeskyDecompositionD decomp = new CholeskyDecompositionD(a);
      decomp.SolveLinearEquations(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixD b = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

      CholeskyDecompositionD decomp = new CholeskyDecompositionD(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException3()
    {
      // Create A.
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 1, 2, 3 } });
      MatrixD b = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      CholeskyDecompositionD decomp = new CholeskyDecompositionD(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixD a = new MatrixD(new[,] {{ 0, 1, 2 }, 
                                      { 1, 4, 3 },
                                      { 5, 3, double.NaN}});

      // Any result is ok. We must only check for infinite loops!
      var d = new CholeskyDecompositionD(a);
      d = new CholeskyDecompositionD(new MatrixD(4, 4, double.NaN));
    }
  }
}
