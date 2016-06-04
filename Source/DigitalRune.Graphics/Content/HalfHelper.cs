// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.InteropServices;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Converts between Half and Float values.
  /// </summary>
  /// <remarks>
  /// Reference: 
  /// </remarks>
  [CLSCompliant(false)]
  internal static class HalfHelper
  {
    // Reference: Jeroen van der Zijp, "Fast Half Float Conversions",
    //            ftp://www.fox-toolkit.org/pub/fasthalffloatconversion.pdf

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [StructLayout(LayoutKind.Explicit)]
    private struct SingleToUInt32
    {
      [FieldOffset(0)]
      public float Single;
      [FieldOffset(0)]
      public uint UInt32;
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Half to Float
    static readonly uint[] MantissaTable = new uint[2048];
    static readonly uint[] ExponentTable = new uint[64];
    static readonly uint[] OffsetTable = new uint[64];

    // Float to Half
    static readonly ushort[] BaseTable = new ushort[512];
    static readonly byte[] ShiftTable = new byte[512];
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="HalfHelper"/> class.
    /// </summary>
    static HalfHelper()
    {
      InitializeHalfToFloatTables();
      InitializeFloatToHalfTables();
    }


    private static void InitializeHalfToFloatTables()
    {
      uint i;

      // Mantissa table
      MantissaTable[0] = 0;
      for (i = 1; i < 1024; i++)
      {
        uint m = i << 13; // Zero pad mantissa bits
        uint e = 0;       // Zero exponent

        while ((m & 0x00800000) == 0) // While not normalized
        {
          e -= 0x00800000;            // Decrement exponent (1<<23)
          m <<= 1;                    // Shift mantissa
        }
        m &= ~0x00800000U;            // Clear leading 1 bit
        e += 0x38800000;              // Adjust bias ((127-14)<<23)
        MantissaTable[i] = m | e;
      }

      for (i = 1024; i < 2048; i++)
        MantissaTable[i] = 0x38000000 + ((i - 1024) << 13);

      // Exponent table
      ExponentTable[0] = 0;
      for (i = 1; i < 31; i++)
        ExponentTable[i] = i << 23;

      ExponentTable[31] = 0x47800000;
      ExponentTable[32] = 0x80000000;

      for (i = 33; i < 63; i++)
        ExponentTable[i] = 0x80000000 + ((i - 32) << 23);

      ExponentTable[63] = 0xC7800000;

      // Offset table
      OffsetTable[0] = 0;
      for (i = 1; i < 64; i++)
        OffsetTable[i] = 1024;

      OffsetTable[32] = 0;
    }


    private static void InitializeFloatToHalfTables()
    {
      // Base and shift table
      for (int i = 0; i < 256; i++)
      {
        int e = i - 127;
        if (e < -24)
        {
          // Very small numbers map to zero
          BaseTable[i | 0x000] = 0x0000;
          BaseTable[i | 0x100] = 0x8000;
          ShiftTable[i | 0x000] = 24;
          ShiftTable[i | 0x100] = 24;
        }
        else if (e < -14)
        {
          // Small numbers map to denorms
          BaseTable[i | 0x000] = (ushort)((0x0400 >> (-e - 14)));
          BaseTable[i | 0x100] = (ushort)((0x0400 >> (-e - 14)) | 0x8000);
          ShiftTable[i | 0x000] = (byte)(-e - 1);
          ShiftTable[i | 0x100] = (byte)(-e - 1);
        }
        else if (e <= 15)
        {
          // Normal numbers just lose precision
          BaseTable[i | 0x000] = (ushort)(((e + 15) << 10));
          BaseTable[i | 0x100] = (ushort)(((e + 15) << 10) | 0x8000);
          ShiftTable[i | 0x000] = 13;
          ShiftTable[i | 0x100] = 13;
        }
        else if (e < 128)
        {
          // Large numbers map to Infinity
          BaseTable[i | 0x000] = 0x7C00;
          BaseTable[i | 0x100] = 0xFC00;
          ShiftTable[i | 0x000] = 24;
          ShiftTable[i | 0x100] = 24;
        }
        else
        {
          // Infinity and NaN's stay Infinity and NaN's
          BaseTable[i | 0x000] = 0x7C00;
          BaseTable[i | 0x100] = 0xFC00;
          ShiftTable[i | 0x000] = 13;
          ShiftTable[i | 0x100] = 13;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the specified 16-bit Half value to a 32-bit Float value.
    /// </summary>
    /// <param name="h">The 16-bit Half value.</param>
    /// <returns>The 32-bit Float value.</returns>
    public static float Unpack(ushort h)
    {
      return ToSingle(MantissaTable[OffsetTable[h >> 10] + (((uint)h) & 0x3ff)] + ExponentTable[h >> 10]);
    }


    /// <summary>
    /// Converts the specified 32-bit Float value to a 16-bit Half value.
    /// </summary>
    /// <param name="f">The 32-bit Float value.</param>
    /// <returns>The 16-bit Half value.</returns>
    public static ushort Pack(float f)
    {
      uint value = ToUint32(f);
      return (ushort)(BaseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> ShiftTable[(value >> 23) & 0x1ff]));
    }


    private static uint ToUint32(float value)
    {
      var t = new SingleToUInt32 { Single = value };
      return t.UInt32;
    }


    private static float ToSingle(uint value)
    {
      var t = new SingleToUInt32 { UInt32 = value };
      return t.Single;
    }
    #endregion
  }
}

