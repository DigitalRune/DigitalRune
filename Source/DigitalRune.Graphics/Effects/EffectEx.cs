// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides additional data for an <see cref="Effect"/>.
  /// </summary>
  internal sealed class EffectEx : GraphicsResourceEx<Effect>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>Counts the bindings for this effect during rendering.</summary>
    internal uint BindingCount;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the original effect parameter values as specified in the .fx file.
    /// </summary>
    /// <value>The original effect parameter values as specified in the .fx file.</value>
    public Dictionary<EffectParameter, object> OriginalParameterValues { get; private set; }


    /// <summary>
    /// Gets the descriptions of the effect techniques.
    /// </summary>
    /// <value>
    /// The descriptions of the effect techniques.
    /// </value>
    public EffectTechniqueDescriptionCollection TechniqueDescriptions { get; private set; }


    /// <summary>
    /// Gets the effect technique binding.
    /// </summary>
    /// <value>The effect technique binding.</value>
    public EffectTechniqueBinding TechniqueBinding { get; private set; }


    /// <summary>
    /// Gets the descriptions of the effect parameters.
    /// </summary>
    /// <value>The descriptions of the effect parameters.</value>
    public EffectParameterDescriptionCollection ParameterDescriptions { get; private set; }


    /// <summary>
    /// Gets the effect parameter bindings.
    /// </summary>
    /// <value>The effect parameter bindings.</value>
    public EffectParameterBindingCollection ParameterBindings { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="EffectEx"/> for the specified <see cref="Effect"/>.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The <see cref="EffectEx"/>.</returns>
    public static EffectEx From(Effect effect, IGraphicsService graphicsService)
    {
      return GetOrCreate<EffectEx>(effect, graphicsService);
    }


    /// <summary>
    /// Initializes the <see cref="EffectEx"/>.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    protected override void Initialize(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      var effect = Resource;

      // When an Effect is used the original values of the effect parameters, as 
      // specified in the .fx file, are lost. --> Store values in dictionary.
      OriginalParameterValues = EffectHelper.GetParameterValues(effect);

      TechniqueDescriptions = new EffectTechniqueDescriptionCollection(graphicsService, effect);
      TechniqueBinding = EffectHelper.CreateTechniqueBinding(graphicsService, effect);

      ParameterDescriptions = new EffectParameterDescriptionCollection(graphicsService, effect);
      ParameterBindings = new EffectParameterBindingCollection(EffectParameterHint.Any);
      EffectHelper.InitializeParameterBindings(graphicsService, this, null, ParameterBindings);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
