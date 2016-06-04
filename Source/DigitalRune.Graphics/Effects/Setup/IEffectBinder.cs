// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Creates bindings for effect techniques and parameters.
  /// </summary>
  public interface IEffectBinder
  {
    /// <summary>
    /// Gets the binding that selects the technique for rendering the specified effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>
    /// The <see cref="EffectTechniqueBinding"/> that selects the technique when
    /// <paramref name="effect"/> needs to be rendered. The method returns <see langword="null"/> if
    /// the effect binder is unable to provide a binding.
    /// </returns>
    EffectTechniqueBinding GetBinding(Effect effect);


    /// <summary>
    /// Gets the binding that provides the value for the specified effect parameter.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="opaqueData">The opaque data. Can be <see langword="null"/>.</param>
    /// <returns>
    /// The <see cref="EffectParameterBinding"/> that should be applied to 
    /// <paramref name="parameter"/>, or <see langword="null"/> if the effect binder is unable to 
    /// provide a binding.
    /// </returns>
    EffectParameterBinding GetBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData);
  }
}
