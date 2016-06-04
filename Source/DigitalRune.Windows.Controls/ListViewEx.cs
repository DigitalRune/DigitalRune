// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Extends the <see cref="ListView"/> to support locally defined styles.
    /// </summary>
    /// <remarks>
    /// <para>This class is a replacement for <see cref="ListView"/>.</para>
    /// <para>
    /// The <see cref="ListView"/> and <see cref="ListViewItem"/> styles can be overridden by the
    /// <see cref="ListView.View"/>. See <see cref="ViewBase.DefaultStyleKey"/> and
    /// <see cref="ViewBase.ItemContainerDefaultStyleKey"/>. However, the <see cref="ListView"/>
    /// ignores local styles when they are set in local resources. It only looks for the these
    /// styles in theme resource dictionaries and generic resource dictionaries. The
    /// <see cref="ListViewEx"/> overrides the default behavior and automatically sets local styles.
    /// </para>
    /// <para>
    /// The <see cref="ListViewEx"/> does not override the
    /// <see cref="FrameworkElement.DefaultStyleKey"/>. It inherits the default style of the
    /// <see cref="ListView"/>
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class ListViewEx : ListView
    {
        /// <summary>
        /// Initializes static members of the <see cref="ListViewEx"/> class.
        /// </summary>
        static ListViewEx()
        {
            ViewProperty.OverrideMetadata(typeof(ListViewEx), new FrameworkPropertyMetadata(OnViewChanged));
        }


        private static void OnViewChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var listViewEx = (ListViewEx)dependencyObject;
            SetDefaultStyle(listViewEx);
        }


        /// <summary>
        /// Sets the styles, templates, and bindings for a <see cref="ListViewItem"/>.
        /// </summary>
        /// <param name="element">
        /// An object that is a <see cref="ListViewItem"/> or that can be converted into one.
        /// </param>
        /// <param name="item">The object to use to create the <see cref="ListViewItem"/>.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            SetDefaultStyle(element);
        }


        private static void SetDefaultStyle(DependencyObject element)
        {
            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null && frameworkElement.ReadLocalValue(StyleProperty) == DependencyProperty.UnsetValue)
            {
                var defaultStyleKey = element.GetValue(DefaultStyleKeyProperty);
                if (defaultStyleKey != null)
                {
                    // Set a resource reference, equivalent to Style="{DynamicResource defaultStyleKey}".
                    frameworkElement.SetResourceReference(StyleProperty, defaultStyleKey);
                }
            }
        }
    }
}
