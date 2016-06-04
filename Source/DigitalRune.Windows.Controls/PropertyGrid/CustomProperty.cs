// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a custom property shown in a <see cref="PropertyGrid"/>.
    /// </summary>
    public class CustomProperty : ObservableObject, IProperty
    {
        // Notes:
        // - We could create generic CustomProperty<T>. 
        //   Pros: Easier to set value from code. PropertyType is determined automatically.
        //   Cons: Data templates have to use converter, e.g. to convert from double to int.
        //   --> The non-generic version is better for data binding because it takes care of
        //   the conversion!


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>The category.</value>
        public string Category
        {
            get { return _category; }
            set { SetProperty(ref _category, value); }
        }
        private string _category = "Misc";


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        private string _description;


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        private string _name;


        /// <summary>
        /// Gets or sets a value indicating whether this instance is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is read-only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value); }
        }
        private bool _isReadOnly;


        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The specified type is not compatible with <see cref="Value"/>.
        /// </exception>
        public Type PropertyType
        {
            get { return _propertyType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (Value != null && !value.IsInstanceOfType(Value))
                    throw new ArgumentException("The specified type is not compatible with the value of the property.");

                SetProperty(ref _propertyType, value);
            }
        }
        private Type _propertyType;


        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is not assignable to the property.
        /// </exception>
        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, PropertyGridHelper.Convert(value, PropertyType)); }
        }
        private object _value;


        /// <inheritdoc/>
        public object DataTemplateKey { get; set; }


        /// <summary>
        /// Gets a value indicating whether this property can be <see cref="Reset"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this property can be reset; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// The default value is <see langword="false"/>. This value must be set manually if the
        /// <see cref="Reset"/> method should be enabled for this property and a valid
        /// <see cref="DefaultValue"/> must be set.
        /// </para>
        /// </remarks>
        public bool CanReset
        {
            get { return _canReset; }
            set { SetProperty(ref _canReset, value); }
        }
        private bool _canReset;


        /// <summary>
        /// Gets or sets the default value to which this property is set when
        /// <see cref="Reset"/> is called.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { SetProperty(ref _defaultValue, PropertyGridHelper.Convert(value, PropertyType)); }
        }
        private object _defaultValue;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class of type 
        /// <see cref="object"/>.
        /// </summary>
        public CustomProperty()
        {
            _propertyType = typeof(object);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> with the specified name
        /// and value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// The <see cref="PropertyType"/> is automatically initialized. If <paramref name="value"/>
        /// is <see langword="null"/>, the <see cref="PropertyType"/> is set to
        /// <see cref="object"/>.
        /// </remarks>
        public CustomProperty(string name, object value)
        {
            _name = name;
            _propertyType = value?.GetType() ?? typeof(object);
            _value = value;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void Reset()
        {
            if (CanReset)
                Value = DefaultValue;
        }
        #endregion
    }
}
