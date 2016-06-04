// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Editor.Options;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Themes
{
    /// <summary>
    /// Allows to choose the WPF theme.
    /// </summary>
    internal class ThemeOptionsPageViewModel : OptionsPageViewModel
    {
        private readonly ThemeExtension _themeExtension;


        /// <summary>
        /// Gets a <see cref="ThemeOptionsPageViewModel"/> instance that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// a <see cref="ThemeOptionsPageViewModel"/> instance that can be used at design-time.
        /// </value>
        internal static ThemeOptionsPageViewModel DesignInstance
        {
            get
            {
                var themesExtension = new ThemeExtension();
                return new ThemeOptionsPageViewModel(themesExtension);
            }
        }


        /// <summary>
        /// Gets a list of available themes.
        /// </summary>
        /// <value>A list of available themes.</value>
        public IEnumerable<string> Themes
        {
            get { return _themeExtension.Themes; }
        }


        /// <summary>
        /// Gets or sets the currently selected theme.
        /// </summary>
        /// <value>The currently selected theme.</value>
        public string Theme
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }
        private string _theme;


        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeOptionsPageViewModel"/> class.
        /// </summary>
        /// <param name="themeExtension">The themes extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="themeExtension"/> is <see langword="null"/>.
        /// </exception>
        public ThemeOptionsPageViewModel(ThemeExtension themeExtension)
            : base("Themes")
        {
            if (themeExtension == null)
                throw new ArgumentNullException(nameof(themeExtension));

            _themeExtension = themeExtension;
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
                Theme = _themeExtension.Theme;

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        protected override void OnApply()
        {
            _themeExtension.Theme = Theme;
        }
    }
}
