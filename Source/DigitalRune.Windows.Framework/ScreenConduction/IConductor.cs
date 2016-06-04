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
using System.Collections.Generic;
using System.Threading.Tasks;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Controls the life cycle of one or more items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Items ("screens") can be ordinary view models (plain CLR objects). It is not required that
    /// items are derived from a certain type or that they implement a certain interface. However,
    /// certain interfaces can be used to define aspects of a screen. A conductor is aware of the
    /// following interfaces:
    /// <list type="bullet">
    /// <listheader>
    /// <term>Interface</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="IScreen"/></term>
    /// <description>
    /// The item is aware of its conductor. The conductor automatically sets the property
    /// <see cref="IScreen.Conductor"/> of the item.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="IActivatable"/></term>
    /// <description>
    /// The item has a custom activation logic. The conductor automatically handles the activation
    /// of the item.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="IGuardClose"/></term>
    /// <description>
    /// The item may prevent closing. The conductor will call <see cref="IGuardClose.CanCloseAsync"/>
    /// before closing the item.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface IConductor
    {
        // Notes:
        // - DeactivateItem must be async because it usually has to call CanCloseAsync. CanClose
        //   is async because on some platforms (e.g. Windows Phone) a non-modal dialog is shown.
        // - ActivateItem is async because this method might have to close the previous active item
        //   and call CanCloseAsync.


        /// <summary>
        /// Gets the items that are controlled by this conductor.
        /// </summary>
        /// <value>
        /// A collection of items that are conducted by this conductor.
        /// </value>
        IEnumerable<object> Items { get; }


        /// <summary>
        /// Gets the currently active items.
        /// </summary>
        /// <value>A collection of items that are currently active.</value>
        IEnumerable<object> ActiveItems { get; }


        /// <summary>
        /// Asynchronously activates the specified item.
        /// </summary>
        /// <param name="item">
        /// The item to activate. (Whether this parameter can be <see langword="null"/> depends on
        /// the type of conductor.)
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the item was activated successfully.
        /// </returns>
        Task<bool> ActivateItemAsync(object item);


        /// <summary>
        /// Asynchronously deactivates the specified item.
        /// </summary>
        /// <param name="item">The item to deactivate. (Must not be <see langword="null"/>.)</param>
        /// <param name="close">
        /// <see langword="true"/> to close the item; <see langword="false"/> to deactivate the item
        /// temporarily.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task indicates
        /// whether the item was deactivated/closed successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is not controlled by this conductor.
        /// </exception>
        Task<bool> DeactivateItemAsync(object item, bool close);
    }
}
