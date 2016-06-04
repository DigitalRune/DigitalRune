// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Text;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides description of effect techniques and parameters by comparing their names, semantics, 
  /// and annotations against a dictionary.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This interpreter owns two dictionaries (see <see cref="TechniqueDescriptions"/> and 
  /// <see cref="ParameterDescriptions"/>), which are empty by default. The dictionary keys are 
  /// <strong>case-insensitive</strong> strings, e.g. "WorldViewProjection". The dictionary value is
  /// a delegate that returns a <see cref="EffectTechniqueDescription"/> or a 
  /// <see cref="EffectParameterDescription"/> for the given string. New dictionary entries can be 
  /// added to add support for new effect techniques and parameters.
  /// </para>
  /// </remarks>
  public class DictionaryEffectInterpreter : IEffectInterpreter
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Creates the description for the specified effect technique.
    /// </summary>
    /// <param name="technique">The effect technique.</param>
    /// <returns>The description of <paramref name="technique"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    public delegate EffectTechniqueDescription CreateEffectTechniqueDescription(EffectTechnique technique);


    /// <summary>
    /// Creates the description for the specified effect parameter.
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="index">The index.</param>
    /// <returns>The description of <paramref name="parameter"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    public delegate EffectParameterDescription CreateEffectParameterDescription(EffectParameter parameter, int index);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets factory methods for effect technique descriptions.
    /// </summary>
    /// <value>
    /// The factory methods for effect technique descriptions. The default value is an empty 
    /// dictionary.
    /// </value>
    /// <remarks>
    /// The key in the dictionary is a case-insensitive string. The dictionary value is a factory 
    /// method that creates an <see cref="EffectTechniqueDescription"/>.
    /// </remarks>
    public Dictionary<string, CreateEffectTechniqueDescription> TechniqueDescriptions { get; private set; }


    /// <summary>
    /// Gets or sets factory methods for effect parameter descriptions.
    /// </summary>
    /// <value>
    /// The factory methods for effect parameter descriptions. The default value is an empty 
    /// dictionary.
    /// </value>
    /// <remarks>
    /// The key in the dictionary is a case-insensitive string, e.g. "WorldViewProjection". The 
    /// dictionary value is a factory method that creates an 
    /// <see cref="EffectParameterDescription"/>.
    /// </remarks>
    public Dictionary<string, CreateEffectParameterDescription> ParameterDescriptions { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryEffectInterpreter"/> class.
    /// </summary>
    public DictionaryEffectInterpreter()
    {
      // Create dictionaries with case-insensitive string comparer!
      TechniqueDescriptions = new Dictionary<string, CreateEffectTechniqueDescription>(StringComparer.OrdinalIgnoreCase);
      ParameterDescriptions = new Dictionary<string, CreateEffectParameterDescription>(StringComparer.OrdinalIgnoreCase);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public virtual EffectTechniqueDescription GetDescription(Effect effect, EffectTechnique technique)
    {
      if (technique == null)
        throw new ArgumentNullException("technique");

      EffectTechniqueDescription description = null;

      // First, try to get description from annotation string.
#if !MONOGAME
      var annotation = technique.Annotations["Semantic"];
      if (annotation != null && annotation.ParameterType == EffectParameterType.String)
        description = GetDescriptionFromString(technique, annotation.GetValueString());
#endif

      // Second, try to get description from technique name.
      if (description == null)
        description = GetDescriptionFromString(technique, technique.Name);

      return description;
    }


    private EffectTechniqueDescription GetDescriptionFromString(EffectTechnique technique, string text)
    {
      // Check whether string matches entry in dictionary.
      CreateEffectTechniqueDescription createDescription;
      if (TechniqueDescriptions.TryGetValue(text, out createDescription))
        return createDescription(technique);

      return null;
    }


    /// <inheritdoc/>
    public virtual EffectParameterDescription GetDescription(Effect effect, EffectParameter parameter)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      EffectParameterDescription description = null;

      // First, try to get usage from annotation string.
#if !MONOGAME
      var annotation = parameter.Annotations["Semantic"];
      if (annotation != null && annotation.ParameterType == EffectParameterType.String)
        description = GetDescriptionFromString(parameter, annotation.GetValueString());
#endif 

      if (description == null)
      {
        // No annotation. 
        // --> Try to get usage from semantic.
        description = GetDescriptionFromString(parameter, parameter.Semantic);
      }

      if (description == null)
      {
        // No annotation, no semantic.
        // --> Try to get usage from parameter name.
        // Check whether string matches entry in dictionary.
        description = GetDescriptionFromString(parameter, parameter.Name);
      }

      if (description == null)
      {
        // Too bad, better luck next time.
        return null;
      }

      // Get the effect parameter hint from annotations.
      var hint = EffectHelper.GetHintFromAnnotations(parameter);
      if (hint.HasValue)
      {
        // User-defined hint found in effect file.
        // --> Override default.
        description.Hint = hint.Value;
      }

      return description;
    }


    private EffectParameterDescription GetDescriptionFromString(EffectParameter parameter, string text)
    {
      string semantic;
      int index;
      text.SplitTextAndNumber(out semantic, out index);

      if (semantic.Length > 0)
      {
        // Check whether string matches entry in dictionary.
        CreateEffectParameterDescription createDescription;
        if (ParameterDescriptions.TryGetValue(semantic, out createDescription))
          return createDescription(parameter, index);
      }

      return null;
    }
    #endregion
  }
}
