// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Printing;
using DigitalRune.Editor.Search;
using DigitalRune.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Formatting;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Represents a <see cref="DocumentViewModel"/> showing a <see cref="TextDocument"/>.
    /// </summary>
    public class TextDocumentViewModel : DocumentViewModel
    {
        // Note: A TextDocumentViewModel has a 1-to-1 relationship with a TextDocumentView.
        // The TextDocumentViewModel has a direct reference to the TextEditor control. (The
        // control is injected into the property by the TextDocumentView code-behind.)

        // TODO: IntelliSense is currently hard-coded. Make extensible.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IHighlightingService _highlightingService;
        private readonly ISearchService _searchService;
        private readonly ITextService _textService;
        private readonly IWindowService _windowService;
        private IDisposable _updateStatusSubscription;

        // Get the word near the cursor or the current selection and highlight all identical words
        // in the document.
        private IDisposable _markSelectedWordSubscription;
        private string _selectedWord;
        private Brush _selectedWordBrush;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public new TextDocument Document
        {
            get { return (TextDocument)base.Document; }
        }


        /// <summary>
        /// Gets or sets the <see cref="ICSharpCode.AvalonEdit.TextEditor"/> control.
        /// </summary>
        /// <value>The <see cref="ICSharpCode.AvalonEdit.TextEditor"/> control.</value>
        public TextEditor TextEditor
        {
            get { return _textEditor; }
            set
            {
                if (_textEditor == value)
                    return;

                _updateStatusSubscription?.Dispose();
                _updateStatusSubscription = null;

                _markSelectedWordSubscription?.Dispose();
                _markSelectedWordSubscription = null;
                _selectedWord = null;
                _selectedWordBrush = null;

                // Back up previous state.
                bool restore = false;
                double horizontalOffset = 0;
                double verticalOffset = 0;
                int caretOffset = 0;
                object selection = null;
                object foldings = null;
                if (_textEditor != null)
                {
                    restore = true;
                    horizontalOffset = _textEditor.HorizontalOffset;
                    verticalOffset = _textEditor.VerticalOffset;
                    caretOffset = _textEditor.CaretOffset;
                    selection = _textEditor.SaveSelection();
                    foldings = _textEditor.SaveFoldings();
                }

                _textEditor = value;

                if (_textEditor != null)
                {
                    _textEditor.Options = _textService.Options;
                    _textEditor.Document = Document.AvalonEditDocument;

                    // Restore previous state.
                    if (restore)
                    {
                        _textEditor.CaretOffset = caretOffset;
                        _textEditor.RestoreSelection(selection);
                        _textEditor.RestoreFoldings(foldings);

                        if (_textEditor.IsLoaded)
                        {
                            _textEditor.HorizontalOffset = horizontalOffset;
                            _textEditor.VerticalOffset = verticalOffset;
                        }
                        else
                        {
                            RoutedEventHandler handler = null;
                            handler = (s, e) =>
                                      {
                                          _textEditor.Loaded -= handler;
                                          _textEditor.HorizontalOffset = horizontalOffset;
                                          _textEditor.VerticalOffset = verticalOffset;
                                      };
                            _textEditor.Loaded += handler;
                        }
                    }

                    // Update IntelliSense.
                    UpdateSyntaxHighlighting();

                    // Monitor focus, caret position, and overstrike mode.
                    var gotFocusEvent = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => _textEditor.GotFocus += h, h => _textEditor.GotFocus -= h)
                                                  .Select(_ => Unit.Default);

                    var caretChangedEvent = Observable.FromEventPattern<EventHandler, EventArgs>(h => _textEditor.TextArea.Caret.PositionChanged += h, h => _textEditor.TextArea.Caret.PositionChanged -= h)
                                                      .Select(_ => Unit.Default);

                    var overstrikeChangedEvent = Observable.FromEventPattern(h => DependencyPropertyDescriptor.FromProperty(TextArea.OverstrikeModeProperty, typeof(TextArea)).AddValueChanged(_textEditor.TextArea, h),
                                                                             h => DependencyPropertyDescriptor.FromProperty(TextArea.OverstrikeModeProperty, typeof(TextArea)).RemoveValueChanged(_textEditor.TextArea, h))
                                                           .Select(_ => Unit.Default);

                    _updateStatusSubscription = gotFocusEvent.Merge(caretChangedEvent)
                                                             .Merge(overstrikeChangedEvent)
                                                             .Subscribe(_ => UpdateStatusInfo());

                    var textChangedEvent = Observable.FromEventPattern<EventHandler, EventArgs>(h => _textEditor.TextChanged += h, h => _textEditor.TextChanged -= h)
                                                     .Select(_ => Unit.Default);

                    _markSelectedWordSubscription = caretChangedEvent.Merge(textChangedEvent)
                                                                     .Throttle(TimeSpan.FromSeconds(0.2f))
                                                                     .ObserveOnDispatcher()
                                                                     .Subscribe(_ => UpdateSelectedWord());

                    WindowsHelper.BeginInvokeOnUI(UpdateSelectedWord);
                }

                RaisePropertyChanged();
            }
        }
        private TextEditor _textEditor;


        /// <summary>
        /// Gets or sets the syntax highlighting.
        /// </summary>
        /// <value>The syntax highlighting.</value>
        public IHighlightingDefinition SyntaxHighlighting
        {
            get { return _syntaxHighlighting; }
            set
            {
                if (_syntaxHighlighting == value)
                    return;

                _syntaxHighlighting = value;
                ConfigureIntelliSense();
                RaisePropertyChanged();
                (_textService as TextExtension)?.UpdateSyntaxHighlightingItem();
            }
        }
        private IHighlightingDefinition _syntaxHighlighting;


        /// <summary>
        /// Gets or sets the context menu of the text editor.
        /// </summary>
        /// <value>The context menu of the text editor.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public MenuItemViewModelCollection TextContextMenu
        {
            get { return _textContextMenu; }
            set { SetProperty(ref _textContextMenu, value); }
        }
        private MenuItemViewModelCollection _textContextMenu;


        /// <summary>
        /// Gets or sets the "Print Preview" command.
        /// </summary>
        /// <value>The "Print Preview" command.</value>
        public DelegateCommand PrintPreviewCommand { get; private set; }


        /// <summary>
        /// Gets or sets the "Print" command.
        /// </summary>
        /// <value>The "Print" command.</value>
        public DelegateCommand PrintCommand { get; private set; }


        /// <summary>
        /// Gets the "Find" command.
        /// </summary>
        /// <value>The "Find" command.</value>
        public DelegateCommand FindCommand { get; private set; }


        /// <summary>
        /// Gets the "Find And Replace" command.
        /// </summary>
        /// <value>The "Find" command.</value>
        public DelegateCommand FindAndReplaceCommand { get; private set; }


        /// <summary>
        /// Gets the "Cancel" command.
        /// </summary>
        /// <value>The "Cancel" command.</value>
        public DelegateCommand CancelCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDocumentViewModel"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public TextDocumentViewModel(TextDocument document)
          : base(document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var editor = document.Editor;

            // Mandatory services.
            _textService = editor.Services.GetInstance<ITextService>().ThrowIfMissing();
            _windowService = editor.Services.GetInstance<IWindowService>().ThrowIfMissing();
            
            // Optional services.
            _searchService = editor.Services.GetInstance<ISearchService>().WarnIfMissing();
            _highlightingService = editor.Services.GetInstance<IHighlightingService>().WarnIfMissing();

            _textContextMenu = _textService.ContextMenu;
            PrintPreviewCommand = new DelegateCommand(ShowPrintPreview);
            PrintCommand = new DelegateCommand(Print);
            FindCommand = new DelegateCommand(Find, CanFind);
            FindAndReplaceCommand = new DelegateCommand(FindAndReplace);
            CancelCommand = new DelegateCommand(Cancel);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Updates the syntax highlighting mode depending on the current document.
        /// </summary>
        public void UpdateSyntaxHighlighting()
        {
            if (_highlightingService == null)
                return;

            if (SyntaxHighlighting == null)
            {
                // Choose syntax highlighting based on file extension.
                var fileExtension = Path.GetExtension(Document.GetName());
                if (fileExtension != null)
                    SyntaxHighlighting = _highlightingService.GetDefinitionByExtension(fileExtension);
            }

            if (SyntaxHighlighting == null)
            {
                // Check for XML header.
                if (SyntaxHighlighting == null && !Document.IsUntitled && TextDocumentHelper.IsXmlFile(Document.Uri.LocalPath))
                    SyntaxHighlighting = _highlightingService.GetDefinition("XML");
            }

            ConfigureIntelliSense();
        }


        private void ConfigureIntelliSense()
        {
            // Note: ConfigureIntelliSense() is called redundantly.

            if (_textEditor == null)
                return;

            // Choose a suitable folding and indentation strategy for the document.
            // We can derive the document type from the syntax highlighting name: XML, C#, C++, PHP, Java
            var highlighting = SyntaxHighlighting;
            string language = highlighting?.Name;
            switch (language)
            {
                case "XML":
                    if (!(_textEditor.FoldingStrategy is XmlFoldingStrategy))
                        _textEditor.FoldingStrategy = new XmlFoldingStrategy();
                    if (!(_textEditor.FormattingStrategy is XmlFormattingStrategy))
                        _textEditor.FormattingStrategy = new XmlFormattingStrategy();
                    if (!(_textEditor.TextArea.IndentationStrategy is XmlIndentationStrategy))
                        _textEditor.TextArea.IndentationStrategy = new XmlIndentationStrategy();
                    break;
                case "C#":
                case "C++":
                case "PHP":
                case "Java":
                    if (!(_textEditor.FoldingStrategy is CSharpFoldingStrategy))
                        _textEditor.FoldingStrategy = new CSharpFoldingStrategy();
                    if (!(_textEditor.FormattingStrategy is CSharpFormattingStrategy))
                        _textEditor.FormattingStrategy = new CSharpFormattingStrategy();
                    if (!(_textEditor.TextArea.IndentationStrategy is CSharpIndentationStrategy))
                        _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy();
                    break;
                case "HLSL":
                case "Cg":
                    if (!(_textEditor.FoldingStrategy is HlslFoldingStrategy))
                        _textEditor.FoldingStrategy = new HlslFoldingStrategy();
                    if (!(_textEditor.FormattingStrategy is HlslFormattingStrategy))
                        _textEditor.FormattingStrategy = new HlslFormattingStrategy();
                    if (!(_textEditor.TextArea.IndentationStrategy is HlslIntendationStrategy))
                        _textEditor.TextArea.IndentationStrategy = new HlslIntendationStrategy();
                    break;
                default:
                    _textEditor.FoldingStrategy = new DefaultFoldingStrategy();
                    _textEditor.FormattingStrategy = null;
                    if (!(_textEditor.TextArea.IndentationStrategy is DefaultIndentationStrategy))
                        _textEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                    break;
            }

            _textEditor.UpdateFoldings();
        }


        private void ShowPrintPreview()
        {
            // Get the page size from the print dialog.
            var printDialog = new PrintDialog();
            var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

            // Convert document into FixedDocument.
            var fixedDocument = TextEditor.CreateFixedDocument(pageSize, DisplayName);

            var viewModel = new PrintPreviewViewModel { PrintDocument = fixedDocument };

            // Show print preview dialog.
            _windowService.ShowDialog(viewModel);
        }


        private void Print()
        {
            // Show print dialog.
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Get the page size from the print dialog.
                var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                // Convert document into FixedDocument.
                var fixedDocument = TextEditor.CreateFixedDocument(pageSize, DisplayName);

                // Print.
                printDialog.PrintDocument(fixedDocument.DocumentPaginator, DisplayName);
            }
        }


        private bool CanFind()
        {
            return _searchService != null;
        }


        private void Find()
        {
            if (_searchService == null)
                return;

            string selection = TextEditor.SelectedText;
            if (!string.IsNullOrEmpty(selection))
                _searchService.Query.FindPattern = selection;

            _searchService.ShowQuickFind();
        }


        private void FindAndReplace()
        {
            if (_searchService == null)
                return;

            string selection = TextEditor.SelectedText;
            if (!string.IsNullOrEmpty(selection))
                _searchService.Query.FindPattern = selection;

            _searchService.ShowFindAndReplace();
        }


        private void Cancel()
        {
            if (_searchService == null)
                return;

            _searchService.Query.FindPattern = null;
        }


        private void UpdateStatusInfo()
        {
            var textArea = TextEditor.TextArea;
            var caret = textArea.Caret;
            _textService.SetStatusInfo(caret.Line, caret.VisualColumn + 1, caret.Column, textArea.OverstrikeMode);
        }


        private void UpdateSelectedWord()
        {
            // Find word near cursor or text in selection.
            string selectedWord;
            if (TextEditor.SelectionLength == 0)
            {
                selectedWord = TextUtilities.GetIdentifierAt(TextEditor.Document, TextEditor.CaretOffset);
            }
            else
            {
                selectedWord = TextEditor.SelectedText;
            }

            if (selectedWord == _selectedWord && _selectedWordBrush != null)
                return;

            if (_selectedWordBrush == null)
                _selectedWordBrush = Application.Current
                                                .FindResource("TextEditor.SelectedWordBackground")
                                                as Brush;

            _selectedWord = selectedWord;

            // Remove old selected word markers.
            var markers = Document.SelectionMarkers;
            markers.Clear();

            if (_selectedWordBrush == null)
                return;

            if (string.IsNullOrEmpty(selectedWord))
                return;

            // Create new markers.
            int offset = 0;
            string text = TextEditor.Text;
            while (offset >= 0 && offset < text.Length)
            {
                offset = text.IndexOf(selectedWord, offset, StringComparison.CurrentCulture);
                if (offset >= 0)
                {
                    // Do not add a marker under the current selection.
                    if (TextEditor.SelectionLength == 0 || offset != TextEditor.SelectionStart)
                    {
                        markers.Add(new SelectedWordMarker(_selectedWordBrush)
                        {
                            StartOffset = offset,
                            Length = selectedWord.Length,
                        });
                    }
                    offset += selectedWord.Length;
                }
            }
        }
        #endregion
    }
}
