using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class MatrixDTest
  {
    //           1,  2,  3,  4
    // Matrix =  5,  6,  7,  8
    //           9, 10, 11, 12,

    // in column-major layout
    double[] columnMajor = new[] { 1.0, 5.0, 9.0, 
                                  2.0, 6.0, 10.0, 
                                  3.0, 7.0, 11.0f,
                                  4.0, 8.0, 12.0 };

    // in row-major layout
    double[] rowMajor = new[] { 1.0, 2.0, 3.0, 4.0, 
                               5.0, 6.0, 7.0, 8.0, 
                               9.0, 10.0, 11.0, 12.0 };


    [Test]
    public void Constructors()
    {
      MatrixD m = new MatrixD(3, 4);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(0, m[i]);
      Assert.AreEqual(3, m.NumberOfRows);
      Assert.AreEqual(4, m.NumberOfColumns);

      m = new MatrixD(20, 2, 777);
      for (int i = 0; i < 20; i++)
        Assert.AreEqual(777, m[i]);
      Assert.AreEqual(20, m.NumberOfRows);
      Assert.AreEqual(2, m.NumberOfColumns);

      m = new MatrixD(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixD(3, 4, new List<double>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixD(3, 4, new List<double>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixD(new double[3, 4] { { 1, 2, 3, 4 }, 
                                        { 5, 6, 7, 8 }, 
                                        { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixD(new double[3][] { new double[4] { 1, 2, 3, 4 }, 
                                       new double[4] { 5, 6, 7, 8 }, 
                                       new double[4] { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }    


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException1()
    {
      new MatrixD(0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new MatrixD(-1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException3()
    {
      new MatrixD(1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException4()
    {
      new MatrixD(0, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixD();
      m.Set((double[])null, MatrixOrder.ColumnMajor);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWith2DArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixD();
      m.Set((double[,])null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWith2DJaggedArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixD();
      m.Set((double[][])null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithListShouldThrowArgumentNullException()
    {
      var m = new MatrixD();
      m.Set((IList<double>)null, MatrixOrder.RowMajor);
    }


    [Test]
    public void Set()
    {
      MatrixD m = new MatrixD(3, 4);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.Set(m2);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
      m[0] = 10;  // Test if original matrix is unchanged.
      Assert.AreEqual(1, m2[0]);

      m.Set(777);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(777, m[i]);

      m.Set(0);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(0, m[i]);
      m.Set(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new List<double>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new List<double>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new double[3, 4] { { 1, 2, 3, 4 }, 
                              { 5, 6, 7, 8 }, 
                              { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new double[3][] { new double[4] { 1, 2, 3, 4 }, 
                             new double[4] { 5, 6, 7, 8 }, 
                             new double[4] { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }


    [Test]
    public void SetIdentity()
    {
      MatrixD m = new MatrixD(3, 3, 12);
      m.SetIdentity();
      for(int i=0; i<m.NumberOfRows; i++)
      {
        for (int j = 0; j < m.NumberOfColumns; j++) 
        {
          if (i == j)
            Assert.AreEqual(1, m[i, j]);
          else
            Assert.AreEqual(0, m[i, j]);
        }
      }


      m = new MatrixD(10, 4, 12);
      m.SetIdentity();
      for (int i = 0; i < m.NumberOfRows; i++)
      {
        for (int j = 0; j < m.NumberOfColumns; j++)
        {
          if (i == j)
            Assert.AreEqual(1, m[i, j]);
          else
            Assert.AreEqual(0, m[i, j]);
        }
      }


      m = new MatrixD(2, 5, 12);
      m.SetIdentity();
      for (int i = 0; i < m.NumberOfRows; i++)
      {
        for (int j = 0; j < m.NumberOfColumns; j++)
        {
          if (i == j)
            Assert.AreEqual(1, m[i, j]);
          else
            Assert.AreEqual(0, m[i, j]);
        }
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException1()
    {
      MatrixD m = new MatrixD(4, 3);
      m.Set(new MatrixD(1, 2, 777));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetException2()
    {
      MatrixD m = null;
      MatrixD n = new MatrixD(1, 1);
      n.Set(m);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException()
    {
      MatrixD m = new MatrixD(4, 3);
      m[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException2()
    {
      MatrixD m = new MatrixD(4, 3);
      m[12] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException3()
    {
      MatrixD m = new MatrixD(4, 3);
      double x = m[-1];
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException4()
    {
      MatrixD m = new MatrixD(4, 3);
      double x = m[12];
    }


    [Test]
    public void Indexer2d()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(columnMajor[column * 3 + row], m[row, column]);

      m = new MatrixD(3, 4);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          m[row, column] = row * 4 + column + 1;

      Assert.AreEqual(new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer2dException()
    {
      MatrixD m = new MatrixD(3, 4);
      m[0, 4] = 1.0;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 8;
      const int numberOfColumns = 8;
      Assert.IsFalse(new MatrixD(numberOfRows, numberOfColumns).IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          MatrixD m = new MatrixD(numberOfRows, numberOfColumns);
          m[r, c] = double.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsSquare()
    {
      MatrixD m = new MatrixD(3, 3, 666);
      Assert.AreEqual(true, m.IsSquare);

      m = new MatrixD(4, 3, 666);
      Assert.AreEqual(false, m.IsSquare);

      m = new MatrixD(3, 4, 666);
      Assert.AreEqual(false, m.IsSquare);
    }


    [Test]
    public void Norm1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(24, m.Norm1);
    }


    [Test]
    public void NormFrobenius()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.IsTrue(Numeric.AreEqual(25.495097567963921, m.NormFrobenius));
    }


    [Test]
    public void NormInfinity()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(42, m.NormInfinity);
    }


    [Test]
    public void GetMinor()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      Assert.AreEqual(new MatrixD(new double[,] { { 6, 7, 8 }, { 10, 11, 12 } }), m.GetMinor(0, 0));
      Assert.AreEqual(new MatrixD(new double[,] { { 5, 7, 8 }, { 9, 11, 12 } }), m.GetMinor(0, 1));
      Assert.AreEqual(new MatrixD(new double[,] { { 1, 2, 3 }, { 5, 6, 7 } }), m.GetMinor(2, 3));
      Assert.AreEqual(new MatrixD(new double[,] { { 1, 3, 4 }, { 5, 7, 8 } }), m.GetMinor(2, 1));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void GetMinorException1()
    {
      MatrixD m = new MatrixD(1, 1);
      m.GetMinor(1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetMinorException2()
    {
      MatrixD m = new MatrixD(4, 3);
      m.GetMinor(4, 3);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetMinorException3()
    {
      MatrixD m = new MatrixD(4, 3);
      m.GetMinor(0, 3);
    }


    [Test]
    public void Transposed()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD mt = new MatrixD(4, 3, rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
    }


    [Test]
    public void Transpose()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.Transpose();
      MatrixD mt = new MatrixD(4, 3, rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(MatrixD.CreateIdentity(3,3), MatrixD.CreateIdentity(3,3).Inverse);

      MatrixD m = new MatrixD(new double[,] {{1, 2,  3, 4},
                                            {2, 5,  8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9,  7, 7}});
      VectorD v = new VectorD(4, 1);
      VectorD w = m * v;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4,4), m * m.Inverse));

      m = new MatrixD(new double[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      // To check the pseudo-inverse we use the definition: A*A.Transposed*A = A
      // see http://en.wikipedia.org/wiki/Moore-Penrose_pseudoinverse
      Assert.IsTrue(MatrixD.AreNumericallyEqual(m, m * m.Inverse * m));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      MatrixD m = new MatrixD(new double[,] {{0.0001, 0, 0, 0},
                                            {0, 0.0001, 0, 0},
                                            {0, 0, 0.0001, 0},
                                            {0, 0,  0, 0.0001}});
      VectorD v = new VectorD(4, 1);
      VectorD w = m * v;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      MatrixD m = new MatrixD(new double[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {3, 7, 11, 7}});
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      MatrixD m = new MatrixD(4,4).Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException3()
    {
      MatrixD m = new MatrixD(new double[,] {{1, 2, 3},
                                            {2, 5, 8},
                                            {7, 6, -1},
                                            {4, 9, 7}}).Transposed;
      MatrixD inverse = m.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(MatrixD.CreateIdentity(3, 3), MatrixD.CreateIdentity(3, 3).Inverse);

      MatrixD m = new MatrixD(new double[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9, 7, 7}});
      MatrixD inverse = m.Clone();
      m.Invert();
      VectorD v = new VectorD(4, 1);
      VectorD w = m * v;
      Assert.IsTrue(VectorD.AreNumericallyEqual(v, inverse * w));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), m * inverse));

      m = new MatrixD(new double[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      // To check the pseudo-inverse we use the definition: A*A.Transposed*A = A
      // see http://en.wikipedia.org/wiki/Moore-Penrose_pseudoinverse
      inverse = m.Clone();
      inverse.Invert();
      Assert.IsTrue(MatrixD.AreNumericallyEqual(m, m * inverse * m));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      MatrixD m = new MatrixD(new double[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {3, 7, 11, 7}});
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      MatrixD m = new MatrixD(4, 4);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException3()
    {
      MatrixD m = new MatrixD(new double[,] {{1, 2, 3},
                                            {2, 5, 8},
                                            {7, 6, -1},
                                            {4, 9, 7}}).Transposed;
      m.Invert();
    }


    [Test]
    public void TryInvert()
    {
      // Regular, square
      MatrixD m = new MatrixD(new double[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9, 7, 7}});
      MatrixD inverse = m.Clone();
      Assert.AreEqual(true, m.TryInvert());
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), m * inverse));

      // Full column rank, rectangular
      m = new MatrixD(new double[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      inverse = m.Clone();
      Assert.AreEqual(true, m.TryInvert());
      Assert.IsTrue(MatrixD.AreNumericallyEqual(m, m * inverse * m));

      // singular
      m = new MatrixD(new double[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {3, 7, 11}});
      inverse = m.Clone();
      Assert.AreEqual(false, m.TryInvert());
    }


    [Test]
    public void Determinant()
    {
      MatrixD m = new Matrix44D(1, 2, 3, 4,
                                5, 6, 7, 8,
                                9, 10, 11, 12,
                                13, 14, 15, 16).ToMatrixD();
      Assert.IsTrue(Numeric.IsZero(m.Determinant));

      m = new Matrix44D(1, 2, 3, 4,
                       -3, 4, 5, 6,
                       2, -5, 7, 4,
                       10, 2, -3, 9).ToMatrixD();
      Assert.IsTrue((Numeric.AreEqual(1142, m.Determinant)));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      double det = m.Determinant;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException2()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      double det = m.Determinant;
    }


    [Test]
    public void IsSymmetric()
    {
      MatrixD m = new MatrixD(new double[4, 4] { { 1, 2, 3, 4 }, 
                                                { 2, 4, 5, 6 }, 
                                                { 3, 5, 7, 8 }, 
                                                { 4, 6, 8, 9 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new MatrixD(new double[4, 4] { { 4, 3, 2, 1 }, 
                                        { 6, 5, 4, 2 }, 
                                        { 8, 7, 5, 3 }, 
                                        { 9, 8, 6, 4 } });
      Assert.AreEqual(false, m.IsSymmetric);

      Assert.IsTrue(new MatrixD(2, 2, 0).IsSymmetric);
      Assert.IsFalse(new MatrixD(3, 2, 0).IsSymmetric);
      Assert.IsFalse(new MatrixD(2, 3, 0).IsSymmetric);
    }


    [Test]
    public void GetSubmatrix()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      Assert.AreEqual(m, m.GetSubmatrix(0, 2, 0, 3));
      Assert.AreEqual(new MatrixD(1, 1, 1), m.GetSubmatrix(0, 0, 0, 0));
      Assert.AreEqual(new MatrixD(1, 1, 12), m.GetSubmatrix(2, 2, 3, 3));
      Assert.AreEqual(new MatrixD(1, 1, 12), m.GetSubmatrix(2, 2, new int[] { 3 }));
      Assert.AreEqual(new MatrixD(1, 1, 4), m.GetSubmatrix(new int[] {0}, 3, 3));
      Assert.AreEqual(new MatrixD(1, 1, 10), m.GetSubmatrix(new int[] { 2 }, new int[] { 1 }));

      Assert.AreEqual(new MatrixD(new double[,] { { 5, 6, 7, 8 }, { 9, 10, 11, 12 } }), m.GetSubmatrix(1, 2, 0, 3));
      Assert.AreEqual(new MatrixD(new double[,] { { 8, 6, 7 }, { 12, 10, 11 } }), m.GetSubmatrix(1, 2, new int[] { 3, 1, 2}));
      Assert.AreEqual(new MatrixD(new double[,] { { 11, 12 }, { 7, 8 }, { 3, 4 } }), m.GetSubmatrix(new int[] { 2, 1, 0 }, 2, 3));
      Assert.AreEqual(new MatrixD(new double[,] { { 8, 7, 5, 6 }, { 12, 11, 9, 10 }, { 4, 3, 1, 2 } }), m.GetSubmatrix(new int[] { 1, 2, 0 }, new int[] {3, 2, 0, 1}));

      Assert.AreEqual(null, m.GetSubmatrix(null, 0, 2));
      Assert.AreEqual(null, m.GetSubmatrix(0, 2, null));
      Assert.AreEqual(null, m.GetSubmatrix(null, new int[] { 1 }));
      Assert.AreEqual(null, m.GetSubmatrix(new int[] { 1 }, null));
      Assert.AreEqual(null, m.GetSubmatrix(null, null));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException1()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 0, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException2()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException3()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 0, new int[] { 1 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException4()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, 1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException5()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(-1, 1, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException6()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(4, 4, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException7()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 4, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException8()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 0, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException9()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 4, new int[]{1});
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException10()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, -1, 0);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException11()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { -1 }, 1, 2);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException12()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 2, new int[] { 4 });
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException13()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 4 }, new int[] { 2 });
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException14()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 2 }, new int[] { 4 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException15()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 4, 5);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException16()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 0, 5);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException17()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(4, 4, new int[] {1 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException18()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, 2, 4);
    }


    [Test]
    public void SetSubmatrix()
    {
      MatrixD m = new MatrixD(4, 5, 0);

      m.SetSubmatrix(2, 2, new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } }));

      Assert.AreEqual(new MatrixD(new double[,] {{0, 0, 0, 0, 0},
                                                {0, 0, 0, 0, 0},
                                                {0, 0, 1, 2, 3},
                                                {0, 0, 4, 5, 6}}), m);

      m.SetSubmatrix(0, 0, new MatrixD(1, 1, 777));
      Assert.AreEqual(777, m[0, 0]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException1()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(-1, 0, new MatrixD(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException2()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(4, 0, new MatrixD(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException3()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, -2, new MatrixD(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException4()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 4, new MatrixD(1, 1));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubmatrixException5()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, new MatrixD(4, 1));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubmatrixException6()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, new MatrixD(1, 4));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetSubmatrixException7()
    {
      MatrixD m = new MatrixD(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, null);
    }


    [Test]
    public void SolveLinearEquationsMatrix()
    {
      // Regular square matrix.
      MatrixD matrixA = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixD matrixB = new MatrixD(new double[,] {{1, 2},{3, 4}, {5, 6}});

      MatrixD matrixX = MatrixD.SolveLinearEquations(matrixA, matrixB);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(matrixB, matrixA * matrixX));

      // Full column rank rectangular matrix.
      matrixA = new MatrixD(new double[,] { { 1, 2 }, { 4, 5}, { 7, -8 } });
      matrixX = MatrixD.SolveLinearEquations(matrixA, matrixB);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(matrixA.Transposed * matrixB, matrixA.Transposed * matrixA * matrixX));  // Normal equation (see least squares, Gauss transformation).
    }


    [Test]
    public void SolveLinearEquationsVector()
    {
      // Regular square matrix.
      MatrixD matrixA = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      VectorD vectorB = new VectorD(new double[] { 1, 2, 4 });

      VectorD vectorX = MatrixD.SolveLinearEquations(matrixA, vectorB);
      Assert.IsTrue(VectorD.AreNumericallyEqual(vectorB, matrixA * vectorX));

      // Full column rank rectangular matrix.
      matrixA = new MatrixD(new double[,] { { 1, 2 }, { 4, 5 }, { 7, -8 } });
      vectorX = MatrixD.SolveLinearEquations(matrixA, vectorB);
      Assert.IsTrue(VectorD.AreNumericallyEqual(matrixA.Transposed * vectorB, matrixA.Transposed * matrixA * vectorX));  // Normal equation (see least squares, Gauss transformation).
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      MatrixD.SolveLinearEquations(null, new MatrixD(1, 1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixD.SolveLinearEquations(a, new MatrixD(4, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException3()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }});  // not full column rank.
      MatrixD.SolveLinearEquations(a, new MatrixD(2, 1));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException4()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixD.SolveLinearEquations(a, new MatrixD(3, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException5()
    {
      MatrixD.SolveLinearEquations(null, new VectorD(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException6()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixD.SolveLinearEquations(a, new VectorD(4)); // number of rows dont fit
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException7()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });  // not full column rank.
      MatrixD.SolveLinearEquations(a, new VectorD(2));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException8()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixD.SolveLinearEquations(a, new VectorD(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException9()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixD.SolveLinearEquations(a, (VectorD)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException10()
    {
      MatrixD a = new MatrixD(new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixD.SolveLinearEquations(a, (MatrixD) null);
    }


    [Test]
    public void Trace()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(18, m.Trace);
    }


    [Test]
    public void GetColumn()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new VectorD(new double[] { 1.0, 5.0, 9.0 }), m.GetColumn(0));
      Assert.AreEqual(new VectorD(new double[] { 2.0, 6.0, 10.0 }), m.GetColumn(1));
      Assert.AreEqual(new VectorD(new double[] { 3.0, 7.0, 11.0 }), m.GetColumn(2));
      Assert.AreEqual(new VectorD(new double[] { 4.0, 8.0, 12.0 }), m.GetColumn(3));
    }

    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetColumnException1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetColumnException2()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(4);
    }


    [Test]
    public void SetColumn()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(0, new VectorD(new double[]{ 0.1, 0.2, 0.3 }));
      Assert.AreEqual(new VectorD(new double[]{ 0.1, 0.2, 0.3 }), m.GetColumn(0));
      Assert.AreEqual(new VectorD(new double[]{ 2.0, 6.0, 10.0 }), m.GetColumn(1));
      Assert.AreEqual(new VectorD(new double[]{ 3.0, 7.0, 11.0 }), m.GetColumn(2));
      Assert.AreEqual(new VectorD(new double[]{ 4.0, 8.0, 12.0 }), m.GetColumn(3));

      m.SetColumn(1, new VectorD(new double[] { 0.4, 0.5, 0.6 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3 }), m.GetColumn(0));
      Assert.AreEqual(new VectorD(new double[] { 0.4, 0.5, 0.6 }), m.GetColumn(1));
      Assert.AreEqual(new VectorD(new double[] { 3.0, 7.0, 11.0 }), m.GetColumn(2));
      Assert.AreEqual(new VectorD(new double[] { 4.0, 8.0, 12.0 }), m.GetColumn(3));

      m.SetColumn(2, new VectorD(new double[] { 0.7, 0.8, 0.9 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3 }), m.GetColumn(0));
      Assert.AreEqual(new VectorD(new double[] { 0.4, 0.5, 0.6 }), m.GetColumn(1));
      Assert.AreEqual(new VectorD(new double[] { 0.7, 0.8, 0.9 }), m.GetColumn(2));
      Assert.AreEqual(new VectorD(new double[] { 4.0, 8.0, 12.0 }), m.GetColumn(3));

      m.SetColumn(3, new VectorD(new double[] { 1.1, 1.8, 1.9 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3 }), m.GetColumn(0));
      Assert.AreEqual(new VectorD(new double[] { 0.4, 0.5, 0.6 }), m.GetColumn(1));
      Assert.AreEqual(new VectorD(new double[] { 0.7, 0.8, 0.9 }), m.GetColumn(2));
      Assert.AreEqual(new VectorD(new double[] { 1.1, 1.8, 1.9 }), m.GetColumn(3));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetColumnException1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(-1, new VectorD(3));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetColumnException2()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(4, new VectorD(3));
    }


    [Test]
    public void GetRow()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new VectorD(new double[] { 1.0, 2.0, 3.0, 4.0 }), m.GetRow(0));
      Assert.AreEqual(new VectorD(new double[] { 5.0, 6.0, 7.0, 8.0 }), m.GetRow(1));
      Assert.AreEqual(new VectorD(new double[] { 9.0, 10.0, 11.0, 12.0 }), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetRowException1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetRowException2()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetRow(3);
    }


    [Test]
    public void SetRow()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(0, new VectorD(new double[]{ 0.1, 0.2, 0.3, 0.4 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3, 0.4 }), m.GetRow(0));
      Assert.AreEqual(new VectorD(new double[] { 5.0, 6.0, 7.0, 8.0 }), m.GetRow(1));
      Assert.AreEqual(new VectorD(new double[] { 9.0, 10.0, 11.0, 12.0 }), m.GetRow(2));

      m.SetRow(1, new VectorD(new double[] { 0.4, 0.5, 0.6, 0.7 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3, 0.4 }), m.GetRow(0));
      Assert.AreEqual(new VectorD(new double[] { 0.4, 0.5, 0.6, 0.7 }), m.GetRow(1));
      Assert.AreEqual(new VectorD(new double[] { 9.0, 10.0, 11.0, 12.0 }), m.GetRow(2));

      m.SetRow(2, new VectorD(new double[] { 0.7, 0.8, 0.9, 1.0 }));
      Assert.AreEqual(new VectorD(new double[] { 0.1, 0.2, 0.3, 0.4 }), m.GetRow(0));
      Assert.AreEqual(new VectorD(new double[] { 0.4, 0.5, 0.6, 0.7 }), m.GetRow(1));
      Assert.AreEqual(new VectorD(new double[] { 0.7, 0.8, 0.9, 1.0 }), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetRowException1()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(-1, new VectorD(4));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetRowException2()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(3, new VectorD(4));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      MatrixD m0 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixD(3, 4, 0.000001);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixD(3, 4, 0.00000001);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(MatrixD.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(m0, m2));

      Assert.IsFalse(MatrixD.AreNumericallyEqual(new MatrixD(1, 2), new MatrixD(2, 2)));
      Assert.IsFalse(MatrixD.AreNumericallyEqual(new MatrixD(2, 1), new MatrixD(2, 2)));

      Assert.IsTrue(MatrixD.AreNumericallyEqual(null, null));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreNumericallyEqualWithEpsilon()
    {
      double epsilon = 0.001;

      MatrixD m0 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixD(3, 4, 0.002);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixD(3, 4, 0.0001);

      Assert.IsTrue(MatrixD.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(MatrixD.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(m0, m2, epsilon));

      Assert.IsFalse(MatrixD.AreNumericallyEqual(new MatrixD(1, 2), new MatrixD(2, 2), epsilon));
      Assert.IsFalse(MatrixD.AreNumericallyEqual(new MatrixD(2, 1), new MatrixD(2, 2), epsilon));

      Assert.IsTrue(MatrixD.AreNumericallyEqual(null, null, epsilon));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException1()
    {
      MatrixD m0 = new MatrixD(3, 4);
      MatrixD m1 = null;
      MatrixD.AreNumericallyEqual(m0, m1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException2()
    {
      MatrixD m0 = null;
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD.AreNumericallyEqual(m0, m1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException3()
    {
      MatrixD m0 = new MatrixD(3, 4);
      MatrixD m1 = null;
      MatrixD.AreNumericallyEqual(m0, m1, 0.1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException4()
    {
      MatrixD m0 = null;
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD.AreNumericallyEqual(m0, m1, 0.1);
    }


    [Test]
    public void HashCode()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreNotEqual(MatrixD.CreateIdentity(3, 4), m.GetHashCode());
      Assert.AreNotEqual(new MatrixD(3, 4), m.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD nullMatrix = null;
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      Assert.IsFalse(m1.Equals(nullMatrix));

      Assert.IsTrue(((object) m1).Equals((object) m1));
      Assert.IsTrue(((object) m1).Equals((object) m2));
      Assert.IsFalse(((object) m1).Equals((object) nullMatrix));

      m2 += new MatrixD(3, 4, 0.1);
      Assert.IsFalse(m1.Equals(m2));
    }


    [Test]
    public void EqualityOperators()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD nullMatrix = null;
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 == nullMatrix);
      Assert.IsFalse(nullMatrix == m2);
      Assert.IsFalse(m1 != m2);
      Assert.IsTrue(m1 != nullMatrix);
      Assert.IsTrue(nullMatrix != m2);

      m2 += new MatrixD(3, 4, 0.1);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);

      m1 = new MatrixD(1, 2);
      m2 = new MatrixD(2, 2);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);

      m1 = new MatrixD(2, 2);
      m2 = new MatrixD(2, 1);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);
    }


    [Test]
    public void ToStringTest()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void Clone()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      var o = m.Clone();
      Assert.AreEqual(m, o);
    }


    [Test]
    public void NegationOperator()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);

      m = null;
      Assert.IsNull(-m);
    }


    [Test]
    public void Negation()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i], MatrixD.Negate(m)[i]);

      m = null;
      Assert.IsNull(MatrixD.Negate(m));
    }


    [Test]
    public void AddMatrixOperator()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) * (-3);
      MatrixD result = m1 + m2;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void AddMatrix()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) * (-3);
      MatrixD result = MatrixD.Add(m1, m2);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixOperatorException1()
    {
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD m2 = null;
      MatrixD result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixOperatorException2()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixOperatorException3()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixOperatorException4()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(4, 3);
      MatrixD result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixException1()
    {
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD m2 = null;
      MatrixD.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixException2()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixException3()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixException4()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(4, 3);
      MatrixD.Add(m1, m2);
    }


    [Test]
    public void SubtractMatrixOperator()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) * 3;
      MatrixD result = m1 - m2;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void SubtractMatrix()
    {
      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor) * 3;
      MatrixD result = MatrixD.Subtract(m1, m2);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]    
    public void SubtractMatrixOperatorException1()
    {
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD m2 = null;
      MatrixD result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixOperatorException2()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixOperatorException3()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixOperatorException4()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(4, 3);
      MatrixD result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixException1()
    {
      MatrixD m1 = new MatrixD(3, 4);
      MatrixD m2 = null;
      MatrixD.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixException2()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixException3()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(3, 4);
      MatrixD.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixException4()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = new MatrixD(4, 3);
      MatrixD.Subtract(m1, m2);
    }


    [Test]
    public void MultiplyScalarOperator()
    {
      double s = 0.1234;
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = s * m;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = m * s;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void MultipyScalar()
    {
      double s = 0.1234;
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = MatrixD.Multiply(s, m);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultipyScalarException()
    {
      double s = 1.0;
      MatrixD m = null;
      m = MatrixD.Multiply(s, m);
    }


    [Test]
    public void DivideByScalarOperator()
    {
      double s = 0.1234;
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = m / s;
      for (int i = 0; i < 12; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void DivideByScalar()
    {
      double s = 0.1234;
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = MatrixD.Divide(m, s);
      for (int i = 0; i < 12; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideByScalarException()
    {
      double s = 1.0;
      MatrixD m = null;
      m = MatrixD.Divide(m, s);
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      double[] values = new double[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixD m = new MatrixD(4, 4, values, MatrixOrder.RowMajor);
      Assert.AreEqual(new MatrixD(4, 4), m * new MatrixD(4, 4));
      Assert.AreEqual(new MatrixD(4, 4), new MatrixD(4, 4) * m);
      Assert.AreEqual(m, m * MatrixD.CreateIdentity(4, 4));
      Assert.AreEqual(m, MatrixD.CreateIdentity(4, 4) * m);
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), m * m.Inverse));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), m.Inverse * m));

      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(4, 4, values, MatrixOrder.RowMajor);

      MatrixD result = m1 * m2;
      Assert.AreEqual(3, result.NumberOfRows);
      Assert.AreEqual(4, result.NumberOfColumns);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(VectorD.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      double[] values = new double[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixD m = new MatrixD(4, 4, values, MatrixOrder.RowMajor);
      Assert.AreEqual(new MatrixD(4, 4), MatrixD.Multiply(m, new MatrixD(4, 4)));
      Assert.AreEqual(new MatrixD(4, 4), MatrixD.Multiply(new MatrixD(4, 4), m));
      Assert.AreEqual(m, MatrixD.Multiply(m, MatrixD.CreateIdentity(4, 4)));
      Assert.AreEqual(m, MatrixD.Multiply(MatrixD.CreateIdentity(4, 4), m));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), MatrixD.Multiply(m, m.Inverse)));
      Assert.IsTrue(MatrixD.AreNumericallyEqual(MatrixD.CreateIdentity(4, 4), MatrixD.Multiply(m.Inverse, m)));

      MatrixD m1 = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixD m2 = new MatrixD(4, 4, values, MatrixOrder.RowMajor);

      MatrixD result = MatrixD.Multiply(m1, m2);
      Assert.AreEqual(3, result.NumberOfRows);
      Assert.AreEqual(4, result.NumberOfColumns);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(VectorD.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixOperatorException1()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(4, 4);
      MatrixD m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixOperatorException2()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = null;
      MatrixD m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyMatrixOperatorException3()
    {
      MatrixD m1 = new MatrixD(4, 3);
      MatrixD m2 = new MatrixD(4, 5);
      MatrixD m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixException1()
    {
      MatrixD m1 = null;
      MatrixD m2 = new MatrixD(4, 4);
      MatrixD m3 = MatrixD.Multiply(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixException2()
    {
      MatrixD m1 = new MatrixD(4, 4);
      MatrixD m2 = null;
      MatrixD m3 = MatrixD.Multiply(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyMatrixException3()
    {
      MatrixD m1 = new MatrixD(4, 3);
      MatrixD m2 = new MatrixD(4, 5);
      MatrixD m3 = MatrixD.Multiply(m1, m2);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      VectorD v = new VectorD(new double[] { 2.34, 3.45, 4.56, 23.4 });
      Assert.AreEqual(v, MatrixD.CreateIdentity(4, 4) * v);
      Assert.AreEqual(new VectorD(4), new MatrixD(4, 4) * v);

      double[] values = new double[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixD m = new MatrixD(4, 4, values, MatrixOrder.RowMajor);
      Assert.IsTrue(VectorD.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(VectorD.Dot(m.GetRow(i), v), (m * v)[i]));
    }


    [Test]
    public void MultiplyVector()
    {
      VectorD v = new VectorD(new double[] { 2.34, 3.45, 4.56, 23.4 });
      Assert.AreEqual(v, MatrixD.Multiply(MatrixD.CreateIdentity(4, 4), v));
      Assert.AreEqual(new VectorD(4), MatrixD.Multiply(new MatrixD(4, 4), v));

      double[] values = new double[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixD m = new MatrixD(4, 4, values, MatrixOrder.RowMajor);
      Assert.IsTrue(VectorD.AreNumericallyEqual(v, MatrixD.Multiply(MatrixD.Multiply(m, m.Inverse), v)));

      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(VectorD.Dot(m.GetRow(i), v), MatrixD.Multiply(m, v)[i]));
    }


    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void MultiplyVectorOperatorException1()
    {
      MatrixD m = null;
      VectorD v = new VectorD(4);
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorOperatorException2()
    {
      MatrixD m = new MatrixD(4, 4);
      VectorD v = null;
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyVectorOperatorException3()
    {
      MatrixD m = new MatrixD(4, 3);
      VectorD v = new VectorD(4);
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorException1()
    {
      MatrixD m = null;
      VectorD v = new VectorD(4);
      v = MatrixD.Multiply(m, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorException2()
    {
      MatrixD m = new MatrixD(4, 4);
      VectorD v = null;
      v = MatrixD.Multiply(m, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyVectorException3()
    {
      MatrixD m = new MatrixD(4, 3);
      VectorD v = new VectorD(4);
      v = MatrixD.Multiply(m, v);
    }


    [Test]
    public void Absolute()
    {
      double[] values = new double[] { -1.0, -2.0, -3.0, 
                                     -4.0, -5.0, -6.0, 
                                     -7.0, -8.0, -9.0 };
      MatrixD m = new MatrixD(3, 3, values, MatrixOrder.RowMajor);

      MatrixD absolute = m.Clone();
      absolute.Absolute();
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      absolute = MatrixD.Absolute(m);
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      values = new double[] { 1.0, 2.0, 3.0, 
                             4.0, 5.0, 6.0, 
                             7.0, 8.0, 9.0 };
      m = new MatrixD(3, 3, values, MatrixOrder.RowMajor);

      absolute = m.Clone();
      absolute.Absolute();
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      absolute = MatrixD.Absolute(m);
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      Assert.IsNull(MatrixD.Absolute(null));
    }


    [Test]
    public void ClampToZero()
    {
      MatrixD m = new MatrixD(5, 6, 0.0000000000001);
      m.ClampToZero();
      Assert.AreEqual(new MatrixD(5, 6), m);

      m = new MatrixD(5, 6, 0.1);
      m.ClampToZero();
      Assert.AreEqual(new MatrixD(5, 6, 0.1), m);

      m = new MatrixD(5, 6, 0.001);
      m.ClampToZero(0.01);
      Assert.AreEqual(new MatrixD(5, 6), m);

      m = new MatrixD(5, 6, 0.1);
      m.ClampToZero(0.01);
      Assert.AreEqual(new MatrixD(5, 6, 0.1), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      MatrixD m = new MatrixD(3, 4, 0.0000000000001);
      Assert.AreEqual(new MatrixD(3, 4), MatrixD.ClampToZero(m));
      Assert.AreEqual(new MatrixD(3, 4, 0.0000000000001), m); // m unchanged?

      m = new MatrixD(3, 4, 0.1);
      Assert.AreEqual(new MatrixD(3, 4, 0.1), MatrixD.ClampToZero(m));
      Assert.AreEqual(new MatrixD(3, 4, 0.1), m);

      m = new MatrixD(3, 4, 0.001);
      Assert.AreEqual(new MatrixD(3, 4), MatrixD.ClampToZero(m, 0.01));
      Assert.AreEqual(new MatrixD(3, 4, 0.001), m);

      m = new MatrixD(3, 4, 0.1);
      Assert.AreEqual(new MatrixD(3, 4, 0.1), MatrixD.ClampToZero(m, 0.01));
      Assert.AreEqual(new MatrixD(3, 4, 0.1), m);

      Assert.IsNull(MatrixD.ClampToZero(null));
      Assert.IsNull(MatrixD.ClampToZero(null, 1.0));
    }


    [Test]
    public void ToArray1D()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      double[] array = m.ToArray1D(MatrixOrder.RowMajor);
      Assert.AreEqual(12, array.Length);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], array[i]);

      m = new MatrixD(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      Assert.AreEqual(12, array.Length); 
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(columnMajor[i], array[i]);      
    }


    [Test]
    public void ToList()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      List<double> list = m.ToList(MatrixOrder.RowMajor);
      Assert.AreEqual(12, list.Count);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], list[i]);

      m = new MatrixD(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      list = m.ToList(MatrixOrder.ColumnMajor);
      Assert.AreEqual(12, list.Count);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      double[,] array = m.ToArray2D();
      Assert.AreEqual(3, array.GetLength(0));
      Assert.AreEqual(4, array.GetLength(1));
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);

      array = (double[,]) m;
      Assert.AreEqual(3, array.GetLength(0));
      Assert.AreEqual(4, array.GetLength(1));
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);

      m = null;
      array = (double[,]) m;
      Assert.IsNull(array);
    }


    [Test]
    public void ToArrayJagged()
    {
      MatrixD m = new MatrixD(3, 4, rowMajor, MatrixOrder.RowMajor);

      double[][] array = m.ToArrayJagged();
      Assert.AreEqual(3, array.Length);
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(4, array[i].Length);
        for (int j = 0; j < 4; j++)
        {
          Assert.AreEqual(i * 4 + j + 1, array[i][j]);
        }
      }

      array = (double[][]) m;
      Assert.AreEqual(3, array.Length);
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(4, array[i].Length);
        for (int j = 0; j < 4; j++)
        {
          Assert.AreEqual(i * 4 + j + 1, array[i][j]);
        }
      }

      m = null;
      array = (double[][]) m;
      Assert.IsNull(array);
    }


    [Test]
    public void ToMatrix22D()
    {
      double[] values = new double[] { 1.0, 2.0, 
                                     3.0, 4.0 };
      MatrixD m = new MatrixD(2, 2, values, MatrixOrder.RowMajor);

      Matrix22D m22 = m.ToMatrix22D();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m22[i, j]);

      m22 = (Matrix22D) m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m22[i, j]);
    }


    [Test]
    public void ToMatrix33D()
    {
      double[] values = new double[] { 1.0, 2.0, 3.0, 
                                     4.0, 5.0, 6.0, 
                                     7.0, 8.0, 9.0 };
      MatrixD m = new MatrixD(3, 3, values, MatrixOrder.RowMajor);

      Matrix33D m33 = m.ToMatrix33D();
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m33[i, j]);

      m33 = (Matrix33D) m;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m33[i, j]);
    }


    [Test]
    public void ToMatrix44D()
    {
      double[] values = new double[] { 1.0, 2.0, 3.0, 4.0, 
                                     5.0, 6.0, 7.0, 8.0, 
                                     9.0, 10.0, 11.0, 12.0, 
                                     13.0, 14.0, 15.0, 16.0 };
      MatrixD m = new MatrixD(4, 4, values, MatrixOrder.RowMajor);

      Matrix44D m44 = m.ToMatrix44D();
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m44[i, j]);

      m44 = (Matrix44D) m;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m44[i, j]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToMatrix22DException1()
    {
      MatrixD m = null;
      Matrix22D m22 = (Matrix22D) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix22DException2()
    {
      MatrixD m = new MatrixD(3, 2);
      Matrix22D m22 = m.ToMatrix22D();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix22DException3()
    {
      MatrixD m = new MatrixD(2, 3);
      Matrix22D m22 = m.ToMatrix22D();
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToMatrix33DException1()
    {
      MatrixD m = null;
      Matrix33D m33 = (Matrix33D) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix33DException2()
    {
      MatrixD m = new MatrixD(4, 3);
      Matrix33D m33 = m.ToMatrix33D();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix33DException3()
    {
      MatrixD m = new MatrixD(3, 4);
      Matrix33D m33 = m.ToMatrix33D();
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void ToMatrix44DException1()
    {
      MatrixD m = null;
      Matrix44D m44 = (Matrix44D) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix44DException2()
    {
      MatrixD m = new MatrixD(5, 4);
      Matrix44D m44 = m.ToMatrix44D();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix44DException3()
    {
      MatrixD m = new MatrixD(4, 5);
      Matrix44D m44 = m.ToMatrix44D();
    }


    [Test]
    public void ExplicitMatrixFCast()
    {
      MatrixD nullRef = null;
      Assert.IsNull((MatrixF)nullRef);

      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0; double m03 = 0.3;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0; double m13 = 0.4;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0; double m23 = 0.5;
      double m30 = 53.5; double m31 = 3.3; double m32 = -14.0; double m33 = 0.6;
      MatrixF matrixF = (MatrixF)new MatrixD(4, 4, new[] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33 }, MatrixOrder.RowMajor);
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrixF[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrixF[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrixF[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m03, matrixF[0, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrixF[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrixF[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrixF[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m13, matrixF[1, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrixF[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrixF[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrixF[2, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m23, matrixF[2, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m30, matrixF[3, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m31, matrixF[3, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m32, matrixF[3, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m33, matrixF[3, 3]));
    }


    [Test]
    public void ToMatrixF()
    {
      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0; double m03 = 0.3;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0; double m13 = 0.4;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0; double m23 = 0.5;
      double m30 = 53.5; double m31 = 3.3; double m32 = -14.0; double m33 = 0.6;
      MatrixF matrixF = new MatrixD(4, 4, new[] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33 }, MatrixOrder.RowMajor).ToMatrixF();
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrixF[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrixF[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrixF[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m03, matrixF[0, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrixF[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrixF[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrixF[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m13, matrixF[1, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrixF[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrixF[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrixF[2, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m23, matrixF[2, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m30, matrixF[3, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m31, matrixF[3, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m32, matrixF[3, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m33, matrixF[3, 3]));
    }


    [Test]
    public void SerializationXml()
    {
      MatrixD m1 = new MatrixD(new double[,] {{1.1, 2, 3, 4},
                                              {2, 5, 8, 3},
                                              {7, 6, -1, 1}});
      MatrixD m2;
      string fileName = "SerializationMatrixD.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(MatrixD));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);   
      writer.Close();

      serializer = new XmlSerializer(typeof(MatrixD));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (MatrixD) serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);

      // We dont have schema.
      Assert.AreEqual(null, new MatrixD().GetSchema());
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      MatrixD m1 = new MatrixD(new double[,] {{1.1, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixD m2;
      string fileName = "SerializationMatrixD.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (MatrixD)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      MatrixD m1 = new MatrixD(new double[,] {{1.1, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixD m2;

      string fileName = "SerializationMatrixD_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(MatrixD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (MatrixD)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      MatrixD m1 = new MatrixD(new double[,] {{1.1, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixD m2;

      string fileName = "SerializationMatrixD.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(MatrixD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (MatrixD)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }


    /* Not available in PCL build.
    [Test]
    [ExpectedException(typeof(SerializationException))]
    public void SerializationConstructorException()
    {
      new MatrixDSerializationTest(); // Will throw exception in serialization ctor.
    }
    private class MatrixDSerializationTest : MatrixD
    {
      public MatrixDSerializationTest()
        : base(null, new StreamingContext())
      {
      }
    }
    */
  }
}
