// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders billboards and particles in batches. (HiDef profile)
  /// </summary>
  /// <inheritdoc/>
  internal sealed class BillboardBatchHiDef : BillboardBatch<BillboardVertex>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardBatchHiDef" /> class.
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
    public BillboardBatchHiDef(GraphicsDevice graphicsDevice, int bufferSize)
      : base(graphicsDevice, bufferSize)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnDrawBillboard(ref BillboardArgs b, PackedTexture texture, BillboardVertex[] vertices, int index)
    {
      // Bottom left vertex
      var v = new BillboardVertex();
      v.Position = b.Position;
      v.Normal = b.Normal;
      v.Axis = b.Axis;
      v.Color3F = b.Color;
      v.Alpha = b.Alpha;
      v.TextureCoordinate = new Vector2F(0, 1);
      v.Orientation = b.Orientation;
      v.Angle = b.Angle;
      v.Size = b.Size;
      v.Softness = b.Softness;
      v.ReferenceAlpha = b.ReferenceAlpha;
      v.AnimationTime = b.AnimationTime;
      v.BlendMode = b.BlendMode;
      v.Texture = texture;
      vertices[index] = v;
      index++;

      // Top left vertex
      v.TextureCoordinate.Y = 0;
      vertices[index] = v;
      index++;

      // Top right vertex
      v.TextureCoordinate.X = 1;
      vertices[index] = v;
      index++;

      // Bottom right vertex
      v.TextureCoordinate.Y = 1;
      vertices[index] = v;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnDrawRibbon(ref RibbonArgs p0, ref RibbonArgs p1, PackedTexture texture, BillboardVertex[] vertices, int index)
    {
      // p0 and p1 specify a segment of a particle ribbon.
      //   --+--------------+--
      //    p0             p1  
      //   --+--------------+--
     
      // Bottom left vertex
      var v = new BillboardVertex();
      v.Position = p0.Position;
      v.Axis = p0.Axis;
      v.Color3F = p0.Color;
      v.Alpha = p0.Alpha;
      v.TextureCoordinate = new Vector2F(p0.TextureCoordinateU, 1);
      v.Size = new Vector2F(p0.Size);
      v.Softness = p0.Softness;
      v.ReferenceAlpha = p0.ReferenceAlpha;
      v.AnimationTime = p0.AnimationTime;
      v.BlendMode = p0.BlendMode;
      v.Texture = texture;
      vertices[index] = v;
      index++;

      // Top left vertex
      v.TextureCoordinate.Y = 0;
      vertices[index] = v;
      index++;

      // Top right vertex
      v.Position = p1.Position;
      v.Axis = p1.Axis;
      v.Color3F = p1.Color;
      v.Alpha = p1.Alpha;
      v.TextureCoordinate = new Vector2F(p1.TextureCoordinateU, 0);
      v.Size = new Vector2F(p1.Size);
      v.Softness = p1.Softness;
      v.ReferenceAlpha = p1.ReferenceAlpha;
      v.AnimationTime = p1.AnimationTime;
      v.BlendMode = p1.BlendMode;
      vertices[index] = v;
      index++;

      // Bottom right vertex
      v.TextureCoordinate.Y = 1;
      vertices[index] = v;
    }
    #endregion
  }
}
