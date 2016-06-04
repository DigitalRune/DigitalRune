// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Editor.Documents;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Extends the default <see cref="DockStrategy"/> and deactivate screens when they are closed
    /// (screen conduction).
    /// </summary>
    internal class EditorDockStrategy : DockStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        private EditorViewModel Editor
        {
            get { return (EditorViewModel)DockControl; }
        }


        /// <inheritdoc/>
        protected override IDockTabPane OnCreateDockTabPane()
        {
            Debug.Assert(DockControl is EditorViewModel);

            return new EditorDockTabPaneViewModel((EditorViewModel)DockControl);
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnCreateDockTabItem(string dockId)
        {
            return ((EditorViewModel)DockControl).Extensions
                                                 .Select(ext => ext.GetViewModel(dockId))
                                                 .FirstOrDefault(ds => ds != null);
        }


        /// <inheritdoc/>
        protected override void OnBegin()
        {
            Logger.Debug("Docking layout operation started.");

            base.OnBegin();
        }


        /// <inheritdoc/>
        protected override void OnEnd()
        {
            base.OnEnd();

            // Validate IActivatable state.
            Validate();

            Logger.Debug("Docking layout operation ended.");

            Editor.OnLayoutChanged(EventArgs.Empty);
        }


        [Conditional("DEBUG")]
        private void Validate()
        {
            var editor = (EditorViewModel)DockControl;
            if (editor.IsActive)
            {
                var dockTabPanes = editor.GetDockElements().OfType<IDockTabPane>();
                foreach (var dockTabPane in dockTabPanes)
                {
                    var selectedItem = dockTabPane.SelectedItem;
                    foreach (var dockTabItem in dockTabPane.Items)
                    {
                        var activatable = dockTabItem as IActivatable;
                        if (activatable == null)
                            continue;

                        if (dockTabItem.DockState == DockState.Hide)
                        {
                            // activatable.IsOpen can be true or false. Depends on whether it has
                            // been activated before.
                            Debug.Assert(!activatable.IsActive);
                            continue;
                        }

                        if (dockTabPane.IsVisible && dockTabItem.DockState == dockTabPane.DockState)
                        {
                            if (dockTabItem == selectedItem)
                            {
                                Debug.Assert(activatable.IsOpen);
                                Debug.Assert(activatable.IsActive);
                            }
                            else
                            {
                                // activatable.IsOpen can be true or false. Depends on whether it has
                                // been activated before.
                                Debug.Assert(!activatable.IsActive);
                            }
                        }
                    }
                }
            }
            else
            {
                var activatables = editor.GetDockElements().OfType<IActivatable>();
                foreach (var activatable in activatables)
                {
                    Debug.Assert(activatable.IsOpen == editor.IsOpen);
                    Debug.Assert(!activatable.IsActive);
                }
            }
        }


        /// <inheritdoc/>
        protected override IDockPane OnGetDefaultDockTarget(IDockElement element)
        {
            // If a document has the focus, we dock new into its pane.
            if (DockControl.ActiveDockTabItem is DocumentViewModel)
                return DockControl.ActiveDockTabPane;

            var dockAnchorPane = First<IDockAnchorPane>(DockControl.RootPane);
            if (dockAnchorPane != null)
            {
                // Search anchor pane first.
                // Use the tab pane that contains the first found document.
                var document = First<DocumentViewModel>(dockAnchorPane);
                if (document != null)
                    return DockHelper.GetParent(dockAnchorPane, document);

                // No document found. Use the anchor pane.
                return dockAnchorPane;
            }
            else
            {
                // No anchor pane found.
                // Use the tab pane that contains the first found document.
                var document = First<DocumentViewModel>(DockControl.RootPane);
                if (document != null)
                    return DockHelper.GetParent(DockControl.RootPane, document);
            }

            // No anchor pane and no document found. Fall back to default behavior.
            return base.OnGetDefaultDockTarget(element);
        }


        /// <summary>
        /// Gets the first visible instance of the specified type in the docking layout.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IDockElement"/></typeparam>
        /// <param name="dockPane">The <see cref="IDockPane"/> to examine.</param>
        /// <returns>
        /// The first instance of <typeparamref name="T"/> found in <paramref name="dockPane"/>.
        /// </returns>
        private static T First<T>(IDockPane dockPane) where T : class, IDockElement
        {
            if (dockPane == null || !dockPane.IsVisible)
                return null;

            T t = dockPane as T;
            if (t != null)
                return t;

            var dockAnchorPane = dockPane as IDockAnchorPane;
            if (dockAnchorPane != null)
                return First<T>(dockAnchorPane.ChildPane);

            var dockSplitPane = dockPane as IDockSplitPane;
            if (dockSplitPane != null)
            {
                foreach (var childPane in dockSplitPane.ChildPanes)
                {
                    var result = First<T>(childPane);
                    if (result != null)
                        return result;
                }
            }

            var dockTabPane = dockPane as IDockTabPane;
            if (dockTabPane != null && dockTabPane.Items.Count > 0)
                return dockTabPane.Items[0] as T;

            return null;
        }


        /// <inheritdoc/>
        public override bool CanClose(IFloatWindow floatWindow)
        {
            // Optional: Handle CanClose for FloatWindow with multiple documents.
            return base.CanClose(floatWindow);
        }


        /// <inheritdoc/>
        public override bool CanClose(IDockTabItem dockTabItem)
        {
            // Screen conduction: IGuardClose
            var guardClose = dockTabItem as IGuardClose;
            if (guardClose != null)
            {
                var canClose = guardClose.CanCloseAsync();
                Debug.Assert(canClose.IsCompleted, "CanCloseAsync expected to be synchronous operation.");
                return canClose.Result;
            }

            return base.CanClose(dockTabItem);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnClose(IDockTabItem dockTabItem)
        {
            base.OnClose(dockTabItem);

            // Screen conduction: IActivatable
            (dockTabItem as IActivatable)?.OnDeactivate(true);

            // Screen conduction: IScreen
            var screen = dockTabItem as IScreen;
            if (screen != null)
                screen.Conductor = null;
        }
    }
}
