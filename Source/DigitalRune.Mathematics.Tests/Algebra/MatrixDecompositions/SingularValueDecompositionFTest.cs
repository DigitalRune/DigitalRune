using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class SingularValueDecompositionFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new SingularValueDecompositionF(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new SingularValueDecompositionF(new MatrixF(2, 3));  // more columns than rows.
    }


    [Test]
    public void TestMatricesWithoutFullRank()
    {
      MatrixF a = new MatrixF(3, 3);
      SingularValueDecompositionF svd = new SingularValueDecompositionF(a);
      Assert.AreEqual(0, svd.NumericalRank);
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);      
      float condNumber = svd.ConditionNumber;  

      a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 4, 5, 6 } });
      svd = new SingularValueDecompositionF(a);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      svd = new SingularValueDecompositionF(a.Transposed);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed)); // Repeat to test with cached values.
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
      
      a = new MatrixF(new float[,] { { 1, 2 }, { 1, 2 }, { 1, 2 } });
      svd = new SingularValueDecompositionF(a);
      Assert.AreEqual(1, svd.NumericalRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
    }


    [Test]
    public void TestMatricesWithFullRank()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      SingularValueDecompositionF svd = new SingularValueDecompositionF(a);
      Assert.AreEqual(3, svd.NumericalRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      float condNumber = svd.ConditionNumber;
      svd = new SingularValueDecompositionF(a.Transposed);
      Assert.AreEqual(3, svd.NumericalRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;

      a = new MatrixF(new float[,] { { 1, 2 }, { 4, 5 }, { 4, 5 } });
      svd = new SingularValueDecompositionF(a);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
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

        SingularValueDecompositionF svd = new SingularValueDecompositionF(a);
        Assert.AreEqual(3, svd.NumericalRank);
        Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        float condNumber = svd.ConditionNumber;
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

        // Check for full rank with QRD.
        QRDecompositionF d = new QRDecompositionF(a);

        SingularValueDecompositionF svd = new SingularValueDecompositionF(a);
        if (d.HasNumericallyFullRank)
        {
          // Rank should be full.          
          Assert.AreEqual(3, svd.NumericalRank);
          Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        }
        else
        {
          // Not full rank - we dont know much, just see if it runs through
          Assert.Greater(3, svd.NumericalRank);
          Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        }
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        float condNumber = svd.ConditionNumber;
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
        MatrixF a = new MatrixF(4, 3);
        RandomHelper.Random.NextMatrixF(a, 0, 1);

        // Check for full rank with QRD.
        QRDecompositionF d = new QRDecompositionF(a);

        SingularValueDecompositionF svd = new SingularValueDecompositionF(a);
        if (d.HasNumericallyFullRank)
        {
           // Rank should be full.          
          Assert.AreEqual(3, svd.NumericalRank);
          Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));          
        }
        else
        {
          // Not full rank - we dont know much, just see if it runs through
          Assert.Greater(3, svd.NumericalRank);
          Assert.IsTrue(MatrixF.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));          
        }
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        float condNumber = svd.ConditionNumber;
      }
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixF a = new MatrixF(new[,] {{ 0, float.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      var d = new SingularValueDecompositionF(a);
      foreach (var element in d.SingularValues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.U.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);

      d = new SingularValueDecompositionF(new MatrixF(4, 3, float.NaN));
      foreach (var element in d.SingularValues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.U.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }
  }
}
