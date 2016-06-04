#region ----- Copyright -----
/*
   The class in this file is based on the StringBuilderExtension class from the Performance 
   Measuring Sample which is licensed under the terms of the Microsoft Permissive License (Ms-PL).
   See http://create.msdn.com/en-US/education/catalog/sample/performance_sample

    Microsoft Permissive License (Ms-PL)

    This license governs use of the accompanying software. If you use the software, you accept this 
    license. If you do not accept the license, do not use the software.

    1. Definitions
    The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same 
    meaning here as under U.S. copyright law.
    A “contribution” is the original software, or any additions or changes to the software.
    A “contributor” is any person that distributes its contribution under this license.
    “Licensed patents” are a contributor’s patent claims that read directly on its contribution.

    2. Grant of Rights
    (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
        limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
        copyright license to reproduce its contribution, prepare derivative works of its contribution, 
        and distribute its contribution or any derivative works that you create.
    (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
        limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
        license under its licensed patents to make, have made, use, sell, offer for sale, import, 
        and/or otherwise dispose of its contribution in the software or derivative works of the 
        contribution in the software.

    3. Conditions and Limitations
    (A) No Trademark License- This license does not grant you rights to use any contributors’ name, 
        logo, or trademarks.
    (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
        by the software, your patent license from such contributor to the software ends automatically.
    (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, 
        and attribution notices that are present in the software.
    (D) If you distribute any portion of the software in source code form, you may do so only under 
        this license by including a complete copy of this license with your distribution. If you 
        distribute any portion of the software in compiled or object code form, you may only do so 
        under a license that complies with this license.
    (E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no 
        express warranties, guarantees or conditions. You may have additional consumer rights under 
        your local laws which this license cannot change. To the extent permitted under your local 
        laws, the contributors exclude the implied warranties of merchantability, fitness for a 
        particular purpose and non-infringement. 
*/
#endregion

#region Original File Description
//-----------------------------------------------------------------------------
// StringBuilderExtensions.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion


using System;
using System.Globalization;
using System.Text;


namespace DigitalRune.Text
{
  /// <summary>
  /// Options for <see cref="StringBuilder"/> extension methods.
  /// </summary>
  [Flags]
  public enum AppendNumberOptions
  {
    ///<summary>
    /// Normal format.
    ///</summary>
    None = 0,

    ///<summary>
    /// Added "+" sign for positive value.
    ///</summary>
    PositiveSign = 1,

    ///<summary>
    /// Insert number group separation characters. In use, added "," for every 3 digits.
    ///</summary>
    NumberGroup = 2,
  }


  /// <summary>
  /// Static class for string builder extension methods.
  /// </summary>
  /// <remarks>
  /// <para>
  /// You can use a <see cref="StringBuilder"/> to avoid unwanted memory allocations. But there are 
  /// still problems for adding numerical values to <see cref="StringBuilder"/>. One of them is 
  /// boxing that occurs when you use the <see cref="StringBuilder.AppendFormat(string,object[])"/> 
  /// method. Another issue is memory allocation that occurs when you specify int or float for the 
  /// <see cref="StringBuilder.Append(float)"/> method.
  /// </para>
  /// <para>
  /// This class provides solution for those issue. All methods are defined as extension methods for 
  /// <see cref="StringBuilder"/>. So, you can use those methods like below.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following sample demonstrates how to append a number to a <see cref="StringBuilder"/> 
  /// using one of the extension methods defined in this class.
  /// <code lang="csharp">
  /// <![CDATA[
  /// stringBuilder.AppendNumber(12345);
  /// ]]>
  /// </code>
  /// </example>
  public static class StringBuilderExtensions
  {
    /// <summary>
    /// Cache for NumberGroupSizes of NumberFormat class.
    /// </summary>
    private static readonly int[] NumberGroupSizes = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes;


    /// <summary>
    /// String conversion buffer.
    /// </summary>
    private static readonly char[] NumberString = new char[32];


    /// <summary>
    /// Appends a copy of a string builder to the end of the string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="text">
    /// The text that is appended to the end of <paramref name="builder"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> or <paramref name="text"/> is <see langword="null"/>.
    /// </exception>
    public static void Append(this StringBuilder builder, StringBuilder text)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");
      if (text == null)
        throw new ArgumentNullException("text");

      var length = text.Length;
      for (int i = 0; i < length; i++)
        builder.Append(text[i]);
    }


    /// <overloads>
    /// <summary>
    /// Converts a number to a string and adds it to the string builder.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts an integer number to a string and adds it to string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="number">The number.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static void AppendNumber(this StringBuilder builder, int number)
    {
      AppendNumberInternal(builder, number, 0, AppendNumberOptions.None);
    }


    /// <summary>
    /// Converts an integer number to a string and adds it to the string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="number">The number.</param>
    /// <param name="options">The format options.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static void AppendNumber(this StringBuilder builder, int number, AppendNumberOptions options)
    {
      AppendNumberInternal(builder, number, 0, options);
    }


    /// <summary>
    /// Converts a <see langword="float"/> number to a string and adds it to the string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="number">The number.</param>
    /// <remarks>It shows 2 decimal digits.</remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static void AppendNumber(this StringBuilder builder, float number)
    {
      AppendNumber(builder, number, 2, AppendNumberOptions.None);
    }


    /// <summary>
    /// Converts a <see langword="float"/> number to a string and adds it to the string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="number">The number.</param>
    /// <param name="options">The format options.</param>
    /// <remarks>It shows 2 decimal digits.</remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static void AppendNumber(this StringBuilder builder, float number, AppendNumberOptions options)
    {
      AppendNumber(builder, number, 2, options);
    }


    /// <summary>
    /// Converts a <see langword="float"/> number to a string and adds it to the string builder.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <param name="number">The number.</param>
    /// <param name="decimalCount">The number of decimal digits to show.</param>
    /// <param name="options">The format options.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static void AppendNumber(this StringBuilder builder, float number, int decimalCount, AppendNumberOptions options)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

      // Handle NaN, Infinity cases.
      if (float.IsNaN(number))
      {
        builder.Append("NaN");
      }
      else if (float.IsNegativeInfinity(number))
      {
        builder.Append("-Infinity");
      }
      else if (float.IsPositiveInfinity(number))
      {
        builder.Append("+Infinity");
      }
      else
      {
        // [FIX, DigitalRune]: The following check was added to catch overflows.
        // The original code does not handle the full range of float values. The variable type
        // in the following code was changed from int to long to extend the range. However,
        // if the value is still out of range, we fall back to the original Append() method (and 
        // accept some garbage).
        try
        {
          // Convert the float value to a long value. (Checking for possible overflows.)
          long longNumber;
          checked
          {
            longNumber = (long)(number * Math.Pow(10, decimalCount) + 0.5);
          }

          AppendNumberInternal(builder, longNumber, decimalCount, options);
        }
        catch
        {
          builder.Append(number);
        }
      }
    }


    private static void AppendNumberInternal(StringBuilder builder, long number, int decimalCount, AppendNumberOptions options)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

      // Initialize variables for conversion.
      NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;

      int idx = NumberString.Length;
      int decimalPos = idx - decimalCount;

      if (decimalPos == idx)
        decimalPos = idx + 1;

      int numberGroupIdx = 0;
      int numberGroupCount = NumberGroupSizes[numberGroupIdx] + decimalCount;

      bool showNumberGroup = (options & AppendNumberOptions.NumberGroup) != 0;
      bool showPositiveSign = (options & AppendNumberOptions.PositiveSign) != 0;

      bool isNegative = number < 0;
      number = Math.Abs(number);

      // Converting from smallest digit.
      do
      {
        // Add decimal separator ("." in US).
        if (idx == decimalPos)
        {
          NumberString[--idx] = nfi.NumberDecimalSeparator[0];
        }

        // Added number group separator ("," in US).
        if (--numberGroupCount < 0 && showNumberGroup)
        {
          NumberString[--idx] = nfi.NumberGroupSeparator[0];

          if (numberGroupIdx < NumberGroupSizes.Length - 1)
            numberGroupIdx++;

          numberGroupCount = NumberGroupSizes[numberGroupIdx] - 1;
        }

        // Convert current digit to character and add to buffer.
        NumberString[--idx] = (char)('0' + (number % 10));
        number /= 10;
      } while (number > 0 || decimalPos <= idx);


      // Added sign character if needed.
      if (isNegative)
      {
        NumberString[--idx] = nfi.NegativeSign[0];
      }
      else if (showPositiveSign)
      {
        NumberString[--idx] = nfi.PositiveSign[0];
      }

      // Added converted string to StringBuilder.
      builder.Append(NumberString, idx, NumberString.Length - idx);
    }


#if SILVERLIGHT || WP7 || XBOX
    /// <summary>
    /// Removes all characters from the <see cref="StringBuilder"/> instance.
    /// </summary>
    /// <param name="builder">The string builder.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static void Clear(this StringBuilder builder)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

    builder.Remove(0, builder.Length);
    }
#endif
  }
}
