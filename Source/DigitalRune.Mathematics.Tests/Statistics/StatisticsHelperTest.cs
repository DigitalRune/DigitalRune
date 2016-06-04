using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class StatisticsHelperTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeCovarianceMatrix3FWithArgumentNull()
    {
      StatisticsHelper.ComputeCovarianceMatrix((List<Vector3F>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeCovarianceMatrix3DWithArgumentNull()
    {
      StatisticsHelper.ComputeCovarianceMatrix((List<Vector3D>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeCovarianceMatrixFWithArgumentNull()
    {
      StatisticsHelper.ComputeCovarianceMatrix((List<VectorF>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComputeCovarianceMatrixDWithArgumentNull()
    {
      StatisticsHelper.ComputeCovarianceMatrix((List<VectorD>)null);
    }


    [Test]
    public void ComputeCovarianceMatrix3FWithEmptyList()
    {
      var result = StatisticsHelper.ComputeCovarianceMatrix(new List<Vector3F>());
      foreach (var element in result.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }


    [Test]
    public void ComputeCovarianceMatrix3DWithEmptyList()
    {
      var result = StatisticsHelper.ComputeCovarianceMatrix(new List<Vector3D>());
      foreach (var element in result.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeCovarianceMatrixFWithEmptyList()
    {
      var result = StatisticsHelper.ComputeCovarianceMatrix(new List<VectorF>());
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeCovarianceMatrixDWithEmptyList()
    {
      var result = StatisticsHelper.ComputeCovarianceMatrix(new List<VectorD>());
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeCovarianceMatrixFWithVectorsOfDifferentLength()
    {
      List<VectorF> points = new List<VectorF>(new[]
      {
        new VectorF(new float[] { -1, -2, 1 }),
        new VectorF(new float[] { 2, -1, 3 }),
        new VectorF(new float[] { 2, -1, 2, 3 }),
      }); 
      var result = StatisticsHelper.ComputeCovarianceMatrix(points);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComputeCovarianceMatrixDWithVectorsOfDifferentLength()
    {
      List<VectorD> points = new List<VectorD>(new[]
      {
        new VectorD(new double[] { -1, -2, 1 }),
        new VectorD(new double[] { 2, -1, 3, 1}),
        new VectorD(new double[] { 2, -1, 2 }),
      });
      var result = StatisticsHelper.ComputeCovarianceMatrix(points);
    }



    [Test]
    public void ComputeCovarianceMatrix3F()
    {
      // Make a random list.
      List<Vector3F> points3F = new List<Vector3F>(new[]
      {
        new Vector3F(-1, -2, 1),
        new Vector3F(1, 0, 2),
        new Vector3F(2, -1, 3),
        new Vector3F(2, -1, 2),
      });

      Matrix33F cov3F = StatisticsHelper.ComputeCovarianceMatrix(points3F);
      Assert.AreEqual(3f / 2, cov3F[0, 0]);
      Assert.AreEqual(1f / 2, cov3F[0, 1]);
      Assert.AreEqual(3f / 4, cov3F[0, 2]);
      Assert.AreEqual(1f / 2, cov3F[1, 0]);
      Assert.AreEqual(1f / 2, cov3F[1, 1]);
      Assert.AreEqual(1f / 4, cov3F[1, 2]);
      Assert.AreEqual(3f / 4, cov3F[2, 0]);
      Assert.AreEqual(1f / 4, cov3F[2, 1]);
      Assert.AreEqual(1f / 2, cov3F[2, 2]);

      // Compare with Vector3D version.
      List<Vector3D> points3D = new List<Vector3D>();
      foreach (var point in points3F)
        points3D.Add(point.ToVector3D());
      Matrix33D cov3D = StatisticsHelper.ComputeCovarianceMatrix(points3D);
      for (int i = 0; i < cov3F.ToArray1D(MatrixOrder.RowMajor).Length; i++)
      {
        var item3F = cov3F.ToArray1D(MatrixOrder.RowMajor)[i];
        var item3D = cov3D.ToArray1D(MatrixOrder.RowMajor)[i];
        Assert.AreEqual(item3F, item3D);          
      }

      // Compare with VectorF version.
      List<VectorF> pointsF = new List<VectorF>();
      foreach (var point in points3F)
        pointsF.Add(point.ToVectorF());
      MatrixF covF = StatisticsHelper.ComputeCovarianceMatrix(pointsF);
      for (int i = 0; i < cov3F.ToArray1D(MatrixOrder.RowMajor).Length; i++)
      {
        var item3F = cov3F.ToArray1D(MatrixOrder.RowMajor)[i];
        var itemF = covF.ToArray1D(MatrixOrder.RowMajor)[i];
        Assert.AreEqual(item3F, itemF);
      }

      // Compare with VectorF version.
      List<VectorD> pointsD = new List<VectorD>();
      foreach (var point in points3D)
        pointsD.Add(point.ToVectorD());
      MatrixD covD = StatisticsHelper.ComputeCovarianceMatrix(pointsD);
      for (int i = 0; i < cov3F.ToArray1D(MatrixOrder.RowMajor).Length; i++)
      {
        var item3F = cov3F.ToArray1D(MatrixOrder.RowMajor)[i];
        var itemD = covD.ToArray1D(MatrixOrder.RowMajor)[i];
        Assert.AreEqual(item3F, itemD);
      }
    }
  }
}