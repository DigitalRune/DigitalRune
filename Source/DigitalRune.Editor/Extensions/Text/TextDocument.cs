// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Properties;
using DigitalRune.Editor.Search;
using DigitalRune.Windows.Controls;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;
using AvalonEditDocument = ICSharpCode.AvalonEdit.Document.TextDocument;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Represents an editable text document.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class TextDocument : Document, ISearchable
    {
        //--------------------------------------------------------------
        #region Nested types
        //--------------------------------------------------------------

        private enum LineEndingType
        {
            Unknown,
            CRLF,  // Windows CR+LF
            CR,    // Macintosh CR
            LF,    // Unix LF
            LS,    // Unicode Line Separator LS
            PS,    // Unicode Paragraph Separator PS
            Mixed,
        }
        #endregion


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        // Line ending characters.
        private const char CarriageReturn = '\u000D';
        private const char LineFeed = '\u000A';
        private const char LineSeparator = '\u2028';
        private const char ParagraphSeparator = '\u2029';
        #endregion



        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false);

        private readonly ISearchService _searchService;
        private readonly IPropertiesService _propertiesService;

        private readonly PropertySource _propertySource = new PropertySource();
        private bool _updateProperties;
        private FileInfo _fileInfo;
        private Encoding _encoding;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the text document of the AvalonEdit text editor.
        /// </summary>
        /// <value>The text document of the AvalonEdit text editor.</value>
        public AvalonEditDocument AvalonEditDocument { get; }


        /// <summary>
        /// Gets the text selection markers.
        /// </summary>
        /// <value>The selection markers.</value>
        public TextSegmentCollection<Marker> SelectionMarkers { get; }


        /// <summary>
        /// Gets the search markers.
        /// </summary>
        /// <value>The search markers.</value>
        public TextSegmentCollection<Marker> SearchMarkers { get; }


        /// <summary>
        /// Gets the error markers.
        /// </summary>
        /// <value>The error markers.</value>
        public TextSegmentCollection<Marker> ErrorMarkers { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDocument"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentType">The type of the document.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentType"/> is
        /// <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TextExtension")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TextDocument(IEditorService editor, DocumentType documentType)
            : base(editor, documentType)
        {
            // Optional services:
            _searchService = editor.Services.GetInstance<ISearchService>().WarnIfMissing();
            _propertiesService = editor.Services.GetInstance<IPropertiesService>().WarnIfMissing();

            AvalonEditDocument = new AvalonEditDocument();
            SelectionMarkers = new TextSegmentCollection<Marker>();
            SearchMarkers = new TextSegmentCollection<Marker>();
            ErrorMarkers = new TextSegmentCollection<Marker>();

            InitializeSearch();

            // The UndoStack indicates whether changes were made to the document.
            AvalonEditDocument.UndoStack.PropertyChanged += OnUndoStackChanged;
            AvalonEditDocument.TextChanged += OnTextChanged;

            Editor.ActiveDockTabItemChanged += OnEditorDockTabItemChanged;

            BeginInvokeUpdateProperties();
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="TextDocument"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    UninitializeSearch();

                    if (_propertiesService != null && _propertiesService.PropertySource == _propertySource)
                        _propertiesService.PropertySource = null;

                    _fileInfo = null;
                    _encoding = null;

                    Editor.ActiveDockTabItemChanged -= OnEditorDockTabItemChanged;
                }

                // Release unmanaged resources.

            }

            base.Dispose(disposing);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override DocumentViewModel OnCreateViewModel()
        {
            return new TextDocumentViewModel(this);
        }


        /// <inheritdoc/>
        protected override void OnLoad()
        {
            // Store caret positions before reload.
            var caretPositions = new TextViewPosition[ViewModels.Count];
            for (int i = 0; i < ViewModels.Count; i++)
                caretPositions[i] = ((TextDocumentViewModel)ViewModels[i]).TextEditor.TextArea.Caret.Position;

            string text;
            using (var fileStream = FileReader.OpenFile(Uri.LocalPath, UTF8NoBOM))
            {
                _encoding = fileStream.CurrentEncoding;
                text = fileStream.ReadToEnd();
            }

            // Check for binary files.
            // See http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
            foreach (char c in text)
            {
                if (char.IsControl(c) && c != '\t' && c != '\n' && c != '\r')
                {
                    // Binary file detected. Cancel?
                    var result = MessageBox.Show(
                        "Loading file: \"" + this.GetName() + "\"\n\n" +
                        "Unsupported file format. Do you want to open the file as a text document?\n\n" +
                        "Warning: Binary files can slow down the text editor.",
                        Editor.ApplicationName,
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Cancel)
                        throw new OperationCanceledException();

                    break;
                }
            }

            AvalonEditDocument.Text = text;
            AvalonEditDocument.UndoStack.ClearAll();
            AvalonEditDocument.UndoStack.MarkAsOriginalFile();

            _fileInfo = new FileInfo(Uri.LocalPath);

            // Update syntax-highlighting mode of all views.
            foreach (var view in ViewModels.OfType<TextDocumentViewModel>())
                view.UpdateSyntaxHighlighting();

            // Restore caret position
            for (int i = 0; i < ViewModels.Count; i++)
                ((TextDocumentViewModel)ViewModels[i]).TextEditor.TextArea.Caret.Position = caretPositions[i];

            BeginInvokeUpdateProperties();
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected override void OnSave()
        {
            // Important: The constructor StreamWriter(stream) uses UTF-8 encoding without a
            // Byte-Order Mark (BOM). Many tools, such as the DirectX effect compiler (fxc.exe),
            // cannot read files with BOM.
            using (var stream = new FileStream(Uri.LocalPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(stream))
                {
                    _encoding = writer.Encoding;
                    AvalonEditDocument.WriteTextTo(writer);
                }
            }

            // Place marker in Undo stack to mark current state as "original".
            AvalonEditDocument.UndoStack.MarkAsOriginalFile();

            _fileInfo = new FileInfo(Uri.LocalPath);

            BeginInvokeUpdateProperties();
        }


        private void OnUndoStackChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // When the document is created or saved, a marker is placed in the undo stack.
            // The marker indicates the unmodified state of the document.
            IsModified = !AvalonEditDocument.UndoStack.IsOriginalFile;
        }


        private void OnTextChanged(object sender, EventArgs eventArgs)
        {
            // Markers are currently not anchored. 
            // Temporary workaround: Clear markers when document is modified.
            ErrorMarkers.Clear();

            BeginInvokeUpdateProperties();
        }


        private void OnEditorDockTabItemChanged(object sender, EventArgs eventArgs)
        {
            // One of our view models was activated. --> Show our properties.
            var documentViewModel = Editor.ActiveDockTabItem as TextDocumentViewModel;
            if (documentViewModel != null && documentViewModel.Document == this)
                if (_propertiesService != null)
                    _propertiesService.PropertySource = _propertySource;
        }


        private void BeginInvokeUpdateProperties()
        {
            _updateProperties = true;
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)UpdateProperties);
        }


        private void UpdateProperties()
        {
            // TODO: We could skip this if the Properties window is hidden.

            // Avoid redundant execution if several updates are queued.
            if (!_updateProperties)
                return;

            _updateProperties = false;

            _propertySource.Name = IsUntitled ? UntitledName : Path.GetFileName(Uri.LocalPath);
            _propertySource.TypeName = (DocumentType.Name != TextDocumentFactory.AnyDocumentTypeName)
                                     ? DocumentType.Name
                                     : "Text file";

            var textBlockKey = new ComponentResourceKey(typeof(PropertyGrid), "TextBlock");

            _propertySource.Properties.Clear();
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "File",
                Name = "File name",
                Value = this.GetName(),
                Description = "The filename of the image.",
                PropertyType = typeof(string),
                //DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Text",
                Name = "Character count",
                Value = AvalonEditDocument.TextLength,
                Description = "The number of characters including whitespace characters.",
                PropertyType = typeof(int),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });

            int numberOfWords;
            LineEndingType lineEndingType;
            GetTextStatistics(AvalonEditDocument.Text, out numberOfWords, out lineEndingType);

            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Text",
                Name = "Word count",
                Value = numberOfWords,
                Description = "The number of words.",
                PropertyType = typeof(int),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            _propertySource.Properties.Add(new CustomProperty
            {
                Category = "Text",
                Name = "Line endings",
                Value = GetLineEndingName(lineEndingType),
                Description = "The line endings used in the text.",
                PropertyType = typeof(string),
                DataTemplateKey = textBlockKey,
                CanReset = false,
                IsReadOnly = true,
            });
            if (_encoding != null)
            {
                _propertySource.Properties.Add(new CustomProperty
                {
                    Category = "Text",
                    Name = "Encoding",
                    Value = _encoding.EncodingName,
                    Description = "The encoding used in the file.",
                    PropertyType = typeof(string),
                    DataTemplateKey = textBlockKey,
                    CanReset = false,
                    IsReadOnly = true,
                });
            }
            if (_fileInfo != null)
            {
                _propertySource.Properties.Add(new CustomProperty
                {
                    Category = "File",
                    Name = "File size",
                    Value = string.Format(CultureInfo.CurrentCulture, "{0:F2} kB", _fileInfo.Length / 1024.0),
                    Description = "The file size in kilobyte (1kB = 1024 bytes).",
                    PropertyType = typeof(string),
                    DataTemplateKey = textBlockKey,
                    CanReset = false,
                    IsReadOnly = true,
                });
                _propertySource.Properties.Add(new CustomProperty
                {
                    Category = "File",
                    Name = "Last modified",
                    Value = string.Format(CultureInfo.CurrentCulture, "{0}", _fileInfo.LastWriteTime),
                    Description = "The time of the last write access.",
                    PropertyType = typeof(string),
                    DataTemplateKey = textBlockKey,
                    CanReset = false,
                    IsReadOnly = true,
                });
            }
        }


        private static void GetTextStatistics(string text, out int numberOfWords, out LineEndingType lineEndingType)
        {
            // Examples: 
            // "blah." = 1 word
            // "blah & blah" = 3 words
            // "blah-blah" = 1 word

            // Make sure the text ends with a space.
            text = text + " ";

            int numberOfCRLF = 0;
            int numberOfCR = 0;
            int numberOfLF = 0;
            int numberOfLS = 0;
            int numberOfPS = 0;

            numberOfWords = 0;
            for (int i = 0; i < text.Length - 1; i++)
            {
                // Every word ends with a whitespace character.
                if (char.IsWhiteSpace(text[i + 1])
                    && (char.IsLetterOrDigit(text[i]) || char.IsPunctuation(text[i])))
                    numberOfWords++;

                if (text[i] == CarriageReturn)
                {
                    if (text[i + 1] == LineFeed)
                        numberOfCRLF++;
                    else
                        numberOfCR++;
                }
                else if (text[i] == LineFeed)
                {
                    // CRLF was already counted.
                    if (i == 0 || text[i - 1] != CarriageReturn)
                        numberOfLF++;
                }
                else if (text[i] == LineSeparator)
                {
                    numberOfLS++;
                }
                else if (text[i] == ParagraphSeparator)
                {
                    numberOfPS++;
                }
            }

            if (numberOfCRLF == 0 && numberOfCR == 0 && numberOfLF == 0 && numberOfLS == 0 && numberOfPS == 0)
                lineEndingType = LineEndingType.Unknown;
            else if (numberOfCR == 0 && numberOfLF == 0 && numberOfLS == 0 && numberOfPS == 0)
                lineEndingType = LineEndingType.CRLF;
            else if (numberOfCRLF == 0 && numberOfLF == 0 && numberOfLS == 0 && numberOfPS == 0)
                lineEndingType = LineEndingType.CR;
            else if (numberOfCRLF == 0 && numberOfCR == 0 && numberOfLS == 0 && numberOfPS == 0)
                lineEndingType = LineEndingType.LF;
            else if (numberOfCRLF == 0 && numberOfCR == 0 && numberOfLF == 0 && numberOfPS == 0)
                lineEndingType = LineEndingType.LS;
            else if (numberOfCRLF == 0 && numberOfCR == 0 && numberOfLF == 0 && numberOfLS == 0 )
                lineEndingType = LineEndingType.PS;
            else 
                lineEndingType = LineEndingType.Mixed;
        }


        private static string GetLineEndingName(LineEndingType lineEndingType)
        {
            switch (lineEndingType)
            {
                case LineEndingType.Unknown:
                    return "Unknown";
                case LineEndingType.CRLF:
                    return "Windows CR+LF";
                case LineEndingType.LF:
                    return "Unix LF";
                case LineEndingType.CR:
                    return "Macintosh CR";
                case LineEndingType.LS:
                    return "Unicode Line Separator";
                case LineEndingType.PS:
                    return "Unicode Paragraph Separator";
                case LineEndingType.Mixed:
                    return "Mixed";
            }

            throw new InvalidOperationException("This line must never be reached.");
        }
        #endregion
    }
}
