using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Algebra.Tests
{
  [TestFixture]
  public class Matrix33DTest
  {
    //           1, 2, 3
    // Matrix =  4, 5, 6
    //           7, 8, 9

    // in column-major layout
    double[] columnMajor = new double[] { 1.0, 4.0, 7.0f,
                                          2.0, 5.0, 8.0f,
                                          3.0, 6.0, 9.0 };

    // in row-major layout
    double[] rowMajor = new double[] { 1.0, 2.0, 3.0f,
                                       4.0, 5.0, 6.0f,
                                       7.0, 8.0, 9.0 };

    [SetUp]
    public void SetUp()
    {
      // Initialize random generator with a specific seed.
      RandomHelper.Random = new Random(123456);
    }


    [Test]
    public void Constants()
    {
      Matrix33D zero = Matrix33D.Zero;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(0.0, zero[i]);

      Matrix33D one = Matrix33D.One;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(1.0, one[i]);
    }


    [Test]
    public void Constructors()
    {
      Matrix33D m = new Matrix33D(1.0, 2.0, 3.0f,
                                  4.0, 5.0, 6.0f,
                                  7.0, 8.0, 9.0);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(new List<double>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(new List<double>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(new double[3, 3] { { 1, 2, 3 }, 
                                          { 4, 5, 6 }, 
                                          { 7, 8, 9 } });
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix33D(new double[3][] { new double[3] { 1, 2, 3 }, 
                                         new double[3] { 4, 5, 6 }, 
                                         new double[3] { 7, 8, 9 } });
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }


    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void ConstructorException1()
    {
      new Matrix33D(new double[3][]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ConstructorException2()
    {
      double[][] elements = new double[3][];
      elements[0] = new double[3];
      elements[1] = new double[2];
      new Matrix33D(elements);
    }


    [Test]
    public void Properties()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(1.0, m.M00);
      Assert.AreEqual(2.0, m.M01);
      Assert.AreEqual(3.0, m.M02);
      Assert.AreEqual(4.0, m.M10);
      Assert.AreEqual(5.0, m.M11);
      Assert.AreEqual(6.0, m.M12);
      Assert.AreEqual(7.0, m.M20);
      Assert.AreEqual(8.0, m.M21);
      Assert.AreEqual(9.0, m.M22);

      m = Matrix33D.Zero;
      m.M00 = 1.0;
      m.M01 = 2.0;
      m.M02 = 3.0;
      m.M10 = 4.0;
      m.M11 = 5.0;
      m.M12 = 6.0;
      m.M20 = 7.0;
      m.M21 = 8.0;
      m.M22 = 9.0;
      Assert.AreEqual(new Matrix33D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    public void Indexer1d()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = Matrix33D.Zero;
      for (int i = 0; i < 9; i++)
        m[i] = rowMajor[i];
      Assert.AreEqual(new Matrix33D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException()
    {
      Matrix33D m = new Matrix33D();
      m[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException2()
    {
      Matrix33D m = new Matrix33D();
      m[9] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException3()
    {
      Matrix33D m = new Matrix33D();
      double x = m[-1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException4()
    {
      Matrix33D m = new Matrix33D();
      double x = m[9];
    }


    [Test]
    public void Indexer2d()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      for (int column = 0; column < 3; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(columnMajor[column * 3 + row], m[row, column]);
      m = Matrix33D.Zero;
      for (int column = 0; column < 3; column++)
        for (int row = 0; row < 3; row++)
          m[row, column] = (double)(row * 3 + column + 1);
      Assert.AreEqual(new Matrix33D(rowMajor, MatrixOrder.RowMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException()
    {
      Matrix33D m = Matrix33D.Zero;
      m[0, 3] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException2()
    {
      Matrix33D m = Matrix33D.Zero;
      m[3, 0] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException3()
    {
      Matrix33D m = Matrix33D.Zero;
      m[0, -1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException4()
    {
      Matrix33D m = Matrix33D.Zero;
      m[-1, 0] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException5()
    {
      Matrix33D m = Matrix33D.Zero;
      m[1, 3] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException6()
    {
      Matrix33D m = Matrix33D.Zero;
      m[2, 3] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException7()
    {
      Matrix33D m = Matrix33D.Zero;
      m[3, 1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException8()
    {
      Matrix33D m = Matrix33D.Zero;
      m[3, 2] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException9()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[0, 3];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException10()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[3, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException11()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[0, -1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException12()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[-1, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException13()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[3, 1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException14()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[3, 2];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException15()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[1, 3];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException16()
    {
      Matrix33D m = Matrix33D.Zero;
      double x = m[2, 3];
    }


    [Test]
    public void Determinant()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      Assert.AreEqual(0.0, m.Determinant);
      Assert.AreEqual(0.0, Matrix33D.Zero.Determinant);
      Assert.AreEqual(0.0, Matrix33D.One.Determinant);
      Assert.AreEqual(1.0, Matrix33D.Identity.Determinant);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 3;
      const int numberOfColumns = 3;
      Assert.IsFalse(new Matrix33D().IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          Matrix33D m = new Matrix33D();
          m[r, c] = double.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsOrthogonal()
    {
      Assert.IsTrue(!Matrix33D.Zero.IsOrthogonal);
      Assert.IsTrue(Matrix33D.Identity.IsOrthogonal);
      Assert.IsTrue(Matrix33D.CreateRotation(new Vector3D(1, 2, 3).Normalized, 0.5).IsOrthogonal);
      Assert.IsTrue(new Matrix33D(1, 0, 0, 0, 1, 0, 0, 0, -1).IsOrthogonal);
    }


    [Test]
    public void IsRotation()
    {
      Assert.IsTrue(!Matrix33D.Zero.IsRotation);
      Assert.IsTrue(Matrix33D.Identity.IsRotation);
      Assert.IsTrue(Matrix33D.CreateRotation(new Vector3D(1, 2, 3).Normalized, 0.5).IsRotation);
      Assert.IsTrue(!new Matrix33D(1, 0, 0, 0, 1, 0, 0, 0, -1).IsRotation);
    }


    [Test]
    public void Orthogonalize()
    {
      var m = Matrix33D.CreateRotationX(0.1) * Matrix33D.CreateRotationX(20) * Matrix33D.CreateRotationZ(1000);

      // Introduce error.
      m.M01 += 0.1f;
      m.M22 += 0.1f;

      Assert.IsFalse(m.IsOrthogonal);
      Assert.IsFalse(m.IsRotation);

      m.Orthogonalize();

      Assert.IsTrue(m.IsOrthogonal);
      Assert.IsTrue(m.IsRotation);

      // Orthogonalizing and orthogonal matrix does not change the matrix.
      var n = m;
      n.Orthogonalize();
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(m, n));
    }


    [Test]
    public void IsSymmetric()
    {
      Matrix33D m = new Matrix33D(new double[3, 3] { { 1, 2, 3 }, 
                                                    { 2, 4, 5 }, 
                                                    { 3, 5, 7 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new Matrix33D(new double[3, 3] { { 1, 2, 3 }, 
                                          { 4, 5, 2 }, 
                                          { 7, 4, 1 } });
      Assert.AreEqual(false, m.IsSymmetric);
    }


    [Test]
    public void Trace()
    {
      Matrix33D m = new Matrix33D(new double[3, 3] { { 1, 2, 3 }, 
                                                    { 2, 4, 5 }, 
                                                    { 3, 5, 7 } });
      Assert.AreEqual(12, m.Trace);
    }


    [Test]
    public void Transposed()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      Matrix33D mt = new Matrix33D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
      Assert.AreEqual(Matrix33D.Identity, Matrix33D.Identity.Transposed);
    }


    [Test]
    public void Transpose()
    {
      Matrix33D m = new Matrix33D(rowMajor, MatrixOrder.RowMajor);
      m.Transpose();
      Matrix33D mt = new Matrix33D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
      Matrix33D i = Matrix33D.Identity;
      i.Transpose();
      Assert.AreEqual(Matrix33D.Identity, i);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(Matrix33D.Identity, Matrix33D.Identity.Inverse);

      Matrix33D m = new Matrix33D(1, 2, 3,
                                  2, 5, 8,
                                  7, 6, -1);
      Vector3D v = Vector3D.One;
      Vector3D w = m * v;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, m * m.Inverse));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      Matrix33D m = new Matrix33D(0.0001, 0, 0,
                                  0, 0.0001, 0,
                                  0, 0, 0.0001);
      Vector3D v = Vector3D.One;
      Vector3D w = m * v;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      Matrix33D m = Matrix33D.Zero.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(Matrix33D.Identity, Matrix33D.Identity.Inverse);

      Matrix33D m = new Matrix33D(1, 2, 3,
                                  2, 5, 8,
                                  7, 6, -1);
      Vector3D v = Vector3D.One;
      Vector3D w = m * v;
      Matrix33D im = m;
      im.Invert();
      Assert.IsTrue(Vector3D.AreNumericallyEqual(v, im * w));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, m * im));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      Matrix33D.Zero.Invert();
    }


    [Test]
    public void GetColumn()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector3D(1.0, 4.0, 7.0), m.GetColumn(0));
      Assert.AreEqual(new Vector3D(2.0, 5.0, 8.0), m.GetColumn(1));
      Assert.AreEqual(new Vector3D(3.0, 6.0, 9.0), m.GetColumn(2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException1()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException2()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(3);
    }


    [Test]
    public void SetColumn()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(0, new Vector3D(0.1, 0.2, 0.3));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetColumn(0));
      Assert.AreEqual(new Vector3D(2.0, 5.0, 8.0), m.GetColumn(1));
      Assert.AreEqual(new Vector3D(3.0, 6.0, 9.0), m.GetColumn(2));

      m.SetColumn(1, new Vector3D(0.4, 0.5, 0.6));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetColumn(0));
      Assert.AreEqual(new Vector3D(0.4, 0.5, 0.6), m.GetColumn(1));
      Assert.AreEqual(new Vector3D(3.0, 6.0, 9.0), m.GetColumn(2));

      m.SetColumn(2, new Vector3D(0.7, 0.8, 0.9));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetColumn(0));
      Assert.AreEqual(new Vector3D(0.4, 0.5, 0.6), m.GetColumn(1));
      Assert.AreEqual(new Vector3D(0.7, 0.8, 0.9), m.GetColumn(2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException1()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(-1, Vector3D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException2()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(3, Vector3D.One);
    }


    [Test]
    public void GetRow()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector3D(1.0, 2.0, 3.0), m.GetRow(0));
      Assert.AreEqual(new Vector3D(4.0, 5.0, 6.0), m.GetRow(1));
      Assert.AreEqual(new Vector3D(7.0, 8.0, 9.0), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException1()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException2()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(3);
    }


    [Test]
    public void SetRow()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(0, new Vector3D(0.1, 0.2, 0.3));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetRow(0));
      Assert.AreEqual(new Vector3D(4.0, 5.0, 6.0), m.GetRow(1));
      Assert.AreEqual(new Vector3D(7.0, 8.0, 9.0), m.GetRow(2));

      m.SetRow(1, new Vector3D(0.4, 0.5, 0.6));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetRow(0));
      Assert.AreEqual(new Vector3D(0.4, 0.5, 0.6), m.GetRow(1));
      Assert.AreEqual(new Vector3D(7.0, 8.0, 9.0), m.GetRow(2));

      m.SetRow(2, new Vector3D(0.7, 0.8, 0.9));
      Assert.AreEqual(new Vector3D(0.1, 0.2, 0.3), m.GetRow(0));
      Assert.AreEqual(new Vector3D(0.4, 0.5, 0.6), m.GetRow(1));
      Assert.AreEqual(new Vector3D(0.7, 0.8, 0.9), m.GetRow(2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException1()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(-1, Vector3D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException2()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(3, Vector3D.One);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Matrix33D m0 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix33D(0.000001);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix33D(0.00000001);

      Assert.IsTrue(Matrix33D.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(Matrix33D.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(m0, m2));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Matrix33D m0 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix33D(0.002);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix33D(0.0001);

      Assert.IsTrue(Matrix33D.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(Matrix33D.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(m0, m2, epsilon));
    }


    [Test]
    public void CreateScale()
    {
      Matrix33D i = Matrix33D.CreateScale(1.0);
      Assert.AreEqual(Matrix33D.Identity, i);

      Vector3D v = Vector3D.One;
      Matrix33D m = Matrix33D.CreateScale(2.0);
      Assert.AreEqual(2 * v, m * v);

      m = Matrix33D.CreateScale(-1.0, 1.5, 2.0);
      Assert.AreEqual(new Vector3D(-1.0, 1.5, 2.0), m * v);

      Vector3D scale = new Vector3D(-2.0, -3.0, -4.0);
      m = Matrix33D.CreateScale(scale);
      v = new Vector3D(1.0, 2.0, 3.0);
      Assert.AreEqual(v * scale, m * v);
    }


    [Test]
    public void CreateRotation()
    {
      Matrix33D m = Matrix33D.CreateRotation(Vector3D.UnitX, 0.0);
      Assert.AreEqual(Matrix33D.Identity, m);

      m = Matrix33D.CreateRotation(Vector3D.UnitX, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitZ, m * Vector3D.UnitY));

      m = Matrix33D.CreateRotation(Vector3D.UnitY, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitX, m * Vector3D.UnitZ));

      m = Matrix33D.CreateRotation(Vector3D.UnitZ, (double)Math.PI / 2);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(Vector3D.UnitY, m * Vector3D.UnitX));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateRotationException()
    {
      Matrix33D.CreateRotation(Vector3D.Zero, 1);
    }


    [Test]
    public void CreateRotationX()
    {
      double angle = (double)MathHelper.ToRadians(30.0);
      Matrix33D m = Matrix33D.CreateRotationX(angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(0, (double)Math.Cos(angle), (double)Math.Sin(angle)), m * Vector3D.UnitY));

      QuaternionD q = QuaternionD.CreateRotation(Vector3D.UnitX, angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(q.Rotate(Vector3D.One), m * Vector3D.One));

      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.CreateRotation(Vector3D.UnitX, angle), m));
    }


    [Test]
    public void CreateRotationY()
    {
      double angle = (double)MathHelper.ToRadians(30);
      Matrix33D m = Matrix33D.CreateRotationY(angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D((double)Math.Sin(angle), 0, (double)Math.Cos(angle)), m * Vector3D.UnitZ));

      QuaternionD q = QuaternionD.CreateRotation(Vector3D.UnitY, angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(q.Rotate(Vector3D.One), m * Vector3D.One));

      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.CreateRotation(Vector3D.UnitY, angle), m));
    }


    [Test]
    public void CreateRotationZ()
    {
      double angle = (double)MathHelper.ToRadians(30);
      Matrix33D m = Matrix33D.CreateRotationZ(angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D((double)Math.Cos(angle), (double)Math.Sin(angle), 0), m * Vector3D.UnitX));

      QuaternionD q = QuaternionD.CreateRotation(Vector3D.UnitZ, angle);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(q.Rotate(Vector3D.One), m * Vector3D.One));

      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.CreateRotation(Vector3D.UnitZ, angle), m));
    }


    [Test]
    public void FromQuaternion()
    {
      double angle = -1.6;
      Vector3D axis = new Vector3D(1.0, 2.0, -3.0);
      Matrix33D matrix = Matrix33D.CreateRotation(axis, angle);
      QuaternionD q = QuaternionD.CreateRotation(axis, angle);
      Matrix33D matrixFromQuaternion = Matrix33D.CreateRotation(q);
      Vector3D v = new Vector3D(0.3, -2.4, 5.6);
      Vector3D result1 = matrix * v;
      Vector3D result2 = matrixFromQuaternion * v;
      Assert.IsTrue(Vector3D.AreNumericallyEqual(result1, result2));
    }


    [Test]
    public void HashCode()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreNotEqual(Matrix33D.Identity.GetHashCode(), m.GetHashCode());
    }


    [Test]
    public void TestEquals()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      for (int i = 0; i < 9; i++)
      {
        m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1.Equals(m2));
      }

      Assert.IsFalse(m1.Equals(m1.ToString()));
    }


    [Test]
    public void EqualityOperators()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 != m2);
      for (int i = 0; i < 9; i++)
      {
        m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1 == m2);
        Assert.IsTrue(m1 != m2);
      }
    }


    [Test]
    public void TestToString()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void NegationOperator()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);
    }


    [Test]
    public void Negation()
    {
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(-rowMajor[i], Matrix33D.Negate(m)[i]);
    }


    [Test]
    public void AdditionOperator()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix33D result = m1 + m2;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] * 4, result[i]);
    }


    [Test]
    public void Addition()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = Matrix33D.One;
      Matrix33D result = Matrix33D.Add(m1, m2);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] + 1.0, result[i]);
    }


    [Test]
    public void SubtractionOperator()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix33D result = m1 - m2;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void Subtraction()
    {
      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = Matrix33D.One;
      Matrix33D result = Matrix33D.Subtract(m1, m2);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] - 1.0, result[i]);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double s = 0.1234;
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = s * m;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = m * s;
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void Multiplication()
    {
      double s = 0.1234;
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix33D.Multiply(s, m);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void DivisionOperator()
    {
      double s = 0.1234;
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = m / s;
      for (int i = 0; i < 9; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void Division()
    {
      double s = 0.1234;
      Matrix33D m = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix33D.Divide(m, s);
      for (int i = 0; i < 9; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      Matrix33D m = new Matrix33D(12, 23, 45,
                                67, 89, 90,
                                43, 65, 87);
      Assert.AreEqual(Matrix33D.Zero, m * Matrix33D.Zero);
      Assert.AreEqual(Matrix33D.Zero, Matrix33D.Zero * m);
      Assert.AreEqual(m, m * Matrix33D.Identity);
      Assert.AreEqual(m, Matrix33D.Identity * m);
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, m * m.Inverse));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, m.Inverse * m));

      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(12, 23, 45,
                                 67, 89, 90,
                                 43, 65, 87);
      Matrix33D result = m1 * m2;
      for (int column = 0; column < 3; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(Vector3D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      Matrix33D m = new Matrix33D(12, 23, 45,
                                67, 89, 90,
                                43, 65, 87);
      Assert.AreEqual(Matrix33D.Zero, Matrix33D.Multiply(m, Matrix33D.Zero));
      Assert.AreEqual(Matrix33D.Zero, Matrix33D.Multiply(Matrix33D.Zero, m));
      Assert.AreEqual(m, Matrix33D.Multiply(m, Matrix33D.Identity));
      Assert.AreEqual(m, Matrix33D.Multiply(Matrix33D.Identity, m));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, Matrix33D.Multiply(m, m.Inverse)));
      Assert.IsTrue(Matrix33D.AreNumericallyEqual(Matrix33D.Identity, Matrix33D.Multiply(m.Inverse, m)));

      Matrix33D m1 = new Matrix33D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix33D m2 = new Matrix33D(12, 23, 45,
                                 67, 89, 90,
                                 43, 65, 87);
      Matrix33D result = Matrix33D.Multiply(m1, m2);
      for (int column = 0; column < 3; column++)
        for (int row = 0; row < 3; row++)
          Assert.AreEqual(Vector3D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      Vector3D v = new Vector3D(2.34, 3.45, 4.56);
      Assert.AreEqual(v, Matrix33D.Identity * v);
      Assert.AreEqual(Vector3D.Zero, Matrix33D.Zero * v);

      Matrix33D m = new Matrix33D(12, 23, 45,
                                67, 89, 90,
                                43, 65, 87);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 3; i++)
        Assert.AreEqual(Vector3D.Dot(m.GetRow(i), v), (m * v)[i]);
    }    


    [Test]
    public void MultiplyVector()
    {
      Vector3D v = new Vector3D(2.34, 3.45, 4.56);
      Assert.AreEqual(v, Matrix33D.Multiply(Matrix33D.Identity, v));
      Assert.AreEqual(Vector3D.Zero, Matrix33D.Multiply(Matrix33D.Zero, v));

      Matrix33D m = new Matrix33D(12, 23, 45,
                                67, 89, 90,
                                43, 65, 87);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(v, Matrix33D.Multiply(m * m.Inverse, v)));

      for (int i = 0; i < 3; i++)
        Assert.AreEqual(Vector3D.Dot(m.GetRow(i), v), Matrix33D.Multiply(m, v)[i]);
    }


    [Test]
    public void MultiplyTransposed()
    {
      var m = RandomHelper.Random.NextMatrix33D(1, 10);
      var v = RandomHelper.Random.NextVector3D(1, 10);

      Assert.AreEqual(m.Transposed * v, Matrix33D.MultiplyTransposed(m, v));
    }


    [Test]
    public void ExplicitMatrix33FCast()
    {
      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0;
      Matrix33F matrix33F = (Matrix33F)new Matrix33D(m00, m01, m02, m10, m11, m12, m20, m21, m22);
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix33F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix33F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrix33F[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix33F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix33F[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrix33F[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrix33F[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrix33F[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrix33F[2, 2]));
    }


    [Test]
    public void ToMatrix33F()
    {
      double m00 = 23.5; double m01 = 0.0; double m02 = -11.0;
      double m10 = 33.5; double m11 = 1.1; double m12 = -12.0;
      double m20 = 43.5; double m21 = 2.2; double m22 = -13.0;
      Matrix33F matrix33F = new Matrix33D(m00, m01, m02, m10, m11, m12, m20, m21, m22).ToMatrix33F();
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix33F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix33F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m02, matrix33F[0, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix33F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix33F[1, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m12, matrix33F[1, 2]));
      Assert.IsTrue(Numeric.AreEqual((float)m20, matrix33F[2, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m21, matrix33F[2, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m22, matrix33F[2, 2]));
    }


    [Test]
    public void SerializationXml()
    {
      Matrix33D m1 = new Matrix33D(12, 23, 45,
                                  67, 89, 90,
                                  43, 65, 87.3);
      Matrix33D m2;

      string fileName = "SerializationMatrix33D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Matrix33D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Matrix33D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (Matrix33D)serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      Matrix33D m1 = new Matrix33D(12, 23, 45,
                                  67, 89, 90,
                                  43, 65, 87.3);
      Matrix33D m2;

      string fileName = "SerializationMatrix33D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Matrix33D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (Matrix33D)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      Matrix33D m1 = new Matrix33D(12, 23, 45,
                                  67, 89, 90,
                                  43, 65, 87.3);
      Matrix33D m2;

      string fileName = "SerializationMatrix33D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Matrix33D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (Matrix33D)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Matrix33D m1 = new Matrix33D(12, 23, 45,
                                   56, 67, 89,
                                   90, 12, 43.3);
      Matrix33D m2;

      string fileName = "SerializationMatrix33D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (Matrix33D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void Absolute()
    {
      Matrix33D absoluteM = new Matrix33D(-1, -2, -3, -4, -5, -6, -7, -8, -9);
      absoluteM.Absolute();

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M10);
      Assert.AreEqual(5, absoluteM.M11);
      Assert.AreEqual(6, absoluteM.M12);
      Assert.AreEqual(7, absoluteM.M20);
      Assert.AreEqual(8, absoluteM.M21);
      Assert.AreEqual(9, absoluteM.M22);

      absoluteM = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);
      absoluteM.Absolute();
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M10);
      Assert.AreEqual(5, absoluteM.M11);
      Assert.AreEqual(6, absoluteM.M12);
      Assert.AreEqual(7, absoluteM.M20);
      Assert.AreEqual(8, absoluteM.M21);
      Assert.AreEqual(9, absoluteM.M22);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Matrix33D m = new Matrix33D(-1, -2, -3, -4, -5, -6, -7, -8, -9);
      Matrix33D absoluteM = Matrix33D.Absolute(m);

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M10);
      Assert.AreEqual(5, absoluteM.M11);
      Assert.AreEqual(6, absoluteM.M12);
      Assert.AreEqual(7, absoluteM.M20);
      Assert.AreEqual(8, absoluteM.M21);
      Assert.AreEqual(9, absoluteM.M22);

      m = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);
      absoluteM = Matrix33D.Absolute(m);
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M02);
      Assert.AreEqual(4, absoluteM.M10);
      Assert.AreEqual(5, absoluteM.M11);
      Assert.AreEqual(6, absoluteM.M12);
      Assert.AreEqual(7, absoluteM.M20);
      Assert.AreEqual(8, absoluteM.M21);
      Assert.AreEqual(9, absoluteM.M22);
    }


    [Test]
    public void ClampToZero()
    {
      Matrix33D m = new Matrix33D(0.0000000000001);
      m.ClampToZero();
      Assert.AreEqual(new Matrix33D(), m);

      m = new Matrix33D(0.1);
      m.ClampToZero();
      Assert.AreEqual(new Matrix33D(0.1), m);

      m = new Matrix33D(0.001);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix33D(), m);

      m = new Matrix33D(0.1);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix33D(0.1), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      Matrix33D m = new Matrix33D(0.0000000000001);
      Assert.AreEqual(new Matrix33D(), Matrix33D.ClampToZero(m));
      Assert.AreEqual(new Matrix33D(0.0000000000001), m); // m unchanged?

      m = new Matrix33D(0.1);
      Assert.AreEqual(new Matrix33D(0.1), Matrix33D.ClampToZero(m));
      Assert.AreEqual(new Matrix33D(0.1), m);

      m = new Matrix33D(0.001);
      Assert.AreEqual(new Matrix33D(), Matrix33D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix33D(0.001), m);

      m = new Matrix33D(0.1);
      Assert.AreEqual(new Matrix33D(0.1), Matrix33D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix33D(0.1), m);
    }


    [Test]
    public void ToArray1D()
    {
      Matrix33D m = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);
      double[] array = m.ToArray1D(MatrixOrder.RowMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], array[i]);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(columnMajor[i], array[i]);
    }


    [Test]
    public void ToList()
    {
      Matrix33D m = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);
      IList<double> list = m.ToList(MatrixOrder.RowMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(rowMajor[i], list[i]);
      list = m.ToList(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 9; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      Matrix33D m = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);

      double[,] array = m.ToArray2D();
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, array[i, j]);

      array = (double[,])m;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, array[i, j]);
    }


    [Test]
    public void ToArrayJagged()
    {
      Matrix33D m = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);

      double[][] array = m.ToArrayJagged();
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, array[i][j]);

      array = (double[][])m;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, array[i][j]);
    }


    [Test]
    public void ToMatrixD()
    {
      Matrix33D m33 = new Matrix33D(1, 2, 3, 4, 5, 6, 7, 8, 9);

      MatrixD m = m33.ToMatrixD();
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m[i, j]);

      m = m33;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          Assert.AreEqual(i * 3 + j + 1, m[i, j]);
    }
  }
}
