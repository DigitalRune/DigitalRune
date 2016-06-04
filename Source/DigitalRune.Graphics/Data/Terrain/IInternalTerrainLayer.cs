// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a layer to be rendered into the terrain clipmap.
  /// </summary>
  /// <remarks>
  /// Every object that needs to draw into the clipmap needs to be implement
  /// <see cref="IInternalTerrainLayer"/>.
  /// </remarks>
  internal interface IInternalTerrainLayer
  {
    /// <inheritdoc cref="TerrainLayer.Aabb"/>
    Aabb? Aabb { get; }

    /// <inheritdoc cref="TerrainLayer.FadeInStart"/>
    int FadeInStart { get; }

    /// <inheritdoc cref="TerrainLayer.FadeOutEnd"/>
    int FadeOutEnd { get; }

    /// <inheritdoc cref="TerrainLayer.Material"/>
    Material Material { get; }

    /// <inheritdoc cref="TerrainLayer.MaterialInstance"/>
    MaterialInstance MaterialInstance { get; }

    /// <inheritdoc cref="TerrainLayer.OnDraw(GraphicsDevice,Rectangle,Vector2F,Vector2F)"/>
    void OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition);
  }
}
