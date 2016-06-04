// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.ServiceLocation
{
    /// <summary>
    /// Specifies when and how a service will be instantiated.
    /// </summary>
    public enum CreationPolicy
    {
        /// <summary>
        /// Specifies that a single shared instance of the associated service will be created by the
        /// <see cref="ServiceContainer"/> and shared by all requests. The service instance is also
        /// reused by all child containers.
        /// </summary>
        Shared,


        /// <summary>
        /// Specifies that a single instance of the associated service will be created by the
        /// <see cref="ServiceContainer"/> and shared locally (per container) by all requests. The
        /// service instance is created per container and is not reused by child containers.
        /// </summary>
        LocalShared,


        /// <summary>
        /// Specifies that a new non-shared instance of the associated service will be created by
        /// the <see cref="ServiceContainer"/> for every request.
        /// </summary>
        NonShared,
    }
}
