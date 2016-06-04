// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the shaders of a <see cref="SkinnedEffect"/>.
  /// </summary>
  [DebuggerDisplay("SkinnedEffectTechniqueBinding(Id = {Id})")]
  internal sealed class SkinnedEffectTechniqueBinding : EffectTechniqueBinding
  {
    private EffectParameter _parameterEmissive;
#if !MONOGAME
    private EffectParameter _parameterShaderIndex;
#endif
    private Vector3 _effectiveEmissive;


    /// <inheritdoc/>
    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new SkinnedEffectTechniqueBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnUpdate(RenderContext context)
    {
      var effectBinding = context.MaterialBinding as SkinnedEffectBinding;
      if (effectBinding == null)
        throw new EffectBindingException("SkinnedEffectBinding not found in render context.");

      // Get ambient light and number of directional lights.
      Vector3 ambientLight = Vector3.Zero;
      int numberOfDirectionalLights = 0;
      if (context.Scene != null && context.SceneNode != null)
      {
        // Note: XNA stock effect lights are always global.
        var query = context.Scene.Query<GlobalLightQuery>(context.CameraNode, context);
        if (query.AmbientLights.Count > 0)
        {
          var light = (AmbientLight)query.AmbientLights[0].Light;
          ambientLight = (Vector3)light.Color * light.Intensity;
        }

        numberOfDirectionalLights = query.DirectionalLights.Count;
      }

      // ----- Special: We have to add the ambient light to the emissive color.

      var diffuseBinding = effectBinding.ParameterBindings["DiffuseColor"]
                           ?? effectBinding.EffectEx.ParameterBindings["DiffuseColor"];
      var emissiveBinding = effectBinding.ParameterBindings["EmissiveColor"]
                            ?? effectBinding.EffectEx.ParameterBindings["EmissiveColor"];
      Vector4 diffuse = BasicEffectTechniqueBinding.GetColor4(diffuseBinding);
      Vector3 emissive = BasicEffectTechniqueBinding.GetColor3(emissiveBinding);

      // Premultiply emissive and add ambient lighting. (Diffuse is already premultiplied.)
      float alpha = diffuse.W;
      _effectiveEmissive.X = emissive.X * alpha + ambientLight.X * diffuse.X;
      _effectiveEmissive.Y = emissive.Y * alpha + ambientLight.Y * diffuse.Y;
      _effectiveEmissive.Z = emissive.Z * alpha + ambientLight.Z * diffuse.Z;

      // ----- Select shader.
      int shaderIndex = 0;

      if (!((IStockEffectBinding)effectBinding).FogEnabled)
        shaderIndex += 1;

      if (effectBinding.WeightsPerVertex == 2)
        shaderIndex += 2;
      else if (effectBinding.WeightsPerVertex == 4)
        shaderIndex += 4;

      if (effectBinding.PreferPerPixelLighting)
        shaderIndex += 12;
      else if (numberOfDirectionalLights <= 1)  // Optimized path if there are no more than one directional lights.
        shaderIndex += 6;

      Id = (byte)shaderIndex;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      if (_parameterEmissive == null)
      {
        _parameterEmissive = effect.Parameters["EmissiveColor"];
#if !MONOGAME
        _parameterShaderIndex = effect.Parameters["ShaderIndex"];
#endif
      }

      _parameterEmissive.SetValue(_effectiveEmissive);

#if !MONOGAME
      _parameterShaderIndex.SetValue(Id);
      return effect.Techniques[0];
#else
      return effect.Techniques[Id];
#endif
    }
  }
}
