// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Empty binding for effect parameter. Does nothing.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterBinding{T}"/>.
  /// </typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name})")]
  public class NullParameterBinding<T> : EffectParameterBinding<T>
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
    /// Initializes a new instance of the <see cref="NullParameterBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="NullParameterBinding{T}"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected NullParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="NullParameterBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public NullParameterBinding(Effect effect, EffectParameter parameter)
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
      return new NullParameterBinding<T>();
    }


    /// <inheritdoc/>
    protected override void OnApply(RenderContext context)
    {
      // Do nothing!
    }
    #endregion
  }
}
