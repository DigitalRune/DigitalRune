// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow mask from the shadow map of a light node with a
  /// <see cref="CompositeShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMaskRenderer"/>
  internal class CompositeShadowMaskRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // Blend states for single channels writes and min() blending.
    private BlendState[] BlendStates =
    {
      new BlendState
      {
        Name = "CompositeShadowMaskRenderer.BlendStateRedMin",
        ColorWriteChannels = ColorWriteChannels.Red,
        ColorBlendFunction = BlendFunction.Min,
        ColorDestinationBlend = Blend.One,
        ColorSourceBlend = Blend.One,
      },
      new BlendState
      {
        Name = "CompositeShadowMaskRenderer.BlendStateGreenMin",
        ColorWriteChannels = ColorWriteChannels.Green,
        ColorBlendFunction = BlendFunction.Min,
        ColorDestinationBlend = Blend.One,
        ColorSourceBlend = Blend.One,
      },
      new BlendState
      {
        Name = "CompositeShadowMaskRenderer.BlendStateBlueMin",
        ColorWriteChannels = ColorWriteChannels.Blue,
        ColorBlendFunction = BlendFunction.Min,
        ColorDestinationBlend = Blend.One,
        ColorSourceBlend = Blend.One,
      },
      new BlendState
      {
        Name = "CompositeShadowMaskRenderer.BlendStateAlphaMin",
        ColorWriteChannels = ColorWriteChannels.Alpha,
        AlphaBlendFunction = BlendFunction.Min,
        AlphaDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
      },
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IList<SceneNodeRenderer> _shadowMaskRenderers;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShadowMaskRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="shadowMaskRenderers">A list with all known shadow mask renderers.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="shadowMaskRenderers"/> is
    /// <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public CompositeShadowMaskRenderer(IGraphicsService graphicsService, IList<SceneNodeRenderer> shadowMaskRenderers)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (shadowMaskRenderers == null)
        throw new ArgumentNullException("shadowMaskRenderers");

      _shadowMaskRenderers = shadowMaskRenderers;
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the
    /// <see cref="SceneNodeRenderer"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        foreach (var blendState in BlendStates)
          blendState.Dispose();
      }

      base.Dispose(disposing);
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as CompositeShadow;
        if (shadow == null)
          continue;

        if (shadow.ShadowMask == null)
          continue;

        // Write into a single channel and use min() blending.
        graphicsDevice.BlendState = BlendStates[shadow.ShadowMaskChannel];

        for (int j = 0; j < shadow.Shadows.Count; j++)
        {
          // Temporarily set shadow mask and shadow mask channel of child shadows.
          var childShadow = shadow.Shadows[j];
          childShadow.ShadowMask = shadow.ShadowMask;
          childShadow.ShadowMaskChannel = shadow.ShadowMaskChannel;

          // Temporarily exchange LightNode.Shadow and render the child shadow.
          lightNode.Shadow = childShadow;

          for (int k = 0; k < _shadowMaskRenderers.Count; k++)
          {
            var renderer = _shadowMaskRenderers[k];
            if (renderer.CanRender(lightNode, context))
            {
              renderer.Render(lightNode, context);
              break;
            }
          }

          // Remove shadow mask references. Strictly speaking, the mask is correct
          // for the composite shadow. It is not correct for the child shadow. The child
          // shadow only contributes to the mask. Therefore, childShadowMask should not be
          // set. 
          childShadow.ShadowMask = null;
          childShadow.ShadowMaskChannel = 0;
        }

        lightNode.Shadow = shadow;
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
