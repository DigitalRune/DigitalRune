// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents the parameter of the <see cref="DragDropBehavior.DragCommand"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DragCommandParameter"/> is passed as the parameter of the
    /// <see cref="ICommand.CanExecute"/> and <see cref="ICommand.Execute"/> methods.
    /// <see cref="ICommand.CanExecute"/> is called directly before <see cref="ICommand.Execute"/>.
    /// The same instance of the <see cref="DragCommandParameter"/> is passed to both methods.
    /// </para>
    /// </remarks>
    public class DragCommandParameter
    {
        /// <summary>
        /// Gets or sets the object associated with the <see cref="DragDropBehavior"/>.
        /// </summary>
        /// <value>
        /// The object associated with the <see cref="DragDropBehavior"/>.
        /// </value>
        public DependencyObject AssociatedObject { get; set; }


        /// <summary>
        /// Gets or sets the visual that was hit by the mouse when the mouse button was pressed.
        /// </summary>
        /// <value>The visual that was hit by the mouse when the mouse button was pressed.</value>
        public DependencyObject HitObject { get; set; }


        /// <summary>
        /// Gets or sets the mouse position  relative to <see cref="AssociatedObject"/>.
        /// </summary>
        /// <value>The mouse position relative to <see cref="AssociatedObject"/>.</value>
        public Point MousePosition { get; set; }


        /// <summary>
        /// Gets or sets an arbitrary object that can be used to store custom information.
        /// </summary>
        /// <value>The custom object. The default value is <see langword="null"/>.</value>
        public object Tag { get; set; }
    }
}
