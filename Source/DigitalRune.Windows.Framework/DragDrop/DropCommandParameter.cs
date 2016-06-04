// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents the parameter of the <see cref="DragDropBehavior.DropCommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DropCommandParameter"/> is passed as the parameter of the
    /// <see cref="ICommand.CanExecute"/> and <see cref="ICommand.Execute"/> methods.
    /// <see cref="ICommand.CanExecute"/> is called directly before <see cref="ICommand.Execute"/>.
    /// The same instance of the <see cref="DropCommandParameter"/> is passed to both methods.
    /// </para>
    /// <para>
    /// This object contains the <see cref="DragEventArgs"/> plus additional information.
    /// </para>
    /// <para>
    /// <see cref="Data"/> represents the actual data that is being dragged. This property is not
    /// set by the <see cref="DragDropBehavior"/>. An object that implements the
    /// <see cref="DragDropBehavior.DropCommand"/> needs to extract the data from the
    /// <see cref="DataObject"/> and set this property in <see cref="ICommand.CanExecute"/>.
    /// <see cref="Data"/> must be set if a representation should be shown near the mouse while
    /// dragging the data.
    /// </para>
    /// </remarks>
    public class DropCommandParameter
    {
        /// <summary>
        /// Gets or sets the drag event arguments.
        /// </summary>
        /// <value>The drag event arguments.</value>
        public DragEventArgs DragEventArgs { get; set; }


        /// <summary>
        /// Gets the object associated with the <see cref="DragDropBehavior"/>.
        /// </summary>
        /// <value>
        /// The object associated with the <see cref="DragDropBehavior"/>.
        /// </value>
        public DependencyObject AssociatedObject
        {
            get { return (DependencyObject)DragEventArgs.Source; }
        }


        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information.
        /// </summary>
        /// <value>The custom object. The default value is <see langword="null"/>.</value>
        public object Tag { get; set; }


        /// <summary>
        /// Gets or sets the data being dragged.
        /// </summary>
        /// <value>The data being dragged.</value>
        /// <remarks>
        /// <para>
        /// This object represents the data that is being dragged. An object that implements the
        /// <see cref="DragDropBehavior.DropCommand"/> needs to extract the data from the
        /// <see cref="DataObject"/> and set this property in <see cref="ICommand.CanExecute"/>.
        /// <see cref="Data"/> must be set if a representation should be shown near the mouse while
        /// dragging the data.
        /// </para>
        /// </remarks>
        public object Data { get; set; }


        /// <summary>
        /// Gets or sets the index at which the data is to be inserted.
        /// </summary>
        /// <value>The index at which the data is to be inserted.</value>
        /// <remarks>
        /// The <see cref="TargetIndex"/> is set if the data should be dropped into an
        /// <see cref="ItemsControl"/>. The <see cref="TargetIndex"/> indicates the new position in
        /// the items collection. A value of -1 indicates that the data should be dropped onto an
        /// existing item.
        /// </remarks>
        public int TargetIndex { get; set; }
    }
}
