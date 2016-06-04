// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders billboards and particles in batches. (Reach profile)
  /// </summary>
  /// <remarks>
  /// Billboards and particles are written into a dynamic vertex buffer. When the buffer size is 
  /// exceeded then the data is submitted and the batch is restarted.
  /// </remarks>
  internal sealed class BillboardBatchReach : BillboardBatch<VertexPositionColorTexture>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Camera parameters.
    private Pose _cameraPose;
    private Vector3F _cameraDown;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion
    

    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardBatchReach" /> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="bufferSize">The size of the internal buffer (number of particles).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="bufferSize"/> is 0, negative, or greater than 
    /// <see cref="BillboardBatch{T}.MaxBufferSize"/>.
    /// </exception>
    public BillboardBatchReach(GraphicsDevice graphicsDevice, int bufferSize)
      : base(graphicsDevice, bufferSize)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void Begin(RenderContext context)
    {
      base.Begin(context);

      // Cache camera parameters.
      var cameraNode = context.CameraNode;
      _cameraPose = cameraNode.PoseWorld;
      _cameraDown = -_cameraPose.Orientation.GetColumn(1);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnDrawBillboard(ref BillboardArgs b, PackedTexture texture, VertexPositionColorTexture[] vertices, int index)
    {
      #region ----- Billboarding -----

      // The billboard orientation is defined by three vectors: normal (pointing to the camera),
      // up and right (both lying in the billboard plane).
      // normal and up are given. right is computed using the cross product up x normal.
      // normal and up should be perpendicular, but usually they are not. Therefore, one vector
      // must be corrected. For spherical billboards, the normal is fixed and the up vector
      // is corrected. For cylindrical billboards (= axial billboards), the up vector is fixed
      // and the b.Normal is corrected.

      // Normal
      if (b.Orientation.Normal == BillboardNormal.ViewpointOriented)
      {
        Vector3F normal = _cameraPose.Position - b.Position;
        if (normal.TryNormalize())
          b.Normal = normal;
      }

      // Axis = up vector
      if (b.Orientation.IsAxisInViewSpace)
        b.Axis = _cameraPose.ToWorldDirection(b.Axis);

      if (1 - Vector3F.Dot(b.Normal, b.Axis) < Numeric.EpsilonF)
      {
        // Normal and axis are parallel.
        // --> Bend normal by adding a fraction of the camera down vector.
        b.Normal += _cameraDown * 0.001f;
        b.Normal.Normalize();
      }

      // Compute right.
      //Vector3F right = Vector3F.Cross(b.Axis, b.Normal);
      // Inlined:
      Vector3F right;
      right.X = b.Axis.Y * b.Normal.Z - b.Axis.Z * b.Normal.Y;
      right.Y = b.Axis.Z * b.Normal.X - b.Axis.X * b.Normal.Z;
      right.Z = b.Axis.X * b.Normal.Y - b.Axis.Y * b.Normal.X;
      if (!right.TryNormalize())
        right = b.Normal.Orthonormal1;   // Normal and axis are parallel --> Choose random perpendicular vector.

      if (b.Orientation.IsAxisFixed)
      {
        // Make sure normal is perpendicular to right and up.
        //normal = Vector3F.Cross(right, b.Axis);
        // Inlined:
        b.Normal.X = right.Y * b.Axis.Z - right.Z * b.Axis.Y;
        b.Normal.Y = right.Z * b.Axis.X - right.X * b.Axis.Z;
        b.Normal.Z = right.X * b.Axis.Y - right.Y * b.Axis.X;

        // No need to normalize because right and up are normalized and perpendicular.
      }
      else
      {
        // Make sure axis is perpendicular to normal and right.
        //b.Axis = Vector3F.Cross(b.Normal, right);
        // Inlined:
        b.Axis.X = b.Normal.Y * right.Z - b.Normal.Z * right.Y;
        b.Axis.Y = b.Normal.Z * right.X - b.Normal.X * right.Z;
        b.Axis.Z = b.Normal.X * right.Y - b.Normal.Y * right.X;

        // No need to normalize because normal and right are normalized and perpendicular.
      }
      #endregion

      #region ----- Rotate up and right vectors -----

      Vector3F upRotated;
      Vector3F rightRotated;

      if (b.Angle != 0.0f)
      {
        // Rotate up and right.
        // Here is the readable code.
        //Matrix33F rotation = Matrix33F.CreateRotation(b.Normal, b.Angle);
        //Vector3F upRotated = rotation * b.Axis;
        //Vector3F rightRotated = rotation * right;

        // Inlined code:
        float x = b.Normal.X;
        float y = b.Normal.Y;
        float z = b.Normal.Z;
        float x2 = x * x;
        float y2 = y * y;
        float z2 = z * z;
        float xy = x * y;
        float xz = x * z;
        float yz = y * z;
        float cos = (float)Math.Cos(b.Angle);
        float sin = (float)Math.Sin(b.Angle);
        float xsin = x * sin;
        float ysin = y * sin;
        float zsin = z * sin;
        float oneMinusCos = 1.0f - cos;
        float m00 = x2 + cos * (1.0f - x2);
        float m01 = xy * oneMinusCos - zsin;
        float m02 = xz * oneMinusCos + ysin;
        float m10 = xy * oneMinusCos + zsin;
        float m11 = y2 + cos * (1.0f - y2);
        float m12 = yz * oneMinusCos - xsin;
        float m20 = xz * oneMinusCos - ysin;
        float m21 = yz * oneMinusCos + xsin;
        float m22 = z2 + cos * (1.0f - z2);

        upRotated.X = m00 * b.Axis.X + m01 * b.Axis.Y + m02 * b.Axis.Z;
        upRotated.Y = m10 * b.Axis.X + m11 * b.Axis.Y + m12 * b.Axis.Z;
        upRotated.Z = m20 * b.Axis.X + m21 * b.Axis.Y + m22 * b.Axis.Z;

        rightRotated.X = m00 * right.X + m01 * right.Y + m02 * right.Z;
        rightRotated.Y = m10 * right.X + m11 * right.Y + m12 * right.Z;
        rightRotated.Z = m20 * right.X + m21 * right.Y + m22 * right.Z;
      }
      else
      {
        // Angle is 0 - no rotation.
        upRotated = b.Axis;
        rightRotated = right;
      }
      #endregion

      #region ----- Handle texture information and size -----

      Vector2F texCoordTopLeft = texture.GetTextureCoordinates(Vector2F.Zero, b.AnimationTime);
      Vector2F texCoordBottomRight = texture.GetTextureCoordinates(Vector2F.One, b.AnimationTime);

      // Handle mirroring.
      if (b.Size.X < 0)
      {
        b.Size.X = -b.Size.X;
        MathHelper.Swap(ref texCoordTopLeft.X, ref texCoordBottomRight.X);
      }
      if (b.Size.Y < 0)
      {
        b.Size.Y = -b.Size.Y;
        MathHelper.Swap(ref texCoordTopLeft.Y, ref texCoordBottomRight.Y);
      }

      b.Size.X /= 2.0f;
      b.Size.Y /= 2.0f;

      // Offset from billboard center to right edge.
      Vector3F hOffset;
      hOffset.X = rightRotated.X * b.Size.X;
      hOffset.Y = rightRotated.Y * b.Size.X;
      hOffset.Z = rightRotated.Z * b.Size.X;

      // Offset from reference point to top edge.
      Vector3F vOffset;
      vOffset.X = upRotated.X * b.Size.Y;
      vOffset.Y = upRotated.Y * b.Size.Y;
      vOffset.Z = upRotated.Z * b.Size.Y;
      #endregion

      #region ----- Get Color -----

      // Premultiply alpha.
      Vector4 color4 = new Vector4
      {
        X = b.Color.X * b.Alpha,
        Y = b.Color.Y * b.Alpha,
        Z = b.Color.Z * b.Alpha,

        // Apply blend mode (0 = additive, 1 = alpha blend).
        W = b.Alpha * b.BlendMode
      };

      var color = new Color(color4);
      #endregion

      #region ----- Initializes vertices in vertex array -----

      // Bottom left vertex
      vertices[index].Position.X = b.Position.X - hOffset.X - vOffset.X;
      vertices[index].Position.Y = b.Position.Y - hOffset.Y - vOffset.Y;
      vertices[index].Position.Z = b.Position.Z - hOffset.Z - vOffset.Z;
      vertices[index].Color = color;
      vertices[index].TextureCoordinate.X = texCoordTopLeft.X;
      vertices[index].TextureCoordinate.Y = texCoordBottomRight.Y;
      index++;

      // Top left vertex
      vertices[index].Position.X = b.Position.X - hOffset.X + vOffset.X;
      vertices[index].Position.Y = b.Position.Y - hOffset.Y + vOffset.Y;
      vertices[index].Position.Z = b.Position.Z - hOffset.Z + vOffset.Z;
      vertices[index].Color = color;
      vertices[index].TextureCoordinate.X = texCoordTopLeft.X;
      vertices[index].TextureCoordinate.Y = texCoordTopLeft.Y;
      index++;

      // Top right vertex
      vertices[index].Position.X = b.Position.X + hOffset.X + vOffset.X;
      vertices[index].Position.Y = b.Position.Y + hOffset.Y + vOffset.Y;
      vertices[index].Position.Z = b.Position.Z + hOffset.Z + vOffset.Z;
      vertices[index].Color = color;
      vertices[index].TextureCoordinate.X = texCoordBottomRight.X;
      vertices[index].TextureCoordinate.Y = texCoordTopLeft.Y;
      index++;

      // Bottom right vertex
      vertices[index].Position.X = b.Position.X + hOffset.X - vOffset.X;
      vertices[index].Position.Y = b.Position.Y + hOffset.Y - vOffset.Y;
      vertices[index].Position.Z = b.Position.Z + hOffset.Z - vOffset.Z;
      vertices[index].Color = color;
      vertices[index].TextureCoordinate.X = texCoordBottomRight.X;
      vertices[index].TextureCoordinate.Y = texCoordBottomRight.Y;
      #endregion
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnDrawRibbon(ref RibbonArgs p0, ref RibbonArgs p1, PackedTexture texture, VertexPositionColorTexture[] vertices, int index)
    {
      // p0 and p1 specify a segment of a particle ribbon.
      //   --+--------------+--
      //    p0             p1  
      //   --+--------------+--

      #region ----- Handle texture information and size -----

      float animationTime = p0.AnimationTime;
      Vector2F texCoordTopLeft = texture.GetTextureCoordinates(new Vector2F(p0.TextureCoordinateU, 0), animationTime);
      Vector2F texCoordBottomRight = texture.GetTextureCoordinates(new Vector2F(p1.TextureCoordinateU, 1), animationTime);

      // Negative sizes (mirroring) is not supported because this conflicts with the
      // texture tiling on ribbons.
      float size0 = Math.Abs(p0.Size) / 2;
      float size1 = Math.Abs(p1.Size) / 2;

      // Offset from particle center to upper edge.
      Vector3F up0;
      up0.X = p0.Axis.X * size0;
      up0.Y = p0.Axis.Y * size0;
      up0.Z = p0.Axis.Z * size0;

      Vector3F up1;
      up1.X = p1.Axis.X * size1;
      up1.Y = p1.Axis.Y * size1;
      up1.Z = p1.Axis.Z * size1;
      #endregion

      #region ----- Get Color -----

      // Premultiply alpha.
      Vector4 color4 = new Vector4
      {
        X = p0.Color.X * p0.Alpha,
        Y = p0.Color.Y * p0.Alpha,
        Z = p0.Color.Z * p0.Alpha,

        // Apply blend mode (0 = additive, 1 = alpha blend).
        W = p0.Alpha * p0.BlendMode
      };
      var color0 = new Color(color4);

      color4 = new Vector4
      {
        X = p1.Color.X * p1.Alpha,
        Y = p1.Color.Y * p1.Alpha,
        Z = p1.Color.Z * p1.Alpha,
        W = p1.Alpha * p1.BlendMode
      };

      var color1 = new Color(color4);
      #endregion

      #region ----- Initializes vertices in vertex array -----

      // Bottom left vertex
      vertices[index].Position.X = p0.Position.X - up0.X;
      vertices[index].Position.Y = p0.Position.Y - up0.Y;
      vertices[index].Position.Z = p0.Position.Z - up0.Z;
      vertices[index].Color = color0;
      vertices[index].TextureCoordinate.X = texCoordTopLeft.X;
      vertices[index].TextureCoordinate.Y = texCoordBottomRight.Y;
      index++;

      // Top left vertex
      vertices[index].Position.X = p0.Position.X + up0.X;
      vertices[index].Position.Y = p0.Position.Y + up0.Y;
      vertices[index].Position.Z = p0.Position.Z + up0.Z;
      vertices[index].Color = color0;
      vertices[index].TextureCoordinate.X = texCoordTopLeft.X;
      vertices[index].TextureCoordinate.Y = texCoordTopLeft.Y;
      index++;

      // Top right vertex
      vertices[index].Position.X = p1.Position.X + up1.X;
      vertices[index].Position.Y = p1.Position.Y + up1.Y;
      vertices[index].Position.Z = p1.Position.Z + up1.Z;
      vertices[index].Color = color1;
      vertices[index].TextureCoordinate.X = texCoordBottomRight.X;
      vertices[index].TextureCoordinate.Y = texCoordTopLeft.Y;
      index++;

      // Bottom right vertex
      vertices[index].Position.X = p1.Position.X - up1.X;
      vertices[index].Position.Y = p1.Position.Y - up1.Y;
      vertices[index].Position.Z = p1.Position.Z - up1.Z;
      vertices[index].Color = color1;
      vertices[index].TextureCoordinate.X = texCoordBottomRight.X;
      vertices[index].TextureCoordinate.Y = texCoordBottomRight.Y;
      #endregion
    }
    #endregion
  }
}
