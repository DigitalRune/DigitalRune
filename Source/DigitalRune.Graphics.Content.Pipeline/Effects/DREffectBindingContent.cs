// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for an <strong>EffectBinding</strong> asset.
  /// </summary>  
  public class DREffectBindingContent : MaterialContent
  {
    // This class stores one new property (<see cref="EffectType"/>). The other 
    // properties access data in the opaque data or in the texture dictionary.

    /// <summary>
    /// Gets or sets the type of the effect.
    /// </summary>
    /// <value>The type of the effect.</value>
    public DREffectType EffectType { get; set; }


    /// <summary>
    /// Gets or sets the current environment map texture.
    /// </summary>
    /// <value>The current environment map texture.</value>
    [ContentSerializerIgnore]
    internal ExternalReference<TextureContent> EnvironmentMap
    {
      get { return GetTexture("EnvironmentMap"); }
      set { SetTexture("EnvironmentMap", value); }
    }


    /// <summary>
    /// Gets or sets the current texture.
    /// </summary>
    /// <value>The current texture.</value>
    [ContentSerializerIgnore]
    internal ExternalReference<TextureContent> Texture
    {
      get { return GetTexture("Texture"); }
      set { SetTexture("Texture", value); }
    }


    /// <summary>
    /// Gets or sets the current overlay texture.
    /// </summary>
    /// <value>The current overlay texture.</value>
    [ContentSerializerIgnore]
    internal ExternalReference<TextureContent> Texture2
    {
      get { return GetTexture("Texture2"); }
      set { SetTexture("Texture2", value); }
    }


    /// <summary>
    /// Gets or sets the imported effect.
    /// </summary>
    /// <value>The imported effect.</value>
    [ContentSerializerIgnore]
    internal ExternalReference<EffectContent> Effect
    {
      get { return GetReferenceTypeProperty<ExternalReference<EffectContent>>("Effect"); }
      set { SetProperty("Effect", value); }
    }


    /// <summary>
    /// Gets or sets the processed effect.
    /// </summary>
    /// <value>The processed effect.</value>
    [ContentSerializerIgnore]
    internal ExternalReference<CompiledEffectContent> CompiledEffect
    {
      get { return GetReferenceTypeProperty<ExternalReference<CompiledEffectContent>>("CompiledEffect"); }
      set { SetProperty("CompiledEffect", value); }
    }


    /// <summary>
    /// Gets or sets the name of the effect, if an external asset is referenced.
    /// </summary>
    /// <value>The name of the effect, if an external asset is referenced.</value>
    [ContentSerializerIgnore]
    internal string EffectAsset
    {
      get { return GetReferenceTypeProperty<string>("EffectAsset"); }
      set { SetProperty("EffectAsset", value); }
    }
  }
}
