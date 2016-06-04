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
using System.Diagnostics;
using System.Threading.Tasks;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Base implementation of <see cref="IScreen"/>.
    /// </summary>
    public class Screen                 // A screen
        : ValidatableObservableObject,  // implements INotifyPropertyChanged, INotifyDataErrorInfo
          IScreen,                      // is aware of its IConductor,
          IActivatable,                 // has an activation logic,
          IGuardClose,                  // may prevent closing,
          IDisplayName                  // and has a title.
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public bool IsOpen
        {
            get { return _isOpen; }
            private set { SetProperty(ref _isOpen, value); }
        }
        private bool _isOpen;


        /// <inheritdoc/>
        public bool IsActive
        {
            get { return _isActive; }
            private set { SetProperty(ref _isActive, value); }
        }
        private bool _isActive;


        /// <inheritdoc/>
        public IConductor Conductor
        {
            get { return _conductor; }
            set
            {
                if (_conductor == value)
                    return;

                if (_conductor != null && value != null)
                    throw new InvalidOperationException("Cannot override conductor. Screen is already being conducted.");

                _conductor = value;
                RaisePropertyChanged();
            }
        }
        private IConductor _conductor;


        /// <summary>
        /// Gets or sets the display name (title).
        /// </summary>
        /// <value>The display name (title) of the screen.</value>
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }
        private string _displayName;


        /// <inheritdoc/>
        public event EventHandler<ActivationEventArgs> Activated;


        /// <inheritdoc/>
        public event EventHandler<DeactivationEventArgs> Deactivated;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Screen"/> class.
        /// </summary>
        public Screen()
        {
            _displayName = GetType().FullName;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IActivatable.OnActivate()
        {
            // Note: The method is an explicit interface implementation because it should never be
            // called directly. It should only be called by an IConductor through the IActivatable
            // interface.
            Debug.Assert(WindowsHelper.CheckAccess(), "Screen conduction not called on UI thread.");
            Debug.Assert(IsOpen || !IsActive, "Invalid state: An item can only be active when it is open.");
            if (IsActive)
                return;

            bool opened = !IsOpen;
            IsOpen = true;
            IsActive = true;
            OnActivated(new ActivationEventArgs(opened));
        }


        /// <summary>
        /// Raises the <see cref="Activated"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="ActivationEventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnActivated"/> in a
        /// derived class, be sure to call the base class's <see cref="OnActivated"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnActivated(ActivationEventArgs eventArgs)
        {
            Activated?.Invoke(this, eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IActivatable.OnDeactivate(bool close)
        {
            // Note: The method is an explicit interface implementation because it should never be
            // called directly. It should only be called by an IConductor through the IActivatable
            // interface.
            Debug.Assert(WindowsHelper.CheckAccess(), "Screen conduction not called on UI thread.");
            Debug.Assert(IsOpen || !IsActive, "Invalid state: An item can only be active when it is open.");
            if (!IsOpen)
                return;

            if (!IsActive && !close)
                return;

            IsActive = false;
            if (close)
                IsOpen = false;

            OnDeactivated(new DeactivationEventArgs(close));
        }


        /// <summary>
        /// Raises the <see cref="Deactivated"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="DeactivationEventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnDeactivated"/> in a
        /// derived class, be sure to call the base class's <see cref="OnDeactivated"/> method so
        /// that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            Deactivated?.Invoke(this, eventArgs);
        }


        /// <inheritdoc/>
        public virtual Task<bool> CanCloseAsync()
        {
            return TaskHelper.FromResult(true);
        }
        #endregion
    }
}
