// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Represents a (key, value) pair for use with the <see cref="LookupConverter"/>.
    /// </summary>
    public class LookupEntry
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>The key.</value>
        public object Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; set; }
    }


    /// <summary>
    /// Converts a value using a lookup table. The value is used as the lookup key and the
    /// associated value is returned.
    /// </summary>
    /// <remarks>
    /// This converter assumes that the number of <see cref="Entries"/> is small. (It uses linear
    /// search.)
    /// </remarks>
    /// <example>
    /// Here is an usage example that converts a user-defined enumeration 'MessageType' to a color:
    /// <code lang="xaml">
    /// <![CDATA[
    /// <local:LookupConverter x:Key="MessageTypeToColorConverter">
    ///   <local:LookupEntry>
    ///     <local:LookupEntry.Key>
    ///       <vm:MessageType>Comment</vm:MessageType>
    ///     </local:LookupEntry.Key>
    ///     <local:LookupEntry.Value>
    ///       <SolidColorBrush Color="#50000000"/>
    ///     </local:LookupEntry.Value>
    ///   </local:LookupEntry>
    ///   <local:LookupEntry Value="{StaticResource WarningBrush}">
    ///     <local:LookupEntry.Key>
    ///       <vm:MessageType>Warning</vm:MessageType>
    ///     </local:LookupEntry.Key>
    ///   </local:LookupEntry>
    /// </local:LookupConverter>
    /// ]]>
    /// </code>
    /// </example>
    [ContentProperty("Entries")]
    public class LookupConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the entries that define the lookup table.
        /// </summary>
        /// <value>The entries that define the lookup table.</value>
        public IList<LookupEntry> Entries { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LookupConverter"/> class.
        /// </summary>
        public LookupConverter()
        {
            Entries = new List<LookupEntry>();
        }


        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the target dependency property.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var entry in Entries)
                if (Equals(value, entry.Key))
                    return entry.Value;

            return DependencyProperty.UnsetValue;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the source object.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
