// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Editor.Text;
using DigitalRune.Editor.Documents;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Shader
{
    /// <summary>
    /// Handles shader documents.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    internal class ShaderDocumentFactory : DocumentFactory
    {
        // Notes:
        // We re-use the TextEditorDocument and add Shader IntelliSense to the TextEditor control
        // using only event handlers.

        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string FxFile = "DirectX Effect file";
        private const string CgFile = "NVIDIA Cg file";
        private const string CgFXFile = "NVIDIA CgFX file";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEditorService _editor;
        private readonly string _filter;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderDocumentFactory" /> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        public ShaderDocumentFactory(IEditorService editor)
            : base("Shader Editor")
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;

            // ----- Initialize supported document types.
            // HLSL Effect Files (*.fx, *.fxh)
            var fxDocumentType = new DocumentType(
                name: FxFile,
                factory: this,
                icon: MultiColorGlyphs.DocumentEffect,
                fileExtensions: new[] { ".fx", ".fxh" },
                isCreatable: true,
                isLoadable: true,
                isSavable: true,
                priority: 10);

            // Cg File (*.cg)
            var cgDocumentType = new DocumentType(
                name: CgFile,
                factory: this,
                icon: MultiColorGlyphs.DocumentEffect,
                fileExtensions: new[] { ".cg", ".cgh" },
                isCreatable: false,
                isLoadable: true,
                isSavable: true,
                priority: 10);

            // Cg Effect Files (*.cgfx)
            var cgFXDocumentType = new DocumentType(
                name: CgFXFile,
                factory: this,
                icon: MultiColorGlyphs.DocumentEffect,
                fileExtensions: new[] { ".cgfx" },
                isCreatable: false,
                isLoadable: true,
                isSavable: true,
                priority: 10);

            // All Effect Files (*.fx, *.cgfx)
            var anyDocumentType = new DocumentType(
                name: "All effect files",
                factory: this,
                icon: null,
                fileExtensions: new[] { ".fx", ".fxh", ".cg", ".cgh", ".cgfx" },
                isCreatable: false,
                isLoadable: true,
                isSavable: true,
                priority: 10);

            DocumentTypes = new[]
            {
                fxDocumentType,
                cgDocumentType,
                cgFXDocumentType,
                anyDocumentType
            };

            _filter = DocumentHelper.GetFilterString(DocumentTypes);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override Document OnCreate(DocumentType documentType)
        {
            // Create a text document.
            var document = new TextDocument(_editor, documentType)
            {
                FileDialogFilter = _filter,
            };

            switch (documentType.FileExtensions.First())
            {
                case ".fx":
                case ".fxh":
                    document.FileDialogFilterIndex = 1;
                    break;
                case ".cg":
                case ".cgh":
                    document.FileDialogFilterIndex = 2;
                    break;
                case ".cgfx":
                    document.FileDialogFilterIndex = 3;
                    break;
                default:
                    document.FileDialogFilterIndex = 1;
                    break;
            }

            // Register event handler that enables IntelliSense on every view which is
            // added to the document.
            ((INotifyCollectionChanged)document.ViewModels).CollectionChanged += OnViewModelsChanged;

            return document;
        }


        private static void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var viewModel in eventArgs.NewItems.OfType<TextDocumentViewModel>())
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var viewModel in eventArgs.OldItems.OfType<TextDocumentViewModel>())
                    viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                var document = (TextDocument)sender;
                foreach (var viewModel in document.ViewModels)
                {
                    viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
                }
            }
        }


        private static void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // Note: When the view is re-docked or the theme changes, a new TextEditor is created.
            if (eventArgs.PropertyName == nameof(TextDocumentViewModel.TextEditor))
                EnableIntelliSense((TextDocumentViewModel)sender);
        }


        private static void EnableIntelliSense(TextDocumentViewModel viewModel)
        {
            var intelliSense = SelectIntelliSense(viewModel.Document.DocumentType);
            intelliSense.ConfigureTextEditor(viewModel.TextEditor);
        }


        private static ShaderIntelliSense SelectIntelliSense(DocumentType documentType)
        {
            switch (documentType.Name)
            {
                case FxFile:
                    return new HlslIntelliSense();
                case CgFile:
                case CgFXFile:
                    return new CgIntelliSense();
                default:
                    Logger.Warn("Unknown document type. Selecting IntelliSense provider for DirectX 10 HLSL.");
                    return new HlslIntelliSense();
            }
        }
        #endregion
    }
}
