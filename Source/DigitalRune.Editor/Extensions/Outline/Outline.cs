// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Represents the outline which is shown in the Outline window. 
    /// </summary>
    public class Outline : ObservableObject
    {
        /// <summary>
        /// Gets or sets the root items.
        /// </summary>
        /// <value>
        /// The root items. The default value is an empty collection.
        /// </value>
        public ObservableCollection<OutlineItem> RootItems { get; } = new ObservableCollection<OutlineItem>();


        /// <summary>
        /// Gets the selected items.
        /// </summary>
        /// <value>
        /// The selected items. The default value is an empty collection.
        /// </value>
        public ObservableCollection<OutlineItem> SelectedItems { get; } = new ObservableCollection<OutlineItem>();
    }
}
