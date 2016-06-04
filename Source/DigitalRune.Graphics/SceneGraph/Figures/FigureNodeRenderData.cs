// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Caches vertex buffer and index buffer for the figure.
  /// </summary>
  /// <remarks>
  /// <para>
  /// It makes sense to cache a vertex buffer if a figure node is static 
  /// (<see cref="SceneNode.IsStatic"/>). The figure should also contain a lot of segments. In such
  /// a case, it is probable that the figure is not shared. We can store the pre-transformed data
  /// in a vertex buffer (e.g. world space positions, colors, dash pattern distances, etc.).
  /// </para>
  /// <para>
  /// <see cref="Dispose"/> can be called to reset the render data. The data will be automatically
  /// recreated by the figure renderer when needed.
  /// </para>
  /// </remarks>
  internal sealed class FigureNodeRenderData : IDisposable
  {
    public bool IsValid;

    public VertexBuffer FillVertexBuffer;
    public IndexBuffer FillIndexBuffer;
    public VertexBuffer StrokeVertexBuffer;
    public IndexBuffer StrokeIndexBuffer;

    public void Dispose()
    {
      IsValid = false;
      FillVertexBuffer.SafeDispose();
      FillIndexBuffer.SafeDispose();
      StrokeVertexBuffer.SafeDispose();
      StrokeIndexBuffer.SafeDispose();
    }
  }
}
