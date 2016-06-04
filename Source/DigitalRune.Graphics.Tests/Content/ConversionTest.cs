using NUnit.Framework;


namespace DigitalRune.Graphics.Content.Tests
{

  [TestFixture]
  public class ConversionTest
  {
    [Test]
    public void Float_SInt_Conversion()
    {
      // Test with 7-bit SInt:
      //   bitmask = 01111111b
      //       min = 01000000b = 40h ... -64
      //       max = 00111111b = 3Fh ... 63
      uint bitmask = 0x7F;

      // Values in range:
      for (int i = -64; i <= 63; i++)
      {
        uint x = (uint)i;
        float f = DataFormatHelper.SIntToFloat(x, bitmask);
        Assert.AreEqual((float)i, f);

        x = DataFormatHelper.FloatToSInt(f, bitmask);
        Assert.AreEqual((uint)i & bitmask, x);
      }

      // Values out of range:
      Assert.AreEqual(0x40, DataFormatHelper.FloatToSInt(-65.0f, bitmask));
      Assert.AreEqual(0x3F, DataFormatHelper.FloatToSInt(64.0f, bitmask));
      Assert.AreEqual(0x40, DataFormatHelper.FloatToSInt(float.NegativeInfinity, bitmask));
      Assert.AreEqual(0x3F, DataFormatHelper.FloatToSInt(float.PositiveInfinity, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToSInt(float.NaN, bitmask));
    }


    [Test]
    public void Float_UInt_Conversion()
    {
      // Test with 7-bit UInt:
      //   bitmask = 01111111b
      //       min = 00000000b = 00h ... 0
      //       max = 01111111b = 7Fh ... 127
      uint bitmask = 0x7F;

      // Values in range:
      for (int i = 0; i <= 127; i++)
      {
        uint x = (uint)i;
        float f = DataFormatHelper.UIntToFloat(x, bitmask);
        Assert.AreEqual((float)i, f);

        x = DataFormatHelper.FloatToUInt(f, bitmask);
        Assert.AreEqual((uint)i & bitmask, x);
      }

      // Values out of range:
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(-1.0f, bitmask));
      Assert.AreEqual(0x7F, DataFormatHelper.FloatToUInt(128.0f, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(float.NegativeInfinity, bitmask));
      Assert.AreEqual(0x7F, DataFormatHelper.FloatToUInt(float.PositiveInfinity, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(float.NaN, bitmask));
    }


    [Test]
    public void Float_SNorm_Conversion()
    {
      // Test with 7-bit SNorm:
      //   bitmask = 01111111b
      //       min = 01000001b = 41h ... -1.0
      //       max = 00111111b = 3Fh ... 1.0
      uint bitmask = 0x7F;

      // Minimum (01000000b = 40h) and second-minimum (01000001b = 41h) map to -1.0.
      Assert.AreEqual(0x41, DataFormatHelper.FloatToSNorm(-1.0f, bitmask));
      Assert.AreEqual(-1.0f, DataFormatHelper.SNormToFloat(0x40, bitmask));
      Assert.AreEqual(-1.0f, DataFormatHelper.SNormToFloat(0x41, bitmask));

      // Values in range:
      for (int i = -63; i <= 63; i++)
      {
        uint x = (uint)i;
        float f = DataFormatHelper.SNormToFloat(x, bitmask);
        Assert.AreEqual(i / 63.0f, f);

        x = DataFormatHelper.FloatToSNorm(f, bitmask);
        Assert.AreEqual((uint)i & bitmask, x);
      }


      // Values out of range:
      Assert.AreEqual(0x41, DataFormatHelper.FloatToSNorm(-2.0f, bitmask));
      Assert.AreEqual(0x3F, DataFormatHelper.FloatToSNorm(2.0f, bitmask));
      Assert.AreEqual(0x41, DataFormatHelper.FloatToSNorm(float.NegativeInfinity, bitmask));
      Assert.AreEqual(0x3F, DataFormatHelper.FloatToSNorm(float.PositiveInfinity, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToSNorm(float.NaN, bitmask));
    }


    [Test]
    public void Float_UNorm_Conversion()
    {
      // Test with 7-bit UNorm:
      //   bitmask = 01111111b
      //       min = 00000000b = 00h ... 0.0
      //       max = 01111111b = 7Fh ... 1.0
      uint bitmask = 0x7F;

      // Values in range:
      for (int i = 0; i <= 127; i++)
      {
        uint x = (uint)i;
        float f = DataFormatHelper.UNormToFloat(x, bitmask);
        Assert.AreEqual(i / 127.0f, f);

        x = DataFormatHelper.FloatToUNorm(f, bitmask);
        Assert.AreEqual((uint)i & bitmask, x);
      }

      // Values out of range:
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(-1.0f, bitmask));
      Assert.AreEqual(0x7F, DataFormatHelper.FloatToUInt(128.0f, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(float.NegativeInfinity, bitmask));
      Assert.AreEqual(0x7F, DataFormatHelper.FloatToUInt(float.PositiveInfinity, bitmask));
      Assert.AreEqual(0x00, DataFormatHelper.FloatToUInt(float.NaN, bitmask));
    }
  }
}
