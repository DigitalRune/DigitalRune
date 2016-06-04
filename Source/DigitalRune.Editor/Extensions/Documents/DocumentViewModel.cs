// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents the view model of a document.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that a document may have multiple view models.
    /// </para>
    /// <para>
    /// In <see cref="OnActivated"/>, when the view model is opened, it will add itself to the
    /// <see cref="Documents.Document.ViewModels"/> collection of the <see cref="Document"/>. In
    /// <see cref="OnDeactivated"/>, when the view model is closed, it will removed itself from the
    /// <see cref="Documents.Document.ViewModels"/> collection.
    /// </para>
    /// <para>
    /// When the last view model of a document is closed, <see cref="CanCloseAsync"/> will show the
    /// "Save Changes" dialog and if successful, <see cref="Screen.OnDeactivated"/> will close the
    /// document.
    /// </para>
    /// </remarks>
    public abstract class DocumentViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public Document Document { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentViewModel"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        protected DocumentViewModel(Document document)
        {
            if (!WindowsHelper.IsInDesignMode)
            {
                if (document == null)
                    throw new ArgumentNullException(nameof(document));

                Document = document;
                DockId = Guid.NewGuid().ToString();

                // Constant properties.
                DockContextMenu = document.DocumentExtension.DockContextMenu;
                Icon = document.DocumentType?.Icon;

                // Derived properties.
                UpdateProperties();
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                Document.RegisterViewModel(this);
                UpdateProperties();
                PropertyChangedEventManager.AddHandler(Document, OnDocumentPropertyChanged, string.Empty);
            }

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                PropertyChangedEventManager.RemoveHandler(Document, OnDocumentPropertyChanged, string.Empty);
                Document.UnregisterViewModel(this);

                // The last view model closes the document.
                if (!Document.IsDisposed && !Document.ViewModels.Any())
                    Document.DocumentService.Close(Document, true);
            }

            base.OnDeactivated(eventArgs);
        }


        /// <inheritdoc/>
        public override Task<bool> CanCloseAsync()
        {
            if (Document.IsDisposed                 // Document has already been closed.
                || Document.ViewModels.Count > 1)   // This is not the last view model.
            {
                return TaskHelper.FromResult(true);
            }

            Logger.Info(CultureInfo.InvariantCulture, "Closing document \"{0}\".", DisplayName);

            // This is the last view model. Do we need to save the document before it is closed?
            bool canClose = Document.DocumentExtension.PromptSaveChanges(Document);
            return TaskHelper.FromResult(canClose);
        }


        private void OnDocumentPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            UpdateProperties();
        }


        private void UpdateProperties()
        {
            DisplayName = Document.GetDisplayName();
            DockToolTip = Document.GetName();

            // TODO: To restore the document using the same document factory, we need to add the document type!
            // TODO: DockId must not be null.
            //DockId = Document.Uri?.LocalPath;
        }
        #endregion
    }
}
