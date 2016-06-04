// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Rendering
{
  internal sealed class WaterRenderData : IDisposable
  {
    // Cached normal map offsets. These are not strictly necessary if we simply sample the water
    // using tex2D(..., texCoord + time * velocity). But if we cache the normal offsets, then we
    // can smoothly change the velocity without visible "jumps".
    public Vector2F NormalMapOffset0;
    public Vector2F NormalMapOffset1;
    public int LastNormalUpdateFrame = -1;

    // A cached submesh for water rendering. Created and used by the WaterRenderer.
    public Submesh Submesh;
    public Matrix44F SubmeshMatrix;


    public void UpdateSubmesh(IGraphicsService graphicsService, WaterNode node)
    {
      if (node.Volume == null)
        return;

      // We have to update the submesh if it is null or disposed.
      //   Submesh == null                            --> Update
      //   Submesh != null && VertexBuffer.IsDisposed --> Update
      //   Submesh != null && VertexBuffer == null    --> This is the EmptyShape. No updated needed.
      if (Submesh == null || (Submesh.VertexBuffer != null && Submesh.VertexBuffer.IsDisposed))
        ShapeMeshCache.GetMesh(graphicsService, node.Volume, out Submesh, out SubmeshMatrix);
    }


    public void Dispose()
    {
      LastNormalUpdateFrame = -1;
      Submesh = null;
    }
  }
}
