// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DigitalRune.Editor.Status;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        private bool _inDrop;


        /// <summary>
        /// Called when an object is dragged onto the editor window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="DragEventArgs"/> instance containing the event data.
        /// </param>
        private void OnDragEnter(object sender, DragEventArgs eventArgs)
        {
            if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = eventArgs.Data.GetData(DataFormats.FileDrop) as string[];
                if (fileNames != null && fileNames.Length > 0)
                {
                    if (fileNames.Any(IsFileSupported))
                    {
                        // Allow drop if any of the files is supported by the application.
                        eventArgs.Effects = DragDropEffects.Copy;
                    }
                }
            }
        }


        /// <summary>
        /// Determines whether a file can be loaded in the application.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>
        /// <see langword="true"/> if the file can be loaded; otherwise, <see langword="false"/>.
        /// </returns>
        private bool IsFileSupported(string fileName)
        {
            var uri = new Uri(fileName);
            return Factories.Any(f => f.GetDocumentType(uri) != null);
        }


        /// <summary>
        /// Called when an object is dropped on the main window of the application.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="DragEventArgs"/> instance containing the event data.
        /// </param>
        private async void OnDrop(object sender, DragEventArgs eventArgs)
        {
            // Avoid re-entrance.
            if (_inDrop)
                return;

            Logger.Debug("Object dropped on main window via drag-and-drop.");

            try
            {
                _inDrop = true;
                if (eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] fileNames = eventArgs.Data.GetData(DataFormats.FileDrop) as string[];
                    if (fileNames != null && fileNames.Length > 0)
                    {
                        eventArgs.Handled = true;

                        // Show status message and Cancel button.
                        var token = new CancellationTokenSource();
                        var status = new StatusViewModel
                        {
                            Message = "Opening files...",
                            Progress = 0,
                            ShowProgress = true,
                            CancellationTokenSource = token
                        };
                        _statusService.Show(status);

                        // Determine on which DockPane the files were dropped.
                        var targetObject = eventArgs.Source as DependencyObject;
                        var targetPane = targetObject?.GetVisualAncestors()
                                                      .OfType<DockTabPane>()
                                                      .Select(dockTabPane => dockTabPane.DataContext as IDockTabPane)
                                                      .FirstOrDefault();

                        for (int i = 0; i < fileNames.Length; i++)
                        {
                            string fileName = fileNames[i];

                            if (token.IsCancellationRequested || Keyboard.IsKeyDown(Key.Escape))
                            {
                                Logger.Info("Opening files via drag-and-drop canceled.");
                                status.Message = "Opening files canceled.";
                                status.IsCompleted = true;
                                status.ShowProgress = false;
                                await status.CloseAfterDefaultDurationAsync();
                                return;
                            }

                            Logger.Info(CultureInfo.InvariantCulture, "Opening file \"{0}\" via drag-and-drop.", fileName);

                            try
                            {
                                // Open document.
                                var document = Open(new Uri(fileName));

                                // Update status.
                                status.Progress = (double)(i + 1) / fileNames.Length;

                                // Move document to target pane.
                                if (targetPane != null && document != null)
                                {
                                    var viewModel = document.ViewModels.FirstOrDefault();
                                    Editor.DockStrategy.Dock(viewModel, targetPane, DockPosition.Inside);
                                }
                            }
                            catch (Exception exception)
                            {
                                Logger.Warn(exception, CultureInfo.InvariantCulture, "Could not open file \"{0}\" via drag-and-drop.", fileName);
                            }

                            // Redraw GUI and keep application responsive.
                            await Dispatcher.Yield();

                            if (Editor.IsShuttingDown)
                                break;
                        }

                        await status.CloseAsync();
                    }
                }
            }
            finally
            {
                _inDrop = false;
            }
        }
    }
}
