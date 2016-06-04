// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Manages status information.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IStatusService"/></item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class StatusExtension : EditorExtension, IStatusService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ResourceDictionary _resourceDictionary;

        // A OneActiveItemsConductor. We add StatusViewModels in Show. The StatusViewModels remove
        // themselves automatically. The conductor is itself a screen which is conducted manually.
        private StatusConductor _statusConductor;

        private TotalMemoryViewModel _totalMemory;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(IStatusService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddDataTemplates();
            AddStatusBarItems();

            // Set default status.
            var status = new StatusViewModel { Message = "Ready" };
            Show(status);
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            RemoveStatusBarItems();
            RemoveDataTemplates();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(IStatusService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Status/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddStatusBarItems()
        {
            // Add StatusConductor to status bar (left).
            _statusConductor = new StatusConductor();
            ((IActivatable)_statusConductor).OnActivate();
            Editor.StatusBarItemsLeft.Add(_statusConductor);

            // Show total managed memory in status bar (right).
            _totalMemory = new TotalMemoryViewModel();
            Editor.StatusBarItemsRight.Add(_totalMemory);
        }


        private void RemoveStatusBarItems()
        {
            // Close conductor and its items without checking CanClose.
            ((IActivatable)_statusConductor).OnDeactivate(true);
            Editor.StatusBarItemsLeft.Remove(_statusConductor);
            _statusConductor = null;

            Editor.StatusBarItemsRight.Remove(_totalMemory);
            _totalMemory.Dispose();
            _totalMemory = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        /// <inheritdoc/>
        public void Show(StatusViewModel viewModel)
        {
            if (!WindowsHelper.CheckAccess())
                throw new InvalidOperationException("Cross-thread operation not valid: The method needs to be invoked on the UI thread.");

            _statusConductor.ActivateItem(viewModel);
        }
        #endregion
    }
}
