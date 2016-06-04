// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  partial class EffectHelper
  {
    /// <summary>
    /// Gets the <see cref="EffectEx"/> object of the specified <see cref="Effect"/>.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>
    /// The <see cref="EffectEx"/>, which provides additional information for 
    /// <paramref name="effect"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// <see cref="EffectEx"/> is not available. <paramref name="effect"/> has not yet been 
    /// initialized.
    /// </exception>
    private static EffectEx GetEffectEx(this Effect effect)
    {
      if (effect == null)
        throw new ArgumentNullException("effect");

      var effectEx = effect.Tag as EffectEx;
      if (effectEx == null)
        throw new GraphicsException("Effect has not been initialized by the graphics service or Effect.Tag has been overwritten.");

      return effectEx;
    }


    /// <summary>
    /// Gets the technique descriptions of the specified effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>The effect technique descriptions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public static EffectTechniqueDescriptionCollection GetTechniqueDescriptions(this Effect effect)
    {
      return effect.GetEffectEx().TechniqueDescriptions;
    }


    /// <summary>
    /// Gets the default effect technique binding of the specified effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>The default effect technique binding.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public static EffectTechniqueBinding GetTechniqueBinding(this Effect effect)
    {
      return effect.GetEffectEx().TechniqueBinding;
    }


    /// <summary>
    /// Gets the effect parameter descriptions of the specified effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>The effect parameter descriptions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public static EffectParameterDescriptionCollection GetParameterDescriptions(this Effect effect)
    {
      return effect.GetEffectEx().ParameterDescriptions;
    }


    /// <summary>
    /// Gets the default effect parameter bindings of the specified effect.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>The default effect parameter bindings.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public static EffectParameterBindingCollection GetParameterBindings(this Effect effect)
    {
      return effect.GetEffectEx().ParameterBindings;
    }


    /// <summary>
    /// Gets the sort hint from the effect parameter annotations.
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <returns>
    /// The sort hint, or <see langword="null"/> if no sort hint was specified in the effect 
    /// parameter annotations.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    internal static EffectParameterHint? GetHintFromAnnotations(EffectParameter parameter)
    {
#if !MONOGAME
      var annotation = parameter.Annotations["Hint"];
      if (annotation != null && annotation.ParameterType == EffectParameterType.String)
      {
        string value = annotation.GetValueString();
        EffectParameterHint hint;
        if (EnumHelper.TryParse(value, true, out hint))
        {
          // Custom hint found.
          return hint;
        }
      }
#endif

      return null;
    }
  }
}
