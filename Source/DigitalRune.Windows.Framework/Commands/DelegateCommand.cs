// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A command that calls .NET delegates to execute and query the state.
    /// </summary>
    /// <remarks>
    /// <strong>Important:</strong> In Silverlight, the event handlers that handle the
    /// <see cref="CanExecuteChanged"/>-event need to be public methods (no private, protected or
    /// anonymous methods). This is necessary because of security restrictions in Silverlight. It is
    /// recommended to handle the commands using the <see cref="EventToCommand"/> action in XAML!
    /// </remarks>
    public class DelegateCommand : IDelegateCommand
    {
        // -----------------------------------------------------------------------------------------
        // UPDATE: WPF 4.5 introduced the CanExecuteChangedEventManager. (Used in ButtonBase and
        // MenuItem.) CanExecutedChanged can be a strong event. Controls and Behaviors need to
        // attach to event using weak-event manager.
        // -----------------------------------------------------------------------------------------
        // WPF 4, Silverlight:
        // The DelegateCommand must not create a strong reference of the event handler. Therefore,
        // the CanExecuteChanged event needs to be a "weak event".

        // This implementation of the DelegateCommand is the correct implementation for WPF. It is -
        // in my opinion - also the best implementation for Silverlight. Its only disadvantage is
        // that it does not work with the Silverlight 4 Button, because the CanExecuteChanged event
        // handler of the button is not a public method.

        // Other version of the DelegateCommand:
        // - PRISM's DelegateCommand implementation stores the event handler using a weak reference.
        //   This works in WPF, but does not work with EventToCommand action (WPF or Silverlight) or
        //   the Button in Silverlight 4 because the event handlers might get garbage collected.
        //   (The EventToCommand action and the Silverlight 4 Button do not explicitly store the
        //   event handler using a strong reference. The Silverlight 4 Button implementation is not
        //   conform to the WPF implementation.)

        // - The RelayCommand in the MVVM Light Toolkit v3 stores the event handler as a strong
        //   reference and is therefore wrong. (This is an official bug.)

        // Solution: Always use our DelegateCommand together with the EventToCommand action in XAML.
        // This combination works in WPF, Silverlight and Windows Phone!
        // -----------------------------------------------------------------------------------------


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
#if !NET45
        private readonly WeakEvent<EventHandler> _canExecuteChangedEvent;
#endif
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

#if NET45
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
#else
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        /// <remarks>
        /// <strong>Important:</strong> In Silverlight, the event handlers that handle the
        /// <see cref="CanExecuteChanged"/>-event need to be public methods (no private, protected
        /// or anonymous methods). This is necessary because of security restrictions in
        /// Silverlight.
        /// </remarks>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecuteChangedEvent != null)
                    _canExecuteChangedEvent.Add(value);
            }
            remove
            {
                if (_canExecuteChangedEvent != null)
                    _canExecuteChangedEvent.Remove(value);
            }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execute">The action that is performed when the command is executed.</param>
        /// <param name="canExecute">
        /// A predicate that determines whether the command can be executed or not.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="execute"/> is <see langword="null"/>.
        /// </exception>
        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;

            if (canExecute != null)
            {
                _canExecute = canExecute;
#if !NET45
                _canExecuteChangedEvent = new WeakEvent<EventHandler>();
#endif
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Ignored by this implementation.</param>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Ignored by this implementation.</param>
        void ICommand.Execute(object parameter)
        {
            Execute();
        }


        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool CanExecute()
        {

            return _canExecute == null || _canExecute();
        }


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        public void Execute()
        {
            _execute();
        }


        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker can requery to check if
        /// the <see cref="DelegateCommand"/> can execute.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger the execution of <see cref="CanExecute"/> once for each
        /// invoker.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> so every command invoker can requery
        /// <see cref="ICommand.CanExecute"/> to check if the <see cref="DelegateCommand{T}"/> can
        /// execute.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
#if NET45
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
#else
            if (_canExecuteChangedEvent != null && _canExecuteChangedEvent.Count > 0)
            {
                WindowsHelper.CheckBeginInvokeOnUI(() =>
                {
                    _canExecuteChangedEvent.Invoke(this, EventArgs.Empty);
                });
            }
#endif
        }
        #endregion
    }
}
