// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Tools
{
    /// <summary>
    /// Defines the encryption algorithm for packages.
    /// </summary>
    public enum PackageEncryption
    {
        /// <summary>
        /// The ZipCrypto algorithm (weak encryption, widely supported).
        /// </summary>
        ZipCrypto,

        /// <summary>
        /// The AES-256 algorithm (strong encryption, not supported by all ZIP tools).
        /// </summary>
        Aes256,
    }
}