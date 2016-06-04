// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Interprets effects and provides descriptions for effect techniques and parameters.
  /// </summary>
  /// <remarks>
  /// The <see cref="IEffectInterpreter"/> interprets effect techniques and parameters by looking at
  /// their name, semantics, and annotations. When the interpreter recognizes an effect technique or
  /// parameter, it returns a description which determines how the effect is used at runtime.
  /// </remarks>
  public interface IEffectInterpreter
  {
    /// <summary>
    /// Interprets the specified effect technique.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="technique">The effect technique.</param>
    /// <returns>
    /// The description of the effect technique, or <see langword="null"/> if the method was not
    /// able to interpret the effect technique.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="technique"/> is <see langword="null"/>.
    /// </exception>
    EffectTechniqueDescription GetDescription(Effect effect, EffectTechnique technique);


    /// <summary>
    /// Interprets the specified effect parameter.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The description of the effect parameter, or <see langword="null"/> if the method was not
    /// able to interpret the effect parameter.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    EffectParameterDescription GetDescription(Effect effect, EffectParameter parameter);
  }
}
