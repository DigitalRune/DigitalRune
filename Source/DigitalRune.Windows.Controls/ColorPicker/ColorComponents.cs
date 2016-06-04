// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// An enumeration for the possible color components.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
    [Flags]
    public enum ColorComponents
    {
        /// <summary>No component.</summary>
        None = 0,

        /// <summary>Hue of a HSV color.</summary>
        Hue = 1,

        /// <summary>Saturation of a HSV color.</summary>
        Saturation = 2,

        /// <summary>Value (brightness) of a HSV color.</summary>
        Value = 4,

        /// <summary>Red of a RGB color.</summary>
        Red = 8,

        /// <summary>Green of a RGB color.</summary>
        Green = 16,

        /// <summary>Blue of a RGB color.</summary>
        Blue = 32,

        /// <summary>Alpha of a RGB color.</summary>
        Alpha = 64,

        /// <summary>All components of a color.</summary>
        All = 0x7FFFFFFF,
    }
}
