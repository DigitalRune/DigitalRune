// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
//#if WINDOWS
//using System.Diagnostics;
//#endif


namespace DigitalRune
{
  /// <exclude/>
  [Obsolete("Use GlobalSettings instead of Global. (Reason: The name Global may conflict with the global keyword in languages other than C#. Therefore the class Global has been renamed to GlobalSettings.)")]
  public static class Global
  {
    /// <inheritdoc cref="GlobalSettings.ValidationLevel"/>
    public static int ValidationLevel
    {
      get { return GlobalSettings.ValidationLevel; }
      set { GlobalSettings.ValidationLevel = value; }
    }


    ///<inheritdoc cref="GlobalSettings.PlatformID"/>
    public static PlatformID PlatformID
    {
      get { return GlobalSettings.PlatformID; }
    }
  }


  /// <summary>
  /// Defines global settings and information for all DigitalRune libraries.
  /// </summary>
  public static class GlobalSettings
  {
    internal const int ValidationLevelNone = 0;
    internal const int ValidationLevelUserHighCheap = 1 << 0;      // Relevant for customer, high priority, low performance impact.
    internal const int ValidationLevelUserHighExpensive = 1 << 1;  // Relevant for customer, high priority, medium performance impact.
    //internal const int ValidationLevel4 = 1 << 2;                // Relevant for customer, medium Priority
    //internal const int ValidationLevel5 = 1 << 3;                // Relevant for customer, low, undefined Priority
    internal const int ValidationLevelUser = 0xff;                 // All checks which are relevant for customers.
    internal const int ValidationLevelDev = 0xff00;                // All checks which are relevant for the DigitalRune team during library development.
    internal const int ValidationLevelDevBasic = 1 << 8;           // Basic validation of algorithms.
    internal const int ValidationLevelDebug = 0xffff;              // All user and dev checks.


    /// <summary>
    /// Gets or sets the validation level for all DigitalRune libraries, used to enable additional
    /// input validation and other checks.
    /// </summary>
    /// <value>
    /// The validation level for all DigitalRune libraries.
    /// </value>
    /// <remarks>
    /// <para>
    /// The default validation level for release builds is 0. Setting a validation level greater
    /// than 0, enables additional checks in the DigitalRune libraries, e.g. more detailed input
    /// validation. These checks are usually turned off (using <see cref="ValidationLevel"/> = 0)
    /// to avoid a performance impact in release builds. During development and for debugging, 
    /// validation can be enabled. Set <see cref="ValidationLevel"/> to 0xff (=255) to enable
    /// all checks which are relevant for DigitalRune customers. Validation levels > 255 are 
    /// reserved for internal development.
    /// </para>
    /// </remarks>
    public static int ValidationLevel
    {
      get { return ValidationLevelInternal; }
      set { ValidationLevelInternal = value; }
    }
#if DEBUG
    internal static int ValidationLevelInternal = ValidationLevelDebug;
#else
    internal static int ValidationLevelInternal = ValidationLevelNone;
#endif


//#if WINDOWS
//    /// <summary>
//    /// Gets or sets the trace source for trace messages from DigitalRune libraries.
//    /// </summary>
//    /// <value>
//    /// The trace source for trace messages from DigitalRune libraries. 
//    /// Must not be <see langword="null"/>.
//    /// </value>
//    public static TraceSource TraceSource
//    {
//      get { return TraceSourceInternal; }
//      set
//      {
//        if (value == null)
//          throw new ArgumentNullException("value");

//        TraceSourceInternal = value;
//      }
//    }
//    internal static TraceSource TraceSourceInternal = new TraceSource("DigitalRune");
//#endif


    /// <summary>
    /// Gets the platform for which this library was built.
    /// </summary>
    /// <value>The platform for which this library was built.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Consistent with System.PlatformID.")]
    public static PlatformID PlatformID
    {
      get
      {
        // We do not use #if ... #elif to get a compiler error if two symbols are 
        // defined in the same build.
#if PORTABLE
        return PlatformID.Portable;
#endif
#if WINDOWS
        if ((int)_platformID == -1)
        {
          if (Environment.OSVersion.Platform == System.PlatformID.MacOSX)
          {
            _platformID = PlatformID.MacOS;
          }
          else if (Environment.OSVersion.Platform == System.PlatformID.Unix)
          {
            // Older Mono versions return "Unix" even on Mac OS X.
            if (PlatformHelper.IsRunningOnMac())
              _platformID = PlatformID.MacOS;
            else
              _platformID = PlatformID.Linux;
          }
          else
          {
            _platformID = PlatformID.Windows;
          }
        }
        return _platformID;
#endif
#if WINDOWS_UWP
        return PlatformID.WindowsUniversal;
#elif NETFX_CORE
        return PlatformID.WindowsStore;
#endif
#if WP7
        return PlatformID.WindowsPhone7;
#endif
#if WP8
        return PlatformID.WindowsPhone8;
#endif
#if XBOX360
        return PlatformID.Xbox360;
#endif
#if SILVERLIGHT
        return PlatformID.Silverlight;
#endif
#if UNITY
        return PlatformID.Unity;
#endif
#if ANDROID
        return PlatformID.Android;
#endif
#if IOS
        return PlatformID.iOS;
#endif
      }
    }
#if WINDOWS
    private static PlatformID _platformID = (PlatformID) -1;
#endif
  }
}
