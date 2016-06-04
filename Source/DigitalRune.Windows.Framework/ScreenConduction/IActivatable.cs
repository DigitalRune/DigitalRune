// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents the activation logic of an item.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The methods <see cref="OnActivate"/> and <see cref="OnDeactivate"/> must only be called by 
    /// an <see cref="IConductor"/>.
    /// </para>
    /// <para>
    /// Unlike <see cref="IConductor.ActivateItemAsync"/> and 
    /// <see cref="IConductor.DeactivateItemAsync"/> of the <see cref="IConductor"/>, the 
    /// <see cref="OnActivate"/> and <see cref="OnDeactivate"/> methods of the 
    /// <see cref="IActivatable"/> are "final" and cannot fail. The <see cref="IActivatable"/> does
    /// not check <see cref="IGuardClose.CanCloseAsync"/> (interface <see cref="IGuardClose"/>).
    /// In contrast, the <see cref="IConductor"/> methods return a Boolean value because the 
    /// activation/deactivation can fail, e.g. if a close operation is canceled using the 
    /// <see cref="IGuardClose"/> mechanism.
    /// </para>
    /// <para>
    /// Redundant calls to <see cref="OnActivate"/> and <see cref="OnDeactivate"/> are allowed.
    /// This means, it is allowed to call <see cref="OnActivate"/> for an already activated item and 
    /// <see cref="OnDeactivate"/> for an already deactivated item. Usually, the 
    /// <see cref="Activated"/> and <see cref="Deactivated"/> events are only raised when the 
    /// <see cref="IsActive"/> was actually changed. The exception is: When an already deactivated 
    /// item is closed, the <see cref="Deactivated"/> event is raised again. This time with the 
    /// <see cref="DeactivationEventArgs.Closed"/> flag set in the 
    /// <see cref="DeactivationEventArgs"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Activatable")]
    public interface IActivatable
    {
        /// <summary>
        /// Gets a value indicating whether this item is open.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is open; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// An item is opened when it is activated for the first time. A value of
        /// <see langword="true"/> indicates that the item is either active or temporarily
        /// deactivated. A value of <see langword="false"/> indicates that the item has never been
        /// active or has been closed.
        /// </para>
        /// <para>
        /// If the item is open, then there is a WPF element which represents this item. The WPF
        /// element can be visible or temporarily hidden.
        /// </para>
        /// </remarks>
        bool IsOpen { get; }


        /// <summary>
        /// Gets a value indicating whether the item is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the item is active; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// An item is active when it is visible on screen. However, it is not guaranteed that the
        /// item has the focus.
        /// </remarks>
        bool IsActive { get; }


        /// <summary>
        /// Occurs when the item is opened or activated.
        /// </summary>
        event EventHandler<ActivationEventArgs> Activated;


        /// <summary>
        /// Occurs when the item is deactivated or closed.
        /// </summary>
        event EventHandler<DeactivationEventArgs> Deactivated;


        /// <summary>
        /// Called by the <see cref="IConductor"/> when the item is activated.
        /// </summary>
        void OnActivate();


        /// <summary>
        /// Called by the <see cref="IConductor"/> when the item is deactivated or closed.
        /// </summary>
        /// <param name="close">
        /// <see langword="true"/> if the item is being closed; <see langword="false"/> if the item
        /// is only deactivated temporarily.
        /// </param>
        void OnDeactivate(bool close);
    }


    /// <summary>
    /// Provides the arguments for the <see cref="IActivatable.Activated"/> event.
    /// </summary>
    public class ActivationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the sender was opened in addition to being activated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the sender was opened; otherwise, <see langword="false"/>.
        /// </value>
        public bool Opened { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ActivationEventArgs"/> class.
        /// </summary>
        /// <param name="opened">
        /// <see langword="true"/> if the sender was opened; otherwise, <see langword="false"/>.
        /// </param>
        public ActivationEventArgs(bool opened)
        {
            Opened = opened;
        }
    }


    /// <summary>
    /// Provides the arguments for the <see cref="IActivatable.Deactivated"/> event.
    /// </summary>
    public class DeactivationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether sender is going to be closed in addition to being 
        /// deactivated.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the sender is closed; otherwise, <see langword="false"/>.
        /// </value>
        public bool Closed { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DeactivationEventArgs"/> class.
        /// </summary>
        /// <param name="closed">
        /// <see langword="true"/> if the sender is closed; otherwise, <see langword="false"/>.
        /// </param>
        public DeactivationEventArgs(bool closed)
        {
            Closed = closed;
        }
    }
}
