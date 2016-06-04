// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if PORTABLE
using System;
using System.Diagnostics;


namespace DigitalRune
{
  /// <summary>
  /// Provides additional information and methods for portable projects.
  /// </summary>
  internal static class Portable
  {
    internal const string Message = "This functionality is not implemented in the PCL (Portable Class Library) implementation of DigitalRune.dll. To use this functionality the executable should reference a platform-specific DLL.";


    public static NotImplementedException NotImplementedException
    {
      get { return new NotImplementedException(Message); }
    }
  }
}
#endif
