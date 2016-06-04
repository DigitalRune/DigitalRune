// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.QuickLaunch
{
    /// <summary>
    /// Adds a Quick Launch box to the caption bar which allows to execute editor commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IQuickLaunchService"/></item>
    /// </list>
    /// </remarks>
    public sealed class QuickLaunchExtension : EditorExtension, IQuickLaunchService
    {
        //--------------------------------------------------------------
        #region Contants
        //--------------------------------------------------------------

        internal const string FocusQuickLaunchMessage = "FocusQuickLaunch";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private QuickLaunchViewModel _quickLaunchViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public IList<QuickLaunchItem> Items { get; } = new List<QuickLaunchItem>();
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
            Editor.Services.Register(typeof(IQuickLaunchService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddCaptionBarItems();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveCaptionBarItems();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IQuickLaunchService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/QuickLaunch/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
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
                new DelegateCommandItem("QuickLaunch", new DelegateCommand(FocusQuickLaunchBox))
                {
                    Category = CommandCategories.Tools,
                    InputGestures = new InputGestureCollection(new []{ new KeyGesture(Key.Q, ModifierKeys.Control) }),
                    Text="_Quick launch",
                    ToolTip="Go to the Quick Launch box."
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
                new MergeableNode<ICommandItem>(new CommandGroup("ToolsGroup", "_Tools"),
                    new MergeableNode<ICommandItem>(CommandItems["QuickLaunch"], new MergePoint(MergeOperation.InsertBefore, "ToolsSeparator"), MergePoint.Append)),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private void AddCaptionBarItems()
        {
            _quickLaunchViewModel = new QuickLaunchViewModel(Editor);
            Editor.CaptionBarItemsRight.Add(_quickLaunchViewModel);
        }


        private void RemoveCaptionBarItems()
        {
            Editor.CaptionBarItemsRight.Remove(_quickLaunchViewModel);
            _quickLaunchViewModel = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void FocusQuickLaunchBox()
        {
            Editor.Focus(_quickLaunchViewModel);
        }
        #endregion
    }
}
