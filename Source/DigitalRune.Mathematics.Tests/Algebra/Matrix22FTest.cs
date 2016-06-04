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
  public class Matrix22FTest
  {
    //           | 1, 2 |
    // Matrix =  | 3, 4 |

    // in column-major layout
    readonly float[] columnMajor = new float[] { 1.0f, 3.0f,
                                                 2.0f, 4.0f };

    // in row-major layout
    readonly float[] rowMajor = new float[] { 1.0f, 2.0f, 
                                              3.0f, 4.0f };


    [Test]
    public void Constants()
    {
      Matrix22F zero = Matrix22F.Zero;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(0.0, zero[i]);

      Matrix22F one = Matrix22F.One;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(1.0f, one[i]);
    }


    [Test]
    public void Constructors()
    {
      Matrix22F m = new Matrix22F(1.0f, 2.0f, 3.0f, 4.0f);

      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22F(rowMajor, MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22F(new List<float>(columnMajor), MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22F(new List<float>(rowMajor), MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22F(new float[2, 2] { { 1, 2 }, { 3, 4 } });
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = new Matrix22F(new float[2][] { new float[2] { 1, 2 }, new float[2] { 3, 4 } });
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);
    }


    [Test]
    [ExpectedException(typeof(NullReferenceException))]
    public void ConstructorException1()
    {
      new Matrix22F(new float[2][]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ConstructorException2()
    {
      float[][] elements = new float[2][];
      elements[0] = new float[2];
      elements[1] = new float[1];
      new Matrix22F(elements);
    }


    [Test]
    public void Properties()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(1.0f, m.M00);
      Assert.AreEqual(2.0f, m.M01);
      Assert.AreEqual(3.0f, m.M10);
      Assert.AreEqual(4.0f, m.M11);

      m = Matrix22F.Zero;
      m.M00 = 1.0f;
      m.M01 = 2.0f;
      m.M10 = 3.0f;
      m.M11 = 4.0f;
      Assert.AreEqual(new Matrix22F(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    public void Indexer1d()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], m[i]);

      m = Matrix22F.Zero;
      for (int i = 0; i < 4; i++)
        m[i] = rowMajor[i];
      Assert.AreEqual(new Matrix22F(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException()
    {
      Matrix22F m = new Matrix22F();
      m[-1] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException2()
    {
      Matrix22F m = new Matrix22F();
      m[4] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException3()
    {
      Matrix22F m = new Matrix22F();
      float x = m[-1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer1dException4()
    {
      Matrix22F m = new Matrix22F();
      float x = m[4];
    }


    [Test]
    public void Indexer2d()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(columnMajor[column * 2 + row], m[row, column]);
      m = Matrix22F.Zero;
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          m[row, column] = (row * 2 + column + 1);
      Assert.AreEqual(new Matrix22F(columnMajor, MatrixOrder.ColumnMajor), m);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException()
    {
      Matrix22F m = Matrix22F.Zero;
      m[0, 2] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException2()
    {
      Matrix22F m = Matrix22F.Zero;
      m[2, 0] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException3()
    {
      Matrix22F m = Matrix22F.Zero;
      m[0, -1] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException4()
    {
      Matrix22F m = Matrix22F.Zero;
      m[-1, 0] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException5()
    {
      Matrix22F m = Matrix22F.Zero;
      m[1, 2] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException6()
    {
      Matrix22F m = Matrix22F.Zero;
      m[2, 1] = 1.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException9()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[0, 2];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException10()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[2, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException11()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[0, -1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException12()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[-1, 0];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException13()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[2, 1];
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void Indexer2dException14()
    {
      Matrix22F m = Matrix22F.Zero;
      float x = m[1, 2];
    }


    [Test]
    public void Determinant()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(-2.0, m.Determinant);
      Assert.AreEqual(0.0, Matrix22F.Zero.Determinant);
      Assert.AreEqual(0.0, Matrix22F.One.Determinant);
      Assert.AreEqual(1.0f, Matrix22F.Identity.Determinant);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 2;
      const int numberOfColumns = 2;
      Assert.IsFalse(new Matrix22F().IsNaN);

      for (int r = 0; r < numberOfRows; r++)
      {
        for (int c = 0; c < numberOfColumns; c++)
        {
          Matrix22F m = new Matrix22F();
          m[r, c] = float.NaN;
          Assert.IsTrue(m.IsNaN);
        }
      }
    }


    [Test]
    public void IsSymmetric()
    {
      Matrix22F m = new Matrix22F(new float[2, 2] {{ 1, 2 }, { 2, 4 } });
      Assert.AreEqual(true, m.IsSymmetric);

      m = new Matrix22F(new float[2, 2] {{ 2, 1 }, { 4, 2 } });
      Assert.AreEqual(false, m.IsSymmetric);
    }


    [Test]
    public void Trace()
    {
      Matrix22F m = new Matrix22F(new float[2, 2] {{ 1, 2 }, { 2, 4 } });
      Assert.AreEqual(5, m.Trace);
    }


    [Test]
    public void Transposed()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F mt = new Matrix22F(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m.Transposed);
      Assert.AreEqual(Matrix22F.Identity, Matrix22F.Identity.Transposed);
    }


    [Test]
    public void Transpose()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.Transpose();
      Matrix22F mt = new Matrix22F(rowMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(mt, m);
      Matrix22F i = Matrix22F.Identity;
      i.Transpose();
      Assert.AreEqual(Matrix22F.Identity, i);
    }


    [Test]
    public void Inverse()
    {
      Assert.AreEqual(Matrix22F.Identity, Matrix22F.Identity.Inverse);

      Matrix22F m = new Matrix22F(1, 2, 3, 4);
      Vector2F v = Vector2F.One;
      Vector2F w = m * v;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, m * m.Inverse));
    }


    [Test]
    public void InverseWithNearSingularMatrix()
    {
      Matrix22F m = new Matrix22F(0.0001f, 0,
                                  0, 0.0001f);
      Vector2F v = Vector2F.One;
      Vector2F w = m * v;
      Assert.IsTrue(Vector2F.AreNumericallyEqual(v, m.Inverse * w));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, m * m.Inverse));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException()
    {
      Matrix22F m = new Matrix22F(1, 2, 4, 8);
      m = m.Inverse;
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InverseException2()
    {
      Matrix22F m = Matrix22F.Zero.Inverse;
    }


    [Test]
    public void Invert()
    {
      Assert.AreEqual(Matrix22F.Identity, Matrix22F.Identity.Inverse);

      Matrix22F m = new Matrix22F(1, 2, 3, 4);
      Vector2F v = Vector2F.One;
      Vector2F w = m * v;
      Matrix22F im = m;
      im.Invert();
      Assert.IsTrue(Vector2F.AreNumericallyEqual(v, im * w));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, m * im));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException()
    {
      Matrix22F m = new Matrix22F(1, 2, 4, 8);
      m.Invert();
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void InvertException2()
    {
      Matrix22F.Zero.Invert();
    }


    [Test]
    public void GetColumn()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector2F(1.0f, 3.0f), m.GetColumn(0));
      Assert.AreEqual(new Vector2F(2.0f, 4.0f), m.GetColumn(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException1()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetColumnException2()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.GetColumn(2);
    }


    [Test]
    public void SetColumn()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(0, new Vector2F(0.1f, 0.2f));
      Assert.AreEqual(new Vector2F(0.1f, 0.2f), m.GetColumn(0));
      Assert.AreEqual(new Vector2F(2.0f, 4.0f), m.GetColumn(1));

      m.SetColumn(1, new Vector2F(0.4f, 0.5f));
      Assert.AreEqual(new Vector2F(0.1f, 0.2f), m.GetColumn(0));
      Assert.AreEqual(new Vector2F(0.4f, 0.5f), m.GetColumn(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException1()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(-1, Vector2F.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetColumnException2()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetColumn(2, Vector2F.One);
    }


    [Test]
    public void GetRow()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreEqual(new Vector2F(1.0f, 2.0f), m.GetRow(0));
      Assert.AreEqual(new Vector2F(3.0f, 4.0f), m.GetRow(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException1()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetRowException2()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.GetRow(2);
    }


    [Test]
    public void SetRow()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(0, new Vector2F(0.1f, 0.2f));
      Assert.AreEqual(new Vector2F(0.1f, 0.2f), m.GetRow(0));
      Assert.AreEqual(new Vector2F(3.0f, 4.0f), m.GetRow(1));

      m.SetRow(1, new Vector2F(0.4f, 0.5f));
      Assert.AreEqual(new Vector2F(0.1f, 0.2f), m.GetRow(0));
      Assert.AreEqual(new Vector2F(0.4f, 0.5f), m.GetRow(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException1()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(-1, Vector2F.One);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetRowException2()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m.SetRow(2, Vector2F.One);
    }


    [Test]
    public void AreEqual()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Matrix22F m0 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix22F(0.000001f);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix22F(0.00000001f);

      Assert.IsTrue(Matrix22F.AreNumericallyEqual(m0, m0));
      Assert.IsFalse(Matrix22F.AreNumericallyEqual(m0, m1));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(m0, m2));

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      float epsilon = 0.001f;
      Matrix22F m0 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m1 += new Matrix22F(0.002f);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m2 += new Matrix22F(0.0001f);

      Assert.IsTrue(Matrix22F.AreNumericallyEqual(m0, m0, epsilon));
      Assert.IsFalse(Matrix22F.AreNumericallyEqual(m0, m1, epsilon));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(m0, m2, epsilon));
    }


    [Test]
    public void CreateScale()
    {
      Matrix22F i = Matrix22F.CreateScale(1.0f);
      Assert.AreEqual(Matrix22F.Identity, i);

      Vector2F v = Vector2F.One;
      Matrix22F m = Matrix22F.CreateScale(2.0f);
      Assert.AreEqual(2 * v, m * v);

      m = Matrix22F.CreateScale(-1.0f, 1.5f);
      Assert.AreEqual(new Vector2F(-1.0f, 1.5f), m * v);

      Vector2F scale = new Vector2F(-2.0f, -3.0f);
      m = Matrix22F.CreateScale(scale);
      v = new Vector2F(1.0f, 2.0f);
      Assert.AreEqual(v * scale, m * v);
    }


    [Test]
    public void CreateRotation()
    {
      Matrix22F m = Matrix22F.CreateRotation(0.0f);
      Assert.AreEqual(Matrix22F.Identity, m);

      m = Matrix22F.CreateRotation((float) Math.PI / 2);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(Vector2F.UnitY, m * Vector2F.UnitX));
    }


    [Test]
    public void CreateRotation2()
    {
      float angle = (float) MathHelper.ToRadians(30);
      Matrix22F m = Matrix22F.CreateRotation(angle);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(new Vector2F((float) Math.Cos(angle), (float) Math.Sin(angle)), m * Vector2F.UnitX));
    }


    [Test]
    public void HashCode()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.AreNotEqual(Matrix22F.Identity.GetHashCode(), m.GetHashCode());
    }


    [Test]
    public void TestEquals()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1.Equals(m1));
      Assert.IsTrue(m1.Equals(m2));
      for (int i = 0; i < 4; i++)
      {
        m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1f;
        Assert.IsFalse(m1.Equals(m2));
      }

      Assert.IsFalse(m1.Equals(m1.ToString()));
    }


    [Test]
    public void EqualityOperators()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsTrue(m1 == m2);
      Assert.IsFalse(m1 != m2);
      for (int i = 0; i < 4; i++)
      {
        m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
        m2[i] += 0.1f;
        Assert.IsFalse(m1 == m2);
        Assert.IsTrue(m1 != m2);
      }
    }


    [Test]
    public void TestToString()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Assert.IsFalse(String.IsNullOrEmpty(m.ToString()));
    }


    [Test]
    public void NegationOperator()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i], (-m)[i]);
    }


    [Test]
    public void Negation()
    {
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i], Matrix22F.Negate(m)[i]);
    }


    [Test]
    public void AdditionOperator()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix22F result = m1 + m2;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * 4, result[i]);
    }


    [Test]
    public void Addition()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = Matrix22F.One;
      Matrix22F result = Matrix22F.Add(m1, m2);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] + 1.0f, result[i]);
    }


    [Test]
    public void SubtractionOperator()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor) * 3;
      Matrix22F result = m1 - m2;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(-rowMajor[i] * 2, result[i]);
    }


    [Test]
    public void Subtraction()
    {
      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = Matrix22F.One;
      Matrix22F result = Matrix22F.Subtract(m1, m2);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] - 1.0f, result[i]);
    }


    [Test]
    public void MultiplicationOperator()
    {
      float s = 0.1234f;
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m = s * m;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);

      m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m = m * s;
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void Multiplication()
    {
      float s = 0.1234f;
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix22F.Multiply(s, m);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i] * s, m[i]);
    }


    [Test]
    public void DivisionOperator()
    {
      float s = 0.1234f;
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m = m / s;
      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void Division()
    {
      float s = 0.1234f;
      Matrix22F m = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      m = Matrix22F.Divide(m, s);
      for (int i = 0; i < 4; i++)
        Assert.IsTrue(Numeric.AreEqual(rowMajor[i] / s, m[i]));
    }


    [Test]
    public void MultiplyMatrixOperator()
    {
      Matrix22F m = new Matrix22F(12, 23, 45, 67);
      Assert.AreEqual(Matrix22F.Zero, m * Matrix22F.Zero);
      Assert.AreEqual(Matrix22F.Zero, Matrix22F.Zero * m);
      Assert.AreEqual(m, m * Matrix22F.Identity);
      Assert.AreEqual(m, Matrix22F.Identity * m);
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, m * m.Inverse));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, m.Inverse * m));

      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(12, 23, 45, 67);
      Matrix22F result = m1 * m2;
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(Vector2F.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyMatrix()
    {
      Matrix22F m = new Matrix22F(12, 23, 45, 67);
      Assert.AreEqual(Matrix22F.Zero, Matrix22F.Multiply(m, Matrix22F.Zero));
      Assert.AreEqual(Matrix22F.Zero, Matrix22F.Multiply(Matrix22F.Zero, m));
      Assert.AreEqual(m, Matrix22F.Multiply(m, Matrix22F.Identity));
      Assert.AreEqual(m, Matrix22F.Multiply(Matrix22F.Identity, m));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, Matrix22F.Multiply(m, m.Inverse)));
      Assert.IsTrue(Matrix22F.AreNumericallyEqual(Matrix22F.Identity, Matrix22F.Multiply(m.Inverse, m)));

      Matrix22F m1 = new Matrix22F(columnMajor, MatrixOrder.ColumnMajor);
      Matrix22F m2 = new Matrix22F(12, 23, 45, 67);
      Matrix22F result = Matrix22F.Multiply(m1, m2);
      for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
          Assert.AreEqual(Vector2F.Dot(m1.GetRow(row), m2.GetColumn(column)), result[row, column]);
    }


    [Test]
    public void MultiplyVectorOperator()
    {
      Vector2F v = new Vector2F(2.34f, 3.45f);
      Assert.AreEqual(v, Matrix22F.Identity * v);
      Assert.AreEqual(Vector2F.Zero, Matrix22F.Zero * v);

      Matrix22F m = new Matrix22F(12, 23, 45, 67);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(v, m * m.Inverse * v));

      for (int i = 0; i < 2; i++)
        Assert.AreEqual(Vector2F.Dot(m.GetRow(i), v), (m * v)[i]);
    }


    [Test]
    public void MultiplyVector()
    {
      Vector2F v = new Vector2F(2.34f, 3.45f);
      Assert.AreEqual(v, Matrix22F.Multiply(Matrix22F.Identity, v));
      Assert.AreEqual(Vector2F.Zero, Matrix22F.Multiply(Matrix22F.Zero, v));

      Matrix22F m = new Matrix22F(12, 23, 45, 67);
      Assert.IsTrue(Vector2F.AreNumericallyEqual(v, Matrix22F.Multiply(m * m.Inverse, v)));

      for (int i = 0; i < 2; i++)
        Assert.AreEqual(Vector2F.Dot(m.GetRow(i), v), Matrix22F.Multiply(m, v)[i]);
    }


    [Test]
    public void ImplicitMatrix22DCast()
    {
      float m00 = 23.5f; float m01 = 0.0f;
      float m10 = 33.5f; float m11 = 1.1f;
      Matrix22D matrix22D = new Matrix22F(m00, m01, m10, m11);
      Assert.IsTrue(Numeric.AreEqual(m00, (float)matrix22D[0, 0]));
      Assert.IsTrue(Numeric.AreEqual(m01, (float)matrix22D[0, 1]));
      Assert.IsTrue(Numeric.AreEqual(m10, (float)matrix22D[1, 0]));
      Assert.IsTrue(Numeric.AreEqual(m11, (float)matrix22D[1, 1]));
    }


    [Test]
    public void ToMatrix22D()
    {
      float m00 = 23.5f; float m01 = 0.0f;
      float m10 = 33.5f; float m11 = 1.1f;
      Matrix22D matrix22D = new Matrix22F(m00, m01, m10, m11).ToMatrix22D();
      Assert.IsTrue(Numeric.AreEqual(m00, (float)matrix22D[0, 0]));
      Assert.IsTrue(Numeric.AreEqual(m01, (float)matrix22D[0, 1]));
      Assert.IsTrue(Numeric.AreEqual(m10, (float)matrix22D[1, 0]));
      Assert.IsTrue(Numeric.AreEqual(m11, (float)matrix22D[1, 1]));
    }


    [Test]
    public void SerializationXml()
    {
      Matrix22F m1 = new Matrix22F(12, 23, 45, 67);
      Matrix22F m2;

      string fileName = "SerializationMatrix22F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Matrix22F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, m1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Matrix22F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      m2 = (Matrix22F) serializer.Deserialize(fileStream);
      Assert.AreEqual(m1, m2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Matrix22F m1 = new Matrix22F(12, 23, 
                                   45, 56.3f);
      Matrix22F m2;

      string fileName = "SerializationMatrix22F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      m2 = (Matrix22F) formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationXml2()
    {
      Matrix22F m1 = new Matrix22F(12, 23,
                                   45, 56.3f);
      Matrix22F m2;

      string fileName = "SerializationMatrix22F_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Matrix22F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        m2 = (Matrix22F)serializer.ReadObject(reader);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void SerializationJson()
    {
      Matrix22F m1 = new Matrix22F(12, 23,
                                   45, 56.3f);
      Matrix22F m2;

      string fileName = "SerializationMatrix22F.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Matrix22F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, m1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        m2 = (Matrix22F)serializer.ReadObject(stream);

      Assert.AreEqual(m1, m2);
    }


    [Test]
    public void Absolute()
    {
      Matrix22F absoluteM = new Matrix22F(-1, -2, -3, -4);
      absoluteM.Absolute();

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);

      absoluteM = new Matrix22F(1, 2, 3, 4);
      absoluteM.Absolute();
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Matrix22F m = new Matrix22F(-1, -2, -3, -4);
      Matrix22F absoluteM = Matrix22F.Absolute(m);

      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);

      m = new Matrix22F(1, 2, 3, 4);
      absoluteM = Matrix22F.Absolute(m);
      Assert.AreEqual(1, absoluteM.M00);
      Assert.AreEqual(2, absoluteM.M01);
      Assert.AreEqual(3, absoluteM.M10);
      Assert.AreEqual(4, absoluteM.M11);
    }


    [Test]
    public void ClampToZero()
    {
      Matrix22F m = new Matrix22F(0.000001f);
      m.ClampToZero();
      Assert.AreEqual(new Matrix22F(), m);

      m = new Matrix22F(0.1f);
      m.ClampToZero();
      Assert.AreEqual(new Matrix22F(0.1f), m);

      m = new Matrix22F(0.001f);
      m.ClampToZero(0.01f);
      Assert.AreEqual(new Matrix22F(), m);

      m = new Matrix22F(0.1f);
      m.ClampToZero(0.01f);
      Assert.AreEqual(new Matrix22F(0.1f), m);
    }


    [Test]
    public void ClampToZeroStatic()
    {
      Matrix22F m = new Matrix22F(0.000001f);
      Assert.AreEqual(new Matrix22F(), Matrix22F.ClampToZero(m));
      Assert.AreEqual(new Matrix22F(0.000001f), m); // m unchanged?

      m = new Matrix22F(0.1f);
      Assert.AreEqual(new Matrix22F(0.1f), Matrix22F.ClampToZero(m));
      Assert.AreEqual(new Matrix22F(0.1f), m);

      m = new Matrix22F(0.001f);
      Assert.AreEqual(new Matrix22F(), Matrix22F.ClampToZero(m, 0.01f));
      Assert.AreEqual(new Matrix22F(0.001f), m);

      m = new Matrix22F(0.1f);
      Assert.AreEqual(new Matrix22F(0.1f), Matrix22F.ClampToZero(m, 0.01f));
      Assert.AreEqual(new Matrix22F(0.1f), m);
    }


    [Test]
    public void ToArray1D()
    {
      Matrix22F m = new Matrix22F(1, 2, 3, 4);
      float[] array = m.ToArray1D(MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], array[i]);
      array = m.ToArray1D(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(columnMajor[i], array[i]);
    }


    [Test]
    public void ToList()
    {
      Matrix22F m = new Matrix22F(1, 2, 3, 4);
      IList<float> list = m.ToList(MatrixOrder.RowMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(rowMajor[i], list[i]);
      list = m.ToList(MatrixOrder.ColumnMajor);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(columnMajor[i], list[i]);
    }


    [Test]
    public void ToArray2D()
    {
      Matrix22F m = new Matrix22F(1, 2, 3, 4);

      float[,] array = m.ToArray2D();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i, j]);

      array = (float[,]) m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i, j]);
    }


    [Test]
    public void ToArrayJagged()
    {
      Matrix22F m = new Matrix22F(1, 2, 3, 4);

      float[][] array = m.ToArrayJagged();
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i][j]);

      array = (float[][]) m;
      for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
          Assert.AreEqual(i * 2 + j + 1, array[i][j]);
    }


    [Test]
    public void ToMatrixF()
    {
      Matrix22F m22 = new Matrix22F(1, 2, 3, 4);

      MatrixF m = m22.ToMatrixF();
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
