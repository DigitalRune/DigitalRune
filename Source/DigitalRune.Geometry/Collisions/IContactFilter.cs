// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Collisions.Algorithms;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// A filter which processes contacts in a contact set.
  /// </summary>
  /// <remarks>
  /// Contact filters are called in the narrow phase (in <see cref="CollisionAlgorithm"/>s) to
  /// post-process the found contacts. Example usages of a contact filter:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Remove redundant contacts. Some applications, like rigid body physics, needs only a minimal
  /// set of contacts, e.g. only 4 contacts per <see cref="ContactSet"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Remove "bad" contacts, for example contacts where the normal direction points into an 
  /// undesired direction.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Merge contacts. For some applications it is useful to keep only one contact which is the
  /// average of all other contacts.
  /// </description>
  /// </item>
  /// </list> 
  /// </remarks>
  public interface IContactFilter
  {
    /// <summary>
    /// Filters the specified contact set.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    void Filter(ContactSet contactSet);
  }
}
