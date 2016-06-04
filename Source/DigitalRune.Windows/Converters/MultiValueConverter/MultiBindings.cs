// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Represents a collection of <see cref="MultiBinding"/>s.
  /// </summary>
  /// <remarks>
  /// This code is derived from Colin Eberhardt and Stefan Olson, see 
  /// http://www.scottlogic.co.uk/blog/colin/2010/05/silverlight-multibinding-solution-for-silverlight-4/
  /// and
  /// http://www.olsonsoft.com/blogs/stefanolson/post/Updates-to-Silverlight-Multi-binding-support.aspx
  /// </remarks>
  [ContentProperty("Bindings")]
  public class MultiBindings : FrameworkElement
  {
    // TODO: !Silverlight code path is not tested.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    
    private FrameworkElement _targetElement;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="MultiBinding"/>s.
    /// </summary>
    /// <value>The <see cref="MultiBinding"/>s.</value>
    public ObservableCollection<MultiBinding> Bindings { get; private set; }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiBindings"/> class.
    /// </summary>
    public MultiBindings()
    {
      Bindings = new ObservableCollection<MultiBinding>();
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if !SILVERLIGHT && !WINDOWS_PHONE
    private void Loaded(object sender, RoutedEventArgs e)
    {
      _targetElement.Loaded -= Loaded;
      foreach (MultiBinding binding in Bindings)
      {
        FieldInfo field = _targetElement.GetType().GetField(binding.TargetProperty + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field == null) continue;

        System.Windows.Data.MultiBinding newBinding = new System.Windows.Data.MultiBinding
                                                      {
                                                          Converter = binding.Converter,
                                                          ConverterParameter = binding.ConverterParameter
                                                      };
        foreach (BindingBase bindingBase in binding.Bindings)
        {
          newBinding.Bindings.Add(bindingBase);
        }

        DependencyProperty dp = (DependencyProperty)field.GetValue(_targetElement);

        BindingOperations.SetBinding(_targetElement, dp, newBinding);
      }

    }
#endif


    /// <summary>
    /// Sets the data context.
    /// </summary>
    /// <param name="dataContext">The data context.</param>
    internal void SetDataContext(object dataContext)
    {
      foreach (MultiBinding multiBinding in Bindings)
        multiBinding.DataContext = dataContext;
    }


    internal void Initialize(FrameworkElement element)
    {
      _targetElement = element;

#if !SILVERLIGHT && !WINDOWS_PHONE
      _targetElement.Loaded += Loaded;
#else
      foreach (MultiBinding multiBinding in Bindings)
      {
        multiBinding.Initialize();

        // Find the target dependency property
        Type targetType;
        string targetProperty;

        // Assume it is an attached property if the dot syntax is used.
        if (multiBinding.TargetProperty.Contains("."))
        {
          // Split to find the type and property name.
          string[] parts = multiBinding.TargetProperty.Split('.');
          targetType = Type.GetType(
            "System.Windows.Controls." + parts[0] +
            ", System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e");
          targetProperty = parts[1];
        }
        else
        {
          targetType = element.GetType();
          targetProperty = multiBinding.TargetProperty;
        }

        FieldInfo[] sourceFields = targetType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        FieldInfo targetDependencyPropertyField =
          sourceFields.First(i => i.Name == targetProperty + "Property");
        DependencyProperty targetDependencyProperty =
          targetDependencyPropertyField.GetValue(null) as DependencyProperty;

        // Bind the ConvertedValue of our MultiBinding instance to the target property
        // of our element
        Binding binding = new Binding("ConvertedValue") { Source = multiBinding };
        element.SetBinding(targetDependencyProperty, binding);
      }
#endif
    }
    #endregion
  }
}