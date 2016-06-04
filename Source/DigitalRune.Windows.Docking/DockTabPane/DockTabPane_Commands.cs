// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace DigitalRune.Windows.Docking
{
    partial class DockTabPane
    {
        private void RegisterCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(DockCommands.Next, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Previous, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.AutoHide, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Dock, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Float, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.ShowMenu, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCommandExecuted, OnCommandCanExecute));
        }


        /// <summary>
        /// Hooks the buttons in the control template up to the dock commands.
        /// </summary>
        private void InitializeButtons()
        {
            var booleanToVisibilityConverter = new BooleanToVisibilityConverter();
            if (_autoHideButton != null)
            {
                _autoHideButton.Command = DockCommands.AutoHide;
                _autoHideButton.CommandTarget = this;
                _autoHideButton.SetBinding(VisibilityProperty, new Binding("IsEnabled") { Source = _autoHideButton, Converter = booleanToVisibilityConverter });
            }

            if (_dockButton != null)
            {
                _dockButton.Command = DockCommands.Dock;
                _dockButton.CommandTarget = this;
                _dockButton.SetBinding(VisibilityProperty, new Binding("IsEnabled") { Source = _dockButton, Converter = booleanToVisibilityConverter });
            }

            if (_closeButton != null)
            {
                _closeButton.Command = ApplicationCommands.Close;
                _closeButton.CommandTarget = this;
                _closeButton.SetBinding(VisibilityProperty, new Binding("IsEnabled") { Source = _closeButton, Converter = booleanToVisibilityConverter });
            }

            if (_windowListButton != null)
            {
                _windowListButton.Command = DockCommands.ShowMenu;
                _windowListButton.CommandTarget = this;
                _windowListButton.SetBinding(VisibilityProperty, new Binding("IsEnabled") { Source = _windowListButton, Converter = booleanToVisibilityConverter });
            }
        }


        /// <summary>
        /// Determines whether a certain <see cref="RoutedCommand"/> can  execute.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            var originalSource = eventArgs.OriginalSource as DependencyObject;
            if (originalSource != null && _contentPresenter != null && _contentPresenter.IsAncestorOf(originalSource))
            {
                // The command originated from within the selected item. When DockTabItems are
                // generated from view-models the routed command does not reach the DockTabItem.
                // --> Manually route the command to the DockTabItem.
                var dockTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DockTabItem;
                dockTabItem?.OnCommandCanExecute(sender, eventArgs);
            }

            if (eventArgs.Handled)
                return;

            if (eventArgs.Command == DockCommands.Next)
            {
                eventArgs.CanExecute = CanActivateNext;
                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.Previous)
            {
                eventArgs.CanExecute = CanActivatePrevious;
                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.ShowMenu)
            {
                eventArgs.CanExecute = CanShowWindowList;
                eventArgs.Handled = true;
            }
            else
            {
                var dockStrategy = _dockControl?.DockStrategy;
                if (dockStrategy != null)
                {
                    var dockTabPane = DataContext as IDockTabPane;
                    if (dockTabPane != null)
                    {
                        // No Dock/Float/AutoHide when layout is locked.
                        bool isLocked = dockStrategy.DockControl.IsLocked;

                        if (eventArgs.Command == DockCommands.AutoHide)
                        {
                            if (!isLocked)
                            {
                                eventArgs.CanExecute = dockStrategy.CanAutoHide(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == DockCommands.Dock)
                        {
                            if (!isLocked)
                            {
                                eventArgs.CanExecute = dockStrategy.CanDock(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == DockCommands.Float)
                        {
                            if (!isLocked)
                            {
                                eventArgs.CanExecute = dockStrategy.CanFloat(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == ApplicationCommands.Close && SelectedItem is IDockTabItem)
                        {

                            eventArgs.CanExecute = true;    // Close button is always enabled.
                            eventArgs.Handled = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Executes a <see cref="RoutedCommand"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnCommandExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var originalSource = eventArgs.OriginalSource as DependencyObject;
            if (originalSource != null && _contentPresenter != null && _contentPresenter.IsAncestorOf(originalSource))
            {
                // The command originated from within the selected item. When DockTabItems are
                // generated from view-models the routed command does not reach the DockTabItem.
                // --> Manually route the command to the DockTabItem.
                var dockTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DockTabItem;
                dockTabItem?.OnCommandExecuted(sender, eventArgs);
            }

            if (eventArgs.Handled)
                return;

            if (eventArgs.Command == DockCommands.Next)
            {
                ActivateNext();
                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.Previous)
            {
                ActivatePrevious();
                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.ShowMenu)
            {
                ShowWindowList();
                eventArgs.Handled = true;
            }
            else
            {
                var dockStrategy = _dockControl?.DockStrategy;
                if (dockStrategy != null)
                {
                    var dockTabPane = DataContext as IDockTabPane;
                    if (dockTabPane != null)
                    {
                        // No Dock/Float/AutoHide when layout is locked.
                        bool isLocked = dockStrategy.DockControl.IsLocked;

                        if (eventArgs.Command == DockCommands.AutoHide)
                        {
                            if (!isLocked)
                            {
                                dockStrategy.AutoHide(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == DockCommands.Dock)
                        {
                            if (!isLocked)
                            {
                                dockStrategy.Dock(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == DockCommands.Float)
                        {
                            if (!isLocked)
                            {
                                dockStrategy.Float(dockTabPane);
                                eventArgs.Handled = true;
                            }
                        }
                        else if (eventArgs.Command == ApplicationCommands.Close && SelectedItem is IDockTabItem)
                        {
                            var dockTabItem = (IDockTabItem)SelectedItem;
                            if (dockStrategy.CanClose(dockTabItem))
                                dockStrategy.Close(dockTabItem);

                            eventArgs.Handled = true;
                        }
                    }
                }
            }
        }


        private bool CanActivateNext
        {
            get { return Items.Count > 1; }
        }


        private bool CanActivatePrevious
        {
            get { return Items.Count > 1; }
        }


        private bool CanShowWindowList
        {
            get
            {
                // The menu items of the window list invoke the dock commands.
                // Therefore, the window list can only be shown if the DockControl is set.
                if (_dockControl == null)
                    return false;

                // Only show window list if the DockTabPane contains at least one DockTabItem.
                return Items.Count > 0;
            }
        }


        private void ActivateNext()
        {
            int index = GetNextIndex(SelectedIndex, +1);
            if (index >= 0)
            {
                var dockTabItem = ItemContainerGenerator.ContainerFromIndex(index) as DockTabItem;
                dockTabItem?.Activate();
            }
        }


        private void ActivatePrevious()
        {
            int index = GetNextIndex(SelectedIndex, -1);
            if (index >= 0)
            {
                var dockTabItem = ItemContainerGenerator.ContainerFromIndex(index) as DockTabItem;
                dockTabItem?.Activate();
            }
        }
    }
}
