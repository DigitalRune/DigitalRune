// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Provides arguments for the <see cref="ChartSelectionBehavior.SelectionChanged"/> event.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ChartSelectionChangedEventArgs : EventArgs
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="Element"/> to be selected or unselected.
        /// </summary>
        /// <value>The <see cref="Element"/> to be selected or unselected.</value>
        public UIElement Element { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Element"/> is to be selected or
        /// unselected.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Element"/> is going to be selected;
        /// <see langword="false"/> if it is going to be unselected.
        /// </value>
        public bool Select { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the
        /// <see cref="ChartSelectionBehavior.SelectionChanged"/> has been handled.
        /// </summary>
        /// <value><see langword="true"/> if handled; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// <para>
        /// When multiple event handlers are connected to the
        /// <see cref="ChartSelectionBehavior.SelectionChanged"/> event an event handler can set
        /// <see cref="Handled"/> to <see langword="true"/> to indicate that no further processing
        /// is necessary. Event handler should first check whether <see cref="Handled"/> is set and
        /// only execute when it is <see langword="false"/>.
        /// </para>
        /// <para>
        /// Important: If this flag is set by an event handler, the
        /// <see cref="ChartSelectionBehavior"/> does not further process the selection and it does
        /// not set or unset the IsSelected attached property on the <see cref="Element"/>! That
        /// means: If this flag is set, the <see cref="ChartSelectionBehavior"/> assumes that the
        /// selection action should be canceled, or that the event handler has taken care of
        /// changing the IsSelected attached property.
        /// </para>
        /// </remarks>
        public bool Handled { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSelectionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="element">The element that is going to be selected or unselected.</param>
        /// <param name="select">
        /// <see langword="true"/> if the <see cref="Element"/> is going to be selected; 
        /// <see langword="false"/> if it is going to be unselected.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public ChartSelectionChangedEventArgs(UIElement element, bool select)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            Element = element;
            Select = select;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
