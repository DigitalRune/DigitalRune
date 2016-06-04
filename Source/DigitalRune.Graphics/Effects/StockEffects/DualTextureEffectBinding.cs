// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the logic for the <see cref="DualTextureEffect"/>.
  /// </summary>
  public class DualTextureEffectBinding : EffectBinding, IStockEffectBinding
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
    /// Initializes a new instance of the <see cref="DualTextureEffectBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DualTextureEffectBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected DualTextureEffectBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DualTextureEffectBinding"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    public DualTextureEffectBinding(IGraphicsService graphicsService, IDictionary<string, object> opaqueData)
      : base(graphicsService, graphicsService.GetDualTextureEffect(), opaqueData)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override EffectBinding CreateInstanceCore()
    {
      return new DualTextureEffectBinding();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectBinding source)
    {
      // Clone EffectBinding properties.
      base.CloneCore(source);

      // Clone DualTextureEffectBinding properties.
      var sourceTyped = (DualTextureEffectBinding)source;
      VertexColorEnabled = sourceTyped.VertexColorEnabled;
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
        new ConstParameterBinding<Texture>(
          Effect,
          Effect.Parameters["Texture"],
          GetOpaqueDatum(opaqueData, "Texture", (Texture)null)));

      ParameterBindings.Add(
        new ConstParameterBinding<Texture>(
          Effect,
          Effect.Parameters["Texture2"],
          GetOpaqueDatum(opaqueData, "Texture2", (Texture)null)));

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
