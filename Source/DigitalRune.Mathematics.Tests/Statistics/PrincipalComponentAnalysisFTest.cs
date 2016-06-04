using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class PrincipalComponentsAnalysisFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ArgumentNull()
    {
      new PrincipalComponentAnalysisF(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void EmptyList()
    {
      new PrincipalComponentAnalysisF(new List<VectorF>());
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void VectorsOfDifferentLength()
    {
      List<VectorF> points = new List<VectorF>(new[]
      {
        new VectorF(new float[] { -1, -2, 1 }),
        new VectorF(new float[] { 2, -1, 3 }),
        new VectorF(new float[] { 2, -1, 2, 3 }),
      });
      new PrincipalComponentAnalysisF(points);
    }


    [Test]
    public void Test()
    {
      // Make a random list.
      RandomHelper.Random = new Random(77);
      List<VectorF> points = new List<VectorF>();
      for (int i = 0; i < 10; i++)
      {
        var vector = new VectorF(4);
        RandomHelper.Random.NextVectorF(vector, -1, 10);
        points.Add(vector);
      }

      PrincipalComponentAnalysisF pca = new PrincipalComponentAnalysisF(points);

      Assert.Greater(pca.Variances[0], pca.Variances[1]);
      Assert.Greater(pca.Variances[1], pca.Variances[2]);
      Assert.Greater(pca.Variances[2], pca.Variances[3]);
      Assert.Greater(pca.Variances[3], 0);

      Assert.IsTrue(pca.V.GetColumn(0).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(1).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(2).IsNumericallyNormalized);
      Assert.IsTrue(pca.V.GetColumn(3).IsNumericallyNormalized);

      // Compute covariance matrix and check if it is diagonal in the transformed space.
      MatrixF cov = StatisticsHelper.ComputeCovarianceMatrix(points);
      MatrixF transformedCov = pca.V.Transposed * cov * pca.V;

      for (int row = 0; row < transformedCov.NumberOfRows; row++)
        for (int column = 0; column < transformedCov.NumberOfColumns; column++)
          if (row != column)
            Assert.IsTrue(Numeric.IsZero(transformedCov[row, column]));

      // The principal components must be Eigenvectors which means that multiplying with the covariance
      // matrix does only change the length!      
      VectorF v0 = pca.V.GetColumn(0);
      VectorF v0Result = cov * v0;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v0.Normalized, v0Result.Normalized));
      VectorF v1 = pca.V.GetColumn(1);
      VectorF v1Result = cov * v1;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v1.Normalized, v1Result.Normalized));
      VectorF v2 = pca.V.GetColumn(2);
      VectorF v2Result = cov * v2;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v2.Normalized, v2Result.Normalized));
      VectorF v3 = pca.V.GetColumn(3);
      VectorF v3Result = cov * v3;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v3.Normalized, v3Result.Normalized));
    }
  }
}
