// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DigitalRune.Editor.Properties;
using DigitalRune.Editor.Themes;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using NLog;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Manages syntax highlighting definitions. (Replaces AvalonEdit's built-in
    /// <see cref="ICSharpCode.AvalonEdit.Highlighting.HighlightingManager"/>.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syntax highlighting definitions are loaded on demand from files. The search folders can
    /// be specified in the application settings. The default search folder is "XSHD".
    /// </para>
    /// <para>
    /// It is possible to override syntax highlighting definitions for specific themes. To do this a
    /// new syntax highlighting definition needs to be stored in a theme subfolder. For example: The
    /// default syntax highlighting for C# is stored as "XSHD\CSharp.xshd", the syntax highlighting
    /// definition for the "Dark" theme is stored as "XSHD\Dark\CSharp.xshd".
    /// </para>
    /// </remarks>
    internal class HighlightingManager : IHighlightingService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IThemeService _themeService;

        private readonly object _definitionsLock = new object();
        private List<ThemeAwareHighlightingDefinition> _definitions;
        private ReadOnlyCollection<ThemeAwareHighlightingDefinition> _definitionsReadOnly;
        private Dictionary<string, ThemeAwareHighlightingDefinition> _definitionsByName;
        private Dictionary<string, ThemeAwareHighlightingDefinition> _definitionsByExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a read-only copy of all registered highlighting definitions.
        /// </summary>
        /// <value>A read-only copy of all registered highlighting definitions.</value>
        public IReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
        {
            get
            {
                lock (_definitionsLock)
                {
                    EnsureDefinitions();
                    return _definitionsReadOnly;
                }
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingManager"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public HighlightingManager(IEditorService editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _themeService = editor.Services.GetInstance<IThemeService>().ThrowIfMissing();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Ensures that all definitions are registered.
        /// </summary>
        private void EnsureDefinitions()
        {
            if (_definitions == null)
            {
                _definitions = new List<ThemeAwareHighlightingDefinition>();
                _definitionsReadOnly = _definitions.AsReadOnly();
                _definitionsByName = new Dictionary<string, ThemeAwareHighlightingDefinition>();
                _definitionsByExtension = new Dictionary<string, ThemeAwareHighlightingDefinition>();

                var appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

                // Locate .XSHD files in search folders.
                var folders = Settings.Default
                                      .SyntaxHighlightingFolders
                                      .AsEnumerable()
                                      .Where(folder => !string.IsNullOrWhiteSpace(folder))
                                      .Select(folder => folder.Trim());
                var themes = _themeService.Themes.ToArray();
                foreach (var folder in folders)
                {
                    // Get absolute path.
                    var rootedFolder = folder;
                    if (!Path.IsPathRooted(rootedFolder))
                        rootedFolder = Path.Combine(appFolder, rootedFolder);

                    if (!Directory.Exists(rootedFolder))
                    {
                        Logger.Warn(CultureInfo.InvariantCulture, "Search directory \"{0}\" for syntax highlighting does not exist.", rootedFolder);
                        continue;
                    }

                    // Register highlighting definitions for default theme.
                    foreach (var file in Directory.EnumerateFiles(rootedFolder, "*.xshd"))
                        Register(file, null);

                    // Register (optional) highlighting definitions for specific themes.
                    foreach (var theme in themes)
                    {
                        var themeFolder = Path.Combine(rootedFolder, theme);
                        if (Directory.Exists(themeFolder))
                            foreach (var file in Directory.EnumerateFiles(themeFolder, "*.xshd"))
                                Register(file, theme);
                    }
                }
            }
        }


        /// <summary>
        /// Registers the specified syntax highlighting definition (.XSHD file).
        /// </summary>
        /// <param name="file">The file including path.</param>
        /// <param name="theme">The theme. (Can be <see langword="null"/>.)</param>
        private void Register(string file, string theme)
        {
            // Quickly inspect the file to get name of language and supported file extensions.
            string name, extensionsString;
            if (InspectHighlightingDefinition(file, out name, out extensionsString))
            {
                Logger.Debug(
                    "Registering syntax highlighting definition (file = \"{0}\", name = \"{1}\", extensions = \"{2}\".",
                    file, name, extensionsString);

                // Use the file name if no name is defined in the highlighting definition.
                if (string.IsNullOrWhiteSpace(name))
                    name = Path.GetFileNameWithoutExtension(file);

                Debug.Assert(name != null, "Sanity check.");

                // Register highlighting definition in list and lookup tables.
                ThemeAwareHighlightingDefinition highlightingDefinition;
                if (!_definitionsByName.TryGetValue(name, out highlightingDefinition))
                {
                    highlightingDefinition = new ThemeAwareHighlightingDefinition(name, _themeService);
                    _definitions.Add(highlightingDefinition);

                    // Register language name.
                    _definitionsByName.Add(name, highlightingDefinition);

                    // Register supported file extensions.
                    if (extensionsString != null)
                    {
                        var extensions = extensionsString.Split(';')
                                                         .Select(extension => extension.Trim())
                                                         .Where(extension => extension.Length > 0);
                        foreach (var extension in extensions)
                            _definitionsByExtension[extension] = highlightingDefinition;
                    }
                }

                // Register function for deferred loading of highlighting definition.
                Func<IHighlightingDefinition> loadDefinition = () =>
                                                               {
                                                                   XshdSyntaxDefinition xshd;
                                                                   using (var reader = XmlReader.Create(file))
                                                                       xshd = HighlightingLoader.LoadXshd(reader);

                                                                   return HighlightingLoader.Load(xshd, this);
                                                               };
                highlightingDefinition.Register(theme, loadDefinition);
            }
        }


        /// <summary>
        /// Inspects the syntax highlighting definition (.XSHD file).
        /// </summary>
        /// <param name="file">The file name including path.</param>
        /// <param name="name">The name of language.</param>
        /// <param name="extensions">The supported file extensions.</param>
        /// <returns>
        /// <see langword="true"/> if the syntax highlighting definition appears to be valid; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// Invalid root element in syntax highlighting definition (.XSHD file).
        /// </exception>
        private static bool InspectHighlightingDefinition(string file, out string name, out string extensions)
        {
            Logger.Debug(CultureInfo.InvariantCulture, "Inspecting syntax highlighting definition \"{0}\".", file);

            try
            {
                using (var reader = XmlReader.Create(file))
                {
                    // Only check the root element "SyntaxDefinition" of the XML file.
                    if (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        if (reader.Name != "SyntaxDefinition")
                            throw new InvalidDataException("Invalid root element in syntax highlighting definition (.XSHD file).");

                        // Read attributes of root element.
                        name = reader.GetAttribute("name");
                        extensions = reader.GetAttribute("extensions");

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, CultureInfo.InvariantCulture, "Invalid syntax highlighting definition \"{0}\".", file);
            }

            // Inspection failed.
            name = null;
            extensions = null;
            return false;
        }


        /// <inheritdoc/>
        public IHighlightingDefinition GetDefinition(string name)
        {
            lock (_definitionsLock)
            {
                EnsureDefinitions();
                ThemeAwareHighlightingDefinition definition;
                _definitionsByName.TryGetValue(name, out definition);
                return definition;
            }
        }


        /// <summary>
        /// Gets the definition by file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>
        /// The definition for the given file extension. Returns <see langword="null"/> if no
        /// matching definition was found.
        /// </returns>
        public IHighlightingDefinition GetDefinitionByExtension(string extension)
        {
            lock (_definitionsLock)
            {
                EnsureDefinitions();
                ThemeAwareHighlightingDefinition definition;
                _definitionsByExtension.TryGetValue(extension, out definition);
                return definition;
            }
        }
        #endregion
    }
}
