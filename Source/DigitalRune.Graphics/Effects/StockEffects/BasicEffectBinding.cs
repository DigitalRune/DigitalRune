// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the logic for the <see cref="BasicEffect"/>.
  /// </summary>
  public class BasicEffectBinding : EffectBinding, IStockEffectBinding
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
    /// Gets or sets a value indicating whether lighting is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if lighting is enabled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    public bool LightingEnabled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether per-pixel lighting should be used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if to use per-pixel lighting; otherwise, <see langword="false"/> to
    /// use per-vertex lighting. The default value is <see langword="false"/>
    /// </value>
    public bool PreferPerPixelLighting { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether texturing is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if texturing is enabled; otherwise, <see langword="false"/>.
    /// </value>
    public bool TextureEnabled { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether vertex color is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if vertex color is enabled; otherwise, <see langword="false"/>.
    /// </value>
    public bool VertexColorEnabled { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicEffectBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="BasicEffectBinding"/> class. (This constructor
    /// creates an uninitialized instance. Use this constructor only for cloning or other special
    /// cases!)
    /// </summary>
    protected BasicEffectBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BasicEffectBinding"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    public BasicEffectBinding(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
      : base(graphicsService, graphicsService.GetBasicEffect(), opaqueData)
    {
      LightingEnabled = true;

      // BasicEffect.PreferPerPixelLighting is true by default. But per-pixel 
      // lighting kill performance on WP7. --> Change the default to false.
      PreferPerPixelLighting = false;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override EffectBinding CreateInstanceCore()
    {
      return new BasicEffectBinding();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectBinding source)
    {
      // Clone EffectBinding properties.
      base.CloneCore(source);

      // Clone BasicEffectBinding properties.
      var sourceTyped = (BasicEffectBinding)source;
      LightingEnabled = sourceTyped.LightingEnabled;
      PreferPerPixelLighting = sourceTyped.PreferPerPixelLighting;
      TextureEnabled = sourceTyped.TextureEnabled;
      VertexColorEnabled = sourceTyped.VertexColorEnabled;
    }


    /// <inheritdoc/>
    protected override void OnInitializeBindings(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
    {
      // We need a special binding for DiffuseColor because when lighting is enabled, the 
      // DiffuseColor parameter should contain diffuse + emissive!
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

      var texture = GetOpaqueDatum(opaqueData, "Texture", (Texture)null);
      ParameterBindings.Add(
        new ConstParameterBinding<Texture>(
          Effect,
          Effect.Parameters["Texture"],
          texture));

      TextureEnabled = (texture != null);
      VertexColorEnabled = GetOpaqueDatum(opaqueData, "VertexColorEnabled", false);

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
