// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Reflection;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a field of an instance (any CLR object) shown in a <see cref="PropertyGrid"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type describes the field of an instance (any CLR object) using reflection.
    /// </para>
    /// <para>
    /// Unlike the <see cref="ReflectedProperty"/>, this class does not monitor the instance field.
    /// <see cref="Update"/> has to be called manually to update the value.
    /// </para>
    /// </remarks>
    public class ReflectedField : IReflectedProperty
    {
        // Exception handling:
        // Reading the Value can cause exceptions in some cases, e.g. the Normalized property
        // of a zero Vector3F throws an exception. Therefore we catch the exceptions from the
        // Value getter.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private object _oldValue;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Category
        {
            get { return Categories.Default; }
        }


        /// <summary>
        /// Gets the field info.
        /// </summary>
        /// <value>The field info.</value>
        public FieldInfo FieldInfo { get; }


        /// <inheritdoc/>
        public string Description { get { return null; } }


        /// <inheritdoc/>
        public string Name
        {
            get { return FieldInfo.Name; }
        }


        /// <summary>
        /// Gets the instance that owns the property.
        /// </summary>
        /// <value>The instance that owns the property.</value>
        public object Instance { get; }


        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get
            {
                return FieldInfo.IsInitOnly 
                       || FieldInfo.IsLiteral
                       || Instance.GetType().IsValueType;  // For value types we would only update a copy...
            }
        }


        /// <inheritdoc/>
        public Type PropertyType
        {
            get { return FieldInfo.FieldType; }
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
            get { return FieldInfo.GetValue(Instance); }
            set
            {
                object currentValue = Value;
                value = PropertyGridHelper.Convert(value, PropertyType);
                if (Equals(currentValue, value))
                    return;

                FieldInfo.SetValue(Instance, value);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }


        /// <inheritdoc/>
        public object DataTemplateKey { get { return null; } }


        /// <inheritdoc/>
        public bool CanReset { get { return false; } }


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectedField"/> class.
        /// </summary>
        /// <param name="instance">The object instance.</param>
        /// <param name="field">The field info.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="instance"/> or <paramref name="field"/> is <see langword="null"/>.
        /// </exception>
        public ReflectedField(object instance, FieldInfo field)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            Instance = instance;
            FieldInfo = field;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

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
                }
            }
        }


        /// <inheritdoc/>
        public void Reset()
        {
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
