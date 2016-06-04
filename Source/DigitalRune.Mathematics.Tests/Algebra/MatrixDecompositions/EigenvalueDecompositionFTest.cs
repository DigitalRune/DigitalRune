using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class EigenvalueDecompositionFTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException1()
    {
      new EigenvalueDecompositionF(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      MatrixF m = new MatrixF(new float[,] {{ 1, 2 }, 
                                           { 3, 4 },
                                           { 5, 6 }});
      new EigenvalueDecompositionF(m);
    }


    [Test]
    public void Test1()
    {
      MatrixF a = new MatrixF(new float[,] {{ 1, -1, 4 }, 
                                           { 3, 2, -1 },
                                           { 2, 1, -1}});
      EigenvalueDecompositionF d = new EigenvalueDecompositionF(a);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(a * d.V, d.V * d.D));
    }


    [Test]
    public void Test2()
    {
      MatrixF a = new MatrixF(new float[,] {{ 0, 1, 2 }, 
                                           { 1, 4, 3 },
                                           { 2, 3, 5}});
      EigenvalueDecompositionF d = new EigenvalueDecompositionF(a);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(a, d.V * d.D * d.V.Transposed));
    }


    [Test]
    public void TestWithNaNValues()
    {
      MatrixF a = new MatrixF(new [,] {{ 0, float.NaN, 2 }, 
                                       { 1, 4, 3 },
                                        { 2, 3, 5}});
      
      var d = new EigenvalueDecompositionF(a);
      foreach (var element in d.RealEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.ImaginaryEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);

      d = new EigenvalueDecompositionF(new MatrixF(4, 4, float.NaN));
      foreach (var element in d.RealEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.ImaginaryEigenvalues.ToList())
        Assert.IsNaN(element);
      foreach (var element in d.V.ToList(MatrixOrder.RowMajor))
        Assert.IsNaN(element);
    }
  }
}
