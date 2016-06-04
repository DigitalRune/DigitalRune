// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Represents the converter that converts <see cref="bool"/> values to and from 
  /// <see cref="Visibility"/> enumeration values.
  /// </summary>
  public class BooleanToVisibilityConverter : IValueConverter
  {
    /// <summary>
    /// Modifies the source data before passing it to the target for display in the UI.
    /// </summary>
    /// <param name="value">The source data being passed to the target.</param>
    /// <param name="targetType">The <see cref="Type"/> of data expected by the target dependency property.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
    /// <param name="culture">The culture of the conversion.</param>
    /// <returns>The value to be passed to the target dependency property.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool isVisible = false;
      if (value is bool)
      {
        isVisible = (bool)value;
      }
      else if (value is bool?)
      {
        bool? valueAsNullable = (bool?)value;
        isVisible = valueAsNullable ?? false;
      }

      return (isVisible ? Visibility.Visible : Visibility.Collapsed);
    }


    /// <summary>
    /// Modifies the target data before passing it to the source object.  This method is called only 
    /// in <see cref="System.Windows.Data.BindingMode.TwoWay"/> bindings.
    /// </summary>
    /// <param name="value">The target data being passed to the source.</param>
    /// <param name="targetType">The <see cref="Type"/> of data expected by the source object.</param>
    /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
    /// <param name="culture">The culture of the conversion.</param>
    /// <returns>The value to be passed to the source object.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((value is Visibility) && (((Visibility)value) == Visibility.Visible));
    }
  }


}
