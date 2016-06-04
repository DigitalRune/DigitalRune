// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !SILVERLIGHT && !WP8
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides a base class for <see cref="INotifyPropertyChanged"/> and
    /// <see cref="INotifyDataErrorInfo"/> implementations with data annotations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Properties of derived classes may be annotated using data annotation attributes (see
    /// namespace <see cref="N:System.ComponentModel.DataAnnotations"/>). The method
    /// <see cref="SetProperty{T}"/> automatically validates individual properties when they are
    /// changed and raises <see cref="ErrorsChanged"/> events if necessary.
    /// </para>
    /// <para>
    /// To explicitly validate a property with data annotations call <see cref="ValidateProperty"/>.
    /// The method <see cref="ValidateProperties"/> automatically validates all properties.
    /// </para>
    /// <para>
    /// It is not required to use .NET data annotations. It is also possible to validate properties
    /// manually in the derived class and add errors to or remove errors from the
    /// <see cref="ErrorsContainer"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable")]
    public class ValidatableObservableObject : ObservableObject, INotifyDataErrorInfo
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const string HasErrorsPropertyName = "HasErrors";
        #endregion

    
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the container that manages validation errors.
        /// </summary>
        /// <value>The errors container.</value>
        protected ErrorsContainer ErrorsContainer { get; }


        /// <summary>
        /// Gets a value that indicates whether the object has validation errors.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object currently has validation errors; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool HasErrors
        {
            get { return ErrorsContainer.HasErrors; }
        }


        /// <summary>
        /// Occurs when the validation errors have changed for a property or for the entire object.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatableObservableObject"/> class.
        /// </summary>
        public ValidatableObservableObject()
        {
            ErrorsContainer = new ErrorsContainer(propertyName =>
                                                  {
                                                      OnErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
                                                      OnPropertyChanged(new PropertyChangedEventArgs(HasErrorsPropertyName));
                                                  });
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to retrieve validation errors for; or <see langword="null"/> or
        /// <see cref="string.Empty"/> to retrieve entity-level errors.
        /// </param>
        /// <returns>The validation errors for the property or object.</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            return ErrorsContainer.GetErrors(propertyName);
        }


        /// <summary>
        /// Returns all validation errors.
        /// </summary>
        /// <returns>
        /// The validation errors per property. (Entity-level errors are stored with key 
        /// <see cref="string.Empty"/> in the dictionary.)
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Dictionary<string, List<string>> GetAllErrors()
        {
            return ErrorsContainer.GetAllErrors();
        }


        /// <summary>
        /// Raises the <see cref="ErrorsChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="DataErrorsChangedEventArgs"/> object that provides the arguments for the
        /// event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding <see cref="OnErrorsChanged"/>
        /// in a derived class, be sure to call the base class's <see cref="OnErrorsChanged"/>
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs eventArgs)
        {
            ErrorsChanged?.Invoke(this, eventArgs);
        }


        /// <inheritdoc/>
        protected override bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            bool hasPropertyChanged = base.SetProperty(ref field, value, propertyName);
            if (hasPropertyChanged && !string.IsNullOrEmpty(propertyName))
                ValidateProperty(propertyName);

            return hasPropertyChanged;
        }


        /// <summary>
        /// Validates the specified property using data annotation attributes.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>
        /// <see langword="true"/> if the property is valid; otherwise, <see langword="false"/> if
        /// it has errors.
        /// </returns>
        public bool ValidateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            var propertyInfo = GetType().GetRuntimeProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("Invalid property name", propertyName);

            if (!propertyInfo.GetCustomAttributes(typeof(ValidationAttribute)).Any())
                return true;

            var errors = new List<string>();
            bool isValid = TryValidateProperty(propertyInfo, errors);
            ErrorsContainer.SetErrors(propertyInfo.Name, errors);
            return isValid;
        }


        /// <summary>
        /// Validates all properties using data annotation attributes.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if all properties are valid; otherwise, <see langword="false"/>
        /// if it has errors.
        /// </returns>
        /// <remarks>
        /// This method does not check for entity-level errors. It only validates properties using
        /// data annotation attributes.
        /// </remarks>
        public bool ValidateProperties()
        {
            // Get all the properties decorated with the ValidationAttribute.
            var propertiesToValidate = GetType().GetRuntimeProperties()
                                                .Where(c => c.GetCustomAttributes(typeof(ValidationAttribute)).Any());

            bool isValid = true;
            var errors = new List<string>();
            foreach (var propertyInfo in propertiesToValidate)
            {
                errors.Clear();
                isValid &= TryValidateProperty(propertyInfo, errors);
                ErrorsContainer.SetErrors(propertyInfo.Name, errors);
            }

            return isValid;
        }


        private bool TryValidateProperty(PropertyInfo propertyInfo, List<string> propertyErrors)
        {
            Debug.Assert(propertyInfo != null);
            Debug.Assert(propertyErrors != null);

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this) { MemberName = propertyInfo.Name };
            var propertyValue = propertyInfo.GetValue(this);
            bool isValid = Validator.TryValidateProperty(propertyValue, validationContext, validationResults);
            foreach (var validationResult in validationResults)
                propertyErrors.Add(validationResult.ErrorMessage);

            return isValid;
        }
        #endregion
    }
}
#endif
