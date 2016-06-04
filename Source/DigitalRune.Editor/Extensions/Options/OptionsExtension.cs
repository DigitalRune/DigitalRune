// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Shows and controls the content of the Options dialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IOptionsService"/></item>
    /// </list>
    /// </remarks>
    public sealed partial class OptionsExtension : EditorExtension, IOptionsService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IWindowService _windowService;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private OptionsViewModel Options
        {
            get
            {
                if (_optionsViewModel == null)
                    _optionsViewModel = new OptionsViewModel(this);

                return _optionsViewModel;
            }
        }
        private OptionsViewModel _optionsViewModel;


        /// <inheritdoc/>
        public ICollection<MergeableNodeCollection<OptionsPageViewModel>> OptionsNodeCollections { get; } = new List<MergeableNodeCollection<OptionsPageViewModel>>();
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
            Editor.Services.Register(typeof(IOptionsService), null, this);
            Editor.Services.RegisterView(typeof(OptionsViewModel), typeof(OptionsWindow));
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _windowService = Editor.Services.GetInstance<IWindowService>().ThrowIfMissing();

            AddCommands();
            AddMenus();

            // Add Quick Launch items on activation.
            Editor.Activated += OnEditorActivated;
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            Editor.Activated -= OnEditorActivated;
            RemoveQuickLaunchItems();

            RemoveMenus();
            RemoveCommands();

            _windowService = null;
            _optionsViewModel = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.UnregisterView(typeof(OptionsViewModel));
            Editor.Services.Unregister(typeof(IOptionsService));
        }


        private void AddCommands()
        {
            CommandItems.Add(
                new DelegateCommandItem("ShowOptions", new DelegateCommand(Show))
                {
                    Category = CommandCategories.Tools,
                    Icon = MultiColorGlyphs.Options,
                    Text = "_Options...",
                    ToolTip = "Show a dialog where the application's options can be viewed and changed."
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            // Add the "Options..." menu item to the "Tools" menu.
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("ToolsGroup", "_Tools"),
                    new MergeableNode<ICommandItem>(CommandItems["ShowOptions"], new MergePoint(MergeOperation.InsertBefore, "OptionsSeparator"), MergePoint.Append))
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        /// <inheritdoc/>
        public void Show()
        {
            Show(null);
        }


        private void Show(MergeableNode<OptionsPageViewModel> optionsNode)
        {
            Logger.Info("Showing Options dialog.");

            if (optionsNode != null)
                Options.SelectedNode = optionsNode;

            _windowService.ShowDialog(Options);

            Logger.Info("Options dialog closed.");
        }
        #endregion
    }
}
