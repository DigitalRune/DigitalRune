// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// A value converter that returns the name of a document.
    /// </summary>
    [ValueConversion(typeof(Document), typeof(string))]
    public class DocumentToNameConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="DocumentToNameConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="DocumentToNameConverter"/>.</value>
        public static DocumentToNameConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DocumentToNameConverter();

                return _instance;
            }
        }
        private static DocumentToNameConverter _instance;


        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The param.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is
        /// used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var document = value as Document;
            if (document == null)
                return DependencyProperty.UnsetValue;

            return document.GetName();
        }


        /// <summary>
        /// Converts a value.
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
    }
}
