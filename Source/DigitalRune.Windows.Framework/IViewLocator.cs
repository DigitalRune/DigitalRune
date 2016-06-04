// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
#if WINDOWS_PHONE
using System;
#endif


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A strategy for determining which view to use for a specific view model.
    /// </summary>
    public interface IViewLocator
    {
        /// <summary>
        /// Resolves the view for the specified view model in the current context.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="parent">The parent control. Can be <see langword="null"/>.</param>
        /// <param name="context">The context. Can be <see langword="null"/>.</param>
        /// <returns>
        /// The view or <see langword="null"/> if no matching view is found.
        /// </returns>
        FrameworkElement GetView(object viewModel, DependencyObject parent = null, object context = null);


#if WINDOWS_PHONE
        /// <summary>
        /// Resolves the URI for the specified view model in the current context.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context. Can be <see langword="null"/>.</param>
        /// <returns>
        /// A <see cref="Uri"/> object initialized with the URI for the desired view.
        /// </returns>
        Uri GetUri(object viewModel, object context = null);
#endif
    }
}
