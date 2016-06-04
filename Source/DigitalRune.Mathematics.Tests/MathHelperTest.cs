using System;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Tests
{
  [TestFixture]
  public class MathHelperTest
  {       
    [Test]
    public void TestSwap()
    {
      double x = 10.0;
      double y = -12.0;
      MathHelper.Swap(ref x, ref y);
      Assert.AreEqual(-12.0, x);
      Assert.AreEqual(10.0, y);

      string a = "A";
      string b = "B";
      MathHelper.Swap(ref a, ref b);
      Assert.AreEqual("B", a);
      Assert.AreEqual("A", b);
    }



    [Test]
    public void Clamp()
    {
      Assert.AreEqual(0, MathHelper.Clamp(0, 0, 0));
      Assert.AreEqual(0, MathHelper.Clamp(-1, 0, 0));
      Assert.AreEqual(0, MathHelper.Clamp(1, 0, 0));
      Assert.AreEqual(-33.04, MathHelper.Clamp(-33.04, -35.00, 0.0));
      Assert.AreEqual(-33.0, MathHelper.Clamp(-33.04, -33.0, 0.0));
      Assert.AreEqual(100.0, MathHelper.Clamp(+111.334, -100.0, 100.0));
      Assert.AreEqual(0.9f, MathHelper.Clamp(0.9f, 0.7f, 0.91f));
      Assert.AreEqual(0.9f, MathHelper.Clamp(0.9f, 0.9f, 0.9f));

      // Test swapped min and max.
      Assert.AreEqual(5, MathHelper.Clamp(6, 5, 4));
      Assert.AreEqual(4, MathHelper.Clamp(3, 5, 4));
      Assert.AreEqual(4.5f, MathHelper.Clamp(4.5f, 5, 4));
    }



    [Test]
    public void Hypotenuse()
    {
      Assert.AreEqual(5f, MathHelper.Hypotenuse(3f, 4f));
      Assert.AreEqual(5f, MathHelper.Hypotenuse(4f, 3f));
      Assert.AreEqual(3f, MathHelper.Hypotenuse(0f, 3f));
      Assert.AreEqual(3f, MathHelper.Hypotenuse(3f, 0f));

      Assert.AreEqual(5d, MathHelper.Hypotenuse(4d, 3d));
      Assert.AreEqual(5d, MathHelper.Hypotenuse(3d, 4d));
      Assert.AreEqual(3d, MathHelper.Hypotenuse(0d, 3d));
      Assert.AreEqual(3d, MathHelper.Hypotenuse(3d, 0d));
    }    


    [Test]
    public void DegreeToRadian()
    {
      Assert.AreEqual(0.0f, MathHelper.ToRadians(0f));      
      Assert.AreEqual(2f * ConstantsF.Pi, MathHelper.ToRadians(360f));
      Assert.AreEqual(-2f* ConstantsF.Pi, MathHelper.ToRadians(-360f));
      Assert.AreEqual(ConstantsF.Pi, MathHelper.ToRadians(180f));
      Assert.AreEqual(-ConstantsF.Pi, MathHelper.ToRadians(-180f));

      Assert.AreEqual(0.0, MathHelper.ToRadians(0d));
      Assert.AreEqual(2.0 * Math.PI, MathHelper.ToRadians(360d));
      Assert.AreEqual(-2.0 * Math.PI, MathHelper.ToRadians(-360d));
      Assert.AreEqual(Math.PI, MathHelper.ToRadians(180d));
      Assert.AreEqual(-Math.PI, MathHelper.ToRadians(-180d));
    }

    [Test]
    public void RadianToDegree()
    {
      Assert.AreEqual(0.0f, MathHelper.ToDegrees(0f));
      Assert.AreEqual(360.0f, MathHelper.ToDegrees(ConstantsF.TwoPi));
      Assert.AreEqual(-360.0f, MathHelper.ToDegrees(-ConstantsF.TwoPi));
      Assert.IsTrue(Numeric.AreEqual(180.0f, MathHelper.ToDegrees(ConstantsF.Pi)));
      Assert.IsTrue(Numeric.AreEqual(-180.0f, MathHelper.ToDegrees(-ConstantsF.Pi)));

      Assert.AreEqual(0.0, MathHelper.ToDegrees(0d));
      Assert.AreEqual(360.0, MathHelper.ToDegrees(2 * Math.PI));
      Assert.AreEqual(-360.0, MathHelper.ToDegrees(-2 * Math.PI));
      Assert.AreEqual(180.0, MathHelper.ToDegrees(Math.PI));
      Assert.AreEqual(-180.0, MathHelper.ToDegrees(-Math.PI));
    }


    [Test]
    public void Bitmask()
    {
      Assert.AreEqual(0, MathHelper.Bitmask(0));
      Assert.AreEqual(1, MathHelper.Bitmask(1));
      Assert.AreEqual(3, MathHelper.Bitmask(2));
      Assert.AreEqual(3, MathHelper.Bitmask(3));
      Assert.AreEqual(7, MathHelper.Bitmask(4));
      Assert.AreEqual((1 << 19) - 1, MathHelper.Bitmask((1 << 19) - 1));
      Assert.AreEqual((1 << 20) - 1, MathHelper.Bitmask(1 << 19));
      Assert.AreEqual(uint.MaxValue, MathHelper.Bitmask(uint.MaxValue - 1));
      Assert.AreEqual(uint.MaxValue, MathHelper.Bitmask(uint.MaxValue));
    }


    [Test]
    public void Log2LessOrEqual()
    {
      Assert.AreEqual(0, MathHelper.Log2LessOrEqual(0));
      Assert.AreEqual(0, MathHelper.Log2LessOrEqual(1));
      Assert.AreEqual(1, MathHelper.Log2LessOrEqual(2));
      Assert.AreEqual(1, MathHelper.Log2LessOrEqual(3));
      Assert.AreEqual(2, MathHelper.Log2LessOrEqual(4));
      Assert.AreEqual(2, MathHelper.Log2LessOrEqual(7));
      Assert.AreEqual(3, MathHelper.Log2LessOrEqual(8));

      Assert.AreEqual(18, MathHelper.Log2LessOrEqual((1 << 19) - 1));
      Assert.AreEqual(19, MathHelper.Log2LessOrEqual(1 << 19));
      Assert.AreEqual(19, MathHelper.Log2LessOrEqual((1 << 19) + 1));
      Assert.AreEqual(31, MathHelper.Log2LessOrEqual(uint.MaxValue - 1));
      Assert.AreEqual(31, MathHelper.Log2LessOrEqual(uint.MaxValue));
    }


    [Test]
    public void Log2GreaterOrEqual()
    {
      Assert.AreEqual(0, MathHelper.Log2GreaterOrEqual(0));
      Assert.AreEqual(0, MathHelper.Log2GreaterOrEqual(1));
      Assert.AreEqual(1, MathHelper.Log2GreaterOrEqual(2));
      Assert.AreEqual(2, MathHelper.Log2GreaterOrEqual(3));
      Assert.AreEqual(2, MathHelper.Log2GreaterOrEqual(4));
      Assert.AreEqual(3, MathHelper.Log2GreaterOrEqual(7));
      Assert.AreEqual(3, MathHelper.Log2GreaterOrEqual(8));

      Assert.AreEqual(8, MathHelper.Log2GreaterOrEqual((1 << 8) - 1));
      Assert.AreEqual(8, MathHelper.Log2GreaterOrEqual(1 << 8));
      Assert.AreEqual(9, MathHelper.Log2GreaterOrEqual((1 << 8) + 1));

      Assert.AreEqual(19, MathHelper.Log2GreaterOrEqual((1 << 19) - 1));
      Assert.AreEqual(19, MathHelper.Log2GreaterOrEqual(1 << 19));
      Assert.AreEqual(20, MathHelper.Log2GreaterOrEqual((1 << 19) + 1));
      Assert.AreEqual(32, MathHelper.Log2GreaterOrEqual(uint.MaxValue - 1));
      Assert.AreEqual(32, MathHelper.Log2GreaterOrEqual(uint.MaxValue));
    }


    [Test]
    public void GaussianF()
    {
      Assert.Less(MathHelper.Gaussian(3f, 4, 10, 2), MathHelper.Gaussian(4f, 4, 10, 2));
      Assert.Less(MathHelper.Gaussian(5f, 4, 10, 2), MathHelper.Gaussian(8f, 4, 10, 2));
      Assert.Less(MathHelper.Gaussian(8f, 4, 10, 2), MathHelper.Gaussian(10f, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(10f, 4, 10, 2), MathHelper.Gaussian(11f, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(11f, 4, 10, 2), MathHelper.Gaussian(12f, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(14f, 4, 10, 2), MathHelper.Gaussian(16f, 4, 10, 2));

      Assert.IsTrue(Numeric.AreEqual(MathHelper.Gaussian(8f, 4, 10, 2), MathHelper.Gaussian(12f, 4, 10, 2)));
    }


    [Test]
    public void GaussianD()
    {
      Assert.Less(MathHelper.Gaussian(3d, 4, 10, 2), MathHelper.Gaussian(4d, 4, 10, 2));
      Assert.Less(MathHelper.Gaussian(5d, 4, 10, 2), MathHelper.Gaussian(8d, 4, 10, 2));
      Assert.Less(MathHelper.Gaussian(8d, 4, 10, 2), MathHelper.Gaussian(10d, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(10d, 4, 10, 2), MathHelper.Gaussian(11d, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(11d, 4, 10, 2), MathHelper.Gaussian(12d, 4, 10, 2));
      Assert.Greater(MathHelper.Gaussian(14d, 4, 10, 2), MathHelper.Gaussian(16d, 4, 10, 2));

      Assert.IsTrue(Numeric.AreEqual(MathHelper.Gaussian(8d, 4, 10, 2), MathHelper.Gaussian(12d, 4, 10, 2)));
    }

    [Test]
    public void BinomialCoefficient()
    {
      Assert.AreEqual(66, MathHelper.BinomialCoefficient(12, 2));
      Assert.AreEqual(126, MathHelper.BinomialCoefficient(9, 5));
      Assert.AreEqual(1, MathHelper.BinomialCoefficient(0, 0));
      Assert.AreEqual(0, MathHelper.BinomialCoefficient(0, 1));
      Assert.AreEqual(0, MathHelper.BinomialCoefficient(0, -1));
      Assert.AreEqual(0, MathHelper.BinomialCoefficient(3, -1));
      Assert.AreEqual(0, MathHelper.BinomialCoefficient(3, 4));
    }


    [Test]
    public void IsPowerOf2()
    {
      Assert.IsTrue(MathHelper.IsPowerOf2(1));
      Assert.IsTrue(MathHelper.IsPowerOf2(2));
      Assert.IsTrue(MathHelper.IsPowerOf2(256));
      Assert.IsFalse(MathHelper.IsPowerOf2(-1));
      Assert.IsFalse(MathHelper.IsPowerOf2(0));
      Assert.IsFalse(MathHelper.IsPowerOf2(3));
      Assert.IsFalse(MathHelper.IsPowerOf2(255));
    }


    [Test]
    public void NextPowerOf2()
    {
      Assert.AreEqual(1, MathHelper.NextPowerOf2(0));
      Assert.AreEqual(2, MathHelper.NextPowerOf2(1));
      Assert.AreEqual(4, MathHelper.NextPowerOf2(2));
      Assert.AreEqual(4, MathHelper.NextPowerOf2(3));
      Assert.AreEqual(1024, MathHelper.NextPowerOf2(512));
      Assert.AreEqual(1024, MathHelper.NextPowerOf2(1023));
      Assert.AreEqual(2048, MathHelper.NextPowerOf2(1024));
    }


    //[Test]
    //public void RoundToPowerOf2()
    //{
    //  Assert.AreEqual(0, MathHelper.RoundToPowerOf2(0));
    //  Assert.AreEqual(1, MathHelper.RoundToPowerOf2(1));
    //  Assert.AreEqual(2, MathHelper.RoundToPowerOf2(2));
    //  Assert.AreEqual(4, MathHelper.RoundToPowerOf2(3));
    //  Assert.AreEqual(32, MathHelper.RoundToPowerOf2(32));
    //  Assert.AreEqual(32, MathHelper.RoundToPowerOf2(47));
    //  Assert.AreEqual(64, MathHelper.RoundToPowerOf2(48));
    //  Assert.AreEqual(64, MathHelper.RoundToPowerOf2(64));
    //  Assert.AreEqual(512, MathHelper.RoundToPowerOf2(512));
    //  Assert.AreEqual(1024, MathHelper.RoundToPowerOf2(1023));
    //  Assert.AreEqual(1023, MathHelper.RoundToPowerOf2(1024));
    //}
  }
}
