// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a property of an instance (any CLR object) shown in a <see cref="PropertyGrid"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type describes the property of an instance (any CLR object) using reflection.
    /// </para>
    /// <para>
    /// The property will try to monitor the instance property and updates when the property value
    /// changes. This works only when the object instance implements
    /// <see cref="INotifyPropertyChanged"/> and in some other cases, e.g. if the monitored object
    /// is a <see cref="DependencyProperty"/>. In all other cases, <see cref="Update"/> has to be
    /// called manually to update the value.
    /// </para>
    /// <para>
    /// Please note that the <see cref="ReflectedProperty"/> may keep the object instance alive
    /// (i.e. it can only be garbage collected if the <see cref="ReflectedProperty"/> was collected
    /// too).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ReflectedProperty : IReflectedProperty
    {
        // Exception handling:
        // Reading the Value can cause exceptions in some cases, e.g. the Normalized property
        // of a zero Vector3F throws an exception. Therefore we catch the exceptions from the
        // Value getter.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private object _oldValue;

        // BindablePropertyObserver implements IDisposable, but we do not need to call it. The
        // BindablePropertyObserver will be garbage collected with the ReflectedProperty and then
        // the source will also be free to collect.
        // The ReflectedProperty needs to reference the BindablePropertyObserver to keep it alive.

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly BindablePropertyObserver _bindablePropertyObserver;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Category
        {
            get { return Descriptor.Category; }
        }


        /// <summary>
        /// Gets the property descriptor.
        /// </summary>
        /// <value>The property descriptor.</value>
        public PropertyDescriptor Descriptor { get; }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        public string Description
        {
            get
            {
                // Avoid returning an empty string which could lead to an annoying empty tool tip
                // when bound in XAML.
                var description = Descriptor.Description;
                return string.IsNullOrEmpty(description) ? null : description;
            }
        }


        /// <inheritdoc/>
        public string Name
        {
            get { return Descriptor.DisplayName ?? Descriptor.Name; }
        }


        /// <summary>
        /// Gets the instance that owns the property.
        /// </summary>
        /// <value>The instance that owns the property.</value>
        public object Instance { get; }


        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return Descriptor.IsReadOnly; }
        }


        /// <inheritdoc/>
        public Type PropertyType
        {
            get { return Descriptor.PropertyType; }
        }


        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <exception cref="ArgumentException">
        /// The specified value is not assignable to the property.
        /// </exception>
        public object Value
        {
            get { return Descriptor.GetValue(Instance); }
            set
            {
                object currentValue = Value;
                value = PropertyGridHelper.Convert(value, PropertyType);
                if (Equals(currentValue, value))
                    return;

                Descriptor.SetValue(Instance, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }


        /// <inheritdoc/>
        public object DataTemplateKey { get { return null; } }


        /// <inheritdoc/>
        public bool CanReset
        {
            get { return !IsReadOnly && Descriptor.CanResetValue(Instance); }
        }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedProperty"/> class.
        /// </summary>
        /// <param name="instance">The object instance.</param>
        /// <param name="property">The property.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="instance"/> or <paramref name="property"/> is <see langword="null"/>.
        /// </exception>
        public ReflectedProperty(object instance, PropertyDescriptor property)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            Instance = instance;
            Descriptor = property;

            _bindablePropertyObserver = new BindablePropertyObserver(instance, property.Name);
            _bindablePropertyObserver.ValueChanged += OnValueChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
        }


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="PropertyChangedEventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> in
        /// a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method
        /// so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            PropertyChanged?.Invoke(this, eventArgs);

            if (string.IsNullOrEmpty(eventArgs.PropertyName) || eventArgs.PropertyName == nameof(Value))
            {
                try
                {
                    _oldValue = Value;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }


        /// <inheritdoc/>
        public void Reset()
        {
            if (CanReset)
                Descriptor.ResetValue(Instance);
        }


        /// <inheritdoc/>
        public void Update()
        {
            bool hasChanged = false;
            try
            {
                hasChanged = !Equals(Value, _oldValue);
            }
            catch (Exception)
            {
                // ignored
            }

            if (hasChanged)
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
        }
        #endregion
    }
}
