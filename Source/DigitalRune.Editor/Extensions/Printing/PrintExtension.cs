// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Printing
{
    /// <summary>
    /// Provides commands and services for printing.
    /// </summary>
    public sealed class PrintExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.RegisterView(typeof(IPrintDocumentProvider), typeof(PrintPreviewWindow));
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddCommands();
            AddMenus();
            AddToolBars();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.UnregisterView(typeof(IPrintDocumentProvider));
        }


        private void AddCommands()
        {
            CommandItems.Add(
                new RoutedCommandItem(ApplicationCommands.PrintPreview)
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.PrintPreview,
                    Text = "Print pre_view...",
                    ToolTip = "Show the print preview.",
                },
                new RoutedCommandItem(ApplicationCommands.Print)
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Print,
                    Text = "_Print...",
                    ToolTip = "Print the document.",
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            var mergeBeforePrintSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "PrintSeparator"), MergePoint.Append };
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("FileGroup", "_File"),
                    new MergeableNode<ICommandItem>(CommandItems["PrintPreview"], mergeBeforePrintSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Print"], mergeBeforePrintSeparator))
            };
            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private void AddToolBars()
        {
            var mergeBeforePrintSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "PrintSeparator"), MergePoint.Append };
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("StandardGroup", "Standard"),
                    new MergeableNode<ICommandItem>(CommandItems["PrintPreview"], mergeBeforePrintSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Print"], mergeBeforePrintSeparator))
            };
            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }
        #endregion
    }
}
