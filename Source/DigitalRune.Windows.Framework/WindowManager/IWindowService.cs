// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System;

#if SILVERLIGHT || WINDOWS_PHONE
using System.Threading.Tasks;
#endif


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Manages windows and dialog boxes.
    /// </summary>
    public interface IWindowService
    {
#if WINDOWS_PHONE
        /// <summary>
        /// Shows a modal dialog for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">
        /// The context. This object will be given to the <see cref="IViewLocator"/>.
        /// </param>
        /// <param name="closeOnBackKeyPress">
        /// If set to <see langword="true"/> the dialog will be closed when the Back button is
        /// pressed and the page navigation will be canceled. Otherwise, if set to
        /// <see langword="false"/> the Back button will not be caught and will usually invoke a
        /// page navigation.
        /// </param>
        /// <param name="hideApplicationBar">
        /// If set to <see langword="true"/> application bar will be hidden while the dialog is
        /// shown.
        /// </param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        Task ShowDialogAsync(object viewModel, object context = null, bool closeOnBackKeyPress = true, bool hideApplicationBar = false);
#elif SILVERLIGHT
        /// <summary>
        /// Asynchronously shows a modal dialog for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">
        /// The context. This object will be given to the <see cref="IViewLocator"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The result of the task is the dialog
        /// result value: A <see cref="Nullable{T}"/> value of type <see cref="bool"/> that
        /// specifies whether the dialog was accepted (true) or canceled (false).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        Task<bool?> ShowDialogAsync(object viewModel, object context = null);
#else
        /// <summary>
        /// Shows a modal dialog for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">
        /// The context. This object will be given to the <see cref="IViewLocator"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Nullable{T}"/> value of type <see cref="bool"/> that specifies whether the
        /// dialog was accepted (true) or canceled (false). The return value is the value of the
        /// <strong>DialogResult</strong> property before the window closes.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        bool? ShowDialog(object viewModel, object context = null);
#endif


#if SILVERLIGHT
        /// <summary>
        /// Shows a toast notification for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">
        /// The context. This object will be given to the <see cref="IViewLocator"/>.
        /// </param>
        /// <param name="durationInMilliseconds">
        /// The duration that the toast notification should remain visible, specified in
        /// milliseconds. The default value is 7000 (1s fade-in + 5s visible + 1s fade-out).
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        void ShowNotification(object viewModel, object context = null, int durationInMilliseconds = 7000);
#endif


#if !SILVERLIGHT && !WINDOWS_PHONE
        /// <summary>
        /// Shows a non-modal window for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">
        /// The context. This object will be given to the <see cref="IViewLocator"/>.
        /// </param>
        /// <param name="asChildWindow">
        /// <see langword="true"/> if the window is a child window; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> is <see langword="null"/>.
        /// </exception>
        void ShowWindow(object viewModel, object context = null, bool asChildWindow = true);
#endif
    }
}
