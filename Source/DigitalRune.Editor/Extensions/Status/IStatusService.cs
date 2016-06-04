// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Manages status information.
    /// </summary>
    public interface IStatusService
    {
        /// <summary>
        /// Shows a status information in the status bar.
        /// </summary>
        /// <param name="viewModel">The view model of the status information.</param>
        /// <exception cref="InvalidOperationException">
        /// Invalid cross-thread access. The method needs to be invoked on the UI thread.
        /// </exception>
        void Show(StatusViewModel viewModel);
    }
}
