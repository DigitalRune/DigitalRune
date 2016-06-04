// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Provides a mechanism for attaching <see cref="MultiBindings"/> to an element.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This helper class provides two attached properties. "AttachedDataContext" is only for
  /// internal use. 
  /// </para>
  /// <para>
  /// This code is derived from Colin Eberhardt and Stefan Olson, see 
  /// http://www.scottlogic.co.uk/blog/colin/2010/05/silverlight-multibinding-solution-for-silverlight-4/
  /// and
  /// http://www.olsonsoft.com/blogs/stefanolson/post/Updates-to-Silverlight-Multi-binding-support.aspx
  /// </para>
  /// </remarks>
  public class BindingHelper
  {
    // For the binding to work, we must pass the DataContext of the target element on to our
    // MultiBindings. Therefore, we bind an attached "AttachedDataContext" property to {Binding}.
    // Whenever the original DataContext is changed, the AttachedDataContext property calls
    // the PropertyChangedCallback and we can pass the new DataContext on to the MultiBindings.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Identifies the <strong>AttachedDataContext</strong> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty AttachedDataContextProperty = DependencyProperty.RegisterAttached(
      "AttachedDataContext", typeof(object), typeof(BindingHelper),
      new PropertyMetadata(null, OnAttachedDataContextChanged));

    /// <summary>
    /// Gets the value of the <strong>AttachedDataContext</strong> attached property from a given 
    /// <see cref="DependencyObject"/> object.
    /// </summary>
    /// <param name="obj">The object from which to read the property value.</param>
    /// <return>The value of the <strong>AttachedDataContext</strong> attached property.</return>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="obj"/> is <see langword="null"/>.
    /// </exception>
    public static object GetAttachedDataContext(DependencyObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      return obj.GetValue(AttachedDataContextProperty);
    }

    /// <summary>
    /// Sets the value of the <strong>AttachedDataContext</strong> attached property to a given 
    /// <see cref="DependencyObject"/> object.
    /// </summary>
    /// <param name="obj">The object on which to set the property value.</param>
    /// <param name="value">The property value to set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="obj"/> is <see langword="null"/>.
    /// </exception>
    public static void SetAttachedDataContext(DependencyObject obj, object value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      obj.SetValue(AttachedDataContextProperty, value);
    }


    /// <summary>
    /// Identifies the <strong>MultiBindings</strong> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty MultiBindingsProperty = DependencyProperty.RegisterAttached(
      "MultiBindings", typeof(MultiBindings), typeof(BindingHelper),
      new PropertyMetadata(null, OnMultiBindingsChanged));

    /// <summary>
    /// Gets the value of the <strong>MultiBindings</strong> attached property from a given 
    /// <see cref="DependencyObject"/> object.
    /// </summary>
    /// <param name="obj">The object from which to read the property value.</param>
    /// <return>The value of the <strong>MultiBindings</strong> attached property.</return>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="obj"/> is <see langword="null"/>.
    /// </exception>
    public static MultiBindings GetMultiBindings(DependencyObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      return (MultiBindings)obj.GetValue(MultiBindingsProperty);
    }

    /// <summary>
    /// Sets the value of the <strong>MultiBindings</strong> attached property to a given 
    /// <see cref="DependencyObject"/> object.
    /// </summary>
    /// <param name="obj">The object on which to set the property value.</param>
    /// <param name="value">The property value to set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="obj"/> is <see langword="null"/>.
    /// </exception>
    public static void SetMultiBindings(DependencyObject obj, MultiBindings value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      obj.SetValue(MultiBindingsProperty, value);
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private static void OnAttachedDataContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      FrameworkElement element = (FrameworkElement)dependencyObject;

      // Copy new DataContext to MultiBindings.
      MultiBindings multiBindings = GetMultiBindings(element);
      multiBindings.SetDataContext(element.DataContext);
    }


    private static void OnMultiBindingsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      FrameworkElement element = (FrameworkElement)dependencyObject;

      // Bind element.DataContext to our attached property. 
      // This allows us to get property changed callbacks when element.DataContext changes
      element.SetBinding(AttachedDataContextProperty, new Binding());

      MultiBindings bindings = GetMultiBindings(element);
      bindings.Initialize(element);
    }
    #endregion
  }
}
