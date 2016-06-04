using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class LUDecompositionDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      LUDecompositionD d = new LUDecompositionD(null);
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

        LUDecompositionD d = new LUDecompositionD(a);

        MatrixD aPermuted = d.L * d.U;        
        Assert.IsTrue(MatrixD.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));
        aPermuted = d.L * d.U; // Repeat with to test cached values.
        Assert.IsTrue(MatrixD.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));

        Assert.AreEqual(false, d.IsNumericallySingular);

        // Check solving of linear equations.
        MatrixD b = new MatrixD(3, 2);
        RandomHelper.Random.NextMatrixD(b, 0, 1);

        MatrixD x = d.SolveLinearEquations(b);
        MatrixD b2 = a * x;
        Assert.IsTrue(MatrixD.AreNumericallyEqual(b, b2, 0.01f));        
      }
    }


    [Test]
    public void TestRectangularA()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
      a.Transpose();
      LUDecompositionD d = new LUDecompositionD(a);
      Assert.IsFalse(d.IsNumericallySingular);
    }


    [Test]
    public void TestRandomA()
    {
      RandomHelper.Random = new Random(1);

      for (int i = 0; i < 100; i++)
      {
        // Create A.
        MatrixD a = new MatrixD(3, 3);
        RandomHelper.Random.NextMatrixD(a, 0, 1);

        LUDecompositionD d = new LUDecompositionD(a);

        if (d.IsNumericallySingular == false)
        {
          // Check solving of linear equations.
          MatrixD b = new MatrixD(3, 2);
          RandomHelper.Random.NextMatrixD(b, 0, 1);

          MatrixD x = d.SolveLinearEquations(b);
          MatrixD b2 = a * x;
          Assert.IsTrue(MatrixD.AreNumericallyEqual(b, b2, 0.01));

          MatrixD aPermuted = d.L * d.U;
          Assert.IsTrue(MatrixD.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));
        }        
      }      
    }


    [Test]
    public void TestSingularMatrix()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } });

      LUDecompositionD d = new LUDecompositionD(a);
      Assert.AreEqual(true, d.IsNumericallySingular);
      Assert.IsTrue(Numeric.IsZero(d.Determinant));
    }


    [Test]
    public void Determinant()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 0, 1, 2, 0 }, { 1, 0, 1, 0 } });

      LUDecompositionD d = new LUDecompositionD(a);
      Assert.AreEqual(false, d.IsNumericallySingular);
      Assert.IsTrue(Numeric.AreEqual(-24, d.Determinant));

      MatrixD aPermuted = d.L * d.U;
      Assert.IsTrue(MatrixD.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 3)));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2 }, { 5, 6 }, { 0, 1 } });

      LUDecompositionD d = new LUDecompositionD(a);
      double det = d.Determinant;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMatrixFormat()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
      LUDecompositionD d = new LUDecompositionD(a);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2 }, { 5, 6 }, { 0, 1 } });
      LUDecompositionD d = new LUDecompositionD(a);
      d.SolveLinearEquations(null);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixD a = new MatrixD(new[,] {{ 0, 1, 2 }, 
                                      { 1, 4, 3 },
                                      { 5, 3, double.NaN}});

      // Any result is ok. We must only check for infinite loops!
      var d = new LUDecompositionD(a);
      d = new LUDecompositionD(new MatrixD(4, 3, double.NaN));
    }
  }
}
