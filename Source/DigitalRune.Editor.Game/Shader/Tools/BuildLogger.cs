// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DigitalRune.Editor.Output;
using DigitalRune.Windows;
using Microsoft.Build.Framework;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Sends MSBuild log messages to the <see cref="IOutputService"/>.
    /// </summary>
    internal class BuildLogger : ILogger
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const string Indentation = "  ";

        private int _indent;
        private readonly IOutputService _outputService;
        private readonly StringBuilder _stringBuilder; // To be used on UI thread!
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the user-defined parameters of the logger.
        /// </summary>
        /// <returns>The logger parameters.</returns>
        public string Parameters { get; set; }


        /// <summary>
        /// Gets or sets the level of detail to show in the event log.
        /// </summary>
        /// <value>
        /// One of the enumeration values. The default is <see cref="LoggerVerbosity.Normal"/>.
        /// </value>
        public LoggerVerbosity Verbosity { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildLogger"/> class.
        /// </summary>
        /// <param name="outputService">The output service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="outputService"/> is <see langword="null"/>.
        /// </exception>
        public BuildLogger(IOutputService outputService)
        {
            if (outputService == null)
                throw new ArgumentNullException(nameof(outputService));

            _outputService = outputService;
            _stringBuilder = new StringBuilder();
            Verbosity = LoggerVerbosity.Normal;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        /// <summary>
        /// Subscribes to specific logger events. 
        /// </summary>
        /// <param name="eventSource">The events available to loggers.</param>
        /// <remarks>
        /// This method is called when the logger is registered with the build engine, before any 
        /// events are raised.
        /// </remarks>
        public void Initialize(IEventSource eventSource)
        {
            if (eventSource == null)
                return;

            eventSource.BuildStarted += OnBuildStarted;
            eventSource.BuildFinished += OnBuildFinished;
            eventSource.TargetStarted += OnTargetStarted;
            eventSource.TargetFinished += OnTargetFinished;
            eventSource.ProjectStarted += OnProjectStarted;
            eventSource.ProjectFinished += OnProjectFinished;
            eventSource.TaskStarted += OnTaskStarted;
            eventSource.TaskFinished += OnTaskFinished;
            eventSource.StatusEventRaised += OnStatusEventRaised;
            eventSource.ErrorRaised += OnErrorRaised;
            eventSource.WarningRaised += OnWarningRaised;
            eventSource.MessageRaised += OnMessageRaised;
            eventSource.CustomEventRaised += OnCustomEventRaised;
        }


        private void OnBuildStarted(object sender, BuildStartedEventArgs eventArgs)
        {
        }


        private void OnBuildFinished(object sender, BuildFinishedEventArgs eventArgs)
        {
            _outputService.WriteLine(string.Empty);
        }


        private void OnTargetStarted(object sender, TargetStartedEventArgs eventArgs)
        {
        }


        private void OnTargetFinished(object sender, TargetFinishedEventArgs eventArgs)
        {
        }


        private void OnProjectStarted(object sender, ProjectStartedEventArgs eventArgs)
        {
            // ProjectStartedEventArgs adds ProjectFile, TargetNames
            // Just the regular message string is good enough here, so just display that.
            WriteLine(String.Empty, eventArgs);
            _indent++;
        }


        private void OnProjectFinished(object sender, ProjectFinishedEventArgs eventArgs)
        {
            _indent--;
            WriteLine(String.Empty, eventArgs);
        }


        private void OnTaskStarted(object sender, TaskStartedEventArgs eventArgs)
        {
            // TaskStartedEventArgs adds ProjectFile, TaskFile, TaskName
            // To keep this log clean, this logger will ignore these events.
        }


        private void OnTaskFinished(object sender, TaskFinishedEventArgs eventArgs)
        {
        }


        private void OnStatusEventRaised(object sender, BuildStatusEventArgs eventArgs)
        {
        }


        private void OnErrorRaised(object sender, BuildErrorEventArgs eventArgs)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = String.Format(CultureInfo.InvariantCulture, "ERROR {0}({1},{2}): ", eventArgs.File, eventArgs.LineNumber, eventArgs.ColumnNumber);
            WriteLineWithSenderAndMessage(line, eventArgs);
        }


        private void OnWarningRaised(object sender, BuildWarningEventArgs eventArgs)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string message = String.Format(CultureInfo.InvariantCulture, "Warning {0}({1},{2}): ", eventArgs.File, eventArgs.LineNumber, eventArgs.ColumnNumber);
            WriteLineWithSenderAndMessage(message, eventArgs);
        }


        private void OnMessageRaised(object sender, BuildMessageEventArgs eventArgs)
        {
            // BuildMessageEventArgs adds Importance to BuildEventArgs
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message
            if ((eventArgs.Importance == MessageImportance.High && Verbosity >= LoggerVerbosity.Minimal)
                || (eventArgs.Importance == MessageImportance.Normal && Verbosity >= LoggerVerbosity.Normal)
                || (eventArgs.Importance == MessageImportance.Low && Verbosity >= LoggerVerbosity.Detailed))
            {
                WriteLineWithSenderAndMessage(String.Empty, eventArgs);
            }
        }


        private void OnCustomEventRaised(object sender, CustomBuildEventArgs eventArgs)
        {
        }


        /// <summary>
        /// Write the specified text and the message in the <see cref="BuildEventArgs"/> to the 
        /// output window, adding the sender name.
        /// </summary>
        private void WriteLineWithSenderAndMessage(string text, BuildEventArgs eventArgs)
        {
            if (String.Compare(eventArgs.SenderName, "MSBuild", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // The sender is MSBuild.
                WriteLine(text, eventArgs);
            }
            else
            {
                text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", eventArgs.SenderName, text);
                WriteLine(text, eventArgs);
            }
        }



        /// <summary>
        /// Writes the specified text and the message in the <see cref="BuildEventArgs"/> to the 
        /// output window.
        /// </summary>
        private void WriteLine(string text, BuildEventArgs eventArgs)
        {
            // Copy field into local variable because the closure below should use the
            // current indentation value.
            int indent = _indent;

            WindowsHelper.CheckBeginInvokeOnUI(() =>
            {
                Debug.Assert(_stringBuilder.Length == 0, "StringBuilder should have been cleared.");

                for (int i = 0; i < indent; i++)
                    _stringBuilder.Append(Indentation);

                _stringBuilder.Append(text);
                _stringBuilder.Append(eventArgs.Message);
                _outputService.WriteLine(_stringBuilder.ToString());
                _stringBuilder.Clear();
            });
        }


        /// <summary>
        /// Releases the resources allocated to the logger at the time of initialization or during
        /// the build.
        /// </summary>
        /// <remarks>
        /// This method is called when the logger is unregistered from the engine, after all events
        /// are raised. A host of MSBuild typically unregisters loggers immediately before quitting.
        /// </remarks>
        public void Shutdown()
        {
        }
        #endregion
    }
}
