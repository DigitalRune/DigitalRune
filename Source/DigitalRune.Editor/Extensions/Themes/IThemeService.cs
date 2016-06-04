// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Themes
{
    /// <summary>
    /// Manages the the UI themes.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the available themes.
        /// </summary>
        IEnumerable<string> Themes { get; }


        /// <summary>
        /// Gets or sets the currently selected theme.
        /// </summary>
        /// <value>The currently selected theme.</value>
        /// <remarks>
        /// Themes can also be applied by using the <see cref="ThemeManager"/> directly. However, it
        /// is recommended to apply a theme by setting this property. The editor ensures that the
        /// selected theme is saved when the application exits and restored when the application is
        /// restarted.
        /// </remarks>
        string Theme { get; set; }


        /// <summary>
        /// Occurs when the UI theme was changed.
        /// </summary>
        /// <seealso cref="ThemeExtension.Theme"/>
        event EventHandler<EventArgs> ThemeChanged;
    }
}
