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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Navigation;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Default implementation of the <see cref="IWindowService"/> for WPF.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="WindowManager"/> uses a <see cref="IViewLocator"/> to find views for the
    /// view models. The <see cref="IViewLocator"/> is optional. If no <see cref="IViewLocator"/>
    /// is used or if the <see cref="IViewLocator"/> returns <see langword="null"/> for a view
    /// model, then the window manager creates a <see cref="Window"/> and sets the window content
    /// to the view model. This approach can be used if views are defined using implicit data 
    /// templates in the application resources.
    /// </para>
    /// <para>
    /// <see cref="ShowWindow"/> will always create new windows. The method cannot be used to 
    /// activate a hidden window.
    /// </para>
    /// </remarks>
    public class WindowManager : IWindowService
    {
        // TODO: Improve WindowManager for navigation-based WPF applications.
        // The WindowManager can be used to show pages in an NavigationWindow. However,
        // the code is not as sophisticated as the PhoneNavigationService in Windows
        // Phone. If necessary, port functionality from Windows Phone project.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IViewLocator _viewLocator;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManager"/> class.
        /// </summary>
        /// <param name="viewLocator">The view locator. Can be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewLocator"/> is <see langword="null"/>.
        /// </exception>
        public WindowManager(IViewLocator viewLocator)
        {
            _viewLocator = viewLocator;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public bool? ShowDialog(object viewModel, object context = null)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var window = CreateWindow(viewModel, context, asChildWindow: true, asDialog: true);
            return window.ShowDialog();
        }


        /// <inheritdoc/>
        public void ShowWindow(object viewModel, object context = null, bool asChildWindow = true)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            NavigationWindow navigationWindow = null;

            if (Application.Current != null && Application.Current.MainWindow != null)
                navigationWindow = Application.Current.MainWindow as NavigationWindow;

            if (navigationWindow != null)
            {
                var window = CreatePage(viewModel, context);
                navigationWindow.Navigate(window);
            }
            else
            {
                var window = CreateWindow(viewModel, context, asChildWindow, false);
                window.Show();
            }
        }


        /// <summary>
        /// Creates the <see cref="Page"/> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="Page"/>.</returns>
        protected virtual Page CreatePage(object viewModel, object context)
        {
            var view = _viewLocator?.GetView(viewModel, null, context);
            var page = EnsurePage(viewModel, view);
            page.DataContext = viewModel;

            var activatable = viewModel as IActivatable;
            if (activatable != null)
            {
                activatable.OnActivate();
                page.Unloaded += (s, e) => activatable.OnDeactivate(true);
            }

            return page;
        }


        private static Page EnsurePage(object viewModel, object view)
        {
            var page = view as Page;
            if (page == null)
            {
                page = new Page { Content = view ?? viewModel };

                var hasDisplayName = viewModel as IDisplayName;
                if (hasDisplayName != null)
                {
                    var binding = new Binding("DisplayName");
                    page.SetBinding(Page.TitleProperty, binding);
                }
            }

            return page;
        }


        /// <summary>
        /// Creates the <see cref="Window"/> for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="asChildWindow">
        /// <see langword="true"/> if the window is a child window; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <param name="asDialog">
        /// <see langword="true"/> if the window will be shown as a modal dialog using
        /// <see cref="Window.ShowDialog"/>; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>The <see cref="Window"/>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        protected virtual Window CreateWindow(object viewModel, object context, bool asChildWindow, bool asDialog)
        {
            var view = _viewLocator?.GetView(viewModel, null, context);
            var window = EnsureWindow(viewModel, view, asChildWindow, asDialog);
            window.DataContext = viewModel;

            // Install a conductor which controls the activation-deactivation life cycle.
            new WindowConductor(viewModel, window);

            return window;
        }


        private static Window EnsureWindow(object viewModel, object view, bool asChildWindow, bool asDialog)
        {
            var window = view as Window;
            if (window == null)
            {
                window = new Window
                {
                    Content = view ?? viewModel,
                    SizeToContent = SizeToContent.WidthAndHeight,
                };
                window.Loaded += (sender, e) => window.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

                if (asDialog)
                {
                    window.ResizeMode = ResizeMode.NoResize;

                    // According to Windows Style Guides: Model dialog should not have system menu.
                    WindowsHelper.SetShowIcon(window, false);
                }
            }

            var hasDisplayName = viewModel as IDisplayName;
            if (hasDisplayName != null
                && window.ReadLocalValue(Window.TitleProperty) == DependencyProperty.UnsetValue // No local value.
                && BindingOperations.GetBinding(window, Window.TitleProperty) == null)          // No binding.
            {
                // Bind Window.Title to ViewModel.DisplayName.
                var binding = new Binding("DisplayName");
                window.SetBinding(Window.TitleProperty, binding);
            }

            var owner = InferOwnerOf(window);
            if (owner != null && asChildWindow)
            {
                window.ShowInTaskbar = false;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = owner;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            var hasDialogResult = viewModel as IDialogResult;
            if (asDialog && hasDialogResult != null)
            {
                var behaviors = Interaction.GetBehaviors(window);
                if (!behaviors.OfType<DialogResultBehavior>().Any())
                {
                    // Bind Window.DialogResult to viewModel.DialogResult using a behavior.
                    var dialogResultBehavior = new DialogResultBehavior();
                    var binding = new Binding("DialogResult") { Mode = BindingMode.TwoWay };
                    BindingOperations.SetBinding(dialogResultBehavior, DialogResultBehavior.DialogResultProperty, binding);
                    behaviors.Add(dialogResultBehavior);
                }
            }

            return window;
        }


        /// <summary>
        /// Returns in this order: the currently active window or the main window or null.
        /// </summary>
        private static Window InferOwnerOf(Window window)
        {
            if (Application.Current == null)
                return null;

            var activeWindow = Application.Current.Windows
                                    .OfType<Window>()
                                    .FirstOrDefault(x => x.IsActive);
            activeWindow = activeWindow ?? Application.Current.MainWindow;
            return (activeWindow != window) ? activeWindow : null;
        }
        #endregion
    }
}
