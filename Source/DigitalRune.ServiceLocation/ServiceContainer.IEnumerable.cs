// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Linq;


namespace DigitalRune.ServiceLocation
{
    // The interface IEnumerable is implemented because it is helpful for debugging.
    // (I.e. for inspecting all registered service instances.)
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    partial class ServiceContainer : IEnumerable
    {
        /// <summary>
        /// Returns an enumerator that iterates through all registered services.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through all available 
        /// services in the <see cref="ServiceContainer"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            // Get all registrations from the current container up to the root container. 
            // (GetRegistrations() may return duplicate entries. Use Enumerable.Distinct() 
            // to keep only the first registration for any given type and name.)
            var registrations = GetRegistrations().Distinct();

            // Try to get all instances.
            foreach (var registration in registrations)
            {
                var instance = GetInstanceImpl(registration.Type, registration.Key, true);
                if (instance != null)
                    yield return instance;
            }
        }
    }
}
