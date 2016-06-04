// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Empty binding for effect parameter array. Does nothing.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterArrayBinding{T}"/>.
  /// </typeparam>
  public class NullParameterArrayBinding<T> : EffectParameterArrayBinding<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="NullParameterArrayBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="NullParameterArrayBinding{T}"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected NullParameterArrayBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="NullParameterArrayBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public NullParameterArrayBinding(Effect effect, EffectParameter parameter)
      : base(effect, parameter)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new NullParameterArrayBinding<T>();
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      // Do nothing!
    }
    #endregion
  }
}
