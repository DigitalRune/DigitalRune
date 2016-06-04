// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Describes a custom vertex format that contains instance data for hardware instancing.
  /// </summary>
  /// <remarks>
  /// <para>
  /// When hardware instancing is used in the <see cref="MeshRenderer"/>, the vertex attribute
  /// semantic "BLENDWEIGHT" is used to add instance information to each vertex. The instance
  /// information is stored in 4 float4 vectors (<see cref="Register0"/> to
  /// <see cref="Register3"/>).
  /// </para>
  /// <para>
  /// By default, the <see cref="MeshRenderer"/> uses <see cref="Register0"/> to
  /// <see cref="Register2"/> to store the first 3 rows<sup>1</sup> of the world matrix. The 4th row
  /// is always (0, 0, 0, 1); hence, we can use <see cref="Register3"/> to store additional data.
  /// The <see cref="MeshRenderer"/> uses <see cref="Register3"/> to store
  /// <see cref="DefaultEffectParameterSemantics.InstanceColor"/> and
  /// <see cref="DefaultEffectParameterSemantics.InstanceAlpha"/>.
  /// </para>
  /// <para>
  /// <sup>1</sup> If the world matrix is stored in a <see cref="Matrix44F"/> (DigitalRune data
  /// type), <see cref="Register0"/> to <see cref="Register2"/> store the first 3 rows of the world
  /// matrix. If we are talking about a world matrix in a <see cref="Matrix"/> (XNA data type),
  /// <see cref="Register0"/> to <see cref="Register2"/> store the first 3 columns of the world
  /// matrix. World matrices in DigitalRune Mathematics and XNA are transposed (see documentation of
  /// DigitalRune Mathematics).
  /// </para>
  /// <para>
  /// The above paragraphs describe how the <see cref="MeshRenderer"/> and the default DigitalRune
  /// material shaders use hardware instancing. However, you can use this vertex structure to store
  /// any other form of instance data in 4 float4 vectors. You need to create a special shader which
  /// knows how to interpret the data.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  [StructLayout(LayoutKind.Sequential)]
  public struct InstanceData : IVertexType
  {
    // Per-instance transform matrices are stored in a dynamic vertex buffer. A 4x4
    // transform matrix is passed as four Vector4 values. The Vector4 values need to
    // be declared with the BLENDWEIGHT semantic in the effect file. (But any other
    // semantic could be used as well.)
    // IMPORTANT: The matrices need to be transposed in the shader!


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex declaration.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
    (
      new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
      new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
      new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
      new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
    )
    {
      Name = "InstanceData.VertexDeclaration"
    };


    /// <summary>
    /// The first instance data register.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector4 Register0;


    /// <summary>
    /// The second instance data register.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector4 Register1;


    /// <summary>
    /// The third instance data register.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector4 Register2;


    /// <summary>
    /// The fourth instance data register.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector4 Register3;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the size of the <see cref="InstanceData"/> structure in bytes.
    /// </summary>
    /// <value>The size of the vertex in bytes.</value>
    public static int SizeInBytes
    {
      get { return 16 * 4; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceData"/> struct.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceData"/> struct.
    /// </summary>
    /// <param name="register0">The first instance data register.</param>
    /// <param name="register1">The second instance data register.</param>
    /// <param name="register2">The third instance data register.</param>
    /// <param name="register3">The fourth instance data register.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
    public InstanceData(Vector4 register0, Vector4 register1, Vector4 register2, Vector4 register3)
    {
      Register0 = register0;
      Register1 = register1;
      Register2 = register2;
      Register3 = register3;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceData"/> struct.
    /// </summary>
    /// <param name="world">The world matrix.</param>
    /// <param name="color">The instance color (RGBA).</param>
    /// <remarks>
    /// The first three columns of the world matrix is stored in <see cref="Register0"/> to
    /// <see cref="Register2"/>. The color is stored in <see cref="Register3"/>.
    ///  </remarks>
    public InstanceData(Matrix world, Vector4 color)
    {
      Register0 = new Vector4(world.M11, world.M21, world.M31, world.M41);
      Register1 = new Vector4(world.M12, world.M22, world.M32, world.M42);
      Register2 = new Vector4(world.M13, world.M23, world.M33, world.M43);
      Register3 = new Vector4(color.X, color.Y, color.Z, color.W);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceData"/> struct.
    /// </summary>
    /// <param name="scale">The instance scale.</param>
    /// <param name="pose">The instance pose.</param>
    /// <param name="color">The instance color (RGBA).</param>
    /// <remarks>
    /// A world matrix is created from <paramref name="scale"/> and <paramref name="pose"/>.
    /// The first three columns of the world matrix is stored in <see cref="Register0"/> to
    /// <see cref="Register2"/>. The color is stored in <see cref="Register3"/>.
    ///  </remarks>
    public InstanceData(Vector3F scale, Pose pose, Vector4F color)
    {
      Matrix world = pose;
      world.M11 *= scale.X; world.M12 *= scale.X; world.M13 *= scale.X;
      world.M21 *= scale.Y; world.M22 *= scale.Y; world.M23 *= scale.Y;
      world.M31 *= scale.Z; world.M32 *= scale.Z; world.M33 *= scale.Z;
      Register0 = new Vector4(world.M11, world.M21, world.M31, world.M41);
      Register1 = new Vector4(world.M12, world.M22, world.M32, world.M42);
      Register2 = new Vector4(world.M13, world.M23, world.M33, world.M43);
      Register3 = new Vector4(color.X, color.Y, color.Z, color.W);
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
      return obj is InstanceData && this == (InstanceData)obj;
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
        int hashCode = Register0.GetHashCode();
        hashCode = (hashCode * 397) ^ Register1.GetHashCode();
        hashCode = (hashCode * 397) ^ Register2.GetHashCode();
        hashCode = (hashCode * 397) ^ Register3.GetHashCode();
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
    public static bool operator ==(InstanceData left, InstanceData right)
    {
      return (left.Register0 == right.Register0)
             && (left.Register1 == right.Register1)
             && (left.Register2 == right.Register2)
             && (left.Register3 == right.Register3);
    }


    /// <summary>
    /// Compares two objects to determine whether they are different. 
    /// </summary>
    /// <param name="left">Object to the left of the inequality operator.</param>
    /// <param name="right">Object to the right of the inequality operator.</param>
    /// <returns>
    /// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise.
    /// </returns>
    public static bool operator !=(InstanceData left, InstanceData right)
    {
      return (left.Register0 != right.Register0)
             || (left.Register1 != right.Register1)
             || (left.Register2 != right.Register2)
             || (left.Register3 != right.Register3);
    }


    /// <summary>
    /// Retrieves a string representation of this object.
    /// </summary>
    /// <returns>String representation of this object.</returns>
    public override string ToString()
    {
      return string.Format(
        CultureInfo.CurrentCulture,
        "{{Register0:{0} Register1:{1} Register2:{2} Register3:{3}}}",
        Register0, Register1, Register2, Register3);
    }
    #endregion
  }
}
