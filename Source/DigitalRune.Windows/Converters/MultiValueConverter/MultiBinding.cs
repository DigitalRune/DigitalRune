// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;


namespace DigitalRune.Windows
{
  /// <summary>
  /// Describes a collection of <see cref="Binding"/> objects attached to a single binding target 
  /// property. 
  /// </summary>
  /// <remarks>
  /// This code is derived from Colin Eberhardt and Stefan Olson, see 
  /// http://www.scottlogic.co.uk/blog/colin/2010/05/silverlight-multibinding-solution-for-silverlight-4/
  /// and
  /// http://www.olsonsoft.com/blogs/stefanolson/post/Updates-to-Silverlight-Multi-binding-support.aspx
  /// </remarks>
  /// <example>
  /// Here is an usage example of a multi binding and a multi value converter:
  /// <code lang="xaml">
  /// <![CDATA[
  /// <TextBlock>
  ///   <dr:BindingHelper.MultiBindings>
  ///     <dr:MultiBindings>
  ///       <dr:MultiBinding TargetProperty="Text" Converter="{StaticResource FormattingConverter}">
  ///         <dr:BindingCollection>
  ///           <Binding Path="Strings.WidthLabel" Source="{StaticResource LocalizedResources}"/>                            
  ///           <Binding Path="Width"/>
  ///         </dr:BindingCollection>
  ///       </dr:MultiBinding>              
  ///     </dr:MultiBindings>  
  ///  </dr:BindingHelper.MultiBindings>
  /// </TextBlock>
  /// ]]>
  /// </code>
  /// </example>
  [ContentProperty("Bindings")]
  public class MultiBinding : Panel, INotifyPropertyChanged
  {
    // This class is a Panel that contains BindingSlaves. A Panel is used because then the
    // BindingSlaves will inherit the DataContext and we don't have to copy the DataContext
    // manually to all BindingSlaves.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// The target property on the element which this <see cref="MultiBinding"/> is associated with.
    /// </summary>
    public string TargetProperty { get; set; }

    /// <summary>
    /// The <see cref="IMultiValueConverter"/> which is invoked to compute the result of the 
    /// multiple bindings
    /// </summary>
    public IMultiValueConverter Converter { get; set; }

    /// <summary>
    /// A parameter supplied to the converter
    /// </summary>
    public object ConverterParameter { get; set; }


    /// <summary>
    /// The bindings, the result of which are supplied to the converter.
    /// </summary>
    public BindingCollection Bindings { get; set; }


    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Dependency Properties & Routed Events
    //--------------------------------------------------------------

    /// <summary>
    /// Identifies the <strong>ConvertedValue</strong> dependency property.
    /// </summary>
    public static readonly DependencyProperty ConvertedValueProperty = DependencyProperty.Register(
      "ConvertedValue", typeof(object), typeof(MultiBinding), 
      new PropertyMetadata(null, OnConvertedValueChanged));

    /// <summary>
    /// Gets or sets the converted value which is the output of the associated 
    /// <see cref="Converter"/>. 
    /// This is a dependency property.
    /// </summary>
    /// <value>The converted value.</value>
    [Description("Gets or sets the converted value which is the output of the associated converter.")]
    [Category(Categories.Default)]
    public object ConvertedValue
    {
      get { return GetValue(ConvertedValueProperty); }
      set { SetValue(ConvertedValueProperty, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiBinding"/> class.
    /// </summary>
    public MultiBinding()
    {
      Bindings = new BindingCollection();
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private static void OnConvertedValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
      MultiBinding multiBinding = (MultiBinding)dependencyObject;
      multiBinding.OnPropertyChanged("ConvertedValue");
    }


    /// <summary>
    /// Creates a BindingSlave for each Binding and binds the Value
    /// accordingly.
    /// </summary>
    internal void Initialize()
    {
      Children.Clear();

      if (Bindings != null)
      {
        foreach (Binding binding in Bindings.OfType<Binding>())
        {
          BindingSlave slave = new BindingSlave();
          slave.SetBinding(BindingSlave.ValueProperty, binding);
          slave.PropertyChanged += OnSlaveValueChanged;
          Children.Add(slave);
        }
      }
    }


    /// <summary>
    /// Invoked when any of the BindingSlave's Value property changes.
    /// </summary>
    private void OnSlaveValueChanged(object sender, PropertyChangedEventArgs e)
    {
      UpdateConvertedValue();
    }


    /// <summary>
    /// Uses the Converter to update the ConvertedValue in order to reflect
    /// the current state of the bindings.
    /// </summary>
    private void UpdateConvertedValue()
    {
      List<object> values = new List<object>();

      foreach (BindingSlave slave in Children)
        values.Add(slave.Value);

      ConvertedValue = Converter.Convert(values.ToArray(), typeof(object), ConverterParameter, CultureInfo.CurrentCulture);
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
    #endregion
  }
}
