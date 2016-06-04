// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a pane that contains multiple windows that share the same space on the screen.
    /// </summary>
    internal class EditorDockTabPaneViewModel : DockTabPaneViewModel
    {
        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the editor.
        /// </summary>
        /// <value>The editor.</value>
        public IEditorService Editor { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorDockTabPaneViewModel" /> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public EditorDockTabPaneViewModel(IEditorService editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            Editor = editor;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            base.OnPropertyChanged(eventArgs);

            if (string.IsNullOrEmpty(eventArgs.PropertyName)
                || eventArgs.PropertyName == nameof(IDockElement.DockState)
                || eventArgs.PropertyName == nameof(IDockPane.IsVisible)
                || eventArgs.PropertyName == nameof(IDockTabPane.SelectedItem))
            {
                ScreenConduction();
            }
        }


        private void ScreenConduction()
        {
            if (!IsVisible)
                return;

            // Set screen conductor.
            foreach (var item in Items)
            {
                var screen = item as IScreen;
                if (screen != null)
                    screen.Conductor = Editor;
            }

            // Note: Redundant calls of IActivatable.OnActivate/OnDeactivate should have no effect.
            if (Editor.IsActive)
            {
                // Deactivate unselected items.
                foreach (var item in Items)
                    if (item != SelectedItem && item.DockState == DockState)
                        (item as IActivatable)?.OnDeactivate(false);

                // Activate selected items.
                if (SelectedItem?.DockState == DockState)
                    (SelectedItem as IActivatable)?.OnActivate();
            }
            else
            {
                // Deactivate all items.
                foreach (var item in Items)
                    if (item.DockState == DockState)
                        (item as IActivatable)?.OnDeactivate(false);
            }
        }
        #endregion
    }
}
