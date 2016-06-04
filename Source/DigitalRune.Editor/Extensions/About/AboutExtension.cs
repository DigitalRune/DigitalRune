// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.About
{
    /// <summary>
    /// Adds an About dialog to the application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IAboutService"/></item>
    /// </list>
    /// <para>
    /// Command line arguments parsed by this extension: --about, --version
    /// </para>
    /// </remarks>
    public partial class AboutExtension : EditorExtension, IAboutService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IWindowService _windowService;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string ApplicationName { get; set; }


        /// <inheritdoc/>
        public string Copyright { get; set; }


        /// <inheritdoc/>
        public string Version { get; set; }


        /// <inheritdoc/>
        public object Information { get; set; }


        /// <inheritdoc/>
        public string InformationAsString { get; set; }


        /// <inheritdoc/>
        public object Icon { get; set; }


        /// <inheritdoc/>
        public ICollection<EditorExtensionDescription> ExtensionDescriptions { get; } = new List<EditorExtensionDescription>();
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutExtension"/> class.
        /// </summary>
        public AboutExtension()
        {
            // Initialize Copyright and Version from assembly attributes of the entry assembly.
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)  // Can be null at design-time.
            {
                var copyrightAttribute = entryAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                                                      .Cast<AssemblyCopyrightAttribute>()
                                                      .FirstOrDefault();

                Copyright = copyrightAttribute?.Copyright;

                var version = entryAssembly.GetName().Version;
                Version = Invariant($"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            }

            // Add extension info for the editor itself.
            Version editorVersion = Assembly.GetAssembly(typeof(EditorViewModel)).GetName().Version;
            string versionString = Invariant($"{editorVersion.Major}.{editorVersion.Minor}.{editorVersion.Build}.{editorVersion.Revision}");

            ExtensionDescriptions.Add(new EditorExtensionDescription
            {
                Name = "DigitalRune Editor",
                Description = "The DigitalRune Editor provides an extensible framework for developing Windows applications.",
                Version = versionString,
            });

            Icon = MultiColorGlyphs.MessageInformation;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            // Copy ApplicationName from editor.
            ApplicationName = Editor.ApplicationName;

            AddCommandLineArguments();

            // Register services and views.
            Editor.Services.Register(typeof(IAboutService), null, this);
            Editor.Services.RegisterView(typeof(AboutViewModel), typeof(AboutWindow));
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _windowService = Editor.Services.GetInstance<IWindowService>().ThrowIfMissing();
            ParseCommandLineArguments();

            AddCommands();
            AddMenus();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveMenus();
            RemoveCommands();

            _windowService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.UnregisterView(typeof(AboutViewModel));
            Editor.Services.Unregister(typeof(IAboutService));
            RemoveCommandLineArguments();
        }


        /// <summary>
        /// Creates the command items of this extension.
        /// </summary>
        private void AddCommands()
        {
            // Add a command item that shows the About dialog.
            CommandItems.Add(
              new DelegateCommandItem("ShowAboutDialog", new DelegateCommand(Show))
              {
                  Category = CommandCategories.Help,
                  Icon = MultiColorGlyphs.MessageInformation,
                  Text = "_About...",
                  ToolTip = "Show a dialog with information about this application."
              });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        /// <summary>
        /// Creates the menu structure of this extension.
        /// </summary>
        private void AddMenus()
        {
            // Add the "About..." menu item to the "Help" menu.
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("HelpGroup", "_Help"),
                    new MergeableNode<ICommandItem>(CommandItems["ShowAboutDialog"])),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        /// <summary>
        /// Shows the About dialog window.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method opens the about dialog and returns only when the About dialog is closed.
        /// </para>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> Inherited classes can override this method to display 
        /// a custom about dialog.
        /// </para>
        /// </remarks>
        public virtual void Show()
        {
            Logger.Debug("Showing About dialog.");
            _windowService.ShowDialog(new AboutViewModel(this));
            Logger.Debug("About dialog closed.");
        }


        /// <inheritdoc/>
        public string CopyInformationToClipboard()
        {
            Logger.Debug("Copying About information to clipboard.");

            var information = GetInformation();
            Logger.Info(CultureInfo.InvariantCulture, "About information:\n{0}", information);

            Clipboard.SetText(information);

            return information;
        }


        private string GetInformation()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Framework name and version
            stringBuilder.AppendLine(ApplicationName);
            stringBuilder.Append("Version: ");
            stringBuilder.AppendLine(Version);
            stringBuilder.AppendLine(Copyright);

            // Write additional info
            if (InformationAsString != null)
                stringBuilder.AppendLine(InformationAsString);    // Use InformationAsString if it is set.
            else if (Information is string)
                stringBuilder.AppendLine((string)Information);    // Else use Information if it is a string.

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Component names, version and description
            stringBuilder.AppendLine("Installed Extensions:");
            stringBuilder.AppendLine("---------------------");
            stringBuilder.AppendLine();
            foreach (var description in ExtensionDescriptions)
            {
                stringBuilder.AppendLine(description.Name);
                stringBuilder.Append("Version: ");
                stringBuilder.AppendLine(description.Version);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(description.Description);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }
        #endregion
    }
}
