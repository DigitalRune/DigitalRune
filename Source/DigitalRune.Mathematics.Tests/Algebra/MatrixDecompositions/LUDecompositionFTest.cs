using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class LUDecompositionFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      LUDecompositionF d = new LUDecompositionF(null);
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

        LUDecompositionF d = new LUDecompositionF(a);

        MatrixF aPermuted = d.L * d.U;        
        Assert.IsTrue(MatrixF.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));
        aPermuted = d.L * d.U; // Repeat with to test cached values.
        Assert.IsTrue(MatrixF.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));

        Assert.AreEqual(false, d.IsNumericallySingular);

        // Check solving of linear equations.
        MatrixF b = new MatrixF(3, 2);
        RandomHelper.Random.NextMatrixF(b, 0, 1);

        MatrixF x = d.SolveLinearEquations(b);
        MatrixF b2 = a * x;
        Assert.IsTrue(MatrixF.AreNumericallyEqual(b, b2, 0.01f));        
      }
    }


    [Test]
    public void TestRectangularA()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });
      a.Transpose();
      LUDecompositionF d = new LUDecompositionF(a);
      Assert.IsFalse(d.IsNumericallySingular);
    }


    [Test]
    public void TestRandomA()
    {
      RandomHelper.Random = new Random(1);

      for (int i = 0; i < 100; i++)
      {
        // Create A.
        MatrixF a = new MatrixF(3, 3);
        RandomHelper.Random.NextMatrixF(a, 0, 1);

        LUDecompositionF d = new LUDecompositionF(a);

        if (d.IsNumericallySingular == false)
        {
          // Check solving of linear equations.
          MatrixF b = new MatrixF(3, 2);
          RandomHelper.Random.NextMatrixF(b, 0, 1);

          MatrixF x = d.SolveLinearEquations(b);
          MatrixF b2 = a * x;
          Assert.IsTrue(MatrixF.AreNumericallyEqual(b, b2, 0.01f));

          MatrixF aPermuted = d.L * d.U;
          Assert.IsTrue(MatrixF.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 2)));
        }        
      }      
    }


    [Test]
    public void TestSingularMatrix()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } });

      LUDecompositionF d = new LUDecompositionF(a);
      Assert.AreEqual(true, d.IsNumericallySingular);
      Assert.IsTrue(Numeric.IsZero(d.Determinant));
    }


    [Test]
    public void Determinant()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 0, 1, 2, 0 }, { 1, 0, 1, 0 } });

      LUDecompositionF d = new LUDecompositionF(a);
      Assert.AreEqual(false, d.IsNumericallySingular);
      Assert.IsTrue(Numeric.AreEqual(-24, d.Determinant));

      MatrixF aPermuted = d.L * d.U;
      Assert.IsTrue(MatrixF.AreNumericallyEqual(aPermuted, a.GetSubmatrix(d.PivotPermutationVector, 0, 3)));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2 }, { 5, 6 }, { 0, 1 } });

      LUDecompositionF d = new LUDecompositionF(a);
      float det = d.Determinant;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMatrixFormat()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });
      LUDecompositionF d = new LUDecompositionF(a);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2 }, { 5, 6 }, { 0, 1 } });
      LUDecompositionF d = new LUDecompositionF(a);
      d.SolveLinearEquations(null);
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixF a = new MatrixF(new[,] {{ 0, float.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      // Any result is ok. We must only check for infinite loops!
      var d = new LUDecompositionF(a);
      d = new LUDecompositionF(new MatrixF(4, 3, float.NaN));
    }
  }
}
