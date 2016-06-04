// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Graphics.Rendering;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Describes a technique of an effect.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="EffectTechniqueDescription"/>s provide additional information for effect 
  /// techniques. This information is used by the graphics engine to understand effects and apply 
  /// them properly during rendering.
  /// </para>
  /// <para>
  /// The descriptions are created automatically by effect interpreters (see 
  /// <see cref="IEffectInterpreter"/>) when an effect is initialized. The graphics service manages 
  /// a list of effect interpreters. Custom interpreters can be added to the graphics service to 
  /// support new types of effects.
  /// </para>
  /// </remarks>
  public class EffectTechniqueDescription
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the effect technique.
    /// </summary>
    /// <value>The effect technique.</value>
    public EffectTechnique Technique { get; private set; }


    /// <summary>
    /// Gets the index of the effect technique.
    /// </summary>
    /// <value>The index of the effect technique.</value>
    public int Index { get; private set; }


    /// <summary>
    /// Gets the associated effect technique that supports hardware instancing.
    /// </summary>
    /// <value>
    /// The effect technique that supports hardware instancing, or <see langword="null"/> if there 
    /// is no associated technique that supports hardware instancing.
    /// </value>
    /// <remarks>
    /// <para>
    /// An effect technique in a DirectX Effect may reference another technique, which supports
    /// instancing. This technique needs to have the same name plus the postfix <c>"Instancing"</c>.
    /// Example:
    /// </para>
    /// <code lang="none">
    /// <![CDATA[
    /// // Technique without hardware instancing
    /// technique MyTechnique
    /// {
    ///     pass
    ///     {
    ///         VertexShader = compile vs_2_0 VS();
    ///         PixelShader = compile ps_2_0 PS();
    ///     }
    /// }
    /// 
    /// // Equivalent of MyTechnique that supports hardware instancing.
    /// technique MyTechniqueInstancing
    /// {
    ///     pass
    ///     {
    ///         VertexShader = compile vs_3_0 HardwareInstancingVS();
    ///         PixelShader = compile ps_3_0 PS();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <para>
    /// Alternatively, the technique can also be identified by adding an effect annotation to the
    /// effect technique:
    /// </para>
    /// <code lang="none">
    /// <![CDATA[
    /// // Default technique without hardware instancing
    /// technique MyTechnique
    /// <
    ///     // There is an equivalent of this technique that supports hardware instancing.
    ///     string InstancingTechnique = "HardwareInstancing"; 
    /// >
    /// {
    ///     pass
    ///     {
    ///         VertexShader = compile vs_2_0 VS();
    ///         PixelShader = compile ps_2_0 PS();
    ///     }
    /// }
    /// 
    /// // Hardware instancing technique.
    /// technique HardwareInstancing
    /// {
    ///     pass
    ///     {
    ///         VertexShader = compile vs_3_0 HardwareInstancingVS();
    ///         PixelShader = compile ps_3_0 PS();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <note type="warning">
    /// <para>
    /// MonoGame currently does not support effect annotations. Therefore, this only works in XNA.
    /// </para>
    /// </note>
    /// <para>
    /// The <see cref="EffectTechniqueDescription"/> checks the names and annotations and will
    /// automatically resolve the technique for hardware instancing and store the reference in
    /// <see cref="InstancingTechnique"/>. The <see cref="MeshRenderer"/> and custom renderers can
    /// check this property, to see if hardware instancing is supported.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Instancing is currently missing in MonoGame. Will be added in the future.")]
    public EffectTechnique InstancingTechnique { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectTechniqueDescription"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="technique">The effect technique.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="technique"/> is <see langword="null"/>.
    /// </exception>
    public EffectTechniqueDescription(Effect effect, EffectTechnique technique)
    {
      if (effect == null)
        throw new ArgumentNullException("effect");
      if (technique == null)
        throw new ArgumentNullException("technique");

      Technique = technique;
      Index = GetIndex(effect, technique);

#if !MONOGAME
      // Check if there is an associated technique for hardware instancing.
      var annotation = technique.Annotations["InstancingTechnique"];
      if (annotation != null && annotation.ParameterType == EffectParameterType.String)
      {
        var techniqueName = annotation.GetValueString();
        if (!string.IsNullOrEmpty(techniqueName))
        {
          InstancingTechnique = effect.Techniques[techniqueName];

          if (InstancingTechnique == null)
          {
            string message = string.Format(CultureInfo.InvariantCulture, "Could not find instancing technique \"{0}\" in the effect \"{1}\".", techniqueName, effect.Name);
            throw new GraphicsException(message);
          }
        }
      }
#else
      // Workaround: MonoGame does not support effect semantics and annotations.
      if (technique.Name.IndexOf("INSTANCING", StringComparison.OrdinalIgnoreCase) == -1)
      {
        if (effect.Techniques.Count == 2
            && effect.Techniques[1].Name.IndexOf("INSTANCING", StringComparison.OrdinalIgnoreCase) >= 0)
        {
          InstancingTechnique = effect.Techniques[1];
        }

        if (InstancingTechnique == null)
        {
          foreach (var otherTechnique in effect.Techniques)
          {
            if (technique == otherTechnique)
              continue;

            if (otherTechnique.Name.IndexOf(technique.Name, StringComparison.OrdinalIgnoreCase) >= 0
                && otherTechnique.Name.IndexOf("INSTANCING", StringComparison.OrdinalIgnoreCase) >= 0)
            {
              InstancingTechnique = otherTechnique;
              break;
            }
          }
        }
      }
#endif
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Validates the specified technique and returns its index.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="technique">The effect technique.</param>
    /// <returns>The index of the effect technique</returns>
    private static int GetIndex(Effect effect, EffectTechnique technique)
    {
      int numberOfTechniques = effect.Techniques.Count;
      for (int i = 0; i < numberOfTechniques; i++)
        if (effect.Techniques[i] == technique)
          return i;

      string message = string.Format(CultureInfo.InvariantCulture, "The effect technique \"{0}\" does not belong to the effect \"{1}\".", technique.Name, effect.Name);
      throw new ArgumentException(message, "technique");
    }
    #endregion
  }
}
