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
  public class VectorDTest
  {
    [Test]
    public void Constructors()
    {
      VectorD v = new VectorD(4);
      Assert.AreEqual(4, v.NumberOfElements);
      for (int i = 0; i < 4; i++)
        Assert.AreEqual(0.0, v[i]);

      v = new VectorD(21, 0.123);
      Assert.AreEqual(21, v.NumberOfElements);
      for (int i = 0; i < 21; i++)
        Assert.AreEqual(0.123, v[i]);

      v = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v = new VectorD(new List<double>(new double[] { 1, 2, 3, 4, 5 }));
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
      VectorD v = new VectorD(0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ConstructorException2()
    {
      VectorD v = new VectorD(-1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithArrayShouldThrowArgumentNullException()
    {
      VectorD v = new VectorD(1);
      v.Set((double[])null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetWithIListShouldThrowArgumentNullException()
    {
      var v = new VectorD(1);
      v.Set((IList<double>)null);
    }


    [Test]
    public void Set()
    {
      VectorD v = new VectorD(5);
      v.Set(0.123);
      Assert.AreEqual(5, v.NumberOfElements);
      for (int i = 0; i < 5; i++)
        Assert.AreEqual(0.123, v[i]);

      v.Set(new VectorD(new double[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v.Set(new double[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(5, v.NumberOfElements);
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v.Set(new List<double>(new double[] { 1, 2, 3, 4, 5 }));
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
      VectorD v = new VectorD(5);
      v.SetSubvector(0, new VectorD(new double[] { 1, 2, 3, 4, 5 }));
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(5, v[4]);

      v = new VectorD(5);
      v.SetSubvector(0, new VectorD(new double[] { 1, 2, 3, 4 }));
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
      Assert.AreEqual(0, v[4]);

      v = new VectorD(5);
      v.SetSubvector(2, new VectorD(new double[] { 1, 2, 3 }));
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
      VectorD v = new VectorD(5);
      v.SetSubvector(1, new VectorD(new double[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubvectorException2()
    {
      VectorD v = new VectorD(5);
      v.SetSubvector(-1, new VectorD(new double[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void SetSubvectorException3()
    {
      VectorD v = new VectorD(5);
      v.SetSubvector(1, new VectorD(new double[] { 1, 2, 3, 4, 5 }));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetSubvectorException4()
    {
      VectorD v = new VectorD(5);
      v.SetSubvector(1, null);
    }


    [Test]
    public void GetSubvector()
    {
      VectorD v = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD subvector = v.GetSubvector(0, 5);
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
      VectorD v = new VectorD(4);
      v.GetSubvector(-1, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException2()
    {
      VectorD v = new VectorD(4);
      v.GetSubvector(4, 1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException3()
    {
      VectorD v = new VectorD(4);
      v.GetSubvector(0, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException4()
    {
      VectorD v = new VectorD(4);
      v.GetSubvector(2, -1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetSubvectorException5()
    {
      VectorD v = new VectorD(4);
      v.GetSubvector(0, 5);
    }


    [Test]
    public void HashCodeTest()
    {
      VectorD v = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD one = new VectorD(5, 1.0);
      Assert.AreNotEqual(one.GetHashCode(), v.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      VectorD v0 = new VectorD(new[] { 678.0, 234.8, -123.987, 4.0 });
      VectorD v1 = new VectorD(new[] { 678.0, 234.8, -123.987, 4.0 });
      VectorD v2 = new VectorD(new[] { 67.0, 234.8, -123.987, 4.0 });
      VectorD v3 = new VectorD(new[] { 678.0, 24.8, -123.987, 4.0 });
      VectorD v4 = new VectorD(new[] { 678.0, 234.8, 123.987, 4.0 });
      VectorD v5 = new VectorD(new[] { 678.0, 234.8, 123.987, 4.1 });
      VectorD v6 = new VectorD(new[] { 678.0, 234.8, -123.987 });
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
      VectorD a = new VectorD(new[] { 1.0, 2.0, 3.0, 4.0 });
      VectorD b = new VectorD(new[] { 1.0, 2.0, 3.0, 4.0 });
      VectorD c = new VectorD(new[] { -1.0, 2.0, 3.0, 4.0 });
      VectorD d = new VectorD(new[] { 1.0, -2.0, 3.0, 4.0 });
      VectorD e = new VectorD(new[] { 1.0, 2.0, -3.0, 4.0 });
      VectorD f = new VectorD(new[] { 1.0, 2.0, 3.0, -4.0 });

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
      VectorD a = new VectorD(new[] { 1.0, 1.0, 1.0, 1.0 });
      VectorD b = new VectorD(new[] { 0.5, 0.5, 0.5, 0.5 });
      VectorD c = new VectorD(new[] { 1.0, 0.5, 0.5, 0.5 });
      VectorD d = new VectorD(new[] { 0.5, 1.0, 0.5, 0.5 });
      VectorD e = new VectorD(new[] { 0.5, 0.5, 1.0, 0.5 });
      VectorD f = new VectorD(new[] { 0.5, 0.5, 0.5, 1.0 });

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
      VectorD v = new VectorD(1);
      bool result = v < null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException2()
    {
      VectorD v = new VectorD(1);
      bool result = v <= null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException3()
    {
      VectorD v = new VectorD(1);
      bool result = v > null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException4()
    {
      VectorD v = new VectorD(1);
      bool result = v >= null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException5()
    {
      VectorD v = new VectorD(1);
      bool result = null < v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException6()
    {
      VectorD v = new VectorD(1);
      bool result = null <= v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException7()
    {
      VectorD v = new VectorD(1);
      bool result = null > v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ComparisonException8()
    {
      VectorD v = new VectorD(1);
      bool result = null >= v;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException9()
    {
      VectorD v1 = new VectorD(1);
      VectorD v2 = new VectorD(2);
      bool result = v1 < v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException10()
    {
      VectorD v1 = new VectorD(1);
      VectorD v2 = new VectorD(2);
      bool result = v1 <= v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException11()
    {
      VectorD v1 = new VectorD(1);
      VectorD v2 = new VectorD(2);
      bool result = v1 > v2;
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void ComparisonException12()
    {
      VectorD v1 = new VectorD(1);
      VectorD v2 = new VectorD(2);
      bool result = v1 >= v2;
    }


    [Test]
    public void CloneTest()
    {
      VectorD v = new VectorD(new List<double>(new double[] { 1, 2, 3, 4, 5 }));
      VectorD clonedVector = v.Clone();
      Assert.AreEqual(v, clonedVector);
    }


    [Test]
    public void ToStringTest()
    {
      VectorD v1 = new VectorD(new List<double>(new double[] { 1, 2, 3, 4, 5 }));
      VectorD v2 = new VectorD(new List<double>(new double[] { 1, 2, 3, 4, 6 }));
      Assert.IsFalse(String.IsNullOrEmpty(v1.ToString()));
      Assert.AreNotEqual(v1.ToString(), v2.ToString());
    }


    [Test]
    public void InternalArray()
    {
      VectorD v1 = new VectorD(new[] { 1.0, 2.0, 3.0, 4.0 });
      VectorD v2 = new VectorD();
      v2.InternalArray = new[] { 1.0, 2.0, 3.0, 4.0 };

      Assert.AreEqual(v1, v2);
      Assert.AreEqual(2.0, v1.InternalArray[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void InternalArrayException()
    {
      new VectorD().InternalArray = null;
    }


    [Test]
    public void Indexer()
    {
      VectorD v = new VectorD(11);
      Assert.AreEqual(11, v.NumberOfElements);
      for (int i = 0; i < 11; i++)
        Assert.AreEqual(0.0, v[i]);
      for (int i = 0; i < 11; i++)
        v[i] = i * 2;
      for (int i = 0; i < 11; i++)
        Assert.AreEqual(i * 2, v[i]);
    }


    [Test]
    public void IsNaN()
    {
      const int numberOfRows = 8;
      Assert.IsFalse(new VectorD(numberOfRows).IsNaN);

      for (int i = 0; i < numberOfRows; i++)
      {
        VectorD v = new VectorD(numberOfRows);
        v[i] = double.NaN;
        Assert.IsTrue(v.IsNaN);
      }
    }


    [Test]
    public void IsNumericallyNormalized()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-7;
      VectorD arbitraryVector = new VectorD(new[] { 1.0, 0.001, 0.001, 0.001, 0.001 });
      Assert.IsFalse(arbitraryVector.IsNumericallyNormalized);

      VectorD normalizedVector = new VectorD(new[] { 1.00000001, 0.00000001, 0.000000001, 0.000000001 });
      Assert.IsTrue(normalizedVector.IsNumericallyNormalized);
      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    public void AreNumericallyEqual()
    {
      Assert.IsTrue(VectorD.AreNumericallyEqual(null, null));
      Assert.IsFalse(VectorD.AreNumericallyEqual(new VectorD(2), new VectorD(1)));

      VectorD nonZero = new VectorD(new[] { 0.001, 0.001, 0.0, 0.001 });
      Assert.IsFalse(VectorD.AreNumericallyEqual(nonZero, new VectorD(4)));

      VectorD zero = new VectorD(new[] { 0.0000000000001, 0.0000000000001, 0.0, 0.0000000000001 });
      Assert.IsTrue(VectorD.AreNumericallyEqual(zero, new VectorD(4)));
    }


    [Test]
    public void AreNumericallyEqualWithEpsilon()
    {
      double epsilon = 0.001;

      Assert.IsTrue(VectorD.AreNumericallyEqual(null, null, epsilon));
      Assert.IsFalse(VectorD.AreNumericallyEqual(new VectorD(2), new VectorD(1), epsilon));

      VectorD u = new VectorD(new[] { 1.0, 2.0, 3.0, 4.0 });
      VectorD v = new VectorD(new[] { 1.002, 2.002, 3.002, 4.002 });
      VectorD w = new VectorD(new[] { 1.0001, 2.0001, 3.0001, 4.0001 });

      Assert.IsTrue(VectorD.AreNumericallyEqual(u, u, epsilon));
      Assert.IsFalse(VectorD.AreNumericallyEqual(u, v, epsilon));
      Assert.IsTrue(VectorD.AreNumericallyEqual(u, w, epsilon));
    }


    [Test]
    public void IsNumericallyZero()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      VectorD u = new VectorD(new[] { 0.0, 0.0, 0.0, 0.0 });
      VectorD v = new VectorD(new[] { 1e-9, -1e-9, 1e-9, 1e-9 });
      VectorD w = new VectorD(new[] { 1e-7, 1e-7, -1e-7, 1e-7 });

      Assert.IsTrue(u.IsNumericallyZero);
      Assert.IsTrue(v.IsNumericallyZero);
      Assert.IsFalse(w.IsNumericallyZero);

      Numeric.EpsilonD = originalEpsilon;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException1()
    {
      VectorD.AreNumericallyEqual(null, new VectorD(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException2()
    {
      VectorD.AreNumericallyEqual(new VectorD(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException3()
    {
      VectorD.AreNumericallyEqual(null, new VectorD(1), 0.1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AreNumericallyEqualException4()
    {
      VectorD.AreNumericallyEqual(new VectorD(1), null, 0.1);
    }


    [Test]
    public void LengthGetter()
    {
      VectorD v = new VectorD(new double[] { 1, 0, 0, 0 });
      Assert.AreEqual(1.0, v.Length);
      v = new VectorD(new double[] { 0, 1, 0, 0 });
      Assert.AreEqual(1.0, v.Length);
      v = new VectorD(new double[] { 0, 0, 1, 0 });
      Assert.AreEqual(1.0, v.Length);
      v = new VectorD(new double[] { 0, 0, 0, 1 });
      Assert.AreEqual(1.0, v.Length);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double w = 1.0;
      double length = (double)Math.Sqrt(x * x + y * y + z * z + w * w);
      v = new VectorD(new[] { x, y, z, w });
      Assert.AreEqual(length, v.Length);
    }


    [Test]
    public void LengthSetter()
    {
      VectorD v = new VectorD(new List<double>(new double[] { 1, 2, 3, 4, 5 }));
      v.Length = 0.3;
      Assert.IsTrue(Numeric.AreEqual(0.3, v.Length));
    }


    [Test]
    [ExpectedException(typeof(MathematicsException))]
    public void LengthException()
    {
      VectorD v = new VectorD(5);
      v.Length = 0.5;
    }


    [Test]
    public void LengthSquared()
    {
      VectorD v = new VectorD(new double[] { 1, 0, 0, 0 });
      Assert.AreEqual(1.0, v.LengthSquared);
      v = new VectorD(new double[] { 0, 1, 0, 0 });
      Assert.AreEqual(1.0, v.LengthSquared);
      v = new VectorD(new double[] { 0, 0, 1, 0 });
      Assert.AreEqual(1.0, v.LengthSquared);
      v = new VectorD(new double[] { 0, 0, 0, 1 });
      Assert.AreEqual(1.0, v.LengthSquared);

      double x = -1.9;
      double y = 2.1;
      double z = 10.0;
      double w = 1.0;
      double lengthSquared = x * x + y * y + z * z + w * w;
      v = new VectorD(new[] { x, y, z, w });
      Assert.AreEqual(lengthSquared, v.LengthSquared);
    }


    [Test]
    public void Normalized()
    {
      VectorD v = new VectorD(new[] { 3.0, -1.0, 23.0, 0.4 });
      VectorD normalized = v.Normalized;
      Assert.AreEqual(new VectorD(new[] { 3.0, -1.0, 23.0, 0.4 }), v);
      Assert.IsFalse(v.IsNumericallyNormalized);
      Assert.IsTrue(normalized.IsNumericallyNormalized);
    }


    [Test]
    public void Normalize()
    {
      VectorD v = new VectorD(new[] { 3.0, -1.0, 23.0, 0.4 });
      v.Normalize();
      Assert.IsTrue(v.IsNumericallyNormalized);
    }


    [Test]
    [ExpectedException(typeof(DivideByZeroException))]
    public void NormalizeException()
    {
      VectorD v = new VectorD(11);
      v.Normalize();
    }


    [Test]
    public void TryNormalize()
    {
      VectorD v = new VectorD(4);
      bool normalized = v.TryNormalize();
      Assert.IsFalse(normalized);

      v = new VectorD(new double[] { 1, 2, 3, 4 });
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new VectorD(new double[] { 1, 2, 3, 4 }).Normalized, v);

      v = new VectorD(new double[] { 0, -1, 0, 0 });
      normalized = v.TryNormalize();
      Assert.IsTrue(normalized);
      Assert.AreEqual(new VectorD(new double[] { 0, -1, 0, 0 }).Normalized, v);
    }


    [Test]
    public void Negate()
    {
      VectorD v = null;
      Assert.IsNull(-v);
      Assert.IsNull(VectorD.Negate(v));

      v = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      Assert.AreEqual(new VectorD(new double[] { -1, -2, -3, -4, -5 }), -v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddException1()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = null;
      VectorD v3 = VectorD.Add(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddException2()
    {
      VectorD v1 = null;
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Add(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void AddException3()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4 });
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Add(v1, v2);
    }


    [Test]
    public void Add()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = new VectorD(new double[] { 6, 7, 8, 9, 10 });
      VectorD v3 = VectorD.Add(v1, v2);
      Assert.AreEqual(new VectorD(new double[] { 7, 9, 11, 13, 15 }), v3);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractException1()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = null;
      VectorD v3 = VectorD.Subtract(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SubtractException2()
    {
      VectorD v1 = null;
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Subtract(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SubtractException3()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4 });
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Subtract(v1, v2);
    }


    [Test]
    public void Subtract()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = new VectorD(new double[] { 6, 7, 8, 9, 10 });
      VectorD v3 = VectorD.Subtract(v1, v2);
      Assert.AreEqual(new VectorD(new double[] { -5, -5, -5, -5, -5 }), v3);
    }


    [Test]
    public void MultiplyScalar()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = VectorD.Multiply(3.5, v1);
      Assert.AreEqual(new VectorD(new[] { 3.5, 7, 10.5, 14, 17.5 }), v2);

      v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      v2 = v1 * 3.5;
      Assert.AreEqual(new VectorD(new[] { 3.5, 7, 10.5, 14, 17.5 }), v2);

      v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      v2 = 3.5 * v1;
      Assert.AreEqual(new VectorD(new[] { 3.5, 7, 10.5, 14, 17.5 }), v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyScalarException1()
    {
      VectorD.Multiply(1.0, null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyException1()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = null;
      VectorD v3 = VectorD.Multiply(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MultiplyException2()
    {
      VectorD v1 = null;
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Multiply(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MultiplyException3()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4 });
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Multiply(v1, v2);
    }


    [Test]
    public void Multiply()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = new VectorD(new double[] { 6, 7, 8, 9, 10 });
      VectorD v3 = VectorD.Multiply(v1, v2);
      Assert.AreEqual(new VectorD(new double[] { 6, 14, 24, 36, 50 }), v3);
    }


    [Test]
    public void DivideByScalar()
    {
      VectorD v1 = new VectorD(new[] { 3.5, 7, 10.5, 14, 17.5 });
      VectorD v2 = VectorD.Divide(v1, 3.5);
      Assert.IsTrue(VectorD.AreNumericallyEqual(new VectorD(new double[] { 1, 2, 3, 4, 5 }), v2));

      v1 = new VectorD(new[] { 3.5, 7, 10.5, 14, 17.5 });
      v2 = v1 / 3.5;
      Assert.IsTrue(VectorD.AreNumericallyEqual(new VectorD(new double[] { 1, 2, 3, 4, 5 }), v2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideByScalarException1()
    {
      VectorD.Divide(null, 1.0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideException1()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v2 = null;
      VectorD v3 = VectorD.Divide(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DivideException2()
    {
      VectorD v1 = null;
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Divide(v1, v2);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DivideException3()
    {
      VectorD v1 = new VectorD(new double[] { 1, 2, 3, 4 });
      VectorD v2 = new VectorD(new double[] { 1, 2, 3, 4, 5 });
      VectorD v3 = VectorD.Divide(v1, v2);
    }


    [Test]
    public void Divide()
    {
      VectorD v1 = new VectorD(new double[] { 6, 14, 24, 36, 50 });
      VectorD v2 = new VectorD(new double[] { 6, 7, 8, 9, 10 });
      VectorD v3 = VectorD.Divide(v1, v2);
      Assert.AreEqual(new VectorD(new double[] { 1, 2, 3, 4, 5 }), v3);
    }


    [Test]
    public void ProjectTo()
    {
      VectorD unitX = new VectorD(new double[] { 1, 0, 0, 0 });
      VectorD unitY = new VectorD(new double[] { 0, 1, 0, 0 });
      VectorD unitZ = new VectorD(new double[] { 0, 0, 1, 0 });

      // Project (1, 1, 1) to axes
      VectorD v = new VectorD(new double[] { 1, 1, 1, 0 });
      VectorD projection = VectorD.ProjectTo(v, unitX);
      Assert.AreEqual(unitX, projection);
      projection = VectorD.ProjectTo(v, unitY);
      Assert.AreEqual(unitY, projection);
      projection = VectorD.ProjectTo(v, unitZ);
      Assert.AreEqual(unitZ, projection);

      // Project axes to (1, 1, 1)
      VectorD expected = new VectorD(new double[] { 1, 1, 1, 0 }) / 3.0;
      projection = VectorD.ProjectTo(unitX, v);
      Assert.AreEqual(expected, projection);
      projection = VectorD.ProjectTo(unitY, v);
      Assert.AreEqual(expected, projection);
      projection = VectorD.ProjectTo(unitZ, v);
      Assert.AreEqual(expected, projection);
    }


    [Test]
    public void ProjectTo2()
    {
      VectorD unitX = new VectorD(new double[] { 1, 0, 0, 0 });
      VectorD unitY = new VectorD(new double[] { 0, 1, 0, 0 });
      VectorD unitZ = new VectorD(new double[] { 0, 0, 1, 0 });
      VectorD one = new VectorD(new double[] { 1, 1, 1, 1 });

      // Project (1, 1, 1) to axes
      VectorD projection = new VectorD(new double[] { 1, 1, 1, 0 });
      projection.ProjectTo(unitX);
      Assert.AreEqual(unitX, projection);
      projection.Set(one);
      projection.ProjectTo(unitY);
      Assert.AreEqual(unitY, projection);
      projection.Set(one);
      projection.ProjectTo(unitZ);
      Assert.AreEqual(unitZ, projection);

      // Project axes to (1, 1, 1)
      VectorD expected = new VectorD(new double[] { 1, 1, 1, 0 }) / 3.0;
      projection.Set(unitX);
      projection.ProjectTo(new VectorD(new double[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
      projection.Set(unitY);
      projection.ProjectTo(new VectorD(new double[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
      projection.Set(unitZ);
      projection.ProjectTo(new VectorD(new double[] { 1, 1, 1, 0 }));
      Assert.AreEqual(expected, projection);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException1()
    {
      VectorD v = new VectorD(1);
      v.ProjectTo(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException2()
    {
      VectorD v = new VectorD(1);
      VectorD.ProjectTo(null, v);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProjectToException3()
    {
      VectorD v = new VectorD(1);
      VectorD.ProjectTo(v, null);
    }


    [Test]
    public void AbsoluteStatic()
    {
      VectorD v = new VectorD(new double[] { -1, -2, -3, -4 });
      VectorD absoluteV = VectorD.Absolute(v);
      Assert.AreEqual(1, absoluteV[0]);
      Assert.AreEqual(2, absoluteV[1]);
      Assert.AreEqual(3, absoluteV[2]);
      Assert.AreEqual(4, absoluteV[3]);

      v = new VectorD(new double[] { 1, 2, 3, 4 });
      absoluteV = VectorD.Absolute(v);
      Assert.AreEqual(1, absoluteV[0]);
      Assert.AreEqual(2, absoluteV[1]);
      Assert.AreEqual(3, absoluteV[2]);
      Assert.AreEqual(4, absoluteV[3]);

      Assert.IsNull(VectorD.Absolute(null));
    }


    [Test]
    public void Absolute()
    {
      VectorD v = new VectorD(new double[] { -1, -2, -3, -4 });
      v.Absolute();
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);

      v = new VectorD(new double[] { 1, 2, 3, 4 });
      v.Absolute();
      Assert.AreEqual(1, v[0]);
      Assert.AreEqual(2, v[1]);
      Assert.AreEqual(3, v[2]);
      Assert.AreEqual(4, v[3]);
    }


    [Test]
    public void Clamp1()
    {
      VectorD clamped = new VectorD(new double[] { -10, 1, 100, 1000 });
      clamped.Clamp(-100, 1000);
      Assert.AreEqual(-10, clamped[0]);
      Assert.AreEqual(1, clamped[1]);
      Assert.AreEqual(100, clamped[2]);
      Assert.AreEqual(1000, clamped[3]);
    }


    [Test]
    public void Clamp2()
    {
      VectorD clamped = new VectorD(new double[] { -10, 1, 100, 1000 });
      clamped.Clamp(-1, 0);
      Assert.AreEqual(-1, clamped[0]);
      Assert.AreEqual(0, clamped[1]);
      Assert.AreEqual(0, clamped[2]);
      Assert.AreEqual(0, clamped[3]);
    }


    [Test]
    public void ClampStatic1()
    {
      VectorD clamped = new VectorD(new double[] { -10, 1, 100, 1000 });
      clamped = VectorD.Clamp(clamped, -100, 1000);
      Assert.AreEqual(-10, clamped[0]);
      Assert.AreEqual(1, clamped[1]);
      Assert.AreEqual(100, clamped[2]);
      Assert.AreEqual(1000, clamped[3]);

      Assert.IsNull(VectorD.Clamp(null, 0, 1));
    }


    [Test]
    public void ClampStatic2()
    {
      VectorD clamped = new VectorD(new double[] { -10, 1, 100, 1000 });
      clamped = VectorD.Clamp(clamped, -1, 0);
      Assert.AreEqual(-1, clamped[0]);
      Assert.AreEqual(0, clamped[1]);
      Assert.AreEqual(0, clamped[2]);
      Assert.AreEqual(0, clamped[3]);
    }


    [Test]
    public void ClampToZero1()
    {
      VectorD zero = new VectorD(4);
      VectorD v = new VectorD(new[] { Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2 });
      v.ClampToZero();
      Assert.AreEqual(zero, v);
      v = new VectorD(new[] { -Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2, Numeric.EpsilonD });
      v.ClampToZero();
      Assert.AreNotEqual(zero, v);
    }


    [Test]
    public void ClampToZero2()
    {
      VectorD zero = new VectorD(4);
      VectorD v = new VectorD(new[] { 0.1, 0.1, -0.1, 0.09 });
      v.ClampToZero(0.11);
      Assert.AreEqual(zero, v);
      v = new VectorD(new[] { 0.1, -0.11, 0.11, 0.0 });
      v.ClampToZero(0.1);
      Assert.AreNotEqual(zero, v);
    }


    [Test]
    public void ClampToZeroStatic1()
    {
      VectorD zero = new VectorD(4);
      VectorD v = new VectorD(new[] { Numeric.EpsilonD / 2, Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2, -Numeric.EpsilonD / 2 });
      v = VectorD.ClampToZero(v);
      Assert.AreEqual(zero, v);
      v = new VectorD(new[] { -Numeric.EpsilonD * 2, Numeric.EpsilonD, Numeric.EpsilonD * 2, Numeric.EpsilonD });
      v = VectorD.ClampToZero(v);
      Assert.AreNotEqual(zero, v);

      Assert.IsNull(VectorD.ClampToZero(null));
    }


    [Test]
    public void ClampToZeroStatic2()
    {
      VectorD zero = new VectorD(4);
      VectorD v = new VectorD(new[] { 0.1, 0.1, -0.1, 0.09 });
      v = VectorD.ClampToZero(v, 0.11);
      Assert.AreEqual(zero, v);
      v = new VectorD(new[] { 0.1, -0.11, 0.11, 0.0 });
      v = VectorD.ClampToZero(v, 0.1);
      Assert.AreNotEqual(zero, v);

      Assert.IsNull(VectorD.ClampToZero(null, 0));
    }


    [Test]
    public void Dot()
    {
      VectorD unitX = new VectorD(new double[] { 1, 0, 0, 0 });
      VectorD unitY = new VectorD(new double[] { 0, 1, 0, 0 });
      VectorD unitZ = new VectorD(new double[] { 0, 0, 1, 0 });
      VectorD unitW = new VectorD(new double[] { 0, 0, 0, 1 });

      // 0°
      Assert.AreEqual(1.0, VectorD.Dot(unitX, unitX));
      Assert.AreEqual(1.0, VectorD.Dot(unitY, unitY));
      Assert.AreEqual(1.0, VectorD.Dot(unitZ, unitZ));
      Assert.AreEqual(1.0, VectorD.Dot(unitW, unitW));

      // 180°
      Assert.AreEqual(-1.0, VectorD.Dot(unitX, -unitX));
      Assert.AreEqual(-1.0, VectorD.Dot(unitY, -unitY));
      Assert.AreEqual(-1.0, VectorD.Dot(unitZ, -unitZ));
      Assert.AreEqual(-1.0, VectorD.Dot(unitW, -unitW));

      // 90°
      Assert.AreEqual(0.0, VectorD.Dot(unitX, unitY));
      Assert.AreEqual(0.0, VectorD.Dot(unitY, unitZ));
      Assert.AreEqual(0.0, VectorD.Dot(unitZ, unitW));
      Assert.AreEqual(0.0, VectorD.Dot(unitW, unitX));

      // 45°
      double angle = Math.Acos(VectorD.Dot(new VectorD(new double[] { 1, 1, 0, 0 }).Normalized, unitX));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(VectorD.Dot(new VectorD(new double[] { 0, 1, 1, 0 }).Normalized, unitY));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(VectorD.Dot(new VectorD(new double[] { 1, 0, 1, 0 }).Normalized, unitZ));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
      angle = Math.Acos(VectorD.Dot(new VectorD(new double[] { 1, 0, 0, 1 }).Normalized, unitW));
      Assert.IsTrue(Numeric.AreEqual(MathHelper.ToRadians(45.0), angle));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DotException1()
    {
      VectorD.Dot(null, new VectorD(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DotException2()
    {
      VectorD.Dot(new VectorD(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void DotException3()
    {
      VectorD.Dot(new VectorD(2), new VectorD(1));
    }


    //[Test]
    //public void Lerp()
    //{
    //  VectorD v = new VectorD(new double[] { 1.0, 10.0, 100.0, 1000.0 });
    //  VectorD w = new VectorD(new double[] { 2.0, 20.0, 200.0, 2000.0 });
    //  VectorD lerp0 = VectorD.Lerp(v, w, 0.0);
    //  VectorD lerp1 = VectorD.Lerp(v, w, 1.0);
    //  VectorD lerp05 = VectorD.Lerp(v, w, 0.5);
    //  VectorD lerp025 = VectorD.Lerp(v, w, 0.25);

    //  Assert.AreEqual(v, lerp0);
    //  Assert.AreEqual(w, lerp1);
    //  Assert.AreEqual(new VectorD(new double[] { 1.5, 15.0, 150.0, 1500.0 }), lerp05);
    //  Assert.AreEqual(new VectorD(new double[] { 1.25, 12.5, 125.0, 1250.0 }), lerp025);
    //}


    //[Test]
    //[ExpectedException(typeof(ArgumentNullException))]
    //public void LerpException1()
    //{
    //  VectorD.Lerp(null, new VectorD(1), 1);
    //}


    //[Test]
    //[ExpectedException(typeof(ArgumentNullException))]
    //public void LerpException2()
    //{
    //  VectorD.Lerp(new VectorD(1), null, 1);
    //}


    //[Test]
    //[ExpectedException(typeof(ArgumentException))]
    //public void LerpException3()
    //{
    //  VectorD.Lerp(new VectorD(2), new VectorD(1), 1);
    //}


    [Test]
    public void Min()
    {
      VectorD v1 = new VectorD(new[] { 1.0, 2.0, 2.5, 4.0 });
      VectorD v2 = new VectorD(new[] { -1.0, 2.0, 3.0, -2.0 });
      VectorD min = VectorD.Min(v1, v2);
      Assert.AreEqual(new VectorD(new[] { -1.0, 2.0, 2.5, -2.0 }), min);
    }


    [Test]
    public void Max()
    {
      VectorD v1 = new VectorD(new[] { 1.0, 2.0, 3.0, 4.0 });
      VectorD v2 = new VectorD(new[] { -1.0, 2.1, 3.0, 8.0 });
      VectorD max = VectorD.Max(v1, v2);
      Assert.AreEqual(new VectorD(new[] { 1.0, 2.1, 3.0, 8.0 }), max);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MinException1()
    {
      VectorD.Min(null, new VectorD(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MinException2()
    {
      VectorD.Min(new VectorD(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MinException3()
    {
      VectorD.Min(new VectorD(2), new VectorD(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MaxException1()
    {
      VectorD.Max(null, new VectorD(1));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MaxException2()
    {
      VectorD.Max(new VectorD(1), null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void MaxException3()
    {
      VectorD.Max(new VectorD(2), new VectorD(1));
    }


    [Test]
    public void ExplicitCastToVector2D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2 });
      Vector2D u = (Vector2D)v;

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
    }


    [Test]
    public void ToVector2D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2 });
      Vector2D u = v.ToVector2D();

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector2DException1()
    {
      VectorD v = null;
      Vector2D u = (Vector2D)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector2DException2()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3, 4.4, 5.5 });
      Vector2D u = (Vector2D)v;
    }


    [Test]
    public void ExplicitCastToVector3D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3 });
      Vector3D u = (Vector3D)v;

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
      Assert.AreEqual(3.3, u[2]);
    }


    [Test]
    public void ToVector3D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3 });
      Vector3D u = v.ToVector3D();

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
      Assert.AreEqual(3.3, u[2]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector3DException1()
    {
      VectorD v = null;
      Vector3D u = (Vector3D)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector3DException2()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3, 4.4, 5.5 });
      Vector3D u = (Vector3D)v;
    }


    [Test]
    public void ExplicitCastToVector4D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3, 4.4 });
      Vector4D u = (Vector4D)v;

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
      Assert.AreEqual(3.3, u[2]);
      Assert.AreEqual(4.4, u[3]);
    }


    [Test]
    public void ToVector4D()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3, 4.4 });
      Vector4D u = v.ToVector4D();

      Assert.AreEqual(1.1, u[0]);
      Assert.AreEqual(2.2, u[1]);
      Assert.AreEqual(3.3, u[2]);
      Assert.AreEqual(4.4, u[3]);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ExplicitCastToVector4DException1()
    {
      VectorD v = null;
      Vector4D u = (Vector4D)v;
    }


    [Test]
    [ExpectedException(typeof(InvalidCastException))]
    public void ExplicitCastToVector4DException2()
    {
      VectorD v = new VectorD(new[] { 1.1, 2.2, 3.3, 4.4, 5.5 });
      Vector4D u = (Vector4D)v;
    }


    [Test]
    public void ExplicitArrayCast()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      double[] values = (double[])v;
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);

      v = null;
      values = (double[])v;
      Assert.IsNull(values);
    }


    [Test]
    public void ToArray()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      double[] values = v.ToArray();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Length);
    }


    [Test]
    public void ExplicitListCast()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      List<double> values = (List<double>)v;
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);

      v = null;
      values = (List<double>)v;
      Assert.IsNull(values);
    }


    [Test]
    public void ToList()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      List<double> values = v.ToList();
      Assert.AreEqual(x, values[0]);
      Assert.AreEqual(y, values[1]);
      Assert.AreEqual(z, values[2]);
      Assert.AreEqual(w, values[3]);
      Assert.AreEqual(4, values.Count);
    }


    [Test]
    public void ExplicitCastToMatrixD()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      MatrixD matrix = (MatrixD)v;
      Assert.AreEqual(4, matrix.NumberOfRows);
      Assert.AreEqual(1, matrix.NumberOfColumns);
      Assert.AreEqual(x, matrix[0, 0]);
      Assert.AreEqual(y, matrix[1, 0]);
      Assert.AreEqual(z, matrix[2, 0]);
      Assert.AreEqual(w, matrix[3, 0]);

      v = null;
      matrix = (MatrixD)v;
      Assert.IsNull(matrix);
    }


    [Test]
    public void ToMatrixD()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      VectorD v = new VectorD(new[] { x, y, z, w });
      MatrixD matrix = v.ToMatrixD();
      Assert.AreEqual(4, matrix.NumberOfRows);
      Assert.AreEqual(1, matrix.NumberOfColumns);
      Assert.AreEqual(x, matrix[0, 0]);
      Assert.AreEqual(y, matrix[1, 0]);
      Assert.AreEqual(z, matrix[2, 0]);
      Assert.AreEqual(w, matrix[3, 0]);
    }


    [Test]
    public void ExplicitCastToVectorF()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] elementsD = new[] { x, y, z, w };
      float[] elementsF = new[] { (float)x, (float)y, (float)z, (float)w };
      VectorD vectorD = new VectorD(elementsD);
      VectorF vectorF = (VectorF)vectorD;
      Assert.AreEqual(new VectorF(elementsF), vectorF);

      vectorD = null;
      vectorF = (VectorF)vectorD;
      Assert.IsNull(vectorF);
    }


    [Test]
    public void ToVectorF()
    {
      double x = 23.4;
      double y = -11.0;
      double z = 0.0;
      double w = 0.3;
      double[] elementsD = new[] { x, y, z, w };
      float[] elementsF = new[] { (float)x, (float)y, (float)z, (float)w };
      VectorD vectorD = new VectorD(elementsD);
      VectorF vectorF = vectorD.ToVectorF();
      Assert.AreEqual(new VectorF(elementsF), vectorF);
    }


    [Test]
    public void GetLargestElement()
    {
      VectorD v = new VectorD(new double[] { -1, -2, -3, -4 });
      Assert.AreEqual(-1, v.LargestElement);

      v = new VectorD(new double[] { 10, 20, -30, -40 });
      Assert.AreEqual(20, v.LargestElement);

      v = new VectorD(new double[] { -1, 20, 30, 20 });
      Assert.AreEqual(30, v.LargestElement);

      v = new VectorD(new double[] { 10, 20, 30, 40 });
      Assert.AreEqual(40, v.LargestElement);
    }


    [Test]
    public void GetIndexOfLargestElement()
    {
      VectorD v = new VectorD(new double[] { -1, -2, -3, -4 });
      Assert.AreEqual(0, v.IndexOfLargestElement);

      v = new VectorD(new double[] { 10, 20, -30, -40 });
      Assert.AreEqual(1, v.IndexOfLargestElement);

      v = new VectorD(new double[] { -1, 20, 30, 20 });
      Assert.AreEqual(2, v.IndexOfLargestElement);

      v = new VectorD(new double[] { 10, 20, 30, 40 });
      Assert.AreEqual(3, v.IndexOfLargestElement);
    }


    [Test]
    public void GetSmallestElement()
    {
      VectorD v = new VectorD(new double[] { -4, -2, -3, -1 });
      Assert.AreEqual(-4, v.SmallestElement);

      v = new VectorD(new double[] { 10, 0, 3, 4 });
      Assert.AreEqual(0, v.SmallestElement);

      v = new VectorD(new double[] { -1, 20, -3, 0 });
      Assert.AreEqual(-3, v.SmallestElement);

      v = new VectorD(new double[] { 40, 30, 20, 10 });
      Assert.AreEqual(10, v.SmallestElement);
    }


    [Test]
    public void GetIndexOfSmallestElement()
    {
      VectorD v = new VectorD(new double[] { -4, -2, -3, -1 });
      Assert.AreEqual(0, v.IndexOfSmallestElement);

      v = new VectorD(new double[] { 10, 0, 3, 4 });
      Assert.AreEqual(1, v.IndexOfSmallestElement);

      v = new VectorD(new double[] { -1, 20, -3, 0 });
      Assert.AreEqual(2, v.IndexOfSmallestElement);

      v = new VectorD(new double[] { 40, 30, 20, 10 });
      Assert.AreEqual(3, v.IndexOfSmallestElement);
    }


    [Test]
    public void SerializationXml()
    {
      VectorD v1 = new VectorD(new[] { 0.1, -0.2, 3, 40, 50 });
      VectorD v2;
      string fileName = "SerializationVectorD.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(VectorD));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, v1);
      writer.Close();

      serializer = new XmlSerializer(typeof(VectorD));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      v2 = (VectorD)serializer.Deserialize(fileStream);
      Assert.AreEqual(v1, v2);

      // We dont have schema.
      Assert.AreEqual(null, new VectorD().GetSchema());
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      VectorD v1 = new VectorD(new[] { 0.1, -0.2, 3, 40, 50 });
      VectorD v2;
      string fileName = "SerializationVectorD.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, v1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      v2 = (VectorD)formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationXml2()
    {
      VectorD v1 = new VectorD(new[] { 0.1, -0.2, 3, 40, 50 });
      VectorD v2;

      string fileName = "SerializationVectorD_DataContractSerializer.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractSerializer(typeof(VectorD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
      using (var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
        serializer.WriteObject(writer, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      using (var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas()))
        v2 = (VectorD)serializer.ReadObject(reader);

      Assert.AreEqual(v1, v2);
    }


    [Test]
    public void SerializationJson()
    {
      VectorD v1 = new VectorD(new[] { 0.1, -0.2, 3, 40, 50 });
      VectorD v2;

      string fileName = "SerializationVectorD.json";

      if (File.Exists(fileName))
        File.Delete(fileName);

      var serializer = new DataContractJsonSerializer(typeof(VectorD));
      using (var stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
        serializer.WriteObject(stream, v1);

      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        v2 = (VectorD)serializer.ReadObject(stream);

      Assert.AreEqual(v1, v2);
    }


    /* Not available in PCL build.
    [Test]
    [ExpectedException(typeof(SerializationException))]
    public void SerializationConstructorException()
    {
      new VectorDSerializationTest(); // Will throw exception in serialization ctor.
    }


    private class VectorDSerializationTest : VectorD
    {
      public VectorDSerializationTest() : base(null, new StreamingContext())
      {
      }
    }
    */
  }
}
 