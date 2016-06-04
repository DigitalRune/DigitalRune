// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Listens to file change notifications and raises events when a file of a document is changed.
    /// (Wraps <see cref="FileSystemWatcher"/>.)
    /// </summary>
    internal class FileWatcher : FileSystemWatcher
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _fileName;
        private int _suspendCounter;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Gets the document associated with the file.
        /// </summary>
        /// <value>The document associated with the file.</value>
        public Document Document { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcher"/> class.
        /// </summary>
        /// <param name="document">The document that shall be watched.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Cannot monitor file. The specified document is untitled or does not contain a valid file
        /// URI.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Cannot monitor file. File not found.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public FileWatcher(Document document)
        {
            // Check whether document is valid.
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (document.IsUntitled)
                throw new ArgumentException("Cannot monitor file. The specified document is untitled.");

            Document = document;

            // Check whether document points to a valid file.
            _fileName = document.Uri.LocalPath;
            if (string.IsNullOrEmpty(_fileName))
                throw new ArgumentException("Cannot monitor file. The specified document does not contain a valid file URI.");

            Logger.Debug(CultureInfo.InvariantCulture, "Initializing FileWatcher for file \"{0}\".", _fileName);

            if (!File.Exists(_fileName))
                Logger.Warn(CultureInfo.InvariantCulture, "Monitoring file \"{0}\". File not found.", _fileName);

            // Configure base FileSystemWatcher.
            Path = System.IO.Path.GetDirectoryName(_fileName);
            Filter = System.IO.Path.GetFileName(_fileName);
            NotifyFilter = NotifyFilters.LastWrite;
            EnableRaisingEvents = true;
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="FileWatcher"/>
        /// class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        Debug.Assert(_suspendCounter == 0, "Disposing suspended file watcher.");

                        // Events need to be disabled before disposal. Otherwise, the program hangs
                        // in Dispose().
                        EnableRaisingEvents = false;
                    }

                    IsDisposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
                Logger.Debug(CultureInfo.InvariantCulture, "FileWatcher of file \"{0}\" disposed.", _fileName);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Suspends file monitoring.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public void Suspend()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Suspending FileWatcher of file \"{0}\".", _fileName);

            _suspendCounter++;
            EnableRaisingEvents = false;
        }


        /// <summary>
        /// Resumes file monitoring.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <see cref="Resume"/> called without prior call of <see cref="Suspend"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public void Resume()
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Trying to resume FileWatcher of file \"{0}\".", _fileName);

            _suspendCounter--;
            if (_suspendCounter < 0)
                throw new InvalidOperationException("FileWatcher.Resume() called without prior call of Suspend().");

            if (_suspendCounter == 0)
            {
                EnableRaisingEvents = true;
                Logger.Debug(CultureInfo.InvariantCulture, "FileWatcher of file \"{0}\" resumed.", _fileName);
            }
        }
        #endregion
    }
}
