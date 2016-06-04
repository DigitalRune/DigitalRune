using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Tests
{
  [TestFixture]
  public class NumericTest
  {
    [Test]
    public void FloatSpecialCases()
    {
      // NaN
      Assert.IsTrue(Single.NaN != Single.NaN);
      Assert.IsFalse(Single.NaN == Single.NaN);
      Assert.IsFalse(Single.NaN < Single.NaN);
      Assert.IsFalse(Single.NaN > Single.NaN);
      Assert.IsFalse(Single.NaN == 0.0f);
      Assert.IsFalse(Single.NaN < 0.0f);
      Assert.IsFalse(Single.NaN > 0.0f);
      Assert.IsFalse(Single.NaN == Single.NegativeInfinity);
      Assert.IsFalse(Single.NaN < Single.NegativeInfinity);
      Assert.IsFalse(Single.NaN > Single.NegativeInfinity);
      Assert.IsFalse(Single.NaN == Single.PositiveInfinity);
      Assert.IsFalse(Single.NaN < Single.PositiveInfinity);
      Assert.IsFalse(Single.NaN > Single.PositiveInfinity);

      // Infinity
      Assert.IsTrue(Single.PositiveInfinity == Single.PositiveInfinity);
      Assert.IsTrue(Single.NegativeInfinity == Single.NegativeInfinity);
      Assert.IsTrue(Single.PositiveInfinity != Single.NegativeInfinity);
      Assert.IsTrue(Single.NegativeInfinity < Single.PositiveInfinity);
      Assert.IsTrue(Single.PositiveInfinity != Single.NaN);
      Assert.IsTrue(Single.NegativeInfinity != Single.NaN);
      Assert.IsTrue(0 < Single.PositiveInfinity);
      Assert.IsTrue(Single.MaxValue < Single.PositiveInfinity);
      Assert.IsTrue(Single.NegativeInfinity < 0);
      Assert.IsTrue(Single.NegativeInfinity < Single.MinValue);
    }


    [Test]
    public void DoubleSpecialCases()
    {
      // NaN
      Assert.IsTrue(Double.NaN != Double.NaN);
      Assert.IsFalse(Double.NaN == Double.NaN);
      Assert.IsFalse(Double.NaN < Double.NaN);
      Assert.IsFalse(Double.NaN > Double.NaN);
      Assert.IsFalse(Double.NaN == 0.0);
      Assert.IsFalse(Double.NaN < 0.0);
      Assert.IsFalse(Double.NaN > 0.0);
      Assert.IsFalse(Double.NaN == Double.NegativeInfinity);
      Assert.IsFalse(Double.NaN < Double.NegativeInfinity);
      Assert.IsFalse(Double.NaN > Double.NegativeInfinity);
      Assert.IsFalse(Double.NaN == Double.PositiveInfinity);
      Assert.IsFalse(Double.NaN < Double.PositiveInfinity);
      Assert.IsFalse(Double.NaN > Double.PositiveInfinity);

      // Infinity
      Assert.IsTrue(Double.PositiveInfinity == Double.PositiveInfinity);
      Assert.IsTrue(Double.NegativeInfinity == Double.NegativeInfinity);
      Assert.IsTrue(Double.PositiveInfinity != Double.NegativeInfinity);
      Assert.IsTrue(Double.NegativeInfinity < Double.PositiveInfinity);
      Assert.IsTrue(Double.PositiveInfinity != Double.NaN);
      Assert.IsTrue(Double.NegativeInfinity != Double.NaN);
      Assert.IsTrue(0 < Double.PositiveInfinity);
      Assert.IsTrue(Double.MaxValue < Double.PositiveInfinity);
      Assert.IsTrue(Double.NegativeInfinity < 0);
      Assert.IsTrue(Double.NegativeInfinity < Double.MinValue);
    }


    [Test]
    public void EpsilonD()
    {
      double original = Numeric.EpsilonD;
      Assert.IsTrue(original < 0.1);
      Assert.IsTrue(original > 0.0);      
      Assert.AreEqual(Numeric.EpsilonD * Numeric.EpsilonD, Numeric.EpsilonDSquared);

      Numeric.EpsilonD = 1.1;
      Assert.AreEqual(1.1, Numeric.EpsilonD);
      Assert.AreEqual(Numeric.EpsilonD * Numeric.EpsilonD, Numeric.EpsilonDSquared);

      Numeric.EpsilonD = original;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonDException()
    {
      Numeric.EpsilonD = -0.0001;
    }

    [Test]
    public void EpsilonF()
    {
      float original = Numeric.EpsilonF;
      Assert.IsTrue(original < 0.1f);
      Assert.IsTrue(original > 0.0f);
      Assert.AreEqual(Numeric.EpsilonF * Numeric.EpsilonF, Numeric.EpsilonFSquared);

      Numeric.EpsilonF = 1.1f;
      Assert.AreEqual(1.1f, Numeric.EpsilonF);
      Assert.AreEqual(Numeric.EpsilonF * Numeric.EpsilonF, Numeric.EpsilonFSquared);

      Numeric.EpsilonF = original;
    }



    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void EpsilonFException()
    {
      Numeric.EpsilonF = -0.0001f;
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void AreEqualDException()
    {
      Numeric.AreEqual(0.0, 0.0, - 0.1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void AreEqualFException()
    {
      Numeric.AreEqual(0.0f, 0.0f, -0.1f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CompareDException()
    {
      Numeric.Compare(0.0, 0.0, -0.1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void CompareFException()
    {
      Numeric.Compare(0.0f, 0.0f, -0.1f);
    }



    [Test]
    public void EqualsD()
    {
      double d1 = 1.1;
      double d2 = 1.099999999999999999;

      int r = Numeric.Compare(d1, d2);
      Assert.AreEqual(0, r);
      
      r = Numeric.Compare(d1, d2, 0.00000001);
      Assert.AreEqual(0, r);
      
      bool b = Numeric.AreEqual(d1, d2);
      Assert.IsTrue(b);

      b = Numeric.AreEqual(d1, d2, 0.0001);
      Assert.IsTrue(b);
    }

    [Test]
    public void EqualsF()
    {
      float f1 = 1.1f;
      float f2 = 1.0999999f;

      int r = Numeric.Compare(f1, f2);
      Assert.AreEqual(0, r);

      r = Numeric.Compare(f1, f2, 0.000001f);
      Assert.AreEqual(0, r);

      bool b = Numeric.AreEqual(f1, f2);
      Assert.IsTrue(b);

      b = Numeric.AreEqual(f1, f2, 0.0001f);
      Assert.IsTrue(b);
    }

    [Test]
    public void UnequalsD()
    {
      double d1 = 1.0999;
      double d2 = 1.1;
      
      int r = Numeric.Compare(d1, d2);
      Assert.AreEqual(-1, r);

      r = Numeric.Compare(d2, d1);
      Assert.AreEqual(1, r);

      r = Numeric.Compare(d1, d2, 0.00001);
      Assert.AreEqual(-1, r);

      r = Numeric.Compare(d2, d1, 0.00001);
      Assert.AreEqual(1, r);

      bool b = Numeric.AreEqual(d1, d2);
      Assert.IsFalse(b);

      b = Numeric.AreEqual(d1, d2, 0.00001);
      Assert.IsFalse(b);
    }

    [Test]
    public void UnequalsF()
    {
      float f1 = 1.0999f;
      float f2 = 1.1f;

      int r = Numeric.Compare(f1, f2);
      Assert.AreEqual(-1, r);

      r = Numeric.Compare(f2, f1);
      Assert.AreEqual(1, r);

      r = Numeric.Compare(f1, f2, 0.00001f);
      Assert.AreEqual(-1, r);

      r = Numeric.Compare(f2, f1, 0.00001f);
      Assert.AreEqual(1, r);

      bool b = Numeric.AreEqual(f1, f2);
      Assert.IsFalse(b);

      b = Numeric.AreEqual(f1, f2, 0.00001f);
      Assert.IsFalse(b);
    }

    [Test]
    public void BigNumbersD()
    {
      double originalEpsilon = Numeric.EpsilonD;
      Numeric.EpsilonD = 1e-8;

      Assert.AreEqual(-1, Numeric.Compare(1e20, 1.0000002e20));
      Assert.AreEqual(0, Numeric.Compare(1e20, 1.00000001e20));
      Assert.AreEqual(1, Numeric.Compare(1.0000002e20, 1e20));

      Assert.AreEqual(-1, Numeric.Compare(1e20, 1.002e20, 0.001e20));
      Assert.AreEqual(0, Numeric.Compare(1e20, 1.0001e20, 0.001e20));
      Assert.AreEqual(1, Numeric.Compare(1.002e20, 1e20, 0.001e20));

      Assert.IsTrue(Numeric.AreEqual(1e20, 1.00000001e20));
      Assert.IsFalse(Numeric.AreEqual(1e20, 1.0000002e20));
      Assert.IsTrue(Numeric.AreEqual(1e20, 1.0001e20, 0.001e20));

      Numeric.EpsilonD = originalEpsilon;
    }

    [Test]
    public void BigNumbersF()
    {
      float originalEpsilon = Numeric.EpsilonF;
      Numeric.EpsilonF = 1e-8f;

      Assert.AreEqual(-1, Numeric.Compare(1e10f, 1.0000002e10f));
      Assert.AreEqual(0, Numeric.Compare(1e10f, 1.00000001e10f));
      Assert.AreEqual(1, Numeric.Compare(1.0000002e10f, 1e10f));

      Assert.AreEqual(-1, Numeric.Compare(1e10f, 1.002e10f, 0.001e10f));
      Assert.AreEqual(0, Numeric.Compare(1e10f, 1.0001e10f, 0.001e10f));
      Assert.AreEqual(1, Numeric.Compare(1.002e10f, 1e10f, 0.001e10f));

      Assert.IsTrue(Numeric.AreEqual(1e10f, 1.00000001e10f));
      Assert.IsFalse(Numeric.AreEqual(1e10f, 1.0000002e10f));
      Assert.IsTrue(Numeric.AreEqual(1e10f, 1.0001e10f, 0.001e10f));

      Numeric.EpsilonF = originalEpsilon;
    }

    [Test]
    public void SmallNumbersD()
    {
      Assert.AreEqual(-1, Numeric.Compare(1e-20, 1.002e-20, 0.001e-20));
      Assert.AreEqual(0, Numeric.Compare(1e-20, 1.0001e-20, 0.001e-20));
      Assert.AreEqual(1, Numeric.Compare(1.002e-20, 1e-20, 0.001e-20));

      // Values near zero are treated as zero
      Assert.IsTrue(Numeric.AreEqual(1e-20, 1.00000001e-20));
      Assert.IsTrue(Numeric.AreEqual(1e-20, 1.0000002e-20));

      // Values near zero can only be compared by specifying an epsilon value.
      Assert.IsTrue(Numeric.AreEqual(1e-20, 1.0001e-20, 0.001e-20));
      Assert.IsFalse(Numeric.AreEqual(1e-20, 1.0002e-20, 0.0001e-20));
    }

    [Test]
    public void SmallNumbersF()
    {
      Assert.AreEqual(-1, Numeric.Compare(1e-10f, 1.002e-10f, 0.001e-10f));
      Assert.AreEqual(0, Numeric.Compare(1e-10f, 1.0001e-10f, 0.001e-10f));
      Assert.AreEqual(1, Numeric.Compare(1.002e-10f, 1e-10f, 0.001e-10f));

      // Values near zero are treated as zero
      Assert.IsTrue(Numeric.AreEqual(1e-10f, 1.0000001e-10f));
      Assert.IsTrue(Numeric.AreEqual(1e-10f, 1.000002e-10f));

      // Values near zero can only be compared by specifying an epsilon value.
      Assert.IsTrue(Numeric.AreEqual(1e-10f, 1.0001e-10f, 0.001e-10f));
      Assert.IsFalse(Numeric.AreEqual(1e-10f, 1.0002e-10f, 0.0001e-10f));
    }


    [Test]
    public void ClampToZeroF()
    {
      Assert.AreEqual(1f, Numeric.ClampToZero(1f));
      Assert.AreEqual(-1f, Numeric.ClampToZero(-1f));
      Assert.AreEqual(0f, Numeric.ClampToZero(0f));
      Assert.AreEqual(Numeric.EpsilonF + 0.0001f, Numeric.ClampToZero(Numeric.EpsilonF + 0.0001f));
      Assert.AreEqual(-Numeric.EpsilonF -0.0001f, Numeric.ClampToZero(-Numeric.EpsilonF - 0.0001f));
      Assert.AreEqual(0, Numeric.ClampToZero(Numeric.EpsilonF - 0.000000001f));
      Assert.AreEqual(0, Numeric.ClampToZero(-Numeric.EpsilonF + 0.000000001f));

      Assert.AreEqual(1f, Numeric.ClampToZero(1f, 0.1f));
      Assert.AreEqual(-1f, Numeric.ClampToZero(-1f, 0.1f));
      Assert.AreEqual(0f, Numeric.ClampToZero(0f, 0.1f));
      Assert.AreEqual(0.11f, Numeric.ClampToZero(0.11f, 0.1f));
      Assert.AreEqual(-0.11f, Numeric.ClampToZero(-0.11f, 0.1f));
      Assert.AreEqual(0, Numeric.ClampToZero(0.09f, 0.1f));
      Assert.AreEqual(0, Numeric.ClampToZero(-0.09f, 0.1f));
    }



    [Test]
    public void ClampToZeroD()
    {
      Assert.AreEqual(1d, Numeric.ClampToZero(1d));
      Assert.AreEqual(-1d, Numeric.ClampToZero(-1d));
      Assert.AreEqual(0d, Numeric.ClampToZero(0d));
      Assert.AreEqual(Numeric.EpsilonD + 0.0001d, Numeric.ClampToZero(Numeric.EpsilonD + 0.0001d));
      Assert.AreEqual(-Numeric.EpsilonD - 0.0001d, Numeric.ClampToZero(-Numeric.EpsilonD - 0.0001d));
      Assert.AreEqual(0, Numeric.ClampToZero(Numeric.EpsilonD - 0.000000000001d));
      Assert.AreEqual(0, Numeric.ClampToZero(-Numeric.EpsilonD + 0.000000000001d));

      Assert.AreEqual(1d, Numeric.ClampToZero(1d, 0.1d));
      Assert.AreEqual(-1d, Numeric.ClampToZero(-1d, 0.1d));
      Assert.AreEqual(0d, Numeric.ClampToZero(0d, 0.1d));
      Assert.AreEqual(0.11d, Numeric.ClampToZero(0.11d, 0.1d));
      Assert.AreEqual(-0.11d, Numeric.ClampToZero(-0.11d, 0.1d));
      Assert.AreEqual(0, Numeric.ClampToZero(0.09d, 0.1d));
      Assert.AreEqual(0, Numeric.ClampToZero(-0.09d, 0.1d));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IsZeroDException()
    {
      Numeric.IsZero(0.0, -0.1);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void IsZeroFException()
    {
      Numeric.IsZero(0.0f, -0.1f);
    }


    [Test]
    public void IsZeroD()
    {
      Assert.IsTrue(Numeric.IsZero(0.0));
      Assert.IsTrue(Numeric.IsZero(1e-20));
      Assert.IsTrue(Numeric.IsZero(-1e-20));
      Assert.IsFalse(Numeric.IsZero(0.00001));
      Assert.IsFalse(Numeric.IsZero(-0.00001));

      Assert.IsTrue(Numeric.IsZero(0.0, 0.0001));
      Assert.IsTrue(Numeric.IsZero(1e-20, 1e-19));
      Assert.IsTrue(Numeric.IsZero(-1e-20, 1e-19));
      Assert.IsTrue(Numeric.IsZero(0.000009, 0.00001));
      Assert.IsTrue(Numeric.IsZero(-0.000009, 0.0001));
      Assert.IsFalse(Numeric.IsZero(0.00001, 0.000009));
      Assert.IsFalse(Numeric.IsZero(-0.00001, 0.000009));
    }

    [Test]
    public void IsZero()
    {
      Assert.IsTrue(Numeric.IsZero(0.0f));
      Assert.IsTrue(Numeric.IsZero(1e-10f));
      Assert.IsTrue(Numeric.IsZero(-1e-10f));
      Assert.IsFalse(Numeric.IsZero(0.00002f));
      Assert.IsFalse(Numeric.IsZero(-0.00002f));

      Assert.IsTrue(Numeric.IsZero(0.0f, 0.0001f));
      Assert.IsTrue(Numeric.IsZero(1e-20f, 1e-19f));
      Assert.IsTrue(Numeric.IsZero(-1e-20f, 1e-19f));
      Assert.IsTrue(Numeric.IsZero(0.000009f, 0.00001f));
      Assert.IsTrue(Numeric.IsZero(-0.000009f, 0.0001f));
      Assert.IsFalse(Numeric.IsZero(0.00001f, 0.000009f));
      Assert.IsFalse(Numeric.IsZero(-0.00001f, 0.000009f));
    }

    [Test]
    public void NegativeValuesD()
    {
      // Test case for bug fix
      Assert.IsTrue(Numeric.AreEqual(-1000.0, -1000.0));
      Assert.IsFalse(Numeric.AreEqual(-1000.0, 1000.0));
      Assert.IsFalse(Numeric.AreEqual(1000.0, -1000.0));

      Assert.AreEqual(0, Numeric.Compare(-1000.0, -1000.0));
      Assert.AreEqual(-1, Numeric.Compare(-1000.0, 1000.0));
      Assert.AreEqual(1, Numeric.Compare(1000.0, -1000.0));
    }

    [Test]
    public void NegativeValuesF()
    {
      // Test case for bug fix
      Assert.IsTrue(Numeric.AreEqual(-1000.0f, -1000.0f));
      Assert.IsFalse(Numeric.AreEqual(-1000.0f, 1000.0f));
      Assert.IsFalse(Numeric.AreEqual(1000.0f, -1000.0f));

      Assert.AreEqual(0, Numeric.Compare(-1000.0f, -1000.0f));
      Assert.AreEqual(-1, Numeric.Compare(-1000.0f, 1000.0f));
      Assert.AreEqual(1, Numeric.Compare(1000.0f, -1000.0f));
    }

    [Test]
    public void ComparisionsWithZeroD()
    {
      // Test case for bug fix
      Assert.IsTrue(Numeric.AreEqual(0.0, 0.0));
      Assert.AreEqual(0, Numeric.Compare(0.0, 0.0));

      // Values near zero are treated as zero
      Assert.IsTrue(Numeric.AreEqual(0.0, 1e-20));
      Assert.IsTrue(Numeric.AreEqual(0.0, -1e-20));
      Assert.IsTrue(Numeric.AreEqual(-1e-20, 0.0));
      Assert.IsTrue(Numeric.AreEqual(1e-20, 0.0));

      Assert.IsFalse(Numeric.AreEqual(0.0, 1e-6));
      Assert.IsFalse(Numeric.AreEqual(0.0, -1e-6));
      Assert.IsFalse(Numeric.AreEqual(-1e-6, 0.0));
      Assert.IsFalse(Numeric.AreEqual(1e-6, 0.0));

      // Values near zero are treated as zero
      Assert.AreEqual(0, Numeric.Compare(0.0, 1e-20));
      Assert.AreEqual(0, Numeric.Compare(0.0, -1e-20));
      Assert.AreEqual(0, Numeric.Compare(-1e-20, 0.0));
      Assert.AreEqual(0, Numeric.Compare(1e-20, 0.0));

      Assert.AreEqual(-1, Numeric.Compare(0.0, 1e-6));
      Assert.AreEqual(+1, Numeric.Compare(0.0, -1e-6));
      Assert.AreEqual(-1, Numeric.Compare(-1e-6, 0.0));
      Assert.AreEqual(+1, Numeric.Compare(1e-6, 0.0));
    }

    [Test]
    public void ComparisionsWithZeroF()
    {
      // Test case for bug fix
      Assert.IsTrue(Numeric.AreEqual(0.0f, 0.0f));
      Assert.AreEqual(0, Numeric.Compare(0.0f, 0.0f));

      // Values near zero are treated as zero
      Assert.IsTrue(Numeric.AreEqual(0.0f, 1e-10f));
      Assert.IsTrue(Numeric.AreEqual(0.0f, -1e-10f));
      Assert.IsTrue(Numeric.AreEqual(-1e-10f, 0.0f));
      Assert.IsTrue(Numeric.AreEqual(1e-10f, 0.0f));

      Assert.IsFalse(Numeric.AreEqual(0.0f, 1e-4f));
      Assert.IsFalse(Numeric.AreEqual(0.0f, -1e-4f));
      Assert.IsFalse(Numeric.AreEqual(-1e-4f, 0.0f));
      Assert.IsFalse(Numeric.AreEqual(1e-4f, 0.0f));      

      // Values near zero are treated as zero
      Assert.AreEqual(0, Numeric.Compare(0.0f, 1e-10f));
      Assert.AreEqual(0, Numeric.Compare(0.0f, -1e-10f));
      Assert.AreEqual(0, Numeric.Compare(-1e-10f, 0.0f));
      Assert.AreEqual(0, Numeric.Compare(1e-10f, 0.0f));

      Assert.AreEqual(-1, Numeric.Compare(0.0f, 1e-4f));
      Assert.AreEqual(+1, Numeric.Compare(0.0f, -1e-4f));
      Assert.AreEqual(-1, Numeric.Compare(-1e-4f, 0.0f));
      Assert.AreEqual(+1, Numeric.Compare(1e-4f, 0.0f));
    }

    [Test]
    public void InfinityEqualityF()
    {      
      Assert.IsTrue(Numeric.AreEqual(float.PositiveInfinity, float.PositiveInfinity));
      Assert.IsTrue(Numeric.AreEqual(float.PositiveInfinity, float.PositiveInfinity, 0.1f));
      Assert.IsTrue(Numeric.AreEqual(float.NegativeInfinity, float.NegativeInfinity));
      Assert.IsTrue(Numeric.AreEqual(float.NegativeInfinity, float.NegativeInfinity, 0.1f));
      
      Assert.IsFalse(Numeric.AreEqual(float.PositiveInfinity, float.NegativeInfinity));
      Assert.IsFalse(Numeric.AreEqual(float.NegativeInfinity, float.PositiveInfinity, 0.1f));
      
      Assert.IsFalse(Numeric.AreEqual(0f, float.PositiveInfinity));
      Assert.IsFalse(Numeric.AreEqual(float.NegativeInfinity, 0f));

      Assert.IsTrue(Numeric.Compare(float.PositiveInfinity, float.PositiveInfinity) == 0);
      Assert.IsTrue(Numeric.Compare(float.NegativeInfinity, float.NegativeInfinity) == 0);
      Assert.IsTrue(Numeric.Compare(float.NegativeInfinity, float.PositiveInfinity) < 0);
      Assert.IsTrue(Numeric.Compare(float.PositiveInfinity, float.NegativeInfinity) > 0);
      Assert.IsTrue(Numeric.Compare(0f, float.NegativeInfinity) > 0);
      Assert.IsTrue(Numeric.Compare(0f, float.PositiveInfinity) < 0);
      Assert.IsTrue(Numeric.Compare(0f, float.NegativeInfinity) > 0);
    }

    [Test]
    public void InfinityEqualityD()
    {
      Assert.IsTrue(Numeric.AreEqual(double.PositiveInfinity, double.PositiveInfinity));
      Assert.IsTrue(Numeric.AreEqual(double.PositiveInfinity, double.PositiveInfinity, 0.1d));
      Assert.IsTrue(Numeric.AreEqual(double.NegativeInfinity, double.NegativeInfinity));
      Assert.IsTrue(Numeric.AreEqual(double.NegativeInfinity, double.NegativeInfinity, 0.1d));

      Assert.IsFalse(Numeric.AreEqual(double.PositiveInfinity, double.NegativeInfinity));
      Assert.IsFalse(Numeric.AreEqual(double.NegativeInfinity, double.PositiveInfinity, 0.1d));

      Assert.IsFalse(Numeric.AreEqual(0d, double.PositiveInfinity));
      Assert.IsFalse(Numeric.AreEqual(double.NegativeInfinity, 0d));

      Assert.IsTrue(Numeric.Compare(double.PositiveInfinity, double.PositiveInfinity) == 0);
      Assert.IsTrue(Numeric.Compare(double.NegativeInfinity, double.NegativeInfinity) == 0);
      Assert.IsTrue(Numeric.Compare(double.NegativeInfinity, double.PositiveInfinity) < 0);
      Assert.IsTrue(Numeric.Compare(double.PositiveInfinity, double.NegativeInfinity) > 0);
      Assert.IsTrue(Numeric.Compare(0d, double.NegativeInfinity) > 0);
      Assert.IsTrue(Numeric.Compare(0d, double.PositiveInfinity) < 0);
      Assert.IsTrue(Numeric.Compare(0d, double.NegativeInfinity) > 0);   
    }


    [Test]
    public void IsFiniteOrNaN()
    {
      Assert.IsFalse(Numeric.IsFiniteOrNaN(Single.NegativeInfinity));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Single.MinValue));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(-1.0f));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(0.0f));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(1.0f));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Single.MaxValue));
      Assert.IsFalse(Numeric.IsFiniteOrNaN(Single.PositiveInfinity));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Single.NaN));

      Assert.IsFalse(Numeric.IsFiniteOrNaN(Double.NegativeInfinity));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Double.MinValue));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(-1.0));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(0.0));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(1.0));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Double.MaxValue));
      Assert.IsFalse(Numeric.IsFiniteOrNaN(Double.PositiveInfinity));
      Assert.IsTrue(Numeric.IsFiniteOrNaN(Double.NaN));
    }


    [Test]
    public void IsFinite()
    {
      Assert.IsFalse(Numeric.IsFinite(Single.NegativeInfinity));
      Assert.IsTrue(Numeric.IsFinite(Single.MinValue));
      Assert.IsTrue(Numeric.IsFinite(-1.0f));
      Assert.IsTrue(Numeric.IsFinite(0.0f));
      Assert.IsTrue(Numeric.IsFinite(1.0f));
      Assert.IsTrue(Numeric.IsFinite(Single.MaxValue));
      Assert.IsFalse(Numeric.IsFinite(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsFinite(Single.NaN));

      Assert.IsFalse(Numeric.IsFinite(Double.NegativeInfinity));
      Assert.IsTrue(Numeric.IsFinite(Double.MinValue));
      Assert.IsTrue(Numeric.IsFinite(-1.0));
      Assert.IsTrue(Numeric.IsFinite(0.0));
      Assert.IsTrue(Numeric.IsFinite(1.0));
      Assert.IsTrue(Numeric.IsFinite(Double.MaxValue));
      Assert.IsFalse(Numeric.IsFinite(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsFinite(Double.NaN));
    }


    [Test]
    public void IsPositive()
    {
      Assert.IsFalse(Numeric.IsPositive(Single.NegativeInfinity));
      Assert.IsFalse(Numeric.IsPositive(Single.MinValue));
      Assert.IsFalse(Numeric.IsPositive(-1.0f));
      Assert.IsFalse(Numeric.IsPositive(0.0f));
      Assert.IsTrue(Numeric.IsPositive(1.0f));
      Assert.IsTrue(Numeric.IsPositive(Single.MaxValue));
      Assert.IsTrue(Numeric.IsPositive(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsPositive(Single.NaN));

      Assert.IsFalse(Numeric.IsPositive(Double.NegativeInfinity));
      Assert.IsFalse(Numeric.IsPositive(Double.MinValue));
      Assert.IsFalse(Numeric.IsPositive(-1.0));
      Assert.IsFalse(Numeric.IsPositive(0.0));
      Assert.IsTrue(Numeric.IsPositive(1.0));
      Assert.IsTrue(Numeric.IsPositive(Double.MaxValue));
      Assert.IsTrue(Numeric.IsPositive(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsPositive(Double.NaN));
    }


    [Test]
    public void IsNegative()
    {
      Assert.IsTrue(Numeric.IsNegative(Single.NegativeInfinity));
      Assert.IsTrue(Numeric.IsNegative(Single.MinValue));
      Assert.IsTrue(Numeric.IsNegative(-1.0f));
      Assert.IsFalse(Numeric.IsNegative(0.0f));
      Assert.IsFalse(Numeric.IsNegative(1.0f));
      Assert.IsFalse(Numeric.IsNegative(Single.MaxValue));
      Assert.IsFalse(Numeric.IsNegative(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsNegative(Single.NaN));

      Assert.IsTrue(Numeric.IsNegative(Double.NegativeInfinity));
      Assert.IsTrue(Numeric.IsNegative(Double.MinValue));
      Assert.IsTrue(Numeric.IsNegative(-1.0));
      Assert.IsFalse(Numeric.IsNegative(0.0));
      Assert.IsFalse(Numeric.IsNegative(1.0));
      Assert.IsFalse(Numeric.IsNegative(Double.MaxValue));
      Assert.IsFalse(Numeric.IsNegative(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsNegative(Double.NaN));
    }


    [Test]
    public void IsPositiveFinite()
    {
      Assert.IsFalse(Numeric.IsPositiveFinite(Single.NegativeInfinity));
      Assert.IsFalse(Numeric.IsPositiveFinite(Single.MinValue));
      Assert.IsFalse(Numeric.IsPositiveFinite(-1.0f));
      Assert.IsFalse(Numeric.IsPositiveFinite(0.0f));
      Assert.IsTrue(Numeric.IsPositiveFinite(1.0f));
      Assert.IsTrue(Numeric.IsPositiveFinite(Single.MaxValue));
      Assert.IsFalse(Numeric.IsPositiveFinite(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsPositiveFinite(Single.NaN));

      Assert.IsFalse(Numeric.IsPositiveFinite(Double.NegativeInfinity));
      Assert.IsFalse(Numeric.IsPositiveFinite(Double.MinValue));
      Assert.IsFalse(Numeric.IsPositiveFinite(-1.0));
      Assert.IsFalse(Numeric.IsPositiveFinite(0.0));
      Assert.IsTrue(Numeric.IsPositiveFinite(1.0));
      Assert.IsTrue(Numeric.IsPositiveFinite(Double.MaxValue));
      Assert.IsFalse(Numeric.IsPositiveFinite(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsPositiveFinite(Double.NaN));
    }


    [Test]
    public void IsNegativeFinite()
    {
      Assert.IsFalse(Numeric.IsNegativeFinite(Single.NegativeInfinity));
      Assert.IsTrue(Numeric.IsNegativeFinite(Single.MinValue));
      Assert.IsTrue(Numeric.IsNegativeFinite(-1.0f));
      Assert.IsFalse(Numeric.IsNegativeFinite(0.0f));
      Assert.IsFalse(Numeric.IsNegativeFinite(1.0f));
      Assert.IsFalse(Numeric.IsNegativeFinite(Single.MaxValue));
      Assert.IsFalse(Numeric.IsNegativeFinite(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsNegativeFinite(Single.NaN));

      Assert.IsFalse(Numeric.IsNegativeFinite(Double.NegativeInfinity));
      Assert.IsTrue(Numeric.IsNegativeFinite(Double.MinValue));
      Assert.IsTrue(Numeric.IsNegativeFinite(-1.0));
      Assert.IsFalse(Numeric.IsNegativeFinite(0.0));
      Assert.IsFalse(Numeric.IsNegativeFinite(1.0));
      Assert.IsFalse(Numeric.IsNegativeFinite(Double.MaxValue));
      Assert.IsFalse(Numeric.IsNegativeFinite(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsNegativeFinite(Double.NaN));
    }


    [Test]
    public void IsZeroOrPositiveFinite()
    {
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Single.NegativeInfinity));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Single.MinValue));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(-1.0f));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(0.0f));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(1.0f));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(Single.MaxValue));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Single.NaN));

      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Double.NegativeInfinity));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Double.MinValue));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(-1.0));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(0.0));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(1.0));
      Assert.IsTrue(Numeric.IsZeroOrPositiveFinite(Double.MaxValue));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsZeroOrPositiveFinite(Double.NaN));
    }


    [Test]
    public void IsZeroOrNegativeFinite()
    {
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Single.NegativeInfinity));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(Single.MinValue));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(-1.0f));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(0.0f));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(1.0f));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Single.MaxValue));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Single.PositiveInfinity));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Single.NaN));

      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Double.NegativeInfinity));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(Double.MinValue));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(-1.0));
      Assert.IsTrue(Numeric.IsZeroOrNegativeFinite(0.0));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(1.0));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Double.MaxValue));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Double.PositiveInfinity));
      Assert.IsFalse(Numeric.IsZeroOrNegativeFinite(Double.NaN));
    }


    [Test]
    public void UnequalityChecks_Float()
    {
      float epsilon = 0.01f;

      // ----- Values are within epsilon tolerance.
      // Default epsilon interval:
      float value1 = -0.1f;
      float value2 = -0.1f + float.Epsilon * 10f;

      Assert.IsFalse(Numeric.IsLess(value1, value2));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsFalse(Numeric.IsGreater(value1, value2));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1f;
      value2 = -0.1f + 0.009f;
      Assert.IsFalse(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2, epsilon));

      // ----- value1 < value2.
      // Default epsilon interval:
      value1 = -0.1f - 2 * Numeric.EpsilonF;
      value2 = -0.1f;
      Assert.IsTrue(Numeric.IsLess(value1, value2));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsFalse(Numeric.IsGreater(value1, value2));
      Assert.IsFalse(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1f - 0.011f;
      value2 = -0.1f;
      Assert.IsTrue(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreaterOrEqual(value1, value2, epsilon));

      // ----- value1 > value2.
      // Default epsilon interval:
      value1 = -0.1f;
      value2 = -0.1f - 2 * Numeric.EpsilonF;
      Assert.IsFalse(Numeric.IsLess(value1, value2));
      Assert.IsFalse(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsTrue(Numeric.IsGreater(value1, value2));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1f;
      value2 = -0.1f - 0.011f;
      Assert.IsFalse(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2, epsilon));
    }


    [Test]
    public void UnequalityChecks_Double()
    {
      double epsilon = 0.01;

      // ----- Values are within epsilon tolerance.
      // Default epsilon interval:
      double value1 = -0.1;
      double value2 = -0.1 + double.Epsilon * 10;

      Assert.IsFalse(Numeric.IsLess(value1, value2));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsFalse(Numeric.IsGreater(value1, value2));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1;
      value2 = -0.1 + 0.009;
      Assert.IsFalse(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2, epsilon));

      // ----- value1 < value2.
      // Default epsilon interval:
      value1 = -0.1 - 2 * Numeric.EpsilonD;
      value2 = -0.1;
      Assert.IsTrue(Numeric.IsLess(value1, value2));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsFalse(Numeric.IsGreater(value1, value2));
      Assert.IsFalse(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1 - 0.011;
      value2 = -0.1;
      Assert.IsTrue(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsGreaterOrEqual(value1, value2, epsilon));

      // ----- value1 > value2.
      // Default epsilon interval:
      value1 = -0.1;
      value2 = -0.1 - 2 * Numeric.EpsilonD;
      Assert.IsFalse(Numeric.IsLess(value1, value2));
      Assert.IsFalse(Numeric.IsLessOrEqual(value1, value2));
      Assert.IsTrue(Numeric.IsGreater(value1, value2));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2));

      // Custom epsilon interval:
      value1 = -0.1;
      value2 = -0.1 - 0.011;
      Assert.IsFalse(Numeric.IsLess(value1, value2, epsilon));
      Assert.IsFalse(Numeric.IsLessOrEqual(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreater(value1, value2, epsilon));
      Assert.IsTrue(Numeric.IsGreaterOrEqual(value1, value2, epsilon));
    }


    [Test]
    public void IsNaN()
    {
      Assert.IsFalse(Numeric.IsNaN(0f));
      Assert.IsFalse(Numeric.IsNaN(1f));
      Assert.IsFalse(Numeric.IsNaN(float.MaxValue));
      Assert.IsFalse(Numeric.IsNaN(float.MinValue));
      Assert.IsFalse(Numeric.IsNaN(float.NegativeInfinity));
      Assert.IsFalse(Numeric.IsNaN(float.PositiveInfinity));
      Assert.IsTrue(Numeric.IsNaN(float.NaN));

      Assert.IsFalse(Numeric.IsNaN(0f));
      Assert.IsFalse(Numeric.IsNaN(1f));
      Assert.IsFalse(Numeric.IsNaN(double.MaxValue));
      Assert.IsFalse(Numeric.IsNaN(double.MinValue));
      Assert.IsFalse(Numeric.IsNaN(double.NegativeInfinity));
      Assert.IsFalse(Numeric.IsNaN(double.PositiveInfinity));
      Assert.IsTrue(Numeric.IsNaN(double.NaN));
    }


    [Test]
    public void GetSignificantBitsUnsigned()
    {
      Assert.AreEqual(0, Numeric.GetSignificantBitsUnsigned(0.0f, 9));
      Assert.AreEqual(240, Numeric.GetSignificantBitsUnsigned(0.01f, 9));
      Assert.AreEqual(247, Numeric.GetSignificantBitsUnsigned(0.1f, 9));
      Assert.AreEqual(254, Numeric.GetSignificantBitsUnsigned(1.0f, 9));
      Assert.AreEqual(260, Numeric.GetSignificantBitsUnsigned(10.0f, 9));
      Assert.AreEqual(267, Numeric.GetSignificantBitsUnsigned(100.0f, 9));
      Assert.AreEqual(273, Numeric.GetSignificantBitsUnsigned(1000.0f, 9));
    }


    [Test]
    public void GetSignificantBitsSigned()
    {
      Assert.AreEqual(244, Numeric.GetSignificantBitsSigned(-100.0f, 10));
      Assert.AreEqual(251, Numeric.GetSignificantBitsSigned(-10.0f, 10));
      Assert.AreEqual(257, Numeric.GetSignificantBitsSigned(-1.0f, 10));
      Assert.AreEqual(264, Numeric.GetSignificantBitsSigned(-0.1f, 10));
      Assert.AreEqual(271, Numeric.GetSignificantBitsSigned(-0.01f, 10));
      Assert.AreEqual(512, Numeric.GetSignificantBitsSigned(0.0f, 10));
      Assert.AreEqual(752, Numeric.GetSignificantBitsSigned(0.01f, 10));
      Assert.AreEqual(759, Numeric.GetSignificantBitsSigned(0.1f, 10));
      Assert.AreEqual(766, Numeric.GetSignificantBitsSigned(1.0f, 10));
      Assert.AreEqual(772, Numeric.GetSignificantBitsSigned(10.0f, 10));
      Assert.AreEqual(779, Numeric.GetSignificantBitsSigned(100.0f, 10));
    }
  }
}
