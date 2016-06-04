// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Represents the "Go To Line" dialog.
    /// </summary>
    internal class GoToLineViewModel : Dialog
    {
        /// <summary>
        /// Gets or sets the total number of lines.
        /// </summary>
        /// <value>The total number of lines.</value>
        public int NumberOfLines
        {
            get { return _numberOfLines; }
            set
            {
                if (SetProperty(ref _numberOfLines, value))
                    OkCommand.RaiseCanExecuteChanged();
            }
        }
        private int _numberOfLines = int.MaxValue;


        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>The line number between 1 and <see cref="NumberOfLines"/>.</value>
        public int LineNumber
        {
            get { return _lineNumber; }
            set
            {
                if (SetProperty(ref _lineNumber, value))
                    OkCommand.RaiseCanExecuteChanged();
            }
        }
        private int _lineNumber = 1;


        /// <summary>
        /// Gets the 'OK' command.
        /// </summary>
        public DelegateCommand OkCommand { get; }


        /// <summary>
        /// Gets the 'Cancel' command.
        /// </summary>
        public DelegateCommand CancelCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="GoToLineViewModel"/> class.
        /// </summary>
        public GoToLineViewModel()
        {
            DisplayName = "Go To Line";
            OkCommand = new DelegateCommand(Ok, CanOk);
            CancelCommand = new DelegateCommand(Cancel);
        }


        private bool CanOk()
        {
            return 1 <= LineNumber && LineNumber <= NumberOfLines;
        }


        private void Ok()
        {
            DialogResult = true;     // Remember: Setting the DialogResult closes the Dialog!
        }


        private void Cancel()
        {
            DialogResult = false;
        }
    }
}
