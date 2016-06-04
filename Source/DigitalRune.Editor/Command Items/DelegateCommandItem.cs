// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Defines an item that invokes a <see cref="DelegateCommand"/>.
    /// </summary>
    public class DelegateCommandItem : CommandItem<IDelegateCommand>
    {
        /// <summary>
        /// Gets or sets the input gestures that trigger the <see cref="CommandItem{T}.Command"/> of
        /// this item.
        /// </summary>
        /// <value>The input gestures. The default value is <see langword="null"/>.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public new InputGestureCollection InputGestures
        {
            get { return base.InputGestures; }
            set { base.InputGestures = value; }   // Setter is protected in the base class!
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommandItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public DelegateCommandItem(string name, IDelegateCommand command)
          : base(name)
        {
            Command = command;
        }
    }
}
