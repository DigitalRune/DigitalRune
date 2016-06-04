#region ----- Copyright -----

/* 
  The class SetterValueBindingHelper is based on the class written by David Anson (Microsoft).
  See http://blogs.msdn.com/b/delay/archive/2009/11/02/as-the-platform-evolves-so-do-the-workarounds-better-settervaluebindinghelper-makes-silverlight-setters-better-er.aspx

  Copyright (C) Microsoft Corporation. All Rights Reserved.
  This code released under the terms of the Microsoft Public License
  (Ms-PL, http://opensource.org/licenses/ms-pl.html).
*/

#endregion


#if SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Allows to set data bindings in Silverlight styles. (Only available in Silverlight and on
  /// Windows Phone.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class implements a workaround for a Silverlight XAML parser limitation that prevents the 
  /// following syntax from working: 
  ///   <c>&lt;Setter Property="IsSelected" Value="{Binding IsSelected}"/&gt;</c>.
  /// </para>
  /// <para>
  /// This type is available only in Silverlight, not in WPF.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example shows a data binding can be set in the style of a button. Here the 
  /// property <see cref="ContentControl.Content"/> is bound to the content of the 
  /// <see cref="FrameworkElement.DataContext"/>.
  /// <code lang="xaml">
  /// <![CDATA[
  ///   <Button DataContext="Binding of Content">
  ///     <Button.Style>
  ///       <Style TargetType="Button">
  ///         <!-- WPF syntax:
  ///         <Setter Property="Content" Value="{Binding}"/> -->
  ///         <Setter Property="dr:SetterValueBindingHelper.PropertyBinding">
  ///           <Setter.Value>
  ///             <dr:SetterValueBindingHelper Property="Content"
  ///                                          Binding="{Binding}"/>
  ///           </Setter.Value>
  ///         </Setter>
  ///       </Style>
  ///     </Button.Style>
  ///  </Button>
  /// ]]>
  /// </code>
  /// The following example shows that data bindings in a style can also be applied to attached
  /// properties.
  /// <code lang="xaml">
  /// <![CDATA[
  ///   <Button Content="Binding of Grid.Column and Grid.Row"
  ///           DataContext="1">
  ///     <Button.Style>
  ///       <Style TargetType="Button">
  ///         <!-- WPF syntax:
  ///         <Setter Property="Grid.Column" Value="{Binding}"/>
  ///         <Setter Property="Grid.Row" Value="{Binding}"/> -->
  ///         <Setter Property="dr:SetterValueBindingHelper.PropertyBinding">
  ///           <Setter.Value>
  ///             <dr:SetterValueBindingHelper>
  ///               <dr:SetterValueBindingHelper Type="System.Windows.Controls.Grid, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
  ///                                            Property="Column"
  ///                                            Binding="{Binding}"/>
  ///               <dr:SetterValueBindingHelper Type="Grid"
  ///                                            Property="Row"
  ///                                            Binding="{Binding}"/>
  ///             </dr:SetterValueBindingHelper>
  ///           </Setter.Value>
  ///         </Setter>
  ///       </Style>
  ///     </Button.Style>
  ///   </Button>
  /// ]]>
  /// </code>
  /// Note the type <see cref="Grid"/> can be specified using the assembly-qualified name, the full 
  /// name ("System.Windows.Controls.Grid") or the short name ("Grid"). The short name can only be 
  /// used if it is unambiguous.
  /// </example>
  [ContentProperty("Values")]
  public class SetterValueBindingHelper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets an optional type parameter used to specify the type of an attached
    /// dependency property as an assembly-qualified name, full name, or short name.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Unambiguous in XAML.")]
    public string Type { get; set; }


    /// <summary>
    /// Gets or sets a property name for the normal/attached dependency property on which to set the 
    /// <see cref="Binding"/>.
    /// </summary>
    public string Property { get; set; }


    /// <summary>
    /// Gets or sets a <see cref="System.Windows.Data.Binding"/> to set on the specified property.
    /// </summary>
    public Binding Binding { get; set; }


    /// <summary>
    /// Gets a collection of <see cref="SetterValueBindingHelper"/> instances to apply to the 
    /// target element.
    /// </summary>
    /// <remarks>
    /// Used when multiple bindings need to be applied to the same element.
    /// </remarks>
    public Collection<SetterValueBindingHelper> Values
    {
      get
      {
        // Defer creating collection until needed
        if (null == _values)
          _values = new Collection<SetterValueBindingHelper>();

        return _values;
      }
    }
    private Collection<SetterValueBindingHelper> _values;
    #endregion


    //--------------------------------------------------------------
    #region Dependency Properties & Routed Events
    //--------------------------------------------------------------

    /// <summary>
    /// Identifies the <strong>PropertyBinding</strong> attached dependency property.
    /// </summary>
    public static readonly DependencyProperty PropertyBindingProperty = DependencyProperty.RegisterAttached(
      "PropertyBinding",
      typeof(SetterValueBindingHelper),
      typeof(SetterValueBindingHelper),
      new PropertyMetadata(null, OnPropertyBindingPropertyChanged));


    /// <summary>
    /// Gets the value of the <strong>PropertyBinding</strong> attached dependency property.
    /// </summary>
    /// <param name="element">The element for which to get the property.</param>
    /// <returns>
    /// The value of the <strong>PropertyBinding</strong> attached dependency property.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null"/>.
    /// </exception>
    public static SetterValueBindingHelper GetPropertyBinding(DependencyObject element)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      return (SetterValueBindingHelper)element.GetValue(PropertyBindingProperty);
    }


    /// <summary>
    /// Sets the value of the <strong>PropertyBinding</strong> attached dependency property.
    /// </summary>
    /// <param name="element">The element on which to set the property.</param>
    /// <param name="value">
    /// The value for the <strong>PropertyBinding</strong> attached dependency property.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null"/>.
    /// </exception>
    public static void SetPropertyBinding(DependencyObject element, SetterValueBindingHelper value)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      element.SetValue(PropertyBindingProperty, value);
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Change handler for the <strong>PropertyBinding</strong> attached dependency property.
    /// </summary>
    /// <param name="dependencyObject">The object on which the property was changed.</param>
    /// <param name="eventArgs">The property change arguments.</param>
    private static void OnPropertyBindingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      // Get/validate parameters
      var element = dependencyObject as FrameworkElement;
      if (element == null)
        return;

      var item = (SetterValueBindingHelper)eventArgs.NewValue;
      if (item != null)
      {
        // Item value is present.
        if (item.Values == null || item.Values.Count == 0)
        {
          // No children. Apply the relevant binding.
          ApplyBinding(element, item);
        }
        else
        {
          // Apply the bindings of each child.
          foreach (var child in item.Values)
          {
            if (item.Property != null || item.Binding != null)
              throw new ArgumentException("A SetterValueBindingHelper with Values may not have its Property or Binding set.");

            if (child.Values.Count != 0)
              throw new ArgumentException("Values of a SetterValueBindingHelper may not have Values themselves.");

            ApplyBinding(element, child);
          }
        }
      }
    }


    /// <summary>
    /// Applies the <see cref="Binding"/> represented by the <see cref="SetterValueBindingHelper"/>.
    /// </summary>
    /// <param name="element">The element to apply the <see cref="Binding"/> to.</param>
    /// <param name="item">
    /// <see cref="SetterValueBindingHelper"/> representing the <see cref="Binding"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <see cref="SetterValueBindingHelper"/>'s <see cref="Property"/> and <see cref="Binding"/> 
    /// must both be set to non-null values.
    /// </exception>
    private static void ApplyBinding(FrameworkElement element, SetterValueBindingHelper item)
    {
      if ((item.Property == null) || (item.Binding == null))
        throw new ArgumentException("SetterValueBindingHelper's Property and Binding must both be set to non-null values.");

      // Get the type on which to set the Binding.
      Type type = null;
      if (item.Type == null)
      {
        // No type specified; setting for the specified element.
        type = element.GetType();
      }
      else
      {
        // Try to get the type from the type system.
        type = System.Type.GetType(item.Type);
        if (type == null)
        {
          // Search for the type in the list of assemblies
          foreach (var assembly in AssembliesToSearch)
          {
            // Match on short or full name
            type = assembly.GetTypes()
                           .Where(t => (t.FullName == item.Type) || (t.Name == item.Type))
                           .FirstOrDefault();
            if (type != null)
            {
              // Found; done searching
              break;
            }
          }

          if (type == null)
          {
            if (WindowsHelper.IsInDesignMode)
              return;

            // Unable to find the requested type anywhere
            string message = string.Format(
              CultureInfo.CurrentCulture,
              "Unable to access type \"{0}\". Try using an assembly qualified type name.",
              item.Type);

            throw new ArgumentException(message);
          }
        }
      }

      // Get the DependencyProperty for which to set the Binding
      DependencyProperty property = null;
      var field = type.GetField(item.Property + "Property", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static);
      if (field != null)
        property = field.GetValue(null) as DependencyProperty;

      if (property == null)
      {
        if (WindowsHelper.IsInDesignMode)
          return;

        // Unable to find the requsted property
        string message = string.Format(
          CultureInfo.CurrentCulture,
          "Unable to access DependencyProperty \"{0}\" on type \"{1}\".",
          item.Property,
          type.Name);

        throw new ArgumentException(message);
      }

      // Set the specified Binding on the specified property
      element.SetBinding(property, item.Binding);
    }


    /// <summary>
    /// Gets a sequence of assemblies to search for the provided type name.
    /// </summary>
    private static IEnumerable<Assembly> AssembliesToSearch
    {
      get
      {
        // Start with the System.Windows assembly (home of all core controls)
        yield return typeof(Control).Assembly;

#if SILVERLIGHT && !WINDOWS_PHONE
        // Fall back by trying each of the assemblies in the Deployment's Parts list
        foreach (var part in Deployment.Current.Parts)
        {
          var streamResourceInfo = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative));
          using (var stream = streamResourceInfo.Stream)
          {
            yield return part.Load(stream);
          }
        }
#endif
      }
    }
    #endregion
  }
}
#endif
