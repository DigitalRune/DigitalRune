// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Errors
{
    /// <summary>
    /// Provides the Errors window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IErrorService"/></item>
    /// </list>
    /// </remarks>
    public sealed class ErrorExtension : EditorExtension, IErrorService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private Lazy<ErrorsViewModel> _errorsViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        ///// <inheritdoc/>
        //EditorDockTabItemViewModel IErrorService.ErrorsViewModel
        //{
        //    get { return _errorsViewModel.Value; }
        //}


        /// <inheritdoc/>
        ObservableCollection<Error> IErrorService.Errors
        {
            get { return _errorsViewModel.Value.Items; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorExtension"/> class.
        /// </summary>
        public ErrorExtension()
        {
            Logger.Debug("Initializing ErrorExtension.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IErrorService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _errorsViewModel = new Lazy<ErrorsViewModel>(() => new ErrorsViewModel(Editor));
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
            _errorsViewModel = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IErrorService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Errors/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
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
                new DelegateCommandItem("Errors", new DelegateCommand(ShowErrors))
                {
                    Category = CommandCategories.View,
                    Icon = MultiColorGlyphs.ErrorList,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control | ModifierKeys.Alt) },
                    Text = "_Errors",
                    ToolTip = "Show the Errors window.",
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
                    new MergeableNode<ICommandItem>(CommandItems["Errors"], insertBeforeWindowSeparator, MergePoint.Append)),
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
                    new MergeableNode<ICommandItem>(CommandItems["Errors"])),
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
            if (dockId == ErrorsViewModel.DockIdString)
                return _errorsViewModel.Value;

            return null;
        }


        /// <inheritdoc/>
        void IErrorService.Show()
        {
            ShowErrors();
        }


        private void ShowErrors()
        {
            Logger.Info("Showing Errors window.");

            Editor.ActivateItem(_errorsViewModel.Value);
        }
        #endregion
    }
}
