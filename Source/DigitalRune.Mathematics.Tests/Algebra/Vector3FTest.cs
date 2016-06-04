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
  public class Vector3FTest
  {
    [Test]
    public void Constructors()
    {
      Vector3F v = new Vector3F();
      Assert.AreEqual(0.0, v.X);
      Assert.AreEqual(0.0, v.Y);
      Assert.AreEqual(0.0, v.Z);

      v = new Vector3F(2.3f);
      Assert.AreEqual(2.3f, v.X);
      Assert.AreEqual(2.3f, v.Y);
      Assert.AreEqual(2.3f, v.Z);

      v = new Vector3F(1.0f, 2.0f, 3.0f);
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
      Assert.AreEqual(3.0f, v.Z);

      v = new Vector3F(new[] { 1.0f, 2.0f, 3.0f });
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
      Assert.AreEqual(3.0f, v.Z);

      v = new Vector3F(new List<float>(new[] { 1.0f, 2.0f, 3.0f }));
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
      Assert.AreEqual(3.0f, v.Z);
    }


    [Test]
    public void Properties()
    {
      Vector3F v = new Vector3F();
      v.X = 1.0f;
      v.Y = 2.0f;
      v.Z = 3.0f;
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
      Assert.AreEqual(3.0f, v.Z);
      Assert.AreEqual(new Vector3F(1.0f, 2.0f, 3.0f), v);
    }


    [Test]
    public void HashCode()
    {
      Vector3F v = new Vector3F(1.0f, 2.0f, 3.0f);
      Assert.AreNotEqual(Vector3F.One.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualityOperators()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F b = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F c = new Vector3F(-1.0f, 2.0f, 3.0f);
      Vector3F d = new Vector3F(1.0f, -2.0f, 3.0f);
      Vector3F e = new Vector3F(1.0f, 2.0f, -3.0f);

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
      Vector3F a = new Vector3F(1.0f, 1.0f, 1.0f);
      Vector3F b = new Vector3F(0.5f, 0.5f, 0.5f);
      Vector3F c = new Vector3F(1.0f, 0.5f, 0.5f);
      Vector3F d = new Vector3F(0.5f, 1.0f, 0.5f);
      Vector3F e = new Vector3F(0.5f, 0.5f, 1.0f);

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
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Vector3F u = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F v = new Vector3F(1.000001f, 2.000001f, 3.000001f);
      Vector3F w = new Vector3F(1.00000001f, 2.00000001f, 3.00000001f);

      Assert.IsTrue(Vector3F.AreNumericallyEqual(u, u));
      Assert.IsFalse(Vector3F.AreNumericallyEqual(u, v));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(u, w));

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      float epsilon = 0.001f;
      Vector3F u = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F v = new Vector3F(1.002f, 2.002f, 3.002f);
      Vector3F w = new Vector3F(1.0001f, 2.0001f, 3.0001f);

      Assert.IsTrue(Vector3F.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(Vector3F.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Vector3F u = new Vector3F(0.0f, 0.0f, 0.0f);
      Vector3F v = new Vector3F(1e-9f, -1e-9f, 1e-9f);
      Vector3F w = new Vector3F(1e-7f, 1e-7f, -1e-7f);

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Vector3F v0 = new Vector3F(678.0f, 234.8f, -123.987f);
      Vector3F v1 = new Vector3F(678.0f, 234.8f, -123.987f);
      Vector3F v2 = new Vector3F(67.0f, 234.8f, -123.987f);
      Vector3F v3 = new Vector3F(678.0f, 24.8f, -123.987f);
      Vector3F v4 = new Vector3F(678.0f, 234.8f, 123.987f);
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
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F b = new Vector3F(2.0f, 3.0f, 4.0f);
      Vector3F c = a + b;
      Assert.AreEqual(new Vector3F(3.0f, 5.0f, 7.0f), c);
    }


    [Test]
    public void Addition()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F b = new Vector3F(2.0f, 3.0f, 4.0f);
      Vector3F c = Vector3F.Add(a, b);
      Assert.AreEqual(new Vector3F(3.0f, 5.0f, 7.0f), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F b = new Vector3F(10.0f, -10.0f, 0.5f);
      Vector3F c = a - b;
      Assert.AreEqual(new Vector3F(-9.0f, 12.0f, 2.5f), c);
    }


    [Test]
    public void Subtraction()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F b = new Vector3F(10.0f, -10.0f, 0.5f);
      Vector3F c = Vector3F.Subtract(a, b);
      Assert.AreEqual(new Vector3F(-9.0f, 12.0f, 2.5f), c);
    }


    [Test]
    public void MultiplicationOperator()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 6.0f;
      float s = 13.3f;

      Vector3F v = new Vector3F(x, y, z);

      Vector3F u = v * s;
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
      float x = 23.4f;
      float y = -11.0f;
      float z = 6.0f;
      float s = 13.3f;

      Vector3F v = new Vector3F(x, y, z);

      Vector3F u = Vector3F.Multiply(s, v);
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
      Assert.AreEqual(z * s, u.Z);
    }


    [Test]
    public void MultiplicationOperatorVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;
      float z1 = 6.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;
      float z2 = -6.0f;

      Vector3F v = new Vector3F(x1, y1, z1);
      Vector3F w = new Vector3F(x2, y2, z2);

      Assert.AreEqual(new Vector3F(x1 * x2, y1 * y2, z1 * z2), v * w);
    }


    [Test]
    public void MultiplicationVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;
      float z1 = 6.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;
      float z2 = -6.0f;

      Vector3F v = new Vector3F(x1, y1, z1);
      Vector3F w = new Vector3F(x2, y2, z2);

      Assert.AreEqual(new Vector3F(x1 * x2, y1 * y2, z1 * z2), Vector3F.Multiply(v, w));
    }


    [Test]
    public void DivisionOperator()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 7.0f;
      float s = 13.3f;

      Vector3F v = new Vector3F(x, y, z);
      Vector3F u = v / s;
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
    }


    [Test]
    public void Division()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 7.0f;
      float s = 13.3f;

      Vector3F v = new Vector3F(x, y, z);
      Vector3F u = Vector3F.Divide(v, s);
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
      Assert.IsTrue(Numeric.AreEqual(z / s, u.Z));
    }


    [Test]
    public void DivisionVectorOperatorVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;
      float z1 = 6.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;
      float z2 = -6.0f;

      Vector3F v = new Vector3F(x1, y1, z1);
      Vector3F w = new Vector3F(x2, y2, z2);

      Assert.AreEqual(new Vector3F(x1 / x2, y1 / y2, z1 / z2), v / w);
    }


    [Test]
    public void DivisionVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;
      float z1 = 6.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;
      float z2 = -6.0f;

      Vector3F v = new Vector3F(x1, y1, z1);
      Vector3F w = new Vector3F(x2, y2, z2);

      Assert.AreEqual(new Vector3F(x1 / x2, y1 / y2, z1 / z2), Vector3F.Divide(v, w));
    }


    [Test]
    public void NegationOperator()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Assert.AreEqual(new Vector3F(-1.0f, -2.0f, -3.0f), -a);
    }


    [Test]
    public void Negation()
    {
      Vector3F a = new Vector3F(1.0f, 2.0f, 3.0f);
      Assert.AreEqual(new Vector3F(-1.0f, -2.0f, -3.0f), Vector3F.Negate(a));
    }


    [Test]
    public void Constants()
    {
      Assert.AreEqual(0.0, Vector3F.Zero.X);
      Assert.AreEqual(0.0, Vector3F.Zero.Y);
      Assert.AreEqual(0.0, Vector3F.Zero.Z);

      Assert.AreEqual(1.0, Vector3F.One.X);
      Assert.AreEqual(1.0, Vector3F.One.Y);
      Assert.AreEqual(1.0, Vector3F.One.Z);

      Assert.AreEqual(1.0, Vector3F.UnitX.X);
      Assert.AreEqual(0.0, Vector3F.UnitX.Y);
      Assert.AreEqual(0.0, Vector3F.UnitX.Z);

      Assert.AreEqual(0.0, Vector3F.UnitY.X);
      Assert.AreEqual(1.0, Vector3F.UnitY.Y);
      Assert.AreEqual(0.0, Vector3F.UnitY.Z);

      Assert.AreEqual(0.0, Vector3F.UnitZ.X);
      Assert.AreEqual(0.0, Vector3F.UnitZ.Y);
      Assert.AreEqual(1.0, Vector3F.UnitZ.Z);
    }


    [Test]
    public void IndexerRead()
    {
      Vector3F v = new Vector3F(1.0f, -10e10f, 0.0f);
      Assert.AreEqual(1.0f, v[0]);
      Assert.AreEqual(-10e10f, v[1]);
      Assert.AreEqual(0.0f, v[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException()
    {
      Vector3F v = new Vector3F(1.0f, -10e10f, 0.0f);
      Assert.AreEqual(1.0, v[-1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException2()
    {
      Vector3F v = new Vector3F(1.0f, -10e10f, 0.0f);
      Assert.AreEqual(1.0f, v[3]);
    }


    [Test]
    public void IndexerWrite()
    {
      Vector3F v = Vector3F.Zero;
      v[0] = 1.0f;
      v[1] = -10e10f;
      v[2] = 0.1f;
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(-10e10f, v.Y);
      Assert.AreEqual(0.1f, v.Z);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException()
    {
      Vector3F v = new Vector3F(1.0f, -10e10f, 0.0f);
      v[-1] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException2()
    {
      Vector3F v = new Vector3F(1.0f, -10e10f, 0.0f);
      v[3] = 0.0f;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 3;
      Assert.IsFalse(new Vector3F().IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        Vector3F v = new Vector3F();
        v[i] = float.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNormalized()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-7f;

      Vector3F arbitraryVector = new Vector3F(1.0f, 0.001f, 0.001f);
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      Vector3F normalizedVector = new Vector3F(1.00000001f, 0.00000001f, 0.000000001f);
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void Length()
    {
      Assert.AreEqual(1.0, Vector3F.UnitX.Length);
      Assert.AreEqual(1.0, Vector3F.UnitY.Length);
      Assert.AreEqual(1.0, Vector3F.UnitZ.Length);

      float x = -1.9f;
      float y = 2.1f;
      float z = 10.0f;
      float length = (float)Math.Sqrt(x * x + y * y + z * z);
      Vector3F v = new Vector3F(x, y, z);
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void Length2()
    {
      Vector3F v = new Vector3F(1.0f, 2.0f, 3.0f);
      v.Length = 0.5f;
      Assert.IsTrue(Numeric.AreEqual(0.5f, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      Vector3F v = Vector3F.Zero;
      v.Length = 0.5f;
    }


    [Test]
    public void LengthSquared()
    {
      Assert.AreEqual(1.0, Vector3F.UnitX.LengthSquared);
      Assert.AreEqual(1.0, Vector3F.UnitY.LengthSquared);
      Assert.AreEqual(1.0, Vector3F.UnitZ.LengthSquared);

      float x = -1.9f;
      float y = 2.1f;
      float z = 10.0f;
      float length = x * x + y * y + z * z;
      Vector3F v = new Vector3F(x, y, z);
      Assert.AreEqual(length, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      Vector3F v = new Vector3F(3.0f, -1.0f, 23.0f);
      Vector3F normalized = v.Normalized;
      Assert.AreEqual(new Vector3F(3.0f, -1.0f, 23.0f), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      Vector3F v = new Vector3F(3.0f, -1.0f, 23.0f);
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      Vector3F.Zero.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      Vector3F v = Vector3F.Zero;
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new Vector3F(1, 2, 3);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector3F(1, 2, 3).Normalized, v);

      v = new Vector3F(0, -1, 0);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector3F(0, -1, 0).Normalized, v);
    }


    [Test]
    public void OrthogonalVectors()
    {
      Vector3F v = Vector3F.UnitX;
      Vector3F orthogonal1 = v.Orthonormal1;
      Vector3F orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(orthogonal1, orthogonal2)));

      v = Vector3F.UnitY;
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(orthogonal1, orthogonal2)));

      v = Vector3F.UnitZ;
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(orthogonal1, orthogonal2)));

      v = new Vector3F(23.0f, 44.0f, 21.0f);
      orthogonal1 = v.Orthonormal1;
      orthogonal2 = v.Orthonormal2;
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal1)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(v, orthogonal2)));
      Assert.IsTrue(Numeric.IsZero(Vector3F.Dot(orthogonal1, orthogonal2)));
    }


    [Test]
    public void DotProduct()
    {
      // 0°
      Assert.AreEqual(1.0, Vector3F.Dot(Vector3F.UnitX, Vector3F.UnitX));
      Assert.AreEqual(1.0, Vector3F.Dot(Vector3F.UnitY, Vector3F.UnitY));
      Assert.AreEqual(1.0, Vector3F.Dot(Vector3F.UnitZ, Vector3F.UnitZ));

      // 180°
      Assert.AreEqual(-1.0, Vector3F.Dot(Vector3F.UnitX, -Vector3F.UnitX));
      Assert.AreEqual(-1.0, Vector3F.Dot(Vector3F.UnitY, -Vector3F.UnitY));
      Assert.AreEqual(-1.0, Vector3F.Dot(Vector3F.UnitZ, -Vector3F.UnitZ));

      // 90°
      Assert.AreEqual(0.0, Vector3F.Dot(Vector3F.UnitX, Vector3F.UnitY));
      Assert.AreEqual(0.0, Vector3F.Dot(Vector3F.UnitY, Vector3F.UnitZ));
      Assert.AreEqual(0.0, Vector3F.Dot(Vector3F.UnitX, Vector3F.UnitZ));

      // 45°
      float angle = (float)Math.Acos(Vector3F.Dot(new Vector3F(1f, 1f, 0f).Normalized, Vector3F.UnitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(Vector3F.Dot(new Vector3F(0f, 1f, 1f).Normalized, Vector3F.UnitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(Vector3F.Dot(new Vector3F(1f, 0f, 1f).Normalized, Vector3F.UnitZ));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
    }


    [Test]
    public void CrossProduct()
    {
      Vector3F result = Vector3F.Cross(Vector3F.UnitX, Vector3F.UnitY);
      Assert.IsTrue(result == Vector3F.UnitZ);

      result = Vector3F.Cross(Vector3F.UnitY, Vector3F.UnitZ);
      Assert.IsTrue(result == Vector3F.UnitX);

      result = Vector3F.Cross(Vector3F.UnitZ, Vector3F.UnitX);
      Assert.IsTrue(result == Vector3F.UnitY);
    }


    [Test]
    public void GetAngle()
    {
      Vector3F x = Vector3F.UnitX;
      Vector3F y = Vector3F.UnitY;
      Vector3F halfvector = x + y;

      // 90°
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 4f, Vector3F.GetAngle(x, halfvector)));

      // 45°
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 2f, Vector3F.GetAngle(x, y)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void GetAngleException()
    {
      Vector3F.GetAngle(Vector3F.UnitX, Vector3F.Zero);
    }


    [Test]
    public void CrossProductMatrix()
    {
      Vector3F v = new Vector3F(-1.0f, -2.0f, -3.0f);
      Vector3F w = new Vector3F(4.0f, 5.0f, 6.0f);
      Matrix33F m = v.ToCrossProductMatrix();
      Assert.AreEqual(Vector3F.Cross(v, w), v.ToCrossProductMatrix() * w);
    }


    [Test]
    public void ImplicitCastToVectorF()
    {
      Vector3F v = new Vector3F(1.1f, 2.2f, 3.3f);
      VectorF v2 = v;

      Assert.AreEqual(3, v2.NumberOfElements);
      Assert.AreEqual(1.1f, v2[0]);
      Assert.AreEqual(2.2f, v2[1]);
      Assert.AreEqual(3.3f, v2[2]);
    }


    [Test]
    public void ToVectorF()
    {
      Vector3F v = new Vector3F(1.1f, 2.2f, 3.3f);
      VectorF v2 = v.ToVectorF();

      Assert.AreEqual(3, v2.NumberOfElements);
      Assert.AreEqual(1.1f, v2[0]);
      Assert.AreEqual(2.2f, v2[1]);
      Assert.AreEqual(3.3f, v2[2]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Vector3 xna = new Vector3(6, 7, 8);
      Vector3F v = (Vector3F)xna;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void FromXna()
    {
      Vector3 xna = new Vector3(6, 7, 8);
      Vector3F v = Vector3F.FromXna(xna);

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Vector3F v = new Vector3F(6, 7, 8);
      Vector3 xna = (Vector3)v;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ToXna()
    {
      Vector3F v = new Vector3F(6, 7, 8);
      Vector3 xna = v.ToXna();

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
      Assert.AreEqual(xna.Z, v.Z);
    }


    [Test]
    public void ExplicitFloatArrayCast()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float[] values = (float[])new Vector3F(x, y, z);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Length);
    }


    [Test]
    public void ExplicitFloatArrayCast2()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float[] values = (new Vector3F(x, y, z)).ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      float x = 23.5f;
      float y = 0.0f;
      float z = -11.0f;
      List<float> values = (List<float>)new Vector3F(x, y, z);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Count);
    }


    [Test]
    public void ExplicitListCast2()
    {
      float x = 23.5f;
      float y = 0.0f;
      float z = -11.0f;
      List<float> values = (new Vector3F(x, y, z)).ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(3, values.Count);
    }


    [Test]
    public void ImplicitVector3DCast()
    {
      float x = 23.5f;
      float y = 0.0f;
      float z = -11.0f;
      Vector3D vector3D = new Vector3F(x, y, z);
      Assert.AreEqual(x, vector3D[0]);
      Assert.AreEqual(y, vector3D[1]);
      Assert.AreEqual(z, vector3D[2]);
    }


    [Test]
    public void ToVector3D()
    {
      float x = 23.5f;
      float y = 0.0f;
      float z = -11.0f;
      Vector3D vector3D = new Vector3F(x, y, z).ToVector3D();
      Assert.AreEqual(x, vector3D[0]);
      Assert.AreEqual(y, vector3D[1]);
      Assert.AreEqual(z, vector3D[2]);
    }


    [Test]
    public void ProjectTo()
    {
      // Project (1, 1, 1) to axes
      Vector3F v = Vector3F.One;
      Vector3F projection = Vector3F.ProjectTo(v, Vector3F.UnitX);
      Assert.AreEqual(Vector3F.UnitX, projection);
      projection = Vector3F.ProjectTo(v, Vector3F.UnitY);
      Assert.AreEqual(Vector3F.UnitY, projection);
      projection = Vector3F.ProjectTo(v, Vector3F.UnitZ);
      Assert.AreEqual(Vector3F.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector3F expected = Vector3F.One / 3.0f;
      projection = Vector3F.ProjectTo(Vector3F.UnitX, v);
      Assert.AreEqual(expected, projection);
      projection = Vector3F.ProjectTo(Vector3F.UnitY, v);
      Assert.AreEqual(expected, projection);
      projection = Vector3F.ProjectTo(Vector3F.UnitZ, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      // Project (1, 1, 1) to axes
      Vector3F projection = Vector3F.One;
      projection.ProjectTo(Vector3F.UnitX);
      Assert.AreEqual(Vector3F.UnitX, projection);
      projection = Vector3F.One;
      projection.ProjectTo(Vector3F.UnitY);
      Assert.AreEqual(Vector3F.UnitY, projection);
      projection = Vector3F.One;
      projection.ProjectTo(Vector3F.UnitZ);
      Assert.AreEqual(Vector3F.UnitZ, projection);

      // Project axes to (1, 1, 1)
      Vector3F expected = Vector3F.One / 3.0f;
      projection = Vector3F.UnitX;
      projection.ProjectTo(Vector3F.One);
      Assert.AreEqual(expected, projection);
      projection = Vector3F.UnitY;
      projection.ProjectTo(Vector3F.One);
      Assert.AreEqual(expected, projection);
      projection = Vector3F.UnitZ;
      projection.ProjectTo(Vector3F.One);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void Clamp1()
    {
      Vector3F clamped = new Vector3F(-10f, 1f, 100f);
      clamped.Clamp(-100f, 100f);
      Assert.AreEqual(-10f, clamped.X);
      Assert.AreEqual(1f, clamped.Y);
      Assert.AreEqual(100f, clamped.Z);
    }


    [Test]
    public void Clamp2()
    {
      Vector3F clamped = new Vector3F(-10, 1, 100);
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
    }


    [Test]
    public void ClampStatic1()
    {
      Vector3F clamped = new Vector3F(-10f, 1f, 100f);
      clamped = Vector3F.Clamp(clamped, -100f, 100f);
      Assert.AreEqual(-10f, clamped.X);
      Assert.AreEqual(1f, clamped.Y);
      Assert.AreEqual(100f, clamped.Z);
    }


    [Test]
    public void ClampStatic2()
    {
      Vector3F clamped = new Vector3F(-10, 1, 100);
      clamped = Vector3F.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
      Assert.AreEqual(0, clamped.Z);
    }


    [Test]
    public void ClampToZero1()
    {
      Vector3F v = new Vector3F(Numeric.EpsilonF / 2, Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2);
      v.ClampToZero();
      Assert.AreEqual(Vector3F.Zero, v);
      v = new Vector3F(-Numeric.EpsilonF * 2, Numeric.EpsilonF, Numeric.EpsilonF * 2);
      v.ClampToZero();
      Assert.AreNotEqual(Vector4F.Zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      Vector3F v = new Vector3F(0.1f, 0.1f, -0.1f);
      v.ClampToZero(0.11f);
      Assert.AreEqual(Vector3F.Zero, v);
      v = new Vector3F(0.1f, -0.11f, 0.11f);
      v.ClampToZero(0.1f);
      Assert.AreNotEqual(Vector3F.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      Vector3F v = new Vector3F(Numeric.EpsilonF / 2, Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2);
      v = Vector3F.ClampToZero(v);
      Assert.AreEqual(Vector3F.Zero, v);
      v = new Vector3F(-Numeric.EpsilonF * 2, Numeric.EpsilonF, Numeric.EpsilonF * 2);
      v = Vector3F.ClampToZero(v);
      Assert.AreNotEqual(Vector3F.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      Vector3F v = new Vector3F(0.1f, 0.1f, -0.1f);
      v = Vector3F.ClampToZero(v, 0.11f);
      Assert.AreEqual(Vector3F.Zero, v);
      v = new Vector3F(0.1f, -0.11f, 0.11f);
      v = Vector3F.ClampToZero(v, 0.1f);
      Assert.AreNotEqual(Vector3F.Zero, v);
    }


    [Test]
    public void Min()
    {
      Vector3F v1 = new Vector3F(1.0f, 2.0f, 2.5f);
      Vector3F v2 = new Vector3F(-1.0f, 2.0f, 3.0f);
      Vector3F min = Vector3F.Min(v1, v2);
      Assert.AreEqual(new Vector3F(-1.0f, 2.0f, 2.5f), min);
    }


    [Test]
    public void Max()
    {
      Vector3F v1 = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F v2 = new Vector3F(-1.0f, 2.1f, 3.0f);
      Vector3F max = Vector3F.Max(v1, v2);
      Assert.AreEqual(new Vector3F(1.0f, 2.1f, 3.0f), max);
    }


    [Test]
    public void SerializationXml()
    {
      Vector3F v1 = new Vector3F(1.0f, 2.0f, 3.0f);
      Vector3F v2;
      string fileName = "SerializationVector3F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Vector3F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Vector3F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (Vector3F)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Vector3F v1 = new Vector3F(0.1f, -0.2f, 2);
      Vector3F v2;
      string fileName = "SerializationVector3F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (Vector3F)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      Vector3F v1 = new Vector3F(0.1f, -0.2f, 2);
      Vector3F v2;

      string fileName = "SerializationVector3F_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Vector3F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (Vector3F)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      Vector3F v1 = new Vector3F(0.1f, -0.2f, 2);
      Vector3F v2;

      string fileName = "SerializationVector3F.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Vector3F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (Vector3F)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }

    [Test]
    public void Parse()
    {
      Vector3F vector = Vector3F.Parse("(0.0123; 9.876; 0.0)", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123f, vector.X);
      Assert.AreEqual(9.876f, vector.Y);
      Assert.AreEqual(0.0f, vector.Z);

      vector = Vector3F.Parse("(   0.0123   ;  9;  0.1 ) ", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123f, vector.X);
      Assert.AreEqual(9f, vector.Y);
      Assert.AreEqual(0.1f, vector.Z);
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException()
    {
      Vector3F vector = Vector3F.Parse("(0.0123; 9.876)");
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException2()
    {
      Vector3F vector = Vector3F.Parse("xyz");
    }


    [Test]
    public void ToStringAndParse()
    {
      Vector3F vector = new Vector3F(0.0123f, 9.876f, -2.3f);
      string s = vector.ToString();
      Vector3F parsedVector = Vector3F.Parse(s);
      Assert.AreEqual(vector, parsedVector);
    }


    [Test]
    public void AbsoluteStatic()
    {
      Vector3F v = new Vector3F(-1, -2, -3);
      Vector3F absoluteV = Vector3F.Absolute(v);

      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);

      v = new Vector3F(1, 2, 3);
      absoluteV = Vector3F.Absolute(v);
      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
      Assert.AreEqual(3, absoluteV.Z);
    }


    [Test]
    public void Absolute()
    {
      Vector3F v = new Vector3F(-1, -2, -3);
      v.Absolute();

      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);

      v = new Vector3F(1, 2, 3);
      v.Absolute();
      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
      Assert.AreEqual(3, v.Z);
    }


    [Test]
    public void GetLargestComponent()
    {
      Vector3F v = new Vector3F(-1, -2, -3);
      Assert.AreEqual(-1, v.LargestComponent);

      v = new Vector3F(10, 20, -30);
      Assert.AreEqual(20, v.LargestComponent);

      v = new Vector3F(-1, 20, 30);
      Assert.AreEqual(30, v.LargestComponent);
    }


    [Test]
    public void GetIndexOfLargestComponent()
    {
      Vector3F v = new Vector3F(-1, -2, -3);
      Assert.AreEqual(0, v.IndexOfLargestComponent);

      v = new Vector3F(10, 20, -30);
      Assert.AreEqual(1, v.IndexOfLargestComponent);

      v = new Vector3F(-1, 20, 30);
      Assert.AreEqual(2, v.IndexOfLargestComponent);
    }


    [Test]
    public void GetSmallestComponent()
    {
      Vector3F v = new Vector3F(-4, -2, -3);
      Assert.AreEqual(-4, v.SmallestComponent);

      v = new Vector3F(10, 0, 3);
      Assert.AreEqual(0, v.SmallestComponent);

      v = new Vector3F(-1, 20, -3);
      Assert.AreEqual(-3, v.SmallestComponent);
    }


    [Test]
    public void GetIndexOfSmallestComponent()
    {
      Vector3F v = new Vector3F(-4, -2, -3);
      Assert.AreEqual(0, v.IndexOfSmallestComponent);

      v = new Vector3F(10, 0, 3);
      Assert.AreEqual(1, v.IndexOfSmallestComponent);

      v = new Vector3F(-1, 20, -3);
      Assert.AreEqual(2, v.IndexOfSmallestComponent);
    }
  }
}