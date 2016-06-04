using System;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class SingularValueDecompositionDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new SingularValueDecompositionD(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new SingularValueDecompositionD(new MatrixD(2, 3));  // more columns than rows.
    }


    [Test]
    public void TestMatricesWithoutFullRank()
    {
      MatrixD a = new MatrixD(3, 3);
      SingularValueDecompositionD svd = new SingularValueDecompositionD(a);
      Assert.AreEqual(0, svd.NumericalRank);
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);      
      double condNumber = svd.ConditionNumber;  

      a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 4, 5, 6 } });
      svd = new SingularValueDecompositionD(a);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      svd = new SingularValueDecompositionD(a.Transposed);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed)); // Repeat to test with cached values.
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
      
      a = new MatrixD(new double[,] { { 1, 2 }, { 1, 2 }, { 1, 2 } });
      svd = new SingularValueDecompositionD(a);
      Assert.AreEqual(1, svd.NumericalRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
    }


    [Test]
    public void TestMatricesWithFullRank()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      SingularValueDecompositionD svd = new SingularValueDecompositionD(a);
      Assert.AreEqual(3, svd.NumericalRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      double condNumber = svd.ConditionNumber;
      svd = new SingularValueDecompositionD(a.Transposed);
      Assert.AreEqual(3, svd.NumericalRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a.Transposed, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;

      a = new MatrixD(new double[,] { { 1, 2 }, { 4, 5 }, { 4, 5 } });
      svd = new SingularValueDecompositionD(a);
      Assert.AreEqual(2, svd.NumericalRank);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
      Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
      condNumber = svd.ConditionNumber;
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

        SingularValueDecompositionD svd = new SingularValueDecompositionD(a);
        Assert.AreEqual(3, svd.NumericalRank);
        Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        double condNumber = svd.ConditionNumber;
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

        // Check for full rank with QRD.
        QRDecompositionD d = new QRDecompositionD(a);

        SingularValueDecompositionD svd = new SingularValueDecompositionD(a);
        if (d.HasNumericallyFullRank)
        {
          // Rank should be full.          
          Assert.AreEqual(3, svd.NumericalRank);
          Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        }
        else
        {
          // Not full rank - we dont know much, just see if it runs through
          Assert.Greater(3, svd.NumericalRank);
          Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));
        }
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        double condNumber = svd.ConditionNumber;
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
        MatrixD a = new MatrixD(4, 3);
        RandomHelper.Random.NextMatrixD(a, 0, 1);

        // Check for full rank with QRD.
        QRDecompositionD d = new QRDecompositionD(a);

        SingularValueDecompositionD svd = new SingularValueDecompositionD(a);
        if (d.HasNumericallyFullRank)
        {
           // Rank should be full.          
          Assert.AreEqual(3, svd.NumericalRank);
          Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));          
        }
        else
        {
          // Not full rank - we dont know much, just see if it runs through
          Assert.Greater(3, svd.NumericalRank);
          Assert.IsTrue(MatrixD.AreNumericallyEqual(a, svd.U * svd.S * svd.V.Transposed));          
        }
        Assert.AreEqual(svd.SingularValues[0], svd.Norm2);
        double condNumber = svd.ConditionNumber;
      }
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixD a = new MatrixD(new[,] {{ 0, double.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      var d = new SingularValueDecompositionD(a);
      foreach (var element in d.SingularValues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.U.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);

      d = new SingularValueDecompositionD(new MatrixD(4, 3, double.NaN));
      foreach (var element in d.SingularValues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.U.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }
  }
}
