// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to the <see cref="AlphaTestEffectBinding.AlphaFunction"/> of a 
  /// <see cref="AlphaTestEffect"/>.
  /// </summary>
  [DebuggerDisplay("AlphaTestParameterBinding(Parameter = {Parameter.Name}, Value = {Value})")]
  internal sealed class AlphaTestParameterBinding : EffectParameterBinding<Vector4>
  {
    // Note: Make default constructor protected, if class is unsealed.


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="AlphaTestParameterBinding"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="AlphaTestParameterBinding"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    private AlphaTestParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AlphaTestParameterBinding"/> class.
    /// </summary>
    /// <param name="effectBinding">The effect binding.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effectBinding"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public AlphaTestParameterBinding(AlphaTestEffectBinding effectBinding, EffectParameter parameter)
      : base(effectBinding.Effect, parameter)
    {
    }


    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new AlphaTestParameterBinding();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void OnUpdate(RenderContext context)
    {
      var effectBinding = context.MaterialBinding as AlphaTestEffectBinding;
      if (effectBinding == null)
        throw new EffectBindingException("AlphaTestEffectBinding not found in render context.");

      Vector4 alphaTest = new Vector4();

      // Convert reference alpha from 8 bit integer to 0-1 float format.
      float reference = effectBinding.ReferenceAlpha / 255.0f;

      // Comparison tolerance of half the 8 bit integer precision.
      const float threshold = 0.5f / 255f;

      switch (effectBinding.AlphaFunction)
      {
        case CompareFunction.Less:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.X = reference - threshold;
          alphaTest.Z = 1;
          alphaTest.W = -1;
          break;

        case CompareFunction.LessEqual:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.X = reference + threshold;
          alphaTest.Z = 1;
          alphaTest.W = -1;
          break;

        case CompareFunction.GreaterEqual:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.X = reference - threshold;
          alphaTest.Z = -1;
          alphaTest.W = 1;
          break;

        case CompareFunction.Greater:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.X = reference + threshold;
          alphaTest.Z = -1;
          alphaTest.W = 1;
          break;

        case CompareFunction.Equal:
          // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
          alphaTest.X = reference;
          alphaTest.Y = threshold;
          alphaTest.Z = 1;
          alphaTest.W = -1;
          break;

        case CompareFunction.NotEqual:
          // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
          alphaTest.X = reference;
          alphaTest.Y = threshold;
          alphaTest.Z = -1;
          alphaTest.W = 1;
          break;

        case CompareFunction.Never:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.Z = -1;
          alphaTest.W = -1;
          break;

        // case CompareFunction.Always:
        default:
          // Shader will evaluate: clip((a < x) ? z : w)
          alphaTest.Z = 1;
          alphaTest.W = 1;
          break;
      }

      Value = alphaTest;
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      Parameter.SetValue(Value);
    }
  }
}
