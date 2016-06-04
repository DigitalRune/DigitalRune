// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Applies a visual theme to the WPF application or control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A WPF theme consists of two parts:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Base theme</term>
    /// <description>
    /// The <i>base theme</i> is a resource dictionary that contains the styles that define the
    /// appearance of the WPF controls.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Color theme</term>
    /// <description>
    /// The <i>color theme</i> is an additional resource dictionary that can be used to customize
    /// the base theme. (The base theme binds certain resources, i.e. colors, brushes, etc. using
    /// the <see cref="DynamicResourceExtension"/>. These resources can be customized in the color
    /// theme.)
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// <strong>Design-time:</strong>
    /// It is save to call <see cref="ApplyTheme(FrameworkElement,Theme,Theme)"/> at design-time.
    /// The <see cref="ThemeManager"/> internally checks whether the code is in the Visual Studio
    /// Designer or Expression Blend and does nothing at design-time.
    /// </para>
    /// </remarks>
    public static class ThemeManager
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        private abstract class ThemeResourceDictionary : ResourceDictionary
        {
            public Theme Theme { get; set; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        private class BaseThemeResourceDictionary : ThemeResourceDictionary
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
        private class ColorThemeResourceDictionary : ThemeResourceDictionary
        {
        }
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private static readonly WeakEvent<EventHandler<ThemeChangedEventArgs>> ThemeChangedEvent = new WeakEvent<EventHandler<ThemeChangedEventArgs>>();

        /// <summary>
        /// Weak event raised when the theme is changed.
        /// </summary>
        public static event EventHandler<EventArgs> ThemeChanged
        {
            add { ThemeChangedEvent.Add(value); }
            remove { ThemeChangedEvent.Remove(value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Applies a WPF theme.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Applies a WPF theme to the specified <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="element">
        /// The element to which the theme and accent should be applied.
        /// </param>
        /// <param name="baseTheme">
        /// The base theme. Can be <see langword="null"/> to clear the base theme.
        /// </param>
        /// <param name="colorTheme">
        /// The color theme. Can be <see langword="null"/> to clear the color theme.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void ApplyTheme(FrameworkElement element, Theme baseTheme, Theme colorTheme)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            ApplyTheme(element, element.Resources, baseTheme, colorTheme);
        }


        /// <summary>
        /// Applies a WPF theme and accent to the specified <see cref="Application"/>.
        /// </summary>
        /// <param name="application">
        /// The application to which the WPF theme and accent should be applied.
        /// </param>
        /// <param name="baseTheme">
        /// The base theme. Can be <see langword="null"/> to clear the base theme.
        /// </param>
        /// <param name="colorTheme">
        /// The color theme. Can be <see langword="null"/> to clear the color theme.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="application"/> is <see langword="null"/>.
        /// </exception>
        public static void ApplyTheme(Application application, Theme baseTheme, Theme colorTheme)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            ApplyTheme(application, application.Resources, baseTheme, colorTheme);
        }


        private static void ApplyTheme(object sender, ResourceDictionary resources, Theme baseTheme, Theme colorTheme)
        {
            Debug.Assert(resources != null);

            if (WindowsHelper.IsInDesignMode)
            {
                // Don't do anything at design-time. (Could mess up Expression Blend.)
                return;
            }

            // Clear previous theme.
            var baseThemeResourceDictionary = ClearThemeResourceDictionary<BaseThemeResourceDictionary>(resources);
            var colorThemeResourceDictionary = ClearThemeResourceDictionary<ColorThemeResourceDictionary>(resources);

            // Apply new Theme.
            ApplyThemeResourceDictionary(resources, baseTheme, baseThemeResourceDictionary);
            ApplyThemeResourceDictionary(resources, colorTheme, colorThemeResourceDictionary);

            // Raise ThemeChanged event.
            var oldBaseTheme = baseThemeResourceDictionary?.Theme;
            var oldColorTheme = colorThemeResourceDictionary?.Theme;
            var eventArgs = new ThemeChangedEventArgs(oldBaseTheme, oldColorTheme, baseTheme, colorTheme);
            ThemeChangedEvent.Invoke(sender, eventArgs);
        }


        private static T ClearThemeResourceDictionary<T>(ResourceDictionary resources) where T : ThemeResourceDictionary
        {
            var themeResourceDictionary = resources.MergedDictionaries
                                                   .OfType<T>()
                                                   .FirstOrDefault();
            if (themeResourceDictionary != null)
                resources.MergedDictionaries.Remove(themeResourceDictionary);

            return themeResourceDictionary;
        }


        private static void ApplyThemeResourceDictionary<T>(ResourceDictionary resources, Theme theme, T themeResourceDictionary) where T : ThemeResourceDictionary, new()
        {
            if (theme == null)
                return;

            if (themeResourceDictionary?.Theme != theme)
                themeResourceDictionary = new T { Source = theme.Source, Theme = theme };

            resources.MergedDictionaries.Add(themeResourceDictionary);
        }
        #endregion
    }
}
