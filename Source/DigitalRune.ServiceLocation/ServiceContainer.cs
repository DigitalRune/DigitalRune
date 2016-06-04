// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DigitalRune.Collections;
using Microsoft.Practices.ServiceLocation;

// Workaround for C# 6 (Roslyn) bug:
// See http://stackoverflow.com/questions/31541390/visual-studio-2015-c-sharp-6-roslyn-cant-compile-xml-comments-in-pcl-projec
// XML comment cref attributes like
//      cref="GetInstance(Type)"
// need to be written as
//      cref="GetInstance(System.Type,string)".


namespace DigitalRune.ServiceLocation
{
    /// <summary>
    /// Implements a simple <i>inversion of control</i> (IoC) container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ServiceContainer"/> supports basic dependency injection:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <strong>Constructor Injection:</strong> When a new instance is created in
    /// <see cref="GetInstance(System.Type,string)"/> all dependencies defined via constructor
    /// parameters are automatically resolved. (The <see cref="ServiceContainer"/> automatically
    /// chooses the constructor with the max number of arguments. The method
    /// <see cref="SelectConstructor"/> can be overridden in derived classes if a different strategy
    /// for choosing a constructor should be applied.)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Property Injection:</strong> The method <see cref="ResolveProperties"/> can be
    /// called to inject dependencies into properties of a given instance. Note that, property
    /// injection is not applied automatically. The method <see cref="ResolveProperties"/> needs to
    /// be called explicitly, if property injection is required.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Named Services:</strong> Services can be registered under a name (key). The name can
    /// be <see langword="null"/>, which is interpreted by the container as the "default" instance
    /// of the service. A string of length 0 is considered to be different from
    /// <see langword="null"/>. Any service where the key is not <see langword="null"/> is called a
    /// "named" service. When the method <see cref="GetInstance(System.Type,string)"/> or its
    /// overloads are called without a key or with a <see langword="null"/> key, the default
    /// (unnamed) instance is returned or <see langword="null"/> if there is no default instance.
    /// And when the method <see cref="GetInstance(System.Type,string)"/> or its overloads are
    /// called with a key that is not <see langword="null"/>, the matching named instance is
    /// returned or <see langword="null"/> if there is no matching named instance (even if there is
    /// a default instance).
    /// </para>
    /// <para>
    /// When registering multiple services of the same type and with the same name (key), the
    /// previous entries will be overwritten.
    /// </para>
    /// <para>
    /// <strong>IEnumerable&lt;TService&gt;:</strong> The method <see cref="GetAllInstances"/>
    /// returns all named instances of a given type. The method <see cref="GetInstance(System.Type)"/>
    /// and its overloads behave the same as <see cref="GetAllInstances"/> when the specified type
    /// is an <see cref="IEnumerable{T}"/>. For example, 
    /// <c>GetInstance(typeof(IEnumerable&lt;IServiceXyz&gt;))</c> in C# returns all named instances
    /// of type <c>IServiceXyz</c>.
    /// </para>
    /// <para>
    /// <strong>Func&lt;TService&gt;:</strong> The method <see cref="GetInstance(System.Type,string)"/>
    /// and its overloads can also be used to get a delegate method that resolves an instance of
    /// type <i>T</i>. The type parameter must be a <see cref="Func{T}"/>. For example, 
    /// <c>GetInstance(typeof(Func&lt;IServiceXyz&gt;))</c> in C# returns a delegate that can be
    /// used to get an instance of type <i>IServiceXyz</i>.
    /// </para>
    /// <para>
    /// <strong>Cyclic Service References:</strong> When services are registered by type (e.g. using
    /// <see cref="Register(System.Type,string,System.Type)"/>) the service container will
    /// automatically create an instance when needed and inject the necessary parameter in the
    /// constructor. Services that are registered by type must not have cyclic dependencies, for
    /// example, where service A needs service B in its constructor and B needs A in its
    /// constructor). Cyclic dependencies must be broken, for example, by changing the constructor
    /// of B to expect a <strong>Func</strong> of A (e.g. <c>Func&lt;A&gt;</c> in C# instead of an
    /// instance of A).
    /// </para>
    /// <para>
    /// <strong>Registered Default Services:</strong> The service container itself is registered in
    /// the container by default and can be retrieved using, for example
    /// <c>GetInstance&lt;ServiceContainer&gt;()</c> (in C#).
    /// </para>
    /// <para>
    /// <strong>Compatibility:</strong> If required, this class can be replaced by a more advanced
    /// IoC container implementing the <see cref="IServiceLocator"/> interface. For more
    /// information: See <see href="http://commonservicelocator.codeplex.com/">Microsoft patterns
    /// &amp; practices - Common Service Locator Library</see>.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> The <see cref="ServiceContainer"/> is thread-safe and can be
    /// accessed from multiple threads simultaneously.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public partial class ServiceContainer : IDisposable
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Dictionary<ServiceRegistration, ServiceEntry> _registry;
        private readonly WeakCollection<ServiceContainer> _childContainers;
        private ServiceContainer _parent;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        public ServiceContainer()
        {
            _registry = new Dictionary<ServiceRegistration, ServiceEntry>();
            _childContainers = new WeakCollection<ServiceContainer>();

            // Register default services. (Needed for automatic parameter injection.)
            Register(typeof(IServiceProvider), null, container => container, CreationPolicy.LocalShared, DisposalPolicy.Manual);
            Register(typeof(IServiceLocator), null, container => container, CreationPolicy.LocalShared, DisposalPolicy.Manual);
            Register(typeof(ServiceContainer), null, container => container, CreationPolicy.LocalShared, DisposalPolicy.Manual);
        }


        /// <overloads>
        /// <summary>
        /// Releases all resources used by an instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Releases all resources used by an instance of the <see cref="ServiceContainer"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the
        /// <see cref="ServiceContainer"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose tracked service instances.
                    lock (_registry)
                    {
                        foreach (var entry in _registry.Values)
                            DisposeInstances(entry);
                    }

                    // Dispose child containers.
                    lock (_childContainers)
                    {
                        foreach (var container in _childContainers)
                            container.Dispose();
                    }
                }

                IsDisposed = true;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the service container has already
        /// been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }


        /// <summary>
        /// Creates a new child container.
        /// </summary>
        /// <returns>The child container.</returns>
        public ServiceContainer CreateChildContainer()
        {
            ThrowIfDisposed();

            var childContainer = OnCreateChildContainer();
            if (childContainer != null)
            {
                // Set parent.
                childContainer._parent = this;

                // Keep track of child container.
                lock (_childContainers)
                {
                    _childContainers.Add(childContainer);
                }
            }

            return childContainer;
        }


        /// <summary>
        /// Called when a new child container needs to be created.
        /// </summary>
        /// <returns>The child container.</returns>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> Derived classes can override this method to create
        /// a new child container of a certain type. The base implementation creates a child
        /// container of type <see cref="ServiceContainer"/>.
        /// </remarks>
        protected virtual ServiceContainer OnCreateChildContainer()
        {
            return new ServiceContainer();
        }


        /// <summary>
        /// Stores the shared service instance, or keeps track of disposable instances for automatic
        /// disposal.
        /// </summary>
        /// <param name="entry">The service entry in the registry.</param>
        /// <param name="instance">The service instance. (Can be <see langword="null"/>.)</param>
        private static void StoreInstance(ServiceEntry entry, object instance)
        {
            if (instance == null)
                return;

            if (entry.CreationPolicy == CreationPolicy.Shared || entry.CreationPolicy == CreationPolicy.LocalShared)
            {
                // Store shared instance.
                Debug.Assert(entry.Instances == null);
                entry.Instances = instance;
            }
            else
            {
                // Keep track of non-shared instance for automatic disposal.
                Debug.Assert(entry.CreationPolicy == CreationPolicy.NonShared);
                if (entry.DisposalPolicy == DisposalPolicy.Automatic)
                {
                    var disposable = instance as IDisposable;
                    if (disposable != null)
                    {
                        if (entry.Instances == null)
                        {
                            // This the first service instance.
                            entry.Instances = new WeakReference(disposable);
                        }
                        else if (entry.Instances is WeakReference)
                        {
                            // This is the second service instance.
                            var weakReference = (WeakReference)entry.Instances;
                            var previousDisposable = (IDisposable)weakReference.Target;
                            if (previousDisposable == null)
                            {
                                // First service instance has already been garbage collected.
                                weakReference.Target = instance;
                            }
                            else
                            {
                                // First service instance is still alive.
                                entry.Instances = new WeakCollection<IDisposable>
                                {
                                    previousDisposable,
                                    disposable
                                };
                            }
                        }
                        else if (entry.Instances is WeakCollection<IDisposable>)
                        {
                            // Multiple service instance.
                            var weakCollection = (WeakCollection<IDisposable>)entry.Instances;
                            weakCollection.Add(disposable);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Disposes of objects whose lifetime is controlled by the service container.
        /// </summary>
        /// <param name="entry">The service entry in the registry.</param>
        private static void DisposeInstances(ServiceEntry entry)
        {
            if (entry.DisposalPolicy != DisposalPolicy.Automatic)
                return;

            if (entry.Instances is IDisposable)
            {
                // Shared disposable instance.
                Debug.Assert(entry.CreationPolicy == CreationPolicy.Shared || entry.CreationPolicy == CreationPolicy.LocalShared);
                var instance = (IDisposable)entry.Instances;
                instance.Dispose();
            }
            else if (entry.Instances is WeakReference)
            {
                // Single non-shared disposable instance.
                Debug.Assert(entry.CreationPolicy == CreationPolicy.NonShared);
                var instance = ((WeakReference)entry.Instances).Target;
                instance.SafeDispose();
            }
            else if (entry.Instances is WeakCollection<IDisposable>)
            {
                //  Multiple non-shared disposable instance.
                Debug.Assert(entry.CreationPolicy == CreationPolicy.NonShared);
                var instances = (WeakCollection<IDisposable>)entry.Instances;
                foreach (var instance in instances)
                    instance.Dispose();
            }
        }


        /// <summary>
        /// Gets all service registrations for the current container up to the root container.
        /// </summary>
        /// <returns>The service registrations. (May include duplicate items.)</returns>
        private IEnumerable<ServiceRegistration> GetRegistrations()
        {
            IEnumerable<ServiceRegistration> registrations;

            lock (_registry)
            {
                // Copy registrations into buffer (to release lock as soon as possible).
                registrations = _registry.Keys.ToArray();
            }

            if (_parent != null)
            {
                // Append registrations of parent containers.
                var parentKeys = _parent.GetRegistrations();
                registrations = registrations.Concat(parentKeys);
            }

            return registrations;
        }


        /// <summary>
        /// Gets all registrations of a given service type for the current container up to the root
        /// container.
        /// </summary>
        /// <returns>
        /// The registration of the given service type. (May include duplicate items.)
        /// </returns>
        private IEnumerable<ServiceRegistration> GetRegistrations(Type serviceType)
        {
            IEnumerable<ServiceRegistration> registrations;

            lock (_registry)
            {
                // Copy registrations into buffer (to release lock as soon as possible).
                registrations = _registry.Keys.Where(r => r.Type == serviceType).ToArray();
            }

            if (_parent != null)
            {
                // Append registrations of parent containers.
                var parentKeys = _parent.GetRegistrations(serviceType);
                registrations = registrations.Concat(parentKeys);
            }

            return registrations;
        }


        /// <summary>
        /// Resets the container and removes all locally registered service types.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            lock (_registry)
            {
                // Dispose tracked service instances.
                foreach (var entry in _registry.Values)
                    DisposeInstances(entry);

                _registry.Clear();
            }
        }


        /// <overloads>
        /// <summary>
        /// Registers a service.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Registers a service using a custom factory method and certain creation and disposal
        /// policies.
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
        /// <param name="disposalPolicy">
        /// The disposal policy that specifies when a service instance will be disposed. (Only
        /// relevant if the service instance implements <see cref="IDisposable"/>.)
        /// </param>
        /// <remarks>
        /// If a service with the same type and name is already registered, the existing entry will
        /// be replaced.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> or <paramref name="createInstance"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void Register(Type serviceType, string key, Func<ServiceContainer, object> createInstance, CreationPolicy creationPolicy, DisposalPolicy disposalPolicy)
        {
            ThrowIfDisposed();

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (createInstance == null)
                throw new ArgumentNullException(nameof(createInstance));

            var registration = new ServiceRegistration(serviceType, key);
            var entry = new ServiceEntry(createInstance, creationPolicy, disposalPolicy);
            lock (_registry)
            {
                _registry[registration] = entry;
            }
        }


        /// <overloads>
        /// <summary>
        /// Unregisters a service.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Unregisters all services of the given service type.
        /// </summary>
        /// <param name="serviceType">The type of service to be removed.</param>
        /// <remarks>
        /// The method removes all services (named and unnamed) that match the given type.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> is <see langword="null"/>.
        /// </exception>
        public void Unregister(Type serviceType)
        {
            ThrowIfDisposed();

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            lock (_registry)
            {
                var services = _registry.Where(service => service.Key.Type == serviceType).ToArray();
                foreach (var service in services)
                {
                    DisposeInstances(service.Value);
                    _registry.Remove(service.Key);
                }
            }
        }


        /// <summary>
        /// Unregisters the service with the specified name.
        /// </summary>
        /// <param name="serviceType">The type of service to be removed.</param>
        /// <param name="key">
        /// The name the object was registered with. Can be <see langword="null"/> or empty. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceType"/> is <see langword="null"/>.
        /// </exception>
        public void Unregister(Type serviceType, string key)
        {
            ThrowIfDisposed();

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            var registration = new ServiceRegistration(serviceType, key);
            lock (_registry)
            {
                ServiceEntry entry;
                if (_registry.TryGetValue(registration, out entry))
                {
                    DisposeInstances(entry);
                    _registry.Remove(registration);
                }
            }
        }


        private IEnumerable<object> GetAllInstancesImpl(Type serviceType)
        {
            // Get the registrations for the given type from the current container up 
            // to the root container. (GetRegistrations() may return duplicate entries. 
            // Use Enumerable.Distinct() to keep only the first registration for any 
            // given name.)
            var registrations = GetRegistrations(serviceType).Distinct();

            // Try to get all instances.
            var instances = new List<object>();
            foreach (var registration in registrations)
            {
                if (registration.Key != null)  // Only named instances.
                {
                    var instance = GetInstanceImpl(serviceType, registration.Key);
                    if (instance != null)
                        instances.Add(instance);
                }
            }

            return instances;
        }


        private object GetInstanceImpl(Type serviceType, string key, bool onlyShared = false)
        {
            lock (_registry)
            {
                // Try to locate matching service entry in the current or a parent container.
                ServiceRegistration registration = new ServiceRegistration(serviceType, key);
                ServiceEntry entry;
                ServiceContainer container;

                if (_registry.TryGetValue(registration, out entry))
                {
                    // Service entry found in local registry.
                    container = this;
                }
                else
                {
                    // Check parent containers.
                    container = _parent;
                    while (container != null)
                    {
                        lock (container._registry)
                        {
                            if (container._registry.TryGetValue(registration, out entry))
                                break;
                        }

                        container = container._parent;
                    }
                }

                if (entry != null)
                {
                    // Resolve instance from service entry.
                    if (entry.CreationPolicy == CreationPolicy.NonShared)
                    {
                        if (onlyShared)
                            return null;

                        // Create non-shared (transient) service instance.
                        var instance = entry.CreateInstance(this);

                        // Track for disposal, if necessary.
                        StoreInstance(entry, instance);

                        return instance;
                    }

                    if (entry.CreationPolicy == CreationPolicy.LocalShared && container != this)
                    {
                        // Service entry was found in a parent container, but the service is 
                        // configured to be created per container. --> Copy into local registry.
                        entry = new ServiceEntry(entry);
                        _registry.Add(registration, entry);
                        container = this;
                    }

                    // Create shared service instance, if not already cached.
                    // (Double-check to avoid unnecessary lock.)
                    if (entry.Instances == null)
                    {
                        lock (entry)
                        {
                            if (entry.Instances == null)
                            {
                                var instance = entry.CreateInstance(container);
                                StoreInstance(entry, instance);
                            }
                        }
                    }

                    return entry.Instances;
                }
            }

            // The requested service type is not directly registered. 

            // Other supported types are:  
            // - IEnumerable<TService> ... service instance collection
            // - Func<TService> .......... factory method for lazy resolution

#if !NETFX_CORE && !NET45
            if (serviceType.IsGenericType)
#else
            if (serviceType.GetTypeInfo().IsGenericType)
#endif
            {
                var genericType = serviceType.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>))
                {
                    // Requested type is IEnumerable<TService>.

                    // Get typeof(TService).
#if !NETFX_CORE && !NET45
                    Type actualServiceType = serviceType.GetGenericArguments()[0];
#else
                    Type actualServiceType = serviceType.GetTypeInfo().GenericTypeArguments[0];
#endif

                    // Get array of all named TService instances.
                    object[] instances = GetAllInstancesImpl(actualServiceType).ToArray();

                    // Create and fill TService[] array.
                    Array array = Array.CreateInstance(actualServiceType, instances.Length);
                    for (int i = 0; i < array.Length; i++)
                        array.SetValue(instances[i], i);

                    return array;
                }

                if (genericType == typeof(Func<>))
                {
                    // Requested type is Func<TService>.
#if !NETFX_CORE && !NET45
                    var actualServiceType = serviceType.GetGenericArguments()[0];
                    var factoryFactoryType = typeof(ServiceFactoryFactory<>).MakeGenericType(actualServiceType);
                    var factoryFactoryInstance = Activator.CreateInstance(factoryFactoryType);
                    var factoryFactoryMethod = factoryFactoryType.GetMethod("Create");
#else
                    var actualServiceType = serviceType.GetTypeInfo().GenericTypeArguments[0];
                    var factoryFactoryType = typeof(ServiceFactoryFactory<>).MakeGenericType(actualServiceType);
                    var factoryFactoryInstance = Activator.CreateInstance(factoryFactoryType);
                    var factoryFactoryMethod = factoryFactoryType.GetTypeInfo().GetDeclaredMethod("Create");
#endif
                    return factoryFactoryMethod.Invoke(factoryFactoryInstance, new object[] { this, key });
                }

                //if (genericType == typeof(Func<,>) && serviceType.GetGenericArguments()[0] == typeof(string))
                //{
                //  // Requested type is Func<string, TService> where the argument is the name of the service.
                //  ...
                //}
            }

            return null;
        }


        /// <summary>
        /// Creates an instance the given type and satisfies the constructor dependencies.
        /// </summary>
        /// <param name="type">The type to instantiate.</param>
        /// <returns>A new instance of the requested type.</returns>
        ///// <remarks>
        ///// <strong>Notes to Inheritors:</strong> The method can be overridden in derived classes to use
        ///// a different strategy for creating a new instance of the requested type. The base 
        ///// implementation calls <see cref="SelectConstructor"/> to select the appropriate constructor,
        ///// resolves the constructor arguments and then calls <see cref="OnCreateInstance"/> to activate
        ///// the instance using reflection.
        ///// </remarks>
        public object CreateInstance(Type type)
        {
            var args = ResolveConstructorArgs(type);
            var instance = OnCreateInstance(type, args);
            return instance;
        }


        private object[] ResolveConstructorArgs(Type type)
        {
            // Select the constructor.
            var constructor = SelectConstructor(type);

            // Resolve the constructor's dependencies.
            var args = new List<object>();
            if (constructor != null)
            {
                foreach (var parameter in constructor.GetParameters())
                {
                    var value = GetInstance(parameter.ParameterType, null);
                    args.Add(value);
                }
            }

            return args.ToArray();
        }


        /// <summary>
        /// Selects the constructor to be used for activating the given type.
        /// </summary>
        /// <param name="type">The type to be activated.</param>
        /// <returns>The constructor that should be used.</returns>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> This method can be overridden in derived classes
        /// if a certain strategy for choosing the constructor should be applied. The base
        /// implementation chooses the constructor with the max number of parameters.
        /// </remarks>
        protected virtual ConstructorInfo SelectConstructor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ConstructorInfo preferredConstructor = null;
            int preferredNumberOfParameters = -1;
#if !NETFX_CORE && !NET45
            foreach (var constructor in type.GetConstructors())
#else
            foreach (var constructor in type.GetTypeInfo().DeclaredConstructors)
#endif
            {
                int numberOfParameters = constructor.GetParameters().Length;

                if (numberOfParameters > preferredNumberOfParameters)
                {
                    preferredConstructor = constructor;
                    preferredNumberOfParameters = numberOfParameters;
                }
            }

            return preferredConstructor;
        }


        /// <summary>
        /// Creates an instance of the type with the specified constructor arguments.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>A new instance of the requested type.</returns>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnCreateInstance"/> be
        /// sure to throw an exception if the activation fails. The method must not return
        /// <see langword="null"/>.
        /// </remarks>
        protected virtual object OnCreateInstance(Type type, object[] args)
        {
            return (args != null && args.Length > 0)
                   ? Activator.CreateInstance(type, args)
                   : Activator.CreateInstance(type);
        }


        /// <summary>
        /// Tries to resolve all property dependencies of the given instance.
        /// </summary>
        /// <param name="instance">The instance to build up.</param>
        /// <remarks>
        /// This method inspects the properties of the given <paramref name="instance"/>. It will
        /// initialize the property with an instance from this container if the property has a
        /// public setter, if the property type is found in the container, and if the property type
        /// is a reference type (not a value type).
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="instance"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ActivationException">
        /// An error occurred while resolving a service instance.
        /// </exception>
        public void ResolveProperties(object instance)
        {
            ThrowIfDisposed();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var instanceType = instance.GetType();
#if !NETFX_CORE && !NET45
            foreach (var property in instanceType.GetProperties())
            {
                // Check whether property has public getter and setter.
                if (property.GetSetMethod() == null || property.GetGetMethod() == null)
                    continue;

                // Get the type of the property (= the service type to resolve).
                var propertyType = property.PropertyType;

                // Ignore value types.
                if (propertyType.IsValueType)
                    continue;
#else
            foreach (var property in instanceType.GetTypeInfo().DeclaredProperties)
            {
                // Check whether property has public getter and setter.
                if (property.SetMethod == null || property.GetMethod == null)
                    continue;
                
                // Get the type of the property (= the service type to resolve).
                var propertyType = property.PropertyType;
                
                // Ignore value types.
                if (propertyType.GetTypeInfo().IsValueType)
                    continue;
#endif
                // Ignore property if a value is already set.
                if (property.GetValue(instance, null) != null)
                    continue;

                // Try to resolve and inject dependency.
                var injection = GetInstance(propertyType, null);
                if (injection != null)
                    property.SetValue(instance, injection, null);
            }
        }
        #endregion
    }
}
