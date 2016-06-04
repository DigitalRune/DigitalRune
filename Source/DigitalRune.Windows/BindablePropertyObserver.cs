// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Raises an event when the value of a monitored property is changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class can monitor a single property of a single target object. The
    /// <see cref="ValueChanged"/> event is raised when the property value is changed.
    /// </para>
    /// <para>
    /// Without this class you could use the <see cref="PropertyDescriptor"/> or
    /// <see cref="DependencyPropertyDescriptor"/> and monitor changes using
    /// <see cref="PropertyDescriptor.AddValueChanged"/>. However, this creates a strong reference:
    /// The static property descriptors will keep the monitored object alive until
    /// <see cref="PropertyDescriptor.RemoveValueChanged"/> is called.
    /// </para>
    /// <para>
    /// In contrast, this class uses a data binding to detect property changes. If the monitored
    /// object is a <see cref="DependencyObject"/>, the <see cref="BindablePropertyObserver"/> will
    /// not keep it alive! If the monitored object is a plain CLR object, then it will be kept
    /// alive until the <see cref="BindablePropertyObserver"/> is garbage collected.
    /// </para>
    /// <para>
    /// Please note any objects which subscribe to the <see cref="ValueChanged"/> event will be kept
    /// alive as long as the <see cref="BindablePropertyObserver"/> is alive!
    /// </para>
    /// </remarks>
    public sealed class BindablePropertyObserver : DependencyObject, IDisposable
    {
        // Notes:
        // This class is based on following blog post by Andrew Smith, see
        // https://agsmith.wordpress.com/2008/04/07/propertydescriptor-addvaluechanged-alternative/
        //
        // The BindablePropertyObserver can be used instead of the 
        // PropertyDescriptor.AddValueChanged method.
        // A few notes about the PropertyDescriptor:
        // When does it see property changes?
        // - When an object implements INotifyPropertyChanged.
        // - When the object has a <Value>Changed event. (I have read in several sources that this 
        //   should work but does not work in my tests.)
        // - When a type has custom PropertyDescriptor which overrides OnValueChanged. This is the
        //   case for DependencyObjects. See DependencyPropertyDescriptor.
        // The PropertyDescriptor does not see value changes for a plain CLR object with a property
        // and no special value changed event mechanism! However, even in this case registering
        // a useless AddValueChanged handler keeps the observed object alive!
        // 
        // Bindings can be used to observe property changes similar to the PropertyDescriptor.
        // If the observed object is a plain CLR object, the binding keeps the observed object alive.
        // If the observed object is a DependencyObject, the binding does not keep it alive and
        // it can be collected.
        // If the binding is collected, the observed object can be collected to.
        


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the observed object.
        /// </summary>
        /// <value>The observed object.</value>
        private DependencyObject Source
        {
            get
            {
                return _source.IsAlive ? (DependencyObject)_source.Target : null;
            }
        }
        private readonly WeakReference _source;



        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(BindablePropertyObserver),
            new PropertyMetadata(null, OnValueChanged));

        /// <summary>
        /// Gets or sets the value of the observed property.
        /// This is a dependency property.
        /// </summary>
        /// <value>The value.</value>
        //[Description("Gets or sets the Value property.")]
        //[Category("Custom Properties")]
        private object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        /// <summary>
        /// Occurs when the value of the monitored property is changed.
        /// </summary>
        public event EventHandler ValueChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePropertyObserver"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePropertyObserver"/> class.
        /// </summary>
        /// <param name="source">The object that owns the property.</param>
        /// <param name="propertyPath">The property path of the observed property.</param>
        public BindablePropertyObserver(object source, string propertyPath)
            : this(source, new PropertyPath(propertyPath))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePropertyObserver"/> class.
        /// </summary>
        /// <param name="source">The object that owns the property.</param>
        /// <param name="property">The observed property.</param>
        public BindablePropertyObserver(DependencyObject source, DependencyProperty property)
            : this(source, new PropertyPath(property))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BindablePropertyObserver"/> class.
        /// </summary>
        /// <param name="source">The object that owns the property.</param>
        /// <param name="property">The property path of the observed property.</param>
        public BindablePropertyObserver(object source, PropertyPath property)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            _source = new WeakReference(source);

            Binding binding = new Binding
            {
                Source = source,
                Path = property,
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ValueProperty);
            _source.Target = null;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        /// <summary>
        /// Called when the <see cref="Value"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (BindablePropertyObserver)dependencyObject;
            //object oldValue = eventArgs.OldValue;
            //object newValue = eventArgs.NewValue;
            //target.OnValueChanged(oldValue, newValue);
            target.OnValueChanged();
        }


        /// <summary>
        /// Called when the <see cref="Value"/> property changed.
        /// </summary>
        ///// <param name="oldValue">The old value.</param>
        ///// <param name="newValue">The new value.</param>
        //private void OnValueChanged(object oldValue, object newValue)
        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
