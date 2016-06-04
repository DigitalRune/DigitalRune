// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents a document that is managed by the <see cref="IDocumentService"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Document"/> implements <see cref="IDisposable"/>.
    /// <see cref="IDisposable.Dispose"/> is called by the document service when the document is
    /// closed.
    /// </para>
    /// <para>
    /// In the MVVM pattern: The <see cref="Document"/> is the model and
    /// <see cref="DocumentViewModel"/> is the view model. The view is a <see cref="DockTabItem"/>
    /// inside the <see cref="DockControl"/>. Note that a document may have multiple view
    /// models/views!
    /// </para>
    /// </remarks>
    public abstract class Document : ObservableObject, IDisposable
    {
        // DOC: The load/save methods in this class do not show dialogs (e.g. to prompt to save
        // changes). The load/save methods throw exceptions if there is a problem.
        // In contrast, the document service open/save methods show dialogs if necessary and 
        // handle exceptions.
        
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Table with key = file extension (e.g. ".fx") and value = last used number for untitled name.
        private static readonly Dictionary<string, int> _lastUntitledNumbers = new Dictionary<string, int>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this document has been closed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this document has been closed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// Gets the document service.
        /// </summary>
        /// <value>The document service.</value>
        public IDocumentService DocumentService
        {
            get { return _documentExtension; }
        }


        /// <summary>
        /// Gets the document extension.
        /// </summary>
        /// <value>The document extension.</value>
        internal DocumentExtension DocumentExtension
        {
            get { return _documentExtension; }
        }
        private readonly DocumentExtension _documentExtension;


        /// <summary>
        /// Gets the editor.
        /// </summary>
        /// <value>The editor.</value>
        public IEditorService Editor { get; }


        /// <summary>
        /// Gets the type of the document.
        /// </summary>
        /// <value>The type of the document.</value>
        public DocumentType DocumentType { get; }


        /// <summary>
        /// Gets or sets the filter string that is used in the "Save As" dialog.
        /// </summary>
        /// <value>The filter string used in the "Save As" dialog.</value>
        public string FileDialogFilter
        {
            get { return _fileDialogFilter; }
            set { SetProperty(ref _fileDialogFilter, value); }
        }
        private string _fileDialogFilter;


        /// <summary>
        /// Gets or sets the index of the filter that is selected in the "Save As" dialog. (Index of
        /// first entry is 1.)
        /// </summary>
        /// <value>
        /// The index of the filter that is selected in the "Save As" dialog. (Index of first entry
        /// is 1.)
        /// </value>
        public int FileDialogFilterIndex
        {
            get { return _fileDialogFilterIndex; }
            set { SetProperty(ref _fileDialogFilterIndex, value); }
        }
        private int _fileDialogFilterIndex;


        /// <summary>
        /// Gets (or sets) a value indicating whether the document has been modified.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the document has been modified; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsModified
        {
            get { return _isModified; }
            protected set
            {
                Debug.Assert(!value || DocumentType.IsSavable, "Document.IsModified should not be set unless document is savable.");
                SetProperty(ref _isModified, value);
            }
        }
        private bool _isModified;


        /// <summary>
        /// Gets a value indicating whether this document is untitled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this document is untitled; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Untitled means that the user has not saved the document yet, or specified a file name by
        /// any other means. The document has a default file name such as "Untitled3.txt". The
        /// component that owns the document is responsible for setting the untitled name of the
        /// document.
        /// </remarks>
        public bool IsUntitled
        {
            get { return Uri == null; }
        }


        /// <summary>
        /// Gets or sets the name of the untitled document.
        /// </summary>
        /// <value>
        /// The name of the untitled document. Returns <see langword="null"/> if the document
        /// already has a valid <see cref="Uri"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// This name is used by the document service until a valid <see cref="Uri"/> is assigned to
        /// this document.
        /// </para>
        /// <para>
        /// The default value is an automatic name, e.g. "Untitled3.txt" that is created when this
        /// property is accessed. (No default name is created if the document already has a valid
        /// <see cref="Uri"/>.)
        /// </para>
        /// </remarks>
        public string UntitledName
        {
            get
            {
                if (_untitledName != null)
                    return _untitledName;

                // We do not create an untitled name if we have a valid title.
                if (!IsUntitled)
                    return null;

                // The name is created only when needed. Otherwise, we would increment the Untitled 
                // names (Untitled1, Untitled2, ...) unnecessarily.
                var documentService = Editor.Services.GetInstance<IDocumentService>().ThrowIfMissing();
                _untitledName = GetUntitledName(documentService, DocumentType);

                return _untitledName;
            }
            set { SetProperty(ref _untitledName, value); }
        }
        private string _untitledName;


        /// <summary>
        /// Gets the URI of the document.
        /// </summary>
        /// <value>The URI of the document - typically the local file name.</value>
        /// <remarks>
        /// This property is automatically set before <see cref="OnLoad"/> or <see cref="OnSave"/>
        /// are executed. Usually it is not necessary for derived classes to change this property.
        /// </remarks>
        public Uri Uri
        {
            get { return _uri; }
            protected set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Value for Uri is null.");
                if (!value.IsFile && !value.IsUnc)
                    throw new ArgumentException("Specified URI is not a file URI or a UNC path.", nameof(value));

                if (SetProperty(ref _uri, value))
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    RaisePropertyChanged(nameof(IsUntitled));
                }
            }
        }
        private Uri _uri;


        /// <summary>
        /// Gets the view models representing this document.
        /// </summary>
        /// <value>The view models representing this document.</value>
        /// <remarks>
        /// <para>
        /// A document can have multiple views. When closing the last view, the document will be
        /// closed.
        /// </para>
        /// </remarks>
        public ReadOnlyObservableCollection<DocumentViewModel> ViewModels { get; }
        private readonly ObservableCollection<DocumentViewModel> _viewModels;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentType">The type of the document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentType"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DocumentExtension")]
        protected Document(IEditorService editor, DocumentType documentType)
        {
            if (!WindowsHelper.IsInDesignMode)
            {
                if (editor == null)
                    throw new ArgumentNullException(nameof(editor));

                Editor = editor;
                _documentExtension = editor.Extensions.OfType<DocumentExtension>().FirstOrDefault().ThrowIfMissing();
            }

            if (documentType == null)
                throw new ArgumentNullException(nameof(documentType));

            DocumentType = documentType;
            _fileDialogFilter = DocumentHelper.GetFilterString(new[] { documentType });
            _fileDialogFilterIndex = 1;
            _viewModels = new ObservableCollection<DocumentViewModel>();
            ViewModels = new ReadOnlyObservableCollection<DocumentViewModel>(_viewModels);

            _documentExtension?.RegisterDocument(this);
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="Document"/> class.
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
        /// Releases the unmanaged resources used by an instance of the <see cref="Document"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (!IsUntitled)
                        _documentExtension.StopFileWatcher(this);

                    _documentExtension.UnregisterDocument(this);
                    IsModified = false;
                }

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new InvalidOperationException("The document has already been closed and cannot be used anymore.");
        }


        /// <summary>
        /// Creates a new view model representing the document.
        /// </summary>
        /// <returns>A new view model representing the document.</returns>
        internal DocumentViewModel CreateViewModel()
        {
            return OnCreateViewModel();
        }


        /// <summary>
        /// Called when the <see cref="IDocumentService"/> needs a view model that represents the
        /// document.
        /// </summary>
        /// <returns>The <see cref="DocumentViewModel"/>.</returns>
        protected abstract DocumentViewModel OnCreateViewModel();


        /// <summary>
        /// Adds the specified view model to the <see cref="ViewModels"/> collection.
        /// </summary>
        /// <param name="viewModel">The view model to add.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        internal void RegisterViewModel(DocumentViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            Debug.Assert(viewModel.Document != null, "View model belongs needs to be assigned to a document.");
            Debug.Assert(viewModel.Document == this, "View model belongs to a different document.");
            Debug.Assert(!_viewModels.Contains(viewModel), "View model is already registered.");

            _viewModels.Add(viewModel);
        }


        /// <summary>
        /// Removes the specified view model from the <see cref="ViewModels"/> collection.
        /// </summary>
        /// <param name="viewModel">The view model to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        internal void UnregisterViewModel(DocumentViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            Debug.Assert(viewModel.Document != null, "View model belongs needs to be assigned to a document.");
            Debug.Assert(viewModel.Document == this, "View model belongs to a different document.");
            Debug.Assert(_viewModels.Contains(viewModel), "View model is not registered.");

            _viewModels.Remove(viewModel);
        }


        /// <summary>
        /// Loads the document from the specified URI.
        /// </summary>
        /// <param name="uri">
        /// The URI of the document (usually a local path, such as "C:\path\file.ext").
        /// </param>
        /// <remarks>
        /// <para>
        /// If the document <see cref="IsModified"/>, the changes are not saved before loading the
        /// new Uri.
        /// </para>
        /// <para>
        /// If <paramref name="uri"/> is equal to the current <see cref="Uri"/>, the document is
        /// reloaded.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="uri"/> is <see langword="null"/>.
        /// </exception>
        internal void Load(Uri uri)
        {
            ThrowIfDisposed();

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var oldUri = Uri;

            // Clean up. (Stop file watcher for previous URI.)
            if (!IsUntitled)
            {
                if (oldUri == uri)
                    _documentExtension.SuspendFileWatcher(this);
                else
                    _documentExtension.StopFileWatcher(this);
            }

            Uri = uri;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                OnLoad();
                IsModified = false;
            }
            catch (Exception)
            {
                // Fallback to previous uri.
                if (oldUri != null)
                    Uri = oldUri;

                throw;
            }
            finally
            {
                Mouse.OverrideCursor = null;

                if (!IsUntitled)
                {
                    if (oldUri == uri)
                        _documentExtension.ResumeFileWatcher(this);
                    else
                        _documentExtension.StartFileWatcher(this);
                }
            }
        }


        /// <summary>
        /// Called when the document should be loaded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method loads the document defined by <see cref="Uri"/>.
        /// </para>
        /// <para>
        /// When the load operation fails an exception should be thrown. The editor will show a
        /// message box with the exception message. If an <see cref="OperationCanceledException"/>
        /// is thrown, the load operation fails silently - no message box will be shown.
        /// </para>
        /// </remarks>
        protected abstract void OnLoad();


        /// <summary>
        /// Saves the document.
        /// </summary>
        internal void Save()
        {
            if (IsUntitled)
                throw new EditorException("Cannot save untitled document. An URI must be specified.");

            Save(Uri);
        }


        /// <summary>
        /// Saves the document to the specified URI.
        /// </summary>
        /// <param name="uri">
        /// The URI of the document (usually a local path, such as "C:\path\file.ext").
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="uri"/> is <see langword="null"/>.
        /// </exception>
        internal void Save(Uri uri)
        {
            ThrowIfDisposed();

            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            // Always save document when requested. The file at URI may have been deleted by another
            // application in the background!
            //if (!IsModified)
            //    return;

            var oldUri = Uri;

            if (!IsUntitled)
            {
                if (oldUri == uri)
                    _documentExtension.SuspendFileWatcher(this);
                else
                    _documentExtension.StopFileWatcher(this);
            }

            Uri = uri;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                OnSave();
                IsModified = false;
            }
            catch (Exception)
            {
                // Fallback to previous uri.
                if (oldUri != null)
                    Uri = oldUri;

                throw;
            }
            finally
            {
                Mouse.OverrideCursor = null;

                if (!IsUntitled)
                {
                    if (oldUri == uri)
                        _documentExtension.ResumeFileWatcher(this);
                    else
                        _documentExtension.StartFileWatcher(this);
                }
            }
        }


        /// <summary>
        /// Called when the document should be saved.
        /// </summary>
        /// <remarks>
        /// <see cref="Uri"/> is set when this method is called. The method throws an exception if
        /// the save operation fails.
        /// </remarks>
        protected abstract void OnSave();


        private static string GetUntitledName(IDocumentService documentService, DocumentType documentType)
        {
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));
            if (documentType == null)
                throw new ArgumentNullException(nameof(documentType));

            string name;
            bool nameAlreadyUsed;
            string extension = documentType.FileExtensions.FirstOrDefault() ?? string.Empty;

            int lastUntitledNumber;
            _lastUntitledNumbers.TryGetValue(extension, out lastUntitledNumber);

            do
            {
                // Use ascending numbering for untitled documents, e.g. "Untitled3.fx".
                lastUntitledNumber++;
                name = "Untitled" + lastUntitledNumber + extension;

                // Check if name is already in use:
                nameAlreadyUsed = false;
                foreach (Document document in documentService.Documents)
                {
                    string documentName = document.IsUntitled
                                        ? document._untitledName   // Important: Use field to avoid endless loops.
                                        : Path.GetFileName(document.Uri.LocalPath);
                    if (name == documentName)
                    {
                        nameAlreadyUsed = true;
                        break;
                    }
                }
            } while (nameAlreadyUsed);

            _lastUntitledNumbers[extension] = lastUntitledNumber;

            return name;
        }


        /// <summary>
        /// Suspends the <see cref="FileSystemWatcher"/> for a document.
        /// </summary>
        public void SuspendFileWatcher()
        {
            if (!IsUntitled)
                _documentExtension.SuspendFileWatcher(this);
        }


        /// <summary>
        /// Resumes the <see cref="FileSystemWatcher"/> for a document.
        /// </summary>
        public void ResumeFileWatcher()
        {
            if (!IsUntitled)
                _documentExtension.ResumeFileWatcher(this);
        }
        #endregion
    }
}
