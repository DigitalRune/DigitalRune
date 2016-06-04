// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Defines an item that invokes an <see cref="ICommand"/> of a given type.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ICommand"/>.</typeparam>
    public abstract class CommandItem<T> : CommandItem where T : ICommand
    {
        /// <summary>
        /// Gets (or sets) the command.
        /// </summary>
        /// <value>The command.</value>
        public new T Command { get; protected set; }


        /// <summary>
        /// Gets the command as <see cref="ICommand"/>.
        /// </summary>
        /// <value>The command as <see cref="ICommand"/>.</value>
        internal override ICommand CommandAsICommand
        {
            get { return Command; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandItem{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        protected CommandItem(string name)
          : base(name)
        {
        }
    }
}
