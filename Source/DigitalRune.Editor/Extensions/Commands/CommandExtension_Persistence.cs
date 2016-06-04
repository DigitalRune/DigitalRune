// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static System.FormattableString;


namespace DigitalRune.Editor.Commands
{
    partial class CommandExtension
    {
        private const string ToolBarStatesFile = "Toolbars.xml";


        /// <summary>
        /// Loads the toolbar states from file.
        /// </summary>
        private void LoadToolBarStates()
        {
            Logger.Debug("Loading toolbar layout.");

            XDocument toolBarStates = null;
            try
            {
                string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
                string fileName = Path.Combine(userSettingsFolder, ToolBarStatesFile);
                toolBarStates = XDocument.Load(fileName);
            }
            catch (Exception exception)
            {
                string message = Invariant($"Unable to restore toolbar states. Could not load \"{ToolBarStatesFile}\".");
                Logger.Warn(exception, message);
            }

            if (toolBarStates == null)
                return;

            try
            {
                var trayNode = toolBarStates.Element("ToolBarTray");
                if (trayNode == null)
                    return;

                var toolBars = Editor.ToolBars;
                foreach (var barNode in trayNode.Elements("ToolBar"))
                {
                    var name = (string)barNode.Attribute("Name");
                    var toolBar = toolBars.FirstOrDefault(tb => tb.CommandGroup.Name == name);
                    if (toolBar == null)
                        continue;

                    toolBar.Band = (int)barNode.Attribute("Band");
                    toolBar.BandIndex = (int)barNode.Attribute("BandIndex");
                    if ((bool)barNode.Attribute("IsVisible"))
                        toolBar.IsVisible = true;
                    else
                        toolBar.IsVisible = false;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Unable to restore toolbar states.");
            }
        }


        /// <summary>
        /// Saves the toolbar states in a file.
        /// </summary>
        private void SaveToolBarStates()
        {
            Logger.Debug("Saving toolbar layout.");
            try
            {
                // Save current toolbar layout.
                XElement trayNode = new XElement("ToolBarTray");
                foreach (var toolBar in Editor.ToolBars)
                {
                    var barNode = new XElement("ToolBar");
                    barNode.Add(new XAttribute("Name", toolBar.CommandGroup.Name));
                    barNode.Add(new XAttribute("Band", toolBar.Band));
                    barNode.Add(new XAttribute("BandIndex", toolBar.BandIndex));
                    barNode.Add(new XAttribute("IsVisible", toolBar.IsVisible));
                    trayNode.Add(barNode);
                }

                XDocument toolBarStates = new XDocument(trayNode);
                string userSettingsFolder = EditorHelper.GetUserSettingsFolder(ConfigurationUserLevel.PerUserRoamingAndLocal);
                string fileName = Path.Combine(userSettingsFolder, ToolBarStatesFile);
                toolBarStates.Save(fileName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Unable to store toolbar states.");
            }
        }
    }
}
