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
    /// Represents a pane in the docking layout that is split horizontally or vertically into two
    /// or more panes.
    /// </summary>
    public class DockSplitPane : ItemsControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Workaround for creating item containers.
        private object _currentItem;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DockWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockWidthProperty = DockControl.DockWidthProperty.AddOwner(typeof(DockSplitPane));

        /// <inheritdoc cref="IDockElement.DockWidth"/>
        [Description("Gets or sets the desired width in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockWidth
        {
            get { return (GridLength)GetValue(DockWidthProperty); }
            set { SetValue(DockWidthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DockHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockHeightProperty = DockControl.DockHeightProperty.AddOwner(typeof(DockSplitPane));

        /// <inheritdoc cref="IDockElement.DockHeight"/>
        [Description("Gets or sets the desired height in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockHeight
        {
            get { return (GridLength)GetValue(DockHeightProperty); }
            set { SetValue(DockHeightProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(DockSplitPane),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the orientation.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The split orientation. <see cref="System.Windows.Controls.Orientation.Horizontal"/>
        /// means that the child elements are arranged horizontally next to each other.
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/> means that the child elements
        /// are stacked vertically. The default value is
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/>.
        /// </value>
        [Description("Gets or sets the split orientation.")]
        [Category(Categories.Layout)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="DockSplitPane"/> class.
        /// </summary>
        static DockSplitPane()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockSplitPane), new FrameworkPropertyMetadata(typeof(DockSplitPane)));
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
                // Filter IDockPanes that are invisible.
                var collectionView = CollectionViewSource.GetDefaultView(newValue);
                var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
                if (collectionViewLiveShaping != null && collectionViewLiveShaping.CanChangeLiveFiltering)
                {
                    collectionViewLiveShaping.LiveFilteringProperties.Clear();
                    collectionViewLiveShaping.LiveFilteringProperties.Add(nameof(IDockPane.IsVisible));
                    collectionViewLiveShaping.IsLiveFiltering = true;
                    collectionView.Filter = Filter;
                }
            }
        }


        private static bool Filter(object item)
        {
            var dockPane = item as IDockPane;
            return dockPane == null || dockPane.IsVisible;
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
            if (item is DockTabPane || item is DockSplitPane || item is DockAnchorPane)
            {
                _currentItem = null;
                return true;
            }

            // Store the current item for use in GetContainerForItemOverride.
            // (The base ItemsControl will call IsItemItsOwnContainerOverride followed by
            // GetContainerForItemOverride if the result was false.)
            _currentItem = item;
            return false;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override DependencyObject GetContainerForItemOverride()
        {
            if (_currentItem != null)
            {
                var type = _currentItem.GetType();
                _currentItem = null;

                // Manually load implicit data template. (Otherwise the ItemsControl will create a
                // ContentPresenter.)
                var dataTemplateKey = new DataTemplateKey(type);
                var dataTemplate = TryFindResource(dataTemplateKey) as DataTemplate;
                var container = dataTemplate?.LoadContent();
                if (container is DockAnchorPane || container is DockSplitPane || container is DockTabPane)
                    return container;
            }

            // Fix for Visual Studio Designer.
            if (WindowsHelper.IsInDesignMode)
                return base.GetContainerForItemOverride();

            throw new DockException("Items in DockSplitPane need to be of type DockAnchorPane/DockSplitPane/DockTabPane "
                                    + "or need to have an implicit data template of the given type.");
        }
        #endregion
    }
}
