// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a text block which displays a trimmed file path, e.g. "C:\...\Abc\Foo.txt".
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a <see cref="Command"/> is set, a <see cref="Hyperlink"/> is created.
    /// </para>
    /// </remarks>
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    public class PathTextBlock : Control, ICommandSource
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Typeface _typeface;
        private TextBlock _textBlock;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(PathTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the file path.
        /// This is a dependency property.
        /// </summary>
        /// <value>The file path.</value>
        [Description("Gets or sets The file path which is trimmed and displayed as the text.")]
        [Category(Categories.Default)]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(PathTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the command that will be executed when the command source is invoked.
        /// This is a dependency property.
        /// </summary>
        /// <value>The command that will be executed when the command source is invoked.</value>
        [Description("Gets or sets the command that will be executed when the command source is invoked.")]
        [Category(Categories.Action)]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(PathTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the a user defined data value that can be passed to the command when it is
        /// executed. This is a dependency property.
        /// </summary>
        /// <value>
        /// The a user defined data value that can be passed to the command when it is executed.
        /// </value>
        [Description("Gets or sets the a user defined data value that can be passed to the command when it is executed.")]
        [Category(Categories.Action)]
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CommandTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(PathTextBlock),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the element that the command is executed on.
        /// This is a dependency property.
        /// </summary>
        /// <value>The element that the command is executed on.</value>
        [Description("Gets or sets the element that the command is executed on.")]
        [Category(Categories.Action)]
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="PathTextBlock"/> class.
        /// </summary>
        static PathTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathTextBlock), new FrameworkPropertyMetadata(typeof(PathTextBlock)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PathTextBlock"/> class.
        /// </summary>
        public PathTextBlock()
        {
            AddVisualChild(_textBlock);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
        }


        /// <inheritdoc/>
        protected override Size MeasureOverride(Size constraint)
        {
            if (_typeface == null
                || !Equals(_typeface.FontFamily, FontFamily)
                || _typeface.Style != FontStyle
                || _typeface.Weight != FontWeight
                || _typeface.Stretch != FontStretch)
            {
                _typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            }

            var trimmedText = TrimText(Text, constraint.Width);
            SetText(trimmedText);

            base.MeasureOverride(constraint);

            return MeasureText(Text);
        }


        private void SetText(string text)
        {
            if (_textBlock == null)
                return;

            if (Command != null)
            {
                if (!(_textBlock.Inlines.FirstInline is Hyperlink))
                {
                    // Turn text block into a hyperlink.
                    _textBlock.Inlines.Clear();
                    _textBlock.Inlines.Add(new Hyperlink(new Run()));
                }

                // Update hyperlink properties.
                var hyperlink = (Hyperlink)_textBlock.Inlines.FirstInline;
                hyperlink.Command = Command;
                hyperlink.CommandParameter = CommandParameter;
                hyperlink.CommandTarget = CommandTarget;

                var run = (Run)hyperlink.Inlines.FirstInline;
                run.Text = text;
            }
            else
            {
                // No hyperlink. Setting Text property replaces any hyperlinks in the Inlines.
                _textBlock.Text = text;
            }
        }


        private Size MeasureText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new Size(0, 0);

            var formattedText = new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    _typeface,
                    FontSize,
                    Foreground);

            return new Size(formattedText.Width, formattedText.Height);
        }


        private string TrimText(string text, double width)
        {
            if (text == null)
                return string.Empty;

            if (MeasureText(text).Width < width)
                return text;

            if (text.Contains("\\"))
            {
                // Treat like a file path and split at "\\".
                string root;
                string filename;
                string directory;
                try
                {
                    root = Path.GetPathRoot(text);

                    // File share paths like "\\Demon\share" do not end with a backslash.
                    if (!string.IsNullOrEmpty(root) && root[root.Length - 1] != Path.DirectorySeparatorChar)
                        root = root + '\\';

                    filename = Path.GetFileName(text);
                    directory = Storages.Path.GetRelativePath(root, Path.GetDirectoryName(text));
                }
                catch (Exception)
                {
                    return text;
                }

                if (string.IsNullOrEmpty(directory))
                    return text;

                var separators = new[] { '\\' };
                var directories = directory.Split(separators, 2);
                while (directories.Length > 1)
                {
                    // Skip the first directory.
                    directory = directories[1];

                    text = root + "...\\" + directory + "\\" + filename;

                    if (MeasureText(text).Width < width)
                        return text;

                    directories = directory.Split(separators, 2);
                }

                return root + "...\\" + filename;
            }
            else
            {
                // Treat like type name, like "DigitalRune.Geometry.Pose", and split at '.'.
                var separators = new[] { '.' };
                var parts = text.Split(separators, 2);

                if (parts.Length == 1)
                    return text;

                while (parts.Length > 1)
                {
                    // Skip the first directory.
                    var trimmedText = "..." + parts[1];

                    if (MeasureText(trimmedText).Width < width)
                        return trimmedText;

                    parts = parts[1].Split(separators, 2);
                }

                return "..." + parts[0];
            }
        }
        #endregion
    }
}
