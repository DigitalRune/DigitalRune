// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Properties
{
    /// <summary>
    /// Provides the Properties window for browsing object properties.
    /// </summary>
    public sealed class PropertiesExtension : EditorExtension, IPropertiesService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private CommandBinding _propertiesCommandBinding;

        private Lazy<PropertiesViewModel> _propertiesViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        EditorDockTabItemViewModel IPropertiesService.PropertiesViewModel
        {
            get { return _propertiesViewModel.Value; }
        }


        /// <inheritdoc/>
        IPropertySource IPropertiesService.PropertySource
        {
            get {  return _propertySource; }
            set
            {
                if (_propertySource == value)
                    return;

                if (value == null)
                    _propertiesViewModel.Value.Hide(_propertySource);

                _propertySource = value;

                if (value != null)
                    _propertiesViewModel.Value.Show(value, false);

            }
        }
        private IPropertySource _propertySource;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertiesExtension"/> class.
        /// </summary>
        public PropertiesExtension()
        {
            Logger.Debug("Initializing PropertiesExtension.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IPropertiesService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _propertiesViewModel = new Lazy<PropertiesViewModel>(() => new PropertiesViewModel(Editor));
            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddToolBars();

            // AddCommandBindings is called after window was loaded.
            Editor.Activated += OnEditorActivated;
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            Editor.Activated -= OnEditorActivated;

            RemoveCommandBindings();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();
            _propertiesViewModel = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IPropertiesService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Properties/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
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
                new DelegateCommandItem("Properties", new DelegateCommand(ShowProperties))
                {
                    Category = CommandCategories.View,
                    Icon = MultiColorGlyphs.Properties,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F4) },
                    Text = "_Properties",
                    ToolTip = "Show the Properties window.",
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
                    new MergeableNode<ICommandItem>(CommandItems["Properties"], insertBeforeWindowSeparator, MergePoint.Append)),
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
                    new MergeableNode<ICommandItem>(CommandItems["Properties"])),
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
            _propertiesCommandBinding = new CommandBinding(ApplicationCommands.Properties, (s, e) => ShowProperties());
            Editor.Window.CommandBindings.Add(_propertiesCommandBinding);
        }


        private void RemoveCommandBindings()
        {
            Editor.Window?.CommandBindings.Remove(_propertiesCommandBinding);
            _propertiesCommandBinding = null;
        }


        private void OnEditorActivated(object sender, ActivationEventArgs eventArgs)
        {
            AddCommandBindings();
            Editor.Activated -= OnEditorActivated;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            if (dockId == PropertiesViewModel.DockIdString)
                return _propertiesViewModel.Value;

            return null;
        }


        private void ShowProperties()
        {
            Logger.Info("Showing Properties window.");

            Editor.ActivateItem(_propertiesViewModel.Value);
        }


        ///// <inheritdoc/>
        //void IPropertiesService.Show(IPropertySource propertySource, bool keepHistory)
        //{
        //    _propertiesViewModel.Value.Show(propertySource, keepHistory);
        //}


        ///// <inheritdoc/>
        //void IPropertiesService.Hide(IPropertySource propertySource)
        //{
        //    _propertiesViewModel.Value.Hide(propertySource);
        //}
        #endregion
    }
}
