// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !SILVERLIGHT
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Creates a <see cref="BitmapSource"/> for bitmap packed in a sprite sheet (texture atlas).
    /// </summary>
    /// <example>
    /// The following example shows how to create an <see cref="Image"/> which is a 32x32 bitmap
    /// packed inside a large bitmap at position (160, 0).
    /// <code lang="xaml">
    /// <![CDATA[
    /// <!-- Using absolute URI: -->
    /// <Image Source="{dr:PackedBitmap /DigitalRune.Windows.Controls;component/Resources/Icons.png, 160 0 32 32}" />
    /// 
    /// <!-- Using relative URI: -->
    /// <Image Source="{dr:PackedBitmap 'pack://application:,,,/DigitalRune.Windows.Controls;component/Resources/Icons.png', 160 0 32 32}" />
    /// ]]>
    /// </code>
    /// </example>
    public class PackedBitmapExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the <see cref="ImageSource"/> that contains all packed bitmaps.
        /// </summary>
        /// <value>The <see cref="ImageSource"/> that contains all packed bitmaps.</value>
        public ImageSource Source { get; set; }


        /// <summary>
        /// Gets or sets the rectangle that is used as the source of the bitmap.
        /// </summary>
        /// <value>The rectangle that is used as the source of the bitmap.</value>
        public Int32Rect SourceRect { get; set; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="PackedBitmapExtension"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="PackedBitmapExtension"/> class.
        /// </summary>
        public PackedBitmapExtension()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PackedBitmapExtension"/> class.
        /// </summary>
        /// <param name="source">The <see cref="ImageSource"/> that contains all packed bitmaps.</param>
        public PackedBitmapExtension(ImageSource source)
        {
            Source = source;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PackedBitmapExtension"/> class.
        /// </summary>
        /// <param name="source">The <see cref="ImageSource"/> that contains all packed bitmaps.</param>
        /// <param name="sourceRect">The rectangle that is used as the source of the bitmap.</param>
        public PackedBitmapExtension(ImageSource source, Int32Rect sourceRect)
        {
            Source = source;
            SourceRect = sourceRect;
        }


        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of
        /// the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">
        /// A service provider helper that can provide services for the markup extension.
        /// </param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Setting BitmapImage.SourceRect has no effect. Need to use CroppedBitmap.
            if (WindowsHelper.IsInDesignMode)
            {
                // ----- Design time:
                // Design mode requires special code when used inside a WPF styles.
                var bitmapImage = Source as BitmapImage;
                if (bitmapImage == null)
                    return null;

                var croppedBitmap = new CroppedBitmap();
                croppedBitmap.BeginInit();
                croppedBitmap.Source = new BitmapImage(bitmapImage.UriSource);
                croppedBitmap.SourceRect = SourceRect;
                croppedBitmap.EndInit();
                croppedBitmap.Freeze();

                return croppedBitmap;
            }
            else
            {
                // ----- Run time:
                var bitmapSource = Source as BitmapSource;
                if (bitmapSource == null)
                    return null;

                // Freeze bitmap for performance.
                bitmapSource.Freeze();

                var croppedBitmap = new CroppedBitmap(bitmapSource, SourceRect);
                croppedBitmap.Freeze();

                return croppedBitmap;
            }
        }
    }
}
#endif
