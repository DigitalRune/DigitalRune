// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="SkyNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SkyRenderer"/> handles <see cref="CloudLayerNode"/>s, 
  /// <see cref="GradientSkyNode"/>s, <see cref="GradientTextureSkyNode"/>s, 
  /// <see cref="ScatteringSkyNode"/>s, <see cref="SkyboxNode"/>s, <see cref="SkyObjectNode"/>s, and
  /// <see cref="StarfieldNode"/>s.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class SkyRenderer : SceneRenderer
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
    /// Initializes a new instance of the <see cref="SkyRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SkyRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
      {
        Renderers.Add(new SkyboxRendererInternal(graphicsService));
      }
      else
      {
        Renderers.Add(new SkyboxRendererInternal(graphicsService));
        Renderers.Add(new StarfieldRenderer(graphicsService));
        Renderers.Add(new SkyObjectRenderer(graphicsService));
        Renderers.Add(new GradientSkyRenderer(graphicsService));
        Renderers.Add(new GradientTextureSkyRenderer(graphicsService));
        Renderers.Add(new ScatteringSkyRenderer(graphicsService));
        Renderers.Add(new CloudLayerRenderer(graphicsService));
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return (node is SkyNode) && base.CanRender(node, context);
    }


    // Same as SceneRenderer.BatchJobs() except we sort by SkyNode.DrawOrder instead of distance.
    internal override void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Assign temporary IDs to scene node renderers.
      for (int i = 0; i < Renderers.Count; i++)
        Renderers[i].Id = (uint)(i & 0xff);  // ID = index clamped to [0, 255].

      // Add draw jobs.
      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as SkyNode;
        if (node == null)
          continue;

        var job = new Job();
        job.Node = node;
        foreach (var renderer in Renderers)
        {
          if (renderer.CanRender(node, context))
          {
            job.Renderer = renderer;
            break;
          }
        }

        if (job.Renderer == null)
          continue;

        job.SortKey = GetSortKey(node.DrawOrder, job.Renderer.Order, job.Renderer.Id);
        Jobs.Add(ref job);
      }

      if (order != RenderOrder.UserDefined)
      {
        // Sort draw jobs.
        Jobs.Sort(Comparer.Instance);
      }
    }


    /// <summary>
    /// Gets the sort key.
    /// </summary>
    /// <param name="drawOrder">The draw order.</param>
    /// <param name="order">The order of the renderer.</param>
    /// <param name="id">The ID of the renderer.</param>
    /// <returns>The key for sorting draw jobs.</returns>
    private static uint GetSortKey(int drawOrder, int order, uint id)
    {
      Debug.Assert(0 <= drawOrder && drawOrder <= ushort.MaxValue, "Draw order is out of range.");
      Debug.Assert(0 <= order && order <= byte.MaxValue, "Order is out of range.");
      Debug.Assert(id <= byte.MaxValue, "ID is out of range.");

      // -------------------------------------
      // |   draw order  |  order  |  ID     |
      // |   16 bit      |  8 bit  |  8 bit  |
      // -------------------------------------

      return (uint)drawOrder << 16
             | (uint)order << 8
             | id;
    }
    #endregion
  }
}
#endif
