// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Textures
{
    [Flags]
    internal enum ColorChannels
    {
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3,
        RGB = Red | Green | Blue,
        RGBA = RGB | Alpha
    }
}
