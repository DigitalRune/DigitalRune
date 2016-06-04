// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a combination of a button and a toggle button, which opens a drop-down.
    /// </summary>
    [TemplatePart(Name = PART_ActionButton, Type = typeof(Button))]
    public class SplitButton : DropDownButton
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string PART_ActionButton = nameof(PART_ActionButton);
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="SplitButton"/> class.
        /// </summary>
        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
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
            Button = null;

            base.OnApplyTemplate();

            Button = GetTemplateChild(PART_ActionButton) as Button;
        }
        #endregion
    }


    /// <summary>
    /// Represents a combination of a button and a toggle button, which opens a drop-down. (Same as
    /// <see cref="SplitButton"/>, but different style.)
    /// </summary>
    /// <inheritdoc/>
    [TemplatePart(Name = PART_ActionButton, Type = typeof(Button))]
    public class ToolBarSplitButton : SplitButton
    {
        /// <summary>
        /// Initializes static members of the <see cref="ToolBarSplitButton"/> class.
        /// </summary>
        static ToolBarSplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBarSplitButton), new FrameworkPropertyMetadata(typeof(ToolBarSplitButton)));
        }
    }
}
