// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Xml.Linq;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Serializes/Deserializes the docking layout of an <see cref="IDockControl"/> to/from an
    /// <see cref="XElement"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only non-default values are written into the <see cref="XDocument"/> to make it more
    /// compact.
    /// </para>
    /// <para>
    /// This class expects that <see cref="IDockTabItem"/>s have unique
    /// <see cref="IDockTabItem.DockId"/>s. The following <see cref="IDockTabItem"/> properties are
    /// not serialized: <see cref="IDockTabItem.Icon"/>, <see cref="IDockTabItem.IsPersistent"/>,
    /// <see cref="IDockTabItem.Title"/>.
    /// </para>
    /// <para>
    /// References to other <see cref="IDockElement"/>s are represented as simple integer IDs.
    /// </para>
    /// </remarks>
    public static partial class DockSerializer
    {
        // Type converters which are re-used.
        private static readonly GridLengthConverter GridLengthConverter = new GridLengthConverter();
    }
}
