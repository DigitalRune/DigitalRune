// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

//using System;
//using System.Globalization;
//using System.Windows.Data;
//using DigitalRune.Windows.Themes;


//namespace DigitalRune.Editor.Errors
//{
//    /// <summary>
//    /// Converts an <see cref="ErrorType"/> to an icon source.
//    /// </summary>
//    [ValueConversion(typeof(ErrorType), typeof(object))]
//    public class ErrorIconConverter : IValueConverter
//    {
//        /// <summary>
//        /// An instance of the <see cref="ErrorIconConverter"/>.
//        /// </summary>
//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
//        public static readonly ErrorIconConverter Instance = new ErrorIconConverter();


//        /// <summary>
//        /// Converts a value.
//        /// </summary>
//        /// <param name="value">The value produced by the binding source.</param>
//        /// <param name="targetType">The type of the binding target property.</param>
//        /// <param name="parameter">The converter parameter to use.</param>
//        /// <param name="culture">The culture to use in the converter.</param>
//        /// <returns>
//        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
//        /// used.
//        /// </returns>
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is ErrorType)
//            {
//                switch ((ErrorType)value)
//                {
//                    case ErrorType.Error:
//                        return MultiColorGlyphs.MessageError;
//                    case ErrorType.Warning:
//                        return MultiColorGlyphs.MessageWarning;
//                    case ErrorType.Message:
//                        return MultiColorGlyphs.MessageInformation;
//                }
//            }

//            return value;
//        }


//        /// <summary>
//        /// Converts a value.
//        /// </summary>
//        /// <param name="value">The value that is produced by the binding target.</param>
//        /// <param name="targetType">The type to convert to.</param>
//        /// <param name="parameter">The converter parameter to use.</param>
//        /// <param name="culture">The culture to use in the converter.</param>
//        /// <returns>
//        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
//        /// used.
//        /// </returns>
//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
