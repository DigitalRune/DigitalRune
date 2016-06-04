// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
#if NETFX_CORE || NET45
using System.Reflection;
#endif


namespace DigitalRune.ServiceLocation
{
    partial class ServiceContainer
    {
        /// <summary>
        /// Registers the specified service instance.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="instance">The service instance to be registered.</param>
        /// <remarks>
        /// <para>
        /// The service instance will be shared by the container and all child containers (creation
        /// policy <see cref="CreationPolicy.Shared"/>) and will not be disposed when the container
        /// is disposed (disposal policy <see cref="DisposalPolicy.Manual"/>).
        /// </para>
        /// <para>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="instance"/> is not a class (or subclass) of
        /// <paramref name="serviceType"/>.
        /// </exception>
        public void Register(Type serviceType, string key, object instance)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

#if !NETFX_CORE && !NET45
            if (!serviceType.IsInstanceOfType(instance))
#else
            if (!serviceType.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo()))
#endif
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture, 
                    "The service instance (type \"{0}\") is not assignable to this service type (\"{1}\").",
                    instance.GetType().Name, serviceType.Name);

                throw new ArgumentException(message, nameof(instance));
            }

            Register(serviceType, key, container => instance, CreationPolicy.Shared, DisposalPolicy.Manual);
        }


        /// <summary>
        /// Registers the specified service type.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="instanceType">The type implementing the service.</param>
        /// <remarks>
        /// The service instance will be shared by the container and all child containers (creation
        /// policy <see cref="CreationPolicy.Shared"/>) and will be automatically disposed when the
        /// container is disposed (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        /// </remarks>
        /// <remarks>
        /// <para>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="instanceType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="instanceType"/> is not compatible with the
        /// <paramref name="serviceType"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Type instanceType)
        {
            Register(serviceType, key, instanceType, CreationPolicy.Shared, DisposalPolicy.Automatic);
        }


        /// <summary>
        /// Registers the specified service type using a certain creation policy.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="instanceType">The type implementing the service.</param>
        /// <param name="creationPolicy">
        /// The creation policy that specifies when and how a service will be instantiated.
        /// </param>
        /// <remarks>
        /// <para>
        /// The service instance will be automatically disposed when the container is disposed
        /// (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        /// </para>
        /// <para>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="instanceType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="instanceType"/> is not compatible with the
        /// <paramref name="serviceType"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Type instanceType, CreationPolicy creationPolicy)
        {
            Register(serviceType, key, instanceType, creationPolicy, DisposalPolicy.Automatic);
        }


        /// <summary>
        /// Registers the specified service type using a certain creation and disposal policy.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="instanceType">The type implementing the service.</param>
        /// <param name="creationPolicy">
        /// The creation policy that specifies when and how a service will be instantiated.
        /// </param>
        /// <param name="disposalPolicy">
        /// The disposal policy that specifies when a service instance will be disposed. (Only
        /// relevant if the service instance implements <see cref="IDisposable"/>.)
        /// </param>
        /// <remarks>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="instanceType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <paramref name="instanceType"/> is not compatible with the
        /// <paramref name="serviceType"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Type instanceType, CreationPolicy creationPolicy, DisposalPolicy disposalPolicy)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

#if !NETFX_CORE && !NET45
            if (!serviceType.IsAssignableFrom(instanceType))
#else
      if (!serviceType.GetTypeInfo().IsAssignableFrom(instanceType.GetTypeInfo()))
#endif
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture, 
                    "The instance type (\"{0}\") does not implement this service type (\"{1}\").",
                    instanceType.Name, serviceType.Name);

                throw new ArgumentException(message, nameof(serviceType));
            }

            Register(serviceType, key, container => container.CreateInstance(instanceType), creationPolicy, disposalPolicy);
        }


        /// <summary>
        /// Registers a services using a custom factory method.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="createInstance">
        /// The factory method responsible for serving the requests from the container.
        /// </param>
        /// <remarks>
        /// <para>
        /// The service instance will be shared by the container and all child containers (creation
        /// policy <see cref="CreationPolicy.Shared"/>) and will be automatically disposed when the
        /// container is disposed (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        /// </para>
        /// <para>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="createInstance"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Func<ServiceContainer, object> createInstance)
        {
            Register(serviceType, key, createInstance, CreationPolicy.Shared, DisposalPolicy.Automatic);
        }


        /// <summary>
        /// Registers a services using a custom factory method and a certain creation policy.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <param name="key">
        /// The name under which the object should be registered. Can be <see langword="null"/> or
        /// empty.
        /// </param>
        /// <param name="createInstance">
        /// The factory method responsible for serving the requests from the container.
        /// </param>
        /// <param name="creationPolicy">
        /// The creation policy that specifies when and how a service will be instantiated.
        /// </param>
        /// <remarks>
        /// <para>
        /// The service instance will be automatically disposed when the container is disposed
        /// (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        /// </para>
        /// <para>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="createInstance"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Func<ServiceContainer, object> createInstance, CreationPolicy creationPolicy)
        {
            Register(serviceType, key, createInstance, creationPolicy, DisposalPolicy.Automatic);
        }


        ///// <summary>
        ///// Registers the specified service instance.
        ///// </summary>
        ///// <typeparam name="TService">The type of the service.</typeparam>
        ///// <typeparam name="TInstance">The type implementing the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="instance">The service instance to be registered.</param>
        ///// <remarks>
        ///// <para>
        ///// The service instance will be shared by the container and all child containers (creation 
        ///// policy <see cref="CreationPolicy.Shared"/>) and will not be disposed when the container is 
        ///// disposed (disposal policy <see cref="DisposalPolicy.Manual"/>).
        ///// </para>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        ///// <exception cref="ArgumentNullException">
        ///// <paramref name="instance"/> is <see langword="null"/>.
        ///// </exception>
        //public void Register<TService, TInstance>(string key, TInstance instance) where TInstance : class, TService
        //{
        //  if (instance == null)
        //    throw new ArgumentNullException("instance");

        //  Register(typeof(TService), key, container => instance, CreationPolicy.Shared, DisposalPolicy.Manual);
        //}


        ///// <summary>
        ///// Registers the specified service type.
        ///// </summary>
        ///// <typeparam name="TService">The type of the service.</typeparam>
        ///// <typeparam name="TInstance">The type implementing the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <remarks>
        ///// The service instance will be shared by the container and all child containers (creation 
        ///// policy <see cref="CreationPolicy.Shared"/>) and will be automatically disposed when the 
        ///// container is disposed (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        ///// </remarks>
        ///// <remarks>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        //public void Register<TService, TInstance>(string key) where TInstance : class, TService
        //{
        //  Register<TService, TInstance>(key, CreationPolicy.Shared, DisposalPolicy.Automatic);
        //}


        ///// <summary>
        ///// Registers the specified service type using a certain creation policy.
        ///// </summary>
        ///// <typeparam name="TService">The type of the service.</typeparam>
        ///// <typeparam name="TInstance">The type implementing the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="creationPolicy">
        ///// The creation policy that specifies when and how a service will be instantiated.
        ///// </param>
        ///// <remarks>
        ///// <para>
        ///// The service instance will be automatically disposed when the container is disposed (disposal 
        ///// policy <see cref="DisposalPolicy.Automatic"/>).
        ///// </para>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        //public void Register<TService, TInstance>(string key, CreationPolicy creationPolicy) where TInstance : class, TService
        //{
        //  Register<TService, TInstance>(key, creationPolicy, DisposalPolicy.Automatic);
        //}


        ///// <summary>
        ///// Registers the specified service type using a certain creation and disposal policy.
        ///// </summary>
        ///// <typeparam name="TService">The type of the service.</typeparam>
        ///// <typeparam name="TInstance">The type implementing the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="creationPolicy">
        ///// The creation policy that specifies when and how a service will be instantiated.
        ///// </param>
        ///// <param name="disposalPolicy">
        ///// The disposal policy that specifies when a service instance will be disposed. (Only relevant
        ///// if the service instance implements <see cref="IDisposable"/>.)
        ///// </param>
        ///// <remarks>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </remarks>
        //public void Register<TService, TInstance>(string key, CreationPolicy creationPolicy, DisposalPolicy disposalPolicy) where TInstance : class, TService
        //{
        //  Register(typeof(TService), key, container => container.CreateInstance(typeof(TInstance)), creationPolicy, disposalPolicy);
        //}


        ///// <summary>
        ///// Registers the specified service instance.
        ///// </summary>
        ///// <typeparam name="T">The type of the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="instance">The service instance to be registered.</param>
        ///// <remarks>
        ///// <para>
        ///// The service instance will be shared by the container and all child containers (creation 
        ///// policy <see cref="CreationPolicy.Shared"/>) and will not be disposed when the container is 
        ///// disposed (disposal policy <see cref="DisposalPolicy.Manual"/>).
        ///// </para>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        ///// <exception cref="ArgumentNullException">
        ///// <paramref name="instance"/> is <see langword="null"/>.
        ///// </exception>
        //public void Register<T>(string key, T instance) where T : class
        //{
        //  if (instance == null)
        //    throw new ArgumentNullException("instance");

        //  Register(typeof(T), key, container => instance, CreationPolicy.Shared, DisposalPolicy.Manual);
        //}


        ///// <summary>
        ///// Registers the specified service type.
        ///// </summary>
        ///// <typeparam name="T">The type of the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <remarks>
        ///// The service instance will be shared by the container and all child containers (creation 
        ///// policy <see cref="CreationPolicy.Shared"/>) and will be automatically disposed when the 
        ///// container is disposed (disposal policy <see cref="DisposalPolicy.Automatic"/>).
        ///// </remarks>
        ///// <remarks>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        //public void Register<T>(string key) where T : class
        //{
        //  Register<T, T>(key, CreationPolicy.Shared, DisposalPolicy.Automatic);
        //}


        ///// <summary>
        ///// Registers the specified service type using a certain creation policy.
        ///// </summary>
        ///// <typeparam name="T">The type of the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="creationPolicy">
        ///// The creation policy that specifies when and how a service will be instantiated.
        ///// </param>
        ///// <remarks>
        ///// <para>
        ///// The service instance will be automatically disposed when the container is disposed (disposal 
        ///// policy <see cref="DisposalPolicy.Automatic"/>).
        ///// </para>
        ///// <para>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </para>
        ///// </remarks>
        //public void Register<T>(string key, CreationPolicy creationPolicy) where T : class
        //{
        //  Register<T, T>(key, creationPolicy, DisposalPolicy.Automatic);
        //}


        ///// <summary>
        ///// Registers the specified service type using a certain creation and disposal policy.
        ///// </summary>
        ///// <typeparam name="T">The type of the service.</typeparam>
        ///// <param name="key">
        ///// The name under which the object should be registered. Can be <see langword="null"/> or 
        ///// empty. 
        ///// </param>
        ///// <param name="creationPolicy">
        ///// The creation policy that specifies when and how a service will be instantiated.
        ///// </param>
        ///// <param name="disposalPolicy">
        ///// The disposal policy that specifies when a service instance will be disposed. (Only relevant
        ///// if the service instance implements <see cref="IDisposable"/>.)
        ///// </param>
        ///// <remarks>
        ///// If a service with the same type and name is already registered, the existing entry will be
        ///// replaced.
        ///// </remarks>
        //public void Register<T>(string key, CreationPolicy creationPolicy, DisposalPolicy disposalPolicy) where T : class
        //{
        //  Register<T, T>(key, creationPolicy, disposalPolicy);
        //}


        ///// <summary>
        ///// Unregisters all instances of the given service type.
        ///// </summary>
        ///// <typeparam name="T">The type of the service to be removed.</typeparam>
        ///// <remarks>
        ///// The method removes all services (named and unnamed) that match the given type.
        ///// </remarks>
        //public void Unregister<T>() where T : class
        //{
        //  Unregister(typeof(T));
        //}


        ///// <summary>
        ///// Unregisters the service with the specified name.
        ///// </summary>
        ///// <typeparam name="T">The type of the service to be removed.</typeparam>
        ///// <param name="key">
        ///// The name the object was registered with. Can be <see langword="null"/> or empty. 
        ///// </param>
        //public void Unregister<T>(string key) where T : class
        //{
        //  Unregister(typeof(T), key);
        //}
    }
}
