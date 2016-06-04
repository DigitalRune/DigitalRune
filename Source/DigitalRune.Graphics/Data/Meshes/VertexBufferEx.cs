// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides additional data for a <see cref="VertexBuffer"/>.
  /// </summary>
  internal sealed class VertexBufferEx : GraphicsResourceEx<VertexBuffer>
  {
    /// <summary>Counts the submeshes for this vertex buffer during rendering.</summary>
    internal uint SubmeshCount;


    /// <summary>
    /// Gets the <see cref="VertexBufferEx"/> for the specified <see cref="VertexBuffer"/>.
    /// </summary>
    /// <param name="vertexBuffer">The vertexBuffer.</param>
    /// <returns>The <see cref="VertexBufferEx"/>.</returns>
    public static VertexBufferEx From(VertexBuffer vertexBuffer)
    {
      return GetOrCreate<VertexBufferEx>(vertexBuffer, null);
    }
  }
}
