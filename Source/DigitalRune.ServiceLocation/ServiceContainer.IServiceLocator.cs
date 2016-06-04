// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.ServiceLocation;


namespace DigitalRune.ServiceLocation
{
    partial class ServiceContainer : IServiceLocator
    {
        /// <overloads>
        /// <summary> Gets all named instances of a given service type. </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets all named instances of the given service type currently registered in the
        /// container.
        /// </summary>
        /// <param name="serviceType">The type of the service requested.</param>
        /// <returns>
        /// A sequence of instances of the requested <paramref name="serviceType"/>.
        /// </returns>
        /// <remarks>
        /// This method returns only "named" instances. An instance is "named" if it was registered
        /// with a key that is not <see langword="null"/> (see method
        /// <see cref="Register(Type, string, object)"/> and its overloads).
        /// </remarks>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            ThrowIfDisposed();

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            try
            {
                return GetAllInstancesImpl(serviceType);
            }
            catch (Exception exception)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture, 
                    "Activation error occurred while trying to get all instances of type {0}.",
                    serviceType.Name);

                throw new ActivationException(message, exception);
            }
        }


        /// <summary>
        /// Gets all named instances of the given service type currently registered in the
        /// container.
        /// </summary>
        /// <typeparam name="TService">The type of the service requested.</typeparam>
        /// <returns>
        /// A sequence of instances of the requested <typeparamref name="TService"/>.
        /// </returns>
        /// <remarks>
        /// This method returns only "named" instances. An instance is "named" if it was registered
        /// with a key that is not <see langword="null"/> (see method
        /// <see cref="Register(Type, string, object)"/> and its overloads).
        /// </remarks>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return GetAllInstances(typeof(TService)).Cast<TService>();
        }


        /// <overloads>
        /// <summary> 
        /// Gets an instance of the given service type.
        /// </summary>
        /// </overloads>
        /// <summary>
        /// Get an instance of the given service type.
        /// </summary>
        /// <param name="serviceType">The type of the service requested.</param>
        /// <returns>
        /// The requested service instance or <see langword="null"/> if the service has not been
        /// registered.
        /// </returns>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public object GetInstance(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }


        /// <summary>
        /// Gets a named instance of the given service type.
        /// </summary>
        /// <param name="serviceType">The type of the service requested.</param>
        /// <param name="key">
        /// The name the object was registered with. Can be <see langword="null"/> or empty. 
        /// </param>
        /// <returns>
        /// The requested service instance or <see langword="null"/> if the service has not been 
        /// registered.
        /// </returns>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public object GetInstance(Type serviceType, string key)
        {
            ThrowIfDisposed();

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            try
            {
                return GetInstanceImpl(serviceType, key);
            }
            catch (Exception exception)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture, 
                    "Activation error occurred while trying to get instance of type {0}, key \"{1}\"",
                    serviceType.Name, key);

                throw new ActivationException(message, exception);
            }
        }


        /// <summary>
        /// Gets an instance of the given service type.
        /// </summary>
        /// <typeparam name="TService">The type of the service requested.</typeparam>
        /// <returns>
        /// The requested service instance or <see langword="null"/> if the service has not been 
        /// registered.
        /// </returns>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public TService GetInstance<TService>()
        {
            return (TService)GetInstance(typeof(TService), null);
        }


        /// <summary>
        /// Gets a named instance of the given service type.
        /// </summary>
        /// <typeparam name="TService">The type of the service requested.</typeparam>
        /// <param name="key">
        /// The name the object was registered with. Can be <see langword="null"/> or empty. 
        /// </param>
        /// <returns>
        /// The requested service instance or <see langword="null"/> if the service has not been 
        /// registered.
        /// </returns>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving the service instance.
        /// </exception>
        public TService GetInstance<TService>(string key)
        {
            return (TService)GetInstance(typeof(TService), key);
        }
    }
}
