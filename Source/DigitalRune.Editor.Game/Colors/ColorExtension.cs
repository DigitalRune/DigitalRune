// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Colors
{
    /// <summary>
    /// Provides tools for working with colors.
    /// </summary>
    public sealed class ColorExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private ColorViewModel _colorViewModel;
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
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _colorViewModel = new ColorViewModel(Editor);
            AddDataTemplates();
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
            RemoveDataTemplates();
            _colorViewModel = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor.Game;component/Colors/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddCommands()
        {
            CommandItems.Add(
                new DelegateCommandItem("ColorPicker", new DelegateCommand(ShowColorPicker))
                {
                    Category = CommandCategories.View,
                    Icon = MultiColorGlyphs.ColorPalette,
                    Text = "_Color picker",
                    ToolTip = "Show the Color Picker.",
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
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup", "_View"),
                    new MergeableNode<ICommandItem>(CommandItems["ColorPicker"], new MergePoint(MergeOperation.InsertBefore, "WindowSeparator"), new MergePoint(MergeOperation.Append)))
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
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup"),
                    new MergeableNode<ICommandItem>(CommandItems["ColorPicker"])),
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
            if (dockId == ColorViewModel.DockIdString)
                return _colorViewModel;

            return null;
        }


        private void ShowColorPicker()
        {
            Logger.Debug("Showing Color Picker.");

            Editor.ActivateItem(_colorViewModel);
        }
        #endregion
    }
}
