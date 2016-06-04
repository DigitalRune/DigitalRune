// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Linq;
using System.Text;
using DigitalRune.Collections;
using DigitalRune.Editor.QuickLaunch;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Options
{
    partial class OptionsExtension
    {
        private void OnEditorActivated(object sender, ActivationEventArgs eventArgs)
        {
            Editor.Activated -= OnEditorActivated;
            AddQuickLaunchItems();
        }


        /// <summary>
        /// Adds the Quick Launch items for accessing options pages.
        /// </summary>
        private void AddQuickLaunchItems()
        {
            var quickLaunchService = Editor.Services.GetInstance<IQuickLaunchService>();
            if (quickLaunchService == null)
                return;

            var command = new DelegateCommand<MergeableNode<OptionsPageViewModel>>(Show);
            var quickLaunchItems = Options.OptionsNodes
                                          .SelectMany(optionsNode => optionsNode.GetSubtree(true))
                                          .Select(optionsNode => new QuickLaunchItem
                                          {
                                              Icon = MultiColorGlyphs.Options,
                                              Title = GetTitle(optionsNode),
                                              Command = command,
                                              CommandParameter = optionsNode,
                                              Tag = this
                                          });

            quickLaunchService.Items.AddRange(quickLaunchItems);
        }


        /// <summary>
        /// Gets the title of the Quick Launch item for the specified options node.
        /// </summary>
        /// <param name="optionsNode">The options node.</param>
        /// <returns>The title of the Quick Launch item.</returns>
        private static string GetTitle(MergeableNode<OptionsPageViewModel> optionsNode)
        {
            var stringBuilder = new StringBuilder("Options → ");

            foreach (var node in optionsNode.GetAncestors().Reverse())
            {
                if (node.Content != null)
                {
                    stringBuilder.Append(node.Content.DisplayName);
                    stringBuilder.Append(" → ");
                }
            }

            stringBuilder.Append(optionsNode.Content.DisplayName);
            return stringBuilder.ToString();
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
