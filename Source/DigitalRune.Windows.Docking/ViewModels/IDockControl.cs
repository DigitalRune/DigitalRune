// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents the control that contains and manages the docking layout.
    /// </summary>
    public interface IDockControl : IDockContainer, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the docking strategy that controls the layout changes.
        /// </summary>
        /// <value>The docking strategy. Must not be <see langword="null"/>.</value>
        DockStrategy DockStrategy { get; }


        /// <summary>
        /// Gets or sets a value indicating whether the docking layout is locked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the layout is locked to prevent dragging operations;
        /// otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// When the docking layout is locked, it is not possible for the user to drag dock items
        /// or to change the dock state using routed commands (e.g. via menus). It is still possible
        /// to show/close dock items.
        /// </remarks>
        bool IsLocked { get; set; }


        /// <summary>
        /// Gets or sets the active pane.
        /// </summary>
        /// <value>The active pane.</value>
        /// <inheritdoc cref="ActiveDockTabItem"/>
        IDockTabPane ActiveDockTabPane { get; set; }


        /// <summary>
        /// Gets or sets the active item.
        /// </summary>
        /// <value>The active item.</value>
        /// <remarks>
        /// <para>
        /// The active pane/item are usually determined automatically by the
        /// <see cref="DockStrategy"/>. To change the currently active pane/item call
        /// <see cref="Docking.DockStrategy.Show"/>.
        /// </para>
        /// <para>
        /// When the properties are set directly the following order of operation should be
        /// obeyed:
        /// </para>
        /// <list type="number">
        /// <item>Set <see cref="IDockTabPane.SelectedItem"/> in <see cref="IDockTabPane"/>.</item>
        /// <item>Set <see cref="ActiveDockTabPane"/>.</item>
        /// <item>Set <see cref="ActiveDockTabItem"/>.</item>
        /// </list>
        /// </remarks>
        IDockTabItem ActiveDockTabItem { get; set; }


        /// <summary>
        /// Gets the <see cref="IFloatWindow"/>s.
        /// </summary>
        /// <value>The <see cref="IFloatWindow"/>s. Must not be <see langword="null"/>.</value>
        FloatWindowCollection FloatWindows { get; }


        /// <summary>
        /// Gets the items in the left auto-hide bar.
        /// </summary>
        /// <value>The items in the left auto-hide bar. Must not be <see langword="null"/>.</value>
        DockTabPaneCollection AutoHideLeft { get; }


        /// <summary>
        /// Gets the items in the right auto-hide bar.
        /// </summary>
        /// <value>The items in the right auto-hide bar. Must not be <see langword="null"/>.</value>
        DockTabPaneCollection AutoHideRight { get; }


        /// <summary>
        /// Gets the items in the top auto-hide bar.
        /// </summary>
        /// <value>The items in the top auto-hide bar. Must not be <see langword="null"/>.</value>
        DockTabPaneCollection AutoHideTop { get; }


        /// <summary>
        /// Gets the items in the bottom auto-hide bar.
        /// </summary>
        /// <value>
        /// The items in the bottom auto-hide bar. Must not be <see langword="null"/>.
        /// </value>
        DockTabPaneCollection AutoHideBottom { get; }


        /// <summary>
        /// Gets or sets the dock control UI element.
        /// </summary>
        /// <value>The dock control UI element.</value>
        /// <remarks>
        /// This property is set automatically when the <see cref="Docking.DockControl"/> is loaded.
        /// </remarks>
        DockControl DockControl { get; set; }
    }
}
