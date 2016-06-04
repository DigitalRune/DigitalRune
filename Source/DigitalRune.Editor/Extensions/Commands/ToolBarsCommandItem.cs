// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Commands
{
    /// <summary>
    /// Creates a menu that controls the visibility of the toolbars.
    /// </summary>
    internal sealed class ToolBarsCommandItem : ObservableObject, ICommandItem, IDisposable
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CommandExtension _commandExtension;
        private MenuItemViewModel _menuItemViewModel;
        private MenuSeparatorViewModel _menuSeparatorViewModel;
        private readonly DelegateCommand<ToolBarViewModel> _toggleToolBarCommand;
        internal readonly DelegateCommand<bool> ToggleAllToolBarsCommand;
        private IDisposable _propertyChangedSubscription;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        /// <inheritdoc/>
        public string Name { get { return "ToolBars"; } }


        /// <inheritdoc/>
        public bool AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        public string Category
        {
            get { return CommandCategories.View; }
        }


        /// <inheritdoc/>
        public ICommand Command { get { return null; } }


        /// <inheritdoc/>
        public object CommandParameter { get { return null; } }


        /// <inheritdoc/>
        public object Icon { get { return null; } }


        /// <inheritdoc/>
        public InputGestureCollection InputGestures { get { return null; } }


        /// <inheritdoc/>
        public bool IsCheckable { get { return false; } }


        /// <inheritdoc/>
        public bool IsChecked { get { return false; } }


        /// <inheritdoc/>
        public string Text { get { return "_Toolbars"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Customize toolbars."; }
        }


        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return true; }
            set { throw new NotSupportedException(); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarsCommandItem"/> class.
        /// </summary>
        /// <param name="commandExtension">The commands extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandExtension"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarsCommandItem(CommandExtension commandExtension)
        {
            if (commandExtension == null)
                throw new ArgumentNullException(nameof(commandExtension));

            _commandExtension = commandExtension;

            _toggleToolBarCommand = new DelegateCommand<ToolBarViewModel>(ToggleToolBar, CanToggleToolBarVisibility);

            ToggleAllToolBarsCommand = new DelegateCommand<bool>(ToggleAllToolBars, CanToggleAllToolBars);

            // Ideally, we remove this event handler when the commands extension is shutdown - 
            // but we skip this for now...
            _commandExtension.Editor.UIInvalidated += OnEditorUIInvalidated;
        }


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="ToolBarsCommandItem"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="ToolBarsCommandItem"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        private /*protected virtual*/ void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    _commandExtension.Editor.UIInvalidated -= OnEditorUIInvalidated;
                    _propertyChangedSubscription?.Dispose();
                }

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public MenuItemViewModel CreateMenuItem()
        {
            if (_menuItemViewModel == null)
            {
                // Use the same collection for the submenu and the toolbars context menu!
                _menuItemViewModel = new MenuItemViewModel(this)
                {
                    Submenu = _commandExtension.Editor.ToolBarContextMenu
                };

                _menuSeparatorViewModel = new MenuSeparatorViewModel(new CommandSeparator("ToolBarsSeparator"));
                _menuItemViewModel.Submenu.Add(_menuSeparatorViewModel);
                _menuItemViewModel.Submenu.Add(_commandExtension.CommandItems["ShowAllToolBars"].CreateMenuItem());
                _menuItemViewModel.Submenu.Add(_commandExtension.CommandItems["HideAllToolBars"].CreateMenuItem());
            }
            
            return _menuItemViewModel;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            return null;
        }


        private void OnEditorUIInvalidated(object sender, EventArgs eventArgs)
        {
            // The CommandGroup.IsVisible influences the CanExecute state.

            // Unsubscribe.
            _propertyChangedSubscription?.Dispose();

            // Subscribe to property changed of all toolbar command groups.
            _propertyChangedSubscription = 
                _commandExtension.Editor
                                  .ToolBars
                                  .Select(t => t.CommandGroup)
                                  .Select(cg => Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                                                    eh => eh.Invoke, 
                                                    eh => cg.PropertyChanged += eh,
                                                    eh => cg.PropertyChanged -= eh))
                                  .Merge()
                                  .Subscribe(e => OnCommandGroupPropertyChanged(e.Sender, e.EventArgs));

            UpdateMenuItems();
        }


        private void OnCommandGroupPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // The command group visibility influences the CanExecute state!
            if (string.IsNullOrEmpty(eventArgs.PropertyName) || eventArgs.PropertyName == nameof(CommandGroup.IsVisible))
                RaiseCanExecuteChanged();
        }


        public void UpdateMenuItems()
        {
            if (_menuItemViewModel == null)
                return;

            var toolBars = _commandExtension.Editor
                                             .ToolBars
                                             .OrderByDescending(t => EditorHelper.FilterAccessKeys(t.CommandGroup.Text));

            // Remove old menu items before separator.
            while (!(_menuItemViewModel.Submenu[0] is MenuSeparatorViewModel))
                _menuItemViewModel.Submenu.RemoveAt(0);

            _menuSeparatorViewModel.IsVisible = toolBars.Any();

            // Create one menu item per toolbar.
            foreach (var toolBar in toolBars)
            {
                _menuItemViewModel.Submenu.Insert(
                    0,
                    new MenuItemViewModel(
                        new DelegateCommandItem(
                            Invariant($"ToggleToolBar{toolBar.CommandGroup.Name}"),
                            _toggleToolBarCommand)
                        {
                            Category = Category,
                            CommandParameter = toolBar,
                            IsCheckable = true,
                            IsChecked = toolBar.IsVisible,
                            Text = toolBar.CommandGroup.Text,
                            ToolTip = "Toggle toolbar visibility.",
                        }));
            }

            RaiseCanExecuteChanged();
        }


        private void RaiseCanExecuteChanged()
        {
            _toggleToolBarCommand.RaiseCanExecuteChanged();
            ToggleAllToolBarsCommand.RaiseCanExecuteChanged();
        }


        private static bool CanToggleToolBarVisibility(ToolBarViewModel toolBar)
        {
            return toolBar != null && toolBar.CommandGroup.IsVisible;
        }


        private void ToggleToolBar(ToolBarViewModel toolBar)
        {
            // Toggle visibility of the toolbar that is associated with this menu item.
            if (toolBar.IsVisible)
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Hiding toolbar {0}.", toolBar.CommandGroup.Name);
                toolBar.IsVisible = false;
            }
            else
            {
                Logger.Debug(CultureInfo.InvariantCulture, "Showing toolbar {0}.", toolBar.CommandGroup.Name);
                toolBar.IsVisible = true;
            }

            UpdateMenuItems();
        }


        private bool CanToggleAllToolBars(bool makeVisible)
        {
            if (makeVisible)
            {
                // Show toolbars menu is only visible if any toolbar is invisible and the
                // CommandGroup allows the toolbar to be visible.
                return _commandExtension.Editor.ToolBars.Any(t => !t.IsVisible && t.CommandGroup.IsVisible);
            }
            else
            {
                return _commandExtension.Editor.ToolBars.Any(t => t.IsVisible);
            }
        }


        private void ToggleAllToolBars(bool makeVisible)
        {
            Logger.Debug(makeVisible ? "Showing all toolbars." : "Hiding all toolbars.");

            foreach (var toolBar in _commandExtension.Editor.ToolBars)
                toolBar.IsVisible = makeVisible;

            UpdateMenuItems();
        }
        #endregion
    }
}
