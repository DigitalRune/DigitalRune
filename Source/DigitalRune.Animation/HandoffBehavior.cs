// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines how new animations interact with existing ones that are already applied to a 
  /// property.
  /// </summary>
  internal enum HandoffBehavior
  {
    /// <summary>
    /// New animations replace any existing animations on the properties to which they are applied.
    /// The new animations are initialized with the base values of the properties. The last 
    /// animation values are ignored.
    /// </summary>
    Replace,


    /// <summary>
    /// New animations replace any existing animations on the properties to which they are applied.
    /// The new animations are initialized with the last animation value of the properties.
    /// </summary>
    SnapshotAndReplace,


    /// <summary>
    /// New animations are combined with existing animations by adding the new animations to the 
    /// composition chains.
    /// </summary>
    Compose,
  }
}
