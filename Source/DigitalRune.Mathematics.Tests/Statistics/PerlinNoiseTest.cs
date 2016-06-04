using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Statistics.Tests
{
  [TestFixture]
  public class PerlinNoiseTest
  {
    [Test]
    public void NoiseAtIntegerPositionsIs0()
    {
      // Test some arbitrary integer points.
      for (float x = 0; x < 255; x += 17f)
      {
        Assert.AreEqual(0, PerlinNoise.Compute(x)); // Zero at integer positions
        Assert.AreEqual(0, PerlinNoise.Compute(x, 14)); // Zero at integer positions

        for (float y = 0; y < 255; y += 15f)
        {
          Assert.AreEqual(0, PerlinNoise.Compute(x, y)); // Zero at integer positions
          Assert.AreEqual(0, PerlinNoise.Compute(x, y, 16, 18)); // Zero at integer positions

          for (float z = 0; z < 255; z += 17f)
          {
            Assert.AreEqual(0, PerlinNoise.Compute(x, y, z)); // Zero at integer positions
            Assert.AreEqual(0, PerlinNoise.Compute(x, y, z, 7, 99, 2055)); // Zero at integer positions
            
            for (float w = 0; w < 255; w += 35f)
            {
              Assert.AreEqual(0, PerlinNoise.Compute(x, y, z, w)); // Zero at integer positions
              Assert.AreEqual(0, PerlinNoise.Compute(x, y, z, w, 100, 14, 4, 234)); // Zero at integer positions
            }
          }
        }
      }
    }


    [Test]
    public void NoiseRange1D()
    {
      double min = double.MaxValue;
      double max = double.MinValue;
      var random = new Random(15485863);
      for (int i = 0; i < 1000000; i++)
      {
        var v = random.NextFloat(0, 255);
        var n = PerlinNoise.Compute(v);
        min = Math.Min(min, n);
        max = Math.Max(max, n);
      }
      Assert.IsTrue(min < 0 && min >= -1);
      Assert.IsTrue(max > 0 && max <= 1);
    }


    [Test]
    public void NoiseRange2D()
    {
      double min = double.MaxValue;
      double max = double.MinValue;
      var random = new Random(15485863);
      for (int i = 0; i < 10000000; i++)
      {
        var v = random.NextVector2F(0, 255);
        var n = PerlinNoise.Compute(v.X, v.Y);
        min = Math.Min(min, n);
        max = Math.Max(max, n);
      }
      Assert.IsTrue(min < 0 && min >= -1);
      Assert.IsTrue(max > 0 && max <= 1);
    }


    [Test]
    public void NoiseRange3D()
    {
      double min = double.MaxValue;
      double max = double.MinValue;
      var random = new Random(15485863);
      for (int i = 0; i < 10000000; i++)
      {
        var v = random.NextVector3F(0, 255);
        var n = PerlinNoise.Compute(v.X, v.Y, v.Z);
        min = Math.Min(min, n);
        max = Math.Max(max, n);
      }
      Assert.IsTrue(min < 0 && min >= -1);
      Assert.IsTrue(max > 0 && max <= 1);
    }


    [Test]
    public void NoiseRange4D()
    {
      double min = double.MaxValue;
      double max = double.MinValue;
      var random = new Random(15485863);
      for (int i = 0; i < 1000000; i++)
      {
        var v = random.NextVector4F(0, 255);
        var n = PerlinNoise.Compute(v.X, v.Y, v.Z, v.W);
        min = Math.Min(min, n);
        max = Math.Max(max, n);
      }
      Assert.IsTrue(min < 0 && min >= -1);
      Assert.IsTrue(max > 0 && max <= 1);
    }


    [Test]
    public void NoiseIsPeriodicWith256()
    {
      Random random = new Random(1234567);
      for (int i = 0; i < 100; i++)
      {
        var v = random.NextVector4D(-1000, 1000);

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X), PerlinNoise.Compute(v.X - 256)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X), PerlinNoise.Compute(v.X + 256)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y), PerlinNoise.Compute(v.X - 256, v.Y - 256)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y), PerlinNoise.Compute(v.X + 256, v.Y + 256)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z), PerlinNoise.Compute(v.X - 256, v.Y - 256, v.Z - 256)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z), PerlinNoise.Compute(v.X + 256, v.Y + 256, v.Z + 256)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, v.W), PerlinNoise.Compute(v.X - 256, v.Y - 256, v.Z - 256, v.W - 256)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, v.W), PerlinNoise.Compute(v.X + 256, v.Y + 256, v.Z + 256, v.W + 256)));
      }
    }


    [Test]
    public void NoiseIsPeriodicWithUserPeriod()
    {
      Random random = new Random(1234567);
      for (int i = 0; i < 100; i++)
      {
        var v = random.NextVector4D(-1000, 1000);

        var randomPeriod = random.NextVector4D(2, 444);
        var px = (int)randomPeriod.X;
        var py = (int)randomPeriod.Y;
        var pz = (int)randomPeriod.Z;
        var pw = (int)randomPeriod.W;

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, px), PerlinNoise.Compute(v.X - px, px)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, px), PerlinNoise.Compute(v.X + px, px)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, px, py), PerlinNoise.Compute(v.X - px, v.Y - py, px, py)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, px, py), PerlinNoise.Compute(v.X + px, v.Y + py, px, py)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, px, py, pz), PerlinNoise.Compute(v.X - px, v.Y - py, v.Z - pz, px, py, pz)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, px, py, pz), PerlinNoise.Compute(v.X + px, v.Y + py, v.Z + pz, px, py, pz)));

        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, v.W, px, py, pz, pw), PerlinNoise.Compute(v.X - px, v.Y - py, v.Z - pz, v.W - pw, px, py, pz, pw)));
        Assert.IsTrue(Numeric.AreEqual(PerlinNoise.Compute(v.X, v.Y, v.Z, v.W, px, py, pz, pw), PerlinNoise.Compute(v.X + px, v.Y + py, v.Z + pz, v.W + pw, px, py, pz, pw)));
      }
    }


    //[Test]
    //public void Noise1()
    //{
    //  Assert.AreEqual(0, PerlinNoise.Noise(0, 0, 0));
    //  Assert.AreEqual(0, PerlinNoise.Noise(1, 2, 3));
    //  Assert.AreNotEqual(0, PerlinNoise.Noise(3.1f, 2, 3));
    //  Assert.AreNotEqual(0, PerlinNoise.Noise(-3.1f, 2, 3));
    //}




    //[Test]
    //public void TestRepetition()
    //{
    //  // Repeats itself after 256 values.
    //  Assert.AreEqual(PerlinNoise.Noise(3.3f, 1.2f, 10.1f, 1), PerlinNoise.Noise(3.3f + 256f, 1.2f, 10.1f, 1));
    //  Assert.AreEqual(PerlinNoise.Noise(3.3f, 1.2f, 10.1f, 3), PerlinNoise.Noise(3.3f, 1.2f + 256f, 10.1f + 256f, 3));
    //}


    //[Test]
    //public void NoiseWithOctaves1()
    //{
    //  Assert.AreEqual(0, PerlinNoise.Noise(0, 0, 0, 3));
    //  Assert.AreEqual(0, PerlinNoise.Noise(1, 2, 3, 3));
    //  Assert.AreNotEqual(0, PerlinNoise.Noise(1.1f, 2, 3, 3));
    //}


    //[Test]
    //[ExpectedException(typeof(ArgumentOutOfRangeException))]
    //public void NoiseWrongParameter1()
    //{
    //  Assert.AreEqual(0, PerlinNoise.Noise(0, 0, 0, 0));
    //}


    //[Test]
    //[ExpectedException(typeof(ArgumentOutOfRangeException))]
    //public void NoiseWrongParameter2()
    //{
    //  Assert.AreEqual(0, PerlinNoise.Noise(0, 0, 0, -1));
    //}
  }  
}
