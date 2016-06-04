using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;

namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class PrincipalComponentsAnalysisDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNull()
    {
      new PrincipalComponentAnalysisD(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void EmptyList()
    {
      new PrincipalComponentAnalysisD(new List<VectorD>());
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void VectorsOfDifferentLength()
    {
      List<VectorD> points = new List<VectorD>(new[]
      {
        new VectorD(new double[] { -1, -2, 1 }),
        new VectorD(new double[] { 2, -1, 3 }),
        new VectorD(new double[] { 2, -1, 2, 3 }),
      });
      new PrincipalComponentAnalysisD(points);
    }


    [Test]
    public void Test()
    {
      // Make a random list.
      RandomHelper.Random = new Random(77);
      List<VectorD> points = new List<VectorD>();
      for (int i = 0; i < 10; i++)
      {
        var vector = new VectorD(4);
        RandomHelper.Random.NextVectorD(vector, -1, 10);
        points.Add(vector);
      }

      PrincipalComponentAnalysisD pca = new PrincipalComponentAnalysisD(points);

      Assert.Greater(pca.Variances[0], pca.Variances[1]);
      Assert.Greater(pca.Variances[1], pca.Variances[2]);
      Assert.Greater(pca.Variances[2], pca.Variances[3]);
      Assert.Greater(pca.Variances[3], 0);

      Assert.IsTrue(pca.V.GetColumn(0).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(1).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(2).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(3).IsNumericallyNormalized);

      // Compute covariance matrix and check if it is diagonal in the transformed space.
      MatrixD cov = StatisticsHelper.ComputeCovarianceMatrix(points);
      MatrixD transformedCov = pca.V.Transposed * cov * pca.V;

      for (int row = 0; row < transformedCov.NumberOfRows; row++)
        for (int column = 0; column < transformedCov.NumberOfColumns; column++)
          if (row != column)
            Assert.IsTrue(Numeric.IsZero(transformedCov[row, column]));

      // The principal components must be Eigenvectors which means that multiplying with the covariance
      // matrix does only change the length!      
      VectorD v0 = pca.V.GetColumn(0);
      VectorD v0Result = cov * v0;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v0.Normalized, v0Result.Normalized));
      VectorD v1 = pca.V.GetColumn(1);
      VectorD v1Result = cov * v1;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v1.Normalized, v1Result.Normalized));
      VectorD v2 = pca.V.GetColumn(2);
      VectorD v2Result = cov * v2;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v2.Normalized, v2Result.Normalized));
      VectorD v3 = pca.V.GetColumn(3);
      VectorD v3Result = cov * v3;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v3.Normalized, v3Result.Normalized));
    }
  }
}
