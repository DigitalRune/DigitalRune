// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Vertex format used in "OcclusionCulling.fx".
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  [StructLayout(LayoutKind.Sequential)]
  internal struct OcclusionVertex : IVertexType
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    public static readonly VertexDeclaration VertexDeclaration =
      new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 3),
        new VertexElement(56, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 4))
      {
        Name = "OcclusionVertex.VertexDeclaration"
      };

    // See OcclusionCulling.fx for description.
    public Vector2F Pixel;
    public Vector3F Minimum;
    public Vector3F Maximum;

    // For LOD metric and distance culling.
    public Vector3F Position;
    public Vector3F Scale;
    public float MaxDistance;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the size of the <see cref="OcclusionVertex"/> structure in bytes.
    /// </summary>
    /// <value>The size of the vertex in bytes.</value>
    public static int SizeInBytes
    {
      get { return 8 + 12 + 12 + 12 + 12 + 4; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and 
    /// represent the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is OcclusionVertex && this == (OcclusionVertex)obj;
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = Pixel.GetHashCode();
        hashCode = (hashCode * 397) ^ Minimum.GetHashCode();
        hashCode = (hashCode * 397) ^ Maximum.GetHashCode();
        hashCode = (hashCode * 397) ^ Position.GetHashCode();
        hashCode = (hashCode * 397) ^ Scale.GetHashCode();
        hashCode = (hashCode * 397) ^ MaxDistance.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Gets the vertex declaration.
    /// </summary>
    /// <value>The vertex declaration.</value>
    VertexDeclaration IVertexType.VertexDeclaration
    {
      get { return VertexDeclaration; }
    }


    /// <summary>
    /// Compares two objects to determine whether they are the same. 
    /// </summary>
    /// <param name="left">Object to the left of the equality operator.</param>
    /// <param name="right">Object to the right of the equality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator ==(OcclusionVertex left, OcclusionVertex right)
    {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return (left.Pixel == right.Pixel)
             && (left.Minimum == right.Minimum)
             && (left.Maximum == right.Maximum)
             && (left.Position == right.Position)
             && (left.Scale == right.Scale)
             && (left.MaxDistance == right.MaxDistance);
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }


    /// <summary>
    /// Compares two objects to determine whether they are different. 
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator !=(OcclusionVertex left, OcclusionVertex right)
    {
      // ReSharper disable CompareOfFloatsByEqualityOperator
      return (left.Pixel != right.Pixel)
             && (left.Minimum != right.Minimum)
             && (left.Maximum != right.Maximum)
             && (left.Position != right.Position)
             && (left.Scale != right.Scale)
             && (left.MaxDistance != right.MaxDistance);
      // ReSharper restore CompareOfFloatsByEqualityOperator
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.CurrentCulture,
        "{{Pixel:{0} Minimum:{1} Maximum:{2} Position:{3} Scale:{4} MaxDistance:{5}}}",
        Pixel, Minimum, Maximum, Position, Scale, MaxDistance);
    }
    #endregion
  }
}
