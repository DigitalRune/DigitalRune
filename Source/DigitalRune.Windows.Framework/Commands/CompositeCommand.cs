#region ----- Copyright -----
/*
  The classes in this file are derived from the CompositeCommand from "patterns and practices 
  Composite WPF and Silverlight" (http://compositewpf.codeplex.com/) licensed under Ms-PL (see 
  below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Combines one or more <see cref="ICommand"/>s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> The <see cref="CompositeCommand"/> only executes if
    /// <strong>all</strong> registered commands can execute.
    /// </para>
    /// <para>
    /// In Silverlight, the event handlers that handle the <see cref="CanExecuteChanged"/>-event
    /// need to be public methods (no private, protected or anonymous methods). This is necessary
    /// because of security restrictions in Silverlight. It is recommended to handle the commands
    /// using the <see cref="EventToCommand"/> action in XAML!
    /// </para>
    /// </remarks>
    public class CompositeCommand : ICommand
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly List<ICommand> _registeredCommands = new List<ICommand>();
#if !NET45
        private readonly WeakEvent<EventHandler> _canExecuteChangedEvent = new WeakEvent<EventHandler>();
#endif
        private readonly EventHandler _onRegisteredCommandCanExecuteChangedHandler;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the list of all the registered commands.
        /// </summary>
        /// <value>A list of registered commands.</value>
        /// <remarks>
        /// This returns a copy of the commands subscribed to the <see cref="CompositeCommand"/>.
        /// </remarks>
        public IList<ICommand> RegisteredCommands
        {
            get
            {
                IList<ICommand> commandList;
                lock (_registeredCommands)
                {
                    commandList = _registeredCommands.ToList();
                }

                return commandList;
            }
        }


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
            add { _canExecuteChangedEvent.Add(value); }
            remove { _canExecuteChangedEvent.Remove(value); }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCommand"/> class.
        /// </summary>
        public CompositeCommand()
        {
            // Store event handler in member variable. This is necessary because most other ICommand
            // implementations are wrong and the event handler might get garbage collected.
            _onRegisteredCommandCanExecuteChangedHandler = OnRegisteredCommandCanExecuteChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Adds a command to the collection and signs up for the
        /// <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        /// <param name="command">The command to register.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="command"/> is a reference of this <see cref="CompositeCommand"/>. Cannot
        /// register CompositeCommand in itself.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="command"/> is already registered. Cannot register same
        /// <see cref="ICommand"/> twice.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public virtual void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (command == this)
                throw new ArgumentException("Cannot register CompositeCommand in itself.");

            lock (_registeredCommands)
            {
                if (_registeredCommands.Contains(command))
                    throw new InvalidOperationException("Cannot register same ICommand twice.");

                _registeredCommands.Add(command);
            }

            command.CanExecuteChanged += _onRegisteredCommandCanExecuteChangedHandler;
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Removes a command from the collection and removes itself from the
        /// <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        /// <param name="command">The command to unregister.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <see langword="null"/>.
        /// </exception>
        public virtual void UnregisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            bool removed;
            lock (_registeredCommands)
            {
                removed = _registeredCommands.Remove(command);
            }

            if (removed)
            {
                command.CanExecuteChanged -= _onRegisteredCommandCanExecuteChangedHandler;
                OnCanExecuteChanged();
            }
        }


#if SILVERLIGHT || WINDOWS_PHONE
        // The CanExecuteChanged handler needs to be public because of security restrictions in
        // Silverlight. (The CanExecuteChanged event is usually implemented as a weak-event which
        // requires reflection.)

        /// <exclude/>
        public
#else
        private
#endif
        void OnRegisteredCommandCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            OnCanExecuteChanged();
        }


        /// <summary>
        /// Forwards <see cref="ICommand.CanExecute"/> to the registered commands and returns
        /// <see langword="true"/> if all of the commands return <see langword="true"/>.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if all of the commands return <see langword="true"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public virtual bool CanExecute(object parameter)
        {
            bool hasCommandsThatCanExecute = false;

            ICommand[] commandList;
            lock (_registeredCommands)
            {
                commandList = _registeredCommands.ToArray();
            }

            foreach (ICommand command in commandList)
            {
                if (!command.CanExecute(parameter))
                    return false;

                hasCommandsThatCanExecute = true;
            }

            return hasCommandsThatCanExecute;
        }


        /// <summary>
        /// Forwards <see cref="ICommand.Execute"/> to the registered commands.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object
        /// can be set to <see langword="null"/>.
        /// </param>
        public virtual void Execute(object parameter)
        {
            ICommand[] commands;
            lock (_registeredCommands)
            {
                commands = _registeredCommands.ToArray();
            }

            foreach (ICommand command in commands)
            {
                command.Execute(parameter);
            }
        }


        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> on the UI thread so every command
        /// invoker can requery <see cref="ICommand.CanExecute"/> to check if the
        /// <see cref="CompositeCommand"/> can execute.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
#if NET45
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
#else
            if (_canExecuteChangedEvent.Count > 0)
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
