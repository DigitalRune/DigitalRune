// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders a batch of points as screen-aligned quads.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A valid <see cref="Effect"/> must be set; otherwise, <see cref="Render"/> will not draw any 
  /// points. The <see cref="PointBatch"/> uses the currently set blend state and depth-stencil 
  /// state. 
  /// </para>
  /// </remarks>
  internal sealed class PointBatch 
  {
    // TODO:
    // - Avoid recomputation of vertices if Points collection and the camera have 
    //   not changed. (Create a special PointsCollection with a dirty flag and store 
    //   the last WorldViewProjection matrix.)
    // - Inline vector operations.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Defines a point.
    /// </summary>
    private struct Point
    {
      /// <summary>The position in world space.</summary>
      public readonly Vector3F Position;

      /// <summary>The color (using premultiplied alpha).</summary>
      public readonly Color Color;

      /// <summary>
      /// Initializes a new instance of the <see cref="Point"/> struct.
      /// </summary>
      /// <param name="position">The position.</param>
      /// <param name="color">The color (using premultiplied alpha).</param>
      public Point(Vector3F position, Color color)
      {
        Position = position;
        Color = color;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<Point> _points = new List<Point>();
    private VertexPositionColor[] _buffer = new VertexPositionColor[128];
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the effect.
    /// </summary>
    /// <value>The effect.</value>
    /// <remarks>
    /// If this value is <see langword="null"/>, then <see cref="Render"/> does nothing.
    /// </remarks>
    public BasicEffect Effect { get; set; }


    /// <summary>
    /// Gets or sets the size of drawn points.
    /// </summary>
    /// <value>The size of a visible point (in pixels). The default value is 5.</value>
    public float PointSize { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PointBatch"/> class.
    /// </summary>
    /// <param name="effect">
    /// The effect. If this value is <see langword="null"/>, then the batch will not draw anything
    /// when <see cref="Render"/> is called.
    /// </param>
    public PointBatch(BasicEffect effect)
    {
      Effect = effect;
      PointSize = 5;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all points.
    /// </summary>
    public void Clear()
    {
      _points.Clear();
    }


    /// <summary>
    /// Adds a point.
    /// </summary>
    /// <param name="position">The position in world space.</param>
    /// <param name="color">The color (non-premultiplied).</param>
    public void Add(Vector3F position, Color color)
    {
      _points.Add(new Point(position, Color.FromNonPremultiplied(color.R, color.G, color.B, color.A)));
    }



    /// <summary>
    /// Draws the points.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <remarks>
    /// If <see cref="Effect"/> is <see langword="null"/>, then <see cref="Render"/> does nothing.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      if (Effect == null)
        return;

      int numberOfPoints = _points.Count;
      if (numberOfPoints <= 0)
        return;

      context.Validate(Effect);
      context.ThrowIfCameraMissing();

      // Render state.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      // Reset the texture stages. If a floating point texture is set, we get exceptions
      // when a sampler with bilinear filtering is set.
      graphicsDevice.ResetTextures();

      // Effect parameters.
      Effect.Alpha = 1;
      Effect.DiffuseColor = new Vector3(1, 1, 1);
      Effect.LightingEnabled = false;
      Effect.TextureEnabled = false;
      Effect.VertexColorEnabled = true;
      Effect.World = Matrix.Identity;
      Effect.View = Matrix.Identity;
      Effect.Projection = Matrix.Identity;
      Effect.CurrentTechnique.Passes[0].Apply();

      // Get WorldViewProjection matrix.
      Matrix view = (Matrix)context.CameraNode.View;
      Matrix projection = context.CameraNode.Camera.Projection;
      Matrix wvp = Matrix.Multiply(view, projection);

      // The x and y point size relative to the viewport.
      Viewport viewport = graphicsDevice.Viewport;
      float sizeX = PointSize / viewport.Width;
      float sizeY = sizeX * viewport.Width / viewport.Height;

      // Resize buffer if necessary.
      ResizeBuffer(graphicsDevice, numberOfPoints);

      // Submit points. The loop is only needed if we have more points than can 
      // be submitted with one draw call.
      var startPointIndex = 0;
      while (startPointIndex < numberOfPoints)
      {
        // Number of points in this batch.
        int pointsPerBatch = Math.Min(numberOfPoints - startPointIndex, _buffer.Length / 6);

        // Create vertices for points. All positions are directly in clip space!
        for (int i = 0; i < pointsPerBatch; i++)
        {
          var point = _points[startPointIndex + i];

          // Transform point position to clip space.
          Vector3 positionWorld = (Vector3)point.Position;
          Vector3 positionClip;
          Vector3.Transform(ref positionWorld, ref wvp, out positionClip);
          float w = (float)((double)positionWorld.X * wvp.M14 + (double)positionWorld.Y * wvp.M24 + (double)positionWorld.Z * wvp.M34 + wvp.M44);

          // Homogeneous divide.
          positionClip /= w;

          // 2 triangles create a point quad. Clip space goes from -1 to 1, therefore 
          // we do not need to divide sizeX and sizeY by 2.
          Vector3 bottomLeft = positionClip + new Vector3(-sizeX, +sizeY, 0);
          Vector3 bottomRight = positionClip + new Vector3(+sizeX, +sizeY, 0);
          Vector3 topLeft = positionClip + new Vector3(-sizeX, -sizeY, 0);
          Vector3 topRight = positionClip + new Vector3(+sizeX, -sizeY, 0);
          _buffer[i * 6 + 0].Position = bottomLeft;
          _buffer[i * 6 + 0].Color = point.Color;
          _buffer[i * 6 + 1].Position = bottomRight;
          _buffer[i * 6 + 1].Color = point.Color;
          _buffer[i * 6 + 2].Position = topLeft;
          _buffer[i * 6 + 2].Color = point.Color;
          _buffer[i * 6 + 3].Position = bottomRight;
          _buffer[i * 6 + 3].Color = point.Color;
          _buffer[i * 6 + 4].Position = topRight;
          _buffer[i * 6 + 4].Color = point.Color;
          _buffer[i * 6 + 5].Position = topLeft;
          _buffer[i * 6 + 5].Color = point.Color;
        }

        // Draw triangles.
        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _buffer, 0, pointsPerBatch * 2);

        startPointIndex += pointsPerBatch;
      }

      savedRenderState.Restore();
    }


    private void ResizeBuffer(GraphicsDevice graphicsDevice, int numberOfPoints)
    {
      int requiredBufferLength = numberOfPoints * 6;
      if (_buffer.Length >= requiredBufferLength)
        return;

      // The current buffer length.
      int bufferLength = _buffer.Length;

      // The maximal buffer length (3 vertices per triangle primitive).
      var maxPrimitivesPerCall = graphicsDevice.GetMaxPrimitivesPerCall();
      var maxBufferLength = maxPrimitivesPerCall * 3;

      // The desired buffer length for the points (6 vertices per point).
      int desiredBufferLength = Math.Min(requiredBufferLength, maxBufferLength);

      if (bufferLength < desiredBufferLength)
      {
        // Buffer needs to be resized.
        while (bufferLength < desiredBufferLength)
          bufferLength *= 2;

        if (bufferLength > maxBufferLength)
          bufferLength = maxBufferLength;

        // No need to copy buffer content because the content is always created from scratch in 
        // Render().

        _buffer = new VertexPositionColor[bufferLength];
      }
    }
    #endregion
  }
}
