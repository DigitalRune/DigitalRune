// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the shaders of a <see cref="DualTextureEffect"/>.
  /// </summary>
  [DebuggerDisplay("DualTextureEffectTechniqueBinding(Id = {Id})")]
  internal sealed class DualTextureEffectTechniqueBinding : EffectTechniqueBinding
  {
    private EffectParameter _parameterShaderIndex;


    /// <inheritdoc/>
    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new DualTextureEffectTechniqueBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnUpdate(RenderContext context)
    {
      var effectBinding = context.MaterialBinding as DualTextureEffectBinding;
      if (effectBinding == null)
        throw new EffectBindingException("DualTextureEffectBinding not found in render context.");

      // ----- Select shader.
      int shaderIndex = 0;

      if (!((IStockEffectBinding)effectBinding).FogEnabled)
        shaderIndex += 1;

      if (effectBinding.VertexColorEnabled)
        shaderIndex += 2;
      
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
