//using System;
//using System.Globalization;
//using System.Windows;
//using System.Windows.Media;
//using ICSharpCode.AvalonEdit.Utils;


//namespace ICSharpCode.AvalonEdit.Rendering
//{
//  /// <summary>
//  /// Renders a vertical line at a given column.
//  /// </summary>
//  /// <remarks>
//  /// This will only work if a fixed-width font like "Courier" is used in the text editor.
//  /// For non-fixed-width fonts no vertical line is drawn.
//  /// </remarks>
//  public class VerticalLineRenderer : IBackgroundRenderer
//  {
//    //--------------------------------------------------------------
//    #region Fields
//    //--------------------------------------------------------------

//    private readonly TextEditor _textEditor;
//    private readonly Pen _pen;
//    #endregion


//    //--------------------------------------------------------------
//    #region Properties & Events
//    //--------------------------------------------------------------

//    /// <summary>
//    /// Gets or sets the column.
//    /// </summary>
//    /// <value>The column.</value>
//    /// <exception cref="ArgumentOutOfRangeException">
//    /// <paramref name="value"/> is 0 or negative.
//    /// </exception>
//    public int Column
//    {
//      get
//      { return _column; }
//      set
//      {
//        if (value <= 0)
//          throw new ArgumentOutOfRangeException("value", "The vertical line column must be greater than 0.");

//        _column = value;
//        if (Enabled)
//          _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
//      }
//    }
//    private int _column = 80;


//    /// <summary>
//    /// Gets or sets a value indicating whether this <see cref="VerticalLineRenderer"/> is enabled.
//    /// </summary>
//    /// <value>
//    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
//    /// </value>
//    public bool Enabled
//    {
//      get { return _enabled; }
//      set
//      {
//        if (_enabled == value)
//          return;

//        _enabled = value;
//        _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
//      }
//    }
//    private bool _enabled;



//    /// <summary>
//    /// Gets the layer on which this background renderer should draw.
//    /// </summary>
//    /// <value>The layer on which this background renderer should draw.</value>
//    public KnownLayer Layer
//    {
//      get { return KnownLayer.Background; }
//    }
//    #endregion


//    //--------------------------------------------------------------
//    #region Creation & Cleanup
//    //--------------------------------------------------------------

//    /// <summary>
//    /// Initializes a new instance of the <see cref="VerticalLineRenderer"/> class.
//    /// </summary>
//    /// <param name="textEditor">The editor.</param>
//    /// <exception cref="ArgumentNullException">
//    /// <paramref name="textEditor"/> is <see langword="null"/>.
//    /// </exception>
//    public VerticalLineRenderer(TextEditor textEditor)
//    {
//      if (textEditor == null)
//        throw new ArgumentNullException("textEditor");

//      _textEditor = textEditor;
//      _pen = new Pen(Brushes.LightGray, 1f) { DashStyle = DashStyles.Dot };
//      _textEditor.TextArea.Caret.PositionChanged += OnCaretChanged;
//    }
//    #endregion


//    //--------------------------------------------------------------
//    #region Methods
//    //--------------------------------------------------------------

//    /// <summary>
//    /// Causes the background renderer to draw.
//    /// </summary>
//    /// <param name="textView"></param>
//    /// <param name="drawingContext"></param>
//    public void Draw(TextView textView, DrawingContext drawingContext)
//    {
//      // Draw only the relevant part.
//      if (!Enabled || textView.VisualLines.Count == 0)
//        return;

//      // We have to compute the width of a letter. We compute the width of A and i.
//      // If the width is equal, we assume that this is fixed-width font.
//      var typeface = _textEditor.CreateTypeface();

//      FormattedText formattedText = TextFormatterFactory.CreateFormattedText(
//        _textEditor,
//        "A",
//        typeface,
//        _textEditor.FontSize,
//        Brushes.Black);

//      FormattedText formattedText2 = TextFormatterFactory.CreateFormattedText(
//        _textEditor,
//        "i",
//        typeface,
//        _textEditor.FontSize,
//        Brushes.Black);

//      // Abort if the font is not fixed-width.
//      if (formattedText.Width != formattedText2.Width)
//        return;

//      // Compute line coordinates.
//      double x = formattedText.Width * Column - textView.HorizontalOffset;
//      var pixelSize = PixelSnapHelpers.GetPixelSize(textView);
//      x = PixelSnapHelpers.PixelAlign(x, pixelSize.Width);
//      var startY = PixelSnapHelpers.PixelAlign(0, pixelSize.Height);
//      var endY = PixelSnapHelpers.PixelAlign(textView.ActualHeight, pixelSize.Height);

//      // Draw line.
//      LineGeometry line = new LineGeometry(new Point(x, startY), new Point(x, endY));
//      drawingContext.DrawGeometry(null, _pen, line);
//    }


//    private void OnCaretChanged(object sender, EventArgs eventArgs)
//    {
//      if (Enabled)
//        _textEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
//    }
//    #endregion
//  }
//}
