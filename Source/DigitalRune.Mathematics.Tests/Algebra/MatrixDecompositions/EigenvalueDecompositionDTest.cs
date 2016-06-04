using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class EigenvalueDecompositionDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new EigenvalueDecompositionD(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      MatrixD m = new MatrixD(new double[,] {{ 1, 2 }, 
                                             { 3, 4 },
                                             { 5, 6 }});
      new EigenvalueDecompositionD(m);
    }


    [Test]
    public void Test1()
    {
      MatrixD a = new MatrixD(new double[,] {{ 1, -1, 4 }, 
                                             { 3, 2, -1 },
                                             { 2, 1, -1 }});
      EigenvalueDecompositionD d = new EigenvalueDecompositionD(a);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(a * d.V, d.V * d.D));
    }


    [Test]
    public void Test2()
    {
      MatrixD a = new MatrixD(new double[,] {{ 0, 1, 2 }, 
                                             { 1, 4, 3 },
                                             { 2, 3, 5 }});
      EigenvalueDecompositionD d = new EigenvalueDecompositionD(a);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(a, d.V * d.D * d.V.Transposed));
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixD a = new MatrixD(new[,] {{ 0, double.NaN, 2 }, 
                                      { 1, 4, 3 },
                                      { 2, 3, 5}});

      var d = new EigenvalueDecompositionD(a);
      foreach (var element in d.RealEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.ImaginaryEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);

      d = new EigenvalueDecompositionD(new MatrixD(4, 4, double.NaN));
      foreach (var element in d.RealEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.ImaginaryEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }
  }
}
