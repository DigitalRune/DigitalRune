// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DigitalRune.Windows;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor.Options;
using ICSharpCode.AvalonEdit;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Represents an options page that allows the user to customize the text editor settings.
    /// </summary>
    internal class TextEditorOptionsPageViewModel : OptionsPageViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly TextExtension _textExtension;
        private readonly FontStretchConverter _fontStretchConverter;
        private readonly FontStyleConverter _fontStyleConverter;
        private readonly FontWeightConverter _fontWeightConverter;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the text editor options.
        /// </summary>
        /// <value>The text editor options.</value>
        public TextEditorOptions Options { get; }

        /// <summary>
        /// Gets or sets the font name of the current text font.
        /// </summary>
        /// <value>The font name of the current text font.</value>
        public string Font
        {
            get { return Invariant($"{Options.FontFamily}; {Options.FontSize * 72.0 / 96.0} pt"); }
        }


        /// <summary>
        /// Gets or sets the "Set Defaults" command.
        /// </summary>
        /// <value>The "Set Defaults" command.</value>
        public DelegateCommand SetDefaultsCommand { get; private set; }


        /// <summary>
        /// Gets or sets the "Select Font" command.
        /// </summary>
        /// <value>The "Select Font" command.</value>
        public DelegateCommand SelectFontCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditorOptionsPageViewModel"/> class. 
        /// (Do not use this constructor it is only for design-time support!)
        /// </summary>
        public TextEditorOptionsPageViewModel()
            : base("Text Editor")
        {
            Trace.Assert(WindowsHelper.IsInDesignMode, "This TextEditorOptionsPageViewModel constructor must not be used at runtime.");
            Options = new TextEditorOptions();
            _fontStyleConverter = new FontStyleConverter();
            _fontWeightConverter = new FontWeightConverter();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditorOptionsPageViewModel"/> class.
        /// </summary>
        /// <param name="textExtension">The <see cref="TextExtension"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textExtension"/> is <see langword="null"/>.
        /// </exception>
        public TextEditorOptionsPageViewModel(TextExtension textExtension)
            : base("Text Editor")
        {
            if (textExtension == null)
                throw new ArgumentNullException(nameof(textExtension));

            _textExtension = textExtension;
            Options = new TextEditorOptions();
            SetDefaultsCommand = new DelegateCommand(SetDefaults);
            SelectFontCommand = new DelegateCommand(SelectFont);

            _fontStretchConverter = new FontStretchConverter();
            _fontStyleConverter = new FontStyleConverter();
            _fontWeightConverter = new FontWeightConverter();
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
                Options.Set(_textExtension.Options);
                RaisePropertyChanged(() => Font);
            }

            base.OnActivated(eventArgs);
        }


        private void SetDefaults()
        {
            Options.Set(new TextEditorOptions());
            RaisePropertyChanged(() => Font);
        }


        private void SelectFont()
        {
            var fontDialog = new FontDialog();

            if (!string.IsNullOrEmpty(Options.FontFamily))
                fontDialog.Chooser.SelectedFontFamily = new FontFamily(Options.FontFamily);
            if (Options.FontSize > 0)
                fontDialog.Chooser.SelectedFontSize = Options.FontSize;

            // ReSharper disable PossibleNullReferenceException
            if (!string.IsNullOrEmpty(Options.FontStretch))
                fontDialog.Chooser.SelectedFontStretch = (FontStretch)_fontStretchConverter.ConvertFrom(Options.FontStretch);
            if (!string.IsNullOrEmpty(Options.FontStyle))
                fontDialog.Chooser.SelectedFontStyle = (FontStyle)_fontStyleConverter.ConvertFrom(Options.FontStyle);
            if (!string.IsNullOrEmpty(Options.FontWeight))
                fontDialog.Chooser.SelectedFontWeight = (FontWeight)_fontWeightConverter.ConvertFrom(Options.FontWeight);
            // ReSharper restore PossibleNullReferenceException

            var result = fontDialog.ShowDialog();
            if (result.GetValueOrDefault())
            {
                Options.FontFamily = fontDialog.Chooser.SelectedFontFamily.ToString();
                Options.FontSize = fontDialog.Chooser.SelectedFontSize;
                Options.FontStyle = fontDialog.Chooser.SelectedFontStyle.ToString();
                Options.FontStretch = fontDialog.Chooser.SelectedFontStretch.ToString();
                Options.FontWeight = fontDialog.Chooser.SelectedFontWeight.ToString();
                RaisePropertyChanged(() => Font);
            }
        }


        /// <inheritdoc/>
        protected override void OnApply()
        {
            Logger.Debug("Applying changes in Text Editor Options page.");

            // Update global text editor options.
            _textExtension.Options.Set(Options);
        }
        #endregion
    }
}
