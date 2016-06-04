// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Selects the technique where the technique name matches the current technique string of the
  /// render context (see property <see cref="RenderContext.Technique"/>).
  /// </summary>
  /// <remarks>
  /// This technique binding compares the technique names with the 
  /// <see cref="RenderContext.Technique"/> string set in the <see cref="RenderContext"/>. If a
  /// matching technique is found, it is used; otherwise, the first technique is used.
  /// </remarks>
  public sealed class ByNameTechniqueBinding : EffectTechniqueBinding
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectTechniqueCollection _techniques;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ByNameTechniqueBinding"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    public ByNameTechniqueBinding(Effect effect)
    {
      if (effect == null)
        throw new ArgumentNullException("effect");

      _techniques = effect.Techniques;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ByNameTechniqueBinding"/> class.
    /// </summary>
    /// <param name="techniques">The effect techniques.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="techniques"/> is <see langword="null"/>.
    /// </exception>
    private ByNameTechniqueBinding(EffectTechniqueCollection techniques)
    {
      if (techniques == null)
        throw new ArgumentNullException("techniques");

      _techniques = techniques;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectTechniqueBinding CreateInstanceCore()
    {
      return new ByNameTechniqueBinding(_techniques);
    }
    #endregion


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnUpdate(RenderContext context)
    {
      var count = _techniques.Count;
      for (int i = 0; i < count; i++)
      {
        if (_techniques[i].Name == context.Technique)
        {
          Id = (byte)i;
          return;
        }
      }

      Id = 0;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override EffectTechnique OnGetTechnique(Effect effect, RenderContext context)
    {
      return effect.Techniques[Id];
    }
    #endregion
  }
}
