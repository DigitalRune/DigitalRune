// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents a Reload File dialog that is shown when files on disk have been modified by
    /// another application.
    /// </summary>
    /// <remarks>
    /// The user has the choice to reload the affected documents or to ignore the changes (see
    /// <see cref="ReloadFileDialogResult"/>).
    /// </remarks>
    internal class ReloadFileViewModel : Dialog
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
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        }
        private string _fileName = string.Empty;



        /// <summary>
        /// Gets or sets a value indicating whether the local document has been modified.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the local document has been modified; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IsFileModified
        {
            get { return _isFileModified; }
            set { SetProperty(ref _isFileModified, value); }
        }
        private bool _isFileModified;



        /// <summary>
        /// Gets or sets the result (user's choice) of the Reload File dialog.
        /// </summary>
        /// <value>The result (user's choice) of the Reload File dialog.</value>
        public ReloadFileDialogResult ReloadFileDialogResult
        {
            get { return _reloadFileDialogResult; }
            set { SetProperty(ref _reloadFileDialogResult, value); }
        }
        private ReloadFileDialogResult _reloadFileDialogResult = ReloadFileDialogResult.No;


        /// <summary>
        /// Gets the command that is executed when the "Yes" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "Yes" button is pressed.</value>
        public DelegateCommand YesCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the "Yes to All" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "Yes to All" button is pressed.</value>
        public DelegateCommand YesToAllCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the "No" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "No" button is pressed.</value>
        public DelegateCommand NoCommand { get; private set; }


        /// <summary>
        /// Gets the command that is executed when the "No to All" button is pressed.
        /// </summary>
        /// <value>The command that is executed when the "No to All" button is pressed.</value>
        public DelegateCommand NoToAllCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ReloadFileViewModel"/> class.
        /// </summary>
        public ReloadFileViewModel()
        {
            if (WindowsHelper.IsInDesignMode)
            {
                _fileName = @"Drive:\Path\Filename";
            }

            YesCommand = new DelegateCommand(Yes);
            YesToAllCommand = new DelegateCommand(YesToAll);
            NoCommand = new DelegateCommand(No);
            NoToAllCommand = new DelegateCommand(NoToAll);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void Yes()
        {
            Logger.Debug("Closing Reload Files dialog. (\"Yes\" selected.)");
            ReloadFileDialogResult = ReloadFileDialogResult.Yes;
            DialogResult = true;
        }


        private void YesToAll()
        {
            Logger.Debug("Closing Reload Files dialog. (\"Yes to all\" selected.)");
            ReloadFileDialogResult = ReloadFileDialogResult.YesToAll;
            DialogResult = true;
        }


        private void No()
        {
            Logger.Debug("Closing Reload Files dialog. (\"No\" selected.)");
            ReloadFileDialogResult = ReloadFileDialogResult.No;
            DialogResult = true;
        }


        private void NoToAll()
        {
            Logger.Debug("Closing Reload Files dialog. (\"No to all\" selected.)");
            ReloadFileDialogResult = ReloadFileDialogResult.NoToAll;
            DialogResult = true;
        }
        #endregion
    }
}
