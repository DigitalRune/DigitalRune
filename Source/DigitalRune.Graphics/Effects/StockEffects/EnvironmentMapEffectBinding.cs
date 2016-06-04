// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the logic for the <see cref="EnvironmentMapEffect"/>.
  /// </summary>
  public class EnvironmentMapEffectBinding : EffectBinding, IStockEffectBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    bool IStockEffectBinding.FogEnabled { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentMapEffectBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentMapEffectBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected EnvironmentMapEffectBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentMapEffectBinding"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    public EnvironmentMapEffectBinding(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
      : base(graphicsService, graphicsService.GetEnvironmentMapEffect(), opaqueData)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override EffectBinding CreateInstanceCore()
    {
      return new EnvironmentMapEffectBinding();
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
          Effect.Parameters["EnvironmentMapSpecular"],
          GetOpaqueDatum(opaqueData, "EnvironmentMapSpecular", new Vector3(0, 0, 0))));

      ParameterBindings.Add(
        new ConstParameterBinding<float>(
          Effect,
          Effect.Parameters["EnvironmentMapAmount"],
          GetOpaqueDatum(opaqueData, "EnvironmentMapAmount", 1.0f)));

      ParameterBindings.Add(
        new ConstParameterBinding<float>(
          Effect,
          Effect.Parameters["FresnelFactor"],
          GetOpaqueDatum(opaqueData, "FresnelFactor", 1.0f)));

      var texture = GetOpaqueDatum(opaqueData, "Texture", (Texture)null);
      ParameterBindings.Add(
        new ConstParameterBinding<Texture>(
          Effect,
          Effect.Parameters["Texture"],
          texture));

      ParameterBindings.Add(
        new ConstParameterBinding<TextureCube>(
          Effect,
          Effect.Parameters["EnvironmentMap"],
          GetOpaqueDatum(opaqueData, "EnvironmentMap", (TextureCube)null)));

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
