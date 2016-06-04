using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        // TODO: Refactor bracket highlighting.
        // - Create interface IBracketSearchStrategy (similar to IFoldingStrategy, IIndentationStrategy).
        // - Create a default CSharpBracketSearchStrategy and move the required functions from
        //   TextUtilities into the strategy.
        //   SharpDevelop has a similar interface. See:
        //     SharpDevelop\src\Main\Base\Project\Src\Editor\IBracketSearcher.cs
        // - Add method GoToMatchingBracket(). See
        //     SharpDevelop\src\Main\Base\Project\Src\Editor\Commands\GoToMatchingBrace.cs


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly char[] OpeningBrackets = { '(', '{', '[' };
        private static readonly char[] ClosingBrackets = { ')', '}', ']' };
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="EnableBracketHighlighting"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableBracketHighlightingProperty = DependencyProperty.Register(
            "EnableBracketHighlighting",
            typeof(bool),
            typeof(TextEditor),
            new FrameworkPropertyMetadata(Boxes.True));

        /// <summary>
        /// Gets or sets a value indicating whether matching brackets are highlighted. 
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to highlight matching brackets; otherwise <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether matching brackets are highlighted.")]
        [Category("Behavior")]
        public bool EnableBracketHighlighting
        {
            get { return (bool)GetValue(EnableBracketHighlightingProperty); }
            set { SetValue(EnableBracketHighlightingProperty, Boxes.Box(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="BracketHighlightingBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BracketHighlightingBrushProperty = DependencyProperty.Register(
            "BracketHighlightingBrush",
            typeof(Brush),
            typeof(TextEditor),
            new FrameworkPropertyMetadata(Brushes.Cyan));

        /// <summary>
        /// Gets or sets the background brush used to highlight matching brackets. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The background brush used to highlight matching brackets.</value>
        [Description("Gets or sets the background brush used to highlight matching brackets.")]
        [Category("Appearance")]
        public Brush BracketHighlightingBrush
        {
            get { return (Brush)GetValue(BracketHighlightingBrushProperty); }
            set { SetValue(BracketHighlightingBrushProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="BracketHighlightingPen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BracketHighlightingPenProperty = DependencyProperty.Register(
            "BracketHighlightingPen",
            typeof(Pen),
            typeof(TextEditor),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the border pen used to highlight matching brackets. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The border pen used to highlight matching brackets.</value>
        [Description("Gets or sets the border pen used to highlight matching brackets.")]
        [Category("Appearance")]
        public Pen BracketHighlightingPen
        {
            get { return (Pen)GetValue(BracketHighlightingPenProperty); }
            set { SetValue(BracketHighlightingPenProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void UpdateBracketHighlighting()
        {
            if (_bracketMarkerRenderer == null)
                return;

            var markers = _bracketMarkerRenderer.Markers;

            // Remove old markers.
            markers.Clear();

            if (!EnableBracketHighlighting || (BracketHighlightingBrush == null && BracketHighlightingPen == null))
                return;

            // See if we are after at closing bracket.
            if (CaretOffset >= 2)
            {
                // Get character before caret.
                var previousChar = Document.GetCharAt(CaretOffset - 1);
                int bracketIndex = Array.IndexOf(ClosingBrackets, previousChar);
                if (bracketIndex >= 0)
                {
                    // Caret is after a closing bracket.

                    // Find matching opening bracket.
                    int openingBracketOffset = TextUtilities.FindOpeningBracket(
                        Document,
                        CaretOffset - 2,
                        OpeningBrackets[bracketIndex],
                        ClosingBrackets[bracketIndex]);

                    if (openingBracketOffset >= 0)
                    {
                        // Opening bracket found. Mark both brackets.
                        var openBracketMarker0 = new BlockMarker { StartOffset = openingBracketOffset, Length = 1, Brush = BracketHighlightingBrush, Pen = BracketHighlightingPen };
                        var closeBracketMarker0 = new BlockMarker { StartOffset = CaretOffset - 1, Length = 1, Brush = BracketHighlightingBrush, Pen = BracketHighlightingPen };
                        markers.Add(openBracketMarker0);
                        markers.Add(closeBracketMarker0);
                    }
                }
            }

            // See if we are before an opening bracket.
            if (Document != null && CaretOffset < Document.TextLength - 1)
            {
                // Get character before caret.
                var nextChar = Document.GetCharAt(CaretOffset);
                int bracketIndex = Array.IndexOf(OpeningBrackets, nextChar);
                if (bracketIndex >= 0)
                {
                    // Caret is before an opening bracket.

                    // Find matching opening bracket.
                    int closingBracketOffset = TextUtilities.FindClosingBracket(
                        Document,
                        CaretOffset + 1,
                        OpeningBrackets[bracketIndex],
                        ClosingBrackets[bracketIndex]);

                    if (closingBracketOffset >= 0)
                    {
                        // Opening bracket found. Mark both brackets.
                        var openBracketMarker1 = new BlockMarker { StartOffset = CaretOffset, Length = 1, Brush = BracketHighlightingBrush, Pen = BracketHighlightingPen };
                        var closeBracketMarker1 = new BlockMarker { StartOffset = closingBracketOffset, Length = 1, Brush = BracketHighlightingBrush, Pen = BracketHighlightingPen };
                        markers.Add(openBracketMarker1);
                        markers.Add(closeBracketMarker1);
                    }
                }
            }
        }
        #endregion
    }
}
