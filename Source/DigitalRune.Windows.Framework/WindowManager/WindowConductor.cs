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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using DigitalRune.Linq;

#if WINDOWS_PHONE
using Window = DigitalRune.Windows.Framework.DialogHost;
using ClosingEventHandler = System.EventHandler<System.ComponentModel.CancelEventArgs>;
using ClosedEventHandler = System.EventHandler<System.EventArgs>;
#elif SILVERLIGHT
using Window = System.Windows.Controls.ChildWindow;
using ClosingEventHandler = System.EventHandler<System.ComponentModel.CancelEventArgs>;
using ClosedEventHandler = System.EventHandler;
#else
using Window = System.Windows.Window;
using ClosingEventHandler = System.ComponentModel.CancelEventHandler;
using ClosedEventHandler = System.EventHandler;
#endif


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A <see cref="IConductor"/> that controls a window.
    /// </summary>
    internal class WindowConductor : IConductor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly object _viewModel;
        private readonly Window _view;
        private bool _deactivatingFromView;
        private bool _deactivateFromViewModel;
        private bool _actuallyClosing;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowConductor"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="view">The view.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="viewModel"/> or <paramref name="view"/> is <see langword="null"/>.
        /// </exception>
        public WindowConductor(object viewModel, Window view)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _viewModel = viewModel;
            _view = view;

            // Set conductor.
            var screen = viewModel as IScreen;
            if (screen != null)
                screen.Conductor = this;

            var activatable = viewModel as IActivatable;
            if (activatable != null)
            {
                activatable.OnActivate();
                activatable.Deactivated += OnDeactivated;
            }

            var guardClose = viewModel as IGuardClose;
            if (guardClose != null)
                view.Closing += OnViewClosing;

            view.Closed += OnViewClosed;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnViewClosing(object sender, CancelEventArgs eventArgs)
        {
            if (eventArgs.Cancel)
            {
                // The closing has already been canceled.
                return;
            }

            if (_actuallyClosing)
            {
                // The closing was initiated by the view model.
                _actuallyClosing = false;
                return;
            }

            Debug.Assert(_viewModel is IGuardClose, "The OnViewClosing method should only be registered if the view model is IGuardClose.");
            var guardClose = (IGuardClose)_viewModel;

            // Only close the view if CanClose() allows it.
            Task<bool> canClose = guardClose.CanCloseAsync();
            if (canClose.IsCompleted)
            {
                // The CanCloseAsync() returned immediately and we have the final answer.
                eventArgs.Cancel = !canClose.Result;
                _actuallyClosing = canClose.Result;
            }
            else
            {
                // The CanCloseAsync() was not executed immediately. Therefore, we cancel the
                // closing here and if necessary call Close() when the result is available.
                eventArgs.Cancel = true;
                canClose.ContinueWith(t =>
                                      {
                                          if (t.Result)
                                          {
                                              _actuallyClosing = true;
                                              _view.Close();
                                          }
                                      }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }


        private void OnViewClosed(object sender, EventArgs eventArgs)
        {
            // Unregister event handlers from view.
            _view.Closing -= OnViewClosing;
            _view.Closed -= OnViewClosed;

#if !SILVERLIGHT && !WINDOWS_PHONE
            // Move focus back to parent window.
            // (Ideally, we do this only if the window was active before closing. However,
            // we cannot check _view.IsActive because this view is already closed. We could 
            // check IsActive in OnViewClosing or keep track somewhere else. --> Just ignore this.)
            var owner = _view.Owner;
            if (owner != null && !owner.IsActive)
            {
                owner.Activate();
            }
#endif

            if (_deactivateFromViewModel)
            {
                // The view model is already deactivated.
                return;
            }

            // Deactivate view model.
            var activatable = _viewModel as IActivatable;
            if (activatable != null)
            {
                _deactivatingFromView = true;
                activatable.OnDeactivate(true);
                _deactivatingFromView = false;
            }
        }


        private void OnDeactivated(object sender, DeactivationEventArgs eventArgs)
        {
            if (!eventArgs.Closed)
            {
                // Do nothing if view is only hidden temporarily.
                return;
            }

            // Unregister event handler from view model.
            var activatable = _viewModel as IActivatable;
            if (activatable != null)
                activatable.Deactivated -= OnDeactivated;

            // Clear conductor.
            var screen = _viewModel as IScreen;
            if (screen != null)
                screen.Conductor = null;

            if (_deactivatingFromView)
            {
                // The view is already closing.
                return;
            }

            // Close view.
            _deactivateFromViewModel = true;
            _actuallyClosing = true;
            _view.Close();
            _actuallyClosing = false;
            _deactivateFromViewModel = false;
        }
        #endregion


        //--------------------------------------------------------------
        #region IConductor
        //--------------------------------------------------------------

        /// <inheritdoc/>
        IEnumerable<object> IConductor.Items
        {
            get { return LinqHelper.Return(_viewModel); }
        }


        /// <inheritdoc/>
        IEnumerable<object> IConductor.ActiveItems
        {
            get
            {
                return LinqHelper.Return(_viewModel);
            }
        }


        /// <summary>
        /// Gets the item which is controlled by this conductor.
        /// </summary>
        /// <value>The item conducted by this conductor.</value>
        public object Item
        {
            get { return _viewModel; }
        }


        /// <inheritdoc/>
        Task<bool> IConductor.ActivateItemAsync(object item)
        {
            // Explicit interface implementation because this method is not really async.

            ActivateItem(item);
            return TaskHelper.FromResult(true);
        }


        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is not the current item. <see cref="ActivateItem"/> may only be
        /// called for the current item. Setting a new item is not supported.
        /// </exception>
        public void ActivateItem(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!Equals(item, Item))
                throw new ArgumentException("Changing the item is not supported by the WindowConductor.");

            // This method might be called by the view model to show the view (in case it is hidden 
            // or to set the focus).
            (item as IActivatable)?.OnActivate();
            _view.Show();
        }


        /// <inheritdoc/>
        public Task<bool> DeactivateItemAsync(object item, bool close)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!item.Equals(Item))
                throw new ArgumentException("Item is not controlled by this conductor.", nameof(item));

            // This method might be called by the view model to close or hide the view.
            if (close)
            {
                var tcs = new TaskCompletionSource<bool>();

                // ReSharper disable AccessToModifiedClosure
                ClosingEventHandler closingHandler = null;
                ClosedEventHandler closedHandler = null;
                closingHandler = (s, e) =>
                                 {
                                     if (e.Cancel)
                                     {
                                         _view.Closing -= closingHandler;
                                         _view.Closed -= closedHandler;
                                         tcs.TrySetResult(false);
                                     }
                                 };
                closedHandler = (s, e) =>
                                {
                                    _view.Closing -= closingHandler;
                                    _view.Closed -= closedHandler;
                                    tcs.TrySetResult(true);
                                };
                _view.Closing += closingHandler;
                _view.Closed += closedHandler;
                // ReSharper restore AccessToModifiedClosure

                _view.Close();
                return tcs.Task;
            }

#if SILVERLIGHT || WINDOWS_PHONE
            throw new InvalidOperationException("Temporarily hiding a window or dialog in Silverlight is not supported.");
#else
            _view.Hide();
            (_viewModel as IActivatable)?.OnDeactivate(false);
            return TaskHelper.FromResult(true);
#endif
        }
        #endregion
    }
}
