// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the shaders of a <see cref="BasicEffect"/>.
  /// </summary>
  [DebuggerDisplay("BasicEffectTechniqueBinding(Id = {Id})")]
  internal sealed class BasicEffectTechniqueBinding : EffectTechniqueBinding
  {
    private EffectParameter _parameterDiffuse;
    private EffectParameter _parameterEmissive;
#if !MONOGAME
    private EffectParameter _parameterShaderIndex;
#endif
    private Vector4 _effectiveDiffuse;
    private Vector3 _effectiveEmissive;


    /// <inheritdoc/>
    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new BasicEffectTechniqueBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnUpdate(RenderContext context)
    {
      var effectBinding = context.MaterialBinding as BasicEffectBinding;
      if (effectBinding == null)
        throw new EffectBindingException("BasicEffectBinding not found in render context.");

      // Get ambient light and number of directional lights.
      Vector3 ambientLight = Vector3.Zero;
      int numberOfDirectionalLights = 0;
      if (effectBinding.LightingEnabled && context.Scene != null && context.SceneNode != null)
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

      // ----- Special: The BasicEffect requires special treatment.

      var diffuseBinding = effectBinding.ParameterBindings["DiffuseColor"]
                           ?? effectBinding.EffectEx.ParameterBindings["DiffuseColor"];
      var emissiveBinding = effectBinding.ParameterBindings["EmissiveColor"]
                            ?? effectBinding.EffectEx.ParameterBindings["EmissiveColor"];
      Vector4 diffuse = GetColor4(diffuseBinding);
      Vector3 emissive = GetColor3(emissiveBinding);

      if (effectBinding.LightingEnabled)
      {
        // ----- When lighting is enabled, add the ambient light to the emissive color.
        // Premultiply emissive and add ambient lighting. (Diffuse is already premultiplied.)
        float alpha = diffuse.W;
        _effectiveEmissive.X = emissive.X * alpha + ambientLight.X * diffuse.X;
        _effectiveEmissive.Y = emissive.Y * alpha + ambientLight.Y * diffuse.Y;
        _effectiveEmissive.Z = emissive.Z * alpha + ambientLight.Z * diffuse.Z;
        _effectiveDiffuse = diffuse;
      }
      else
      {
        // When lighting is disabled, premultiply emissive color and add to diffuse color.
        // (Diffuse is already premultiplied.)
        float alpha = diffuse.W;
        _effectiveDiffuse.X = diffuse.X + emissive.X * alpha;
        _effectiveDiffuse.Y = diffuse.Y + emissive.Y * alpha;
        _effectiveDiffuse.Z = diffuse.Z + emissive.Z * alpha;
        _effectiveDiffuse.W = alpha;
        _effectiveEmissive = emissive;
      }

      // ----- Select shader.
      int shaderIndex = 0;

      if (!((IStockEffectBinding)effectBinding).FogEnabled)
        shaderIndex += 1;

      if (effectBinding.VertexColorEnabled)
        shaderIndex += 2;

      if (effectBinding.TextureEnabled)
        shaderIndex += 4;

      if (effectBinding.LightingEnabled)
      {
        if (effectBinding.PreferPerPixelLighting)
          shaderIndex += 24;
        else if (numberOfDirectionalLights <= 1)  // Optimized path if there are no more than one directional lights.
          shaderIndex += 16;
        else
          shaderIndex += 8;
      }

      Id = (byte)shaderIndex;
    }


    internal static Vector3 GetColor3(EffectParameterBinding binding)
    {
      var binding3 = binding as EffectParameterBinding<Vector3>;
      if (binding3 != null)
        return binding3.Value;

      var binding3F = binding as EffectParameterBinding<Vector3F>;
      if (binding3F != null)
        return (Vector3)binding3F.Value;

      return new Vector3(1, 1, 1);
    }


    internal static Vector4 GetColor4(EffectParameterBinding binding)
    {
      var binding4 = binding as EffectParameterBinding<Vector4>;
      if (binding4 != null)
        return binding4.Value;

      var binding4F = binding as EffectParameterBinding<Vector4F>;
      if (binding4F != null)
        return (Vector4)binding4F.Value;

      var binding3 = binding as EffectParameterBinding<Vector3>;
      if (binding3 != null)
        return new Vector4(binding3.Value, 1);

      var binding3F = binding as EffectParameterBinding<Vector3F>;
      if (binding3F != null)
        return new Vector4((Vector3)binding3F.Value, 1);

      var bindingQ = binding as EffectParameterBinding<Quaternion>;
      if (bindingQ != null)
        return ToVector(bindingQ.Value);

      var bindingQF = binding as EffectParameterBinding<QuaternionF>;
      if (bindingQF != null)
        return ToVector(bindingQF.Value);

      return new Vector4(1, 1, 1, 1);
    }


    private static Vector4 ToVector(Quaternion quaternion)
    {
      return new Vector4(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }


    private static Vector4 ToVector(QuaternionF quaternion)
    {
      return new Vector4(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      if (_parameterDiffuse == null)
      {
        _parameterDiffuse = effect.Parameters["DiffuseColor"];
        _parameterEmissive = effect.Parameters["EmissiveColor"];
#if !MONOGAME
        _parameterShaderIndex = effect.Parameters["ShaderIndex"];
#endif
      }

      _parameterDiffuse.SetValue(_effectiveDiffuse);
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
