// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Windows;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Manages a pool of <see cref="Style"/>s.
    /// </summary>
    public class StyleDispenser : ItemDispenser<Style>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StyleDispenser"/> class.
        /// </summary>
        public StyleDispenser()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StyleDispenser"/> class with the given
        /// styles.
        /// </summary>
        /// <param name="styles">The styles.</param>
        public StyleDispenser(IEnumerable<Style> styles)
            : base(styles)
        {
        }
    }
}
