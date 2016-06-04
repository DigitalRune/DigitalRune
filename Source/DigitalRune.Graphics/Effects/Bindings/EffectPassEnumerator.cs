// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Iterates the effect passes of the current technique in the order determined by the effect pass
  /// binding.
  /// </summary>
  public struct EffectPassEnumerator : IEnumerator<EffectPass>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectTechniqueBinding _techniqueBinding;
    private readonly EffectTechnique _technique;
    private readonly RenderContext _context;
    private int _index; // Index of the next effect pass.
    private EffectPass _current;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    public EffectPass Current
    {
      get { return _current; }
    }


    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    object IEnumerator.Current
    {
      get { return _current; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectPassEnumerator"/> struct.
    /// </summary>
    /// <param name="techniqueBinding">The effect technique binding.</param>
    /// <param name="technique">The effect technique.</param>
    /// <param name="context">The render context.</param>
    internal EffectPassEnumerator(EffectTechniqueBinding techniqueBinding, EffectTechnique technique, RenderContext context)
    {
      _techniqueBinding = techniqueBinding;
      _technique = technique;
      _context = context;
      _index = 0;
      _current = null;
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting 
    /// unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      _index = 0;
      _current = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
    /// <see langword="false"/> if the enumerator has passed the end of the collection.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="EffectPassBinding"/> is invalid.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public bool MoveNext()
    {
      if (_techniqueBinding == null)
      {
        // The EffectPassEnumerator is invalid (e.g. created via default constructor).
        throw new InvalidOperationException("Invalid EffectPassEnumerator.");
      }

      return _techniqueBinding.OnNextPass(_technique, _context, ref _index, out _current);
    }


    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first element in the 
    /// collection.
    /// </summary>
    public void Reset()
    {
      _index = 0;
      _current = null;
    }
    #endregion
  }
}
