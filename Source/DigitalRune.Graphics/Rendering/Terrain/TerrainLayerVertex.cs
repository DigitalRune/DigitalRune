// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Describes a custom vertex format structure for terrain clipmap rendering.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  [StructLayout(LayoutKind.Sequential)]
  internal struct TerrainLayerVertex : IVertexType
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly VertexDeclaration VertexDeclaration =
      new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
        new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0))
      {
        Name = "TerrainLayerVertex.VertexDeclaration"
      };


    /// <summary>
    /// The vertex position.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector2 Position;


    /// <summary>
    /// The texture coordinates.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector2 TextureCoordinate;
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
      get { return 16; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainLayerVertex" /> structure.
    /// </summary>
    /// <param name="position">The position of the vertex.</param>
    /// <param name="textureCoordinate">The texture coordinates.</param>
    public TerrainLayerVertex(Vector2 position, Vector2 textureCoordinate)
    {
      Position = position;
      TextureCoordinate = textureCoordinate;
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
      return obj is TerrainLayerVertex && this == (TerrainLayerVertex)obj;
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
        int hashCode = Position.GetHashCode();
        hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
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
    public static bool operator ==(TerrainLayerVertex left, TerrainLayerVertex right)
    {
      return (left.Position == right.Position)
             && (left.TextureCoordinate == right.TextureCoordinate);
    }


    /// <summary>
    /// Compares two objects to determine whether they are different.
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
    /// </returns>
    public static bool operator !=(TerrainLayerVertex left, TerrainLayerVertex right)
    {
      return (left.Position != right.Position)
             || (left.TextureCoordinate != right.TextureCoordinate);
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return string.Format(
        CultureInfo.CurrentCulture,
        "{{Position:{0} TextureCoordinate:{1}}}",
        Position, TextureCoordinate);
    }
    #endregion
  }
}
