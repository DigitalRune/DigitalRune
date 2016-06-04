// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Windows.Docking;


namespace DigitalRune.Editor
{
    public partial class EditorViewModel
    {
        // Notes:
        // The main window can have input binding (e.g. key bindings handle key input and raise
        // a command) and command bindings (e.g. the main window handles the "Find" command and
        // activates the quick find box). Windows of the DockControl should work as if they are
        // in the visual tree of the main window - which they are not: Input events for input
        // bindings and commands are not routed from the child window to its main window. Therefore,
        // we have to install the input bindings and command bindings of the main window in the
        // windows created by the dock control: float window and auto-hide overlay windows.
        // Command routing in the other direction (from the window to the focused element) also
        // needs work, but this is handled by the EditorWindow code-behind.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Original input bindings of the main window.
        private readonly List<InputBinding> _inputBindings = new List<InputBinding>();

        // Input bindings created from command item.
        private readonly List<InputBinding> _commandItemInputBindings = new List<InputBinding>();

        // Original command bindings of the main window.
        private readonly List<CommandBinding> _commandBindings = new List<CommandBinding>();
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void UpdateInputAndCommandBindings()
        {
            if (Window == null)
            {
                // Abort. This method is called again in EditorViewModel.OnActivated.
                return;
            }

            // Remove previous input bindings from main window and float windows.
            RemoveInputBindings(Window);
            RemoveInputBindings(DockControl);
            RemoveCommandBindings(DockControl);

            _inputBindings.Clear();
            _commandItemInputBindings.Clear();
            _commandBindings.Clear();

            // Get existing input bindings of main window.
            _inputBindings.AddRange(Window.InputBindings.OfType<InputBinding>());

            // Add input bindings for command items.
            var commandItems = MenuManager.CommandItems
                                          .Concat(ToolBarManager.CommandItems)
                                          .OfType<CommandItem>()
                                          .Distinct();
            foreach (var commandItem in commandItems)
            {
                if (commandItem?.Command != null 
                    && !(commandItem.Command is RoutedCommand) 
                    && commandItem.InputGestures != null)
                {
                    foreach (var gesture in commandItem.InputGestures.OfType<InputGesture>())
                    {
                        var inputBinding = new InputBinding(commandItem.Command, gesture)
                        {
                            CommandParameter = commandItem.CommandParameter
                        };

                        _commandItemInputBindings.Add(inputBinding);
                    }
                }
            }

            // Get list of command bindings.
            // Command bindings could be defined in XAML or they are added to the window by 
            // extensions.
            _commandBindings.AddRange(Window.CommandBindings.OfType<CommandBinding>());

            // Add input bindings to main window and float windows.
            AddInputBindings(Window);
            AddInputBindings(DockControl);
            AddCommandBindings(DockControl);
        }


        private void AddInputBindings(Window window)
        {
            if (window == null)
                return;

            if (window != Window)
                window.InputBindings.AddRange(_inputBindings);

            window.InputBindings.AddRange(_commandItemInputBindings);
        }


        private void RemoveInputBindings(Window window)
        {
            if (window == null)
                return;

            if (window != Window)
                foreach (var inputBinding in _inputBindings)
                    window.InputBindings.Remove(inputBinding);

            foreach (var inputBinding in _commandItemInputBindings)
                window.InputBindings.Remove(inputBinding);
        }


        private void AddCommandBindings(Window window)
        {
            window?.CommandBindings.AddRange(_commandBindings);
        }


        private void RemoveCommandBindings(Window window)
        {
            if (window != null)
                foreach (var commandBinding in _commandBindings)
                    window.CommandBindings.Remove(commandBinding);
        }


        private void AddInputBindings(DockControl dockControl)
        {
            if (dockControl == null)
                return;

            foreach (var floatWindow in dockControl.FloatWindows)
                AddInputBindings(floatWindow);

            foreach (var autoHideOverlay in dockControl.AutoHideOverlays)
                AddInputBindings(autoHideOverlay);
        }


        private void RemoveInputBindings(DockControl dockControl)
        {
            if (dockControl == null)
                return;

            foreach (var floatWindow in dockControl.FloatWindows)
                RemoveInputBindings(floatWindow);

            foreach (var autoHideOverlay in dockControl.AutoHideOverlays)
                RemoveInputBindings(autoHideOverlay);
        }


        private void AddCommandBindings(DockControl dockControl)
        {
            if (dockControl == null)
                return;

            foreach (var floatWindow in dockControl.FloatWindows)
                AddCommandBindings(floatWindow);

            foreach (var autoHideOverlay in dockControl.AutoHideOverlays)
                AddCommandBindings(autoHideOverlay);
        }


        private void RemoveCommandBindings(DockControl dockControl)
        {
            if (dockControl == null)
                return;

            foreach (var floatWindow in dockControl.FloatWindows)
                RemoveCommandBindings(floatWindow);

            foreach (var autoHideOverlay in dockControl.AutoHideOverlays)
                RemoveCommandBindings(autoHideOverlay);
        }
        #endregion
    }
}
