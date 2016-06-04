// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Vertex format used in "Line.fx".
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  //[Serializable] // HalfVector4 is not serializable!
#endif
  [StructLayout(LayoutKind.Sequential)]
  internal struct StrokeVertex : IVertexType
  {
    // TODO: Compress vertex data. (Byte4 for color - not yet implemented in MonoGame)


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    public static readonly VertexDeclaration VertexDeclaration = 
      new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0), 
        new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(32, VertexElementFormat.HalfVector4, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(40, VertexElementFormat.HalfVector4, VertexElementUsage.TextureCoordinate, 2),
        new VertexElement(48, VertexElementFormat.HalfVector4, VertexElementUsage.TextureCoordinate, 3)
        )
      {
        Name = "StrokeVertex.VertexDeclaration"
      };

    public Vector4 Start;     // Start position (world space) and distance (for dash patterns).
    public Vector4 End;       // End position (world space) and distance (for dash patterns);
    public HalfVector4 Data;  // (U, V, Thickness)
    public HalfVector4 Color; // RGBA premultiplied
    public HalfVector4 Dash;  // Dash pattern data
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
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
      return obj is StrokeVertex && this == (StrokeVertex)obj;
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
        int hashCode = Start.GetHashCode();
        hashCode = (hashCode * 397) ^ End.GetHashCode();
        hashCode = (hashCode * 397) ^ Data.GetHashCode();
        hashCode = (hashCode * 397) ^ Color.GetHashCode();
        hashCode = (hashCode * 397) ^ Dash.GetHashCode();
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
    public static bool operator ==(StrokeVertex left, StrokeVertex right)
    {
      return (left.Start == right.Start)
             && (left.End == right.End)
             && (left.Data == right.Data)
             && (left.Color == right.Color)
             && (left.Dash == right.Dash);
    }


    /// <summary>
    /// Compares two objects to determine whether they are different. 
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator !=(StrokeVertex left, StrokeVertex right)
    {
      return (left.Start != right.Start)
             || (left.End != right.End)
             || (left.Data != right.Data)
             || (left.Color != right.Color)
             || (left.Dash != right.Dash);
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return string.Format(
        CultureInfo.CurrentCulture,
        "{{Start:{0} End:{1} Data:{2} Color:{3} Dash:{4}}}",
        Start, End, Data, Color, Dash); 
    }
    #endregion
  }
}
