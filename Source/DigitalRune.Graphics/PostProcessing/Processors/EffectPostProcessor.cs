// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Performs post-processing using a custom <see cref="Effect"/> and automatically bound effect 
  /// parameters.
  /// </summary>
  /// <remarks>
  /// This post processor takes an <see cref="EffectBinding"/> and uses automatically generated 
  /// effect parameter bindings to set the effect parameters. All passes of the first technique in 
  /// the effect are executed. (Most effects will use only one pass. If the effect defines several 
  /// passes, then all passes are executed using the same source image and all render into the same 
  /// render target - so multiple passes only make sense if a form of alpha blending is configured 
  /// in the effect.)
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
  public class EffectPostProcessor : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the effect binding.
    /// </summary>
    /// <value>The effect binding.</value>
    public EffectBinding EffectBinding { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectPostProcessor"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public EffectPostProcessor(IGraphicsService graphicsService, Effect effect)
      : base(graphicsService)
    {
      EffectBinding = new EffectBinding(graphicsService, effect);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      // Set the render target - but only if no kind of alpha blending is currently set.
      // If alpha-blending is set, then we have to assume that the render target is already
      // set - everything else does not make sense.
      if (graphicsDevice.BlendState.ColorDestinationBlend == Blend.Zero
          && graphicsDevice.BlendState.AlphaDestinationBlend == Blend.Zero)
      {
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        graphicsDevice.Viewport = context.Viewport;
      }

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      // Update and apply all parameter bindings, except per-pass bindings.
      foreach (var parameterBinding in EffectBinding.ParameterBindings)
      {
        if (parameterBinding.Description.Hint != EffectParameterHint.PerPass)
        {
          parameterBinding.Update(context);
          parameterBinding.Apply(context);
        }
      }

      var effect = EffectBinding.Effect;

      // Select technique.
      var techniqueBinding = EffectBinding.TechniqueBinding;
      techniqueBinding.Update(context);
      var technique = techniqueBinding.GetTechnique(effect, context);
      effect.CurrentTechnique = technique;

      var passBinding = techniqueBinding.GetPassBinding(technique, context);
      foreach (var pass in passBinding)
      {
        // Update and apply per-pass bindings.
        foreach (var parameterBinding in EffectBinding.ParameterBindings)
        {
          if (parameterBinding.Description.Hint == EffectParameterHint.PerPass)
          {
            parameterBinding.Update(context);
            parameterBinding.Apply(context);
          }
        }

        pass.Apply();
        graphicsDevice.DrawFullScreenQuad();
      }
    }
    #endregion
  }
}
#endif
