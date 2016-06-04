// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using DigitalRune.Collections;
using DigitalRune.Editor.Properties;
using DigitalRune.Editor.Output;
using DigitalRune.Storages;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Diagnostics
{
    /// <summary>
    /// Provides debugging functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// At the start of the application the extension logs helpful information.
    /// </para>
    /// <para>
    /// This extension adds the "Tools | Debugging" menu.
    /// </para>
    /// </remarks>
    public sealed class DiagnosticsExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const string NodesView = "Nodes";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private MergeableNodeCollection<ICommandItem> _menuNodes;

        private Process _systemInfoProcess;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsExtension"/> class.
        /// </summary>
        public DiagnosticsExtension()
        {
            // Log helpful data.
            Logger.Debug(CultureInfo.InvariantCulture, "Current folder: {0}", Environment.CurrentDirectory);
            Logger.Debug(CultureInfo.InvariantCulture, "Executable folder: {0}", Environment.CurrentDirectory);
            Logger.Debug(CultureInfo.InvariantCulture, "Application settings folder: {0}", EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.None));
            Logger.Debug(CultureInfo.InvariantCulture, "User settings folder: {0}", EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddCommands();
            AddMenus();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveMenus();
            RemoveCommands();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
        }


        private void AddCommands()
        {
            CommandItems.Add(
                new DelegateCommandItem("InspectEditor", new DelegateCommand(InspectEditor))
                {
                    Category = CommandCategories.Tools,
                    Text = "Inspect editor",
                    ToolTip = "Show the editor in the Properties window."
                },
                new DelegateCommandItem("LaunchDebugger", new DelegateCommand(LaunchDebugger))
                {
                    Category = CommandCategories.Tools,
                    Text = "Launch debugger",
                    ToolTip = "Launch the Visual Studio Debugger for debugging the running application."
                },
                new DelegateCommandItem("OpenExecutableFolder", new DelegateCommand(OpenExecutableFolder))
                {
                    Category = CommandCategories.Tools,
                    Text = "Open executable folder",
                    ToolTip = "Open the folder containing the executable."
                },
                new DelegateCommandItem("OpenApplicationSettings", new DelegateCommand(OpenApplicationSettings))
                {
                    Category = CommandCategories.Tools,
                    Text = "Open application settings",
                    ToolTip = "Open the application settings folder in Windows Explorer."
                },
                new DelegateCommandItem("OpenUserSettings", new DelegateCommand(OpenUserSettings))
                {
                    Category = CommandCategories.Tools,
                    Text = "Open user settings",
                    ToolTip = "Open the user settings folder in Windows Explorer."
                },
                new DelegateCommandItem("OutputCommandNodes", new DelegateCommand(OutputCommandNodes, CanOutputCommandNodes))
                {
                    Category = CommandCategories.Tools,
                    Text = "Output command nodes",
                    ToolTip = "Prints the structure of all menu and toolbar commands."
                },
                new DelegateCommandItem("SystemInfo", new DelegateCommand(ShowSystemInfo))
                {
                    Category = CommandCategories.Help,
                    //Icon = MultiColorGlyphs.MessageInformation,
                    Text = "_System Info...",
                    ToolTip = "Open the Microsoft System Information application (msinfo32.exe)."
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("ToolsGroup", "_Tools"),
                    new MergeableNode<ICommandItem>(new CommandGroup("DebuggingGroup", "Debugging"), new MergePoint(MergeOperation.InsertBefore, "ToolsSeparator"), MergePoint.Append,
                        new MergeableNode<ICommandItem>(CommandItems["InspectEditor"]),
                        new MergeableNode<ICommandItem>(CommandItems["LaunchDebugger"]),
                        new MergeableNode<ICommandItem>(CommandItems["OpenExecutableFolder"]),
                        new MergeableNode<ICommandItem>(CommandItems["OpenApplicationSettings"]),
                        new MergeableNode<ICommandItem>(CommandItems["OpenUserSettings"]),
                        new MergeableNode<ICommandItem>(CommandItems["OutputCommandNodes"]))),
                new MergeableNode<ICommandItem>(new CommandGroup("HelpGroup", "_Help"),
                    new MergeableNode<ICommandItem>(CommandItems["SystemInfo"])),
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


        private void InspectEditor()
        {
            Logger.Debug("Inspecting editor.");

            var inspectionService = Editor.Services.GetInstance<IPropertiesService>().WarnIfMissing();
            if (inspectionService != null)
            {
                // Important: The order of ActivateItem and PropertySource.set matters.
                // Make Properties window active before changing the property source. Otherwise, 
                // document may think they are still in control of the properties window and change
                // the content.
                Editor.ActivateItem(inspectionService.PropertiesViewModel);

                inspectionService.PropertySource = PropertyGridHelper.CreatePropertySource(Editor);
            }
        }


        private void LaunchDebugger()   // Do not make method static to make debugging easier.
        {
            Logger.Info("Launching debugger.");

            if (Debugger.IsAttached)
                Debugger.Break();
            else
                Debugger.Launch();
        }


        private static void OpenExecutableFolder()
        {
            var executableFolder = GetExecutableFolder();
            Logger.Info(CultureInfo.InvariantCulture, "Opening executable folder \"{0}\".", executableFolder);

            if (!string.IsNullOrEmpty(executableFolder))
                Process.Start(executableFolder);
        }


        private static string GetExecutableFolder()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        }


        private static void OpenApplicationSettings()
        {
            Logger.Info("Opening application settings.");

            Process.Start(EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.None));
        }


        private static void OpenUserSettings()
        {
            Logger.Info("Opening user settings.");

            Process.Start(EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal));
        }


        private bool CanOutputCommandNodes()
        {
            return Editor.Services.GetInstance<IOutputService>() != null;
        }


        #region ----- Output nodes -----

        private void OutputCommandNodes()
        {
            var outputService = Editor.Services.GetInstance<IOutputService>();
            if (outputService == null)
                return;

            outputService.Clear(NodesView);

            outputService.WriteLine("Menus:", NodesView);
            OutputMenuNodes(outputService, Editor.Menu, 1);
            outputService.WriteLine(string.Empty, NodesView);

            outputService.WriteLine("Toolbars:", NodesView);
            OutputToolBarNodes(outputService, Editor.ToolBars, 1);
            outputService.WriteLine(string.Empty, NodesView);

            outputService.WriteLine("Dock context menu:", NodesView);
            OutputMenuNodes(outputService, Editor.DockContextMenu, 1);

            outputService.Show(NodesView);
        }


        private static string Indent(int level)
        {
            return string.Empty.PadLeft(level * 4, ' ');
        }


        private static void OutputMenuNodes(IOutputService outputService, MenuItemViewModelCollection menuItems, int level = 0)
        {
            if (menuItems == null || menuItems.Count == 0)
                return;

            var indent = Indent(level);
            foreach (var menuItem in menuItems)
            {
                outputService.WriteLine(Invariant($"{indent}\"{menuItem.CommandItem.Name}\""), NodesView);
                OutputMenuNodes(outputService, menuItem.Submenu, level + 1);
            }
        }


        private static void OutputToolBarNodes(IOutputService outputService, ToolBarViewModelCollection toolBars, int level = 0)
        {
            if (toolBars == null || toolBars.Count == 0)
                return;

            var indent = Indent(level);
            foreach (var toolBar in toolBars)
            {
                outputService.WriteLine(Invariant($"{indent}\"{toolBar.CommandGroup.Name}\""), NodesView);
                OutputToolBarNodes(outputService, toolBar.Items, level + 1);
            }
        }


        private static void OutputToolBarNodes(IOutputService outputService, ToolBarItemViewModelCollection toolBarItems, int level = 0)
        {
            if (toolBarItems == null || toolBarItems.Count == 0)
                return;

            var indent = Indent(level);
            foreach (var toolBarItem in toolBarItems)
            {
                outputService.WriteLine(Invariant($"{indent}\"{toolBarItem.CommandItem.Name}\""), NodesView);
            }
        }
        #endregion


        private void ShowSystemInfo()
        {
            Logger.Info("Running msinfo32.exe.");

            // Run msinfo32.exe.
            // See also
            // https://support.microsoft.com/en-us/kb/300887
            // https://technet.microsoft.com/en-us/library/bb490937.aspx

            // If the process is already running, we only need to make the msinfo32 window visible.
            if (_systemInfoProcess != null && !_systemInfoProcess.HasExited)
            {
                SetForegroundWindow(_systemInfoProcess.MainWindowHandle);
                return;
            }

            try
            {
                _systemInfoProcess = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo =
                    {
                        UseShellExecute = false,
                        FileName = "msinfo32.exe",
                        CreateNoWindow = true,
                        //StartInfo.Arguments =...,
                        //WorkingDirectory = ...,
                    }
                };
                _systemInfoProcess.Start();
            }
            catch (Exception exception)
            {
                _systemInfoProcess = null;

                var message = "Failed to run msinfo32.exe.";
                Logger.Error(exception, message);
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion
    }
}
