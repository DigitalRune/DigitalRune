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
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <remarks>
    /// <strong>Important:</strong> In Silverlight, the event handlers that handle the
    /// <see cref="CanExecuteChanged"/>-event need to be public methods (no private, protected or
    /// anonymous methods). This is necessary because of security restrictions in Silverlight. It is
    /// recommended to handle the commands using the <see cref="EventToCommand"/> action in XAML!
    /// </remarks>
    public class DelegateCommand<T> : IDelegateCommand
    {
        // See notes in DelegateCommand.cs.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        // See also How to: Identify a Nullable Type (C# Programming Guide), https://msdn.microsoft.com/en-us/library/ms366789.aspx
        private static readonly bool IsNotNullableValueType = 
            typeof(T).IsValueType && !(typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>));

        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
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
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
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
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter == null && IsNotNullableValueType)
                return false;

            return _canExecute == null || _canExecute((T)parameter);
        }


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }


        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this command can be executed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }


        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        public void Execute(T parameter)
        {
            _execute(parameter);
        }


        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> on the UI thread so every command invoker can
        /// requery to check if the <see cref="DelegateCommand"/> can execute. <remarks> Note that
        /// this will trigger the execution of <see cref="CanExecute"/> once for each invoker.
        /// </remarks>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> on the UI thread so every command
        /// invoker can requery <see cref="ICommand.CanExecute"/> to check if the
        /// <see cref="DelegateCommand"/> can execute.
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
