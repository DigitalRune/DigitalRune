// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Helper methods for handling fonts.
    /// </summary>
    internal static class FontHelper
    {
        // See the Windowsclient WPF FontDialog sample:
        // http://windowsclient.net/downloads/folders/wpfsamples/entry5092.aspx

        /// <summary>
        /// Compares two typefaces to determine their order in a human readable list.
        /// </summary>
        /// <param name="typefaceA">The typeface A.</param>
        /// <param name="typefaceB">The typeface B.</param>
        /// <returns>
        /// -1 if typeface A should be listed before B. 0 if they are equal. +1 otherwise. 
        /// </returns>
        public static int Compare(Typeface typefaceA, Typeface typefaceB)
        {
            // Normal should come first.
            if (typefaceA.Style != typefaceB.Style)
            {
                if (typefaceA.Style == FontStyles.Normal)
                    return -1;
                if (typefaceB.Style == FontStyles.Normal)
                    return 1;
            }

            // Then sort by weight.
            int weightDifference = typefaceA.Weight.ToOpenTypeWeight() - typefaceB.Weight.ToOpenTypeWeight();
            if (weightDifference != 0)
                return weightDifference < 0 ? -1 : 1;

            // Then sort by Normal --> Italic --> Oblique.
            if (typefaceA.Style != typefaceB.Style)
            {
                if (typefaceA.Style == FontStyles.Normal)
                    return -1;
                if (typefaceB.Style == FontStyles.Normal)
                    return 1;
                if (typefaceA.Style == FontStyles.Italic)
                    return -1;

                return 1;
            }

            // Then sort by stretch.
            int stretchDifference = typefaceA.Stretch.ToOpenTypeStretch() - typefaceB.Stretch.ToOpenTypeStretch();
            if (stretchDifference != 0)
            {
                if (typefaceA.Stretch == FontStretches.Normal)
                    return -1;
                if (typefaceB.Stretch == FontStretches.Normal)
                    return 1;
            }
            if (stretchDifference < 0)
                return -1;
            if (stretchDifference > 0)
                return 1;

            return 0;
        }

        /// <summary>
        /// Gets the display name for the current UI language.
        /// </summary>
        /// <param name="nameDictionary">The name dictionary.</param>
        /// <returns>The best name for the current language.</returns>
        public static string GetDisplayName(LanguageSpecificStringDictionary nameDictionary)
        {
            // Get the language tag for the current UI culture.
            XmlLanguage userLanguage = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);

            // Search dictionary entry for this language.
            string name;
            if (nameDictionary.TryGetValue(userLanguage, out name))
                return name;

            // No exact match. Make a fuzzy search.
            int bestRelatedness = int.MinValue;
            string bestName = string.Empty;
            foreach (KeyValuePair<XmlLanguage, string> pair in nameDictionary)
            {
                int relatedness = GetRelatedness(pair.Key, userLanguage);
                if (relatedness > bestRelatedness)
                {
                    bestRelatedness = relatedness;
                    bestName = pair.Value;
                }
            }

            return bestName;
        }


        /// <summary>
        /// Measures how related two language tags are.
        /// </summary>
        /// <param name="keyLang">The candidate tag.</param>
        /// <param name="userLang">The desired tag.</param>
        /// <returns></returns>
        private static int GetRelatedness(XmlLanguage keyLang, XmlLanguage userLang)
        {
            try
            {
                // Get equivalent cultures.
                CultureInfo keyCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(keyLang.IetfLanguageTag);
                CultureInfo userCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(userLang.IetfLanguageTag);
                if (!userCulture.IsNeutralCulture)
                    userCulture = userCulture.Parent;

                // If the key is a prefix or parent of the user language it's a good match.
                if (IsLanguageTagPrefixOf(keyLang.IetfLanguageTag, userLang.IetfLanguageTag) || userCulture.Equals(keyCulture))
                    return 2;

                // If the key and user language share a common prefix or parent neutral culture, it's a reasonable match.
                if (IsLanguageTagPrefixOf(TrimLanguageTagSuffix(userLang.IetfLanguageTag), keyLang.IetfLanguageTag) || userCulture.Equals(keyCulture.Parent))
                    return 1;
            }
            catch (ArgumentException)
            {
                // Language tag with no corresponding CultureInfo.
            }

            // Unrelated.
            return 0;
        }


        /// <summary>
        /// Removes the suffix after the last '-'.
        /// </summary>
        /// <param name="tag">The language tag.</param>
        /// <returns>The tag without the last suffix.</returns>
        private static string TrimLanguageTagSuffix(string tag)
        {
            int i = tag.LastIndexOf('-');

            if (i > 0)
                return tag.Substring(0, i);

            return tag;
        }


        /// <summary>
        /// Determines whether the a language tag begins with given prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="tag">The language tag.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="prefix"/> is a prefix of <paramref name="tag"/>; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method works only for language tags. The character after the prefix must be '-'.
        /// Example: Prefix is "zh". This matches "zh-CHT" or "zh-HANT".
        /// </remarks>
        private static bool IsLanguageTagPrefixOf(string prefix, string tag)
        {
            return prefix.Length < tag.Length
                   && tag[prefix.Length] == '-' &&
                   string.CompareOrdinal(prefix, 0, tag, 0, prefix.Length) == 0;
        }


        /// <summary>
        /// Determines whether the font is a symbol font.
        /// </summary>
        /// <param name="fontFamily">The font family.</param>
        /// <returns>
        /// <see langword="true"/> if font is a symbol font; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSymbolFont(FontFamily fontFamily)
        {
            foreach (Typeface typeface in fontFamily.GetTypefaces())
            {
                GlyphTypeface face;
                if (typeface.TryGetGlyphTypeface(out face))
                    return face.Symbol;
            }
            return false;
        }


        #region ----- pt <-> px -----

        private const double PixelsPerPoint = 96.0 / 72.0;
        private const double PointsPerPixel = 72.0 / 96.0;


        /// <summary>
        /// Converts a value given in points (pt) to pixels (px).
        /// </summary>
        /// <param name="points">The value in pt.</param>
        /// <returns>The value in px.</returns>
        public static double PointsToPixels(double points)
        {
            return points * PixelsPerPoint;
        }


        /// <summary>
        /// Converts a value given in pixels (px) to points (pt).
        /// </summary>
        /// <param name="pixels">The value in px.</param>
        /// <returns>The value in pt.</returns>
        public static double PixelsToPoints(double pixels)
        {
            return pixels * PointsPerPixel;
        }
        #endregion
    }
}
