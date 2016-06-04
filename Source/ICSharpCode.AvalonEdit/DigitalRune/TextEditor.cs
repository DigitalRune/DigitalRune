using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private MarkerRenderer _bracketMarkerRenderer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ColumnRuler"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnRulerProperty = DependencyProperty.Register(
            "ColumnRuler",
            typeof(Pen),
            typeof(TextEditor),
            new FrameworkPropertyMetadata(CreateFrozenPen(Brushes.LightGray), OnColumnRulerChanged));

        /// <summary>
        /// Gets or sets the pen used to draw the column ruler. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The pen used to draw the column ruler.</value>
        [Description("Gets or sets the pen used to draw the column ruler.")]
        [Category("Appearance")]
        public Pen ColumnRuler
        {
            get { return (Pen)GetValue(ColumnRulerProperty); }
            set { SetValue(ColumnRulerProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Called in the constructor of <see cref="TextEditor"/>.
        /// </summary>
        private void Initialize()
        {
            // ----- Bracket markers
            _bracketMarkerRenderer = new MarkerRenderer(TextArea.TextView) { Markers = new TextSegmentCollection<Marker>() };
            _bracketMarkerRenderer.Markers.CollectionChanged += (s, e) => TextArea.TextView.InvalidateLayer(_bracketMarkerRenderer.Layer);
            TextArea.TextView.BackgroundRenderers.Add(_bracketMarkerRenderer);

            // ----- Bracket highlighting
            TextArea.Caret.PositionChanged += (s, e) => UpdateBracketHighlighting();

            // ----- Formatting
            TextArea.TextEntered += OnTextEntered;

            // ----- Add command bindings
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.PasteMultiple, OnPasteMultiple, CanPasteMultiple));
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.Comment, (s, e) => CommentSelection(), CanCommentSelection));
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.Uncomment, (s, e) => UncommentSelection(), CanCommentSelection));
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.ToggleAllFolds, (s, e) => ToggleAllFoldings(), CanToggleFold));
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.ToggleFold, (s, e) => ToggleCurrentFolding(), CanToggleFold));
            CommandBindings.Add(new CommandBinding(AvalonEditCommands.SyntaxHighlighting, (s, e) => SyntaxHighlighting = e.Parameter as IHighlightingDefinition));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static Pen CreateFrozenPen(SolidColorBrush brush)
        {
            var pen = new Pen(brush, 1);
            pen.Freeze();
            return pen;
        }


        private static void OnColumnRulerChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var textEditor = (TextEditor)dependencyObject;
            textEditor.OnColumnRulerChanged();
        }


        private void OnColumnRulerChanged()
        {
            var pen = ColumnRuler ?? CreateFrozenPen(Brushes.LightGray);
            textArea.TextView.ColumnRulerPen = pen;
        }


        /// <summary>
        /// Called when document changes.
        /// </summary>
        private void OnDocumentChanged()
        {
            UpdateFoldingManager();
        }


        private void OnOptionsChanged()
        {
            var options = Options;
            if (options == null)
                return;

            EnableBracketHighlighting = options.EnableBracketHighlighting;
            EnableFolding = options.EnableFolding;
            ShowLineNumbers = options.ShowLineNumbers;
            WordWrap = options.WordWrap;

            if (!string.IsNullOrEmpty(options.FontFamily))
                FontFamily = new FontFamily(options.FontFamily);
            if (options.FontSize > 0)
                FontSize = options.FontSize;
            if (!string.IsNullOrEmpty(options.FontStretch))
                FontStretch = (FontStretch)new FontStretchConverter().ConvertFrom(options.FontStretch);
            if (!string.IsNullOrEmpty(options.FontStyle))
                FontStyle = (FontStyle)new FontStyleConverter().ConvertFrom(options.FontStyle);
            if (!string.IsNullOrEmpty(options.FontWeight))
                FontWeight = (FontWeight)new FontWeightConverter().ConvertFrom(options.FontWeight);
        }


        ///// <summary>
        ///// Gets a brush based on the current syntax highlighting.
        ///// </summary>
        ///// <param name="name">
        ///// The name of the brush/color. See <see cref="HighlightingKnownNames"/>.
        ///// </param>
        ///// <returns>
        ///// The brush. Returns <see langword="null"/> if no syntax highlighting is set, or the syntax
        ///// highlighting does not define a color with specified name.
        ///// </returns>
        //internal Brush GetNamedBrush(string name)
        //{
        //  var brush = (Brush)null;
        //  var syntaxHighlighting = SyntaxHighlighting;
        //  if (syntaxHighlighting != null)
        //  {
        //    var highlightingColor = syntaxHighlighting.GetNamedColor(name);
        //    if (highlightingColor != null)
        //    {
        //      var highlightingBrush = highlightingColor.Background;
        //      if (highlightingBrush != null)
        //        brush = highlightingBrush.GetBrush(null);
        //    }
        //  }

        //  return brush;
        //}


        /// <summary>
        /// Reloads the syntax highlighting.
        /// </summary>
        public void ReloadSyntaxHighlighting()
        {
            OnSyntaxHighlightingChanged(SyntaxHighlighting);
        }


        /// <summary>
        /// Saves the current selection.
        /// </summary>
        /// <returns>An object representing the selection.</returns>
        public object SaveSelection()
        {
            return TextArea.Selection;
        }


        /// <summary>
        /// Restores the previously saved selection.
        /// </summary>
        /// <param name="selection">The object representing the selection.</param>
        public void RestoreSelection(object selection)
        {
            if (selection == null)
                return;

            if (selection is EmptySelection)
            {
                TextArea.Selection = new EmptySelection(TextArea);
            }
            else if (selection is SimpleSelection)
            {
                var simpleSelection = (SimpleSelection)selection;
                TextArea.Selection = new SimpleSelection(TextArea, simpleSelection.StartPosition, simpleSelection.EndPosition);
            }
            else if (selection is RectangleSelection)
            {
                var rectangleSelection = (RectangleSelection)selection;
                TextArea.Selection = new RectangleSelection(TextArea, rectangleSelection.StartPosition, rectangleSelection.EndPosition);
            }
        }
        #endregion
    }
}
