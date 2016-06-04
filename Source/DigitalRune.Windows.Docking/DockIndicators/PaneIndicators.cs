// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Appears over a <see cref="DockTabPane"/> when a <see cref="DockTabItem"/> is dragged and
    /// visualizes the areas where the user can dock the window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default, the <see cref="PaneIndicators"/> shows dock indicators for the drop areas:
    /// <see cref="DockPosition.Left"/>, <see cref="DockPosition.Right"/>,
    /// <see cref="DockPosition.Top"/>, <see cref="DockPosition.Bottom"/>, and
    /// <see cref="DockPosition.Inside"/>.
    /// </para>
    /// <para>
    /// See base class <see cref="DockIndicatorOverlay"/> for informations regarding styling.
    /// </para>
    /// </remarks>
    public class PaneIndicators : DockIndicatorOverlay
    {
        /// <summary>
        /// Initializes static members of the <see cref="DockIndicatorOverlay"/> class.
        /// </summary>
        static PaneIndicators()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneIndicators), new FrameworkPropertyMetadata(typeof(PaneIndicators)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaneIndicators"/> class.
        /// </summary>
        /// <param name="target">
        /// The target element over which the indicators should appear. (Typically a
        /// <see cref="DockTabPane"/>.)
        /// </param>
        public PaneIndicators(FrameworkElement target) 
            : base(target)
        {
        }
    }
}
