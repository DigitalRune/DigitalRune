// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="LightNode"/>s into the light buffer.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="LightRenderer"/> handles <see cref="AmbientLight"/>s, 
  /// <see cref="DirectionalLight"/>s, <see cref="PointLight"/>s, <see cref="ProjectorLight"/>s,
  /// and <see cref="Spotlight"/>s.
  /// </para>
  /// <para>
  /// <see cref="RenderContext.GBuffer0"/>, and <see cref="RenderContext.GBuffer1"/> need to be set
  /// in the render context. 
  /// </para>
  /// <para>
  /// <strong>Render Targets and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device. The
  /// render target should be the light buffer.
  /// </para>
  /// </remarks>
  public class LightRenderer : SceneRenderer
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
    /// Initializes a new instance of the <see cref="LightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public LightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Renderers.Add(new AmbientLightRenderer(graphicsService));
      Renderers.Add(new ImageBasedLightRenderer(graphicsService));
      Renderers.Add(new DirectionalLightRenderer(graphicsService));
      Renderers.Add(new PointLightRenderer(graphicsService));
      Renderers.Add(new ProjectorLightRenderer(graphicsService));
      Renderers.Add(new SpotlightRenderer(graphicsService));
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return FilterSceneNode(node) && base.CanRender(node, context);
    }


    internal override bool FilterSceneNode(SceneNode node)
    {
      return (node is LightNode);
    }
    #endregion
  }
}
#endif
