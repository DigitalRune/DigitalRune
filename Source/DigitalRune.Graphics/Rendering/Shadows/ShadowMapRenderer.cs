// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders the shadow maps of <see cref="LightNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="ShadowMapRenderer"/> handles <see cref="StandardShadow"/>s, 
  /// <see cref="CubeMapShadow"/>s, <see cref="CascadedShadow"/>s, and
  /// <see cref="CompositeShadow"/>s. Support for new shadow types can be implemented by creating a
  /// custom <see cref="SceneNodeRenderer"/> and adding it to the
  /// <see cref="SceneRenderer.Renderers"/> collection.
  /// </para>
  /// <para>
  /// During rendering the renderer changes the render context: Depending on the shadow type
  /// <c>"Default"</c>, <c>"Omnidirectional"</c>, or <c>"Directional"</c> is set as the
  /// <see cref="RenderContext.Technique"/>.
  /// </para>
  /// <para>
  /// The shadow maps are stored in the <see cref="LightNode.Shadow"/> property of the
  /// <see cref="LightNode"/>.
  /// </para>
  /// <para>
  /// <strong>Render Callback:</strong><br/>
  /// The shadow map renderer requires a <see cref="RenderCallback"/> method to render the scene.
  /// The callback method needs to render the scene using the camera and the information given in
  /// the <see cref="RenderContext"/>.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the
  /// graphics device to render the shadow maps. The render target and the viewport of the graphics
  /// device are undefined after rendering.
  /// </para>
  /// </remarks>
  public class ShadowMapRenderer : SceneRenderer, IShadowMapRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public Func<RenderContext, bool> RenderCallback
    {
      get { return _renderCallback; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _renderCallback = value;
        foreach (var renderer in Renderers)
        {
          var shadowMapRenderer = renderer as IShadowMapRenderer;
          if (shadowMapRenderer != null)
            shadowMapRenderer.RenderCallback = value;
        }
      }
    }
    private Func<RenderContext, bool> _renderCallback;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMapRenderer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMapRenderer"/> class using the specified
    /// render callback.
    /// </summary>
    /// <param name="render">
    /// The method which renders the scene into the shadow map. Must not be <see langword="null"/>.
    /// See <see cref="RenderCallback"/> for more information.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="render"/> is <see langword="null"/>.
    /// </exception>
    public ShadowMapRenderer(Func<RenderContext, bool> render)
    {
      if (render == null)
        throw new ArgumentNullException("render");

      _renderCallback = render;
      AddDefaultRenderers(render);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowMapRenderer"/> class using the specified
    /// scene node renderer.
    /// </summary>
    /// <param name="sceneNodeRenderer">
    /// The renderer for shadow-casting objects. A <see cref="RenderCallback"/> is created
    /// automatically which calls the specified renderer.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNodeRenderer"/> is <see langword="null"/>.
    /// </exception>
    public ShadowMapRenderer(SceneNodeRenderer sceneNodeRenderer)
    {
      if (sceneNodeRenderer == null)
        throw new ArgumentNullException("sceneNodeRenderer");

      _renderCallback = context =>
                        {
                          var query = context.Scene.Query<ShadowCasterQuery>(context.CameraNode, context);
                          if (query.ShadowCasters.Count == 0)
                            return false;

                          sceneNodeRenderer.Render(query.ShadowCasters, context);
                          return true;
                        };

      AddDefaultRenderers(_renderCallback);
    }


    private void AddDefaultRenderers(Func<RenderContext, bool> renderCallback)
    {
      Renderers.Add(new StandardShadowMapRenderer(renderCallback));
      Renderers.Add(new CubeMapShadowMapRenderer(renderCallback));
      Renderers.Add(new CascadedShadowMapRenderer(renderCallback));
      Renderers.Add(new CompositeShadowMapRenderer(Renderers));
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


    /// <inheritdoc/>
    internal override bool FilterSceneNode(SceneNode node)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow != null;
    }
    #endregion
  }
}
