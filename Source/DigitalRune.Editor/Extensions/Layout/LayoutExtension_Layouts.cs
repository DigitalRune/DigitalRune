// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using DigitalRune.Collections;
using DigitalRune.Editor.Properties;


namespace DigitalRune.Editor.Layout
{
    sealed partial class LayoutExtension
    {
        // Terminology:
        // - "Factory preset" ... Built-in window layout stored in application folder.
        // - "User preset" ...... User-defined window layout stored in user settings folder.
        // - "Session" .......... Any changes applied to active window layout, stored separately.
        // - "Reset layout" ..... Delete any session data and restore factory/user preset.


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        // Folders storing the window layouts:
        // - Factory presets: "<EXECUTABLE_FOLDER>\Layouts\Presets\*.xml"
        // - User presets:    "%LOCALAPPDATA%\Layouts\Presets\*.xml".
        // - Session layouts: "%LOCALAPPDATA%\Layouts\*.xml"
        private const string LayoutsFolder = "Layouts";
        private const string PresetsFolder = "Presets";
        private const string DefaultLayout = "Default";
        private const string LayoutPattern = "*.xml";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the window layouts.
        /// </summary>
        /// <value>The window layouts.</value>
        internal ObservableCollection<WindowLayout> Layouts { get; } = new ObservableCollection<WindowLayout>();


        /// <summary>
        /// Gets the active window layout.
        /// </summary>
        /// <value>The active window layout.</value>
        internal WindowLayout ActiveLayout
        {
            get { return _activeLayout; }
            private set
            {
                if (_activeLayout == value)
                    return;

                if (_activeLayout != null)
                    _activeLayout.IsActive = false;

                _activeLayout = value;

                if (_activeLayout != null)
                    _activeLayout.IsActive = true;
            }
        }
        private WindowLayout _activeLayout;
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Updates the window layout menu and toolbar items.
        /// </summary>
        /// <remarks>
        /// Needs to be called when the <see cref="Layouts"/> collection or the
        /// <see cref="ActiveLayout"/> changes.
        /// </remarks>
        private void UpdateWindowLayoutItem()
        {
            ((WindowLayoutItem)CommandItems["WindowLayout"]).Update();
        }


        internal void InitializeLayouts()
        {
            Logger.Info("Loading window layouts.");

            Layouts.Clear();

            try
            {
                // Load factory presets.
                string applicationFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.None);
                string layoutsFolder = Path.Combine(applicationFolder, LayoutsFolder, PresetsFolder);
                Layouts.AddRange(Directory.EnumerateFiles(layoutsFolder, LayoutPattern)
                                          .Select(Path.GetFileNameWithoutExtension)
                                          .Select(name => new WindowLayout(name, true)));
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to load factory presets.");
            }

            try
            {
                // Load user presets.
                string applicationFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
                string layoutsFolder = Path.Combine(applicationFolder, LayoutsFolder, PresetsFolder);
                Layouts.AddRange(Directory.EnumerateFiles(layoutsFolder, LayoutPattern)
                                          .Select(Path.GetFileNameWithoutExtension)
                                          .Select(name => new WindowLayout(name, false)));
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to load user presets.");
            }

            // Add a dummy entry, if no presets are available.
            if (Layouts.Count == 0)
                Layouts.Add(new WindowLayout(DefaultLayout, true));

            // The window layout will be loaded in OnEditorActivated().
            ActiveLayout = null;
        }


        private void UninitializeLayouts()
        {
            Layouts.Clear();
            ActiveLayout = null;
            UpdateWindowLayoutItem();
        }


        internal void SwitchLayout(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            Logger.Info("Switching to window layout \"{0}\".", layout.Name);

            try
            {
                // Save current window layout to memory.
                if (ActiveLayout != null)
                {
                    ActiveLayout.SerializedLayout = Editor.SaveLayout();
                    ActiveLayout.IsDirty = true;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to store active window layout to memory.");
            }

            ActiveLayout = layout;
            UpdateWindowLayoutItem();

            try
            {
                // Try to load window layout from memory.
                if (layout.SerializedLayout != null)
                {
                    Editor.LoadLayout(layout.SerializedLayout);
                    return;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to restore window layout from memory.");
            }

            try
            {
                // Try to load user session.
                LoadUserSession(layout);
                return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to restore window layout from session data.");
            }

            try
            {
                // Try to load factory/user preset.
                LoadPreset(layout);
                return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to restore window layout from preset.");
            }
        }


        private void LoadUserSession(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            // Load window layout from "%LOCALAPPDATA%\Layouts\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string fileName = Path.Combine(userSettingsFolder, LayoutsFolder, $"{layout.Name}.xml");
            layout.SerializedLayout = XDocument.Load(fileName).Root;
            Editor.LoadLayout(layout.SerializedLayout);
            layout.IsDirty = false;
        }


        private void LoadPreset(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            string layoutFolder;
            if (layout.IsFactoryPreset)
            {
                // Load window layout from "<EXECUTABLE_FOLDER>\Layouts\Presets\*.xml".
                string applicationFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.None);
                layoutFolder = Path.Combine(applicationFolder, LayoutsFolder, PresetsFolder);
            }
            else
            {
                // Load window layout from "%LOCALAPPDATA%\Layouts\Presets\*.xml".
                string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
                layoutFolder = Path.Combine(userSettingsFolder, LayoutsFolder, PresetsFolder);
            }

            string fileName = Path.Combine(layoutFolder, $"{layout.Name}.xml");
            layout.SerializedLayout = XDocument.Load(fileName).Root;
            Editor.LoadLayout(layout.SerializedLayout);
            layout.IsDirty = false;
        }


        private void SaveLayouts()
        {
            Logger.Info("Saving window layouts.");

            if (ActiveLayout != null)
            {
                // Remember active window layout.
                Settings.Default.ActiveWindowLayout = ActiveLayout.Name;

                try
                {
                    // Save active window layout.
                    ActiveLayout.SerializedLayout = Editor.SaveLayout();
                    SaveUserSession(ActiveLayout);
                    ActiveLayout.IsDirty = false;
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception, "Failed to save window layout \"{0}\".", ActiveLayout.Name);
                }
            }

            foreach (var layout in Layouts)
            {
                if (!layout.IsDirty)
                    continue;

                try
                {
                    SaveUserSession(layout);
                    layout.IsDirty = false;
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception, "Failed to save window layout \"{0}\".", layout.Name);
                }
            }
        }


        private static void SaveUserSession(WindowLayout layout)
        {
            Debug.Assert(layout != null);
            Debug.Assert(layout.SerializedLayout != null);

            // Save window layout to "%LOCALAPPDATA%\Layouts\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string layoutFolder = Path.Combine(userSettingsFolder, LayoutsFolder);
            string fileName = Path.Combine(layoutFolder, $"{layout.Name}.xml");

            if (!Directory.Exists(layoutFolder))
                Directory.CreateDirectory(layoutFolder);

            new XDocument(layout.SerializedLayout).Save(fileName);
        }


        private static void SaveUserPreset(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            // Save window layout to "%LOCALAPPDATA%\Layouts\Presets\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string layoutFolder = Path.Combine(userSettingsFolder, LayoutsFolder, PresetsFolder);
            string fileName = Path.Combine(layoutFolder, $"{layout.Name}.xml");

            if (!Directory.Exists(layoutFolder))
                Directory.CreateDirectory(layoutFolder);

            new XDocument(layout.SerializedLayout).Save(fileName);
        }


        private bool CanResetWindowLayout()
        {
            return ActiveLayout != null;
        }


        private void ResetWindowLayout()
        {
            if (CanResetWindowLayout())
                ResetWindowLayout(ActiveLayout);
        }


        private void ResetWindowLayout(WindowLayout layout)
        {
            Logger.Info("Resetting window layout \"{0}\".", layout.Name);

            // Delete session in memory.
            layout.SerializedLayout = null;
            layout.IsDirty = false;

            try
            {
                // Delete session on disk.
                DeleteUserSession(layout);

                // Restore original preset.
                LoadPreset(layout);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, $"Failed to restore preset \"{0}\".", layout.Name);
            }
        }


        private bool CanSavePreset()
        {
            return ActiveLayout != null && !ActiveLayout.IsFactoryPreset;
        }


        private void SavePreset()
        {
            if (!CanSavePreset())
                return;

            try
            {
                // Save window layout as new preset.
                ActiveLayout.SerializedLayout = Editor.SaveLayout(true);
                SaveUserPreset(ActiveLayout);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Could not save window layout \"{0}\".", ActiveLayout.Name);

                string message = $"Could not save window layout \"{ActiveLayout.Name}\".\n\n{exception.Message}";
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SavePresetAs()
        {
            Logger.Info("Saving window layout as new preset.");

            ShowSaveLayoutDialog:
            var saveLayoutDialog = new SaveLayoutViewModel
            {
                DisplayName = "Save Window Layout",
                LayoutName = "New layout"
            };

            string layoutName = null;
            WindowLayout existingLayout = null;
            var result = _windowService.ShowDialog(saveLayoutDialog);
            if (result.HasValue && result.Value)
            {
                Debug.Assert(!string.IsNullOrEmpty(saveLayoutDialog.LayoutName), "The layout name must not be null or empty.");
                Debug.Assert(saveLayoutDialog.LayoutName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1, "The layout name must not contain invalid characters.");

                layoutName = saveLayoutDialog.LayoutName;

                // Overwrite existing window layout?
                existingLayout = Layouts.FirstOrDefault(l => string.Compare(l.Name, layoutName, StringComparison.OrdinalIgnoreCase) == 0);
                if (existingLayout != null)
                {
                    if (existingLayout.IsFactoryPreset)
                    {
                        MessageBox.Show(
                            $"\"{layoutName}\" is a factory preset. Factory presets cannot be overwritten.",
                            Editor.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                        // Try again.
                        goto ShowSaveLayoutDialog;
                    }
                    else
                    {
                        var messageBoxResult = MessageBox.Show(
                            $"The layout \"{layoutName}\" already exists. Overwrite existing?",
                            Editor.ApplicationName, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                        if (messageBoxResult == MessageBoxResult.No)
                        {
                            // Try again.
                            goto ShowSaveLayoutDialog;
                        }

                        if (messageBoxResult == MessageBoxResult.Cancel)
                        {
                            // Abort.
                            layoutName = null;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(layoutName))
            {
                try
                {
                    // Save window layout as new preset.
                    var serializedLayout = Editor.SaveLayout(true);
                    var layout = new WindowLayout(layoutName, false) { SerializedLayout = serializedLayout };
                    SaveUserPreset(layout);

                    if (existingLayout != null)
                        Layouts.Remove(existingLayout);

                    Layouts.Add(layout);
                    ActiveLayout = layout;
                    UpdateWindowLayoutItem();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Could not save window layout as new preset \"{0}\".", layoutName);

                    string message = $"Could not save window layout as new preset \"{layoutName}\".\n\n{exception.Message}";
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        internal void RenameWindowLayout(WindowLayout layout)
        {
            Debug.Assert(layout != null);
            Debug.Assert(Layouts.Contains(layout));

            string oldLayoutName = layout.Name;

            Logger.Info("Renaming window layout \"{0}\".", oldLayoutName);

            ShowSaveLayoutDialog:
            var saveLayoutDialog = new SaveLayoutViewModel
            {
                DisplayName = "Rename Window Layout",
                LayoutName = oldLayoutName
            };

            string newLayoutName = null;
            WindowLayout existingLayout = null;
            var result = _windowService.ShowDialog(saveLayoutDialog);
            if (result.HasValue && result.Value && saveLayoutDialog.LayoutName != oldLayoutName)
            {
                Debug.Assert(!string.IsNullOrEmpty(saveLayoutDialog.LayoutName), "The layout name must not be null or empty.");
                Debug.Assert(saveLayoutDialog.LayoutName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1, "The layout name must not contain invalid characters.");

                newLayoutName = saveLayoutDialog.LayoutName;

                // Overwrite existing file?
                existingLayout = Layouts.FirstOrDefault(l => l.Name == newLayoutName);
                if (existingLayout != null)
                {
                    if (existingLayout.IsFactoryPreset)
                    {
                        MessageBox.Show(
                            $"\"{newLayoutName}\" is a factory preset. Factory presets cannot be overwritten.",
                            Editor.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                        // Try again.
                        goto ShowSaveLayoutDialog;
                    }
                    else
                    {
                        var messageBoxResult = MessageBox.Show(
                            $"The layout \"{newLayoutName}\" already exists. Overwrite existing?",
                            Editor.ApplicationName, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                        if (messageBoxResult == MessageBoxResult.No)
                        {
                            // Try again.
                            goto ShowSaveLayoutDialog;
                        }

                        if (messageBoxResult == MessageBoxResult.Cancel)
                        {
                            // Abort.
                            newLayoutName = null;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(newLayoutName))
            {
                try
                {
                    // Rename window layout.
                    RenameUserSession(layout.Name, newLayoutName);
                    RenameUserPreset(layout.Name, newLayoutName);

                    if (existingLayout != null)
                        Layouts.Remove(existingLayout);

                    layout.Name = newLayoutName;
                    UpdateWindowLayoutItem();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Could not rename window layout (old name: \"{0}\", new name: \"{1}\").", oldLayoutName, newLayoutName);

                    string message = $"Could not rename window layout.\n\n{exception.Message}";
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private static void RenameUserSession(string oldLayoutName, string newLayoutName)
        {
            if (oldLayoutName == newLayoutName)
                return;

            // Rename window layout in "%LOCALAPPDATA%\Layouts\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string oldFileName = Path.Combine(userSettingsFolder, LayoutsFolder, $"{oldLayoutName}.xml");
            string newFileName = Path.Combine(userSettingsFolder, LayoutsFolder, $"{newLayoutName}.xml");

            File.Delete(newFileName);
            if (File.Exists(oldFileName))   // Session layout file may not yet exist!
                File.Move(oldFileName, newFileName);
        }


        private static void RenameUserPreset(string oldLayoutName, string newLayoutName)
        {
            if (oldLayoutName == newLayoutName)
                return;

            // Rename window layout in "%LOCALAPPDATA%\Layouts\Presets\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string oldFileName = Path.Combine(userSettingsFolder, LayoutsFolder, PresetsFolder, $"{oldLayoutName}.xml");
            string newFileName = Path.Combine(userSettingsFolder, LayoutsFolder, PresetsFolder, $"{newLayoutName}.xml");

            File.Delete(newFileName);
            File.Move(oldFileName, newFileName);
        }


        internal void Delete(WindowLayout layout)
        {
            Debug.Assert(layout != null);
            Debug.Assert(layout != ActiveLayout, "Cannot delete active window layout.");

            Logger.Info("Deleting window layout \"{0}\".", layout.Name);

            Layouts.Remove(layout);
            UpdateWindowLayoutItem();

            try
            {
                DeleteUserSession(layout);
                DeleteUserPreset(layout);
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to delete window layout \"{0}\".", layout.Name);
            }
        }


        private static void DeleteUserSession(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            // Delete window layout from "%LOCALAPPDATA%\Layouts\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string fileName = Path.Combine(userSettingsFolder, LayoutsFolder, $"{layout.Name}.xml");
            File.Delete(fileName);
        }


        private static void DeleteUserPreset(WindowLayout layout)
        {
            Debug.Assert(layout != null);

            // Delete window layout from "%LOCALAPPDATA%\Layouts\Presets\*.xml".
            string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
            string fileName = Path.Combine(userSettingsFolder, LayoutsFolder, PresetsFolder, $"{layout.Name}.xml");
            File.Delete(fileName);
        }


        /// <summary>
        /// Shows the "Manage Layouts" dialog.
        /// </summary>
        private void ManageWindowLayouts()
        {
            Logger.Info("Showing Manage Layouts dialog.");

            _windowService.ShowDialog(new ManageLayoutsViewModel(this));
        }
        #endregion
    }
}
