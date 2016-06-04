// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


// Concept:
//   Scene nodes can implement the interface IRenderable:
//     interface IRenderable { void Render(RenderContext context); }
//   The RenderableSceneRenderer renders all scene nodes that implement the interface.
// 
//   + Useful for prototyping prototyping.
//   - Not recommended for actual game (overhead, no batching, ...).
//   - Additional API might confuse users.


/*
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders scene nodes which implement <see cref="IRenderable"/>.
  /// </summary>
  /// <remarks>
  /// This renderer checks if a <see cref="SceneNode"/> implements <see cref="IRenderable"/>
  /// and if it does it calls <see cref="IRenderable.Render"/> to let the scene node render
  /// itself.
  /// </remarks>
  public class RenderableSceneNodeRenderer : SceneNodeRenderer
  {
    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is IRenderable;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      throw new System.NotImplementedException();
    }
  }
}
*/