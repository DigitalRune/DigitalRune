#region ----- Copyright -----
/*
   This control is a modified version of the MultiFrameImage implemented in MahApps.Metro (see
   https://github.com/MahApps/MahApps.Metro) which is licensed under Ms-PL (see below).


    Microsoft Public License (Ms-PL)

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

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that displays an image from a multi-frame image source.
    /// </summary>
    /// <remarks>
    /// The <see cref="MultiFrameImage"/> can be used with image files which contain one picture
    /// in different resolutions (e.g. .ico files). <see cref="MultiFrameImage"/> renders the frame
    /// which is best for the current size of the <see cref="MultiFrameImage"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi")]
    public class MultiFrameImage : Icon
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private BitmapFrame[] _frames;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="MultiFrameImage"/> class.
        /// </summary>
        static MultiFrameImage()
        {
            SourceProperty.OverrideMetadata(typeof(MultiFrameImage), new FrameworkPropertyMetadata(OnSourceChanged));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var multiFrameImage = (MultiFrameImage)dependencyObject;
            multiFrameImage.UpdateFrameList();
        }


        private void UpdateFrameList()
        {
            var decoder = (Source as BitmapFrame)?.Decoder;
            if (decoder == null || decoder.Frames.Count == 0)
                return;

            // Order all frames by size, take the frame with the highest color depth per size.
            _frames = decoder.Frames
                             .GroupBy(f => f.PixelWidth * f.PixelHeight)
                             .OrderBy(g => g.Key)
                             .Select(g => g.OrderByDescending(f => f.Format.BitsPerPixel).First())
                             .ToArray();
        }


        /// <summary>
        /// Renders the optimal frame from the image.
        /// </summary>
        /// <param name="drawingContext">The drawing context used to render the control.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext == null)
                throw new ArgumentNullException(nameof(drawingContext));

            if (_frames == null || _frames.Length == 0)
            {
                base.OnRender(drawingContext);
            }
            else
            {
                double size = Math.Max(RenderSize.Width, RenderSize.Height);
                var frame = GetFrame(_frames, size);
                drawingContext.DrawImage(frame, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
            }
        }


        private static BitmapFrame GetFrame(BitmapFrame[] frames, double size)
        {
            Debug.Assert(frames != null && frames.Length > 0);

            for (int i = 0; i < frames.Length; i++)
                if (frames[i].Width >= size && frames[i].Height >= size)
                    return frames[i];

            return frames[frames.Length - 1];
        }
        #endregion
    }
}
