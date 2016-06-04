// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Manages validation errors for objects implementing <see cref="INotifyDataErrorInfo"/>.
    /// </summary>
    public sealed class ErrorsContainer
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Dictionary<string, List<string>> _errorsPerProperty = new Dictionary<string, List<string>>();
        private readonly Action<string> _errorsChanged;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value that indicates whether the object has validation errors.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object currently has validation errors; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool HasErrors
        {
            get
            {
                if (!_hasErrors.HasValue)
                {
                    _hasErrors = false;
                    foreach (var errors in _errorsPerProperty.Values)
                    {
                        if (errors.Count > 0)
                        {
                            _hasErrors = true;
                            break;
                        }
                    }
                }

                return _hasErrors.Value;
            }
        }
        private bool? _hasErrors;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorsContainer"/> class.
        /// </summary>
        /// <param name="errorsChanged">
        /// A callback method that is called when the errors changed.
        /// </param>
        public ErrorsContainer(Action<string> errorsChanged)
        {
            if (errorsChanged == null)
                throw new ArgumentNullException(nameof(errorsChanged));

            _errorsChanged = errorsChanged;
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
            if (propertyName == null)
                propertyName = string.Empty;

            List<string> errors;
            if (_errorsPerProperty.TryGetValue(propertyName, out errors))
                return errors;

            return Enumerable.Empty<string>();
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
            return _errorsPerProperty;
        }


        /// <summary>
        /// Adds a validation error for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property; or <see langword="null"/> or <see cref="string.Empty"/> to add
        /// an entity-level error.
        /// </param>
        /// <param name="error">The validation error.</param>
        public void AddError(string propertyName, string error)
        {
            if (propertyName == null)
                propertyName = string.Empty;

            List<string> errors;
            if (!_errorsPerProperty.TryGetValue(propertyName, out errors))
            {
                errors = new List<string>();
                _errorsPerProperty.Add(propertyName, errors);
            }

            if (errors.Contains(error))
                return;

            errors.Add(error);
            _hasErrors = true;
            _errorsChanged(propertyName);
        }


        /// <summary>
        /// Removes a validation error for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property; or <see langword="null"/> or <see cref="string.Empty"/> to
        /// remove an entity-level error.
        /// </param>
        /// <param name="validationError">The validation error.</param>
        public void RemoveError(string propertyName, string validationError)
        {
            if (propertyName == null)
                propertyName = string.Empty;

            List<string> errors;
            if (!_errorsPerProperty.TryGetValue(propertyName, out errors))
                return;

            if (!errors.Contains(validationError))
                return;

            errors.Remove(validationError);
            _hasErrors = null;
            _errorsChanged(propertyName);
        }


        /// <summary>
        /// Removes all validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property; or <see langword="null"/> or <see cref="string.Empty"/> to
        /// remove entity-level errors.
        /// </param>
        /// <remarks>
        /// Clarification: <c>ClearErrors(null)</c> clears only entity-level errors. Validation
        /// errors for specific properties are not removed.
        /// </remarks>
        public void ClearErrors(string propertyName)
        {
            if (propertyName == null)
                propertyName = string.Empty;

            List<string> errors;
            if (!_errorsPerProperty.TryGetValue(propertyName, out errors))
                return;

            if (errors.Count == 0)
                return;

            errors.Clear();
            _hasErrors = null;
            _errorsChanged(propertyName);
        }


        /// <summary>
        /// Sets the validation errors for a specified property or for the entire object.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property; or <see langword="null"/> or <see cref="string.Empty"/> to
        /// remove entity-level errors.
        /// </param>
        /// <param name="validationErrors">
        /// The validation errors. Can be <see langword="null"/> or an empty collection to remove
        /// any validation errors. (The specified enumerable needs to support multiple
        /// enumerations.)
        /// </param>
        public void SetErrors(string propertyName, IEnumerable<string> validationErrors)
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (propertyName == null)
                propertyName = string.Empty;

            if (validationErrors == null || !validationErrors.Any())
            {
                ClearErrors(propertyName);
            }
            else
            {
                List<string> errors;
                if (!_errorsPerProperty.TryGetValue(propertyName, out errors))
                {
                    errors = new List<string>();
                    _errorsPerProperty.Add(propertyName, errors);
                }

                errors.Clear();
                errors.AddRange(validationErrors);
                _hasErrors = true;
                _errorsChanged(propertyName);
            }
            // ReSharper restore PossibleMultipleEnumeration
        }
        #endregion
    }
}
