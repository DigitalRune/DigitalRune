// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Provides arguments for the <see cref="ChartSelectionBehavior.SelectionRectangle"/> event.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ChartSelectionRectangleEventArgs : EventArgs
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the left edge of the selection relative to the <see cref="ChartPanel"/>.
        /// </summary>
        /// <value>The left edge of the selection relative to the <see cref="ChartPanel"/>.</value>
        public double Left { get; private set; }


        /// <summary>
        /// Gets the top edge of the selection relative to the <see cref="ChartPanel"/>.
        /// </summary>
        /// <value>The top edge of the selection relative to the <see cref="ChartPanel"/>.</value>
        public double Top { get; private set; }


        /// <summary>
        /// Gets the right edge of the selection relative to the <see cref="ChartPanel"/>.
        /// </summary>
        /// <value>The right edge of the selection relative to the <see cref="ChartPanel"/>.</value>
        public double Right { get; private set; }


        /// <summary>
        /// Gets the bottom edge of the selection relative to the <see cref="ChartPanel"/>.
        /// </summary>
        /// <value>The bottom edge of the selection relative to the <see cref="ChartPanel"/>.</value>
        public double Bottom { get; private set; }


        /// <summary>
        /// Gets or sets the modifier keys that were pressed when the selection rectangle was committed.
        /// </summary>
        /// <value>The modifier keys that were pressed when the selection rectangle was committed.</value>
        public ModifierKeys ModifierKeys { get; private set; }


        /// <summary>
        /// Gets or sets a value indicating whether the
        /// <see cref="ChartSelectionBehavior.SelectionRectangle"/> has been handled.
        /// </summary>
        /// <value><see langword="true"/> if handled; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// When multiple event handlers are connected to the
        /// <see cref="ChartSelectionBehavior.SelectionRectangle"/> event an event handler can set
        /// <see cref="Handled"/> to <see langword="true"/> to indicate that no further processing
        /// is necessary. Event handler should first check whether <see cref="Handled"/> is set and
        /// only execute when it is <see langword="false"/>.
        /// </remarks>
        public bool Handled { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSelectionRectangleEventArgs"/> class.
        /// </summary>
        /// <param name="left">The left edge of the selection rectangle.</param>
        /// <param name="top">The top edge of the selection rectangle.</param>
        /// <param name="right">The right edge of the selection rectangle.</param>
        /// <param name="bottom">The bottom edge of the selection rectangle.</param>
        /// <param name="modifierKeys">The modifier keys that were pressed.</param>
        /// <remarks>
        /// <paramref name="left"/>, <paramref name="right"/>, <paramref name="top"/> and
        /// <paramref name="bottom"/> are automatically sorted such that <paramref name="left"/>
        /// &lt; <paramref name="right"/> and <paramref name="top"/> &lt; <paramref name="bottom"/>.
        /// </remarks>
        public ChartSelectionRectangleEventArgs(double left, double top, double right, double bottom, ModifierKeys modifierKeys)
        {
            if (left > right)
                ChartHelper.Swap(ref left, ref right);

            if (top > bottom)
                ChartHelper.Swap(ref top, ref bottom);

            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
            ModifierKeys = modifierKeys;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
