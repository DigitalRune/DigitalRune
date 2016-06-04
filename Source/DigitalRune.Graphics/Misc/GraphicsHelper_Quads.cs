// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class GraphicsHelper
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // For rendering of quads in screen space:
    private static readonly VertexPositionTexture[] QuadVertices =
    {
      new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
      new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1)),
      new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 0)),
      new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1)),
    };
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

    /// <overloads>
    /// <summary>
    /// Draws a screen-aligned quad.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Draws a screen-aligned quad.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="rectangle">
    /// The rectangle describing vertex positions of the quad. (This can be in pixels or in another
    /// unit depending on the current vertex shader.)
    /// </param>
    /// <remarks>
    /// <para>
    /// The quad vertices use the vertex type <see cref="VertexPositionTexture"/>.
    /// </para>
    /// <para>
    /// The upper left corner of the quad uses the texture coordinate (0, 0), the lower right corner
    /// of the quad uses the texture coordinate (0, 1).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static void DrawQuad(this GraphicsDevice graphicsDevice, Rectangle rectangle)
    {
      DrawQuad(graphicsDevice, rectangle, new Vector2F(0, 0), new Vector2F(1, 1));
    }


    /// <summary>
    /// Draws a screen-aligned quad.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="rectangle">
    /// The rectangle describing vertex positions of the quad. (This can be in pixels or in another
    /// unit depending on the current vertex shader.)
    /// </param>
    /// <param name="texCoordTopLeft">
    /// The texture coordinates of the top left vertex of the quad.
    /// </param>
    /// <param name="texCoordBottomRight">
    /// The texture coordinates of the bottom right vertex of the quad.</param>
    /// <remarks>
    /// <para>
    /// The quad vertices use the vertex type <see cref="VertexPositionTexture"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void DrawQuad(this GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F texCoordTopLeft, Vector2F texCoordBottomRight)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      int left = rectangle.Left;
      int top = rectangle.Top;
      int bottom = rectangle.Bottom;
      int right = rectangle.Right;

      QuadVertices[0].Position.X = left;
      QuadVertices[0].Position.Y = top;
      QuadVertices[0].Position.Z = 0;
      QuadVertices[0].TextureCoordinate = new Vector2(texCoordTopLeft.X, texCoordTopLeft.Y);
      QuadVertices[1].Position.X = right;
      QuadVertices[1].Position.Y = top;
      QuadVertices[1].Position.Z = 0;
      QuadVertices[1].TextureCoordinate = new Vector2(texCoordBottomRight.X, texCoordTopLeft.Y);
      QuadVertices[2].Position.X = left;
      QuadVertices[2].Position.Y = bottom;
      QuadVertices[2].Position.Z = 0;
      QuadVertices[2].TextureCoordinate = new Vector2(texCoordTopLeft.X, texCoordBottomRight.Y);
      QuadVertices[3].Position.X = right;
      QuadVertices[3].Position.Y = bottom;
      QuadVertices[3].Position.Z = 0;
      QuadVertices[3].TextureCoordinate = new Vector2(texCoordBottomRight.X, texCoordBottomRight.Y);

      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, QuadVertices, 0, 2);
    }


    /// <summary>
    /// Draws a screen-aligned quad.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="topLeft">The top left vertex.</param>
    /// <param name="bottomRight">The bottom right vertex.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static void DrawQuad(this GraphicsDevice graphicsDevice, VertexPositionTexture topLeft, VertexPositionTexture bottomRight)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      // Use the same z position of all vertices.
      var z = topLeft.Position.Z;
      QuadVertices[0].Position.Z = z;
      QuadVertices[1].Position.Z = z;
      QuadVertices[2].Position.Z = z;
      QuadVertices[3].Position.Z = z;

      QuadVertices[0].Position.X = topLeft.Position.X;
      QuadVertices[0].Position.Y = topLeft.Position.Y;
      QuadVertices[0].TextureCoordinate.X = topLeft.TextureCoordinate.X;
      QuadVertices[0].TextureCoordinate.Y = topLeft.TextureCoordinate.Y;
      QuadVertices[1].Position.X = bottomRight.Position.X;
      QuadVertices[1].Position.Y = topLeft.Position.Y;
      QuadVertices[1].TextureCoordinate.X = bottomRight.TextureCoordinate.X;
      QuadVertices[1].TextureCoordinate.Y = topLeft.TextureCoordinate.Y;
      QuadVertices[2].Position.X = topLeft.Position.X;
      QuadVertices[2].Position.Y = bottomRight.Position.Y;
      QuadVertices[2].TextureCoordinate.X = topLeft.TextureCoordinate.X;
      QuadVertices[2].TextureCoordinate.Y = bottomRight.TextureCoordinate.Y;
      QuadVertices[3].Position.X = bottomRight.Position.X;
      QuadVertices[3].Position.Y = bottomRight.Position.Y;
      QuadVertices[3].TextureCoordinate.X = bottomRight.TextureCoordinate.X;
      QuadVertices[3].TextureCoordinate.Y = bottomRight.TextureCoordinate.Y;

      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, QuadVertices, 0, 2);
    }


    /// <summary>
    /// Draws a full-screen quad.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <remarks>
    /// <para>
    /// The quad will fill the current viewport. The vertex positions are in pixels. (The vertex
    /// shader must work like VSScreenSpaceDraw() in "PostProcessing.fxh".)
    /// </para>
    /// <para>
    /// The quad vertices use the vertex type <see cref="VertexPositionTexture"/>.
    /// </para>
    /// <para>
    /// The upper left corner of the quad uses the texture coordinate (0, 0), the lower right corner
    /// of the quad uses the texture coordinate (0, 1).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    public static void DrawFullScreenQuad(this GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      // This code is not needed anymore if we use the current viewport dimensions:

      //RenderTargetBinding[] targets = graphicsDevice.GetRenderTargets();   // Note: GetRenderTargets returns a RenderTargetBinding[0] array if the render target is the back-buffer.
      //Texture2D target = null; 
      //if (targets != null && targets.Length > 0)
      //  target = targets[0].RenderTarget as Texture2D;

      //// Get width and height of current render target.
      //int width, height;
      //if (target == null)
      //{
      //  width = graphicsDevice.PresentationParameters.BackBufferWidth;
      //  height = graphicsDevice.PresentationParameters.BackBufferHeight;
      //}
      //else
      //{
      //  width = target.Width;
      //  height = target.Height;
      //}

      DrawQuad(graphicsDevice, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
    }


    /// <overloads>
    /// <summary>
    /// Gets the view space positions of the 4 far corners of the viewing frustum.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the view space positions of the 4 far corners of the viewing frustum.
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <param name="frustumFarCorners">
    /// A 4 element array that will be initialized with the frustum far corners.
    /// </param>
    /// <remarks>
    /// <paramref name="frustumFarCorners"/> will be initialized with the 4 corner positions (in 
    /// view space) of the far plane of the projection frustum. The order of the corners is: 
    /// top-left, top-right, bottom-left, bottom-right.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projection"/> or <paramref name="frustumFarCorners"/> is 
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="frustumFarCorners"/> has wrong length. Expected length: 4.
    /// </exception>
    public static void GetFrustumFarCorners(Projection projection, Vector3[] frustumFarCorners)
    {
      if (projection == null)
        throw new ArgumentNullException("projection");
      if (frustumFarCorners == null)
        throw new ArgumentNullException("frustumFarCorners");
      if (frustumFarCorners.Length != 4)
        throw new ArgumentException("frustumFarCorners must be an array with 4 elements.");

      var farOverNear = projection.Far / projection.Near;
      frustumFarCorners[0] = new Vector3(projection.Left, projection.Top, -projection.Near) * farOverNear;
      frustumFarCorners[1] = new Vector3(projection.Right, projection.Top, -projection.Near) * farOverNear;
      frustumFarCorners[2] = new Vector3(projection.Left, projection.Bottom, -projection.Near) * farOverNear;
      frustumFarCorners[3] = new Vector3(projection.Right, projection.Bottom, -projection.Near) * farOverNear;
    }


    /// <summary>
    /// Gets the view space positions of the 4 far corners of a quad at the far plane of the viewing
    /// frustum.
    /// </summary>
    /// <param name="projection">The projection.</param>
    /// <param name="topLeftTexCoord">The top left texture coordinate.</param>
    /// <param name="bottomRightTexCoord">The bottom right texture coordinate.</param>
    /// <param name="frustumFarCorners">
    /// A 4 element array that will be initialized with the frustum far corners.
    /// </param>
    /// <remarks>
    /// <paramref name="frustumFarCorners"/> will be initialized with the 4 corner positions (in 
    /// view space) of a quad on the far plane of the projection frustum. The order of the corners
    /// is: top-left, top-right, bottom-left, bottom-right. The position of the quad is 
    /// determined by <paramref name="topLeftTexCoord"/> and <paramref name="bottomRightTexCoord"/>.
    /// If <paramref name="topLeftTexCoord"/> is (0, 0) and <paramref name="bottomRightTexCoord"/>
    /// is (1, 1), then the result is the same as in 
    /// <see cref="GetFrustumFarCorners(Projection,Vector3[])"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projection"/> or <paramref name="frustumFarCorners"/> is 
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="frustumFarCorners"/> has wrong length. Expected length: 4.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void GetFrustumFarCorners(Projection projection, Vector2F topLeftTexCoord, Vector2F bottomRightTexCoord, Vector3[] frustumFarCorners)
    {
      GetFrustumFarCorners(projection, frustumFarCorners);
      float dX = frustumFarCorners[1].X - frustumFarCorners[0].X;
      float dY = frustumFarCorners[2].Y - frustumFarCorners[0].Y;

      float left = frustumFarCorners[0].X;
      float top = frustumFarCorners[0].Y;

      var texCoordLeft = topLeftTexCoord.X;
      var texCoordTop = topLeftTexCoord.Y;
      var texCoordRight = bottomRightTexCoord.X;
      var texCoordBottom = bottomRightTexCoord.Y;

      frustumFarCorners[0].X = left + texCoordLeft * dX;
      frustumFarCorners[0].Y = top + texCoordTop * dY;
      frustumFarCorners[1].X = left + texCoordRight * dX;
      frustumFarCorners[1].Y = top + texCoordTop * dY;
      frustumFarCorners[2].X = left + texCoordLeft * dX;
      frustumFarCorners[2].Y = top + texCoordBottom * dY;
      frustumFarCorners[3].X = left + texCoordRight * dX;
      frustumFarCorners[3].Y = top + texCoordBottom * dY;
    }
    #endregion
  }
}
