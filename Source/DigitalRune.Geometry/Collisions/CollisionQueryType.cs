// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// The type of collision query.
  /// </summary>
  public enum CollisionQueryType
  {
    /// <summary>
    /// A boolean ("have contact") query. The result of a boolean query is either 
    /// <see langword="true"/> to indicate contact or <see langword="false"/> to indicate 
    /// separation.
    /// </summary>
    Boolean,

    /// <summary>
    /// A collision query that computes detailed contact information. Contact information is only 
    /// computed for objects in contact, not for separated objects.
    /// </summary>
    Contacts,

    /// <summary>
    /// Searching for 1 pair of closest points. The closest-points information is computed for 
    /// separated objects and for objects in contact.
    /// </summary>
    ClosestPoints,
  }
}
