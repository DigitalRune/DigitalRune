// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a <see cref="IDockTabPane"/> with multiple <see cref="IDockTabItem"/>s that are
    /// in <see cref="DockState.AutoHide"/> state.
    /// </summary>
    public class AutoHideGroup : ItemsControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the <see cref="AutoHideGroup"/> class.
        /// </summary>
        static AutoHideGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoHideGroup), new FrameworkPropertyMetadata(typeof(AutoHideGroup)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="ItemsControl.ItemsSource"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (newValue != null)
            {
                // Filter IDockTabItems that are invisible.
                var collectionView = CollectionViewSource.GetDefaultView(newValue);
                var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
                if (collectionViewLiveShaping != null && collectionViewLiveShaping.CanChangeLiveFiltering)
                {
                    collectionViewLiveShaping.LiveFilteringProperties.Clear();
                    collectionViewLiveShaping.LiveFilteringProperties.Add(nameof(IDockTabItem.DockState));
                    collectionViewLiveShaping.IsLiveFiltering = true;
                    collectionView.Filter = Filter;
                }
            }
        }


        private static bool Filter(object item)
        {
            var dockTabItem = item as IDockTabItem;
            return dockTabItem == null || dockTabItem.DockState == DockState.AutoHide;
        }


        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// <see langword="true"/> if the item is (or is eligible to be) its own container;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AutoHideTab;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AutoHideTab();
        }
        #endregion
    }
}
