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
  public class Vector2FTest
  {
    [Test]
    public void Constructors()
    {
      Vector2F v = new Vector2F();
      Assert.AreEqual(0.0, v.X);
      Assert.AreEqual(0.0, v.Y);

      v = new Vector2F(2.3f);
      Assert.AreEqual(2.3f, v.X);
      Assert.AreEqual(2.3f, v.Y);

      v = new Vector2F(1.0f, 2.0f);
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);

      v = new Vector2F(new[] { 1.0f, 2.0f });
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);

      v = new Vector2F(new List<float>(new[] { 1.0f, 2.0f }));
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
    }


    [Test]
    public void Properties()
    {
      Vector2F v = new Vector2F();
      v.X = 1.0f;
      v.Y = 2.0f;
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(2.0f, v.Y);
      Assert.AreEqual(new Vector2F(1.0f, 2.0f), v);
    }


    [Test]
    public void HashCode()
    {
      Vector2F v = new Vector2F(1.0f, 2.0f);
      Assert.AreNotEqual(Vector2F.One.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualityOperators()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Vector2F b = new Vector2F(1.0f, 2.0f);
      Vector2F c = new Vector2F(-1.0f, 2.0f);
      Vector2F d = new Vector2F(1.0f, -2.0f);

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
      Vector2F a = new Vector2F(1.0f, 1.0f);
      Vector2F b = new Vector2F(0.5f, 0.5f);
      Vector2F c = new Vector2F(1.0f, 0.5f);
      Vector2F d = new Vector2F(0.5f, 1.0f);

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
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Vector2F u = new Vector2F(1.0f, 2.0f);
      Vector2F v = new Vector2F(1.000001f, 2.000001f);
      Vector2F w = new Vector2F(1.00000001f, 2.00000001f);

      Assert.IsTrue(Vector2F.AreNumericallyEqual(u, u));
      Assert.IsFalse(Vector2F.AreNumericallyEqual(u, v));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(u, w));

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void AreEqualWithEpsilon()
    {
      float epsilon = 0.001f;
      Vector2F u = new Vector2F(1.0f, 2.0f);
      Vector2F v = new Vector2F(1.002f, 2.002f);
      Vector2F w = new Vector2F(1.0001f, 2.0001f);

      Assert.IsTrue(Vector2F.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(Vector2F.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(Vector2F.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Vector2F u = new Vector2F(0.0f, 0.0f);
      Vector2F v = new Vector2F(1e-9f, -1e-9f);
      Vector2F w = new Vector2F(1e-7f, 1e-7f);

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void TestEquals()
    {
      Vector2F v0 = new Vector2F(678.0f, 234.8f);
      Vector2F v1 = new Vector2F(678.0f, 234.8f);
      Vector2F v2 = new Vector2F(67.0f, 234.8f);
      Vector2F v3 = new Vector2F(678.0f, 24.8f);
      Assert.IsTrue(v0.Equals(v0));
      Assert.IsTrue(v0.Equals(v1));
      Assert.IsFalse(v0.Equals(v2));
      Assert.IsFalse(v0.Equals(v3));
      Assert.IsFalse(v0.Equals(v0.ToString()));
    }


    [Test]
    public void AdditionOperator()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Vector2F b = new Vector2F(2.0f, 3.0f);
      Vector2F c = a + b;
      Assert.AreEqual(new Vector2F(3.0f, 5.0f), c);
    }


    [Test]
    public void Addition()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Vector2F b = new Vector2F(2.0f, 3.0f);
      Vector2F c = Vector2F.Add(a, b);
      Assert.AreEqual(new Vector2F(3.0f, 5.0f), c);
    }


    [Test]
    public void SubtractionOperator()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Vector2F b = new Vector2F(10.0f, -10.0f);
      Vector2F c = a - b;
      Assert.AreEqual(new Vector2F(-9.0f, 12.0f), c);
    }


    [Test]
    public void Subtraction()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Vector2F b = new Vector2F(10.0f, -10.0f);
      Vector2F c = Vector2F.Subtract(a, b);
      Assert.AreEqual(new Vector2F(-9.0f, 12.0f), c);
    }


    [Test]
    public void MultiplicationOperator()
    {
      float x = 23.4f;
      float y = -11.0f;
      float s = 13.3f;

      Vector2F v = new Vector2F(x, y);

      Vector2F u = v * s;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);

      u = s * v;
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
    }


    [Test]
    public void Multiplication()
    {
      float x = 23.4f;
      float y = -11.0f;
      float s = 13.3f;

      Vector2F v = new Vector2F(x, y);

      Vector2F u = Vector2F.Multiply(s, v);
      Assert.AreEqual(x * s, u.X);
      Assert.AreEqual(y * s, u.Y);
    }


    [Test]
    public void MultiplicationOperatorVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;

      Vector2F v = new Vector2F(x1, y1);
      Vector2F w = new Vector2F(x2, y2);

      Assert.AreEqual(new Vector2F(x1 * x2, y1 * y2), v * w);
    }


    [Test]
    public void MultiplicationVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;

      Vector2F v = new Vector2F(x1, y1);
      Vector2F w = new Vector2F(x2, y2);

      Assert.AreEqual(new Vector2F(x1 * x2, y1 * y2), Vector2F.Multiply(v, w));
    }


    [Test]
    public void DivisionOperator()
    {
      float x = 23.4f;
      float y = -11.0f;
      float s = 13.3f;

      Vector2F v = new Vector2F(x, y);
      Vector2F u = v / s;
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
    }


    [Test]
    public void Division()
    {
      float x = 23.4f;
      float y = -11.0f;
      float s = 13.3f;

      Vector2F v = new Vector2F(x, y);
      Vector2F u = Vector2F.Divide(v, s);
      Assert.IsTrue(Numeric.AreEqual(x / s, u.X));
      Assert.IsTrue(Numeric.AreEqual(y / s, u.Y));
    }


    [Test]
    public void DivisionVectorOperatorVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;

      Vector2F v = new Vector2F(x1, y1);
      Vector2F w = new Vector2F(x2, y2);

      Assert.AreEqual(new Vector2F(x1 / x2, y1 / y2), v / w);
    }


    [Test]
    public void DivisionVector()
    {
      float x1 = 23.4f;
      float y1 = -11.0f;

      float x2 = 34.0f;
      float y2 = 1.2f;

      Vector2F v = new Vector2F(x1, y1);
      Vector2F w = new Vector2F(x2, y2);

      Assert.AreEqual(new Vector2F(x1 / x2, y1 / y2), Vector2F.Divide(v, w));
    }


    [Test]
    public void NegationOperator()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Assert.AreEqual(new Vector2F(-1.0f, -2.0f), -a);
    }


    [Test]
    public void Negation()
    {
      Vector2F a = new Vector2F(1.0f, 2.0f);
      Assert.AreEqual(new Vector2F(-1.0f, -2.0f), Vector2F.Negate(a));
    }


    [Test]
    public void Constants()
    {
      Assert.AreEqual(0.0, Vector2F.Zero.X);
      Assert.AreEqual(0.0, Vector2F.Zero.Y);

      Assert.AreEqual(1.0, Vector2F.One.X);
      Assert.AreEqual(1.0, Vector2F.One.Y);

      Assert.AreEqual(1.0, Vector2F.UnitX.X);
      Assert.AreEqual(0.0, Vector2F.UnitX.Y);

      Assert.AreEqual(0.0, Vector2F.UnitY.X);
      Assert.AreEqual(1.0, Vector2F.UnitY.Y);
    }


    [Test]
    public void IndexerRead()
    {
      Vector2F v = new Vector2F(1.0f, -10e10f);
      Assert.AreEqual(1.0f, v[0]);
      Assert.AreEqual(-10e10f, v[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException()
    {
      Vector2F v = new Vector2F(1.0f, -10e10f);
      Assert.AreEqual(1.0, v[-1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerReadException2()
    {
      Vector2F v = new Vector2F(1.0f, -10e10f);
      Assert.AreEqual(1.0f, v[2]);
    }


    [Test]
    public void IndexerWrite()
    {
      Vector2F v = Vector2F.Zero;
      v[0] = 1.0f;
      v[1] = -10e10f;
      Assert.AreEqual(1.0f, v.X);
      Assert.AreEqual(-10e10f, v.Y);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException()
    {
      Vector2F v = new Vector2F(1.0f, -10e10f);
      v[-1] = 0.0f;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IndexerWriteException2()
    {
      Vector2F v = new Vector2F(1.0f, -10e10f);
      v[2] = 0.0f;
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 2;
      Assert.IsFalse(new Vector2F().IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        Vector2F v = new Vector2F();
        v[i] = float.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNormalized()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-7f;

      Vector2F arbitraryVector = new Vector2F(1.0f, 0.001f);
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      Vector2F normalizedVector = new Vector2F(1.00000001f, 0.00000001f);
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void Length()
    {
      Assert.AreEqual(1.0, Vector2F.UnitX.Length);
      Assert.AreEqual(1.0, Vector2F.UnitY.Length);

      float x = -1.9f;
      float y = 2.1f;
      float length = (float)Math.Sqrt(x * x + y * y);
      Vector2F v = new Vector2F(x, y);
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void Length2()
    {
      Vector2F v = new Vector2F(1.0f, 2.0f);
      v.Length = 0.5f;
      Assert.IsTrue(Numeric.AreEqual(0.5f, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      Vector2F v = Vector2F.Zero;
      v.Length = 0.5f;
    }


    [Test]
    public void LengthSquared()
    {
      Assert.AreEqual(1.0, Vector2F.UnitX.LengthSquared);
      Assert.AreEqual(1.0, Vector2F.UnitY.LengthSquared);

      float x = -1.9f;
      float y = 2.1f;
      float length = x * x + y * y;
      Vector2F v = new Vector2F(x, y);
      Assert.AreEqual(length, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      Vector2F v = new Vector2F(3.0f, -1.0f);
      Vector2F normalized = v.Normalized;
      Assert.AreEqual(new Vector2F(3.0f, -1.0f), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      Vector2F v = new Vector2F(3.0f, -1.0f);
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      Vector2F.Zero.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      Vector2F v = Vector2F.Zero;
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new Vector2F(1, 2);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector2F(1, 2).Normalized, v);

      v = new Vector2F(0, -1);
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new Vector2F(0, -1).Normalized, v);
    }


    [Test]
    public void OrthogonalVectors()
    {
      Vector2F v = Vector2F.UnitX;
      Vector2F orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2F.Dot(v, orthogonal)));

      v = Vector2F.UnitY;
      orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2F.Dot(v, orthogonal)));

      v = new Vector2F(23.0f, 44.0f);
      orthogonal = v.Orthonormal;
      Assert.IsTrue(Numeric.IsZero(Vector2F.Dot(v, orthogonal)));
    }


    [Test]
    public void AbsoluteStatic()
    {
      Vector2F v = new Vector2F(-1, -2);
      Vector2F absoluteV = Vector2F.Absolute(v);

      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);

      v = new Vector2F(1, 2);
      absoluteV = Vector2F.Absolute(v);
      Assert.AreEqual(1, absoluteV.X);
      Assert.AreEqual(2, absoluteV.Y);
    }


    [Test]
    public void Absolute()
    {
      Vector2F v = new Vector2F(-1, -2);
      v.Absolute();

      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);

      v = new Vector2F(1, 2);
      v.Absolute();
      Assert.AreEqual(1, v.X);
      Assert.AreEqual(2, v.Y);
    }


    [Test]
    public void DotProduct()
    {
      // 0°
      Assert.AreEqual(1.0, Vector2F.Dot(Vector2F.UnitX, Vector2F.UnitX));
      Assert.AreEqual(1.0, Vector2F.Dot(Vector2F.UnitY, Vector2F.UnitY));

      // 180°
      Assert.AreEqual(-1.0, Vector2F.Dot(Vector2F.UnitX, -Vector2F.UnitX));
      Assert.AreEqual(-1.0, Vector2F.Dot(Vector2F.UnitY, -Vector2F.UnitY));

      // 90°
      Assert.AreEqual(0.0, Vector2F.Dot(Vector2F.UnitX, Vector2F.UnitY));

      // 45°
      float angle = (float)Math.Acos(Vector2F.Dot(new Vector2F(1f, 1f).Normalized, Vector2F.UnitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(Vector2F.Dot(new Vector2F(1f, 1f).Normalized, Vector2F.UnitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
    }


    [Test]
    public void GetAngle()
    {
      Vector2F x = Vector2F.UnitX;
      Vector2F y = Vector2F.UnitY;
      Vector2F halfvector = x + y;

      // 90°
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 4f, Vector2F.GetAngle(x, halfvector)));

      // 45°
      Assert.IsTrue(Numeric.AreEqual((float)Math.PI / 2f, Vector2F.GetAngle(x, y)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void GetAngleException()
    {
      Vector2F.GetAngle(Vector2F.UnitX, Vector2F.Zero);
    }


    [Test]
    public void ImplicitCastToVectorF()
    {
      Vector2F v = new Vector2F(1.1f, 2.2f);
      VectorF v2 = v;

      Assert.AreEqual(2, v2.NumberOfElements);
      Assert.AreEqual(1.1f, v2[0]);
      Assert.AreEqual(2.2f, v2[1]);
    }


    [Test]
    public void ToVectorF()
    {
      Vector2F v = new Vector2F(1.1f, 2.2f);
      VectorF v2 = v.ToVectorF();

      Assert.AreEqual(2, v2.NumberOfElements);
      Assert.AreEqual(1.1f, v2[0]);
      Assert.AreEqual(2.2f, v2[1]);
    }


    [Test]
    public void ExplicitFromXnaCast()
    {
      Vector2 xna = new Vector2(6, 7);
      Vector2F v = (Vector2F)xna;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void FromXna()
    {
      Vector2 xna = new Vector2(6, 7);
      Vector2F v = Vector2F.FromXna(xna);

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ExplicitToXnaCast()
    {
      Vector2F v = new Vector2F(6, 7);
      Vector2 xna = (Vector2)v;

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ToXna()
    {
      Vector2F v = new Vector2F(6, 7);
      Vector2 xna = v.ToXna();

      Assert.AreEqual(xna.X, v.X);
      Assert.AreEqual(xna.Y, v.Y);
    }


    [Test]
    public void ExplicitFloatArrayCast()
    {
      float x = 23.4f;
      float y = -11.0f;
      float[] values = (float[])new Vector2F(x, y);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Length);
    }


    [Test]
    public void ExplicitFloatArrayCast2()
    {
      float x = 23.4f;
      float y = -11.0f;
      float[] values = (new Vector2F(x, y)).ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      float x = 23.5f;
      float y = 0.0f;
      List<float> values = (List<float>)new Vector2F(x, y);
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Count);
    }


    [Test]
    public void ExplicitListCast2()
    {
      float x = 23.5f;
      float y = 0.0f;
      List<float> values = (new Vector2F(x, y)).ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(2, values.Count);
    }


    [Test]
    public void ImplicitVector2DCast()
    {
      float x = 23.5f;
      float y = 0.0f;
      Vector2D vector2D = new Vector2F(x, y);
      Assert.AreEqual(x, vector2D[0]);
      Assert.AreEqual(y, vector2D[1]);
    }


    [Test]
    public void ToVector2D()
    {
      float x = 23.5f;
      float y = 0.0f;
      Vector2D vector2D = new Vector2F(x, y).ToVector2D();
      Assert.AreEqual(x, vector2D[0]);
      Assert.AreEqual(y, vector2D[1]);
    }


    [Test]
    public void ProjectTo()
    {
      // Project (1, 1) to axes
      Vector2F v = Vector2F.One;
      Vector2F projection = Vector2F.ProjectTo(v, Vector2F.UnitX);
      Assert.AreEqual(Vector2F.UnitX, projection);
      projection = Vector2F.ProjectTo(v, Vector2F.UnitY);
      Assert.AreEqual(Vector2F.UnitY, projection);

      // Project axes to (1, 1)
      Vector2F expected = Vector2F.One / 2.0f;
      projection = Vector2F.ProjectTo(Vector2F.UnitX, v);
      Assert.AreEqual(expected, projection);
      projection = Vector2F.ProjectTo(Vector2F.UnitY, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      // Project (1, 1) to axes
      Vector2F projection = Vector2F.One;
      projection.ProjectTo(Vector2F.UnitX);
      Assert.AreEqual(Vector2F.UnitX, projection);
      projection = Vector2F.One;
      projection.ProjectTo(Vector2F.UnitY);
      Assert.AreEqual(Vector2F.UnitY, projection);
      projection = Vector2F.One;

      // Project axes to (1, 1, 1)
      Vector2F expected = Vector2F.One / 2.0f;
      projection = Vector2F.UnitX;
      projection.ProjectTo(Vector2F.One);
      Assert.AreEqual(expected, projection);
      projection = Vector2F.UnitY;
      projection.ProjectTo(Vector2F.One);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void Clamp1()
    {
      Vector2F clamped = new Vector2F(-10f, 1f);
      clamped.Clamp(-100f, 100f);
      Assert.AreEqual(-10f, clamped.X);
      Assert.AreEqual(1f, clamped.Y);
    }


    [Test]
    public void Clamp2()
    {
      Vector2F clamped = new Vector2F(-10, 1);
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
    }


    [Test]
    public void ClampStatic1()
    {
      Vector2F clamped = new Vector2F(-10f, 1f);
      clamped = Vector2F.Clamp(clamped, -100f, 100f);
      Assert.AreEqual(-10f, clamped.X);
      Assert.AreEqual(1f, clamped.Y);
    }


    [Test]
    public void ClampStatic2()
    {
      Vector2F clamped = new Vector2F(-10, 1);
      clamped = Vector2F.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped.X);
      Assert.AreEqual(0, clamped.Y);
    }


    [Test]
    public void ClampToZero1()
    {
      Vector2F v = new Vector2F(Numeric.EpsilonF / 2, Numeric.EpsilonF / 2);
      v.ClampToZero();
      Assert.AreEqual(Vector2F.Zero, v);
      v = new Vector2F(-Numeric.EpsilonF * 2, Numeric.EpsilonF);
      v.ClampToZero();
      Assert.AreNotEqual(Vector2F.Zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      Vector2F v = new Vector2F(0.1f, 0.1f);
      v.ClampToZero(0.11f);
      Assert.AreEqual(Vector2F.Zero, v);
      v = new Vector2F(0.1f, -0.11f);
      v.ClampToZero(0.1f);
      Assert.AreNotEqual(Vector2F.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      Vector2F v = new Vector2F(Numeric.EpsilonF / 2, Numeric.EpsilonF / 2);
      v = Vector2F.ClampToZero(v);
      Assert.AreEqual(Vector2F.Zero, v);
      v = new Vector2F(-Numeric.EpsilonF * 2, Numeric.EpsilonF);
      v = Vector2F.ClampToZero(v);
      Assert.AreNotEqual(Vector2F.Zero, v);
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      Vector2F v = new Vector2F(0.1f, 0.1f);
      v = Vector2F.ClampToZero(v, 0.11f);
      Assert.AreEqual(Vector2F.Zero, v);
      v = new Vector2F(0.1f, -0.11f);
      v = Vector2F.ClampToZero(v, 0.1f);
      Assert.AreNotEqual(Vector2F.Zero, v);
    }


    [Test]
    public void Min()
    {
      Vector2F v1 = new Vector2F(1.0f, 2.0f);
      Vector2F v2 = new Vector2F(-1.0f, 2.0f);
      Vector2F min = Vector2F.Min(v1, v2);
      Assert.AreEqual(new Vector2F(-1.0f, 2.0f), min);
    }


    [Test]
    public void Max()
    {
      Vector2F v1 = new Vector2F(1.0f, 2.0f);
      Vector2F v2 = new Vector2F(-1.0f, 2.1f);
      Vector2F max = Vector2F.Max(v1, v2);
      Assert.AreEqual(new Vector2F(1.0f, 2.1f), max);
    }


    [Test]
    public void SerializationXml()
    {
      Vector2F v1 = new Vector2F(1.0f, 2.0f);
      Vector2F v2;
      string fileName = "SerializationVector2F.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(Vector2F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(Vector2F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (Vector2F)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      Vector2F v1 = new Vector2F(0.1f, -0.2f);
      Vector2F v2;
      string fileName = "SerializationVector2F.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (Vector2F)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      Vector2F v1 = new Vector2F(0.1f, -0.2f);
      Vector2F v2;

      string fileName = "SerializationVector2F_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(Vector2F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (Vector2F)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      Vector2F v1 = new Vector2F(0.1f, -0.2f);
      Vector2F v2;

      string fileName = "SerializationVector2F.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(Vector2F));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (Vector2F)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void Parse()
    {
      Vector2F vector = Vector2F.Parse("(0.0123; 9.876)", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123f, vector.X);
      Assert.AreEqual(9.876f, vector.Y);

      vector = Vector2F.Parse("(   0.0123   ;  9 ) ", CultureInfo.InvariantCulture);
      Assert.AreEqual(0.0123f, vector.X);
      Assert.AreEqual(9f, vector.Y);
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException()
    {
      Vector2F vector = Vector2F.Parse("(0.0123; )");
    }


    [Test]
    [ExpectedException(typeof(FormatException))]
    public void ParseException2()
    {
      Vector2F vector = Vector2F.Parse("xyz");
    }


    [Test]
    public void ToStringAndParse()
    {
      Vector2F vector = new Vector2F(0.0123f, 9.876f);
      string s = vector.ToString();
      Vector2F parsedVector = Vector2F.Parse(s);
      Assert.AreEqual(vector, parsedVector);
    }


    [Test]
    public void GetLargestComponent()
    {
      Vector2F v = new Vector2F(-1, -2);
      Assert.AreEqual(-1, v.LargestComponent);

      v = new Vector2F(10, 20);
      Assert.AreEqual(20, v.LargestComponent);
    }


    [Test]
    public void GetIndexOfLargestComponent()
    {
      Vector2F v = new Vector2F(-1, -2);
      Assert.AreEqual(0, v.IndexOfLargestComponent);

      v = new Vector2F(10, 20);
      Assert.AreEqual(1, v.IndexOfLargestComponent);
    }


    [Test]
    public void GetSmallestComponent()
    {
      Vector2F v = new Vector2F(-4, -2);
      Assert.AreEqual(-4, v.SmallestComponent);

      v = new Vector2F(10, 0);
      Assert.AreEqual(0, v.SmallestComponent);
    }


    [Test]
    public void GetIndexOfSmallestComponent()
    {
      Vector2F v = new Vector2F(-4, -2);
      Assert.AreEqual(0, v.IndexOfSmallestComponent);

      v = new Vector2F(10, 0);
      Assert.AreEqual(1, v.IndexOfSmallestComponent);
    }
  }
}