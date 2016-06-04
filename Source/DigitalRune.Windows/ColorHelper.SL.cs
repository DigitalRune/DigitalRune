// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if SILVERLIGHT
using System;
using System.Windows.Media;


namespace DigitalRune.Windows
{
  static partial class ColorHelper
  {
    /// <summary>
    /// Adds two <see cref="Color"/> structures. 
    /// </summary>
    /// <param name="color1">The first <see cref="Color"/> structure to add.</param>
    /// <param name="color2">The second <see cref="Color"/> structure to add.</param>
    /// <returns>
    /// A new <see cref="Color"/> structure whose color values are the results of the addition operation.
    /// </returns>
    /// <remarks>
    /// <see cref="Color"/> structures are added together by adding the alpha, red, green, 
    /// and blue channels of the first color to the alpha, red, green, and blue channels 
    /// of the second color. For example, the alpha channel of <paramref name="color1"/> 
    /// and the alpha channel of <paramref name="color2"/> are added together to produce 
    /// the alpha channel of the resulting color. The same is done with the red, green, 
    /// and blue channels to produce the red, green, and blue channels of the new color. 
    /// </remarks>
    public static Color Add(Color color1, Color color2)
    {
      Color result = new Color
      {
        R = (byte)Math.Min(color1.R + color2.R, 255),
        G = (byte)Math.Min(color1.G + color2.G, 255),
        B = (byte)Math.Min(color1.B + color2.B, 255),
        A = (byte)Math.Min(color1.A + color2.A, 255)
      };
      return result;
    }


    /// <summary>
    /// Multiplies the alpha, red, blue, and green channels of the specified <see cref="Color"/> 
    /// structure by the specified value.
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to be multiplied.</param>
    /// <param name="coefficient">The value to multiply by.</param>
    /// <returns>
    /// A new <see cref="Color"/> structure whose color values are the results of the 
    /// multiplication operation. 
    /// </returns>
    /// <remarks>
    /// </remarks>
    public static Color Multiply(Color color, float coefficient)
    {
      Color result = new Color
      {
        R = (byte)Math.Min(Math.Max(color.R * coefficient, 0), 255),
        G = (byte)Math.Min(Math.Max(color.G * coefficient, 0), 255),
        B = (byte)Math.Min(Math.Max(color.B * coefficient, 0), 255),
        A = (byte)Math.Min(Math.Max(color.A * coefficient, 0), 255)
      };
      return result;
    }


    /// <summary>
    /// Subtracts a <see cref="Color"/> structure from a <see cref="Color"/> structure. 
    /// </summary>
    /// <param name="color1">The <see cref="Color"/> structure to be subtracted from. </param>
    /// <param name="color2">The <see cref="Color"/> structure to subtract from <paramref name="color1"/>. </param>
    /// <returns>
    /// A new <see cref="Color"/> structure whose color values are the results of the 
    /// subtraction operation. 
    /// </returns>
    /// <remarks>
    /// <see cref="Color"/> structures are subtracted from one another by subtracting the alpha, 
    /// red, green, and blue channels of the second color from the alpha, red, green, and blue channels 
    /// of the first color. For example, the alpha channel of <paramref name="color2"/> is subtracted from the 
    /// alpha channel of <paramref name="color1"/> to produce the alpha channel of the resulting Color structure. 
    /// The same is done with the red, green, and blue channels to produce the red, green, 
    /// and blue channels of the new Color structure. 
    /// </remarks>
    public static Color Subtract(Color color1, Color color2)
    {
      Color result = new Color
      {
        R = (byte)Math.Max(color1.R - color2.R, 0),
        G = (byte)Math.Max(color1.G - color2.G, 0),
        B = (byte)Math.Max(color1.B - color2.B, 0),
        A = (byte)Math.Max(color1.A - color2.A, 0)
      };
      return result;
    }
  }
}
#endif
