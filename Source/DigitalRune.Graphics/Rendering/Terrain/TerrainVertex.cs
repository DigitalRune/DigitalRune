// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Describes a custom vertex format structure for terrain rendering.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  [StructLayout(LayoutKind.Sequential)]
  internal struct TerrainVertex : IVertexType
  {
    // This struct is like VertexPosition. However, we use HalfVector4 instead of Vector3.
    // HalfVector3 would be even better - or HalfVector2 (X, Z) + Byte (LOD).

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly VertexDeclaration VertexDeclaration =
      new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.HalfVector4, VertexElementUsage.Position, 0))
      {
        Name = "TerrainVertex.VertexDeclaration"
      };


    /// <summary>
    /// The vertex position.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public HalfVector4 Position;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the size of the <see cref="TerrainVertex"/> structure in bytes.
    /// </summary>
    /// <value>The size of the vertex in bytes.</value>
    public static int SizeInBytes
    {
      get { return 8; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainVertex"/> struct.
    /// </summary>
    /// <param name="position">The position of the vertex.</param>
    public TerrainVertex(HalfVector4 position)
    {
      Position = position;
    }
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
      return obj is TerrainVertex && this == (TerrainVertex)obj;
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      return Position.GetHashCode();
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
    public static bool operator ==(TerrainVertex left, TerrainVertex right)
    {
      return left.Position == right.Position;
    }


    /// <summary>
    /// Compares two objects to determine whether they are different.
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
    /// </returns>
    public static bool operator !=(TerrainVertex left, TerrainVertex right)
    {
      return left.Position != right.Position;
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return string.Format(CultureInfo.CurrentCulture, "{{Position:{0}}}", Position);
    }
    #endregion
  }
}
