// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a <see cref="TextBox"/> with a watermark and a button.
    /// </summary>
    /// <remarks>
    /// This control combines a <see cref="WatermarkedTextBox"/> with a button. It can be used, 
    /// e.g., for a search box where the watermark is "Search" and the button is a button with a
    /// magnifying glass icon which triggers a search.
    /// </remarks>
    public class CommandTextBox : WatermarkedTextBox
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
            "ButtonContent",
            typeof(object),
            typeof(CommandTextBox),
            new FrameworkPropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the content of the button.
        /// This is a dependency property.
        /// </summary>
        /// <value>The content of the button.</value>
        [Description("Gets or sets the content of the button.")]
        [Category(Categories.Default)]
        public object ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(CommandTextBox),
            new FrameworkPropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets or sets the command that is invoked when the button is clicked.
        /// This is a dependency property.
        /// </summary>
        /// <value>The command that is invoked when the button is clicked.</value>
        [Description("Gets or sets the command that is invoked when the button is clicked.")]
        [Category(Categories.Action)]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="CommandTextBox"/> class.
        /// </summary>
        static CommandTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandTextBox), new FrameworkPropertyMetadata(typeof(CommandTextBox)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
