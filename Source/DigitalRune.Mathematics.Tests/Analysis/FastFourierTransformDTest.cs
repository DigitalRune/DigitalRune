using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Analysis.Tests
{
  [TestFixture]
  public class FastFourierTransformDTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorShouldThrow()
    {
      new FastFourierTransformD(-1);
    }


    [Test]
    public void Transform1DArguments()
    {
      Assert.Throws<ArgumentNullException>(() => FastFourierTransformD.Transform1D(null, true));
      Assert.Throws<ArgumentNullException>(() => FastFourierTransformD.Transform1D(null, 2, true));

      Assert.Throws<ArgumentException>(() => FastFourierTransformD.Transform1D(new Vector2D[0], true));
      Assert.Throws<ArgumentException>(() => FastFourierTransformD.Transform1D(new Vector2D[13], true));

      FastFourierTransformD.Transform1D(new Vector2D[1], true);

      // This is ok.
      FastFourierTransformD.Transform1D(new Vector2D[2], true);
      FastFourierTransformD.Transform1D(new Vector2D[16], false);
    }


    [Test]
    public void Transform1D()
    {
      // Transform forward and inverse and compare with initial values.
      var random = new Random(1234567);

      var s = new Vector2D[16];
      var t = new Vector2D[16];
      for (int i = 0; i < s.Length; i++)
      {
        s[i] = random.NextVector2D(-10, 10);
        t[i] = s[i];
      }

      FastFourierTransformD.Transform1D(t, true);
      FastFourierTransformD.Transform1D(t, false);

      for (int i = 0; i < s.Length; i++)
        Assert.IsTrue(Vector2D.AreNumericallyEqual(s[i], t[i]));
    }


    [Test]
    public void Transform2DArguments()
    {
      var fft = new FastFourierTransformD(16);

      Assert.Throws<ArgumentNullException>(() => fft.Transform2D(null, true));

      // Senseless.
      Assert.Throws<ArgumentException>(() => fft.Transform2D(new Vector2D[0, 0], true));

      // Not POT.
      Assert.Throws<ArgumentException>(() => fft.Transform2D(new Vector2D[13, 2], true));
      Assert.Throws<ArgumentException>(() => fft.Transform2D(new Vector2D[2, 6], true));

      // Too large for capacity.
      Assert.Throws<ArgumentException>(() => fft.Transform2D(new Vector2D[32, 6], true));

      fft.Transform2D(new Vector2D[1, 1], true);

      // This is ok.
      fft.Transform2D(new Vector2D[2, 4], true);
      fft.Transform2D(new Vector2D[16, 1], false);
    }


    [Test]
    public void Transform1DAs2DColumn()
    {
      // Result of 2D with one row/column must be the same as 1D.
      var fft = new FastFourierTransformD(16);

      var random = new Random(1234567);

      var s1D = new Vector2D[16];
      var s2D = new Vector2D[16, 1];

      for (int i = 0; i < s1D.Length; i++)
      {
        s1D[i] = random.NextVector2D(-10, 10);
        s2D[i, 0] = s1D[i];
      }

      FastFourierTransformD.Transform1D(s1D, true);
      fft.Transform2D(s2D, true);

      for (int i = 0; i < s1D.Length; i++)
        Assert.AreEqual(s1D[i], s2D[i, 0]);
    }


    [Test]
    public void Transform1DAs2DRow()
    {
      // Result of 2D with one row/column must be the same as 1D.
      var fft = new FastFourierTransformD(16);

      var random = new Random(1234567);

      var s1D = new Vector2D[8];
      var s2D = new Vector2D[1, 8];

      for (int i = 0; i < s1D.Length; i++)
      {
        s1D[i] = random.NextVector2D(-10, 10);
        s2D[0, i] = s1D[i];
      }

      FastFourierTransformD.Transform1D(s1D, true);
      fft.Transform2D(s2D, true);

      for (int i = 0; i < s1D.Length; i++)
        Assert.AreEqual(s1D[i], s2D[0, i]);
    }


    [Test]
    public void Transform2D()
    {
      // Transform forward and inverse and compare with initial values.
      var random = new Random(1234567);

      var s = new Vector2D[16, 8];
      var t = new Vector2D[16, 8];
      for (int i = 0; i < s.GetLength(0); i++)
      {
        for (int j = 0; j < s.GetLength(1); j++)
        {
          s[i, j] = random.NextVector2D(-10, 10);
          t[i, j] = s[i, j];
        }
      }

      var fft = new FastFourierTransformD(16);
      fft.Transform2D(t, true);

      Assert.IsFalse(Vector2D.AreNumericallyEqual(s[0, 0], t[0, 0]));

      fft.Transform2D(t, false);

      for (int i = 0; i < s.GetLength(0); i++)
        for (int j = 0; j < s.GetLength(1); j++)
          Assert.IsTrue(Vector2D.AreNumericallyEqual(s[i, j], t[i, j]));
    }
  }
}
