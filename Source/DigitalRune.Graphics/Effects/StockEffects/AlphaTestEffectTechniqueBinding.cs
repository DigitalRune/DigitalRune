// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the shaders of a <see cref="AlphaTestEffect"/>.
  /// </summary>
  [DebuggerDisplay("AlphaTestEffectTechniqueBinding(Id = {Id})")]
  internal sealed class AlphaTestEffectTechniqueBinding : EffectTechniqueBinding
  {
    private EffectParameter _parameterShaderIndex;


    /// <inheritdoc/>
    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new AlphaTestEffectTechniqueBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnUpdate(RenderContext context)
    {
      var effectBinding = context.MaterialBinding as AlphaTestEffectBinding;
      if (effectBinding == null)
        throw new EffectBindingException("AlphaTestEffectBinding not found in render context.");

      // ----- Select shader.
      int shaderIndex = 0;

      if (!((IStockEffectBinding)effectBinding).FogEnabled) 
        shaderIndex += 1;

      if (effectBinding.VertexColorEnabled)
        shaderIndex += 2;

      if (effectBinding.AlphaFunction == CompareFunction.Equal
         || effectBinding.AlphaFunction == CompareFunction.NotEqual)
        shaderIndex += 4;

      Id = (byte)shaderIndex;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      if (_parameterShaderIndex == null)
        _parameterShaderIndex = effect.Parameters["ShaderIndex"];

#if !MONOGAME
      _parameterShaderIndex.SetValue(Id);
      return effect.Techniques[0];
#else
      return effect.Techniques[Id];
#endif
    }
  }
}
