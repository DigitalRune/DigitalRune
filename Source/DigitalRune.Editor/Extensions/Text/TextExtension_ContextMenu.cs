// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Editor.Commands;


namespace DigitalRune.Editor.Text
{
    partial class TextExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly MenuManager _menuManager;
        private MergeableNodeCollection<ICommandItem> _contextMenuNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public ICollection<MergeableNodeCollection<ICommandItem>> ContextMenuNodeCollections
        {
            get { return _contextMenuNodeCollections; }
        }
        private readonly Collection<MergeableNodeCollection<ICommandItem>> _contextMenuNodeCollections = new Collection<MergeableNodeCollection<ICommandItem>>();


        /// <inheritdoc/>
        public MenuItemViewModelCollection ContextMenu
        {
            get { return _menuManager.Menu; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Creates the structure of the text document's context menu.
        /// </summary>
        private void AddContextMenu()
        {
            var commandExtension = Editor.Extensions.OfType<CommandExtension>().FirstOrDefault().WarnIfMissing();
            if (commandExtension == null)
            {
                _contextMenuNodes = new MergeableNodeCollection<ICommandItem>();
            }
            else
            {
                _contextMenuNodes = new MergeableNodeCollection<ICommandItem>
                {
                    new MergeableNode<ICommandItem>(commandExtension.CommandItems["Cut"]),
                    new MergeableNode<ICommandItem>(commandExtension.CommandItems["Copy"]),
                    new MergeableNode<ICommandItem>(commandExtension.CommandItems["Paste"]),
                    new MergeableNode<ICommandItem>(commandExtension.CommandItems["Delete"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("ClipboardSeparator")),
                    new MergeableNode<ICommandItem>(commandExtension.CommandItems["SelectAll"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("SelectSeparator")),
                    CreateFormatMenu(),
                    CreateFoldingMenu(),
                    new MergeableNode<ICommandItem>(new CommandSeparator("EditSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["SyntaxHighlighting"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("ViewSeparator"))
                };
            }

            ContextMenuNodeCollections.Add(_contextMenuNodes);
        }


        private void RemoveContextMenu()
        {
            ContextMenuNodeCollections.Remove(_contextMenuNodes);
            _contextMenuNodes = null;
        }


        private void UpdateContextMenu()
        {
            Logger.Debug("Updating text context menu.");
            _menuManager.Update(ContextMenuNodeCollections);
        }
        #endregion
    }
}
