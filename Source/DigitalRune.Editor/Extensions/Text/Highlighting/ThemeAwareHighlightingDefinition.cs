// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DigitalRune.Editor.Themes;
using ICSharpCode.AvalonEdit.Highlighting;
using NLog;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Provides a syntax highlighting definition taking the current theme into account.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The actual syntax highlighting definition (.XSHD file) is loaded on demand, i.e. the file is
    /// only read when needed.
    /// </para>
    /// <para>
    /// The <see cref="ThemeAwareHighlightingDefinition"/> is aware of the current theme (see
    /// <see cref="IThemeService.Theme"/>). It may contain one syntax highlighting definition per
    /// theme.
    /// </para>
    /// </remarks>
    internal class ThemeAwareHighlightingDefinition : IHighlightingDefinition
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string UnsetTheme = "_UnsetTheme_";
        private static readonly HighlightingRuleSet EmptyRuleSet = new HighlightingRuleSet();

        private readonly IThemeService _themeService;
        private readonly Dictionary<string, Func<IHighlightingDefinition>> _definitionsByTheme;

        // Current selection:
        private string _theme;
        private IHighlightingDefinition _definition;
        private bool _isLoading;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public HighlightingRuleSet MainRuleSet
        {
            get { return EnsureDefinition() ? _definition.MainRuleSet : EmptyRuleSet; }
        }


        /// <inheritdoc/>
        public IEnumerable<HighlightingColor> NamedHighlightingColors
        {
            get { return EnsureDefinition() ? _definition.NamedHighlightingColors : Enumerable.Empty<HighlightingColor>(); }
        }


        /// <inheritdoc/>
        public IDictionary<string, string> Properties
        {
            get { return EnsureDefinition() ? _definition.Properties : null; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeAwareHighlightingDefinition"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="themeService">The theme service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="themeService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is empty.
        /// </exception>
        public ThemeAwareHighlightingDefinition(string name, IThemeService themeService)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("Name of syntax highlighting definition must not be empty.", nameof(name));
            if (themeService == null)
                throw new ArgumentNullException(nameof(themeService));

            Name = name;
            _themeService = themeService;
            _definitionsByTheme = new Dictionary<string, Func<IHighlightingDefinition>>();
            _theme = UnsetTheme;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Registers the highlighting definition for the specified theme.
        /// </summary>
        /// <param name="theme">The theme. (Can be <see langword="null"/>.)</param>
        /// <param name="loadDefinition">
        /// The function that loads the highlighting definition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="loadDefinition"/> is <see langword="null"/>.
        /// </exception>
        public void Register(string theme, Func<IHighlightingDefinition> loadDefinition)
        {
            if (loadDefinition == null)
                throw new ArgumentNullException(nameof(loadDefinition));

            _definitionsByTheme[theme ?? string.Empty] = loadDefinition;
        }


        /// <summary>
        /// Ensures that the highlighting definition is loaded.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the highlighting definition is valid; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        private bool EnsureDefinition()
        {
            if (_isLoading)
                throw new InvalidOperationException("Recursive syntax highlighting definition detected. Make sure that there are no cyclic reference between syntax highlighting definitions.");

            if (_theme != _themeService.Theme)
            {
                // Reload definition.
                _theme = _themeService.Theme;
                _definition = null;

                try
                {
                    _isLoading = true;

                    // Look for themes in the following order.
                    var keys = new[] { _theme, "Generic", "Default", string.Empty, null };
                    foreach (var key in keys)
                    {
                        _definition = LoadDefinition(key);
                        if (_definition != null)
                            return true;
                    }
                }
                finally
                {
                    _isLoading = false;
                }
            }

            return _definition != null;
        }


        /// <summary>
        /// Loads the highlighting definition for the specified theme.
        /// </summary>
        /// <param name="theme">The theme.</param>
        /// <returns>
        /// The highlighting definition for the specified theme. Returns the first available 
        /// highlighting definition if <paramref name="theme"/> is <see langword="null"/>.
        /// </returns>
        private IHighlightingDefinition LoadDefinition(string theme)
        {
            Func<IHighlightingDefinition> loadDefinition;
            if (theme == null)
            {
                // Fallback in case no default highlighting definition is registered.
                // (Only happens if there is a "XSHD\Theme\abc.xshd" but no "XSHD\abc.xshd".)
                loadDefinition = _definitionsByTheme.Values.FirstOrDefault();
            }
            else
            {
                _definitionsByTheme.TryGetValue(theme, out loadDefinition);
            }

            if (loadDefinition != null)
            {
                try
                {
                    return loadDefinition();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, CultureInfo.InvariantCulture, "Failed to load syntax highlighting definition \"{0}\" for theme \"{1}\"", Name, theme);
                }
            }

            return null;
        }


        /// <inheritdoc/>
        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            if (EnsureDefinition())
                return _definition.GetNamedRuleSet(name);

            return null;
        }


        /// <inheritdoc/>
        public HighlightingColor GetNamedColor(string name)
        {
            if (EnsureDefinition())
                return _definition.GetNamedColor(name);

            return null;
        }


        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns><see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
