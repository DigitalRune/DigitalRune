// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a <see cref="TextBox"/>, which shows a watermark (a custom object, usually a
    /// short text) when empty.
    /// </summary>
    /// <remarks>
    /// This controls is a text box. Without user input, the watermark is shown. This is
    /// usually a short, grayed-out text which tells you what you should enter (e.g. "First name").
    /// </remarks>
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ScrollViewer))]
    public class WatermarkedTextBox : TextBox
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ScrollViewer _contentHost;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ClearOnEscape"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClearOnEscapeProperty = DependencyProperty.Register(
            "ClearOnEscape",
            typeof(bool),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets or sets a value indicating whether to clear the content of the text box when ESC is
        /// pressed. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to select the whole content when focused; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>
        /// </value>
        [Description("Gets or sets a value indicating whether to clear the content when ESC is pressed.")]
        [Category(Categories.Behavior)]
        public bool ClearOnEscape
        {
            get { return (bool)GetValue(ClearOnEscapeProperty); }
            set { SetValue(ClearOnEscapeProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectAllOnFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.Register(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets or sets a value indicating whether to select the whole content when the text box
        /// receives the focus. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to select the whole content when focused; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>
        /// </value>
        [Description("Gets or sets a value indicating whether to select the content when the text box receives the focus.")]
        [Category(Categories.Behavior)]
        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty); }
            set { SetValue(SelectAllOnFocusProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Watermark"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark",
            typeof(object),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the watermark (a custom object, usually a short text).
        /// This is a dependency property.
        /// </summary>
        /// <value>The watermark (a custom object, usually a short text).</value>
        [Description("Gets or sets the watermark (a custom object, usually a short text).")]
        [Category(Categories.Default)]
        public object Watermark
        {
            get { return GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="WatermarkTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register(
            "WatermarkTemplate",
            typeof(DataTemplate),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the template used for the watermark.
        /// This is a dependency property.
        /// </summary>
        /// <value>The data template for the watermark.</value>
        [Description("Gets or sets the template used for the watermark.")]
        [Category(Categories.Default)]
        public DataTemplate WatermarkTemplate
        {
            get { return (DataTemplate)GetValue(WatermarkTemplateProperty); }
            set { SetValue(WatermarkTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="WatermarkTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkTemplateSelectorProperty = DependencyProperty.Register(
            "WatermarkTemplateSelector",
            typeof(DataTemplateSelector),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data template selector for the watermark.
        /// This is a dependency property.
        /// </summary>
        /// <value>The data template selector for the watermark.</value>
        [Description("Gets or sets the data template selector for the watermark.")]
        [Category(Categories.Default)]
        public DataTemplateSelector WatermarkTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(WatermarkTemplateSelectorProperty); }
            set { SetValue(WatermarkTemplateSelectorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="WatermarkStringFormat"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkStringFormatProperty = DependencyProperty.Register(
            "WatermarkStringFormat",
            typeof(string),
            typeof(WatermarkedTextBox),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the format used to display the watermark.
        /// This is a dependency property.
        /// </summary>
        /// <value>The format used to display the watermark.</value>
        [Description("Gets or sets the format used to display the watermark.")]
        [Category(Categories.Default)]
        public string WatermarkStringFormat
        {
            get { return (string)GetValue(WatermarkStringFormatProperty); }
            set { SetValue(WatermarkStringFormatProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="WatermarkedTextBox"/> class.
        /// </summary>
        static WatermarkedTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WatermarkedTextBox), new FrameworkPropertyMetadata(typeof(WatermarkedTextBox)));
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
            base.OnApplyTemplate();

            _contentHost = GetTemplateChild("PART_ContentHost") as ScrollViewer;
        }


        /// <summary>
        /// Invoked whenever an unhandled <strong>Keyboard.GotKeyboardFocus</strong> attached routed
        /// event reaches an element derived from this class in its route. Implement this method to
        /// add class handling for this event.
        /// </summary>
        /// <param name="e">Provides data about the event.</param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            // Select all when user moves focus to TextBox (e.g. by keyboard navigation).
            if (SelectAllOnFocus)
                SelectAll();
        }


        /// <summary>
        /// Invoked whenever an unhandled <strong>Keyboard.KeyDown</strong> attached routed event
        /// reaches an element derived from this class in its route. Implement this method to add
        /// class handling for this event.
        /// </summary>
        /// <param name="e">Provides data about the event.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Key == Key.Escape && ClearOnEscape)
            {
                Clear();
                e.Handled = true;
                return;
            }

            base.OnKeyDown(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.PreviewMouseLeftButtonDown"/> routed
        /// event reaches an element in its route that is derived from this class. Implement this
        /// method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that the left mouse button was pressed.
        /// </param>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (SelectAllOnFocus && !IsKeyboardFocused)
            {
                // Intercept mouse event to prevent TextBox from setting the caret and
                // thereby clearing the selection.
                Focus();

                // Set the routed event to handled, if the TextBoxView was hit. Otherwise, let
                // the event tunnel through. (Maybe a scroll button was clicked.)
                if (IsTextBoxViewHit(e))
                    e.Handled = true;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }


        // Returns true if the TextBoxView was hit.
        private bool IsTextBoxViewHit(MouseButtonEventArgs eventArgs)
        {
            var source = eventArgs.OriginalSource as DependencyObject;
            if (source == null || _contentHost == null)
                return false;

            // The visual tree of a TextBox looks like this:
            // TextBox
            //   ...
            //     ScrollViewer "PART_ContentHost"
            //       Grid
            //         ScrollContentPresenter
            //           TextBoxView
            //         ScrollBar
            //         ScrollBar
            var scrollContentPresenter = _contentHost.GetVisualDescendants()
                                                     .OfType<ScrollContentPresenter>()
                                                     .FirstOrDefault();

            // ReSharper disable once PossibleUnintendedReferenceComparison
            return source.GetVisualParent() == scrollContentPresenter;
        }
        #endregion
    }
}
