// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Runtime.InteropServices;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The <see cref="POINT"/> structure defines the x- and y- coordinates of a point. (See header
    /// file "windef.h").
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        /// Specifies the x-coordinate of the point.
        /// </summary>
        public int X;


        /// <summary>
        /// Specifies the y-coordinate of the point.
        /// </summary>
        public int Y;
    }

    // ReSharper restore InconsistentNaming
}
