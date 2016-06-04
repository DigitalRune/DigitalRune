// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Input;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Displays a status information.
    /// </summary>
    internal partial class StatusView
    {
        private Window _window;
        private CommandBinding _cancelCommandBinding;


        /// <summary>
        /// Initializes a new instance of the <see cref="StatusView"/> class.
        /// </summary>
        public StatusView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            _window = Window.GetWindow(this);
            if (_window != null)
            {
                // Attach "Stop" command binding to main window.
                _cancelCommandBinding = new CommandBinding(ApplicationCommands.Stop, ExecutedStopCommand, CanExecuteStopCommand);
                _window.CommandBindings.Add(_cancelCommandBinding);
            }
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            if (_window != null)
            {
                // Detach "Stop" command binding from main window.
                _window.CommandBindings.Remove(_cancelCommandBinding);
            }
        }


        private void CanExecuteStopCommand(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            var statusViewModel = (StatusViewModel)DataContext;
            bool canExecute = statusViewModel.CancelCommand.CanExecute();
            eventArgs.CanExecute = canExecute;
            eventArgs.Handled = canExecute;
        }


        private void ExecutedStopCommand(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var statusViewModel = (StatusViewModel)DataContext;
            statusViewModel.CancelCommand.Execute();
            eventArgs.Handled = true;
        }
    }
}
