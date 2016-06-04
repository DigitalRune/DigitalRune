// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a selectable, draggable item in the docking layout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="IDockTabItem"/> represents (or contains) a "dockable window" that can be
    /// docked somewhere in the <see cref="IDockControl"/> or a <see cref="IFloatWindow"/>. (The
    /// "parent" in the docking layout is always a <see cref="IDockTabPane"/>.) 
    /// </para>
    /// </remarks>
    public interface IDockTabItem : IDockElement
    {
        /// <summary>
        /// Gets or sets the last known state in the docking layout.
        /// </summary>
        /// <value>The last known state in the docking layout.</value>
        /// <remarks>
        /// <para>
        /// <see cref="DockState"/> and <see cref="LastDockState"/> are usually identical - except
        /// when <see cref="DockState"/> is <see cref="Docking.DockState.Hide"/>, then 
        /// <see cref="LastDockState"/> represents state of the item before the item was 
        /// hidden.
        /// </para>
        /// <para>
        /// This property is set automatically by the <see cref="DockStrategy"/>.
        /// </para>
        /// </remarks>
        DockState LastDockState { get; set; }


        /// <summary>
        /// Gets a value indicating whether this item remains in the docking layout even when
        /// hidden.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item remains in the docking layout; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// "Persistent" means that the item is not removed from the docking layout when it is
        /// closed. Instead it is only hidden. When the item is shown again it is docked in the last
        /// position.
        /// </para>
        /// <para>
        /// Typically "tool windows" are persistent items (e.g. a "Solution Explorer" or a
        /// "Properties Window"). These kind of windows should always reappear at the same position.
        /// In contrast, "document windows" are usually not persistent: If a document window is
        /// opened, it is always treated as a new window that is docked at the default dock
        /// position.
        /// </para>
        /// </remarks>
        bool IsPersistent { get; }


        /// <summary>
        /// Gets or sets the time when the item was activated.
        /// </summary>
        /// <value>The time when the item was activated.</value>
        /// <remarks>
        /// This property is set automatically by the <see cref="DockStrategy"/>.
        /// </remarks>
        DateTime LastActivation { get; set; }


        /// <summary>
        /// Gets or sets the width of the pane in the auto-hide state.
        /// </summary>
        /// <value>The width of pane in the auto-hide state.</value>
        /// <remarks>
        /// This property is only used if the item is in <see cref="IDockControl.AutoHideLeft"/> or
        /// <see cref="IDockControl.AutoHideRight"/>.
        /// </remarks>
        double AutoHideWidth { get; set; }


        /// <summary>
        /// Gets or sets the height of the pane in the auto-hide state.
        /// </summary>
        /// <value>The height of pane in the auto-hide state.</value>
        /// <remarks>
        /// This property is only used if the item is in <see cref="IDockControl.AutoHideTop"/> or
        /// <see cref="IDockControl.AutoHideBottom"/>.
        /// </remarks>
        double AutoHideHeight { get; set; }


        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <value>The icon.</value>
        object Icon { get; }


        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>The title.</value>
        string Title { get; }


        /// <summary>
        /// Gets or sets the ID of this <see cref="IDockTabItem"/>.
        /// </summary>
        /// <value>The ID of this <see cref="IDockTabItem"/>.</value>
        /// <remarks>
        /// <para>
        /// This ID is used when a docking layout is serialized/deserialized. The content of a dock
        /// tab item is not serialized. Instead, this ID is stored. When the layout is loaded the
        /// application must be able to reconstruct the <see cref="IDockTabItem"/> based on this
        /// IDs.
        /// </para>
        /// <para>
        /// For example: For tool windows the dock ID could store the name of the tool window, e.g.
        /// "SolutionExplorer", "Properties". For documents the dock ID could store the file path of
        /// the document.
        /// </para>
        /// </remarks>
        string DockId { get; }
    }
}
