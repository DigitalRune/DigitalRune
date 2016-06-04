// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads an <see cref="EffectBinding"/> from binary format.
  /// </summary>
  public class EffectBindingReader : ContentTypeReader<EffectBinding>
  {
    // IMPORTANT: This enumeration must be kept in sync with the corresponding 
    // enumeration in the content pipeline project.
    private enum EffectType
    {
      AlphaTestEffect,
      BasicEffect,
      DualTextureEffect,
      EnvironmentMapEffect,
      SkinnedEffect,
      CustomEffect,
    }


    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override EffectBinding Read(ContentReader input, EffectBinding existingInstance)
    {
      if (input == null)
        throw new ArgumentNullException("input");
      if (existingInstance != null)
        throw new NotSupportedException("Reading an EffectBinding into an existing object is not yet supported.");

      // Get graphics service from service provider.
      var graphicsService = (IGraphicsService)input.ContentManager.ServiceProvider.GetService(typeof(IGraphicsService));
      if (graphicsService == null)
        throw new ContentLoadException("Could not find graphics service. Please make sure that ContentManager.ServiceProvider contains the IGraphicsService.");

      var effectType = (EffectType)input.ReadInt32();

      Effect effect = null;
      if (effectType == EffectType.CustomEffect)
      {
        bool isPrebuiltAsset = input.ReadBoolean();
        if (isPrebuiltAsset)
        {
          // The effect is a prebuilt asset. Use content manager to resolve asset.
          effect = input.ContentManager.Load<Effect>(input.ReadString());
        }
        else
        {
          // The effect is built as part of the material.
          effect = input.ReadExternalReference<Effect>();
        }
      }

      var opaqueData = input.ReadObject<Dictionary<string, object>>();

      switch (effectType)
      {
        case EffectType.AlphaTestEffect:
          return new AlphaTestEffectBinding(graphicsService, opaqueData);
        case EffectType.BasicEffect:
          return new BasicEffectBinding(graphicsService, opaqueData);
        case EffectType.DualTextureEffect:
          return new DualTextureEffectBinding(graphicsService, opaqueData);
        case EffectType.EnvironmentMapEffect:
          return new EnvironmentMapEffectBinding(graphicsService, opaqueData);
        case EffectType.SkinnedEffect:
          return new SkinnedEffectBinding(graphicsService, opaqueData);
        default:
          return new EffectBinding(graphicsService, effect, opaqueData);
      }
    }
  }
}
