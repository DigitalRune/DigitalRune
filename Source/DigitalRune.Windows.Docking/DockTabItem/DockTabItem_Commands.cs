// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;


namespace DigitalRune.Windows.Docking
{
    partial class DockTabItem
    {
        private void RegisterCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.AutoHide, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Dock, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Float, OnCommandExecuted, OnCommandCanExecute));
            CommandBindings.Add(new CommandBinding(DockCommands.Show, OnCommandExecuted, OnCommandCanExecute));
        }


        internal void OnCommandCanExecute(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            var dockStrategy = _dockControl?.DockStrategy;
            if (dockStrategy == null)
                return;

            // No Dock/Float/AutoHide when layout is locked.
            bool isLocked = dockStrategy.DockControl.IsLocked;

            var dockTabItem = DataContext as IDockTabItem;
            if (dockTabItem == null)
                return;

            if (eventArgs.Command == ApplicationCommands.Close)
            {
                eventArgs.CanExecute = true;    // Close button is always enabled.
                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.AutoHide)
            {
                if (!isLocked)
                {
                    eventArgs.CanExecute = dockStrategy.CanAutoHide(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Dock)
            {
                if (!isLocked)
                {
                    eventArgs.CanExecute = dockStrategy.CanDock(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Float)
            {
                if (!isLocked)
                {
                    eventArgs.CanExecute = dockStrategy.CanFloat(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Show)
            {
                eventArgs.CanExecute = true;
                eventArgs.Handled = true;
            }
        }


        internal void OnCommandExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var dockStrategy = _dockControl?.DockStrategy;
            if (dockStrategy == null)
                return;

            var dockTabItem = DataContext as IDockTabItem;
            if (dockTabItem == null)
                return;

            // No Dock/Float/AutoHide when layout is locked.
            bool isLocked = dockStrategy.DockControl.IsLocked;

            if (eventArgs.Command == ApplicationCommands.Close)
            {
                if (dockStrategy.CanClose(dockTabItem))
                    dockStrategy.Close(dockTabItem);

                eventArgs.Handled = true;
            }
            else if (eventArgs.Command == DockCommands.AutoHide)
            {
                if (!isLocked)
                {
                    dockStrategy.AutoHide(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Dock)
            {
                if (!isLocked)
                {
                    dockStrategy.Dock(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Float)
            {
                if (!isLocked)
                {
                    dockStrategy.Float(dockTabItem);
                    eventArgs.Handled = true;
                }
            }
            else if (eventArgs.Command == DockCommands.Show)
            {
                dockStrategy.Show(dockTabItem);
                eventArgs.Handled = true;
            }
        }
    }
}
