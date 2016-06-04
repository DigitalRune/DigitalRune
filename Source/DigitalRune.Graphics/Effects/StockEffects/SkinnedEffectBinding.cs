// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the logic for the <see cref="SkinnedEffect"/>.
  /// </summary>
  public class SkinnedEffectBinding : EffectBinding, IStockEffectBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    bool IStockEffectBinding.FogEnabled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether per-pixel lighting should be used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if to use per-pixel lighting; otherwise, <see langword="false"/> to
    /// use per-vertex lighting. The default value is <see langword="false"/>.
    /// </value>
    public bool PreferPerPixelLighting { get; set; }


    /// <summary>
    /// Gets or sets the max number of bone weights per vertex.
    /// </summary>
    /// <value>The max number of bone weights per vertex.</value>
    public int WeightsPerVertex { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SkinnedEffectBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SkinnedEffectBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected SkinnedEffectBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SkinnedEffectBinding"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    public SkinnedEffectBinding(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
      : base(graphicsService, graphicsService.GetSkinnedEffect(), opaqueData)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override EffectBinding CreateInstanceCore()
    {
      return new SkinnedEffectBinding();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectBinding source)
    {
      // Clone EffectBinding properties.
      base.CloneCore(source);

      // Clone SkinnedEffectBinding properties.
      var sourceTyped = (SkinnedEffectBinding)source;
      PreferPerPixelLighting = sourceTyped.PreferPerPixelLighting;
      WeightsPerVertex = sourceTyped.WeightsPerVertex;
    }


    /// <inheritdoc/>
    protected override void OnInitializeBindings(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
    {
      float alpha = GetOpaqueDatum(opaqueData, "Alpha", 1.0f);
      Vector3 diffuse = GetOpaqueDatum(opaqueData, "DiffuseColor", new Vector3(1, 1, 1));
      ParameterBindings.Add(
        new ConstParameterBinding<Vector4>(
          Effect,
          Effect.Parameters["DiffuseColor"],
          new Vector4(diffuse.X * alpha, diffuse.Y * alpha, diffuse.Z * alpha, alpha)));  // Pre-multiplied alpha.

      ParameterBindings.Add(
        new ConstParameterBinding<Vector3>(
          Effect,
          Effect.Parameters["EmissiveColor"],
          GetOpaqueDatum(opaqueData, "EmissiveColor", new Vector3(0, 0, 0))));

      ParameterBindings.Add(
        new ConstParameterBinding<Vector3>(
          Effect,
          Effect.Parameters["SpecularColor"],
          GetOpaqueDatum(opaqueData, "SpecularColor", new Vector3(1, 1, 1))));

      ParameterBindings.Add(
        new ConstParameterBinding<float>(
          Effect,
          Effect.Parameters["SpecularPower"],
          GetOpaqueDatum(opaqueData, "SpecularPower", 16.0f)));

      ParameterBindings.Add(
        new ConstParameterBinding<Texture>(
          Effect,
          Effect.Parameters["Texture"],
          GetOpaqueDatum(opaqueData, "Texture", (Texture)null)));

      WeightsPerVertex = GetOpaqueDatum(opaqueData, "WeightsPerVertex", 4);

      base.OnInitializeBindings(graphicsService, opaqueData);
    }


    private static T GetOpaqueDatum<T>(IDictionary<string, object> opaqueData, string key, T defaultValue)
    {
      object datum;
      if (opaqueData == null || !opaqueData.TryGetValue(key, out datum) || !(datum is T))
        return defaultValue;

      return (T)datum;
    }
    #endregion
  }
}
