// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Shows a preview of the dragged <see cref="Data"/> (usually near the mouse position).
    /// </summary>
    /// <remarks>
    /// <see cref="Data"/> should be set to the object which should be shown near the mouse while
    /// dragging. <see cref="Data"/> is presented using the data template specified in 
    /// <see cref="DataTemplate"/>. <see cref="SingleChildAdorner.HorizontalOffset"/> and
    /// <see cref="SingleChildAdorner.VerticalOffset"/> must be set to position the preview.
    /// Both offsets can set in one call using (<see cref="SingleChildAdorner.SetOffsets"/>).
    /// </remarks>
    internal class DragAdorner : SingleChildAdorner
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the dragged data.
        /// </summary>
        /// <value>The dragged data.</value>
        public object Data
        {
            get { return ((ContentPresenter)Child).Content; }
            set { ((ContentPresenter)Child).Content = value; }
        }


        /// <summary>
        /// Gets or sets the data template.
        /// </summary>
        /// <value>The data template.</value>
        public DataTemplate DataTemplate 
        {
            get { return ((ContentPresenter)Child).ContentTemplate; }
            set { ((ContentPresenter)Child).ContentTemplate = value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DragAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The element to bind the adorner to.</param>
        public DragAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            Child = new ContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            IsHitTestVisible = false;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
