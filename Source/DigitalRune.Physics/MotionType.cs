// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Physics
{
  /// <summary>
  /// Defines how the simulation computes the rigid body movement.
  /// </summary>
  public enum MotionType
  {
    /// <summary>
    /// The body is static. It will never move.
    /// </summary>
    Static,

    /// <summary>
    /// The body is kinematic. Its movement is controlled by the user, not by simulation forces.
    /// </summary>
    Kinematic,

    /// <summary>
    /// The body is dynamic. Its movement is controlled by the simulation forces and collision 
    /// response.
    /// </summary>
    Dynamic,
  }
}
