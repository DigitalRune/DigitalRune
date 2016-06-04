// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using NLog;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Creates and destroys a temporary directory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The temp directory is created in the constructor of this class. The temp directory is
    /// deleted when this class is disposed.
    /// </para>
    /// <para>
    /// Creation and deletion of temporary directories using this class is thread-safe.
    /// </para>
    /// </remarks>
    public sealed class TempDirectoryHelper : IDisposable
    {
        // Notes:
        // Format of the temp directory:
        //      %Temp%\<application name>\<process ID>\<folder name> + <index>
        // For example:
        //      C:\Users\UserName\AppData\Local\Temp\DigitalRune.EditorApp\123\ContentBuilder-00001
        // The process ID is used in case several program instances are running parallel.
        // The index is a unique index that is used in case several callers use the same 
        // folderName (e.g. if several "XNAContentBuilder" folders are needed in parallel.)
        //
        // The class tries to delete temp directories of past processes.
        // Cases where temp directories are not deleted:
        // - Debugging in Visual Studio


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static int _nextIndex = -1;

        private static readonly object _lock = new object();

        // e.g. C:\Users\UserName\AppData\Local\Temp\DigitalRune.EditorApp\
        private readonly string _applicationDirectoryName;

        // e.g. C:\Users\UserName\AppData\Local\Temp\DigitalRune.EditorApp\123\
        private readonly string _processDirectoryName;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------


        /// <summary>
        /// Gets the name (full path) of the temporary directory.
        /// </summary>
        /// <value>The name (full path) of the temporary directory.</value>
        public string TempDirectoryName { get; }


        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TempDirectoryHelper"/> class.
        /// </summary>
        /// <param name="applicationName">The unique name of the application.</param>
        /// <param name="folderName">
        /// The name of the temp folder. (Info will be added to this folder name to make it unique.)
        /// </param>
        public TempDirectoryHelper(string applicationName, string folderName)
        {
            // %TEMP%\<application name>\
            _applicationDirectoryName = Path.Combine(Path.GetTempPath(), applicationName);

            // %TEMP%\<application name>\<process ID>
            int processId = Process.GetCurrentProcess().Id;
            _processDirectoryName = Path.Combine(_applicationDirectoryName, processId.ToString(CultureInfo.InvariantCulture));

            // Add <folderName> + <index>.
            var index = Interlocked.Increment(ref _nextIndex);
            TempDirectoryName = Path.Combine(
                _processDirectoryName,
                string.Format(CultureInfo.InvariantCulture, "{0}-{1:D4}", folderName, index));

            // Create our temporary directory.
            Logger.Info("Creating temp directory:" + TempDirectoryName);
            lock (_lock)
                Directory.CreateDirectory(TempDirectoryName);

            PurgeStaleProcessDirectories();
        }


        /// <summary>
        /// Releases unmanaged resources before an instance of the <see cref="TempDirectoryHelper"/>
        /// class is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method releases unmanaged resources by calling the virtual
        /// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        /// </remarks>
        ~TempDirectoryHelper()
        {
            Dispose(false);
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="TempDirectoryHelper"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the
        /// <see cref="TempDirectoryHelper"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Release unmanaged resources.
                TryDeleteDirectory(TempDirectoryName, true);
                TryDeleteApplicationAndProcessDirectory();

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        // Try to delete any directories that were left over from other application instances.
        private void PurgeStaleProcessDirectories()
        {
            lock (_lock)
            {
                // Check all process directories of our application.
                foreach (string directory in Directory.GetDirectories(_applicationDirectoryName))
                {
                    // The process directory name is the ID of the process which created it.
                    int processId;
                    if (int.TryParse(Path.GetFileName(directory), out processId))
                    {
                        try
                        {
                            // Try to get creating process.
                            Process.GetProcessById(processId);
                        }
                        catch (ArgumentException)
                        {
                            // Process is gone, delete directory.
                            TryDeleteDirectory(directory, true);
                        }
                    }
                }
            }
        }


        // The "last" temp directory helper deletes the application directory.
        private void TryDeleteApplicationAndProcessDirectory()
        {
            // If there are no other subfolders in the application directory, we can remove it.
            lock (_lock)
            {
                if (Directory.GetDirectories(_processDirectoryName).Length == 0)
                {
                    // Process directory is empty. Delete it.
                    TryDeleteDirectory(_processDirectoryName, false);

                    // Try to delete application directory.
                    if (Directory.GetDirectories(_applicationDirectoryName).Length == 0)
                        TryDeleteDirectory(_applicationDirectoryName, false);
                }
            }
        }


        private static void TryDeleteDirectory(string path, bool recursive)
        {
            lock (_lock)
            {
                try
                {
                    Logger.Info("Deleting temp directory: " + path);
                    Directory.Delete(path, recursive);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Could not delete temp directory: " + path);
                }
            }
        }
        #endregion
    }
}
