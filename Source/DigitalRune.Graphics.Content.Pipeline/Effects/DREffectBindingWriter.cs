// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Writes an <see cref="DREffectBindingContent"/> to a binary format that can be read by the 
  /// <strong>EffectBindingReader</strong> to load an <strong>EffectBinding</strong>.
  /// </summary>
  [ContentTypeWriter]
  public class DREffectBindingWriter : ContentTypeWriter<DREffectBindingContent>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Effects.EffectBinding, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return "DigitalRune.Graphics.Content.EffectBindingReader, DigitalRune.Graphics, Version=1.2.0.0";
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void Write(ContentWriter output, DREffectBindingContent value)
    {
      output.Write((int)value.EffectType);

      if (value.EffectType == DREffectType.CustomEffect)
      {
        var compiledEffect = value.CompiledEffect;
        var effectAsset = value.EffectAsset;
        if (!string.IsNullOrWhiteSpace(effectAsset))
        {
          // The effect is a prebuilt asset.
          output.Write(true);
          output.Write(effectAsset);
        }
        else if (compiledEffect != null)
        {
          // The effect is built as part of the material.
          output.Write(false);
          output.WriteExternalReference(compiledEffect);
        }
        else
        {
          throw new InvalidContentException("Cannot write EffectBinding. The CompiledEffect and EffectAsset properties are null.", value.Identity);
        }
      }

      // Copy all data (opaque data, textures) into a dictionary and write the dictionary 
      // to binary format.
      var dictionary = new Dictionary<string, object>();

      foreach (var data in value.OpaqueData)
      {
        if ((data.Key == "Effect") || (data.Key == "CompiledEffect"))
        {
          // "Effect" and "CompiledEffect" have already been handled.
          continue;
        }

        dictionary.Add(data.Key, data.Value);
      }

      foreach (var texture in value.Textures)
        dictionary.Add(texture.Key, texture.Value);

      output.WriteObject(dictionary);
    }
  }
}
