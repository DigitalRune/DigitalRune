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
  public class VectorFTest
  {
    [Test]
    public void Constructors()
    {
      VectorF v = new VectorF(4);
      Assert.AreEqual(4, v.NumberOfElements);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(0.0f, v[i]);

      v = new VectorF(21, 0.123f);
      Assert.AreEqual(21, v.NumberOfElements);
      for (int i = 0; i < 21; i++)
        Assert.AreEqual(0.123f, v[i]);

      v = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v = new VectorF(new List<float>(new float[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException1()
    {
      VectorF v = new VectorF(0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      VectorF v = new VectorF(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithIListShouldThrowArgumentNullException()
    {
      VectorF v = new VectorF(1);
      v.Set((IList<float>)null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithArrayShouldThrowArgumentNullException()
    {
      VectorF v = new VectorF(1);
      v.Set((float[])null);
    }


    [Test]
    public void Set()
    {
      VectorF v = new VectorF(5);
      v.Set(0.123f);
      Assert.AreEqual(5, v.NumberOfElements);
      for (int i = 0; i < 5; i++)
        Assert.AreEqual(0.123f, v[i]);

      v.Set(new VectorF(new float[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v.Set(new float[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v.Set(new List<float>(new float[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);
    }


    [Test]
    public void SetSubvector()
    {
      VectorF v = new VectorF(5);
      v.SetSubvector(0, new VectorF(new float[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v = new VectorF(5);
      v.SetSubvector(0, new VectorF(new float[] { 1, 2, 3, 4 }));
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(0, v[4]);

      v = new VectorF(5);
      v.SetSubvector(2, new VectorF(new float[] { 1, 2, 3 }));
      Assert.AreEqual(0, v[0]);
      Assert.AreEqual(0, v[1]);
      Assert.AreEqual(1, v[2]);
      Assert.AreEqual(2, v[3]);
      Assert.AreEqual(3, v[4]);
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubvectorException1()
    {
      VectorF v = new VectorF(5);
      v.SetSubvector(1, new VectorF(new float[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubvectorException2()
    {
      VectorF v = new VectorF(5);
      v.SetSubvector(-1, new VectorF(new float[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubvectorException3()
    {
      VectorF v = new VectorF(5);
      v.SetSubvector(1, new VectorF(new float[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetSubvectorException4()
    {
      VectorF v = new VectorF(5);
      v.SetSubvector(1, null);
    }


    [Test]
    public void GetSubvector()
    {
      VectorF v = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF subvector = v.GetSubvector(0, 5);
      Assert.AreEqual(5, subvector.NumberOfElements);
      Assert.AreEqual(1, subvector[0]);
      Assert.AreEqual(2, subvector[1]);
      Assert.AreEqual(3, subvector[2]);
      Assert.AreEqual(4, subvector[3]);
      Assert.AreEqual(5, subvector[4]);

      subvector = v.GetSubvector(0, 3);
      Assert.AreEqual(3, subvector.NumberOfElements);
      Assert.AreEqual(1, subvector[0]);
      Assert.AreEqual(2, subvector[1]);
      Assert.AreEqual(3, subvector[2]);

      subvector = v.GetSubvector(2, 3);
      Assert.AreEqual(3, subvector.NumberOfElements);
      Assert.AreEqual(3, subvector[0]);
      Assert.AreEqual(4, subvector[1]);
      Assert.AreEqual(5, subvector[2]);

      subvector = v.GetSubvector(3, 1);
      Assert.AreEqual(1, subvector.NumberOfElements);
      Assert.AreEqual(4, subvector[0]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException()
    {
      VectorF v = new VectorF(4);
      v.GetSubvector(-1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException2()
    {
      VectorF v = new VectorF(4);
      v.GetSubvector(4, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException3()
    {
      VectorF v = new VectorF(4);
      v.GetSubvector(0, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException4()
    {
      VectorF v = new VectorF(4);
      v.GetSubvector(2, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException5()
    {
      VectorF v = new VectorF(4);
      v.GetSubvector(0, 5);
    }


    [Test]
    public void HashCodeTest()
    {
      VectorF v = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF one = new VectorF(5, 1.0f);
      Assert.AreNotEqual(one.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      VectorF v0 = new VectorF(new[] { 678.0f, 234.8f, -123.987f, 4.0f });
      VectorF v1 = new VectorF(new[] { 678.0f, 234.8f, -123.987f, 4.0f });
      VectorF v2 = new VectorF(new[] { 67.0f, 234.8f, -123.987f, 4.0f });
      VectorF v3 = new VectorF(new[] { 678.0f, 24.8f, -123.987f, 4.0f });
      VectorF v4 = new VectorF(new[] { 678.0f, 234.8f, 123.987f, 4.0f });
      VectorF v5 = new VectorF(new[] { 678.0f, 234.8f, 123.987f, 4.1f });
      VectorF v6 = new VectorF(new[] { 678.0f, 234.8f, -123.987f });
      Assert.IsTrue(v0.Equals(v0));
      Assert.IsTrue(v0.Equals(v1));
      Assert.IsFalse(v0.Equals(v2));
      Assert.IsFalse(v0.Equals(v3));
      Assert.IsFalse(v0.Equals(v4));
      Assert.IsFalse(v0.Equals(v5));
      Assert.IsFalse(v0.Equals(v0.ToString()));
      Assert.IsFalse(v0.Equals(v6));
    }


    [Test]
    public void EqualityOperators()
    {
      VectorF a = new VectorF(new[] { 1.0f, 2.0f, 3.0f, 4.0f });
      VectorF b = new VectorF(new[] { 1.0f, 2.0f, 3.0f, 4.0f });
      VectorF c = new VectorF(new[] { -1.0f, 2.0f, 3.0f, 4.0f });
      VectorF d = new VectorF(new[] { 1.0f, -2.0f, 3.0f, 4.0f });
      VectorF e = new VectorF(new[] { 1.0f, 2.0f, -3.0f, 4.0f });
      VectorF f = new VectorF(new[] { 1.0f, 2.0f, 3.0f, -4.0f });

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
      VectorF a = new VectorF(new[] { 1.0f, 1.0f, 1.0f, 1.0f });
      VectorF b = new VectorF(new[] { 0.5f, 0.5f, 0.5f, 0.5f });
      VectorF c = new VectorF(new[] { 1.0f, 0.5f, 0.5f, 0.5f });
      VectorF d = new VectorF(new[] { 0.5f, 1.0f, 0.5f, 0.5f });
      VectorF e = new VectorF(new[] { 0.5f, 0.5f, 1.0f, 0.5f });
      VectorF f = new VectorF(new[] { 0.5f, 0.5f, 0.5f, 1.0f });

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
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException1()
    {
      VectorF v = new VectorF(1);
      bool result = v < null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException2()
    {
      VectorF v = new VectorF(1);
      bool result = v <= null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException3()
    {
      VectorF v = new VectorF(1);
      bool result = v > null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException4()
    {
      VectorF v = new VectorF(1);
      bool result = v >= null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException5()
    {
      VectorF v = new VectorF(1);
      bool result = null < v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException6()
    {
      VectorF v = new VectorF(1);
      bool result = null <= v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException7()
    {
      VectorF v = new VectorF(1);
      bool result = null > v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException8()
    {
      VectorF v = new VectorF(1);
      bool result = null >= v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException9()
    {
      VectorF v1 = new VectorF(1);
      VectorF v2 = new VectorF(2);
      bool result = v1 < v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException10()
    {
      VectorF v1 = new VectorF(1);
      VectorF v2 = new VectorF(2);
      bool result = v1 <= v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException11()
    {
      VectorF v1 = new VectorF(1);
      VectorF v2 = new VectorF(2);
      bool result = v1 > v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException12()
    {
      VectorF v1 = new VectorF(1);
      VectorF v2 = new VectorF(2);
      bool result = v1 >= v2;
    }


    [Test]
    public void CloneTest()
    {
      VectorF v = new VectorF(new List<float>(new float[] { 1, 2, 3, 4, 5 }));
      VectorF clonedVector = v.Clone();
      Assert.AreEqual(v, clonedVector);
    }


    [Test]
    public void ToStringTest()
    {
      VectorF v1 = new VectorF(new List<float>(new float[] { 1, 2, 3, 4, 5 }));
      VectorF v2 = new VectorF(new List<float>(new float[] { 1, 2, 3, 4, 6 }));
      Assert.IsFalse(String.IsNullOrEmpty(v1.ToString()));
      Assert.AreNotEqual(v1.ToString(), v2.ToString());
    }


    [Test]
    public void InternalArray()
    {
      VectorF v1 = new VectorF(new[] { 1.0f, 2.0f, 3.0f, 4.0f });
      VectorF v2 = new VectorF();
      v2.InternalArray = new[] { 1.0f, 2.0f, 3.0f, 4.0f };

      Assert.AreEqual(v1, v2);
      Assert.AreEqual(2.0f, v1.InternalArray[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void InternalArrayException()
    {
      new VectorF().InternalArray = null;
    }


    [Test]
    public void Indexer()
    {
      VectorF v = new VectorF(11);
      Assert.AreEqual(11, v.NumberOfElements);
      for (int i = 0; i < 11; i++)
        Assert.AreEqual(0.0f, v[i]);
      for (int i = 0; i < 11; i++)
        v[i] = i * 2;
      for (int i = 0; i < 11; i++)
        Assert.AreEqual(i * 2, v[i]);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 8;
      Assert.IsFalse(new VectorF(numberOfRows).IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        VectorF v = new VectorF(numberOfRows);
        v[i] = float.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNumericallyNormalized()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-7f;
      VectorF arbitraryVector = new VectorF(new[] { 1.0f, 0.001f, 0.001f, 0.001f, 0.001f });
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      VectorF normalizedVector = new VectorF(new[] { 1.00000001f, 0.00000001f, 0.000000001f, 0.000000001f });
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    public void AreNumericallyEqual()
    {
      Assert.IsTrue(VectorF.AreNumericallyEqual(null, null));
      Assert.IsFalse(VectorF.AreNumericallyEqual(new VectorF(2), new VectorF(1)));

      VectorF nonZero = new VectorF(new[] { 0.001f, 0.001f, 0.0f, 0.001f });
      Assert.IsFalse(VectorF.AreNumericallyEqual(nonZero, new VectorF(4)));

      VectorF zero = new VectorF(new[] { 0.0000001f, 0.0000001f, 0.0f, 0.0000001f });
      Assert.IsTrue(VectorF.AreNumericallyEqual(zero, new VectorF(4)));
    }


    [Test]
    public void AreNumericallyEqualWithEpsilon()
    {
      float epsilon = 0.001f;

      Assert.IsTrue(VectorF.AreNumericallyEqual(null, null, epsilon));
      Assert.IsFalse(VectorF.AreNumericallyEqual(new VectorF(2), new VectorF(1), epsilon));

      VectorF u = new VectorF(new[] { 1.0f, 2.0f, 3.0f, 4.0f });
      VectorF v = new VectorF(new[] { 1.002f, 2.002f, 3.002f, 4.002f });
      VectorF w = new VectorF(new[] { 1.0001f, 2.0001f, 3.0001f, 4.0001f });

      Assert.IsTrue(VectorF.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(VectorF.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(VectorF.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      VectorF u = new VectorF(new[] { 0.0f, 0.0f, 0.0f, 0.0f });
      VectorF v = new VectorF(new[] { 1e-9f, -1e-9f, 1e-9f, 1e-9f });
      VectorF w = new VectorF(new[] { 1e-7f, 1e-7f, -1e-7f, 1e-7f });

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonF = originalEpsilon;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException1()
    {
      VectorF.AreNumericallyEqual(null, new VectorF(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException2()
    {
      VectorF.AreNumericallyEqual(new VectorF(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException3()
    {
      VectorF.AreNumericallyEqual(null, new VectorF(1), 0.1f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException4()
    {
      VectorF.AreNumericallyEqual(new VectorF(1), null, 0.1f);
    }


    [Test]
    public void LengthGetter()
    {
      VectorF v = new VectorF(new float[] { 1, 0, 0, 0 });
      Assert.AreEqual(1.0f, v.Length);
      v = new VectorF(new float[] { 0, 1, 0, 0 });
      Assert.AreEqual(1.0f, v.Length);
      v = new VectorF(new float[] { 0, 0, 1, 0 });
      Assert.AreEqual(1.0f, v.Length);
      v = new VectorF(new float[] { 0, 0, 0, 1 });
      Assert.AreEqual(1.0f, v.Length);

      float x = -1.9f;
      float y = 2.1f;
      float z = 10.0f;
      float w = 1.0f;
      float length = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
      v = new VectorF(new[] { x, y, z, w });
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void LengthSetter()
    {
      VectorF v = new VectorF(new List<float>(new float[] { 1, 2, 3, 4, 5 }));
      v.Length = 0.3f;
      Assert.IsTrue(Numeric.AreEqual(0.3f, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      VectorF v = new VectorF(5);
      v.Length = 0.5f;
    }


    [Test]
    public void LengthSquared()
    {
      VectorF v = new VectorF(new float[] { 1, 0, 0, 0 });
      Assert.AreEqual(1.0f, v.LengthSquared);
      v = new VectorF(new float[] { 0, 1, 0, 0 });
      Assert.AreEqual(1.0f, v.LengthSquared);
      v = new VectorF(new float[] { 0, 0, 1, 0 });
      Assert.AreEqual(1.0f, v.LengthSquared);
      v = new VectorF(new float[] { 0, 0, 0, 1 });
      Assert.AreEqual(1.0f, v.LengthSquared);

      float x = -1.9f;
      float y = 2.1f;
      float z = 10.0f;
      float w = 1.0f;
      float lengthSquared = x * x + y * y + z * z + w * w;
      v = new VectorF(new[] { x, y, z, w });
      Assert.AreEqual(lengthSquared, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      VectorF v = new VectorF(new[] { 3.0f, -1.0f, 23.0f, 0.4f });
      VectorF normalized = v.Normalized;
      Assert.AreEqual(new VectorF(new[] { 3.0f, -1.0f, 23.0f, 0.4f }), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      VectorF v = new VectorF(new[] { 3.0f, -1.0f, 23.0f, 0.4f });
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      VectorF v = new VectorF(11);
      v.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      VectorF v = new VectorF(4);
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new VectorF(new float[] { 1, 2, 3, 4 });
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new VectorF(new float[] { 1, 2, 3, 4 }).Normalized, v);

      v = new VectorF(new float[] { 0, -1, 0, 0 });
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new VectorF(new float[] { 0, -1, 0, 0 }).Normalized, v);
    }


    [Test]
    public void Negate()
    {
      VectorF v = null;
      Assert.IsNull(-v);
      Assert.IsNull(VectorF.Negate(v));

      v = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(new VectorF(new float[] { -1, -2, -3, -4, -5 }), -v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddException1()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = null;
      VectorF v3 = VectorF.Add(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddException2()
    {
      VectorF v1 = null;
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Add(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddException3()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4 });
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Add(v1, v2);
    }


    [Test]
    public void Add()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = new VectorF(new float[] { 6, 7, 8, 9, 10 });
      VectorF v3 = VectorF.Add(v1, v2);
      Assert.AreEqual(new VectorF(new float[] { 7, 9, 11, 13, 15 }), v3);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractException1()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = null;
      VectorF v3 = VectorF.Subtract(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractException2()
    {
      VectorF v1 = null;
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Subtract(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractException3()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4 });
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Subtract(v1, v2);
    }


    [Test]
    public void Subtract()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = new VectorF(new float[] { 6, 7, 8, 9, 10 });
      VectorF v3 = VectorF.Subtract(v1, v2);
      Assert.AreEqual(new VectorF(new float[] { -5, -5, -5, -5, -5 }), v3);
    }


    [Test]
    public void MultiplyScalar()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = VectorF.Multiply(3.5f, v1);
      Assert.AreEqual(new VectorF(new[] { 3.5f, 7f, 10.5f, 14f, 17.5f }), v2);

      v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      v2 = v1 * 3.5f;
      Assert.AreEqual(new VectorF(new[] { 3.5f, 7f, 10.5f, 14f, 17.5f }), v2);

      v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      v2 = 3.5f * v1;
      Assert.AreEqual(new VectorF(new[] { 3.5f, 7f, 10.5f, 14f, 17.5f }), v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyScalarException1()
    {
      VectorF.Multiply(1.0f, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyException1()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = null;
      VectorF v3 = VectorF.Multiply(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyException2()
    {
      VectorF v1 = null;
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Multiply(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyException3()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4 });
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Multiply(v1, v2);
    }


    [Test]
    public void Multiply()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = new VectorF(new float[] { 6, 7, 8, 9, 10 });
      VectorF v3 = VectorF.Multiply(v1, v2);
      Assert.AreEqual(new VectorF(new float[] { 6, 14, 24, 36, 50 }), v3);
    }


    [Test]
    public void DivideByScalar()
    {
      VectorF v1 = new VectorF(new[] { 3.5f, 7f, 10.5f, 14f, 17.5f });
      VectorF v2 = VectorF.Divide(v1, 3.5f);
      Assert.IsTrue(VectorF.AreNumericallyEqual(new VectorF(new float[] { 1, 2, 3, 4, 5 }), v2));

      v1 = new VectorF(new[] { 3.5f, 7f, 10.5f, 14f, 17.5f });
      v2 = v1 / 3.5f;
      Assert.IsTrue(VectorF.AreNumericallyEqual(new VectorF(new float[] { 1, 2, 3, 4, 5 }), v2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideByScalarException1()
    {
      VectorF.Divide(null, 1.0f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideException1()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v2 = null;
      VectorF v3 = VectorF.Divide(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideException2()
    {
      VectorF v1 = null;
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Divide(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DivideException3()
    {
      VectorF v1 = new VectorF(new float[] { 1, 2, 3, 4 });
      VectorF v2 = new VectorF(new float[] { 1, 2, 3, 4, 5 });
      VectorF v3 = VectorF.Divide(v1, v2);
    }


    [Test]
    public void Divide()
    {
      VectorF v1 = new VectorF(new float[] { 6, 14, 24, 36, 50 });
      VectorF v2 = new VectorF(new float[] { 6, 7, 8, 9, 10 });
      VectorF v3 = VectorF.Divide(v1, v2);
      Assert.AreEqual(new VectorF(new float[] { 1, 2, 3, 4, 5 }), v3);
    }


    [Test]
    public void ProjectTo()
    {
      VectorF unitX = new VectorF(new float[] { 1, 0, 0, 0 });
      VectorF unitY = new VectorF(new float[] { 0, 1, 0, 0 });
      VectorF unitZ = new VectorF(new float[] { 0, 0, 1, 0 });

      // Project (1, 1, 1) to axes
      VectorF v = new VectorF(new float[] { 1, 1, 1, 0 });
      VectorF projection = VectorF.ProjectTo(v, unitX);
      Assert.AreEqual(unitX, projection);
      projection = VectorF.ProjectTo(v, unitY);
      Assert.AreEqual(unitY, projection);
      projection = VectorF.ProjectTo(v, unitZ);
      Assert.AreEqual(unitZ, projection);

      // Project axes to (1, 1, 1)
      VectorF expected = new VectorF(new float[] { 1, 1, 1, 0 }) / 3.0f;
      projection = VectorF.ProjectTo(unitX, v);
      Assert.AreEqual(expected, projection);
      projection = VectorF.ProjectTo(unitY, v);
      Assert.AreEqual(expected, projection);
      projection = VectorF.ProjectTo(unitZ, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      VectorF unitX = new VectorF(new float[] { 1, 0, 0, 0 });
      VectorF unitY = new VectorF(new float[] { 0, 1, 0, 0 });
      VectorF unitZ = new VectorF(new float[] { 0, 0, 1, 0 });
      VectorF one = new VectorF(new float[] { 1, 1, 1, 1 });

      // Project (1, 1, 1) to axes
      VectorF projection = new VectorF(new float[] { 1, 1, 1, 0 });
      projection.ProjectTo(unitX);
      Assert.AreEqual(unitX, projection);
      projection.Set(one);
      projection.ProjectTo(unitY);
      Assert.AreEqual(unitY, projection);
      projection.Set(one);
      projection.ProjectTo(unitZ);
      Assert.AreEqual(unitZ, projection);

      // Project axes to (1, 1, 1)
      VectorF expected = new VectorF(new float[] { 1, 1, 1, 0 }) / 3.0f;
      projection.Set(unitX);
      projection.ProjectTo(new VectorF(new float[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
      projection.Set(unitY);
      projection.ProjectTo(new VectorF(new float[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
      projection.Set(unitZ);
      projection.ProjectTo(new VectorF(new float[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException1()
    {
      VectorF v = new VectorF(1);
      v.ProjectTo(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException2()
    {
      VectorF v = new VectorF(1);
      VectorF.ProjectTo(null, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException3()
    {
      VectorF v = new VectorF(1);
      VectorF.ProjectTo(v, null);
    }


    [Test]
    public void AbsoluteStatic()
    {
      VectorF v = new VectorF(new float[] { -1, -2, -3, -4 });
      VectorF absoluteV = VectorF.Absolute(v);
      Assert.AreEqual(1, absoluteV[0]);
      Assert.AreEqual(2, absoluteV[1]);
      Assert.AreEqual(3, absoluteV[2]);
      Assert.AreEqual(4, absoluteV[3]);

      v = new VectorF(new float[] { 1, 2, 3, 4 });
      absoluteV = VectorF.Absolute(v);
      Assert.AreEqual(1, absoluteV[0]);
      Assert.AreEqual(2, absoluteV[1]);
      Assert.AreEqual(3, absoluteV[2]);
      Assert.AreEqual(4, absoluteV[3]);

      Assert.IsNull(VectorF.Absolute(null));
    }


    [Test]
    public void Absolute()
    {
      VectorF v = new VectorF(new float[] { -1, -2, -3, -4 });
      v.Absolute();
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);

      v = new VectorF(new float[] { 1, 2, 3, 4 });
      v.Absolute();
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
    }


    [Test]
    public void Clamp1()
    {
      VectorF clamped = new VectorF(new float[] { -10, 1, 100, 1000 });
      clamped.Clamp(-100, 1000);
      Assert.AreEqual(-10, clamped[0]);
      Assert.AreEqual(1, clamped[1]);
      Assert.AreEqual(100, clamped[2]);
      Assert.AreEqual(1000, clamped[3]);
    }


    [Test]
    public void Clamp2()
    {
      VectorF clamped = new VectorF(new float[] { -10, 1, 100, 1000 });
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped[0]);
      Assert.AreEqual(0, clamped[1]);
      Assert.AreEqual(0, clamped[2]);
      Assert.AreEqual(0, clamped[3]);
    }


    [Test]
    public void ClampStatic1()
    {
      VectorF clamped = new VectorF(new float[] { -10, 1, 100, 1000 });
      clamped = VectorF.Clamp(clamped, -100, 1000);
      Assert.AreEqual(-10, clamped[0]);
      Assert.AreEqual(1, clamped[1]);
      Assert.AreEqual(100, clamped[2]);
      Assert.AreEqual(1000, clamped[3]);

      Assert.IsNull(VectorF.Clamp(null, 0, 1));
    }


    [Test]
    public void ClampStatic2()
    {
      VectorF clamped = new VectorF(new float[] { -10, 1, 100, 1000 });
      clamped = VectorF.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped[0]);
      Assert.AreEqual(0, clamped[1]);
      Assert.AreEqual(0, clamped[2]);
      Assert.AreEqual(0, clamped[3]);
    }


    [Test]
    public void ClampToZero1()
    {
      VectorF zero = new VectorF(4);
      VectorF v = new VectorF(new[] { Numeric.EpsilonF / 2, Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2 });
      v.ClampToZero();
      Assert.AreEqual(zero, v);
      v = new VectorF(new[] { -Numeric.EpsilonF * 2, Numeric.EpsilonF, Numeric.EpsilonF * 2, Numeric.EpsilonF });
      v.ClampToZero();
      Assert.AreNotEqual(zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      VectorF zero = new VectorF(4);
      VectorF v = new VectorF(new[] { 0.1f, 0.1f, -0.1f, 0.09f });
      v.ClampToZero(0.11f);
      Assert.AreEqual(zero, v);
      v = new VectorF(new[] { 0.1f, -0.11f, 0.11f, 0.0f });
      v.ClampToZero(0.1f);
      Assert.AreNotEqual(zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      VectorF zero = new VectorF(4);
      VectorF v = new VectorF(new[] { Numeric.EpsilonF / 2, Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2, -Numeric.EpsilonF / 2 });
      v = VectorF.ClampToZero(v);
      Assert.AreEqual(zero, v);
      v = new VectorF(new[] { -Numeric.EpsilonF * 2, Numeric.EpsilonF, Numeric.EpsilonF * 2, Numeric.EpsilonF });
      v = VectorF.ClampToZero(v);
      Assert.AreNotEqual(zero, v);

      Assert.IsNull(VectorF.ClampToZero(null));
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      VectorF zero = new VectorF(4);
      VectorF v = new VectorF(new[] { 0.1f, 0.1f, -0.1f, 0.09f });
      v = VectorF.ClampToZero(v, 0.11f);
      Assert.AreEqual(zero, v);
      v = new VectorF(new[] { 0.1f, -0.11f, 0.11f, 0.0f });
      v = VectorF.ClampToZero(v, 0.1f);
      Assert.AreNotEqual(zero, v);

      Assert.IsNull(VectorF.ClampToZero(null, 0));
    }


    [Test]
    public void Dot()
    {
      VectorF unitX = new VectorF(new float[] { 1, 0, 0, 0 });
      VectorF unitY = new VectorF(new float[] { 0, 1, 0, 0 });
      VectorF unitZ = new VectorF(new float[] { 0, 0, 1, 0 });
      VectorF unitW = new VectorF(new float[] { 0, 0, 0, 1 });

      // 0°
      Assert.AreEqual(1.0f, VectorF.Dot(unitX, unitX));
      Assert.AreEqual(1.0f, VectorF.Dot(unitY, unitY));
      Assert.AreEqual(1.0f, VectorF.Dot(unitZ, unitZ));
      Assert.AreEqual(1.0f, VectorF.Dot(unitW, unitW));

      // 180°
      Assert.AreEqual(-1.0f, VectorF.Dot(unitX, -unitX));
      Assert.AreEqual(-1.0f, VectorF.Dot(unitY, -unitY));
      Assert.AreEqual(-1.0f, VectorF.Dot(unitZ, -unitZ));
      Assert.AreEqual(-1.0f, VectorF.Dot(unitW, -unitW));

      // 90°
      Assert.AreEqual(0.0f, VectorF.Dot(unitX, unitY));
      Assert.AreEqual(0.0f, VectorF.Dot(unitY, unitZ));
      Assert.AreEqual(0.0f, VectorF.Dot(unitZ, unitW));
      Assert.AreEqual(0.0f, VectorF.Dot(unitW, unitX));

      // 45°
      float angle = (float)Math.Acos(VectorF.Dot(new VectorF(new float[] { 1, 1, 0, 0 }).Normalized, unitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(VectorF.Dot(new VectorF(new float[] { 0, 1, 1, 0 }).Normalized, unitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(VectorF.Dot(new VectorF(new float[] { 1, 0, 1, 0 }).Normalized, unitZ));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
      angle = (float)Math.Acos(VectorF.Dot(new VectorF(new float[] { 1, 0, 0, 1 }).Normalized, unitW));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45), angle));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DotException1()
    {
      VectorF.Dot(null, new VectorF(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DotException2()
    {
      VectorF.Dot(new VectorF(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DotException3()
    {
      VectorF.Dot(new VectorF(2), new VectorF(1));
    }


    [Test]
    public void Min()
    {
      VectorF v1 = new VectorF(new[] { 1.0f, 2.0f, 2.5f, 4.0f });
      VectorF v2 = new VectorF(new[] { -1.0f, 2.0f, 3.0f, -2.0f });
      VectorF min = VectorF.Min(v1, v2);
      Assert.AreEqual(new VectorF(new[] { -1.0f, 2.0f, 2.5f, -2.0f }), min);
    }


    [Test]
    public void Max()
    {
      VectorF v1 = new VectorF(new[] { 1.0f, 2.0f, 3.0f, 4.0f });
      VectorF v2 = new VectorF(new[] { -1.0f, 2.1f, 3.0f, 8.0f });
      VectorF max = VectorF.Max(v1, v2);
      Assert.AreEqual(new VectorF(new[] { 1.0f, 2.1f, 3.0f, 8.0f }), max);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MinException1()
    {
      VectorF.Min(null, new VectorF(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MinException2()
    {
      VectorF.Min(new VectorF(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MinException3()
    {
      VectorF.Min(new VectorF(2), new VectorF(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MaxException1()
    {
      VectorF.Max(null, new VectorF(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MaxException2()
    {
      VectorF.Max(new VectorF(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MaxException3()
    {
      VectorF.Max(new VectorF(2), new VectorF(1));
    }


    [Test]
    public void ExplicitCastToVector2F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f });
      Vector2F u = (Vector2F)v;

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
    }


    [Test]
    public void ToVector2F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f });
      Vector2F u = v.ToVector2F();

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector2FException1()
    {
      VectorF v = null;
      Vector2F u = (Vector2F)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector2FException2()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f });
      Vector2F u = (Vector2F)v;
    }


    [Test]
    public void ExplicitCastToVector3F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f });
      Vector3F u = (Vector3F)v;

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
      Assert.AreEqual(3.3f, u[2]);
    }


    [Test]
    public void ToVector3F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f });
      Vector3F u = v.ToVector3F();

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
      Assert.AreEqual(3.3f, u[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector3FException1()
    {
      VectorF v = null;
      Vector3F u = (Vector3F)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector3FException2()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f });
      Vector3F u = (Vector3F)v;
    }


    [Test]
    public void ExplicitCastToVector4F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f, 4.4f });
      Vector4F u = (Vector4F)v;

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
      Assert.AreEqual(3.3f, u[2]);
      Assert.AreEqual(4.4f, u[3]);
    }


    [Test]
    public void ToVector4F()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f, 4.4f });
      Vector4F u = v.ToVector4F();

      Assert.AreEqual(1.1f, u[0]);
      Assert.AreEqual(2.2f, u[1]);
      Assert.AreEqual(3.3f, u[2]);
      Assert.AreEqual(4.4f, u[3]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector4FException1()
    {
      VectorF v = null;
      Vector4F u = (Vector4F)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector4FException2()
    {
      VectorF v = new VectorF(new[] { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f });
      Vector4F u = (Vector4F)v;
    }


    [Test]
    public void ImplicitCastToVectorD()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorD vectorD = new VectorF(new[] { x, y, z, w });
      Assert.AreEqual(4, vectorD.NumberOfElements);
      Assert.AreEqual((double)x, vectorD[0]);
      Assert.AreEqual((double)y, vectorD[1]);
      Assert.AreEqual((double)z, vectorD[2]);
      Assert.AreEqual((double)w, vectorD[3]);

      VectorF vectorF = null;
      vectorD = vectorF;
      Assert.IsNull(vectorD);
    }


    [Test]
    public void ToVectorD()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorD vectorD = new VectorF(new[] { x, y, z, w }).ToVectorD();
      Assert.AreEqual(4, vectorD.NumberOfElements);
      Assert.AreEqual((double)x, vectorD[0]);
      Assert.AreEqual((double)y, vectorD[1]);
      Assert.AreEqual((double)z, vectorD[2]);
      Assert.AreEqual((double)w, vectorD[3]);
    }


    [Test]
    public void ExplicitArrayCast()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      float[] values = (float[])v;
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);

      v = null;
      values = (float[])v;
      Assert.IsNull(values);
    }


    [Test]
    public void ToArray()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      float[] values = v.ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      List<float> values = (List<float>)v;
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);

      v = null;
      values = (List<float>)v;
      Assert.IsNull(values);
    }


    [Test]
    public void ToList()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      List<float> values = v.ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);
    }


    [Test]
    public void ExplicitCastToMatrixF()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      MatrixF matrix = (MatrixF)v;
      Assert.AreEqual(4, matrix.NumberOfRows);
      Assert.AreEqual(1, matrix.NumberOfColumns);
      Assert.AreEqual(x, matrix[0, 0]);
      Assert.AreEqual(y, matrix[1, 0]);
      Assert.AreEqual(z, matrix[2, 0]);
      Assert.AreEqual(w, matrix[3, 0]);

      v = null;
      matrix = (MatrixF)v;
      Assert.IsNull(matrix);
    }


    [Test]
    public void ToMatrixF()
    {
      float x = 23.4f;
      float y = -11.0f;
      float z = 0.0f;
      float w = 0.3f;
      VectorF v = new VectorF(new[] { x, y, z, w });
      MatrixF matrix = v.ToMatrixF();
      Assert.AreEqual(4, matrix.NumberOfRows);
      Assert.AreEqual(1, matrix.NumberOfColumns);
      Assert.AreEqual(x, matrix[0, 0]);
      Assert.AreEqual(y, matrix[1, 0]);
      Assert.AreEqual(z, matrix[2, 0]);
      Assert.AreEqual(w, matrix[3, 0]);
    }


    [Test]
    public void GetLargestElement()
    {
      VectorF v = new VectorF(new float[] { -1, -2, -3, -4 });
      Assert.AreEqual(-1, v.LargestElement);

      v = new VectorF(new float[] { 10, 20, -30, -40 });
      Assert.AreEqual(20, v.LargestElement);

      v = new VectorF(new float[] { -1, 20, 30, 20 });
      Assert.AreEqual(30, v.LargestElement);

      v = new VectorF(new float[] { 10, 20, 30, 40 });
      Assert.AreEqual(40, v.LargestElement);
    }


    [Test]
    public void GetIndexOfLargestElement()
    {
      VectorF v = new VectorF(new float[] { -1, -2, -3, -4 });
      Assert.AreEqual(0, v.IndexOfLargestElement);

      v = new VectorF(new float[] { 10, 20, -30, -40 });
      Assert.AreEqual(1, v.IndexOfLargestElement);

      v = new VectorF(new float[] { -1, 20, 30, 20 });
      Assert.AreEqual(2, v.IndexOfLargestElement);

      v = new VectorF(new float[] { 10, 20, 30, 40 });
      Assert.AreEqual(3, v.IndexOfLargestElement);
    }


    [Test]
    public void GetSmallestElement()
    {
      VectorF v = new VectorF(new float[] { -4, -2, -3, -1 });
      Assert.AreEqual(-4, v.SmallestElement);

      v = new VectorF(new float[] { 10, 0, 3, 4 });
      Assert.AreEqual(0, v.SmallestElement);

      v = new VectorF(new float[] { -1, 20, -3, 0 });
      Assert.AreEqual(-3, v.SmallestElement);

      v = new VectorF(new float[] { 40, 30, 20, 10 });
      Assert.AreEqual(10, v.SmallestElement);
    }


    [Test]
    public void GetIndexOfSmallestElement()
    {
      VectorF v = new VectorF(new float[] { -4, -2, -3, -1 });
      Assert.AreEqual(0, v.IndexOfSmallestElement);

      v = new VectorF(new float[] { 10, 0, 3, 4 });
      Assert.AreEqual(1, v.IndexOfSmallestElement);

      v = new VectorF(new float[] { -1, 20, -3, 0 });
      Assert.AreEqual(2, v.IndexOfSmallestElement);

      v = new VectorF(new float[] { 40, 30, 20, 10 });
      Assert.AreEqual(3, v.IndexOfSmallestElement);
    }


    [Test]
    public void SerializationXml()
    {
      VectorF v1 = new VectorF(new[] { 0.1f, -0.2f, 3, 40, 50 });
      VectorF v2;
      string fileName = "SerializationVectorF.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(VectorF));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(VectorF));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (VectorF)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);

      // We dont have schema.
      Assert.AreEqual(null, new VectorF().GetSchema());
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      VectorF v1 = new VectorF(new[] { 0.1f, -0.2f, 3, 40, 50 });
      VectorF v2;
      string fileName = "SerializationVectorF.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (VectorF)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      VectorF v1 = new VectorF(new[] { 0.1f, -0.2f, 3, 40, 50 });
      VectorF v2;

      string fileName = "SerializationVectorF_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(VectorF));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (VectorF)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      VectorF v1 = new VectorF(new[] { 0.1f, -0.2f, 3, 40, 50 });
      VectorF v2;

      string fileName = "SerializationVectorF.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(VectorF));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (VectorF)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    /* Not available in PCL build.
    [Test]
    [ExpectedException(typeof(SerializationException))]
    public void SerializationConstructorException()
    {
      new VectorFSerializationTest(); // Will throw exception in serialization ctor.
    }


    private class VectorFSerializationTest : VectorF
    {
      public VectorFSerializationTest() : base(null, new StreamingContext())
      {
      }
    }
    */
  }
}
 