// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Represents the window layout drop-down button in the caption bar.
    /// </summary>
    internal class WindowLayoutCaptionBarViewModel : ObservableObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public ICommandItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        private ICommandItem _selectedItem;


        /// <summary>
        /// Gets or sets the items shown in the drop-down.
        /// </summary>
        /// <value>The items shown in the drop-down.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public MenuItemViewModelCollection Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
        private MenuItemViewModelCollection _items;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
