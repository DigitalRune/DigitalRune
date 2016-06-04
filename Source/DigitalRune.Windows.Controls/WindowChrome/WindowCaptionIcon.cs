// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that shows the window icon in the title bar and shows the system menu
    /// when it is clicked.
    /// </summary>
    /// <remarks>
    /// Possible interactions with icon: left-click or right-click to open menu, double-click to 
    /// close window.
    /// </remarks>
    public class WindowCaptionIcon : ContentControl
    {
        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="WindowCaptionIcon"/> class.
        /// </summary>
        static WindowCaptionIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowCaptionIcon), new FrameworkPropertyMetadata(typeof(WindowCaptionIcon)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseDown"/> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. This event data
        /// reports details about the mouse button that was pressed and the handled state.
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseDown(e);

            if (e.Handled)
                return;

            var window = Window.GetWindow(this);
            if (e.ClickCount == 1)
            {
                Point position;
                if (e.ChangedButton == MouseButton.Left)
                {
                    // Open menu under icon.
                    position = new Point(0, ActualHeight);
                    position = PointToScreen(position);
                }
                else
                {
                    // Open menu at mouse position.
                    position = e.GetPosition(this);
                    position = PointToScreen(position);
                }

                SystemCommands.ShowSystemMenu(window, position);
                e.Handled = true;
            }
            else if (e.ClickCount > 1 && e.ChangedButton == MouseButton.Left)
            {
                SystemCommands.CloseWindow(window);
                e.Handled = true;
            }
        }
        #endregion
    }
}
