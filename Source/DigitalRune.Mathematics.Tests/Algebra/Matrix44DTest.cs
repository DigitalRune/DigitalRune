using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class Matrix44DTest
  {
    //           1,  2,  3,  4
    // Matrix =  5,  6,  7,  8
    //           9, 10, 11, 12,
    //          13, 14, 15, 16 

    // in column-major layout
    private readonly double[] columnMajor = new[] { 1.0, 5.0, 9.0, 13.0f,
                                                   2.0, 6.0, 10.0, 14.0f,
                                                   3.0, 7.0, 11.0, 15.0f,
                                                   4.0, 8.0, 12.0, 16.0 };

    // in row-major layout
    private readonly double[] rowMajor = new[] { 1.0, 2.0, 3.0, 4.0, 
                                                5.0, 6.0, 7.0, 8.0, 
                                                9.0, 10.0, 11.0, 12.0f,
                                                13.0, 14.0, 15.0, 16.0 };

    [Test]
    public void Constants()
    {
      Matrix44D zero = Matrix44D.Zero;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(0.0, zero[i]);

      Matrix44D one = Matrix44D.One;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(1.0, one[i]);
    }


    [Test]
    public void Constructors()
    {
      Matrix44D m = new Matrix44D(1.0, 2.0, 3.0, 4.0f,
                                  5.0, 6.0, 7.0, 8.0f,
                                  9.0, 10.0, 11.0, 12.0f,
                                  13.0, 14.0, 15.0, 16.0);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(new List<double>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(new List<double>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(new double[4, 4] { { 1, 2, 3, 4 }, 
                                          { 5, 6, 7, 8 }, 
                                          { 9, 10, 11, 12 }, 
                                          { 13, 14, 15, 16}});
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(new double[4][] { new double[4] { 1, 2, 3, 4 }, 
                                         new double[4] { 5, 6, 7, 8 }, 
                                         new double[4] { 9, 10, 11, 12 },
                                         new double[4] { 13, 14, 15, 16}});
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix44D(new Matrix33D(1, 2, 3,
                                      4, 5, 6,
                                      7, 8, 9),
                        new Vector3D(10, 11, 12));
      Assert.AreEqual(new Matrix44D(1, 2, 3, 10,
                                    4, 5, 6, 11,
                                    7, 8, 9, 12,
                                    0, 0, 0, 1), m);
    }


    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void ConstructorException1()
    {
      new Matrix44D(new double[4][]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ConstructorException2()
    {
      double[][] elements = new double[4][];
      elements[0] = new double[4];
      elements[1] = new double[3];
      new Matrix44D(elements);
    }


    [Test]
    public void Properties()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(1.0, m.M00);
      Assert.AreEqual(2.0, m.M01);
      Assert.AreEqual(3.0, m.M02);
      Assert.AreEqual(4.0, m.M03);
      Assert.AreEqual(5.0, m.M10);
      Assert.AreEqual(6.0, m.M11);
      Assert.AreEqual(7.0, m.M12);
      Assert.AreEqual(8.0, m.M13);
      Assert.AreEqual(9.0, m.M20);
      Assert.AreEqual(10.0, m.M21);
      Assert.AreEqual(11.0, m.M22);
      Assert.AreEqual(12.0, m.M23);
      Assert.AreEqual(13.0, m.M30);
      Assert.AreEqual(14.0, m.M31);
      Assert.AreEqual(15.0, m.M32);
      Assert.AreEqual(16.0, m.M33);

      m = Matrix44D.Zero;
      m.M00 = 1.0;
      m.M01 = 2.0;
      m.M02 = 3.0;
      m.M03 = 4.0;
      m.M10 = 5.0;
      m.M11 = 6.0;
      m.M12 = 7.0;
      m.M13 = 8.0;
      m.M20 = 9.0;
      m.M21 = 10.0;
      m.M22 = 11.0;
      m.M23 = 12.0;
      m.M30 = 13.0;
      m.M31 = 14.0;
      m.M32 = 15.0;
      m.M33 = 16.0;
      Assert.AreEqual(new Matrix44D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    public void Indexer1d()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = Matrix44D.Zero;
      for (int i = 0; i < 16; i++)
        m[i] = rowMajor[i];
      Assert.AreEqual(new Matrix44D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException()
    {
      Matrix44D m = new Matrix44D();
      m[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException2()
    {
      Matrix44D m = new Matrix44D();
      m[16] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException3()
    {
      Matrix44D m = new Matrix44D();
      double x = m[-1];
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException4()
    {
      Matrix44D m = new Matrix44D();
      double x = m[16];
    }


    [Test]
    public void Indexer2d()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 4; row++)
          Assert.AreEqual(columnMajor[column * 4 + row], m[row, column]);
      m = Matrix44D.Zero;
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 4; row++)
          m[row, column] = (double)(row * 4 + column + 1);
      Assert.AreEqual(new Matrix44D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException()
    {
      Matrix44D m = Matrix44D.Zero;
      m[0, 4] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException2()
    {
      Matrix44D m = Matrix44D.Zero;
      m[4, 0] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException3()
    {
      Matrix44D m = Matrix44D.Zero;
      m[3, -1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException4()
    {
      Matrix44D m = Matrix44D.Zero;
      m[-1, 3] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException5()
    {
      Matrix44D m = Matrix44D.Zero;
      m[1, 4] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException6()
    {
      Matrix44D m = Matrix44D.Zero;
      m[2, 4] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException7()
    {
      Matrix44D m = Matrix44D.Zero;
      m[4, 1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException8()
    {
      Matrix44D m = Matrix44D.Zero;
      m[4, 2] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException9()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[0, 4];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException10()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[4, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException11()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[3, -1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException12()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[-1, 3];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException13()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[4, 1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException14()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[4, 2];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException15()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[1, 4];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException16()
    {
      Matrix44D m = Matrix44D.Zero;
      double x = m[2, 4];
    }


    [Test]
    public void GetMinor()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix33D minor = m.Minor;
      Matrix33D expected = new Matrix33D(1.0, 2.0, 3.0f,
                                       5.0, 6.0, 7.0f,
                                       9.0, 10.0, 11.0);
      Assert.AreEqual(expected, minor);
    }


    [Test]
    public void SetMinor()
    {
      Matrix44D m = Matrix44D.Zero;
      Matrix33D minor = new Matrix33D(1.0, 2.0, 3.0f,
                                    5.0, 6.0, 7.0f,
                                    9.0, 10.0, 11.0);
      m.Minor = minor;
      Assert.AreEqual(minor, m.Minor);
    }


    [Test]
    public void Rotation()
    {
      double angle = 0.3;
      Vector3D axis = new Vector3D(1.0, 2.0, 3.0);
      Matrix44D m = Matrix44D.CreateRotation(axis, angle);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(QuaternionD.CreateRotation(axis, angle).ToRotationMatrix44(), m));
    }


    [Test]
    public void Translation()
    {
      Vector3D translation = new Vector3D(1.0, 2.0, 3.0);
      Matrix44D m = Matrix44D.CreateTranslation(translation);
      Assert.AreEqual(translation, m.GetColumn(3).XYZ);
    }


    [Test]
    public void Translation2()
    {
      Vector3D translation = new Vector3D(1.0, 2.0, 3.0);
      Matrix44D m = Matrix44D.CreateTranslation(1.0, 2.0, 3.0);
      Assert.AreEqual(translation, m.GetColumn(3).XYZ);
    }


    [Test]
    public void Transposed()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D mt = new Matrix44D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
      Assert.AreEqual(Matrix44D.Identity, Matrix44D.Identity.Transposed);
    }


    [Test]
    public void Transpose()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.Transpose();
      Matrix44D mt = new Matrix44D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
      Matrix44D i = Matrix44D.Identity;
      i.Transpose();
      Assert.AreEqual(Matrix44D.Identity, i);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(Matrix44D.Identity, Matrix44D.Identity.Inverse);

      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                2, 5, 8, 3,
                                7, 6, -1, 1,
                                4, 9, 7, 7);
      Vector4D v = Vector4D.One;
      Vector4D w = m * v;
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m * m.Inverse));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      Matrix44D m = new Matrix44D(0.0001f, 0, 0, 0,
                                  0, 0.0001f, 0, 0,
                                  0, 0, 0.0001f, 0,
                                  0, 0, 0, 0.0001f);
      Vector4D v = Vector4D.One;
      Vector4D w = m * v;
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      Matrix44D m = Matrix44D.Zero.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(Matrix44D.Identity, Matrix44D.Identity.Inverse);

      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                2, 5, 8, 3,
                                7, 6, -1, 1,
                                4, 9, 7, 7);
      Vector4D v = Vector4D.One;
      Vector4D w = m * v;
      Matrix44D im = m;
      im.Invert();
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, im * w));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m * im));

      m = new Matrix44D(0.4, 34, 0.33, 4,
                                2, 5, -8, 3,
                                7, 0, -1, 1,
                                4, 9, -7, -45);
      v = Vector4D.One;
      w = m * v;
      im = m;
      im.Invert();
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, im * w));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m * im));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      Matrix44D.Zero.Invert();
    }


    [Test]
    public void Determinant()
    {
      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                5, 6, 7, 8,
                                9, 10, 11, 12,
                                13, 14, 15, 16);
      Assert.AreEqual(0, m.Determinant);

      m = new Matrix44D(1, 2, 3, 4,
                       -3, 4, 5, 6,
                       2, -5, 7, 4,
                       10, 2, -3, 9);
      Assert.AreEqual(1142, m.Determinant);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 4;
      const int numberOfColumns = 4;
      Assert.IsFalse(new Matrix44D().IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          Matrix44D m = new Matrix44D();
          m[r, c] = double.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsSymmetric()
    {
      Matrix44D m = new Matrix44D(new double[4, 4] { { 1, 2, 3, 4 }, 
                                                    { 2, 4, 5, 6 }, 
                                                    { 3, 5, 7, 8 }, 
                                                    { 4, 6, 8, 9 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new Matrix44D(new double[4, 4] { { 4, 3, 2, 1 }, 
                                          { 6, 5, 4, 2 }, 
                                          { 8, 7, 5, 3 }, 
                                          { 9, 8, 6, 4 } });
      Assert.AreEqual(false, m.IsSymmetric);
    }


    [Test]
    public void Trace()
    {
      Matrix44D m = new Matrix44D(new double[4, 4] { { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 10, 11, 12 }, { 13, 14, 15, 16 } });
      Assert.AreEqual(34, m.Trace);
    }



    [Test]
    public void GetColumn()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new Vector4D(1.0, 5.0, 9.0, 13.0), m.GetColumn(0));
      Assert.AreEqual(new Vector4D(2.0, 6.0, 10.0, 14.0), m.GetColumn(1));
      Assert.AreEqual(new Vector4D(3.0, 7.0, 11.0, 15.0), m.GetColumn(2));
      Assert.AreEqual(new Vector4D(4.0, 8.0, 12.0, 16.0), m.GetColumn(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException1()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException2()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.GetColumn(4);
    }


    [Test]
    public void SetColumn()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(0, new Vector4D(0.1, 0.2, 0.3, 0.4));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetColumn(0));
      Assert.AreEqual(new Vector4D(2.0, 6.0, 10.0, 14.0), m.GetColumn(1));
      Assert.AreEqual(new Vector4D(3.0, 7.0, 11.0, 15.0), m.GetColumn(2));
      Assert.AreEqual(new Vector4D(4.0, 8.0, 12.0, 16.0), m.GetColumn(3));

      m.SetColumn(1, new Vector4D(0.4, 0.5, 0.6, 0.7));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetColumn(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetColumn(1));
      Assert.AreEqual(new Vector4D(3.0, 7.0, 11.0, 15.0), m.GetColumn(2));
      Assert.AreEqual(new Vector4D(4.0, 8.0, 12.0, 16.0), m.GetColumn(3));

      m.SetColumn(2, new Vector4D(0.7, 0.8, 0.9, 1.0));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetColumn(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetColumn(1));
      Assert.AreEqual(new Vector4D(0.7, 0.8, 0.9, 1.0), m.GetColumn(2));
      Assert.AreEqual(new Vector4D(4.0, 8.0, 12.0, 16.0), m.GetColumn(3));

      m.SetColumn(3, new Vector4D(1.1, 1.8, 1.9, 1.2));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetColumn(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetColumn(1));
      Assert.AreEqual(new Vector4D(0.7, 0.8, 0.9, 1.0), m.GetColumn(2));
      Assert.AreEqual(new Vector4D(1.1, 1.8, 1.9, 1.2), m.GetColumn(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException1()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(-1, Vector4D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException2()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetColumn(4, Vector4D.One);
    }


    [Test]
    public void GetRow()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(new Vector4D(1.0, 2.0, 3.0, 4.0), m.GetRow(0));
      Assert.AreEqual(new Vector4D(5.0, 6.0, 7.0, 8.0), m.GetRow(1));
      Assert.AreEqual(new Vector4D(9.0, 10.0, 11.0, 12.0), m.GetRow(2));
      Assert.AreEqual(new Vector4D(13.0, 14.0, 15.0, 16.0), m.GetRow(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException1()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException2()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.GetRow(4);
    }


    [Test]
    public void SetRow()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetRow(0, new Vector4D(0.1, 0.2, 0.3, 0.4));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetRow(0));
      Assert.AreEqual(new Vector4D(5.0, 6.0, 7.0, 8.0), m.GetRow(1));
      Assert.AreEqual(new Vector4D(9.0, 10.0, 11.0, 12.0), m.GetRow(2));
      Assert.AreEqual(new Vector4D(13.0, 14.0, 15.0, 16.0), m.GetRow(3));

      m.SetRow(1, new Vector4D(0.4, 0.5, 0.6, 0.7));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetRow(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetRow(1));
      Assert.AreEqual(new Vector4D(9.0, 10.0, 11.0, 12.0), m.GetRow(2));
      Assert.AreEqual(new Vector4D(13.0, 14.0, 15.0, 16.0), m.GetRow(3));

      m.SetRow(2, new Vector4D(0.7, 0.8, 0.9, 1.0));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetRow(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetRow(1));
      Assert.AreEqual(new Vector4D(0.7, 0.8, 0.9, 1.0), m.GetRow(2));
      Assert.AreEqual(new Vector4D(13.0, 14.0, 15.0, 16.0), m.GetRow(3));

      m.SetRow(3, new Vector4D(1.7, 1.8, 1.9, 1.3));
      Assert.AreEqual(new Vector4D(0.1, 0.2, 0.3, 0.4), m.GetRow(0));
      Assert.AreEqual(new Vector4D(0.4, 0.5, 0.6, 0.7), m.GetRow(1));
      Assert.AreEqual(new Vector4D(0.7, 0.8, 0.9, 1.0), m.GetRow(2));
      Assert.AreEqual(new Vector4D(1.7, 1.8, 1.9, 1.3), m.GetRow(3));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException1()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetRow(-1, Vector4D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException2()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m.SetRow(4, Vector4D.One);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Matrix44D m0 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m1 += new Matrix44D(0.000001);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m2 += new Matrix44D(0.00000001);

      Assert.IsTrue(Matrix44D.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(Matrix44D.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(m0, m2));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Matrix44D m0 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m1 += new Matrix44D(0.002);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m2 += new Matrix44D(0.0001);

      Assert.IsTrue(Matrix44D.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(Matrix44D.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(m0, m2, epsilon));
    }


    [Test]
    public void CreateScale()
    {
      Matrix44D i = Matrix44D.CreateScale(1.0);
      Assert.AreEqual(Matrix44D.Identity, i);

      Vector4D v = Vector4D.One;
      Matrix44D m = Matrix44D.CreateScale(2.0);
      Vector4D scaled = m * v;
      Assert.AreEqual(2 * v.X, scaled.X);
      Assert.AreEqual(2 * v.Y, scaled.Y);
      Assert.AreEqual(2 * v.Z, scaled.Z);
      Assert.AreEqual(1.0, scaled.W);


      m = Matrix44D.CreateScale(-1.0, 1.5, 2.0);
      scaled = m * v;
      Assert.AreEqual(-1.0 * v.X, scaled.X);
      Assert.AreEqual(1.5 * v.Y, scaled.Y);
      Assert.AreEqual(2.0 * v.Z, scaled.Z);
      Assert.AreEqual(1.0, scaled.W);

      Vector3D scale = new Vector3D(-2.0, -3.0, -4.0);
      m = Matrix44D.CreateScale(scale);
      v = new Vector4D(1.0, 2.0, 3.0, 1.0);
      scaled = m * v;
      Assert.AreEqual(-2.0 * v.X, scaled.X);
      Assert.AreEqual(-3.0 * v.Y, scaled.Y);
      Assert.AreEqual(-4.0 * v.Z, scaled.Z);
      Assert.AreEqual(1.0, scaled.W);
    }


    [Test]
    public void CreateRotation()
    {
      Matrix44D m = Matrix44D.CreateRotation(Vector3D.UnitX, 0.0);
      Assert.AreEqual(Matrix44D.Identity, m);

      m = Matrix44D.CreateRotation(Vector3D.UnitX, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitZ, m.TransformPosition(Vector3D.UnitY)));

      m = Matrix44D.CreateRotation(Vector3D.UnitY, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitX, m.TransformPosition(Vector3D.UnitZ)));

      m = Matrix44D.CreateRotation(Vector3D.UnitZ, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitY, m.TransformPosition(Vector3D.UnitX)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateRotationException()
    {
      Matrix44D.CreateRotation(Vector3D.Zero, 1);
    }


    [Test]
    public void CreateRotationX()
    {
      double angle = (double)MathHelper.ToRadians(30);
      Matrix44D m = Matrix44D.CreateRotationX(angle);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.CreateRotation(Vector3D.UnitX, angle), m));
    }


    [Test]
    public void CreateRotationY()
    {
      double angle = (double)MathHelper.ToRadians(30);
      Matrix44D m = Matrix44D.CreateRotationY(angle);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.CreateRotation(Vector3D.UnitY, angle), m));
    }


    [Test]
    public void CreateRotationZ()
    {
      double angle = MathHelper.ToRadians(30.0);
      Matrix44D m = Matrix44D.CreateRotationZ(angle);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.CreateRotation(Vector3D.UnitZ, angle), m));
    }


    [Test]
    public void FromQuaternion()
    {
      double angle = -1.6;
      Vector3D axis = new Vector3D(1.0, 2.0, -3.0);
      Matrix44D matrix = Matrix44D.CreateRotation(axis, angle);
      QuaternionD q = QuaternionD.CreateRotation(axis, angle);
      Matrix44D matrixFromQuaternion = Matrix44D.CreateRotation(q);
      Vector4D v = new Vector4D(0.3, -2.4, 5.6, 1.0);
      Vector4D result1 = matrix * v;
      Vector4D result2 = matrixFromQuaternion * v;
      Assert.IsTrue(Vector4D.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void HashCode()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreNotEqual(Matrix44D.Identity.GetHashCode(), m.GetHashCode());
    }


    [Test]
    public void TestEquals()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      for (int i = 0; i < 16; i++)
      {
        m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1.Equals(m2));
      }

      Assert.IsFalse(m1.Equals(m1.ToString()));
    }


    [Test]
    public void EqualityOperators()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 != m2);
      for (int i = 0; i < 16; i++)
      {
        m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1 == m2);
        Assert.IsTrue(m1 != m2);
      }
    }


    [Test]
    public void TestToString()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void NegationOperator()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);
    }


    [Test]
    public void Negation()
    {
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(-rowMajor[i], Matrix44D.Negate(m)[i]);
    }


    [Test]
    public void AdditionOperator()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor) * 3;
      Matrix44D result = m1 + m2;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] * 4, result[i]);
    }


    [Test]
    public void Addition()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = Matrix44D.One;
      Matrix44D result = Matrix44D.Add(m1, m2);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] + 1.0, result[i]);
    }


    [Test]
    public void SubtractionOperator()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(rowMajor, MatrixOrder.RowMajor) * 3;
      Matrix44D result = m1 - m2;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void Subtraction()
    {
      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = Matrix44D.One;
      Matrix44D result = Matrix44D.Subtract(m1, m2);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] - 1.0, result[i]);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double s = 0.1234;
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = s * m;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = m * s;
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void Multiplication()
    {
      double s = 0.1234;
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = Matrix44D.Multiply(s, m);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void DivisionOperator()
    {
      double s = 0.1234;
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = m / s;
      for (int i = 0; i < 16; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void Division()
    {
      double s = 0.1234;
      Matrix44D m = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      m = Matrix44D.Divide(m, s);
      for (int i = 0; i < 16; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      Matrix44D m = new Matrix44D(12, 23, 45, 56,
                                  67, 89, 90, 12,
                                  43, 65, 87, 43,
                                  34, -12, 84, 44);
      Assert.AreEqual(Matrix44D.Zero, m * Matrix44D.Zero);
      Assert.AreEqual(Matrix44D.Zero, Matrix44D.Zero * m);
      Assert.AreEqual(m, m * Matrix44D.Identity);
      Assert.AreEqual(m, Matrix44D.Identity * m);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m * m.Inverse));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, m.Inverse * m));

      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(12, 23, 45, 56,
                                   67, 89, 90, 12,
                                   43, 65, 87, 43,
                                   34, -12, 84, 44);
      Matrix44D result = m1 * m2;
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 4; row++)
          Assert.AreEqual(Vector4D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      Matrix44D m = new Matrix44D(12, 23, 45, 56,
                                  67, 89, 90, 12,
                                  43, 65, 87, 43,
                                  34, -12, 84, 44);
      Assert.AreEqual(Matrix44D.Zero, Matrix44D.Multiply(m, Matrix44D.Zero));
      Assert.AreEqual(Matrix44D.Zero, Matrix44D.Multiply(Matrix44D.Zero, m));
      Assert.AreEqual(m, Matrix44D.Multiply(m, Matrix44D.Identity));
      Assert.AreEqual(m, Matrix44D.Multiply(Matrix44D.Identity, m));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, Matrix44D.Multiply(m, m.Inverse)));
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(Matrix44D.Identity, Matrix44D.Multiply(m.Inverse, m)));

      Matrix44D m1 = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      Matrix44D m2 = new Matrix44D(12, 23, 45, 56,
                                   67, 89, 90, 12,
                                   43, 65, 87, 43,
                                   34, -12, 84, 44);
      Matrix44D result = Matrix44D.Multiply(m1, m2);
      for (int column = 0; column < 4; column++)
        for (int row = 0; row < 4; row++)
          Assert.AreEqual(Vector4D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      Vector4D v = new Vector4D(2.34, 3.45, 4.56, 23.4);
      Assert.AreEqual(v, Matrix44D.Identity * v);
      Assert.AreEqual(Vector4D.Zero, Matrix44D.Zero * v);

      Matrix44D m = new Matrix44D(12, 23, 45, 56,
                                  67, 89, 90, 12,
                                  43, 65, 87, 43,
                                  34, -12, 84, 44);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 4; i++)
        Assert.AreEqual(Vector4D.Dot(m.GetRow(i), v), (m * v)[i]);
    }


    [Test]
    public void MultiplyVector()
    {
      Vector4D v = new Vector4D(2.34, 3.45, 4.56, 23.4);
      Assert.AreEqual(v, Matrix44D.Multiply(Matrix44D.Identity, v));
      Assert.AreEqual(Vector4D.Zero, Matrix44D.Multiply(Matrix44D.Zero, v));

      Matrix44D m = new Matrix44D(12, 23, 45, 56,
                                  67, 89, 90, 12,
                                  43, 65, 87, 43,
                                  34, -12, 84, 44);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(v, Matrix44D.Multiply(m * m.Inverse, v)));

      for (int i = 0; i < 4; i++)
        Assert.AreEqual(Vector4D.Dot(m.GetRow(i), v), Matrix44D.Multiply(m, v)[i]);
    }


    [Test]
    public void ExplicitMatrix44FCast()
    {
      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0; double m03 = 0.3;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0; double m13 = 0.4;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0; double m23 = 0.5;
      double m30 = 53.5; double m31 = 3.3; double m32 = -14.0; double m33 = 0.6;
      Matrix44F matrix44F = (Matrix44F)new Matrix44D(m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33);
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix44F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix44F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrix44F[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m03, matrix44F[0, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix44F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix44F[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrix44F[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m13, matrix44F[1, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrix44F[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrix44F[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrix44F[2, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m23, matrix44F[2, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m30, matrix44F[3, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m31, matrix44F[3, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m32, matrix44F[3, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m33, matrix44F[3, 3]));
    }


    [Test]
    public void ToMatrix44F()
    {
      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0; double m03 = 0.3;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0; double m13 = 0.4;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0; double m23 = 0.5;
      double m30 = 53.5; double m31 = 3.3; double m32 = -14.0; double m33 = 0.6;
      Matrix44F matrix44F = new Matrix44D(m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33).ToMatrix44F();
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix44F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix44F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrix44F[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m03, matrix44F[0, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix44F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix44F[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrix44F[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m13, matrix44F[1, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrix44F[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrix44F[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrix44F[2, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m23, matrix44F[2, 3]));
      Assert.IsTrue(Numeric.AreEqual((float)m30, matrix44F[3, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m31, matrix44F[3, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m32, matrix44F[3, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m33, matrix44F[3, 3]));
    }


    [Test]
    public void TransformPosition()
    {
      Matrix44D scale = Matrix44D.CreateScale(2.5);
      Matrix44D rotation = Matrix44D.CreateRotation(Vector3D.One, 0.3);
      Matrix44D translation = Matrix44D.CreateTranslation(new Vector3D(1.0, 2.0, 3.0));

      // Random transformation
      Matrix44D transform = translation * rotation * scale * translation.Inverse * rotation.Inverse;
      Vector4D v4 = new Vector4D(1.0, 2.0, 0.5, 1.0);
      Vector3D v3 = new Vector3D(1.0, 2.0, 0.5);

      v4 = transform * v4;
      v3 = transform.TransformPosition(v3);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(v4.X / v4.W, v4.Y / v4.W, v4.Z / v4.W), v3));

      // Test that involves a homogenous coordinate W component.
      translation = Matrix44D.CreateTranslation(2, 4, 6);
      translation.M33 = 2;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(5, 8, 11) / 2, translation.TransformPosition(new Vector3D(3, 4, 5))));
    }


    [Test]
    public void TransformVector()
    {
      Matrix44D scale = Matrix44D.CreateScale(2.5);
      Matrix44D rotation = Matrix44D.CreateRotation(Vector3D.One, 0.3);
      Matrix44D translation = Matrix44D.CreateTranslation(new Vector3D(1.0, 2.0, 3.0));

      // Random transformation
      Matrix44D transform = translation * rotation * scale * translation.Inverse * rotation.Inverse;
      Vector4D p1 = new Vector4D(1.0, 2.0, 0.5, 1.0);
      Vector4D p2 = new Vector4D(-3.4, 5.5, -0.5, 1.0);
      Vector4D d = p1 - p2;
      Vector3D v = new Vector3D(d.X, d.Y, d.Z);

      p1 = transform * p1;
      p1 /= p1.W;
      p2 = transform * p2;
      p2 /= p2.W;
      d = p1 - p2;
      v = transform.TransformDirection(v);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(d.X, d.Y, d.Z), v));
    }


    [Test]
    public void TransformNormal()
    {
      // Random matrix
      Matrix44D transform = new Matrix44D(1, 2, 3, 4,
                                2, 5, 8, 3,
                                7, 6, -1, 1,
                                0, 0, 0, 1);

      Vector3D p3 = new Vector3D(1.0, 2.0, 0.5);
      Vector3D x3 = new Vector3D(-3.4, 5.5, -0.5);
      Vector3D d = (x3 - p3);
      Vector3D n3 = d.Orthonormal1;

      Vector4D p4 = new Vector4D(p3.X, p3.Y, p3.Z, 1.0);
      Vector4D x4 = new Vector4D(x3.X, x3.Y, x3.Z, 1.0);
      Vector4D n4 = new Vector4D(n3.X, n3.Y, n3.Z, 0.0);
      double planeEquation = Vector4D.Dot((x4 - p4), n4);
      Assert.IsTrue(Numeric.IsZero(planeEquation));

      p4 = transform * p4;
      x4 = transform * x4;
      n3 = transform.TransformNormal(n3);
      n4 = new Vector4D(n3.X, n3.Y, n3.Z, 0.0);
      planeEquation = Vector4D.Dot((x4 - p4), n4);
      Assert.IsTrue(Numeric.IsZero(planeEquation));
    }


    [Test]
    public void SerializationXml()
    {
      Matrix44D m1 = new Matrix44D(12, 23, 45, 56,
                                67, 89, 90, 12,
                                43, 65, 87, 43,
                                34, -12, 84, 44.3);
      Matrix44D m2;

      string fileName = "SerializationMatrix44D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Matrix44D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Matrix44D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (Matrix44D)serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Matrix44D m1 = new Matrix44D(12, 23, 45, 56,
                                   67, 89, 90, 12,
                                   43, 65, 87, 43,
                                   34, -12, 84, 44.3);
      Matrix44D m2;

      string fileName = "SerializationMatrix44D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (Matrix44D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      Matrix44D m1 = new Matrix44D(12, 23, 45, 56,
                                   67, 89, 90, 12,
                                   43, 65, 87, 43,
                                   34, -12, 84, 44.3);
      Matrix44D m2;

      string fileName = "SerializationMatrix44D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Matrix44D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (Matrix44D)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      Matrix44D m1 = new Matrix44D(12, 23, 45, 56,
                                   67, 89, 90, 12,
                                   43, 65, 87, 43,
                                   34, -12, 84, 44.3);
      Matrix44D m2;

      string fileName = "SerializationMatrix44D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Matrix44D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (Matrix44D)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }

    [Test]
    public void Absolute()
    {
      Matrix44D absoluteM = new Matrix44D(-1, -2, -3, -4,
                                          -5, -6, -7, -8,
                                          -9, -10, -11, -12,
                                          -13, -14, -15, -16);
      absoluteM.Absolute();

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M03);
      Assert.AreEqual(5, absoluteM.M10);
      Assert.AreEqual(6, absoluteM.M11);
      Assert.AreEqual(7, absoluteM.M12);
      Assert.AreEqual(8, absoluteM.M13);
      Assert.AreEqual(9, absoluteM.M20);
      Assert.AreEqual(10, absoluteM.M21);
      Assert.AreEqual(11, absoluteM.M22);
      Assert.AreEqual(12, absoluteM.M23);
      Assert.AreEqual(13, absoluteM.M30);
      Assert.AreEqual(14, absoluteM.M31);
      Assert.AreEqual(15, absoluteM.M32);
      Assert.AreEqual(16, absoluteM.M33);

      absoluteM = new Matrix44D(1, 2, 3, 4,
                                5, 6, 7, 8,
                                9, 10, 11, 12,
                                13, 14, 15, 16);
      absoluteM.Absolute();
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M03);
      Assert.AreEqual(5, absoluteM.M10);
      Assert.AreEqual(6, absoluteM.M11);
      Assert.AreEqual(7, absoluteM.M12);
      Assert.AreEqual(8, absoluteM.M13);
      Assert.AreEqual(9, absoluteM.M20);
      Assert.AreEqual(10, absoluteM.M21);
      Assert.AreEqual(11, absoluteM.M22);
      Assert.AreEqual(12, absoluteM.M23);
      Assert.AreEqual(13, absoluteM.M30);
      Assert.AreEqual(14, absoluteM.M31);
      Assert.AreEqual(15, absoluteM.M32);
      Assert.AreEqual(16, absoluteM.M33);
    }

    [Test]
    public void AbsoluteStatic()
    {
      Matrix44D m = new Matrix44D(-1, -2, -3, -4,
                                  -5, -6, -7, -8,
                                  -9, -10, -11, -12,
                                  -13, -14, -15, -16);
      Matrix44D absoluteM = Matrix44D.Absolute(m);

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M03);
      Assert.AreEqual(5, absoluteM.M10);
      Assert.AreEqual(6, absoluteM.M11);
      Assert.AreEqual(7, absoluteM.M12);
      Assert.AreEqual(8, absoluteM.M13);
      Assert.AreEqual(9, absoluteM.M20);
      Assert.AreEqual(10, absoluteM.M21);
      Assert.AreEqual(11, absoluteM.M22);
      Assert.AreEqual(12, absoluteM.M23);
      Assert.AreEqual(13, absoluteM.M30);
      Assert.AreEqual(14, absoluteM.M31);
      Assert.AreEqual(15, absoluteM.M32);
      Assert.AreEqual(16, absoluteM.M33);

      m = new Matrix44D(1, 2, 3, 4,
                        5, 6, 7, 8,
                        9, 10, 11, 12,
                        13, 14, 15, 16);
      absoluteM = Matrix44D.Absolute(m);
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M03);
      Assert.AreEqual(5, absoluteM.M10);
      Assert.AreEqual(6, absoluteM.M11);
      Assert.AreEqual(7, absoluteM.M12);
      Assert.AreEqual(8, absoluteM.M13);
      Assert.AreEqual(9, absoluteM.M20);
      Assert.AreEqual(10, absoluteM.M21);
      Assert.AreEqual(11, absoluteM.M22);
      Assert.AreEqual(12, absoluteM.M23);
      Assert.AreEqual(13, absoluteM.M30);
      Assert.AreEqual(14, absoluteM.M31);
      Assert.AreEqual(15, absoluteM.M32);
      Assert.AreEqual(16, absoluteM.M33);
    }


    [Test]
    public void ClampToZero()
    {
      Matrix44D m = new Matrix44D(0.0000000000001);
      m.ClampToZero();
      Assert.AreEqual(new Matrix44D(), m);

      m = new Matrix44D(0.1);
      m.ClampToZero();
      Assert.AreEqual(new Matrix44D(0.1), m);

      m = new Matrix44D(0.001);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix44D(), m);

      m = new Matrix44D(0.1);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix44D(0.1), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      Matrix44D m = new Matrix44D(0.0000000000001);
      Assert.AreEqual(new Matrix44D(), Matrix44D.ClampToZero(m));
      Assert.AreEqual(new Matrix44D(0.0000000000001), m); // m unchanged?

      m = new Matrix44D(0.1);
      Assert.AreEqual(new Matrix44D(0.1), Matrix44D.ClampToZero(m));
      Assert.AreEqual(new Matrix44D(0.1), m);

      m = new Matrix44D(0.001);
      Assert.AreEqual(new Matrix44D(), Matrix44D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix44D(0.001), m);

      m = new Matrix44D(0.1);
      Assert.AreEqual(new Matrix44D(0.1), Matrix44D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix44D(0.1), m);
    }


    [Test]
    public void ToArray1D()
    {
      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                  5, 6, 7, 8,
                                  9, 10, 11, 12,
                                  13, 14, 15, 16);
      double[] array = m.ToArray1D(MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], array[i]);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(columnMajor[i], array[i]);
    }


    [Test]
    public void ToList()
    {
      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                  5, 6, 7, 8,
                                  9, 10, 11, 12,
                                  13, 14, 15, 16);
      IList<double> list = m.ToList(MatrixOrder.RowMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(rowMajor[i], list[i]);
      list = m.ToList(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 16; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                  5, 6, 7, 8,
                                  9, 10, 11, 12, 13,
                                  14, 15, 16);

      double[,] array = m.ToArray2D();
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);

      array = (double[,])m;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i, j]);
    }


    [Test]
    public void ToArrayJagged()
    {
      Matrix44D m = new Matrix44D(1, 2, 3, 4,
                                  5, 6, 7, 8,
                                  9, 10, 11, 12,
                                  13, 14, 15, 16);

      double[][] array = m.ToArrayJagged();
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i][j]);

      array = (double[][])m;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, array[i][j]);
    }


    [Test]
    public void ToMatrixD()
    {
      Matrix44D m44 = new Matrix44D(1, 2, 3, 4,
                                    5, 6, 7, 8,
                                    9, 10, 11, 12,
                                    13, 14, 15, 16);

      MatrixD m = m44.ToMatrixD();
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m[i, j]);

      m = m44;
      for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
          Assert.AreEqual(i * 4 + j + 1, m[i, j]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Matrix xna = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
      Matrix44D v = (Matrix44D)xna;

      Assert.AreEqual(xna.M11, v.M00);
      Assert.AreEqual(xna.M12, v.M10);
      Assert.AreEqual(xna.M13, v.M20);
      Assert.AreEqual(xna.M14, v.M30);
      Assert.AreEqual(xna.M21, v.M01);
      Assert.AreEqual(xna.M22, v.M11);
      Assert.AreEqual(xna.M23, v.M21);
      Assert.AreEqual(xna.M24, v.M31);
      Assert.AreEqual(xna.M31, v.M02);
      Assert.AreEqual(xna.M32, v.M12);
      Assert.AreEqual(xna.M33, v.M22);
      Assert.AreEqual(xna.M34, v.M32);
      Assert.AreEqual(xna.M41, v.M03);
      Assert.AreEqual(xna.M42, v.M13);
      Assert.AreEqual(xna.M43, v.M23);
      Assert.AreEqual(xna.M44, v.M33);
    }


    [Test]
    public void FromXna()
    {
      Matrix xna = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
      Matrix44D v = Matrix44D.FromXna(xna);

      Assert.AreEqual(xna.M11, v.M00);
      Assert.AreEqual(xna.M12, v.M10);
      Assert.AreEqual(xna.M13, v.M20);
      Assert.AreEqual(xna.M14, v.M30);
      Assert.AreEqual(xna.M21, v.M01);
      Assert.AreEqual(xna.M22, v.M11);
      Assert.AreEqual(xna.M23, v.M21);
      Assert.AreEqual(xna.M24, v.M31);
      Assert.AreEqual(xna.M31, v.M02);
      Assert.AreEqual(xna.M32, v.M12);
      Assert.AreEqual(xna.M33, v.M22);
      Assert.AreEqual(xna.M34, v.M32);
      Assert.AreEqual(xna.M41, v.M03);
      Assert.AreEqual(xna.M42, v.M13);
      Assert.AreEqual(xna.M43, v.M23);
      Assert.AreEqual(xna.M44, v.M33);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Matrix44D v = new Matrix44D(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
      Matrix xna = (Matrix)v;

      Assert.AreEqual(xna.M11, v.M00);
      Assert.AreEqual(xna.M12, v.M10);
      Assert.AreEqual(xna.M13, v.M20);
      Assert.AreEqual(xna.M14, v.M30);
      Assert.AreEqual(xna.M21, v.M01);
      Assert.AreEqual(xna.M22, v.M11);
      Assert.AreEqual(xna.M23, v.M21);
      Assert.AreEqual(xna.M24, v.M31);
      Assert.AreEqual(xna.M31, v.M02);
      Assert.AreEqual(xna.M32, v.M12);
      Assert.AreEqual(xna.M33, v.M22);
      Assert.AreEqual(xna.M34, v.M32);
      Assert.AreEqual(xna.M41, v.M03);
      Assert.AreEqual(xna.M42, v.M13);
      Assert.AreEqual(xna.M43, v.M23);
      Assert.AreEqual(xna.M44, v.M33);
    }


    [Test]
    public void ToXna()
    {
      Matrix44D v = new Matrix44D(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
      Matrix xna = v.ToXna();

      Assert.AreEqual(xna.M11, v.M00);
      Assert.AreEqual(xna.M12, v.M10);
      Assert.AreEqual(xna.M13, v.M20);
      Assert.AreEqual(xna.M14, v.M30);
      Assert.AreEqual(xna.M21, v.M01);
      Assert.AreEqual(xna.M22, v.M11);
      Assert.AreEqual(xna.M23, v.M21);
      Assert.AreEqual(xna.M24, v.M31);
      Assert.AreEqual(xna.M31, v.M02);
      Assert.AreEqual(xna.M32, v.M12);
      Assert.AreEqual(xna.M33, v.M22);
      Assert.AreEqual(xna.M34, v.M32);
      Assert.AreEqual(xna.M41, v.M03);
      Assert.AreEqual(xna.M42, v.M13);
      Assert.AreEqual(xna.M43, v.M23);
      Assert.AreEqual(xna.M44, v.M33);
    }


    [Test]
    public void TranslationProperty()
    {
      Vector3D translation = new Vector3D(1, 2, 3);
      Matrix44D srt = Matrix44D.CreateTranslation(translation) * Matrix44D.CreateRotation(new Vector3D(4, 5, 6), MathHelper.ToRadians(37)) * Matrix44D.CreateScale(2.3);
      Assert.AreEqual(translation, srt.Translation);

      translation = new Vector3D(-3, 0, 9);
      srt.Translation = translation;
      Assert.AreEqual(translation, srt.Translation);
    }


    [Test]
    public void DecomposeTest()
    {
      Vector3D scale = new Vector3D(1.0, 2.0, 3.0);
      QuaternionD rotation = QuaternionD.CreateRotation(new Vector3D(4, 5, 6), MathHelper.ToRadians(37));
      Vector3D translation = new Vector3D(-3.0, 0.5, 9.0);

      Matrix44D srt = Matrix44D.CreateTranslation(translation) * Matrix44D.CreateRotation(rotation) * Matrix44D.CreateScale(scale);

      Vector3D scaleOfMatrix;
      QuaternionD rotationOfMatrix;
      Vector3D translationOfMatrix;
      bool result = srt.Decompose(out scaleOfMatrix, out rotationOfMatrix, out translationOfMatrix);
      Assert.IsTrue(result);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(scale, scaleOfMatrix));
      Assert.IsTrue(QuaternionD.AreNumericallyEqual(rotation, rotationOfMatrix));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(translation, translationOfMatrix));
    }


    [Test]
    public void DecomposeWithNegativeScaleTest()
    {
      Vector3D scale = new Vector3D(-2.0, 3.0, 4.0);
      QuaternionD rotation = QuaternionD.CreateRotation(new Vector3D(4, 5, 6), MathHelper.ToRadians(37));
      Vector3D translation = new Vector3D(-3.0f, 0.5f, 9.0f);

      Matrix44D srt = Matrix44D.CreateTranslation(translation) * Matrix44D.CreateRotation(rotation) * Matrix44D.CreateScale(scale);

      Vector3D scaleOfMatrix;
      QuaternionD rotationOfMatrix;
      Vector3D translationOfMatrix;
      bool result = srt.Decompose(out scaleOfMatrix, out rotationOfMatrix, out translationOfMatrix);
      Assert.IsTrue(result);
      Matrix44D srt2 = Matrix44D.CreateTranslation(translationOfMatrix) * Matrix44D.CreateRotation(rotationOfMatrix) * Matrix44D.CreateScale(scaleOfMatrix);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt, srt2));
    }


    [Test]
    public void DecomposeShouldFail()
    {
      Matrix44D matrix = new Matrix44D();

      Vector3D scaleOfMatrix;
      QuaternionD rotationOfMatrix;
      Vector3D translationOfMatrix;
      bool result = matrix.Decompose(out scaleOfMatrix, out rotationOfMatrix, out translationOfMatrix);
      Assert.IsFalse(result);

      matrix = new Matrix44D(rowMajor, MatrixOrder.RowMajor);
      result = matrix.Decompose(out scaleOfMatrix, out rotationOfMatrix, out translationOfMatrix);
      Assert.IsFalse(result);
    }


    [Test]
    public void DecomposeWithZeroScale()
    {
      Vector3D s0;
      QuaternionD r0 = QuaternionD.CreateRotation(new Vector3D(4, 5, 6), MathHelper.ToRadians(37));
      Vector3D t0 = new Vector3D(-3.0, 0.5, 9.0);

      s0 = new Vector3D(0, -2, 3);
      Matrix44D srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      Vector3D s1;
      QuaternionD r1;
      Vector3D t1;
      bool result = srt0.Decompose(out s1, out r1, out t1);
      Matrix44D srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(result);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(-2, 0, 3);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      result = srt0.Decompose(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(result);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(2, -3, 0);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      result = srt0.Decompose(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(result);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(1, 0, 0);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);
      result = srt0.Decompose(out s1, out r1, out t1);
      Assert.IsFalse(result);

      s0 = new Vector3D(0, 1, 0);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);
      result = srt0.Decompose(out s1, out r1, out t1);
      Assert.IsFalse(result);

      s0 = new Vector3D(0, 0, 1);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);
      result = srt0.Decompose(out s1, out r1, out t1);
      Assert.IsFalse(result);
    }


    [Test]
    public void DecomposeFast()
    {
      Vector3D s0;
      QuaternionD r0 = QuaternionD.CreateRotation(new Vector3D(4, 5, 6), MathHelper.ToRadians(37));
      Vector3D t0 = new Vector3D(-3.0, 0.5, 9.0);

      s0 = new Vector3D(-4, -2, 3);
      Matrix44D srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      Vector3D s1;
      QuaternionD r1;
      Vector3D t1;
      srt0.DecomposeFast(out s1, out r1, out t1);
      Matrix44D srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(2, -2, 3);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);
      srt0.DecomposeFast(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(0, -2, 3);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);
      srt0.DecomposeFast(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(-2, 0, 3);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      srt0.DecomposeFast(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));

      s0 = new Vector3D(2, -3, 0);
      srt0 = Matrix44D.CreateTranslation(t0) * Matrix44D.CreateRotation(r0) * Matrix44D.CreateScale(s0);

      srt0.DecomposeFast(out s1, out r1, out t1);
      srt1 = Matrix44D.CreateTranslation(t1) * Matrix44D.CreateRotation(r1) * Matrix44D.CreateScale(s1);
      Assert.IsTrue(Matrix44D.AreNumericallyEqual(srt0, srt1));
    }


    [Test]
    public void CreateLookAtTest()
    {
      Vector3D cameraPosition = new Vector3D(100, 10, -50);
      Vector3D cameraForward = new Vector3D(0.5f, -2, -3);
      Vector3D cameraTarget = cameraPosition + cameraForward * 100;
      Vector3D cameraUpVector = new Vector3D(2, 3, 4).Normalized;
      Matrix44D view = Matrix44D.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);

      // ---- Construct a point that is at position (1, 2, -3) in view space:
      // Move point to (0, 0, -3) in view space.
      Vector3D cameraRight = Vector3D.Cross(cameraForward, cameraUpVector).Normalized;
      Vector3D cameraUp = Vector3D.Cross(cameraRight, cameraForward).Normalized;
      Vector3D point = cameraPosition + 3 * cameraForward.Normalized;
      // Move point to (0, 2, -3) in view space.
      point += 2 * cameraUp;
      // Move point to (1, 2, -3) in view space.
      point += 1 * cameraRight;

      point = view.TransformPosition(point);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 2, -3), point));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateLookAtException()
    {
      Vector3D cameraPosition = new Vector3D(100, 10, -50);
      Vector3D cameraTarget = new Vector3D(100, 10, -50);
      Vector3D cameraUpVector = new Vector3D(2, 3, 4).Normalized;
      Matrix44D.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateLookAtException2()
    {
      Vector3D cameraPosition = new Vector3D(100, 10, -50);
      Vector3D cameraTarget = new Vector3D(110, 10, -50);
      Vector3D cameraUpVector = new Vector3D(0, 0, 0);
      Matrix44D.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);
    }


    //--------------------------------------------------------------
    #region Projection Matrices
    //--------------------------------------------------------------

    [Test]
    public void CreateOrthographicTest()
    {
      Matrix44D projection = Matrix44D.CreateOrthographic(4, 3, 1, 11);

      Vector3D pointProjectionSpace = projection.TransformPosition(new Vector3D(2, 0.75, -6));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 0.5, 0.5), pointProjectionSpace));

      // zNear = 0 should be allowed.
      Matrix44D.CreateOrthographic(4, 3, 0, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreateOrthographicException()
    {
      Matrix44D.CreateOrthographic(0, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreateOrthographicException2()
    {
      Matrix44D.CreateOrthographic(4, 0, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateOrthographicException3()
    {
      Matrix44D.CreateOrthographic(4, 3, 1, 1);
    }


    [Test]
    public void OrthographicProjectionWithNegativeNear()
    {
      Matrix44D projection = Matrix44D.CreateOrthographic(4, 3, -1, 11);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(-1, 1, 0, 1), projection * new Vector4D(-2, 1.5, 1, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(1, 1, 0.5, 1), projection * new Vector4D(2, 1.5, -5, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(1, -1, 1, 1), projection * new Vector4D(2, -1.5, -11, 1)));
    }


    [Test]
    public void CreateOrthographicOffCenterTest()
    {
      Matrix44D projection = Matrix44D.CreateOrthographicOffCenter(0, 4, 0, 3, 1, 11);

      Vector3D pointProjectionSpace = projection.TransformPosition(new Vector3D(4, 2.25, -6));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 0.5, 0.5), pointProjectionSpace));

      // zNear = 0 should be allowed.
      Matrix44D.CreateOrthographicOffCenter(0, 4, 0, 3, 0, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateOrthographicOffCenterException()
    {
      Matrix44D.CreateOrthographicOffCenter(4, 4, 0, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateOrthographicOffCenterException2()
    {
      Matrix44D.CreateOrthographicOffCenter(0, 4, 3, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateOrthographicOffCenterException3()
    {
      Matrix44D.CreateOrthographicOffCenter(0, 4, 0, 3, 1, 1);
    }

    [Test]
    public void OrthographicOffCenterWithNegativeNear()
    {
      Matrix44D projection = Matrix44D.CreateOrthographicOffCenter(0, 4, 0, 3, -1, 11);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(-1, 1, 0, 1), projection * new Vector4D(0, 3, 1, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(1, 1, 0.5, 1), projection * new Vector4D(4, 3, -5, 1)));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(new Vector4D(1, -1, 1, 1), projection * new Vector4D(4, 0, -11, 1)));
    }

    [Test]
    public void CreatePerspectiveTest()
    {
      Matrix44D projection = Matrix44D.CreatePerspective(4, 3, 1, 11);

      Vector3D p = Vector4D.HomogeneousDivide(projection * new Vector4D(-2, -1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(2, 1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(0, 0, -11, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(0, 0, 1), p));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveException()
    {
      Matrix44D.CreatePerspective(0, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveException2()
    {
      Matrix44D.CreatePerspective(4, 0, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveException3()
    {
      Matrix44D.CreatePerspective(4, 3, 0, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveException4()
    {
      Matrix44D.CreatePerspective(4, 3, 1, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreatePerspectiveException5()
    {
      Matrix44D.CreatePerspective(4, 3, 1, 1);
    }

    [Test]
    public void CreatePerspectiveFieldOfViewTest()
    {
      // Use same field of view as in CreatePerspectiveTest
      double fieldOfView = 2 * Math.Atan(1.5 / 1);
      Matrix44D projection = Matrix44D.CreatePerspectiveFieldOfView(fieldOfView, 4.0 / 3.0, 1, 11);

      Vector3D p = Vector4D.HomogeneousDivide(projection * new Vector4D(-2, -1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(2, 1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(0, 0, -11, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(0, 0, 1), p));
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveFieldOfViewException()
    {
      Matrix44D.CreatePerspectiveFieldOfView(0, 4.0 / 3.0, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveFieldOfViewException2()
    {
      Matrix44D.CreatePerspectiveFieldOfView(ConstantsF.Pi, 4.0 / 3.0, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveFieldOfViewException3()
    {
      Matrix44D.CreatePerspectiveFieldOfView(ConstantsF.PiOver2, 0, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveFieldOfViewException4()
    {
      Matrix44D.CreatePerspectiveFieldOfView(ConstantsF.PiOver2, 4.0 / 3.0, 0, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveFieldOfViewException5()
    {
      Matrix44D.CreatePerspectiveFieldOfView(ConstantsF.PiOver2, 4.0 / 3.0, 1, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreatePerspectiveFieldOfViewException6()
    {
      Matrix44D.CreatePerspectiveFieldOfView(ConstantsF.PiOver2, 4.0 / 3.0, 1, 1);
    }

    [Test]
    public void CreatePerspectiveOffCenterTest()
    {
      Matrix44D projection = Matrix44D.CreatePerspectiveOffCenter(0, 4, 0, 3, 1, 11);

      Vector3D p = Vector4D.HomogeneousDivide(projection * new Vector4D(0, 0, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(4, 3, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), p));
      p = Vector4D.HomogeneousDivide(projection * new Vector4D(0, 0, -11, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 1), p));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreatePerspectiveOffCenterException()
    {
      Matrix44D.CreatePerspectiveOffCenter(4, 4, 0, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreatePerspectiveOffCenterException2()
    {
      Matrix44D.CreatePerspectiveOffCenter(0, 4, 3, 3, 1, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveOffCenterException3()
    {
      Matrix44D.CreatePerspectiveOffCenter(0, 4, 0, 3, 0, 11);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CreatePerspectiveOffCenterException4()
    {
      Matrix44D.CreatePerspectiveOffCenter(0, 4, 0, 3, 1, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreatePerspectiveOffCenterException5()
    {
      Matrix44D.CreatePerspectiveOffCenter(0, 4, 0, 3, 1, 1);
    }

    [Test]
    public void CreateInfinitePerspectiveTest()
    {
      Matrix44D pInfinite = Matrix44D.CreatePerspective(4, 3, 1, double.PositiveInfinity);

      Vector3D v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(-2, -1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), v));
      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(2, 1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), v));

      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(0, 0, -1000, 1));
      Assert.AreEqual(0, v.X);
      Assert.AreEqual(0, v.Y);
      Assert.LessOrEqual(0.999, v.Z);
    }

    [Test]
    public void CreateInfinitePerspectiveFieldOfViewTest()
    {
      // Use same field of view as in CreatePerspectiveTest
      double fieldOfView = 2 * Math.Atan(1.5 / 1);
      Matrix44D pInfinite = Matrix44D.CreatePerspectiveFieldOfView(fieldOfView, 4.0 / 3.0, 1, double.PositiveInfinity);

      Vector3D v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(-2, -1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), v));
      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(2, 1.5, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), v));

      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(0, 0, -1000, 1));
      Assert.AreEqual(0, v.X);
      Assert.AreEqual(0, v.Y);
      Assert.LessOrEqual(0.999, v.Z);
    }

    [Test]
    public void CreateInfinitePerspectiveOffCenterTest()
    {
      Matrix44D pInfinite = Matrix44D.CreatePerspectiveOffCenter(0, 4, 0, 3, 1, double.PositiveInfinity);

      Vector3D v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(0, 0, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(-1, -1, 0), v));
      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(4, 3, -1, 1));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1, 1, 0), v));

      v = Vector4D.HomogeneousDivide(pInfinite * new Vector4D(0, 0, -1000, 1));
      Assert.AreEqual(-1, v.X);
      Assert.AreEqual(-1, v.Y);
      Assert.LessOrEqual(0.999, v.Z);
    }
    #endregion
  }
}
