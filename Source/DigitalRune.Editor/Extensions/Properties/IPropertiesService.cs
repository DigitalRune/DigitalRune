// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows.Controls;


namespace DigitalRune.Editor.Properties
{
    /// <summary>
    /// Provides the Properties window for browsing object properties.
    /// </summary>
    public interface IPropertiesService
    {
        /// <summary>
        /// Gets the view model of the Properties window.
        /// </summary>
        /// <value>The view model of the Properties window.</value>
        EditorDockTabItemViewModel PropertiesViewModel { get; }


        /// <summary>
        /// Gets or sets the properties to show in the Properties window.
        /// </summary>
        /// <value>
        /// The properties to show in the InspPropertiesector window.
        /// </value>
        IPropertySource PropertySource { get; set; }


        ///// <summary>
        ///// Shows the specified property source in the Properties window.
        ///// </summary>
        ///// <param name="propertySource">
        ///// The property source that will be shown in the Properties window.
        ///// </param>
        ///// <param name="keepHistory">
        ///// If set to <see langword="true"/>, the Properties window will remember the previously
        ///// inspected history and the user can use "Back" commands to show the previous object in
        ///// the Properties window.
        ///// </param>
        ///// <remarks>
        ///// This method changes only the data displayed in the Properties window. If the Properties 
        ///// window is not visible, it will NOT be opened.
        ///// </remarks>
        //void Show(IPropertySource propertySource, bool keepHistory);


        ///// <summary>
        ///// Removes the specified property source from the Properties window.
        ///// </summary>
        ///// <param name="propertySource">
        ///// The property source which should no longer be shown in the Properties window.
        ///// </param>
        ///// <remarks>
        ///// This method does not close the Properties window.
        ///// </remarks>
        //void Hide(IPropertySource propertySource);
    }
}
