// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Represents the "Manage Layouts" dialog.
    /// </summary>
    internal class ManageLayoutsViewModel : Dialog
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly LayoutExtension _layoutExtension;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the layouts.
        /// </summary>
        /// <value>The layouts.</value>
        public ObservableCollection<WindowLayout> Layouts
        {
            get { return _layoutExtension.Layouts; }
        }


        /// <summary>
        /// Gets or sets the selected list box item.
        /// </summary>
        /// <value>The selected list box item.</value>
        public WindowLayout SelectedLayout
        {
            get { return _selectedLayout; }
            set
            {
                SetProperty(ref _selectedLayout, value);
                {
                    RenameCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private WindowLayout _selectedLayout;


        /// <summary>
        /// Gets the command that is invoked when the Rename button is clicked.
        /// </summary>
        /// <value>The Rename command.</value>
        public DelegateCommand RenameCommand { get; }


        /// <summary>
        /// Gets the command that is invoked when the Delete button is clicked.
        /// </summary>
        /// <value>The Delete command.</value>
        public DelegateCommand DeleteCommand { get; }


        /// <summary>
        /// Gets the command that is invoked when the Close button is clicked.
        /// </summary>
        /// <value>The Close command.</value>
        public DelegateCommand CloseCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageLayoutsViewModel"/> class.
        /// </summary>
        /// <param name="layoutExtension">The <see cref="LayoutExtension"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="layoutExtension"/> is <see langword="null"/>.
        /// </exception>
        public ManageLayoutsViewModel(LayoutExtension layoutExtension)
        {
            if (layoutExtension == null)
                throw new ArgumentNullException(nameof(layoutExtension));

            DisplayName = "Manage Layouts";

            _layoutExtension = layoutExtension;

            // Initial selection in the list box is the active window layout.
            _selectedLayout = _layoutExtension.ActiveLayout;

            RenameCommand = new DelegateCommand(Rename, CanRename);
            DeleteCommand = new DelegateCommand(Delete, CanDelete);
            CloseCommand = new DelegateCommand(Close);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private bool CanRename()
        {
            return SelectedLayout != null && !SelectedLayout.IsFactoryPreset;
        }


        private void Rename()
        {
            if (CanRename())
                _layoutExtension.RenameWindowLayout(SelectedLayout);
        }


        private bool CanDelete()
        {
            return CanRename() && SelectedLayout != _layoutExtension.ActiveLayout;
        }


        private void Delete()
        {
            if (CanDelete())
                _layoutExtension.Delete(SelectedLayout);
        }


        private void Close()
        {
            DialogResult = true;
        }
        #endregion
    }
}
