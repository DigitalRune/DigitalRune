// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Shows an Exception window, which displays an exception message.
    /// </summary>
    public static class ExceptionHelper
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Shows the exception window and returns when the window is closed.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="applicationName">Optional: The name of the application.</param>
        /// <param name="email">Optional: The error report recipient.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        public static void ShowException(Exception exception, string applicationName = null, string email = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var window = CreateWindow(exception, applicationName, email);

            if (Application.Current == null)
            {
                // No WPF application is running in the application domain.
                Logger.Debug("Starting Exception application.");
                var application = new Application();
                application.Run(window);
            }
            else
            {
                // WPF application is already running in the application domain.
                Logger.Debug("Showing Exception window.");
                window.ShowDialog();
            }
        }


        private static Window CreateWindow(Exception exception, string applicationName, string email)
        {
            var window = new Window
            {
                Title = string.IsNullOrEmpty(applicationName)
                        ? "Unexpected Error"
                        : Invariant($"Unexpected Error in {applicationName}"),
                Width = 640,
                Height = 480,
                MinWidth = 410,
            };
            var control = new ExceptionControl
            {
                ApplicationName = applicationName,
                Exception = exception,
                Email = email,
            };
            window.Content = control;
            control.Close += (s, e) => window.Close();
            window.Closed += (s, e) => Logger.Debug("Exception window closed.");
            return window;
        }
        #endregion
    }
}
