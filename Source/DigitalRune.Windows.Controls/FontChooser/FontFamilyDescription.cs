// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Describes a font family. (Used internally by the <see cref="FontChooser"/>.)
    /// </summary>
    public class FontFamilyDescription
    {
        /// <summary>
        /// Gets or sets the display name of the font family.
        /// </summary>
        /// <value>The display name font family.</value>
        public string DisplayName { get; set; }


        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// <value>The font family.</value>
        public FontFamily FontFamily { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the font is symbol font.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the font is symbol font; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsSymbolFont { get; set; }
    }
}
