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
  public class MatrixFTest
  {
    //           1,  2,  3,  4
    // Matrix =  5,  6,  7,  8
    //           9, 10, 11, 12,

    // in column-major layout
    float[] columnMajor = new[] { 1.0f, 5.0f, 9.0f, 
                                  2.0f, 6.0f, 10.0f, 
                                  3.0f, 7.0f, 11.0f,
                                  4.0f, 8.0f, 12.0f };

    // in row-major layout
    float[] rowMajor = new[] { 1.0f, 2.0f, 3.0f, 4.0f, 
                               5.0f, 6.0f, 7.0f, 8.0f, 
                               9.0f, 10.0f, 11.0f, 12.0f };


    [Test]
    public void Constructors()
    {
      MatrixF m = new MatrixF(3, 4);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(0, m[i]);
      Assert.AreEqual(3, m.NumberOfRows);
      Assert.AreEqual(4, m.NumberOfColumns);

      m = new MatrixF(20, 2, 777);
      for (int i = 0; i < 20; i++)
        Assert.AreEqual(777, m[i]);
      Assert.AreEqual(20, m.NumberOfRows);
      Assert.AreEqual(2, m.NumberOfColumns);

      m = new MatrixF(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixF(3, 4, new List<float>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixF(3, 4, new List<float>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixF(new float[3, 4] { { 1, 2, 3, 4 }, 
                                        { 5, 6, 7, 8 }, 
                                        { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new MatrixF(new float[3][] { new float[4] { 1, 2, 3, 4 }, 
                                       new float[4] { 5, 6, 7, 8 }, 
                                       new float[4] { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }    


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException1()
    {
      new MatrixF(0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      new MatrixF(-1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException3()
    {
      new MatrixF(1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException4()
    {
      new MatrixF(0, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixF();
      m.Set((float[])null, MatrixOrder.ColumnMajor);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWith2DArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixF();
      m.Set((float[,])null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWith2DJaggedArrayShouldThrowArgumentNullException()
    {
      var m = new MatrixF();
      m.Set((float[][])null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithListShouldThrowArgumentNullException()
    {
      var m = new MatrixF();
      m.Set((IList<float>)null, MatrixOrder.RowMajor);
    }

    [Test]
    public void Set()
    {
      MatrixF m = new MatrixF(3, 4);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
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
      m.Set(new List<float>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new List<float>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new float[3, 4] { { 1, 2, 3, 4 }, 
                              { 5, 6, 7, 8 }, 
                              { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m.Set(0);
      m.Set(new float[3][] { new float[4] { 1, 2, 3, 4 }, 
                             new float[4] { 5, 6, 7, 8 }, 
                             new float[4] { 9, 10, 11, 12 }});
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }


    [Test]
    public void SetIdentity()
    {
      MatrixF m = new MatrixF(3, 3, 12);
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


      m = new MatrixF(10, 4, 12);
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


      m = new MatrixF(2, 5, 12);
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
      MatrixF m = new MatrixF(4, 3);
      m.Set(new MatrixF(1, 2, 777));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetException2()
    {
      MatrixF m = null;
      MatrixF n = new MatrixF(1, 1);
      n.Set(m);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException()
    {
      MatrixF m = new MatrixF(4, 3);
      m[-1] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException2()
    {
      MatrixF m = new MatrixF(4, 3);
      m[12] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException3()
    {
      MatrixF m = new MatrixF(4, 3);
      float x = m[-1];
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer1dException4()
    {
      MatrixF m = new MatrixF(4, 3);
      float x = m[12];
    }


    [Test]
    public void Indexer2d()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(columnMajor[column * 3 + row], m[row, column]);

      m = new MatrixF(3, 4);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          m[row, column] = row * 4 + column + 1;

      Assert.AreEqual(new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void Indexer2dException()
    {
      MatrixF m = new MatrixF(3, 4);
      m[0, 4] = 1.0f;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 8;
      const int numberOfColumns = 8;
      Assert.IsFalse(new MatrixF(numberOfRows, numberOfColumns).IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          MatrixF m = new MatrixF(numberOfRows, numberOfColumns);
          m[r, c] = float.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsSquare()
    {
      MatrixF m = new MatrixF(3, 3, 666);
      Assert.AreEqual(true, m.IsSquare);

      m = new MatrixF(4, 3, 666);
      Assert.AreEqual(false, m.IsSquare);

      m = new MatrixF(3, 4, 666);
      Assert.AreEqual(false, m.IsSquare);
    }


    [Test]
    public void Norm1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(24, m.Norm1);
    }


    [Test]
    public void NormFrobenius()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.IsTrue(Numeric.AreEqual(25.49509f, m.NormFrobenius));
    }


    [Test]
    public void NormInfinity()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(42, m.NormInfinity);
    }


    [Test]
    public void GetMinor()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      Assert.AreEqual(new MatrixF(new float[,] { { 6, 7, 8 }, { 10, 11, 12 } }), m.GetMinor(0, 0));
      Assert.AreEqual(new MatrixF(new float[,] { { 5, 7, 8 }, { 9, 11, 12 } }), m.GetMinor(0, 1));
      Assert.AreEqual(new MatrixF(new float[,] { { 1, 2, 3 }, { 5, 6, 7 } }), m.GetMinor(2, 3));
      Assert.AreEqual(new MatrixF(new float[,] { { 1, 3, 4 }, { 5, 7, 8 } }), m.GetMinor(2, 1));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void GetMinorException1()
    {
      MatrixF m = new MatrixF(1, 1);
      m.GetMinor(1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetMinorException2()
    {
      MatrixF m = new MatrixF(4, 3);
      m.GetMinor(4, 3);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetMinorException3()
    {
      MatrixF m = new MatrixF(4, 3);
      m.GetMinor(0, 3);
    }


    [Test]
    public void Transposed()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF mt = new MatrixF(4, 3, rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
    }


    [Test]
    public void Transpose()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.Transpose();
      MatrixF mt = new MatrixF(4, 3, rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(MatrixF.CreateIdentity(3,3), MatrixF.CreateIdentity(3,3).Inverse);

      MatrixF m = new MatrixF(new float[,] {{1, 2,  3, 4},
                                            {2, 5,  8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9,  7, 7}});
      VectorF v = new VectorF(4, 1);
      VectorF w = m * v;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4,4), m * m.Inverse));

      m = new MatrixF(new float[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      // To check the pseudo-inverse we use the definition: A*A.Transposed*A = A
      // see http://en.wikipedia.org/wiki/Moore-Penrose_pseudoinverse
      Assert.IsTrue(MatrixF.AreNumericallyEqual(m, m * m.Inverse * m));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      MatrixF m = new MatrixF(new float[,] {{0.0001f, 0, 0, 0},
                                            {0, 0.0001f, 0, 0},
                                            {0, 0, 0.0001f, 0},
                                            {0, 0,  0, 0.0001f}});
      VectorF v = new VectorF(4, 1);
      VectorF w = m * v;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      MatrixF m = new MatrixF(new float[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {3, 7, 11, 7}});
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      MatrixF m = new MatrixF(4,4).Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException3()
    {
      MatrixF m = new MatrixF(new float[,] {{1, 2, 3},
                                            {2, 5, 8},
                                            {7, 6, -1},
                                            {4, 9, 7}}).Transposed;
      MatrixF inverse = m.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(MatrixF.CreateIdentity(3, 3), MatrixF.CreateIdentity(3, 3).Inverse);

      MatrixF m = new MatrixF(new float[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9, 7, 7}});
      MatrixF inverse = m.Clone();
      m.Invert();
      VectorF v = new VectorF(4, 1);
      VectorF w = m * v;
      Assert.IsTrue(VectorF.AreNumericallyEqual(v, inverse * w));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), m * inverse));

      m = new MatrixF(new float[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      // To check the pseudo-inverse we use the definition: A*A.Transposed*A = A
      // see http://en.wikipedia.org/wiki/Moore-Penrose_pseudoinverse
      inverse = m.Clone();
      inverse.Invert();
      Assert.IsTrue(MatrixF.AreNumericallyEqual(m, m * inverse * m));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      MatrixF m = new MatrixF(new float[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {3, 7, 11, 7}});
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      MatrixF m = new MatrixF(4, 4);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException3()
    {
      MatrixF m = new MatrixF(new float[,] {{1, 2, 3},
                                            {2, 5, 8},
                                            {7, 6, -1},
                                            {4, 9, 7}}).Transposed;
      m.Invert();
    }


    [Test]
    public void TryInvert()
    {
      // Regular, square
      MatrixF m = new MatrixF(new float[,] {{1, 2, 3, 4},
                                            {2, 5, 8, 3},
                                            {7, 6, -1, 1},
                                            {4, 9, 7, 7}});
      MatrixF inverse = m.Clone();
      Assert.AreEqual(true, m.TryInvert());
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), m * inverse));

      // Full column rank, rectangular
      m = new MatrixF(new float[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {7, 6, -1},
                                    {4, 9, 7}});
      inverse = m.Clone();
      Assert.AreEqual(true, m.TryInvert());
      Assert.IsTrue(MatrixF.AreNumericallyEqual(m, m * inverse * m));

      // singular
      m = new MatrixF(new float[,] {{1, 2, 3},
                                    {2, 5, 8},
                                    {3, 7, 11}});
      inverse = m.Clone();
      Assert.AreEqual(false, m.TryInvert());
    }


    [Test]
    public void Determinant()
    {
      MatrixF m = new Matrix44F(1, 2, 3, 4,
                                5, 6, 7, 8,
                                9, 10, 11, 12,
                                13, 14, 15, 16).ToMatrixF();
      Assert.IsTrue(Numeric.IsZero(m.Determinant));

      m = new Matrix44F(1, 2, 3, 4,
                       -3, 4, 5, 6,
                       2, -5, 7, 4,
                       10, 2, -3, 9).ToMatrixF();
      Assert.AreEqual(1142, m.Determinant);
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      float det = m.Determinant;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void DeterminantException2()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      float det = m.Determinant;
    }


    [Test]
    public void IsSymmetric()
    {
      MatrixF m = new MatrixF(new float[4, 4] { { 1, 2, 3, 4 }, 
                                                { 2, 4, 5, 6 }, 
                                                { 3, 5, 7, 8 }, 
                                                { 4, 6, 8, 9 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new MatrixF(new float[4, 4] { { 4, 3, 2, 1 }, 
                                        { 6, 5, 4, 2 }, 
                                        { 8, 7, 5, 3 }, 
                                        { 9, 8, 6, 4 } });
      Assert.AreEqual(false, m.IsSymmetric);

      Assert.IsTrue(new MatrixF(2, 2, 0).IsSymmetric);
      Assert.IsFalse(new MatrixF(3, 2, 0).IsSymmetric);
      Assert.IsFalse(new MatrixF(2, 3, 0).IsSymmetric);
    }


    [Test]
    public void GetSubmatrix()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      Assert.AreEqual(m, m.GetSubmatrix(0, 2, 0, 3));
      Assert.AreEqual(new MatrixF(1, 1, 1), m.GetSubmatrix(0, 0, 0, 0));
      Assert.AreEqual(new MatrixF(1, 1, 12), m.GetSubmatrix(2, 2, 3, 3));
      Assert.AreEqual(new MatrixF(1, 1, 12), m.GetSubmatrix(2, 2, new int[] { 3 }));
      Assert.AreEqual(new MatrixF(1, 1, 4), m.GetSubmatrix(new int[] {0}, 3, 3));
      Assert.AreEqual(new MatrixF(1, 1, 10), m.GetSubmatrix(new int[] { 2 }, new int[] { 1 }));

      Assert.AreEqual(new MatrixF(new float[,] { { 5, 6, 7, 8 }, { 9, 10, 11, 12 } }), m.GetSubmatrix(1, 2, 0, 3));
      Assert.AreEqual(new MatrixF(new float[,] { { 8, 6, 7 }, { 12, 10, 11 } }), m.GetSubmatrix(1, 2, new int[] { 3, 1, 2}));
      Assert.AreEqual(new MatrixF(new float[,] { { 11, 12 }, { 7, 8 }, { 3, 4 } }), m.GetSubmatrix(new int[] { 2, 1, 0 }, 2, 3));
      Assert.AreEqual(new MatrixF(new float[,] { { 8, 7, 5, 6 }, { 12, 11, 9, 10 }, { 4, 3, 1, 2 } }), m.GetSubmatrix(new int[] { 1, 2, 0 }, new int[] {3, 2, 0, 1}));

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
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 0, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException2()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException3()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 0, new int[] { 1 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException4()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, 1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException5()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(-1, 1, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException6()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(4, 4, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException7()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 4, 0, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException8()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 0, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException9()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 4, new int[]{1});
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException10()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, -1, 0);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException11()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { -1 }, 1, 2);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException12()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(1, 2, new int[] { 4 });
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException13()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 4 }, new int[] { 2 });
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetSubmatrixException14()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 2 }, new int[] { 4 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException15()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 4, 5);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException16()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(0, 1, 0, 5);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException17()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(4, 4, new int[] {1 });
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubmatrixException18()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.GetSubmatrix(new int[] { 1 }, 2, 4);
    }


    [Test]
    public void SetSubmatrix()
    {
      MatrixF m = new MatrixF(4, 5, 0);

      m.SetSubmatrix(2, 2, new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } }));

      Assert.AreEqual(new MatrixF(new float[,] {{0, 0, 0, 0, 0},
                                                {0, 0, 0, 0, 0},
                                                {0, 0, 1, 2, 3},
                                                {0, 0, 4, 5, 6}}), m);

      m.SetSubmatrix(0, 0, new MatrixF(1, 1, 777));
      Assert.AreEqual(777, m[0, 0]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException1()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(-1, 0, new MatrixF(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException2()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(4, 0, new MatrixF(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException3()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, -2, new MatrixF(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetSubmatrixException4()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 4, new MatrixF(1, 1));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubmatrixException5()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, new MatrixF(4, 1));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubmatrixException6()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, new MatrixF(1, 4));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetSubmatrixException7()
    {
      MatrixF m = new MatrixF(4, 3, rowMajor, MatrixOrder.RowMajor);
      m.SetSubmatrix(1, 1, null);
    }


    [Test]
    public void SolveLinearEquationsMatrix()
    {
      // Regular square matrix.
      MatrixF matrixA = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixF matrixB = new MatrixF(new float[,] {{1, 2},{3, 4}, {5, 6}});

      MatrixF matrixX = MatrixF.SolveLinearEquations(matrixA, matrixB);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(matrixB, matrixA * matrixX));

      // Full column rank rectangular matrix.
      matrixA = new MatrixF(new float[,] { { 1, 2 }, { 4, 5}, { 7, -8 } });
      matrixX = MatrixF.SolveLinearEquations(matrixA, matrixB);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(matrixA.Transposed * matrixB, matrixA.Transposed * matrixA * matrixX));  // Normal equation (see least squares, Gauss transformation).
    }


    [Test]
    public void SolveLinearEquationsVector()
    {
      // Regular square matrix.
      MatrixF matrixA = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      VectorF vectorB = new VectorF(new float[] { 1, 2, 4 });

      VectorF vectorX = MatrixF.SolveLinearEquations(matrixA, vectorB);
      Assert.IsTrue(VectorF.AreNumericallyEqual(vectorB, matrixA * vectorX));

      // Full column rank rectangular matrix.
      matrixA = new MatrixF(new float[,] { { 1, 2 }, { 4, 5 }, { 7, -8 } });
      vectorX = MatrixF.SolveLinearEquations(matrixA, vectorB);
      Assert.IsTrue(VectorF.AreNumericallyEqual(matrixA.Transposed * vectorB, matrixA.Transposed * matrixA * vectorX));  // Normal equation (see least squares, Gauss transformation).
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException1()
    {
      MatrixF.SolveLinearEquations(null, new MatrixF(1, 1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException2()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixF.SolveLinearEquations(a, new MatrixF(4, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException3()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }});  // not full column rank.
      MatrixF.SolveLinearEquations(a, new MatrixF(2, 1));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException4()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixF.SolveLinearEquations(a, new MatrixF(3, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException5()
    {
      MatrixF.SolveLinearEquations(null, new VectorF(1, 1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException6()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, -9 } });
      MatrixF.SolveLinearEquations(a, new VectorF(4)); // number of rows dont fit
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SolveLinearEquationsException7()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 } });  // not full column rank.
      MatrixF.SolveLinearEquations(a, new VectorF(2));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void SolveLinearEquationsException8()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixF.SolveLinearEquations(a, new VectorF(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException9()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixF.SolveLinearEquations(a, (VectorF)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SolveLinearEquationsException10()
    {
      MatrixF a = new MatrixF(new float[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 5, 7, 9 } }); // not full rank.
      MatrixF.SolveLinearEquations(a, (MatrixF) null);
    }


    [Test]
    public void Trace()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(18, m.Trace);
    }


    [Test]
    public void GetColumn()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new VectorF(new float[] { 1.0f, 5.0f, 9.0f }), m.GetColumn(0));
      Assert.AreEqual(new VectorF(new float[] { 2.0f, 6.0f, 10.0f }), m.GetColumn(1));
      Assert.AreEqual(new VectorF(new float[] { 3.0f, 7.0f, 11.0f }), m.GetColumn(2));
      Assert.AreEqual(new VectorF(new float[] { 4.0f, 8.0f, 12.0f }), m.GetColumn(3));
    }

    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetColumnException1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetColumnException2()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(4);
    }


    [Test]
    public void SetColumn()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(0, new VectorF(new float[]{ 0.1f, 0.2f, 0.3f }));
      Assert.AreEqual(new VectorF(new float[]{ 0.1f, 0.2f, 0.3f }), m.GetColumn(0));
      Assert.AreEqual(new VectorF(new float[]{ 2.0f, 6.0f, 10.0f }), m.GetColumn(1));
      Assert.AreEqual(new VectorF(new float[]{ 3.0f, 7.0f, 11.0f }), m.GetColumn(2));
      Assert.AreEqual(new VectorF(new float[]{ 4.0f, 8.0f, 12.0f }), m.GetColumn(3));

      m.SetColumn(1, new VectorF(new float[] { 0.4f, 0.5f, 0.6f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f }), m.GetColumn(0));
      Assert.AreEqual(new VectorF(new float[] { 0.4f, 0.5f, 0.6f }), m.GetColumn(1));
      Assert.AreEqual(new VectorF(new float[] { 3.0f, 7.0f, 11.0f }), m.GetColumn(2));
      Assert.AreEqual(new VectorF(new float[] { 4.0f, 8.0f, 12.0f }), m.GetColumn(3));

      m.SetColumn(2, new VectorF(new float[] { 0.7f, 0.8f, 0.9f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f }), m.GetColumn(0));
      Assert.AreEqual(new VectorF(new float[] { 0.4f, 0.5f, 0.6f }), m.GetColumn(1));
      Assert.AreEqual(new VectorF(new float[] { 0.7f, 0.8f, 0.9f }), m.GetColumn(2));
      Assert.AreEqual(new VectorF(new float[] { 4.0f, 8.0f, 12.0f }), m.GetColumn(3));

      m.SetColumn(3, new VectorF(new float[] { 1.1f, 1.8f, 1.9f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f }), m.GetColumn(0));
      Assert.AreEqual(new VectorF(new float[] { 0.4f, 0.5f, 0.6f }), m.GetColumn(1));
      Assert.AreEqual(new VectorF(new float[] { 0.7f, 0.8f, 0.9f }), m.GetColumn(2));
      Assert.AreEqual(new VectorF(new float[] { 1.1f, 1.8f, 1.9f }), m.GetColumn(3));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetColumnException1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(-1, new VectorF(3));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetColumnException2()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(4, new VectorF(3));
    }


    [Test]
    public void GetRow()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new VectorF(new float[] { 1.0f, 2.0f, 3.0f, 4.0f }), m.GetRow(0));
      Assert.AreEqual(new VectorF(new float[] { 5.0f, 6.0f, 7.0f, 8.0f }), m.GetRow(1));
      Assert.AreEqual(new VectorF(new float[] { 9.0f, 10.0f, 11.0f, 12.0f }), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetRowException1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void GetRowException2()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.GetRow(3);
    }


    [Test]
    public void SetRow()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(0, new VectorF(new float[]{ 0.1f, 0.2f, 0.3f, 0.4f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f, 0.4f }), m.GetRow(0));
      Assert.AreEqual(new VectorF(new float[] { 5.0f, 6.0f, 7.0f, 8.0f }), m.GetRow(1));
      Assert.AreEqual(new VectorF(new float[] { 9.0f, 10.0f, 11.0f, 12.0f }), m.GetRow(2));

      m.SetRow(1, new VectorF(new float[] { 0.4f, 0.5f, 0.6f, 0.7f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f, 0.4f }), m.GetRow(0));
      Assert.AreEqual(new VectorF(new float[] { 0.4f, 0.5f, 0.6f, 0.7f }), m.GetRow(1));
      Assert.AreEqual(new VectorF(new float[] { 9.0f, 10.0f, 11.0f, 12.0f }), m.GetRow(2));

      m.SetRow(2, new VectorF(new float[] { 0.7f, 0.8f, 0.9f, 1.0f }));
      Assert.AreEqual(new VectorF(new float[] { 0.1f, 0.2f, 0.3f, 0.4f }), m.GetRow(0));
      Assert.AreEqual(new VectorF(new float[] { 0.4f, 0.5f, 0.6f, 0.7f }), m.GetRow(1));
      Assert.AreEqual(new VectorF(new float[] { 0.7f, 0.8f, 0.9f, 1.0f }), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetRowException1()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(-1, new VectorF(4));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetRowException2()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m.SetRow(3, new VectorF(4));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      MatrixF m0 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixF(3, 4, 0.000001f);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixF(3, 4, 0.00000001f);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(MatrixF.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(m0, m2));

      Assert.IsFalse(MatrixF.AreNumericallyEqual(new MatrixF(1, 2), new MatrixF(2, 2)));
      Assert.IsFalse(MatrixF.AreNumericallyEqual(new MatrixF(2, 1), new MatrixF(2, 2)));

      Assert.IsTrue(MatrixF.AreNumericallyEqual(null, null));

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void AreNumericallyEqualWithEpsilon()
    {
      float epsilon = 0.001f;

      MatrixF m0 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixF(3, 4, 0.002f);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) + new MatrixF(3, 4, 0.0001f);

      Assert.IsTrue(MatrixF.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(MatrixF.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(m0, m2, epsilon));

      Assert.IsFalse(MatrixF.AreNumericallyEqual(new MatrixF(1, 2), new MatrixF(2, 2), epsilon));
      Assert.IsFalse(MatrixF.AreNumericallyEqual(new MatrixF(2, 1), new MatrixF(2, 2), epsilon));

      Assert.IsTrue(MatrixF.AreNumericallyEqual(null, null, epsilon));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException1()
    {
      MatrixF m0 = new MatrixF(3, 4);
      MatrixF m1 = null;
      MatrixF.AreNumericallyEqual(m0, m1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException2()
    {
      MatrixF m0 = null;
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF.AreNumericallyEqual(m0, m1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException3()
    {
      MatrixF m0 = new MatrixF(3, 4);
      MatrixF m1 = null;
      MatrixF.AreNumericallyEqual(m0, m1, 0.1f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException4()
    {
      MatrixF m0 = null;
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF.AreNumericallyEqual(m0, m1, 0.1f);
    }


    [Test]
    public void HashCode()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.AreNotEqual(MatrixF.CreateIdentity(3, 4), m.GetHashCode());
      Assert.AreNotEqual(new MatrixF(3, 4), m.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF nullMatrix = null;
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      Assert.IsFalse(m1.Equals(nullMatrix));

      Assert.IsTrue(((object) m1).Equals((object) m1));
      Assert.IsTrue(((object) m1).Equals((object) m2));
      Assert.IsFalse(((object) m1).Equals((object) nullMatrix));

      m2 += new MatrixF(3, 4, 0.1f);
      Assert.IsFalse(m1.Equals(m2));
    }


    [Test]
    public void EqualityOperators()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF nullMatrix = null;
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 == nullMatrix);
      Assert.IsFalse(nullMatrix == m2);
      Assert.IsFalse(m1 != m2);
      Assert.IsTrue(m1 != nullMatrix);
      Assert.IsTrue(nullMatrix != m2);

      m2 += new MatrixF(3, 4, 0.1f);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);

      m1 = new MatrixF(1, 2);
      m2 = new MatrixF(2, 2);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);

      m1 = new MatrixF(2, 2);
      m2 = new MatrixF(2, 1);
      Assert.IsFalse(m1 == m2);
      Assert.IsTrue(m1 != m2);
    }


    [Test]
    public void ToStringTest()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void Clone()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      var o = m.Clone();
      Assert.AreEqual(m, o);
    }


    [Test]
    public void NegationOperator()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);

      m = null;
      Assert.IsNull(-m);
    }


    [Test]
    public void Negation()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i], MatrixF.Negate(m)[i]);

      m = null;
      Assert.IsNull(MatrixF.Negate(m));
    }


    [Test]
    public void AddMatrixOperator()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) * (-3);
      MatrixF result = m1 + m2;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void AddMatrix()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) * (-3);
      MatrixF result = MatrixF.Add(m1, m2);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixOperatorException1()
    {
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF m2 = null;
      MatrixF result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixOperatorException2()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixOperatorException3()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixOperatorException4()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(4, 3);
      MatrixF result = m1 + m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixException1()
    {
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF m2 = null;
      MatrixF.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddMatrixException2()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixException3()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF.Add(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddMatrixException4()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(4, 3);
      MatrixF.Add(m1, m2);
    }


    [Test]
    public void SubtractMatrixOperator()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) * 3;
      MatrixF result = m1 - m2;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void SubtractMatrix()
    {
      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor) * 3;
      MatrixF result = MatrixF.Subtract(m1, m2);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]    
    public void SubtractMatrixOperatorException1()
    {
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF m2 = null;
      MatrixF result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixOperatorException2()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixOperatorException3()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixOperatorException4()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(4, 3);
      MatrixF result = m1 - m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixException1()
    {
      MatrixF m1 = new MatrixF(3, 4);
      MatrixF m2 = null;
      MatrixF.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractMatrixException2()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixException3()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(3, 4);
      MatrixF.Subtract(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractMatrixException4()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = new MatrixF(4, 3);
      MatrixF.Subtract(m1, m2);
    }


    [Test]
    public void MultiplyScalarOperator()
    {
      float s = 0.1234f;
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = s * m;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = m * s;
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void MultipyScalar()
    {
      float s = 0.1234f;
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = MatrixF.Multiply(s, m);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultipyScalarException()
    {
      float s = 1.0f;
      MatrixF m = null;
      m = MatrixF.Multiply(s, m);
    }


    [Test]
    public void DivideByScalarOperator()
    {
      float s = 0.1234f;
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = m / s;
      for (int i = 0; i < 12; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void DivideByScalar()
    {
      float s = 0.1234f;
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      m = MatrixF.Divide(m, s);
      for (int i = 0; i < 12; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideByScalarException()
    {
      float s = 1.0f;
      MatrixF m = null;
      m = MatrixF.Divide(m, s);
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      float[] values = new float[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixF m = new MatrixF(4, 4, values, MatrixOrder.RowMajor);
      Assert.AreEqual(new MatrixF(4, 4), m * new MatrixF(4, 4));
      Assert.AreEqual(new MatrixF(4, 4), new MatrixF(4, 4) * m);
      Assert.AreEqual(m, m * MatrixF.CreateIdentity(4, 4));
      Assert.AreEqual(m, MatrixF.CreateIdentity(4, 4) * m);
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), m * m.Inverse));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), m.Inverse * m));

      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(4, 4, values, MatrixOrder.RowMajor);

      MatrixF result = m1 * m2;
      Assert.AreEqual(3, result.NumberOfRows);
      Assert.AreEqual(4, result.NumberOfColumns);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(VectorF.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      float[] values = new float[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixF m = new MatrixF(4, 4, values, MatrixOrder.RowMajor);
      Assert.AreEqual(new MatrixF(4, 4), MatrixF.Multiply(m, new MatrixF(4, 4)));
      Assert.AreEqual(new MatrixF(4, 4), MatrixF.Multiply(new MatrixF(4, 4), m));
      Assert.AreEqual(m, MatrixF.Multiply(m, MatrixF.CreateIdentity(4, 4)));
      Assert.AreEqual(m, MatrixF.Multiply(MatrixF.CreateIdentity(4, 4), m));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), MatrixF.Multiply(m, m.Inverse)));
      Assert.IsTrue(MatrixF.AreNumericallyEqual(MatrixF.CreateIdentity(4, 4), MatrixF.Multiply(m.Inverse, m)));

      MatrixF m1 = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);
      MatrixF m2 = new MatrixF(4, 4, values, MatrixOrder.RowMajor);

      MatrixF result = MatrixF.Multiply(m1, m2);
      Assert.AreEqual(3, result.NumberOfRows);
      Assert.AreEqual(4, result.NumberOfColumns);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(VectorF.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixOperatorException1()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(4, 4);
      MatrixF m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixOperatorException2()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = null;
      MatrixF m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyMatrixOperatorException3()
    {
      MatrixF m1 = new MatrixF(4, 3);
      MatrixF m2 = new MatrixF(4, 5);
      MatrixF m3 = m1 * m2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixException1()
    {
      MatrixF m1 = null;
      MatrixF m2 = new MatrixF(4, 4);
      MatrixF m3 = MatrixF.Multiply(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyMatrixException2()
    {
      MatrixF m1 = new MatrixF(4, 4);
      MatrixF m2 = null;
      MatrixF m3 = MatrixF.Multiply(m1, m2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyMatrixException3()
    {
      MatrixF m1 = new MatrixF(4, 3);
      MatrixF m2 = new MatrixF(4, 5);
      MatrixF m3 = MatrixF.Multiply(m1, m2);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      VectorF v = new VectorF(new float[] { 2.34f, 3.45f, 4.56f, 23.4f });
      Assert.AreEqual(v, MatrixF.CreateIdentity(4, 4) * v);
      Assert.AreEqual(new VectorF(4), new MatrixF(4, 4) * v);

      float[] values = new float[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixF m = new MatrixF(4, 4, values, MatrixOrder.RowMajor);
      Assert.IsTrue(VectorF.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(VectorF.Dot(m.GetRow(i), v), (m * v)[i]));
    }


    [Test]
    public void MultiplyVector()
    {
      VectorF v = new VectorF(new float[] { 2.34f, 3.45f, 4.56f, 23.4f });
      Assert.AreEqual(v, MatrixF.Multiply(MatrixF.CreateIdentity(4, 4), v));
      Assert.AreEqual(new VectorF(4), MatrixF.Multiply(new MatrixF(4, 4), v));

      float[] values = new float[] { 12, 23, 45, 56,
                                     67, 89, 90, 12,
                                     43, 65, 87, 43,
                                     34, -12, 84, 44 };
      MatrixF m = new MatrixF(4, 4, values, MatrixOrder.RowMajor);
      Assert.IsTrue(VectorF.AreNumericallyEqual(v, MatrixF.Multiply(MatrixF.Multiply(m, m.Inverse), v)));

      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(VectorF.Dot(m.GetRow(i), v), MatrixF.Multiply(m, v)[i]));
    }


    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void MultiplyVectorOperatorException1()
    {
      MatrixF m = null;
      VectorF v = new VectorF(4);
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorOperatorException2()
    {
      MatrixF m = new MatrixF(4, 4);
      VectorF v = null;
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyVectorOperatorException3()
    {
      MatrixF m = new MatrixF(4, 3);
      VectorF v = new VectorF(4);
      v = m * v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorException1()
    {
      MatrixF m = null;
      VectorF v = new VectorF(4);
      v = MatrixF.Multiply(m, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyVectorException2()
    {
      MatrixF m = new MatrixF(4, 4);
      VectorF v = null;
      v = MatrixF.Multiply(m, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyVectorException3()
    {
      MatrixF m = new MatrixF(4, 3);
      VectorF v = new VectorF(4);
      v = MatrixF.Multiply(m, v);
    }


    [Test]
    public void Absolute()
    {
      float[] values = new float[] { -1.0f, -2.0f, -3.0f, 
                                     -4.0f, -5.0f, -6.0f, 
                                     -7.0f, -8.0f, -9.0f };
      MatrixF m = new MatrixF(3, 3, values, MatrixOrder.RowMajor);

      MatrixF absolute = m.Clone();
      absolute.Absolute();
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      absolute = MatrixF.Absolute(m);
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      values = new float[] { 1.0f, 2.0f, 3.0f, 
                             4.0f, 5.0f, 6.0f, 
                             7.0f, 8.0f, 9.0f };
      m = new MatrixF(3, 3, values, MatrixOrder.RowMajor);

      absolute = m.Clone();
      absolute.Absolute();
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      absolute = MatrixF.Absolute(m);
      for (int i = 0; i < absolute.NumberOfRows; i++)
        for (int j = 0; j < absolute.NumberOfColumns; j++)
          Assert.AreEqual(i * absolute.NumberOfColumns + j + 1, absolute[i, j]);

      Assert.IsNull(MatrixF.Absolute(null));
    }


    [Test]
    public void ClampToZero()
    {
      MatrixF m = new MatrixF(5, 6, 0.000001f);
      m.ClampToZero();
      Assert.AreEqual(new MatrixF(5, 6), m);

      m = new MatrixF(5, 6, 0.1f);
      m.ClampToZero();
      Assert.AreEqual(new MatrixF(5, 6, 0.1f), m);

      m = new MatrixF(5, 6, 0.001f);
      m.ClampToZero(0.01f);
      Assert.AreEqual(new MatrixF(5, 6), m);

      m = new MatrixF(5, 6, 0.1f);
      m.ClampToZero(0.01f);
      Assert.AreEqual(new MatrixF(5, 6, 0.1f), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      MatrixF m = new MatrixF(3, 4, 0.000001f);
      Assert.AreEqual(new MatrixF(3, 4), MatrixF.ClampToZero(m));
      Assert.AreEqual(new MatrixF(3, 4, 0.000001f), m); // m unchanged?

      m = new MatrixF(3, 4, 0.1f);
      Assert.AreEqual(new MatrixF(3, 4, 0.1f), MatrixF.ClampToZero(m));
      Assert.AreEqual(new MatrixF(3, 4, 0.1f), m);

      m = new MatrixF(3, 4, 0.001f);
      Assert.AreEqual(new MatrixF(3, 4), MatrixF.ClampToZero(m, 0.01f));
      Assert.AreEqual(new MatrixF(3, 4, 0.001f), m);

      m = new MatrixF(3, 4, 0.1f);
      Assert.AreEqual(new MatrixF(3, 4, 0.1f), MatrixF.ClampToZero(m, 0.01f));
      Assert.AreEqual(new MatrixF(3, 4, 0.1f), m);

      Assert.IsNull(MatrixF.ClampToZero(null));
      Assert.IsNull(MatrixF.ClampToZero(null, 1.0f));
    }


    [Test]
    public void ToArray1D()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      float[] array = m.ToArray1D(MatrixOrder.RowMajor);
      Assert.AreEqual(12, array.Length);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], array[i]);

      m = new MatrixF(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      Assert.AreEqual(12, array.Length); 
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(columnMajor[i], array[i]);      
    }


    [Test]
    public void ToList()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      List<float> list = m.ToList(MatrixOrder.RowMajor);
      Assert.AreEqual(12, list.Count);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(rowMajor[i], list[i]);

      m = new MatrixF(3, 4, columnMajor, MatrixOrder.ColumnMajor);
      list = m.ToList(MatrixOrder.ColumnMajor);
      Assert.AreEqual(12, list.Count);
      for (int i = 0; i < 12; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      float[,] array = m.ToArray2D();
      Assert.AreEqual(3, array.GetLength(0));
      Assert.AreEqual(4, array.GetLength(1));
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);

      array = (float[,]) m;
      Assert.AreEqual(3, array.GetLength(0));
      Assert.AreEqual(4, array.GetLength(1));
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);

      m = null;
      array = (float[,]) m;
      Assert.IsNull(array);
    }


    [Test]
    public void ToArrayJagged()
    {
      MatrixF m = new MatrixF(3, 4, rowMajor, MatrixOrder.RowMajor);

      float[][] array = m.ToArrayJagged();
      Assert.AreEqual(3, array.Length);
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(4, array[i].Length);
        for (int j = 0; j < 4; j++)
        {
          Assert.AreEqual(i * 4 + j + 1, array[i][j]);
        }
      }

      array = (float[][]) m;
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
      array = (float[][]) m;
      Assert.IsNull(array);
    }


    [Test]
    public void ToMatrix22F()
    {
      float[] values = new float[] { 1.0f, 2.0f, 
                                     3.0f, 4.0f };
      MatrixF m = new MatrixF(2, 2, values, MatrixOrder.RowMajor);

      Matrix22F m22 = m.ToMatrix22F();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m22[i, j]);

      m22 = (Matrix22F) m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m22[i, j]);
    }


    [Test]
    public void ToMatrix33F()
    {
      float[] values = new float[] { 1.0f, 2.0f, 3.0f, 
                                     4.0f, 5.0f, 6.0f, 
                                     7.0f, 8.0f, 9.0f };
      MatrixF m = new MatrixF(3, 3, values, MatrixOrder.RowMajor);

      Matrix33F m33 = m.ToMatrix33F();
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m33[i, j]);

      m33 = (Matrix33F) m;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m33[i, j]);
    }


    [Test]
    public void ToMatrix44F()
    {
      float[] values = new float[] { 1.0f, 2.0f, 3.0f, 4.0f, 
                                     5.0f, 6.0f, 7.0f, 8.0f, 
                                     9.0f, 10.0f, 11.0f, 12.0f, 
                                     13.0f, 14.0f, 15.0f, 16.0f };
      MatrixF m = new MatrixF(4, 4, values, MatrixOrder.RowMajor);

      Matrix44F m44 = m.ToMatrix44F();
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m44[i, j]);

      m44 = (Matrix44F) m;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m44[i, j]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToMatrix22FException1()
    {
      MatrixF m = null;
      Matrix22F m22 = (Matrix22F) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix22FException2()
    {
      MatrixF m = new MatrixF(3, 2);
      Matrix22F m22 = m.ToMatrix22F();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix22FException3()
    {
      MatrixF m = new MatrixF(2, 3);
      Matrix22F m22 = m.ToMatrix22F();
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ToMatrix33FException1()
    {
      MatrixF m = null;
      Matrix33F m33 = (Matrix33F) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix33FException2()
    {
      MatrixF m = new MatrixF(4, 3);
      Matrix33F m33 = m.ToMatrix33F();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix33FException3()
    {
      MatrixF m = new MatrixF(3, 4);
      Matrix33F m33 = m.ToMatrix33F();
    }

    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void ToMatrix44FException1()
    {
      MatrixF m = null;
      Matrix44F m44 = (Matrix44F) m;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix44FException2()
    {
      MatrixF m = new MatrixF(5, 4);
      Matrix44F m44 = m.ToMatrix44F();
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ToMatrix44FException3()
    {
      MatrixF m = new MatrixF(4, 5);
      Matrix44F m44 = m.ToMatrix44F();
    }


    [Test]
    public void ImplicitMatrixDCast()
    {
      MatrixF nullRef = null;
      Assert.IsNull((MatrixD)nullRef);

      float m00 = 23.5f; float m01 = 0.0f; float m02 = -11.0f; float m03 = 0.3f;
      float m10 = 33.5f; float m11 = 1.1f; float m12 = -12.0f; float m13 = 0.4f;
      float m20 = 43.5f; float m21 = 2.2f; float m22 = -13.0f; float m23 = 0.5f;
      MatrixD matrixD = new MatrixF(3, 4, new [] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23 }, MatrixOrder.RowMajor);
      Assert.AreEqual(3, matrixD.NumberOfRows);
      Assert.AreEqual(4, matrixD.NumberOfColumns);
      Assert.IsTrue(Numeric.AreEqual(m00, (float)matrixD[0, 0]));
      Assert.IsTrue(Numeric.AreEqual(m01, (float)matrixD[0, 1]));
      Assert.IsTrue(Numeric.AreEqual(m02, (float)matrixD[0, 2]));
      Assert.IsTrue(Numeric.AreEqual(m03, (float)matrixD[0, 3]));
      Assert.IsTrue(Numeric.AreEqual(m10, (float)matrixD[1, 0]));
      Assert.IsTrue(Numeric.AreEqual(m11, (float)matrixD[1, 1]));
      Assert.IsTrue(Numeric.AreEqual(m12, (float)matrixD[1, 2]));
      Assert.IsTrue(Numeric.AreEqual(m13, (float)matrixD[1, 3]));
      Assert.IsTrue(Numeric.AreEqual(m20, (float)matrixD[2, 0]));
      Assert.IsTrue(Numeric.AreEqual(m21, (float)matrixD[2, 1]));
      Assert.IsTrue(Numeric.AreEqual(m22, (float)matrixD[2, 2]));
      Assert.IsTrue(Numeric.AreEqual(m23, (float)matrixD[2, 3]));
    }


    [Test]
    public void ToMatrixD()
    {
      float m00 = 23.5f; float m01 = 0.0f; float m02 = -11.0f; float m03 = 0.3f;
      float m10 = 33.5f; float m11 = 1.1f; float m12 = -12.0f; float m13 = 0.4f;
      float m20 = 43.5f; float m21 = 2.2f; float m22 = -13.0f; float m23 = 0.5f;
      MatrixD matrixD = new MatrixF(3, 4, new[] { m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23 }, MatrixOrder.RowMajor).ToMatrixD();
      Assert.AreEqual(3, matrixD.NumberOfRows);
      Assert.AreEqual(4, matrixD.NumberOfColumns);
      Assert.IsTrue(Numeric.AreEqual(m00, (float)matrixD[0, 0]));
      Assert.IsTrue(Numeric.AreEqual(m01, (float)matrixD[0, 1]));
      Assert.IsTrue(Numeric.AreEqual(m02, (float)matrixD[0, 2]));
      Assert.IsTrue(Numeric.AreEqual(m03, (float)matrixD[0, 3]));
      Assert.IsTrue(Numeric.AreEqual(m10, (float)matrixD[1, 0]));
      Assert.IsTrue(Numeric.AreEqual(m11, (float)matrixD[1, 1]));
      Assert.IsTrue(Numeric.AreEqual(m12, (float)matrixD[1, 2]));
      Assert.IsTrue(Numeric.AreEqual(m13, (float)matrixD[1, 3]));
      Assert.IsTrue(Numeric.AreEqual(m20, (float)matrixD[2, 0]));
      Assert.IsTrue(Numeric.AreEqual(m21, (float)matrixD[2, 1]));
      Assert.IsTrue(Numeric.AreEqual(m22, (float)matrixD[2, 2]));
      Assert.IsTrue(Numeric.AreEqual(m23, (float)matrixD[2, 3]));
    }


    [Test]
    public void SerializationXml()
    {
      MatrixF m1 = new MatrixF(new float[,] {{1.1f, 2, 3, 4},
                                    {2, 5, 8, 3},
                                    {7, 6, -1, 1}});
      MatrixF m2;
      string fileName = "SerializationMatrixF.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(MatrixF));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);   
      writer.Close();

      serializer = new XmlSerializer(typeof(MatrixF));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (MatrixF) serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);

      // We dont have schema.
      Assert.AreEqual(null, new MatrixF().GetSchema());
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      MatrixF m1 = new MatrixF(new float[,] {{1.1f, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixF m2;
      string fileName = "SerializationMatrixF.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (MatrixF)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      MatrixF m1 = new MatrixF(new float[,] {{1.1f, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixF m2;

      string fileName = "SerializationMatrixF_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(MatrixF));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (MatrixF)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      MatrixF m1 = new MatrixF(new float[,] {{1.1f, 2,  3, 4},
                                             {2,    5,  8, 3},
                                             {7,    6, -1, 1}});
      MatrixF m2;

      string fileName = "SerializationMatrixF.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(MatrixF));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (MatrixF)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }


    /* Not available in PCL build.
    [Test]
    [ExpectedException(typeof(SerializationException))]
    public void SerializationConstructorException()
    {
      new MatrixFSerializationTest(); // Will throw exception in serialization ctor.
    }
    private class MatrixFSerializationTest : MatrixF
    {
      public MatrixFSerializationTest()
        : base(null, new StreamingContext())
      {
      }
    }
    */
  }
}
