// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an element in the docking layout.
    /// </summary>
    public interface IDockElement
    {
        /// <summary>
        /// Gets (or sets) the current state in the docking layout.
        /// </summary>
        /// <value>The current state in the docking layout.</value>
        /// <remarks>
        /// <para>
        /// This property is set automatically by the <see cref="DockStrategy"/>. It has different
        /// meanings for <see cref="IDockPane"/>s and <see cref="IDockTabItem"/>s.
        /// </para>
        /// <para>
        /// The <see cref="IDockPane"/>s define the docking layout. A specific instance of
        /// <see cref="IDockPane"/> may only be referenced at one location in the docking layout.
        /// The property <see cref="DockState"/> in this case indicates the position of the of the
        /// pane. <see cref="Docking.DockState.Dock"/> means that the pane is in the
        /// <see cref="IDockControl"/>, <see cref="Docking.DockState.Float"/> means that the pane is
        /// in an <see cref="IFloatWindow"/>, and <see cref="Docking.DockState.AutoHide"/> means
        /// that the pane is in one of the auto-hide bars (<see cref="IDockControl.AutoHideLeft"/>,
        /// <see cref="IDockControl.AutoHideRight"/>, <see cref="IDockControl.AutoHideTop"/>, or
        /// <see cref="IDockControl.AutoHideBottom"/>).
        /// </para>
        /// <para>
        /// An instance of <see cref="IDockTabItem"/> may be referenced multiple times in the
        /// docking layout. It may be stored in the <see cref="IDockControl"/>, in an
        /// <see cref="IFloatWindow"/>, and in one of the auto-hide bars. The item is only shown at
        /// one of these locations. The <see cref="DockState"/> indicates where the item is
        /// currently visible. At the other locations in the docking layout the item is hidden. For
        /// example, changing the <see cref="DockState"/> from <see cref="Docking.DockState.Dock"/>
        /// to <see cref="Docking.DockState.Float"/> means that the item is hidden in the
        /// <see cref="IDockControl"/> and shown in the <see cref="IFloatWindow"/>.
        /// </para>
        /// <para>
        /// The <see cref="DockState"/> can be changed by calling
        /// <see cref="DockStrategy.Dock(IDockTabPane)"/>,
        /// <see cref="DockStrategy.Float(IDockTabPane)"/>, or
        /// <see cref="DockStrategy.AutoHide(IDockTabPane)"/>. It is not recommended to change the
        /// property directly as this may lead to inconsistent docking layouts or states.
        /// </para>
        /// </remarks>
        DockState DockState { get; set; }


        /// <summary>
        /// Gets or sets the desired width in the docking layout.
        /// </summary>
        /// <value>The desired width in the docking layout.</value>
        GridLength DockWidth { get; set; }


        /// <summary>
        /// Gets or sets the desired height in the docking layout.
        /// </summary>
        /// <value>The desired height in the docking layout.</value>
        GridLength DockHeight { get; set; }
    }
}
