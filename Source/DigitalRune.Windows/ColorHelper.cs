// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Helper functions for <see cref="Color"/>s.
    /// </summary>
    public static partial class ColorHelper
    {
        /// <summary>
        /// Converts the specified floating-point value [0, 1] to a byte value.
        /// </summary>
        /// <param name="value">The floating-point value in the range [0, 1].</param>
        /// <returns>The (rounded) byte value.</returns>
        private static byte ToByte(double value)
        {
            Debug.Assert(0 <= value && value <= 1, "Value in the range [0, 1] expected.");
            return (byte)(value * 255.0 + 0.5);
        }


        /// <summary>
        /// Converts the specified byte value to a floating-point value.
        /// </summary>
        /// <param name="value">The byte value.</param>
        /// <returns>The floating-point value.</returns>
        private static double ToDouble(byte value)
        {
            return value / 255.0;
        }


        #region ----- HSV, HSL -----

        /// <summary>
        /// Creates a <see cref="Color"/> from HSV values.
        /// </summary>
        /// <param name="h">The hue [0, 360[.</param>
        /// <param name="s">The saturation [0, 100].</param>
        /// <param name="v">The value [0, 100].</param>
        /// <returns>The <see cref="Color"/> instance for this HSV color.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static Color FromHsv(double h, double s, double v)
        {
            // See http://de.wikipedia.org/wiki/HSV-Farbraum

            double r, g, b;

            v /= 100.0;
            s /= 100.0;

            if (s > 0)
            {
                if (h == 360)
                    h = 0;

                double hi = Math.Floor(h / 60);
                double f = h / 60.0 - hi;
                double p = v * (1 - s);
                double q = v * (1 - s * f);
                double t = v * (1 - s * (1 - f));

                switch ((int)hi)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;
                    default:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }
            else
            {
                r = g = b = v;
            }

            return Color.FromArgb(255, ToByte(r), ToByte(g), ToByte(b));
        }


        /// <summary>
        /// Converts a color to HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">The hue [0, 360[.</param>
        /// <param name="s">The saturation [0, 100].</param>
        /// <param name="v">The value [0, 100].</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static void ToHsv(this Color color, out double h, out double s, out double v)
        {
            // see http://de.wikipedia.org/wiki/HSV-Farbraum

            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            double delta = max - min;

            if (max > 0)
                s = delta / max * 100.0;
            else
                s = 0;

            v = max / 255.0 * 100.0;

            if (s > 0)
            {
                if (color.R == max)
                    h = 0.0 + (color.G - color.B) / delta;
                else if (color.G == max)
                    h = 2.0 + (color.B - color.R) / delta;
                else // color.B == max
                    h = 4.0 + (color.R - color.G) / delta;

                h *= 60.0;

                if (h < 0)
                    h += 360;
            }
            else
            {
                h = 0;
            }

            Debug.Assert(h >= 0 && h < 360, "Hue of HSV color is out of range.");
            Debug.Assert(s >= 0 && s <= 100, "Saturation of HSV color is out of range.");
            Debug.Assert(v >= 0 && v <= 100, "Value of HSV color is out of range.");
        }


        /// <summary>
        /// Creates a <see cref="Color"/> from HSL values.
        /// </summary>
        /// <param name="h">The hue [0, 360[.</param>
        /// <param name="s">The saturation [0, 100].</param>
        /// <param name="l">The lightness [0, 100].</param>
        /// <returns>The <see cref="Color"/> instance for this HSV color.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static Color FromHsl(double h, double s, double l)
        {
            // See http://www5.informatik.tu-muenchen.de/lehre/vorlesungen/graphik/info/csc/COL_26.htm#topic25

            double r, g, b;

            s /= 100.0;
            l /= 100.0;

            if (s == 0.0)
            {
                r = g = b = l;
            }
            else
            {
                double m1, m2;
                if (l <= 0.5)
                    m2 = l * (1 + s);
                else
                    m2 = l + s - l * s;

                m1 = 2 * l - m2;

                r = GetValue(m1, m2, h + 120);
                g = GetValue(m1, m2, h);
                b = GetValue(m1, m2, h - 120);
            }

            return Color.FromArgb(255, ToByte(r), ToByte(g), ToByte(b));
        }


        private static double GetValue(double n1, double n2, double hue)
        {
            double value;
            if (hue > 360.0)
                hue -= 360.0;
            else if (hue < 0)
                hue += 360.0;

            if (hue < 60)
                value = n1 + (n2 - n1) * hue / 60;
            else if (hue < 180)
                value = n2;
            else if (hue < 240)
                value = n1 + (n2 - n1) * (240 - hue) / 60;
            else
                value = n1;

            return value;
        }


        /// <summary>Converts a color to HSL color space.</summary>
        /// <param name="color">The color.</param>
        /// <param name="h">The hue [0, 360[.</param>
        /// <param name="s">The saturation [0, 100].</param>
        /// <param name="l">The lightness [0, 100].</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static void ToHsl(this Color color, out double h, out double s, out double l)
        {
            // See http://www5.informatik.tu-muenchen.de/lehre/vorlesungen/graphik/info/csc/COL_26.htm#topic25
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            int max = Math.Max(color.R, Math.Max(color.G, color.B));

            l = (min + max) / 2.0;
            l = l / 255.0;

            if (min == max)
            {
                h = 0;
                s = 0;
                l *= 100;
                return;
            }

            if (l <= 0.5)
                s = (double)(max - min) / (max + min);
            else
                s = (double)(max - min) / (2 * 255 - max - min);

            double delta = max - min;
            if (color.R == max)
                h = 0.0 + (color.G - color.B) / delta;
            else if (color.G == max)
                h = 2.0 + (color.B - color.R) / delta;
            else // color.B == max
                h = 4.0 + (color.R - color.G) / delta;

            h *= 60.0;

            if (h < 0)
                h += 360;

            s *= 100.0;
            l *= 100.0;

            Debug.Assert(h >= 0 && h < 360, "Hue of HSV color is out of range.");
            Debug.Assert(s >= 0 && s <= 100, "Saturation of HSV color is out of range.");
            Debug.Assert(l >= 0 && l <= 100, "Lightness of HSV color is out of range.");
        }
        #endregion


        /// <summary>
        /// Gets the H component ("hue") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The hue [0, 360[.</returns>
        public static double GetH(this Color color)
        {
            double h, s, v;
            color.ToHsv(out h, out s, out v);
            return h;
        }


        /// <summary>
        /// Gets the S component ("saturation") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The saturation [0, 100].</returns>
        public static double GetS(this Color color)
        {
            double h, s, v;
            color.ToHsv(out h, out s, out v);
            return s;
        }


        /// <summary>
        /// Gets the V component ("value") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The value [0, 100].</returns>
        public static double GetV(this Color color)
        {
            double h, s, v;
            color.ToHsv(out h, out s, out v);
            return v;
        }


        ///// <summary>
        ///// Gets the L component ("lightness") of the color in HSL color space.
        ///// </summary>
        ///// <param name="color">The color.</param>
        ///// <returns>The lightness [0, 100].</returns>
        //public static double GetL(this Color color)
        //{
        //  double h, s, l;
        //  color.ToHsl(out h, out s, out l);
        //  return l;
        //}


        /// <summary>
        /// Sets the H component ("hue") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">The hue [0, 360[.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static void SetH(ref Color color, double h)
        {
            double dummy, s, v;
            color.ToHsv(out dummy, out s, out v);
            Color temp = FromHsv(h, s, v);
            color.R = temp.R;
            color.G = temp.G;
            color.B = temp.B;
        }


        /// <summary>
        /// Sets the S component ("saturation") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="s">The saturation [0, 100].</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static void SetS(ref Color color, double s)
        {
            double h, dummy, v;
            color.ToHsv(out h, out dummy, out v);
            Color temp = FromHsv(h, s, v);
            color.R = temp.R;
            color.G = temp.G;
            color.B = temp.B;
        }


        /// <summary>
        /// Sets the V component ("value") of the color in HSV color space.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="v">The value [0, 100].</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static void SetV(ref Color color, double v)
        {
            double h, s, dummy;
            color.ToHsv(out h, out s, out dummy);
            Color temp = FromHsv(h, s, v);
            color.R = temp.R;
            color.G = temp.G;
            color.B = temp.B;
        }


        ///// <summary>
        ///// Sets the L component ("lightness") of the color in HSL color space.
        ///// </summary>
        ///// <param name="color">The color.</param>
        ///// <param name="l">The lightness [0, 100].</param>
        //public static void SetL(ref Color color, double l)
        //{
        //  double h, s, dummy;
        //  color.ToHsl(out h, out s, out dummy);
        //  Color temp = FromHsl(h, s, l);
        //  color.R = temp.R;
        //  color.G = temp.G;
        //  color.B = temp.B;
        //}


        #region ----- sRGB -----

        // References: 
        // -  Adventures with Gamma-Correct Rendering,
        //    http://renderwonk.com/blog/index.php/archive/adventures-with-gamma-correct-rendering/

        /// <summary>
        /// Converts the specified color from linear color space to sRGB color space.
        /// </summary>
        /// <param name="colorLinear">The color in linear space.</param>
        /// <returns>The color in sRGB space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static Color ToSRgb(this Color colorLinear)
        {
            return Color.FromArgb(
              colorLinear.A,
              ToSRgb(colorLinear.R),
              ToSRgb(colorLinear.G),
              ToSRgb(colorLinear.B));
        }


        /// <summary>
        /// Converts the specified color from sRGB color space to linear color space.
        /// </summary>
        /// <param name="colorSRgb">The color in sRGB space.</param>
        /// <returns>The color in linear space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static Color ToLinear(this Color colorSRgb)
        {
            return Color.FromArgb(
              colorSRgb.A,
              ToLinear(colorSRgb.R),
              ToLinear(colorSRgb.G),
              ToLinear(colorSRgb.B));
        }


        /// <summary>
        /// Converts the specified color component (red, green, or blue) from linear color space to
        /// sRGB color space.
        /// </summary>
        /// <param name="c">The color component (red, green, or blue) in linear space.</param>
        /// <returns>The color component (red, green, or blue) in sRGB space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static byte ToSRgb(byte c)
        {
            return ToByte(ToSRgb(ToDouble(c)));
        }


        /// <summary>
        /// Converts the specified color component (red, green, or blue) from sRGB color space to
        /// linear color space.
        /// </summary>
        /// <param name="c">The color component (red, green, or blue) in sRGB space.</param>
        /// <returns>The color component (red, green, or blue) in linear space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static byte ToLinear(byte c)
        {
            return ToByte(ToLinear(ToDouble(c)));
        }


        /// <summary>
        /// Converts the specified color component (red, green, or blue) from linear color space to
        /// sRGB color space.
        /// </summary>
        /// <param name="c">The color component (red, green, or blue) in linear space.</param>
        /// <returns>The color component (red, green, or blue) in sRGB space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static double ToSRgb(double c)
        {
            if (Numeric.IsNaN(c))
                c = 0.0;
            else if (c > 1.0)
                c = 1.0;
            else if (c < 0.0)
                c = 0.0;
            else if (c <= 0.0031308)
                c = 12.92 * c; // When converting from bytes this branch is never used, because c ≤ 0.0031308 cannot be represented.
            else
                c = 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;

            return c;
        }


        /// <summary>
        /// Converts the specified color component (red, green, or blue) from sRGB color space to
        /// linear color space.
        /// </summary>
        /// <param name="c">The color component (red, green, or blue) in sRGB space.</param>
        /// <returns>The color component (red, green, or blue) in linear space.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static double ToLinear(double c)
        {
            if (c <= 0.04045)
                c = c / 12.92;
            else
                c = Math.Pow((c + 0.055) / 1.055, 2.4);

            return c;
        }
        #endregion
    }
}
