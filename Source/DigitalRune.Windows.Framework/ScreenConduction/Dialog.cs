// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Base implementation of a screen which represents a dialog box.
    /// </summary>
    /// <remarks>
    /// Important: Setting the <see cref="DialogResult"/> must close the dialog. This is not handled
    /// by this class! (When using the <see cref="WindowManager"/>, the <see cref="WindowManager"/>
    /// takes care of this by binding the <see cref="DialogResult"/> property to the 
    /// <see cref="Window.DialogResult"/> property of the WPF <see cref="Window"/>. When the 
    /// dialog result of a window is set, WPF window will close the window automatically.)
    /// </remarks>
    public class Dialog : Screen, IDialogResult
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the dialog box was accepted or canceled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the dialog box was accepted; <see langword="false"/> if the
        /// dialog box was canceled. The default is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Setting the dialog result automatically closes the dialog!
        /// </remarks>
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value); }
        }
        private bool? _dialogResult;
    }
}
