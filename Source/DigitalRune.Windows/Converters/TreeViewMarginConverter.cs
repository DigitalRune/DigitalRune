// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Calculates the margin for <see cref="TreeViewItem"/>s in a <see cref="TreeView"/>.
    /// </summary>
    [ValueConversion(typeof(TreeViewItem), typeof(Thickness), ParameterType = typeof(double))]
    public sealed class TreeViewMarginConverter : IValueConverter
    {
        // Reference: http://stackoverflow.com/questions/664632/highlight-whole-treeviewitem-line-in-wpf

        /// <summary>
        /// Gets an instance of the <see cref="TreeViewMarginConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="TreeViewMarginConverter"/>.</value>
        public static TreeViewMarginConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TreeViewMarginConverter();

                return _instance;
            }
        }
        private static TreeViewMarginConverter _instance;


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">
        /// The offset between the left border of the <see cref="TreeView"/> and the
        /// <see cref="TreeViewItem"/> header.
        /// </param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is
        /// used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = value as TreeViewItem;
                if (item == null)
                    return new Thickness(0);

                double offset = ObjectHelper.ConvertTo<double>(parameter);
                return new Thickness(offset * GetDepth(item), 0, 0, 0);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is
        /// used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        private static int GetDepth(TreeViewItem item)
        {
            var parent = GetParent(item);
            if (parent != null)
                return GetDepth(parent) + 1;

            return 0;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem) && !(parent is TreeView))
                parent = VisualTreeHelper.GetParent(parent);

            return parent as TreeViewItem;
        }
    }
}
