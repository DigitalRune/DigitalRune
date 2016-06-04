// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Errors
{
    /// <summary>
    /// Represents an item in the Errors window.
    /// </summary>
    public class Error : ObservableObject
    {
        // Note: ErrorType is read-only to make it easier for the ErrorsViewModel to keep the
        // number of errors/warnings/messages up-to-date.


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the error type.
        /// </summary>
        /// <value>The error type.</value>
        public ErrorType ErrorType { get; }


        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        private string _description;


        /// <summary>
        /// Gets the location (e.g. file name).
        /// </summary>
        /// <value>The location (e.g. file name).</value>
        public string Location
        {
            get { return _location; }
            set { SetProperty(ref _location, value); }
        }
        private string _location;


        /// <summary>
        /// Gets the line number.
        /// </summary>
        /// <value>The line number.</value>
        public int? Line
        {
            get { return _line; }
            set { SetProperty(ref _line, value); }
        }
        private int? _line;


        /// <summary>
        /// Gets the column number.
        /// </summary>
        /// <value>The column number.</value>
        public int? Column
        {
            get { return _column; }
            set { SetProperty(ref _column, value); }
        }
        private int? _column;


        /// <summary>
        /// Gets or sets user-defined data.
        /// </summary>
        /// <value>The user-defined data.</value>
        public object UserData
        {
            get { return _userData; }
            set { SetProperty(ref _userData, value); }
        }
        private object _userData;


        /// <summary>
        /// Gets or sets the command that is executed to navigate to the error location.
        /// </summary>
        /// <value>
        /// The command that is executed to navigate to the error location. If this property is
        /// <see langword="null"/> (default value), then the errors service will open the
        /// document that matches the <see cref="Location"/>.
        /// </value>
        public DelegateCommand<Error> GoToLocationCommand
        {
            get { return _goToLocationCommand; }
            set { SetProperty(ref _goToLocationCommand, value); }
        }
        private DelegateCommand<Error> _goToLocationCommand;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="errorType">The type of the item.</param>
        /// <param name="description">The description.</param>
        /// <param name="location">
        /// The location, e.g. the file name. Can be <see langword="null"/>.
        /// </param>
        /// <param name="line">
        /// The line number. Use <see langword="null"/> to show no line number.
        /// </param>
        /// <param name="column">
        /// The column number. Use <see langword="null"/> to show no column number.
        /// </param>
        public Error(ErrorType errorType, string description, string location = null, int? line = null, int? column = null)
        {
            ErrorType = errorType;
            _description = description;
            _location = location;
            _line = line;
            _column = column;
        }
        #endregion
    }
}
