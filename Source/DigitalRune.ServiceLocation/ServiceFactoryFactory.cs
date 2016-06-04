// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.ServiceLocation
{
    /// <summary>
    /// Creates a <see cref="Func{TResult}"/> which returns a service instance.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    internal sealed class ServiceFactoryFactory<T>
    {
        /// <summary>
        /// Creates a factory method for services of type <typeparamref name="T"/> and a specific
        /// container.
        /// </summary>
        /// <param name="container">The <see cref="ServiceContainer"/>.</param>
        /// <param name="key">
        /// The name the service was registered with. Can be <see langword="null"/> or empty. 
        /// </param>
        /// <returns>The factory method.</returns>
        public static Func<T> Create(ServiceContainer container, string key)
        {
            return () => (T)container.GetInstance(typeof(T), key);
        }
    }
}
