// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a multi-color glyph. (For use with the <see cref="Icon"/>.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multi-color glyphs are vector glyphs defined as regular TrueType/OpenType fonts. The font
    /// glyphs layered on top of each other.
    /// </para>
    /// <list type="number">
    /// <item>
    /// The <see cref="BackgroundGlyph"/> is rendered in the background using the background brush.
    /// This glyph usually defines the outline of the icon.
    /// </item>
    /// <item>
    /// The <see cref="ForegroundGlyph"/> is rendered on top using the foreground brush. This glyph
    /// usually shows the main motive.
    /// </item>
    /// <item>
    /// The <see cref="OverlayBackgroundGlyph"/> is rendered on top using the background brush. This
    /// glyph usually defines the outline of the overlay element.
    /// </item>
    /// <item>
    /// The <see cref="OverlayForegroundGlyph"/> is rendered on top using the overlay brush. If no
    /// overlay brush is specified the foreground brush is used. This glyph usually defines an
    /// optional overlay element.
    /// </item>
    /// </list>
    /// <para>
    /// Each layer is optional and can be <see langword="null"/>.
    /// </para>
    /// <para>
    /// The brushes need to be registered in a resource dictionary accessible by the
    /// <see cref="Icon"/>. The brushes are identified using resource keys. The resource keys can be
    /// <see langword="null"/>. In this case any default brushes defined in the theme resource
    /// dictionaries will be used. (If no brushes are defined, nothing will be drawn.)
    /// </para>
    /// <para>
    /// Note that <see cref="MultiColorGlyph"/> does not implement
    /// <see cref="INotifyPropertyChanged"/>, i.e. property changes do not automatically invalidate
    /// the <see cref="Icon"/>.
    /// </para>
    /// </remarks>
    public class MultiColorGlyph
    {
        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// <value>
        /// The font family. Examples:
        ///   "/AssemblyName;component/path/#FontName", 
        ///   "/DigitalRune.Windows.Themes;component/Resources/#DigitalRune Icons"
        /// </value>
        [TypeConverter(typeof(FontFamilyConverter))]
        public FontFamily FontFamily { get; set; }


        /// <summary>
        /// Gets or sets the resource key identifying the background brush.
        /// </summary>
        /// <value>
        /// The resource key identifying the background brush. If <see langword="null"/> the default
        /// background brush (see <see cref="Icon.Background"/>) is used.
        /// </value>
        public object BackgroundBrushKey { get; set; }


        /// <summary>
        /// Gets or sets the resource key identifying the foreground brush.
        /// </summary>
        /// <value>
        /// The resource key identifying the foreground brush. If <see langword="null"/> the default
        /// foreground brush (see <see cref="Icon.Foreground"/>) is used.
        /// </value>
        public object ForegroundBrushKey { get; set; }


        /// <summary>
        /// Gets or sets the resource key identifying the brush used for background of the overlay
        /// glyph.
        /// </summary>
        /// <value>
        /// The resource key identifying the brush used for background of the overlay glyph. If
        /// <see langword="null"/> the <see cref="BackgroundBrushKey"/> is used.
        /// </value>
        public object OverlayBackgroundBrushKey { get; set; }


        /// <summary>
        /// Gets or sets the resource key identifying the brush used for foreground of the overlay
        /// glyph.
        /// </summary>
        /// <value>
        /// The resource key identifying the brush used for foreground of the overlay glyph. If
        /// <see langword="null"/> the <see cref="ForegroundBrushKey"/> is used.
        /// </value>
        public object OverlayForegroundBrushKey { get; set; }


        /// <summary>
        /// Gets or sets the background glyph.
        /// </summary>
        /// <value>The background glyph. Can be <see langword="null"/> or empty.</value>
        public string BackgroundGlyph { get; set; }


        /// <summary>
        /// Gets or sets the foreground glyph.
        /// </summary>
        /// <value>The foreground glyph. Can be <see langword="null"/> or empty.</value>
        public string ForegroundGlyph { get; set; }


        /// <summary>
        /// Gets or sets the overlay background glyph.
        /// </summary>
        /// <value>The overlay background glyph. Can be <see langword="null"/> or empty.</value>
        public string OverlayBackgroundGlyph { get; set; }


        /// <summary>
        /// Gets or sets the overlay foreground glyph.
        /// </summary>
        /// <value>The overlay foreground glyph. Can be <see langword="null"/> or empty.</value>
        public string OverlayForegroundGlyph { get; set; }
    }
}
