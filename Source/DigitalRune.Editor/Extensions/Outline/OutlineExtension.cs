// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Outlines
{
    /// <summary>
    /// Provides the Outline window for browsing object hierarchies.
    /// </summary>
    public sealed class OutlineExtension : EditorExtension, IOutlineService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private Lazy<OutlineViewModel> _outlineViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        EditorDockTabItemViewModel IOutlineService.OutlineViewModel
        {
            get { return _outlineViewModel.Value; }
        }


        /// <inheritdoc/>
        Outline IOutlineService.Outline
        {
            get { return _outlineViewModel.Value.Outline; }
            set { _outlineViewModel.Value.Outline = value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OutlineExtension"/> class.
        /// </summary>
        public OutlineExtension()
        {
            Logger.Debug("Initializing OutlineExtension.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IOutlineService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _outlineViewModel = new Lazy<OutlineViewModel>(() => new OutlineViewModel(Editor));
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
            _outlineViewModel = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IOutlineService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Outline/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
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
                new DelegateCommandItem("Outline", new DelegateCommand(ShowOutline))
                {
                    Category = CommandCategories.View,
                    Icon = MultiColorGlyphs.Outline,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Alt) },
                    Text = "_Outline",
                    ToolTip = "Show the Outline window.",
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            var insertBeforeWindowSeparator = new MergePoint(MergeOperation.InsertBefore, "WindowSeparator");

            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup", "_View"),
                    new MergeableNode<ICommandItem>(CommandItems["Outline"], insertBeforeWindowSeparator, MergePoint.Append)),
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
                    new MergeableNode<ICommandItem>(CommandItems["Outline"])),
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
            if (dockId == OutlineViewModel.DockIdString)
                return _outlineViewModel.Value;

            return null;
        }


        private void ShowOutline()
        {
            Logger.Info("Showing Outline window.");

            Editor.ActivateItem(_outlineViewModel.Value);
        }
        #endregion
    }
}
