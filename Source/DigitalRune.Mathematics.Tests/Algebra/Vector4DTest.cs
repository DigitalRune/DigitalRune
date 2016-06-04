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
  public class Vector4DTest
  {
    [Test]
    public void Constructors()
    {
      Vector4D v = new Vector4D();
      Assert.AreEqual(0.0, v.X);
      Assert.AreEqual(0.0, v.Y);
      Assert.AreEqual(0.0, v.Z);
      Assert.AreEqual(0.0, v.W);

      v = new Vector4D(2.3);
      Assert.AreEqual(2.3, v.X);
      Assert.AreEqual(2.3, v.Y);
      Assert.AreEqual(2.3, v.Z);
      Assert.AreEqual(2.3, v.W);

      v = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(4.0, v.W);

      v = new Vector4D(new[] { 1.0, 2.0, 3.0, 4.0 });
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(4.0, v.W);

      v = new Vector4D(new List<double>(new[] { 1.0, 2.0, 3.0, 4.0 }));
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(4.0, v.W);

      v = new Vector4D(new Vector3D(1.0, 2.0, 3.0), 4.0);
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(4.0, v.W);
    }


    [Test]
    public void Properties()
    {
      Vector4D v = new Vector4D();
      v.X = 1.0;
      v.Y = 2.0;
      v.Z = 3.0;
      v.W = 4.0;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(4.0, v.W);
      Assert.AreEqual(new Vector4D(1.0, 2.0, 3.0, 4.0), v);
    }


    [Test]
    public void HashCode()
    {
      Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Assert.AreNotEqual(Vector4D.One.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualityOperators()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D b = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D c = new Vector4D(-1.0, 2.0, 3.0, 4.0);
      Vector4D d = new Vector4D(1.0, -2.0, 3.0, 4.0);
      Vector4D e = new Vector4D(1.0, 2.0, -3.0, 4.0);
      Vector4D f = new Vector4D(1.0, 2.0, 3.0, -4.0);

      Assert.IsTrue(a == b);
      Assert.IsFalse(a == c);
      Assert.IsFalse(a == d);
      Assert.IsFalse(a == e);
      Assert.IsFalse(a == f);
      Assert.IsFalse(a != b);
      Assert.IsTrue(a != c);
      Assert.IsTrue(a != d);
      Assert.IsTrue(a != e);
      Assert.IsTrue(a != f);
    }


    [Test]
    public void ComparisonOperators()
    {
      Vector4D a = new Vector4D(1.0, 1.0, 1.0, 1.0);
      Vector4D b = new Vector4D(0.5, 0.5, 0.5, 0.5);
      Vector4D c = new Vector4D(1.0, 0.5, 0.5, 0.5);
      Vector4D d = new Vector4D(0.5, 1.0, 0.5, 0.5);
      Vector4D e = new Vector4D(0.5, 0.5, 1.0, 0.5);
      Vector4D f = new Vector4D(0.5, 0.5, 0.5, 1.0);

      Assert.IsTrue(a > b);
      Assert.IsFalse(a > c);
      Assert.IsFalse(a > d);
      Assert.IsFalse(a > e);
      Assert.IsFalse(a > f);

      Assert.IsTrue(b < a);
      Assert.IsFalse(c < a);
      Assert.IsFalse(d < a);
      Assert.IsFalse(e < a);
      Assert.IsFalse(f < a);

      Assert.IsTrue(a >= b);
      Assert.IsTrue(a >= c);
      Assert.IsTrue(a >= d);
      Assert.IsTrue(a >= e);
      Assert.IsTrue(a >= f);

      Assert.IsFalse(b >= a);
      Assert.IsFalse(b >= c);
      Assert.IsFalse(b >= d);
      Assert.IsFalse(b >= e);
      Assert.IsFalse(b >= f);

      Assert.IsTrue(b <= a);
      Assert.IsTrue(c <= a);
      Assert.IsTrue(d <= a);
      Assert.IsTrue(e <= a);
      Assert.IsTrue(f <= a);

      Assert.IsFalse(a <= b);
      Assert.IsFalse(c <= b);
      Assert.IsFalse(d <= b);
      Assert.IsFalse(e <= b);
      Assert.IsFalse(f <= b);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector4D u = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D v = new Vector4D(1.000001, 2.000001, 3.000001, 4.000001);
      Vector4D w = new Vector4D(1.00000001, 2.00000001, 3.00000001, 4.00000001);

      Assert.IsTrue(Vector4D.AreNumericallyEqual(u, u));
      Assert.IsFalse(Vector4D.AreNumericallyEqual(u, v));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(u, w));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Vector4D u = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D v = new Vector4D(1.002, 2.002, 3.002, 4.002);
      Vector4D w = new Vector4D(1.0001, 2.0001, 3.0001, 4.0001);

      Assert.IsTrue(Vector4D.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(Vector4D.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(Vector4D.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector4D u = new Vector4D(0.0, 0.0, 0.0, 0.0);
      Vector4D v = new Vector4D(1e-9, -1e-9, 1e-9, 1e-9);
      Vector4D w = new Vector4D(1e-7, 1e-7, -1e-7, 1e-7);

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Vector4D v0 = new Vector4D(678.0, 234.8, -123.987, 4.0);
      Vector4D v1 = new Vector4D(678.0, 234.8, -123.987, 4.0);
      Vector4D v2 = new Vector4D(67.0, 234.8, -123.987, 4.0);
      Vector4D v3 = new Vector4D(678.0, 24.8, -123.987, 4.0);
      Vector4D v4 = new Vector4D(678.0, 234.8, 123.987, 4.0);
      Vector4D v5 = new Vector4D(678.0, 234.8, 123.987, 4.1);
      Assert.IsTrue(v0.Equals(v0));
      Assert.IsTrue(v0.Equals(v1));
      Assert.IsFalse(v0.Equals(v2));
      Assert.IsFalse(v0.Equals(v3));
      Assert.IsFalse(v0.Equals(v4));
      Assert.IsFalse(v0.Equals(v5));
      Assert.IsFalse(v0.Equals(v0.ToString()));
    }


    [Test]
    public void AdditionOperator()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D b = new Vector4D(2.0, 3.0, 4.0, 5.0);
      Vector4D c = a + b;
      Assert.AreEqual(new Vector4D(3.0, 5.0, 7.0, 9.0), c);
    }


    [Test]
    public void Addition()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D b = new Vector4D(2.0, 3.0, 4.0, 5.0);
      Vector4D c = Vector4D.Add(a, b);
      Assert.AreEqual(new Vector4D(3.0, 5.0, 7.0, 9.0), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D b = new Vector4D(10.0, -10.0, 0.5, 2.5);
      Vector4D c = a - b;
      Assert.AreEqual(new Vector4D(-9.0, 12.0, 2.5, 1.5), c);
    }


    [Test]
    public void Subtraction()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D b = new Vector4D(10.0, -10.0, 0.5, 2.5);
      Vector4D c = Vector4D.Subtract(a, b);
      Assert.AreEqual(new Vector4D(-9.0, 12.0, 2.5, 1.5), c);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 6.0;
      double w = 0.4;
      double s = 13.3;

      Vector4D v = new Vector4D(x, y, z, w);

      Vector4D u = v * s;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
      Assert.AreEqual(w * s, u.W);

      u = s * v;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
      Assert.AreEqual(w * s, u.W);
    }


    [Test]
    public void Multiplication()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 6.0;
      double w = 0.4;
      double s = 13.3;

      Vector4D v = new Vector4D(x, y, z, w);

      Vector4D u = Vector4D.Multiply(s, v);
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
      Assert.AreEqual(w * s, u.W);
    }


    [Test]
    public void MultiplicationOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;
      double w1 = 0.2;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;
      double w2 = -0.2;

      Vector4D v = new Vector4D(x1, y1, z1, w1);
      Vector4D w = new Vector4D(x2, y2, z2, w2);

      Assert.AreEqual(new Vector4D(x1 * x2, y1 * y2, z1 * z2, w1 * w2), v * w);
    }


    [Test]
    public void MultiplicationVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;
      double w1 = 0.2;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;
      double w2 = -0.2;

      Vector4D v = new Vector4D(x1, y1, z1, w1);
      Vector4D w = new Vector4D(x2, y2, z2, w2);

      Assert.AreEqual(new Vector4D(x1 * x2, y1 * y2, z1 * z2, w1 * w2), Vector4D.Multiply(v, w));
    }


    [Test]
    public void DivisionOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 7.0;
      double w = 1.9;
      double s = 13.3;

      Vector4D v = new Vector4D(x, y, z, w);
      Vector4D u = v / s;
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
      Assert.IsTrue(Numeric.AreEqual(w / s, u.W));
    }


    [Test]
    public void Division()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 7.0;
      double w = 1.9;
      double s = 13.3;

      Vector4D v = new Vector4D(x, y, z, w);
      Vector4D u = Vector4D.Divide(v, s);
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
      Assert.IsTrue(Numeric.AreEqual(w / s, u.W));
    }


    [Test]
    public void DivisionVectorOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;
      double w1 = 0.2;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;
      double w2 = -0.2;

      Vector4D v = new Vector4D(x1, y1, z1, w1);
      Vector4D w = new Vector4D(x2, y2, z2, w2);

      Assert.AreEqual(new Vector4D(x1 / x2, y1 / y2, z1 / z2, w1 / w2), v / w);
    }


    [Test]
    public void DivisionVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;
      double w1 = 0.2;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;
      double w2 = -0.2;

      Vector4D v = new Vector4D(x1, y1, z1, w1);
      Vector4D w = new Vector4D(x2, y2, z2, w2);

      Assert.AreEqual(new Vector4D(x1 / x2, y1 / y2, z1 / z2, w1 / w2), Vector4D.Divide(v, w));
    }


    [Test]
    public void NegationOperator()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Assert.AreEqual(new Vector4D(-1.0, -2.0, -3.0, -4.0), -a);
    }


    [Test]
    public void Negation()
    {
      Vector4D a = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Assert.AreEqual(new Vector4D(-1.0, -2.0, -3.0, -4.0), Vector4D.Negate(a));
    }


    [Test]
    public void Constants()
    {
      Assert.AreEqual(0.0, Vector4D.Zero.X);
      Assert.AreEqual(0.0, Vector4D.Zero.Y);
      Assert.AreEqual(0.0, Vector4D.Zero.Z);
      Assert.AreEqual(0.0, Vector4D.Zero.W);

      Assert.AreEqual(1.0, Vector4D.One.X);
      Assert.AreEqual(1.0, Vector4D.One.Y);
      Assert.AreEqual(1.0, Vector4D.One.Z);
      Assert.AreEqual(1.0, Vector4D.One.W);

      Assert.AreEqual(1.0, Vector4D.UnitX.X);
      Assert.AreEqual(0.0, Vector4D.UnitX.Y);
      Assert.AreEqual(0.0, Vector4D.UnitX.Z);
      Assert.AreEqual(0.0, Vector4D.UnitX.W);

      Assert.AreEqual(0.0, Vector4D.UnitY.X);
      Assert.AreEqual(1.0, Vector4D.UnitY.Y);
      Assert.AreEqual(0.0, Vector4D.UnitY.Z);
      Assert.AreEqual(0.0, Vector4D.UnitX.W);

      Assert.AreEqual(0.0, Vector4D.UnitZ.X);
      Assert.AreEqual(0.0, Vector4D.UnitZ.Y);
      Assert.AreEqual(1.0, Vector4D.UnitZ.Z);
      Assert.AreEqual(0.0, Vector4D.UnitX.W);

      Assert.AreEqual(0.0, Vector4D.UnitW.X);
      Assert.AreEqual(0.0, Vector4D.UnitW.Y);
      Assert.AreEqual(0.0, Vector4D.UnitW.Z);
      Assert.AreEqual(1.0, Vector4D.UnitW.W);
    }


    [Test]
    public void IndexerRead()
    {
      Vector4D v = new Vector4D(1.0, -10e10, 0.0, 0.3);
      Assert.AreEqual(1.0, v[0]);
      Assert.AreEqual(-10e10, v[1]);
      Assert.AreEqual(0.0, v[2]);
      Assert.AreEqual(0.3, v[3]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException()
    {
      Vector4D v = new Vector4D(1.0, -10e10, 0.0, 0.3);
      Assert.AreEqual(1.0, v[-1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException2()
    {
      Vector4D v = new Vector4D(1.0, -10e10, 0.0, 0.3);
      Assert.AreEqual(1.0, v[4]);
    }


    [Test]
    public void IndexerWrite()
    {
      Vector4D v = Vector4D.Zero;
      v[0] = 1.0;
      v[1] = -10e10;
      v[2] = 0.1;
      v[3] = 0.3;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(-10e10, v.Y);
      Assert.AreEqual(0.1, v.Z);
      Assert.AreEqual(0.3, v.W);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException()
    {
      Vector4D v = new Vector4D(1.0, -10e10, 0.0, 0.3);
      v[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException2()
    {
      Vector4D v = new Vector4D(1.0, -10e10, 0.0, 0.3);
      v[4] = 0.0;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 4;
      Assert.IsFalse(new Vector4D().IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        Vector4D v = new Vector4D();
        v[i] = double.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNormalized()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-7;
      Vector4D arbitraryVector = new Vector4D(1.0, 0.001, 0.001, 0.001);
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      Vector4D normalizedVector = new Vector4D(1.00000001, 0.00000001, 0.000000001, 0.000000001);
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void TestIsZero()
    {
      Vector4D nonZero = new Vector4D(0.001, 0.001, 0.0, 0.001);
      Assert.IsFalse(Vector4D.AreNumericallyEqual(nonZero, Vector4D.Zero));

      Vector4D zero = new Vector4D(0.0000000000001, 0.0000000000001, 0.0, 0.0000000000001);
      Assert.IsTrue(Vector4D.AreNumericallyEqual(zero, Vector4D.Zero));
    }


    [Test]
    public void Length()
    {
      Assert.AreEqual(1.0, Vector4D.UnitX.Length);
      Assert.AreEqual(1.0, Vector4D.UnitY.Length);
      Assert.AreEqual(1.0, Vector4D.UnitZ.Length);
      Assert.AreEqual(1.0, Vector4D.UnitW.Length);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double w = 1.0;
      double length = (double)Math.Sqrt(x * x + y * y + z * z + w * w);
      Vector4D v = new Vector4D(x, y, z, w);
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void Length2()
    {
      Vector4D v = new Vector4D(1.0, 2.0, 3.0, 4.0);
      v.Length = 0.3;
      Assert.IsTrue(Numeric.AreEqual(0.3, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      Vector4D v = Vector4D.Zero;
      v.Length = 0.5;
    }


    [Test]
    public void LengthSquared()
    {
      Assert.AreEqual(1.0, Vector4D.UnitX.LengthSquared);
      Assert.AreEqual(1.0, Vector4D.UnitY.LengthSquared);
      Assert.AreEqual(1.0, Vector4D.UnitZ.LengthSquared);
      Assert.AreEqual(1.0, Vector4D.UnitW.LengthSquared);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double w = 1.0;
      double length = x * x + y * y + z * z + w * w;
      Vector4D v = new Vector4D(x, y, z, w);
      Assert.AreEqual(length, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      Vector4D v = new Vector4D(3.0, -1.0, 23.0, 0.4);
      Vector4D normalized = v.Normalized;
      Assert.AreEqual(new Vector4D(3.0, -1.0, 23.0, 0.4), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      Vector4D v = new Vector4D(3.0, -1.0, 23.0, 0.4);
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      Vector4D.Zero.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      Vector4D v = Vector4D.Zero;
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new Vector4D(1, 2, 3, 4);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector4D(1, 2, 3, 4).Normalized, v);

      v = new Vector4D(0, -1, 0, 0);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector4D(0, -1, 0, 0).Normalized, v);
    }


    [Test]
    public void DotProduct()
    {
      // 0°
      Assert.AreEqual(1.0, Vector4D.Dot(Vector4D.UnitX, Vector4D.UnitX));
      Assert.AreEqual(1.0, Vector4D.Dot(Vector4D.UnitY, Vector4D.UnitY));
      Assert.AreEqual(1.0, Vector4D.Dot(Vector4D.UnitZ, Vector4D.UnitZ));
      Assert.AreEqual(1.0, Vector4D.Dot(Vector4D.UnitW, Vector4D.UnitW));

      // 180°
      Assert.AreEqual(-1.0, Vector4D.Dot(Vector4D.UnitX, -Vector4D.UnitX));
      Assert.AreEqual(-1.0, Vector4D.Dot(Vector4D.UnitY, -Vector4D.UnitY));
      Assert.AreEqual(-1.0, Vector4D.Dot(Vector4D.UnitZ, -Vector4D.UnitZ));
      Assert.AreEqual(-1.0, Vector4D.Dot(Vector4D.UnitW, -Vector4D.UnitW));

      // 90°
      Assert.AreEqual(0.0, Vector4D.Dot(Vector4D.UnitX, Vector4D.UnitY));
      Assert.AreEqual(0.0, Vector4D.Dot(Vector4D.UnitY, Vector4D.UnitZ));
      Assert.AreEqual(0.0, Vector4D.Dot(Vector4D.UnitZ, Vector4D.UnitW));
      Assert.AreEqual(0.0, Vector4D.Dot(Vector4D.UnitW, Vector4D.UnitX));

      // 45°
      double angle = Math.Acos(Vector4D.Dot(new Vector4D(1, 1, 0, 0).Normalized, Vector4D.UnitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector4D.Dot(new Vector4D(0, 1, 1, 0).Normalized, Vector4D.UnitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector4D.Dot(new Vector4D(1, 0, 1, 0).Normalized, Vector4D.UnitZ));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector4D.Dot(new Vector4D(1, 0, 0, 1).Normalized, Vector4D.UnitW));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
    }


    [Test]
    public void XYZ()
    {
      Vector4D v4 = new Vector4D(1.0, 2.0, 3.0, 5.0);
      Vector3D v3 = v4.XYZ;
      Assert.AreEqual(new Vector3D(1.0, 2.0, 3.0), v3);

      v4.XYZ = new Vector3D(0.1f, 0.2f, 0.3f);
      Assert.AreEqual(0.1f, v4.X);
      Assert.AreEqual(0.2f, v4.Y);
      Assert.AreEqual(0.3f, v4.Z);
    }


    [Test]
    public void HomogeneousDivide()
    {
      Vector4D v4 = new Vector4D(1.0, 2.0, 3.0, 1.0);
      Vector3D v3 = Vector4D.HomogeneousDivide(v4);
      Assert.AreEqual(new Vector3D(1.0, 2.0, 3.0), v3);

      v4 = new Vector4D(1.0, 2.0, 3.0, 10.0);
      v3 = Vector4D.HomogeneousDivide(v4);
      Assert.IsTrue(Vector3D.AreNumericallyEqual(new Vector3D(1.0 / 10.0, 2.0 / 10.0, 3.0 / 10.0), v3));
    }


    [Test]
    public void ImplicitCastToVectorD()
    {
      Vector4D v = new Vector4D(1.1, 2.2, 3.3, 4.4);
      VectorD v2 = v;

      Assert.AreEqual(4, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
      Assert.AreEqual(3.3, v2[2]);
      Assert.AreEqual(4.4, v2[3]);
    }


    [Test]
    public void ToVectorD()
    {
      Vector4D v = new Vector4D(1.1, 2.2, 3.3, 4.4);
      VectorD v2 = v.ToVectorD();

      Assert.AreEqual(4, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
      Assert.AreEqual(3.3, v2[2]);
      Assert.AreEqual(4.4, v2[3]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Vector4 xna = new Vector4(6, 7, 8, 9);
      Vector4D v = (Vector4D)xna;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
      Assert.AreEqual(xna.W, v.W);
    }


    [Test]
    public void FromXna()
    {
      Vector4 xna = new Vector4(6, 7, 8, 9);
      Vector4D v = Vector4D.FromXna(xna);

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
      Assert.AreEqual(xna.W, v.W);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Vector4D v = new Vector4D(6, 7, 8, 9);
      Vector4 xna = (Vector4)v;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
      Assert.AreEqual(xna.W, v.W);
    }


    [Test]
    public void ToXna()
    {
      Vector4D v = new Vector4D(6, 7, 8, 9);
      Vector4 xna = v.ToXna();

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
      Assert.AreEqual(xna.W, v.W);
    }


    [Test]
    public void ExplicitCastToVector4F()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] elementsD = new[] { x, y, z, w };
      float[] elementsF = new[] { (float)x, (float)y, (float)z, (float)w };
      Vector4D vectorD = new Vector4D(elementsD);
      Vector4F vectorF = (Vector4F)vectorD;
      Assert.AreEqual(new Vector4F(elementsF), vectorF);
    }


    [Test]
    public void ToVector4F()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] elementsD = new[] { x, y, z, w };
      float[] elementsF = new[] { (float)x, (float)y, (float)z, (float)w };
      Vector4D vectorD = new Vector4D(elementsD);
      Vector4F vectorF = vectorD.ToVector4F();
      Assert.AreEqual(new Vector4F(elementsF), vectorF);
    }


    [Test]
    public void ExplicitArrayCast()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] values = (double[])new Vector4D(x, y, z, w);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);
    }


    [Test]
    public void ExplicitArrayCast2()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] values = (new Vector4D(x, y, z, w)).ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      double x = 23.5;
      double y = 0.0;
      double z = -11.0;
      double w = 0.3;
      List<double> values = (List<double>)new Vector4D(x, y, z, w);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);
    }


    [Test]
    public void ExplicitListCast2()
    {
      double x = 23.5;
      double y = 0.0;
      double z = -11.0;
      double w = 0.3;
      List<double> values = (new Vector4D(x, y, z, w)).ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);
    }


    [Test]
    public void ProjectTo()
    {
      // Project (1, 1, 1) to axes
      Vector4D v = new Vector4D(1, 1, 1, 0);
      Vector4D projection = Vector4D.ProjectTo(v, Vector4D.UnitX);
      Assert.AreEqual(Vector4D.UnitX, projection);
      projection = Vector4D.ProjectTo(v, Vector4D.UnitY);
      Assert.AreEqual(Vector4D.UnitY, projection);
      projection = Vector4D.ProjectTo(v, Vector4D.UnitZ);
      Assert.AreEqual(Vector4D.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector4D expected = new Vector4D(1, 1, 1, 0) / 3.0;
      projection = Vector4D.ProjectTo(Vector4D.UnitX, v);
      Assert.AreEqual(expected, projection);
      projection = Vector4D.ProjectTo(Vector4D.UnitY, v);
      Assert.AreEqual(expected, projection);
      projection = Vector4D.ProjectTo(Vector4D.UnitZ, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      // Project (1, 1, 1) to axes
      Vector4D projection = new Vector4D(1, 1, 1, 0);
      projection.ProjectTo(Vector4D.UnitX);
      Assert.AreEqual(Vector4D.UnitX, projection);
      projection = Vector4D.One;
      projection.ProjectTo(Vector4D.UnitY);
      Assert.AreEqual(Vector4D.UnitY, projection);
      projection = Vector4D.One;
      projection.ProjectTo(Vector4D.UnitZ);
      Assert.AreEqual(Vector4D.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector4D expected = new Vector4D(1, 1, 1, 0) / 3.0;
      projection = Vector4D.UnitX;
      projection.ProjectTo(new Vector4D(1, 1, 1, 0));
      Assert.AreEqual(expected, projection);
      projection = Vector4D.UnitY;
      projection.ProjectTo(new Vector4D(1, 1, 1, 0));
      Assert.AreEqual(expected, projection);
      projection = Vector4D.UnitZ;
      projection.ProjectTo(new Vector4D(1, 1, 1, 0));
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void Clamp1()
    {
      Vector4D clamped = new Vector4D(-10, 1, 100, 1000);
      clamped.Clamp(-100, 1000);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
      Assert.AreEqual(100, clamped.Z);
      Assert.AreEqual(1000, clamped.W);
    }


    [Test]
    public void Clamp2()
    {
      Vector4D clamped = new Vector4D(-10, 1, 100, 1000);
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
      Assert.AreEqual(0, clamped.W);
    }


    [Test]
    public void ClampStatic1()
    {
      Vector4D clamped = new Vector4D(-10, 1, 100, 1000);
      clamped = Vector4D.Clamp(clamped, -100, 1000);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
      Assert.AreEqual(100, clamped.Z);
      Assert.AreEqual(1000, clamped.W);
    }


    [Test]
    public void ClampStatic2()
    {
      Vector4D clamped = new Vector4D(-10, 1, 100, 1000);
      clamped = Vector4D.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
      Assert.AreEqual(0, clamped.W);
    }


    [Test]
    public void ClampToZero1()
    {
      Vector4D v = new Vector4D(
        Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2);
      v.ClampToZero();
      Assert.AreEqual(Vector4D.Zero, v);
      v = new Vector4D(-Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2, Numeric.EpsilonD);
      v.ClampToZero();
      Assert.AreNotEqual(Vector4D.Zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      Vector4D v = new Vector4D(0.1, 0.1, -0.1, 0.09);
      v.ClampToZero(0.11);
      Assert.AreEqual(Vector4D.Zero, v);
      v = new Vector4D(0.1, -0.11, 0.11, 0.0);
      v.ClampToZero(0.1);
      Assert.AreNotEqual(Vector4D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      Vector4D v = new Vector4D(
        Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2);
      v = Vector4D.ClampToZero(v);
      Assert.AreEqual(Vector4D.Zero, v);
      v = new Vector4D(-Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2, Numeric.EpsilonD);
      v = Vector4D.ClampToZero(v);
      Assert.AreNotEqual(Vector4D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      Vector4D v = new Vector4D(0.1, 0.1, -0.1, 0.09);
      v = Vector4D.ClampToZero(v, 0.11);
      Assert.AreEqual(Vector4D.Zero, v);
      v = new Vector4D(0.1, -0.11, 0.11, 0.0);
      v = Vector4D.ClampToZero(v, 0.1);
      Assert.AreNotEqual(Vector4D.Zero, v);
    }


    [Test]
    public void Min()
    {
      Vector4D v1 = new Vector4D(1.0, 2.0, 2.5, 4.0);
      Vector4D v2 = new Vector4D(-1.0, 2.0, 3.0, -2.0);
      Vector4D min = Vector4D.Min(v1, v2);
      Assert.AreEqual(new Vector4D(-1.0, 2.0, 2.5, -2.0), min);
    }


    [Test]
    public void Max()
    {
      Vector4D v1 = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D v2 = new Vector4D(-1.0, 2.1, 3.0, 8.0);
      Vector4D max = Vector4D.Max(v1, v2);
      Assert.AreEqual(new Vector4D(1.0, 2.1, 3.0, 8.0), max);
    }


    [Test]
    public void SerializationXml()
    {
      Vector4D v1 = new Vector4D(1.0, 2.0, 3.0, 4.0);
      Vector4D v2;
      string fileName = "SerializationVector4D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Vector4D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Vector4D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (Vector4D)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Vector4D v1 = new Vector4D(0.1, -0.2, 2, 40);
      Vector4D v2;
      string fileName = "SerializationVector4D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (Vector4D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      Vector4D v1 = new Vector4D(0.1, -0.2, 2, 40);
      Vector4D v2;

      string fileName = "SerializationVector4D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Vector4D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (Vector4D)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      Vector4D v1 = new Vector4D(0.1, -0.2, 2, 40);
      Vector4D v2;

      string fileName = "SerializationVector4D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Vector4D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (Vector4D)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void Parse()
    {
      Vector4D vector = Vector4D.Parse("(0.0123; 9.876; 0.0; -2.3)", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9.876, vector.Y);
      Assert.AreEqual(0.0, vector.Z);
      Assert.AreEqual(-2.3, vector.W);

      vector = Vector4D.Parse("(   0.0123   ;  9;  0.1 ; -2.3 ) ", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9, vector.Y);
      Assert.AreEqual(0.1, vector.Z);
      Assert.AreEqual(-2.3, vector.W);
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException()
    {
      Vector4D vector = Vector4D.Parse("(0.0123; 9.876)");
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException2()
    {
      Vector4D vector = Vector4D.Parse("xyzw");
    }


    [Test]
    public void ToStringAndParse()
    {
      Vector4D vector = new Vector4D(0.0123, 9.876, 0.0, -2.3);
      string s = vector.ToString();
      Vector4D parsedVector = Vector4D.Parse(s);
      Assert.AreEqual(vector, parsedVector);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Vector4D v = new Vector4D(-1, -2, -3, -4);
      Vector4D absoluteV = Vector4D.Absolute(v);

      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);
      Assert.AreEqual(4, absoluteV.W);

      v = new Vector4D(1, 2, 3, 4);
      absoluteV = Vector4D.Absolute(v);
      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);
      Assert.AreEqual(4, absoluteV.W);
    }


    [Test]
    public void Absolute()
    {
      Vector4D v = new Vector4D(-1, -2, -3, -4);
      v.Absolute();

      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);
      Assert.AreEqual(4, v.W);

      v = new Vector4D(1, 2, 3, 4);
      v.Absolute();
      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);
      Assert.AreEqual(4, v.W);
    }


    [Test]
    public void GetLargestComponent()
    {
      Vector4D v = new Vector4D(-1, -2, -3, -4);
      Assert.AreEqual(-1, v.LargestComponent);

      v = new Vector4D(10, 20, -30, -40);
      Assert.AreEqual(20, v.LargestComponent);

      v = new Vector4D(-1, 20, 30, 20);
      Assert.AreEqual(30, v.LargestComponent);

      v = new Vector4D(10, 20, 30, 40);
      Assert.AreEqual(40, v.LargestComponent);
    }


    [Test]
    public void GetIndexOfLargestComponent()
    {
      Vector4D v = new Vector4D(-1, -2, -3, -4);
      Assert.AreEqual(0, v.IndexOfLargestComponent);

      v = new Vector4D(10, 20, -30, -40);
      Assert.AreEqual(1, v.IndexOfLargestComponent);

      v = new Vector4D(-1, 20, 30, 20);
      Assert.AreEqual(2, v.IndexOfLargestComponent);

      v = new Vector4D(10, 20, 30, 40);
      Assert.AreEqual(3, v.IndexOfLargestComponent);
    }


    [Test]
    public void GetSmallestComponent()
    {
      Vector4D v = new Vector4D(-4, -2, -3, -1);
      Assert.AreEqual(-4, v.SmallestComponent);

      v = new Vector4D(10, 0, 3, 4);
      Assert.AreEqual(0, v.SmallestComponent);

      v = new Vector4D(-1, 20, -3, 0);
      Assert.AreEqual(-3, v.SmallestComponent);

      v = new Vector4D(40, 30, 20, 10);
      Assert.AreEqual(10, v.SmallestComponent);
    }


    [Test]
    public void GetIndexOfSmallestComponent()
    {
      Vector4D v = new Vector4D(-4, -2, -3, -1);
      Assert.AreEqual(0, v.IndexOfSmallestComponent);

      v = new Vector4D(10, 0, 3, 4);
      Assert.AreEqual(1, v.IndexOfSmallestComponent);

      v = new Vector4D(-1, 20, -3, 0);
      Assert.AreEqual(2, v.IndexOfSmallestComponent);

      v = new Vector4D(40, 30, 20, 10);
      Assert.AreEqual(3, v.IndexOfSmallestComponent);
    }
  }
}