// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Manages a list of <see cref="ICommandItem"/>s.
    /// </summary>
    public class CommandItemCollection : NamedObjectCollection<ICommandItem>
    {
        /// <summary>
        /// Adds the specified commands to the collection.
        /// </summary>
        /// <param name="commands">The commands to add.</param>
        public void Add(params ICommandItem[] commands)
        {
            AddRange(commands);
        }
    }
}
