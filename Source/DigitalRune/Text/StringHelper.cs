// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Text
{
  /// <summary>
  /// Contains additional string methods.
  /// </summary>
  public static class StringHelper
  {
    /// <summary>
    /// Splits a string that has an integer number as suffix (e.g. "text123") into its components.
    /// </summary>
    /// <param name="str">
    /// A string that has an positive integer number as suffix. The suffix is optional. Examples:
    /// "text", "text123".
    /// </param>
    /// <param name="text">
    /// The text before the number suffix. "" if <paramref name="str"/> is <see langword="null"/> or
    /// empty.
    /// </param>
    /// <param name="number">
    /// The positive integer number. -1 if <paramref name="str"/> does not contain a suffix.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method should only be used if <paramref name="str"/> contains only letters and digits.
    /// If <paramref name="str"/> contains characters other than letters and digits the result is
    /// undefined.
    /// </para>
    /// <para>
    /// Here is a list of examples:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Input</term>
    /// <description>Output</description>
    /// </listheader>
    /// <item>
    /// <term><see langword="null"/></term>
    /// <description>"", -1</description>
    /// </item>
    /// <item>
    /// <term>""</term>
    /// <description>"", -1</description>
    /// </item>
    /// <item>
    /// <term>"text"</term>
    /// <description>"text", -1</description>
    /// </item>
    /// <item>
    /// <term>"123"</term>
    /// <description>"", 123</description>
    /// </item>
    /// <item>
    /// <term>"text123"</term>
    /// <description>"text", 123</description>
    /// </item>
    /// <item>
    /// <term>"123text"</term>
    /// <description>"123text", -1</description>
    /// </item>
    /// <item>
    /// <term>"123text456"</term>
    /// <description>"123text", 456</description>
    /// </item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void SplitTextAndNumber(this string str, out string text, out int number)
    {
      if (String.IsNullOrEmpty(str))
      {
        text = String.Empty;
        number = -1;
      }
      else if (!Char.IsDigit(str, str.Length - 1))
      {
        text = str;
        number = -1;
      }
      else
      {
        // Separate text and number:
        int i = str.Length - 1;
        while (i > 0 && !Char.IsLetter(str[i - 1]))
          i--;

        number = Int32.Parse(str.Substring(i), CultureInfo.InvariantCulture);
        text = str.Substring(0, i);
      }
    }



    /// <summary>
    /// Computes a value that indicates the match between two strings.
    /// </summary>
    /// <param name="textA">The first string.</param>
    /// <param name="textB">The second string.</param>
    /// <returns>
    /// If the strings have nothing in common 0 is returned. If the strings are identical 1 is
    /// returned. Otherwise a value between 0 and 1 is returned that indicates how similar the
    /// strings are.
    /// </returns>
    /// <remarks>
    /// This method can be used to perform a fuzzy search among keywords.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textB"/> is <see langword="null"/>.
    /// </exception>
    public static float ComputeMatch(string textA, string textB)
    {
      if (textA == null)
        throw new ArgumentNullException("textA");
      if (textB == null)
        throw new ArgumentNullException("textB");

      // See Game Programming Gems 6, "Closest-String Matching Algorithm"

      int maxLength = Math.Max(textA.Length, textB.Length);
      int indexA = 0;
      int indexB = 0;
      float result = 0;

      // Iterate through the left string until we run out of room
      while (indexA < textA.Length && indexB < textB.Length)
      {
        // First, check for a simple left/right match
        if (textA[indexA] == textB[indexB])
        {
          // ----- A simple match:
          // Add a proportional character's value to the match total.
          result += 1.0f / maxLength;
          // Advance both indices.
          indexA++;
          indexB++;
        }
        else if (char.ToUpperInvariant(textA[indexA]) == char.ToUpperInvariant(textB[indexB]))
        {
          // ----- A simple match when upper/lower case is ignored.
          // We'll consider a capitalization mismatch worth 90% 
          // of a normal match
          result += 0.9f / maxLength;
          // Advance both indices.
          indexA++;
          indexB++;
        }
        else
        {
          // ----- Find the next best match position.
          int indexABest = textA.Length;
          int indexBBest = textB.Length;

          int bestCount = int.MaxValue;
          int countA = 0;
          int countB = 0;

          // Here we loop through word A in an outer loop, but we also check for an 
          // early out by ensuring we don't exceed our best current count.
          for (int i = indexA; i < textA.Length && (countA + countB < bestCount); i++)
          {
            // Inner loop counting
            for (int j = indexB; j < textB.Length && (countA + countB < bestCount); j++)
            {
              // At this point, we don't care about case
              if (char.ToUpperInvariant(textA[i]) == char.ToUpperInvariant(textB[j]))
              {
                // This is the fitness measurement
                int totalCount = countA + countB;
                if (totalCount < bestCount)
                {
                  bestCount = totalCount;
                  indexABest = i;
                  indexBBest = j;
                }
              }
              countB++;
            }
            countA++;
            countB = 0;
          }
          indexA = indexABest;
          indexB = indexBBest;
        }
      }

      // Clamp to 0 or 1 to remove numerical errors.
      if (result < 0.01f)
        result = 0;
      else if (result > 0.99f)
        result = 1;

      return result;
    }
  }
}
