// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow maps of a <see cref="CompositeShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMapRenderer"/>
  internal class CompositeShadowMapRenderer : SceneNodeRenderer
  {
    // Notes:
    // IShadowMapRenderer is not needed when used internally. But in case the class
    // is made public the interface should be implemented similar to ShadowMapRenderer.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IList<SceneNodeRenderer> _shadowMapRenderers;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShadowMapRenderer" /> class.
    /// </summary>
    /// <param name="shadowMapRenderers">A list of all known shadow map renderers.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shadowMapRenderers"/> is <see langword="null"/>.
    /// </exception>
    public CompositeShadowMapRenderer(IList<SceneNodeRenderer> shadowMapRenderers)
    {
      if (shadowMapRenderers == null)
        throw new ArgumentNullException("shadowMapRenderers");

      _shadowMapRenderers = shadowMapRenderers;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow is CompositeShadow;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as CompositeShadow;
        if (shadow == null)
          continue;

        // Set LightNode.Shadow to child shadow temporarily and call a suitable renderer.
        for (int j = 0; j < shadow.Shadows.Count; j++)
        {
          lightNode.Shadow = shadow.Shadows[j];

          int numberOfRenderers = _shadowMapRenderers.Count;
          for (int k = 0; k < numberOfRenderers; k++)
          {
            var renderer = _shadowMapRenderers[k];
            if (renderer.CanRender(lightNode, context))
            {
              renderer.Render(lightNode, context);
              break;
            }
          }
        }

        lightNode.Shadow = shadow;
      }
    }
    #endregion
  }
}
