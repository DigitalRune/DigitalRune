// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using NLog;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Shows the exception message.
    /// </summary>
    internal partial class ExceptionControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name of the application that threw the exception.
        /// </summary>
        /// <value>The name of the application that threw the exception.</value>
        public string ApplicationName { get; set; }


        /// <summary>
        /// Gets or sets the email where the exception should be reported.
        /// </summary>
        /// <value>The email where the exception should be reported.</value>
        public string Email { get; set; }


        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }


        /// <summary>
        /// Occurs when the Close button is clicked.
        /// </summary>
        public event EventHandler<EventArgs> Close;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionControl"/> class.
        /// </summary>
        public ExceptionControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the window was loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            Logger.Debug("Exception window loaded.");
            Logger.Debug(CultureInfo.InvariantCulture, "ApplicationName = {0}", ApplicationName);
            Logger.Debug(CultureInfo.InvariantCulture, "Email = {0}", Email);

            // Use message string as the data context.
            string message = GetExceptionMessage(Exception);
            Logger.Debug(CultureInfo.InvariantCulture, "Exception = {0}", message);
            DataContext = message;

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(Email))
                ReportButton.IsEnabled = false;

            if (string.IsNullOrEmpty(message))
                CopyButton.IsEnabled = false;
        }


        private void OnCloseButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            OnClose(EventArgs.Empty);
        }


        /// <summary>
        /// Raises the <see cref="Close"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/>
        /// When overriding <see cref="OnClose"/> in a derived class, be sure to call the base class's
        /// <see cref="OnClose"/> method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnClose(EventArgs eventArgs)
        {
            Close?.Invoke(this, eventArgs);
        }


        private void OnCopyButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            Logger.Info("Copying exception message to clipboard.");
            CopyToClipboard();
        }


        private void CopyToClipboard()
        {
            // Copy the content of the text box to the clipboard.
            MessageTextBox.SelectAll();
            MessageTextBox.Copy();
            MessageTextBox.Select(0, 0);
        }


        private void OnReportButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            Logger.Info("Sending error report via e-mail.");

            try
            {
                CopyToClipboard();

                // We could add the message to the 'body' parameter of the mailto string but the
                // mailto string seems to be limited and the message would be cut off. Instead, we 
                // let the user paste the message into the body.
                // Send message per email.
                //string message = (string)DataContext;

                // For mailto: url we need to replace special characters.
                //message = message.Replace("%", "%25");    // Must be the first replace action!
                //message = message.Replace("\r\n", "%0A");
                //message = message.Replace("\n", "%0A");
                //message = message.Replace("&", "%26");

                // Build mailto: string.
                string navigateUri = string.Format(
                    CultureInfo.InvariantCulture,
                    "mailto:{0}?Subject=Error Report: Exception in {1}&body={2}",
                    Email,
                    (!string.IsNullOrEmpty(ApplicationName) ? ApplicationName : "DigitalRune Application"),
                    "<The error message is in the clipboard. Please paste it (Ctrl + V) into the content of this e-mail.>");

                // Start process with "mailto:...." as the command.
                Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch (Exception exception)
            {
                // Could not report error. Just log it - don't throw a nested exception!
                Logger.Error(exception, "Sending error report per email failed.");
            }
        }


        /// <summary>
        /// Gets the exception message with exception details.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        /// A detailed message describing the exception. If <paramref name="exception"/> is 
        /// <see langword="null"/>, <see langword="null"/> is returned.
        /// </returns>
        private static string GetExceptionMessage(Exception exception)
        {
            if (exception == null)
                return null;

            // Recursively collect all stack traces and inner exceptions.
            var message = new StringBuilder();
            message.Append("An exception has occurred in ");
            message.Append(Assembly.GetEntryAssembly().FullName);
            message.AppendLine(".");
            message.AppendLine();
            message.AppendLine("Exception:");
            while (exception != null)
            {
                message.AppendLine(exception.Message);
                message.AppendLine();
                message.AppendLine(exception.StackTrace);
                message.AppendLine();
                message.AppendLine("Inner exception:");

                // Continue with inner exception.
                exception = exception.InnerException;
            }

            message.AppendLine("-");
            return message.ToString();
        }
        #endregion
    }
}
