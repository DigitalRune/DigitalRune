// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents an item which is controlled by a <see cref="IConductor"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A screen is UI part (a window, a modal dialog, a page in a browser, a document view, etc.)
    /// of an application. A screen can have an activation-deactivation life cycle which is enforced
    /// by a screen conductor. The screen conductor activates the item when it is shown and
    /// deactivates the item when it is hidden or removed. In an MVVM architecture a screen is
    /// usually a view model. The screen conductor can be a parent view model, a view, a control, or
    /// another object.
    /// </para>
    /// <para>
    /// The interface <see cref="IScreen"/> indicates that an item is a screen which is aware of its
    /// screen conductor (see property <see cref="Conductor"/>).
    /// </para>
    /// <para>
    /// The interface <see cref="IActivatable"/> represent the activation logic of an item.
    /// </para>
    /// <para>
    /// The interface <see cref="IGuardClose"/> represents an item which may prevent closing. For
    /// example, a document which shows a "Save changes?" dialog when the content is modified.
    /// </para>
    /// </remarks>
    public interface IScreen
    {
        /// <summary>
        /// Gets or sets the conductor that controls the life cycle of this item.
        /// </summary>
        /// <value>
        /// The <see cref="IConductor"/> that controls the life cycle of the item. Can be 
        /// <see langword="null"/>.
        /// </value>
        IConductor Conductor { get; set; }
    }
}
