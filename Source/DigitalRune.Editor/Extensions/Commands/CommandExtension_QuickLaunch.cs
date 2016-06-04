// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Editor.QuickLaunch;


namespace DigitalRune.Editor.Commands
{
    partial class CommandExtension
    {
        /// <summary>
        /// Adds the Quick Launch items for accessing commands.
        /// </summary>
        private void AddQuickLaunchItems()
        {
            var quickLaunchService = Editor.Services.GetInstance<IQuickLaunchService>();
            if (quickLaunchService == null)
                return;

            var editorViewModel = Editor as EditorViewModel;
            if (editorViewModel == null)
                return;

            var quickLaunchItems = new List<QuickLaunchItem>();
            foreach (var menuItem in editorViewModel.MenuManager.Menu)
                CreateQuickLaunchItems(quickLaunchItems, menuItem, string.Empty);

            // Sort using OrderBy (stable sort).
            quickLaunchService.Items.AddRange(quickLaunchItems.OrderBy(item => item.Title));

        }


        private static KeyGesture GetKeyGesture(CommandItem item)
        {
            var inputGestures = item?.InputGestures;
            if (inputGestures != null && inputGestures.Count != 0)
                return inputGestures[0] as KeyGesture;

            return null;
        }


        private void CreateQuickLaunchItems(List<QuickLaunchItem> quickLaunchItems, MenuItemViewModel menuItem, string prefix)
        {
            Debug.Assert(quickLaunchItems != null);
            Debug.Assert(prefix != null);

            if (menuItem == null)
                return;

            var commandItem = menuItem.CommandItem as CommandItem;
            if (commandItem != null && !string.IsNullOrEmpty(commandItem.Text) && commandItem.Command != null)
            {
                quickLaunchItems.Add(new QuickLaunchItem
                {
                    Icon = commandItem.Icon,
                    Title = $"{prefix}{EditorHelper.FilterAccessKeys(commandItem.Text)}",
                    KeyGesture = GetKeyGesture(commandItem),
                    Description = commandItem.ToolTip,
                    Command = commandItem.Command,
                    CommandParameter = commandItem.CommandParameter,
                    Tag = this
                });
            }

            if (menuItem.Submenu != null && menuItem.Submenu.Count > 0)
            {
                if (!string.IsNullOrEmpty(menuItem.CommandItem.Text))
                    prefix = $"{prefix}{EditorHelper.FilterAccessKeys(menuItem.CommandItem.Text)} → ";

                foreach (var subMenuItem in menuItem.Submenu)
                    CreateQuickLaunchItems(quickLaunchItems, subMenuItem, prefix);
            }
        }


        /// <summary>
        /// Removes the Quick Launch items for accessing options pages.
        /// </summary>
        private void RemoveQuickLaunchItems()
        {
            var quickLaunchService = Editor.Services.GetInstance<IQuickLaunchService>();
            if (quickLaunchService == null)
                return;

            var items = quickLaunchService.Items
                                          .Where(item => item.Tag == this)
                                          .Reverse() // Reverse for more efficient removal.
                                          .ToArray();
            foreach (var item in items)
                quickLaunchService.Items.Remove(item);
        }
    }
}
