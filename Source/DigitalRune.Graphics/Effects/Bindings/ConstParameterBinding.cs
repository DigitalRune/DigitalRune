// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Binds an effect parameter to a value.
  /// </summary>
  /// <typeparam name="T">
  /// The value type. See <see cref="EffectParameterBinding{T}"/>.
  /// </typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter.Name}, Value = {Value})")]
  public class ConstParameterBinding<T> : EffectParameterBinding<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the value of the effect parameter.
    /// </summary>
    /// <value>The value of the effect parameter.</value>
    public new T Value
    {
      get { return base.Value; }
      set { base.Value = value; } // Same as in base class, but this time the setter is public.
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterBinding{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterBinding{T}"/> class. (This
    /// constructor creates an uninitialized instance. Use this constructor only for cloning or
    /// other special cases!)
    /// </summary>
    protected ConstParameterBinding()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConstParameterBinding{T}"/> class.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="effect"/> or <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="EffectBindingException">
    /// The value type <typeparamref name="T"/> is not supported.
    /// </exception>
    public ConstParameterBinding(Effect effect, EffectParameter parameter, T value)
      : base(effect, parameter, value)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override EffectParameterBinding CreateInstanceCore()
    {
      return new ConstParameterBinding<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(EffectParameterBinding source)
    {
      // Clone EffectParameterBinding<T> properties.
      base.CloneCore(source);

      // Clone ConstParameterBinding<T> properties.
      var sourceTyped = (ConstParameterBinding<T>)source;
      Value = sourceTyped.Value;
    }
    #endregion

    #endregion
  }
}
