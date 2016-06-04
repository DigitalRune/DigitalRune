// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel.DataAnnotations;
using System.IO;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Represents the "Save Layout" dialog.
    /// </summary>
    internal class SaveLayoutViewModel : Dialog
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const string InvalidCharactersErrorMessage = "The layout name must be a valid file name. Remove any invalid characters.";
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name of the layout.
        /// </summary>
        /// <value>The name of the layout.</value>
        [Required(ErrorMessage = "The layout name is required.")]
        [MaxLength(200, ErrorMessage = "The layout name must be less than 200 characters.")]
        public string LayoutName
        {
            get { return _layoutName; }
            set
            {
                if (SetProperty(ref _layoutName, value) && value != null)
                {
                    // The layout name needs to be a valid file name.
                    if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                        ErrorsContainer.AddError(nameof(LayoutName), InvalidCharactersErrorMessage);
                    else
                        ErrorsContainer.RemoveError(nameof(LayoutName), InvalidCharactersErrorMessage);
                }
            }
        }
        private string _layoutName;


        /// <summary>
        /// Gets the command that is invoked when the Ok button is clicked.
        /// </summary>
        /// <value>The Ok command.</value>
        public DelegateCommand OkCommand { get; }


        /// <summary>
        /// Gets the command that is invoked when the Cancel button is clicked.
        /// </summary>
        /// <value>The Cancel command.</value>
        public DelegateCommand CancelCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveLayoutViewModel"/> class.
        /// </summary>
        public SaveLayoutViewModel()
        {
            OkCommand = new DelegateCommand(Ok, CanOk);
            CancelCommand = new DelegateCommand(Cancel);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private bool CanOk()
        {
            return !HasErrors;
        }


        private void Ok()
        {
            if (!HasErrors)
                DialogResult = true;
        }


        private void Cancel()
        {
            DialogResult = false;
        }
        #endregion
    }
}
