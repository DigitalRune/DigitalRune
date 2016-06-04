// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Defines the type of collision object.
  /// </summary>
  public enum CollisionObjectType
  {
    /// <summary>
    /// A normal collision object. When this type of collision object collides with another 
    /// collision object of type <see cref="Default"/>, the full contact details (contact points, 
    /// normal vectors, penetration depths, etc.) are computed and stored in the 
    /// <see cref="ContactSet"/>s.
    /// </summary>
    Default,

    /// <summary>
    /// A collision object that is used as a trigger and does not need contact details. When this 
    /// type of collision object collides with another collision object, contact details (contact 
    /// points, normal vectors, penetration depths, etc.) will be omitted. The flag 
    /// <see cref="ContactSet.HaveContact"/> will be set in the <see cref="ContactSet"/>s for this 
    /// object. But the <see cref="ContactSet"/>s will not contain any <see cref="Contact"/>s. Use
    /// trigger collision objects instead of default collision objects to improve performance if
    /// contact details are not required.
    /// </summary>
    Trigger, 

    // Composite    // A composite shape. Compute a full contact set for each child feature.
    // CompositeTrigger   // Only 1 object in broad phase but HaveContact is computed for each child trigger. Is this useful?
  }
}
