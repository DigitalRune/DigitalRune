// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an entry in the <see cref="LodCollection"/>.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [DebuggerDisplay("{GetType().Name,nq}(Distance = {Distance}, Node = {Node})")]
  public struct LodEntry : IEquatable<LodEntry>
  {
    /// <summary>
    /// Gets or sets the LOD distance. (Needs to be normalized - see remarks.)
    /// </summary>
    /// <value>
    /// The LOD distance, which is the view-normalized distance at which the current LOD is
    /// displayed.
    /// </value>
    /// <remarks>
    /// The value stored in this property is a <i>view-normalized distance</i> as described here:
    /// <see cref="GraphicsHelper.GetViewNormalizedDistance(SceneNode,CameraNode)"/>. The method
    /// <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> can be used to
    /// convert a distance to a view-normalized distance. The resulting value is independent of the
    /// camera's field-of-view.
    /// </remarks>
    public float Distance { get; set; }


    /// <summary>
    /// Gets or sets the LOD node.
    /// </summary>
    /// <value>
    /// The LOD node, which is a single scene node or subtree that represents the LOD.
    /// </value>
    public SceneNode Node { get; set; }


    #region ----- Equality Members -----

    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is LodEntry && Equals((LodEntry)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(LodEntry other)
    {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      return Distance == other.Distance
             && Node == other.Node;
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = Distance.GetHashCode();
        hashCode = (hashCode * 397) ^ ((Node != null) ? Node.GetHashCode() : 0);
        return hashCode;
      }
    }


    /// <summary>
    /// Compares two <see cref="LodEntry"/> instances to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="LodEntry"/>.</param>
    /// <param name="right">The second <see cref="LodEntry"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(LodEntry left, LodEntry right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="LodEntry"/> instances to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="LodEntry"/>.</param>
    /// <param name="right">The second <see cref="LodEntry"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(LodEntry left, LodEntry right)
    {
      return !left.Equals(right);
    }
    #endregion
    
  }
}
