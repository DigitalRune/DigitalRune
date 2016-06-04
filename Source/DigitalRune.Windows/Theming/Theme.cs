// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows
{
    /// <summary>
   /// Describes a WPF theme which consists of a name and a resource dictionary.
    /// </summary>
    public sealed class Theme : INamedObject
    {
        /// <summary>
        /// Gets the name of the WPF theme.
        /// </summary>
        /// <value>The name of the WPF theme.</value>
        public string Name { get; }


        /// <summary>
        /// Gets the URI of the resource dictionary that defines the WPF theme.
        /// </summary>
        /// <value>The URI of the resource dictionary.</value>
        public Uri Source { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Theme" /> class.
        /// </summary>
        /// <param name="name">The name of the theme.</param>
        /// <param name="source">The URI of the resource dictionary that defines the WPF theme.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is empty.
        /// </exception>
        public Theme(string name, Uri source)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("The name must not be empty.", nameof(name));
            if (source == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Source = source;
        }


        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "Unnamed theme" : Name;
        }
    }
}
