// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Extends the existing toolbar to support data binding with view models.
    /// </summary>
    /// <remarks>
    /// The <see cref="ItemsControl.ItemsSource"/> can be bound to a view model collection. The item
    /// containers are generated from implicit data templates.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class ToolBarEx : ToolBar
    {
        private object _currentItem;


        /// <summary>
        /// Initializes static members of the <see cref="ToolBarEx"/> class.
        /// </summary>
        static ToolBarEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBarEx), new FrameworkPropertyMetadata(typeof(ToolBarEx)));
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
            bool isItemItsOwnContainer = base.IsItemItsOwnContainerOverride(item);
            if (isItemItsOwnContainer)
            {
                _currentItem = null;
            }
            else
            {
                // Store the current item for use in GetContainerForItemOverride.
                // (The base ItemsControl will call IsItemItsOwnContainerOverride followed by
                // GetContainerForItemOverride if the result was false.)
                _currentItem = item;
            }

            return isItemItsOwnContainer;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
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
                if (dataTemplate != null)
                    return dataTemplate.LoadContent();
            }

            return base.GetContainerForItemOverride();
        }
    }
}
