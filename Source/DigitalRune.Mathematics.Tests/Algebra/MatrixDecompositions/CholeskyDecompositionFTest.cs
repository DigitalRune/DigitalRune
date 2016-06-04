using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class CholeskyDecompositionFTest
  {

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new CholeskyDecompositionF(null);
    }    



    [Test]
    public void Test1()
    {
      MatrixF a = new MatrixF(new float[,] { { 2, -1, 0}, 
                                             { -1, 2, -1}, 
                                             { 0, -1, 2} });

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);

      Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

      MatrixF l = d.L;

      Assert.AreEqual(0, l[0, 1]);
      Assert.AreEqual(0, l[0, 2]);
      Assert.AreEqual(0, l[1, 2]);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, l * l.Transposed));

      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, l * l.Transposed));

      // Check solving of linear equations.
      MatrixF x = new MatrixF(new float[,] { { 1, 2}, 
                                             { 3, 4}, 
                                             { 5, 6} });
      MatrixF b = a * x;

      Assert.IsTrue(MatrixF.AreNumericallyEqual(x, d.SolveLinearEquations(b)));
    }


    [Test]
    public void Test2()
    {
      // Every transpose(A)*A is SPD if A has full column rank and m > n.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3}, 
                                             { 4, 5, 6}, 
                                             { 7, 8, 9},
                                             { 10, 11, -12}});
      //MatrixF a = new MatrixF(new float[,] { { 1, 2}, 
      //                                       { 4, 5}, 
      //                                       { 7, 8}});

      a = a.Transposed * a;

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);

      Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

      MatrixF l = d.L;

      Assert.AreEqual(0, l[0, 1]);
      Assert.AreEqual(0, l[0, 2]);
      Assert.AreEqual(0, l[1, 2]);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, l * l.Transposed));
    }


    [Test]
    public void TestEmptyMatrix()
    {
      MatrixF a = new MatrixF(2, 2);

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);

      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestRectangularA()
    {
      MatrixF a = new MatrixF(new float[,] { { 2, -1}, 
                                             { -1, 2}, 
                                             { 0, -1} });

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestNonSymmetricA()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3}, 
                                             { 2, 2, 2}, 
                                             { 0, 0, 0} });

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestNonSpdA()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3}, 
                                             { 2, 5, 6}, 
                                             { 3, 6, 4} });

      CholeskyDecompositionF d = new CholeskyDecompositionF(a);
      Assert.AreEqual(false, d.IsSymmetricPositiveDefinite);
    }


    [Test]
    public void TestRandomSpdA()
    {
      RandomHelper.Random = new Random(1);

      // Every transpose(A) * A is SPD if A has full column rank and m>n.
      for (int i = 0; i < 100; i++)
      {        
        VectorF column1 = new VectorF(4);
        RandomHelper.Random.NextVectorF(column1, 1, 2);
        VectorF column2 = new VectorF(4);
        RandomHelper.Random.NextVectorF(column2, 1, 2);

        // Make linearly independent.
        if (column1 / column1[0] == column2 / column2[0])
          column2[0]++;

        // Create linearly independent third column.
        VectorF column3 = column1 + column2;
        column3[1]++;

        // Create A.
        MatrixF a = new MatrixF(4, 3);
        a.SetColumn(0, column1);
        a.SetColumn(1, column2);
        a.SetColumn(2, column3);

        MatrixF spdMatrix = a.Transposed * a;

        CholeskyDecompositionF d = new CholeskyDecompositionF(spdMatrix);

        Assert.AreEqual(true, d.IsSymmetricPositiveDefinite);

        MatrixF l = d.L;

        // Test if L is a lower triangular matrix.
        for (int j = 0; j < l.NumberOfRows; j++)
          for (int k = 0; k < l.NumberOfColumns; k++)
            if (j < k)
              Assert.AreEqual(0, l[j, k]);

        Assert.IsTrue(MatrixF.AreNumericallyEqual(spdMatrix, l * l.Transposed));

        // Check solving of linear equations.
        MatrixF b = new MatrixF(3, 2);
        RandomHelper.Random.NextMatrixF(b, 0, 1);

        MatrixF x = d.SolveLinearEquations(b);
        MatrixF b2 = spdMatrix * x;
        Assert.IsTrue(MatrixF.AreNumericallyEqual(b, b2, 0.01f));
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      CholeskyDecompositionF decomp = new CholeskyDecompositionF(a);
      decomp.SolveLinearEquations(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixF b = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });

      CholeskyDecompositionF decomp = new CholeskyDecompositionF(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException3()
    {
      // Create A.
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 1, 2, 3 } });
      MatrixF b = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });

      CholeskyDecompositionF decomp = new CholeskyDecompositionF(a);
      decomp.SolveLinearEquations(b);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixF a = new MatrixF(new[,] {{ 0, float.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      // Any result is ok. We must only check for infinite loops!
      var d = new CholeskyDecompositionF(a);
      d = new CholeskyDecompositionF(new MatrixF(4, 3, float.NaN));
    }
  }
}
