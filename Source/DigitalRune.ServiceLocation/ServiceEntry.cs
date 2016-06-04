// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.ServiceLocation
{
    /// <summary>
    /// Represents an entry in the <see cref="ServiceContainer"/>.
    /// </summary>
    internal sealed class ServiceEntry
    {
        /// <summary>
        /// Specifies when and how a new instance of the service is created.
        /// </summary>
        public readonly CreationPolicy CreationPolicy;


        /// <summary>
        /// Specifies when an instance of the service is disposed.
        /// </summary>
        public readonly DisposalPolicy DisposalPolicy;


        /// <summary>
        /// The factory method that creates a new instance of the service.
        /// </summary>
        public readonly Func<ServiceContainer, object> CreateInstance;


        /// <summary>
        /// The cached service instance(s). See remarks.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The field stores:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// The cached service instance if the scope is
        /// <see cref="ServiceLocation.CreationPolicy.Shared"/> or
        /// <see cref="ServiceLocation.CreationPolicy.LocalShared"/>.
        /// </item>
        /// <item>
        /// A WeakReference or WeakCollection to the instances if they are disposable and the scope
        /// is <see cref="ServiceLocation.CreationPolicy.NonShared"/> and
        /// <see cref="ServiceLocation.DisposalPolicy.Automatic"/>.
        /// </item>
        /// </list>
        /// </remarks>
        public object Instances;  // object/WeakReference/WeakCollection<IDisposable>


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEntry"/> class.
        /// </summary>
        /// <param name="createInstance">The factory method.</param>
        /// <param name="creationPolicy">The creation policy.</param>
        /// <param name="disposalPolicy">The disposal policy.</param>
        public ServiceEntry(Func<ServiceContainer, object> createInstance, CreationPolicy creationPolicy, DisposalPolicy disposalPolicy)
        {
            CreateInstance = createInstance;
            CreationPolicy = creationPolicy;
            DisposalPolicy = disposalPolicy;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceEntry"/> class by copying the settings
        /// of an existing service entry.
        /// </summary>
        /// <param name="entry">The <see cref="ServiceEntry"/> from which to copy the settings.</param>
        public ServiceEntry(ServiceEntry entry)
        {
            CreateInstance = entry.CreateInstance;
            CreationPolicy = entry.CreationPolicy;
            DisposalPolicy = entry.DisposalPolicy;
        }
    }
}
