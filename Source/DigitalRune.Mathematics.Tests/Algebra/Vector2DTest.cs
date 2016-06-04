using System;
using System.Collections.Generic;
using System.Globalization;
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
  public class Vector2DTest
  {
    [Test]
    public void Constructors()
    {
      Vector2D v = new Vector2D();
      Assert.AreEqual(0.0, v.X);
      Assert.AreEqual(0.0, v.Y);

      v = new Vector2D(2.3);
      Assert.AreEqual(2.3, v.X);
      Assert.AreEqual(2.3, v.Y);

      v = new Vector2D(1.0, 2.0);
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);

      v = new Vector2D(new[] { 1.0, 2.0 });
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);

      v = new Vector2D(new List<double>(new[] { 1.0, 2.0 }));
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
    }


    [Test]
    public void Properties()
    {
      Vector2D v = new Vector2D();
      v.X = 1.0;
      v.Y = 2.0;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(new Vector2D(1.0, 2.0), v);
    }


    [Test]
    public void HashCode()
    {
      Vector2D v = new Vector2D(1.0, 2.0);
      Assert.AreNotEqual(Vector2D.One.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualityOperators()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Vector2D b = new Vector2D(1.0, 2.0);
      Vector2D c = new Vector2D(-1.0, 2.0);
      Vector2D d = new Vector2D(1.0, -2.0);

      Assert.IsTrue(a == b);
      Assert.IsFalse(a == c);
      Assert.IsFalse(a == d);
      Assert.IsFalse(a != b);
      Assert.IsTrue(a != c);
      Assert.IsTrue(a != d);
    }


    [Test]
    public void ComparisonOperators()
    {
      Vector2D a = new Vector2D(1.0, 1.0);
      Vector2D b = new Vector2D(0.5, 0.5);
      Vector2D c = new Vector2D(1.0, 0.5);
      Vector2D d = new Vector2D(0.5, 1.0);

      Assert.IsTrue(a > b);
      Assert.IsFalse(a > c);
      Assert.IsFalse(a > d);

      Assert.IsTrue(b < a);
      Assert.IsFalse(c < a);
      Assert.IsFalse(d < a);

      Assert.IsTrue(a >= b);
      Assert.IsTrue(a >= c);
      Assert.IsTrue(a >= d);

      Assert.IsFalse(b >= a);
      Assert.IsFalse(b >= c);
      Assert.IsFalse(b >= d);

      Assert.IsTrue(b <= a);
      Assert.IsTrue(c <= a);
      Assert.IsTrue(d <= a);

      Assert.IsFalse(a <= b);
      Assert.IsFalse(c <= b);
      Assert.IsFalse(d <= b);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector2D u = new Vector2D(1.0, 2.0);
      Vector2D v = new Vector2D(1.000001, 2.000001);
      Vector2D w = new Vector2D(1.00000001, 2.00000001);

      Assert.IsTrue(Vector2D.AreNumericallyEqual(u, u));
      Assert.IsFalse(Vector2D.AreNumericallyEqual(u, v));
      Assert.IsTrue(Vector2D.AreNumericallyEqual(u, w));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Vector2D u = new Vector2D(1.0, 2.0);
      Vector2D v = new Vector2D(1.002, 2.002);
      Vector2D w = new Vector2D(1.0001, 2.0001);

      Assert.IsTrue(Vector2D.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(Vector2D.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(Vector2D.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector2D u = new Vector2D(0.0, 0.0);
      Vector2D v = new Vector2D(1e-9, -1e-9);
      Vector2D w = new Vector2D(1e-7, 1e-7);

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Vector2D v0 = new Vector2D(678.0, 234.8);
      Vector2D v1 = new Vector2D(678.0, 234.8);
      Vector2D v2 = new Vector2D(67.0, 234.8);
      Vector2D v3 = new Vector2D(678.0, 24.8);
      Assert.IsTrue(v0.Equals(v0));
      Assert.IsTrue(v0.Equals(v1));
      Assert.IsFalse(v0.Equals(v2));
      Assert.IsFalse(v0.Equals(v3));
      Assert.IsFalse(v0.Equals(v0.ToString()));
    }


    [Test]
    public void AdditionOperator()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Vector2D b = new Vector2D(2.0, 3.0);
      Vector2D c = a + b;
      Assert.AreEqual(new Vector2D(3.0, 5.0), c);
    }


    [Test]
    public void Addition()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Vector2D b = new Vector2D(2.0, 3.0);
      Vector2D c = Vector2D.Add(a, b);
      Assert.AreEqual(new Vector2D(3.0, 5.0), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Vector2D b = new Vector2D(10.0, -10.0);
      Vector2D c = a - b;
      Assert.AreEqual(new Vector2D(-9.0, 12.0), c);
    }


    [Test]
    public void Subtraction()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Vector2D b = new Vector2D(10.0, -10.0);
      Vector2D c = Vector2D.Subtract(a, b);
      Assert.AreEqual(new Vector2D(-9.0, 12.0), c);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double s = 13.3;

      Vector2D v = new Vector2D(x, y);

      Vector2D u = v * s;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);

      u = s * v;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
    }


    [Test]
    public void Multiplication()
    {
      double x = 23.4;
      double y = -11.0;
      double s = 13.3;

      Vector2D v = new Vector2D(x, y);

      Vector2D u = Vector2D.Multiply(s, v);
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
    }


    [Test]
    public void MultiplicationOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;

      double x2 = 34.0;
      double y2 = 1.2;

      Vector2D v = new Vector2D(x1, y1);
      Vector2D w = new Vector2D(x2, y2);

      Assert.AreEqual(new Vector2D(x1 * x2, y1 * y2), v * w);
    }


    [Test]
    public void MultiplicationVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;

      double x2 = 34.0;
      double y2 = 1.2;

      Vector2D v = new Vector2D(x1, y1);
      Vector2D w = new Vector2D(x2, y2);

      Assert.AreEqual(new Vector2D(x1 * x2, y1 * y2), Vector2D.Multiply(v, w));
    }


    [Test]
    public void DivisionOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double s = 13.3;

      Vector2D v = new Vector2D(x, y);
      Vector2D u = v / s;
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
    }


    [Test]
    public void Division()
    {
      double x = 23.4;
      double y = -11.0;
      double s = 13.3;

      Vector2D v = new Vector2D(x, y);
      Vector2D u = Vector2D.Divide(v, s);
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
    }


    [Test]
    public void DivisionVectorOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;

      double x2 = 34.0;
      double y2 = 1.2;

      Vector2D v = new Vector2D(x1, y1);
      Vector2D w = new Vector2D(x2, y2);

      Assert.AreEqual(new Vector2D(x1 / x2, y1 / y2), v / w);
    }


    [Test]
    public void DivisionVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;

      double x2 = 34.0;
      double y2 = 1.2;

      Vector2D v = new Vector2D(x1, y1);
      Vector2D w = new Vector2D(x2, y2);

      Assert.AreEqual(new Vector2D(x1 / x2, y1 / y2), Vector2D.Divide(v, w));
    }


    [Test]
    public void NegationOperator()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Assert.AreEqual(new Vector2D(-1.0, -2.0), -a);
    }


    [Test]
    public void Negation()
    {
      Vector2D a = new Vector2D(1.0, 2.0);
      Assert.AreEqual(new Vector2D(-1.0, -2.0), Vector2D.Negate(a));
    }


    [Test]
    public void Constants()
    {
      Assert.AreEqual(0.0, Vector2D.Zero.X);
      Assert.AreEqual(0.0, Vector2D.Zero.Y);

      Assert.AreEqual(1.0, Vector2D.One.X);
      Assert.AreEqual(1.0, Vector2D.One.Y);

      Assert.AreEqual(1.0, Vector2D.UnitX.X);
      Assert.AreEqual(0.0, Vector2D.UnitX.Y);

      Assert.AreEqual(0.0, Vector2D.UnitY.X);
      Assert.AreEqual(1.0, Vector2D.UnitY.Y);
    }


    [Test]
    public void IndexerRead()
    {
      Vector2D v = new Vector2D(1.0, -10e10);
      Assert.AreEqual(1.0, v[0]);
      Assert.AreEqual(-10e10, v[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException()
    {
      Vector2D v = new Vector2D(1.0, -10e10);
      Assert.AreEqual(1.0, v[-1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException2()
    {
      Vector2D v = new Vector2D(1.0, -10e10);
      Assert.AreEqual(1.0, v[2]);
    }


    [Test]
    public void IndexerWrite()
    {
      Vector2D v = Vector2D.Zero;
      v[0] = 1.0;
      v[1] = -10e10;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(-10e10, v.Y);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException()
    {
      Vector2D v = new Vector2D(1.0, -10e10);
      v[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException2()
    {
      Vector2D v = new Vector2D(1.0, -10e10);
      v[2] = 0.0;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 2;
      Assert.IsFalse(new Vector2D().IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        Vector2D v = new Vector2D();
        v[i] = double.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNormalized()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-7;

      Vector2D arbitraryVector = new Vector2D(1.0, 0.001);
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      Vector2D normalizedVector = new Vector2D(1.00000001, 0.00000001);
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void Length()
    {
      Assert.AreEqual(1.0, Vector2D.UnitX.Length);
      Assert.AreEqual(1.0, Vector2D.UnitY.Length);

      double x = -1.9;
      double y = 2.1;
      double length = (double)Math.Sqrt(x * x + y * y);
      Vector2D v = new Vector2D(x, y);
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void Length2()
    {
      Vector2D v = new Vector2D(1.0, 2.0);
      v.Length = 0.5;
      Assert.IsTrue(Numeric.AreEqual(0.5, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      Vector2D v = Vector2D.Zero;
      v.Length = 0.5;
    }


    [Test]
    public void LengthSquared()
    {
      Assert.AreEqual(1.0, Vector2D.UnitX.LengthSquared);
      Assert.AreEqual(1.0, Vector2D.UnitY.LengthSquared);

      double x = -1.9;
      double y = 2.1;
      double length = x * x + y * y;
      Vector2D v = new Vector2D(x, y);
      Assert.AreEqual(length, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      Vector2D v = new Vector2D(3.0, -1.0);
      Vector2D normalized = v.Normalized;
      Assert.AreEqual(new Vector2D(3.0, -1.0), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      Vector2D v = new Vector2D(3.0, -1.0);
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      Vector2D.Zero.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      Vector2D v = Vector2D.Zero;
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new Vector2D(1, 2);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector2D(1, 2).Normalized, v);

      v = new Vector2D(0, -1);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector2D(0, -1).Normalized, v);
    }


    [Test]
    public void OrthogonalVectors()
    {
      Vector2D v = Vector2D.UnitX;
      Vector2D orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2D.Dot(v, orthogonal)));

      v = Vector2D.UnitY;
      orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2D.Dot(v, orthogonal)));

      v = new Vector2D(23.0, 44.0);
      orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2D.Dot(v, orthogonal)));
    }


    [Test]
    public void AbsoluteStatic()
    {
      Vector2D v = new Vector2D(-1, -2);
      Vector2D absoluteV = Vector2D.Absolute(v);

      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);

      v = new Vector2D(1, 2);
      absoluteV = Vector2D.Absolute(v);
      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
    }


    [Test]
    public void Absolute()
    {
      Vector2D v = new Vector2D(-1, -2);
      v.Absolute();

      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);

      v = new Vector2D(1, 2);
      v.Absolute();
      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
    }


    [Test]
    public void DotProduct()
    {
      // 0°
      Assert.AreEqual(1.0, Vector2D.Dot(Vector2D.UnitX, Vector2D.UnitX));
      Assert.AreEqual(1.0, Vector2D.Dot(Vector2D.UnitY, Vector2D.UnitY));

      // 180°
      Assert.AreEqual(-1.0, Vector2D.Dot(Vector2D.UnitX, -Vector2D.UnitX));
      Assert.AreEqual(-1.0, Vector2D.Dot(Vector2D.UnitY, -Vector2D.UnitY));

      // 90°
      Assert.AreEqual(0.0, Vector2D.Dot(Vector2D.UnitX, Vector2D.UnitY));

      // 45°
      double angle = Math.Acos(Vector2D.Dot(new Vector2D(1, 1).Normalized, Vector2D.UnitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector2D.Dot(new Vector2D(1, 1).Normalized, Vector2D.UnitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
    }


    [Test]
    public void GetAngle()
    {
      Vector2D x = Vector2D.UnitX;
      Vector2D y = Vector2D.UnitY;
      Vector2D halfvector = x + y;

      // 90°
      Assert.IsTrue(Numeric.AreEqual((double)Math.PI / 4, Vector2D.GetAngle(x, halfvector)));

      // 45°
      Assert.IsTrue(Numeric.AreEqual((double)Math.PI / 2, Vector2D.GetAngle(x, y)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void GetAngleException()
    {
      Vector2D.GetAngle(Vector2D.UnitX, Vector2D.Zero);
    }


    [Test]
    public void ImplicitCastToVectorD()
    {
      Vector2D v = new Vector2D(1.1, 2.2);
      VectorD v2 = v;

      Assert.AreEqual(2, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
    }


    [Test]
    public void ToVectorD()
    {
      Vector2D v = new Vector2D(1.1, 2.2);
      VectorD v2 = v.ToVectorD();

      Assert.AreEqual(2, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Vector2 xna = new Vector2(6, 7);
      Vector2D v = (Vector2D)xna;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void FromXna()
    {
      Vector2 xna = new Vector2(6, 7);
      Vector2D v = Vector2D.FromXna(xna);

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Vector2D v = new Vector2D(6, 7);
      Vector2 xna = (Vector2)v;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ToXna()
    {
      Vector2D v = new Vector2D(6, 7);
      Vector2 xna = v.ToXna();

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ExplicitCastToVector2F()
    {
      double x = 23.4;
      double y = -11.0;
      double[] elementsD = new[] { x, y };
      float[] elementsF = new[] { (float)x, (float)y };
      Vector2D vectorD = new Vector2D(elementsD);
      Vector2F vectorF = (Vector2F)vectorD;
      Assert.AreEqual(new Vector2F(elementsF), vectorF);
    }


    [Test]
    public void ToVector2F()
    {
      double x = 23.4;
      double y = -11.0;
      double[] elementsD = new[] { x, y };
      float[] elementsF = new[] { (float)x, (float)y };
      Vector2D vectorD = new Vector2D(elementsD);
      Vector2F vectorF = vectorD.ToVector2F();
      Assert.AreEqual(new Vector2F(elementsF), vectorF);
    }


    [Test]
    public void ExplicitDoubleArrayCast()
    {
      double x = 23.4;
      double y = -11.0;
      double[] values = (double[])new Vector2D(x, y);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Length);
    }


    [Test]
    public void ExplicitDoubleArrayCast2()
    {
      double x = 23.4;
      double y = -11.0;
      double[] values = (new Vector2D(x, y)).ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      double x = 23.5;
      double y = 0.0;
      List<double> values = (List<double>)new Vector2D(x, y);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Count);
    }


    [Test]
    public void ExplicitListCast2()
    {
      double x = 23.5;
      double y = 0.0;
      List<double> values = (new Vector2D(x, y)).ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Count);
    }


    [Test]
    public void ProjectTo()
    {
      // Project (1, 1) to axes
      Vector2D v = Vector2D.One;
      Vector2D projection = Vector2D.ProjectTo(v, Vector2D.UnitX);
      Assert.AreEqual(Vector2D.UnitX, projection);
      projection = Vector2D.ProjectTo(v, Vector2D.UnitY);
      Assert.AreEqual(Vector2D.UnitY, projection);

      // Project axes to (1, 1)
      Vector2D expected = Vector2D.One / 2.0;
      projection = Vector2D.ProjectTo(Vector2D.UnitX, v);
      Assert.AreEqual(expected, projection);
      projection = Vector2D.ProjectTo(Vector2D.UnitY, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      // Project (1, 1) to axes
      Vector2D projection = Vector2D.One;
      projection.ProjectTo(Vector2D.UnitX);
      Assert.AreEqual(Vector2D.UnitX, projection);
      projection = Vector2D.One;
      projection.ProjectTo(Vector2D.UnitY);
      Assert.AreEqual(Vector2D.UnitY, projection);
      projection = Vector2D.One;

      // Project axes to (1, 1, 1)
      Vector2D expected = Vector2D.One / 2.0;
      projection = Vector2D.UnitX;
      projection.ProjectTo(Vector2D.One);
      Assert.AreEqual(expected, projection);
      projection = Vector2D.UnitY;
      projection.ProjectTo(Vector2D.One);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void Clamp1()
    {
      Vector2D clamped = new Vector2D(-10, 1);
      clamped.Clamp(-100, 100);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
    }


    [Test]
    public void Clamp2()
    {
      Vector2D clamped = new Vector2D(-10, 1);
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
    }


    [Test]
    public void ClampStatic1()
    {
      Vector2D clamped = new Vector2D(-10, 1);
      clamped = Vector2D.Clamp(clamped, -100, 100);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
    }


    [Test]
    public void ClampStatic2()
    {
      Vector2D clamped = new Vector2D(-10, 1);
      clamped = Vector2D.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
    }


    [Test]
    public void ClampToZero1()
    {
      Vector2D v = new Vector2D(Numeric.EpsilonD / 2, Numeric.EpsilonD / 2);
      v.ClampToZero();
      Assert.AreEqual(Vector2D.Zero, v);
      v = new Vector2D(-Numeric.EpsilonD * 2, Numeric.EpsilonD);
      v.ClampToZero();
      Assert.AreNotEqual(Vector2D.Zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      Vector2D v = new Vector2D(0.1, 0.1);
      v.ClampToZero(0.11);
      Assert.AreEqual(Vector2D.Zero, v);
      v = new Vector2D(0.1, -0.11);
      v.ClampToZero(0.1);
      Assert.AreNotEqual(Vector2D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      Vector2D v = new Vector2D(Numeric.EpsilonD / 2, Numeric.EpsilonD / 2);
      v = Vector2D.ClampToZero(v);
      Assert.AreEqual(Vector2D.Zero, v);
      v = new Vector2D(-Numeric.EpsilonD * 2, Numeric.EpsilonD);
      v = Vector2D.ClampToZero(v);
      Assert.AreNotEqual(Vector2D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      Vector2D v = new Vector2D(0.1, 0.1);
      v = Vector2D.ClampToZero(v, 0.11);
      Assert.AreEqual(Vector2D.Zero, v);
      v = new Vector2D(0.1, -0.11);
      v = Vector2D.ClampToZero(v, 0.1);
      Assert.AreNotEqual(Vector2D.Zero, v);
    }


    [Test]
    public void Min()
    {
      Vector2D v1 = new Vector2D(1.0, 2.0);
      Vector2D v2 = new Vector2D(-1.0, 2.0);
      Vector2D min = Vector2D.Min(v1, v2);
      Assert.AreEqual(new Vector2D(-1.0, 2.0), min);
    }


    [Test]
    public void Max()
    {
      Vector2D v1 = new Vector2D(1.0, 2.0);
      Vector2D v2 = new Vector2D(-1.0, 2.1);
      Vector2D max = Vector2D.Max(v1, v2);
      Assert.AreEqual(new Vector2D(1.0, 2.1), max);
    }


    [Test]
    public void SerializationXml()
    {
      Vector2D v1 = new Vector2D(1.0, 2.0);
      Vector2D v2;
      string fileName = "SerializationVector2D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Vector2D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Vector2D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (Vector2D)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Vector2D v1 = new Vector2D(0.1, -0.2);
      Vector2D v2;
      string fileName = "SerializationVector2D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (Vector2D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      Vector2D v1 = new Vector2D(0.1, -0.2);
      Vector2D v2;

      string fileName = "SerializationVector2D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Vector2D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (Vector2D)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      Vector2D v1 = new Vector2D(0.1, -0.2);
      Vector2D v2;

      string fileName = "SerializationVector2D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Vector2D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (Vector2D)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void Parse()
    {
      Vector2D vector = Vector2D.Parse("(0.0123; 9.876)", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9.876, vector.Y);

      vector = Vector2D.Parse("(   0.0123   ;  9 ) ", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9, vector.Y);
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException()
    {
      Vector2D vector = Vector2D.Parse("(0.0123; )");
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException2()
    {
      Vector2D vector = Vector2D.Parse("xyz");
    }


    [Test]
    public void ToStringAndParse()
    {
      Vector2D vector = new Vector2D(0.0123, 9.876);
      string s = vector.ToString();
      Vector2D parsedVector = Vector2D.Parse(s);
      Assert.AreEqual(vector, parsedVector);
    }


    [Test]
    public void GetLargestComponent()
    {
      Vector2D v = new Vector2D(-1, -2);
      Assert.AreEqual(-1, v.LargestComponent);

      v = new Vector2D(10, 20);
      Assert.AreEqual(20, v.LargestComponent);
    }


    [Test]
    public void GetIndexOfLargestComponent()
    {
      Vector2D v = new Vector2D(-1, -2);
      Assert.AreEqual(0, v.IndexOfLargestComponent);

      v = new Vector2D(10, 20);
      Assert.AreEqual(1, v.IndexOfLargestComponent);
    }


    [Test]
    public void GetSmallestComponent()
    {
      Vector2D v = new Vector2D(-4, -2);
      Assert.AreEqual(-4, v.SmallestComponent);

      v = new Vector2D(10, 0);
      Assert.AreEqual(0, v.SmallestComponent);
    }


    [Test]
    public void GetIndexOfSmallestComponent()
    {
      Vector2D v = new Vector2D(-4, -2);
      Assert.AreEqual(0, v.IndexOfSmallestComponent);

      v = new Vector2D(10, 0);
      Assert.AreEqual(1, v.IndexOfSmallestComponent);
    }
  }
}