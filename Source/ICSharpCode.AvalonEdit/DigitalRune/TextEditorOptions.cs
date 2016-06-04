using System.ComponentModel;
using System.Configuration;
using System.Reflection;


namespace ICSharpCode.AvalonEdit
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    partial class TextEditorOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether matching brackets are highlighted.
        /// </summary>
        /// <value><see langword="true"/> if matching brackets are highlighted; otherwise,
        /// <see langword="false"/>.</value>
        public bool EnableBracketHighlighting
        {
            get { return _enableBracketHighlighting; }
            set
            {
                if (_enableBracketHighlighting != value)
                {
                    _enableBracketHighlighting = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(EnableBracketHighlighting)));
                }
            }
        }
        private bool _enableBracketHighlighting = true;


        // TODO: Add brush and pen with serialization.
        ///// <summary>
        ///// Gets or sets the brush used to highlight matching brackets.
        ///// </summary>
        ///// <value>The bracket highlighting brush.</value>
        //public Brush BracketHighlightingBrush
        //{
        //  get { return _bracketHighlightingBrush; }
        //  set
        //  {
        //    if (_bracketHighlightingBrush != value)
        //    {
        //      _bracketHighlightingBrush = value;
        //      OnPropertyChanged(new PropertyChangedEventArgs(nameof(BracketHighlightingBrush)));
        //    }
        //  }
        //}
        //private Brush _bracketHighlightingBrush;


        ///// <summary>
        ///// Gets or sets the pen used to highlight matching brackets.
        ///// </summary>
        ///// <value>The bracket highlighting pen.</value>
        //public Pen BracketHighlightingPen
        //{
        //  get { return _bracketHighlightingPen; }
        //  set
        //  {
        //    if (_bracketHighlightingPen != value)
        //    {
        //      _bracketHighlightingPen = value;
        //      OnPropertyChanged(new PropertyChangedEventArgs(nameof(BracketHighlightingPen)));
        //    }
        //  }
        //}
        //private Pen _bracketHighlightingPen;


        /// <summary>
        /// Gets or sets a value indicating whether folding ("outlining") is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if folding is enabled; otherwise, <see langword="false"/>.
        /// </value>
        public bool EnableFolding
        {
            get { return _enableFolding; }
            set
            {
                if (_enableFolding != value)
                {
                    _enableFolding = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(EnableFolding)));
                }
            }
        }
        private bool _enableFolding = true;


        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// <value>The font family.</value>
        public string FontFamily
        {
            get { return _fontFamily; }
            set
            {
                //if (String.IsNullOrEmpty(value))
                //  throw new ArgumentNullException("value", "FontFamily is null or empty.");
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontFamily)));
                }
            }
        }
        private string _fontFamily = "Consolas";


        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        /// <value>The font size.</value>
        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontSize)));
                }
            }
        }
        private double _fontSize = 10 * 96.0 / 72.0;  // 10 pt = 13.333 px


        /// <summary>
        /// Gets or sets the font stretch.
        /// </summary>
        /// <value>The font style.</value>
        public string FontStretch
        {
            get { return _fontStretch; }
            set
            {
                if (_fontStretch != value)
                {
                    _fontStretch = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontStretch)));
                }
            }
        }
        private string _fontStretch = "Normal";


        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        /// <value>The font style.</value>
        public string FontStyle
        {
            get { return _fontStyle; }
            set
            {
                if (_fontStyle != value)
                {
                    _fontStyle = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontStyle)));
                }
            }
        }
        private string _fontStyle = "Normal";


        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        /// <value>The font weight.</value>
        public string FontWeight
        {
            get { return _fontWeight; }
            set
            {
                if (_fontWeight != value)
                {
                    _fontWeight = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(FontWeight)));
                }
            }
        }
        private string _fontWeight = "Normal";


        /// <summary>
        /// Gets or sets a value indicating whether line number shall be shown.
        /// </summary>
        /// <value><see langword="true"/> if line numbers are visible; otherwise, <see langword="false"/>.</value>
        public bool ShowLineNumbers
        {
            get { return _showLineNumbers; }
            set
            {
                if (_showLineNumbers != value)
                {
                    _showLineNumbers = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ShowLineNumbers)));
                }
            }
        }
        private bool _showLineNumbers = true;


        /// <summary>
        /// Gets or sets a value indicating whether word wrapping is used.
        /// </summary>
        /// <value><see langword="true"/> if word wrapping is used; otherwise, <see langword="false"/>.</value>
        public bool WordWrap
        {
            get { return _wordWrap; }
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(WordWrap)));
                }
            }
        }
        private bool _wordWrap;


        /// <summary>
        /// Copies the options from the specified object.
        /// </summary>
        /// <param name="options">The options to copy.</param>
        public void Set(TextEditorOptions options)
        {
            var fields = typeof(TextEditorOptions).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in fields)
            {
                if (!fieldInfo.IsNotSerialized)
                    fieldInfo.SetValue(this, fieldInfo.GetValue(options));
            }

            OnPropertyChanged(string.Empty);
        }
    }
}
