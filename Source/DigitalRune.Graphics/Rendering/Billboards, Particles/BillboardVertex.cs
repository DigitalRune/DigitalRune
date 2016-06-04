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
  /// Vertex format used in "Billboard.fx".
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  [StructLayout(LayoutKind.Sequential)]
  internal struct BillboardVertex : IVertexType
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    public static readonly VertexDeclaration VertexDeclaration = 
      new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),            // Position
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),             // Normal
        new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),            // Axis
        new VertexElement(36, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),              // Color
        new VertexElement(52, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),  // Texture coordinate
        new VertexElement(60, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),  // Args0
        new VertexElement(76, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),  // Args1
        new VertexElement(92, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),  // Args2
        new VertexElement(108, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4)) // Args3
      {
        Name = "BillboardVertex.VertexDeclaration"
      };

    // See Billboard.fx for description.
    public Vector3F Position;
    public Vector3F Normal;   // If (0, 0, 0) => vertex belongs to ribbon.
    public Vector3F Axis;
    public Vector4F Color;
    public Vector2F TextureCoordinate;
    public Vector4F Args0;
    public Vector4F Args1;
    public Vector4F Args2;
    public Vector4F Args3;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the size of the <see cref="BillboardVertex"/> structure in bytes.
    /// </summary>
    /// <value>The size of the vertex in bytes.</value>
    public static int SizeInBytes
    {
      get { return 12 + 12 + 12 + 16 + 8 + 16 + 16 + 16 + 16; }
    }


    // The (non-premultiplied) tint color.
    public Vector3F Color3F
    {
      set 
      { 
        Color.X = value.X;
        Color.Y = value.Y;
        Color.Z = value.Z;
      }
    }


    // The billboard opacity.
    public float Alpha
    {
      set { Color.W = value; }
    }


    // The billboard orientation.
    public BillboardOrientation Orientation
    {
      set
      {
        Args0.X = (value.Normal == BillboardNormal.ViewpointOriented) ? 1 : 0;
        Args0.Y = value.IsAxisInViewSpace ? 1 : 0;
        Args0.Z = value.IsAxisFixed ? 1 : 0;
      }
    }


    // The rotation angle [rad].
    public float Angle
    {
      set { Args0.W = value; }
    }


    // The billboard size (width, height).
    public Vector2F Size
    {
      set 
      {
        Args1.X = value.X;
        Args1.Y = value.Y;
      }
    }


    // The soft particle distance threshold.
    public float Softness
    {
      set { Args1.Z = value; }
    }


    // The reference value used in the alpha test.
    public float ReferenceAlpha
    {
      set { Args1.W = value; }
    }


    // The packed texture.
    public PackedTexture Texture
    {
      set
      {
        Vector2F textureScale = value.Scale;
        Args2.X = textureScale.X;
        Args2.Y = textureScale.Y;

        Vector2F textureOffset = value.Offset;
        Args2.Z = textureOffset.X;
        Args2.W = textureOffset.Y;

        Args3.X = value.NumberOfColumns;
        Args3.Y = value.NumberOfRows;
      }
    }


    // The normalized animation time [0, 1].
    public float AnimationTime
    {
      set { Args3.Z = value; }
    }


    // The blend mode (0 = additive, 1 = alpha blend).
    public float BlendMode
    {
      set { Args3.W = value; }
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
      return obj is BillboardVertex && this == (BillboardVertex)obj;
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
        hashCode = (hashCode * 397) ^ Normal.GetHashCode();
        hashCode = (hashCode * 397) ^ Axis.GetHashCode();
        hashCode = (hashCode * 397) ^ Color.GetHashCode();
        hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
        hashCode = (hashCode * 397) ^ Args0.GetHashCode();
        hashCode = (hashCode * 397) ^ Args1.GetHashCode();
        hashCode = (hashCode * 397) ^ Args2.GetHashCode();
        hashCode = (hashCode * 397) ^ Args3.GetHashCode();
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
    public static bool operator ==(BillboardVertex left, BillboardVertex right)
    {
      return (left.Position == right.Position)
             && (left.Normal == right.Normal)
             && (left.Axis == right.Axis)
             && (left.Color == right.Color)
             && (left.TextureCoordinate == right.TextureCoordinate)
             && (left.Args0 == right.Args0)
             && (left.Args1 == right.Args1)
             && (left.Args2 == right.Args2)
             && (left.Args3 == right.Args3);
    }


    /// <summary>
    /// Compares two objects to determine whether they are different. 
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise. 
    /// </returns>
    public static bool operator !=(BillboardVertex left, BillboardVertex right)
    {
      return (left.Position != right.Position)
             && (left.Normal != right.Normal)
             && (left.Axis != right.Axis)
             && (left.Color != right.Color)
             && (left.TextureCoordinate != right.TextureCoordinate)
             && (left.Args0 != right.Args0)
             && (left.Args1 != right.Args1)
             && (left.Args2 != right.Args2)
             && (left.Args3 != right.Args3);
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.CurrentCulture,
        "{{Position:{0} Normal:{1} Axis:{2} Color:{3} TextureCoordinate:{4}}}",
        Position, Normal, Axis, Color, TextureCoordinate);
    }
    #endregion
  }
}
