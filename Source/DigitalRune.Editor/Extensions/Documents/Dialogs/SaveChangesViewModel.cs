// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents the Save Changes dialog that prompts the user to save modified files.
    /// </summary>
    /// <remarks>
    /// The dialog sets the <see cref="SaveChangesDialogResult"/> to indicate the user's choice.
    /// </remarks>
    internal class SaveChangesViewModel : Dialog
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the modified documents.
        /// </summary>
        /// <value>The modified documents.</value>
        public IEnumerable<Document> ModifiedDocuments
        {
            get { return _modifiedDocuments; }
            set { SetProperty(ref _modifiedDocuments, value); }
        }
        private IEnumerable<Document> _modifiedDocuments;



        /// <summary>
        /// Gets or sets the result (user's choice) of the Save Changes dialog.
        /// </summary>
        /// <value>The result (user's choice) of the Save Changes dialog.</value>
        public SaveChangesDialogResult SaveChangesDialogResult
        {
            get { return _saveChangesDialogResult; }
            set { SetProperty(ref _saveChangesDialogResult, value); }
        }
        private SaveChangesDialogResult _saveChangesDialogResult = SaveChangesDialogResult.Cancel;


        /// <summary>
        /// Gets the command that is executed when the "Yes" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "Yes" button is pressed.</value>
        public DelegateCommand YesCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the "No" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "No" button is pressed.</value>
        public DelegateCommand NoCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the "Cancel" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "Cancel" button is pressed.</value>
        public DelegateCommand CancelCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveChangesViewModel"/> class.
        /// </summary>
        public SaveChangesViewModel()
        {
            if (WindowsHelper.IsInDesignMode)
            {
                ModifiedDocuments = new[] {
                    new DesignTimeDocument(),
                    new DesignTimeDocument(),
                    new DesignTimeDocument(),
                };
            }

            YesCommand = new DelegateCommand(Yes);
            NoCommand = new DelegateCommand(No);
            CancelCommand = new DelegateCommand(Cancel);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void Yes()
        {
            Logger.Debug("Closing Save Changes dialog. (\"Yes\" clicked.)");
            SaveChangesDialogResult = SaveChangesDialogResult.SaveAndClose;
            DialogResult = true;
        }


        private void No()
        {
            Logger.Debug("Closing Save Changes dialog. (\"No\" clicked.)");
            SaveChangesDialogResult = SaveChangesDialogResult.CloseWithoutSaving;
            DialogResult = true;
        }


        private void Cancel()
        {
            Logger.Debug("Closing Save Changes dialog. (\"Cancel\" clicked.)");
            SaveChangesDialogResult = SaveChangesDialogResult.Cancel;
            DialogResult = false;
        }
        #endregion
    }
}
