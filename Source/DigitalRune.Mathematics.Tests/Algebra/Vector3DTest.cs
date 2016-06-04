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
  public class Vector3DTest
  {
    [Test]
    public void Constructors()
    {
      Vector3D v = new Vector3D();
      Assert.AreEqual(0.0, v.X);
      Assert.AreEqual(0.0, v.Y);
      Assert.AreEqual(0.0, v.Z);

      v = new Vector3D(2.3);
      Assert.AreEqual(2.3, v.X);
      Assert.AreEqual(2.3, v.Y);
      Assert.AreEqual(2.3, v.Z);

      v = new Vector3D(1.0, 2.0, 3.0);
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);

      v = new Vector3D(new[] { 1.0, 2.0, 3.0 });
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);

      v = new Vector3D(new List<double>(new[] { 1.0, 2.0, 3.0 }));
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
    }


    [Test]
    public void Properties()
    {
      Vector3D v = new Vector3D();
      v.X = 1.0;
      v.Y = 2.0;
      v.Z = 3.0;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(2.0, v.Y);
      Assert.AreEqual(3.0, v.Z);
      Assert.AreEqual(new Vector3D(1.0, 2.0, 3.0), v);
    }


    [Test]
    public void HashCode()
    {
      Vector3D v = new Vector3D(1.0, 2.0, 3.0);
      Assert.AreNotEqual(Vector3D.One.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualityOperators()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Vector3D b = new Vector3D(1.0, 2.0, 3.0);
      Vector3D c = new Vector3D(-1.0, 2.0, 3.0);
      Vector3D d = new Vector3D(1.0, -2.0, 3.0);
      Vector3D e = new Vector3D(1.0, 2.0, -3.0);

      Assert.IsTrue(a == b);
      Assert.IsFalse(a == c);
      Assert.IsFalse(a == d);
      Assert.IsFalse(a == e);
      Assert.IsFalse(a != b);
      Assert.IsTrue(a != c);
      Assert.IsTrue(a != d);
      Assert.IsTrue(a != e);
    }


    [Test]
    public void ComparisonOperators()
    {
      Vector3D a = new Vector3D(1.0, 1.0, 1.0);
      Vector3D b = new Vector3D(0.5, 0.5, 0.5);
      Vector3D c = new Vector3D(1.0, 0.5, 0.5);
      Vector3D d = new Vector3D(0.5, 1.0, 0.5);
      Vector3D e = new Vector3D(0.5, 0.5, 1.0);

      Assert.IsTrue(a > b);
      Assert.IsFalse(a > c);
      Assert.IsFalse(a > d);
      Assert.IsFalse(a > e);

      Assert.IsTrue(b < a);
      Assert.IsFalse(c < a);
      Assert.IsFalse(d < a);
      Assert.IsFalse(e < a);

      Assert.IsTrue(a >= b);
      Assert.IsTrue(a >= c);
      Assert.IsTrue(a >= d);
      Assert.IsTrue(a >= e);

      Assert.IsFalse(b >= a);
      Assert.IsFalse(b >= c);
      Assert.IsFalse(b >= d);
      Assert.IsFalse(b >= e);

      Assert.IsTrue(b <= a);
      Assert.IsTrue(c <= a);
      Assert.IsTrue(d <= a);
      Assert.IsTrue(e <= a);

      Assert.IsFalse(a <= b);
      Assert.IsFalse(c <= b);
      Assert.IsFalse(d <= b);
      Assert.IsFalse(e <= b);
    }


    [Test]
    public void AreEqual()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector3D u = new Vector3D(1.0, 2.0, 3.0);
      Vector3D v = new Vector3D(1.000001, 2.000001, 3.000001);
      Vector3D w = new Vector3D(1.00000001, 2.00000001, 3.00000001);

      Assert.IsTrue(Vector3D.AreNumericallyEqual(u, u));
      Assert.IsFalse(Vector3D.AreNumericallyEqual(u, v));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(u, w));

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      double epsilon = 0.001;
      Vector3D u = new Vector3D(1.0, 2.0, 3.0);
      Vector3D v = new Vector3D(1.002, 2.002, 3.002);
      Vector3D w = new Vector3D(1.0001, 2.0001, 3.0001);

      Assert.IsTrue(Vector3D.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(Vector3D.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(Vector3D.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Vector3D u = new Vector3D(0.0, 0.0, 0.0);
      Vector3D v = new Vector3D(1e-9, -1e-9, 1e-9);
      Vector3D w = new Vector3D(1e-7, 1e-7, -1e-7);

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Vector3D v0 = new Vector3D(678.0, 234.8, -123.987);
      Vector3D v1 = new Vector3D(678.0, 234.8, -123.987);
      Vector3D v2 = new Vector3D(67.0, 234.8, -123.987);
      Vector3D v3 = new Vector3D(678.0, 24.8, -123.987);
      Vector3D v4 = new Vector3D(678.0, 234.8, 123.987);
      Assert.IsTrue(v0.Equals(v0));
      Assert.IsTrue(v0.Equals(v1));
      Assert.IsFalse(v0.Equals(v2));
      Assert.IsFalse(v0.Equals(v3));
      Assert.IsFalse(v0.Equals(v4));
      Assert.IsFalse(v0.Equals(v0.ToString()));
    }


    [Test]
    public void AdditionOperator()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Vector3D b = new Vector3D(2.0, 3.0, 4.0);
      Vector3D c = a + b;
      Assert.AreEqual(new Vector3D(3.0, 5.0, 7.0), c);
    }


    [Test]
    public void Addition()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Vector3D b = new Vector3D(2.0, 3.0, 4.0);
      Vector3D c = Vector3D.Add(a, b);
      Assert.AreEqual(new Vector3D(3.0, 5.0, 7.0), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Vector3D b = new Vector3D(10.0, -10.0, 0.5);
      Vector3D c = a - b;
      Assert.AreEqual(new Vector3D(-9.0, 12.0, 2.5), c);
    }


    [Test]
    public void Subtraction()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Vector3D b = new Vector3D(10.0, -10.0, 0.5);
      Vector3D c = Vector3D.Subtract(a, b);
      Assert.AreEqual(new Vector3D(-9.0, 12.0, 2.5), c);
    }


    [Test]
    public void MultiplicationOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 6.0;
      double s = 13.3;

      Vector3D v = new Vector3D(x, y, z);

      Vector3D u = v * s;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);

      u = s * v;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
    }


    [Test]
    public void Multiplication()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 6.0;
      double s = 13.3;

      Vector3D v = new Vector3D(x, y, z);

      Vector3D u = Vector3D.Multiply(s, v);
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
    }


    [Test]
    public void MultiplicationOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;

      Vector3D v = new Vector3D(x1, y1, z1);
      Vector3D w = new Vector3D(x2, y2, z2);

      Assert.AreEqual(new Vector3D(x1 * x2, y1 * y2, z1 * z2), v * w);
    }


    [Test]
    public void MultiplicationVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;

      Vector3D v = new Vector3D(x1, y1, z1);
      Vector3D w = new Vector3D(x2, y2, z2);

      Assert.AreEqual(new Vector3D(x1 * x2, y1 * y2, z1 * z2), Vector3D.Multiply(v, w));
    }


    [Test]
    public void DivisionOperator()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 7.0;
      double s = 13.3;

      Vector3D v = new Vector3D(x, y, z);
      Vector3D u = v / s;
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
    }


    [Test]
    public void Division()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 7.0;
      double s = 13.3;

      Vector3D v = new Vector3D(x, y, z);
      Vector3D u = Vector3D.Divide(v, s);
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
    }


    [Test]
    public void DivisionVectorOperatorVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;

      Vector3D v = new Vector3D(x1, y1, z1);
      Vector3D w = new Vector3D(x2, y2, z2);

      Assert.AreEqual(new Vector3D(x1 / x2, y1 / y2, z1 / z2), v / w);
    }


    [Test]
    public void DivisionVector()
    {
      double x1 = 23.4;
      double y1 = -11.0;
      double z1 = 6.0;

      double x2 = 34.0;
      double y2 = 1.2;
      double z2 = -6.0;

      Vector3D v = new Vector3D(x1, y1, z1);
      Vector3D w = new Vector3D(x2, y2, z2);

      Assert.AreEqual(new Vector3D(x1 / x2, y1 / y2, z1 / z2), Vector3D.Divide(v, w));
    }


    [Test]
    public void NegationOperator()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Assert.AreEqual(new Vector3D(-1.0, -2.0, -3.0), -a);
    }


    [Test]
    public void Negation()
    {
      Vector3D a = new Vector3D(1.0, 2.0, 3.0);
      Assert.AreEqual(new Vector3D(-1.0, -2.0, -3.0), Vector3D.Negate(a));
    }


    [Test]
    public void Constants()
    {
      Assert.AreEqual(0.0, Vector3D.Zero.X);
      Assert.AreEqual(0.0, Vector3D.Zero.Y);
      Assert.AreEqual(0.0, Vector3D.Zero.Z);

      Assert.AreEqual(1.0, Vector3D.One.X);
      Assert.AreEqual(1.0, Vector3D.One.Y);
      Assert.AreEqual(1.0, Vector3D.One.Z);

      Assert.AreEqual(1.0, Vector3D.UnitX.X);
      Assert.AreEqual(0.0, Vector3D.UnitX.Y);
      Assert.AreEqual(0.0, Vector3D.UnitX.Z);

      Assert.AreEqual(0.0, Vector3D.UnitY.X);
      Assert.AreEqual(1.0, Vector3D.UnitY.Y);
      Assert.AreEqual(0.0, Vector3D.UnitY.Z);

      Assert.AreEqual(0.0, Vector3D.UnitZ.X);
      Assert.AreEqual(0.0, Vector3D.UnitZ.Y);
      Assert.AreEqual(1.0, Vector3D.UnitZ.Z);
    }


    [Test]
    public void IndexerRead()
    {
      Vector3D v = new Vector3D(1.0, -10e10, 0.0);
      Assert.AreEqual(1.0, v[0]);
      Assert.AreEqual(-10e10, v[1]);
      Assert.AreEqual(0.0, v[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException()
    {
      Vector3D v = new Vector3D(1.0, -10e10, 0.0);
      Assert.AreEqual(1.0, v[-1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException2()
    {
      Vector3D v = new Vector3D(1.0, -10e10, 0.0);
      Assert.AreEqual(1.0, v[3]);
    }


    [Test]
    public void IndexerWrite()
    {
      Vector3D v = Vector3D.Zero;
      v[0] = 1.0;
      v[1] = -10e10;
      v[2] = 0.1;
      Assert.AreEqual(1.0, v.X);
      Assert.AreEqual(-10e10, v.Y);
      Assert.AreEqual(0.1, v.Z);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException()
    {
      Vector3D v = new Vector3D(1.0, -10e10, 0.0);
      v[-1] = 0.0;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException2()
    {
      Vector3D v = new Vector3D(1.0, -10e10, 0.0);
      v[3] = 0.0;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 3;
      Assert.IsFalse(new Vector3D().IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        Vector3D v = new Vector3D();
        v[i] = double.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNormalized()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-7;

      Vector3D arbitraryVector = new Vector3D(1.0, 0.001, 0.001);
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      Vector3D normalizedVector = new Vector3D(1.00000001, 0.00000001, 0.000000001);
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void Length()
    {
      Assert.AreEqual(1.0, Vector3D.UnitX.Length);
      Assert.AreEqual(1.0, Vector3D.UnitY.Length);
      Assert.AreEqual(1.0, Vector3D.UnitZ.Length);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double length = (double)Math.Sqrt(x * x + y * y + z * z);
      Vector3D v = new Vector3D(x, y, z);
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void Length2()
    {
      Vector3D v = new Vector3D(1.0, 2.0, 3.0);
      v.Length = 0.5;
      Assert.IsTrue(Numeric.AreEqual(0.5, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      Vector3D v = Vector3D.Zero;
      v.Length = 0.5;
    }


    [Test]
    public void LengthSquared()
    {
      Assert.AreEqual(1.0, Vector3D.UnitX.LengthSquared);
      Assert.AreEqual(1.0, Vector3D.UnitY.LengthSquared);
      Assert.AreEqual(1.0, Vector3D.UnitZ.LengthSquared);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double length = x * x + y * y + z * z;
      Vector3D v = new Vector3D(x, y, z);
      Assert.AreEqual(length, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      Vector3D v = new Vector3D(3.0, -1.0, 23.0);
      Vector3D normalized = v.Normalized;
      Assert.AreEqual(new Vector3D(3.0, -1.0, 23.0), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      Vector3D v = new Vector3D(3.0, -1.0, 23.0);
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      Vector3D.Zero.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      Vector3D v = Vector3D.Zero;
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new Vector3D(1, 2, 3);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector3D(1, 2, 3).Normalized, v);

      v = new Vector3D(0, -1, 0);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector3D(0, -1, 0).Normalized, v);
    }


    [Test]
    public void OrthogonalVectors()
    {
      Vector3D v = Vector3D.UnitX;
      Vector3D orthogonal1 = v.Orthonormal1;
      Vector3D orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(orthogonal1, orthogonal2)));

      v = Vector3D.UnitY;
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(orthogonal1, orthogonal2)));

      v = Vector3D.UnitZ;
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(orthogonal1, orthogonal2)));

      v = new Vector3D(23.0, 44.0, 21.0);
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3D.Dot(orthogonal1, orthogonal2)));
    }


    [Test]
    public void DotProduct()
    {
      // 0°
      Assert.AreEqual(1.0, Vector3D.Dot(Vector3D.UnitX, Vector3D.UnitX));
      Assert.AreEqual(1.0, Vector3D.Dot(Vector3D.UnitY, Vector3D.UnitY));
      Assert.AreEqual(1.0, Vector3D.Dot(Vector3D.UnitZ, Vector3D.UnitZ));

      // 180°
      Assert.AreEqual(-1.0, Vector3D.Dot(Vector3D.UnitX, -Vector3D.UnitX));
      Assert.AreEqual(-1.0, Vector3D.Dot(Vector3D.UnitY, -Vector3D.UnitY));
      Assert.AreEqual(-1.0, Vector3D.Dot(Vector3D.UnitZ, -Vector3D.UnitZ));

      // 90°
      Assert.AreEqual(0.0, Vector3D.Dot(Vector3D.UnitX, Vector3D.UnitY));
      Assert.AreEqual(0.0, Vector3D.Dot(Vector3D.UnitY, Vector3D.UnitZ));
      Assert.AreEqual(0.0, Vector3D.Dot(Vector3D.UnitX, Vector3D.UnitZ));

      // 45°
      double angle = Math.Acos(Vector3D.Dot(new Vector3D(1, 1, 0).Normalized, Vector3D.UnitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector3D.Dot(new Vector3D(0, 1, 1).Normalized, Vector3D.UnitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(Vector3D.Dot(new Vector3D(1, 0, 1).Normalized, Vector3D.UnitZ));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
    }


    [Test]
    public void CrossProduct()
    {
      Vector3D result = Vector3D.Cross(Vector3D.UnitX, Vector3D.UnitY);
      Assert.IsTrue(result == Vector3D.UnitZ);

      result = Vector3D.Cross(Vector3D.UnitY, Vector3D.UnitZ);
      Assert.IsTrue(result == Vector3D.UnitX);

      result = Vector3D.Cross(Vector3D.UnitZ, Vector3D.UnitX);
      Assert.IsTrue(result == Vector3D.UnitY);
    }


    [Test]
    public void GetAngle()
    {
      Vector3D x = Vector3D.UnitX;
      Vector3D y = Vector3D.UnitY;
      Vector3D halfvector = x + y;

      // 90°
      Assert.IsTrue(Numeric.AreEqual((double)Math.PI / 4, Vector3D.GetAngle(x, halfvector)));

      // 45°
      Assert.IsTrue(Numeric.AreEqual((double)Math.PI / 2, Vector3D.GetAngle(x, y)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void GetAngleException()
    {
      Vector3D.GetAngle(Vector3D.UnitX, Vector3D.Zero);
    }


    [Test]
    public void CrossProductMatrix()
    {
      Vector3D v = new Vector3D(-1.0, -2.0, -3.0);
      Vector3D w = new Vector3D(4.0, 5.0, 6.0);
      Matrix33D m = v.ToCrossProductMatrix();
      Assert.AreEqual(Vector3D.Cross(v, w), v.ToCrossProductMatrix() * w);
    }


    [Test]
    public void ImplicitCastToVectorD()
    {
      Vector3D v = new Vector3D(1.1, 2.2, 3.3);
      VectorD v2 = v;

      Assert.AreEqual(3, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
      Assert.AreEqual(3.3, v2[2]);
    }


    [Test]
    public void ToVectorD()
    {
      Vector3D v = new Vector3D(1.1, 2.2, 3.3);
      VectorD v2 = v.ToVectorD();

      Assert.AreEqual(3, v2.NumberOfElements);
      Assert.AreEqual(1.1, v2[0]);
      Assert.AreEqual(2.2, v2[1]);
      Assert.AreEqual(3.3, v2[2]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Vector3 xna = new Vector3(6, 7, 8);
      Vector3D v = (Vector3D)xna;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void FromXna()
    {
      Vector3 xna = new Vector3(6, 7, 8);
      Vector3D v = Vector3D.FromXna(xna);

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Vector3D v = new Vector3D(6, 7, 8);
      Vector3 xna = (Vector3)v;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ToXna()
    {
      Vector3D v = new Vector3D(6, 7, 8);
      Vector3 xna = v.ToXna();

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ExplicitCastToVector3F()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double[] elementsD = new[] { x, y, z };
      float[] elementsF = new[] { (float)x, (float)y, (float)z };
      Vector3D vectorD = new Vector3D(elementsD);
      Vector3F vectorF = (Vector3F)vectorD;
      Assert.AreEqual(new Vector3F(elementsF), vectorF);
    }


    [Test]
    public void ToVector3F()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double[] elementsD = new[] { x, y, z };
      float[] elementsF = new[] { (float)x, (float)y, (float)z };
      Vector3D vectorD = new Vector3D(elementsD);
      Vector3F vectorF = vectorD.ToVector3F();
      Assert.AreEqual(new Vector3F(elementsF), vectorF);
    }


    [Test]
    public void ExplicitDoubleArrayCast()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double[] values = (double[])new Vector3D(x, y, z);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Length);
    }


    [Test]
    public void ExplicitDoubleArrayCast2()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double[] values = (new Vector3D(x, y, z)).ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      double x = 23.5;
      double y = 0.0;
      double z = -11.0;
      List<double> values = (List<double>)new Vector3D(x, y, z);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Count);
    }


    [Test]
    public void ExplicitListCast2()
    {
      double x = 23.5;
      double y = 0.0;
      double z = -11.0;
      List<double> values = (new Vector3D(x, y, z)).ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Count);
    }


    [Test]
    public void ProjectTo()
    {
      // Project (1, 1, 1) to axes
      Vector3D v = Vector3D.One;
      Vector3D projection = Vector3D.ProjectTo(v, Vector3D.UnitX);
      Assert.AreEqual(Vector3D.UnitX, projection);
      projection = Vector3D.ProjectTo(v, Vector3D.UnitY);
      Assert.AreEqual(Vector3D.UnitY, projection);
      projection = Vector3D.ProjectTo(v, Vector3D.UnitZ);
      Assert.AreEqual(Vector3D.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector3D expected = Vector3D.One / 3.0;
      projection = Vector3D.ProjectTo(Vector3D.UnitX, v);
      Assert.AreEqual(expected, projection);
      projection = Vector3D.ProjectTo(Vector3D.UnitY, v);
      Assert.AreEqual(expected, projection);
      projection = Vector3D.ProjectTo(Vector3D.UnitZ, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      // Project (1, 1, 1) to axes
      Vector3D projection = Vector3D.One;
      projection.ProjectTo(Vector3D.UnitX);
      Assert.AreEqual(Vector3D.UnitX, projection);
      projection = Vector3D.One;
      projection.ProjectTo(Vector3D.UnitY);
      Assert.AreEqual(Vector3D.UnitY, projection);
      projection = Vector3D.One;
      projection.ProjectTo(Vector3D.UnitZ);
      Assert.AreEqual(Vector3D.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector3D expected = Vector3D.One / 3.0;
      projection = Vector3D.UnitX;
      projection.ProjectTo(Vector3D.One);
      Assert.AreEqual(expected, projection);
      projection = Vector3D.UnitY;
      projection.ProjectTo(Vector3D.One);
      Assert.AreEqual(expected, projection);
      projection = Vector3D.UnitZ;
      projection.ProjectTo(Vector3D.One);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void Clamp1()
    {
      Vector3D clamped = new Vector3D(-10, 1, 100);
      clamped.Clamp(-100, 100);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
      Assert.AreEqual(100, clamped.Z);
    }


    [Test]
    public void Clamp2()
    {
      Vector3D clamped = new Vector3D(-10, 1, 100);
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
    }


    [Test]
    public void ClampStatic1()
    {
      Vector3D clamped = new Vector3D(-10, 1, 100);
      clamped = Vector3D.Clamp(clamped, -100, 100);
      Assert.AreEqual(-10, clamped.X);
      Assert.AreEqual(1, clamped.Y);
      Assert.AreEqual(100, clamped.Z);
    }


    [Test]
    public void ClampStatic2()
    {
      Vector3D clamped = new Vector3D(-10, 1, 100);
      clamped = Vector3D.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
    }


    [Test]
    public void ClampToZero1()
    {
      Vector3D v = new Vector3D(Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2);
      v.ClampToZero();
      Assert.AreEqual(Vector3D.Zero, v);
      v = new Vector3D(-Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2);
      v.ClampToZero();
      Assert.AreNotEqual(Vector4D.Zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      Vector3D v = new Vector3D(0.1, 0.1, -0.1);
      v.ClampToZero(0.11);
      Assert.AreEqual(Vector3D.Zero, v);
      v = new Vector3D(0.1, -0.11, 0.11);
      v.ClampToZero(0.1);
      Assert.AreNotEqual(Vector3D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      Vector3D v = new Vector3D(Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2);
      v = Vector3D.ClampToZero(v);
      Assert.AreEqual(Vector3D.Zero, v);
      v = new Vector3D(-Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2);
      v = Vector3D.ClampToZero(v);
      Assert.AreNotEqual(Vector3D.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      Vector3D v = new Vector3D(0.1, 0.1, -0.1);
      v = Vector3D.ClampToZero(v, 0.11);
      Assert.AreEqual(Vector3D.Zero, v);
      v = new Vector3D(0.1, -0.11, 0.11);
      v = Vector3D.ClampToZero(v, 0.1);
      Assert.AreNotEqual(Vector3D.Zero, v);
    }


    [Test]
    public void Min()
    {
      Vector3D v1 = new Vector3D(1.0, 2.0, 2.5);
      Vector3D v2 = new Vector3D(-1.0, 2.0, 3.0);
      Vector3D min = Vector3D.Min(v1, v2);
      Assert.AreEqual(new Vector3D(-1.0, 2.0, 2.5), min);
    }


    [Test]
    public void Max()
    {
      Vector3D v1 = new Vector3D(1.0, 2.0, 3.0);
      Vector3D v2 = new Vector3D(-1.0, 2.1, 3.0);
      Vector3D max = Vector3D.Max(v1, v2);
      Assert.AreEqual(new Vector3D(1.0, 2.1, 3.0), max);
    }


    [Test]
    public void SerializationXml()
    {
      Vector3D v1 = new Vector3D(1.0, 2.0, 3.0);
      Vector3D v2;
      string fileName = "SerializationVector3D.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Vector3D));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Vector3D));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (Vector3D)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Vector3D v1 = new Vector3D(0.1, -0.2, 2);
      Vector3D v2;
      string fileName = "SerializationVector3D.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (Vector3D)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      Vector3D v1 = new Vector3D(0.1, -0.2, 2);
      Vector3D v2;

      string fileName = "SerializationVector3D_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Vector3D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (Vector3D)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      Vector3D v1 = new Vector3D(0.1, -0.2, 2);
      Vector3D v2;

      string fileName = "SerializationVector3D.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Vector3D));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (Vector3D)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void Parse()
    {
      Vector3D vector = Vector3D.Parse("(0.0123; 9.876; 0.0)", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9.876, vector.Y);
      Assert.AreEqual(0.0, vector.Z);

      vector = Vector3D.Parse("(   0.0123   ;  9;  0.1 ) ", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123, vector.X);
      Assert.AreEqual(9, vector.Y);
      Assert.AreEqual(0.1, vector.Z);
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException()
    {
      Vector3D vector = Vector3D.Parse("(0.0123; 9.876)");
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException2()
    {
      Vector3D vector = Vector3D.Parse("xyz");
    }


    [Test]
    public void ToStringAndParse()
    {
      Vector3D vector = new Vector3D(0.0123, 9.876, -2.3);
      string s = vector.ToString();
      Vector3D parsedVector = Vector3D.Parse(s);
      Assert.AreEqual(vector, parsedVector);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Vector3D v = new Vector3D(-1, -2, -3);
      Vector3D absoluteV = Vector3D.Absolute(v);

      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);

      v = new Vector3D(1, 2, 3);
      absoluteV = Vector3D.Absolute(v);
      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);
    }


    [Test]
    public void Absolute()
    {
      Vector3D v = new Vector3D(-1, -2, -3);
      v.Absolute();

      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);

      v = new Vector3D(1, 2, 3);
      v.Absolute();
      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);
    }


    [Test]
    public void GetLargestComponent()
    {
      Vector3D v = new Vector3D(-1, -2, -3);
      Assert.AreEqual(-1, v.LargestComponent);

      v = new Vector3D(10, 20, -30);
      Assert.AreEqual(20, v.LargestComponent);

      v = new Vector3D(-1, 20, 30);
      Assert.AreEqual(30, v.LargestComponent);
    }


    [Test]
    public void GetIndexOfLargestComponent()
    {
      Vector3D v = new Vector3D(-1, -2, -3);
      Assert.AreEqual(0, v.IndexOfLargestComponent);

      v = new Vector3D(10, 20, -30);
      Assert.AreEqual(1, v.IndexOfLargestComponent);

      v = new Vector3D(-1, 20, 30);
      Assert.AreEqual(2, v.IndexOfLargestComponent);
    }


    [Test]
    public void GetSmallestComponent()
    {
      Vector3D v = new Vector3D(-4, -2, -3);
      Assert.AreEqual(-4, v.SmallestComponent);

      v = new Vector3D(10, 0, 3);
      Assert.AreEqual(0, v.SmallestComponent);

      v = new Vector3D(-1, 20, -3);
      Assert.AreEqual(-3, v.SmallestComponent);
    }


    [Test]
    public void GetIndexOfSmallestComponent()
    {
      Vector3D v = new Vector3D(-4, -2, -3);
      Assert.AreEqual(0, v.IndexOfSmallestComponent);

      v = new Vector3D(10, 0, 3);
      Assert.AreEqual(1, v.IndexOfSmallestComponent);

      v = new Vector3D(-1, 20, -3);
      Assert.AreEqual(2, v.IndexOfSmallestComponent);
    }
  }
}