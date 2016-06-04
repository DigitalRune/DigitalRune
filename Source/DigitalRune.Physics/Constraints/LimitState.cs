// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Describe the state of a constraint limit.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Justification = "Breaking change. Fix in next version.")]
  [Flags]
  public enum LimitState
  {
    /// <summary>
    /// The limit is inactive. The bodies are in an allowed position.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// The minimal limit is reached.
    /// </summary>
    Min = 1,

    /// <summary>
    /// The maximal limit is reached.
    /// </summary>
    Max = 2,

    /// <summary>
    /// The constraint is locked and does not allow relative movement on this constraint axis.
    /// </summary>
    Locked = Max | Min
  }
}
