// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune
{
  /// <summary>
  /// Identifies the platform for which an assembly was built.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Consistent with System.PlatformID.")]
  public enum PlatformID
  {
    /// <summary>
    /// Android
    /// </summary>
    Android,

    /// <summary>
    /// Apple iOS
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
    iOS,

    /// <summary>
    /// Linux
    /// </summary>
    Linux,

    /// <summary>
    /// Apple Mac OS X
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
    MacOS,

    /// <summary>
    /// Portable class library (PCL)
    /// </summary>
    Portable,

    /// <summary>
    /// Silverlight
    /// </summary>
    Silverlight,

    /// <summary>
    /// Unity game engine
    /// </summary>
    Unity,

    /// <summary>
    /// Windows desktop
    /// </summary>
    Windows,

    /// <summary>
    /// Windows Phone 7
    /// </summary>
    WindowsPhone7,

    /// <summary>
    /// Windows Phone 8
    /// </summary>
    WindowsPhone8,

    /// <summary>
    /// Windows Store Apps (Windows 8)
    /// </summary>
    WindowsStore,

    /// <summary>
    /// Universal windows platform
    /// </summary>
    WindowsUniversal,

    /// <summary>
    /// Xbox 360
    /// </summary>
    Xbox360,
    
    //Ouya,
    //PlayStationMobile
  }
}
