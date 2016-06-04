// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.ServiceLocation
{
    partial class ServiceContainer
    {
        // IMPORTANT: GetService() needs to be an implicit interface implementation.
        // Explicit interface implementations of IServiceProvider.GetService() are removed
        // by Visual Studio 2015 when targeting the Universal Windows Platform (.NET Native).

        /// <summary>
        /// Gets the service instance of the specified type.
        /// </summary>
        /// <param name="serviceType">
        /// An object that specifies the type of service object to get.
        /// </param>
        /// <returns>
        /// A service instance of type <paramref name="serviceType"/>. Or <see langword="null"/> if 
        /// there is no service instance of type <paramref name="serviceType"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        public object GetService(Type serviceType)
        {
            return GetInstance(serviceType, null);
        }
    }
}
