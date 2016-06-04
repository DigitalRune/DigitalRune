// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides event data for the <see cref="E:DigitalRune.Windows.ThemeManager.ThemeChanged"/>
    /// routed event.
    /// </summary>
    public sealed class ThemeChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldBaseTheme">The previous base theme.</param>
        /// <param name="oldColorTheme">The previous color theme.</param>
        /// <param name="newBaseTheme">The current base theme.</param>
        /// <param name="newColorTheme">The current color theme.</param>
        public ThemeChangedEventArgs(Theme oldBaseTheme, Theme oldColorTheme, Theme newBaseTheme, Theme newColorTheme)
        {
            OldBaseTheme = oldBaseTheme;
            OldColorTheme = oldColorTheme;
            NewBaseTheme = newBaseTheme;
            NewColorTheme = newColorTheme;
        }


        /// <summary>
        /// Gets the previous base theme.
        /// </summary>
        ///<value>The previous base theme.</value>
        public Theme OldBaseTheme { get; }


        /// <summary>
        /// Gets the previous color theme.
        /// </summary>
        ///<value>The previous color theme.</value>
        public Theme OldColorTheme { get; }


        /// <summary>
        /// Gets the current base theme.
        /// </summary>
        ///<value>The current base theme.</value>
        public Theme NewBaseTheme { get; }


        /// <summary>
        /// Gets the current color theme.
        /// </summary>
        ///<value>The current color theme.</value>
        public Theme NewColorTheme { get; }
    }
}
