// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DigitalRune.Editor.Status;
using DigitalRune.Windows.Framework;
using static System.FormattableString;


namespace DigitalRune.Editor.Documents
{
    public partial class DocumentExtension
    {
        /// <inheritdoc/>
        public Document New(DocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException(nameof(documentType));
            if (!documentType.IsCreatable)
                throw new NotSupportedException(Invariant($"Creating new documents of type \"{documentType.Name}\" is not supported."));

            Logger.Info(CultureInfo.InvariantCulture, "Creating new document of type \"{0}\".", documentType.Name);

            var document = documentType.Factory.Create(documentType);
            if (document != null)
            {
                var viewModel = document.CreateViewModel();
                Editor.ActivateItem(viewModel);
            }

            return document;
        }


        /// <inheritdoc/>
        public Document Open(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            Logger.Info("Opening document from URI {0}.", uri);

            // Check whether the document already exists.
            // Show existing document, do not open a document multiple times.
            foreach (var existingDocument in _documents)
            {
                if (!existingDocument.IsUntitled && existingDocument.Uri == uri)
                {
                    ShowDocument(existingDocument);
                    return existingDocument;
                }
            }

            // Look for document factory with the highest priority
            var documentType = Factories.Select(f => f.GetDocumentType(uri))
                                        .Where(t => t != null)
                                        .OrderByDescending(t => t.Priority)
                                        .FirstOrDefault();

            // Try to open the file.
            if (documentType == null)
            {
                string message = Invariant($"Cannot open file \"{uri.LocalPath}\". This type of file is not supported.");
                Logger.Error(message);
                MessageBox.Show(message, Editor.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var factory = documentType.Factory;
            Logger.Debug("Opening document {0} using the {1}.", uri, factory.Name);
            var document = factory.Create(documentType);
            try
            {
                document.Load(uri);
                RememberRecentFile(document.Uri);
                var viewModel = document.CreateViewModel();
                Editor.ActivateItem(viewModel);
            }
            catch (OperationCanceledException exception)
            {
                // Operation was canceled by user. No need to show error message.
                Logger.Info(exception, "Load operation for file \"{0}\" canceled.", uri.LocalPath);
                document.Dispose();
                document = null;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Could not load file \"{0}\".", uri.LocalPath);
                document.Dispose();
                document = null;

                string message = Invariant($"Could not load file \"{Path.GetFileName(uri.LocalPath)}\".\n\n{exception.Message}");
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return document;
        }


        /// <summary>
        /// Asynchronously opens a file (or several files) that the user can select from an "Open
        /// File" dialog.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task are the newly
        /// opened documents. This can be an empty list if no document could be loaded or if the
        /// operation was canceled by the user.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private async Task<IEnumerable<Document>> OpenAsync()
        {
            Logger.Debug("Opening document using the Open File dialog.");

            // First, collect all supported file types
            var documentTypes = new SortedList<string, DocumentType>();
            foreach (var documentHandler in Factories)
            {
                foreach (var documentType in documentHandler.DocumentTypes)
                {
                    if (documentType.IsLoadable && !documentTypes.Keys.Contains(documentType.Name))
                        documentTypes.Add(documentType.Name, documentType);
                }
            }

            if (documentTypes.Count == 0)
                return Enumerable.Empty<Document>();

            // Build filter string.
            // (This code is similar to DocumentHelper.GetFilterString. The difference is that we
            // do not always add the wildcard factory and remember the last filter index.)
            int numberOfFilters = 0;
            bool wildcardHandlerFound = false;
            var filterString = new StringBuilder();
            var fileExtensions = new StringBuilder();
            foreach (var documentType in documentTypes.Values)
            {
                if (documentType.FileExtensions.Any())
                {
                    // Build new file extension string, e.g. "*.bmp;*.jpg;*.png"
                    fileExtensions.Remove(0, fileExtensions.Length);
                    foreach (string fileExtension in documentType.FileExtensions)
                    {
                        if (fileExtension != ".*")
                        {
                            // Build ";*.bmp"
                            if (fileExtensions.Length > 0)
                                fileExtensions.Append(';');

                            fileExtensions.Append("*");
                            fileExtensions.Append(fileExtension);
                        }
                        else
                            wildcardHandlerFound = true;
                    }
                }

                if (fileExtensions.Length > 0)
                {
                    // Build filter string, e.g. "|Image Files (*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png"
                    if (filterString.Length > 0)
                        filterString.Append('|');

                    filterString.Append(documentType.Name);
                    filterString.Append(" (");
                    filterString.Append(fileExtensions);
                    filterString.Append(")|");
                    filterString.Append(fileExtensions);
                    numberOfFilters++;
                }
            }

            // Make sure "All Files (*.*)" is the last entry in the list
            if (wildcardHandlerFound)
            {
                if (filterString.Length > 0)
                    filterString.Append('|');

                filterString.Append("All Files (*.*)|*.*");
                numberOfFilters++;
            }

            if (filterString.Length == 0)
                return Enumerable.Empty<Document>();

            var openFileDialog = OpenFileDialog;
            openFileDialog.Filter = filterString.ToString();
            openFileDialog.FilterIndex = numberOfFilters;     // Select the "*.*" filter
            bool? result = openFileDialog.ShowDialog();
            if (result != true)
                return Enumerable.Empty<Document>();

            var fileNames = openFileDialog.FileNames;
            return await OpenAsync(fileNames);
        }


        private async Task<IEnumerable<Document>> OpenAsync(IList<string> fileNames)
        {
            if (fileNames.Count == 1)
            {
                // ---- Single document.
                var document = Open(new Uri(fileNames[0]));
                return (document != null) ? new[] { document } : Enumerable.Empty<Document>();
            }
            else
            {
                // ----- Multiple documents: Show status message and Cancel button.
                var numberOfFiles = fileNames.Count;
                var documents = new List<Document>(numberOfFiles);
                var status = new StatusViewModel
                {
                    Message = "Opening files...",
                    Progress = 0,
                    ShowProgress = true,
                    CancellationTokenSource = new CancellationTokenSource()
                };
                _statusService.Show(status);
                var token = status.CancellationTokenSource.Token;

                for (int i = 0; i < numberOfFiles; i++)
                {
                    string fileName = fileNames[i];

                    if (token.IsCancellationRequested || Keyboard.IsKeyDown(Key.Escape))
                    {
                        Logger.Info("Opening files canceled.");
                        status.Message = "Opening files canceled.";
                        status.ShowProgress = false;
                        status.IsCompleted = true;
                        status.CloseAfterDefaultDurationAsync().Forget();
                        return documents;
                    }

                    if (Editor.IsShuttingDown)
                        break;

                    // Open document.
                    var document = Open(new Uri(fileName));

                    // Update status.
                    status.Progress = (double)(i + 1) / numberOfFiles;

                    if (document != null)
                    {
                        documents.Add(document);

                        // Redraw GUI and keep app responsive.
                        await Dispatcher.Yield();

                        if (Editor.IsShuttingDown)
                            break;
                    }
                }

                status.CloseAsync().Forget();
                return documents;
            }
        }


        private void Reload()
        {
            Logger.Info("Reloading active document.");

            var document = ActiveDocument;
            if (document == null)
                return;

            Reload(ActiveDocument, false);
        }


        private void Reload(Document document, bool forceReload)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.IsUntitled)
                return;

            if (document.IsModified && !forceReload)
            {
                var result = MessageBox.Show(
                    "The document has been modified.\n\nDo you still want to reload the file and lose the changes made in the editor?",
                    Editor.ApplicationName,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.No)
                    return;
            }

            try
            {
                document.Load(document.Uri);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, CultureInfo.InvariantCulture, "Could not reload file {0}.", document.Uri);

                string message = Invariant($"Could not reload file.\n\n{exception.Message}");
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <inheritdoc/>
        public bool Save(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.IsDisposed)
                return true;

            Logger.Info(CultureInfo.InvariantCulture, "Saving document \"{0}\".", document.GetName());

            if (document.IsUntitled)
                return SaveAs(document);

            try
            {
                document.Save();
                RememberRecentFile(document.Uri);
                UpdateCommands();
                return true;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, CultureInfo.InvariantCulture, "Could not save file {0}.", document.Uri);
                string message = Invariant($"Could not save file.\n\n{exception.Message}");
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private void SaveActiveDocument()
        {
            Logger.Debug("Saving active document.");

            var document = ActiveDocument;
            if (document != null)
                Save(document);
        }


        private void SaveAs()
        {
            Logger.Debug("Saving active document using the Save File dialog.");

            var document = ActiveDocument;
            if (document != null)
                SaveAs(document);
        }


        /// <summary>
        /// Saves the specified document to a file that the user can select from a "Save File"
        /// dialog.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if all changes were saved successfully or can be discarded;
        /// otherwise <see langword="false"/> if there are still changes that need to be saved.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        private bool SaveAs(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            Logger.Debug(CultureInfo.InvariantCulture, "Saving document \"{0}\" using the Save File dialog.", document.GetName());

            var saveFileDialog = SaveFileDialog;
            if (document.IsUntitled)
            {
                saveFileDialog.FileName = document.UntitledName;
            }
            else
            {
                string path = document.Uri.LocalPath;
                string fileName = Path.GetFileName(path);
                string directory = Path.GetDirectoryName(path);
                saveFileDialog.FileName = fileName;
                saveFileDialog.InitialDirectory = directory;
            }
            saveFileDialog.Filter = document.FileDialogFilter;
            saveFileDialog.FilterIndex = document.FileDialogFilterIndex;

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    document.Save(new Uri(saveFileDialog.FileName));
                    RememberRecentFile(document.Uri);
                    UpdateCommands();
                    return true;
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception, CultureInfo.InvariantCulture, "Could not save file as \"{0}\".", saveFileDialog.FileName);

                    string message = Invariant($"Could not save file.\n\n{exception.Message}");
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return false;
        }


        /// <inheritdoc/>
        public bool SaveAll()
        {
            Logger.Info("Saving all documents.");

            _suppressUpdateCommandItems = true;
            try
            {

                bool allSaved = true;
                var unsavedDocuments = _documents.Where(document => document.IsModified);
                foreach (var document in unsavedDocuments)
                {
                    Debug.Assert(document.DocumentType.IsSavable, "Document.IsModified should not be set unless document is savable.");

                    if (!Save(document))
                        allSaved = false;
                }

                return allSaved;
            }
            finally
            {
                _suppressUpdateCommandItems = false;
            }
        }


        // Same as SaveAll() except that UI is updated.
        private async Task SaveAllAsync()
        {
            Logger.Info("Saving all documents.");

            var unsavedDocuments = _documents.Where(document => document.IsModified);
            foreach (var document in unsavedDocuments)
            {
                Debug.Assert(document.DocumentType.IsSavable, "Document.IsModified should not be set unless document is savable.");

                Save(document);

                // Redraw UI and keep app responsive.
                await Dispatcher.Yield();
            }
        }


        /// <summary>
        /// Shows the "Save Changes" dialog for a document that is about to be closed and saves the
        /// document if required.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// <see langword="true"/> if all changes are saved or can be discarded; otherwise
        /// <see langword="false"/> if there are still changes that need to be saved.
        /// </returns>
        /// <remarks>
        /// This method checks if the document is modified and can be saved. If this is the case a
        /// dialog is displayed that tells the user that the document is about to close and asks if
        /// any changes should be saved or discarded, or if any close operation should be canceled.
        /// If necessary, <see cref="Save"/> is called automatically.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public bool PromptSaveChanges(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (!document.IsModified || !document.DocumentType.IsSavable)
                return true;

            // Ask the user.
            var saveChangesDialog = new SaveChangesViewModel
            {
                ModifiedDocuments = new[] { document },
                DisplayName = Editor.ApplicationName,
            };
            _windowService.ShowDialog(saveChangesDialog);

            if (saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.SaveAndClose)
            {
                Logger.Info(CultureInfo.InvariantCulture, "Saving document \"{0}\".", document.GetName());
                return Save(document);
            }

            if (saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.CloseWithoutSaving)
            {
                Logger.Info(CultureInfo.InvariantCulture, "Discarding changes of document \"{0}\".", document.GetName());
                return true;
            }

            Debug.Assert(saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.Cancel);
            Logger.Info(CultureInfo.InvariantCulture, "Closing of document canceled by user.");
            return false;
        }


        private void Close()
        {
            Logger.Info("Closing active document.");

            var document = ActiveDocument;
            if (document != null)
                Close(document, false);
        }


        /// <inheritdoc/>
        public bool Close(Document document, bool force)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.IsDisposed)
                return true;

            Debug.Assert(_documents.Contains(document), "Document already closed.");
            Logger.Info(CultureInfo.InvariantCulture, "Closing document \"{0}\".", document.GetName());

            bool canClose = force || PromptSaveChanges(document);
            if (canClose)
            {
                if (!document.IsUntitled)
                    RememberRecentFile(document.Uri);

                document.Dispose();

                foreach (var viewModel in document.ViewModels.ToArray())
                {
                    var task = viewModel.Conductor?.DeactivateItemAsync(viewModel, true);
                    Debug.Assert(task.IsCompleted, "DeactivateItem expected to be synchronous operation.");
                    Debug.Assert(task.Result, "DeactivateItem failed.");
                }

                Debug.Assert(!document.ViewModels.Any(),
                             "One or more view models found. All document view models expected to be closed.");
                Debug.Assert(Editor.Items.OfType<DocumentViewModel>().All(vm => vm.Document != document),
                             "Open view model is still referencing the closed document.");

                return true;
            }

            return false;
        }


        /// <summary>
        /// Asynchronously closes all documents.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the documents were closed. <see langword="true"/> if the documents were closed;
        /// otherwise, <see langword="false"/> if the operation was canceled by the user.
        /// </returns>
        private async Task<bool> CloseAllAsync()
        {
            Logger.Info("Closing all documents.");
            return await CloseAllDocumentsButAsync(null);
        }


        /// <summary>
        /// Asynchronously closes all documents, except for the currently selected document.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the documents were closed. <see langword="true"/> if the documents were closed;
        /// otherwise, <see langword="false"/> if the operation was canceled by the user.
        /// </returns>
        private async Task<bool> CloseAllButActiveAsync()
        {
            Logger.Info("Closing all but active document.");
            return await CloseAllDocumentsButAsync(ActiveDocument);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
        private async Task<bool> CloseAllDocumentsButAsync(Document excludedDocument)
        {
            if (excludedDocument == null)
                Logger.Info("Closing all documents.");
            else
                Logger.Info(CultureInfo.InvariantCulture, "Closing all documents except: \"{0}\".", excludedDocument.GetName());

            // Collect all documents that need to be saved.
            var modifiedDocuments = _documents.Where(document => document != excludedDocument
                                                                 && document.IsModified
                                                                 && document.DocumentType.IsSavable)
                                              .ToArray();

            // Do we need to save the documents before closing them?
            if (modifiedDocuments.Length > 0)
            {
                var saveChangesDialog = new SaveChangesViewModel
                {
                    ModifiedDocuments = modifiedDocuments,
                    DisplayName = Editor.ApplicationName,
                };
                _windowService.ShowDialog(saveChangesDialog);

                if (saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.SaveAndClose)
                {
                    foreach (var document in modifiedDocuments)
                    {
                        bool success = Save(document);
                        if (!success)
                        {
                            // The save operation failed or was canceled. --> Abort!
                            Logger.Info("Save operation failed or was canceled by user. Canceling close operation");
                            return false;
                        }
                    }
                }
                else if (saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.CloseWithoutSaving)
                {
                    Logger.Info("Discarding changes of remaining document.");
                }
                else
                {
                    Debug.Assert(saveChangesDialog.SaveChangesDialogResult == SaveChangesDialogResult.Cancel);
                    Logger.Info("Close operation canceled by user.");
                    return false;
                }
            }

            // Close all documents
            foreach (var document in _documents.ToArray())
            {
                if (document != excludedDocument)
                {
                    Close(document, true);

                    // Redraw GUI and keep app responsive.
                    await Dispatcher.Yield();
                }
            }

            return true;
        }


        private async Task<bool> CloseAllDocumentsAndWindowsAsync()
        {
            Logger.Info("Closing all documents and tool windows.");

            // First close all documents. (User gets a chance to cancel the operation.)
            bool allClosed = await CloseAllAsync();
            if (!allClosed)
                return false;

            // Then close all remaining docking windows.
            var viewModels = Editor.Items.ToArray();
            foreach (var viewModel in viewModels)
                await Editor.DeactivateItemAsync(viewModel, true);

            return true;
        }
    }
}
