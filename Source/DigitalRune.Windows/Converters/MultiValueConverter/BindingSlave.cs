// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows
{
  /// <summary>
  /// A simple element with a single Value property, used as a 'slave' for a <see cref="Binding"/>.
  /// </summary>
  /// <remarks>
  /// This code is derived from Colin Eberhardt and Stefan Olson, see 
  /// http://www.scottlogic.co.uk/blog/colin/2010/05/silverlight-multibinding-solution-for-silverlight-4/
  /// and
  /// http://www.olsonsoft.com/blogs/stefanolson/post/Updates-to-Silverlight-Multi-binding-support.aspx
  /// </remarks>
  public class BindingSlave : FrameworkElement, INotifyPropertyChanged
	{
    /// <summary>
    /// Identifies the <strong>Value</strong> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      "Value", 
      typeof(object), 
      typeof(BindingSlave), 
      new PropertyMetadata(null, OnValueChanged));

    /// <summary>
    /// Gets or sets the value. This is a dependency property.
    /// </summary>
    /// <value>The value.</value>
    [Description("Gets or sets the value.")]
    [Category(Categories.Default)]
    public object Value
    {
      get { return GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }


    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;


    private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      BindingSlave slave = (BindingSlave)dependencyObject;
      slave.OnPropertyChanged("Value");
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event indicating that a certain property has
    /// changed.
    /// </summary>
    /// <param name="name">The name of the property that was changed.</param>
    protected void OnPropertyChanged(string name)
    {
      var handler = PropertyChanged;

      if (handler != null)
        handler(this, new PropertyChangedEventArgs(name));
    }
	}
}
