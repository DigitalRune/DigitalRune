// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Linq;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private CommandBinding _openCommandBinding;
        private bool _suppressUpdateCommandItems;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void AddCommands()
        {
            CommandItems.Add(
                new NewDocumentItem(this),
                new DelegateCommandItem("Open", new DelegateCommand(() => OpenAsync().Forget(), CanOpen))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Open,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) },
                    Text = "_Open",
                    ToolTip = "Open document."
                },
                new DelegateCommandItem("Reload", new DelegateCommand(Reload, CanReload))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Reload,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) },
                    Text = "_Reload",
                    ToolTip = "Reload document."
                },
                new DelegateCommandItem("Close", new DelegateCommand(Close, HasActiveDocument))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Close,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F4, ModifierKeys.Control) },
                    Text = "_Close",
                    ToolTip = "Close the document."
                },
                new DelegateCommandItem("CloseAll", new DelegateCommand(() => CloseAllAsync().Forget(), HasActiveDocument))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.CloseAll,
                    Text = "Close all",
                    ToolTip = "Close all documents."
                },
                new DelegateCommandItem("CloseAllButThis", new DelegateCommand(() => CloseAllButActiveAsync().Forget(), CanCloseAllButActiveDocument))
                {
                    Category = CommandCategories.File,
                    Text = "Close all but this",
                    ToolTip = "Close all open documents except the current."
                },
                new DelegateCommandItem("CloseAllDocumentsAndWindows", new DelegateCommand(() => CloseAllDocumentsAndWindowsAsync().Forget(), CanCloseAllDocumentsAndWindows))
                {
                    Category = CommandCategories.Window,
                    Text = "Close all",
                    ToolTip = "Close all documents and tool windows."
                },
                new DelegateCommandItem("Save", new DelegateCommand(SaveActiveDocument, CanSave))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Save,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) },
                    Text = "_Save",
                    ToolTip = "Save the document.",
                },
                new DelegateCommandItem("SaveAs", new DelegateCommand(SaveAs, CanSave))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.SaveAs,
                    Text = "Save _as...",
                    ToolTip = "Save the document under a new filename.",
                },
                new DelegateCommandItem("SaveAll", new DelegateCommand(() => SaveAllAsync().Forget(), CanSaveAll))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.SaveAll,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift) },
                    Text = "Save a_ll",
                    ToolTip = "Save all documents."
                },
                new RecentDocumentsItem(this),
                new DelegateCommandItem("CopyFullPath", new DelegateCommand(CopyUri, HasActiveDocument))
                {
                    Category = CommandCategories.File,
                    Text = "Copy full path",
                    ToolTip = "Copy full path and name of the document."
                },
                new DelegateCommandItem("OpenContainingFolder", new DelegateCommand(OpenContainingFolder, CanOpenContainingFolder))
                {
                    Category = CommandCategories.File,
                    Text = "Open containing folder",
                    ToolTip = "Open the folder containing the document in the Windows Explorer."
                },
                new DelegateCommandItem("NewWindow", new DelegateCommand(AddView, IsDocumentFocused))
                {
                    Category = CommandCategories.Window,
                    Icon = MultiColorGlyphs.NewWindow,
                    Text = "_New window",
                    ToolTip = "Open a new window for the current document.",
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            var insertBeforeOpenSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "OpenSeparator"), MergePoint.Append };
            var insertBeforeCloseSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "CloseSeparator"), MergePoint.Append };
            var insertBeforeSaveSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "SaveSeparator"), MergePoint.Append };
            var insertBeforeExit = new[] { new MergePoint(MergeOperation.InsertBefore, "Exit"), MergePoint.Append };
            var insertBeforeRecentFileSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "RecentFilesSeparator"), MergePoint.Append };
            var insertBeforeWindowSpecificSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "WindowSpecificSeparator"), MergePoint.Append };

            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                // File menu
                new MergeableNode<ICommandItem>(new CommandGroup("FileGroup", "_File"), insertBeforeOpenSeparator,
                    new MergeableNode<ICommandItem>(CommandItems["New"], insertBeforeOpenSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Open"], insertBeforeOpenSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Reload"], insertBeforeOpenSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Close"], insertBeforeCloseSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["CloseAll"], insertBeforeCloseSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Save"], insertBeforeSaveSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["SaveAs"], insertBeforeSaveSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["SaveAll"], insertBeforeSaveSeparator),
                    new MergeableNode<ICommandItem>(new CommandSeparator("RecentFilesSeparator"), insertBeforeExit),
                    new MergeableNode<ICommandItem>(CommandItems["RecentFiles"], insertBeforeRecentFileSeparator)),

                // Window menu
                new MergeableNode<ICommandItem>(new CommandGroup("WindowGroup", "_Window"),
                    new MergeableNode<ICommandItem>(CommandItems["NewWindow"], insertBeforeWindowSpecificSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["CloseAllDocumentsAndWindows"], insertBeforeCloseSeparator)),
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
            var insertBeforeFileSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "FileSeparator"), MergePoint.Append };

            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("StandardGroup", "Standard"),
                    new MergeableNode<ICommandItem>(CommandItems["New"], insertBeforeFileSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Open"], insertBeforeFileSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Save"], insertBeforeFileSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["SaveAll"], insertBeforeFileSeparator)),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        private void AddCommandBindings()
        {
            _openCommandBinding = new CommandBinding(
                ApplicationCommands.Open, 
                (s, e) => Open(new Uri((string)e.Parameter)));
            Editor.Window.CommandBindings.Add(_openCommandBinding);
        }


        private void RemoveCommandBindings()
        {
            Editor.Window?.CommandBindings.Remove(_openCommandBinding);
            _openCommandBinding = null;
        }


        private bool HasActiveDocument()
        {
            return ActiveDocument != null;
        }


        private bool IsDocumentFocused()
        {
            return Editor.ActiveDockTabItem is DocumentViewModel;
        }


        private bool CanOpen()
        {
            return Factories.SelectMany(documentHandler => documentHandler.DocumentTypes)
                            .Any(documentType => documentType.IsLoadable);
        }


        private bool CanReload()
        {
            return ActiveDocument != null && !ActiveDocument.IsUntitled;
        }


        private bool CanCloseAllButActiveDocument()
        {
            return ActiveDocument != null && _documents.Count > 1;
        }


        private bool CanSave()
        {
            return ActiveDocument != null && ActiveDocument.DocumentType.IsSavable;
        }


        private bool CanSaveAll()
        {
            return _documents.Any(document => document.DocumentType.IsSavable);
        }


        private bool CanCloseAllDocumentsAndWindows()
        {
            return Editor.Items.Any();
        }


        private void CopyUri()
        {
            Logger.Debug("Copying file URI to clipboard.");

            var document = ActiveDocument;
            if (document != null)
            {
                string uri = document.GetName();
                Logger.Info(CultureInfo.InvariantCulture, "File URI: {0}", uri);
                Clipboard.SetData(DataFormats.UnicodeText, uri);
            }
        }


        private bool CanOpenContainingFolder()
        {
            return ActiveDocument != null && !ActiveDocument.IsUntitled;
        }


        private void OpenContainingFolder()
        {
            Logger.Info("Opening containing folder.");

            string fileName = ActiveDocument?.Uri?.LocalPath;
            if (!string.IsNullOrEmpty(fileName))
            {
                string folder = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(folder))
                {
                    Logger.Info("Folder: {0}", folder);
                    Process.Start(folder);
                }
                else
                {
                    Logger.Info("Folder: -");
                }
            }
        }


        private void AddView()
        {
            var document = ActiveDocument;
            if (document != null)
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Adding new view for \"{0}\".", document.GetName());
                var viewModel = document.CreateViewModel();
                Editor.ActivateItem(viewModel);
            }
        }


        private void UpdateCommands()
        {
            if (_suppressUpdateCommandItems)
                return;

            // Invalidate all DelegateCommands.
            CommandItems.OfType<DelegateCommandItem>()
                        .Select(item => item.Command)
                        .ForEach(command => command.RaiseCanExecuteChanged());
        }
        #endregion
    }
}
