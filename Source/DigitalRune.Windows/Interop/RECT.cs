// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.InteropServices;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int left;


        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        public int top;


        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int right;


        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        public int bottom;


        /// <summary>
        /// Gets the width of the rectangle.
        /// </summary>
        /// <value>The width. The value is either positive or 0.</value>
        public int Width
        {
            get { return Math.Abs(right - left); }
        }


        /// <summary>
        /// Gets the height of the rectangle.
        /// </summary>
        /// <value>The height. The value is either positive or 0.</value>
        public int Height
        {
            get { return Math.Abs(bottom - top); }
        }
    }

    // ReSharper restore InconsistentNaming
}
