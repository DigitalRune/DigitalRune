// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Text;
using DigitalRune.Mathematics;


namespace DigitalRune.Diagnostics
{
  /// <summary>
  /// Provides methods to format output of profilers.
  /// </summary>
  internal static class ProfileFormatter
  {
    // The Append methods add the value to the string builder and format it for a fixed
    // column count.
    // TODO: Maybe the string.Format() methods provide a way to to this more elegantly.

    // Add name to stringBuilder and add blanks as required.
    public static void Append(StringBuilder stringBuilder, string name)
    {
      stringBuilder.Append(name);
      int numberOfMissingBlanks = 15 - name.Length;
      if (numberOfMissingBlanks > 0)
        stringBuilder.Append(' ', numberOfMissingBlanks);
    }


    // Add value to stringBuilder and add blanks as required.
    public static void Append(StringBuilder stringBuilder, int value, IFormatProvider formatProvider)
    {
      string valueText = Numeric.IsNaN(value) ? "         -" : value.ToString(formatProvider);

      int numberOfMissingBlanks = 8 - valueText.Length;
      if (numberOfMissingBlanks > 0)
        stringBuilder.Append(' ', numberOfMissingBlanks);

      stringBuilder.Append(valueText);
    }


    // Add value to stringBuilder and add blanks as required.
    public static void Append(StringBuilder stringBuilder, double value, IFormatProvider formatProvider)
    {
      string valueText = Numeric.IsNaN(value) ? "         -" : value.ToString("F3", formatProvider);

      int numberOfMissingBlanks = 10 - valueText.Length;
      if (numberOfMissingBlanks > 0)
        stringBuilder.Append(' ', numberOfMissingBlanks);

      stringBuilder.Append(valueText);
    }
  }
}
