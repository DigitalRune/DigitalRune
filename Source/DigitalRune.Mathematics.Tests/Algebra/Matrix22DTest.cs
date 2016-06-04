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
  public class Matrix22DTest
  {
    //           | 1, 2 |
    // Matrix =  | 3, 4 |

    // in column-major layout
    readonly double[] columnMajor = new double[] { 1.0, 3.0f,
                                                   2.0, 4.0 };

    // in row-major layout
    readonly double[] rowMajor = new double[] { 1.0, 2.0, 
                                                3.0, 4.0 };


    [Test]
    public void Constants()
    {
      Matrix22D zero = Matrix22D.Zero;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(0.0, zero[i]);

      Matrix22D one = Matrix22D.One;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(1.0, one[i]);
    }


    [Test]
    public void Constructors()
    {
      Matrix22D m = new Matrix22D(1.0, 2.0, 3.0, 4.0);

      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22D(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22D(new List<double>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22D(new List<double>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22D(new double[2, 2] { { 1, 2 }, { 3, 4 } });
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22D(new double[2][] { new double[2] { 1, 2 }, new double[2] { 3, 4 } });
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }


    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void ConstructorException1()
    {
      new Matrix22D(new double[2][]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ConstructorException2()
    {
      double[][] elements = new double[2][];
      elements[0] = new double[2];
      elements[1] = new double[1];
      new Matrix22D(elements);
    }


    [Test]
    public void Properties()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(1.0, m.M00);
      Assert.AreEqual(2.0, m.M01);
      Assert.AreEqual(3.0, m.M10);
      Assert.AreEqual(4.0, m.M11);

      m = Matrix22D.Zero;
      m.M00 = 1.0;
      m.M01 = 2.0;
      m.M10 = 3.0;
      m.M11 = 4.0;
      Assert.AreEqual(new Matrix22D(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    public void Indexer1d()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = Matrix22D.Zero;
      for (int i = 0; i < 4; i++)
        m[i] = rowMajor[i];
      Assert.AreEqual(new Matrix22D(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException()
    {
      Matrix22D m = new Matrix22D();
      m[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException2()
    {
      Matrix22D m = new Matrix22D();
      m[4] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException3()
    {
      Matrix22D m = new Matrix22D();
      double x = m[-1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException4()
    {
      Matrix22D m = new Matrix22D();
      double x = m[4];
    }


    [Test]
    public void Indexer2d()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(columnMajor[column * 2 + row], m[row, column]);
      m = Matrix22D.Zero;
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          m[row, column] = (row * 2 + column + 1);
      Assert.AreEqual(new Matrix22D(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException()
    {
      Matrix22D m = Matrix22D.Zero;
      m[0, 2] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException2()
    {
      Matrix22D m = Matrix22D.Zero;
      m[2, 0] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException3()
    {
      Matrix22D m = Matrix22D.Zero;
      m[0, -1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException4()
    {
      Matrix22D m = Matrix22D.Zero;
      m[-1, 0] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException5()
    {
      Matrix22D m = Matrix22D.Zero;
      m[1, 2] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException6()
    {
      Matrix22D m = Matrix22D.Zero;
      m[2, 1] = 1.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException9()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[0, 2];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException10()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[2, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException11()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[0, -1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException12()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[-1, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException13()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[2, 1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException14()
    {
      Matrix22D m = Matrix22D.Zero;
      double x = m[1, 2];
    }


    [Test]
    public void Determinant()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(-2.0, m.Determinant);
      Assert.AreEqual(0.0, Matrix22D.Zero.Determinant);
      Assert.AreEqual(0.0, Matrix22D.One.Determinant);
      Assert.AreEqual(1.0, Matrix22D.Identity.Determinant);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 2;
      const int numberOfColumns = 2;
      Assert.IsFalse(new Matrix22D().IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          Matrix22D m = new Matrix22D();
          m[r, c] = double.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsSymmetric()
    {
      Matrix22D m = new Matrix22D(new double[2, 2] { { 1, 2 }, { 2, 4 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new Matrix22D(new double[2, 2] { { 2, 1 }, { 4, 2 } });
      Assert.AreEqual(false, m.IsSymmetric);
    }


    [Test]
    public void Trace()
    {
      Matrix22D m = new Matrix22D(new double[2, 2] { { 1, 2 }, { 2, 4 } });
      Assert.AreEqual(5, m.Trace);
    }


    [Test]
    public void Transposed()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D mt = new Matrix22D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
      Assert.AreEqual(Matrix22D.Identity, Matrix22D.Identity.Transposed);
    }


    [Test]
    public void Transpose()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.Transpose();
      Matrix22D mt = new Matrix22D(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
      Matrix22D i = Matrix22D.Identity;
      i.Transpose();
      Assert.AreEqual(Matrix22D.Identity, i);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(Matrix22D.Identity, Matrix22D.Identity.Inverse);

      Matrix22D m = new Matrix22D(1, 2, 3, 4);
      Vector2D v = Vector2D.One;
      Vector2D w = m * v;
      Assert.IsTrue(Vector2D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, m * m.Inverse));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      Matrix22D m = new Matrix22D(0.0001, 0,
                                  0, 0.0001);
      Vector2D v = Vector2D.One;
      Vector2D w = m * v;
      Assert.IsTrue(Vector2D.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      Matrix22D m = new Matrix22D(1, 2, 4, 8);
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      Matrix22D m = Matrix22D.Zero.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(Matrix22D.Identity, Matrix22D.Identity.Inverse);

      Matrix22D m = new Matrix22D(1, 2, 3, 4);
      Vector2D v = Vector2D.One;
      Vector2D w = m * v;
      Matrix22D im = m;
      im.Invert();
      Assert.IsTrue(Vector2D.AreNumericallyEqual(v, im * w));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, m * im));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      Matrix22D m = new Matrix22D(1, 2, 4, 8);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      Matrix22D.Zero.Invert();
    }


    [Test]
    public void GetColumn()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector2D(1.0, 3.0), m.GetColumn(0));
      Assert.AreEqual(new Vector2D(2.0, 4.0), m.GetColumn(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException1()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException2()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(2);
    }


    [Test]
    public void SetColumn()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(0, new Vector2D(0.1, 0.2));
      Assert.AreEqual(new Vector2D(0.1, 0.2), m.GetColumn(0));
      Assert.AreEqual(new Vector2D(2.0, 4.0), m.GetColumn(1));

      m.SetColumn(1, new Vector2D(0.4, 0.5));
      Assert.AreEqual(new Vector2D(0.1, 0.2), m.GetColumn(0));
      Assert.AreEqual(new Vector2D(0.4, 0.5), m.GetColumn(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException1()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(-1, Vector2D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException2()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(2, Vector2D.One);
    }


    [Test]
    public void GetRow()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector2D(1.0, 2.0), m.GetRow(0));
      Assert.AreEqual(new Vector2D(3.0, 4.0), m.GetRow(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException1()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException2()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(2);
    }


    [Test]
    public void SetRow()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(0, new Vector2D(0.1, 0.2));
      Assert.AreEqual(new Vector2D(0.1, 0.2), m.GetRow(0));
      Assert.AreEqual(new Vector2D(3.0, 4.0), m.GetRow(1));

      m.SetRow(1, new Vector2D(0.4, 0.5));
      Assert.AreEqual(new Vector2D(0.1, 0.2), m.GetRow(0));
      Assert.AreEqual(new Vector2D(0.4, 0.5), m.GetRow(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException1()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(-1, Vector2D.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException2()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(2, Vector2D.One);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Matrix22D m0 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix22D(0.000001);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix22D(0.00000001);

      Assert.IsTrue(Matrix22D.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(Matrix22D.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(m0, m2));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Matrix22D m0 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix22D(0.002);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix22D(0.0001);

      Assert.IsTrue(Matrix22D.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(Matrix22D.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(m0, m2, epsilon));
    }


    [Test]
    public void CreateScale()
    {
      Matrix22D i = Matrix22D.CreateScale(1.0);
      Assert.AreEqual(Matrix22D.Identity, i);

      Vector2D v = Vector2D.One;
      Matrix22D m = Matrix22D.CreateScale(2.0);
      Assert.AreEqual(2 * v, m * v);

      m = Matrix22D.CreateScale(-1.0, 1.5);
      Assert.AreEqual(new Vector2D(-1.0, 1.5), m * v);

      Vector2D scale = new Vector2D(-2.0, -3.0);
      m = Matrix22D.CreateScale(scale);
      v = new Vector2D(1.0, 2.0);
      Assert.AreEqual(v * scale, m * v);
    }


    [Test]
    public void CreateRotation()
    {
      Matrix22D m = Matrix22D.CreateRotation(0.0);
      Assert.AreEqual(Matrix22D.Identity, m);

      m = Matrix22D.CreateRotation((double)Math.PI / 2);
      Assert.IsTrue(Vector2D.AreNumericallyEqual(Vector2D.UnitY, m * Vector2D.UnitX));
    }


    [Test]
    public void CreateRotation2()
    {
      double angle = (double)MathHelper.ToRadians(30);
      Matrix22D m = Matrix22D.CreateRotation(angle);
      Assert.IsTrue(Vector2D.AreNumericallyEqual(new Vector2D((double)Math.Cos(angle), (double)Math.Sin(angle)), m * Vector2D.UnitX));
    }


    [Test]
    public void HashCode()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreNotEqual(Matrix22D.Identity.GetHashCode(), m.GetHashCode());
    }


    [Test]
    public void TestEquals()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      for (int i = 0; i < 4; i++)
      {
        m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1.Equals(m2));
      }

      Assert.IsFalse(m1.Equals(m1.ToString()));
    }


    [Test]
    public void EqualityOperators()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 != m2);
      for (int i = 0; i < 4; i++)
      {
        m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1;
        Assert.IsFalse(m1 == m2);
        Assert.IsTrue(m1 != m2);
      }
    }


    [Test]
    public void TestToString()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void NegationOperator()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);
    }


    [Test]
    public void Negation()
    {
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i], Matrix22D.Negate(m)[i]);
    }


    [Test]
    public void AdditionOperator()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix22D result = m1 + m2;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * 4, result[i]);
    }


    [Test]
    public void Addition()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = Matrix22D.One;
      Matrix22D result = Matrix22D.Add(m1, m2);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] + 1.0, result[i]);
    }


    [Test]
    public void SubtractionOperator()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix22D result = m1 - m2;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void Subtraction()
    {
      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = Matrix22D.One;
      Matrix22D result = Matrix22D.Subtract(m1, m2);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] - 1.0, result[i]);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double s = 0.1234;
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m = s * m;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m = m * s;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void Multiplication()
    {
      double s = 0.1234;
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix22D.Multiply(s, m);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void DivisionOperator()
    {
      double s = 0.1234;
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m = m / s;
      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void Division()
    {
      double s = 0.1234;
      Matrix22D m = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix22D.Divide(m, s);
      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      Matrix22D m = new Matrix22D(12, 23, 45, 67);
      Assert.AreEqual(Matrix22D.Zero, m * Matrix22D.Zero);
      Assert.AreEqual(Matrix22D.Zero, Matrix22D.Zero * m);
      Assert.AreEqual(m, m * Matrix22D.Identity);
      Assert.AreEqual(m, Matrix22D.Identity * m);
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, m * m.Inverse));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, m.Inverse * m));

      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(12, 23, 45, 67);
      Matrix22D result = m1 * m2;
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(Vector2D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      Matrix22D m = new Matrix22D(12, 23, 45, 67);
      Assert.AreEqual(Matrix22D.Zero, Matrix22D.Multiply(m, Matrix22D.Zero));
      Assert.AreEqual(Matrix22D.Zero, Matrix22D.Multiply(Matrix22D.Zero, m));
      Assert.AreEqual(m, Matrix22D.Multiply(m, Matrix22D.Identity));
      Assert.AreEqual(m, Matrix22D.Multiply(Matrix22D.Identity, m));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, Matrix22D.Multiply(m, m.Inverse)));
      Assert.IsTrue(Matrix22D.AreNumericallyEqual(Matrix22D.Identity, Matrix22D.Multiply(m.Inverse, m)));

      Matrix22D m1 = new Matrix22D(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22D m2 = new Matrix22D(12, 23, 45, 67);
      Matrix22D result = Matrix22D.Multiply(m1, m2);
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(Vector2D.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      Vector2D v = new Vector2D(2.34, 3.45);
      Assert.AreEqual(v, Matrix22D.Identity * v);
      Assert.AreEqual(Vector2D.Zero, Matrix22D.Zero * v);

      Matrix22D m = new Matrix22D(12, 23, 45, 67);
      Assert.IsTrue(Vector2D.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 2; i++)
        Assert.AreEqual(Vector2D.Dot(m.GetRow(i), v), (m * v)[i]);
    }


    [Test]
    public void MultiplyVector()
    {
      Vector2D v = new Vector2D(2.34, 3.45);
      Assert.AreEqual(v, Matrix22D.Multiply(Matrix22D.Identity, v));
      Assert.AreEqual(Vector2D.Zero, Matrix22D.Multiply(Matrix22D.Zero, v));

      Matrix22D m = new Matrix22D(12, 23, 45, 67);
      Assert.IsTrue(Vector2D.AreNumericallyEqual(v, Matrix22D.Multiply(m * m.Inverse, v)));

      for (int i = 0; i < 2; i++)
        Assert.AreEqual(Vector2D.Dot(m.GetRow(i), v), Matrix22D.Multiply(m, v)[i]);
    }


    [Test]
    public void ExplicitMatrix22FCast()
    {
      double m00 = 23.5; double m01 = 0.0;
      double m10 = 33.5; double m11 = 1.1;
      Matrix22F matrix22F = (Matrix22F)new Matrix22D(m00, m01, m10, m11);
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix22F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix22F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix22F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix22F[1, 1]));
    }


    [Test]
    public void ToMatrix22F()
    {
      double m00 = 23.5; double m01 = 0.0;
      double m10 = 33.5; double m11 = 1.1;
      Matrix22F matrix22F = new Matrix22D(m00, m01, m10, m11).ToMatrix22F();
      Assert.IsTrue(Numeric.AreEqual((float)m00, matrix22F[0, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m01, matrix22F[0, 1]));
      Assert.IsTrue(Numeric.AreEqual((float)m10, matrix22F[1, 0]));
      Assert.IsTrue(Numeric.AreEqual((float)m11, matrix22F[1, 1]));
    }


    [Test]
    public void SerializationXml()
    {
      Matrix22D m1 = new Matrix22D(12, 23, 45, 67);
      Matrix22D m2;

      string fileName = "SerializationMatrix22D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Matrix22D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Matrix22D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (Matrix22D)serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Matrix22D m1 = new Matrix22D(12, 23,
                                   45, 56.3);
      Matrix22D m2;

      string fileName = "SerializationMatrix22D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (Matrix22D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      Matrix22D m1 = new Matrix22D(12, 23, 45, 67);
      Matrix22D m2;

      string fileName = "SerializationMatrix22D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Matrix22D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (Matrix22D)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      Matrix22D m1 = new Matrix22D(12, 23, 45, 67);
      Matrix22D m2;

      string fileName = "SerializationMatrix22D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Matrix22D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (Matrix22D)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void Absolute()
    {
      Matrix22D absoluteM = new Matrix22D(-1, -2, -3, -4);
      absoluteM.Absolute();

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);

      absoluteM = new Matrix22D(1, 2, 3, 4);
      absoluteM.Absolute();
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Matrix22D m = new Matrix22D(-1, -2, -3, -4);
      Matrix22D absoluteM = Matrix22D.Absolute(m);

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);

      m = new Matrix22D(1, 2, 3, 4);
      absoluteM = Matrix22D.Absolute(m);
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);
    }


    [Test]
    public void ClampToZero()
    {
      Matrix22D m = new Matrix22D(0.0000000000001);
      m.ClampToZero();
      Assert.AreEqual(new Matrix22D(), m);

      m = new Matrix22D(0.1);
      m.ClampToZero();
      Assert.AreEqual(new Matrix22D(0.1), m);

      m = new Matrix22D(0.001);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix22D(), m);

      m = new Matrix22D(0.1);
      m.ClampToZero(0.01);
      Assert.AreEqual(new Matrix22D(0.1), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      Matrix22D m = new Matrix22D(0.0000000000001);
      Assert.AreEqual(new Matrix22D(), Matrix22D.ClampToZero(m));
      Assert.AreEqual(new Matrix22D(0.0000000000001), m); // m unchanged?

      m = new Matrix22D(0.1);
      Assert.AreEqual(new Matrix22D(0.1), Matrix22D.ClampToZero(m));
      Assert.AreEqual(new Matrix22D(0.1), m);

      m = new Matrix22D(0.001);
      Assert.AreEqual(new Matrix22D(), Matrix22D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix22D(0.001), m);

      m = new Matrix22D(0.1);
      Assert.AreEqual(new Matrix22D(0.1), Matrix22D.ClampToZero(m, 0.01));
      Assert.AreEqual(new Matrix22D(0.1), m);
    }


    [Test]
    public void ToArray1D()
    {
      Matrix22D m = new Matrix22D(1, 2, 3, 4);
      double[] array = m.ToArray1D(MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], array[i]);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(columnMajor[i], array[i]);
    }


    [Test]
    public void ToList()
    {
      Matrix22D m = new Matrix22D(1, 2, 3, 4);
      IList<double> list = m.ToList(MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], list[i]);
      list = m.ToList(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      Matrix22D m = new Matrix22D(1, 2, 3, 4);

      double[,] array = m.ToArray2D();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i, j]);

      array = (double[,])m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i, j]);
    }


    [Test]
    public void ToArrayJagged()
    {
      Matrix22D m = new Matrix22D(1, 2, 3, 4);

      double[][] array = m.ToArrayJagged();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i][j]);

      array = (double[][])m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i][j]);
    }


    [Test]
    public void ToMatrixD()
    {
      Matrix22D m22 = new Matrix22D(1, 2, 3, 4);

      MatrixD m = m22.ToMatrixD();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m[i, j]);

      m = m22;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, m[i, j]);
    }
  }
}
