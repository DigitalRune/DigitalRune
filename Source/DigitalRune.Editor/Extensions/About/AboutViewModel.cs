// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Windows.Framework;
using static System.FormattableString;


namespace DigitalRune.Editor.About
{
    /// <summary>
    /// Represents the About dialog.
    /// </summary>
    internal class AboutViewModel : Screen
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// An <see cref="AboutViewModel"/> instance that can be used at design-time.
        /// </summary>
        internal static AboutViewModel DesignInstance
        {
            get { return new AboutViewModel(new DesignTimeAboutService()); }
        }


        /// <summary>
        /// Gets the About dialog service.
        /// </summary>
        /// <value>The About dialog service.</value>
        public IAboutService AboutService { get; }


        /// <summary>
        /// Gets the command that closes the About dialog.
        /// </summary>
        /// <value>The command that closes the About dialog.</value>
        public DelegateCommand CloseAboutDialogCommand { get; private set; }


        /// <summary>
        /// Gets the command that copies the About dialog information to the clipboard.
        /// </summary>
        /// <value>The command that copies the About dialog information to the clipboard.</value>
        public DelegateCommand CopyToClipboardCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
        /// </summary>
        /// <param name="aboutService">The <see cref="IAboutService"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="aboutService"/> is <see langword="null"/>.
        /// </exception>
        public AboutViewModel(IAboutService aboutService)
        {
            if (aboutService == null)
                throw new ArgumentNullException(nameof(aboutService));

            AboutService = aboutService;
            DisplayName = Invariant($"About {AboutService.ApplicationName}");
            CloseAboutDialogCommand = new DelegateCommand(() => Conductor.DeactivateItemAsync(this, true));
            CopyToClipboardCommand = new DelegateCommand(() => AboutService.CopyInformationToClipboard());
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
