// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Provides functions for logging and displaying output messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IOutputService"/></item>
    /// </list>
    /// </remarks>
    public sealed class OutputExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ResourceDictionary _resourceDictionary;
        private OutputViewModel _output;
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
            // Register NLogTarget, if available. (Used in OutputViewModel.)
            var nlogTarget = LogManager.Configuration.AllTargets.OfType<INLogTarget>().FirstOrDefault();
            if (nlogTarget != null)
                Editor.Services.Register(typeof(INLogTarget), null, nlogTarget);

            _output = new OutputViewModel(Editor);
            Editor.Services.Register(typeof(IOutputService), null, _output);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
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
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            _output = null;
            Editor.Services.Unregister(typeof(IOutputService));
            Editor.Services.Unregister(typeof(INLogTarget));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Output/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
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
                new DelegateCommandItem("Output", new DelegateCommand(() => _output.Show()))
                {
                    Category = "View",
                    Icon = MultiColorGlyphs.Output,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Alt) },
                    Text = "_Output",
                    ToolTip = "Show the Output window.",
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
                    new MergeableNode<ICommandItem>(CommandItems["Output"], insertBeforeWindowSeparator, MergePoint.Append))
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
                    new MergeableNode<ICommandItem>(CommandItems["Output"])),
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
            if (dockId == OutputViewModel.DockIdString)
                return _output;

            return null;
        }
        #endregion
    }
}
