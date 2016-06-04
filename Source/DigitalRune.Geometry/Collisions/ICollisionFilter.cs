// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Partitioning;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Represents a configurable collision filter supporting pairwise filtering and collision groups.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Per default, all collisions are enabled. Collisions can be disabled for pairs of
  /// <see cref="CollisionObject"/>s, for a pair of collision groups, or for a whole collision
  /// group. Collision groups are identified by an <see cref="int"/> ID, stored in the
  /// <see cref="CollisionObject"/> (see <see cref="CollisionObject.CollisionGroup"/>).
  /// </para>
  /// <para>
  /// Two collision objects A and B will NOT collide if one of the following conditions is met:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Collisions for the collision group of A are disabled (see <see cref="Set(int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions for the collision group of B are disabled (see <see cref="Set(int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions between the collision group of A and the collision group of B are disabled (see
  /// <see cref="Set(int,int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions between collision object A and B are disabled (see
  /// <see cref="Set(CollisionObject,CollisionObject,bool)"/>).
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public interface ICollisionFilter : IPairFilter<CollisionObject>
  {
    /// <overloads>
    /// <summary>
    /// Enables or disables collisions.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Enables or disables collisions between the given <see cref="CollisionObject"/>s.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <param name="collisionsEnabled">
    /// If set to <see langword="true"/> collisions between <paramref name="objectA"/> and 
    /// <paramref name="objectB"/> are enabled. Use <see langword="false"/> to disable collisions.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
    void Set(CollisionObject objectA, CollisionObject objectB, bool collisionsEnabled);


    /// <summary>
    /// Enables or disables collisions with the given collision group.
    /// </summary>
    /// <param name="collisionGroup">The collision group.</param>
    /// <param name="collisionsEnabled">
    /// If set to <see langword="true"/> collisions for object in the given group are enabled. Use 
    /// <see langword="false"/> to disable collisions.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
    void Set(int collisionGroup, bool collisionsEnabled);


    /// <summary>
    /// Enables or disables collisions between a pair of collision groups.
    /// </summary>
    /// <param name="groupA">The first collision group.</param>
    /// <param name="groupB">The second collision group.</param>
    /// <param name="collisionsEnabled">
    /// If set to <see langword="true"/> collisions between objects in <paramref name="groupA"/> and 
    /// objects in <paramref name="groupB"/> are enabled. Use <see langword="false"/> to disable 
    /// collisions.
    /// </param>
    /// <remarks>
    /// To disable collisions for objects within one group, this method can be called with 
    /// <paramref name="groupA"/> == <paramref name="groupB"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
    void Set(int groupA, int groupB, bool collisionsEnabled);


    /// <overloads>
    /// <summary>
    /// Determines whether collisions are enabled or disabled.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns <see langword="true"/> if collisions between two <see cref="CollisionObject"/>s are
    /// enabled (without testing collision groups).
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if collisions between the given <see cref="CollisionObject"/> pair 
    /// are enabled; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks only the flag that was set with 
    /// <see cref="Set(CollisionObject, CollisionObject, bool)"/>. It is not tested whether 
    /// collisions are disabled between the collision groups.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
    bool Get(CollisionObject objectA, CollisionObject objectB);


    /// <summary>
    /// Returns <see langword="true"/> if collisions with the given collision group are enabled.
    /// </summary>
    /// <param name="collisionGroup">The collision group.</param>
    /// <returns>
    /// <see langword="true"/> if collisions with the given collision group are enabled; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks only the flag that was set with <see cref="Set(int, bool)"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
    bool Get(int collisionGroup);


    /// <summary>
    /// Returns <see langword="true"/> if collisions between two collision groups are enabled.
    /// </summary>
    /// <param name="groupA">The first collision group.</param>
    /// <param name="groupB">The second collision group.</param>
    /// <returns>
    /// <see langword="true"/> if collisions with the between <paramref name="groupA"/> and 
    /// <paramref name="groupB"/> are enabled; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks only the flag that was set with <see cref="Set(int, int, bool)"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
    bool Get(int groupA, int groupB);
  }
}
