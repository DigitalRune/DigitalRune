// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Manages a collection of brushes and selects brushes for items based on their index.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class IndexedBrushSelector : Collection<Brush>, IBrushSelector
    {
        /// <inheritdoc/>
        public Brush SelectBrush(object item, int index)
        {
            if (0 <= index && index < Count)
                return Items[index];

            return null;
        }
    }
}
