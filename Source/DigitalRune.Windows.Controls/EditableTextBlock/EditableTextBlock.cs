// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a <see cref="TextBlock"/> that can be edited.
    /// </summary>
    /// <remarks>
    /// By default, a <see cref="TextBlock"/> is shown. When the user clicks on the text the control
    /// switches to editing mode and renders a <see cref="TextBlock"/>.
    /// </remarks>
    [ContentProperty("Text")]
    [StyleTypedProperty(Property = "TextBlockStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "TextBoxStyle", StyleTargetType = typeof(TextBox))]
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    public class EditableTextBlock : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBlock _textBlock;
        private TextBoxAdorner _textBoxAdorner;
        private bool _canBeEdited;
        private bool _cancelEditing;
        private bool _mouseDownDetected;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEditable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
            "IsEditable",
            typeof(bool),
            typeof(EditableTextBlock),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether this text content can be edited.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this text block is editable; otherwise
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this text content can be edited.")]
        [Category(Categories.Default)]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="IsEditing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing",
            typeof(bool),
            typeof(EditableTextBlock),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsEditingChanged,
                CoerceIsEditing));

        /// <summary>
        /// Gets or sets a value indicating whether this control is in editing mode. 
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the control is in editing mode; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this control is in editing mode.")]
        [Category(Categories.Default)]
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(
            typeof(EditableTextBlock),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.Journal| FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the text contents of the <see cref="EditableTextBlock"/>. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The text contents.</value>
        [Description("Gets or sets the contents of this text block.")]
        [Category(Categories.Common)]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TextBlockStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextBlockStyleProperty = DependencyProperty.Register(
            "TextBlockStyle",
            typeof(Style),
            typeof(EditableTextBlock));

        /// <summary>
        /// Gets or sets the style of the <see cref="TextBox"/> (read-only mode). 
        /// This is a dependency property.
        /// </summary>
        /// <value>A <see cref="Style"/> with the target type <see cref="TextBox"/>.</value>
        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TextBoxStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextBoxStyleProperty = DependencyProperty.Register(
            "TextBoxStyle",
            typeof(Style),
            typeof(EditableTextBlock));

        /// <summary>
        /// Gets or sets the style of the <see cref="TextBox"/> (editing mode). 
        /// This is a dependency property.
        /// </summary>
        /// <value>A <see cref="Style"/> with the target type <see cref="TextBox"/>.</value>
        public Style TextBoxStyle
        {
            get { return (Style)GetValue(TextBoxStyleProperty); }
            set { SetValue(TextBoxStyleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="EditableTextBlock"/> class.
        /// </summary>
        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
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
            _textBlock = null;

            base.OnApplyTemplate();

            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
        }


        private static void OnIsEditingChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var editableTextBlock = (EditableTextBlock)dependencyObject;
            bool newValue = (bool)eventArgs.NewValue;

            if (newValue)
                editableTextBlock.ShowTextBox();
            else
                editableTextBlock.RemoveTextBox();
        }


        private static object CoerceIsEditing(DependencyObject dependencyObject, object baseValue)
        {
            var editableTextBlock = (EditableTextBlock)dependencyObject;

            // IsEditing can only be set to true when IsEditable is set.
            if (!editableTextBlock.IsEditable && (bool)baseValue)
                return false;

            return baseValue;
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseEnter"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (IsEditable && !IsEditing)
                _canBeEdited = true;
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseLeave"/> attached event is raised on 
        /// this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _canBeEdited = false;
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseLeftButtonDown"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _mouseDownDetected = true;
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseLeftButtonUp"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            OnMouseUp(e);

            if (!e.Handled && !IsEditing)
            {
                if (_mouseDownDetected)
                {
                    if (_canBeEdited)
                    {
                        // When _canBeEdited is set, we can switch to editing mode immediately.
                        IsEditing = true;
                    }

                    if (IsEditable)
                    {
                        // _canBeEdited is not set, but the IsEditable is set.
                        // Start editing on the next mouse click.
                        _canBeEdited = true;
                    }
                }

                _mouseDownDetected = false;
            }
        }


        /// <summary>
        /// Shows the <see cref="TextBox"/>.
        /// </summary>
        private void ShowTextBox()
        {
            _cancelEditing = false;

            _textBoxAdorner = new TextBoxAdorner(_textBlock);
            _textBoxAdorner.MaxWidth = MaxWidth;
            _textBoxAdorner.MaxHeight = MaxHeight;

            _textBoxAdorner.TextBox.Text = Text;
            _textBoxAdorner.TextBox.SelectAll();

            // Bind style of text box to TextBoxStyle
            Binding binding = new Binding("TextBoxStyle") { Source = this };
            _textBoxAdorner.TextBox.SetBinding(StyleProperty, binding);

            _textBoxAdorner.TextBox.KeyDown += OnTextBoxKeyDown;
            _textBoxAdorner.TextBox.LostKeyboardFocus += OnTextBoxLostKeyboardFocus;

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_textBlock);
            adornerLayer.Add(_textBoxAdorner);
        }


        /// <summary>
        /// Removes the <see cref="TextBox"/>.
        /// </summary>
        private void RemoveTextBox()
        {
            if (_textBoxAdorner != null)
            {
                if (!_cancelEditing)
                {
                    // Accept input
                    Text = _textBoxAdorner.TextBox.Text;
                }

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_textBlock);
                adornerLayer.Remove(_textBoxAdorner);
                _textBoxAdorner = null;
            }
        }


        private void OnTextBoxKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Enter)
            {
                // Accept input and switch back to read-only mode.
                _cancelEditing = false;
                IsEditing = false;
            }
            else if (eventArgs.Key == Key.Escape)
            {
                // Reject input and switch back to read-only mode.
                _cancelEditing = true;
                IsEditing = false;
            }
        }


        private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            // Switch back to read-only mode.
            IsEditing = false;
        }
        #endregion
    }
}
