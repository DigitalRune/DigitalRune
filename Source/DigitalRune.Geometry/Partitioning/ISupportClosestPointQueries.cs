// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Indicates that an object (normally a <see cref="ISpatialPartition{T}"/>) has special support
  /// for closest-point queries.
  /// </summary>
  /// <typeparam name="T">The type of the queried items.</typeparam>
  /// <remarks>
  /// <para>
  /// Given a collection of items, the goal is to find the item that is closest to a given item or
  /// volume. Without further information the closest-point on each item in the collection has to be
  /// computed, and the item with the smallest closest-point distance is the winner. Some 
  /// <see cref="ISpatialPartition{T}"/>s have an internal structure that can speed up the search.
  /// </para>
  /// <para>
  /// The methods in this interface find the items that are good candidates to be closest to the
  /// given volume (axis-aligned bounding box or another partition). A callback method is called
  /// with each candidate item. The callback must compute the closest-point on the item and return
  /// the squared closest-point distance. The returned value helps the spatial partition to filter
  /// out bad candidates.
  /// </para>
  /// <para>
  /// The callback method must return 0 if the candidate item is touching/penetrating the other 
  /// item. -1 can be returned if the search for more candidates should be aborted. The callback can
  /// return positive infinity if it cannot compute a squared closest point distance.
  /// </para>
  /// </remarks>
  public interface ISupportClosestPointQueries<T>
  {
    /// <summary>
    /// Gets all items that are candidates for the smallest closest-point distance to a given
    /// axis-aligned bounding box (AABB).
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="maxDistanceSquared">
    /// The allowed squared distance between two points. (This an optional parameter that is used 
    /// internally to improve performance. The <paramref name="callback"/> might still be called 
    /// with an item that has a distance greater than <c>Math.Sqrt(maxDistanceSquared)</c>! So use 
    /// this parameter with care. To check all items, set the parameter to 
    /// <see cref="float.PositiveInfinity"/>.)
    /// </param>
    /// <param name="callback">
    /// The callback that is called with each found candidate item. The method must compute the
    /// closest-point on the candidate item and return the squared closest-point distance.
    /// </param>
    /// <returns>
    /// The squared closest-point distance found during the search. -1 if the search was aborted or 
    /// the search space is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    float GetClosestPointCandidates(Aabb aabb, float maxDistanceSquared, Func<T, float> callback);


    /// <summary>
    /// Gets all items that are candidates for the smallest closest-point distance to items in a
    /// given partition.
    /// </summary>
    /// <param name="scale">The scale of this spatial partition.</param>
    /// <param name="pose">The pose of this spatial partition.</param>
    /// <param name="otherPartition">The other spatial partition to test against.</param>
    /// <param name="otherScale">The scale of the <paramref name="otherPartition"/>.</param>
    /// <param name="otherPose">
    /// The pose of the <paramref name="otherPartition"/> relative to this spatial partition.
    /// </param>
    /// <param name="callback">
    /// The callback that is called with each found candidate item. The method must compute the
    /// closest-point on the candidate item and return the squared closest-point distance.
    /// </param>
    /// <remarks>
    /// This method can be used to quickly find the closest-points between two models which are both
    /// managed using a <see cref="ISpatialPartition{T}"/>. The given spatial partition can be
    /// rotated and scaled. The scale and the pose transform the other
    /// <paramref name="otherPartition"/> into the space of this spatial partition. The scale is
    /// applied before the pose.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="otherPartition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="callback"/> is <see langword="null"/>.
    /// </exception>
    void GetClosestPointCandidates(Vector3F scale, Pose pose, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose, Func<T, T, float> callback);
  }
}
