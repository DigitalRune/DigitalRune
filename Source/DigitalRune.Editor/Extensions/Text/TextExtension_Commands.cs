// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor.Documents;
using DigitalRune.Windows.Themes;
using ICSharpCode.AvalonEdit;


namespace DigitalRune.Editor.Text
{
    partial class TextExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _showCommands = true;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private SyntaxHighlightingItem _syntaxHighlightingItem;
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void AddCommands()
        {
            // Add input gestures to routed commands. (Routed commands do not have a setter for InputGestures.)
            AvalonEditCommands.Comment.InputGestures.Add(new MultiKeyGesture(new[] { Key.K, Key.C }, ModifierKeys.Control));
            AvalonEditCommands.Uncomment.InputGestures.Add(new MultiKeyGesture(new[] { Key.K, Key.U }, ModifierKeys.Control));
            AvalonEditCommands.ConvertToUppercase.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control | ModifierKeys.Shift));
            AvalonEditCommands.ConvertToLowercase.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));

            // Add CommandItems.
            _syntaxHighlightingItem = new SyntaxHighlightingItem(this);

            CommandItems.AddRange(new ICommandItem[]
            {
                new RoutedCommandItem(AvalonEditCommands.PasteMultiple)
                {
                    Category = CommandCategories.Edit,
                    Text = "Paste multiple",
                    ToolTip = "Show the most recent entries in the clipboard."
                },
                new DelegateCommandItem("GoToLine", new DelegateCommand(GoToLineNumber, CanGoToLineNumber))
                {
                    Category = CommandCategories.Edit,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.G, ModifierKeys.Control) },
                    Text = "_Go to...",
                    ToolTip = "Go to line"
                },
                new RoutedCommandItem(AvalonEditCommands.RemoveLeadingWhitespace)
                {
                    Category = CommandCategories.Edit,
                    Text = "Remove leading white space"
                },
                new RoutedCommandItem(AvalonEditCommands.RemoveTrailingWhitespace)
                {
                    Category = CommandCategories.Edit,
                    Text = "Remove trailing white space"
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertToUppercase)
                {
                    Category = CommandCategories.Edit,
                    Text = "Make upper case",
                    ToolTip = "Converts the selected text to upper case."
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertToLowercase)
                {
                    Category = CommandCategories.Edit,
                    Text = "Make lower case",
                    ToolTip = "Converts the selected text to lower case."
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertToTitleCase)
                {
                    Category = CommandCategories.Edit,
                    Text = "Make title case",
                    ToolTip = "Converts the selected text to title case."
                },
                new RoutedCommandItem(AvalonEditCommands.InvertCase)
                {
                    Category = CommandCategories.Edit,
                    Text = "Invert case",
                    ToolTip = "Inverts the case in the selected text."
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertTabsToSpaces)
                {
                    Category = CommandCategories.Edit,
                    Text = "Tabs to spaces"
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertSpacesToTabs)
                {
                    Category = CommandCategories.Edit,
                    Text = "Spaces to tabs"
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertLeadingTabsToSpaces)
                {
                    Category = CommandCategories.Edit,
                    Text = "Leading tabs to spaces"
                },
                new RoutedCommandItem(AvalonEditCommands.ConvertLeadingSpacesToTabs)
                {
                    Category = CommandCategories.Edit,
                    Text = "Leading spaces to tabs"
                },
                new RoutedCommandItem(AvalonEditCommands.IndentSelection)
                {
                    Category = CommandCategories.Edit,
                    Text = "Indent selection"
                },
                new RoutedCommandItem(AvalonEditCommands.Comment)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Comment,
                    Text = "Comment selection",
                    ToolTip = "Comment out selected lines."
                },
                new RoutedCommandItem(AvalonEditCommands.Uncomment)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Uncomment,
                    Text = "Uncomment selection",
                    ToolTip = "Uncomment selected lines."
                },
                new RoutedCommandItem(AvalonEditCommands.ToggleFold)
                {
                    Category = CommandCategories.Edit,
                    Text = "Fold/unfold",
                    ToolTip = "Toggle the current fold."
                },
                new RoutedCommandItem(AvalonEditCommands.ToggleAllFolds)
                {
                    Category = CommandCategories.Edit,
                    Text = "Fold/unfold All",
                    ToolTip = "Toggle all folds (toggle outlining)."
                },
                _syntaxHighlightingItem,
            });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
            _syntaxHighlightingItem = null;
        }


        private void ShowCommands(bool show)
        {
            if (_showCommands == show)
                return;

            _showCommands = show;
            foreach (var commandItem in CommandItems)
                commandItem.IsVisible = show;
        }


        private void AddMenus()
        {
            var insertAfterPaste = new[] { new MergePoint(MergeOperation.InsertAfter, "Paste"), MergePoint.Append };
            var insertBeforeSearchSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "SearchSeparator"), MergePoint.Append };
            var insertBeforeDocumentSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "DocumentSeparator"), MergePoint.Append };

            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("EditGroup", "_Edit"),
                    new MergeableNode<ICommandItem>(CommandItems["PasteMultiple"], insertAfterPaste), 
                    new MergeableNode<ICommandItem>(CommandItems["GoToLine"], insertBeforeSearchSeparator),
                    CreateFormatMenu(),
                    CreateFoldingMenu()),
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup", "_View"),
                    new MergeableNode<ICommandItem>(CommandItems["SyntaxHighlighting"], insertBeforeDocumentSeparator)),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private MergeableNode<ICommandItem> CreateFormatMenu()
        {
            return new MergeableNode<ICommandItem>(new CommandGroup("Format"),
                new MergeableNode<ICommandItem>(CommandItems["RemoveLeadingWhitespace"]),
                new MergeableNode<ICommandItem>(CommandItems["RemoveTrailingWhitespace"]),
                new MergeableNode<ICommandItem>(new CommandSeparator("WhitespaceSeparator")),
                new MergeableNode<ICommandItem>(CommandItems["ConvertToUppercase"]),
                new MergeableNode<ICommandItem>(CommandItems["ConvertToLowercase"]),
                new MergeableNode<ICommandItem>(CommandItems["ConvertToTitleCase"]),
                new MergeableNode<ICommandItem>(CommandItems["InvertCase"]),
                new MergeableNode<ICommandItem>(new CommandSeparator("CaseSeparator")),
                new MergeableNode<ICommandItem>(CommandItems["ConvertTabsToSpaces"]),
                new MergeableNode<ICommandItem>(CommandItems["ConvertSpacesToTabs"]),
                new MergeableNode<ICommandItem>(CommandItems["ConvertLeadingTabsToSpaces"]),
                new MergeableNode<ICommandItem>(CommandItems["ConvertLeadingSpacesToTabs"]),
                new MergeableNode<ICommandItem>(new CommandSeparator("TabsSeparator")),
                new MergeableNode<ICommandItem>(CommandItems["Comment"]),
                new MergeableNode<ICommandItem>(CommandItems["Uncomment"]),
                new MergeableNode<ICommandItem>(CommandItems["IndentSelection"]));
        }


        private MergeableNode<ICommandItem> CreateFoldingMenu()
        {
            return new MergeableNode<ICommandItem>(new CommandGroup("Folding"),
                new MergeableNode<ICommandItem>(CommandItems["ToggleFold"]),
                new MergeableNode<ICommandItem>(CommandItems["ToggleAllFolds"]));
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private bool CanGoToLineNumber()
        {
            return _documentService.ActiveDocument is TextDocument;
        }


        private void GoToLineNumber()
        {
            var document = _documentService.ActiveDocument as TextDocument;
            if (document == null)
                return;

            Logger.Debug(CultureInfo.InvariantCulture, "Showing Go To Line dialog for \"{0}\".", document.GetName());

            var textEditor = document.GetLastActiveTextEditor();
            if (textEditor == null)
                return;

            var viewModel = new GoToLineViewModel
            {
                LineNumber = textEditor.TextArea.Caret.Line,
                NumberOfLines = Math.Max(1, document.AvalonEditDocument.LineCount),
            };
            var result = _windowService.ShowDialog(viewModel);
            if (result == true)
            {
                int lineNumber = viewModel.LineNumber;

                Logger.Debug("Jumping to line {0} in \"{1}\".", lineNumber, document.GetName());

                if (lineNumber < 1)
                    lineNumber = 1;
                else if (lineNumber > document.AvalonEditDocument.LineCount)
                    lineNumber = document.AvalonEditDocument.LineCount;

                textEditor.TextArea.Caret.Line = lineNumber;
                textEditor.TextArea.Caret.BringCaretToView();
            }
        }
        #endregion
    }
}
