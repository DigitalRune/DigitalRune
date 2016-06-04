// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DigitalRune.Mathematics;
using DigitalRune.Text;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that lets the user choose a font.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [TemplatePart(Name = "PART_FontFamilyTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_FontFamilyListBox", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_SizeTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_SizeListBox", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_TypefaceListBox", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_UnderlineCheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_BaselineCheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_OverlineCheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_StrikethroughCheckBox", Type = typeof(CheckBox))]
    public class FontChooser : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBox _fontFamilyTextBox;
        private ListBox _fontFamilyListBox;
        private TextBox _fontSizeTextBox;
        private ListBox _fontSizeListBox;
        private ListBox _typefaceListBox;
        private CheckBox _underlineCheckBox;
        private CheckBox _overlineCheckBox;
        private CheckBox _baselineCheckBox;
        private CheckBox _strikethroughCheckBox;

        private bool _isInternalUpdate;   // true if a UI update (e.g. check box change) is caused by code.
        private BackgroundWorker _backgroundWorker;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        private static readonly DependencyPropertyKey IsLoadingPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsLoading",
            typeof(bool),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Identifies the <see cref="IsLoading"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value indicating whether the <see cref="FontChooser"/> control is busy loading
        /// fonts. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The list of fonts is built in a background thread. The list box containing all font
        /// families is empty until the background thread has finished. The property
        /// <see cref="IsLoading"/> indicates whether the background thread is busy gathering all
        /// fonts. <see cref="IsLoading"/> is typically used in the control template to display an
        /// "Is Loading..." animation while its value is <see langword="true"/>.
        /// </remarks>
        [Browsable(false)]
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            private set { SetValue(IsLoadingPropertyKey, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="FontFamilies"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamiliesProperty = DependencyProperty.Register(
            "FontFamilies",
            typeof(ICollection<FontFamily>),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(null, (d, e) => ((FontChooser)d).OnFontFamiliesChanged()));

        /// <summary>
        /// Gets or sets the font families from which the user can choose.
        /// This is a dependency property.
        /// </summary>
        /// <value>A collection of font families from which the user can choose.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the font families from which the user can choose.")]
        [Category(Categories.Default)]
        public ICollection<FontFamily> FontFamilies
        {
            get { return (ICollection<FontFamily>)GetValue(FontFamiliesProperty); }
            set { SetValue(FontFamiliesProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="PreviewText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(
            "PreviewText",
            typeof(string),
            typeof(FontChooser),
            new FrameworkPropertyMetadata("The quick brown fox jumps over the lazy dog."));

        /// <summary>
        /// Gets or sets the text for the preview box.
        /// This is a dependency property.
        /// </summary>
        /// <value>The text for the preview box.</value>
        [Description("Gets or sets the text for the preview box.")]
        [Category(Categories.Default)]
        public string PreviewText
        {
            get { return (string)GetValue(PreviewTextProperty); }
            set { SetValue(PreviewTextProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedFontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedFontFamilyProperty = DependencyProperty.Register(
          "SelectedFontFamily",
          typeof(FontFamily),
          typeof(FontChooser),
          new FrameworkPropertyMetadata(TextBlock.FontFamilyProperty.DefaultMetadata.DefaultValue,
                                        (d, e) => ((FontChooser)d).OnSelectedFontFamilyChanged()));

        /// <summary>
        /// Gets or sets the selected font family.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected font family.</value>
        [Description("Gets or sets the selected font family.")]
        [Category(Categories.Default)]
        public FontFamily SelectedFontFamily
        {
            get { return (FontFamily)GetValue(SelectedFontFamilyProperty); }
            set { SetValue(SelectedFontFamilyProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedFontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedFontSizeProperty = DependencyProperty.Register(
            "SelectedFontSize",
            typeof(double),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(TextBlock.FontSizeProperty.DefaultMetadata.DefaultValue,
                                          (d, e) => ((FontChooser)d).OnSelectedFontSizeChanged()));

        /// <summary>
        /// Gets or sets the selected font size in pixels (px).
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected font size in pixels (px).</value>
        [Description("Gets or sets the selected font size in pixels (px).")]
        [Category(Categories.Default)]
        public double SelectedFontSize
        {
            get { return (double)GetValue(SelectedFontSizeProperty); }
            set { SetValue(SelectedFontSizeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedFontStretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedFontStretchProperty = DependencyProperty.Register(
            "SelectedFontStretch",
            typeof(FontStretch),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(TextBlock.FontStretchProperty.DefaultMetadata.DefaultValue,
                                          (d, e) => ((FontChooser)d).OnSelectedTypefaceChanged()));

        /// <summary>
        /// Gets or sets the selected font stretch.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected font stretch.</value>
        [Description("Gets or sets the selected font stretch.")]
        [Category(Categories.Default)]
        public FontStretch SelectedFontStretch
        {
            get { return (FontStretch)GetValue(SelectedFontStretchProperty); }
            set { SetValue(SelectedFontStretchProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedFontStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedFontStyleProperty = DependencyProperty.Register(
            "SelectedFontStyle",
            typeof(FontStyle),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(TextBlock.FontStyleProperty.DefaultMetadata.DefaultValue,
                                          (d, e) => ((FontChooser)d).OnSelectedTypefaceChanged()));

        /// <summary>
        /// Gets or sets the selected font style.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected font style.</value>
        [Description("Gets or sets the selected font style.")]
        [Category(Categories.Default)]
        public FontStyle SelectedFontStyle
        {
            get { return (FontStyle)GetValue(SelectedFontStyleProperty); }
            set { SetValue(SelectedFontStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedFontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedFontWeightProperty = DependencyProperty.Register(
            "SelectedFontWeight",
            typeof(FontWeight),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(TextBlock.FontWeightProperty.DefaultMetadata.DefaultValue,
                                          (d, e) => ((FontChooser)d).OnSelectedTypefaceChanged()));


        /// <summary>
        /// Gets or sets the selected font weight.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected font weight.</value>
        [Description("Gets or sets the selected font weight.")]
        [Category(Categories.Default)]
        public FontWeight SelectedFontWeight
        {
            get { return (FontWeight)GetValue(SelectedFontWeightProperty); }
            set { SetValue(SelectedFontWeightProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedTextDecorations"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTextDecorationsProperty = DependencyProperty.Register(
            "SelectedTextDecorations",
            typeof(TextDecorationCollection),
            typeof(FontChooser),
            new FrameworkPropertyMetadata(TextBlock.TextDecorationsProperty.DefaultMetadata.DefaultValue,
                                          (d, e) => ((FontChooser)d).OnSelectedTextDecorationsChanged()));

        /// <summary>
        /// Gets or sets the selected text decorations.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selected text decorations.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the selected text decorations.")]
        [Category(Categories.Default)]
        public TextDecorationCollection SelectedTextDecorations
        {
            get { return (TextDecorationCollection)GetValue(SelectedTextDecorationsProperty); }
            set { SetValue(SelectedTextDecorationsProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="FontChooser"/> class.
        /// </summary>
        static FontChooser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontChooser), new FrameworkPropertyMetadata(typeof(FontChooser)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FontChooser"/> class.
        /// </summary>
        public FontChooser()
        {
            FontFamilies = Fonts.SystemFontFamilies;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // ----- Clean up.
            if (_fontFamilyListBox != null)
            {
                _fontFamilyTextBox.TextChanged -= OnFontFamilyTextBoxTextChanged;
                _fontFamilyTextBox.LostFocus -= OnFontFamilyTextBoxLostFocus;
                _fontFamilyListBox.SelectionChanged -= OnFontFamilyListBoxSelectionChanged;
                _fontSizeTextBox.TextChanged -= OnFontSizeTextBoxTextChanged;
                _fontSizeListBox.SelectionChanged -= OnFontSizeListBoxSelectionChanged;
                _typefaceListBox.SelectionChanged -= OnTypefaceListBoxSelectionChanged;
                _underlineCheckBox.Checked -= OnTextDecorationCheckBoxChanged;
                _underlineCheckBox.Unchecked -= OnTextDecorationCheckBoxChanged;
                _baselineCheckBox.Checked -= OnTextDecorationCheckBoxChanged;
                _baselineCheckBox.Unchecked -= OnTextDecorationCheckBoxChanged;
                _strikethroughCheckBox.Checked -= OnTextDecorationCheckBoxChanged;
                _strikethroughCheckBox.Unchecked -= OnTextDecorationCheckBoxChanged;
                _overlineCheckBox.Checked -= OnTextDecorationCheckBoxChanged;
                _overlineCheckBox.Unchecked -= OnTextDecorationCheckBoxChanged;

                _fontFamilyTextBox = null;
                _fontFamilyListBox = null;
                _fontSizeTextBox = null;
                _fontSizeListBox = null;
                _typefaceListBox = null;
                _underlineCheckBox = null;
                _baselineCheckBox = null;
                _strikethroughCheckBox = null;
                _overlineCheckBox = null;
            }

            base.OnApplyTemplate();

            // ----- Get template parts.
            _fontFamilyTextBox = GetTemplateChild("PART_FontFamilyTextBox") as TextBox ?? new TextBox();
            _fontFamilyListBox = GetTemplateChild("PART_FontFamilyListBox") as ListBox ?? new ListBox();
            _fontSizeTextBox = GetTemplateChild("PART_SizeTextBox") as TextBox ?? new TextBox();
            _fontSizeListBox = GetTemplateChild("PART_SizeListBox") as ListBox ?? new ListBox();
            _typefaceListBox = GetTemplateChild("PART_TypefaceListBox") as ListBox ?? new ListBox();
            _underlineCheckBox = GetTemplateChild("PART_UnderlineCheckBox") as CheckBox ?? new CheckBox();
            _baselineCheckBox = GetTemplateChild("PART_BaselineCheckBox") as CheckBox ?? new CheckBox();
            _strikethroughCheckBox = GetTemplateChild("PART_StrikethroughCheckBox") as CheckBox ?? new CheckBox();
            _overlineCheckBox = GetTemplateChild("PART_OverlineCheckBox") as CheckBox ?? new CheckBox();

            // ----- Register event handlers.
            _fontFamilyTextBox.TextChanged += OnFontFamilyTextBoxTextChanged;
            _fontFamilyTextBox.LostFocus += OnFontFamilyTextBoxLostFocus;
            _fontFamilyListBox.SelectionChanged += OnFontFamilyListBoxSelectionChanged;
            _fontSizeTextBox.TextChanged += OnFontSizeTextBoxTextChanged;
            _fontSizeListBox.SelectionChanged += OnFontSizeListBoxSelectionChanged;
            _typefaceListBox.SelectionChanged += OnTypefaceListBoxSelectionChanged;
            _underlineCheckBox.Checked += OnTextDecorationCheckBoxChanged;
            _underlineCheckBox.Unchecked += OnTextDecorationCheckBoxChanged;
            _baselineCheckBox.Checked += OnTextDecorationCheckBoxChanged;
            _baselineCheckBox.Unchecked += OnTextDecorationCheckBoxChanged;
            _strikethroughCheckBox.Checked += OnTextDecorationCheckBoxChanged;
            _strikethroughCheckBox.Unchecked += OnTextDecorationCheckBoxChanged;
            _overlineCheckBox.Checked += OnTextDecorationCheckBoxChanged;
            _overlineCheckBox.Unchecked += OnTextDecorationCheckBoxChanged;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // Call PropertyChanged handlers to initialize template parts.      
            OnFontFamiliesChanged();
            OnSelectedFontSizeChanged();
            OnSelectedTextDecorationsChanged();
        }


        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _backgroundWorker?.CancelAsync();
        }


        private void OnFontFamilyTextBoxTextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            // Only handle if changed was caused by user.
            if (_isInternalUpdate)
                return;

            // Find matching entry in the list.
            var items = (IEnumerable<FontFamilyDescription>)_fontFamilyListBox.ItemsSource;
            var matchingItem = items.FirstOrDefault(item => string.Compare(item.DisplayName, _fontFamilyTextBox.Text, true, CultureInfo.CurrentCulture) == 0);
            if (matchingItem != null)
            {
                SelectedFontFamily = matchingItem.FontFamily;
                return;
            }

            // No exact match found. See if the text is a matching prefix.
            matchingItem = items.FirstOrDefault(item => item.DisplayName.StartsWith(_fontFamilyTextBox.Text, true, null));
            if (matchingItem != null)
            {
                SelectedFontFamily = matchingItem.FontFamily;
                return;
            }

            // No prefix match found. Make fuzzy search.
            float bestMatchValue = -1;
            FontFamilyDescription bestMatchItem = null;
            foreach (var item in _fontFamilyListBox.Items.OfType<FontFamilyDescription>())
            {
                float match = StringHelper.ComputeMatch(_fontFamilyTextBox.Text, item.DisplayName);
                if (match > bestMatchValue)
                {
                    bestMatchValue = match;
                    bestMatchItem = item;
                }
            }

            if (bestMatchItem != null)
                SelectedFontFamily = bestMatchItem.FontFamily;
        }


        private void OnFontFamilyTextBoxLostFocus(object sender, RoutedEventArgs eventArgs)
        {
            _isInternalUpdate = true;

            // Copy correct text to text box.
            var selectedItem = _fontFamilyListBox.SelectedItem as FontFamilyDescription;
            if (selectedItem != null)
                _fontFamilyTextBox.Text = selectedItem.DisplayName;

            _isInternalUpdate = false;
        }


        private void OnFontFamilyListBoxSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            // Only handle if changed was caused by user.
            if (_isInternalUpdate)
                return;

            var selectedItem = _fontFamilyListBox.SelectedItem as FontFamilyDescription;
            if (selectedItem != null)
                SelectedFontFamily = selectedItem.FontFamily;
        }


        private void OnFontSizeTextBoxTextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            // Only handle if changed was caused by user.
            if (_isInternalUpdate)
                return;

            // Find matching entry in the list.
            double newSize;
            bool isValid = double.TryParse(_fontSizeTextBox.Text, out newSize);
            if (isValid)
            {
                SelectedFontSize = FontHelper.PointsToPixels(newSize);
                return;
            }
        }


        private void OnFontSizeListBoxSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            // Only handle if changed was caused by user.
            if (_isInternalUpdate)
                return;

            var selectedItem = _fontSizeListBox.SelectedItem;
            if (selectedItem != null)
                SelectedFontSize = FontHelper.PointsToPixels((double)selectedItem);
        }


        private void OnTypefaceListBoxSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            // Only handle if change was caused by user.
            if (_isInternalUpdate)
                return;

            var selectedItem = _typefaceListBox.SelectedItem as TypefaceDescription;
            if (selectedItem != null)
            {
                var typeface = selectedItem.Typeface;
                SelectedFontStretch = typeface.Stretch;
                SelectedFontStyle = typeface.Style;
                SelectedFontWeight = typeface.Weight;
            }
        }


        private void OnTextDecorationCheckBoxChanged(object sender, RoutedEventArgs eventArgs)
        {
            // Only handle if changed was caused by user.
            if (_isInternalUpdate)
                return;

            var textDecorations = new TextDecorationCollection();

            if (_underlineCheckBox.IsChecked.GetValueOrDefault())
                textDecorations.Add(TextDecorations.Underline);

            if (_baselineCheckBox.IsChecked.GetValueOrDefault())
                textDecorations.Add(TextDecorations.Baseline);

            if (_strikethroughCheckBox.IsChecked.GetValueOrDefault())
                textDecorations.Add(TextDecorations.Strikethrough);

            if (_overlineCheckBox.IsChecked.GetValueOrDefault())
                textDecorations.Add(TextDecorations.OverLine);

            textDecorations.Freeze();
            SelectedTextDecorations = textDecorations;
        }


        private void OnFontFamiliesChanged()
        {
            if (_fontFamilyListBox == null)
                return;

            // Fill the font family list box.
            if (FontFamilies == null)
            {
                _fontFamilyListBox.ItemsSource = null;
                return;
            }

            // Create a FontFamilyDescription for each font family.
            // Do it in the background because this can take a while.
            IsLoading = true;
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += CreateFontFamilyDescriptions;
            _backgroundWorker.ProgressChanged += OnWorkerProgressChanged;
            _backgroundWorker.RunWorkerCompleted += OnWorkerCompleted;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.RunWorkerAsync(FontFamilies);
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.PreviewKeyDown</strong> attached event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Key == Key.Escape && _backgroundWorker.IsBusy)
            {
                // Cancel loading of font families, if it is in progress.
                _backgroundWorker.CancelAsync();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }


        private void CreateFontFamilyDescriptions(object sender, DoWorkEventArgs eventArgs)
        {
            var fontFamilies = (ICollection<FontFamily>)eventArgs.Argument;
            double i = 0;
            int numberOfFontFamilies = fontFamilies.Count;

            var items = new List<FontFamilyDescription>(numberOfFontFamilies);
            foreach (var fontFamily in fontFamilies)
            {
                // Check whether user has canceled the loading process.
                if (_backgroundWorker.CancellationPending)
                {
                    eventArgs.Result = items;
                    return;
                }

                // Load font family.
                var description = new FontFamilyDescription
                {
                    DisplayName = FontHelper.GetDisplayName(fontFamily.FamilyNames),
                    FontFamily = fontFamily,
                    IsSymbolFont = FontHelper.IsSymbolFont(fontFamily),
                };
                items.Add(description);

                // Report progress
                i++;
                int progress = (int)(i / numberOfFontFamilies * 100);
                _backgroundWorker.ReportProgress(progress);
            }

            // Sort items by display name.
            items.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCulture));
            eventArgs.Result = items;
        }


        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs eventArgs)
        {
            // TODO: We could use a progress bar instead of the busy animation.
        }


        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs eventArgs)
        {
            if (eventArgs.Error == null)
                _fontFamilyListBox.ItemsSource = (IEnumerable)eventArgs.Result;

            IsLoading = false;
            OnSelectedFontFamilyChanged();
            OnSelectedTypefaceChanged();
        }


        private void OnSelectedFontFamilyChanged()
        {
            if (_fontFamilyListBox == null)
                return;

            _isInternalUpdate = true;

            // Select matching entry in list box.
            IEnumerable<FontFamilyDescription> fontFamilyItems = (IEnumerable<FontFamilyDescription>)_fontFamilyListBox.ItemsSource;
            var selectedItem = fontFamilyItems.FirstOrDefault(item => item.FontFamily.Equals(SelectedFontFamily));
            _fontFamilyListBox.SelectedItem = selectedItem;

            // Get display name.
            var displayName = (selectedItem != null) ? selectedItem.DisplayName : string.Empty;

            // Scroll list box entry into view.
            if (selectedItem != null)
                _fontFamilyListBox.ScrollIntoView(selectedItem);

            // If text box does not already contain a matching text, update the text box.
            // Do nothing if the cursor is currently in the text box (the user is typing).
            if (!_fontFamilyTextBox.IsKeyboardFocused && string.Compare(_fontFamilyTextBox.Text, displayName, true, CultureInfo.CurrentCulture) != 0)
                _fontFamilyTextBox.Text = displayName;

            // ----- Initialize typefaces list.
            // Fill the font family list box.
            if (SelectedFontFamily == null)
            {
                _typefaceListBox.ItemsSource = null;
            }
            else
            {
                var typefaces = SelectedFontFamily.GetTypefaces();

                // Create a text block for each font family
                var items = new List<TypefaceDescription>(typefaces.Count);
                foreach (var typeface in typefaces)
                {
                    var item = new TypefaceDescription
                    {
                        DisplayName = FontHelper.GetDisplayName(typeface.FaceNames),
                        FontFamily = SelectedFontFamily,
                        IsSymbolFont = FontHelper.IsSymbolFont(SelectedFontFamily),
                        Typeface = typeface,
                    };

                    items.Add(item);
                }

                // Sort items by display name and assign the to the list box.
                items.Sort((a, b) => FontHelper.Compare(a.Typeface, b.Typeface));
                _typefaceListBox.ItemsSource = items;

                // Update the typeface controls.
                OnSelectedTypefaceChanged();
            }

            _isInternalUpdate = false;
        }


        private void OnSelectedFontSizeChanged()
        {
            if (_fontSizeListBox == null)
                return;

            _isInternalUpdate = true;

            // Select matching entry in list box.
            var items = (IEnumerable<double>)_fontSizeListBox.ItemsSource ?? Enumerable.Empty<double>();

            double points = FontHelper.PixelsToPoints(SelectedFontSize);
            double selectedItem = items.FirstOrDefault(item => Numeric.AreEqual(item, points, 0.01f));

            if (selectedItem > 0)
            {
                _fontSizeListBox.SelectedItem = selectedItem;

                // Scroll list box entry into view.
                _fontSizeListBox.ScrollIntoView(_fontSizeListBox.SelectedItem);
            }
            else
            {
                _fontSizeListBox.SelectedItem = null;
            }

            // If text box does not already contain a matching text, update the text box.
            // Do nothing if the cursor is currently in the text box (the user is typing).
            if (!_fontSizeTextBox.IsKeyboardFocused && string.Compare(_fontSizeTextBox.Text, points.ToString(CultureInfo.CurrentCulture), true, CultureInfo.CurrentCulture) != 0)
                _fontSizeTextBox.Text = points.ToString(CultureInfo.CurrentCulture);

            _isInternalUpdate = false;
        }


        private void OnSelectedTypefaceChanged()
        {
            if (_typefaceListBox == null)
                return;

            _isInternalUpdate = true;

            var selectedTypeface = new Typeface(SelectedFontFamily, SelectedFontStyle, SelectedFontWeight, SelectedFontStretch);

            // Select matching entry in list box.
            var items = (IEnumerable<TypefaceDescription>)_typefaceListBox.ItemsSource ?? Enumerable.Empty<TypefaceDescription>();
            var selectedItem = items.FirstOrDefault(item => FontHelper.Compare(selectedTypeface, item.Typeface) == 0);
            if (selectedItem != null)
            {
                _typefaceListBox.SelectedItem = selectedItem;

                // Scroll list box entry into view.
                _typefaceListBox.ScrollIntoView(selectedItem);
            }

            _isInternalUpdate = false;
        }


        private void OnSelectedTextDecorationsChanged()
        {
            if (_underlineCheckBox == null)
                return;

            _isInternalUpdate = true;

            bool underline = false;
            bool baseline = false;
            bool strikethrough = false;
            bool overline = false;

            // Find out which text decoration is used.
            TextDecorationCollection textDecorations = SelectedTextDecorations;
            if (textDecorations != null)
            {
                foreach (TextDecoration textDecoration in textDecorations)
                {
                    switch (textDecoration.Location)
                    {
                        case TextDecorationLocation.Underline:
                            underline = true;
                            break;
                        case TextDecorationLocation.Baseline:
                            baseline = true;
                            break;
                        case TextDecorationLocation.Strikethrough:
                            strikethrough = true;
                            break;
                        case TextDecorationLocation.OverLine:
                            overline = true;
                            break;
                    }
                }
            }

            _underlineCheckBox.IsChecked = underline;
            _baselineCheckBox.IsChecked = baseline;
            _strikethroughCheckBox.IsChecked = strikethrough;
            _overlineCheckBox.IsChecked = overline;
            _isInternalUpdate = false;
        }
        #endregion
    }
}
