// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Provides the Outline window for browsing object hierarchies.
    /// </summary>
    public interface IOutlineService
    {
        /// <summary>
        /// Gets the view model of the Outline window.
        /// </summary>
        /// <value>The view model of the Outline window.</value>
        EditorDockTabItemViewModel OutlineViewModel { get; }


        /// <summary>
        /// Gets or sets the currently displayed outline.
        /// </summary>
        /// <value>The currently displayed outline.</value>
        Outline Outline { get; set; }
    }
}
