// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents a command that calls .NET delegates to execute and query the state.
    /// </summary>
    public interface IDelegateCommand : ICommand
    {
        /// <summary>
        /// Raises <see cref="DelegateCommand.CanExecuteChanged"/> so every command invoker can
        /// requery to check if the <see cref="DelegateCommand"/> can execute.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger the execution of <see cref="DelegateCommand.CanExecute"/>
        /// once for each invoker.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        void RaiseCanExecuteChanged();
    }
}
