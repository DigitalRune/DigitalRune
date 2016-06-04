// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        // TODO: Add support for monitoring dependencies. (E.g. document = HTML file, dependencies = CSS/JPG files.)

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly List<FileWatcher> _fileWatchers = new List<FileWatcher>();
        private readonly List<FileChangeRecord> _fileChangeRecords = new List<FileChangeRecord>();
        private bool _applyingFileChanges;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Starts a <see cref="FileSystemWatcher"/> that monitors all file changes for a document.
        /// </summary>
        /// <param name="document">The document which shall be monitored.</param>
        /// <remarks>
        /// <para>
        /// Once the file watcher is started, the document service catches all file changes. A
        /// dialog window notifies the user about any file changes. The user can decide whether she
        /// wants to reload the document from disk or to ignore the file changes.
        /// </para>
        /// <para>
        /// The user is notified about any file changes the next time she accesses the associated
        /// document. That means: If the modified file belongs to the currently active document, the
        /// user is immediately notified about all file changes. When the document is not currently
        /// active the user is notified when he activates a document.
        /// </para>
        /// <para>
        /// The file watcher only registers when the content of the file changes. All other changes
        /// - such as renaming or deleting the file - are ignored.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> You need to call <see cref="SuspendFileWatcher"/> before you
        /// change a file. After modifying the file call <see cref="ResumeFileWatcher"/> to continue
        /// monitoring the file.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="StopFileWatcher"/>
        /// <seealso cref="SuspendFileWatcher"/>
        /// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void StartFileWatcher(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            Logger.Debug(CultureInfo.InvariantCulture, "Starting file watcher for document \"{0}\".", document.GetName());

            if (_fileWatchers.Exists(fw => fw.Document == document))
            {
                Logger.Warn(CultureInfo.InvariantCulture, "File watcher for document \"{0}\" already exists.", document.GetName());
                return;
            }

            try
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Creating new file watcher for document \"{0}\".", document.GetName());
                var fileWatcher = new FileWatcher(document);
                fileWatcher.Changed += OnFileChanged;
                _fileWatchers.Add(fileWatcher);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, CultureInfo.InvariantCulture, "Unable to start file watcher for document \"{0}\".", document.GetName());
                throw;
            }
        }


        /// <summary>
        /// Stops the <see cref="FileSystemWatcher"/> for a document.
        /// </summary>
        /// <param name="document">The document which is monitored.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="StartFileWatcher"/>
        /// <seealso cref="SuspendFileWatcher"/>
        /// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        internal void StopFileWatcher(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            Logger.Debug(CultureInfo.InvariantCulture, "Stopping file watcher for document \"{0}\".", document.GetName());

            var fileWatcher = _fileWatchers.Find(fw => fw.Document == document);
            if (fileWatcher == null)
            {
                Logger.Warn(CultureInfo.InvariantCulture, "No file watcher found for document \"{0}\".", document.GetName());
                return;
            }

            Logger.Debug(CultureInfo.InvariantCulture, "Stopping file watcher for document \"{0}\".", document.GetName());
            fileWatcher.Dispose();
            _fileWatchers.Remove(fileWatcher);

            // Remove any pending file changes for this document.
            lock (_fileChangeRecords)
            {
                _fileChangeRecords.RemoveAll(r => r.Document == document);
            }
        }


        /// <summary>
        /// Suspends the <see cref="FileSystemWatcher"/> for a document.
        /// </summary>
        /// <param name="document">The document which is monitored.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="StartFileWatcher"/>
        /// <seealso cref="StopFileWatcher"/>
        /// <seealso cref="SuspendFileWatcher"/>
        /// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        internal void SuspendFileWatcher(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var fileWatcher = _fileWatchers.Find(fw => fw.Document == document);
            if (fileWatcher != null)
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Suspending file watcher for document \"{0}\".", document.GetName());
                fileWatcher.Suspend();
            }
        }


        /// <summary>
        /// Resumes the <see cref="FileSystemWatcher"/> for a document.
        /// </summary>
        /// <param name="document">The document which is monitored.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="StartFileWatcher"/>
        /// <seealso cref="StopFileWatcher"/>
        /// <seealso cref="SuspendFileWatcher"/>
        /// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        internal void ResumeFileWatcher(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var fileWatcher = _fileWatchers.Find(fw => fw.Document == document);
            if (fileWatcher != null)
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Resuming file watcher for document \"{0}\".", document.GetName());
                fileWatcher.Resume();
            }
        }


        // SuspendAllFileWatchers/ResumeAllFileWatchers are commented out. They are useful if an
        // extension performs an action and then EVERYTHING is reloaded. But it is also dangerous
        // if the file watchers can be used to watch files that are not registered documents and
        // we cannot make if they are also reloaded.
        ///// <summary>
        ///// Suspends the <see cref="FileSystemWatcher"/>s for all documents.
        ///// </summary>
        ///// <seealso cref="StartFileWatcher"/>
        ///// <seealso cref="StopFileWatcher"/>
        ///// <seealso cref="SuspendFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        //public void SuspendAllFileWatchers()
        //{
        //    Logger.Debug("Suspending all file watchers.");

        //    foreach (var fileWatcher in _fileWatchers)
        //        fileWatcher.Suspend();
        //}


        ///// <summary>
        ///// Resumes the <see cref="FileSystemWatcher"/>.
        ///// </summary>
        ///// <seealso cref="StopFileWatcher"/>
        ///// <seealso cref="SuspendFileWatcher"/>
        ///// <seealso cref="SuspendAllFileWatchers"/>
        ///// <seealso cref="ResumeFileWatcher"/>
        ///// <seealso cref="ResumeAllFileWatchers"/>
        //public void ResumeAllFileWatchers()
        //{
        //    Logger.Debug("Resuming all file watchers.");

        //    foreach (var fileWatcher in _fileWatchers)
        //        fileWatcher.Resume();
        //}


        private void OnFileChanged(object sender, FileSystemEventArgs eventArgs)
        {
            var fileWatcher = (FileWatcher)sender;

            Debug.Assert(sender != null);
            Debug.Assert(eventArgs.ChangeType == WatcherChangeTypes.Changed);

            var document = fileWatcher.Document;

            Logger.Debug(CultureInfo.InvariantCulture, "File watcher detected change for document \"{0}\".", document.GetName());

            // Record file change.
            var fileChangeRecord = new FileChangeRecord(document, eventArgs);
            lock (_fileChangeRecords)
            {
                if (_fileChangeRecords.Find(record => record.Document == document) == null)
                {
                    // Multiple changes of the same file are only recorded once.
                    _fileChangeRecords.Add(fileChangeRecord);
                }
            }
        }


        /// <summary>
        /// Applies all pending file changes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a file watcher is started ( <see cref="StartFileWatcher"/>) all file changes are
        /// monitored and recorded. Once the user access a document that belongs to one of the
        /// changed files, the document service will automatically prompt the user to apply or 
        /// ignore the change.
        /// </para>
        /// <para>
        /// But you can also explicitly apply all pending changes by calling this method. This will
        /// display a dialog and reload documents - based on the users choice. You should
        /// call this method before you access multiple documents. This will make sure that the user
        /// acknowledges all pending file changes, before you change any of the associated
        /// documents.
        /// </para>
        /// </remarks>
        /// <seealso cref="StartFileWatcher"/>
        /// <seealso cref="StopFileWatcher"/>
        /// <seealso cref="SuspendFileWatcher"/>
        /// <seealso cref="ResumeFileWatcher"/>
        public void ApplyPendingFileChanges()
        {
            // Avoid re-entrance.
            if (_applyingFileChanges)
                return;

            if (_fileChangeRecords.Count == 0)
                return;

            Logger.Debug("Applying pending file changes.");
            _applyingFileChanges = true;

            try
            {
                var unmodifiedDocumentChoice = ReloadFileChoice.Prompt;
                var modifiedDocumentChoice = ReloadFileChoice.Prompt;

                // Start with current file.
                var activeDocument = ActiveDocument;
                {
                    FileChangeRecord record;
                    lock (_fileChangeRecords)
                        record = _fileChangeRecords.Find(r => r.Document == activeDocument);

                    if (record != null)
                    {
                        ApplyFileChange(record, ref unmodifiedDocumentChoice, ref modifiedDocumentChoice);
                        lock (_fileChangeRecords)
                            _fileChangeRecords.Remove(record);
                    }
                }

                // Apply remaining file changes.
                while (_fileChangeRecords.Count > 0)
                {
                    FileChangeRecord record;
                    lock (_fileChangeRecords)
                    {
                        if (_fileChangeRecords.Count == 0)  // Double-check.
                            break;

                        record = _fileChangeRecords[_fileChangeRecords.Count - 1];
                    }

                    ApplyFileChange(record, ref unmodifiedDocumentChoice, ref modifiedDocumentChoice);
                    lock (_fileChangeRecords)
                        _fileChangeRecords.Remove(record);
                }
            }
            finally
            {
                _applyingFileChanges = false;
            }
        }


        /// <summary>
        /// Describes the action that has to be taken when a file on disk changes.
        /// </summary>
        private enum ReloadFileChoice
        {
            /// <summary>Show a dialog and ask the user what she wants to do.</summary>
            Prompt,

            /// <summary>Ignore file changes and do not reload anything.</summary>
            IgnoreAll,

            /// <summary>Reload documents if file has changed.</summary>
            ReloadAll
        }


        /// <summary>
        /// Applies the file changes which affect a specific document.
        /// </summary>
        /// <param name="record">The file change record.</param>
        /// <param name="unmodifiedDocumentsChoice">
        /// The user choice that shall be applied to all documents that are unmodified in the
        /// editor.
        /// </param>
        /// <param name="modifiedDocumentsChoice">
        /// The user choice that shall be applied to all documents that are modified in the editor.
        /// </param>
        /// <remarks>
        /// <para>
        /// Any pending file changes are usually applied when the application regains focus. The
        /// document service then calls <see cref="ApplyFileChange"/> for each file that has changed
        /// on disk. A dialog is shown to the user. The user can choose to reload the changed file,
        /// reload all changed files, ignore the changed file, or ignore all changed files. If the
        /// document was modified in the editor, the dialog warns the user that reloading the
        /// document would discard the user's modifications.
        /// </para>
        /// <para>
        /// The parameters <paramref name="unmodifiedDocumentsChoice"/> and
        /// <paramref name="modifiedDocumentsChoice"/> remember the user's choice and are passed
        /// from one call of <see cref="ApplyFileChange"/> to the next call of
        /// <see cref="ApplyFileChange"/>.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void ApplyFileChange(FileChangeRecord record, ref ReloadFileChoice unmodifiedDocumentsChoice, ref ReloadFileChoice modifiedDocumentsChoice)
        {
            Debug.Assert(record.Document != null);
            Debug.Assert(record.FileSystemEventArgs != null && record.FileSystemEventArgs.ChangeType == WatcherChangeTypes.Changed);

            var document = record.Document;
            bool isModified = document.IsModified;
            bool reloadFile = false;

            if (!isModified && unmodifiedDocumentsChoice == ReloadFileChoice.Prompt
                || isModified && modifiedDocumentsChoice == ReloadFileChoice.Prompt)
            {
                // Select the affected document
                ShowDocument(document);

                // Ask the user what to do.
                var reloadFileDialog = new ReloadFileViewModel
                {
                    FileName = document.Uri.LocalPath,
                    IsFileModified = document.IsModified,
                    DisplayName = Editor.ApplicationName,
                };
                _windowService.ShowDialog(reloadFileDialog);

                switch (reloadFileDialog.ReloadFileDialogResult)
                {
                    case ReloadFileDialogResult.Yes:
                        // The user has chosen to reload the current document.
                        // Prompt her again, when another file has changed.
                        reloadFile = true;
                        if (isModified)
                            modifiedDocumentsChoice = ReloadFileChoice.Prompt;
                        else
                            unmodifiedDocumentsChoice = ReloadFileChoice.Prompt;
                        break;

                    case ReloadFileDialogResult.YesToAll:
                        // The user has chosen to reload the documents for all changed files.
                        // Apply this rule to all similar files.
                        reloadFile = true;
                        if (isModified)
                            modifiedDocumentsChoice = ReloadFileChoice.ReloadAll;
                        else
                            unmodifiedDocumentsChoice = ReloadFileChoice.ReloadAll;
                        break;

                    case ReloadFileDialogResult.No:
                        // The user has chosen to ignore the file change for the current document.
                        // Prompt her again, when another file has changed.
                        reloadFile = false;
                        if (isModified)
                            modifiedDocumentsChoice = ReloadFileChoice.Prompt;
                        else
                            unmodifiedDocumentsChoice = ReloadFileChoice.Prompt;
                        break;

                    case ReloadFileDialogResult.NoToAll:
                        // The user has chosen to ignore the file change for the current document.
                        // Apply the same to all other files.
                        reloadFile = false;
                        if (isModified)
                            modifiedDocumentsChoice = ReloadFileChoice.IgnoreAll;
                        else
                            unmodifiedDocumentsChoice = ReloadFileChoice.IgnoreAll;
                        break;

                    default:
                        throw new InvalidOperationException("Invalid value returned by ReloadFileDialog");
                }
            }
            else
            {
                // The user has already decided what to do.
                if (!document.IsModified && unmodifiedDocumentsChoice == ReloadFileChoice.ReloadAll
                    || document.IsModified && modifiedDocumentsChoice == ReloadFileChoice.ReloadAll)
                {
                    reloadFile = true;
                }
            }

            if (reloadFile)
            {
                Logger.Info(CultureInfo.InvariantCulture, "Reloading document \"{0}\".", document.GetName());
                Reload(document, true);
                Debug.Assert(document.IsModified == false);
            }
        }
        #endregion
    }
}
