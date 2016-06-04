// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune
{
  /// <summary>
  /// Manages copy protection and licensing.
  /// </summary>
  /// <remarks>
  /// All serial numbers (license keys) must be added using <see cref="AddSerialNumber"/> at the
  /// start of the application before any license protected code is executed!
  /// </remarks>
  public static class Licensing
  {
    /// <summary>
    /// Adds the specified serial number.
    /// </summary>
    /// <param name="serialNumber">A serial number (license key) for a DigitalRune product.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serialNumber"/> is <see langword="null"/>.
    /// </exception>
    [Obsolete("Serial numbers are no longer required. Please remove any AddSerialNumber calls.")]
    public static void AddSerialNumber(string serialNumber)
    {
    }
  }
}
